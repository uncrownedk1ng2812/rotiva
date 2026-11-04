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
  <b>Français</b> ·
  <a href="../../docs/readme/README.th.md">ไทย</a> ·
  <a href="../../docs/readme/README.tr.md">Türkçe</a> ·
  <a href="../../docs/readme/README.id.md">Bahasa Indonesia</a> ·
  <a href="../../docs/readme/README.it.md">Italiano</a>
</p>
<!-- /languages -->

<p align="center">
  <img src="../../assets/og.png" alt="Rotiva — faites glisser un fichier, appuyez sur Maj, déposez-le sur la roue" width="720">
</p>

<p align="center">
  <b>Convertissez vos fichiers sans ouvrir aucune application.</b><br>
  Faites glisser un fichier, appuyez sur <kbd>Maj</kbd>, déposez-le sur la roue. C'est tout.
</p>

<p align="center">
  <a href="https://github.com/uncrownedk1ng2812/rotiva/releases/latest"><img src="https://img.shields.io/github/v/release/uncrownedk1ng2812/rotiva?label=version&color=ff8a1f" alt="Dernière version"></a>
  <img src="https://img.shields.io/badge/Windows-10%20%7C%2011-0078d4" alt="Windows 10 | 11">
  <img src="https://img.shields.io/badge/macOS-Apple%20Silicon%20%7C%20Intel-555" alt="macOS : Apple Silicon | Intel">
  <img src="https://img.shields.io/badge/Linux-AppImage-e95420" alt="Linux AppImage">
  <img src="https://img.shields.io/badge/prix-gratuit-1f9d55" alt="Gratuit">
  <img src="https://img.shields.io/badge/100%25-hors%20ligne-5a5a66" alt="100 % hors ligne">
  <a href="../../LICENSE"><img src="https://img.shields.io/badge/license-GPL--3.0-blue" alt="Licence : GPL-3.0"></a>
</p>

<p align="center">
  <a href="https://uncrownedk1ng2812.github.io/rotiva/"><b>🌐 Site web et démo</b></a>
  &nbsp;·&nbsp;
  <a href="https://github.com/uncrownedk1ng2812/rotiva/releases/latest"><b>⬇️ Télécharger l'installateur</b></a>
  &nbsp;·&nbsp;
  <a href="https://ko-fi.com/denishchairany"><b>☕ Soutenir sur Ko-fi</b></a>
</p>

> Ceci est une traduction du [README en anglais](../../README.md). En cas de différence, la version anglaise fait foi.

---

> [!NOTE]
> **Rotiva a été créé de zéro par Denish Chairany** et est open source sous licence [GPL-3.0](../../LICENSE).
> Les forks sont les bienvenus, mais ils doivent rester open source, créditer l'auteur et utiliser **un autre nom
> et un autre logo**. Voir [TRADEMARKS.md](../../TRADEMARKS.md).

## Qu'est-ce que Rotiva ?

Rotiva est une petite application qui se loge dans la zone de notification. Pendant que vous faites glisser une photo,
une vidéo, un document ou n'importe quel autre fichier, appuyez sur **Maj** : une roue de formats apparaît juste sous
le curseur. Visez le format voulu et relâchez : le fichier converti est enregistré à côté de l'original.

Pas d'envoi vers un convertisseur en ligne, pas de publicité, pas de file d'attente.

<p align="center">
  <img src="../../assets/readme/demo.gif" alt="Démo : faire glisser un fichier, appuyer sur Maj, choisir un format, déposer" width="320">
  &nbsp;&nbsp;
  <img src="../../assets/readme/wheel.png" alt="La roue de formats pour une image" width="420">
</p>

## Comment ça marche

| | Étape | Détails |
|---|---|---|
| **1** | **Faites glisser un fichier** | Depuis l'Explorateur, le bureau ou n'importe quel dossier. Un seul fichier ou plusieurs à la fois. |
| **2** | **Appuyez sur <kbd>Maj</kbd>** | Sans relâcher le bouton gauche de la souris. La roue apparaît juste sous le curseur. |
| **3** | **Visez et déposez** | C'est la direction du curseur qui choisit le format : inutile de tomber pile sur un segment. Déposez au centre pour annuler. |

Vous préférez le clic droit ? Tout ce qui est sur la roue se trouve aussi dans le menu contextuel de l'Explorateur, sous
**Rotiva ▸** (Windows 11 : *Afficher d'autres options*).

## Formats pris en charge

La roue s'adapte au fichier que vous faites glisser. Le format du fichier lui-même est masqué automatiquement.

| Type | Depuis | Vers |
|---|---|---|
| 🖼️ Images | PNG, JPG, WEBP, AVIF, HEIC, BMP, TIFF, GIF, ICO | PNG, JPG, WEBP, AVIF, PDF, ICO, TIFF, **Compresser**, **Texte (OCR)** |
| 🎬 Vidéo | MP4, MKV, MOV, AVI, WEBM, WMV, FLV, 3GP | MP4, WEBM, MOV, MKV, AVI, GIF, MP3, WAV, **Compresser** |
| 🎵 Audio | MP3, WAV, FLAC, OGG, OPUS, M4A, AAC, WMA | MP3, WAV, FLAC, OGG, OPUS, M4A |
| 📄 Documents | DOCX, DOC, ODT, RTF, TXT, MD, HTML | PDF, DOCX, ODT, TXT, HTML, MD, RTF |
| 📊 Tableurs | XLSX, XLS, CSV, TSV, ODS | XLSX, CSV, ODS, PDF, XLS, HTML |
| 📽️ Présentations | PPTX, PPT, ODP, PPSX, PPS | PDF, PPTX, ODP, PNG, JPG |
| 📕 PDF | PDF | DOCX, ODT, PNG, JPG, **Scinder**, **Compresser**, **Texte (OCR)** |
| 🗜️ Archives | ZIP, RAR, 7Z, TAR.GZ, … | **Extraire**, ZIP |

## Fonctionnalités

<p align="center">
  <img src="../../assets/readme/features.gif" alt="MP4 en MP3, Word en PDF, plusieurs fichiers dans un ZIP, RAR extrait instantanément" width="320">
</p>

- **Compresser** : réduisez photos, vidéos (MP4 jusqu'à 1080p) et PDF scannés sans changer de format.
- **Fusionner en 1 PDF** : faites glisser plusieurs images, documents, tableurs ou PDF à la fois.
- **Récupérer le texte (OCR)** : lisez le texte d'une image ou d'un PDF scanné avec l'OCR intégré de Windows, directement dans le presse-papiers.
- **Scinder un PDF** : un PDF par page, ou exportez chaque page en PNG/JPG.
- **ZIP et extraction** : regroupez fichiers et dossiers dans un seul ZIP ; .zip, .rar, .7z et .tar.gz s'extraient d'un seul geste.
- **Léger et rapide** : la roue est une petite fenêtre sous le curseur, sans effet plein écran, elle s'ouvre donc
  instantanément même sur un écran 4K. Choisissez la taille **S / M / L** dans le menu de la zone de notification.
- **Vous changez d'avis ?** Revenez au centre de la roue et relâchez : rien n'est converti.
- **100 % hors ligne** : FFmpeg, ImageMagick, Ghostscript, LibreOffice, Pandoc et 7-Zip sont inclus. Vos fichiers ne quittent jamais votre PC.

## Installation

1. Téléchargez **`Rotiva-Setup-x.y.z.exe`** depuis les [Releases](https://github.com/uncrownedk1ng2812/rotiva/releases/latest).
2. Lancez l'installateur. Si SmartScreen s'affiche, cliquez sur **Informations complémentaires → Exécuter quand même**
   (l'installateur n'est pas encore signé avec un certificat de signature de code payant).
3. Repérez l'icône de Rotiva dans la zone de notification, puis faites glisser un fichier + <kbd>Maj</kbd>.

S'installe par utilisateur dans `%LOCALAPPDATA%\Programs\Rotiva` : pas besoin de droits administrateur ni d'installer .NET.
Désinstallation : *Paramètres → Applications → Applications installées → Rotiva*.

Cliquez sur l'icône de la zone de notification pour ouvrir le menu de Rotiva : lancement au démarrage de Windows,
menu contextuel de l'Explorateur, taille de la roue, et liens vers le site, GitHub et le soutien.

### macOS et Linux

Téléchargez le **`.dmg` pour macOS** (Apple Silicon ou Intel) ou l'**`AppImage` pour Linux** depuis les
[Releases](https://github.com/uncrownedk1ng2812/rotiva/releases/latest).
Même roue, mêmes conversions : clic droit sur des fichiers → **Ouvrir avec → Rotiva**, ou faites glisser des fichiers
et appuyez sur <kbd>Maj</kbd>. Les moteurs de conversion viennent de votre système (`brew install …` /
`apt install …`) ; les notes de version donnent les commandes. L'application macOS n'est pas encore notarisée :
autorisez-la une fois dans *Réglages Système → Confidentialité et sécurité*.

## Open source

Rotiva est open source sous licence **GPL-3.0**. L'application se trouve dans [`src/Rotiva`](../../src/Rotiva)
(C# / .NET 8 / WPF) et l'installateur dans [`installer/`](../../installer). Voir [BUILDING.md](../../BUILDING.md) pour
l'exécuter depuis le code source ou construire l'installateur.

Rapports de bugs, idées et pull requests sont les bienvenus dans les [Issues](https://github.com/uncrownedk1ng2812/rotiva/issues) ;
voir [CONTRIBUTING.md](../../CONTRIBUTING.md) et le [Code de conduite](../../CODE_OF_CONDUCT.md). Problèmes de sécurité :
signalez-les en privé comme indiqué dans [SECURITY.md](../../SECURITY.md).

## Politique de signature de code

Les installateurs Windows sont construits à partir de ce dépôt par le
[workflow de l'installateur Windows](../../.github/workflows/windows-installer.yml) et signés à cet endroit ; rien de
construit ailleurs n'est signé. Signature de code gratuite fournie par [SignPath.io](https://about.signpath.io),
certificat de la [SignPath Foundation](https://signpath.org). Tous les détails dans le
[README en anglais](../../README.md#code-signing-policy).

**Confidentialité :** Rotiva n'envoie aucune information à d'autres systèmes du réseau. Les conversions ont lieu sur
votre ordinateur ; les liens du menu (site, GitHub, soutien) n'ouvrent le navigateur que lorsque vous cliquez dessus.

## Moteurs inclus

Rotiva effectue les conversions avec ces excellents logiciels open source, inclus dans l'installateur sous
leurs propres licences :

| Moteur | Licence | Utilisé pour |
|---|---|---|
| [FFmpeg](https://ffmpeg.org) | GPL-3.0 | Vidéo et audio |
| [ImageMagick](https://imagemagick.org) | ImageMagick License | Images |
| [Ghostscript](https://ghostscript.com) | AGPL-3.0 | Compresser, fusionner et scinder des PDF |
| [LibreOffice](https://www.libreoffice.org) | MPL-2.0 | Documents, tableurs et présentations |
| [Pandoc](https://pandoc.org) | GPL-2.0-or-later | Markdown, HTML et RTF |
| [7-Zip](https://www.7-zip.org) | LGPL-2.1-or-later | ZIP et extraction |

Versions, fichiers de licence et liens vers le code source : [THIRD-PARTY-NOTICES.md](../../THIRD-PARTY-NOTICES.md).

## Soutenir Rotiva

Rotiva est gratuit et sans publicité. S'il vous fait gagner du temps, vous pouvez
[offrir un café au développeur](https://ko-fi.com/denishchairany) ☕ sur Ko-fi.

## Copyright et licence

Copyright © 2026 **Denish Chairany**. Distribué sous la
[GNU General Public License v3.0](../../LICENSE).

- ✅ Utilisez Rotiva gratuitement, pour un usage personnel ou commercial.
- ✅ Étudiez, modifiez et partagez le code.
- 🔁 Une version modifiée que vous distribuez doit elle aussi être sous **GPL-3.0 avec tout son code source**
  et conserver la mention de copyright.
- 🏷️ **Le nom, le logo et l'icône de Rotiva ne font pas partie de la licence.** Un fork doit avoir son propre nom
  et son propre logo, et doit indiquer : `Based on Rotiva by Denish Chairany — https://github.com/uncrownedk1ng2812/rotiva`.
  Détails dans [TRADEMARKS.md](../../TRADEMARKS.md).

Les moteurs inclus sont des programmes distincts sous leurs propres licences, listées dans
[THIRD-PARTY-NOTICES.md](../../THIRD-PARTY-NOTICES.md).
