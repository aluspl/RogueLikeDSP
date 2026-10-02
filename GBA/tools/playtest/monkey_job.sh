#!/bin/sh
# Jeden przebieg monkey (w kontenerze, wołany przez monkey.sh): monkey_job.sh NAZWA ZIARNO KLATKI
d=/out/$1/s$2
cp /out/roms/$1.gba $d/rom.gba
printf 'wait 30\nmonkey %s %s\n' "$2" "$3" | /tmp/playtest $d/rom.gba $d > $d/stdout.txt 2>&1
code=$?
rm -f $d/rom.gba $d/rom.sav $d/audio.wav
case $code in 0) st=OK ;; 3) st=CRASH ;; 4) st=SOFTLOCK ;; 5) st=HANG ;; *) st=ERR$code ;; esac
echo "$st $1 seed=$2 $(grep '^MONKEY ' $d/stdout.txt | tail -1)"
