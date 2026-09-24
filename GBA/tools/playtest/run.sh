#!/usr/bin/env bash
# Playtest ROM-u bez okna: tools/playtest/run.sh SKRYPT KATALOG [--fresh]
# Kopiuje PlanBudowlanyRogue.gba do KATALOG/rom.gba (zapis SRAM w KATALOG/rom.sav zostaje
# między uruchomieniami; --fresh go usuwa), wykonuje SKRYPT i zamienia zrzuty na PNG (2x).
set -euo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
gba="$(cd "$here/../.." && pwd)"
script="$(cd "$(dirname "$1")" && pwd)/$(basename "$1")"
out="$2"; mkdir -p "$out"; out="$(cd "$out" && pwd)"
[[ "${3:-}" == "--fresh" ]] && rm -f "$out/rom.sav"
cp "$gba/PlanBudowlanyRogue.gba" "$out/rom.gba"
docker image inspect pb-playtest >/dev/null 2>&1 || docker build -q -t pb-playtest "$here" >/dev/null
docker run --rm -i -v "$here:/src:ro" -v "$out:/out" pb-playtest sh -c \
  'gcc -O2 -o /tmp/playtest /src/playtest.c -lmgba && /tmp/playtest /out/rom.gba /out' < "$script"
python3 - "$out" <<'PY'
import sys, glob, os
from PIL import Image
for p in glob.glob(os.path.join(sys.argv[1], "*.ppm")):
    im = Image.open(p); im.resize((im.width * 2, im.height * 2), Image.NEAREST).save(p[:-4] + ".png"); os.remove(p)
PY
