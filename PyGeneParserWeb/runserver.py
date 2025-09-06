"""
This script runs the PyGeneParserWeb application using a development server.
"""

from os import environ
from PyGeneParserWeb import app
from flask_cors import CORS


if __name__ == '__main__':
    CORS(app)
    
    HOST = environ.get('SERVER_HOST', 'localhost')
    try:
        PORT = int(environ.get('SERVER_PORT', '5555'))
    except ValueError:
        PORT = 5555
    
    app.run(HOST, PORT, debug=True)

