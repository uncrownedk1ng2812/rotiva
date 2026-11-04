<!-- languages -->
<p align="center">
  <b>English</b> ·
  <a href="docs/readme/README.es.md">Español</a> ·
  <a href="docs/readme/README.ja.md">日本語</a> ·
  <a href="docs/readme/README.ko.md">한국어</a> ·
  <a href="docs/readme/README.zh-CN.md">简体中文</a> ·
  <a href="docs/readme/README.zh-TW.md">繁體中文</a> ·
  <a href="docs/readme/README.pl.md">Polski</a> ·
  <a href="docs/readme/README.hi.md">हिन्दी</a> ·
  <a href="docs/readme/README.ru.md">Русский</a> ·
  <a href="docs/readme/README.ar.md">العربية</a> ·
  <a href="docs/readme/README.pt-BR.md">Português (Brasil)</a> ·
  <a href="docs/readme/README.pt-PT.md">Português (Portugal)</a> ·
  <a href="docs/readme/README.fr.md">Français</a> ·
  <a href="docs/readme/README.th.md">ไทย</a> ·
  <a href="docs/readme/README.tr.md">Türkçe</a> ·
  <a href="docs/readme/README.id.md">Bahasa Indonesia</a> ·
  <a href="docs/readme/README.it.md">Italiano</a>
</p>
<!-- /languages -->

<p align="center">
  <img src="assets/og.png" alt="Rotiva — drag a file, press Shift, drop it on the wheel" width="720">
</p>

<p align="center">
  <b>Convert files without opening any app.</b><br>
  Drag a file, press <kbd>Shift</kbd>, drop it on the wheel — done.
</p>

<p align="center">
  <a href="https://github.com/uncrownedk1ng2812/rotiva/releases/latest"><img src="https://img.shields.io/github/v/release/uncrownedk1ng2812/rotiva?label=version&color=ff8a1f" alt="Latest version"></a>
  <!-- Re-enable once downloads pass ~500:
  <a href="https://github.com/uncrownedk1ng2812/rotiva/releases"><img src="https://img.shields.io/github/downloads/uncrownedk1ng2812/rotiva/total?label=downloads&color=ff8a1f" alt="Total downloads"></a>
  -->
  <img src="https://img.shields.io/badge/Windows-10%20%7C%2011-0078d4" alt="Windows 10 | 11">
  <img src="https://img.shields.io/badge/macOS-Apple%20Silicon%20%7C%20Intel-555" alt="macOS: Apple Silicon | Intel">
  <img src="https://img.shields.io/badge/Linux-AppImage-e95420" alt="Linux AppImage">
  <img src="https://img.shields.io/badge/price-free-1f9d55" alt="Free">
  <img src="https://img.shields.io/badge/100%25-offline-5a5a66" alt="100% offline">
  <a href="LICENSE"><img src="https://img.shields.io/badge/license-GPL--3.0-blue" alt="License: GPL-3.0"></a>
</p>

<p align="center">
  <a href="https://uncrownedk1ng2812.github.io/rotiva/"><b>🌐 Website & live demo</b></a>
  &nbsp;·&nbsp;
  <a href="https://uncrownedk1ng2812.github.io/rotiva/id/"><b>🇮🇩 Bahasa Indonesia</b></a>
  &nbsp;·&nbsp;
  <a href="https://github.com/uncrownedk1ng2812/rotiva/releases/latest"><b>⬇️ Download the installer</b></a>
  &nbsp;·&nbsp;
  <a href="https://ko-fi.com/denishchairany"><b>☕ Support on Ko-fi</b></a>
</p>

---

> [!NOTE]
> **Rotiva was built from scratch by Denish Chairany** and is open source under [GPL-3.0](LICENSE).
> Forks are welcome, but they must stay open source, credit the author, and use **a different name and
> logo**. See [TRADEMARKS.md](TRADEMARKS.md).

## What is Rotiva?

Rotiva is a small Windows app that lives in your system tray. While you're dragging a photo, video,
document or any other file, press **Shift** and a format wheel pops up right at your cursor. Point at the
format you want and let go — the converted file is saved right next to the original.

No uploading to online converters, no ads, no queues.

<p align="center">
  <img src="assets/readme/demo.gif" alt="Demo: drag a file, press Shift, pick a format, drop" width="320">
  &nbsp;&nbsp;
  <img src="assets/readme/wheel.png" alt="The format wheel for an image file" width="420">
</p>

## How it works

| | Step | Details |
|---|---|---|
| **1** | **Drag a file** | From Explorer, the desktop or any folder. One file or several at once. |
| **2** | **Press <kbd>Shift</kbd>** | While still holding the left mouse button. The format wheel appears right at your cursor. |
| **3** | **Point & drop** | The direction of your cursor picks the format — no need to land exactly on a segment. Drop in the center to cancel. |

Prefer right-click? Everything on the wheel is also in the Explorer right-click menu under
**Rotiva ▸** (Windows 11: *Show more options*).

## Supported formats

The wheel adapts to the file you drag. The file's own format is hidden automatically.

| Type | From | To |
|---|---|---|
| 🖼️ Images | PNG, JPG, WEBP, AVIF, HEIC, BMP, TIFF, GIF, ICO | PNG, JPG, WEBP, AVIF, PDF, ICO, TIFF, **Compress**, **Text (OCR)** |
| 🎬 Video | MP4, MKV, MOV, AVI, WEBM, WMV, FLV, 3GP | MP4, WEBM, MOV, MKV, AVI, GIF, MP3, WAV, **Compress** |
| 🎵 Audio | MP3, WAV, FLAC, OGG, OPUS, M4A, AAC, WMA | MP3, WAV, FLAC, OGG, OPUS, M4A |
| 📄 Documents | DOCX, DOC, ODT, RTF, TXT, MD, HTML | PDF, DOCX, ODT, TXT, HTML, MD, RTF |
| 📊 Spreadsheets | XLSX, XLS, CSV, TSV, ODS | XLSX, CSV, ODS, PDF, XLS, HTML |
| 📕 PDF | PDF | DOCX, ODT, PNG, JPG, **Split**, **Compress**, **Text (OCR)** |
| 🗜️ Archives | ZIP, RAR, 7Z, TAR.GZ, … | **Extract**, ZIP |

## Features

<p align="center">
  <img src="assets/readme/features.gif" alt="MP4 to MP3, Word to PDF, many files into a ZIP, RAR extracted instantly" width="320">
</p>

- **Compress** — shrink photos, videos (MP4 up to 1080p) and scanned PDFs without changing the format.
- **Merge into 1 PDF** — drag several images, documents, spreadsheets or PDFs at once.
- **Grab text (OCR)** — read the words in an image or scanned PDF with Windows' built-in OCR, straight to your clipboard.
- **Split PDFs** — one PDF per page, or export every page as PNG/JPG.
- **ZIP & extract** — pack many files/folders into one ZIP; .zip, .rar, .7z and .tar.gz extract in one move.
- **Light & fast** — the wheel is a small window right at your cursor, with no full-screen effects, so it opens
  instantly even on 4K screens. Pick **S / M / L** size from the tray menu.
- **Changed your mind?** Move back to the center of the wheel and let go: nothing is converted.
- **100% offline** — FFmpeg, ImageMagick, Ghostscript, LibreOffice, Pandoc & 7-Zip come bundled. Your files never leave your PC.

## Installation

1. Download **`Rotiva-Setup-x.y.z.exe`** from [Releases](https://github.com/uncrownedk1ng2812/rotiva/releases/latest).
2. Run the installer. If SmartScreen appears, click **More info → Run anyway**
   (the installer isn't signed with a paid code-signing certificate yet).
3. Find the Rotiva icon in the system tray, then drag a file + <kbd>Shift</kbd>.

Installs per user to `%LOCALAPPDATA%\Programs\Rotiva` — no admin rights and no .NET install needed.
Uninstall via *Settings → Apps → Installed apps → Rotiva*.

Click the tray icon for the Rotiva menu: start with Windows, the Explorer right-click menu, wheel size,
and links to the website, GitHub and support.

### macOS & Linux

Download the **macOS** `.dmg` (Apple Silicon or Intel) or the **Linux** `AppImage` from
[Releases](https://github.com/uncrownedk1ng2812/rotiva/releases/latest).
Same wheel and conversions: right-click files → **Open With → Rotiva**, or drag files and press
<kbd>Shift</kbd>. The conversion engines come from your system (`brew install …` / `apt install …`);
the release notes list the commands. The macOS app isn't notarized yet, so allow it once in
*System Settings → Privacy & Security*.

## Open source

Rotiva is open source under **GPL-3.0**. The app is in [`src/Rotiva`](src/Rotiva) (C# / .NET 8 / WPF) and
the installer in [`installer/`](installer). See [BUILDING.md](BUILDING.md) to run it from source or
build the installer.

Bug reports, ideas and pull requests are welcome in [Issues](https://github.com/uncrownedk1ng2812/rotiva/issues); see [CONTRIBUTING.md](CONTRIBUTING.md)
and the [Code of Conduct](CODE_OF_CONDUCT.md). Security problems: please report them privately as described in
[SECURITY.md](SECURITY.md).

## Code signing policy

Windows installers are built from this repository by the
[Windows installer workflow](.github/workflows/windows-installer.yml) and signed there; nothing built
elsewhere is signed. Free code signing provided by [SignPath.io](https://about.signpath.io),
certificate by [SignPath Foundation](https://signpath.org).

- Committers and reviewers: [Denish Chairany](https://github.com/uncrownedk1ng2812)
- Approvers: [Denish Chairany](https://github.com/uncrownedk1ng2812)

Every release is approved by hand before it is signed.

Signing is being set up: installers up to 1.10.0 were built locally and are not signed.

**Privacy:** Rotiva does not send any information to other networked systems. Conversions run on your
computer; links in the menu (website, GitHub, support) only open your browser when you click them.

## Bundled engines

Rotiva does the conversions with these excellent open-source programs, bundled in the installer under
their own licenses:

| Engine | License | Used for |
|---|---|---|
| [FFmpeg](https://ffmpeg.org) | GPL-3.0 | Video & audio |
| [ImageMagick](https://imagemagick.org) | ImageMagick License | Images |
| [Ghostscript](https://ghostscript.com) | AGPL-3.0 | PDF compress, merge & split |
| [LibreOffice](https://www.libreoffice.org) | MPL-2.0 | Documents & spreadsheets |
| [Pandoc](https://pandoc.org) | GPL-2.0-or-later | Markdown, HTML & RTF |
| [7-Zip](https://www.7-zip.org) | LGPL-2.1-or-later | ZIP & extract |

Versions, license files and source code links: [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).

## Support Rotiva

Rotiva is free and ad-free. If it saves you time, you can
[buy the developer a coffee](https://ko-fi.com/denishchairany) ☕ on Ko-fi.
Donating from Indonesia? Use [SociaBuzz](https://sociabuzz.com/kianaldrich) (QRIS, e-wallet, bank transfer).

## Copyright & license

Copyright © 2026 **Denish Chairany**. Licensed under the
[GNU General Public License v3.0](LICENSE).

- ✅ Use Rotiva for free, for personal or commercial work.
- ✅ Study, modify and share the code.
- 🔁 A modified version you distribute must also be **GPL-3.0 with its full source code**, and must keep
  the copyright notice.
- 🏷️ The **Rotiva name, logo and icon are not part of the license.** A fork needs its own name and logo,
  and must credit: `Based on Rotiva by Denish Chairany — https://github.com/uncrownedk1ng2812/rotiva`.
  Details in [TRADEMARKS.md](TRADEMARKS.md).

The bundled engines are separate programs under their own licenses, listed in
[THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).
