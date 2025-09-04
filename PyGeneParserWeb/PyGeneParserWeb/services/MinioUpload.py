from minio import Minio
import os
import json
import logging

SCRAPED_DATA = r"D:\Repo\Algebra\PPPK_projekt\PPPKprojekt\PyGeneParserWeb\PyGeneParserWeb\downloads"

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

def load_secrets():
    """Load secrets from secrets.json file"""
    try:
        # Get the directory where this script is located
        script_dir = os.path.dirname(os.path.abspath(__file__))
        secrets_path = os.path.join(script_dir, '_secrets.json')
        
        with open(secrets_path, 'r') as f:
            secrets = json.load(f)
        
        logger.info("Secrets loaded successfully")
        return secrets
    except FileNotFoundError:
        logger.error(f"secrets.json file not found at {secrets_path}")
        raise
    except json.JSONDecodeError as e:
        logger.error(f"Error parsing secrets.json: {str(e)}")
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

URL_MONGO = secrets['mongodb']['url']
DB_NAME_MONGO = secrets['mongodb']['database_name']
COLLECTION_NAME_MONGO = secrets['mongodb']['collection_name']

minio_client = Minio(
    URL_MINIO,
    access_key=USERNAME_MINIO,
    secret_key=PASSWORD_MINIO,
    secure=False
)

isFound = minio_client.bucket_exists(BUCKET_NAME)

if not isFound:
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
            print(f"Uploaded: {file_name} to MinIO bucket: {BUCKET_NAME}/{minio_path}")

        except Exception as e:
            print(f"Error uploading {file_name}: {e}")

