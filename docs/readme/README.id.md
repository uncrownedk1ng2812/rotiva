<!-- languages -->
<p align="center">
  <a href="../../README.md">English</a> ·
  <a href="../../docs/readme/README.es.md">Español</a> ·
  <a href="../../docs/readme/README.ja.md">日本語</a> ·
  <a href="../../docs/readme/README.ko.md">한국어</a> ·
  <a href="../../docs/readme/README.zh-CN.md">简体中文</a> ·
  <a href="../../docs/readme/README.zh-TW.md">繁體中文</a> ·
  <a href="../../docs/readme/README.pl.md">Polski</a> ·
  <a href="../../docs/readme/README.hi.md">हिन्दी</a> ·
  <a href="../../docs/readme/README.ru.md">Русский</a> ·
  <a href="../../docs/readme/README.ar.md">العربية</a> ·
  <a href="../../docs/readme/README.pt-BR.md">Português (Brasil)</a> ·
  <a href="../../docs/readme/README.pt-PT.md">Português (Portugal)</a> ·
  <a href="../../docs/readme/README.fr.md">Français</a> ·
  <a href="../../docs/readme/README.th.md">ไทย</a> ·
  <a href="../../docs/readme/README.tr.md">Türkçe</a> ·
  <b>Bahasa Indonesia</b> ·
  <a href="../../docs/readme/README.it.md">Italiano</a>
</p>
<!-- /languages -->

<p align="center">
  <img src="../../assets/og.png" alt="Rotiva — drag file, tekan Shift, lepas di roda" width="720">
</p>

<p align="center">
  <b>Ubah format file tanpa membuka aplikasi apa pun.</b><br>
  Drag file, tekan <kbd>Shift</kbd>, lepas di roda — selesai.
</p>

<p align="center">
  <a href="https://apps.microsoft.com/detail/9N5FPM017GN5"><img src="../../assets/microsoft-store-badge.svg" alt="Get it from Microsoft Store" width="240"></a>
</p>

<p align="center">
  <a href="https://github.com/uncrownedk1ng2812/rotiva/releases/latest"><img src="https://img.shields.io/github/v/release/uncrownedk1ng2812/rotiva?label=version&color=ff8a1f" alt="Versi terbaru"></a>
  <img src="https://img.shields.io/badge/Windows-10%20%7C%2011-0078d4" alt="Windows 10 | 11">
  <img src="https://img.shields.io/badge/macOS-Apple%20Silicon%20%7C%20Intel-555" alt="macOS: Apple Silicon | Intel">
  <img src="https://img.shields.io/badge/Linux-AppImage-e95420" alt="Linux AppImage">
  <img src="https://img.shields.io/badge/harga-gratis-1f9d55" alt="Gratis">
  <img src="https://img.shields.io/badge/100%25-offline-5a5a66" alt="100% offline">
  <a href="../../LICENSE"><img src="https://img.shields.io/badge/license-GPL--3.0-blue" alt="Lisensi: GPL-3.0"></a>
</p>

<p align="center">
  <a href="https://uncrownedk1ng2812.github.io/rotiva/id/"><b>🌐 Website & demo langsung</b></a>
  &nbsp;·&nbsp;
  <a href="https://github.com/uncrownedk1ng2812/rotiva/releases/latest"><b>⬇️ Download installer</b></a>
  &nbsp;·&nbsp;
  <a href="https://apps.microsoft.com/detail/9N5FPM017GN5"><b>🛍️ Microsoft Store</b></a>
  &nbsp;·&nbsp;
  <a href="https://sociabuzz.com/kianaldrich"><b>☕ Dukung lewat SociaBuzz</b></a>
</p>

> Ini terjemahan dari [README bahasa Inggris](../../README.md). Kalau ada perbedaan, versi bahasa Inggris yang berlaku.

---

> [!NOTE]
> **Rotiva dibuat dari nol oleh Denish Chairany** dan open source di bawah [GPL-3.0](../../LICENSE).
> Fork boleh, tapi harus tetap open source, mencantumkan pembuatnya, dan memakai **nama dan logo
> yang berbeda**. Lihat [TRADEMARKS.md](../../TRADEMARKS.md).

## Apa itu Rotiva?

Rotiva adalah aplikasi kecil yang tinggal di system tray. Saat kamu sedang men-drag foto, video,
dokumen, atau file apa pun, tekan **Shift** dan roda format langsung muncul di kursor. Arahkan ke
format yang kamu mau lalu lepas — file hasilnya disimpan tepat di sebelah file asli.

Tanpa upload ke converter online, tanpa iklan, tanpa antre.

<p align="center">
  <img src="../../assets/readme/demo.gif" alt="Demo: drag file, tekan Shift, pilih format, lepas" width="320">
  &nbsp;&nbsp;
  <img src="../../assets/readme/wheel.png" alt="Roda format untuk file gambar" width="420">
</p>

## Cara kerja

| | Langkah | Keterangan |
|---|---|---|
| **1** | **Drag file** | Dari Explorer, desktop, atau folder mana pun. Satu file atau beberapa sekaligus. |
| **2** | **Tekan <kbd>Shift</kbd>** | Sambil tetap menahan klik kiri. Roda format muncul tepat di kursor. |
| **3** | **Arahkan & lepas** | Arah kursor yang memilih format — tidak harus tepat di atas segmen. Lepas di tengah untuk batal. |

Lebih suka klik kanan? Semua pilihan di roda juga ada di menu klik kanan Explorer, di bawah
**Rotiva ▸** (Windows 11: *Show more options*).

## Format yang didukung

Roda menyesuaikan dengan file yang kamu drag. Format file itu sendiri otomatis disembunyikan.

| Jenis | Dari | Ke |
|---|---|---|
| 🖼️ Gambar | PNG, JPG, WEBP, AVIF, HEIC, BMP, TIFF, GIF, ICO, PSD, SVG, JXL, DNG, CR2, NEF, ARW, … | PNG, JPG, WEBP, AVIF, PDF, ICO, TIFF, **Kompres**, **Teks (OCR)**, **Edit** |
| 🎬 Video | MP4, MKV, MOV, AVI, WEBM, WMV, FLV, 3GP, MPG, TS, VOB, … | MP4, WEBM, MOV, MKV, AVI, GIF, MP3, WAV, **Kompres**, **Edit** |
| 🎵 Audio | MP3, WAV, FLAC, OGG, OPUS, M4A, AAC, WMA, AIFF, M4B, AMR, APE, … | MP3, WAV, FLAC, OGG, OPUS, M4A, AAC, AIFF, **Edit** |
| 📄 Dokumen | DOCX, DOC, ODT, RTF, TXT, MD, HTML | PDF, DOCX, ODT, TXT, HTML, MD, RTF, EPUB |
| 📊 Spreadsheet | XLSX, XLS, CSV, TSV, ODS | XLSX, CSV, ODS, PDF, XLS, HTML, JSON |
| 📽️ Slide | PPTX, PPT, ODP, PPSX, PPS | PDF, PPTX, ODP, PNG, JPG |
| 📕 PDF | PDF | DOCX, ODT, PNG, JPG, **Pisah**, **Kompres**, **Teks (OCR)** |
| 🗜️ Arsip | ZIP, RAR, 7Z, TAR.GZ, … | **Ekstrak**, ZIP |
| 📚 E-book | EPUB, FB2 | PDF, EPUB, DOCX, HTML, TXT, MD |
| 💬 Subtitle | SRT, VTT, ASS, SSA, SMI, LRC | SRT, VTT, ASS, LRC |
| 🧾 Data | JSON, YAML | JSON, YAML, CSV |

## Fitur

<p align="center">
  <img src="../../assets/readme/features.gif" alt="MP4 ke MP3, Word ke PDF, banyak file jadi ZIP, RAR langsung diekstrak" width="320">
</p>

- **Kompres** — perkecil foto, video (MP4 hingga 1080p), dan PDF hasil scan tanpa mengubah formatnya.
- **Gabung jadi 1 PDF** — drag beberapa gambar, dokumen, spreadsheet, atau PDF sekaligus.
- **Ambil teks (OCR)** — baca tulisan di gambar atau PDF hasil scan dengan OCR bawaan Windows, langsung ke clipboard.
- **Pisah PDF** — satu PDF per halaman, atau ekspor tiap halaman jadi PNG/JPG.
- **ZIP & ekstrak** — satukan banyak file/folder jadi satu ZIP; .zip, .rar, .7z, dan .tar.gz diekstrak sekali gerak.
- **Edit** — segmen **EDIT** membuka halaman kedua: putar, balik, atau hitam-putihkan gambar; putar, balik, atau mundurkan video (mundur hingga 30 detik); ratakan volume atau mundurkan audio, atau simpan gambar sampulnya.
- **Tempel subtitle** — drag satu video dan satu file subtitle sekaligus, lalu pilih **TEMPEL**.
- **ZIP berpassword (Windows)** — **KUNCI** mengemas file jadi ZIP AES-256. Buka dengan 7-Zip atau WinRAR.
- **Lembar kontak** — **LEMBAR** menyatukan beberapa foto, atau 16 cuplikan video, dalam satu gambar.
- **Konversi clipboard (Windows)** — salin file atau gambar, atau ambil screenshot, lalu tekan <kbd>Ctrl</kbd>+<kbd>Alt</kbd>+<kbd>Shift</kbd>+<kbd>V</kbd>.
- **Panduan bawaan (Windows)** — buka **Panduan** dari menu tray untuk langkah-langkahnya.
- **Ringan & cepat** — roda berupa jendela kecil tepat di kursor, tanpa efek layar penuh, jadi langsung terbuka
  bahkan di layar 4K. Pilih ukuran **S / M / L** dari menu tray.
- **Berubah pikiran?** Kembalikan kursor ke tengah roda lalu lepas: tidak ada yang dikonversi.
- **100% offline** — FFmpeg, ImageMagick, Ghostscript, LibreOffice, Pandoc & 7-Zip sudah ikut di dalamnya. File kamu tidak pernah keluar dari PC.

## Instalasi

1. Download **`Rotiva-Setup-x.y.z.exe`** dari [Releases](https://github.com/uncrownedk1ng2812/rotiva/releases/latest).
2. Jalankan installer. Kalau SmartScreen muncul, klik **More info → Run anyway**
   (installer belum ditandatangani dengan sertifikat code signing berbayar).
3. Cari ikon Rotiva di system tray, lalu drag file + <kbd>Shift</kbd>.

Terpasang per pengguna di `%LOCALAPPDATA%\Programs\Rotiva` — tidak perlu hak admin dan tidak perlu install .NET.
Uninstall lewat *Settings → Apps → Installed apps → Rotiva*.

Klik ikon tray untuk membuka menu Rotiva: jalankan saat Windows mulai, menu klik kanan Explorer, ukuran roda,
dan link ke website, GitHub, dan dukungan.

### macOS & Linux

Download **`.dmg` macOS** (Apple Silicon atau Intel) atau **`AppImage` Linux** dari
[Releases](https://github.com/uncrownedk1ng2812/rotiva/releases/latest).
Roda dan konversinya sama: klik kanan file → **Open With → Rotiva**, atau drag file lalu tekan
<kbd>Shift</kbd>. Engine konversi diambil dari sistem kamu (`brew install …` / `apt install …`);
perintahnya ada di catatan rilis. Aplikasi macOS belum di-notarize, jadi izinkan sekali di
*System Settings → Privacy & Security*.

## Open source

Rotiva open source di bawah **GPL-3.0**. Kode aplikasinya ada di [`src/Rotiva`](../../src/Rotiva) (C# / .NET 8 / WPF) dan
installer di [`installer/`](../../installer). Lihat [BUILDING.md](../../BUILDING.md) untuk menjalankan dari source atau
membuat installer.

Laporan bug, ide, dan pull request sangat diterima di [Issues](https://github.com/uncrownedk1ng2812/rotiva/issues); lihat
[CONTRIBUTING.md](../../CONTRIBUTING.md) dan [Code of Conduct](../../CODE_OF_CONDUCT.md). Masalah keamanan: laporkan secara
privat seperti dijelaskan di [SECURITY.md](../../SECURITY.md).

## Code signing

Installer Windows belum ditandatangani (code signing), jadi Windows SmartScreen bisa menampilkan peringatan saat pertama kali dijalankan. Setiap rilis menyertakan checksum SHA-256 di `SHA256SUMS.txt` supaya kamu bisa memeriksa file yang diunduh. Mau versi yang sudah ditandatangani? Rotiva juga tersedia di [Microsoft Store](https://apps.microsoft.com/detail/9N5FPM017GN5), ditandatangani oleh Microsoft.

**Privasi:** Rotiva tidak mengirim informasi apa pun ke sistem lain di jaringan. Konversi berjalan di
komputer kamu; link di menu (website, GitHub, dukungan) hanya membuka browser saat kamu mengkliknya.

## Engine yang dibundel

Rotiva melakukan konversi dengan program open source hebat berikut, dibundel di installer dengan
lisensinya masing-masing:

| Engine | Lisensi | Dipakai untuk |
|---|---|---|
| [FFmpeg](https://ffmpeg.org) | GPL-3.0 | Video & audio |
| [ImageMagick](https://imagemagick.org) | ImageMagick License | Gambar |
| [Ghostscript](https://ghostscript.com) | AGPL-3.0 | Kompres, gabung & pisah PDF |
| [LibreOffice](https://www.libreoffice.org) | MPL-2.0 | Dokumen, spreadsheet & slide |
| [Pandoc](https://pandoc.org) | GPL-2.0-or-later | Markdown, HTML & RTF |
| [7-Zip](https://www.7-zip.org) | LGPL-2.1-or-later | ZIP & ekstrak |

Versi, file lisensi, dan link source code: [THIRD-PARTY-NOTICES.md](../../THIRD-PARTY-NOTICES.md).

## Dukung Rotiva

Rotiva gratis dan tanpa iklan. Kalau Rotiva menghemat waktumu, kamu bisa mentraktir developernya kopi lewat
[SociaBuzz](https://sociabuzz.com/kianaldrich) (QRIS, e-wallet, transfer bank) ☕, atau
[Ko-fi](https://ko-fi.com/denishchairany) dari luar Indonesia.

## Hak cipta & lisensi

Copyright © 2026 **Denish Chairany**. Dilisensikan di bawah
[GNU General Public License v3.0](../../LICENSE).

- ✅ Pakai Rotiva gratis, untuk keperluan pribadi maupun komersial.
- ✅ Pelajari, ubah, dan bagikan kodenya.
- 🔁 Versi modifikasi yang kamu sebarkan juga harus **GPL-3.0 beserta source code lengkapnya**, dan harus
  tetap mencantumkan pemberitahuan hak cipta.
- 🏷️ **Nama, logo, dan ikon Rotiva tidak termasuk dalam lisensi.** Fork harus punya nama dan logo sendiri,
  dan harus mencantumkan: `Based on Rotiva by Denish Chairany — https://github.com/uncrownedk1ng2812/rotiva`.
  Detail di [TRADEMARKS.md](../../TRADEMARKS.md).

Engine yang dibundel adalah program terpisah dengan lisensinya sendiri, tercantum di
[THIRD-PARTY-NOTICES.md](../../THIRD-PARTY-NOTICES.md).
