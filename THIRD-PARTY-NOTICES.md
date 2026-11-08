# Third-party software in Rotiva

Rotiva's own code is GPL-3.0 (see [LICENSE](LICENSE)). The Windows installer also bundles the programs
below. Each one is a **separate program** that Rotiva starts as its own process. Each keeps its own
license, and its license file is installed next to it in `%LOCALAPPDATA%\Programs\Rotiva\tools\<engine>\`.

Versions are the ones bundled in **Rotiva 1.9.0**.

| Engine | Version | License | Used for | License file in the app | Source code |
|---|---|---|---|---|---|
| [FFmpeg](https://ffmpeg.org) (gyan.dev "full" build) | 9.0.2 | GPL-3.0 (built with `--enable-gpl --enable-version3`) | Video & audio | `tools\ffmpeg\LICENSE.txt` | [n9.0.2](https://github.com/FFmpeg/FFmpeg/tree/n9.0.2) · [ffmpeg.org releases](https://ffmpeg.org/releases/) · [build info](https://www.gyan.dev/ffmpeg/builds/) |
| [ImageMagick](https://imagemagick.org) (portable Q16-HDRI) | 7.1.2-34 | [ImageMagick License](https://imagemagick.org/script/license.php) (Apache-2.0 style) | Images | `tools\imagemagick\LICENSE.txt`, `NOTICE.txt` | [7.1.2-34](https://github.com/ImageMagick/ImageMagick/releases/tag/7.1.2-34) |
| [Ghostscript](https://ghostscript.com) | 10.08.0 | AGPL-3.0 | PDF compress, merge, split, render | `tools\ghostscript\LICENSE.txt` | [gs10080](https://github.com/ArtifexSoftware/ghostpdl-downloads/releases/tag/gs10080) |
| [LibreOffice](https://www.libreoffice.org) (trimmed, headless) | 26.8.1 | MPL-2.0 (some parts LGPL-3.0+ / others, listed in its license file) | Documents & spreadsheets | `tools\libreoffice\license.txt`, `LICENSE.html`, `NOTICE` | [LibreOffice source](https://download.documentfoundation.org/libreoffice/src/) |
| [Pandoc](https://pandoc.org) | 3.12.1 | GPL-2.0-or-later | Markdown / HTML / RTF documents | `tools\pandoc\COPYRIGHT.txt` | [3.12.1](https://github.com/jgm/pandoc/releases/tag/3.12.1) |
| [7-Zip](https://www.7-zip.org) | 25.01 | LGPL-2.1-or-later, BSD-3-Clause parts, unRAR restriction | ZIP & extract | `tools\7zip\License.txt` | [7-Zip downloads](https://www.7-zip.org/download.html) |

The installer also includes:

| Component | License |
|---|---|
| [.NET 8 runtime](https://github.com/dotnet/runtime) (self-contained, inside `Rotiva.exe`) | MIT |
| [YamlDotNet](https://github.com/aaubry/YamlDotNet) 16.3.0 (inside `Rotiva.exe`, for YAML files) | MIT |
| Microsoft Visual C++ runtime DLLs (next to ImageMagick and LibreOffice) | [Microsoft Visual C++ Redistributable license](https://learn.microsoft.com/cpp/windows/redistributing-visual-cpp-files) |

Text recognition (OCR) uses the `Windows.Media.Ocr` API that is part of Windows. It is not bundled.

## Source code offer

For the GPL/AGPL/LGPL/MPL engines above, the complete corresponding source code for the exact bundled
versions is available from the links in the table. If a link stops working, open an issue at
<https://github.com/uncrownedk1ng2812/rotiva/issues> and the source will be provided.
