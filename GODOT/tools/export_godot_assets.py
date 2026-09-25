#!/usr/bin/env python3
"""Eksport grafik, fontu i dźwięków z wersji GBA do projektu Godota (GODOT/godot/assets).

Skrypt tylko CZYTA pliki z katalogu GBA (graphics/*.bmp, audio/*, include/font_widths.h) i zapisuje PNG/WAV/OGG
do GODOT/godot/assets. Wynik jest deterministyczny (stałe ziarna losowania, stała kolejność), więc ponowne
uruchomienie bez zmian w GBA nie zmienia plików.

Co powstaje:
  sprites/actors.png         postacie, problemy budowy, znajdźki, bossowie: klatki 32x32 (Scale2x z 16x16 GBA),
                             kolejność klatek jak GBA/graphics/actors.bmp (patrz GBA/tools/make_assets.py);
                             sprites/actors_white.png - białe sylwetki do błysku trafienia
  sprites/actors_anim.png    animacje postaci i problemów budowy (klatki 32x32, wiersz = numer klatki z actors.png):
                             4 klatki chodu (A, A w kroku, B, B w kroku - tułów o piksel wyżej, nogi zostają) i klatka
                             oddechu (głowa i tułów o pół piksela GBA niżej); sprites/actors_anim_white.png - sylwetki
  sprites/truck.png          pickup z prologu (2 klatki kół), Scale2x
  sprites/particles.png      cząsteczki 16x16 (Scale2x z 8x8), kolejność jak particles.bmp
  sprites/houses.png         domy Osiedla 32x32
  sprites/menu_icons.png     menu akcji wokół bohatera 32x32; ui/menu_icons.png 16x16 (HUD: termos)
  sprites/ability_icons.png  moce zawodów 32x32; ui/ability_icons.png 16x16 (HUD) + ui/ability_icons_gray.png (ładowanie)
  ui/phone_icons.png         ikony zakładek telefonu (aktywne, fiolet) + ikona aplikacji PB (klatka 5)
  ui/phone_icons_dim.png     te same ikony w kolorze nieaktywnym (wycięte z phone_chrome.bmp)
  ui/title.png, ui/end.png   logo z napisem (tytuł) i plansza z kodem QR (koniec), tło przezroczyste
  tiles/stage_N.png          kafle 32x32 etapu N (8 etapów): 4 warianty podłogi, 2 podłogi z cieniem muru,
                             mur, lico muru, schody - rysowane w paletach etapów z GBA, bogatsze niż 8x8 z GBA
  fx/shadow.png, fx/danger.png, fx/range.png   cień pod postacią, pole zapowiedzianego ciosu, ramka zasięgu
  font/glyphs.png, font/glyphs_edge.png, font/font.json
                             pikselowy font 8x16 z polskimi znakami (litery i cień osobno, szerokości znaków)
  ui/touch_icons.png         ikony sterowania dotykiem 16x16 (klucz ustawień, telefon, mapa, link, zamknij, wstecz)
  ../icon.png                ikona aplikacji 1024x1024 (bohater z actors.png klatka 0 na fiolecie marki, pas ostrzegawczy)
  audio/sfx_*.wav            efekty (kopie 1:1), audio/music_*.mp3 - moduły .mod wyrenderowane przez
                             openmpt123 i zakodowane ffmpeg (libmp3lame; Godot gra MP3 bez dodatków) (bez tych narzędzi muzyka jest pomijana)

Uruchom z katalogu repozytorium:  python3 GODOT/tools/export_godot_assets.py
Wymaga: Pillow; opcjonalnie openmpt123 + ffmpeg (muzyka).
"""
import json
import os
import random
import re
import shutil
import subprocess
import sys
import tempfile

from PIL import Image

HERE = os.path.dirname(os.path.abspath(__file__))
REPO = os.path.dirname(os.path.dirname(HERE))
GBA = os.path.join(REPO, "GBA")
GFX = os.path.join(GBA, "graphics")
OUT = os.path.join(REPO, "GODOT", "godot", "assets")

TRANSPARENT = (0, 0, 0, 0)


# ------------------------------------------------------------------ odczyt BMP z GBA
def flat(im):
    return list(im.get_flattened_data()) if hasattr(im, "get_flattened_data") else list(im.getdata())


def load_indexed(name):
    """Zwraca (szerokość, wysokość, piksele jako lista indeksów, paleta RGB)."""
    im = Image.open(os.path.join(GFX, name + ".bmp"))
    assert im.mode == "P", name
    pal = im.getpalette()
    palette = [tuple(pal[i:i + 3]) for i in range(0, len(pal), 3)]
    return im.width, im.height, flat(im), palette


def frames_of(pixels, w, fh, count):
    """Pionowy pasek klatek (szerokość w, wysokość klatki fh) -> lista klatek (listy wierszy indeksów)."""
    out = []
    for f in range(count):
        out.append([pixels[(f * fh + y) * w:(f * fh + y + 1) * w] for y in range(fh)])
    return out


def scale2x(frame):
    """Scale2x (EPX) na indeksach palety: podwaja rozdzielczość i wygładza skosy bez nowych kolorów."""
    h, w = len(frame), len(frame[0])
    out = [[0] * (w * 2) for _ in range(h * 2)]

    def px(x, y):
        x = min(max(x, 0), w - 1)
        y = min(max(y, 0), h - 1)
        return frame[y][x]

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


def frames_to_image(frames, palette, transparent_index=0):
    """Klatki (indeksy) -> pionowy pasek RGBA; indeks przezroczysty -> alfa 0."""
    fh, fw = len(frames[0]), len(frames[0][0])
    im = Image.new("RGBA", (fw, fh * len(frames)), TRANSPARENT)
    px = im.load()
    for f, fr in enumerate(frames):
        for y in range(fh):
            for x in range(fw):
                i = fr[y][x]
                if i != transparent_index:
                    r, g, b = palette[i]
                    px[x, f * fh + y] = (r, g, b, 255)
    return im


def save(im, rel):
    path = os.path.join(OUT, rel)
    os.makedirs(os.path.dirname(path), exist_ok=True)
    im.save(path, optimize=False)
    return rel


def gray_palette(pal):
    """Paleta w skali szarości (jak set_grayscale_intensity na GBA: ikona mocy w trakcie ładowania)."""
    return [(int(0.3 * r + 0.59 * g + 0.11 * b),) * 3 for (r, g, b) in pal]


def white_palette(pal):
    """Biała sylwetka (błysk trafienia): każdy nieprzezroczysty piksel na biało."""
    return [(255, 255, 255)] * len(pal)


def export_sheet(name, frame_h, rel_hd, rel_1x=None, rel_gray=None, rel_white=None):
    w, h, pixels, pal = load_indexed(name)
    frames = frames_of(pixels, w, frame_h, h // frame_h)
    written = []
    if rel_1x:
        written.append(save(frames_to_image(frames, pal), rel_1x))
    if rel_gray:
        written.append(save(frames_to_image(frames, gray_palette(pal)), rel_gray))
    if rel_hd:
        written.append(save(frames_to_image([scale2x(f) for f in frames], pal), rel_hd))
    if rel_white:
        written.append(save(frames_to_image([scale2x(f) for f in frames], white_palette(pal)), rel_white))
    return len(frames), written


# ------------------------------------------------------------------ animacje chodu i oddechu
ANIM_FRAMES = 5          # chód x4 + oddech
CHARACTER_FRAMES = list(range(0, 15)) + [46, 47]   # zawody, problemy budowy, bossowie (jak anim_b w main.cpp)


def anim_b(f):
    return f + 27 if f < 15 else f + 2


def rows_span(frame):
    """Pierwszy i ostatni niepusty wiersz klatki (indeks 0 = przezroczysty)."""
    rows = [y for y, row in enumerate(frame) if any(row)]
    return (rows[0], rows[-1]) if rows else (0, len(frame) - 1)


def step_frame(frame):
    """Klatka „w kroku”: wszystko nad stopami (4 dolne wiersze sylwetki) o piksel wyżej, stopy zostają."""
    top, bottom = rows_span(frame)
    feet = max(top, bottom - 3)
    out = [row[:] for row in frame]
    for y in range(max(top - 1, 0), feet):
        out[y] = frame[y + 1][:] if y + 1 < feet else frame[feet - 1][:] if feet - 1 >= 0 else out[y]
    if top > 0:
        out[top - 1] = frame[top][:]
    return out


def breath_frame(frame2x):
    """Oddech (w 32x32 po Scale2x): górne 55% sylwetki o piksel niżej - pierś „opada”."""
    top, bottom = rows_span(frame2x)
    mid = top + int((bottom - top) * 0.55)
    out = [row[:] for row in frame2x]
    for y in range(mid, top, -1):
        out[y] = frame2x[y - 1][:]
    out[top] = [0] * len(frame2x[0])
    return out


def export_actor_anims():
    """sprites/actors_anim.png: w wierszu klatki postaci 5 klatek (chód A, A w kroku, B, B w kroku, oddech)."""
    w, h, pixels, pal = load_indexed("actors")
    frames = frames_of(pixels, w, 16, h // 16)
    count = len(frames)
    sheet = Image.new("RGBA", (32 * ANIM_FRAMES, 32 * count), TRANSPARENT)
    white = Image.new("RGBA", (32 * ANIM_FRAMES, 32 * count), TRANSPARENT)
    for f in CHARACTER_FRAMES:
        a, b = frames[f], frames[anim_b(f)]
        cells = [scale2x(a), scale2x(step_frame(a)), scale2x(b), scale2x(step_frame(b))]
        cells.append(breath_frame(cells[0]))
        for k, cell in enumerate(cells):
            sheet.paste(frames_to_image([cell], pal), (32 * k, 32 * f))
            white.paste(frames_to_image([cell], white_palette(pal)), (32 * k, 32 * f))
    save(sheet, "sprites/actors_anim.png")
    save(white, "sprites/actors_anim_white.png")
    return len(CHARACTER_FRAMES)


# ------------------------------------------------------------------ telefon: ikony zakładek
def export_phone_icons():
    w, h, pixels, pal = load_indexed("phone_icons")
    frames = frames_of(pixels, w, 16, h // 16)
    save(frames_to_image(frames, pal), "ui/phone_icons.png")
    # nieaktywne ikony: z ramki telefonu (środki ikon co 48 px od x=24, górna krawędź y=134), tło karty -> przezroczyste
    cw, ch, cpx, cpal = load_indexed("phone_chrome")
    card = cpal.index((255, 255, 255))
    dim = []
    for i in range(5):
        x0, y0 = 24 + 48 * i - 8, 134
        dim.append([[cpx[(y0 + y) * cw + x0 + x] for x in range(16)] for y in range(16)])
    save(frames_to_image(dim, cpal, transparent_index=card), "ui/phone_icons_dim.png")
    return len(frames)


# ------------------------------------------------------------------ dotyk: ikony paska akcji i ustawień
TOUCH_PAL = {"K": (16, 16, 24), "W": (250, 250, 250), "l": (200, 204, 214), "g": (130, 134, 148),
             "B": (107, 78, 255), "b": (180, 166, 255), "O": (255, 122, 61), "Y": (255, 215, 90), "D": (54, 58, 72)}
TOUCH_ICONS = [
    [   # 0 klucz płaski (ustawienia)
        "................", ".........KKK....", "........KllK.KK.", "........KlK.KlK.", "........KlKKllK.",
        ".......KlllllK..", "......KllllgK...", ".....KlllgKK....", "....KlllgK......", "...KlllgK.......",
        "..KlllgK........", ".KlllgK.........", ".KllgK..........", "..KKK...........", "................", "................"],
    [   # 1 telefon z aplikacją PB
        "................", ".....KKKKKK.....", "....KDDDDDDK....", "....KDKKKKDK....", "....KDBBBBDK....",
        "....KDBWWBDK....", "....KDBWBBDK....", "....KDBWWBDK....", "....KDBWBBDK....", "....KDBBBBDK....",
        "....KDBBBBDK....", "....KDDDDDDK....", "....KDDWWDDK....", ".....KKKKKK.....", "................", "................"],
    [   # 2 mapa etapu (składana)
        "................", "................", "..KKK..KKK..KKK.", ".KYYYKKlllKKYYYK", ".KYYYKlllKKYYYYK", ".KYOYKllOlKYYYYK",
        ".KYYYKlOlOKYOYYK", ".KYYYKllllKYYYYK", ".KYYOKlllKKYYYYK", ".KYYYKlllKKYYYYK", ".KYYYKllllKYYOYK",
        ".KYYYKKlllKKYYYK", "..KKK..KKK..KKK.", "................", "................", "................"],
    [   # 3 link (globus)
        "................", ".....KKKKKK.....", "....KbBWWBbK....", "...KbBWBBWBbK...", "..KbBWBBBBWBbK..",
        "..KWWWWWWWWWWK..", "..KBWBBBBBBWBK..", "..KBWBBBBBBWBK..", "..KWWWWWWWWWWK..", "..KbBWBBBBWBbK..",
        "...KbBWBBWBbK...", "....KbBWWBbK....", ".....KKKKKK.....", "................", "................", "................"],
    [   # 4 zamknij
        "................", "................", "...KK......KK...", "..KWWK....KWWK..", "...KWWK..KWWK...",
        "....KWWKKWWK....", ".....KWWWWK.....", "......KWWK......", ".....KWWWWK.....", "....KWWKKWWK....",
        "...KWWK..KWWK...", "..KWWK....KWWK..", "...KK......KK...", "................", "................", "................"],
    [   # 5 wstecz
        "................", "................", ".......KK.......", "......KWWK......", ".....KWWK.......",
        "....KWWK........", "...KWWWKKKKKKK..", "..KWWWWWWWWWWWK.", "...KWWWKKKKKKK..", "....KWWK........",
        ".....KWWK.......", "......KWWK......", ".......KK.......", "................", "................", "................"],
]


def export_touch_icons():
    im = Image.new("RGBA", (16, 16 * len(TOUCH_ICONS)), TRANSPARENT)
    px = im.load()
    for f, rows in enumerate(TOUCH_ICONS):
        assert len(rows) == 16 and all(len(r) == 16 for r in rows), f
        for y, row in enumerate(rows):
            for x, ch in enumerate(row):
                if ch != ".":
                    px[x, f * 16 + y] = TOUCH_PAL[ch] + (255,)
    save(im, "ui/touch_icons.png")
    return len(TOUCH_ICONS)


# ------------------------------------------------------------------ ikona aplikacji (iOS / Android)
def make_app_icon():
    """1024x1024: bohater (actors.png klatka 0, 32x32) powiększony bez wygładzania na fiolecie marki,
    u dołu pas ostrzegawczy placu budowy (pomarańczowe skosy) - jak ekran tytułowy."""
    size, brand, accent, dark = 1024, (107, 78, 255), (255, 122, 61), (22, 20, 30)
    icon = Image.new("RGBA", (size, size), brand + (255,))
    actors = Image.open(os.path.join(OUT, "sprites", "actors.png")).convert("RGBA")
    hero = actors.crop((0, 0, 32, 32))
    box = hero.getbbox()
    hero = hero.crop(box)
    k = 23
    big = hero.resize((hero.width * k, hero.height * k), Image.NEAREST)
    band_top = 860
    icon.alpha_composite(big, ((size - big.width) // 2, (band_top - big.height) // 2 + 20))
    px = icon.load()
    for y in range(band_top, size):
        for x in range(size):
            if y < band_top + 20:
                px[x, y] = dark + (255,)
            else:
                px[x, y] = accent + (255,) if ((x + (size - y)) // 45) % 2 == 0 else brand + (255,)
    path = os.path.join(REPO, "GODOT", "godot", "icon.png")
    icon.convert("RGB").save(path, optimize=False)
    return os.path.relpath(path, REPO)


# ------------------------------------------------------------------ ekran tytułowy i końcowy
def export_screen(name, crop_h, rel):
    """Plansza 240 x crop_h z bmp 8bpp; najczęstszy kolor (tło, na GBA gradient HDMA) staje się przezroczysty."""
    im = Image.open(os.path.join(GFX, name + ".bmp"))
    pixels = flat(im)
    counts = {}
    for y in range(160):
        for x in range(240):
            v = pixels[y * 256 + x]
            counts[v] = counts.get(v, 0) + 1
    bg = max(counts, key=counts.get)
    pal = im.getpalette()
    out = Image.new("RGBA", (240, crop_h), TRANSPARENT)
    px = out.load()
    for y in range(crop_h):
        for x in range(240):
            v = pixels[y * 256 + x]
            if v != bg:
                px[x, y] = (pal[v * 3], pal[v * 3 + 1], pal[v * 3 + 2], 255)
    save(out, rel)


# ------------------------------------------------------------------ font 8x16 z polskimi znakami
PL_CHARS = "ąćęłńóśźżĄĆĘŁŃÓŚŹŻ"


def export_font():
    w, h, pixels, pal = load_indexed("font_8x16")
    count = h // 16
    text = open(os.path.join(GBA, "include", "font_widths.h"), encoding="utf-8").read()
    widths = [int(v) for v in re.search(r"\{([^}]*)\}", text).group(1).split(",")]
    chars = "".join(chr(c) for c in range(33, 127)) + PL_CHARS
    assert count == len(chars) and len(widths) == len(chars) + 1, (count, len(chars), len(widths))
    cols = 16
    rows = (count + cols - 1) // cols
    letters = Image.new("RGBA", (cols * 8, rows * 16), TRANSPARENT)
    edges = Image.new("RGBA", (cols * 8, rows * 16), TRANSPARENT)
    lp, ep = letters.load(), edges.load()
    for g in range(count):
        ox, oy = (g % cols) * 8, (g // cols) * 16
        for y in range(16):
            for x in range(8):
                v = pixels[(g * 16 + y) * w + x]
                if v == 1:
                    lp[ox + x, oy + y] = (255, 255, 255, 255)
                elif v == 2:
                    ep[ox + x, oy + y] = (255, 255, 255, 255)
    save(letters, "font/glyphs.png")
    save(edges, "font/glyphs_edge.png")
    meta = {"cell": [8, 16], "cols": cols, "space": widths[0], "chars": chars, "widths": widths[1:],
            "source": "GBA/graphics/font_8x16.bmp + GBA/include/font_widths.h"}
    with open(os.path.join(OUT, "font", "font.json"), "w", encoding="utf-8") as f:
        json.dump(meta, f, ensure_ascii=False, indent=1)
    return count


# ------------------------------------------------------------------ kafle etapów 32x32
def stage_colors(i):
    """Paleta pełnego światła etapu z GBA: podłoga, detal, ściana, jasny, cień, schody x2, cień podłogi, zagrożenie."""
    _, _, _, pal = load_indexed("stage_palettes_%d" % i)
    return {"floor": pal[1], "detail": pal[2], "wall": pal[3], "light": pal[4], "shadow": pal[5],
            "stairs": pal[6], "stairs2": pal[7], "floor_shadow": pal[8], "danger": pal[9]}


def mix(a, b, t):
    return tuple(int(round(x + (y - x) * t)) for x, y in zip(a, b))


def shade(c, f):
    return tuple(max(0, min(255, int(round(v * f)))) for v in c)


class Canvas:
    def __init__(self, base):
        self.im = Image.new("RGBA", (32, 32), base + (255,))
        self.px = self.im.load()

    def set(self, x, y, c):
        if 0 <= x < 32 and 0 <= y < 32:
            self.px[x, y] = tuple(c) + (255,)

    def get(self, x, y):
        return self.px[x % 32, y % 32][:3]

    def rect(self, x0, y0, x1, y1, c):
        for y in range(y0, y1 + 1):
            for x in range(x0, x1 + 1):
                self.set(x, y, c)

    def hline(self, x0, x1, y, c):
        self.rect(x0, y, x1, y, c)

    def vline(self, x, y0, y1, c):
        self.rect(x, y0, x, y1, c)


# rodzaje podłóg i murów etapów (kolejność jak etapy w GBA/data/game.json)
FLOOR_KIND = ["dirt", "screed", "slab", "planks", "planks", "screed", "screed", "tiles"]
WALL_KIND = ["formwork", "brick", "concrete", "rooftile", "plaster", "pipes", "plaster", "tiles"]


def draw_floor(c, col, kind, rnd):
    fl, dt = col["floor"], col["detail"]
    if kind == "dirt":   # ziemia z kamykami i grudkami
        for _ in range(26):
            x, y = rnd.randrange(32), rnd.randrange(32)
            c.set(x, y, dt)
            if rnd.random() < 0.4:
                c.set(x + 1, y, dt)
        for _ in range(5):
            x, y = rnd.randrange(1, 30), rnd.randrange(1, 30)
            c.set(x, y, mix(fl, col["light"], 0.5))
            c.set(x + 1, y, mix(fl, col["light"], 0.3))
            c.set(x, y + 1, shade(dt, 0.8))
    elif kind in ("screed", "slab"):   # wylewka: drobne ziarno, w stropie fugi płyt
        for _ in range(34):
            c.set(rnd.randrange(32), rnd.randrange(32), dt if rnd.random() < 0.7 else mix(fl, (255, 255, 255), 0.12))
        if kind == "slab":
            c.hline(0, 31, 31, shade(dt, 0.85))
            c.vline(31, 0, 31, shade(dt, 0.85))
            c.hline(0, 30, 0, mix(fl, (255, 255, 255), 0.08))
    elif kind == "planks":   # deski z usłojeniem i gwoździami
        for band in range(4):
            y0 = band * 8
            c.hline(0, 31, y0 + 7, shade(dt, 0.8))
            c.hline(0, 31, y0, mix(fl, (255, 255, 255), 0.08))
            seam = (band * 13 + rnd.randrange(6)) % 32
            c.vline(seam, y0, y0 + 6, shade(dt, 0.85))
            for _ in range(3):
                gx, gy = rnd.randrange(32), y0 + 2 + rnd.randrange(4)
                c.hline(gx, gx + 3, gy, dt)
            c.set((seam + 2) % 32, y0 + 3, shade(dt, 0.6))
    elif kind == "tiles":   # płytki 16x16 z fugą
        grout = shade(dt, 0.9)
        for k in (0, 16):
            c.hline(0, 31, k, grout)
            c.vline(k, 0, 31, grout)
        for (x0, y0) in ((1, 1), (17, 1), (1, 17), (17, 17)):
            c.hline(x0, x0 + 13, y0, mix(fl, (255, 255, 255), 0.25))
            if rnd.random() < 0.5:
                c.set(x0 + 3 + rnd.randrange(8), y0 + 4 + rnd.randrange(8), dt)


def draw_wall(c, col, kind, rnd):
    wl, lt, sh = col["wall"], col["light"], col["shadow"]
    if kind == "brick":   # cegły 16x8 w przewiązce, jasna fuga
        mortar = mix(lt, wl, 0.25)
        for row in range(4):
            y0 = row * 8
            c.hline(0, 31, y0 + 7, mortar)
            off = 0 if row % 2 == 0 else 8
            for k in range(3):
                x = (off + k * 16) % 32
                c.vline(x, y0, y0 + 6, mortar)
            for k in range(3):
                bx = (off + 1 + k * 16) % 32
                c.hline(bx, min(bx + 13, 31), y0, mix(wl, lt, 0.35))
                c.hline(bx, min(bx + 13, 31), y0 + 6, shade(wl, 0.8))
            for _ in range(4):
                c.set(rnd.randrange(32), y0 + 1 + rnd.randrange(5), shade(wl, 0.9 + rnd.random() * 0.2))
    elif kind == "formwork":   # szalunek: poziome deski i ściągi
        for band in range(4):
            y0 = band * 8
            c.hline(0, 31, y0, mix(wl, lt, 0.45))
            c.hline(0, 31, y0 + 7, sh)
            for _ in range(5):
                c.set(rnd.randrange(32), y0 + 2 + rnd.randrange(4), shade(wl, 0.88))
        for x in (7, 23):
            c.rect(x, 3, x + 1, 4, shade(sh, 0.7))
            c.rect(x, 19, x + 1, 20, shade(sh, 0.7))
    elif kind == "concrete":   # beton z porami i otworami po ściągach
        for _ in range(40):
            c.set(rnd.randrange(32), rnd.randrange(32), shade(wl, 0.86 if rnd.random() < 0.6 else 1.1))
        for (x, y) in ((6, 8), (24, 8), (6, 24), (24, 24)):
            c.rect(x, y, x + 1, y + 1, sh)
            c.set(x, y, shade(sh, 0.7))
        c.hline(0, 31, 15, shade(wl, 0.9))
    elif kind == "rooftile":   # dachówki w łuskę
        for row in range(4):
            y0 = row * 8
            off = 0 if row % 2 == 0 else 4
            for k in range(-1, 4):
                x0 = off + k * 8
                for x in range(8):
                    arc = 5 + (1 if x in (0, 7) else 0) - (1 if 2 <= x <= 5 else 0)
                    c.set(x0 + x, y0 + arc + 1, sh)
                    c.set(x0 + x, y0 + 1, mix(wl, lt, 0.5) if 1 <= x <= 6 else wl)
                c.vline(x0, y0, y0 + 6, shade(sh, 1.1))
    elif kind == "plaster":   # tynk z pociągnięciami pacy
        for _ in range(6):
            x, y = rnd.randrange(32), rnd.randrange(32)
            c.hline(x, x + 5 + rnd.randrange(6), y, mix(wl, lt, 0.5))
            c.hline(x + 1, x + 4, y + 1, shade(wl, 0.93))
        for _ in range(20):
            c.set(rnd.randrange(32), rnd.randrange(32), shade(wl, 0.95))
    elif kind == "pipes":   # instalacje: rury w bruzdach
        for _ in range(20):
            c.set(rnd.randrange(32), rnd.randrange(32), shade(wl, 0.9))
        pipe = lt
        c.rect(0, 9, 31, 11, pipe)
        c.hline(0, 31, 9, mix(pipe, (255, 255, 255), 0.35))
        c.hline(0, 31, 12, sh)
        c.rect(20, 0, 22, 31, shade(wl, 1.2))
        c.vline(20, 0, 31, mix(wl, (255, 255, 255), 0.3))
        c.vline(23, 0, 31, sh)
        c.rect(19, 8, 23, 13, mix(pipe, sh, 0.3))
    elif kind == "tiles":   # glazura 8x8
        grout = mix(lt, (255, 255, 255), 0.3)
        for k in range(0, 32, 8):
            c.hline(0, 31, k, grout)
            c.vline(k, 0, 31, grout)
        for (x, y) in ((1, 1), (9, 9), (17, 1), (25, 17), (1, 25), (17, 25)):
            c.hline(x, x + 5, y, mix(wl, (255, 255, 255), 0.35))


def make_stage_tiles(i):
    col = stage_colors(i)
    rnd = random.Random(1000 + i)
    cells = []
    for v in range(4):   # podłoga: 4 warianty wybierane w grze skrótem pozycji pola
        c = Canvas(col["floor"])
        draw_floor(c, col, FLOOR_KIND[i], rnd)
        cells.append(c)
    for v in range(2):   # podłoga pod murem: pas cienia u góry (jak kafel 5 na GBA)
        c = Canvas(col["floor"])
        c.im.paste(cells[v].im, (0, 0))
        c.px = c.im.load()
        for y in range(10):
            a = 0.62 if y < 5 else (0.62 - (y - 4) * 0.1)
            for x in range(32):
                if y < 5 or (x + y) % 2 == 0 or y < 7:
                    c.set(x, y, mix(c.get(x, y), col["floor_shadow"], a))
        cells.append(c)
    wall = Canvas(col["wall"])
    draw_wall(wall, col, WALL_KIND[i], rnd)
    cells.append(wall)
    face = Canvas(col["wall"])   # lico muru nad podłogą: jasna korona u góry, ciemniejszy dół
    face.im.paste(wall.im, (0, 0))
    face.px = face.im.load()
    face.rect(0, 0, 31, 3, col["light"])
    face.hline(0, 31, 4, mix(col["light"], col["wall"], 0.5))
    face.hline(0, 31, 0, mix(col["light"], (255, 255, 255), 0.35))
    for y in range(26, 32):
        for x in range(32):
            face.set(x, y, mix(face.get(x, y), col["shadow"], 0.25 + (y - 26) * 0.1))
    cells.append(face)
    st = Canvas(col["wall"])   # schody w dół: boczne ściany, stopnie coraz ciemniejsze, u dołu otwór
    for k in range(5):
        y0 = 2 + k * 6
        c0 = mix(col["stairs"], col["stairs2"], k / 4.0)
        c0 = mix(c0, (12, 12, 20), k * 0.14)
        st.rect(4, y0, 27, y0 + 5, c0)
        st.hline(4, 27, y0, mix(c0, (255, 255, 255), 0.35))
        st.hline(4, 27, y0 + 5, shade(c0, 0.55))
    st.rect(0, 0, 3, 31, col["wall"])
    st.rect(28, 0, 31, 31, col["wall"])
    st.vline(3, 0, 31, col["shadow"])
    st.vline(28, 0, 31, col["light"])
    st.rect(0, 0, 31, 1, col["light"])
    st.rect(4, 30, 27, 31, (12, 12, 20))
    cells.append(st)
    sheet = Image.new("RGBA", (32 * len(cells), 32), TRANSPARENT)
    for k, c in enumerate(cells):
        sheet.paste(c.im, (k * 32, 0))
    save(sheet, "tiles/stage_%d.png" % i)
    return col


def make_fx(danger):
    # cień pod postacią: elipsa (kształt jak kafel cienia na GBA, podwojony), półprzezroczysta
    im = Image.new("RGBA", (24, 8), TRANSPARENT)
    px = im.load()
    for y in range(8):
        for x in range(24):
            dx, dy = (x - 11.5) / 11.5, (y - 3.5) / 3.6
            d = dx * dx + dy * dy
            if d <= 1.0:
                px[x, y] = (8, 6, 16, 150 if d < 0.55 else 95)
    save(im, "fx/shadow.png")
    # zapowiedziany cios bossa: czerwona ramka + ukośne kreski (kafel 8 na GBA w 32x32)
    im = Image.new("RGBA", (32, 32), TRANSPARENT)
    px = im.load()
    for y in range(32):
        for x in range(32):
            edge = x < 2 or y < 2 or x > 29 or y > 29
            stripe = (x + y) % 8 < 2
            if edge:
                px[x, y] = danger + (235,)
            elif stripe:
                px[x, y] = danger + (150,)
            else:
                px[x, y] = danger + (60,)
    save(im, "fx/danger.png")
    # pole w zasięgu broni: narożniki ramki (kafel 7 na GBA)
    im = Image.new("RGBA", (32, 32), TRANSPARENT)
    px = im.load()
    for (cx, cy, sx, sy) in ((0, 0, 1, 1), (31, 0, -1, 1), (0, 31, 1, -1), (31, 31, -1, -1)):
        for k in range(6):
            for t in range(2):
                px[cx + sx * k, cy + sy * t] = (255, 230, 120, 230)
                px[cx + sx * t, cy + sy * k] = (255, 230, 120, 230)
    save(im, "fx/range.png")


# ------------------------------------------------------------------ dźwięk
def export_audio():
    src = os.path.join(GBA, "audio")
    dst = os.path.join(OUT, "audio")
    os.makedirs(dst, exist_ok=True)
    sfx = sorted(f for f in os.listdir(src) if f.endswith(".wav"))
    for f in sfx:
        shutil.copyfile(os.path.join(src, f), os.path.join(dst, f))
    music = []
    mpt, ff = shutil.which("openmpt123"), shutil.which("ffmpeg")
    mods = sorted(f for f in os.listdir(src) if f.endswith(".mod"))
    if not (mpt and ff):
        print("UWAGA: brak openmpt123/ffmpeg - muzyka (.mod) pominięta")
        return sfx, music
    with tempfile.TemporaryDirectory() as tmp:
        for f in mods:
            base = os.path.splitext(f)[0]
            mod = os.path.join(tmp, f)
            shutil.copyfile(os.path.join(src, f), mod)
            subprocess.run([mpt, "--quiet", "--render", "--samplerate", "44100", "--channels", "2", mod],
                           check=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
            wav = mod + ".wav"
            mp3 = os.path.join(dst, base + ".mp3")
            subprocess.run([ff, "-y", "-loglevel", "error", "-i", wav, "-map_metadata", "-1", "-fflags", "+bitexact",
                            "-id3v2_version", "0", "-write_xing", "0", "-ar", "44100", "-ac", "2",
                            "-c:a", "libmp3lame", "-b:a", "128k", mp3], check=True)
            music.append(base + ".mp3")
    return sfx, music


def main():
    if not os.path.isdir(GFX):
        sys.exit("Brak katalogu " + GFX)
    os.makedirs(OUT, exist_ok=True)
    report = {}
    report["actors"] = export_sheet("actors", 16, "sprites/actors.png", rel_white="sprites/actors_white.png")[0]
    report["actor_anims"] = export_actor_anims()
    report["truck"] = export_sheet("truck", 16, "sprites/truck.png")[0]
    report["particles"] = export_sheet("particles", 8, "sprites/particles.png")[0]
    report["houses"] = export_sheet("houses", 16, "sprites/houses.png")[0]
    report["menu_icons"] = export_sheet("menu_icons", 16, "sprites/menu_icons.png", "ui/menu_icons.png")[0]
    report["ability_icons"] = export_sheet("ability_icons", 16, "sprites/ability_icons.png", "ui/ability_icons.png",
                                           "ui/ability_icons_gray.png")[0]
    report["phone_icons"] = export_phone_icons()
    report["touch_icons"] = export_touch_icons()
    report["app_icon"] = make_app_icon()
    export_screen("title", 128, "ui/title.png")
    export_screen("end", 104, "ui/end.png")
    report["font_glyphs"] = export_font()
    danger = None
    for i in range(8):
        col = make_stage_tiles(i)
        danger = danger or col["danger"]
    make_fx(danger)
    sfx, music = export_audio()
    report["sfx"] = len(sfx)
    report["music"] = music
    print("Eksport do", os.path.relpath(OUT, REPO), json.dumps(report, ensure_ascii=False))


if __name__ == "__main__":
    main()
