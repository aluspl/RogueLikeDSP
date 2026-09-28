#!/usr/bin/env bash
# Wydanie gry Godot do TestFlight (App Store Connect, aplikacja „PB Rogue”, online.planbudowlany.rogue).
#
#   GODOT/tools/testflight_upload.sh             eksport release + archiwum + wysyłka do App Store Connect
#   GODOT/tools/testflight_upload.sh --no-upload tylko archiwum i .ipa w GODOT/build/testflight (bez wysyłki)
#
# Numer wersji (CFBundleShortVersionString) bierze z GBA/data/game.json, numer buildu (CFBundleVersion)
# to data i godzina (RRMMDDGGMM), więc każdy upload ma nowy numer.
#
# Podpis: lokalny certyfikat „Apple Distribution” + profil App Store z asc_profile.py.
# Zmienne (z otoczenia albo GODOT/.env.local): ASC_KEY (.p8 – zawartości skrypt nie czyta), ASC_KEY_ID, ASC_ISSUER_ID, TEAM_ID, BUNDLE_ID,
#   GODOT_DIR (inny katalog GODOT, np. rozpakowany commit), BUILD_NUMBER.
set -euo pipefail

HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
GODOT_DIR="${GODOT_DIR:-$(cd "$HERE/.." && pwd)}"
REPO="$(cd "$GODOT_DIR/.." && pwd)"
PROJECT="$GODOT_DIR/godot"
# Lokalne ścieżki i identyfikatory (klucze, zespół, urządzenie) – GODOT/.env.local, poza gitem (wzór: GODOT/.env.example)
if [ -f "$HERE/../.env.local" ]; then set -a; . "$HERE/../.env.local"; set +a; fi
OUT="$GODOT_DIR/build/testflight"

: "${ASC_KEY:?ustaw ASC_KEY (GODOT/.env.local)}"
: "${ASC_KEY_ID:?ustaw ASC_KEY_ID}"
: "${ASC_ISSUER_ID:?ustaw ASC_ISSUER_ID}"
: "${TEAM_ID:?ustaw TEAM_ID}"
BUNDLE_ID="${BUNDLE_ID:-online.planbudowlany.rogue}"
GODOT_BIN="${GODOT_BIN:-godot-mono}"
BUILD_NUMBER="${BUILD_NUMBER:-$(date +%y%m%d%H%M)}"

UPLOAD=1
for a in "$@"; do
    case "$a" in
        --no-upload) UPLOAD=0 ;;
        *) echo "Nieznana opcja: $a" >&2; exit 2 ;;
    esac
done

step() { printf '\n==> %s\n' "$*"; }

# Zespół Apple wstawiany do presetu tylko na czas eksportu (w repo pole zostaje puste)
PRESET="$PROJECT/export_presets.cfg"
set_team() { python3 - "$PRESET" "$1" <<'PY'
import re, sys
p, team = sys.argv[1], sys.argv[2]
s = open(p).read()
open(p, "w").write(re.sub(r'application/app_store_team_id="[^"]*"', f'application/app_store_team_id="{team}"', s))
PY
}
trap 'set_team ""' EXIT
set_team "$TEAM_ID"
[ -f "$ASC_KEY" ] || { echo "Brak klucza App Store Connect (ASC_KEY): $ASC_KEY" >&2; exit 1; }
AUTH=(-allowProvisioningUpdates -authenticationKeyPath "$ASC_KEY" -authenticationKeyID "$ASC_KEY_ID" -authenticationKeyIssuerID "$ASC_ISSUER_ID")

# 1. Wersja i numer buildu do presetu eksportu; deklaracja szyfrowania (tylko HTTPS systemu = zwolnione)
VERSION="$(python3 -c 'import json,sys; print(json.load(open(sys.argv[1]))["version"].lstrip("v"))' "$REPO/GBA/data/game.json")"
step "Wersja $VERSION, build $BUILD_NUMBER"
python3 - "$PROJECT/export_presets.cfg" "$VERSION" "$BUILD_NUMBER" <<'PY'
import re, sys
path, ver, build = sys.argv[1], sys.argv[2], sys.argv[3]
s = open(path).read()
s = re.sub(r'application/short_version="[^"]*"', f'application/short_version="{ver}"', s)
s = re.sub(r'application/version="[^"]*"', f'application/version="{build}"', s)
s = re.sub(r'version/name="[^"]*"', f'version/name="{ver}"', s)
plist = 'application/additional_plist_content="<key>ITSAppUsesNonExemptEncryption</key><false/>"'
if "application/additional_plist_content=" in s:
    s = re.sub(r'application/additional_plist_content="[^\n]*"', plist, s)
else:
    s = s.replace('application/export_project_only=true', 'application/export_project_only=true\n' + plist, 1)
# Godot dodaje klucze NS*UsageDescription; puste są odrzucane przy walidacji App Store – gra z nich nie korzysta
for key, text in (("camera", "Gra nie korzysta z aparatu."), ("microphone", "Gra nie korzysta z mikrofonu."),
                  ("photolibrary", "Gra nie korzysta z biblioteki zdjęć.")):
    line = f'privacy/{key}_usage_description="{text}"'
    if f"privacy/{key}_usage_description=" in s:
        s = re.sub(rf'privacy/{key}_usage_description="[^\n]*"', line, s)
    else:
        s = s.replace(plist, plist + "\n" + line, 1)
open(path, "w").write(s)
PY

# 2. Kompilacja C# i import zasobów
step "dotnet build"
dotnet build "$PROJECT/LifeLike.Game.csproj" -nologo -v q
[ -d "$PROJECT/.godot/imported" ] || "$GODOT_BIN" --headless --path "$PROJECT" --import

# 3. Eksport projektu Xcode (release)
step "Eksport iOS (release) -> $OUT"
rm -rf "$OUT"
mkdir -p "$OUT"
"$GODOT_BIN" --headless --path "$PROJECT" --export-release iOS "$OUT/PBRogue.ipa"
[ -d "$OUT/PBRogue.xcodeproj" ] || { echo "Eksport nie utworzył projektu Xcode" >&2; exit 1; }

# 4. Archiwum (podpis automatyczny, dystrybucja)
step "xcodebuild archive"
cd "$OUT"
xcodebuild -project PBRogue.xcodeproj -scheme PBRogue -configuration Release \
    -destination "generic/platform=iOS" -archivePath "$OUT/PBRogue.xcarchive" "${AUTH[@]}" \
    CODE_SIGN_STYLE=Automatic CODE_SIGN_IDENTITY="Apple Development" DEVELOPMENT_TEAM="$TEAM_ID" \
    PRODUCT_BUNDLE_IDENTIFIER="$BUNDLE_ID" archive -quiet

# 5. Eksport .ipa i wysyłka do App Store Connect (TestFlight)
# Klucz API nie ma dostępu do podpisu w chmurze: dystrybucja lokalnym certyfikatem „Apple Distribution”
# (pęk kluczy) i profilem App Store, który asc_profile.py tworzy/instaluje przez API.
PROFILE_NAME="${PROFILE_NAME:-PB-Rogue-AppStore}"
python3 "$HERE/asc_profile.py" --name "$PROFILE_NAME" --bundle "$BUNDLE_ID"
DEST=export
[ "$UPLOAD" = 1 ] && DEST=upload
cat > "$OUT/ExportOptions.plist" <<EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>method</key><string>app-store-connect</string>
    <key>destination</key><string>$DEST</string>
    <key>teamID</key><string>$TEAM_ID</string>
    <key>signingStyle</key><string>manual</string>
    <key>signingCertificate</key><string>Apple Distribution</string>
    <key>provisioningProfiles</key>
    <dict><key>$BUNDLE_ID</key><string>$PROFILE_NAME</string></dict>
    <key>uploadSymbols</key><true/>
    <key>manageAppVersionAndBuildNumber</key><false/>
</dict>
</plist>
EOF
step "xcodebuild -exportArchive ($DEST)"
# /usr/bin/rsync: rsync z Homebrew psuje krok pakowania xcodebuild (jak w planbudowlany-mobile/dev.sh)
PATH="/usr/bin:/bin:/usr/sbin:/sbin:$PATH" xcodebuild -exportArchive -archivePath "$OUT/PBRogue.xcarchive" -exportPath "$OUT/export" \
    -exportOptionsPlist "$OUT/ExportOptions.plist" "${AUTH[@]}"
step "Gotowe: $VERSION ($BUILD_NUMBER)$([ "$UPLOAD" = 1 ] && echo ' wysłane do App Store Connect – TestFlight po przetworzeniu (kilka–kilkanaście minut)')"
