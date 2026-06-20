#!/usr/bin/env bash
# Creates a macOS .app bundle from the published TimeTracker binary.
# Run from the project root: bash bundle-macos.sh
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PUBLISH="$SCRIPT_DIR/bin/Release/net10.0/osx-arm64/publish"
APP_NAME="TimeTracker"
BUNDLE="$HOME/Desktop/$APP_NAME.app"
ICON_SRC="$SCRIPT_DIR/Assets/app-icon.png"

echo "▶  Publishing..."
# Clean first so the build always copies native libraries fresh
rm -rf "$PUBLISH"
dotnet publish "$SCRIPT_DIR" -r osx-arm64 --self-contained \
    -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=false \
    -c Release

# --- Build .app folder structure ---
echo "▶  Building .app bundle..."
rm -rf "$BUNDLE"
mkdir -p "$BUNDLE/Contents/MacOS"
mkdir -p "$BUNDLE/Contents/Resources"

# Copy the binary and all native libraries
cp "$PUBLISH/$APP_NAME"     "$BUNDLE/Contents/MacOS/"
shopt -s nullglob
dylibs=("$PUBLISH"/*.dylib)
if (( ${#dylibs[@]} > 0 )); then
    cp "${dylibs[@]}" "$BUNDLE/Contents/MacOS/"
fi

# --- Convert PNG → .icns using built-in macOS tools ---
echo "▶  Converting icon..."
ICONSET="$(mktemp -d)/AppIcon.iconset"
mkdir -p "$ICONSET"

# sips resizes the source PNG to each required size
sips -z 16   16   "$ICON_SRC" --out "$ICONSET/icon_16x16.png"    > /dev/null
sips -z 32   32   "$ICON_SRC" --out "$ICONSET/icon_16x16@2x.png" > /dev/null
sips -z 32   32   "$ICON_SRC" --out "$ICONSET/icon_32x32.png"    > /dev/null
sips -z 64   64   "$ICON_SRC" --out "$ICONSET/icon_32x32@2x.png" > /dev/null
sips -z 128  128  "$ICON_SRC" --out "$ICONSET/icon_128x128.png"  > /dev/null
sips -z 256  256  "$ICON_SRC" --out "$ICONSET/icon_128x128@2x.png" > /dev/null
sips -z 256  256  "$ICON_SRC" --out "$ICONSET/icon_256x256.png"  > /dev/null
sips -z 512  512  "$ICON_SRC" --out "$ICONSET/icon_256x256@2x.png" > /dev/null
sips -z 512  512  "$ICON_SRC" --out "$ICONSET/icon_512x512.png"  > /dev/null
sips -z 1024 1024 "$ICON_SRC" --out "$ICONSET/icon_512x512@2x.png" > /dev/null

iconutil -c icns "$ICONSET" -o "$BUNDLE/Contents/Resources/AppIcon.icns"

# --- Write Info.plist (tells macOS the app name, icon, and identifier) ---
cat > "$BUNDLE/Contents/Info.plist" << 'PLIST'
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN"
    "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleName</key>             <string>TimeTracker</string>
    <key>CFBundleDisplayName</key>      <string>TimeTracker</string>
    <key>CFBundleIdentifier</key>       <string>com.slok.timetracker</string>
    <key>CFBundleVersion</key>          <string>1.0.0</string>
    <key>CFBundleShortVersionString</key><string>1.0</string>
    <key>CFBundlePackageType</key>      <string>APPL</string>
    <key>CFBundleExecutable</key>       <string>TimeTracker</string>
    <key>CFBundleIconFile</key>         <string>AppIcon</string>
    <key>NSHighResolutionCapable</key>  <true/>
    <key>NSPrincipalClass</key>         <string>NSApplication</string>
</dict>
</plist>
PLIST

# Clear the quarantine flag macOS sets on files built locally
xattr -cr "$BUNDLE" 2>/dev/null || true

echo ""
echo "✅  Done! TimeTracker.app is on your Desktop."
echo "    Drag it to /Applications to install."
