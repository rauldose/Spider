@echo off
echo Starting Spider IoT Platform - All APIs
echo =====================================

:: Start Device Management API (Port 5001)
echo Starting Device Management API on port 5001...
start "Device Management API" cmd /k "cd /d %~dp0src\BoundedContexts\DeviceManagement\Spider.DeviceManagement.API && dotnet run --urls http://localhost:5001"

:: Wait 3 seconds
timeout /t 3 /nobreak >nul

:: Start Data Acquisition API (Port 5003)  
echo Starting Data Acquisition API on port 5003...
start "Data Acquisition API" cmd /k "cd /d %~dp0src\BoundedContexts\DataAcquisition\Spider.DataAcquisition.API && dotnet run --urls http://localhost:5003"

:: Wait 3 seconds
timeout /t 3 /nobreak >nul

:: Start Connection Management API (Port 5005)
echo Starting Connection Management API on port 5005...
start "Connection Management API" cmd /k "cd /d %~dp0src\BoundedContexts\ConnectionManagement\Spider.ConnectionManagement.API && dotnet run --urls http://localhost:5005"

:: Wait 3 seconds
timeout /t 3 /nobreak >nul

:: Start Project Management API (Port 5007)
echo Starting Project Management API on port 5007...
start "Project Management API" cmd /k "cd /d %~dp0src\BoundedContexts\ProjectManagement\Spider.ProjectManagement.API && dotnet run --urls http://localhost:5007"

:: Wait 3 seconds
timeout /t 3 /nobreak >nul

:: Start Communication API (Port 5009)
echo Starting Communication API on port 5009...
start "Communication API" cmd /k "cd /d %~dp0src\BoundedContexts\Communication\Spider.Communication.API && dotnet run --urls http://localhost:5009"

:: Wait 5 seconds for APIs to start
timeout /t 5 /nobreak >nul

:: Start Blazor UI (Port 5267)
echo Starting Blazor UI on port 5267...
start "Spider Studio UI" cmd /k "cd /d %~dp0src\UI\SpiderStudio.BlazorServer && dotnet run --urls http://localhost:5267"

echo.
echo All APIs and UI starting...
echo.
echo Access URLs:
echo - Device Management API: http://localhost:5001/swagger
echo - Data Acquisition API: http://localhost:5003/swagger 
echo - Connection Management API: http://localhost:5005/swagger
echo - Project Management API: http://localhost:5007/swagger
echo - Communication API: http://localhost:5009/swagger
echo - Spider Studio UI: http://localhost:5267
echo.
echo Press any key to close this window (APIs will continue running)
pause >nul