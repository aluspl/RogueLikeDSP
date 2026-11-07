#!/bin/bash
# Reklamówka v0.23: nagranie ujęć (Godot Movie Maker, okno 540x960, pion, dotyk) i montaż 1080x1920.
# Użycie: nagraj.sh KATALOG   (klipy w KATALOG/clips, film KATALOG/reklama_v023.mp4). Wymaga okna – bez --headless.
# Lista ujęć z podpisami: shots_v023.json (ścieżki klipów podmieniane na KATALOG/clips).
set -euo pipefail
OUT=${1:?katalog}; HERE=$(cd "$(dirname "$0")" && pwd)
mkdir -p "$OUT/clips"
rec(){ local n=$1; shift; python3 "$HERE/record.py" "$OUT/clips/$n.avi" 540 960 --touch --portrait "$@"; }
# sceny pokazowe (--bench liczony zegarem, Movie Maker nagrywa szybciej – klip ~5 s)
for sc in title classselect weapon-arc hero-weapons hero-gear weapon-tip-kielnia weapon-legend-found artifact-found case-some; do
  rec "$sc" --screenshot "$OUT/x.png" --scene "$sc" --bench 1.2
done
# bot gra (seed domyślny 424242): deszcz i latarka, filtry, Kwas z bliska, boss aktu
rec murarz --demo 5 --class 1 --view 34 --lights on
rec noir   --demo 4 --class 3 --stage 2 --filter noir
rec retro  --demo 4 --class 6 --stage 4 --filter retro
rec neon   --demo 4 --class 9 --stage 6 --filter neon
rec kwas3  --demo 7 --class 11 --stage 5 --filter kwas --step 4
rec boss3  --demo 7 --class 8 --stage 5 --step 6
sed "s#\"clip\": \"clips/#\"clip\": \"$OUT/clips/#" "$HERE/shots_v023.json" > "$OUT/shots.json"
python3 "$HERE/make_reklama.py" "$OUT/shots.json" "$OUT/reklama_v023.mp4"
