import os

# Override default storage and session directory
STORAGE_DIR = os.path.join('/var/lib/pgadmin', 'storage')
SESSION_DB_PATH = os.path.join('/var/lib/pgadmin', 'sessions')
