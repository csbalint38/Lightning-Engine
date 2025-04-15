@echo off

fxc ChannelSelect.hlsl /E main /T ps_3_0 /O3 /Fo ChannelSelect.cso

IF %ERRORLEVEL% NEQ 0 (
	echo Shader compilation failed!
	pause
) ELSE (
	echo Shader compiled successfully.
)