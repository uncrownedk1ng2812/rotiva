# Screenshot untuk listing Microsoft Store (min. 1366x768), dari paket MSIX yang sudah terpasang di
# runner GitHub. Menggerakkan mouse: jangan dijalankan di PC sendiri. Hasil: -Out\store-*.png

param([Parameter(Mandatory)][string]$Out)

$ErrorActionPreference = "Stop"
New-Item -ItemType Directory -Force $Out | Out-Null
Add-Type -AssemblyName System.Drawing, System.Windows.Forms
Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class Input {
    [DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
    [DllImport("user32.dll")] public static extern void mouse_event(uint flags, int dx, int dy, uint data, UIntPtr extra);
    [DllImport("user32.dll")] public static extern void keybd_event(byte vk, byte scan, uint flags, UIntPtr extra);
    public static void Click() { mouse_event(0x02, 0, 0, 0, UIntPtr.Zero); System.Threading.Thread.Sleep(60); mouse_event(0x04, 0, 0, 0, UIntPtr.Zero); }
    public static void Escape() { keybd_event(0x1B, 0, 0, UIntPtr.Zero); keybd_event(0x1B, 0, 2, UIntPtr.Zero); }
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

# Contoh file yang dibuat sendiri (tanpa engine): foto bergradasi, WAV, Markdown, folder.
$work = Join-Path $env:USERPROFILE "Documents\Holiday trip"
New-Item -ItemType Directory -Force $work | Out-Null
function New-Photo([string]$path, [string]$from, [string]$to) {
    $bitmap = New-Object System.Drawing.Bitmap 1600, 1067
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $rect = New-Object System.Drawing.Rectangle 0, 0, 1600, 1067
    $brush = New-Object System.Drawing.Drawing2D.LinearGradientBrush $rect, ([System.Drawing.ColorTranslator]::FromHtml($from)), ([System.Drawing.ColorTranslator]::FromHtml($to)), 60
    $graphics.FillRectangle($brush, $rect)
    $sun = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(200, 255, 220, 150))
    $graphics.FillEllipse($sun, 1050, 180, 260, 260)
    $hill = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(230, 30, 40, 70))
    $graphics.FillPolygon($hill, [System.Drawing.Point[]]@((New-Object System.Drawing.Point 0, 1067), (New-Object System.Drawing.Point 0, 700), (New-Object System.Drawing.Point 500, 520), (New-Object System.Drawing.Point 900, 760), (New-Object System.Drawing.Point 1300, 600), (New-Object System.Drawing.Point 1600, 720), (New-Object System.Drawing.Point 1600, 1067)))
    $bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Jpeg)
    $graphics.Dispose(); $bitmap.Dispose()
}
New-Photo (Join-Path $work "beach.jpg") "#ff8a3d" "#6a3df0"
New-Photo (Join-Path $work "sunset.jpg") "#ffb347" "#ff3d6e"
New-Photo (Join-Path $work "mountains.jpg") "#3dc1ff" "#1d3b8f"

$rate = 44100; $samples = $rate * 3
$wav = New-Object System.IO.MemoryStream
$w = New-Object System.IO.BinaryWriter $wav
$w.Write([Text.Encoding]::ASCII.GetBytes("RIFF")); $w.Write([int](36 + $samples * 2)); $w.Write([Text.Encoding]::ASCII.GetBytes("WAVEfmt "))
$w.Write([int]16); $w.Write([int16]1); $w.Write([int16]1); $w.Write([int]$rate); $w.Write([int]($rate * 2)); $w.Write([int16]2); $w.Write([int16]16)
$w.Write([Text.Encoding]::ASCII.GetBytes("data")); $w.Write([int]($samples * 2))
for ($i = 0; $i -lt $samples; $i++) { $w.Write([int16](8000 * [Math]::Sin(2 * [Math]::PI * 440 * $i / $rate))) }
[IO.File]::WriteAllBytes((Join-Path $work "voice note.wav"), $wav.ToArray())

Set-Content (Join-Path $work "itinerary.md") "# Holiday trip`n`n- Day 1: beach`n- Day 2: mountains`n" -Encoding UTF8
New-Item -ItemType Directory -Force (Join-Path $work "Photos to share") | Out-Null
Copy-Item (Join-Path $work "*.jpg") (Join-Path $work "Photos to share")

# Roda besar (setting dibaca dari HKCU; paket membaca registry asli kalau belum punya salinan sendiri).
New-Item -Path "HKCU:\Software\Rotiva" -Force | Out-Null
Set-ItemProperty "HKCU:\Software\Rotiva" -Name WheelSize -Value 2 -Type DWord

$screen = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
"screen $($screen.Width)x$($screen.Height)"

# Jendela lain disembunyikan, lalu Explorer dibuka di folder contoh, tampilan ikon besar.
(New-Object -ComObject Shell.Application).MinimizeAll()
Start-Sleep -Seconds 1
Start-Process explorer.exe $work
Start-Sleep -Seconds 4
$explorer = (New-Object -ComObject Shell.Application).Windows() | Where-Object { $_.LocationURL -like "*Holiday*" } | Select-Object -First 1
if ($explorer) {
    $explorer.Left = 120; $explorer.Top = 80; $explorer.Width = $screen.Width - 240; $explorer.Height = $screen.Height - 200
    try { $explorer.Document.CurrentViewMode = 1 } catch { }
}
Start-Sleep -Seconds 2
Shot "store-0-explorer"

$shell = New-Object -ComObject Shell.Application
$folder = $shell.Namespace($work)
$cx = [int]($screen.Width / 2); $cy = [int]($screen.Height / 2) + 40

# Tiap adegan: buka roda lewat menu "Convert with Rotiva" di file itu, arahkan ke satu format, foto.
$scenes = @(
    @{ File = "beach.jpg"; Angle = 60; Name = "store-1-image" },
    @{ File = "voice note.wav"; Angle = 0; Name = "store-2-audio" },
    @{ File = "itinerary.md"; Angle = 0; Name = "store-3-document" },
    @{ File = "Photos to share"; Angle = 0; Name = "store-4-folder" }
)
foreach ($scene in $scenes) {
    $verb = @($folder.ParseName($scene.File).Verbs()) | Where-Object { $_.Name -replace "&", "" -like "*Rotiva*" } | Select-Object -First 1
    if (-not $verb) { throw "No 'Convert with Rotiva' on $($scene.File)" }
    [Input]::SetCursorPos($cx, $cy) | Out-Null
    $verb.DoIt()
    Start-Sleep -Seconds 4
    $radians = $scene.Angle * [Math]::PI / 180
    [Input]::SetCursorPos($cx + [int](200 * [Math]::Sin($radians)), $cy - [int](200 * [Math]::Cos($radians))) | Out-Null
    Start-Sleep -Milliseconds 700
    Shot $scene.Name
    [Input]::Escape()
    Start-Sleep -Seconds 1
}

# Konversi sungguhan: foto → PNG, lalu notifikasi "Saved as ...".
$verb = @($folder.ParseName("sunset.jpg").Verbs()) | Where-Object { $_.Name -replace "&", "" -like "*Rotiva*" } | Select-Object -First 1
[Input]::SetCursorPos($cx, $cy) | Out-Null
$verb.DoIt()
Start-Sleep -Seconds 4
[Input]::SetCursorPos($cx, $cy - 200) | Out-Null
Start-Sleep -Milliseconds 500
[Input]::Click()
$deadline = (Get-Date).AddSeconds(60)
while (-not (Get-ChildItem $work -Filter "sunset*" | Where-Object Extension -ne ".jpg") -and (Get-Date) -lt $deadline) { Start-Sleep -Milliseconds 300 }
Start-Sleep -Milliseconds 600
Shot "store-5-done"
Get-ChildItem $work | Format-Table Name, Length -AutoSize | Out-String
