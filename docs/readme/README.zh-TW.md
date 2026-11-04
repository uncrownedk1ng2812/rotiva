<!-- languages -->
<p align="center">
  <a href="../../README.md">English</a> ·
  <a href="../../docs/readme/README.es.md">Español</a> ·
  <a href="../../docs/readme/README.ja.md">日本語</a> ·
  <a href="../../docs/readme/README.ko.md">한국어</a> ·
  <a href="../../docs/readme/README.zh-CN.md">简体中文</a> ·
  <b>繁體中文</b> ·
  <a href="../../docs/readme/README.pl.md">Polski</a> ·
  <a href="../../docs/readme/README.hi.md">हिन्दी</a> ·
  <a href="../../docs/readme/README.ru.md">Русский</a> ·
  <a href="../../docs/readme/README.ar.md">العربية</a> ·
  <a href="../../docs/readme/README.pt-BR.md">Português (Brasil)</a> ·
  <a href="../../docs/readme/README.pt-PT.md">Português (Portugal)</a> ·
  <a href="../../docs/readme/README.fr.md">Français</a> ·
  <a href="../../docs/readme/README.th.md">ไทย</a> ·
  <a href="../../docs/readme/README.tr.md">Türkçe</a> ·
  <a href="../../docs/readme/README.id.md">Bahasa Indonesia</a> ·
  <a href="../../docs/readme/README.it.md">Italiano</a>
</p>
<!-- /languages -->

<p align="center">
  <img src="../../assets/og.png" alt="Rotiva — 拖曳檔案、按下 Shift、放到轉盤上" width="720">
</p>

<p align="center">
  <b>不必開啟任何應用程式就能轉換檔案。</b><br>
  拖曳檔案，按下 <kbd>Shift</kbd>，放到轉盤上——完成。
</p>

<p align="center">
  <a href="https://github.com/uncrownedk1ng2812/rotiva/releases/latest"><img src="https://img.shields.io/github/v/release/uncrownedk1ng2812/rotiva?label=version&color=ff8a1f" alt="最新版本"></a>
  <img src="https://img.shields.io/badge/Windows-10%20%7C%2011-0078d4" alt="Windows 10 | 11">
  <img src="https://img.shields.io/badge/macOS-Apple%20Silicon%20%7C%20Intel-555" alt="macOS: Apple Silicon | Intel">
  <img src="https://img.shields.io/badge/Linux-AppImage-e95420" alt="Linux AppImage">
  <img src="https://img.shields.io/badge/%E5%83%B9%E6%A0%BC-%E5%85%8D%E8%B2%BB-1f9d55" alt="免費">
  <img src="https://img.shields.io/badge/100%25-%E9%9B%A2%E7%B7%9A-5a5a66" alt="100% 離線">
  <a href="../../LICENSE"><img src="https://img.shields.io/badge/license-GPL--3.0-blue" alt="授權：GPL-3.0"></a>
</p>

<p align="center">
  <a href="https://uncrownedk1ng2812.github.io/rotiva/"><b>🌐 網站與線上展示</b></a>
  &nbsp;·&nbsp;
  <a href="https://github.com/uncrownedk1ng2812/rotiva/releases/latest"><b>⬇️ 下載安裝程式</b></a>
  &nbsp;·&nbsp;
  <a href="https://ko-fi.com/denishchairany"><b>☕ 在 Ko-fi 上支持</b></a>
</p>

> 本文是[英文 README](../../README.md) 的翻譯。如有出入，以英文版為準。

---

> [!NOTE]
> **Rotiva 由 Denish Chairany 從零打造**，以 [GPL-3.0](../../LICENSE) 授權開源。
> 歡迎 fork，但必須維持開源、標示原作者，並使用**不同的名稱與標誌**。
> 詳見 [TRADEMARKS.md](../../TRADEMARKS.md)。

## Rotiva 是什麼？

Rotiva 是一款常駐在系統匣的小程式。拖曳照片、影片、文件或任何檔案時按下 **Shift**，
游標旁就會出現格式轉盤。指向想要的格式再放開，轉換好的檔案就會存在原始檔案旁邊。

不必上傳到線上轉檔網站，沒有廣告，也不用排隊。

<p align="center">
  <img src="../../assets/readme/demo.gif" alt="展示：拖曳檔案、按下 Shift、選擇格式、放開" width="320">
  &nbsp;&nbsp;
  <img src="../../assets/readme/wheel.png" alt="圖片檔案的格式轉盤" width="420">
</p>

## 使用方式

| | 步驟 | 說明 |
|---|---|---|
| **1** | **拖曳檔案** | 可以從檔案總管、桌面或任何資料夾拖曳。一個或多個檔案都可以。 |
| **2** | **按下 <kbd>Shift</kbd>** | 持續按住滑鼠左鍵。格式轉盤會出現在游標旁邊。 |
| **3** | **指向並放開** | 由游標的方向決定格式，不必精準落在某個區塊上。在中心放開即可取消。 |

比較習慣按右鍵？轉盤上的所有選項也都在檔案總管右鍵選單的 **Rotiva ▸** 裡
（Windows 11：*顯示其他選項*）。

## 支援的格式

轉盤會依照拖曳的檔案自動調整，檔案本身的格式會自動隱藏。

| 類型 | 來源格式 | 目標格式 |
|---|---|---|
| 🖼️ 圖片 | PNG, JPG, WEBP, AVIF, HEIC, BMP, TIFF, GIF, ICO | PNG, JPG, WEBP, AVIF, PDF, ICO, TIFF, **壓縮**, **文字 (OCR)** |
| 🎬 影片 | MP4, MKV, MOV, AVI, WEBM, WMV, FLV, 3GP | MP4, WEBM, MOV, MKV, AVI, GIF, MP3, WAV, **壓縮** |
| 🎵 音訊 | MP3, WAV, FLAC, OGG, OPUS, M4A, AAC, WMA | MP3, WAV, FLAC, OGG, OPUS, M4A |
| 📄 文件 | DOCX, DOC, ODT, RTF, TXT, MD, HTML | PDF, DOCX, ODT, TXT, HTML, MD, RTF |
| 📊 試算表 | XLSX, XLS, CSV, TSV, ODS | XLSX, CSV, ODS, PDF, XLS, HTML |
| 📽️ 簡報 | PPTX, PPT, ODP, PPSX, PPS | PDF, PPTX, ODP, PNG, JPG |
| 📕 PDF | PDF | DOCX, ODT, PNG, JPG, **分割**, **壓縮**, **文字 (OCR)** |
| 🗜️ 壓縮檔 | ZIP, RAR, 7Z, TAR.GZ, … | **解壓縮**, ZIP |

## 功能

<p align="center">
  <img src="../../assets/readme/features.gif" alt="MP4 轉 MP3、Word 轉 PDF、多個檔案打包成 ZIP、RAR 立即解壓縮" width="320">
</p>

- **壓縮**——不改變格式，縮小照片、影片（MP4，最高 1080p）與掃描的 PDF。
- **合併成 1 個 PDF**——一次拖入多張圖片、多份文件、試算表或 PDF。
- **擷取文字 (OCR)**——用 Windows 內建的 OCR 辨識圖片或掃描 PDF 裡的文字，直接複製到剪貼簿。
- **分割 PDF**——每頁一個 PDF，或把每一頁匯出成 PNG/JPG。
- **ZIP 與解壓縮**——把多個檔案與資料夾打包成一個 ZIP；.zip、.rar、.7z 與 .tar.gz 一個動作就能解壓縮。
- **輕巧快速**——轉盤是游標旁的小視窗，沒有全螢幕特效，即使在 4K 螢幕上也能立刻開啟。
  可在系統匣選單選擇 **S / M / L** 尺寸。
- **改變心意？** 回到轉盤中心再放開，就不會轉換任何東西。
- **100% 離線**——內建 FFmpeg、ImageMagick、Ghostscript、LibreOffice、Pandoc 與 7-Zip。你的檔案永遠不會離開電腦。

## 安裝

1. 從 [Releases](https://github.com/uncrownedk1ng2812/rotiva/releases/latest) 下載 **`Rotiva-Setup-x.y.z.exe`**。
2. 執行安裝程式。如果出現 SmartScreen，請按 **其他資訊 → 仍要執行**
   （安裝程式尚未使用付費的程式碼簽章憑證簽署）。
3. 在系統匣找到 Rotiva 圖示，然後拖曳檔案並按下 <kbd>Shift</kbd>。

以使用者為單位安裝到 `%LOCALAPPDATA%\Programs\Rotiva`，不需要系統管理員權限，也不需要安裝 .NET。
解除安裝：*設定 → 應用程式 → 已安裝的應用程式 → Rotiva*。

按一下系統匣圖示即可開啟 Rotiva 選單：隨 Windows 啟動、檔案總管右鍵選單、轉盤尺寸，以及網站、GitHub 與支持連結。

### macOS 與 Linux

從 [Releases](https://github.com/uncrownedk1ng2812/rotiva/releases/latest) 下載 **macOS 版 `.dmg`**（Apple Silicon 或 Intel）
或 **Linux 版 `AppImage`**。
轉盤與轉換功能完全相同：在檔案上按右鍵 → **打開方式 → Rotiva**，或拖曳檔案並按下 <kbd>Shift</kbd>。
轉換引擎使用系統中安裝的版本（`brew install …` / `apt install …`），指令請見發行說明。
macOS 版尚未經過公證（notarize），第一次使用時請在 *系統設定 → 隱私權與安全性* 中允許一次。

## 開源

Rotiva 以 **GPL-3.0** 授權開源。應用程式在 [`src/Rotiva`](../../src/Rotiva)（C# / .NET 8 / WPF），
安裝程式在 [`installer/`](../../installer)。若要從原始碼執行或建置安裝程式，請參考 [BUILDING.md](../../BUILDING.md)。

歡迎在 [Issues](https://github.com/uncrownedk1ng2812/rotiva/issues) 回報錯誤、提出想法或送出 pull request；
請參考 [CONTRIBUTING.md](../../CONTRIBUTING.md) 與[行為準則](../../CODE_OF_CONDUCT.md)。安全性問題請依照
[SECURITY.md](../../SECURITY.md) 的說明私下回報。

## 程式碼簽章政策

Windows 安裝程式由本儲存庫透過
[Windows 安裝程式工作流程](../../.github/workflows/windows-installer.yml) 建置並在其中簽署；在其他地方建置的內容
不會被簽署。免費的程式碼簽章由 [SignPath.io](https://about.signpath.io) 提供，憑證由
[SignPath Foundation](https://signpath.org) 提供。完整說明請見[英文 README](../../README.md#code-signing-policy)。

**隱私：** Rotiva 不會傳送任何資訊到網路上的其他系統。轉換在你的電腦上完成；選單中的連結（網站、GitHub、支持）
只有在你點擊時才會開啟瀏覽器。

## 內建引擎

Rotiva 使用下列優秀的開源程式完成轉換，它們依照各自的授權隨安裝程式一起提供：

| 引擎 | 授權 | 用途 |
|---|---|---|
| [FFmpeg](https://ffmpeg.org) | GPL-3.0 | 影片與音訊 |
| [ImageMagick](https://imagemagick.org) | ImageMagick License | 圖片 |
| [Ghostscript](https://ghostscript.com) | AGPL-3.0 | PDF 壓縮、合併與分割 |
| [LibreOffice](https://www.libreoffice.org) | MPL-2.0 | 文件、試算表與簡報 |
| [Pandoc](https://pandoc.org) | GPL-2.0-or-later | Markdown、HTML 與 RTF |
| [7-Zip](https://www.7-zip.org) | LGPL-2.1-or-later | ZIP 與解壓縮 |

版本、授權檔案與原始碼連結：[THIRD-PARTY-NOTICES.md](../../THIRD-PARTY-NOTICES.md)。

## 支持 Rotiva

Rotiva 免費且沒有廣告。如果它幫你省下了時間，歡迎在 Ko-fi 上
[請開發者喝杯咖啡](https://ko-fi.com/denishchairany) ☕。

## 著作權與授權

Copyright © 2026 **Denish Chairany**。依
[GNU General Public License v3.0](../../LICENSE) 授權。

- ✅ 可免費使用 Rotiva，無論個人或商業用途。
- ✅ 可以研究、修改與分享程式碼。
- 🔁 你散布的修改版本也必須採用 **GPL-3.0 並附上完整原始碼**，且必須保留著作權聲明。
- 🏷️ **Rotiva 的名稱、標誌與圖示不在授權範圍內。** fork 需要使用自己的名稱與標誌，並註明：
  `Based on Rotiva by Denish Chairany — https://github.com/uncrownedk1ng2812/rotiva`。
  詳見 [TRADEMARKS.md](../../TRADEMARKS.md)。

內建引擎是各自擁有獨立授權的個別程式，列於
[THIRD-PARTY-NOTICES.md](../../THIRD-PARTY-NOTICES.md)。
