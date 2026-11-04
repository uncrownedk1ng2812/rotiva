# Membuat paket MSIX untuk Microsoft Store: dist\Rotiva-<versi>.msix
#
# Jalankan installer\build.ps1 dulu (isi dist\stage: aplikasi + semua engine). Butuh Windows SDK
# (makeappx.exe, makepri.exe) dan Visual Studio Build Tools dengan C++ untuk menu klik kanan
# (src\Rotiva.ShellExtension); keduanya sudah ada di runner GitHub windows-2025. Nilai identitas diambil dari
# Partner Center → Product management → Product identity. Paket untuk Store tidak perlu ditandatangani
# (Store menandatanganinya sendiri); -CertificatePath hanya untuk menguji pemasangan di PC/CI.
#
# Pemakaian:
#   powershell -ExecutionPolicy Bypass -File installer\build-msix.ps1 -Version 1.10.2 `
#       -IdentityName <Name> -Publisher "CN=..." -PublisherDisplayName "Denish Chairany"

param(
    [Parameter(Mandatory)][string]$Version,
    [Parameter(Mandatory)][string]$IdentityName,
    [Parameter(Mandatory)][string]$Publisher,
    [Parameter(Mandatory)][string]$PublisherDisplayName,
    [string]$CertificatePath,
    [string]$CertificatePassword
)

$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot
$stage = Join-Path $root "dist\stage"
$layout = Join-Path $root "dist\msix-layout"
$package = Join-Path $root "dist\Rotiva-$Version.msix"

if (-not (Test-Path (Join-Path $stage "Rotiva.exe"))) { throw "dist\stage belum ada: jalankan installer\build.ps1 dulu." }

function Find-SdkTool([string]$name) {
    $hit = Get-ChildItem "${env:ProgramFiles(x86)}\Windows Kits\10\bin\*\x64\$name" -ErrorAction SilentlyContinue |
        Sort-Object { [version]$_.Directory.Parent.Name } -Descending | Select-Object -First 1
    if (-not $hit) { throw "$name tidak ditemukan. Pasang Windows SDK." }
    return $hit.FullName
}
$makeappx = Find-SdkTool "makeappx.exe"
$makepri = Find-SdkTool "makepri.exe"

# Store memakai versi 4 angka dengan angka terakhir 0.
$parts = $Version.Split(".")
if ($parts.Count -ne 3) { throw "Versi harus X.Y.Z, misalnya 1.10.2" }
$packageVersion = "$Version.0"

Write-Host "==> Susun isi paket: $layout"
if (Test-Path $layout) { Remove-Item $layout -Recurse -Force }
robocopy $stage $layout /E /NFL /NDL /NJH /NJS /NP | Out-Null
if ($LASTEXITCODE -ge 8) { throw "Gagal menyalin dist\stage" }
# Sama dengan file lisensi yang dipasang installer biasa (Rotiva.iss).
Copy-Item (Join-Path $root "LICENSE") (Join-Path $layout "LICENSE.txt")
Copy-Item (Join-Path $root "NOTICE") (Join-Path $layout "NOTICE.txt")
Copy-Item (Join-Path $root "TRADEMARKS.md") (Join-Path $layout "TRADEMARKS.txt")
Copy-Item (Join-Path $root "THIRD-PARTY-NOTICES.md") (Join-Path $layout "THIRD-PARTY-NOTICES.txt")
Copy-Item (Join-Path $PSScriptRoot "msix\Assets") (Join-Path $layout "Assets") -Recurse

Write-Host "==> Compile menu klik kanan (RotivaShell.dll)"
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$vs = if (Test-Path $vswhere) { & $vswhere -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath }
if (-not $vs) { throw "Visual Studio Build Tools (C++) tidak ditemukan." }
$extension = Join-Path $root "src\Rotiva.ShellExtension"
$objDir = Join-Path $root "dist\shellext-obj"
New-Item -ItemType Directory -Force $objDir | Out-Null
# Lewat file .cmd: quoting argumen cl.exe lewat "cmd /c" dari PowerShell mudah rusak.
$batch = Join-Path $objDir "build.cmd"
@"
@echo off
call "$vs\VC\Auxiliary\Build\vcvars64.bat" >nul || exit /b 1
cl /nologo /O2 /W4 /EHsc /MT /LD /std:c++17 /DUNICODE /D_UNICODE /Fo"$objDir\ConvertCommand.obj" "$extension\ConvertCommand.cpp" /link /DEF:"$extension\RotivaShell.def" /OUT:"$layout\RotivaShell.dll" ole32.lib shell32.lib shlwapi.lib
"@ | Set-Content $batch -Encoding ASCII
& cmd /c $batch
if ($LASTEXITCODE -ne 0) { throw "Compile RotivaShell.dll gagal" }
Remove-Item (Join-Path $layout "RotivaShell.lib"), (Join-Path $layout "RotivaShell.exp") -ErrorAction SilentlyContinue

function Escape-Xml([string]$text) { [System.Security.SecurityElement]::Escape($text) }
$manifest = (Get-Content (Join-Path $PSScriptRoot "msix\AppxManifest.xml") -Raw -Encoding UTF8).
    Replace('$(IdentityName)', (Escape-Xml $IdentityName)).
    Replace('$(Publisher)', (Escape-Xml $Publisher)).
    Replace('$(PublisherDisplayName)', (Escape-Xml $PublisherDisplayName)).
    Replace('$(Version)', $packageVersion)
if ($manifest -match '\$\((IdentityName|Publisher|PublisherDisplayName|Version)\)') { throw "Masih ada placeholder di AppxManifest.xml" }
[System.IO.File]::WriteAllText((Join-Path $layout "AppxManifest.xml"), $manifest, (New-Object System.Text.UTF8Encoding $false))

# resources.pri supaya Windows memilih logo sesuai skala layar (scale-200, targetsize-24, ...).
# Diindeks dari folder kecil berisi Assets saja: folder layout penuh berisi ribuan file LibreOffice.
Write-Host "==> Buat resources.pri"
$priRoot = Join-Path $root "dist\msix-pri"
if (Test-Path $priRoot) { Remove-Item $priRoot -Recurse -Force }
New-Item -ItemType Directory -Force $priRoot | Out-Null
Copy-Item (Join-Path $layout "Assets") (Join-Path $priRoot "Assets") -Recurse
Copy-Item (Join-Path $layout "AppxManifest.xml") $priRoot
$priConfig = Join-Path $priRoot "priconfig.xml"
& $makepri createconfig /cf $priConfig /dq en-US /pv 10.0.0 /o | Out-Null
if ($LASTEXITCODE -ne 0) { throw "makepri createconfig gagal" }
& $makepri new /pr $priRoot /cf $priConfig /mn (Join-Path $priRoot "AppxManifest.xml") /of (Join-Path $layout "resources.pri") /o | Out-Null
if ($LASTEXITCODE -ne 0) { throw "makepri new gagal" }
Remove-Item $priRoot -Recurse -Force

Write-Host "==> Pack $package"
& $makeappx pack /d $layout /p $package /o
if ($LASTEXITCODE -ne 0) { throw "makeappx gagal" }

if ($CertificatePath) {
    Write-Host "==> Tanda tangan (uji): $CertificatePath"
    $signtool = Find-SdkTool "signtool.exe"
    & $signtool sign /fd SHA256 /f $CertificatePath /p $CertificatePassword $package
    if ($LASTEXITCODE -ne 0) { throw "signtool gagal" }
}

Write-Host ("==> Selesai: {0} ({1:N0} MB)" -f $package, ((Get-Item $package).Length / 1MB))
