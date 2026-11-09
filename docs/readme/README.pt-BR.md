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
  <b>Português (Brasil)</b> ·
  <a href="../../docs/readme/README.pt-PT.md">Português (Portugal)</a> ·
  <a href="../../docs/readme/README.fr.md">Français</a> ·
  <a href="../../docs/readme/README.th.md">ไทย</a> ·
  <a href="../../docs/readme/README.tr.md">Türkçe</a> ·
  <a href="../../docs/readme/README.id.md">Bahasa Indonesia</a> ·
  <a href="../../docs/readme/README.it.md">Italiano</a>
</p>
<!-- /languages -->

<p align="center">
  <img src="../../assets/og.png" alt="Rotiva — arraste um arquivo, aperte Shift e solte na roda" width="720">
</p>

<p align="center">
  <b>Converta arquivos sem abrir nenhum aplicativo.</b><br>
  Arraste um arquivo, aperte <kbd>Shift</kbd> e solte na roda. Pronto.
</p>

<p align="center">
  <a href="https://apps.microsoft.com/detail/9N5FPM017GN5"><img src="../../assets/microsoft-store-badge.svg" alt="Get it from Microsoft Store" width="240"></a>
</p>

<p align="center">
  <a href="https://github.com/uncrownedk1ng2812/rotiva/releases/latest"><img src="https://img.shields.io/github/v/release/uncrownedk1ng2812/rotiva?label=version&color=ff8a1f" alt="Versão mais recente"></a>
  <img src="https://img.shields.io/badge/Windows-10%20%7C%2011-0078d4" alt="Windows 10 | 11">
  <img src="https://img.shields.io/badge/macOS-Apple%20Silicon%20%7C%20Intel-555" alt="macOS: Apple Silicon | Intel">
  <img src="https://img.shields.io/badge/Linux-AppImage-e95420" alt="Linux AppImage">
  <img src="https://img.shields.io/badge/pre%C3%A7o-gr%C3%A1tis-1f9d55" alt="Grátis">
  <img src="https://img.shields.io/badge/100%25-offline-5a5a66" alt="100% offline">
  <a href="../../LICENSE"><img src="https://img.shields.io/badge/license-GPL--3.0-blue" alt="Licença: GPL-3.0"></a>
</p>

<p align="center">
  <a href="https://uncrownedk1ng2812.github.io/rotiva/"><b>🌐 Site e demonstração</b></a>
  &nbsp;·&nbsp;
  <a href="https://github.com/uncrownedk1ng2812/rotiva/releases/latest"><b>⬇️ Baixar o instalador</b></a>
  &nbsp;·&nbsp;
  <a href="https://apps.microsoft.com/detail/9N5FPM017GN5"><b>🛍️ Microsoft Store</b></a>
  &nbsp;·&nbsp;
  <a href="https://ko-fi.com/denishchairany"><b>☕ Apoiar no Ko-fi</b></a>
</p>

> Esta é uma tradução do [README em inglês](../../README.md). Em caso de diferença, vale a versão em inglês.

---

> [!NOTE]
> **O Rotiva foi criado do zero por Denish Chairany** e é open source sob a [GPL-3.0](../../LICENSE).
> Forks são bem-vindos, mas precisam continuar open source, dar crédito ao autor e usar **outro nome e
> outro logotipo**. Veja [TRADEMARKS.md](../../TRADEMARKS.md).

## O que é o Rotiva?

O Rotiva é um aplicativo pequeno que fica na bandeja do sistema. Enquanto você arrasta uma foto, um vídeo,
um documento ou qualquer outro arquivo, aperte **Shift** e uma roda de formatos aparece bem no cursor.
Aponte para o formato que quiser e solte: o arquivo convertido é salvo ao lado do original.

Sem enviar nada para conversores online, sem anúncios, sem filas.

<p align="center">
  <img src="../../assets/readme/demo.gif" alt="Demonstração: arrastar um arquivo, apertar Shift, escolher um formato, soltar" width="320">
  &nbsp;&nbsp;
  <img src="../../assets/readme/wheel.png" alt="A roda de formatos para uma imagem" width="420">
</p>

## Como funciona

| | Passo | Detalhes |
|---|---|---|
| **1** | **Arraste um arquivo** | Do Explorador de Arquivos, da área de trabalho ou de qualquer pasta. Um ou vários de uma vez. |
| **2** | **Aperte <kbd>Shift</kbd>** | Ainda segurando o botão esquerdo do mouse. A roda aparece bem no cursor. |
| **3** | **Aponte e solte** | A direção do cursor escolhe o formato; não é preciso cair exatamente em um segmento. Solte no centro para cancelar. |

Prefere o botão direito? Tudo o que está na roda também fica no menu de contexto do Explorador, em
**Rotiva ▸** (Windows 11: *Mostrar mais opções*).

## Formatos suportados

A roda se adapta ao arquivo que você arrasta. O formato do próprio arquivo é escondido automaticamente.

| Tipo | De | Para |
|---|---|---|
| 🖼️ Imagens | PNG, JPG, WEBP, AVIF, HEIC, BMP, TIFF, GIF, ICO, PSD, SVG, JXL, DNG, CR2, NEF, ARW, … | PNG, JPG, WEBP, AVIF, PDF, ICO, TIFF, **Comprimir**, **Texto (OCR)**, **Editar** |
| 🎬 Vídeo | MP4, MKV, MOV, AVI, WEBM, WMV, FLV, 3GP, MPG, TS, VOB, … | MP4, WEBM, MOV, MKV, AVI, GIF, MP3, WAV, **Comprimir**, **Editar** |
| 🎵 Áudio | MP3, WAV, FLAC, OGG, OPUS, M4A, AAC, WMA, AIFF, M4B, AMR, APE, … | MP3, WAV, FLAC, OGG, OPUS, M4A, AAC, AIFF, **Editar** |
| 📄 Documentos | DOCX, DOC, ODT, RTF, TXT, MD, HTML | PDF, DOCX, ODT, TXT, HTML, MD, RTF, EPUB |
| 📊 Planilhas | XLSX, XLS, CSV, TSV, ODS | XLSX, CSV, ODS, PDF, XLS, HTML, JSON |
| 📽️ Apresentações | PPTX, PPT, ODP, PPSX, PPS | PDF, PPTX, ODP, PNG, JPG |
| 📕 PDF | PDF | DOCX, ODT, PNG, JPG, **Dividir**, **Comprimir**, **Texto (OCR)** |
| 🗜️ Arquivos compactados | ZIP, RAR, 7Z, TAR.GZ, … | **Extrair**, ZIP |
| 📚 E-books | EPUB, FB2 | PDF, EPUB, DOCX, HTML, TXT, MD |
| 💬 Legendas | SRT, VTT, ASS, SSA, SMI, LRC | SRT, VTT, ASS, LRC |
| 🧾 Dados | JSON, YAML | JSON, YAML, CSV |

## Recursos

<p align="center">
  <img src="../../assets/readme/features.gif" alt="MP4 para MP3, Word para PDF, vários arquivos em um ZIP, RAR extraído na hora" width="320">
</p>

- **Comprimir**: diminua fotos, vídeos (MP4 até 1080p) e PDFs escaneados sem mudar o formato.
- **Juntar em 1 PDF**: arraste várias imagens, documentos, planilhas ou PDFs de uma vez.
- **Copiar texto (OCR)**: leia as palavras de uma imagem ou de um PDF escaneado com o OCR do próprio Windows, direto para a área de transferência.
- **Dividir PDFs**: um PDF por página, ou exporte cada página como PNG/JPG.
- **ZIP e extração**: junte vários arquivos e pastas em um ZIP; .zip, .rar, .7z e .tar.gz são extraídos com um gesto só.
- **Editar**: o segmento **EDIT** abre uma segunda página: gire, espelhe ou deixe imagens em cinza; gire, espelhe ou inverta vídeos (inverter até 30 s); iguale o volume ou inverta áudio, ou salve a capa.
- **Embutir legendas**: arraste juntos um vídeo e um arquivo de legendas e escolha **BURN IN**.
- **ZIP com senha (Windows)**: **LOCK** compacta os arquivos em um ZIP AES-256. Abra com 7-Zip ou WinRAR.
- **Folha de contatos**: **SHEET** reúne várias fotos, ou 16 quadros de um vídeo, em uma só imagem.
- **Converter a área de transferência (Windows)**: copie arquivos ou uma imagem, ou tire um print, e pressione <kbd>Ctrl</kbd>+<kbd>Alt</kbd>+<kbd>Shift</kbd>+<kbd>V</kbd>.
- **Guia integrado (Windows)**: abra **Guide** no menu da bandeja.
- **Leve e rápido**: a roda é uma janela pequena no cursor, sem efeitos de tela cheia, então abre na hora
  mesmo em telas 4K. Escolha o tamanho **S / M / L** no menu da bandeja.
- **Mudou de ideia?** Volte para o centro da roda e solte: nada é convertido.
- **100% offline**: FFmpeg, ImageMagick, Ghostscript, LibreOffice, Pandoc e 7-Zip já vêm junto. Seus arquivos nunca saem do seu PC.

## Instalação

1. Baixe **`Rotiva-Setup-x.y.z.exe`** em [Releases](https://github.com/uncrownedk1ng2812/rotiva/releases/latest).
2. Execute o instalador. Se o SmartScreen aparecer, clique em **Mais informações → Executar assim mesmo**
   (o instalador ainda não é assinado com um certificado de assinatura de código pago).
3. Procure o ícone do Rotiva na bandeja do sistema e arraste um arquivo + <kbd>Shift</kbd>.

Instala por usuário em `%LOCALAPPDATA%\Programs\Rotiva`: não precisa de permissão de administrador nem de instalar o .NET.
Para desinstalar: *Configurações → Aplicativos → Aplicativos instalados → Rotiva*.

Clique no ícone da bandeja para abrir o menu do Rotiva: iniciar com o Windows, menu de contexto do Explorador,
tamanho da roda e links para o site, o GitHub e o apoio.

### macOS e Linux

Baixe o **`.dmg` para macOS** (Apple Silicon ou Intel) ou o **`AppImage` para Linux** em
[Releases](https://github.com/uncrownedk1ng2812/rotiva/releases/latest).
A mesma roda e as mesmas conversões: clique com o botão direito nos arquivos → **Abrir com → Rotiva**, ou arraste
arquivos e aperte <kbd>Shift</kbd>. Os mecanismos de conversão vêm do seu sistema (`brew install …` /
`apt install …`); as notas da versão trazem os comandos. O app para macOS ainda não é notarizado, então
permita-o uma vez em *Ajustes do Sistema → Privacidade e Segurança*.

## Open source

O Rotiva é open source sob a **GPL-3.0**. O app está em [`src/Rotiva`](../../src/Rotiva) (C# / .NET 8 / WPF) e
o instalador em [`installer/`](../../installer). Veja [BUILDING.md](../../BUILDING.md) para rodar a partir do código-fonte
ou gerar o instalador.

Relatos de bugs, ideias e pull requests são bem-vindos em [Issues](https://github.com/uncrownedk1ng2812/rotiva/issues);
veja [CONTRIBUTING.md](../../CONTRIBUTING.md) e o [Código de Conduta](../../CODE_OF_CONDUCT.md). Problemas de segurança:
relate em particular, como explicado em [SECURITY.md](../../SECURITY.md).

## Assinatura de código

Os instaladores do Windows ainda não têm assinatura de código, então o Windows SmartScreen pode mostrar um aviso na primeira vez que você executar um. Cada versão traz as somas SHA-256 em `SHA256SUMS.txt` para você conferir o download. Prefere uma versão assinada? O Rotiva também está na [Microsoft Store](https://apps.microsoft.com/detail/9N5FPM017GN5), assinado pela Microsoft.

**Privacidade:** o Rotiva não envia nenhuma informação para outros sistemas na rede. As conversões acontecem no
seu computador; os links do menu (site, GitHub, apoio) só abrem o navegador quando você clica neles.

## Mecanismos incluídos

O Rotiva faz as conversões com estes ótimos programas open source, incluídos no instalador sob
suas próprias licenças:

| Mecanismo | Licença | Usado para |
|---|---|---|
| [FFmpeg](https://ffmpeg.org) | GPL-3.0 | Vídeo e áudio |
| [ImageMagick](https://imagemagick.org) | ImageMagick License | Imagens |
| [Ghostscript](https://ghostscript.com) | AGPL-3.0 | Comprimir, juntar e dividir PDF |
| [LibreOffice](https://www.libreoffice.org) | MPL-2.0 | Documentos, planilhas e apresentações |
| [Pandoc](https://pandoc.org) | GPL-2.0-or-later | Markdown, HTML e RTF |
| [7-Zip](https://www.7-zip.org) | LGPL-2.1-or-later | ZIP e extração |

Versões, arquivos de licença e links para o código-fonte: [THIRD-PARTY-NOTICES.md](../../THIRD-PARTY-NOTICES.md).

## Apoie o Rotiva

O Rotiva é grátis e sem anúncios. Se ele economiza seu tempo, você pode
[pagar um café para o desenvolvedor](https://ko-fi.com/denishchairany) ☕ no Ko-fi.

## Copyright e licença

Copyright © 2026 **Denish Chairany**. Licenciado sob a
[GNU General Public License v3.0](../../LICENSE).

- ✅ Use o Rotiva de graça, para uso pessoal ou comercial.
- ✅ Estude, modifique e compartilhe o código.
- 🔁 Uma versão modificada que você distribuir também precisa ser **GPL-3.0 com todo o código-fonte** e manter
  o aviso de copyright.
- 🏷️ **O nome, o logotipo e o ícone do Rotiva não fazem parte da licença.** Um fork precisa de nome e logotipo
  próprios e deve informar: `Based on Rotiva by Denish Chairany — https://github.com/uncrownedk1ng2812/rotiva`.
  Detalhes em [TRADEMARKS.md](../../TRADEMARKS.md).

Os mecanismos incluídos são programas separados com suas próprias licenças, listadas em
[THIRD-PARTY-NOTICES.md](../../THIRD-PARTY-NOTICES.md).
