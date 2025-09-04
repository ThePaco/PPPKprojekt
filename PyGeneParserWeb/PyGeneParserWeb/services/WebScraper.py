import os
import time
import zipfile
import requests
from selenium import webdriver
from selenium.webdriver.chrome.service import Service
from selenium.webdriver.chrome.options import Options
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from bs4 import BeautifulSoup
from urllib.parse import urljoin, urlparse
import logging

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

class TCGAWebScraper:
    def __init__(self, downloads_folder="downloads"):

        self.url = 'https://xenabrowser.net/datapages/?host=https%3A%2F%2Ftcga.xenahubs.net&removeHub=https%3A%2F%2Fpancanatlas.xenahubs.net'
        self.downloads_folder = downloads_folder
        self.driver = None
        
        # Create downloads folder if it doesn't exist
        if not os.path.exists(self.downloads_folder):
            os.makedirs(self.downloads_folder)
            logger.info(f"Created downloads folder: {self.downloads_folder}")
    
    def setup_chrome_driver(self):
        chrome_options = Options()
        chrome_options.add_argument("--headless")  # Run in background
        chrome_options.add_argument("--no-sandbox")
        chrome_options.add_argument("--disable-dev-shm-usage")
        chrome_options.add_argument("--disable-gpu")
        chrome_options.add_argument("--window-size=1920,1080")
        
        download_path = os.path.abspath(self.downloads_folder)
        prefs = {
            "download.default_directory": download_path,
            "download.prompt_for_download": False,
            "download.directory_upgrade": True,
            "safebrowsing.enabled": True
        }
        chrome_options.add_experimental_option("prefs", prefs)
        
        self.driver = webdriver.Chrome(options=chrome_options)
        logger.info("Chrome WebDriver initialized successfully")
    
    def scrape_cohorts(self):
        if not self.driver:
            self.setup_chrome_driver()
        
        try:
            logger.info(f"Loading page: {self.url}")
            self.driver.get(self.url)
            
            WebDriverWait(self.driver, 30).until(
                EC.presence_of_element_located((By.TAG_NAME, "body"))
            )
            
            time.sleep(5)
            
            page_source = self.driver.page_source
            soup = BeautifulSoup(page_source, 'html.parser')
            
            cohorts = []
            
            cohort_elements = soup.find_all(['div', 'tr', 'li'], class_=lambda x: x and ('cohort' in x.lower() or 'dataset' in x.lower()))
            
            if not cohort_elements:
                cohort_elements = soup.find_all('a', href=True)
            
            logger.info(f"Found {len(cohort_elements)} potential cohort elements")
            
            for element in cohort_elements:
                cohort_info = self.extract_cohort_info(element)
                if cohort_info:
                    cohorts.append(cohort_info)
            
            logger.info(f"Successfully extracted {len(cohorts)} cohorts")
            return cohorts
            
        except Exception as e:
            logger.error(f"Error scraping cohorts: {str(e)}")
            return []
    
    def extract_cohort_info(self, element):
        try:
            cohort_info = {}
            
            cohort_name = element.get_text(strip=True)
            if not cohort_name or len(cohort_name) < 3:
                return None
            
            cohort_info['name'] = cohort_name
            
            download_links = []
            
            if element.name == 'a' and element.get('href'):
                href = element.get('href')
                if self.is_download_link(href):
                    download_links.append(href)
            
            for link in element.find_all('a', href=True):
                href = link.get('href')
                if self.is_download_link(href):
                    download_links.append(href)
            
            if download_links:
                cohort_info['download_links'] = download_links
                return cohort_info
            
            return None
            
        except Exception as e:
            logger.error(f"Error extracting cohort info: {str(e)}")
            return None
    
    def is_download_link(self, href):
        if not href:
            return False
        
        download_extensions = ['.zip', '.gz', '.tar', '.tar.gz', '.bz2', '.xz']
        href_lower = href.lower()
        
        return any(ext in href_lower for ext in download_extensions)
    
    def download_cohort_files(self, cohorts, max_downloads=None):
        downloaded_files = []
        download_count = 0
        
        for cohort in cohorts:
            if max_downloads and download_count >= max_downloads:
                break
                
            cohort_name = cohort.get('name', 'unknown')
            download_links = cohort.get('download_links', [])
            
            logger.info(f"Processing cohort: {cohort_name}")
            
            for link in download_links:
                if max_downloads and download_count >= max_downloads:
                    break
                
                try:
                    absolute_url = urljoin(self.url, link)
                    
                    file_path = self.download_file(absolute_url, cohort_name)
                    if file_path:
                        downloaded_files.append(file_path)
                        download_count += 1
                        
                        if file_path.endswith('.zip'):
                            self.extract_zip_file(file_path)
                            
                except Exception as e:
                    logger.error(f"Error downloading {link}: {str(e)}")
        
        logger.info(f"Downloaded {len(downloaded_files)} files")
        return downloaded_files
    
    def download_file(self, url, cohort_name):
        try:
            logger.info(f"Downloading: {url}")
            
            cohort_folder = os.path.join(self.downloads_folder, self.sanitize_filename(cohort_name))
            if not os.path.exists(cohort_folder):
                os.makedirs(cohort_folder)
            
            parsed_url = urlparse(url)
            filename = os.path.basename(parsed_url.path)
            if not filename:
                filename = f"download_{int(time.time())}.zip"
            
            file_path = os.path.join(cohort_folder, filename)
            
            response = requests.get(url, stream=True, timeout=30)
            response.raise_for_status()
            
            with open(file_path, 'wb') as f:
                for chunk in response.iter_content(chunk_size=8192):
                    f.write(chunk)
            
            logger.info(f"Downloaded: {file_path}")
            return file_path
            
        except Exception as e:
            logger.error(f"Error downloading {url}: {str(e)}")
            return None
    
    def extract_zip_file(self, zip_path):
        try:
            extract_folder = os.path.splitext(zip_path)[0]
            
            with zipfile.ZipFile(zip_path, 'r') as zip_ref:
                zip_ref.extractall(extract_folder)
            
            logger.info(f"Extracted: {zip_path} to {extract_folder}")
            
        except Exception as e:
            logger.error(f"Error extracting {zip_path}: {str(e)}")
    
    def sanitize_filename(self, filename):
        import re
        sanitized = re.sub(r'[<>:"/\\|?*]', '_', filename)
        sanitized = re.sub(r'\s+', '_', sanitized.strip())
        return sanitized[:100]
    
    def run_scraping_pipeline(self, max_downloads=5):
        try:
            logger.info("Starting TCGA cohort scraping pipeline")

            cohorts = self.scrape_cohorts()
            
            if not cohorts:
                logger.warning("No cohorts found")
                return
            
            downloaded_files = self.download_cohort_files(cohorts, max_downloads)
            
            logger.info(f"Pipeline completed. Downloaded {len(downloaded_files)} files")
            
        except Exception as e:
            logger.error(f"Pipeline error: {str(e)}")
        finally:
            self.cleanup()
    
    def cleanup(self):
        """
        Clean up resources
        """
        if self.driver:
            self.driver.quit()
            logger.info("WebDriver closed")




def main():

    scraper = TCGAWebScraper(downloads_folder="downloads")
    
    try:
        scraper.run_scraping_pipeline(max_downloads=3)
        
    except Exception as e:
        logger.error(f"Main execution error: {str(e)}")
    finally:
        scraper.cleanup()

if __name__ == "__main__":
    main()