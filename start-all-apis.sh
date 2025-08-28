#!/bin/bash

echo "Starting Spider IoT Platform - All APIs"
echo "====================================="

# Function to start API with proper build sequencing
start_api() {
    local name="$1"
    local path="$2"
    local port="$3"
    
    echo "Starting $name on port $port..."
    echo "Building $name..."
    (cd "$path" && dotnet build -c Release > "/tmp/spider-build-$port.log" 2>&1)
    if [ $? -eq 0 ]; then
        echo "Build successful, starting $name..."
        (cd "$path" && dotnet run --urls "http://localhost:$port" > "/tmp/spider-$port.log" 2>&1) &
        local PID=$!
        echo "Started $name (PID: $PID)"
        sleep 5  # Give more time for each API to start
    else
        echo "Build failed for $name, check /tmp/spider-build-$port.log"
        return 1
    fi
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
sleep 10

# Function to check API health
check_api_health() {
    local name="$1"
    local port="$2"
    local retries=5
    
    echo "Checking $name health..."
    for i in $(seq 1 $retries); do
        if curl -s "http://localhost:$port/health" > /dev/null 2>&1; then
            echo "✅ $name is healthy"
            return 0
        elif curl -s "http://localhost:$port/swagger" > /dev/null 2>&1; then
            echo "✅ $name is responsive (swagger available)"
            return 0
        else
            echo "⏳ Waiting for $name... (attempt $i/$retries)"
            sleep 3
        fi
    done
    echo "❌ $name is not responding"
    return 1
}

echo "Checking API health..."
check_api_health "Device Management API" 5001
check_api_health "Data Acquisition API" 5003
check_api_health "Connection Management API" 5005
check_api_health "Project Management API" 5007
check_api_health "Communication API" 5009

# Start Blazor UI
echo "Building Blazor UI..."
(cd "$SCRIPT_DIR/src/UI/SpiderStudio.BlazorServer" && dotnet build -c Release > "/tmp/spider-build-5267.log" 2>&1)
if [ $? -eq 0 ]; then
    echo "Starting Blazor UI on port 5267..."
    (cd "$SCRIPT_DIR/src/UI/SpiderStudio.BlazorServer" && dotnet run --urls "http://localhost:5267" > "/tmp/spider-5267.log" 2>&1) &
    echo "Started Blazor UI (PID: $!)"
    sleep 5
    
    echo "Checking Blazor UI health..."
    if curl -s "http://localhost:5267" > /dev/null 2>&1; then
        echo "✅ Blazor UI is healthy"
    else
        echo "❌ Blazor UI is not responding"
    fi
else
    echo "Build failed for Blazor UI, check /tmp/spider-build-5267.log"
fi

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