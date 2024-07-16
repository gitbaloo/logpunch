#!/bin/bash

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
cd database
docker compose pull
docker compose build
cd ..

# Navigate to the backend directory and pull/build images
echo "Setting up the backend..."
cd backend
docker compose build
cd ..

# Navigate to the frontend directory and pull/build images
# echo "Setting up the frontend..."
# cd frontend
# docker-compose pull
# docker-compose build
# cd ..

# Make other scripts executable - launch.sh, kill.sh and killandwipe.sh
chmod +x launch.sh
chmod +x kill.sh
chmod +x killandwipe.sh

# Output success message
echo "Setup is complete. You can now run launch.sh to start the application."
