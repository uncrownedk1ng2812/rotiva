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
  <b>Türkçe</b> ·
  <a href="../../docs/readme/README.id.md">Bahasa Indonesia</a> ·
  <a href="../../docs/readme/README.it.md">Italiano</a>
</p>
<!-- /languages -->

<p align="center">
  <img src="../../assets/og.png" alt="Rotiva — bir dosyayı sürükleyin, Shift'e basın, çarkın üzerine bırakın" width="720">
</p>

<p align="center">
  <b>Hiçbir uygulama açmadan dosya dönüştürün.</b><br>
  Bir dosyayı sürükleyin, <kbd>Shift</kbd>'e basın, çarkın üzerine bırakın. Bu kadar.
</p>

<p align="center">
  <a href="https://github.com/uncrownedk1ng2812/rotiva/releases/latest"><img src="https://img.shields.io/github/v/release/uncrownedk1ng2812/rotiva?label=version&color=ff8a1f" alt="Son sürüm"></a>
  <img src="https://img.shields.io/badge/Windows-10%20%7C%2011-0078d4" alt="Windows 10 | 11">
  <img src="https://img.shields.io/badge/macOS-Apple%20Silicon%20%7C%20Intel-555" alt="macOS: Apple Silicon | Intel">
  <img src="https://img.shields.io/badge/Linux-AppImage-e95420" alt="Linux AppImage">
  <img src="https://img.shields.io/badge/fiyat-%C3%BCcretsiz-1f9d55" alt="Ücretsiz">
  <img src="https://img.shields.io/badge/100%25-%C3%A7evrimd%C4%B1%C5%9F%C4%B1-5a5a66" alt="%100 çevrimdışı">
  <a href="../../LICENSE"><img src="https://img.shields.io/badge/license-GPL--3.0-blue" alt="Lisans: GPL-3.0"></a>
</p>

<p align="center">
  <a href="https://uncrownedk1ng2812.github.io/rotiva/"><b>🌐 Web sitesi ve canlı demo</b></a>
  &nbsp;·&nbsp;
  <a href="https://github.com/uncrownedk1ng2812/rotiva/releases/latest"><b>⬇️ Yükleyiciyi indir</b></a>
  &nbsp;·&nbsp;
  <a href="https://ko-fi.com/denishchairany"><b>☕ Ko-fi'de destek ol</b></a>
</p>

> Bu, [İngilizce README](../../README.md)'nin çevirisidir. Bir fark olursa İngilizce sürüm geçerlidir.

---

> [!NOTE]
> **Rotiva, Denish Chairany tarafından sıfırdan geliştirildi** ve [GPL-3.0](../../LICENSE) lisansıyla açık kaynaktır.
> Fork'lar memnuniyetle karşılanır; ancak açık kaynak kalmaları, yazarı belirtmeleri ve **farklı bir ad ve
> logo** kullanmaları gerekir. Bkz. [TRADEMARKS.md](../../TRADEMARKS.md).

## Rotiva nedir?

Rotiva, sistem tepsisinde duran küçük bir uygulamadır. Bir fotoğrafı, videoyu, belgeyi veya herhangi bir dosyayı
sürüklerken **Shift**'e basın; imlecin hemen yanında bir format çarkı açılır. İstediğiniz formatı gösterip bırakın:
dönüştürülen dosya, orijinalin hemen yanına kaydedilir.

Çevrimiçi dönüştürücülere yükleme yok, reklam yok, sıra beklemek yok.

<p align="center">
  <img src="../../assets/readme/demo.gif" alt="Demo: dosyayı sürükle, Shift'e bas, format seç, bırak" width="320">
  &nbsp;&nbsp;
  <img src="../../assets/readme/wheel.png" alt="Bir görsel için format çarkı" width="420">
</p>

## Nasıl çalışır?

| | Adım | Ayrıntılar |
|---|---|---|
| **1** | **Bir dosyayı sürükleyin** | Dosya Gezgini'nden, masaüstünden veya herhangi bir klasörden. Tek dosya ya da birden fazlası aynı anda. |
| **2** | **<kbd>Shift</kbd>'e basın** | Sol fare tuşunu bırakmadan. Format çarkı imlecin hemen yanında açılır. |
| **3** | **Gösterin ve bırakın** | Formatı imlecin yönü seçer; tam olarak bir dilimin üzerine gelmeniz gerekmez. İptal etmek için ortada bırakın. |

Sağ tıklamayı mı tercih edersiniz? Çarktaki her şey Dosya Gezgini'nin sağ tık menüsünde de
**Rotiva ▸** altında bulunur (Windows 11: *Daha fazla seçenek göster*).

## Desteklenen formatlar

Çark, sürüklediğiniz dosyaya göre değişir. Dosyanın kendi formatı otomatik olarak gizlenir.

| Tür | Kaynak | Hedef |
|---|---|---|
| 🖼️ Görseller | PNG, JPG, WEBP, AVIF, HEIC, BMP, TIFF, GIF, ICO | PNG, JPG, WEBP, AVIF, PDF, ICO, TIFF, **Sıkıştır**, **Metin (OCR)** |
| 🎬 Video | MP4, MKV, MOV, AVI, WEBM, WMV, FLV, 3GP | MP4, WEBM, MOV, MKV, AVI, GIF, MP3, WAV, **Sıkıştır** |
| 🎵 Ses | MP3, WAV, FLAC, OGG, OPUS, M4A, AAC, WMA | MP3, WAV, FLAC, OGG, OPUS, M4A |
| 📄 Belgeler | DOCX, DOC, ODT, RTF, TXT, MD, HTML | PDF, DOCX, ODT, TXT, HTML, MD, RTF |
| 📊 Tablolar | XLSX, XLS, CSV, TSV, ODS | XLSX, CSV, ODS, PDF, XLS, HTML |
| 📽️ Sunumlar | PPTX, PPT, ODP, PPSX, PPS | PDF, PPTX, ODP, PNG, JPG |
| 📕 PDF | PDF | DOCX, ODT, PNG, JPG, **Böl**, **Sıkıştır**, **Metin (OCR)** |
| 🗜️ Arşivler | ZIP, RAR, 7Z, TAR.GZ, … | **Çıkart**, ZIP |

## Özellikler

<p align="center">
  <img src="../../assets/readme/features.gif" alt="MP4'ten MP3'e, Word'den PDF'e, birçok dosya tek ZIP'te, RAR anında çıkartıldı" width="320">
</p>

- **Sıkıştır** — fotoğrafları, videoları (1080p'ye kadar MP4) ve taranmış PDF'leri formatlarını değiştirmeden küçültün.
- **Tek PDF'te birleştir** — birden fazla görseli, belgeyi, tabloyu veya PDF'i aynı anda sürükleyin.
- **Metni kopyala (OCR)** — bir görseldeki veya taranmış PDF'teki yazıları Windows'un yerleşik OCR'ı ile okuyup doğrudan panoya alın.
- **PDF böl** — her sayfa için ayrı bir PDF ya da her sayfayı PNG/JPG olarak dışa aktarın.
- **ZIP ve çıkartma** — birçok dosya ve klasörü tek bir ZIP'te toplayın; .zip, .rar, .7z ve .tar.gz tek hamlede çıkartılır.
- **Hafif ve hızlı** — çark, imlecin yanında açılan küçük bir penceredir; tam ekran efekti olmadığı için 4K ekranlarda
  bile anında açılır. **S / M / L** boyutunu tepsi menüsünden seçin.
- **Fikrinizi mi değiştirdiniz?** Çarkın ortasına dönüp bırakın: hiçbir şey dönüştürülmez.
- **%100 çevrimdışı** — FFmpeg, ImageMagick, Ghostscript, LibreOffice, Pandoc ve 7-Zip dahildir. Dosyalarınız bilgisayarınızdan hiç çıkmaz.

## Kurulum

1. **`Rotiva-Setup-x.y.z.exe`** dosyasını [Releases](https://github.com/uncrownedk1ng2812/rotiva/releases/latest) sayfasından indirin.
2. Yükleyiciyi çalıştırın. SmartScreen çıkarsa **Ek bilgi → Yine de çalıştır**'a tıklayın
   (yükleyici henüz ücretli bir kod imzalama sertifikasıyla imzalanmadı).
3. Sistem tepsisinde Rotiva simgesini bulun, ardından bir dosyayı sürükleyip <kbd>Shift</kbd>'e basın.

Kullanıcı başına `%LOCALAPPDATA%\Programs\Rotiva` konumuna kurulur; yönetici izni ve .NET kurulumu gerekmez.
Kaldırmak için: *Ayarlar → Uygulamalar → Yüklü uygulamalar → Rotiva*.

Rotiva menüsünü açmak için tepsi simgesine tıklayın: Windows ile başlat, Dosya Gezgini sağ tık menüsü, çark boyutu,
web sitesi, GitHub ve destek bağlantıları.

### macOS ve Linux

**macOS için `.dmg`** (Apple Silicon veya Intel) ya da **Linux için `AppImage`** dosyasını
[Releases](https://github.com/uncrownedk1ng2812/rotiva/releases/latest) sayfasından indirin.
Aynı çark, aynı dönüştürmeler: dosyalara sağ tıklayın → **Birlikte Aç → Rotiva**, ya da dosyaları sürükleyip
<kbd>Shift</kbd>'e basın. Dönüştürme motorları sisteminizden gelir (`brew install …` / `apt install …`); komutlar
sürüm notlarında yer alır. macOS uygulaması henüz noter onaylı (notarized) değil; bu yüzden bir kez
*Sistem Ayarları → Gizlilik ve Güvenlik* bölümünden izin verin.

## Açık kaynak

Rotiva, **GPL-3.0** lisansıyla açık kaynaktır. Uygulama [`src/Rotiva`](../../src/Rotiva) (C# / .NET 8 / WPF),
yükleyici ise [`installer/`](../../installer) klasöründedir. Kaynaktan çalıştırmak veya yükleyiciyi oluşturmak için
[BUILDING.md](../../BUILDING.md) dosyasına bakın.

Hata bildirimleri, fikirler ve pull request'ler [Issues](https://github.com/uncrownedk1ng2812/rotiva/issues) bölümüne
memnuniyetle beklenir; bkz. [CONTRIBUTING.md](../../CONTRIBUTING.md) ve [Davranış Kuralları](../../CODE_OF_CONDUCT.md).
Güvenlik sorunlarını [SECURITY.md](../../SECURITY.md) dosyasında anlatıldığı gibi gizli olarak bildirin.

## Kod imzalama politikası

Windows yükleyicileri bu depodan
[Windows yükleyici iş akışı](../../.github/workflows/windows-installer.yml) ile oluşturulur ve orada imzalanır; başka
bir yerde oluşturulan hiçbir şey imzalanmaz. Ücretsiz kod imzalama [SignPath.io](https://about.signpath.io) tarafından,
sertifika [SignPath Foundation](https://signpath.org) tarafından sağlanır. Tüm ayrıntılar
[İngilizce README](../../README.md#code-signing-policy)'de.

**Gizlilik:** Rotiva ağdaki başka sistemlere hiçbir bilgi göndermez. Dönüştürmeler bilgisayarınızda yapılır;
menüdeki bağlantılar (web sitesi, GitHub, destek) yalnızca tıkladığınızda tarayıcıyı açar.

## Dahil edilen motorlar

Rotiva, dönüştürmeleri yükleyiciye kendi lisanslarıyla dahil edilen şu harika açık kaynak programlarla yapar:

| Motor | Lisans | Kullanım |
|---|---|---|
| [FFmpeg](https://ffmpeg.org) | GPL-3.0 | Video ve ses |
| [ImageMagick](https://imagemagick.org) | ImageMagick License | Görseller |
| [Ghostscript](https://ghostscript.com) | AGPL-3.0 | PDF sıkıştırma, birleştirme ve bölme |
| [LibreOffice](https://www.libreoffice.org) | MPL-2.0 | Belgeler, tablolar ve sunumlar |
| [Pandoc](https://pandoc.org) | GPL-2.0-or-later | Markdown, HTML ve RTF |
| [7-Zip](https://www.7-zip.org) | LGPL-2.1-or-later | ZIP ve çıkartma |

Sürümler, lisans dosyaları ve kaynak kod bağlantıları: [THIRD-PARTY-NOTICES.md](../../THIRD-PARTY-NOTICES.md).

## Rotiva'yı destekleyin

Rotiva ücretsizdir ve reklam içermez. Size zaman kazandırıyorsa, Ko-fi'de
[geliştiriciye bir kahve ısmarlayabilirsiniz](https://ko-fi.com/denishchairany) ☕.

## Telif hakkı ve lisans

Copyright © 2026 **Denish Chairany**.
[GNU General Public License v3.0](../../LICENSE) ile lisanslanmıştır.

- ✅ Rotiva'yı kişisel veya ticari işleriniz için ücretsiz kullanın.
- ✅ Kodu inceleyin, değiştirin ve paylaşın.
- 🔁 Dağıttığınız değiştirilmiş bir sürüm de **tam kaynak koduyla birlikte GPL-3.0** olmalı ve telif hakkı
  bildirimini korumalıdır.
- 🏷️ **Rotiva adı, logosu ve simgesi lisansın parçası değildir.** Bir fork'un kendi adı ve logosu olmalı ve şunu
  belirtmelidir: `Based on Rotiva by Denish Chairany — https://github.com/uncrownedk1ng2812/rotiva`.
  Ayrıntılar [TRADEMARKS.md](../../TRADEMARKS.md)'de.

Dahil edilen motorlar, kendi lisanslarına sahip ayrı programlardır ve
[THIRD-PARTY-NOTICES.md](../../THIRD-PARTY-NOTICES.md) dosyasında listelenir.
