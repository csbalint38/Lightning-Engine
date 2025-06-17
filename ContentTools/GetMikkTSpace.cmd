@echo off
setlocal

set "TARGET_DIR=..\packages\MikkTSpace"

if not exist "packages" (
    mkdir packages
)

if exist "%TARGET_DIR%" (
    echo Directory %TARGET_DIR% already exists. Skipping clone...
    goto :eof
)

echo Cloning MikkTSpace repository into %TARGET_DIR%...

git clone https://github.com/mmikk/MikkTSpace "%TARGET_DIR%"

if exist "%TARGET_DIR%" (
    echo MikkTSpace successfully cloned.
) else (
    echo Failed to clone MikkTSpace.
)

endlocal
pause