#!/bin/bash

echo "Starting Spider IoT Platform - All APIs"
echo "====================================="

# Function to start API in background
start_api() {
    local name="$1"
    local path="$2"
    local port="$3"
    
    echo "Starting $name on port $port..."
    (cd "$path" && dotnet run --urls "http://localhost:$port" > "/tmp/spider-$port.log" 2>&1) &
    echo "Started $name (PID: $!)"
    sleep 2
}

# Get script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Start all APIs
start_api "Device Management API" "$SCRIPT_DIR/src/BoundedContexts/DeviceManagement/Spider.DeviceManagement.API" 5001
start_api "Data Acquisition API" "$SCRIPT_DIR/src/BoundedContexts/DataAcquisition/Spider.DataAcquisition.API" 5003  
start_api "Connection Management API" "$SCRIPT_DIR/src/BoundedContexts/ConnectionManagement/Spider.ConnectionManagement.API" 5005
start_api "Project Management API" "$SCRIPT_DIR/src/BoundedContexts/ProjectManagement/Spider.ProjectManagement.API" 5007
start_api "Communication API" "$SCRIPT_DIR/src/BoundedContexts/Communication/Spider.Communication.API" 5009

echo "Waiting for APIs to start..."
sleep 5

# Start Blazor UI
echo "Starting Blazor UI on port 5267..."
(cd "$SCRIPT_DIR/src/UI/SpiderStudio.BlazorServer" && dotnet run --urls "http://localhost:5267" > "/tmp/spider-5267.log" 2>&1) &
echo "Started Blazor UI (PID: $!)"

echo ""
echo "All APIs and UI starting..."
echo ""
echo "Access URLs:"
echo "- Device Management API: http://localhost:5001/swagger"
echo "- Data Acquisition API: http://localhost:5003/swagger"
echo "- Connection Management API: http://localhost:5005/swagger" 
echo "- Project Management API: http://localhost:5007/swagger"
echo "- Communication API: http://localhost:5009/swagger"
echo "- Spider Studio UI: http://localhost:5267"
echo ""
echo "Logs are available in /tmp/spider-*.log"
echo ""
echo "To stop all services, run: pkill -f \"dotnet run\""
echo "Press Ctrl+C to exit"

# Keep script running
while true; do
    sleep 60
done