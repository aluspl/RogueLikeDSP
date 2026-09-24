#!/usr/bin/env python3
"""Generuje dźwięk gry (Maxmod): efekty WAV i 2 utwory MOD (ProTracker, 4 kanały) - wszystko z kodu.

Uruchom: python3 tools/make_audio.py   (bez zależności poza biblioteką standardową)
Wynik: audio/*.wav (efekty -> bn::sound_items::*) i audio/*.mod (muzyka -> bn::music_items::*).
"""
import math, os, random, struct, wave

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT = os.path.join(ROOT, "audio")
RATE = 16000

# ---------------------------------------------------------------- efekty (8-bit mono WAV)
def square(f, t): return 1.0 if (t * f) % 1.0 < 0.5 else -1.0
def tri(f, t): p = (t * f) % 1.0; return 4 * p - 1 if p < 0.5 else 3 - 4 * p
def sine(f, t): return math.sin(2 * math.pi * f * t)

def render(parts):
    """parts: lista (czas_s, funkcja(t_lokalne) -> -1..1, głośność). Segmenty grane po kolei."""
    out = []
    for dur, fn, vol in parts:
        n = int(dur * RATE)
        for i in range(n):
            env = 1.0 - i / n * 0.7          # lekkie wybrzmiewanie
            att = min(1.0, i / 40)           # bez trzasku na starcie
            out.append(fn(i / RATE) * vol * env * att)
    return out

def write_wav(name, samples):
    rnd = random.Random(1)
    with wave.open(os.path.join(OUT, name + ".wav"), "wb") as w:
        w.setnchannels(1); w.setsampwidth(1); w.setframerate(RATE)
        w.writeframes(bytes(max(0, min(255, int(128 + s * 110))) for s in samples))

def make_sfx():
    rnd = random.Random(7)
    noise = lambda t: rnd.uniform(-1, 1)
    write_wav("sfx_hit", render([(0.03, noise, 0.9), (0.06, lambda t: sine(110 - t * 600, t), 1.0)]))
    write_wav("sfx_hurt", render([(0.16, lambda t: square(320 - t * 1200, t), 0.55)]))
    write_wav("sfx_pickup", render([(0.06, lambda t: square(660, t), 0.45), (0.09, lambda t: square(990, t), 0.45)]))
    write_wav("sfx_level", render([(0.07, lambda t: square(523, t), 0.45), (0.07, lambda t: square(659, t), 0.45),
                                   (0.07, lambda t: square(784, t), 0.45), (0.18, lambda t: square(1047, t), 0.45)]))
    write_wav("sfx_notify", render([(0.10, lambda t: sine(1319, t), 0.8), (0.20, lambda t: sine(988, t), 0.8)]))  # "ding-dong" telefonu
    write_wav("sfx_ability", render([(0.22, lambda t: tri(200 + t * 2400, t), 0.8)]))
    write_wav("sfx_stage", render([(0.09, lambda t: square(392, t), 0.4), (0.09, lambda t: square(523, t), 0.4),
                                   (0.09, lambda t: square(659, t), 0.4), (0.30, lambda t: square(784, t), 0.4)]))
    write_wav("sfx_buy", render([(0.05, lambda t: square(988, t), 0.4), (0.14, lambda t: square(1319, t), 0.4)]))
    write_wav("sfx_menu", render([(0.03, lambda t: square(880, t), 0.3)]))

# ---------------------------------------------------------------- muzyka (MOD)
PERIODS = {}   # nuta "C-2" -> okres Amigi
_names = ["C-", "C#", "D-", "D#", "E-", "F-", "F#", "G-", "G#", "A-", "A#", "B-"]
_base = [856, 808, 762, 720, 678, 640, 604, 570, 538, 508, 480, 453]
for octave in (1, 2, 3):
    for i, n in enumerate(_names):
        PERIODS[f"{n}{octave}"] = _base[i] >> (octave - 1)

def loop_wave(fn, n=32):
    return [int(max(-127, min(127, fn(i / n) * 100))) for i in range(n)]

SAMPLES = [   # (nazwa, dane 8-bit ze znakiem, pętla?)
    ("lead", loop_wave(lambda p: 1 if p < 0.5 else -1), True),                  # 1: prostokąt
    ("bass", loop_wave(lambda p: 4 * p - 1 if p < 0.5 else 3 - 4 * p), True),   # 2: trójkąt
    ("hat", [int(random.Random(3).uniform(-90, 90) * (1 - i / 700)) for i in range(700)], False),  # 3: szum
    ("kick", [int(110 * math.sin(2 * math.pi * (70 - i / 40) * i / 8287) * (1 - i / 1600)) for i in range(1600)], False),  # 4
    ("pad", loop_wave(lambda p: math.sin(2 * math.pi * p) * 0.7 + math.sin(6 * math.pi * p) * 0.3), True),  # 5: miękki
]

def cell(note=None, sample=0, effect=0, param=0):
    period = PERIODS[note] if note else 0
    return bytes([(sample & 0xF0) | (period >> 8), period & 0xFF, ((sample & 0x0F) << 4) | effect, param])

def pattern(chords, melody, lead_sample=1, drums=True):
    """chords: 4 akordy (nuta basu) na 16 rzędów każdy; melody: 64 nuty/None dla kanału prowadzącego."""
    rows = []
    for r in range(64):
        bass_note = chords[r // 16]
        bass = cell(bass_note, 2, 0xC, 40) if r % 4 == 0 else cell()
        lead = cell(melody[r], lead_sample, 0xC, 28) if melody[r] else cell()
        if drums:
            drum = cell("C-2", 4, 0xC, 48) if r % 8 == 0 else (cell("C-3", 3, 0xC, 16) if r % 4 == 2 else cell())
        else:
            drum = cell()
        pad = cell(bass_note.replace("1", "2"), 5, 0xC, 14) if r % 16 == 0 else cell()
        rows.append(lead + bass + drum + pad)
    return b"".join(rows)

def mel(notes, step=2):
    """Lista nut co `step` rzędów -> 64 rzędy."""
    out = [None] * 64
    for i, n in enumerate(notes):
        if i * step < 64: out[i * step] = n
    return out

def write_mod(name, title, patterns, order, speed=6):
    data = title.encode("ascii")[:20].ljust(20, b"\0")
    for i in range(31):
        if i < len(SAMPLES):
            sname, sdata, loop = SAMPLES[i]
            n = len(sdata) + (len(sdata) & 1)
            data += sname.encode("ascii").ljust(22, b"\0") + struct.pack(">HBBHH", n // 2, 0, 64,
                                                                         0 if loop else 0, n // 2 if loop else 1)
        else:
            data += b"\0" * 22 + struct.pack(">HBBHH", 0, 0, 0, 0, 1)
    data += bytes([len(order), 127]) + bytes(order + [0] * (128 - len(order))) + b"M.K."
    first = True
    for p in patterns:
        if first:   # tempo w pierwszym rzędzie kanału perkusji (efekt F: prędkość)
            p = bytearray(p); p[8:12] = bytes([p[8], p[9], (p[10] & 0xF0) | 0xF, speed]); first = False
        data += bytes(p)
    for _, sdata, _ in SAMPLES:
        s = bytes((v + 256) % 256 for v in sdata)
        data += s + (b"\0" if len(s) & 1 else b"")
    open(os.path.join(OUT, name + ".mod"), "wb").write(data)

def make_music():
    # Motyw tytułowy: pogodny, C - G - Am - F
    t1 = pattern(["C-1", "G-1", "A-1", "F-1"],
                 mel(["E-3", "G-3", "C-3", "G-3", "E-3", "D-3", "C-3", None,
                      "D-3", "G-3", "B-2", "G-3", "D-3", "C-3", "B-2", None,
                      "C-3", "E-3", "A-2", "E-3", "C-3", "B-2", "A-2", None,
                      "A-2", "C-3", "F-2", "C-3", "A-2", "G-2", "F-2", None]))
    t2 = pattern(["C-1", "G-1", "F-1", "G-1"],
                 mel(["G-3", None, "E-3", None, "C-3", None, "E-3", "G-3",
                      "B-2", None, "D-3", None, "G-3", None, "F-3", "D-3",
                      "C-3", None, "A-2", None, "F-2", None, "A-2", "C-3",
                      "D-3", None, "B-2", None, "G-2", None, None, None]))
    write_mod("music_title", "PB Roguelike title", [t1, t2], [0, 0, 1, 1])
    # Pętla w grze: skupiona, Am - F - C - G, spokojniejsza melodia
    g1 = pattern(["A-1", "F-1", "C-1", "G-1"],
                 mel(["A-2", None, None, "C-3", None, "E-3", None, None,
                      "F-2", None, None, "A-2", None, "C-3", None, None,
                      "G-2", None, None, "C-3", None, "E-3", None, "D-3",
                      "B-2", None, None, "D-3", None, "G-2", None, None]), lead_sample=5)
    g2 = pattern(["A-1", "F-1", "C-1", "E-1"],
                 mel(["E-3", None, "D-3", None, "C-3", None, "A-2", None,
                      "C-3", None, "A-2", None, "F-2", None, None, None,
                      "E-3", None, "G-3", None, "E-3", None, "C-3", None,
                      "B-2", None, "G#2", None, "E-2", None, None, None]), lead_sample=1)
    write_mod("music_game", "PB Roguelike build", [g1, g2], [0, 1, 0, 1], speed=7)

if __name__ == "__main__":
    os.makedirs(OUT, exist_ok=True)
    make_sfx()
    make_music()
    print("audio:", sorted(f for f in os.listdir(OUT) if not f.startswith(".")))
