#!/usr/bin/env bash
# Kopiuje najnowszy ROM na kartę SD konsoli (np. Miyoo) BEZ montowania – przez mtools (mcopy) prosto na partycję FAT.
# macOS często nie montuje karty FAT32 z konsoli („failed to mount”, flaga dirty), a dostęp do surowego urządzenia
# wymaga sudo, więc skrypt uruchamiasz sam:
#
#   sudo GBA/tools/sd_copy.sh                 najnowszy GBA/PlanBudowlanyRogue-v*.gba -> karta
#   sudo GBA/tools/sd_copy.sh plik.gba        wskazany ROM
#   sudo GBA/tools/sd_copy.sh --list          tylko pokaż katalog ROM-ów na karcie
#
# Zmienne: SD_DEV (np. disk4s1; domyślnie pierwsza partycja FAT32 na czytniku „Secure Digital”),
#          SD_DIR (domyślnie /Roms/GBA), SD_NAME (domyślnie 2137-planbudowlany-rogue.gba – stała nazwa, więc zapis .sav zostaje).
set -euo pipefail

HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
GBA_DIR="$(cd "$HERE/.." && pwd)"
SD_DIR="${SD_DIR:-/Roms/GBA}"
SD_NAME="${SD_NAME:-2137-planbudowlany-rogue.gba}"

command -v mcopy >/dev/null || { echo "Brak mtools: brew install mtools" >&2; exit 1; }
[ "$(id -u)" = 0 ] || { echo "Uruchom przez sudo (dostęp do /dev/rdisk…)" >&2; exit 1; }

# 1. Partycja karty
DEV="${SD_DEV:-}"
if [ -z "$DEV" ]; then
    for d in $(diskutil list | awk '/Windows_FAT_32|DOS_FAT_32/ {print $NF}'); do
        if diskutil info "$d" | grep -q "Protocol: *Secure Digital"; then DEV="$d"; break; fi
    done
fi
[ -n "$DEV" ] || { echo "Nie znaleziono karty SD z FAT32 (ustaw SD_DEV=diskNsM)" >&2; exit 1; }
diskutil info "$DEV" | grep -E "Volume Name|Disk Size|File System Personality|Protocol" | sed 's/^ */  /'
diskutil unmount "$DEV" >/dev/null 2>&1 || true
RAW="/dev/r${DEV#/dev/}"
export MTOOLS_SKIP_CHECK=1

if [ "${1:-}" = "--list" ]; then
    mdir -i "$RAW" "::$SD_DIR"
    exit 0
fi

# 2. ROM
ROM="${1:-$(ls -t "$GBA_DIR"/PlanBudowlanyRogue-v*.gba 2>/dev/null | head -n 1)}"
[ -f "$ROM" ] || { echo "Brak ROM-u (zbuduj go albo podaj ścieżkę)" >&2; exit 1; }
echo "ROM: $ROM -> $DEV:$SD_DIR/$SD_NAME"

# 3. Kopia (nadpisuje poprzednią wersję; .sav o tej samej nazwie zostaje)
mmd -i "$RAW" "::$SD_DIR" 2>/dev/null || true
mcopy -o -i "$RAW" "$ROM" "::$SD_DIR/$SD_NAME"
mdir -i "$RAW" "::$SD_DIR" | grep -i "planbud" || true
sync
diskutil eject "${DEV%s*}" >/dev/null 2>&1 && echo "Karta wysunięta – możesz ją wyjąć." || echo "Gotowe (wysuń kartę ręcznie)."
echo "Na Miyoo: lista gier -> Y -> Refresh, żeby nowy ROM się pokazał."
