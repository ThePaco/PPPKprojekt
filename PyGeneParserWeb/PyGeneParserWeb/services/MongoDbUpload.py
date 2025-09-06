import os
import io
import gzip
import pandas as pd
import json
from datetime import datetime
from pymongo import MongoClient
from minio import Minio
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

URL_MONGO = secrets['mongodb']['url']
DB_NAME_MONGO = secrets['mongodb']['database_name']
COLLECTION_NAME_MONGO = secrets['mongodb']['collection_name']

minio_client = Minio(
    URL_MINIO,
    access_key=USERNAME_MINIO,
    secret_key=PASSWORD_MINIO,
    secure=False
)

mongo_client = MongoClient(URL_MONGO)
db = mongo_client[DB_NAME_MONGO]
col = db[COLLECTION_NAME_MONGO]

minioObjects = minio_client.list_objects(BUCKET_NAME)
GENES = [
    "C6orf150", "CCL5", "CXCL10", "TMEM173", "CXCL9", "CXCL11", "NFKB1", "IKBKE", "IRF3", "TREX1", "ATM", "IL6", "IL8"
]

def pushToMongo():
    for obj in minioObjects:
        if not obj.object_name.endswith("PANCAN"):
            continue

        cohort = obj.object_name.replace(".tsv", "")
        logger.info(f"Processing {obj.object_name} (cohort: {cohort})")

        try:
            response = minio_client.get_object(BUCKET_NAME, obj.object_name)
            
            file_data = response.read()

            # Redundancy gzip check
            if file_data[:2] == b'\x1f\x8b':  # Gzip magic number
                logger.info(f"Decompressing gzip file: {obj.object_name}")
                decompressed_data = gzip.decompress(file_data)
                content = io.StringIO(decompressed_data.decode('utf-8'))
            else:
                content = io.StringIO(file_data.decode('utf-8'))
            
            df = pd.read_csv(content, sep="\t", index_col=0)
            df = df.transpose()

            genes_present = list(set(GENES) & set(df.columns))
            if not genes_present:
                logger.info(f"Target genes not found in {cohort}.")
                continue

            for pid, row in df.iterrows():
                document = {
                    "patient_id": pid,
                    "cancer_cohort": cohort,
                    "genes": {g: row[g] for g in genes_present if pd.notna(row[g])}
                }
                col.insert_one(document)

            logger.info(f"Inserted {len(df)} patients from {cohort}. Starting next cohort.")

        except Exception as e:
            logger.error(f"An error occurred while processing {obj.object_name}: {e}")

pushToMongo()