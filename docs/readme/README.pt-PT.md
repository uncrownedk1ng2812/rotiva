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
  <b>Português (Portugal)</b> ·
  <a href="../../docs/readme/README.fr.md">Français</a> ·
  <a href="../../docs/readme/README.th.md">ไทย</a> ·
  <a href="../../docs/readme/README.tr.md">Türkçe</a> ·
  <a href="../../docs/readme/README.id.md">Bahasa Indonesia</a> ·
  <a href="../../docs/readme/README.it.md">Italiano</a>
</p>
<!-- /languages -->

<p align="center">
  <img src="../../assets/og.png" alt="Rotiva — arraste um ficheiro, prima Shift e largue-o na roda" width="720">
</p>

<p align="center">
  <b>Converta ficheiros sem abrir nenhuma aplicação.</b><br>
  Arraste um ficheiro, prima <kbd>Shift</kbd> e largue-o na roda. Está feito.
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
  <a href="https://github.com/uncrownedk1ng2812/rotiva/releases/latest"><b>⬇️ Transferir o instalador</b></a>
  &nbsp;·&nbsp;
  <a href="https://ko-fi.com/denishchairany"><b>☕ Apoiar no Ko-fi</b></a>
</p>

> Esta é uma tradução do [README em inglês](../../README.md). Em caso de diferença, prevalece a versão em inglês.

---

> [!NOTE]
> **O Rotiva foi criado de raiz por Denish Chairany** e é open source sob a [GPL-3.0](../../LICENSE).
> Os forks são bem-vindos, mas têm de continuar open source, dar crédito ao autor e usar **outro nome e
> outro logótipo**. Consulte [TRADEMARKS.md](../../TRADEMARKS.md).

## O que é o Rotiva?

O Rotiva é uma pequena aplicação que fica na área de notificação. Enquanto arrasta uma fotografia, um vídeo,
um documento ou qualquer outro ficheiro, prima **Shift** e aparece uma roda de formatos mesmo junto ao cursor.
Aponte para o formato que pretende e largue: o ficheiro convertido é guardado ao lado do original.

Sem enviar nada para conversores online, sem publicidade, sem filas de espera.

<p align="center">
  <img src="../../assets/readme/demo.gif" alt="Demonstração: arrastar um ficheiro, premir Shift, escolher um formato, largar" width="320">
  &nbsp;&nbsp;
  <img src="../../assets/readme/wheel.png" alt="A roda de formatos para uma imagem" width="420">
</p>

## Como funciona

| | Passo | Detalhes |
|---|---|---|
| **1** | **Arraste um ficheiro** | A partir do Explorador de Ficheiros, do ambiente de trabalho ou de qualquer pasta. Um ou vários de uma vez. |
| **2** | **Prima <kbd>Shift</kbd>** | Sem largar o botão esquerdo do rato. A roda aparece mesmo junto ao cursor. |
| **3** | **Aponte e largue** | A direção do cursor escolhe o formato; não é preciso acertar exatamente num segmento. Largue no centro para cancelar. |

Prefere o botão direito? Tudo o que está na roda está também no menu de contexto do Explorador, em
**Rotiva ▸** (Windows 11: *Mostrar mais opções*).

## Formatos suportados

A roda adapta-se ao ficheiro que arrasta. O formato do próprio ficheiro é ocultado automaticamente.

| Tipo | De | Para |
|---|---|---|
| 🖼️ Imagens | PNG, JPG, WEBP, AVIF, HEIC, BMP, TIFF, GIF, ICO | PNG, JPG, WEBP, AVIF, PDF, ICO, TIFF, **Comprimir**, **Texto (OCR)** |
| 🎬 Vídeo | MP4, MKV, MOV, AVI, WEBM, WMV, FLV, 3GP | MP4, WEBM, MOV, MKV, AVI, GIF, MP3, WAV, **Comprimir** |
| 🎵 Áudio | MP3, WAV, FLAC, OGG, OPUS, M4A, AAC, WMA | MP3, WAV, FLAC, OGG, OPUS, M4A |
| 📄 Documentos | DOCX, DOC, ODT, RTF, TXT, MD, HTML | PDF, DOCX, ODT, TXT, HTML, MD, RTF |
| 📊 Folhas de cálculo | XLSX, XLS, CSV, TSV, ODS | XLSX, CSV, ODS, PDF, XLS, HTML |
| 📽️ Apresentações | PPTX, PPT, ODP, PPSX, PPS | PDF, PPTX, ODP, PNG, JPG |
| 📕 PDF | PDF | DOCX, ODT, PNG, JPG, **Dividir**, **Comprimir**, **Texto (OCR)** |
| 🗜️ Arquivos comprimidos | ZIP, RAR, 7Z, TAR.GZ, … | **Extrair**, ZIP |

## Funcionalidades

<p align="center">
  <img src="../../assets/readme/features.gif" alt="MP4 para MP3, Word para PDF, muitos ficheiros num ZIP, RAR extraído na hora" width="320">
</p>

- **Comprimir**: reduza fotografias, vídeos (MP4 até 1080p) e PDF digitalizados sem mudar o formato.
- **Juntar num só PDF**: arraste várias imagens, documentos, folhas de cálculo ou PDF de uma vez.
- **Copiar texto (OCR)**: leia as palavras de uma imagem ou de um PDF digitalizado com o OCR incluído no Windows, diretamente para a área de transferência.
- **Dividir PDF**: um PDF por página, ou exporte cada página como PNG/JPG.
- **ZIP e extração**: junte muitos ficheiros e pastas num ZIP; .zip, .rar, .7z e .tar.gz extraem-se com um só gesto.
- **Leve e rápido**: a roda é uma janela pequena junto ao cursor, sem efeitos de ecrã inteiro, por isso abre de imediato
  mesmo em ecrãs 4K. Escolha o tamanho **S / M / L** no menu da área de notificação.
- **Mudou de ideias?** Volte ao centro da roda e largue: nada é convertido.
- **100% offline**: FFmpeg, ImageMagick, Ghostscript, LibreOffice, Pandoc e 7-Zip vêm incluídos. Os seus ficheiros nunca saem do seu PC.

## Instalação

1. Transfira **`Rotiva-Setup-x.y.z.exe`** a partir de [Releases](https://github.com/uncrownedk1ng2812/rotiva/releases/latest).
2. Execute o instalador. Se o SmartScreen aparecer, clique em **Mais informações → Executar mesmo assim**
   (o instalador ainda não está assinado com um certificado de assinatura de código pago).
3. Procure o ícone do Rotiva na área de notificação e arraste um ficheiro + <kbd>Shift</kbd>.

Instala-se por utilizador em `%LOCALAPPDATA%\Programs\Rotiva`: não precisa de direitos de administrador nem de instalar o .NET.
Para desinstalar: *Definições → Aplicações → Aplicações instaladas → Rotiva*.

Clique no ícone da área de notificação para abrir o menu do Rotiva: iniciar com o Windows, menu de contexto do Explorador,
tamanho da roda e ligações para o site, o GitHub e o apoio.

### macOS e Linux

Transfira o **`.dmg` para macOS** (Apple Silicon ou Intel) ou o **`AppImage` para Linux** a partir de
[Releases](https://github.com/uncrownedk1ng2812/rotiva/releases/latest).
A mesma roda e as mesmas conversões: clique com o botão direito nos ficheiros → **Abrir com → Rotiva**, ou arraste
ficheiros e prima <kbd>Shift</kbd>. Os motores de conversão vêm do seu sistema (`brew install …` /
`apt install …`); as notas da versão indicam os comandos. A aplicação para macOS ainda não está notarizada, por isso
autorize-a uma vez em *Definições do Sistema → Privacidade e segurança*.

## Open source

O Rotiva é open source sob a **GPL-3.0**. A aplicação está em [`src/Rotiva`](../../src/Rotiva) (C# / .NET 8 / WPF) e
o instalador em [`installer/`](../../installer). Consulte [BUILDING.md](../../BUILDING.md) para a executar a partir do código-fonte
ou criar o instalador.

Relatórios de erros, ideias e pull requests são bem-vindos em [Issues](https://github.com/uncrownedk1ng2812/rotiva/issues);
consulte [CONTRIBUTING.md](../../CONTRIBUTING.md) e o [Código de Conduta](../../CODE_OF_CONDUCT.md). Problemas de segurança:
comunique-os em privado, como explicado em [SECURITY.md](../../SECURITY.md).

## Política de assinatura de código

Os instaladores para Windows são criados a partir deste repositório pelo
[workflow do instalador do Windows](../../.github/workflows/windows-installer.yml) e assinados aí; nada criado
noutro local é assinado. Assinatura de código gratuita fornecida por [SignPath.io](https://about.signpath.io),
certificado da [SignPath Foundation](https://signpath.org). Detalhes completos no
[README em inglês](../../README.md#code-signing-policy).

**Privacidade:** o Rotiva não envia qualquer informação para outros sistemas na rede. As conversões são feitas no
seu computador; as ligações do menu (site, GitHub, apoio) só abrem o navegador quando clica nelas.

## Motores incluídos

O Rotiva faz as conversões com estes excelentes programas open source, incluídos no instalador sob
as suas próprias licenças:

| Motor | Licença | Usado para |
|---|---|---|
| [FFmpeg](https://ffmpeg.org) | GPL-3.0 | Vídeo e áudio |
| [ImageMagick](https://imagemagick.org) | ImageMagick License | Imagens |
| [Ghostscript](https://ghostscript.com) | AGPL-3.0 | Comprimir, juntar e dividir PDF |
| [LibreOffice](https://www.libreoffice.org) | MPL-2.0 | Documentos, folhas de cálculo e apresentações |
| [Pandoc](https://pandoc.org) | GPL-2.0-or-later | Markdown, HTML e RTF |
| [7-Zip](https://www.7-zip.org) | LGPL-2.1-or-later | ZIP e extração |

Versões, ficheiros de licença e ligações para o código-fonte: [THIRD-PARTY-NOTICES.md](../../THIRD-PARTY-NOTICES.md).

## Apoie o Rotiva

O Rotiva é gratuito e sem publicidade. Se lhe poupa tempo, pode
[oferecer um café ao programador](https://ko-fi.com/denishchairany) ☕ no Ko-fi.

## Copyright e licença

Copyright © 2026 **Denish Chairany**. Licenciado sob a
[GNU General Public License v3.0](../../LICENSE).

- ✅ Use o Rotiva gratuitamente, para fins pessoais ou comerciais.
- ✅ Estude, modifique e partilhe o código.
- 🔁 Uma versão modificada que distribua tem de ser também **GPL-3.0 com todo o código-fonte** e manter
  o aviso de copyright.
- 🏷️ **O nome, o logótipo e o ícone do Rotiva não fazem parte da licença.** Um fork precisa de nome e logótipo
  próprios e tem de indicar: `Based on Rotiva by Denish Chairany — https://github.com/uncrownedk1ng2812/rotiva`.
  Detalhes em [TRADEMARKS.md](../../TRADEMARKS.md).

Os motores incluídos são programas independentes com as suas próprias licenças, indicadas em
[THIRD-PARTY-NOTICES.md](../../THIRD-PARTY-NOTICES.md).
