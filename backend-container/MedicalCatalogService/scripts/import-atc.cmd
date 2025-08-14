@echo off
setlocal ENABLEDELAYEDEXPANSION

REM Usage: import-atc.cmd <file> [version]
REM Example: import-atc.cmd "D:\\projects\\medicare-application\\backend-container\\MedicalCatalogService\\ATC\\WHO ATC-DDD 2024-07-31.csv" 2024-07-31

if "%~1"=="" (
  echo Usage: %~nx0 ^<file^> [version]
  exit /b 1
)

set FILE=%~1
set VER=%~2
if "%VER%"=="" set VER=2024-07-31
set BASE=http://localhost:8083

if not exist "%FILE%" (
  echo [ERROR] File not found: %FILE%
  exit /b 2
)

echo [INFO] Importing ATC/DDD sample (purge=true) version=%VER%
curl -f -s -X POST "%BASE%/api/catalog/import/atc?version=%VER%^&purge=true" -F "file=@%FILE%" || goto :err

echo [DONE]
exit /b 0

:err
echo.
echo [ERROR] ATC import failed. See output above.
exit /b 1
