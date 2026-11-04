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
  <a href="../../docs/readme/README.id.md">Bahasa Indonesia</a> ·
  <b>Italiano</b>
</p>
<!-- /languages -->

<p align="center">
  <img src="../../assets/og.png" alt="Rotiva — trascina un file, premi Shift e rilascialo sulla ruota" width="720">
</p>

<p align="center">
  <b>Converti i file senza aprire nessuna app.</b><br>
  Trascina un file, premi <kbd>Shift</kbd> e rilascialo sulla ruota. Fatto.
</p>

<p align="center">
  <a href="https://github.com/uncrownedk1ng2812/rotiva/releases/latest"><img src="https://img.shields.io/github/v/release/uncrownedk1ng2812/rotiva?label=version&color=ff8a1f" alt="Ultima versione"></a>
  <img src="https://img.shields.io/badge/Windows-10%20%7C%2011-0078d4" alt="Windows 10 | 11">
  <img src="https://img.shields.io/badge/macOS-Apple%20Silicon%20%7C%20Intel-555" alt="macOS: Apple Silicon | Intel">
  <img src="https://img.shields.io/badge/Linux-AppImage-e95420" alt="Linux AppImage">
  <img src="https://img.shields.io/badge/prezzo-gratis-1f9d55" alt="Gratis">
  <img src="https://img.shields.io/badge/100%25-offline-5a5a66" alt="100% offline">
  <a href="../../LICENSE"><img src="https://img.shields.io/badge/license-GPL--3.0-blue" alt="Licenza: GPL-3.0"></a>
</p>

<p align="center">
  <a href="https://uncrownedk1ng2812.github.io/rotiva/"><b>🌐 Sito e demo</b></a>
  &nbsp;·&nbsp;
  <a href="https://github.com/uncrownedk1ng2812/rotiva/releases/latest"><b>⬇️ Scarica il programma di installazione</b></a>
  &nbsp;·&nbsp;
  <a href="https://ko-fi.com/denishchairany"><b>☕ Supporta su Ko-fi</b></a>
</p>

> Questa è una traduzione del [README in inglese](../../README.md). In caso di differenze, vale la versione inglese.

---

> [!NOTE]
> **Rotiva è stato creato da zero da Denish Chairany** ed è open source con licenza [GPL-3.0](../../LICENSE).
> I fork sono benvenuti, ma devono restare open source, citare l'autore e usare **un nome e un logo
> diversi**. Vedi [TRADEMARKS.md](../../TRADEMARKS.md).

## Cos'è Rotiva?

Rotiva è una piccola app che vive nell'area di notifica. Mentre trascini una foto, un video, un documento o
qualsiasi altro file, premi **Shift** e compare una ruota dei formati proprio sotto il cursore. Punta al formato
che vuoi e rilascia: il file convertito viene salvato accanto all'originale.

Niente caricamenti su convertitori online, niente pubblicità, niente code.

<p align="center">
  <img src="../../assets/readme/demo.gif" alt="Demo: trascina un file, premi Shift, scegli un formato, rilascia" width="320">
  &nbsp;&nbsp;
  <img src="../../assets/readme/wheel.png" alt="La ruota dei formati per un'immagine" width="420">
</p>

## Come funziona

| | Passo | Dettagli |
|---|---|---|
| **1** | **Trascina un file** | Da Esplora file, dal desktop o da qualsiasi cartella. Un file o più file insieme. |
| **2** | **Premi <kbd>Shift</kbd>** | Tenendo ancora premuto il tasto sinistro del mouse. La ruota compare proprio sotto il cursore. |
| **3** | **Punta e rilascia** | È la direzione del cursore a scegliere il formato: non serve centrare un segmento. Rilascia al centro per annullare. |

Preferisci il clic destro? Tutto ciò che c'è sulla ruota si trova anche nel menu contestuale di Esplora file, sotto
**Rotiva ▸** (Windows 11: *Mostra altre opzioni*).

## Formati supportati

La ruota si adatta al file che trascini. Il formato del file stesso viene nascosto automaticamente.

| Tipo | Da | A |
|---|---|---|
| 🖼️ Immagini | PNG, JPG, WEBP, AVIF, HEIC, BMP, TIFF, GIF, ICO | PNG, JPG, WEBP, AVIF, PDF, ICO, TIFF, **Comprimi**, **Testo (OCR)** |
| 🎬 Video | MP4, MKV, MOV, AVI, WEBM, WMV, FLV, 3GP | MP4, WEBM, MOV, MKV, AVI, GIF, MP3, WAV, **Comprimi** |
| 🎵 Audio | MP3, WAV, FLAC, OGG, OPUS, M4A, AAC, WMA | MP3, WAV, FLAC, OGG, OPUS, M4A |
| 📄 Documenti | DOCX, DOC, ODT, RTF, TXT, MD, HTML | PDF, DOCX, ODT, TXT, HTML, MD, RTF |
| 📊 Fogli di calcolo | XLSX, XLS, CSV, TSV, ODS | XLSX, CSV, ODS, PDF, XLS, HTML |
| 📕 PDF | PDF | DOCX, ODT, PNG, JPG, **Dividi**, **Comprimi**, **Testo (OCR)** |
| 🗜️ Archivi | ZIP, RAR, 7Z, TAR.GZ, … | **Estrai**, ZIP |

## Funzionalità

<p align="center">
  <img src="../../assets/readme/features.gif" alt="Da MP4 a MP3, da Word a PDF, tanti file in uno ZIP, RAR estratto all'istante" width="320">
</p>

- **Comprimi**: riduci foto, video (MP4 fino a 1080p) e PDF scansionati senza cambiare formato.
- **Unisci in 1 PDF**: trascina insieme più immagini, documenti, fogli di calcolo o PDF.
- **Copia il testo (OCR)**: leggi le parole di un'immagine o di un PDF scansionato con l'OCR integrato di Windows, direttamente negli appunti.
- **Dividi i PDF**: un PDF per pagina, oppure esporta ogni pagina in PNG/JPG.
- **ZIP ed estrazione**: raccogli tanti file e cartelle in un unico ZIP; .zip, .rar, .7z e .tar.gz si estraggono con un solo gesto.
- **Leggero e veloce**: la ruota è una piccola finestra sotto il cursore, senza effetti a schermo intero, quindi si apre
  all'istante anche su schermi 4K. Scegli la dimensione **S / M / L** dal menu dell'area di notifica.
- **Hai cambiato idea?** Torna al centro della ruota e rilascia: non viene convertito nulla.
- **100% offline**: FFmpeg, ImageMagick, Ghostscript, LibreOffice, Pandoc e 7-Zip sono inclusi. I tuoi file non lasciano mai il tuo PC.

## Installazione

1. Scarica **`Rotiva-Setup-x.y.z.exe`** dalle [Releases](https://github.com/uncrownedk1ng2812/rotiva/releases/latest).
2. Avvia il programma di installazione. Se compare SmartScreen, fai clic su **Ulteriori informazioni → Esegui comunque**
   (il programma di installazione non è ancora firmato con un certificato di firma del codice a pagamento).
3. Trova l'icona di Rotiva nell'area di notifica, poi trascina un file + <kbd>Shift</kbd>.

Si installa per utente in `%LOCALAPPDATA%\Programs\Rotiva`: non servono diritti di amministratore né l'installazione di .NET.
Per disinstallare: *Impostazioni → App → App installate → Rotiva*.

Fai clic sull'icona nell'area di notifica per aprire il menu di Rotiva: avvio con Windows, menu contestuale di Esplora file,
dimensione della ruota e link al sito, a GitHub e al supporto.

### macOS e Linux

Scarica il **`.dmg` per macOS** (Apple Silicon o Intel) o l'**`AppImage` per Linux** dalle
[Releases](https://github.com/uncrownedk1ng2812/rotiva/releases/latest).
Stessa ruota e stesse conversioni: clic destro sui file → **Apri con → Rotiva**, oppure trascina i file e premi
<kbd>Shift</kbd>. I motori di conversione vengono dal tuo sistema (`brew install …` / `apt install …`); le note di
rilascio riportano i comandi. L'app per macOS non è ancora notarizzata, quindi consentila una volta in
*Impostazioni di Sistema → Privacy e sicurezza*.

## Open source

Rotiva è open source con licenza **GPL-3.0**. L'app si trova in [`src/Rotiva`](../../src/Rotiva) (C# / .NET 8 / WPF) e
il programma di installazione in [`installer/`](../../installer). Vedi [BUILDING.md](../../BUILDING.md) per eseguirla dal
codice sorgente o creare il programma di installazione.

Segnalazioni di bug, idee e pull request sono benvenute nelle [Issues](https://github.com/uncrownedk1ng2812/rotiva/issues);
vedi [CONTRIBUTING.md](../../CONTRIBUTING.md) e il [Codice di condotta](../../CODE_OF_CONDUCT.md). Problemi di sicurezza:
segnalali in privato come spiegato in [SECURITY.md](../../SECURITY.md).

## Politica di firma del codice

I programmi di installazione per Windows vengono creati da questo repository dal
[workflow del programma di installazione Windows](../../.github/workflows/windows-installer.yml) e firmati lì; nulla di
creato altrove viene firmato. Firma del codice gratuita offerta da [SignPath.io](https://about.signpath.io),
certificato della [SignPath Foundation](https://signpath.org). Tutti i dettagli nel
[README in inglese](../../README.md#code-signing-policy).

**Privacy:** Rotiva non invia alcuna informazione ad altri sistemi in rete. Le conversioni avvengono sul tuo
computer; i link del menu (sito, GitHub, supporto) aprono il browser solo quando ci fai clic.

## Motori inclusi

Rotiva esegue le conversioni con questi ottimi programmi open source, inclusi nel programma di installazione con
le rispettive licenze:

| Motore | Licenza | Usato per |
|---|---|---|
| [FFmpeg](https://ffmpeg.org) | GPL-3.0 | Video e audio |
| [ImageMagick](https://imagemagick.org) | ImageMagick License | Immagini |
| [Ghostscript](https://ghostscript.com) | AGPL-3.0 | Comprimere, unire e dividere PDF |
| [LibreOffice](https://www.libreoffice.org) | MPL-2.0 | Documenti e fogli di calcolo |
| [Pandoc](https://pandoc.org) | GPL-2.0-or-later | Markdown, HTML e RTF |
| [7-Zip](https://www.7-zip.org) | LGPL-2.1-or-later | ZIP ed estrazione |

Versioni, file di licenza e link al codice sorgente: [THIRD-PARTY-NOTICES.md](../../THIRD-PARTY-NOTICES.md).

## Supporta Rotiva

Rotiva è gratuito e senza pubblicità. Se ti fa risparmiare tempo, puoi
[offrire un caffè allo sviluppatore](https://ko-fi.com/denishchairany) ☕ su Ko-fi.

## Copyright e licenza

Copyright © 2026 **Denish Chairany**. Distribuito con licenza
[GNU General Public License v3.0](../../LICENSE).

- ✅ Usa Rotiva gratis, per uso personale o commerciale.
- ✅ Studia, modifica e condividi il codice.
- 🔁 Una versione modificata che distribuisci deve essere anch'essa **GPL-3.0 con tutto il codice sorgente** e
  mantenere l'avviso di copyright.
- 🏷️ **Il nome, il logo e l'icona di Rotiva non fanno parte della licenza.** Un fork deve avere un nome e un logo
  propri e deve indicare: `Based on Rotiva by Denish Chairany — https://github.com/uncrownedk1ng2812/rotiva`.
  Dettagli in [TRADEMARKS.md](../../TRADEMARKS.md).

I motori inclusi sono programmi separati con le proprie licenze, elencate in
[THIRD-PARTY-NOTICES.md](../../THIRD-PARTY-NOTICES.md).
