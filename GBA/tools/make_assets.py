#!/usr/bin/env python3
"""Generuje grafiki GBA (BMP + JSON dla Butano) z kodu.

Wejście: assets_src/pb_logo.svg (znak PlanBudowlany). Wszystko inne rysowane proceduralnie,
żeby dało się to podmienić pixel-artem z Aseprite bez zmiany kodu gry (te same nazwy plików).
Uruchom: python3 tools/make_assets.py  (wymaga: pillow, cairosvg, qrcode)
"""
import io, json, os, struct
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

BRAND_ORANGE = (242, 140, 40)
BRAND_NAVY = (27, 42, 65)
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
# 26 skrzynka z narzędziem
def make_actors():
    frames = [sprite(f) for f in CLASSES_SPR + ENEMY_SPR + PICKUP_SPR + UI_SPR]
    frames += [silhouette(sprite(f)) for f in CLASSES_SPR]
    frames += [sprite(p_toolbox)]
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
    # (podłoga, detal, ściana, jasny, cień)   Fundamenty, Stan surowy, Dach, Instalacje, Wykończenie
    [(96, 70, 44), (80, 58, 36), (150, 138, 118), (180, 170, 150), (100, 92, 80)],
    [(118, 118, 118), (100, 100, 100), (178, 82, 59), (222, 200, 170), (120, 50, 36)],
    [(169, 116, 59), (130, 86, 40), (122, 46, 46), (170, 80, 70), (80, 28, 28)],
    [(160, 160, 160), (135, 135, 135), (90, 111, 143), (200, 120, 60), (56, 70, 96)],
    [(224, 214, 192), (190, 178, 150), (111, 168, 160), (160, 210, 200), (70, 118, 110)],
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

TILES = [t_empty, t_floor, t_wall, t_walltop, t_stairs]

def make_tiles():
    px_tiles = [tile(f) for f in TILES]
    w = 8 * len(px_tiles)
    px = []
    for y in range(8):
        for ti in px_tiles:
            px += ti[y * 8:(y + 1) * 8]
    write_bmp(os.path.join(G, "tiles.bmp"), px, w, 8, [(0, 0, 0)] * 16, 4)
    write_json("tiles", {"type": "regular_bg_tiles", "bpp_mode": "bpp_4"})
    pal = []
    for c in STAGE_COLORS:
        stage = [(12, 12, 20)] + c + [(245, 211, 61), (180, 120, 20)] + [(0, 0, 0)] * 8
        pal += stage
    # palety 5..9: te same etapy przyciemnione (zapamiętane pola poza polem widzenia)
    for c in STAGE_COLORS:
        dim = [tuple(int(v * 0.5 + n * 0.25) for v, n in zip(col, BRAND_NAVY)) for col in c]
        pal += [(12, 12, 20)] + dim + [(100, 90, 40), (70, 50, 20)] + [(0, 0, 0)] * 8
    write_bmp(os.path.join(G, "stage_palettes.bmp"), [0] * 64, 8, 8, pal, 8)
    write_json("stage_palettes", {"type": "bg_palette", "bpp_mode": "bpp_4", "colors_count": 32 * len(STAGE_COLORS)})

# ---------------------------------------------------------------- 4. ekran tytułowy i końcowy (regular bg 256x256, 8bpp)
def logo_image(size):
    png = cairosvg.svg2png(url=os.path.join(SRC, "pb_logo.svg"), output_width=size, output_height=size)
    return Image.open(io.BytesIO(png)).convert("RGBA")

def screen_bg(name, img240):
    canvas = Image.new("RGB", (256, 256), BRAND_NAVY)
    canvas.paste(img240, (0, 0))
    px, pal = quantize(canvas, 64, first=BRAND_NAVY)
    write_bmp(os.path.join(G, name + ".bmp"), px, 256, 256, pal, 8)
    write_json(name, {"type": "regular_bg", "bpp_mode": "bpp_8"})
    canvas.crop((0, 0, 240, 160)).resize((480, 320), Image.NEAREST).save(os.path.join(DOCS, "preview_" + name + ".png"))

def centered(d, y, text, font, fill):
    w = d.textlength(text, font=font)
    d.text(((240 - w) / 2, y), text, font=font, fill=fill)

def make_title():
    im = Image.new("RGB", (240, 160), BRAND_NAVY)
    d = ImageDraw.Draw(im)
    # "plac budowy": pas ostrzegawczy u dołu
    for x in range(-20, 260, 16):
        d.polygon([(x, 160), (x + 8, 160), (x + 16, 148), (x + 8, 148)], fill=BRAND_ORANGE)
    d.rectangle([0, 146, 240, 147], fill=(20, 20, 20))
    logo = logo_image(80)
    im.paste(logo, (80, -6), logo)
    centered(d, 66, "PlanBudowlany", ImageFont.truetype(FONT_SANS_B, 22), (250, 250, 250))
    centered(d, 91, "ROGUELIKE", ImageFont.truetype(FONT_SANS_B, 14), BRAND_ORANGE)
    centered(d, 109, "Zbuduj dom. Przetrwaj budowę.", ImageFont.truetype(FONT_SANS, 10), (200, 200, 210))
    screen_bg("title", im)

def make_end():
    im = Image.new("RGB", (240, 160), BRAND_NAVY)
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
    d.text((6, 80), URL, font=ImageFont.truetype(FONT_SANS_B, 10), fill=BRAND_ORANGE)
    d.rectangle([0, 104, 240, 105], fill=BRAND_ORANGE)
    screen_bg("end", im)
    return qr.version, q.size

if __name__ == "__main__":
    os.makedirs(G, exist_ok=True)
    os.makedirs(DOCS, exist_ok=True)
    print("font glyphs:", make_font())
    print("actor frames:", make_actors())
    print("hp bar frames:", make_hp_bar())
    make_tiles()
    make_title()
    print("QR version/size:", make_end())
