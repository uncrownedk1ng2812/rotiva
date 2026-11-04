<!-- languages -->
<p align="center">
  <a href="../../README.md">English</a> ·
  <a href="../../docs/readme/README.es.md">Español</a> ·
  <a href="../../docs/readme/README.ja.md">日本語</a> ·
  <a href="../../docs/readme/README.ko.md">한국어</a> ·
  <a href="../../docs/readme/README.zh-CN.md">简体中文</a> ·
  <a href="../../docs/readme/README.zh-TW.md">繁體中文</a> ·
  <b>Polski</b> ·
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
  <img src="../../assets/og.png" alt="Rotiva — przeciągnij plik, naciśnij Shift, upuść na kole" width="720">
</p>

<p align="center">
  <b>Konwertuj pliki bez otwierania żadnej aplikacji.</b><br>
  Przeciągnij plik, naciśnij <kbd>Shift</kbd> i upuść go na kole. Gotowe.
</p>

<p align="center">
  <a href="https://github.com/uncrownedk1ng2812/rotiva/releases/latest"><img src="https://img.shields.io/github/v/release/uncrownedk1ng2812/rotiva?label=version&color=ff8a1f" alt="Najnowsza wersja"></a>
  <img src="https://img.shields.io/badge/Windows-10%20%7C%2011-0078d4" alt="Windows 10 | 11">
  <img src="https://img.shields.io/badge/macOS-Apple%20Silicon%20%7C%20Intel-555" alt="macOS: Apple Silicon | Intel">
  <img src="https://img.shields.io/badge/Linux-AppImage-e95420" alt="Linux AppImage">
  <img src="https://img.shields.io/badge/cena-za%20darmo-1f9d55" alt="Za darmo">
  <img src="https://img.shields.io/badge/100%25-offline-5a5a66" alt="100% offline">
  <a href="../../LICENSE"><img src="https://img.shields.io/badge/license-GPL--3.0-blue" alt="Licencja: GPL-3.0"></a>
</p>

<p align="center">
  <a href="https://uncrownedk1ng2812.github.io/rotiva/"><b>🌐 Strona i demo na żywo</b></a>
  &nbsp;·&nbsp;
  <a href="https://github.com/uncrownedk1ng2812/rotiva/releases/latest"><b>⬇️ Pobierz instalator</b></a>
  &nbsp;·&nbsp;
  <a href="https://ko-fi.com/denishchairany"><b>☕ Wesprzyj na Ko-fi</b></a>
</p>

> To jest tłumaczenie [README w języku angielskim](../../README.md). W razie rozbieżności obowiązuje wersja angielska.

---

> [!NOTE]
> **Rotiva została stworzona od podstaw przez Denisha Chairany'ego** i jest open source na licencji [GPL-3.0](../../LICENSE).
> Forki są mile widziane, ale muszą pozostać open source, podawać autora i używać **innej nazwy oraz
> innego logo**. Zobacz [TRADEMARKS.md](../../TRADEMARKS.md).

## Czym jest Rotiva?

Rotiva to mała aplikacja, która siedzi w zasobniku systemowym. Gdy przeciągasz zdjęcie, film, dokument
albo dowolny inny plik, naciśnij **Shift**, a tuż przy kursorze pojawi się koło formatów. Wskaż wybrany format
i puść: przekonwertowany plik zapisze się obok oryginału.

Bez wysyłania plików do konwerterów online, bez reklam, bez kolejek.

<p align="center">
  <img src="../../assets/readme/demo.gif" alt="Demo: przeciągnij plik, naciśnij Shift, wybierz format, upuść" width="320">
  &nbsp;&nbsp;
  <img src="../../assets/readme/wheel.png" alt="Koło formatów dla obrazu" width="420">
</p>

## Jak to działa

| | Krok | Szczegóły |
|---|---|---|
| **1** | **Przeciągnij plik** | Z Eksploratora, pulpitu lub dowolnego folderu. Jeden plik albo kilka naraz. |
| **2** | **Naciśnij <kbd>Shift</kbd>** | Nadal trzymając lewy przycisk myszy. Koło formatów pojawi się tuż przy kursorze. |
| **3** | **Wskaż i upuść** | Format wybiera kierunek kursora — nie trzeba trafiać dokładnie w segment. Upuść na środku, aby anulować. |

Wolisz prawy przycisk myszy? Wszystko, co jest na kole, znajdziesz też w menu kontekstowym Eksploratora, pod
**Rotiva ▸** (Windows 11: *Pokaż więcej opcji*).

## Obsługiwane formaty

Koło dopasowuje się do przeciąganego pliku. Format samego pliku jest automatycznie ukrywany.

| Typ | Z | Na |
|---|---|---|
| 🖼️ Obrazy | PNG, JPG, WEBP, AVIF, HEIC, BMP, TIFF, GIF, ICO | PNG, JPG, WEBP, AVIF, PDF, ICO, TIFF, **Kompresja**, **Tekst (OCR)** |
| 🎬 Wideo | MP4, MKV, MOV, AVI, WEBM, WMV, FLV, 3GP | MP4, WEBM, MOV, MKV, AVI, GIF, MP3, WAV, **Kompresja** |
| 🎵 Audio | MP3, WAV, FLAC, OGG, OPUS, M4A, AAC, WMA | MP3, WAV, FLAC, OGG, OPUS, M4A |
| 📄 Dokumenty | DOCX, DOC, ODT, RTF, TXT, MD, HTML | PDF, DOCX, ODT, TXT, HTML, MD, RTF |
| 📊 Arkusze kalkulacyjne | XLSX, XLS, CSV, TSV, ODS | XLSX, CSV, ODS, PDF, XLS, HTML |
| 📕 PDF | PDF | DOCX, ODT, PNG, JPG, **Podział**, **Kompresja**, **Tekst (OCR)** |
| 🗜️ Archiwa | ZIP, RAR, 7Z, TAR.GZ, … | **Wypakuj**, ZIP |

## Funkcje

<p align="center">
  <img src="../../assets/readme/features.gif" alt="MP4 na MP3, Word na PDF, wiele plików w jednym ZIP, RAR rozpakowany od razu" width="320">
</p>

- **Kompresja** — zmniejsz zdjęcia, filmy (MP4 do 1080p) i zeskanowane PDF-y bez zmiany formatu.
- **Scal w 1 PDF** — przeciągnij naraz kilka obrazów, dokumentów, arkuszy lub PDF-ów.
- **Kopiuj tekst (OCR)** — odczytaj tekst z obrazu lub zeskanowanego PDF-a wbudowanym OCR Windows, prosto do schowka.
- **Dzielenie PDF** — jeden PDF na stronę albo eksport każdej strony do PNG/JPG.
- **ZIP i wypakowywanie** — spakuj wiele plików i folderów do jednego ZIP-a; .zip, .rar, .7z i .tar.gz wypakujesz jednym ruchem.
- **Lekka i szybka** — koło to małe okno przy kursorze, bez efektów pełnoekranowych, więc otwiera się
  natychmiast nawet na ekranach 4K. Rozmiar **S / M / L** wybierzesz w menu zasobnika.
- **Zmiana zdania?** Wróć na środek koła i puść: nic nie zostanie przekonwertowane.
- **100% offline** — FFmpeg, ImageMagick, Ghostscript, LibreOffice, Pandoc i 7-Zip są w zestawie. Twoje pliki nigdy nie opuszczają komputera.

## Instalacja

1. Pobierz **`Rotiva-Setup-x.y.z.exe`** z [Releases](https://github.com/uncrownedk1ng2812/rotiva/releases/latest).
2. Uruchom instalator. Jeśli pojawi się SmartScreen, kliknij **Więcej informacji → Uruchom mimo to**
   (instalator nie jest jeszcze podpisany płatnym certyfikatem podpisywania kodu).
3. Znajdź ikonę Rotivy w zasobniku systemowym, a potem przeciągnij plik + <kbd>Shift</kbd>.

Instaluje się dla bieżącego użytkownika w `%LOCALAPPDATA%\Programs\Rotiva` — bez uprawnień administratora i bez instalowania .NET.
Odinstalowanie: *Ustawienia → Aplikacje → Zainstalowane aplikacje → Rotiva*.

Kliknij ikonę w zasobniku, aby otworzyć menu Rotivy: uruchamianie z Windows, menu kontekstowe Eksploratora,
rozmiar koła oraz linki do strony, GitHuba i wsparcia.

### macOS i Linux

Pobierz **`.dmg` dla macOS** (Apple Silicon lub Intel) albo **`AppImage` dla Linuksa** z
[Releases](https://github.com/uncrownedk1ng2812/rotiva/releases/latest).
To samo koło i te same konwersje: prawy przycisk na plikach → **Otwórz za pomocą → Rotiva** albo przeciągnij pliki
i naciśnij <kbd>Shift</kbd>. Silniki konwersji pochodzą z systemu (`brew install …` / `apt install …`); polecenia
znajdziesz w informacjach o wydaniu. Aplikacja na macOS nie jest jeszcze notaryzowana, więc zezwól na nią raz w
*Ustawieniach systemowych → Prywatność i ochrona*.

## Open source

Rotiva jest open source na licencji **GPL-3.0**. Aplikacja znajduje się w [`src/Rotiva`](../../src/Rotiva) (C# / .NET 8 / WPF),
a instalator w [`installer/`](../../installer). Zobacz [BUILDING.md](../../BUILDING.md), aby uruchomić ją ze źródeł lub
zbudować instalator.

Zgłoszenia błędów, pomysły i pull requesty są mile widziane w [Issues](https://github.com/uncrownedk1ng2812/rotiva/issues);
zobacz [CONTRIBUTING.md](../../CONTRIBUTING.md) i [Kodeks postępowania](../../CODE_OF_CONDUCT.md). Problemy z bezpieczeństwem
zgłaszaj prywatnie, jak opisano w [SECURITY.md](../../SECURITY.md).

## Zasady podpisywania kodu

Instalatory dla Windows są budowane z tego repozytorium przez
[workflow instalatora Windows](../../.github/workflows/windows-installer.yml) i tam podpisywane; nic, co zbudowano
gdzie indziej, nie jest podpisywane. Bezpłatne podpisywanie kodu zapewnia [SignPath.io](https://about.signpath.io),
certyfikat — [SignPath Foundation](https://signpath.org). Pełne szczegóły w
[README w języku angielskim](../../README.md#code-signing-policy).

**Prywatność:** Rotiva nie wysyła żadnych informacji do innych systemów w sieci. Konwersje odbywają się na Twoim
komputerze; linki w menu (strona, GitHub, wsparcie) otwierają przeglądarkę tylko wtedy, gdy je klikniesz.

## Dołączone silniki

Rotiva wykonuje konwersje za pomocą tych świetnych programów open source, dołączonych do instalatora na
ich własnych licencjach:

| Silnik | Licencja | Do czego |
|---|---|---|
| [FFmpeg](https://ffmpeg.org) | GPL-3.0 | Wideo i audio |
| [ImageMagick](https://imagemagick.org) | ImageMagick License | Obrazy |
| [Ghostscript](https://ghostscript.com) | AGPL-3.0 | Kompresja, scalanie i dzielenie PDF |
| [LibreOffice](https://www.libreoffice.org) | MPL-2.0 | Dokumenty i arkusze |
| [Pandoc](https://pandoc.org) | GPL-2.0-or-later | Markdown, HTML i RTF |
| [7-Zip](https://www.7-zip.org) | LGPL-2.1-or-later | ZIP i wypakowywanie |

Wersje, pliki licencji i linki do kodu źródłowego: [THIRD-PARTY-NOTICES.md](../../THIRD-PARTY-NOTICES.md).

## Wesprzyj Rotivę

Rotiva jest darmowa i bez reklam. Jeśli oszczędza Ci czas, możesz
[postawić autorowi kawę](https://ko-fi.com/denishchairany) ☕ na Ko-fi.

## Prawa autorskie i licencja

Copyright © 2026 **Denish Chairany**. Udostępniono na licencji
[GNU General Public License v3.0](../../LICENSE).

- ✅ Korzystaj z Rotivy za darmo, prywatnie lub komercyjnie.
- ✅ Analizuj, modyfikuj i udostępniaj kod.
- 🔁 Zmodyfikowana wersja, którą rozpowszechniasz, również musi być na **GPL-3.0 z pełnym kodem źródłowym** i
  zachować informację o prawach autorskich.
- 🏷️ **Nazwa, logo i ikona Rotivy nie są objęte licencją.** Fork potrzebuje własnej nazwy i logo
  i musi zawierać: `Based on Rotiva by Denish Chairany — https://github.com/uncrownedk1ng2812/rotiva`.
  Szczegóły w [TRADEMARKS.md](../../TRADEMARKS.md).

Dołączone silniki to osobne programy na własnych licencjach, wymienionych w
[THIRD-PARTY-NOTICES.md](../../THIRD-PARTY-NOTICES.md).
