from minio import Minio
import os
import json
import logging

SCRAPED_DATA = r"D:\Repo\Algebra\PPPK_projekt\PPPKprojekt\PyGeneParserWeb\PyGeneParserWeb\downloads\uncompressed"

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

def load_secrets():
    """Load configuration secrets from a JSON file."""
    try:
        # Get the directory where this script is located
        script_dir = os.path.dirname(os.path.abspath(__file__))
        secrets_path = os.path.join(script_dir, '_secrets.json')
        
        with open(secrets_path, 'r') as f:
            return json.load(f)
    except FileNotFoundError:
        logger.error("_secrets.json file not found")
        raise
    except json.JSONDecodeError:
        logger.error("Invalid JSON in _secrets.json")
        raise
    except Exception as e:
        logger.error(f"Error loading secrets: {str(e)}")
        raise

# Load secrets
secrets = load_secrets()

# CONSTANTS from secrets
URL_MINIO = secrets['minio']['url']
USERNAME_MINIO = secrets['minio']['username']
PASSWORD_MINIO = secrets['minio']['password']
BUCKET_NAME = secrets['minio']['bucket_name']

minio_client = Minio(
    URL_MINIO,
    access_key=USERNAME_MINIO,
    secret_key=PASSWORD_MINIO,
    secure=False
)

if not minio_client.bucket_exists(BUCKET_NAME):
    minio_client.make_bucket(BUCKET_NAME)
    print(f"Bucket {BUCKET_NAME} created.")
else:
    print(f"Bucket {BUCKET_NAME} successefuly found.")

for file_name in os.listdir(SCRAPED_DATA):
    if file_name.endswith("PANCAN"):  
        file_path = os.path.join(SCRAPED_DATA, file_name)
        minio_path = f"{file_name}"  

        try:
            minio_client.fput_object(BUCKET_NAME, minio_path, file_path)
            logger.info(f"Uploaded: {file_name} to MinIO bucket: {BUCKET_NAME}/{minio_path}")

        except Exception as e:
            logger.error(f"Error uploading {file_name}: {e}")

