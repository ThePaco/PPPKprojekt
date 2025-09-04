import os
import io
import pandas as pd
import json
from datetime import datetime
from pymongo import MongoClient
from minio import Minio
import logging

# Configure logging
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
BUCKET_NAME_MINIO = secrets['minio']['bucket_name']

URL_MONGO = secrets['mongodb']['url']
DB_NAME_MONGO = secrets['mongodb']['database_name']
COLLECTION_NAME_MONGO = secrets['mongodb']['collection_name']

GENES = [
    "C6orf150", "CCL5", "CXCL10", "TMEM173", "CXCL9", "CXCL11",
    "NFKB1", "IKBKE", "IRF3", "TREX1", "ATM", "IL6", "IL8"
]

# Initialize clients
minio_client = Minio(
    URL_MINIO,
    access_key=USERNAME_MINIO,
    secret_key=PASSWORD_MINIO,
    secure=False
)

mongo_client = MongoClient(URL_MONGO)
db = mongo_client[DB_NAME_MONGO]
col = db[COLLECTION_NAME_MONGO]

class GeneDataProcessor:
    
    def __init__(self):
        self.processed_files = []
        self.uploaded_documents = 0
    
    def list_minio_objects(self):
        try:
            objects = minio_client.list_objects(BUCKET_NAME_MINIO, recursive=True)
            object_names = [obj.object_name for obj in objects]
            logger.info(f"Found {len(object_names)} objects in MinIO bucket")
            return object_names
        except Exception as e:
            logger.error(f"Error listing MinIO objects: {str(e)}")
            return []
    
    def process_gene_file(self, file_content, filename):
        try:
            file_content.seek(0)
            
            gene_records = []
            
            if filename.endswith('.csv'):
                df = pd.read_csv(file_content)
                gene_records = self.process_csv_data(df, filename)
            elif filename.endswith('.tsv') or filename.endswith('.txt'):
                df = pd.read_csv(file_content, sep='\t')
                gene_records = self.process_tsv_data(df, filename)
            elif filename.endswith('.json'):
                file_content.seek(0)
                data = json.load(file_content)
                gene_records = self.process_json_data(data, filename)
            else:
                file_content.seek(0)
                df = pd.read_csv(file_content, sep='\t')
                gene_records = self.process_tsv_data(df, filename)
            
            logger.info(f"Processed {len(gene_records)} records from {filename}")
            return gene_records
            
        except Exception as e:
            logger.error(f"Error processing {filename}: {str(e)}")
            return []
    
    def process_csv_data(self, df, filename):
        records = []
        
        for index, row in df.iterrows():
            record = self.create_gene_record(row, filename, index)
            if record:
                records.append(record)
        
        return records
    
    def create_gene_record(self, row, filename, index):
        try:
            record = {
                "_id": f"{filename}_{index}",
                "source_file": filename,
                "upload_timestamp": datetime.utcnow(),
                "data_index": index,
                "raw_data": {}
            }
            
            row_dict = row.to_dict()
            for key, value in row_dict.items():
                if pd.isna(value):
                    record["raw_data"][str(key)] = None
                else:
                    record["raw_data"][str(key)] = value
            
            gene_info = self.extract_gene_info(row_dict)
            if gene_info:
                record.update(gene_info)
            
            return record
            
        except Exception as e:
            logger.error(f"Error creating record from row {index}: {str(e)}")
            return None
    
    
    def upload_to_mongodb(self, records):
        if not records:
            logger.warning("No records to upload")
            return False
        
        try:
            for record in records:
                col.replace_one(
                    {"_id": record["_id"]}, 
                    record, 
                    upsert=True
                )
            
            self.uploaded_documents += len(records)
            logger.info(f"Successfully uploaded {len(records)} records to MongoDB")
            return True
            
        except Exception as e:
            logger.error(f"Error uploading to MongoDB: {str(e)}")
            return False
    
    def process_all_files(self):
        try:
            logger.info("Starting gene data processing pipeline")
            
            object_names = self.list_minio_objects()
            
            if not object_names:
                logger.warning("No files found in MinIO bucket")
                return
            
            total_records = 0
            
            for object_name in object_names:
                logger.info(f"Processing file: {object_name}")
                
                file_content = self.download_file_from_minio(object_name)
                if not file_content:
                    continue
                
                records = self.process_gene_file(file_content, object_name)
                if records:
                    success = self.upload_to_mongodb(records)
                    if success:
                        self.processed_files.append(object_name)
                        total_records += len(records)
                
                file_content.close()
            
            logger.info(f"Pipeline completed. Processed {len(self.processed_files)} files, "
                       f"uploaded {total_records} total records")
            
        except Exception as e:
            logger.error(f"Pipeline error: {str(e)}")

def main():
    processor = GeneDataProcessor()
    
    try:
        processor.process_all_files()
        
        stats = processor.get_gene_statistics()
        print("\n=== Gene Data Statistics ===")
        print(f"Total documents in MongoDB: {stats.get('total_documents', 0)}")
        print(f"Target gene documents: {stats.get('target_genes', 0)}")
        
        if stats.get('files'):
            print("\nFiles processed:")
            for file_stat in stats['files']:
                print(f"  {file_stat['_id']}: {file_stat['count']} records")
        
        if stats.get('top_genes'):
            print("\nTop genes by frequency:")
            for gene_stat in stats['top_genes'][:10]:
                print(f"  {gene_stat['_id']}: {gene_stat['count']} records")
        
    except Exception as e:
        logger.error(f"Main execution error: {str(e)}")
    finally:
        mongo_client.close()
        logger.info("Connections closed")

def test_connection():
    try:
        bucket_exists = minio_client.bucket_exists(BUCKET_NAME_MINIO)
        print(f"MinIO connection: {'✓' if bucket_exists else '✗'}")
        print(f"Bucket '{BUCKET_NAME_MINIO}' exists: {bucket_exists}")
        
        db.command('ping')
        print("MongoDB connection: ✓")
        
        objects = list(minio_client.list_objects(BUCKET_NAME_MINIO))
        print(f"Objects in bucket: {len(objects)}")
        
        return True
        
    except Exception as e:
        print(f"Connection test failed: {str(e)}")
        return False

if __name__ == "__main__":
    # Test
    if test_connection():
        main()
    else:
        print("Connection test failed. Please check your credentials and network connectivity.")