# Building Rotiva

Rotiva is a C# / .NET 8 / WPF app for Windows 10 and 11.

## Layout

```
src/Rotiva.Core/        conversion core: formats, engines, queue (plain .NET 8, any OS)
src/Rotiva.Core.Tests/  xUnit tests for the core (run on Windows, macOS and Linux in CI)
src/Rotiva/             the Windows app (WPF): wheel, hooks, tray, toast, Explorer menu, Windows OCR
installer/              Inno Setup script + build.ps1
assets/, id/, index.html, ...   the website (GitHub Pages)
```

The core has no UI and no Windows-only APIs. Anything platform-specific is plugged in by the app:
OCR through `ITextRecognizer` (`Converter.TextRecognizer`), progress and results through
`IConversionReporter`. A future macOS/Linux app reuses the core and provides its own UI and these two
pieces.

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
dotnet test src/Rotiva.Core.Tests     # works on macOS and Linux too
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
