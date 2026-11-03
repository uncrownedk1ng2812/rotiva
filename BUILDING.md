# Building Rotiva

Rotiva is a C# / .NET 8 / WPF app for Windows 10 and 11.

## Layout

```
src/Rotiva/   the app (WPF)
installer/    Inno Setup script + build.ps1
assets/, id/, index.html, ...   the website (GitHub Pages)
```

| Part              | Choice                                     |
|-------------------|--------------------------------------------|
| Hooks & drag-drop | Win32 low-level hooks + OLE drop target (P/Invoke) |
| Wheel             | WPF Path/Geometry in a small transparent window at the cursor |
| Tray              | WinForms NotifyIcon + a custom WPF menu    |
| Images            | ImageMagick (portable)                     |
| Video / audio     | FFmpeg                                     |
| PDF               | Ghostscript                                |
| Documents         | LibreOffice (headless) and Pandoc          |
| Archives          | 7-Zip                                      |
| Installer         | Inno Setup 6                               |

## Run from source

```
winget install Microsoft.DotNet.SDK.8
dotnet run --project src/Rotiva
```

Conversions need the engines. During development Rotiva also finds them when they are installed
system-wide (FFmpeg, LibreOffice, Pandoc via winget).

## Build the installer

```
winget install Gyan.FFmpeg
winget install JRSoftware.InnoSetup
winget install TheDocumentFoundation.LibreOffice
winget install JohnMacFarlane.Pandoc
powershell -ExecutionPolicy Bypass -File installer\build.ps1 -Version 1.9.0
```

ImageMagick, Ghostscript and 7-Zip are downloaded automatically into `dist\cache`. The result is
`dist\Rotiva-Setup-<version>.exe`: a self-contained per-user installer (no admin rights, .NET bundled).

## License

GPL-3.0, see [LICENSE](LICENSE). If you publish a fork, give it a different name and logo
([TRADEMARKS.md](TRADEMARKS.md)).
