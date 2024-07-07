# Kill processes using ports 7206 (backend), 5173 (frontend), and any relevant docker ports
$ports = @(5017, 7206, 5173, 5433, 8081)
foreach ($port in $ports) {
    Stop-Process -Id (Get-NetTCPConnection -LocalPort $port).OwningProcess -Force -ErrorAction SilentlyContinue
}
