# Odtwarza inputs.log z monkey testu jako skrypt playtestera, ze zrzutem co EVERY klatek w [OD, DO] (do szukania,
# co wywołało błąd):  python3 tools/playtest/monkey_replay.py inputs.log OD DO EVERY > skrypt.txt
#                     ROM=scn55.gba tools/playtest/run.sh skrypt.txt /tmp/powtorka --fresh
# Zamiast "shot" można wstawić "status" (klatka, puls gry, PC). Skrypt zaczyna się od "wait 30" jak w monkey.sh.
import sys
log, f0, f1, every = sys.argv[1], int(sys.argv[2]), int(sys.argv[3]), int(sys.argv[4])
print("wait 30"); frame = 30; nxt = f0
def adv(keys, n):
    global frame, nxt
    while n > 0:
        step = n
        if f0 <= nxt <= f1 and frame + step > nxt: step = nxt - frame
        if step > 0:
            print(("hold %s %d" % (keys, step)) if keys != "-" else ("wait %d" % step)); frame += step; n -= step
        if frame == nxt and f0 <= nxt <= f1: print("shot f%06d" % frame); nxt += every
for line in open(log):
    f, k, h, r = line.split(); h = int(h); r = int(r)
    if frame > f1: break
    adv(k, h); adv("-", r)
