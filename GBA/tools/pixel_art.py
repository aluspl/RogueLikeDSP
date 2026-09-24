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
}

# (kask, kamizelka, narzędzie) w kolejności zawodów z data/game.json
WORKERS = [("W", "O", "log"), ("O", "B", "trowel"), ("Y", "T", "nailgun"),
           ("B", "N", "tester"), ("R", "B", "wrench"), ("G", "g", "grinder")]


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
]


def particle_frames():
    out = []
    for rows in PARTICLES:
        assert len(rows) == 8 and all(len(r) == 8 for r in rows), rows
        out += [CODES[ch] for r in rows for ch in r]
    return out


# ------------------------------------------------------------------ Osiedle: domy 16x16
# klatka = wielkość * 6 + zawód (dach w kolorze kasku zawodu), 24 = pusta działka
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


def house_frames():
    return [p for size in range(4) for cls in range(6) for p in house_frame(cls, size)] + parse(EMPTY_PLOT)
