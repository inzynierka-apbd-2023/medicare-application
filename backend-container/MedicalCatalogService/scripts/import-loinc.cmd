@echo off
setlocal ENABLEDELAYEDEXPANSION

REM Usage: import-loinc.cmd <folder-with-CSV> [version]
REM Example: import-loinc.cmd C:\data\loinc 2.81

if "%~1"=="" (
  echo Usage: %~nx0 ^<folder-with-CSV^> [version]
  exit /b 1
)

set DIR=%~1
set VER=%~2
if "%VER%"=="" set VER=2.81
set BASE=http://localhost:8083

REM Map well-known file locations in the LOINC distribution
set LOINC=%DIR%\LoincTable\Loinc.csv
set MAPTO=%DIR%\LoincTable\MapTo.csv
set ANSWERS=%DIR%\AccessoryFiles\AnswerFile\AnswerList.csv
set LINK=%DIR%\AccessoryFiles\AnswerFile\LoincAnswerListLink.csv
set PANELS_AND_FORMS=%DIR%\AccessoryFiles\PanelsAndForms\PanelsAndForms.csv

if not exist "%LOINC%" echo [WARN] Missing Loinc.csv at "%LOINC%"
if not exist "%MAPTO%" echo [WARN] Missing MapTo.csv at "%MAPTO%"
if not exist "%ANSWERS%" echo [WARN] Missing AnswerList.csv at "%ANSWERS%"
if not exist "%LINK%" echo [WARN] Missing LoincAnswerListLink.csv at "%LINK%"
if not exist "%ALIASES%" echo [WARN] Missing LoincAliases.csv at "%ALIASES%"
if not exist "%PANELS%" echo [WARN] Missing LoincPanels.csv at "%PANELS%"
if not exist "%PANELITEMS%" echo [WARN] Missing LoincPanelItems.csv at "%PANELITEMS%"

if exist "%LOINC%" (
  echo [INFO] Importing LOINC main (purge=true) version=%VER%
  curl -f -s -X POST "%BASE%/api/catalog/import/loinc?version=%VER%&purge=true" -F "file=@%LOINC%" || goto :err
  echo.
)

if exist "%MAPTO%" (
  echo [INFO] Importing LOINC MapTo version=%VER%
  curl -f -s -X POST "%BASE%/api/catalog/import/loinc-mapto?version=%VER%" -F "file=@%MAPTO%" || goto :err
  echo.
)

if exist "%ANSWERS%" if exist "%LINK%" (
  echo [INFO] Importing LOINC Answers version=%VER%
  curl -f -s -X POST "%BASE%/api/catalog/import/loinc-answers?version=%VER%" -F "answerList=@%ANSWERS%" -F "listLink=@%LINK%" || goto :err
  echo.
)

if exist "%LOINC%" (
  echo [INFO] Importing LOINC Aliases version=%VER%
  curl -f -s -X POST "%BASE%/api/catalog/import/loinc?version=%VER%^&purge=true" -F "file=@%LOINC%" || goto :err
  echo.
)

if exist "%PANELS%" (
  echo [INFO] Importing LOINC Panels version=%VER%
  curl -f -s -X POST "%BASE%/api/catalog/import/loinc-mapto?version=%VER%" -F "file=@%MAPTO%" || goto :err
  echo.
)

if exist "%PANELITEMS%" (
  echo [INFO] Importing LOINC Panel Items version=%VER%
  curl -f -s -X POST "%BASE%/api/catalog/import/loinc-panel-items?version=%VER%" -F "file=@%PANELITEMS%" || goto :err
  echo.
)

if exist "%PANELS_AND_FORMS%" (
  echo [INFO] Importing LOINC Panels and Forms version=%VER%
  curl -f -s -X POST "%BASE%/api/catalog/import/loinc-panels-and-forms?version=%VER%^&purge=true" -F "file=@%PANELS_AND_FORMS%" || goto :err
  echo.
)

echo [DONE]
exit /b 0

:err
echo.
echo [ERROR] An import step failed. See output above.
exit /b 1
