import time
from selenium import webdriver
from selenium.webdriver.common.by import By
import os

SCRAPING_URL = 'https://xenabrowser.net/datapages/?host=https%3A%2F%2Ftcga.xenahubs.net&removeHub=https%3A%2F%2Fpancanatlas.xenahubs.net'
SCRAPED_DATA = r"D:\Repo\Algebra\PPPK_projekt\PPPKprojekt\PyGeneParserWeb\PyGeneParserWeb\downloads"
UNCOMPRESSED_DATA = r"D:\Repo\Algebra\PPPK_projekt\PPPKprojekt\PyGeneParserWeb\PyGeneParserWeb\downloads\uncompressed"

# Ensure the download directory exists
os.makedirs(SCRAPED_DATA, exist_ok=True)
os.makedirs(UNCOMPRESSED_DATA, exist_ok=True)

def scrapeAndDownload():
    options = webdriver.ChromeOptions()
    options.add_argument('--headless')
    options.add_argument('--no-sandbox')
    options.add_argument('--disable-dev-shm-usage')
    prefs = {
        "download.default_directory": SCRAPED_DATA,
        "download.prompt_for_download": False,
        "download.directory_upgrade": True,
        "safebrowsing.enabled": True
    }
    options.add_experimental_option("prefs", prefs)

    driver = webdriver.Chrome(options=options)
    driver.get(SCRAPING_URL)

    time.sleep(3)  # Wait for the page to load

    PageElms = driver.find_elements(By.TAG_NAME, 'a')
    WantedElms = []

    for e in PageElms:
        if 'TCGA' in e.text:
            WantedElms.append(e)

    WantedElms.pop(0) #Remove some junk on bottom   

    counter = 0

    for i in range(38): #magic number, nr. cohorts
        WantedElms[0].click()
        time.sleep(3)

        PendingDownloadElms = driver.find_elements(By.TAG_NAME, 'a')
        for El in PendingDownloadElms:
            try:
                if (El.text == 'IlluminaHiSeq pancan normalized'):
                    El.click()
                    time.sleep(3)
                    link = driver.find_elements(By.TAG_NAME, 'a')
                    for l in link:
                        if ('.gz' in l.text):
                            l.click()
                            time.sleep(5)       
            
                    driver.back()
                    time.sleep(3)
                    break
            except:
                print(f"There was an error parsing this cohort! Chohort nr. {i}")

        driver.back()
        time.sleep(3)

        WantedElms.clear()
        PageElms.clear()

        PageElms = driver.find_elements(By.TAG_NAME, 'a')
        for e in PageElms:
            if ('TCGA' in e.text):
                WantedElms.append(e)
                
        WantedElms.pop(0)

        for y in range(counter + 1):
            WantedElms.pop(0)
        counter = counter + 1

    driver.close()

def decompress():
    for item in os.listdir(SCRAPED_DATA):
        if item.endswith('.gz'):
            with open(os.path.join(SCRAPED_DATA, item), 'rb') as f_in:
                with open(os.path.join(UNCOMPRESSED_DATA, item[:-3]), 'wb') as f_out:
                    f_out.write(f_in.read())
        
            print(f"Decompressed: {item} to {item[:-3]}")

#run only once, very long and expensive
#scrapeAndDownload()

decompress()