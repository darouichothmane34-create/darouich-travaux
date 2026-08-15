#define MyAppName "Darouich Travaux"
#define MyAppExeName "DarouichTravaux.exe"
[Setup]
AppId={{81E66914-946B-432E-B103-73DDA5F83C04}
AppName={#MyAppName}
AppVersion=1.0.0
DefaultDirName={autopf}\Darouich Travaux
DefaultGroupName={#MyAppName}
OutputDir=output
OutputBaseFilename=DarouichTravauxSetup
Compression=lzma2
SolidCompression=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayIcon={app}\{#MyAppExeName}
[Files]
Source: "..\artifacts\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
[Tasks]
Name: "desktopicon"; Description: "Créer un raccourci sur le Bureau"; GroupDescription: "Raccourcis :"; Flags: checkedonce
[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Lancer {#MyAppName}"; Flags: nowait postinstall skipifsilent
