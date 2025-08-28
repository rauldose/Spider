#!/bin/bash

echo "🧪 Spider IoT Platform - Comprehensive Button Function Validation"
echo "================================================================="

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to check if method exists in file
check_method() {
    local file="$1"
    local method="$2"
    if grep -q "$method" "$file"; then
        echo -e "  ✅ $method: ${GREEN}IMPLEMENTED${NC}"
        return 0
    else
        echo -e "  ❌ $method: ${RED}MISSING${NC}"
        return 1
    fi
}

# Get script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
UI_DIR="$SCRIPT_DIR/src/UI/SpiderStudio.BlazorServer/Components/Pages"

total_methods=0
implemented_methods=0

echo ""
echo "📝 Checking UI Button Implementations..."
echo ""

# Check DeviceManagement.razor
echo "🔧 DeviceManagement.razor:"
methods=("ShowCreateDeviceForm" "HideCreateDeviceForm" "CheckApiHealth" "CreateDevice")
for method in "${methods[@]}"; do
    if check_method "$UI_DIR/DeviceManagement.razor" "$method"; then
        ((implemented_methods++))
    fi
    ((total_methods++))
done

echo ""

# Check DeviceDetail.razor
echo "📱 DeviceDetail.razor:"
methods=("SetActiveTab" "TestConnection" "SaveDevice" "AddDataPoint" "EditDataPoint" "DeleteDataPoint")
for method in "${methods[@]}"; do
    if check_method "$UI_DIR/DeviceDetail.razor" "$method"; then
        ((implemented_methods++))
    fi
    ((total_methods++))
done

echo ""

# Check Drivers.razor
echo "🚗 Drivers.razor:"
methods=("ShowAddDriverModal" "ImportDrivers" "ExportDrivers" "SelectAvailableDriver" "TestConnection" "LoadTemplate" "StartDriver" "StopDriver")
for method in "${methods[@]}"; do
    if check_method "$UI_DIR/Drivers.razor" "$method"; then
        ((implemented_methods++))
    fi
    ((total_methods++))
done

echo ""

# Check ProjectManagement.razor
echo "📋 ProjectManagement.razor:"
methods=("ShowCreateProjectForm" "CheckApiHealth" "EditProject" "ActivateProject" "DeleteProject" "CreateProject")
for method in "${methods[@]}"; do
    if check_method "$UI_DIR/ProjectManagement.razor" "$method"; then
        ((implemented_methods++))
    fi
    ((total_methods++))
done

echo ""

# Check Tags.razor
echo "🏷️ Tags.razor:"
methods=("ShowCreateTagModal" "ImportTags" "ExportTags" "EditTag" "TestTag" "DeleteTag" "CreateTag")
for method in "${methods[@]}"; do
    if check_method "$UI_DIR/Tags.razor" "$method"; then
        ((implemented_methods++))
    fi
    ((total_methods++))
done

echo ""

# Check Monitoring.razor
echo "📊 Monitoring.razor:"
methods=("StartMonitoring" "StopMonitoring" "RefreshData" "ClearData" "ExportData")
for method in "${methods[@]}"; do
    if check_method "$UI_DIR/Monitoring.razor" "$method"; then
        ((implemented_methods++))
    fi
    ((total_methods++))
done

echo ""

# Check Settings.razor
echo "⚙️ Settings.razor:"
methods=("SaveSettings" "ResetToDefaults" "ExportSettings" "SetActiveSection" "GetActiveClass" "CreateBackup" "RestoreBackup")
for method in "${methods[@]}"; do
    if check_method "$UI_DIR/Settings.razor" "$method"; then
        ((implemented_methods++))
    fi
    ((total_methods++))
done

echo ""
echo "📊 SUMMARY:"
echo "==========="
percentage=$((implemented_methods * 100 / total_methods))

if [ $percentage -ge 90 ]; then
    color=$GREEN
elif [ $percentage -ge 70 ]; then
    color=$YELLOW  
else
    color=$RED
fi

echo -e "Total Methods Checked: $total_methods"
echo -e "Implemented: ${color}$implemented_methods${NC}"
echo -e "Missing: $((total_methods - implemented_methods))"
echo -e "Implementation Rate: ${color}${percentage}%${NC}"

echo ""
if [ $percentage -ge 90 ]; then
    echo "🎉 Excellent! Most button functions are implemented."
elif [ $percentage -ge 70 ]; then
    echo "⚠️  Good progress, some functions still need implementation."
else
    echo "🚨 Many button functions are missing implementation."
fi

echo ""
echo "🔧 To run full system test, execute:"
echo "   ./start-all-apis.sh"
echo "   Then access: http://localhost:5267"