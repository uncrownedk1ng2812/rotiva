#!/usr/bin/env bash
# Membuat dist/Rotiva-<versi>-x86_64.AppImage: Rotiva.Desktop (self-contained, .NET ikut) dalam satu file.
#
# Kebutuhan: Linux x86_64, .NET 8 SDK, curl. Engine konversi (FFmpeg, ImageMagick, Ghostscript, Pandoc,
# 7-Zip, LibreOffice) TIDAK dibundel: dipakai dari paket sistem, lihat BUILDING.md.
#
# Pemakaian:  installer/build-appimage.sh [versi]      mis. installer/build-appimage.sh 1.9.0
set -euo pipefail

version="${1:-0.0.0}"
root="$(cd "$(dirname "$0")/.." && pwd)"
dist="$root/dist"
appdir="$dist/appimage/Rotiva.AppDir"
output="$dist/Rotiva-$version-x86_64.AppImage"

# appimagetool versi tetap, dicek SHA-256-nya (sama dengan digest rilis di GitHub).
tool_version="1.9.1"
tool_sha256="ed4ce84f0d9caff66f50bcca6ff6f35aae54ce8135408b3fa33abfc3cb384eb0"
tool="$dist/cache/appimagetool-$tool_version-x86_64.AppImage"

echo "==> Publish Rotiva.Desktop $version (linux-x64, self-contained)"
rm -rf "$dist/appimage"
dotnet publish "$root/src/Rotiva.Desktop" --configuration Release --runtime linux-x64 --self-contained true \
  -p:DebugType=none -p:Version="$version" -o "$appdir/usr/bin"

echo "==> AppDir"
cp "$root/src/Rotiva.Desktop/Assets/icon.png" "$appdir/rotiva.png"
install -Dm644 "$root/src/Rotiva.Desktop/Assets/icon.png" "$appdir/usr/share/icons/hicolor/256x256/apps/rotiva.png"
cp "$root/LICENSE" "$root/NOTICE" "$root/TRADEMARKS.md" "$root/THIRD-PARTY-NOTICES.md" "$appdir/usr/bin/"
cat > "$appdir/rotiva.desktop" <<'EOF'
[Desktop Entry]
Type=Application
Name=Rotiva
GenericName=File Converter
Comment=Convert files from a wheel
Exec=Rotiva %F
Icon=rotiva
Terminal=false
Categories=Utility;
EOF
cat > "$appdir/AppRun" <<'EOF'
#!/bin/sh
# Titik masuk AppImage: jalankan Rotiva dari dalam AppImage (APPIMAGE berisi path file .AppImage).
here="$(dirname "$(readlink -f "$0")")"
exec "$here/usr/bin/Rotiva" "$@"
EOF
chmod +x "$appdir/AppRun" "$appdir/usr/bin/Rotiva"

if [ ! -f "$tool" ]; then
  echo "==> Unduh appimagetool $tool_version"
  mkdir -p "$dist/cache"
  curl -fsSL -o "$tool.part" "https://github.com/AppImage/appimagetool/releases/download/$tool_version/appimagetool-x86_64.AppImage"
  echo "$tool_sha256  $tool.part" | sha256sum --check --quiet
  mv "$tool.part" "$tool"
  chmod +x "$tool"
fi

echo "==> Buat AppImage"
# Tanpa FUSE (CI, container, WSL): appimagetool membongkar dirinya sendiri lalu jalan.
ARCH=x86_64 APPIMAGE_EXTRACT_AND_RUN=1 "$tool" --no-appstream "$appdir" "$output"
echo "==> Selesai: $output ($(du -h "$output" | cut -f1))"
