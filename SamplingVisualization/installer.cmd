@echo off

echo Creating executable...
pyinstaller main.py --onefile --windowed --name "SamplingVisualization" --paths=modules --paths=common --distpath ..\x64\Debug

IF ERRORLEVEL 1 (
	echo Error: Failed to create the executable.
	pause
) ELSE (
	echo Executable created successfully in the ..\x64\Debug folder.
	exit /b 0
)