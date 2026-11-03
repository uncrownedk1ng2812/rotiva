# Membuat installer Rotiva: dist\Rotiva-Setup-<versi>.exe
#
# Kebutuhan di PC developer:
#   winget install Microsoft.DotNet.SDK.8
#   winget install Gyan.FFmpeg
#   winget install JRSoftware.InnoSetup
#   winget install TheDocumentFoundation.LibreOffice
#   winget install JohnMacFarlane.Pandoc
# ImageMagick (portable), Ghostscript, dan 7-Zip diunduh otomatis (disimpan di dist\cache).
# Semua path relatif ke root repo; folder dist\ tidak ikut git (.gitignore).
#
# Pemakaian:  powershell -ExecutionPolicy Bypass -File installer\build.ps1 [-Version 1.0.1]

param([string]$Version = "1.0.0", [string]$MagickVersion = "7.1.2-34", [string]$GhostscriptVersion = "10080")

$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot
$stage = Join-Path $root "dist\stage"
$project = Join-Path $root "src\Rotiva\Rotiva.csproj"

function Find-First([string[]]$patterns, [string]$what) {
    foreach ($pattern in $patterns) {
        $hit = Get-ChildItem $pattern -ErrorAction SilentlyContinue | Sort-Object FullName -Descending | Select-Object -First 1
        if ($hit) { return $hit.FullName }
    }
    throw "$what tidak ditemukan. Lihat kebutuhan di bagian atas build.ps1."
}

$ffmpeg = Find-First @(
    "$env:LOCALAPPDATA\Microsoft\WinGet\Packages\Gyan.FFmpeg*\*\bin\ffmpeg.exe",
    "$env:ProgramFiles\ffmpeg\bin\ffmpeg.exe") "FFmpeg"
$libreOfficeDir = Split-Path (Split-Path (Find-First @("$env:ProgramFiles\LibreOffice\program\soffice.exe") "LibreOffice"))
$pandoc = Find-First @(
    "$env:LOCALAPPDATA\Pandoc\pandoc.exe",
    "$env:ProgramFiles\Pandoc\pandoc.exe") "Pandoc"
$iscc = Find-First @(
    "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe",
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe") "Inno Setup"

Write-Host "==> Publish aplikasi (self-contained, .NET ikut dibundel)"
if (Test-Path $stage) { Remove-Item $stage -Recurse -Force }
dotnet publish $project -c Release -r win-x64 --self-contained true `
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:DebugType=none -p:Version=$Version -o $stage
if ($LASTEXITCODE -ne 0) { throw "dotnet publish gagal" }

Write-Host "==> Bundel FFmpeg: $ffmpeg"
$ffmpegDest = New-Item -ItemType Directory -Force (Join-Path $stage "tools\ffmpeg")
Copy-Item $ffmpeg $ffmpegDest
Copy-Item (Join-Path (Split-Path (Split-Path $ffmpeg)) "LICENSE") (Join-Path $ffmpegDest "LICENSE.txt") -ErrorAction SilentlyContinue

# Harus versi portable: ImageMagick hasil installer mencari folder modul & config lewat registry
# (HKLM\SOFTWARE\ImageMagick), jadi gagal di PC yang tidak menginstal ImageMagick sendiri.
$magickName = "ImageMagick-$MagickVersion-portable-Q16-HDRI-x64"
$magickDir = Join-Path $root "dist\cache\$magickName"
if (-not (Test-Path (Join-Path $magickDir "magick.exe"))) {
    Write-Host "==> Unduh $magickName"
    $archive = Join-Path $root "dist\cache\$magickName.7z"
    New-Item -ItemType Directory -Force $magickDir | Out-Null
    Invoke-WebRequest "https://github.com/ImageMagick/ImageMagick/releases/download/$MagickVersion/$magickName.7z" -OutFile $archive
    # tar.exe bawaan Windows 11 bisa membuka .7z.
    & "$env:WINDIR\System32\tar.exe" -xf $archive -C $magickDir
    if ($LASTEXITCODE -ne 0) { throw "Gagal mengekstrak $archive" }
    Remove-Item $archive
}

Write-Host "==> Bundel ImageMagick: $magickDir"
$magickDest = Join-Path $stage "tools\imagemagick"
New-Item -ItemType Directory -Force $magickDest | Out-Null
# Hanya magick.exe yang dipakai; compare/convert/identify/dll. tidak perlu ikut.
Get-ChildItem $magickDir | Where-Object { ($_.Extension -ne ".exe" -or $_.Name -eq "magick.exe") -and $_.Name -ne "ChangeLog.md" } |
    Copy-Item -Destination $magickDest -Recurse -Force
# magick.exe portable butuh vcomp140.dll (OpenMP) dari Visual C++ runtime; bawa sendiri.
Copy-Item (Join-Path $env:WINDIR "System32\vcomp140.dll") $magickDest -Force

# 7-Zip: dibundel untuk fitur EKSTRAK (zip/rar/7z/tar.gz), dan dipakai di sini untuk membongkar
# installer Ghostscript. 7zr.exe hanya bisa membuka .7z, jadi dipakai untuk membongkar paket 7-Zip lengkap.
$cache = Join-Path $root "dist\cache"
$sevenZipDir = Join-Path $cache "7zip"
$sevenZip = Join-Path $sevenZipDir "7z.exe"
if (-not (Test-Path $sevenZip)) {
    Write-Host "==> Unduh 7-Zip"
    New-Item -ItemType Directory -Force $cache | Out-Null
    Invoke-WebRequest "https://www.7-zip.org/a/7zr.exe" -OutFile (Join-Path $cache "7zr.exe")
    Invoke-WebRequest "https://www.7-zip.org/a/7z2501-x64.exe" -OutFile (Join-Path $cache "7z-x64.exe")
    & (Join-Path $cache "7zr.exe") x -y "-o$sevenZipDir" (Join-Path $cache "7z-x64.exe") | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "Gagal mengekstrak 7-Zip" }
    Remove-Item (Join-Path $cache "7zr.exe"), (Join-Path $cache "7z-x64.exe")
}

Write-Host "==> Bundel 7-Zip: $sevenZipDir"
# 7z.exe butuh 7z.dll (berisi semua format, termasuk RAR untuk dibuka).
$sevenZipDest = New-Item -ItemType Directory -Force (Join-Path $stage "tools\7zip")
Copy-Item (Join-Path $sevenZipDir "7z.exe"), (Join-Path $sevenZipDir "7z.dll") $sevenZipDest
Copy-Item (Join-Path $sevenZipDir "License.txt") $sevenZipDest

# Ghostscript (kompres & gabung PDF) tidak ada di winget dan hanya tersedia sebagai installer NSIS
# yang minta hak admin. Isinya dibongkar dengan 7-Zip.
$gsDir = Join-Path $root "dist\cache\gs$GhostscriptVersion"
if (-not (Test-Path (Join-Path $gsDir "bin\gswin64c.exe"))) {
    Write-Host "==> Unduh Ghostscript $GhostscriptVersion"
    $gsSetup = Join-Path $cache "gs$GhostscriptVersion-w64.exe"
    Invoke-WebRequest "https://github.com/ArtifexSoftware/ghostpdl-downloads/releases/download/gs$GhostscriptVersion/gs${GhostscriptVersion}w64.exe" -OutFile $gsSetup
    & $sevenZip x -y "-o$gsDir" $gsSetup | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "Gagal mengekstrak $gsSetup" }
    Remove-Item $gsSetup
}

Write-Host "==> Bundel Ghostscript: $gsDir"
# Resource & font sudah tertanam di gsdll64.dll, jadi cukup dua file ini.
$gsDest = New-Item -ItemType Directory -Force (Join-Path $stage "tools\ghostscript")
Copy-Item (Join-Path $gsDir "bin\gswin64c.exe"), (Join-Path $gsDir "bin\gsdll64.dll") $gsDest
Copy-Item (Join-Path $gsDir "doc\COPYING") (Join-Path $gsDest "LICENSE.txt")

Write-Host "==> Bundel Pandoc: $pandoc"
$pandocDest = New-Item -ItemType Directory -Force (Join-Path $stage "tools\pandoc")
Copy-Item $pandoc $pandocDest
Copy-Item (Join-Path (Split-Path $pandoc) "COPYRIGHT.txt") $pandocDest -ErrorAction SilentlyContinue

Write-Host "==> Bundel LibreOffice: $libreOfficeDir"
$loDest = Join-Path $stage "tools\libreoffice"
robocopy $libreOfficeDir $loDest /E /NFL /NDL /NJH /NJS /NP | Out-Null
if ($LASTEXITCODE -ge 8) { throw "Gagal menyalin LibreOffice" }
# Buang bagian yang tidak dipakai untuk konversi headless (bantuan, kamus ejaan,
# galeri, template, Python, Java, tema ikon selain default).
$trim = @("help", "readmes", "CREDITS.fodt", "share\extensions", "share\gallery", "share\template",
          "share\wizards", "program\classes", "program\python.exe", "program\python-core-*")
foreach ($item in $trim) {
    Get-ChildItem (Join-Path $loDest $item) -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force
}
Get-ChildItem (Join-Path $loDest "share\config") -Filter "images_*.zip" |
    Where-Object Name -ne "images_colibre.zip" | Remove-Item -Force
# LibreOffice memakai Visual C++ runtime yang terinstal di Windows; bawa sendiri (app-local)
# supaya tetap jalan di PC yang belum punya VC++ Redistributable.
foreach ($dll in "vcruntime140.dll", "vcruntime140_1.dll", "msvcp140.dll", "msvcp140_1.dll", "msvcp140_2.dll",
                 "msvcp140_atomic_wait.dll", "msvcp140_codecvt_ids.dll", "concrt140.dll") {
    $system = Join-Path $env:WINDIR "System32\$dll"
    if (Test-Path $system) { Copy-Item $system (Join-Path $loDest "program") -Force }
}

Write-Host "==> Compile installer"
& $iscc /Q "/DAppVersion=$Version" "/DStageDir=$stage" "/DOutputDir=$(Join-Path $root 'dist')" (Join-Path $PSScriptRoot "Rotiva.iss")
if ($LASTEXITCODE -ne 0) { throw "Inno Setup gagal" }

$setup = Join-Path $root "dist\Rotiva-Setup-$Version.exe"
Write-Host ("==> Selesai: {0} ({1:N0} MB)" -f $setup, ((Get-Item $setup).Length / 1MB))
