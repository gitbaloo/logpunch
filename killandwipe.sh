#!/bin/bash

# Get the directory of the script
SCRIPT_DIR=$(dirname "$(realpath "$0")")

# Helper function to check and change directories
change_directory() {
    if cd "$1"; then
        echo "Changed directory to $1"
    else
        echo "Failed to change directory to $1"
        exit 1
    fi
}

# Stop the database (and pgAdmin 4)
echo "Stopping database..."
change_directory "$SCRIPT_DIR/database/data/"
docker compose down -v

# Stop the backend
echo "Stopping backend..."
change_directory "$SCRIPT_DIR/backend/src/Server/"
docker compose down -v

# Stop the frontend
# echo "Stopping frontend..."
# change_directory "$SCRIPT_DIR/frontend/"
# docker compose down -v

echo "All services stopped and all volumes taken down!"
