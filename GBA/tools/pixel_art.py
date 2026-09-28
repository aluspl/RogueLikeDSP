"""Pixel-art postaci 16x16 jako mapy znaków (czytelne i łatwe do poprawiania w edytorze tekstu).

Znaki -> indeksy palety SPR_PAL z make_assets.py:
  .  przezroczysty   K obrys (ciemny)   W biały      S skóra      O pomarańcz (akcent)
  N  ciemny fiolet   B niebieski        Y żółty      R czerwony   G zielony
  D  ciemna zieleń   T brąz             g szary      l jasnoszary C cyjan      P fiolet
  H / V  kolor kasku / kamizelki fachowca (podstawiane)
Każda postać ma 2 klatki (A, B) do animacji: fachowcy przestawiają nogi, problemy "oddychają".
"""

CODES = {".": 0, "K": 1, "W": 2, "S": 3, "O": 4, "N": 5, "B": 6, "Y": 7, "R": 8, "G": 9,
         "D": 10, "T": 11, "g": 12, "l": 13, "C": 14, "P": 15}


def parse(rows, subst=None):
    assert len(rows) == 16, len(rows)
    out = []
    for r in rows:
        assert len(r) == 16, (len(r), r)
        for ch in r:
            if subst and ch in subst:
                ch = subst[ch]
            out.append(CODES[ch])
    return out


# ------------------------------------------------------------------ fachowiec (wspólna sylwetka)
WORKER_TOP = [
    "................",
    ".....KKKKKK.....",
    "....KHHWHHHK....",
    "...KHHWHHHHHK...",
    "...KKKKKKKKKK...",
    "....KSSSSSSK....",
    "....KSKSSKSK....",
    "....KSSSSSSK....",
    ".....KSSSSK.....",
    "...KKVVVVVVKK...",
    "..KSKYYYYYYKSK..",
    "..KSKVVVVVVKSK..",
    "...K.KVVVVK.K...",
]
WORKER_LEGS_A = [
    ".....KNNNNK.....",
    ".....KNKKNK.....",
    ".....KKK.KKK....",
]
WORKER_LEGS_B = [
    ".....KNNNNK.....",
    "....KNK..KNK....",
    "....KKK..KKK....",
]


def worker(helmet, vest, frame):
    rows = WORKER_TOP + (WORKER_LEGS_A if frame == 0 else WORKER_LEGS_B)
    return parse(rows, {"H": helmet, "V": vest})


# Narzędzia w prawej ręce: lista (x, y, znak) nakładana na sylwetkę.
TOOLS = {
    "log":     [(12, 8, "W"), (13, 8, "W"), (14, 8, "W"), (12, 9, "W"), (13, 9, "g"), (14, 9, "W"),
                (12, 10, "W"), (13, 10, "W"), (14, 10, "W"), (15, 9, "K")],                         # dziennik
    "trowel":  [(13, 9, "l"), (14, 9, "l"), (15, 10, "l"), (14, 10, "l"), (13, 10, "l"), (12, 11, "T")],  # kielnia
    "nailgun": [(12, 8, "Y"), (13, 8, "Y"), (14, 8, "Y"), (15, 8, "K"), (13, 9, "K"), (13, 10, "K")],     # gwoździarka
    "tester":  [(13, 6, "R"), (13, 7, "Y"), (13, 8, "Y"), (13, 9, "Y"), (13, 10, "K")],                   # próbnik
    "wrench":  [(12, 11, "g"), (13, 10, "g"), (14, 9, "g"), (14, 8, "g"), (15, 8, "g"), (15, 7, "K")],    # klucz
    "grinder": [(12, 9, "l"), (13, 9, "l"), (14, 8, "g"), (15, 8, "g"), (14, 10, "g"), (15, 10, "g"),
                (15, 9, "Y")],                                                                             # szlifierka
    "tile":    [(12, 7, "K"), (13, 7, "K"), (14, 7, "K"), (15, 7, "K"), (12, 8, "R"), (13, 8, "O"), (14, 8, "R"),
                (15, 8, "K"), (12, 9, "R"), (13, 9, "R"), (14, 9, "O"), (15, 9, "K"), (13, 10, "K"), (14, 10, "K")],  # dachówka
    "hawk":    [(11, 8, "W"), (12, 7, "W"), (13, 7, "W"), (14, 7, "W"), (12, 8, "W"), (13, 8, "W"), (14, 8, "W"),
                (15, 8, "K"), (11, 9, "g"), (12, 9, "g"), (13, 9, "g"), (14, 9, "g"), (15, 9, "K"), (13, 10, "T"),
                (13, 11, "T")],                                                                            # paca z tynkiem
    "bucket":  [(12, 7, "K"), (13, 7, "Y"), (14, 7, "Y"), (15, 7, "K"), (12, 8, "Y"), (13, 8, "Y"), (14, 8, "Y"),
                (15, 8, "K"), (12, 9, "Y"), (13, 9, "Y"), (14, 9, "Y"), (15, 9, "K"), (12, 10, "l"), (13, 10, "K"),
                (14, 10, "l"), (15, 10, "K")],                                                             # łyżka koparki z zębami
}

# (kask, kamizelka, narzędzie) w kolejności zawodów z data/game.json
WORKERS = [("W", "O", "log"), ("O", "B", "trowel"), ("Y", "T", "nailgun"),
           ("B", "N", "tester"), ("R", "B", "wrench"), ("G", "g", "grinder"),
           ("C", "R", "tile"), ("l", "P", "hawk"), ("D", "O", "bucket")]   # v0.21.49: Dekarz, Tynkarz, Operator koparki


def worker_frame(index, frame):
    helmet, vest, tool = WORKERS[index]
    px = worker(helmet, vest, frame)
    for x, y, ch in TOOLS[tool]:
        px[y * 16 + x] = CODES[ch]
    return px


# ------------------------------------------------------------------ problemy budowy (klatka A; B = oddech)
ENEMIES = {
    "przeciek": [
        "................",
        ".......KK.......",
        "......KCCK......",
        "......KCCK......",
        ".....KCWCCK.....",
        ".....KCWCCK.....",
        "....KCCCCCCK....",
        "...KCCCCCCCCK...",
        "...KCKCCCCKCK...",
        "...KCKCCCCKCK...",
        "...KCCCCCCCCK...",
        "...KBCCKKCCBK...",
        "....KBCCCCBK....",
        ".....KBBBBK.....",
        "......KKKK......",
        "................"],
    "zwarcie": [
        ".........KK.....",
        "........KYK.....",
        ".......KYYK.....",
        "......KYYK......",
        ".....KYWYK......",
        "....KYYYYKKKK...",
        "...KYKYYKYYYK...",
        "..KYYYYYYYYK....",
        "...KKKKYYYK.....",
        "......KYYK......",
        ".....KYOK.......",
        "....KYOK........",
        "....KOK.........",
        "...KOK..........",
        "...KK...........",
        "................"],
    "plesn": [
        "................",
        "................",
        "......KKKK......",
        "....KKGGGGKK....",
        "...KGGDGGGGGK...",
        "..KGGGGGGDGGGK..",
        "..KGKKGGGKKGGK..",
        "..KGWKGGGWKGGK..",
        ".KGGGGGGGGGGDGK.",
        ".KGDGGGKKGGGGGK.",
        ".KGGGGGGGGGDGGK.",
        "..KDGGGDGGGGGK..",
        "..KKDDDDDDDDKK..",
        "...KKKKKKKKKK...",
        "................",
        "................"],
    "kornik": [
        "................",
        ".....K....K.....",
        "......K..K......",
        ".....KKKKKK.....",
        "....KKWKKWKK....",
        "....KKKKKKKK....",
        "...KTTTTKTTTTK..",
        "..KTTlTTKTTlTK..",
        ".KKTTTTTKTTTTKK.",
        "..KTTTTTKTTTTK..",
        ".KKTTlTTKTTlTKK.",
        "..KTTTTTKTTTTK..",
        "...KTTTTKTTTK...",
        "....KKKKKKKK....",
        "...K........K...",
        "................"],
    "papierologia": [
        "................",
        ".......KKKKKKK..",
        ".......KWWWWWK..",
        ".....KKKKKKKWK..",
        ".....KWWWWWKWK..",
        "...KKKKKKKKWWK..",
        "...KWWWWWWKWWK..",
        "...KWggggWKKK...",
        "...KWWWWWWK.....",
        "...KWKWWKWK.....",
        "...KWWWWWWK.....",
        "...KWWRRWWK.....",
        "...KWWWWWWK.....",
        "...KKKKKKKK.....",
        "................",
        "................"],
    "dostawa": [
        "................",
        "................",
        "..KKKKKKKK......",
        "..KOOOOOOK......",
        "..KOWWKWOKKKK...",
        "..KOWKWWOKllK...",
        "..KOWWWWOKlBBK..",
        "..KOOOOOOKlBBK..",
        "..KOOOOOOKllllK.",
        "..KKKKKKKKKKKKK.",
        "...KKK....KKK...",
        "..KgggK..KgggK..",
        "..KgKgK..KgKgK..",
        "...KKK....KKK...",
        "................",
        "................"],
    "ulewa": [
        "................",
        ".....KKKK.......",
        "...KKllllKKK....",
        "..KllllllllKK...",
        ".KllWllllllllK..",
        ".KlllKllllKllK..",
        ".KlllKllllKllK..",
        ".KllllllKllllK..",
        "..KggggggggggK..",
        "...KKKKKKKKKK...",
        "....B...B...B...",
        "...B...B...B....",
        "....B...B...B...",
        "...B...B...B....",
        "................",
        "................"],
    "budzet": [
        "................",
        "......KKKK......",
        ".....KTTTTK.....",
        "......KTTK......",
        "....KKYYYYKK....",
        "...KYYYYYYYYK...",
        "..KYYKYYYYKYYK..",
        "..KYYKYYYYKYYK..",
        "..KYYYYTTYYYYK..",
        "..KYYYTYYTYYYK..",
        "..KYYYYTTTYYYK..",
        "..KYYYYYYYTYYK..",
        "...KYYYTTTYYK...",
        "....KKYYYYKK....",
        "......KKKK......",
        "................"],
    "termin": [
        "..KKK......KKK..",
        "..KgK......KgK..",
        "...KKKKKKKKKK...",
        "..KRRRRRRRRRRK..",
        ".KRRWWWWWWWWRRK.",
        ".KRWWWWKWWWWWRK.",
        ".KRWWWWKWWWWWRK.",
        ".KRWKWWKWWWKWRK.",
        ".KRWWWWKKKKWWRK.",
        ".KRWWWWWWWWWWRK.",
        ".KRWWRWWWWRWWRK.",
        ".KRRWWRRRRWWRRK.",
        "..KRRWWWWWWRRK..",
        "...KKRRRRRRKK...",
        "..KK..KKKK..KK..",
        "................"],
}
ENEMY_ORDER = ["przeciek", "zwarcie", "plesn", "kornik", "papierologia", "dostawa", "ulewa", "budzet", "termin"]


def enemy_frame(index, frame):
    rows = ENEMIES[ENEMY_ORDER[index]]
    px = parse(rows)
    if frame == 1:   # "oddech": wszystko 1 px w dół, dolny rząd przycięty
        px = [0] * 16 + px[:16 * 15]
    return px


# ------------------------------------------------------------------ v0.21.49: problemy etapów (klatki 61-80, druga 81-100)
# Kolejność jak nowe wrogi w data/game.json (od "woda"); klatka B = oddech jak u pozostałych.
STAGE_ENEMIES = {
"woda": [
"................","................","......KKKK......",".....KCCCCK.....","....KCWCCCCK....","....KCCCCCCK....",
"...KCCKCCKCCK...","...KCCKCCKCCK...","...KCCCCCCCCK...","..KCCCBBBBCCCK..",".KCCBCCCCCCBCCK.",".KBCCCCCCCCCCBK.",
"KTTKBBBBBBBBKTTK","KTTTKKKKKKKKTTTK",".KTTTTTTTTTTTTK.","..KKKKKKKKKKKK.."],
"kamien": [
"................","................","................",".....KKKKKK.....","...KKlllgggKK...","..KllllggggggK..",
"..KlWllgggggggK.",".KlllKKgggKKggK.",".KlllgggggggggK.",".KllggggKKKgggK.",".KgggggggggggDK.",".KggggggggggDDK.",
"..KgggDgggggDK..","...KKKKKKKKKK...","..TTTTTTTTTTTT..","................"],
"osuwisko": [
"................","................","........KK......",".......KTTK.....","......KTTTTK....",".....KTWKTTTK...",
"....KTTKKTWKTK..","...KTTTTTTKKTTK.","..KTTgTTTTTTTTK.",".KTTTTTKKKTTgTTK","KTTTgTTTTTTTTTTK","KKKKKKKKKKKKKKKK",
"l.l...l...l.....","..l.l...l...l...","................","................"],
"folia": [
"................","..KKKKKKKKKKKK..","..KNNNNNNNNNNK..","..KNWKNNNNWKNK..","..KNKKNNNNKKNK..","..KNNNN..NNNNK..",
"..KNNN....NNNK..","..KNNNN..NNNNK..","..KNYYYNNNNNNK..","..KNYYYNNNNNNK..","..KNNNNNNNKKNK..","..KNNNNNNNK.KK..",
"..KNNNNNNNNNNK..","...KNNKNNNKNNK..","....KK.KKK.KK...","................"],
"krzywy_mur": [
"................","....KKKKKKKK....","....KRRKORRK....","...KKKKKKKKKK...","...KORRKRRORK...","..KKKKKKKKKKK...",
"..KRWKRRWKRRK...","..KRKKRRKKRRK...",".KKKKKKKKKKKK...",".KORRKRRORRKK...",".KKKKKKKKKKKK...","KRRORKRRRKRRK...",
"KKKKKKKKKKKKK...","TTTTTTTTTTTTTT..","................","................"],
"mostek": [
"................","...R....R.......","....R..R..R.....","...R....R.......","..KKKKKKKKKKKK..","..KBBBBBBBBBBK..",
"..KBWKBBBBWKBK..","..KBKKBBBBKKBK..","..KCCCCCCCCCCK..","..KCCCKKKKCCCK..","..KCCCCCCCCCCK..","..KBBBBBBBBBBK..",
"..KKKKKKKKKKKK..","...W...W...W....","....W...W...W...","................"],
"ugiecie": [
"................","................","KKK..........KKK","KllKK......KKllK","KlllllKKKKKllllK",".KllllllllllllK.",
".KlllWKllWKlllK.","..KllKKllKKllK..","..KllllllllllK..","...KlllKKlllK...","...KllKllKllK...","....KKllllKK....",
"......KKKK......","......g..g......","................","................"],
"zbrojenie": [
"................","....K......K....","...KOK....KOK...","...KOK....KOK...","..KKKKKK.KKKKK..","..KlllllKKlllK..",
"..KlWKllKlWKlK..","..KlKKlKllKKlK..","..KllllKlllllK..","..KlllKllllllK..","..KllllKllKKlK..","..KlllllKlllK...",
"..KKKKKK.KKKKK..","................","................","................"],
"papa": [
"................","..KKKKKKKKKKKK..",".KNNNNNNNNNNNNK.",".KNgNNNNNNNNgNK.",".KNWKNNNNNWKNNK.",".KNKKNNNNNKKNNK.",
".KNNNNNKKNNNNNK.","..KKKKKKKKKKKK..","...KC....KC.....","...KC.....KC....","....C..C...C....","..C...KCK.......",
".....KCCCK..C...","......KKK.......","................","................"],
"rynna": [
"................","......G..D......","....DGGDGGDG....","KKKKGDGGDGDGKKKK","KlllKGDKKDGKlllK","KlllllllllllllgK",
"KgWKllllllWKllgK",".KKKllllllKKlgK.","..KgllllllllgK..","...KKKKKKKKKK...","....C....C......","....C..C.C..C...",
"...CCC.C...CCC..","....C...........","................","................"],
"pustak": [
"................","................","..KKKKKKKKKKKK..","..KOOOOOKOOOOK..","..KOKKOKOKKOOK..","..KOKKOOKKKOOK..",
"..KOOOOKOOOOOK..","..KRWKRRKRWKRK..","..KRKKRKRRKKRK..","..KRRRRRKRRRRK..","..KRRKKRRKRRRK..","..KRRRRKRRRRRK..",
"..KKKKKKKKKKKK..","...Y..Y...Y.....","....Y....Y......","................"],
"wymiarowka": [
"................","................","...KKKKKKK......","..KYYYYYYYK.....","..KYWKYWKYK.....","..KYKKYKKYK.....",
"..KYYYYYYYKKKKKK","..KYYKKKYYYYKYYK","..KYYYYYYYKKKKKK","..KgggggggK.....","...KKKKKKK......","....K...K.......",
"................","................","................","................"],
"ramka": [
"..KKKKKKKKKKKK..","..KWWWWWWWWWWK..","..KWCCCCKCCCWK..","..KWCWKCKCWKWK..","..KWCKKCKCKKWK..","..KWCCCCKCCCWK..",
"..KWKKKKKKKKWK..","..KWCCCCKCCCWK..","..KWCCKKKKCCWK..","..KWCCCCKCCCWK..","..KWWWWWWWWWWK..","..KKKKKKKKKKKK..",
"l..............l",".l............l.","................","................"],
"przeciag": [
"................","......KKKKK.....","....KKlllllKK...","...KllWWlllllK..","..KllWKlllWKlK..","..KlllKKllKKlK..",
"..KllllllllllK..","...KlllKKKllK...","....KllllllK....",".....KlllllK....","....KlllKKK.....","...KllK.........",
"..KlK...........","..KK............","................","................"],
"zapowietrzenie": [
"................","................","......KKKK......","....KKWWCCKK....","...KWWCCCCCCK...","...KWCWKCWKCK...",
"...KCCKKCKKCK...","...KCCCCCCCCK...","...KCCCKKCCCK...","....KKCCCCKK....","KKKKKKKKKKKKKKKK","gggggggggggggggg",
"llllllllllllllll","KKKKKKKKKKKKKKKK","................","................"],
"uziemienie": [
"................","..Y.........Y...","...Y..KKKK.Y....","....KKggggKK....","...KggggggggK...","...KgWKggWKgK...",
"...KgKKggKKgK...","...KggggggggK...","...KggKKKKggK...","...KgKOKKOKgK...","...KggggggggK...","....KKKKKKKK....",
"......KYK.......",".....KYK........","....KYK.........","....KK.........."],
"rysa": [
"................","..KKKKKKKKKKKK..","..KlWWWWKWWWWK..","..KWWWWKWWWWlK..","..KWKWWWKWWKWK..","..KWWWWKWWWWWK..",
"..KWWWWWKWWWWK..","..KWWKKWWKKWWK..","..KWWWKKKKWWWK..","..KlWWWWKWWWWK..","..KWWWWKWWWWlK..","..KKKKKKKKKKKK..",
"................","................","................","................"],
"wilgoc": [
"................","................",".....KKKKKK.....","...KKBBBBBBKK...","..KBBNBBBBNBBK..","..KBBBBBBBBBBK..",
".KBBWKBBBBWKBBK.",".KBBKKBBBBKKBBK.",".KBBBBBGBBBBBBK.",".KBNBBBBBBBBNBK.","..KBBBKKKKBBBK..","..KBBBBBBBBBBK..",
"...KBKBBBKBKK...","....K.KBK.K.....",".......C........","................"],
"odpryski": [
"................",".l.........W....","...W....l.......","....KKKKKKKK..l.","....KCCCCCCK....","...KCWKCCWKCK...",
"...KCKKCCKKCK...","...KCCCCCCCCK...","...KCCKKKKCCK...","...KCCCCCCCK....","....KCCCCCK.....",".....KKKKK..W...",
"..W.............","........l.......","................","................"],
"poprawki": [
"................","...KKKKKKKKK....","...KYYYYYYYKK...","...KYRYYYRYYK...","...KYYRYRYYYK...","...KYYYRYYYYK...",
"...KYYRYRYYYK...","...KYRYYYRYYK...","...KYYYYYYYYK...","...KYWKYYWKYK...","...KYKKYYKKYK...","...KYYYYYYYYK...",
"...KYYKKKKYYK...","...KKKKKKKKKK...","................","................"],
}
STAGE_ENEMY_ORDER = list(STAGE_ENEMIES)


def stage_enemy_frame(index, frame):
    px = parse(STAGE_ENEMIES[STAGE_ENEMY_ORDER[index]])
    if frame == 1:
        px = [0] * 16 + px[:16 * 15]
    return px


# ------------------------------------------------------------------ znajdźki
PICKUPS = {
    "coffee": [
        "................",
        "......W..W......",
        ".......W..W.....",
        "......W..W......",
        "....KKKKKKK.....",
        "....KllllllKK...",
        "....KTTTTTTK.K..",
        "....KWWWWWWK.K..",
        "....KWWOOWWKK...",
        "....KWWOOWWK....",
        "....KWWWWWWK....",
        "....KllllllK....",
        ".....KKKKKK.....",
        "................",
        "................",
        "................"],
    "helmet": [
        "................",
        "................",
        "................",
        "......KKKK......",
        "....KKOWOOKK....",
        "...KOOWOOOOOK...",
        "..KOOWOOOOOOOK..",
        "..KOOOOYYOOOOK..",
        "..KOOOOOOOOOOK..",
        ".KKKKKKKKKKKKKK.",
        ".KOOOOOOOOOOOOK.",
        "..KKKKKKKKKKKK..",
        "................",
        "................",
        "................",
        "................"],
    "plan": [
        "................",
        "................",
        "..KKKKKKKKKKK...",
        "..KBBBBBBBBBKK..",
        "..KBWWWWWWWBKK..",
        "..KBWBBBBBWBKK..",
        "..KBWBWWWBWBKK..",
        "..KBWBWBWBWBKK..",
        "..KBWWWBWWWBKK..",
        "..KBBBBBBBBBKK..",
        "..KBWWWBBBBBKK..",
        "..KBBBBBBBBBKK..",
        "..KKKKKKKKKKKK..",
        "...KKKKKKKKKKK..",
        "................",
        "................"],
    "toolbox": [
        "................",
        "................",
        "................",
        "......KKKK......",
        ".....KggggK.....",
        ".....Kg..gK.....",
        "..KKKKKKKKKKKK..",
        "..KRRRRRRRRRRK..",
        "..KRRRRYYRRRRK..",
        "..KKKKKYYKKKKK..",
        "..KRRRRRRRRRRK..",
        "..KRRRRRRRRRRK..",
        "..KRRRRRRRRRRK..",
        "..KKKKKKKKKKKK..",
        "................",
        "................"],
}


def pickup_frame(name):
    return parse(PICKUPS[name])


# ------------------------------------------------------------------ cząsteczki 8x8
# klatki: 0-2 pył (duży -> mały), 3-4 iskra, 5-8 konfetti (4 kolory), 9 gwiazdka awansu
PARTICLES = [
    ["........", "..llll..", ".llllll.", ".lllgll.", ".llgggl.", "..gggg..", "........", "........"],
    ["........", "........", "..lll...", ".llgll..", "..ggg...", "........", "........", "........"],
    ["........", "........", "........", "...ll...", "...gl...", "........", "........", "........"],
    ["...Y....", "...Y....", ".YYWYY..", "...Y....", "...Y....", "........", "........", "........"],
    ["........", "..O.O...", "...W....", "..O.O...", "........", "........", "........", "........"],
    ["........", "..OOO...", "..OOO...", "........", "........", "........", "........", "........"],
    ["........", "..PPP...", "..PPP...", "........", "........", "........", "........", "........"],
    ["........", "..CC....", "..CCC...", "...C....", "........", "........", "........", "........"],
    ["........", "..GGG...", "...GG...", "........", "........", "........", "........", "........"],
    ["...W....", "..WYW...", ".WYYYW..", "..WYW...", "...W....", "........", "........", "........"],
    # 10-11 krąg megafonu (Odprawa)
    ["........", "...PP...", "..P..P..", ".P....P.", ".P....P.", "..P..P..", "...PP...", "........"],
    ["..PPPP..", ".P....P.", "P......P", "P......P", "P......P", "P......P", ".P....P.", "..PPPP.."],
    # 12 cegła (Ścianka)
    ["........", "KKKKKKK.", "KOROROK.", "KRRKRRK.", "KKKKKKK.", "........", "........", "........"],
    # 13 gwóźdź (Seria)
    ["........", "........", "g.......", "glllllll", "g.......", "........", "........", "........"],
    # 14-15 piorun (Łańcuch)
    ["...CW...", "..CW....", ".CWWC...", "...WC...", "..WC....", ".WC.....", "........", "........"],
    ["....WC..", "...WC...", "..CWWC..", "....WC..", "...WC...", "..WC....", "........", "........"],
    # 16 kropla wody, 17 zielony plus (Zawór)
    ["...C....", "..CC....", ".CCWC...", ".CCCC...", "..CC....", "........", "........", "........"],
    ["...G....", "...G....", ".GGGGG..", "...G....", "...G....", "........", "........", "........"],
    # 18 "z" ogłuszenia
    ["........", ".WWWW...", "...W....", "..W.....", ".WWWW...", "........", "........", "........"],
    # 19 "!" - wróg Cię zauważył
    ["..KKK...", "..KRK...", "..KRK...", "..KRK...", "..KKK...", "..KRK...", "..KKK...", "........"],
    # 20 strzałka nad oznaczonym celem (krótkie A trafi właśnie jego)
    ["KKKKKKK.", "KYYYYYK.", ".KYYYK..", "..KYK...", "...K....", "........", "........", "........"],
    # 21-23 ikony stanów nad bohaterem: zatrucie, porażenie, poślizg
    [".KKKK...", "KGGDGK..", "KGDGGK..", "KGGGDK..", ".KGGK...", "..KK....", "........", "........"],
    ["...KYK..", "..KYK...", ".KYYYK..", "..KYK...", ".KYK....", ".KK.....", "........", "........"],
    ["........", ".C...C..", "C.C.C.C.", "...C...C", "........", "CCCCCCC.", "........", "........"],
]


def particle_frames():
    out = []
    for rows in PARTICLES:
        assert len(rows) == 8 and all(len(r) == 8 for r in rows), rows
        out += [CODES[ch] for r in rows for ch in r]
    return out


# ------------------------------------------------------------------ Osiedle: domy 16x16
# klatka = wielkość * 9 + zawód (dach w kolorze kasku zawodu), 36 = pusta działka
HOUSE_SMALL = [
    "................",
    "................",
    "................",
    "................",
    "................",
    ".......KK.......",
    "......KHHK......",
    ".....KHHHHK.....",
    "....KHHHHHHK....",
    "...KKKKKKKKKK...",
    "....KWWWWWWK....",
    "....KWBKWWWK....",
    "....KWBKWTWK....",
    "....KWWWWTWK....",
    "....KKKKKKKK....",
    "..GGGGGGGGGGGG.."]
HOUSE_TALL = [
    "................",
    "................",
    "......KKKK......",
    ".....KHHHHK.....",
    "....KHHHHHHK....",
    "...KHHHHHHHHK...",
    "..KKKKKKKKKKKK..",
    "...KWWWWWWWWK...",
    "...KWBKWWBKWK...",
    "...KWBKWWBKWK...",
    "...KWWWWWWWWK...",
    "...KWBKWWTTWK...",
    "...KWBKWWTTWK...",
    "...KWWWWWTTWK...",
    "...KKKKKKKKKK...",
    ".GGGGGGGGGGGGGG."]
EMPTY_PLOT = [
    "................", "................", "................", "................", "................",
    "................", "................", "................", "................", "................",
    "................", "..T.T.T.T.T.T...", "..T.T.T.T.T.T...", "..TTTTTTTTTTT...", "..T.T.T.T.T.T...",
    "..DDDDDDDDDDDD.."]


def house_frame(cls, size):
    helmet = WORKERS[cls][0]
    rows = [r.replace("H", helmet) for r in (HOUSE_SMALL if size < 2 else HOUSE_TALL)]
    px = parse(rows)
    if size in (1, 3):   # komin / garaż jako dodatek
        for x, y in ((11, 3), (11, 4), (12, 3), (12, 4)) if size == 1 else ((13, 11), (14, 11), (13, 12), (14, 12), (13, 13), (14, 13)):
            px[y * 16 + x] = CODES["K" if size == 1 else "g"]
    return px


def house_frames():   # klatka = wielkość * liczba zawodów + zawód; ostatnia = pusta działka
    return [p for size in range(4) for cls in range(len(WORKERS)) for p in house_frame(cls, size)] + parse(EMPTY_PLOT)


# ------------------------------------------------------------------ ikony mocy do HUD (kolejność zawodów)
ABILITY_ICONS = [
    [   # Odprawa: megafon
        "................", "................", "...........KK...", ".........KKWK...", "......KKKWWWK...",
        "..KKKKWWWWWWK...", "..KPPKWWWWWWK.P.", "..KPPKWWWWWWK..P", "..KPPKWWWWWWK.P.", "..KKKKWWWWWWK...",
        "....KKKKWWWWK...", "....KTK.KKWWK...", "....KTK...KKK...", "....KKK.........", "................", "................"],
    [   # Ścianka: cegły
        "................", "................", "................", ".KKKKKKKKKKKKKK.", ".KOORRKOORRKOOK.",
        ".KRRRRKRRRRKRRK.", ".KKKKKKKKKKKKKK.", ".KOOKOORRKOORRK.", ".KRRKRRRRKRRRRK.", ".KKKKKKKKKKKKKK.",
        ".KOORRKOORRKOOK.", ".KRRRRKRRRRKRRK.", ".KKKKKKKKKKKKKK.", "................", "................", "................"],
    [   # Seria: trzy gwoździe
        "................", "................", "..g.............", "..glllllllll....", "..g.............",
        "................", "....g...........", "....glllllllll..", "....g...........", "................",
        "..g.............", "..glllllllll....", "..g.............", "................", "................", "................"],
    [   # Łańcuch: piorun
        "................", ".........KKK....", "........KYYK....", ".......KYYK.....", "......KYYK......",
        ".....KYYYKKKK...", "....KYYYYYYYK...", "...KKKKYYYYK....", "......KYYYK.....", ".....KYYK.......",
        "....KYYK........", "....KYK.........", "...KYK..........", "...KK...........", "................", "................"],
    [   # Zawór: kurek z kroplą
        "................", "....KKKKKKK.....", "....KRRRRRK.....", "....KKKKKKK.....", ".......K........",
        "..KKKKKgKKKKKK..", "..KllllgllllgK..", "..KKKKKKKKKKgK..", "...........KgK..", "...........KKK..",
        "............C...", "...........CCC..", "...........CWC..", "............C...", "................", "................"],
    [   # Wirówka: tarcza szlifierki
        "................", "......K..K......", "....KKllllKK....", "...KllllllllK...", "..KlllggggllK...",
        ".KllgglllgglK...", ".KllglKKKlglK...", ".KllglKWKlglK...", ".KllglKKKlglK...", ".KllgglllgglK...",
        "..KlllggggllK...", "...KllllllllK...", "....KKllllKK....", "......K..K......", "................", "................"],
    [   # Rynna: dachówki lecą w linii
        "................", "...........KKKK.", "..........KROROK", "..........KRRRRK", "...........KKKK.",
        "......KKKK......", ".....KROROK.....", ".....KRRRRK.....", "......KKKK......", "..KKKK..........",
        ".KROROK.........", ".KRRRRK.........", "..KKKK..........", "................", "l.l.l.l.........", "................"],
    [   # Narzut: kielnia i chlapnięcie tynku
        "..W......W......", "....W..W....W...", ".W..KKKKK..W....", "...KWWWWWK......", "..KWWWWWWWK..W..",
        "W.KWWlWWWWK.....", "..KWWWWWlWK.W...", "...KWWWWWK......", "....KKKKK..W....", "..W...KgK.......",
        "......KgK...W...", ".....KgggK......", "....KggggK......", "....KKKKKK......", ".......KTK......", ".......KKK......"],
    [   # Taran: łyżka koparki z ramieniem, pęd z lewej
        "................", "................", ".....KKKK.......", "....KgggK.......", ".....KKgK.......",
        "......KgKKKKK...", "l.l..KYYYYYYYK..", ".....KYYYYYYYYK.", "llll.KYYYYYYYYK.", ".....KYYYYYYYKlK",
        "l.l..KKYYYYYKlK.", "......KKKKKKlK..", "................", "................", "................", "................"],
]


def ability_icon_frames():
    return [p for rows in ABILITY_ICONS for p in parse(rows)]


# ------------------------------------------------------------------ paczki ze sprzętem (drop): zwykły, solidny, markowy
GEAR_BOX = [
    "................",
    "................",
    ".....KKKKKK.....",
    "....KAAAAAAK....",
    "...KAAAAAAAAK...",
    "..KKKKKKKKKKKK..",
    "..KAAAAWWAAAAK..",
    "..KAAAAWWAAAAK..",
    "..KAAAAAAAAAAK..",
    "..KAAAKKKKAAAK..",
    "..KAAAKYYKAAAK..",
    "..KAAAKKKKAAAK..",
    "..KAAAAAAAAAAK..",
    "..KKKKKKKKKKKK..",
    "................",
    "................"]


def gear_frames():
    return [p for color in ("g", "B", "O") for p in parse(GEAR_BOX, {"A": color})]


# ------------------------------------------------------------------ celownik (klatka 45 aktorów)
RETICLE = [
    "KKKKK....KKKKK..", "KYYYK....KYYYK..", "KYKK......KKYK..", "KYK........KYK..", "KKK........KKK..",
    "................", "................", "................", "................", "................",
    "KKK........KKK..", "KYK........KYK..", "KYKK......KKYK..", "KYYYK....KYYYK..", "KKKKK....KKKKK..", "................"]


def reticle_frame():
    rows = ["." + r[:15] for r in RETICLE]   # wyśrodkowanie
    return parse(rows)


# ------------------------------------------------------------------ bossowie (klatki 46-47, druga klatka 48-49; Inspekcja 50-51)
BOSSES = {
    "betoniarka": [
        "................",
        "......KKKKK.....",
        ".....KOOOOOK....",
        "....KOOKOOOOK...",
        "...KOOOOKOOOOK..",
        "...KOKOOOOKOOK..",
        "...KOOOKOOOOOK..",
        "....KOOOOKOOK...",
        ".....KOOOOOKgK..",
        "......KKKKKgK...",
        "..KKKKKKKKKKKK..",
        "..KggggggggggK..",
        "..KgWKgggWKggK..",
        "..KKKKKKKKKKKK..",
        "...KgK....KgK...",
        "....K......K...."],
    "nawalnica": [
        "................",
        "....KKKK..KKK...",
        "..KKggggKKgggK..",
        ".KggggggggggggK.",
        ".KggWKggggWKggK.",
        ".KggKKggggKKggK.",
        ".KgggggKKgggggK.",
        "..KKKKKKKKKKKK..",
        "...B..KYK..B....",
        "..B..KYYK.B.....",
        ".....KYK...B....",
        "....KYYK..B.....",
        "....KYK....B....",
        "...KYK.....B....",
        "...KK...........",
        "................"],
    # Inspekcja Pracy: podkładka z protokołem (surowe brwi), odlatująca kartka i pieczątka z czerwonym tuszem
    "inspekcja": [
        ".....KKKK..KKKK.",
        "....KgllgK.KWWWK",
        ".KKKKKKKKKKKWggK",
        ".KTTTTTTTTTKWWWK",
        ".KTWWWWWWWTKKKK.",
        ".KTWKKWKKWTK....",
        ".KTWWKWKWWTK....",
        ".KTWWKWKWWTK.KK.",
        ".KTWWWWWWWTKKTTK",
        ".KTWgggggWTK.KTK",
        ".KTWWWWWWWTK.KTK",
        ".KTWWWRRWWTKKKKK",
        ".KTWWRWWRWTKRRRK",
        ".KTWWWRRWWTKKKKK",
        ".KTTTTTTTTTK....",
        ".KKKKKKKKKKK...."],
}


def boss_frames():
    out = []
    for name in ("betoniarka", "nawalnica"):
        out.append(parse(BOSSES[name]))
    for name in ("betoniarka", "nawalnica"):   # druga klatka: "oddech" 1 px w dół
        px = parse(BOSSES[name]); out.append([0] * 16 + px[:16 * 15])
    px = parse(BOSSES["inspekcja"])            # 50-51: Inspekcja Pracy (klatka A, B)
    out += [px, [0] * 16 + px[:16 * 15]]
    return out


# ------------------------------------------------------------------ prolog: pickup PlanBudowlany 32x16 (2 klatki: koła)
TRUCK = [
    "................................",
    "................................",
    "..................KKKKKKKK......",
    ".................KPPPPPPPPK.....",
    "................KPPlllllPPPK....",
    "...............KPPPlllllPPPPK...",
    "..KKKKKKKKKKKKKKPPPPPPPPPPPPPK..",
    "..KPPPPPPPPPPPPPPPPPPPPPPPPPPPK.",
    "..KPPWWPPPPPPPPPPPPPPPPPPPPPPYK.",
    "..KOOOOOOOOOOOOOOOOOOOOOOOOOOOK.",
    "..KPPPPPPPPPPPPPPPPPPPPPPPPPPPK.",
    "..KKKKKKKKKKKKKKKKKKKKKKKKKKKKK.",
    ".....KKKK...............KKKK....",
    "....KgKKgK.............KgKKgK...",
    "....KKggKK.............KKggKK...",
    ".....KKKK...............KKKK...."]


def truck_frames():
    rows = [r for r in TRUCK]
    a = [CODES[ch] for r in rows for ch in r]
    b_rows = rows[:13] + ["....KKggKK.............KKggKK...", "....KgKKgK.............KgKKgK...", rows[15]]
    b = [CODES[ch] for r in b_rows for ch in r]
    assert len(a) == len(b) == 32 * 16
    return a + b


# ------------------------------------------------------------------ menu akcji pod START (16x16): atak, termos, czekaj, ramka wyboru;
# 4-5: mała kłódka i strzałki góra/dół (wybór zawodu); 6-10: pogoda dnia (Słonecznie, Upał, Mróz, Wiatr, Deszcz);
# 11-13: materiały w HUD (cement, stal, drewno - małe, w lewej części klatki, cyfra obok); 14: kalendarz (budowa dnia);
# 15-18: nagrody za odbiór (Młot udarowy, Pistolet do kotew, Buty robocze, Pas narzędziowy); 19: Respekt
MENU_ICONS = [
    [   # Atak: młotek
        "................", "................", "....KKKKKKK.....", "...KgllllllK....", "...KggggggKK....",
        "....KKKTKKK.....", "......KTK.......", "......KTK.......", "......KTK.......", "......KTK.......",
        "......KTK.......", "......KTK.......", ".....KTTTK......", ".....KKKKK......", "................", "................"],
    [   # Termos: stalowy termos z pomarańczową nakrętką
        "................", "......KKKK......", ".....KOOOOK.....", ".....KKKKKK.....", "....KllllllK....",
        "....KlWllggK....", "....KlWllggK....", "....KOOOOOOK....", "....KlWllggK....", "....KlWllggK....",
        "....KlWllggK....", "....KlllgggK....", "....KllllggK....", ".....KKKKKK.....", "................", "................"],
    [   # Czekaj: klepsydra
        "................", "...KKKKKKKKKK...", "...KTTTTTTTTK...", "....KYYYYYYK....", ".....KYYYYK.....",
        "......KYYK......", ".......KK.......", "......K..K......", ".....K..Y.K.....", "....K..YYY.K....",
        "...KYYYYYYYYK...", "...KTTTTTTTTK...", "...KKKKKKKKKK...", "................", "................", "................"],
    [   # ramka zaznaczenia
        "WWWW........WWWW", "WYY..........YYW", "WY............YW", "W..............W", "................",
        "................", "................", "................", "................", "................",
        "................", "................", "W..............W", "WY............YW", "WYY..........YYW", "WWWW........WWWW"],
    [   # mała kłódka w prawym dolnym rogu (zablokowany zawód na liście portretów)
        "................", "................", "................", "................", "................",
        "..........KKKK..", ".........KgKKgK.", ".........Kg..gK.", "........KKKKKKKK", "........KYYYYYYK",
        "........KYYKKYYK", "........KYYKKYYK", "........KYYYYYYK", "........KTTTTTTK", "........KKKKKKKK", "................"],
    [   # strzałki góra/dół (zmiana trudności)
        "................", "................", ".......KK.......", "......KPPK......", ".....KPPPPK.....",
        "....KPPPPPPK....", "....KKKPPKKK....", "......KPPK......", "......KPPK......", "....KKKPPKKK....",
        "....KPPPPPPK....", ".....KPPPPK.....", "......KPPK......", ".......KK.......", "................", "................"],
    [   # pogoda: Słonecznie (słońce)
        ".......Y........", "..Y....Y....Y...", "...Y.......Y....", "................", ".....KKKKKK.....",
        "....KYYYYYYK....", "...KYYWWYYYYK...", "YY.KYWYYYYYYK.YY", "...KYYYYYYYYK...", "...KYYYYYYYOK...",
        "....KYYYYYOK....", ".....KKKKKK.....", "................", "...Y.......Y....", "..Y....Y....Y...", ".......Y........"],
    [   # pogoda: Upał (termometr i fale gorąca)
        "......KKK.......", ".....KWWWK..O...", ".....KWlWK.O....", ".....KWlWK..O...", ".....KWRWK...O..",
        ".....KWRWK..O...", ".....KWRWK.O....", ".....KWRWK..O...", ".....KWRWK......", "....KWRRRWK.....",
        "...KWRRRRRWK....", "...KRRWRRRRK....", "...KRRRRRRRK....", "....KRRRRRK.....", ".....KKKKK......", "................"],
    [   # pogoda: Mróz (płatek śniegu)
        "................", ".......C........", "....C..C..C.....", ".....C.W.C......", "......CWC.......",
        "..C...CWC...C...", "...C..CWC..C....", ".CCWWWWWWWWWCC..", "...C..CWC..C....", "..C...CWC...C...",
        "......CWC.......", ".....C.W.C......", "....C..C..C.....", ".......C........", "................", "................"],
    [   # pogoda: Wiatr (smugi powietrza)
        "................", "..........KKK...", ".........KWWWK..", "..KKKKKKKK..WK..", ".KWWWWWWWWWWWK..",
        "..KKKKKKKKKKK...", "................", "...KKKKKKKKKK...", "..KllllllllllK..", "...KKKKKKKKK.lK.",
        "............KlK.", "...........KK...", ".KKKKKK.........", "KWWWWWWK........", ".KKKKKK.........", "................"],
    [   # pogoda: Deszcz (chmura i krople)
        "................", "......KKKK......", "....KKllllKK....", "...KllWWllllK...", ".KKlWlllllllKK..",
        "KllllllllllllgK.", "KlllllllllllggK.", ".KggggggggggggK.", "..KKKKKKKKKKKK..", "...B...B...B....",
        "..BB..BB..BB....", "..B...B...B.....", ".B...B...B......", "................", "..B...B...B.....", ".B...B...B......"],
    [   # materiał: cement (worek)
        "................", "................", "................", "..KKKKKKK.......", ".KllWlllgK......",
        ".KlllllggK......", ".KlKKKKlgK......", ".KlKTTKlgK......", ".KlKKKKlgK......", ".KlllllggK......",
        ".KgggggggK......", "..KKKKKKK.......", "................", "................", "................", "................"],
    [   # materiał: stal (pręty zbrojeniowe)
        "................", "................", "................", ".......KK.......", ".....KKCK.......",
        "...KKClKK.......", ".KKClKKCK.......", "KClKKClKK.......", "KKKClKKK........", ".KClKKK.........",
        "KClKK...........", "KKK.............", "................", "................", "................", "................"],
    [   # materiał: drewno (deski)
        "................", "................", "................", "KKKKKKKKKK......", "KOTTOTTTTK......",
        "KKKKKKKKKK......", ".KKKKKKKKKK.....", ".KTTOTTTOTK.....", ".KKKKKKKKKK.....", "KKKKKKKKKK......",
        "KTOTTTTOTK......", "KKKKKKKKKK......", "................", "................", "................", "................"],
    [   # kalendarz (codzienna budowa)
        "................", "...K......K.....", "..KKKKKKKKKKK...", "..KRRRRRRRRRK...", "..KRRRRRRRRRK...",
        "..KKKKKKKKKKK...", "..KWWWWWWWWWK...", "..KWKWKWKWWWK...", "..KWWWWWWWWWK...", "..KWKWKWPPWWK...",
        "..KWWWWWPPWWK...", "..KWKWKWWWWWK...", "..KWWWWWWWWWK...", "..KKKKKKKKKKK...", "................", "................"],
    # 15-18: nagrody za odbiór (Młot udarowy, Pistolet do kotew, Buty robocze, Pas narzędziowy), 19: Respekt (medal)
    [   # Młot udarowy: żółta obudowa, czarny uchwyt, grot
        "................", "................", "....KKKKKKK.....", "...KYYYYYYYKKKK.", "...KYYWYYYYKllK.",
        "...KYYYYYYYKKKKK", "...KKKKKYYYK.KlK", ".......KYYYK..KK", "......KKKKKK....", "......KKKKK.....",
        ".....KKKKK......", ".....KKKK.......", "....KKKK........", "................", "................", "................"],
    [   # Pistolet do kotew: pomarańczowy korpus, szara lufa, kotwa
        "................", "................", "................", "..KKKKKKKKKKK...", ".KOOOOOOOOOOKKK.",
        ".KOOWOOOOOOOKlK.", ".KOOOOOOOOOOKgK.", ".KKKKKOOOKKKKKKK", ".....KOOOK...KgK", ".....KOOOK....K.",
        "....KOOOOK......", "....KOOOK.......", "....KKKKK.......", "................", "................", "................"],
    [   # Buty robocze: brązowe z pomarańczowym noskiem
        "................", "................", "....KKKKK.......", "....KTTTK.......", "....KTTTK.......",
        "....KTlTK.......", "....KTTTK.......", "....KTTTKKKKK...", "...KTTTTTTTOOK..", "...KTTTTTTOOOOK.",
        "...KKKKKKKKKKKK.", "...KggggggggggK.", "....KKKKKKKKKK..", "................", "................", "................"],
    [   # Pas narzędziowy: pas z kieszeniami i młotkiem
        "................", "................", "................", "KKKKKKKKKKKKKKKK", "TTTTTTYYTTTTTTTT",
        "KKKKKKKKKKKKKKKK", "..KTTTK..KTTTK..", "..KTOTK..KTTTK.g", "..KTTTK..KTlTKgg", "..KTTTK..KTTTK.T",
        "...KKK....KKK..T", "...............T", "................", "................", "................", "................"],
    [   # Respekt: medal z gwiazdą
        "....KKK..KKK....", "....KRRK.KBK....", ".....KRRKBK.....", "......KKKK......", ".....KKYYKK.....",
        "....KYYWYYYK....", "...KYYYYYYYYK...", "...KYYYWYYYYK...", "...KYWWWWWYYK...", "...KYYWWWYYYK...",
        "...KYYWYWYYYK...", "...KYYYYYYYOK...", "....KYYYYYOK....", ".....KKKKKK.....", "................", "................"],
    # 20-22: mechaniki aktów (błoto, porywy wiatru, pył), 23: statystyki (pomoc "i")
    [   # błoto: płaska brązowa kałuża z bąblami
        "................", "................", "................", "......KK....K...", ".....KOOK..KOK..",
        "......KK....K...", "..KKKKKKKKKKKK..", ".KTTTTTTTTTTTTK.", "KTTOTTTTTTTOTTTK", "KTTTTTTTTTTTTTTK",
        ".KTTTOTTTTTTTTK.", "..KKKKKKKKKKKK..", "................", "................", "................", "................"],
    [   # porywy: zawijasy wiatru
        "................", "................", "........KKK.....", ".......KlllK....", "KKKKKKKKK.lK....",
        "lllllllllKlK....", "KKKKKKKKKKK.....", "................", "KKKKKKKKKKKKK...", "llllllllllllK...",
        "KKKKKKKKKK.lK...", ".........KlK....", "........KlK.....", ".........K......", "................", "................"],
    [   # pył: szara chmura z drobinkami
        "................", "................", "..l....l........", "......KKKK...l..", "..l.KKggggKK....",
        "...KggllggggK...", "..KgglllgggggK..", ".KggggggggglgK..", ".KgglggggggggK..", "..KgggggglggK..l",
        "...KKggggggK....", "....l.KKKK..l...", "..l.......l.....", "......l.........", "................", "................"],
    [   # statystyki: fioletowe kółko z "i"
        "................", "....KKKKKKK.....", "...KPPPPPPPK....", "..KPPPPWPPPPK...", "..KPPPPPPPPPK...",
        "..KPPPWWPPPPK...", "..KPPPPWPPPPK...", "..KPPPPWPPPPK...", "..KPPPPWPPPPK...", "..KPPPWWWPPPK...",
        "...KPPPPPPPK....", "....KKKKKKK.....", "................", "................", "................", "................"],
]


def menu_icon_frames():
    return [p for rows in MENU_ICONS for p in parse(rows)]
