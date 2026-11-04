; Installer Rotiva (Inno Setup 6). Jangan di-compile langsung; jalankan build.ps1
; yang menyiapkan folder StageDir (aplikasi + FFmpeg + ImageMagick) terlebih dulu.

#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif
#ifndef StageDir
  #define StageDir "..\dist\stage"
#endif
#ifndef OutputDir
  #define OutputDir "..\dist"
#endif

[Setup]
; AppId sama dengan ConvertWheel (nama lama) supaya instalasi lama di-upgrade, bukan dobel.
AppId={{8C1F5B7E-4D3A-4E0B-9C7A-2F6E1D9B3A51}
AppName=Rotiva
AppVersion={#AppVersion}
AppVerName=Rotiva {#AppVersion}
AppPublisher=Denish Chairany
AppPublisherURL=https://uncrownedk1ng2812.github.io/rotiva/
AppSupportURL=https://github.com/uncrownedk1ng2812/rotiva
AppCopyright=Copyright (c) 2026 Denish Chairany. Licensed under GPL-3.0.
VersionInfoVersion={#AppVersion}
VersionInfoCompany=Denish Chairany
VersionInfoCopyright=Copyright (c) 2026 Denish Chairany. Licensed under GPL-3.0.
; Halaman lisensi: ringkasan GPL-3.0 + aturan nama/logo (teks lengkap GPL ikut terpasang sebagai LICENSE.txt).
LicenseFile=..\NOTICE
; Per-user: tidak butuh hak admin.
PrivilegesRequired=lowest
DefaultDirName={localappdata}\Programs\Rotiva
; Jangan pakai folder instalasi sebelumnya (bisa jadi ...\Programs\ConvertWheel).
UsePreviousAppDir=no
DisableProgramGroupPage=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0
; Mutex yang sama dengan App.xaml.cs: installer/uninstaller meminta aplikasi ditutup dulu.
AppMutex=Rotiva.SingleInstance,ConvertWheel.SingleInstance
SetupIconFile=..\src\Rotiva\Assets\app.ico
UninstallDisplayIcon={app}\Rotiva.exe
UninstallDisplayName=Rotiva
OutputDir={#OutputDir}
OutputBaseFilename=Rotiva-Setup-{#AppVersion}
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Types]
Name: "full"; Description: "Full (images, video, audio, documents, spreadsheets)"
Name: "compact"; Description: "Compact (no document & spreadsheet conversion)"
Name: "custom"; Description: "Custom"; Flags: iscustom

[Components]
Name: "main"; Description: "Rotiva + image, video & audio conversion, PDF compress & merge, ZIP & archive extraction"; Types: full compact custom; Flags: fixed
Name: "documents"; Description: "Document, Excel/CSV spreadsheet & PowerPoint conversion (LibreOffice + Pandoc)"; Types: full

[Tasks]
Name: "autostart"; Description: "Start Rotiva automatically with Windows"; GroupDescription: "Additional options:"
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional options:"; Flags: unchecked

[Files]
Source: "..\LICENSE"; DestDir: "{app}"; DestName: "LICENSE.txt"; Flags: ignoreversion; Components: main
Source: "..\NOTICE"; DestDir: "{app}"; DestName: "NOTICE.txt"; Flags: ignoreversion; Components: main
Source: "..\TRADEMARKS.md"; DestDir: "{app}"; DestName: "TRADEMARKS.txt"; Flags: ignoreversion; Components: main
Source: "..\THIRD-PARTY-NOTICES.md"; DestDir: "{app}"; DestName: "THIRD-PARTY-NOTICES.txt"; Flags: ignoreversion; Components: main
Source: "{#StageDir}\*"; DestDir: "{app}"; Excludes: "\tools\libreoffice\*,\tools\pandoc\*"; Flags: ignoreversion recursesubdirs createallsubdirs; Components: main
Source: "{#StageDir}\tools\libreoffice\*"; DestDir: "{app}\tools\libreoffice"; Flags: ignoreversion recursesubdirs createallsubdirs; Components: documents
Source: "{#StageDir}\tools\pandoc\*"; DestDir: "{app}\tools\pandoc"; Flags: ignoreversion recursesubdirs createallsubdirs; Components: documents

[Icons]
Name: "{autoprograms}\Rotiva"; Filename: "{app}\Rotiva.exe"
Name: "{autodesktop}\Rotiva"; Filename: "{app}\Rotiva.exe"; Tasks: desktopicon

[Registry]
; Nilai yang sama dengan menu tray "Jalankan saat Windows menyala".
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "Rotiva"; ValueData: """{app}\Rotiva.exe"""; Tasks: autostart

[Run]
; Menu klik kanan Explorer (aplikasi juga menulis ulang menu ini setiap kali start).
Filename: "{app}\Rotiva.exe"; Parameters: "--register"; Flags: runhidden waituntilterminated
Filename: "{app}\Rotiva.exe"; Description: "Launch Rotiva now"; Flags: nowait postinstall skipifsilent

[UninstallRun]
Filename: "{app}\Rotiva.exe"; Parameters: "--unregister"; Flags: runhidden waituntilterminated; RunOnceId: "UnregisterShellMenu"

[InstallDelete]
; Sisa versi lama yang masih bernama ConvertWheel (menu klik kanannya dibersihkan di CurStepChanged).
Type: filesandordirs; Name: "{localappdata}\Programs\ConvertWheel"
Type: filesandordirs; Name: "{localappdata}\ConvertWheel"
Type: files; Name: "{autoprograms}\ConvertWheel.lnk"
Type: files; Name: "{autodesktop}\ConvertWheel.lnk"

[UninstallDelete]
; Profil LibreOffice sementara yang dibuat saat konversi dokumen.
Type: filesandordirs; Name: "{localappdata}\Rotiva"

[Code]
const
  RunKey = 'Software\Microsoft\Windows\CurrentVersion\Run';
  UninstallKey = 'Software\Microsoft\Windows\CurrentVersion\Uninstall\{8C1F5B7E-4D3A-4E0B-9C7A-2F6E1D9B3A51}_is1';

var
  OldAutostart: Boolean;

procedure CurStepChanged(CurStep: TSetupStep);
var
  OldDir, OldExe: String;
  ResultCode: Integer;
begin
  // Dipanggil sebelum [InstallDelete]: exe lama masih ada untuk menghapus menu klik kanannya sendiri.
  if CurStep = ssInstall then
  begin
    OldAutostart := RegValueExists(HKCU, RunKey, 'ConvertWheel');
    if RegQueryStringValue(HKCU, UninstallKey, 'InstallLocation', OldDir) then
    begin
      OldExe := AddBackslash(OldDir) + 'ConvertWheel.exe';
      if FileExists(OldExe) then
      begin
        Exec(OldExe, '--unregister', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
        // Folder pilihan pengguna tidak dihapus seluruhnya; folder default dihapus lewat [InstallDelete].
        DeleteFile(OldExe);
      end;
    end;
    RegDeleteValue(HKCU, RunKey, 'ConvertWheel');
    RegDeleteKeyIncludingSubkeys(HKCU, 'Software\ConvertWheel');
  end;
  // Pengguna yang dulu menyalakan autostart ConvertWheel tetap mendapat autostart.
  if (CurStep = ssPostInstall) and OldAutostart then
    RegWriteStringValue(HKCU, RunKey, 'Rotiva', '"' + ExpandConstant('{app}\Rotiva.exe') + '"');
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  // Autostart bisa dinyalakan dari menu tray walau tidak dicentang saat instalasi,
  // jadi selalu dihapus saat uninstall.
  // Pengaturan dari menu tray (misal menu klik kanan dimatikan) juga dihapus.
  if CurUninstallStep = usPostUninstall then
  begin
    RegDeleteValue(HKCU, RunKey, 'Rotiva');
    RegDeleteKeyIncludingSubkeys(HKCU, 'Software\Rotiva');
  end;
end;
