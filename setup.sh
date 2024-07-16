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

# Check if Docker is installed
if ! command -v docker &> /dev/null
then
    echo "Docker is not installed. Please install Docker before running this script."
    exit
fi

# Check if Docker Compose is installed
if ! command -v docker compose &> /dev/null
then
    echo "Docker Compose is not installed. Please install Docker Compose before running this script."
    exit
fi

# Create the logpunch_network if it does not exist
if ! docker network ls | grep -w logpunch_network &> /dev/null
then
    echo "Creating logpunch_network..."
    docker network create logpunch_network
else
    echo "logpunch_network already exists."
fi

# Navigate to the database directory and pull/build images
echo "Setting up the database..."
change_directory "$SCRIPT_DIR/database/"
sudo mkdir -p data/pgadmin_data
sudo mkdir -p data/pgpassfile
sudo chown -R 5050:5050 data/pgadmin_data
sudo chown -R 5050:5050 data/pgpassfile
docker compose pull
docker compose build
change_directory "$SCRIPT_DIR/"

# Navigate to the backend directory and pull/build images
echo "Setting up the backend..."
change_directory "$SCRIPT_DIR/backend/"
docker compose build
change_directory "$SCRIPT_DIR/"


# Navigate to the frontend directory and pull/build images
# echo "Setting up the frontend..."
# change_directory "$SCRIPT_DIR/frontend"
# docker-compose build
# change_directory "$SCRIPT_DIR/"

# Make other scripts executable - launch.sh, kill.sh and killandwipe.sh
chmod +x launch.sh
chmod +x kill.sh
chmod +x wipe.sh
chmod +x reset.sh

# Output success message
echo "Setup is complete. You can now run launch.sh to start the application."
