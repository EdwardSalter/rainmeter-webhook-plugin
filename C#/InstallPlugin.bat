SET rm="C:\Program Files\Rainmeter\Rainmeter.exe"

%rm% !Quit

"C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\msbuild.exe" SDK-CS.sln

COPY "%~dp0PluginEmpty\x64\Debug\Empty.dll" "C:\Program Files\Rainmeter\Plugins\Empty.dll"

REM START "" "%~dp0LaunchRainmeter.bat"
START "" %rm%


sleep 5

%rm% !About
%rm% !Manage Skins MySkins\Plex Plex_Webhook.ini