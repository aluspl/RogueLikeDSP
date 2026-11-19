#!/usr/bin/env python3
"""Sprite'y broni (v0.23, TODO #84-#86): broń w ręce (4 kierunki), ikony UI, animacje zamachu - wg rzadkości.

Uruchom z katalogu repozytorium:   python3 GBA/tools/weapon_sprites.py
Wynik: art/weapons/ (PNG + weapons.json, opis w art/weapons/README.md). Wymaga Pillow.

Zasady (z ustaleń właściciela, bez nazw marek):
  * każda broń to mapa znaków skierowana W PRAWO (rękojeść po lewej); kierunki powstają przez odbicie / obrót o 90 st.
  * jedna mapa = jeden zestaw indeksów palety; rzadkość to tylko inna paleta (indeksy 12-15) - na GBA wystarczy
    podmienić paletę sprite'a, w Godocie gotowe PNG dla każdej rzadkości;
  * elektronarzędzia: obudowa H (główny) + h (drugi) w kolorach rzadkości; narzędzia ręczne: drewno/stal/guma,
    kolor rzadkości (A) na akcentach (pasek rękojeści, owijka, ostrze);
  * wspólny akcent: obrys K + 1 px poświaty w kolorze rzadkości (A), Mistrzowska z iskrą (*).

Znaki mapy (indeks palety):
  .  0 przezroczysty     K  1 obrys/czerń       W  2 biały          l  3 jasnoszary (stal)   g  4 szary
  d  5 ciemnoszary       T  6 brąz (drewno)     t  7 jasne drewno   R  8 czerwony            Y  9 żółty
  C 10 cyjan (szkło)     O 11 pomarańcz         H 12 obudowa        h 13 obudowa 2           A 14 kolor rzadkości
  * 15 jasny akcent rzadkości (iskra / połysk)
"""
import json
import math
import os
import sys

from PIL import Image, ImageDraw, ImageFont

HERE = os.path.dirname(os.path.abspath(__file__))
REPO = os.path.dirname(os.path.dirname(HERE))
OUT = os.path.join(REPO, "art", "weapons")
GAME_JSON = os.path.join(REPO, "GBA", "data", "game.json")

CODES = {".": 0, "K": 1, "W": 2, "l": 3, "g": 4, "d": 5, "T": 6, "t": 7, "R": 8, "Y": 9, "C": 10, "O": 11,
         "H": 12, "h": 13, "A": 14, "*": 15}
I_K, I_A, I_STAR = 1, 14, 15

# Paleta stała (indeksy 0-11), wspólna z SPR_PAL z make_assets.py tam, gdzie to możliwe.
BASE_PAL = [(255, 0, 255), (26, 26, 26), (240, 240, 240), (200, 200, 200), (138, 138, 138), (84, 84, 94),
            (139, 90, 43), (196, 140, 78), (214, 60, 60), (245, 211, 61), (90, 208, 230), (255, 122, 61)]

# Rzadkości: (nazwa, obudowa H, obudowa h, akcent A = kolor z game.json weaponRarities, jasny akcent *)
RARITIES = [
    ("Zwykła",      (44, 104, 58),  (36, 36, 40),   (0x8a, 0x87, 0x96), (196, 194, 206)),
    # Solidna: poświata #5cb4f0 (= weaponRarities.color od task 9c; wcześniej #2f8fd6), żeby różniła się jasnością od Zwykłej
    # także w deuteranopii (#8a8796 i #2f8fd6 mają tę samą luminancję 0.25; #5cb4f0 = 0.41) - task 9a
    ("Solidna",     (32, 150, 164), (18, 72, 96),   (0x5c, 0xb4, 0xf0), (200, 236, 255)),
    ("Markowa",     (246, 192, 32), (36, 36, 40),   (0x6b, 0x4e, 0xff), (178, 160, 255)),
    ("Mistrzowska", (214, 44, 40),  (246, 246, 246), (0xd9, 0xa4, 0x00), (255, 244, 160)),
]
RAR_KEYS = ["zwykla", "solidna", "markowa", "mistrzowska"]
DIRS = ["down", "up", "left", "right"]
# Prawa dłoń bohatera w klatce 16x16 actors (WORKER_TOP w pixel_art.py: skóra w x=12, wiersze 10-11). Bohater nie ma
# klatek kierunkowych - przy ruchu w lewo cała klatka jest odbijana (ActorSprite.Flip), dłoń trafia wtedy na x=3.
HAND = (12, 11)
HAND_FLIPPED = (3, 11)


def palette(r):
    _, h1, h2, acc, star = RARITIES[r]
    return BASE_PAL + [h1, h2, acc, star]


# ------------------------------------------------------------------ mapy broni (skierowane w prawo)
# kind: "power" (obudowa w kolorach rzadkości) / "hand" (akcent rzadkości); family: animacja zamachu;
# grip: piksel chwytu w mapie (x, y); rotate=False - przedmiot się nie obraca (książka, tablet, skrzynka lasera).
# shot: pocisk/efekt dla rodzin shoot/beam/spray.
W = {}


def weapon(wid, kind, family, grip, rows, rotate=True, shot=None):
    w = max(len(r) for r in rows)
    W[wid] = dict(kind=kind, family=family, grip=grip, rows=[r.ljust(w, ".") for r in rows], rotate=rotate, shot=shot)


weapon("dziennik", "hand", "beam", (0, 4), [
    "TTTTTTTTW",
    "TTTTTTTTW",
    "TtWWWWtTW",
    "TtWddWtTW",
    "TtWWWWtTW",
    "TTTTTTTTW",
    "TTTTTTTTW",
    "TTTTTTTTW",
    ".AA......",
], rotate=False, shot="mark")
weapon("kielnia", "hand", "swing", (1, 2), [
    ".......ll...",
    "......lWll..",
    "TTAAdglllll*",
    "TTAA..lggl..",
    ".......ll...",
])
weapon("gwozdziarka", "power", "shoot", (2, 4), [   # cecha: skośny magazynek z gwoździami (ld)
    ".HHHHHHHH...",
    "HWWHHHHHHdd.",
    "HHHHHHHHHHdl",
    ".hh...ldldd.",
    ".hh.ldldl...",
    ".hhldld.....",
    ".hdld.......",
], shot="nail")
weapon("probnik", "hand", "beam", (1, 2), [
    "..l.........",
    "ACCCCCCgg...",
    "ACRRRRCglllW",
    "ACCCCCCgg...",
], shot="volt")
weapon("klucz", "hand", "slam", (1, 2), [
    ".........ll.",
    "........lg..",
    "AAAglllllg..",
    "AAAgggggglg.",
    ".........lll",
])
weapon("szlifierka", "power", "swing", (1, 3), [
    "...hh....d..",
    "...hh...dld.",
    "HHHHHHHggld.",
    "hHHhhHHggld.",
    "HHHHHHHggld.",
    "........dld.",
    ".........d..",
], shot="sparks")
weapon("lom", "hand", "slam", (1, 3), [
    "........ddd.",
    ".......d..gd",
    "...........d",
    "AAAgggggggdd",
    "AAAddddddddd",
])
weapon("wkretarka", "power", "stab", (3, 4), [   # cecha: schodkowy uchwyt wiertła + akumulator
    ".HHHHHH.....",
    "HWHHHHHdd...",
    "HHhhhHHddlll",
    ".HHHHHHdd...",
    "..hhh.......",
    "..hhh.......",
    ".hhhhh......",
    ".HHHHH......",
])
weapon("laser", "power", "beam", (0, 6), [
    "...ll.......",
    "..CCCC......",
    "..CWRCRRRRRR",
    "..CCCC......",
    ".HHHHHH.....",
    ".HhhhhH.....",
    "HHHHHHHH....",
], rotate=False, shot="laser")
weapon("mlot", "hand", "slam", (1, 3), [
    "........lllg",
    "........lggd",
    "........lggd",
    "AATTTTTTlggd",
    "AATTTTTTlggd",
    "........lggd",
    "........gddd",
])
weapon("tablet", "power", "beam", (0, 3), [
    "HHHHHHHHH",
    "HCCCCCCCH",
    "HCWWWCCCH",
    "HCCCWWWCH",
    "HCWWCCWCH",
    "HCCCCCCCH",
    "HHHHhHHHH",
], rotate=False, shot="mark")
weapon("miernik", "power", "beam", (1, 5), [
    "HHHHHH......",
    "HCCCCH......",
    "HCWWCHRRRRRR",
    "HCCCCH......",
    "HhKhKH......",
    "HhhhhH......",
    "HHHHHH......",
], shot="laser")
weapon("dachowka", "hand", "shoot", (0, 3), [
    "..RRRRRR..",
    ".RORORORR.",
    "RRRRRRRRRR",
    "AAAAAAAAAA",
    "..RRRRRR..",
    ".RORORORR.",
    "RRRRRRRRRR",
], rotate=False, shot="tile")
weapon("agregat", "power", "spray", (4, 6), [   # cecha: lej z tynkiem nad pistoletem + wąż z tyłu
    "..lWWWWWl...",
    "..lgWWWgl...",
    "...lgggl....",
    "....lgl.....",
    "...HHHHHgggl",
    "d..HhHHHgg.l",
    "dd..hh......",
    ".ddhhh......",
], shot="plaster")
weapon("lyzka", "power", "swing", (2, 1), [   # cecha: łyżka z urobkiem i zębami na krawędzi
    ".dlld.......",
    ".dggdTTtT...",
    "HHHHTTtTTT..",
    "HWHHHHHHHHH.",
    "HHHhhhhhhHHl",
    "HHhhhhhhhHH.",
    ".HHhhhhhHHHl",
    "..HHHHHHHHH.",
    "....l.l.l.l.",
])
weapon("udarowy", "power", "stab", (0, 2), [   # cecha: pałąk z tyłu, boczna rękojeść, długie dłuto
    "hhhHHHHH....",
    "h.hHWHHHd...",
    "h.hHHHHHdlll",
    "hhhHHHHHd...",
    "......hh....",
    ".......hh...",
])
weapon("kotwy", "power", "shoot", (1, 4), [   # cecha: bardzo długa lufa z kołnierzem
    "HHHH........",
    "HWHHgllllldl",
    "HHHHggggggdl",
    ".hh.........",
    ".hh.........",
    ".hh.........",
], shot="anchor")
weapon("spawarka", "power", "spray", (1, 2), [
    "..........Y.",
    "........gYWY",
    "HHHHHhgg..Y.",
    "HhhhHhg.....",
    "HHHHHh......",
    "d...........",
], shot="sparks")
weapon("tyczka", "hand", "stab", (1, 1), [
    "AARRWWRRWWRg",
    "AARRWWRRWWRg",
])
weapon("mlotek", "hand", "slam", (1, 3), [
    ".......lll..",
    ".......lgg..",
    ".......lgg..",
    "AATTTTTlgg..",
    "AATTTTTgdd..",
    "........dd..",
    ".........d..",
])
weapon("mlot_zenka", "hand", "slam", (1, 3), [
    ".......lllll",
    ".......*AAAA",
    ".......lgggd",
    "AATTTTTlgggd",
    "AATTTTTlgggd",
    ".......*AAAA",
    ".......gdddd",
])
weapon("poziomica_m", "hand", "beam", (1, 1), [
    "AttttttttttA",
    "ATdCYdTdCYdA",
    "AttttttttttA",
], shot="laser")
weapon("lopata", "hand", "swing", (1, 2), [
    "tt......ll..",
    "t.t....lggl.",
    "tAtTTTdlgggl",
    "t.t....lggl.",
    "tt......ll..",
])
weapon("siekiera", "hand", "swing", (1, 3), [
    ".......AA...",
    ".......lWW..",
    ".......lggW.",
    "AATTTTTTggd.",
    "AATTTTTTd...",
])
weapon("pila", "hand", "swing", (1, 1), [
    "TTTlllllll..",
    "TATlllllllll",
    "TTTggggggggg",
    "..Tdldldldld",
])
weapon("kilof", "hand", "slam", (1, 3), [
    "......ll....",
    ".......lg...",
    "........gg..",
    "AATTTTTTTgg.",
    "AATTTTTTTgg.",
    "........gg..",
    ".......lg...",
    "......ll....",
])
weapon("przecinak", "hand", "slam", (3, 2), [
    "...AA.......",
    "..AAAA......",
    "llAAAAgggggl",
    "llAAAAddddWl",
    "..AAAA......",
    "...AA.......",
])
weapon("nozyce", "hand", "swing", (1, 0), [
    "AAAA........",
    ".AAAAd......",
    "......dlllll",
    ".AAAAd.ggg..",
    "AAAA........",
])
weapon("silikon", "power", "shoot", (2, 5), [   # cecha: biała tuba w ramce, tłoczysko z tyłu, stożek dyszy
    "...ggggggg..",
    "ddgWWWWWWWl.",
    "...WWWWWWWll",
    "...ggggggg..",
    "..Hh........",
    "..HH........",
    "..HH........",
], shot="blob")
weapon("myjka", "power", "spray", (1, 4), [
    "..........C.",
    ".HHHgggggggC",
    "HHhHH.....C.",
    ".hh.........",
    ".hh.........",
    ".hd.........",
    "..dd........",
], shot="water")
weapon("palnik", "power", "spray", (1, 2), [
    ".h.......ll.",
    "HHHggggggllO",
    "HHHh.....lOY",
    "d.h......ll.",
], shot="flame")
weapon("taker", "power", "shoot", (0, 1), [   # cecha: dźwignia u góry i szeroka, wysoka głowica z przodu
    "hhhhhhhh....",
    "h......hh...",
    "HHHHHHHHHHHH",
    "HWWHHHHHHHHH",
    "HHHHHHHHHddH",
    ".........HWH",
    ".........HHH",
], shot="staple")
weapon("mlotek_gumowy", "hand", "slam", (1, 3), [
    ".......gddd.",
    ".......dddd.",
    ".......dddd.",
    "AATTTTTdddd.",
    "AATTTTTdddd.",
    ".......dddd.",
    ".......ddKd.",
])
weapon("pilarka", "power", "swing", (1, 1), [   # cecha: okrągła tarcza z zębami pod osłoną
    "..hhh.......",
    ".h..hHHHH...",
    ".hhhHHWHHH..",
    "...HHHHHHHH.",
    ".l.glllllg.l",
    "..glllWlllg.",
    ".lglllKlllgl",
    "..glllllllg.",
    ".l..ggggg..l",
], shot="sparks")
weapon("poziomica_2m", "hand", "slam", (1, 1), [
    "AllllllllllA",
    "AgggdCWdgggA",
    "AllllllllllA",
])
weapon("mieszadlo", "power", "slam", (1, 4), [   # cecha: spirala (helisa) na końcu trzpienia
    ".hh.....l.l.",
    "HHHHH...glgl",
    "HWHHHggggglg",
    "HHHHH...lglg",
    ".hh.....glgl",
    ".hh......l.l",
    ".hhh........",
])
weapon("laser_krzyzowy", "power", "beam", (0, 5), [
    "HHHHHHHH",
    "HhhhRhhH",
    "HRRRRRRH",
    "HhhhRhhH",
    "HhhhRhhH",
    "HHHHHHHH",
    "...dd...",
    "..d..d..",
], rotate=False, shot="laser")
weapon("lutownica", "power", "stab", (2, 1), [
    "dHHHHhlllg..",
    "dHhhHhgggggO",
    "dHHHHhllll..",
], shot="flame")
weapon("opalarka", "power", "spray", (3, 5), [   # cecha: gruba dysza z żarem + szeroka stopka
    ".HHHHHH.....",
    "HKHKHHHlllO.",
    "HKHKHHHgggOO",
    "HKHKHHHlllO.",
    ".HHHHHH.....",
    "...hhh......",
    "...hhh......",
    ".HHHHHHH....",
], shot="heat")
weapon("noz", "hand", "stab", (1, 2), [
    "....d.......",
    "AAAAgggglll.",
    "AAAAggggllll",
    "AAAAgggg....",
])
# Nóż majstra seniora (wg zdjęcia właściciela): pomarańczowa, wytarta rękojeść z wybrzuszeniem pod palec,
# zeszlifowane, cienkie ostrze lekko wygięte ku górze. Tylko Mistrzowska (złoty akcent dokłada obrys).
weapon("noz_majstra", "hand", "stab", (1, 3), [
    "...........W",
    "..........lW",
    ".......lllg.",
    "OtOOOYOlgg..",
    "OOOOtOOO....",
    ".....OO.....",
])

# v0.23.0: artefakty (Gablota) – unikatowe warianty broni bazowych, tylko Mistrzowska; każdy z cechą rozpoznawczą.
weapon("mlot_grom", "hand", "slam", (1, 4), [   # cecha: błyskawica (Y) na obuchu, iskra nad głową młota
    ".........YY.",
    "........lllg",
    "........lgYd",
    "........lYgd",
    "AATTTTTTYYYd",
    "AATTTTTTlgYd",
    "........lYgd",
    "........Yddd",
])
weapon("kielnia_cegla", "hand", "swing", (1, 3), [   # cecha: cegła z zaprawą na kielni
    ".....RRWRR..",
    ".....RWRRW..",
    "......lWll..",
    "TTAAdglllll*",
    "TTAA..lggl..",
    ".......ll...",
])
weapon("szlifierka_isk", "power", "swing", (1, 3), [   # cecha: snop iskier spod tarczy
    "...hh....d..Y",
    "...hh...dldY.",
    "HHHHHHHggld.W",
    "hHHhhHHggldY.",
    "HHHHHHHggld.Y",
    "........dldW.",
    ".........d..Y",
], shot="sparks")
weapon("lom_deska", "hand", "slam", (1, 3), [   # cecha: podważona deska (ścięgno drewna) przy pazurze
    ".......ddd..",
    "......d..gd.",
    ".....TtTt..d",
    "AAAgggggggdd",
    "AAAddddddddd",
    ".....tTtT...",
])

# Bronie dokładane w Task 8 (fallback, gdyby jeszcze ich nie było w game.json; dopasowanie także po nazwie): id -> (nazwa, master)
FALLBACK = {"noz": ("Nóż do tapet", False), "noz_majstra": ("Nóż majstra seniora", True)}
MASTER_IDS = {"mlot_zenka", "poziomica_m", "pilarka", "noz_majstra", "mlot_grom", "kielnia_cegla", "szlifierka_isk", "lom_deska"}


# ------------------------------------------------------------------ operacje na siatkach indeksów
def grid_of(rows):
    return [[CODES[ch] for ch in r] for r in rows]


def rot_cw(g):
    return [list(r) for r in zip(*g[::-1])]


def rot_ccw(g):
    return [list(r) for r in zip(*g)][::-1]


def mirror(g):
    return [r[::-1] for r in g]


def blank(w, h):
    return [[0] * w for _ in range(h)]


def paste(dst, src, ox, oy):
    for y, r in enumerate(src):
        for x, v in enumerate(r):
            if v and 0 <= y + oy < len(dst) and 0 <= x + ox < len(dst[0]):
                dst[y + oy][x + ox] = v


def ring(g, color, only_empty=True, diag=False):
    """Dorysowuje 1 px dookoła niepustych pikseli (4-sąsiedztwo, opcjonalnie skosy)."""
    h, w = len(g), len(g[0])
    out = [r[:] for r in g]
    nb = [(1, 0), (-1, 0), (0, 1), (0, -1)] + ([(1, 1), (1, -1), (-1, 1), (-1, -1)] if diag else [])
    for y in range(h):
        for x in range(w):
            if g[y][x]:
                continue
            if any(0 <= x + dx < w and 0 <= y + dy < h and g[y + dy][x + dx] for dx, dy in nb):
                out[y][x] = color
    return out


def finish(g, rarity, glow=True, diag=False):
    """Obrys K + poświata rzadkości (+ iskra Mistrzowskiej). diag=True: obrys także po skosach (obrócone ikony)."""
    g = ring(g, I_K, diag=diag)
    if glow:
        inner = [[1 if v else 0 for v in r] for r in g]
        gl = ring(inner, I_A)
        for y, r in enumerate(gl):
            for x, v in enumerate(r):
                if v == I_A and not g[y][x]:
                    g[y][x] = I_A
    if rarity == 3:
        sparkle(g)
    elif rarity == 1 and glow:
        glint(g)
    return g


def glint(g):
    """Solidna: 1 px jasnego błysku na poświacie w prawym górnym rogu (rozpoznawalna bez koloru, task 9a)."""
    for y in range(len(g)):
        for x in range(len(g[0]) - 1, -1, -1):
            if g[y][x] == I_A:
                g[y][x] = I_STAR
                return


def sparkle(g):
    """Iskra Mistrzowskiej: krzyżyk 3x3 (* w środku, A w ramionach) w najwyżej-prawym wolnym miejscu."""
    h, w = len(g), len(g[0])
    best = None
    for y in range(1, h - 1):
        for x in range(w - 2, 0, -1):
            cells = [(x, y), (x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1)]
            near = any(g[yy][xx] for yy in range(max(0, y - 2), min(h, y + 3))
                       for xx in range(max(0, x - 2), min(w, x + 3)))
            if near and all(g[cy][cx] == 0 for cx, cy in cells):
                score = y * 2 - x
                if best is None or score < best[0]:
                    best = (score, x, y)
    if best:
        _, x, y = best
        g[y][x] = I_STAR
        for cx, cy in ((x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1)):
            g[cy][cx] = I_A
    else:   # brak miejsca: jeden jasny piksel na poświacie w prawym górnym rogu
        for y in range(h):
            for x in range(w - 1, -1, -1):
                if g[y][x] == I_A:
                    g[y][x] = I_STAR
                    return


def oriented(wid, d):
    """Siatka broni (bez obrysu) skierowana w danym kierunku + pozycja chwytu."""
    spec = W[wid]
    g = grid_of(spec["rows"])
    gx, gy = spec["grip"]
    h, w = len(g), len(g[0])
    if d == "right" or (not spec["rotate"] and d in ("down", "up")):
        return g, (gx, gy)
    if d == "left":
        return mirror(g), (w - 1 - gx, gy)
    if d == "up":     # obrót przeciwnie do zegara: (x, y) -> (y, w-1-x)
        return rot_ccw(g), (gy, w - 1 - gx)
    # down: obrót zgodnie z zegarem: (x, y) -> (h-1-y, x)
    return rot_cw(g), (h - 1 - gy, gx)


def frame16(g, grip, rarity, glow=True):
    """Wyśrodkowanie w klatce 16x16 (z obrysem i poświatą) -> (klatka, chwyt w klatce)."""
    h, w = len(g), len(g[0])
    ox, oy = (16 - w) // 2, (16 - h) // 2
    f = blank(16, 16)
    paste(f, g, ox, oy)
    return finish(f, rarity, glow), (grip[0] + ox, grip[1] + oy)


MAP_SCALE = 0.75   # v0.23.0 (task 9c): broń w ręce na mapie ~3/4 (12 px na bohaterze 16 px)


def shrink(g, grip, f=MAP_SCALE):
    """Mapa broni zmniejszona najbliższym sąsiadem (próbki ze środków pól) + przeliczony chwyt."""
    h, w = len(g), len(g[0])
    nh, nw = max(1, round(h * f)), max(1, round(w * f))
    out = [[g[min(h - 1, int((y + 0.5) / f))][min(w - 1, int((x + 0.5) / f))] for x in range(nw)] for y in range(nh)]
    gx, gy = min(nw - 1, int(grip[0] * f)), min(nh - 1, int(grip[1] * f))
    if not out[gy][gx]:   # chwyt na pustym polu - najbliższe niepuste
        cand = [(abs(x - gx) + abs(y - gy), x, y) for y in range(nh) for x in range(nw) if out[y][x]]
        _, gx, gy = min(cand)
    return out, (gx, gy)


def map_frame16(wid, d, rarity):
    """Broń w ręce na mapie: mapa 3/4, obwódka (poświata) tylko Markowa i Mistrzowska - Zwykła i Solidna sam obrys
    i kolor akcentu (pełny rozmiar przytłaczał postać i zwisał pod butami przy down)."""
    spec = W[wid]
    small, sgrip = shrink(grid_of(spec["rows"]), spec["grip"])
    saved = dict(spec)
    spec["rows"] = ["".join(".KWlgdTtRYCOHhA*"[v] for v in r) for r in small]
    spec["grip"] = sgrip
    try:
        g, grip = oriented(wid, d)
    finally:
        spec.update(saved)
    return frame16(g, grip, rarity, glow=rarity >= 2)


def diagonal16(wid):
    """Broń obrócona o 45 st. (RotSprite) i wyśrodkowana w 16x16 - ikony narzędzi ręcznych (jak w klasycznych RPG)."""
    g = grid_of(W[wid]["rows"])
    h, w = len(g), len(g[0])
    gx, gy = W[wid]["grip"]
    cx, cy = (w - 1) / 2 - gx, (h - 1) / 2 - gy
    a = math.radians(45)
    rx, ry = cx * math.cos(a) + cy * math.sin(a), -cx * math.sin(a) + cy * math.cos(a)
    return rotated(wid, 45, 16, (round(7.5 - rx), round(7.5 - ry)))


def icon_is_diagonal(wid):
    return W[wid]["kind"] == "hand" and W[wid]["rotate"]


def icon16(wid, rarity):
    if icon_is_diagonal(wid):
        f = finish(diagonal16(wid), rarity, diag=True)
    else:
        g, _ = oriented(wid, "right")
        f, _ = frame16(g, (0, 0), rarity)
    # znacznik rzadkości niezależny od koloru (daltonizm): Solidna 1, Markowa 2 kropki w lewym dolnym rogu,
    # Mistrzowska - iskra (sparkle) już jest
    for i in range(rarity if rarity in (1, 2) else 0):
        x, y = 1 + i * 2, 14
        if f[y][x] in (0, I_A):
            f[y][x] = I_STAR
            f[y + 1][x] = I_K if f[y + 1][x] == 0 else f[y + 1][x]
    return f


# ------------------------------------------------------------------ RotSprite (obrót bez rozmycia)
def scale2x(frame):
    h, w = len(frame), len(frame[0])
    out = [[0] * (w * 2) for _ in range(h * 2)]

    def px(x, y):
        return frame[min(max(y, 0), h - 1)][min(max(x, 0), w - 1)]

    for y in range(h):
        for x in range(w):
            p = frame[y][x]
            a, b, c, d = px(x, y - 1), px(x + 1, y), px(x - 1, y), px(x, y + 1)
            e0 = e1 = e2 = e3 = p
            if c == a and c != d and a != b:
                e0 = a
            if a == b and a != c and b != d:
                e1 = b
            if d == c and d != b and c != a:
                e2 = c
            if b == d and b != a and d != c:
                e3 = d
            out[2 * y][2 * x], out[2 * y][2 * x + 1] = e0, e1
            out[2 * y + 1][2 * x], out[2 * y + 1][2 * x + 1] = e2, e3
    return out


_ROT_CACHE = {}


def rotated(wid, angle_deg, size, pivot):
    """Broń obrócona o kąt (0 = w prawo, 90 = w górę) wokół chwytu, chwyt w punkcie pivot klatki size x size."""
    key = (wid, angle_deg, size, pivot)
    if key in _ROT_CACHE:
        return _ROT_CACHE[key]
    g = grid_of(W[wid]["rows"])
    gx, gy = W[wid]["grip"]
    big = scale2x(scale2x(scale2x(g)))
    S = 8
    a = math.radians(angle_deg)
    ca, sa = math.cos(a), math.sin(a)
    out = blank(size, size)
    cx, cy = gx * S + S / 2, gy * S + S / 2
    bh, bw = len(big), len(big[0])
    for y in range(size):
        for x in range(size):
            # środek piksela docelowego względem chwytu (oś y w dół), obrót odwrotny
            dx, dy = (x - pivot[0]) * S, (y - pivot[1]) * S
            sx = dx * ca - dy * sa
            sy = dx * sa + dy * ca
            bx, by = int(round(cx + sx)), int(round(cy + sy))
            if 0 <= bx < bw and 0 <= by < bh:
                out[y][x] = big[by][bx]
    _ROT_CACHE[key] = out
    return out


# ------------------------------------------------------------------ animacja zamachu (32x32 GBA, 6 klatek)
SW = 32
PIVOT = (11, 20)
FAMILY_FRAMES = {
    # kąt broni w kolejnych klatkach (stopnie, 0 = w prawo, 90 = w górę) i przesunięcie chwytu
    "swing": [(150, 0, 0), (125, 0, 0), (75, 1, 0), (20, 2, 0), (-35, 2, 1), (-25, 1, 1)],
    "slam":  [(70, 0, 0), (115, -1, -1), (130, -1, -2), (60, 1, 0), (-5, 2, 2), (-5, 2, 2)],
    "stab":  [(10, 0, 0), (10, -3, 0), (5, 3, 0), (5, 7, 0), (5, 7, 0), (10, 1, 0)],
    "shoot": [(15, 0, 0), (5, 0, 0), (5, 0, 0), (20, -2, -1), (12, -1, 0), (8, 0, 0)],
    "beam":  [(10, 0, 0), (5, 0, 0), (5, 0, 0), (5, 0, 0), (5, 0, 0), (8, 0, 0)],
    "spray": [(10, 0, 0), (5, 0, 0), (5, 0, 0), (8, -1, 0), (5, 0, 0), (8, 0, 0)],
}
SHOT_COLORS = {"nail": [3, 4], "anchor": [4, 5], "blob": [2, 3], "staple": [3], "tile": [8, 11],
               "laser": [8, 2], "volt": [9, 2, 10], "mark": [9, 2], "plaster": [2, 3], "water": [10, 2],
               "flame": [11, 9, 8], "heat": [11, 8], "sparks": [9, 2, 11]}


def tip_of(fr):
    """Najdalszy od chwytu piksel broni (koniec ostrza) w klatce."""
    best, bp = -1, None
    for y, r in enumerate(fr):
        for x, v in enumerate(r):
            if v:
                d = (x - PIVOT[0]) ** 2 + (y - PIVOT[1]) ** 2
                if d > best:
                    best, bp = d, (x, y)
    return bp


def swing_frames(wid, rarity):
    spec = W[wid]
    fam = spec["family"]
    shot = spec["shot"]
    cols = SHOT_COLORS.get(shot, [9, 2])
    frames = []
    tips = []
    for i, (ang, mx, my) in enumerate(FAMILY_FRAMES[fam]):
        pivot = (PIVOT[0] + mx, PIVOT[1] + my)
        g = rotated(wid, ang, SW, pivot)
        tip = tip_of(g) or pivot
        fx = blank(SW, SW)
        if fam == "swing" and i in (2, 3, 4):   # smuga za ostrzem w kolorze rzadkości
            for t0 in tips[-2:]:
                draw_line(fx, t0, tip, I_A)
            if shot == "sparks" and i == 3:
                burst(fx, tip, cols, 3)
        if fam == "slam" and i in (3, 4):
            for t0 in tips[-1:]:
                draw_line(fx, t0, tip, I_A)
        if fam == "slam" and i in (4, 5):        # uderzenie: gwiazdka + pył przy obuchu
            star(fx, (tip[0], min(tip[1] + 1, SW - 3)), [9, 2] if i == 4 else [3], 3 if i == 4 else 2)
        if fam == "stab" and i in (3, 4):
            for k in range(3, 8):                 # smugi pchnięcia
                fx[pivot[1] - 3][max(0, pivot[0] - k)] = I_A if k % 2 else 0
                fx[pivot[1] + 2][max(0, pivot[0] - k - 1)] = I_A if k % 2 == 0 else 0
            if i == 3:
                star(fx, (min(tip[0] + 1, SW - 3), tip[1]), [9, 2], 2)
        if fam == "shoot":
            if i == 1:
                star(fx, (min(tip[0] + 1, SW - 2), tip[1]), [9, 2], 1)
            if i in (2, 3, 4):
                px_x = tip[0] + 3 + (i - 2) * 6
                projectile(fx, shot, (px_x, tip[1] - (1 if shot == "tile" and i > 2 else 0) * (i - 2)), cols)
        if fam == "beam" and i in (1, 2, 3, 4):
            length = [5, 14, 30, 30][i - 1]
            beam(fx, shot, tip, length, cols, i)
        if fam == "spray" and i in (1, 2, 3, 4):
            cone(fx, tip, [4, 8, 12, 12][i - 1], cols, seed=i + len(wid))
        tips.append(tip)
        # efekt pod bronią, broń na wierzchu, potem obrys i poświata
        layer = [r[:] for r in fx]
        paste(layer, g, 0, 0)
        weapon_only = [r[:] for r in g]
        outl = finish(weapon_only, rarity, glow=True)
        for y in range(SW):
            for x in range(SW):
                if outl[y][x] and not g[y][x] and not fx[y][x]:
                    layer[y][x] = outl[y][x]
        frames.append(layer)
    return frames


def draw_line(g, a, b, c):
    (x0, y0), (x1, y1) = a, b
    n = max(abs(x1 - x0), abs(y1 - y0), 1)
    for k in range(n + 1):
        x = round(x0 + (x1 - x0) * k / n)
        y = round(y0 + (y1 - y0) * k / n)
        if 0 <= x < len(g[0]) and 0 <= y < len(g) and not g[y][x]:
            g[y][x] = c


def setp(g, x, y, c):
    if 0 <= x < len(g[0]) and 0 <= y < len(g):
        g[y][x] = c


def star(g, p, cols, r):
    x, y = p
    setp(g, x, y, cols[-1] if len(cols) > 1 else cols[0])
    for k in range(1, r + 1):
        c = cols[0]
        for dx, dy in ((k, 0), (-k, 0), (0, k), (0, -k)):
            setp(g, x + dx, y + dy, c)
    if r >= 2:
        for dx, dy in ((1, 1), (1, -1), (-1, 1), (-1, -1)):
            setp(g, x + dx, y + dy, cols[0])


def burst(g, p, cols, n):
    x, y = p
    for k, (dx, dy) in enumerate(((2, -2), (3, 0), (2, 2), (0, -3), (4, -1))[:n + 2]):
        setp(g, x + dx, y + dy, cols[k % len(cols)])


def projectile(g, shot, p, cols):
    x, y = p
    if shot == "tile":
        for dx in range(-1, 3):
            setp(g, x + dx, y, 8)
            setp(g, x + dx, y + 1, 11 if dx % 2 else 8)
        setp(g, x - 1, y + 2, 1)
    elif shot == "blob":
        for dx, dy in ((0, 0), (1, 0), (0, 1), (1, 1), (2, 0)):
            setp(g, x + dx, y + dy, 2)
        setp(g, x + 2, y + 1, 3)
    elif shot == "staple":
        for dx in range(3):
            setp(g, x + dx, y - 1, 3)
        setp(g, x, y, 3)
        setp(g, x + 2, y, 3)
    else:   # gwóźdź / kotwa
        for dx in range(4):
            setp(g, x + dx, y, cols[0])
        setp(g, x, y - 1, cols[-1])
        setp(g, x, y + 1, cols[-1])
    setp(g, x - 2, y, I_A)
    setp(g, x - 4, y, I_A)


def beam(g, shot, tip, length, cols, i):
    x0, y = tip[0] + 1, tip[1]
    for k in range(length):
        x = x0 + k
        if shot == "volt":
            yy = y + (0, -1, 0, 1)[k % 4]
            setp(g, x, yy, cols[k % 2])
        elif shot == "mark":
            if k % 3 != 2:
                setp(g, x, y, 9)
        else:
            setp(g, x, y, cols[0])
            if i == 3 and k % 2 == 0:
                setp(g, x, y - 1, I_A)
                setp(g, x, y + 1, I_A)
    end = (min(x0 + length, SW - 2), y)
    if i >= 2:
        star(g, end, [cols[0], 2], 1 if i != 3 else 2)
    if shot == "mark" and i >= 3:
        star(g, end, [9, 2], 2)


def cone(g, tip, length, cols, seed):
    rnd = (seed * 2654435761) & 0xFFFFFFFF
    x0, y0 = tip[0] + 1, tip[1]
    for k in range(length):
        spread = k // 2
        for j in range(-spread, spread + 1):
            rnd = (rnd * 1103515245 + 12345) & 0x7FFFFFFF
            if (rnd >> 8) % 3 == 0 or j == 0 and k % 2 == 0:
                setp(g, x0 + k, y0 + j, cols[(rnd >> 4) % len(cols)])


# ------------------------------------------------------------------ zapis
def to_image(frame, pal):
    h, w = len(frame), len(frame[0])
    im = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    px = im.load()
    for y in range(h):
        for x in range(w):
            v = frame[y][x]
            if v:
                px[x, y] = pal[v] + (255,)
    return im


def weapon_list():
    """Kolejność wierszy: bronie z game.json, potem brakujące z FALLBACK; nazwa i flaga Mistrzowskiej z danych."""
    data = json.load(open(GAME_JSON, encoding="utf-8"))
    out = []
    seen = set()
    for w in data["weapons"]:
        wid = w["id"]
        if wid not in W:
            # nowa broń bez mapy - spróbuj dopasować po nazwie do FALLBACK, inaczej ostrzeż
            match = [k for k, (n, _) in FALLBACK.items() if n == w["name"]]
            if match:
                W[wid] = W[match[0]]
                seen.add(match[0])
            else:
                print("UWAGA: brak mapy sprite'a dla broni", wid, w["name"], file=sys.stderr)
                continue
        seen.add(wid)
        out.append((wid, w["name"], bool(w.get("master")) or wid in MASTER_IDS))
    for wid, (name, master) in FALLBACK.items():
        if wid not in seen:
            out.append((wid, name, master))
    return out


def rarities_for(master):
    return [3] if master else [0, 1, 2, 3]


def main():
    os.makedirs(OUT, exist_ok=True)
    weapons = weapon_list()
    pals = [palette(r) for r in range(4)]
    n = len(weapons)

    inhand = Image.new("RGBA", (16 * 16, 16 * n), (0, 0, 0, 0))
    inhand2 = Image.new("RGBA", (32 * 16, 32 * n), (0, 0, 0, 0))
    inmap = Image.new("RGBA", (16 * 16, 16 * n), (0, 0, 0, 0))
    inmap2 = Image.new("RGBA", (32 * 16, 32 * n), (0, 0, 0, 0))
    icons = Image.new("RGBA", (16 * 4, 16 * n), (0, 0, 0, 0))
    icons2 = Image.new("RGBA", (32 * 4, 32 * n), (0, 0, 0, 0))
    gba_idx = []   # indeksy (bez palety) dla GBA: wiersz = broń, kolumna = kierunek (paleta = rzadkość)
    swing_rows = []
    manifest = {"version": 1, "frame": {"inhand": 16, "inhandGodot": 32, "icon": 16, "iconGodot": 32,
                                        "swing": SW, "swingGodot": SW * 2, "swingFrames": 6},
                "heroHand": {"default": list(HAND), "flipped": list(HAND_FLIPPED),
                             "godot": [HAND[0] * 2, HAND[1] * 2], "godotFlipped": [HAND_FLIPPED[0] * 2, HAND_FLIPPED[1] * 2]},
                "directions": DIRS, "rarities": [r[0] for r in RARITIES], "rarityKeys": RAR_KEYS,
                "palettes": {RAR_KEYS[r]: ["#%02x%02x%02x" % c for c in pals[r]] for r in range(4)},
                "weapons": []}
    for row, (wid, name, master) in enumerate(weapons):
        rars = rarities_for(master)
        anchors = {}
        map_anchors = {}
        for di, d in enumerate(DIRS):
            g, grip = oriented(wid, d)
            for r in rars:
                f, anchor = frame16(g, grip, r)
                anchors[d] = list(anchor)
                im = to_image(f, pals[r])
                inhand.paste(im, ((r * 4 + di) * 16, row * 16))
                im2 = to_image(scale2x(f), pals[r])
                inhand2.paste(im2, ((r * 4 + di) * 32, row * 32))
                fm, manchor = map_frame16(wid, d, r)
                map_anchors[d] = list(manchor)
                inmap.paste(to_image(fm, pals[r]), ((r * 4 + di) * 16, row * 16))
                inmap2.paste(to_image(scale2x(fm), pals[r]), ((r * 4 + di) * 32, row * 32))
        for r in rars:
            f = icon16(wid, r)
            icons.paste(to_image(f, pals[r]), (r * 16, row * 16))
            icons2.paste(to_image(scale2x(f), pals[r]), (r * 32, row * 32))
        swing_list = []
        for r in rars:
            if r < 2:
                continue
            frs = swing_frames(wid, r)
            swing_rows.append((wid, r, frs))
            swing_list.append({"rarity": RAR_KEYS[r], "row": len(swing_rows) - 1})
        manifest["weapons"].append({"id": wid, "name": name, "row": row, "kind": W[wid]["kind"],
                                    "family": W[wid]["family"], "master": master,
                                    "rarities": [RAR_KEYS[r] for r in rars],
                                    "grip": anchors, "gripGodot": {k: [v[0] * 2, v[1] * 2] for k, v in anchors.items()},
                                    "gripMap": map_anchors,
                                    "gripMapGodot": {k: [v[0] * 2, v[1] * 2] for k, v in map_anchors.items()},
                                    "swing": swing_list})
    inhand.save(os.path.join(OUT, "weapons_inhand.png"))
    inhand2.save(os.path.join(OUT, "weapons_inhand_2x.png"))
    inmap.save(os.path.join(OUT, "weapons_inhand_map.png"))
    inmap2.save(os.path.join(OUT, "weapons_inhand_map_2x.png"))
    icons.save(os.path.join(OUT, "weapons_icons.png"))
    icons2.save(os.path.join(OUT, "weapons_icons_2x.png"))

    sw = Image.new("RGBA", (SW * 6, SW * len(swing_rows)), (0, 0, 0, 0))
    sw2 = Image.new("RGBA", (SW * 2 * 6, SW * 2 * len(swing_rows)), (0, 0, 0, 0))
    for i, (wid, r, frs) in enumerate(swing_rows):
        for k, f in enumerate(frs):
            sw.paste(to_image(f, pals[r]), (k * SW, i * SW))
            sw2.paste(to_image(scale2x(f), pals[r]), (k * SW * 2, i * SW * 2))
    sw.save(os.path.join(OUT, "weapons_swing.png"))
    sw2.save(os.path.join(OUT, "weapons_swing_2x.png"))
    manifest["swingRows"] = [{"id": wid, "rarity": RAR_KEYS[r]} for wid, r, _ in swing_rows]
    with open(os.path.join(OUT, "weapons.json"), "w", encoding="utf-8") as fh:
        json.dump(manifest, fh, ensure_ascii=False, indent=1)
        fh.write("\n")
    make_preview(weapons, inhand2, icons2, sw2, swing_rows)
    print("bronie:", n, "| klatki zamachu:", len(swing_rows), "->", os.path.relpath(OUT, REPO))


# ------------------------------------------------------------------ podgląd (contact sheet) + test daltonizmu
def font(size):
    for p in ("/System/Library/Fonts/Supplemental/Arial.ttf", "/Library/Fonts/Arial.ttf",
              "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf"):
        if os.path.exists(p):
            return ImageFont.truetype(p, size)
    return ImageFont.load_default()


def deutan(im):
    """Symulacja deuteranopii (Machado 2009, nasilenie 1.0) w liniowym RGB."""
    M = [(0.367322, 0.860646, -0.227968), (0.280085, 0.672501, 0.047413), (-0.011820, 0.042940, 0.968881)]
    lut_in = [((c / 255) / 12.92 if c / 255 <= 0.04045 else ((c / 255 + 0.055) / 1.055) ** 2.4) for c in range(256)]

    def enc(v):
        v = min(max(v, 0.0), 1.0)
        return int(round(255 * (12.92 * v if v <= 0.0031308 else 1.055 * v ** (1 / 2.4) - 0.055)))

    out = im.copy()
    px = out.load()
    cache = {}
    for y in range(out.height):
        for x in range(out.width):
            p = px[x, y]
            k = p[:3]
            if k not in cache:
                r, g, b = (lut_in[c] for c in k)
                cache[k] = tuple(enc(m[0] * r + m[1] * g + m[2] * b) for m in M)
            px[x, y] = cache[k] + (p[3],)
    return out


def make_preview(weapons, inhand2, icons2, sw2, swing_rows):
    f_name, f_small, f_head = font(15), font(11), font(13)
    BG, PANEL = (46, 44, 58), (60, 58, 74)
    ROW = 76
    left = 190
    col_w = 32 * 2 + 4 + 4 * 34 + 14   # ikona 2x + 4 kierunki w skali gry
    W_ = left + 4 * col_w + 120
    H_ = 40 + ROW * len(weapons)
    sheet = Image.new("RGBA", (W_, H_), BG + (255,))
    d = ImageDraw.Draw(sheet)
    for r, (name, *_c) in enumerate(RARITIES):
        x = left + r * col_w
        d.text((x, 10), name, font=f_head, fill=tuple(_c[2]))
    d.text((left + 4 * col_w + 4, 10), "na postaci", font=f_head, fill=(220, 220, 230))
    try:
        actors = Image.open(os.path.join(REPO, "GODOT", "godot", "assets", "sprites", "actors.png"))
        hero = actors.crop((0, 32 * 1, 32, 32 * 2))
    except OSError:
        hero = None
    man = json.load(open(os.path.join(OUT, "weapons.json"), encoding="utf-8"))
    for i, (wid, name, master) in enumerate(weapons):
        y = 36 + i * ROW
        d.rectangle((4, y, W_ - 4, y + ROW - 6), fill=PANEL)
        d.text((10, y + 8), name, font=f_name, fill=(240, 240, 245))
        d.text((10, y + 30), f"{wid}  [{W[wid]['kind']}, {W[wid]['family']}]", font=f_small, fill=(170, 170, 185))
        if master:
            d.text((10, y + 46), "tylko Mistrzowska", font=f_small, fill=(217, 164, 0))
        for r in rarities_for(master):
            x = left + r * col_w
            ic = icons2.crop((r * 32, i * 32, r * 32 + 32, i * 32 + 32)).resize((64, 64), Image.NEAREST)
            sheet.alpha_composite(ic, (x, y + 3))
            for di in range(4):
                fr = inhand2.crop(((r * 4 + di) * 32, i * 32, (r * 4 + di) * 32 + 32, i * 32 + 32))
                sheet.alpha_composite(fr, (x + 68 + di * 34, y + 20))
        if hero is not None:   # bohater w skali gry z bronią Markową/Mistrzowską (kierunek "down", chwyt przy prawej ręce)
            r = 3 if master else 2
            x0 = left + 4 * col_w + 10
            sheet.alpha_composite(hero, (x0 + 10, y + 18))
            gx, gy = man["weapons"][i]["gripGodot"]["down"]
            fr = inhand2.crop(((r * 4 + 0) * 32, i * 32, (r * 4 + 0) * 32 + 32, i * 32 + 32))
            hand = (x0 + 10 + HAND[0] * 2, y + 18 + HAND[1] * 2)   # prawa dłoń bohatera
            sheet.alpha_composite(fr, (hand[0] - gx, hand[1] - gy))
    sheet.save(os.path.join(OUT, "preview.png"))
    deutan(sheet).save(os.path.join(OUT, "preview_deutan.png"))
    make_hero_preview(weapons, inhand2, man, f_name, f_small, f_head)

    # podgląd animacji
    S = 64
    sp = Image.new("RGBA", (220 + 6 * (S + 6), 30 + len(swing_rows) * (S + 8)), BG + (255,))
    d = ImageDraw.Draw(sp)
    names = {w: n for w, n, _ in weapons}
    for i, (wid, r, _f) in enumerate(swing_rows):
        y = 20 + i * (S + 8)
        d.text((8, y + 18), names[wid], font=f_name, fill=(240, 240, 245))
        d.text((8, y + 38), f"{RARITIES[r][0]} · {W[wid]['family']}", font=f_small, fill=RARITIES[r][3])
        for k in range(6):
            fr = sw2.crop((k * S, i * S, k * S + S, i * S + S))
            d.rectangle((220 + k * (S + 6), y, 220 + k * (S + 6) + S - 1, y + S - 1), fill=PANEL)
            sp.alpha_composite(fr, (220 + k * (S + 6), y))
    sp.save(os.path.join(OUT, "preview_swing.png"))


HERO_PREVIEW = ["murarz", "operator", "geodeta"]   # mała broń ręczna, duża elektryczna, długa tyczka


def hero_frames():
    """(id zawodu, nazwa, broń domyślna, klatka actors.png 32x32, ta sama klatka bez narzędzia zawodu) dla podglądu."""
    sys.path.insert(0, HERE)
    import pixel_art as pa
    raw = Image.open(os.path.join(REPO, "GBA", "graphics", "actors.bmp")).getpalette()   # SPR_PAL z make_assets.py
    SPR_PAL = [tuple(raw[i:i + 3]) for i in range(0, 48, 3)]
    data = json.load(open(GAME_JSON, encoding="utf-8"))
    actors = Image.open(os.path.join(REPO, "GODOT", "godot", "assets", "sprites", "actors.png")).convert("RGBA")
    out = []
    for ci, c in enumerate(data["classes"]):
        if c["id"] not in HERO_PREVIEW:
            continue
        real = actors.crop((0, 32 * c["frame"], 32, 32 * c["frame"] + 32))
        helmet, vest, tool = pa.WORKERS[ci]
        px = pa.worker(helmet, vest, 0)
        for y, row in pa.FACES.get(tool, {}).items():
            for x, ch in enumerate(row):
                if ch != ".":
                    px[y * 16 + x] = pa.CODES[ch]
        g = [px[y * 16:(y + 1) * 16] for y in range(16)]
        bare = to_image(scale2x(g), SPR_PAL)
        out.append((c["id"], c["name"], c["weapon"], real, bare))
    return out


def make_hero_preview(weapons, inhand2, man, f_name, f_small, f_head):
    """art/weapons/preview_hero.png: 3 zawody x 4 rzadkości x 4 kierunki, broń na pikselu chwytu w dłoni bohatera.
    Lewa część: klatka z actors.png (z narzędziem zawodu wrysowanym w klatkę), prawa: ta sama postać bez narzędzia.
    Kierunek left = klatka bohatera odbita (Flip), up = broń pod postacią."""
    BG, PANEL = (46, 44, 58), (60, 58, 74)
    Z, C, CELL = 2, 72, 150   # podgląd w skali Godota x2, kanwa 72x72 z bohaterem 32x32 w środku
    rows_by_id = {w["id"]: w for w in man["weapons"]}
    heroes = hero_frames()
    left = 170
    W_ = left + 2 * (4 * CELL + 16) + 10
    H_ = 54 + len(heroes) * 4 * CELL
    sheet = Image.new("RGBA", (W_, H_), BG + (255,))
    d = ImageDraw.Draw(sheet)
    for half, title in enumerate(("actors.png (z narzędziem zawodu)", "bez narzędzia zawodu")):
        x0 = left + half * (4 * CELL + 16)
        d.text((x0, 6), title, font=f_head, fill=(220, 220, 230))
        for di, dname in enumerate(DIRS):
            d.text((x0 + di * CELL + 60, 28), dname, font=f_small, fill=(170, 170, 185))
    for hi, (cid, cname, wid, real, bare) in enumerate(heroes):
        w = rows_by_id[wid]
        rars = [RAR_KEYS.index(k) for k in w["rarities"]]
        for ri, r in enumerate(range(4)):
            y = 50 + (hi * 4 + ri) * CELL
            d.rectangle((4, y + 2, W_ - 4, y + CELL - 4), fill=PANEL)
            y -= 2
            if ri == 0:
                d.text((10, y + 10), cname, font=f_name, fill=(240, 240, 245))
                d.text((10, y + 32), wid, font=f_small, fill=(170, 170, 185))
            d.text((10, y + 70), RARITIES[r][0], font=f_small, fill=RARITIES[r][3])
            if r not in rars:
                continue
            for half, hero in enumerate((real, bare)):
                for di, dname in enumerate(DIRS):
                    cell = Image.new("RGBA", (C, C), (0, 0, 0, 0))
                    hx, hy = 20, 20
                    h_img = hero.transpose(Image.FLIP_LEFT_RIGHT) if dname == "left" else hero
                    hand = HAND_FLIPPED if dname == "left" else HAND
                    gx, gy = w["gripGodot"][dname]
                    fr = inhand2.crop(((r * 4 + di) * 32, w["row"] * 32, (r * 4 + di) * 32 + 32, w["row"] * 32 + 32))
                    pos = (hx + hand[0] * 2 - gx, hy + hand[1] * 2 - gy)
                    if dname == "up":
                        cell.alpha_composite(fr, pos)
                        cell.alpha_composite(h_img, (hx, hy))
                    else:
                        cell.alpha_composite(h_img, (hx, hy))
                        cell.alpha_composite(fr, pos)
                    x = left + half * (4 * CELL + 16) + di * CELL
                    sheet.alpha_composite(cell.resize((C * Z, C * Z), Image.NEAREST), (x, y))
    sheet.save(os.path.join(OUT, "preview_hero.png"))


if __name__ == "__main__":
    main()
