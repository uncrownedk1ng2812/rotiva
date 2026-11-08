<!-- languages -->
<p align="center">
  <a href="../../README.md">English</a> ·
  <b>Español</b> ·
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
  <a href="../../docs/readme/README.it.md">Italiano</a>
</p>
<!-- /languages -->

<p align="center">
  <img src="../../assets/og.png" alt="Rotiva — arrastra un archivo, pulsa Shift y suéltalo en la rueda" width="720">
</p>

<p align="center">
  <b>Convierte archivos sin abrir ninguna aplicación.</b><br>
  Arrastra un archivo, pulsa <kbd>Shift</kbd> y suéltalo en la rueda. Listo.
</p>

<p align="center">
  <a href="https://github.com/uncrownedk1ng2812/rotiva/releases/latest"><img src="https://img.shields.io/github/v/release/uncrownedk1ng2812/rotiva?label=version&color=ff8a1f" alt="Última versión"></a>
  <a href="https://apps.microsoft.com/detail/9N5FPM017GN5"><img src="https://img.shields.io/badge/Microsoft%20Store-Get%20it-0078d4" alt="Microsoft Store"></a>
  <img src="https://img.shields.io/badge/Windows-10%20%7C%2011-0078d4" alt="Windows 10 | 11">
  <img src="https://img.shields.io/badge/macOS-Apple%20Silicon%20%7C%20Intel-555" alt="macOS: Apple Silicon | Intel">
  <img src="https://img.shields.io/badge/Linux-AppImage-e95420" alt="Linux AppImage">
  <img src="https://img.shields.io/badge/precio-gratis-1f9d55" alt="Gratis">
  <img src="https://img.shields.io/badge/100%25-sin%20conexión-5a5a66" alt="100 % sin conexión">
  <a href="../../LICENSE"><img src="https://img.shields.io/badge/license-GPL--3.0-blue" alt="Licencia: GPL-3.0"></a>
</p>

<p align="center">
  <a href="https://uncrownedk1ng2812.github.io/rotiva/"><b>🌐 Web y demo en vivo</b></a>
  &nbsp;·&nbsp;
  <a href="https://github.com/uncrownedk1ng2812/rotiva/releases/latest"><b>⬇️ Descargar el instalador</b></a>
  &nbsp;·&nbsp;
  <a href="https://apps.microsoft.com/detail/9N5FPM017GN5"><b>🛍️ Microsoft Store</b></a>
  &nbsp;·&nbsp;
  <a href="https://ko-fi.com/denishchairany"><b>☕ Apoyar en Ko-fi</b></a>
</p>

> Esta es una traducción del [README en inglés](../../README.md). Si hay alguna diferencia, vale la versión en inglés.

---

> [!NOTE]
> **Rotiva fue creado desde cero por Denish Chairany** y es de código abierto bajo [GPL-3.0](../../LICENSE).
> Los forks son bienvenidos, pero deben seguir siendo de código abierto, dar crédito al autor y usar
> **otro nombre y otro logotipo**. Consulta [TRADEMARKS.md](../../TRADEMARKS.md).

## ¿Qué es Rotiva?

Rotiva es una pequeña aplicación que vive en la bandeja del sistema. Mientras arrastras una foto, un vídeo,
un documento o cualquier otro archivo, pulsa **Shift** y aparece una rueda de formatos justo en el cursor.
Apunta al formato que quieras y suelta: el archivo convertido se guarda junto al original.

Sin subir nada a convertidores en línea, sin anuncios, sin colas.

<p align="center">
  <img src="../../assets/readme/demo.gif" alt="Demostración: arrastrar un archivo, pulsar Shift, elegir un formato, soltar" width="320">
  &nbsp;&nbsp;
  <img src="../../assets/readme/wheel.png" alt="La rueda de formatos para una imagen" width="420">
</p>

## Cómo funciona

| | Paso | Detalles |
|---|---|---|
| **1** | **Arrastra un archivo** | Desde el Explorador, el escritorio o cualquier carpeta. Uno o varios a la vez. |
| **2** | **Pulsa <kbd>Shift</kbd>** | Sin soltar el botón izquierdo del ratón. La rueda aparece justo en el cursor. |
| **3** | **Apunta y suelta** | La dirección del cursor elige el formato; no hace falta caer justo sobre un segmento. Suelta en el centro para cancelar. |

¿Prefieres el clic derecho? Todo lo de la rueda también está en el menú contextual del Explorador, en
**Rotiva ▸** (Windows 11: *Mostrar más opciones*).

## Formatos compatibles

La rueda se adapta al archivo que arrastras. El formato del propio archivo se oculta automáticamente.

| Tipo | Desde | Hacia |
|---|---|---|
| 🖼️ Imágenes | PNG, JPG, WEBP, AVIF, HEIC, BMP, TIFF, GIF, ICO, PSD, SVG, JXL, DNG, CR2, NEF, ARW, … | PNG, JPG, WEBP, AVIF, PDF, ICO, TIFF, **Comprimir**, **Texto (OCR)**, **Editar** |
| 🎬 Vídeo | MP4, MKV, MOV, AVI, WEBM, WMV, FLV, 3GP, MPG, TS, VOB, … | MP4, WEBM, MOV, MKV, AVI, GIF, MP3, WAV, **Comprimir**, **Editar** |
| 🎵 Audio | MP3, WAV, FLAC, OGG, OPUS, M4A, AAC, WMA, AIFF, M4B, AMR, APE, … | MP3, WAV, FLAC, OGG, OPUS, M4A, AAC, AIFF, **Editar** |
| 📄 Documentos | DOCX, DOC, ODT, RTF, TXT, MD, HTML | PDF, DOCX, ODT, TXT, HTML, MD, RTF, EPUB |
| 📊 Hojas de cálculo | XLSX, XLS, CSV, TSV, ODS | XLSX, CSV, ODS, PDF, XLS, HTML, JSON |
| 📽️ Presentaciones | PPTX, PPT, ODP, PPSX, PPS | PDF, PPTX, ODP, PNG, JPG |
| 📕 PDF | PDF | DOCX, ODT, PNG, JPG, **Dividir**, **Comprimir**, **Texto (OCR)** |
| 🗜️ Archivos comprimidos | ZIP, RAR, 7Z, TAR.GZ, … | **Extraer**, ZIP |
| 📚 Libros electrónicos | EPUB, FB2 | PDF, EPUB, DOCX, HTML, TXT, MD |
| 💬 Subtítulos | SRT, VTT, ASS, SSA, SMI, LRC | SRT, VTT, ASS, LRC |
| 🧾 Datos | JSON, YAML | JSON, YAML, CSV |

## Funciones

<p align="center">
  <img src="../../assets/readme/features.gif" alt="MP4 a MP3, Word a PDF, muchos archivos en un ZIP, RAR extraído al instante" width="320">
</p>

- **Comprimir**: reduce fotos, vídeos (MP4 hasta 1080p) y PDF escaneados sin cambiar el formato.
- **Unir en 1 PDF**: arrastra varias imágenes, documentos, hojas de cálculo o PDF a la vez.
- **Extraer texto (OCR)**: lee las palabras de una imagen o de un PDF escaneado con el OCR integrado de Windows y cópialas al portapapeles.
- **Dividir PDF**: un PDF por página, o exporta cada página como PNG/JPG.
- **ZIP y extracción**: junta muchos archivos y carpetas en un ZIP; .zip, .rar, .7z y .tar.gz se extraen de un solo gesto.
- **Editar**: el segmento **EDIT** abre una segunda página: gira, voltea o pasa a gris imágenes; gira, voltea o invierte vídeos (invertir hasta 30 s); iguala el volumen o invierte audio, o guarda su carátula.
- **Incrustar subtítulos**: arrastra juntos un vídeo y un archivo de subtítulos y elige **BURN IN**.
- **ZIP con contraseña (Windows)**: **LOCK** empaqueta archivos en un ZIP AES-256. Ábrelo con 7-Zip o WinRAR.
- **Hoja de contactos**: **SHEET** reúne varias fotos, o 16 fotogramas de un vídeo, en una sola imagen.
- **Convertir el portapapeles (Windows)**: copia archivos o una imagen, o haz una captura, y pulsa <kbd>Ctrl</kbd>+<kbd>Alt</kbd>+<kbd>Shift</kbd>+<kbd>V</kbd>.
- **Guía integrada (Windows)**: abre **Guide** en el menú de la bandeja para ver los pasos.
- **Ligero y rápido**: la rueda es una ventana pequeña junto al cursor, sin efectos a pantalla completa, así que se abre
  al instante incluso en pantallas 4K. Elige el tamaño **S / M / L** en el menú de la bandeja.
- **¿Has cambiado de idea?** Vuelve al centro de la rueda y suelta: no se convierte nada.
- **100 % sin conexión**: FFmpeg, ImageMagick, Ghostscript, LibreOffice, Pandoc y 7-Zip vienen incluidos. Tus archivos nunca salen de tu PC.

## Instalación

1. Descarga **`Rotiva-Setup-x.y.z.exe`** desde [Releases](https://github.com/uncrownedk1ng2812/rotiva/releases/latest).
2. Ejecuta el instalador. Si aparece SmartScreen, haz clic en **Más información → Ejecutar de todas formas**
   (el instalador todavía no está firmado con un certificado de firma de código de pago).
3. Busca el icono de Rotiva en la bandeja del sistema y arrastra un archivo + <kbd>Shift</kbd>.

Se instala por usuario en `%LOCALAPPDATA%\Programs\Rotiva`: no necesita permisos de administrador ni instalar .NET.
Para desinstalar: *Configuración → Aplicaciones → Aplicaciones instaladas → Rotiva*.

Haz clic en el icono de la bandeja para abrir el menú de Rotiva: iniciar con Windows, menú contextual del Explorador,
tamaño de la rueda y enlaces a la web, GitHub y apoyo.

### macOS y Linux

Descarga el **`.dmg` para macOS** (Apple Silicon o Intel) o el **`AppImage` para Linux** desde
[Releases](https://github.com/uncrownedk1ng2812/rotiva/releases/latest).
La misma rueda y las mismas conversiones: clic derecho sobre los archivos → **Abrir con → Rotiva**, o arrastra
archivos y pulsa <kbd>Shift</kbd>. Los motores de conversión se instalan en tu sistema (`brew install …` /
`apt install …`); las notas de la versión incluyen los comandos. La app de macOS aún no está notarizada, así que
permítela una vez en *Ajustes del Sistema → Privacidad y seguridad*.

## Código abierto

Rotiva es de código abierto bajo **GPL-3.0**. La app está en [`src/Rotiva`](../../src/Rotiva) (C# / .NET 8 / WPF) y
el instalador en [`installer/`](../../installer). Consulta [BUILDING.md](../../BUILDING.md) para ejecutarla desde el código
o crear el instalador.

Los informes de errores, ideas y pull requests son bienvenidos en [Issues](https://github.com/uncrownedk1ng2812/rotiva/issues);
consulta [CONTRIBUTING.md](../../CONTRIBUTING.md) y el [Código de conducta](../../CODE_OF_CONDUCT.md). Problemas de seguridad:
infórmalos de forma privada como se explica en [SECURITY.md](../../SECURITY.md).

## Firma de código

Los instaladores de Windows todavía no tienen firma de código, así que Windows SmartScreen puede mostrar una advertencia la primera vez que ejecutes uno. Cada versión incluye las sumas SHA-256 en `SHA256SUMS.txt` para que puedas comprobar la descarga. ¿Prefieres una versión firmada? Rotiva también está en [Microsoft Store](https://apps.microsoft.com/detail/9N5FPM017GN5), firmada por Microsoft.

**Privacidad:** Rotiva no envía ninguna información a otros sistemas de la red. Las conversiones se hacen en tu
ordenador; los enlaces del menú (web, GitHub, apoyo) solo abren el navegador cuando haces clic en ellos.

## Motores incluidos

Rotiva hace las conversiones con estos excelentes programas de código abierto, incluidos en el instalador bajo
sus propias licencias:

| Motor | Licencia | Se usa para |
|---|---|---|
| [FFmpeg](https://ffmpeg.org) | GPL-3.0 | Vídeo y audio |
| [ImageMagick](https://imagemagick.org) | ImageMagick License | Imágenes |
| [Ghostscript](https://ghostscript.com) | AGPL-3.0 | Comprimir, unir y dividir PDF |
| [LibreOffice](https://www.libreoffice.org) | MPL-2.0 | Documentos, hojas de cálculo y presentaciones |
| [Pandoc](https://pandoc.org) | GPL-2.0-or-later | Markdown, HTML y RTF |
| [7-Zip](https://www.7-zip.org) | LGPL-2.1-or-later | ZIP y extracción |

Versiones, archivos de licencia y enlaces al código fuente: [THIRD-PARTY-NOTICES.md](../../THIRD-PARTY-NOTICES.md).

## Apoya Rotiva

Rotiva es gratis y sin anuncios. Si te ahorra tiempo, puedes
[invitar a un café al desarrollador](https://ko-fi.com/denishchairany) ☕ en Ko-fi.

## Copyright y licencia

Copyright © 2026 **Denish Chairany**. Publicado bajo la
[GNU General Public License v3.0](../../LICENSE).

- ✅ Usa Rotiva gratis, para uso personal o comercial.
- ✅ Estudia, modifica y comparte el código.
- 🔁 Una versión modificada que distribuyas también debe ser **GPL-3.0 con todo su código fuente** y conservar
  el aviso de copyright.
- 🏷️ **El nombre, el logotipo y el icono de Rotiva no forman parte de la licencia.** Un fork necesita su propio nombre
  y logotipo, y debe indicar: `Based on Rotiva by Denish Chairany — https://github.com/uncrownedk1ng2812/rotiva`.
  Detalles en [TRADEMARKS.md](../../TRADEMARKS.md).

Los motores incluidos son programas independientes con sus propias licencias, enumeradas en
[THIRD-PARTY-NOTICES.md](../../THIRD-PARTY-NOTICES.md).
