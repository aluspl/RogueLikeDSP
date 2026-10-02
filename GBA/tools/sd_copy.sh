#!/usr/bin/env bash
# Kopiuje najnowszy ROM na kartę SD konsoli (np. Miyoo).
# W najnowszych macOS system często odmawia montowania (albo robi to po 15 minutach,
# sprawdzając kartę fsck_msdos). Narzędzie `mtools` (mcopy) z kolei zawiesza się 
# na dużych kartach (np. 128 GB), bo ręczne skanowanie FAT32 zajmuje mu wieki.
# Ten skrypt rozwiązuje oba problemy: montuje kartę ręcznie "pod maską" w /tmp
# omijając powolne sprawdzanie przez GUI macOS.

set -euo pipefail

HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
GBA_DIR="$(cd "$HERE/.." && pwd)"
SD_DIR="${SD_DIR:-/Roms/GBA}"
SD_NAME="${SD_NAME:-2137-planbudowlany-rogue.gba}"

[ "$(id -u)" = 0 ] || { echo "Uruchom przez sudo: sudo GBA/tools/sd_copy.sh" >&2; exit 1; }

# 1. Szukamy partycji FAT32 karty SD
DEV="${SD_DEV:-}"
if [ -z "$DEV" ]; then
    for d in $(diskutil list | awk '/Windows_FAT_32|DOS_FAT_32/ {print $NF}'); do
        if diskutil info "$d" | grep -q "Protocol: *Secure Digital"; then DEV="$d"; break; fi
    done
fi
[ -n "$DEV" ] || { echo "Nie znaleziono karty SD z FAT32 (ustaw SD_DEV=diskNsM)" >&2; exit 1; }

echo "Karta:"
diskutil info "$DEV" | grep -E "Volume Name|Disk Size|File System Personality|Protocol" | sed 's/^ */  /'

# 2. ROM
ROM="${1:-$(ls -t "$GBA_DIR"/PlanBudowlanyRogue-v*.gba 2>/dev/null | head -n 1)}"
[ -f "$ROM" ] || { echo "Brak ROM-u (zbuduj go albo podaj ścieżkę)" >&2; exit 1; }
echo "ROM: $ROM -> $DEV:$SD_DIR/$SD_NAME"

if [ "${1:-}" = "--list" ]; then
    echo "Opcja --list wymaga teraz standardowego montowania. Użyj Findera."
    exit 0
fi

# 3. Montowanie i kopiowanie
MNT_DIR="/tmp/miyoo_sd"

echo "Sprawdzanie stanu zamontowania $DEV..."
if diskutil info "$DEV" | grep -q "Mounted: *Yes"; then
    MOUNT_POINT=$(diskutil info "$DEV" | grep "Mount Point" | sed 's/.*Mount Point: *//')
    echo "Karta jest już zamontowana w systemie ($MOUNT_POINT)."
    
    echo "Kopiowanie na kartę SD (natywne cp)..."
    mkdir -p "$MOUNT_POINT$SD_DIR"
    cp "$ROM" "$MOUNT_POINT$SD_DIR/$SD_NAME"
    sync
    echo "Skopiowano pomyślnie!"
else
    echo "Karta nie jest zamontowana w systemie."
    echo "Wymuszam szybkie, tymczasowe zamontowanie w $MNT_DIR (pomija powolne skanowanie systemu)..."
    
    mkdir -p "$MNT_DIR"
    # Używamy natywnego montowania, które ignoruje fochy macOS'a
    if ! mount -t msdos "/dev/$DEV" "$MNT_DIR"; then
        echo "Błąd: Nie udało się wymusić zamontowania partycji /dev/$DEV!" >&2
        echo "Spróbuj wyjąć i włożyć czytnik ponownie." >&2
        exit 1
    fi
    
    echo "Kopiowanie na kartę SD (natywne cp)..."
    mkdir -p "$MNT_DIR$SD_DIR"
    cp "$ROM" "$MNT_DIR$SD_DIR/$SD_NAME"
    sync
    echo "Skopiowano pomyślnie!"
    
    echo "Odmontowywanie tymczasowe..."
    umount "$MNT_DIR" || true
fi

if diskutil eject "${DEV%s*}" >/dev/null 2>&1; then
    echo "Gotowe. Karta wysunięta – możesz ją wyjąć."
else
    echo "Gotowe (wysuń kartę ręcznie w Finderze)."
fi
echo "Na Miyoo: lista gier -> Y -> Refresh, żeby nowy ROM się pokazał."
