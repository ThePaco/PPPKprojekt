import os
import time
import pandas as pd
import json
from pymongo import MongoClient
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
# URL_MINIO = secrets['minio']['url']
# USERNAME_MINIO = secrets['minio']['username']
# PASSWORD_MINIO = secrets['minio']['password']
# BUCKET_NAME = secrets['minio']['bucket_name']

URL_MONGO = secrets['mongodb']['url']
DB_NAME_MONGO = secrets['mongodb']['database_name']
COLLECTION_NAME_MONGO = secrets['mongodb']['collection_name']

# minio_client = Minio(
#     URL_MINIO,
#     access_key=USERNAME_MINIO,
#     secret_key=PASSWORD_MINIO,
#     secure=False
# )

mongo_client = MongoClient(URL_MONGO)
db = mongo_client[DB_NAME_MONGO]
col = db[COLLECTION_NAME_MONGO]

# minioObjects = minio_client.list_objects(BUCKET_NAME)
# GENES = [
#     "C6orf150", "CCL5", "CXCL10", "TMEM173", "CXCL9", "CXCL11", "NFKB1", "IKBKE", "IRF3", "TREX1", "ATM", "IL6", "IL8"
# ]

def refineData():
    SURVIVAL_DATA = pd.read_csv(r"D:\Repo\Algebra\PPPK_projekt\PPPKprojekt\PyGeneParserWeb\PyGeneParserWeb\resources\TCGA_clinical_survival_data.tsv", sep="\t", index_col="bcr_patient_barcode")
    updated_count = 0
    total_patients = 0
    
    for patient in col.find():
        total_patients += 1
        patient_id = patient["patient_id"]
        base_pid = patient_id[:12] #Get base patient ID (first 12 characters)
        if base_pid in SURVIVAL_DATA.index:
            row = SURVIVAL_DATA.loc[base_pid]
            update_fields = {
                "DSS": int(row["DSS"]) if not pd.isna(row["DSS"]) else None, #Disease-Specific Survival (days)
                "OS": int(row["OS"]) if not pd.isna(row["OS"]) else None, #Overall Survival (days)
                "clinical_stage": row["clinical_stage"] if pd.notna(row["clinical_stage"]) else None #Clinical cancer stage
            }
            col.update_one({"_id": patient["_id"]}, {"$set": update_fields})
            updated_count += 1
            logger.info(f"Updated patient {patient_id} with survival data.")
    
    logger.info(f"Data refinement completed: {updated_count}/{total_patients} patients updated with survival data.")
    return updated_count, total_patients

start_time = time.time()
start_timestamp = time.strftime("%Y-%m-%d %H:%M:%S", time.localtime(start_time))
logger.info(f"Starting data refinement process at {start_timestamp}")

try:
    updated_count, total_patients = refineData()
    
    end_time = time.time()
    end_timestamp = time.strftime("%Y-%m-%d %H:%M:%S", time.localtime(end_time))
    duration = end_time - start_time
    
    logger.info(f"Data refinement process completed at {end_timestamp}")
    logger.info(f"Total execution time: {duration:.2f} seconds ({duration/60:.2f} minutes)")
    logger.info(f"Performance: {total_patients/duration:.2f} patients processed per second")
    
except Exception as e:
    end_time = time.time()
    end_timestamp = time.strftime("%Y-%m-%d %H:%M:%S", time.localtime(end_time))
    duration = end_time - start_time
    
    logger.error(f"Data refinement process failed at {end_timestamp}")
    logger.error(f"Error occurred after {duration:.2f} seconds: {str(e)}")
    logger.error("Full error details:", exc_info=True)
    raise