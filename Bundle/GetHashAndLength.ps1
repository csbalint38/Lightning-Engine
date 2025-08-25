Invoke-WebRequest "https://aka.ms/vs/17/release/vs_BuildTools.exe" -OutFile .\vs_BuildTools.exe

(Get-FileHash .\vs_BuildTools.exe -Algorithm SHA512).Hash 

(gci .\vs_BuildTools.exe).Length

pause

Remove-Item .\vs_BuildTools.exe