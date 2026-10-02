#!/usr/bin/env bash
# Monkey test: losowe klawisze w wielu przebiegach (ziarna x ROM-y), równolegle w jednym kontenerze Docker.
#   tools/playtest/monkey.sh [-s "1 2 3" | -n LICZBA_ZIAREN] [-b PIERWSZE_ZIARNO] [-f KLATKI] [-j RÓWNOLEGLE] [-o KATALOG] CEL...
# CEL: numer scenariusza (scnN.gba z -DPB_SCENARIO=N), "main" (monkey.gba = zwykła gra z -DPB_DEBUG_STATS, pusty profil)
#      albo ścieżka do pliku .gba. Każdy przebieg zaczyna od pustego zapisu (rom.sav usuwany).
# Wynik: KATALOG/summary.txt (wiersz na przebieg: OK / CRASH / SOFTLOCK / HANG, maksimum kafli sprite'ów,
#        sprite'ów i stosu),
#        przy błędzie KATALOG/<cel>/s<ziarno>/crash.png, ring_*.png (ostatnie ~30 s co 300 klatek) i inputs.log.
# Powtórka błędu: ten sam ROM + ziarno + klatki (np. -s 17 -f 20000 55) daje dokładnie ten sam przebieg.
set -euo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
gba="$(cd "$here/../.." && pwd)"
seeds=""; count=8; base=1; frames=20000; jobs=8; out="/tmp/pb_monkey"
while getopts "s:n:b:f:j:o:" opt; do
  case $opt in
    s) seeds="$OPTARG" ;; n) count="$OPTARG" ;; b) base="$OPTARG" ;; f) frames="$OPTARG" ;;
    j) jobs="$OPTARG" ;; o) out="$OPTARG" ;; *) exit 2 ;;
  esac
done
shift $((OPTIND - 1))
[[ $# -gt 0 ]] || { sed -n 2,9p "$0"; exit 2; }
[[ -n "$seeds" ]] || seeds="$(seq "$base" $((base + count - 1)) | tr '\n' ' ')"
mkdir -p "$out"; out="$(cd "$out" && pwd)"
jobfile="$out/jobs.txt"; : > "$jobfile"
for t in "$@"; do
  case "$t" in
    main) rom="$gba/monkey.gba"; name="main" ;;
    *.gba) rom="$(cd "$(dirname "$t")" && pwd)/$(basename "$t")"; name="$(basename "$t" .gba)" ;;
    *) rom="$gba/scn$t.gba"; name="scn$t" ;;
  esac
  [[ -f "$rom" ]] || { echo "brak ROM-u: $rom (zbuduj go, opis w GBA/README.md)"; exit 1; }
  mkdir -p "$out/roms"; cp "$rom" "$out/roms/$name.gba"
  for s in $seeds; do
    rm -rf "$out/$name/s$s"; mkdir -p "$out/$name/s$s"
    echo "$name $s" >> "$jobfile"
  done
done
docker image inspect pb-playtest >/dev/null 2>&1 || docker build -q -t pb-playtest "$here" >/dev/null
echo "monkey: $(wc -l < "$jobfile" | tr -d ' ') przebiegów x $frames klatek, $jobs równolegle -> $out"
docker run --rm -v "$here:/src:ro" -v "$out:/out" pb-playtest sh -c \
  "gcc -O2 -o /tmp/playtest /src/playtest.c -lmgba && sed 's/\$/ $frames/' /out/jobs.txt | xargs -P $jobs -L 1 sh /src/monkey_job.sh" | tee "$out/summary.txt"
python3 - "$out" <<'PY'
import sys, glob, os
from PIL import Image
for p in glob.glob(os.path.join(sys.argv[1], "*", "s*", "*.ppm")):
    im = Image.open(p); im.resize((im.width * 2, im.height * 2), Image.NEAREST).save(p[:-4] + ".png"); os.remove(p)
PY
bad=$(grep -cv '^OK ' "$out/summary.txt" || true)
echo "monkey: $(grep -c '^OK ' "$out/summary.txt" || true) OK, $bad z błędem (szczegóły: $out/summary.txt)"
[[ "$bad" == 0 ]]
