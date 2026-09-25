#!/usr/bin/env bash
# Build gry Godot na iOS i instalacja na podłączonym iPhonie (USB albo Wi-Fi, sparowany w Xcode).
#
#   GODOT/tools/ios_deploy.sh              eksport + xcodebuild + instalacja + uruchomienie
#   GODOT/tools/ios_deploy.sh --export     tylko eksport projektu Xcode (GODOT/build/ios)
#   GODOT/tools/ios_deploy.sh --no-launch  bez uruchamiania po instalacji
#
# Zmienne (opcjonalne):
#   UDID     identyfikator urządzenia (xcrun devicectl list devices), domyślnie iPhone (Szymon)
#   ASC_KEY  klucz App Store Connect API (.p8) do automatycznego podpisu; zawartości klucza skrypt nie czyta
#   ASC_KEY_ID, ASC_ISSUER_ID, TEAM_ID, BUNDLE_ID
#
# Wymaga: Godot 4.7.2 .NET (godot-mono) z szablonami eksportu 4.7.2.stable.mono, .NET SDK, Xcode.
# Wyniki (projekt Xcode, DerivedData, .app) w GODOT/build/ios - poza gitem.
set -euo pipefail

HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
GODOT_DIR="$(cd "$HERE/.." && pwd)"
REPO="$(cd "$GODOT_DIR/.." && pwd)"
PROJECT="$GODOT_DIR/godot"
OUT="$GODOT_DIR/build/ios"

UDID="${UDID:-00008120-00090CA60280201E}"
ASC_KEY="${ASC_KEY:-$HOME/Dev/PlanerBudowlany/Organizacja/Mobile/certs/AuthKey_GWD3KYV9YM.p8}"
ASC_KEY_ID="${ASC_KEY_ID:-GWD3KYV9YM}"
ASC_ISSUER_ID="${ASC_ISSUER_ID:-fbb8976f-4c67-45fe-a2ed-f9edc603512e}"
TEAM_ID="${TEAM_ID:-4DKWK44657}"
BUNDLE_ID="${BUNDLE_ID:-online.planbudowlany.rogue}"
GODOT_BIN="${GODOT_BIN:-godot-mono}"

EXPORT_ONLY=0
LAUNCH=1
for a in "$@"; do
    case "$a" in
        --export) EXPORT_ONLY=1 ;;
        --no-launch) LAUNCH=0 ;;
        *) echo "Nieznana opcja: $a" >&2; exit 2 ;;
    esac
done

step() { printf '\n==> %s\n' "$*"; }

# 1. Wersja aplikacji z game.json (np. v0.21.45 -> 0.21.45) do presetu eksportu
VERSION="$(python3 -c 'import json,sys; print(json.load(open(sys.argv[1]))["version"].lstrip("v"))' "$REPO/GBA/data/game.json")"
step "Wersja $VERSION"
python3 - "$PROJECT/export_presets.cfg" "$VERSION" <<'PY'
import re, sys
path, ver = sys.argv[1], sys.argv[2]
s = open(path).read()
s2 = re.sub(r'application/short_version="[^"]*"', f'application/short_version="{ver}"', s)
s2 = re.sub(r'version/name="[^"]*"', f'version/name="{ver}"', s2)
if s2 != s:
    open(path, "w").write(s2)
PY

# 2. Kompilacja C# (kopiuje też GBA/data/game.json do godot/data) i import zasobów przy pierwszym uruchomieniu
step "dotnet build"
dotnet build "$PROJECT/LifeLike.Game.csproj" -nologo -v q
if [ ! -d "$PROJECT/.godot/imported" ]; then
    step "Import zasobów Godota"
    "$GODOT_BIN" --headless --path "$PROJECT" --import
fi

# 3. Eksport projektu Xcode (preset „iOS”, export_project_only)
step "Eksport iOS -> $OUT"
rm -rf "$OUT/PBRogue" "$OUT/PBRogue.xcodeproj" "$OUT/PBRogue.pck"
mkdir -p "$OUT"
"$GODOT_BIN" --headless --path "$PROJECT" --export-debug iOS "$OUT/PBRogue.ipa"
if [ ! -d "$OUT/PBRogue.xcodeproj" ]; then
    echo "Eksport nie utworzył $OUT/PBRogue.xcodeproj (sprawdź szablony eksportu 4.7.2.stable.mono," >&2
    echo "rendering/textures/vram_compression/import_etc2_astc=true i godot/LifeLike.Game.sln)." >&2
    exit 1
fi
[ "$EXPORT_ONLY" = 1 ] && { echo "Projekt Xcode: $OUT/PBRogue.xcodeproj"; exit 0; }

# 4. Build i podpis (automatyczny, klucz API App Store Connect)
if [ ! -f "$ASC_KEY" ]; then
    echo "Brak klucza App Store Connect (ASC_KEY): $ASC_KEY" >&2
    exit 1
fi
step "xcodebuild (urządzenie $UDID)"
cd "$OUT"
xcodebuild -project PBRogue.xcodeproj -scheme PBRogue -configuration Debug \
    -destination "id=$UDID" -derivedDataPath dd -allowProvisioningUpdates \
    -authenticationKeyPath "$ASC_KEY" -authenticationKeyID "$ASC_KEY_ID" -authenticationKeyIssuerID "$ASC_ISSUER_ID" \
    CODE_SIGN_STYLE=Automatic DEVELOPMENT_TEAM="$TEAM_ID" PRODUCT_BUNDLE_IDENTIFIER="$BUNDLE_ID" \
    build -quiet
APP="$(find "$OUT/dd/Build/Products" -maxdepth 2 -name 'PBRogue.app' -type d | head -n 1)"
if [ -z "$APP" ]; then
    echo "Nie znaleziono PBRogue.app w $OUT/dd/Build/Products" >&2
    exit 1
fi

# 5. Instalacja i uruchomienie
step "Instalacja $APP"
xcrun devicectl device install app --device "$UDID" "$APP"
if [ "$LAUNCH" = 1 ]; then
    step "Uruchomienie $BUNDLE_ID"
    xcrun devicectl device process launch --device "$UDID" "$BUNDLE_ID"
fi
step "Gotowe"
