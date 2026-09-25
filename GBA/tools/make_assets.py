#!/usr/bin/env python3
"""Generuje grafiki GBA (BMP + JSON dla Butano) z kodu.

Wejście: assets_src/pb_logo.svg (znak PlanBudowlany). Wszystko inne rysowane proceduralnie,
żeby dało się to podmienić pixel-artem z Aseprite bez zmiany kodu gry (te same nazwy plików).
Uruchom: python3 tools/make_assets.py  (wymaga: pillow, cairosvg, qrcode)
"""
import io, json, os, struct, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from PIL import Image, ImageDraw, ImageFont
import cairosvg, qrcode

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
G = os.path.join(ROOT, "graphics")
SRC = os.path.join(ROOT, "assets_src")
DOCS = os.path.join(ROOT, "docs")

def find_font_dir():
    """Katalog z fontami DejaVu: zmienna DEJAVU_DIR albo typowe lokalizacje Linux/macOS."""
    candidates = [os.environ.get("DEJAVU_DIR"), "/usr/share/fonts/truetype/dejavu",
                  "/usr/share/fonts/TTF", os.path.expanduser("~/Library/Fonts"), "/Library/Fonts"]
    for d in filter(None, candidates):
        if os.path.isfile(os.path.join(d, "DejaVuSans.ttf")):
            return d
    raise SystemExit("Brak fontów DejaVu - zainstaluj je albo ustaw DEJAVU_DIR")

FONT_DIR = find_font_dir()
FONT_MONO_B = os.path.join(FONT_DIR, "DejaVuSansMono-Bold.ttf")
FONT_SANS_B = os.path.join(FONT_DIR, "DejaVuSans-Bold.ttf")
FONT_SANS = os.path.join(FONT_DIR, "DejaVuSans.ttf")

# kolory marki z design systemu PlanBudowlany (AppColors w aplikacji mobilnej)
BRAND_VIOLET = (107, 78, 255)    # brand #6B4EFF
BRAND_ORANGE = (255, 122, 61)    # brandAccent #FF7A3D
BRAND_NAVY = (27, 20, 64)        # ciemny fiolet: tła ekranów tekstowych (= bn::color(3, 2, 8))
DARK = (11, 11, 15)              # textLight2 #0B0B0F
URL = "planbudowlany.online"

# ---------------------------------------------------------------- BMP writer
def write_bmp(path, pixels, w, h, palette, bpp):
    """pixels: lista indeksów (w*h), palette: lista (r,g,b). Bez kompresji, bez color space info."""
    ncolors = 16 if bpp == 4 else 256
    pal = list(palette) + [(0, 0, 0)] * (ncolors - len(palette))
    row_bytes = ((w * bpp + 31) // 32) * 4
    data = bytearray()
    for y in range(h - 1, -1, -1):
        row = bytearray()
        if bpp == 8:
            row += bytes(pixels[y * w:(y + 1) * w])
        else:
            for x in range(0, w, 2):
                a = pixels[y * w + x]
                b = pixels[y * w + x + 1] if x + 1 < w else 0
                row.append((a << 4) | b)
        row += b"\0" * (row_bytes - len(row))
        data += row
    pal_bytes = b"".join(struct.pack("<BBBB", b, g, r, 0) for (r, g, b) in pal)
    off = 14 + 40 + len(pal_bytes)
    hdr = struct.pack("<2sIHHI", b"BM", off + len(data), 0, 0, off)
    info = struct.pack("<IiiHHIIiiII", 40, w, h, 1, bpp, 0, len(data), 2835, 2835, ncolors, 0)
    with open(path, "wb") as f:
        f.write(hdr + info + pal_bytes + data)

def write_json(name, obj):
    with open(os.path.join(G, name + ".json"), "w") as f:
        json.dump(obj, f, indent=2)

def quantize(img, colors, first=(0, 0, 0)):
    """RGB -> indeksy; indeks 0 = `first` (tło/przezroczystość)."""
    q = img.convert("RGB").quantize(colors=colors - 1, method=Image.Quantize.MEDIANCUT, dither=Image.Dither.NONE)
    pal = q.getpalette()[: 3 * (colors - 1)]
    palette = [first] + [tuple(pal[i:i + 3]) for i in range(0, len(pal), 3)]
    pixels = [p + 1 for p in (q.get_flattened_data() if hasattr(q,"get_flattened_data") else q.getdata())]
    return pixels, palette

# ---------------------------------------------------------------- 1. font 8x16 z polskimi znakami
PL_CHARS = ["ą", "ć", "ę", "ł", "ń", "ó", "ś", "ź", "ż", "Ą", "Ć", "Ę", "Ł", "Ń", "Ó", "Ś", "Ź", "Ż"]

def make_font():
    chars = [chr(c) for c in range(33, 127)] + PL_CHARS
    font = ImageFont.truetype(FONT_MONO_B, 12)
    W, H = 8, 16
    strip = Image.new("L", (W, H * len(chars)), 0)
    for i, ch in enumerate(chars):
        cell = Image.new("L", (W, H), 0)
        d = ImageDraw.Draw(cell)
        bbox = font.getbbox(ch)
        gw = bbox[2] - bbox[0]
        d.text(((W - gw) // 2 - bbox[0], 0), ch, font=font, fill=255)
        strip.paste(cell, (0, i * H))
    # 0 = przezroczysty, 1 = tekst, 2 = obrys (czytelność na mapie)
    px = strip.load()
    out = []
    for y in range(strip.height):
        for x in range(W):
            if px[x, y] > 110:
                out.append(1)
            else:
                near = any(0 <= x + dx < W and 0 <= y + dy < strip.height and px[x + dx, y + dy] > 110
                           for dx in (-1, 0, 1) for dy in (-1, 0, 1))
                out.append(2 if near else 0)
    write_bmp(os.path.join(G, "font_8x16.bmp"), out, W, strip.height,
              [(255, 0, 255), (250, 250, 250), (20, 20, 30)], 4)
    write_json("font_8x16", {"type": "sprite", "height": 16})
    return len(chars)

# ---------------------------------------------------------------- 2. sprite'y aktorów (16x16, wspólna paleta)
SPR_PAL = [(255, 0, 255), (26, 26, 26), (240, 240, 240), (240, 192, 144), BRAND_ORANGE, BRAND_NAVY,
           (58, 123, 213), (245, 211, 61), (214, 60, 60), (76, 175, 80), (46, 107, 48), (139, 90, 43),
           (138, 138, 138), (200, 200, 200), (90, 208, 230), (142, 91, 208)]
K, WH, SK, OR, NV, BL, YE, RD, GR, DG, BR, GY, LG, CY, PU = range(1, 16)

def sprite(draw_fn):
    im = Image.new("P", (16, 16), 0)
    d = ImageDraw.Draw(im)
    draw_fn(d)
    return list(im.get_flattened_data()) if hasattr(im,"get_flattened_data") else list(im.getdata())

def worker(helmet, vest, tool):
    def f(d):
        d.rectangle([5, 1, 10, 3], fill=helmet)          # kask
        d.rectangle([4, 3, 11, 3], fill=helmet)
        d.rectangle([5, 4, 10, 7], fill=SK)             # twarz
        d.point([(6, 5), (9, 5)], fill=K)
        d.rectangle([4, 8, 11, 12], fill=vest)           # kamizelka
        d.line([(4, 10), (11, 10)], fill=YE)             # pas odblaskowy
        d.rectangle([5, 13, 6, 15], fill=NV); d.rectangle([9, 13, 10, 15], fill=NV)
        tool(d)
    return f

def t_log(d):      d.rectangle([12, 7, 15, 12], fill=WH); d.line([(13, 9), (14, 9)], fill=GY)       # dziennik
def t_trowel(d):   d.polygon([(12, 8), (15, 10), (12, 12)], fill=LG); d.line([(11, 10), (12, 10)], fill=BR)  # kielnia
def t_hammer(d):   d.line([(12, 7), (12, 13)], fill=BR); d.rectangle([11, 6, 14, 7], fill=GY)     # młotek
def t_tester(d):   d.line([(13, 6), (13, 13)], fill=YE); d.point((13, 5), fill=RD)                # próbnik
def t_wrench(d):   d.line([(12, 13), (14, 7)], fill=GY); d.rectangle([13, 5, 15, 7], fill=GY); d.point((14, 6), fill=0)  # klucz
def t_grinder(d):  d.ellipse([11, 8, 15, 12], fill=LG); d.point((13, 10), fill=K)                  # szlifierka

CLASSES_SPR = [worker(WH, OR, t_log), worker(OR, BL, t_trowel), worker(YE, BR, t_hammer),
               worker(BL, NV, t_tester), worker(RD, BL, t_wrench), worker(GR, GY, t_grinder)]

def e_leak(d):   d.polygon([(8, 1), (13, 10), (8, 15), (3, 10)], fill=CY); d.ellipse([3, 7, 13, 15], fill=CY); d.point([(6, 9), (10, 9)], fill=K); d.point((6, 7), fill=WH)
def e_spark(d):  d.polygon([(9, 0), (3, 9), (7, 9), (5, 15), (13, 6), (9, 6), (11, 0)], fill=YE, outline=OR); d.point([(7, 7), (9, 7)], fill=K)
def e_mold(d):   d.ellipse([1, 5, 15, 15], fill=DG); d.ellipse([3, 7, 13, 14], fill=GR); d.point([(5, 9), (10, 11), (7, 12)], fill=DG); d.point([(6, 8), (9, 8)], fill=K)
def e_beetle(d): d.ellipse([3, 4, 12, 14], fill=BR); d.line([(8, 4), (8, 14)], fill=K); d.ellipse([5, 1, 10, 5], fill=K); d.line([(2, 7), (13, 7)], fill=K); d.line([(2, 11), (13, 11)], fill=K)
def e_paper(d):
    for i, y in enumerate([10, 7, 4]):
        d.rectangle([2 + i, y, 12 + i, y + 5], fill=WH, outline=GY)
    d.line([(6, 7), (12, 7)], fill=GY); d.point([(7, 10), (11, 10)], fill=K); d.line([(8, 12), (10, 12)], fill=RD)
def e_delivery(d): d.rectangle([1, 5, 10, 12], fill=OR, outline=BR); d.rectangle([10, 7, 14, 12], fill=LG); d.ellipse([2, 11, 5, 14], fill=K); d.ellipse([10, 11, 13, 14], fill=K); d.ellipse([4, 6, 8, 10], fill=WH); d.line([(6, 8), (6, 7)], fill=K)
def e_rain(d):   d.ellipse([1, 2, 9, 9], fill=GY); d.ellipse([6, 1, 15, 9], fill=GY); d.rectangle([3, 5, 13, 9], fill=GY); d.point([(6, 5), (10, 5)], fill=K); [d.line([(x, 11), (x - 1, 14)], fill=BL) for x in (4, 8, 12)]
def e_budget(d): d.ellipse([2, 4, 14, 15], fill=YE, outline=BR); d.rectangle([6, 1, 10, 4], fill=BR); d.text((4, 6), "zł", fill=BR); d.point([(5, 12), (11, 12)], fill=K)
def e_deadline(d):
    d.ellipse([1, 2, 15, 15], fill=RD, outline=K); d.ellipse([3, 4, 13, 13], fill=WH)
    d.line([(8, 8), (8, 5)], fill=K); d.line([(8, 8), (11, 9)], fill=K); d.rectangle([2, 0, 4, 2], fill=GY); d.rectangle([12, 0, 14, 2], fill=GY)
def p_coffee(d): d.rectangle([4, 6, 10, 13], fill=WH, outline=GY); d.rectangle([5, 7, 9, 8], fill=BR); d.arc([9, 7, 13, 11], 270, 90, fill=GY); d.line([(6, 2), (6, 4)], fill=LG); d.line([(8, 1), (8, 4)], fill=LG)
def p_helmet(d): d.pieslice([2, 4, 14, 16], 180, 360, fill=OR); d.rectangle([1, 10, 15, 11], fill=OR); d.line([(8, 4), (8, 9)], fill=YE)
def p_plan(d):   d.rectangle([2, 3, 13, 13], fill=BL, outline=WH); d.line([(4, 6), (11, 6)], fill=WH); d.rectangle([5, 8, 10, 11], outline=WH)
def fx_hit(d):   d.line([(3, 3), (12, 12)], fill=WH); d.line([(12, 3), (3, 12)], fill=WH); d.point((8, 8), fill=YE)

def ui_lock(d):
    d.arc([4, 1, 11, 10], 180, 360, fill=LG); d.line([(4, 5), (4, 7)], fill=LG); d.line([(11, 5), (11, 7)], fill=LG)
    d.rectangle([3, 7, 12, 14], fill=YE, outline=BR); d.rectangle([7, 9, 8, 12], fill=K)

def silhouette(frame):   # ciemna sylwetka zablokowanego zawodu
    return [0 if p == 0 else K for p in frame]

ENEMY_SPR = [e_leak, e_spark, e_mold, e_beetle, e_paper, e_delivery, e_rain, e_budget, e_deadline]
PICKUP_SPR = [p_coffee, p_helmet, p_plan, fx_hit]
def p_toolbox(d):   # skrzynka z narzędziem (drop)
    d.rectangle([2, 7, 13, 14], fill=RD, outline=K); d.rectangle([2, 7, 13, 9], fill=OR, outline=K)
    d.arc([5, 3, 10, 9], 180, 360, fill=GY); d.rectangle([7, 10, 8, 11], fill=YE)

UI_SPR = [ui_lock]

# klatki: 0-5 zawody, 6-14 wrogowie, 15-17 znajdźki, 18 efekt trafienia, 19 kłódka, 20-25 sylwetki zawodów,
# 26 skrzynka z narzędziem, 27-41 druga klatka animacji zawodów i wrogów (pixel-art: tools/pixel_art.py)
def make_actors():
    import pixel_art as pa
    workers = [pa.worker_frame(i, 0) for i in range(6)]
    enemies = [pa.enemy_frame(i, 0) for i in range(9)]
    frames = workers + enemies
    frames += [pa.pickup_frame(n) for n in ("coffee", "helmet", "plan")]
    frames += [sprite(fx_hit), sprite(ui_lock)]
    frames += [silhouette(f) for f in workers]
    frames += [pa.pickup_frame("toolbox")]
    frames += [pa.worker_frame(i, 1) for i in range(6)] + [pa.enemy_frame(i, 1) for i in range(9)]   # klatki B animacji
    px = [p for fr in frames for p in fr]
    write_bmp(os.path.join(G, "actors.bmp"), px, 16, 16 * len(frames), SPR_PAL, 4)
    write_json("actors", {"type": "sprite", "height": 16})
    return len(frames)

# ---------------------------------------------------------------- 2b. pasek życia (2 sprite'y 32x8 = 64 px)
# klatka = segment * 96 + kolor * 32 + wypełnienie; segment 0 = lewy, 1 = prawy; kolor 0 zielony, 1 żółty, 2 czerwony
# wnętrze paska ma 62 px: lewy segment pokazuje 0..31, prawy 0..31 pikseli
HP_PAL = [(255, 0, 255), (16, 16, 24), (54, 58, 72), (250, 250, 250),
          (76, 175, 80), (46, 107, 48), (245, 211, 61), (190, 150, 30), (214, 60, 60), (140, 30, 30)]

def hp_frame(seg, color, fill):
    main, shade = 4 + color * 2, 5 + color * 2
    px = [[0] * 32 for _ in range(8)]
    for y in range(8):
        for x in range(32):
            border = y in (0, 7) or (seg == 0 and x == 0) or (seg == 1 and x == 31)
            if border: px[y][x] = 1; continue
            inner = x - 1 if seg == 0 else x          # pozycja w obrębie segmentu
            if inner < fill:
                px[y][x] = 3 if y == 1 else (shade if y == 6 else main)
            else:
                px[y][x] = 2
    return [p for row in px for p in row]

def make_hp_bar():
    frames = [hp_frame(seg, c, f) for seg in range(2) for c in range(3) for f in range(32)]
    write_bmp(os.path.join(G, "hp_bar.bmp"), [p for fr in frames for p in fr], 32, 8 * len(frames), HP_PAL, 4)
    write_json("hp_bar", {"type": "sprite", "height": 8})
    return len(frames)

# ---------------------------------------------------------------- 3. kafelki + palety etapów
# indeksy: 0 tło, 1 podłoga, 2 detal podłogi, 3 ściana, 4 jasny detal ściany, 5 cień ściany, 6/7 schody
STAGE_COLORS = [
    # (podłoga, detal, ściana, jasny, cień) - kolejność jak etapy w data/game.json
    [(96, 70, 44), (80, 58, 36), (150, 138, 118), (180, 170, 150), (100, 92, 80)],        # Fundamenty
    [(118, 118, 118), (100, 100, 100), (178, 82, 59), (222, 200, 170), (120, 50, 36)],   # Mury parteru
    [(128, 128, 124), (108, 108, 104), (150, 150, 146), (190, 190, 186), (90, 90, 88)],  # Strop (beton)
    [(169, 116, 59), (130, 86, 40), (122, 46, 46), (170, 80, 70), (80, 28, 28)],         # Dach
    [(150, 104, 62), (122, 82, 46), (210, 210, 200), (240, 240, 232), (120, 140, 170)],  # Okna i drzwi
    [(160, 160, 160), (135, 135, 135), (90, 111, 143), (200, 120, 60), (56, 70, 96)],    # Instalacje
    [(200, 196, 184), (176, 170, 156), (226, 218, 196), (246, 240, 224), (150, 140, 120)],  # Tynki i wylewki
    [(224, 214, 192), (190, 178, 150), (111, 168, 160), (160, 210, 200), (70, 118, 110)],   # Wykończenie
]

def tile(fn):
    t = [[0] * 8 for _ in range(8)]
    fn(t)
    return [c for row in t for c in row]

def t_empty(t): pass
def t_floor(t):
    for y in range(8):
        for x in range(8):
            t[y][x] = 1
    for (x, y) in [(1, 2), (5, 5), (6, 1), (2, 6)]:
        t[y][x] = 2
def t_wall(t):   # mur z fugami (wzór cegieł przez 2 kafle w pionie się powtarza)
    for y in range(8):
        for x in range(8):
            t[y][x] = 3
    for x in range(8):
        t[3][x] = 5; t[7][x] = 5
    t[0][3] = t[1][3] = t[2][3] = 5
    t[4][7] = t[5][7] = t[6][7] = 5
    t[0][0] = t[4][4] = 4
def t_walltop(t):  # ściana z podłogą poniżej: jasna krawędź u góry
    t_wall(t)
    for x in range(8):
        t[0][x] = 4
def t_stairs(t):
    for y in range(8):
        for x in range(8):
            t[y][x] = 6 if (y // 2) % 2 == 0 else 7
    for y in range(8):
        t[y][0] = 5

def t_floor_wall_shadow(t):   # podłoga tuż pod ścianą: pas cienia u góry (kolor 8)
    t_floor(t)
    for x in range(8):
        t[0][x] = t[1][x] = 8
        if x % 2 == 0: t[2][x] = 8

def t_actor_shadow(t):   # lewa dolna ćwiartka elipsy cienia pod postacią (prawa = odbicie)
    t_floor(t)
    for y in range(8):
        for x in range(8):
            dx, dy = (x - 7.5) / 7.2, (y - 4.8) / 3.0   # środek elipsy na styku kafli, w dolnej części pola
            if dx * dx + dy * dy <= 1.0: t[y][x] = 8

# indeksy: 0 pusty, 1 podłoga, 2 mur, 3 lico muru, 4 schody, 5 podłoga z cieniem muru, 6 cień postaci (ćwiartka)
TILES = [t_empty, t_floor, t_wall, t_walltop, t_stairs, t_floor_wall_shadow, t_actor_shadow]

def make_tiles():
    px_tiles = [tile(f) for f in TILES]
    w = 8 * len(px_tiles)
    px = []
    for y in range(8):
        for ti in px_tiles:
            px += ti[y * 8:(y + 1) * 8]
    write_bmp(os.path.join(G, "tiles.bmp"), px, w, 8, [(0, 0, 0)] * 16, 4)
    write_json("tiles", {"type": "regular_bg_tiles", "bpp_mode": "bpp_4"})
    # Każdy etap: 4 palety światła - pełne, 80%, 60% (skraj pola widzenia), zapamiętane (przyciemnione).
    # Osobny plik na etap, bo 5 etapów x 64 kolory nie mieści się w jednej palecie BMP.
    for si, c in enumerate(STAGE_COLORS):
        pal = []
        for level in range(4):
            if level == 3:
                cols = [tuple(int(v * 0.5 + n * 0.25) for v, n in zip(col, BRAND_NAVY)) for col in c]
                stairs = [(100, 90, 40), (70, 50, 20)]
            else:
                f = (1.0, 0.8, 0.6)[level]
                cols = [tuple(int(v * f + n * (1 - f) * 0.6) for v, n in zip(col, BRAND_NAVY)) for col in c]
                stairs = [tuple(int(v * f) for v in (245, 211, 61)), tuple(int(v * f) for v in (180, 120, 20))]
            shadow = tuple(int(v * 0.45) for v in cols[0])   # cień na podłodze
            pal += [(12, 12, 20)] + cols + stairs + [shadow] + [(0, 0, 0)] * 7
        write_bmp(os.path.join(G, f"stage_palettes_{si}.bmp"), [0] * 64, 8, 8, pal, 8)
        write_json(f"stage_palettes_{si}", {"type": "bg_palette", "bpp_mode": "bpp_4", "colors_count": 64})

# ---------------------------------------------------------------- 3b. telefon z aplikacją PlanBudowlany (menu w grze)
# Wspólna paleta 16 kolorów = tokeny AppColors z aplikacji mobilnej.
PHONE_PAL = [(255, 0, 255),       # 0  przezroczysty
             (246, 245, 248),     # 1  bgLight2 (tło aplikacji)
             (255, 255, 255),     # 2  card
             (239, 234, 247),     # 3  nagłówek grupy / tor paska
             BRAND_VIOLET,        # 4  brand
             BRAND_ORANGE,        # 5  brandAccent
             DARK,                # 6  tekst
             (99, 99, 102),       # 7  textDim
             (229, 231, 235),     # 8  border (szara pastylka)
             (148, 163, 184),     # 9  statusTodo (slate-400)
             (245, 158, 11),      # 10 statusInProgress (amber-500)
             (16, 185, 129),      # 11 statusDone (emerald-500)
             (239, 68, 68),       # 12 statusDelayed / error (red-500)
             (254, 243, 222),     # 13 tło pastylki "w trakcie"
             (254, 226, 226),     # 14 tło pastylki "otwarta"
             (209, 250, 229)]     # 15 tło pastylki "gotowe"
P_BG, P_CARD, P_GROUP, P_BRAND, P_ACCENT, P_TEXT, P_DIM, P_BORDER, P_TODO, P_PROG, P_DONE, P_LATE, P_PROG_BG, P_LATE_BG, P_DONE_BG = range(1, 16)
TAB_X = [24 + 48 * i for i in range(5)]   # środki ikon zakładek: Zadania, Usterki, Start, Zespół, Koszty
TAB_Y = 134                               # górna krawędź ikon 16x16

def tab_icon(d, i, c, ox=0, oy=0):
    """Proste ikony jak w pasku aplikacji; (ox, oy) = lewy górny róg pola 16x16."""
    P = lambda x, y: (ox + x, oy + y)
    if i == 0:   # Zadania: znaczniki + linie
        for y in (2, 8):
            d.ellipse([P(1, y), P(4, y + 3)], outline=c); d.line([P(6, y + 1), P(14, y + 1)], fill=c); d.line([P(6, y + 2), P(14, y + 2)], fill=c)
        d.point([P(2, 3), P(3, 4)], fill=c)
    elif i == 1:   # Usterki: dokument
        d.rectangle([P(3, 1), P(12, 13)], outline=c); d.rectangle([P(3, 1), P(12, 13)], fill=c)
        for y in (6, 9): d.line([P(5, y), P(10, y)], fill=PHONE_PAL[P_CARD])
    elif i == 2:   # Start: domek
        d.polygon([P(1, 7), P(8, 1), P(15, 7)], fill=c); d.rectangle([P(3, 7), P(13, 13)], fill=c)
        d.rectangle([P(7, 9), P(9, 13)], fill=PHONE_PAL[P_CARD])
    elif i == 3:   # Zespół: dwie osoby
        for x in (1, 8):
            d.ellipse([P(x + 1, 1), P(x + 5, 5)], fill=c); d.pieslice([P(x, 6), P(x + 7, 16)], 180, 360, fill=c)
    else:          # Koszty: banknot
        d.rectangle([P(1, 3), P(14, 11)], outline=c); d.rectangle([P(2, 4), P(13, 10)], outline=c)
        d.ellipse([P(6, 5), P(9, 9)], outline=c)

def make_phone():
    # ramka telefonu: tło aplikacji, pasek statusu, dolny pasek zakładek (szare ikony)
    im = Image.new("RGB", (256, 256), PHONE_PAL[P_BG])
    d = ImageDraw.Draw(im)
    d.text((10, 1), "09:41", font=ImageFont.truetype(FONT_SANS_B, 9), fill=DARK)
    for k in range(4): d.rectangle([196 + k * 3, 8 - k * 2, 197 + k * 3, 9], fill=DARK)           # zasięg
    d.rectangle([212, 3, 227, 9], outline=DARK); d.rectangle([214, 5, 222, 7], fill=PHONE_PAL[P_DONE])   # bateria
    d.rectangle([228, 5, 229, 7], fill=DARK)
    d.rounded_rectangle([4, 130, 235, 157], radius=8, fill=PHONE_PAL[P_CARD], outline=PHONE_PAL[P_BORDER])
    for i, x in enumerate(TAB_X): tab_icon(d, i, PHONE_PAL[P_DIM], x - 8, TAB_Y)
    # ciemna ramka urządzenia z zaokrąglonymi rogami
    d.rounded_rectangle([-2, -2, 241, 161], radius=12, outline=DARK, width=4)
    d.rectangle([240, 0, 256, 256], fill=DARK); d.rectangle([0, 160, 256, 256], fill=DARK)
    idx = [PHONE_PAL.index(im.getpixel((x, y))) if im.getpixel((x, y)) in PHONE_PAL else nearest(im.getpixel((x, y)))
           for y in range(256) for x in range(256)]
    write_bmp(os.path.join(G, "phone_chrome.bmp"), idx, 256, 256, PHONE_PAL, 4)
    write_json("phone_chrome", {"type": "regular_bg", "bpp_mode": "bpp_4"})

    # ikony: 0-4 aktywne zakładki (fiolet + kreska pod spodem), 5 ikona aplikacji PB do powiadomień
    frames = []
    for i in range(5):
        fr = Image.new("RGB", (16, 16), PHONE_PAL[0]); fd = ImageDraw.Draw(fr)
        fd.rectangle([0, 0, 15, 15], fill=PHONE_PAL[P_CARD]); tab_icon(fd, i, BRAND_VIOLET)
        fd.rectangle([3, 15, 12, 15], fill=BRAND_VIOLET)
        frames.append(fr)
    fr = Image.new("RGB", (16, 16), PHONE_PAL[0]); fd = ImageDraw.Draw(fr)
    fd.rounded_rectangle([0, 0, 15, 15], radius=4, fill=BRAND_VIOLET)
    fd.polygon([(3, 7), (8, 2), (13, 7)], outline=PHONE_PAL[P_CARD]); fd.rectangle([4, 7, 12, 13], outline=PHONE_PAL[P_CARD])
    fd.line([(7, 9), (7, 13)], fill=PHONE_PAL[P_CARD]); fd.line([(9, 9), (9, 13)], fill=PHONE_PAL[P_CARD])
    frames.append(fr)
    px = [nearest(fr.getpixel((x, y))) for fr in frames for y in range(16) for x in range(16)]
    write_bmp(os.path.join(G, "phone_icons.bmp"), px, 16, 16 * len(frames), PHONE_PAL, 4)
    write_json("phone_icons", {"type": "sprite", "height": 16})

    # kafelki treści (karty, pastylki, paski postępu) + nagłówek z indeksami dla kodu gry
    tiles, names = [[0] * 64], ["empty"]
    def add(name, t): names.append(name); tiles.append(t)
    fills = {"card": P_CARD, "group": P_GROUP, "brand": P_BRAND, "prog_bg": P_PROG_BG, "late_bg": P_LATE_BG,
             "done_bg": P_DONE_BG, "gray": P_BORDER, "bg": P_BG}
    for n, c in fills.items():
        add("fill_" + n, [c] * 64)
        corner = [0] * 64          # lewy górny róg, promień ~3 px (reszta rogów przez odbicia)
        for y in range(8):
            for x in range(8):
                if (x - 3.5) ** 2 + (y - 3.5) ** 2 <= 16 or x >= 4 or y >= 4: corner[y * 8 + x] = c
        add("corner_" + n, corner)
    for n, c in {"todo": P_TODO, "prog": P_PROG, "done": P_DONE, "brand": P_BRAND, "late": P_LATE}.items():
        add("stripe_" + n, [c if x < 2 else P_CARD for y in range(8) for x in range(8)])
    for n, c in {"brand": P_BRAND, "done": P_DONE, "late": P_LATE}.items():
        for k in range(9):   # pasek postępu: k z 8 pikseli wypełnione, wiersze 6-7 (środek 16-pikselowego wiersza listy)
            add(f"bar_{n}_{k}", [(c if x < k else P_GROUP) if 6 <= y <= 7 else P_CARD for y in range(8) for x in range(8)])
    px = []
    for y in range(8):
        for t in tiles: px += t[y * 8:(y + 1) * 8]
    write_bmp(os.path.join(G, "phone_tiles.bmp"), px, 8 * len(tiles), 8, PHONE_PAL, 4)
    write_json("phone_tiles", {"type": "regular_bg_tiles", "bpp_mode": "bpp_4"})
    write_bmp(os.path.join(G, "phone_palette.bmp"), [0] * 64, 8, 8, PHONE_PAL, 4)
    write_json("phone_palette", {"type": "bg_palette", "bpp_mode": "bpp_4"})
    h = ["// WYGENEROWANE przez tools/make_assets.py - indeksy kafelków graphics/phone_tiles.bmp.", "#pragma once", "",
         "namespace phone_tile", "{"]
    h += [f"    constexpr int {n} = {i};" for i, n in enumerate(names)]
    h += [f"    constexpr int tab_x[] = {{ {', '.join(str(x - 120) for x in TAB_X)} }};   // środki ikon (współrzędne sprite'ów)",
          f"    constexpr int tab_y = {TAB_Y + 8 - 80};", "}", ""]
    open(os.path.join(ROOT, "include", "phone_tiles.h"), "w", encoding="utf-8").write("\n".join(h))

    # palety tekstu: [przezroczysty, litera, obrys] - obrys w kolorze karty, żeby na jasnym tle był niewidoczny
    for n, (fill, edge) in {"dark": (DARK, PHONE_PAL[P_CARD]), "dim": (PHONE_PAL[P_DIM], PHONE_PAL[P_CARD]),
                            "brand": (BRAND_VIOLET, PHONE_PAL[P_CARD]), "prog": ((180, 110, 5), PHONE_PAL[P_PROG_BG]),
                            "done": ((4, 120, 87), PHONE_PAL[P_DONE_BG]), "late": ((185, 28, 28), PHONE_PAL[P_LATE_BG]),
                            "white": ((255, 255, 255), BRAND_VIOLET)}.items():
        write_bmp(os.path.join(G, "font_" + n + ".bmp"), [0] * 64, 8, 8, [(255, 0, 255), fill, edge], 4)
        write_json("font_" + n, {"type": "sprite_palette"})
    return len(tiles)

def nearest(rgb):
    return min(range(1, 16), key=lambda i: sum((a - b) ** 2 for a, b in zip(rgb, PHONE_PAL[i]))) if rgb != PHONE_PAL[0] else 0

# ---------------------------------------------------------------- 4. ekran tytułowy i końcowy (regular bg 256x256, 8bpp)
def logo_image(size):
    png = cairosvg.svg2png(url=os.path.join(SRC, "pb_logo.svg"), output_width=size, output_height=size)
    return Image.open(io.BytesIO(png)).convert("RGBA")

SCREEN_BG_INDEX = {}

def screen_bg(name, img240, bg=BRAND_VIOLET):
    canvas = Image.new("RGB", (256, 256), bg)
    canvas.paste(img240, (0, 0))
    px, pal = quantize(canvas, 64, first=bg)
    # indeks koloru tła (najczęstszy) - gra podmienia go gradientem HDMA
    counts = {}
    for v in px: counts[v] = counts.get(v, 0) + 1
    SCREEN_BG_INDEX[name] = max(counts, key=counts.get)
    write_bmp(os.path.join(G, name + ".bmp"), px, 256, 256, pal, 8)
    write_json(name, {"type": "regular_bg", "bpp_mode": "bpp_8"})
    canvas.crop((0, 0, 240, 160)).resize((480, 320), Image.NEAREST).save(os.path.join(DOCS, "preview_" + name + ".png"))

def centered(d, y, text, font, fill):
    w = d.textlength(text, font=font)
    d.text(((240 - w) / 2, y), text, font=font, fill=fill)

def make_title():
    im = Image.new("RGB", (240, 160), BRAND_VIOLET)
    d = ImageDraw.Draw(im)
    # "plac budowy": pas ostrzegawczy u dołu
    d.rectangle([0, 148, 240, 160], fill=DARK)
    for x in range(-20, 260, 16):
        d.polygon([(x, 160), (x + 8, 160), (x + 16, 148), (x + 8, 148)], fill=BRAND_ORANGE)
    d.rectangle([0, 146, 240, 147], fill=DARK)
    logo = logo_image(80)
    im.paste(logo, (80, -6), logo)
    centered(d, 66, "PlanBudowlany", ImageFont.truetype(FONT_SANS_B, 22), (250, 250, 250))
    centered(d, 91, "ROGUELIKE", ImageFont.truetype(FONT_SANS_B, 14), BRAND_ORANGE)
    centered(d, 109, "Zbuduj dom. Przetrwaj budowę.", ImageFont.truetype(FONT_SANS, 10), (225, 220, 255))
    screen_bg("title", im)

def make_end():
    im = Image.new("RGB", (240, 160), BRAND_VIOLET)
    d = ImageDraw.Draw(im)
    qr = qrcode.QRCode(version=None, error_correction=qrcode.constants.ERROR_CORRECT_M, box_size=3, border=2)
    qr.add_data("https://" + URL)
    qr.make(fit=True)
    q = qr.make_image(fill_color="black", back_color="white").convert("RGB")
    im.paste(q, (240 - q.width - 6, 8))
    logo = logo_image(40)
    im.paste(logo, (6, 4), logo)
    f = ImageFont.truetype(FONT_SANS_B, 11)
    d.text((6, 48), "Zaplanuj prawdziwą", font=f, fill=(250, 250, 250))
    d.text((6, 62), "budowę:", font=f, fill=(250, 250, 250))
    d.text((6, 80), URL, font=ImageFont.truetype(FONT_SANS_B, 10), fill=(255, 255, 255))
    d.rectangle([0, 104, 240, 105], fill=BRAND_ORANGE)
    screen_bg("end", im)
    return qr.version, q.size

if __name__ == "__main__":
    os.makedirs(G, exist_ok=True)
    os.makedirs(DOCS, exist_ok=True)
    print("font glyphs:", make_font())
    print("actor frames:", make_actors())
    print("hp bar frames:", make_hp_bar())
    import pixel_art as pa
    write_bmp(os.path.join(G, "particles.bmp"), pa.particle_frames(), 8, 8 * len(pa.PARTICLES), SPR_PAL, 4)
    write_json("particles", {"type": "sprite", "height": 8})
    write_bmp(os.path.join(G, "houses.bmp"), pa.house_frames(), 16, 16 * 25, SPR_PAL, 4)
    write_json("houses", {"type": "sprite", "height": 16})
    # osobna paleta (inaczej Butano współdzieli ją z postaciami i szarość ikony objęłaby bohatera)
    icon_pal = list(SPR_PAL); icon_pal[SK] = (0, 0, 0)   # kolor skóry nieużywany w ikonach
    write_bmp(os.path.join(G, "ability_icons.bmp"), pa.ability_icon_frames(), 16, 16 * len(pa.ABILITY_ICONS), icon_pal, 4)
    write_json("ability_icons", {"type": "sprite", "height": 16})
    print("phone tiles:", make_phone())
    make_tiles()
    make_title()
    print("QR version/size:", make_end())
    h = ["// WYGENEROWANE przez tools/make_assets.py - indeksy koloru tła ekranów (gradient HDMA).", "#pragma once", "",
         "namespace screen_info", "{"]
    h += [f"    constexpr int {n}_bg_index = {i};" for n, i in SCREEN_BG_INDEX.items()]
    h += ["}", ""]
    open(os.path.join(ROOT, "include", "screen_info.h"), "w", encoding="utf-8").write("\n".join(h))
