; Script de Inno Setup para PingMonitorApp
; Descargar Inno Setup de: https://jrsoftware.org/isdl.php

[Setup]
AppName=PingMonitor MOPT
AppVersion=2.0
DefaultDirName={autopf}\PingMonitorMOPT
DefaultGroupName=PingMonitor MOPT
OutputDir=.
OutputBaseFilename=PingMonitor_Setup
Compression=lzma
SolidCompression=yes
PrivilegesRequired=admin

[Files]
Source: "c:\respos_mop\PingMonitorApp\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\PingMonitor MOPT"; Filename: "{app}\PingMonitorApp.exe"
Name: "{autodesktop}\PingMonitor MOPT"; Filename: "{app}\PingMonitorApp.exe"

[Run]
Filename: "{app}\PingMonitorApp.exe"; Description: "Lanzar PingMonitor"; Flags: nowait postinstall skipifsilent
