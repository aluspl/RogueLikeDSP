#!/usr/bin/env bash
# Wydanie Androida gry Godot: podpisany AAB (Google Play) w GODOT/build/android/,
# z --upload od razu wysyłany na ścieżkę testów wewnętrznych (play_upload.py).
#
#   GODOT/tools/android_release.sh            AAB release
#   GODOT/tools/android_release.sh --upload   AAB release + wysyłka do Google Play (internal)
#
# Podpis jak w planbudowlany-mobile (Organizacja/Mobile/DEPLOY.md): zmienne ANDROID_KEYSTORE_PATH, ANDROID_KEY_ALIAS,
# ANDROID_KEYSTORE_PASSWORD (i PLAY_SERVICE_ACCOUNT_JSON do wysyłki) – z otoczenia albo z GODOT/.env.local (poza gitem).
# versionCode = RRDDDGGMM (rok, dzień roku, godzina, minuta) – rośnie z każdym buildem.
set -euo pipefail

HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
GODOT_DIR="${GODOT_DIR:-$(cd "$HERE/.." && pwd)}"
REPO="$(cd "$GODOT_DIR/.." && pwd)"
PROJECT="$GODOT_DIR/godot"
OUT="$GODOT_DIR/build/android"
GODOT_BIN="${GODOT_BIN:-godot-mono}"
if [ -f "$HERE/../.env.local" ]; then set -a; . "$HERE/../.env.local"; set +a; fi
export JAVA_HOME="${JAVA_HOME:-/opt/homebrew/opt/openjdk@17/libexec/openjdk.jdk/Contents/Home}"
export ANDROID_HOME="${ANDROID_HOME:-$HOME/Library/Android/sdk}"

UPLOAD=0
for a in "$@"; do
    case "$a" in
        --upload) UPLOAD=1 ;;
        *) echo "Nieznana opcja: $a" >&2; exit 2 ;;
    esac
done
step() { printf '\n==> %s\n' "$*"; }

: "${ANDROID_KEYSTORE_PATH:?ustaw ANDROID_KEYSTORE_PATH (np. w GODOT/.env.local)}"
: "${ANDROID_KEY_ALIAS:?ustaw ANDROID_KEY_ALIAS}"
: "${ANDROID_KEYSTORE_PASSWORD:?ustaw ANDROID_KEYSTORE_PASSWORD}"

VERSION="$(python3 -c 'import json,sys; print(json.load(open(sys.argv[1]))["version"].lstrip("v"))' "$REPO/GBA/data/game.json")"
CODE="${VERSION_CODE:-$(date +%y%j%H%M | sed 's/^0*//')}"
step "Wersja $VERSION, versionCode $CODE"
python3 - "$PROJECT/export_presets.cfg" "$VERSION" "$CODE" <<'PY'
import re, sys
path, ver, code = sys.argv[1], sys.argv[2], sys.argv[3]
s = open(path).read()
s = re.sub(r'version/name="[^"]*"', f'version/name="{ver}"', s)
s = re.sub(r'version/code=\d+', f'version/code={code}', s)
s = re.sub(r'application/short_version="[^"]*"', f'application/short_version="{ver}"', s)
open(path, "w").write(s)
PY

step "dotnet build"
dotnet build "$PROJECT/LifeLike.Game.csproj" -nologo -v q
[ -d "$PROJECT/.godot/imported" ] || "$GODOT_BIN" --headless --path "$PROJECT" --import

step "Eksport Android (AAB release)"
mkdir -p "$OUT"
AAB="$OUT/PBRogue-$VERSION-$CODE.aab"
TEMPLATE=()
[ -d "$PROJECT/android/build" ] || TEMPLATE=(--install-android-build-template)
GODOT_ANDROID_KEYSTORE_RELEASE_PATH="$ANDROID_KEYSTORE_PATH" GODOT_ANDROID_KEYSTORE_RELEASE_USER="$ANDROID_KEY_ALIAS" \
GODOT_ANDROID_KEYSTORE_RELEASE_PASSWORD="$ANDROID_KEYSTORE_PASSWORD" \
    "$GODOT_BIN" --headless --path "$PROJECT" "${TEMPLATE[@]}" --export-release Android "$AAB"
[ -f "$AAB" ] || { echo "Eksport nie utworzył $AAB" >&2; exit 1; }
step "AAB: $AAB"

if [ "$UPLOAD" = 1 ]; then
    step "Google Play – ścieżka internal"
    python3 "$HERE/play_upload.py" "$AAB" --changelog "$REPO/GBA/CHANGELOG.md" --version "v$VERSION"
fi
step "Gotowe"
