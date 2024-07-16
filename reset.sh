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

# Take down all containers and their volumes
./wipe.sh

# Remove the local pgAdmin data
change_directory "$SCRIPT_DIR/database/data/"

sudo rm -rf pgadmin_data
sudo rm -rf pgpassfile

change_directory "$SCRIPT_DIR/"

echo "Reset complete"
