# Uji paket MSIX yang sudah terpasang (dipakai workflow Windows installer di runner GitHub, bukan di
# PC sendiri: skrip ini menggerakkan mouse). Alurnya seperti pengguna: klik kanan tone.wav →
# "Convert with Rotiva" → roda muncul di kursor → klik MP3 → tone.mp3 harus jadi.
# Screenshot tiap langkah disimpan di -Out.

param([Parameter(Mandatory)][string]$Out)

$ErrorActionPreference = "Stop"
New-Item -ItemType Directory -Force $Out | Out-Null

Add-Type -AssemblyName System.Drawing, System.Windows.Forms
Add-Type @"
using System.Runtime.InteropServices;
public static class Input {
    [DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
    [DllImport("user32.dll")] public static extern void mouse_event(uint flags, int dx, int dy, uint data, System.UIntPtr extra);
    public static void Click() { mouse_event(0x02, 0, 0, 0, System.UIntPtr.Zero); System.Threading.Thread.Sleep(60); mouse_event(0x04, 0, 0, 0, System.UIntPtr.Zero); }
}
"@

function Shot([string]$name) {
    $bounds = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
    $bitmap = New-Object System.Drawing.Bitmap $bounds.Width, $bounds.Height
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.CopyFromScreen($bounds.Location, [System.Drawing.Point]::Empty, $bounds.Size)
    $bitmap.Save((Join-Path $Out "$name.png"), [System.Drawing.Imaging.ImageFormat]::Png)
    $graphics.Dispose(); $bitmap.Dispose()
}

$work = Join-Path $env:RUNNER_TEMP "msix-files"
New-Item -ItemType Directory -Force $work | Out-Null
& "$env:ProgramFiles\ffmpeg\bin\ffmpeg.exe" -hide_banner -loglevel error -y -f lavfi -i "sine=frequency=440:duration=2" (Join-Path $work "tone.wav")
if ($LASTEXITCODE -ne 0) { throw "ffmpeg could not make tone.wav" }

$screen = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
$cx = [int]($screen.Width / 2); $cy = [int]($screen.Height / 2)
"screen $($screen.Width)x$($screen.Height)" | Tee-Object -FilePath (Join-Path $Out "notes.txt")

$shell = New-Object -ComObject Shell.Application
$item = $shell.Namespace($work).ParseName("tone.wav")
$verbs = @($item.Verbs())
"verbs: " + (($verbs | ForEach-Object { $_.Name }) -join " | ") | Tee-Object -FilePath (Join-Path $Out "notes.txt") -Append
$verb = $verbs | Where-Object { $_.Name -replace "&", "" -like "*Rotiva*" } | Select-Object -First 1
if (-not $verb) { throw "'Convert with Rotiva' is not in the right-click menu of tone.wav" }

[Input]::SetCursorPos($cx, $cy) | Out-Null
$verb.DoIt()
Start-Sleep -Seconds 4
Get-Process Rotiva -ErrorAction SilentlyContinue | Format-Table Id, Path -AutoSize | Out-String | Tee-Object -FilePath (Join-Path $Out "notes.txt") -Append
Shot "1-wheel"

# Roda audio: MP3 di segmen atas (format asli WAV disembunyikan).
[Input]::SetCursorPos($cx, $cy - 160) | Out-Null
Start-Sleep -Milliseconds 400
Shot "2-hover"
[Input]::Click()

$result = Join-Path $work "tone.mp3"
$deadline = (Get-Date).AddSeconds(60)
while (-not (Test-Path $result) -and (Get-Date) -lt $deadline) { Start-Sleep -Milliseconds 500 }
Start-Sleep -Seconds 1
Shot "3-result"
Get-ChildItem $work | Format-Table Name, Length -AutoSize | Out-String | Tee-Object -FilePath (Join-Path $Out "notes.txt") -Append
if (-not (Test-Path $result)) { throw "tone.mp3 was not created" }

# "Start with Windows" dari manifest (desktop:StartupTask).
Get-AppxPackage | Where-Object { $_.InstallLocation -and (Test-Path (Join-Path $_.InstallLocation "Rotiva.exe")) } |
    Format-List Name, PackageFullName, InstallLocation | Out-String | Tee-Object -FilePath (Join-Path $Out "notes.txt") -Append
Get-Process Rotiva -ErrorAction SilentlyContinue | Stop-Process -Force
Write-Host "MSIX test passed"
