# Get the directory of the script
$SCRIPT_DIR = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition

# Helper function to check and change directories
function Change-Directory {
    param (
        [string]$path
    )
    if (Test-Path -Path $path) {
        Set-Location -Path $path
        Write-Output "Changed directory to $path"
    } else {
        Write-Output "Failed to change directory to $path"
        exit 1
    }
}

# Stop the backend
Write-Output "Stopping backend..."
Change-Directory -path "$SCRIPT_DIR/backend/src/Server/"
docker-compose down

# Stop the frontend
Write-Output "Stopping frontend..."
Change-Directory -path "$SCRIPT_DIR/frontend/"
docker-compose down

# Stop the database
Write-Output "Stopping database..."
Change-Directory -path "$SCRIPT_DIR/database/data/"
docker-compose down

Write-Output "All services stopped!"
