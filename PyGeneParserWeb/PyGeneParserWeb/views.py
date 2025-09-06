from datetime import datetime
from flask import render_template, request, jsonify
from pymongo import MongoClient
from PyGeneParserWeb import app
import logging
import os
import json

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

def load_secrets():
    """Load configuration secrets from a JSON file."""
    try:
        # Get the directory where this script is located
        script_dir = os.path.dirname(os.path.abspath(__file__))
        # Navigate to the services directory where _secrets.json is located
        secrets_path = os.path.join(script_dir, 'services', '_secrets.json')
        
        with open(secrets_path, 'r') as f:
            return json.load(f)
    except FileNotFoundError:
        logger.error(f"_secrets.json file not found at: {secrets_path}")
        raise
    except json.JSONDecodeError:
        logger.error("Invalid JSON in _secrets.json")
        raise
    except Exception as e:
        logger.error(f"Error loading secrets: {str(e)}")
        raise

# Load secrets
secrets = load_secrets()

URL_MONGO = secrets['mongodb']['url']
DB_NAME_MONGO = secrets['mongodb']['database_name']
COLLECTION_NAME_MONGO = secrets['mongodb']['collection_name']






@app.route('/', methods=['GET'])
@app.route('/home', methods=['GET'])
def home():
    """Render the home page."""
    return render_template('home.html')

@app.route('/api/gene_expression', methods=['GET'])
def get_patient_data():
    """Get gene expression data for a specific patient."""
    mongo_client = MongoClient(URL_MONGO)
    db = mongo_client[DB_NAME_MONGO]
    col = db[COLLECTION_NAME_MONGO]

    patient_id = request.args.get('patient_id')
    if not patient_id:
        return jsonify({"error": "patient_id is required"}), 400

    query = {"patient_id": patient_id}
    projection = {"_id": 0, "patient_id": 1, "cancer_cohort": 1, "genes": 1, "DSS": 1, "OS": 1, "clinical_stage": 1}
    result = col.find_one(query, projection)

    if not result:
        return jsonify({"error": "No data found for the given patient_id"}), 404

    return jsonify(result)