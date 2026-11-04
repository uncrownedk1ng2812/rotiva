# Security policy

## Supported versions

Security fixes go into the latest release only. Please update to the
[latest version](https://github.com/uncrownedk1ng2812/rotiva/releases/latest) before reporting.

## Reporting a vulnerability

Please **don't open a public issue** for a security problem. Report it privately through GitHub:
**[Report a vulnerability](https://github.com/uncrownedk1ng2812/rotiva/security/advisories/new)**
(only the maintainer can see it).

Include what you can:

- the Rotiva version and your OS
- what an attacker could do, and what they need (for example a crafted file the user converts)
- steps or a sample file to reproduce it

You can expect a first reply within 7 days. Once the problem is confirmed, a fixed release is prepared
and the report is published as a GitHub security advisory, crediting you unless you prefer otherwise.

## Scope

In scope:

- the Rotiva apps for Windows, macOS and Linux, and their installers and packages
- the Explorer / file manager integration Rotiva installs
- the build and release workflows in this repository

Problems inside a bundled engine (FFmpeg, ImageMagick, Ghostscript, LibreOffice, Pandoc, 7-Zip,
Tesseract) should also be reported to that project. If Rotiva ships a vulnerable version, tell us too,
so the next installer bundles a fixed one.

## How releases are built

Windows installers are built from this repository with `installer/build.ps1`. The public
[Windows installer workflow](.github/workflows/windows-installer.yml) builds the same installer on GitHub
Actions, with engine downloads pinned and checked against their SHA-256. Each release lists SHA-256
checksums in `SHA256SUMS.txt`. Installers are not code-signed yet; see [Code signing](README.md#code-signing).
