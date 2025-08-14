@echo off
setlocal ENABLEDELAYEDEXPANSION

REM Usage: purge-and-import-atc.cmd [version]
REM Imports a small test sample first, then purges and imports the full WHO file from the repo.

set VER=%~1
if "%VER%"=="" set VER=2024-07-31
set BASE=http://localhost:8083
set SAMPLE=%~dp0..\test-data\ATC-sample.csv
set FULL=%~dp0..\ATC\WHO ATC-DDD 2024-07-31.csv

if not exist "%SAMPLE%" (
  echo [ERROR] Sample not found: %SAMPLE%
  exit /b 2
)
if not exist "%FULL%" (
  echo [ERROR] Full WHO file not found: %FULL%
  exit /b 3
)

echo [STEP 1/3] Importing ATC sample (purge=true) version=%VER%
curl -f -s -X POST "%BASE%/api/catalog/import/atc?version=%VER%^&purge=true" -F "file=@%SAMPLE%" || goto :err

echo [STEP 2/3] Purging ATC table
curl -f -s -X POST "%BASE%/api/catalog/import/atc?version=%VER%^&purge=true" -F "file=@%SAMPLE%" >NUL 2>&1 || goto :err

REM The previous step already purged; proceed to full import

echo [STEP 3/3] Importing full WHO ATC/DDD version=%VER%
curl -f -s -X POST "%BASE%/api/catalog/import/atc?version=%VER%" -F "file=@%FULL%" || goto :err

echo [DONE]
exit /b 0

:err
echo.
echo [ERROR] ATC import sequence failed.
exit /b 1
