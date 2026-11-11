#!/usr/bin/env bash
# Membuat dist/Rotiva-<versi>-macos-<arm64|x64>.dmg berisi Rotiva.app (self-contained, .NET ikut).
#
# Kebutuhan: macOS, .NET 8 SDK, librsvg (brew install librsvg) untuk ikon. Engine konversi tidak
# dibundel: brew install ffmpeg imagemagick ghostscript pandoc sevenzip tesseract (lihat BUILDING.md).
#
# App ditandatangani ad-hoc (tanpa Apple Developer ID, tanpa notarization): saat pertama dibuka
# macOS meminta konfirmasi (System Settings → Privacy & Security → Open Anyway).
#
# Pemakaian:  installer/build-macos.sh <versi> <arm64|x64>     mis. installer/build-macos.sh 1.10.0 arm64
set -euo pipefail

version="${1:?versi}"
arch="${2:?arm64 atau x64}"
root="$(cd "$(dirname "$0")/.." && pwd)"
work="$root/dist/macos-$arch"
app="$work/Rotiva.app"
output="$root/dist/Rotiva-$version-macos-$arch.dmg"

echo "==> Publish Rotiva.Desktop $version (osx-$arch, self-contained)"
rm -rf "$work"
mkdir -p "$app/Contents/MacOS" "$app/Contents/Resources"
dotnet publish "$root/src/Rotiva.Desktop" --configuration Release --runtime "osx-$arch" --self-contained true \
  -p:DebugType=none -p:Version="$version" -p:UseAppHost=true -o "$app/Contents/MacOS"
cp "$root/LICENSE" "$root/NOTICE" "$root/TRADEMARKS.md" "$root/THIRD-PARTY-NOTICES.md" "$app/Contents/Resources/"

echo "==> Ikon"
iconset="$work/Rotiva.iconset"
mkdir -p "$iconset"
for size in 16 32 128 256 512; do
  rsvg-convert -w "$size" -h "$size" "$root/src/Rotiva/Assets/app.svg" -o "$iconset/icon_${size}x${size}.png"
  rsvg-convert -w $((size * 2)) -h $((size * 2)) "$root/src/Rotiva/Assets/app.svg" -o "$iconset/icon_${size}x${size}@2x.png"
done
iconutil -c icns "$iconset" -o "$app/Contents/Resources/Rotiva.icns"

echo "==> Info.plist"
# LSUIElement: hidup di menu bar saja, tanpa ikon di Dock (seperti tray di Windows).
# CFBundleDocumentTypes: Rotiva muncul di Finder "Open With" untuk semua file dan folder, sebagai
# pilihan tambahan (Alternate), tidak pernah jadi aplikasi default.
cat > "$app/Contents/Info.plist" <<EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>CFBundleName</key><string>Rotiva</string>
  <key>CFBundleDisplayName</key><string>Rotiva</string>
  <key>CFBundleIdentifier</key><string>com.denishchairany.rotiva</string>
  <key>CFBundleExecutable</key><string>Rotiva</string>
  <key>CFBundleIconFile</key><string>Rotiva</string>
  <key>CFBundlePackageType</key><string>APPL</string>
  <key>CFBundleShortVersionString</key><string>$version</string>
  <key>CFBundleVersion</key><string>$version</string>
  <key>LSMinimumSystemVersion</key><string>12.0</string>
  <key>LSApplicationCategoryType</key><string>public.app-category.utilities</string>
  <key>LSUIElement</key><true/>
  <key>NSHighResolutionCapable</key><true/>
  <key>NSHumanReadableCopyright</key><string>Copyright (c) 2026 Denish Chairany. Licensed under GPL-3.0.</string>
  <key>CFBundleDocumentTypes</key>
  <array>
    <dict>
      <key>CFBundleTypeName</key><string>File</string>
      <key>CFBundleTypeRole</key><string>Viewer</string>
      <key>LSHandlerRank</key><string>Alternate</string>
      <key>LSItemContentTypes</key>
      <array>
        <string>public.data</string>
        <string>public.folder</string>
      </array>
    </dict>
  </array>
</dict>
</plist>
EOF
plutil -lint "$app/Contents/Info.plist"

echo "==> Tanda tangan ad-hoc"
# Apple Silicon menolak menjalankan program tanpa tanda tangan sama sekali.
codesign --force --deep --sign - "$app"
codesign --verify --deep "$app"

echo "==> DMG"
staging="$work/dmg"
mkdir -p "$staging"
cp -R "$app" "$staging/"
ln -s /Applications "$staging/Applications"
rm -f "$output"
# hdiutil di runner GitHub kadang gagal "Resource busy"; coba ulang beberapa kali.
for attempt in 1 2 3 4 5; do
  if hdiutil create -volname "Rotiva" -srcfolder "$staging" -ov -format UDZO "$output" >/dev/null; then
    break
  fi
  if [ "$attempt" -eq 5 ]; then
    echo "hdiutil gagal setelah $attempt percobaan" >&2
    exit 1
  fi
  echo "==> hdiutil gagal (percobaan $attempt), coba lagi..."
  sleep $((attempt * 5))
done
echo "==> Selesai: $output ($(du -h "$output" | cut -f1))"
