; Inno Setup Script for YTDownloader
; 使用說明：
;   1. 先執行 dotnet publish，輸出至 publish\ 資料夾
;   2. 開啟此腳本，點選「Compile」或執行 ISCC.exe setup.iss
;   3. 安裝檔輸出至 installer\Output\

#define AppName "YTDownloader"
#define AppVersion "1.0.0"
#define AppPublisher "YourName"
#define AppURL "https://github.com/your-username/YTDownloader"
#define AppExeName "YTDownloader.exe"

; 發布輸出資料夾（相對於此 .iss 檔案所在的 installer\ 目錄）
#define PublishDir "..\publish"

[Setup]
AppId={{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppURL}
AppSupportURL={#AppURL}
AppUpdatesURL={#AppURL}
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
AllowNoIcons=yes
; 安裝檔輸出位置
OutputDir=Output
OutputBaseFilename=YTDownloader_Setup_v{#AppVersion}
SetupIconFile=
Compression=lzma
SolidCompression=yes
WizardStyle=modern
; 要求管理員權限（安裝到 Program Files 需要）
PrivilegesRequired=admin
; 最低 Windows 版本：Windows 10
MinVersion=10.0.17763
; 架構：x64
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; 複製 publish 資料夾內的所有檔案
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; 注意：yt-dlp.exe 與 ffmpeg.exe 已包含在 publish 輸出中（CopyToOutputDirectory: Always）

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{group}\{cm:UninstallProgram,{#AppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(AppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; 移除安裝時不存在、但執行期間產生的資料夾（logs 等）
Type: filesandordirs; Name: "{app}\logs"
