# Building Rotiva

Rotiva is a C# / .NET 8 / WPF app for Windows 10 and 11.

## Layout

```
src/Rotiva.Core/        conversion core: formats, engines, queue (plain .NET 8, any OS)
src/Rotiva.Core.Tests/  xUnit tests for the core (run on Windows, macOS and Linux in CI)
src/Rotiva/             the Windows app (WPF): wheel, hooks, tray, toast, Explorer menu, Windows OCR
src/Rotiva.Desktop/     macOS & Linux app (Avalonia): click-mode wheel, toast, tray
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

## macOS & Linux

`src/Rotiva.Desktop` runs the same conversion core with an Avalonia UI. It also runs on Windows, which
is handy for testing.

```
dotnet run --project src/Rotiva.Desktop -- ~/Pictures/photo.heic    # opens the wheel for that file
dotnet run --project src/Rotiva.Desktop                             # tray / menu bar only
```

- From **Open With**, the tray or the command line, the wheel opens in **click mode** right at the mouse cursor (where you right-clicked or dropped the
  file; the center of the screen if the cursor position can't be read, e.g. pure Wayland or WSL):
  point at a format and click (or use the arrow keys + Enter). Click the center or press Esc to
  cancel. Pick files from the tray menu (**Convert files…**), or launch `Rotiva <files>` again while
  it's running: the files go to the running instance.
- **Linux file managers**: on start Rotiva adds itself (per user, no root) to **Open With**
  (`~/.local/share/applications/rotiva.desktop`), Nautilus (**Scripts → Convert with Rotiva**),
  Dolphin and Nemo (**Convert with Rotiva**). Turn it off with **File manager menu** in the tray.
- Engines come from the system: `brew install ffmpeg imagemagick ghostscript pandoc sevenzip tesseract` and
  LibreOffice.app on macOS, or `apt install ffmpeg imagemagick ghostscript pandoc 7zip libreoffice`
  on Debian/Ubuntu. Images need **ImageMagick 7** (the `magick` command): Debian 13 has it, but
  Ubuntu 24.04 still ships version 6, so there use the AppImage from imagemagick.org on your `PATH`.
- **Drag + Shift** works like on Windows: Rotiva polls the left button and Shift (X11
  `XQueryPointer`, macOS `CGEventSource`) and opens an invisible drop window at the cursor; the wheel
  only shows when files are dragged in. Needs X11/XWayland on Linux. Tray switch: **Drag + Shift opens
  the wheel**.
- **TEXT (OCR)** uses Tesseract when it's installed (`brew install tesseract`, `apt install
  tesseract-ocr`; add `tesseract-lang` / `tesseract-ocr-ind` for Indonesian).
- **macOS app:** `installer/build-macos.sh 1.10.0 arm64` (or `x64`) builds `Rotiva.app` (menu bar only,
  listed in Finder **Open With**) and a `.dmg`, signed ad hoc. Not notarized yet (needs a paid Apple
  Developer account), so macOS asks for confirmation the first time.
- Not there yet: notarized `.dmg`. On GNOME the tray icon needs the AppIndicator extension.

**AppImage (Linux).** `installer/build-appimage.sh 1.10.0` builds `dist/Rotiva-1.10.0-x86_64.AppImage`
(needs Linux x86_64, the .NET 8 SDK and curl; appimagetool and the AppImage runtime are downloaded at
pinned versions and checked against their SHA-256). The
[Linux AppImage workflow](https://github.com/uncrownedk1ng2812/rotiva/actions/workflows/linux-appimage.yml)
builds it on every push, runs it on a virtual X11 screen, converts a file with it and keeps the
AppImage as an artifact. To use it: `chmod +x Rotiva-*.AppImage && ./Rotiva-*.AppImage`. Without FUSE
(containers, WSL) run it with `APPIMAGE_EXTRACT_AND_RUN=1`. The engines are not inside the AppImage;
install them from your distribution as above.

Every push also builds unsigned test builds for `linux-x64`, `osx-arm64` and `osx-x64` as artifacts of the
[Build workflow](https://github.com/uncrownedk1ng2812/rotiva/actions/workflows/build.yml). On macOS,
right-click → Open the first time (the app isn't notarized), or run `xattr -dr com.apple.quarantine`
on it.

## Build the installer

```
winget install Gyan.FFmpeg
winget install JRSoftware.InnoSetup
winget install TheDocumentFoundation.LibreOffice
winget install JohnMacFarlane.Pandoc
powershell -ExecutionPolicy Bypass -File installer\build.ps1 -Version 1.10.0
```

ImageMagick, Ghostscript and 7-Zip are downloaded automatically into `dist\cache`. The result is
`dist\Rotiva-Setup-<version>.exe`: a self-contained per-user installer (no admin rights, .NET bundled).

## License

GPL-3.0, see [LICENSE](LICENSE). If you publish a fork, give it a different name and logo
([TRADEMARKS.md](TRADEMARKS.md)).
