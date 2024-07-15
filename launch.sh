#!/bin/bash

# Set environment variables
export JWT_KEY="8e73b388-707e-49cf-912d-334c988af3e0"
export JWT_ISSUER="logpunch"
export JWT_AUDIENCE="Audience"
export CONNECTION_STRING="Host=localhost:5433;Database=logpunchdb;Username=logpunchuser;Password=logpunch1234"

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

# Start the database
echo "Starting database..."
change_directory "$SCRIPT_DIR/database/data/"
docker compose up -d &
DATABASE_PID=$!

# Start the backend
echo "Starting backend..."
change_directory "$SCRIPT_DIR/backend/src/Server/"
docker compose up -d &
BACKEND_PID=$!

# Start the frontend
# echo "Starting frontend..."
# change_directory "$SCRIPT_DIR/frontend/"
# docker compose up -d &
# FRONTEND_PID=$!

# Wait for a few seconds to ensure services start properly
sleep 10

echo "Solution launched!"
echo "Launching browser tabs..."

# database (pgAdmin 4)
xdg-open "http://localhost:8081/browser/"

# backend
xdg-open "http://localhost:7206/swagger/index.html"

# frontend
# xdg-open "http://localhost:5173/"

# Wait for all processes to finish
wait $DATABASE_PID
wait $BACKEND_PID
# wait $FRONTEND_PID
