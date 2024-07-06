#!/bin/bash

# Set environment variables
export JWT_KEY="8e73b388-707e-49cf-912d-334c988af3e0"
export JWT_ISSUER="logpunch"
export JWT_AUDIENCE="Audience"
export CONNECTION_STRING="Host=localhost:5433;Database=logpunchdb;Username=logpunchuser;Password=logpunch1234"

# Kill processes using ports 7206 (backend), 5173 (frontend), and any relevant docker ports
kill -9 $(lsof -t -i:5017) 2> /dev/null
kill -9 $(lsof -t -i:7206) 2> /dev/null
kill -9 $(lsof -t -i:5173) 2> /dev/null
kill -9 $(lsof -t -i:5433) 2> /dev/null
kill -9 $(lsof -t -i:8081) 2> /dev/null

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

# Start the backend
echo "Starting backend..."
change_directory "$SCRIPT_DIR/backend/src/Server/"
dotnet run dev &
BACKEND_PID=$!

# Start the frontend
echo "Starting frontend..."
change_directory "$SCRIPT_DIR/frontend/"
npm run dev &
FRONTEND_PID=$!

# Start the database
echo "Starting database..."
change_directory "$SCRIPT_DIR/database/data/"
docker compose up &
DATABASE_PID=$!

# Wait for all processes to finish
wait $BACKEND_PID
wait $FRONTEND_PID
wait $DATABASE_PID

echo "Solution launched!"
