# Set environment variables
$env:JWT_KEY = "8e73b388-707e-49cf-912d-334c988af3e0"
$env:JWT_ISSUER = "logpunch"
$env:JWT_AUDIENCE = "Audience"
$env:CONNECTION_STRING = "Host=localhost:5433;Database=logpunchdb;Username=logpunchuser;Password=logpunch1234"

# Kill processes using ports 7206 (backend), 5173 (frontend), and any relevant docker ports
$ports = @(5017, 7206, 5173, 5433, 8081)
foreach ($port in $ports) {
    Stop-Process -Id (Get-NetTCPConnection -LocalPort $port).OwningProcess -Force -ErrorAction SilentlyContinue
}

# Get the directory of the script
$SCRIPT_DIR = Split-Path -Parent $MyInvocation.MyCommand.Definition

# Helper function to check and change directories
function Change-Directory {
    param (
        [string]$path
    )
    if (Test-Path $path) {
        Set-Location $path
        Write-Output "Changed directory to $path"
    } else {
        Write-Output "Failed to change directory to $path"
        exit 1
    }
}

# Start the backend
Write-Output "Starting backend..."
Change-Directory "$SCRIPT_DIR\backend\src\Server\"
Start-Process -NoNewWindow -FilePath "dotnet" -ArgumentList "run dev" -PassThru
$BACKEND_PID = $LastExitCode

# Start the frontend
Write-Output "Starting frontend..."
Change-Directory "$SCRIPT_DIR\frontend\"
Start-Process -NoNewWindow -FilePath "npm" -ArgumentList "run dev" -PassThru
$FRONTEND_PID = $LastExitCode

# Start the database
Write-Output "Starting database..."
Change-Directory "$SCRIPT_DIR\database\data\"
Start-Process -NoNewWindow -FilePath "docker-compose" -ArgumentList "up" -PassThru
$DATABASE_PID = $LastExitCode

# Wait for all processes to finish
Wait-Process -Id $BACKEND_PID
Wait-Process -Id $FRONTEND_PID
Wait-Process -Id $DATABASE_PID

Write-Output "Solution launched!"
