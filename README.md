<p align="center">
  <img src="assets/og.png" alt="Rotiva — Drag file, tekan Shift, lepas di roda" width="720">
</p>

<p align="center">
  <b>Ubah format file tanpa buka aplikasi apa pun.</b><br>
  Drag file, tekan <kbd>Shift</kbd>, lepas di roda — beres.
</p>

<p align="center">
  <a href="https://github.com/uncrownedk1ng2812/rotiva/releases/latest"><img src="https://img.shields.io/github/v/release/uncrownedk1ng2812/rotiva?label=versi&color=ff8a1f" alt="Versi terbaru"></a>
  <img src="https://img.shields.io/badge/Windows-10%20%7C%2011-0078d4" alt="Windows 10 | 11">
  <img src="https://img.shields.io/badge/harga-gratis-1f9d55" alt="Gratis">
  <img src="https://img.shields.io/badge/100%25-offline-5a5a66" alt="100% offline">
</p>

<p align="center">
  <a href="https://uncrownedk1ng2812.github.io/rotiva/"><b>🌐 Website & coba di browser</b></a>
  &nbsp;·&nbsp;
  <a href="https://github.com/uncrownedk1ng2812/rotiva/releases/latest"><b>⬇️ Unduh installer</b></a>
</p>

---

> [!IMPORTANT]
> **Rotiva dibuat dari nol oleh Denish Chairany. Seluruh hak cipta dilindungi.**
> Repo ini **bukan untuk di-fork, disalin, atau didistribusikan ulang.** Kamu bebas mengunduh dan
> memakai aplikasinya, tapi kode, nama, logo, dan desain Rotiva tidak boleh dipakai tanpa izin
> tertulis. Penggunaan yang diizinkan **wajib mencantumkan kredit** ke pembuatnya. Lihat [LICENSE](LICENSE).

## Apa itu Rotiva?

Rotiva adalah aplikasi kecil untuk Windows yang diam di system tray. Saat kamu sedang men-drag
foto, video, dokumen, atau file lain lalu menekan **Shift**, sebuah roda pilihan format muncul tepat
di posisi kursor. Arahkan ke format yang kamu mau, lepas — file hasil konversi langsung tersimpan
di sebelah file aslinya.

Tidak perlu upload ke converter online, tidak ada iklan, tidak ada antrean.

<p align="center">
  <img src="assets/readme/demo.gif" alt="Demo: drag file, tekan Shift, pilih format, lepas" width="320">
  &nbsp;&nbsp;
  <img src="assets/readme/wheel.png" alt="Roda pilihan format untuk file gambar" width="420">
</p>

## Cara pakai

| | Langkah | Keterangan |
|---|---|---|
| **1** | **Drag file** | Dari Explorer, desktop, atau folder mana pun. Satu file atau beberapa sekaligus. |
| **2** | **Tekan <kbd>Shift</kbd>** | Sambil klik kiri masih ditahan. Layar meredup dan roda format muncul di kursor. |
| **3** | **Arahkan & lepas** | Pilihan ditentukan arah kursor, tidak harus tepat di atas segmen. Selesai. |

Lebih suka klik kanan? Semua pilihan di roda juga ada di menu klik kanan Explorer, di bawah
**Rotiva ▸** (Windows 11: *Show more options*).

## Format yang didukung

Roda menyesuaikan jenis file yang kamu drag. Format yang sama dengan file asli otomatis disembunyikan.

| Jenis | Dari | Ke |
|---|---|---|
| 🖼️ Gambar | PNG, JPG, WEBP, AVIF, HEIC, BMP, TIFF, GIF, ICO | PNG, JPG, WEBP, AVIF, PDF, ICO, TIFF, **Kompres**, **Teks (OCR)** |
| 🎬 Video | MP4, MKV, MOV, AVI, WEBM, WMV, FLV, 3GP | MP4, WEBM, MOV, MKV, AVI, GIF, MP3, WAV, **Kompres** |
| 🎵 Audio | MP3, WAV, FLAC, OGG, OPUS, M4A, AAC, WMA | MP3, WAV, FLAC, OGG, OPUS, M4A |
| 📄 Dokumen | DOCX, DOC, ODT, RTF, TXT, MD, HTML | PDF, DOCX, ODT, TXT, HTML, MD, RTF |
| 📊 Tabel | XLSX, XLS, CSV, TSV, ODS | XLSX, CSV, ODS, PDF, XLS, HTML |
| 📕 PDF | PDF | DOCX, ODT, PNG, JPG, **Pisah**, **Kompres**, **Teks (OCR)** |
| 🗜️ Arsip | ZIP, RAR, 7Z, TAR.GZ, … | **Ekstrak**, ZIP |

## Fitur

<p align="center">
  <img src="assets/readme/features.gif" alt="MP4 ke MP3, banyak file jadi ZIP, RAR langsung diekstrak" width="320">
</p>

- **Kompres** — perkecil foto, video (MP4 maks. 1080p), dan PDF hasil scan tanpa ganti format.
- **Gabung jadi 1 PDF** — drag beberapa gambar, dokumen, tabel, atau PDF sekaligus.
- **Ambil teks (OCR)** — baca tulisan dari gambar atau PDF scan pakai OCR bawaan Windows, langsung masuk clipboard.
- **Pisah PDF** — satu PDF jadi PDF per halaman, atau ekspor semua halaman jadi PNG/JPG.
- **ZIP & ekstrak** — banyak file/folder jadi satu ZIP; .zip, .rar, .7z, .tar.gz langsung diekstrak.
- **100% offline** — FFmpeg, ImageMagick, Ghostscript, LibreOffice, Pandoc & 7-Zip sudah dibundel. File tidak pernah keluar dari PC kamu.

## Instalasi

1. Unduh **`Rotiva-Setup-x.y.z.exe`** dari [Releases](https://github.com/uncrownedk1ng2812/rotiva/releases/latest).
2. Jalankan installer. Kalau muncul SmartScreen, klik **More info → Run anyway**
   (installer belum ditandatangani sertifikat code-signing berbayar).
3. Cari ikon Rotiva di system tray, lalu drag file + <kbd>Shift</kbd>.

Terpasang per-user di `%LOCALAPPDATA%\Programs\Rotiva` — tanpa hak admin, tanpa perlu install .NET.
Uninstall lewat *Settings → Apps → Installed apps → Rotiva*.

## Dukung Rotiva

Rotiva gratis dan tanpa iklan. Kalau Rotiva menghemat waktumu, kamu bisa
[traktir kopi buat pengembangnya](https://uncrownedk1ng2812.github.io/rotiva/#dukung) ☕

## Hak cipta & lisensi

Copyright © 2026 **Denish Chairany**. All Rights Reserved.

Rotiva bukan proyek open source. Yang **boleh**: mengunduh installer resmi dan memakai aplikasinya,
serta membagikan link repo/website ini. Yang **tidak boleh** tanpa izin tertulis: menyalin atau
memodifikasi kode, mengunggah ulang atau menjual installer, memakai nama/logo/desain Rotiva untuk
produk lain, atau mengaku sebagai pembuatnya. Fork di GitHub tidak memberi hak-hak tersebut.

Kalau kamu mendapat izin, cantumkan kredit:
`Rotiva by Denish Chairany — https://github.com/uncrownedk1ng2812/rotiva`

Pelanggaran dapat ditindak melalui DMCA takedown dan jalur hukum. Teks lengkap ada di [LICENSE](LICENSE).
