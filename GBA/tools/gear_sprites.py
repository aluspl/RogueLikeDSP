#!/usr/bin/env python3
"""Nakładki sprzętu na bohatera (v0.23, TODO #88): Kask, Rękawice, Kamizelka, Buty, Pas × 3 rzadkości.

Uruchom z katalogu repozytorium:   python3 GBA/tools/gear_sprites.py
Wynik: art/gear/ (PNG + gear.json, opis w art/gear/README.md). Wymaga Pillow.

Zasady:
  * wszystkie 12 zawodów mają tę samą sylwetkę (WORKER_TOP + nogi A/B w pixel_art.py), więc nakładka jest jedna
    dla wszystkich - skrypt to sprawdza, składając każdy zawód z każdą nakładką (także po Scale2x, kroku i oddechu);
  * nakładka zmienia tylko piksele wewnątrz sylwetki (obrys K bohatera zostaje), wyjątek: sakwa pasa i kabura;
  * własna paleta 16 kolorów (GBA 4bpp, osobny bank palety sprite'a), barwy wg rzadkości spójne z bronią:
    Zwykły - stonowany (biały kask, szarozielona kamizelka, brąz), Solidny - morski niebieski, Markowy - żółty+czarny;
  * każda rzadkość ma też cechę bez koloru (daltonizm): Solidny kask z latarką, kamizelka pikowana, buty ze stalowym
    podnoskiem, pas z kaburą; Markowy kask z czarnym grzebieniem, czarny dół kamizelki, pas z dwiema sakwami.
Bez nazw i barw cudzych marek.
"""
import json
import os
import sys

from PIL import Image, ImageDraw, ImageFont

HERE = os.path.dirname(os.path.abspath(__file__))
REPO = os.path.dirname(os.path.dirname(HERE))
OUT = os.path.join(REPO, "art", "gear")
GAME_JSON = os.path.join(REPO, "GBA", "data", "game.json")
sys.path.insert(0, HERE)
sys.path.insert(0, os.path.join(REPO, "GODOT", "tools"))
import pixel_art as pa                                                       # noqa: E402
from export_godot_assets import scale2x, step_frame, breath_frame          # noqa: E402

# ------------------------------------------------------------------ paleta nakładek (16 kolorów, idx 0 przezroczysty)
GEAR_CODES = {".": 0, "K": 1, "W": 2, "l": 3, "g": 4, "d": 5, "G": 6, "F": 7, "S": 8, "s": 9, "Z": 10,
              "Y": 11, "y": 12, "N": 13, "T": 14, "t": 15}
GEAR_PAL = [(255, 0, 255), (26, 26, 26), (240, 240, 240), (200, 200, 200), (138, 138, 138), (84, 84, 94),
            (122, 140, 112), (84, 100, 78),                     # G / F szarozielony (Zwykły) + cień
            (32, 150, 164), (18, 72, 96), (132, 214, 222),      # S / s / Z morski (Solidny) + głęboki + blik
            (246, 192, 32), (186, 132, 18),                     # Y / y żółty (Markowy) + cień
            (36, 36, 40),                                       # N czerń (Markowy)
            (139, 90, 43), (196, 140, 78)]                      # T / t skóra (brąz) pasa i butów
PAL_NAMES = ["przezroczysty", "obrys", "biel", "jasnoszary", "szary", "ciemnoszary", "szarozielony",
             "szarozielony cień", "morski", "głęboki morski", "morski blik", "żółty", "żółty cień", "czerń",
             "brąz", "jasny brąz"]

SLOTS = ["kask", "rekawice", "kamizelka", "buty", "pas"]          # kolejność jak equipment.slots w game.json
RARITIES = ["Zwykły", "Solidny", "Markowy"]
RAR_KEYS = ["zwykly", "solidny", "markowy"]
DIRS = ["down", "up", "left", "right"]
DRAW_ORDER = ["buty", "kamizelka", "pas", "rekawice", "kask"]
GODOT_FRAMES = ["walkA", "walkAstep", "walkB", "walkBstep", "breath"]   # jak actors_anim.png (AnimWalk0..3, AnimBreath)

# ------------------------------------------------------------------ mapy nakładek (wiersz: 16 znaków, "." = bez zmian)
# Sylwetka bohatera (pixel_art.WORKER_TOP): kask wiersze 1-4, twarz 5-8, kamizelka 9-12 (x 5-10), dłonie x=3 i x=12
# w wierszach 10-11, pas = wiersz 13 (góra spodni), buty = wiersze 14-15 (inne w klatce A i B).
HELMET = {
    0: {2: ".....WWWWWl.....",          # kask budowlany: biały, cień jasnoszary, szare rondo
        3: "....WWWWWWll....",
        4: "....gggggggg...."},
    1: {2: ".....SZSSSs.....",          # kask z latarką: morski, blik, latarka na środku czoła
        3: "....SZSYWSss....",
        4: "....ssssssss...."},
    2: {1: ".......NN.......",          # kask markowy: żółty z czarnym grzebieniem i czarnym rondem
        2: ".....YWNNYy.....",
        3: "....YYYNNYyy....",
        4: "....NNNNNNNN...."},
}
GLOVES = {
    0: {10: "...g........g...", 11: "...l........l..."},     # robocze: jasnoszare, szary mankiet
    1: {10: "...s........s...", 11: "...S........S..."},     # wzmacniane: morskie, głęboki mankiet
    2: {10: "...N........N...", 11: "...Y........Y..."},     # markowe: żółte, czarny mankiet
}
VEST = {   # rozpięta kamizelka: boki x 5-6 i 9-10, środek (kolor zawodu) widać
    0: {9: ".....GG..GG.....", 10: ".....lW..Wl.....", 11: ".....GF..FG.....", 12: "......F..F......"},  # odblaskowa
    1: {9: ".....SS..SS.....", 10: ".....sS..Ss.....", 11: ".....Ss..sS.....", 12: "......s..s......"},  # ocieplana, pikowana
    2: {9: ".....YY..YY.....", 10: ".....WW..WW.....", 11: ".....Yy..yY.....", 12: "......N..N......"},  # markowa
}
BELT = {   # wiersz 13 = góra spodni (x 6-9), sakwa / kabura na brzegu sylwetki (x 5 / x 10)
    0: {13: ".....TTlTt......"},                     # narzędziowy: skóra, metalowa sprzączka, sakwa z lewej
    1: {13: ".....dsZsS......", 14: "..........S....."},   # monterski: morski, kabura z prawej
    2: {13: "......NYYN......"},                     # markowy: czarny z szeroką żółtą sprzączką
}
BOOTS = {   # (klatka A, klatka B): nogi A - stopy x 5-7 i 9-11, B - x 4-6 i 9-11
    0: ({14: "......d..d......", 15: "......T...T....."}, {14: ".....d....d.....", 15: ".....T....T....."}),
    1: ({14: "......s..s......", 15: "......l...l....."}, {14: ".....s....s.....", 15: ".....l....l....."}),
    2: ({14: "......N..N......", 15: "......Y...Y....."}, {14: ".....N....N.....", 15: ".....Y....Y....."}),
}


def overlay16(slot, rarity, frame):
    """Nakładka 16x16 (lista wierszy indeksów GEAR_PAL) dla klatki bohatera A (0) / B (1)."""
    src = {"kask": HELMET, "rekawice": GLOVES, "kamizelka": VEST, "pas": BELT}.get(slot)
    rows = BOOTS[rarity][frame] if slot == "buty" else src[rarity]
    g = [[0] * 16 for _ in range(16)]
    for y, row in rows.items():
        assert len(row) == 16, (slot, rarity, y, row)
        for x, ch in enumerate(row):
            g[y][x] = GEAR_CODES[ch]
    return g


# ------------------------------------------------------------------ składanie z bohaterem
OFF = 16   # indeksy nakładki w złożonej klatce: 16 + idx GEAR_PAL (bohater: 0-15 = SPR_PAL)


def hero16(c, frame):
    px = pa.worker_frame(c, frame)
    return [px[y * 16:(y + 1) * 16] for y in range(16)]


def compose(base, over):
    return [[(OFF + o) if o else b for b, o in zip(rb, ro)] for rb, ro in zip(base, over)]


def mask_of(over):
    return [[1 if v else 0 for v in r] for r in over]


def mask2x(m):
    return [[m[y // 2][x // 2] for x in range(len(m[0]) * 2)] for y in range(len(m) * 2)]


def godot_cells(frame16):
    """Jak export_godot_assets.export_actor_anims: chód A, A w kroku, B, B w kroku (z klatek A/B) + oddech."""
    a, b = frame16
    cells = [scale2x(a), scale2x(step_frame(a)), scale2x(b), scale2x(step_frame(b))]
    cells.append(breath_frame(cells[0]))
    return cells


def mask_cells(ma, mb):
    """Te same przesunięcia wierszy dla maski (krok i oddech tylko przesuwają wiersze)."""
    cells = [mask2x(ma), mask2x(step_frame_like(ma)), mask2x(mb), mask2x(step_frame_like(mb))]
    cells.append(breath_like(cells[0]))
    return cells


# step_frame/breath_frame liczą zakres wierszy z niepustej sylwetki - dla maski trzeba użyć zakresu bohatera
_REF = None


def step_frame_like(m):
    top, bottom = _REF["span16"]
    feet = max(top, bottom - 3)
    out = [row[:] for row in m]
    for y in range(max(top - 1, 0), feet):
        out[y] = m[y + 1][:] if y + 1 < feet else m[feet - 1][:] if feet - 1 >= 0 else out[y]
    if top > 0:
        out[top - 1] = m[top][:]
    return out


def breath_like(m2):
    top, bottom = _REF["span32"]
    mid = top + int((bottom - top) * 0.55)
    out = [row[:] for row in m2]
    for y in range(mid, top, -1):
        out[y] = m2[y - 1][:]
    out[top] = [0] * len(m2[0])
    return out


def span(frame):
    rows = [y for y, r in enumerate(frame) if any(r)]
    return rows[0], rows[-1]


def overlay_cells(slot, rarity, ref_class=0):
    """Nakładki Godota 32x32 (5 klatek) wyliczone na zawodzie ref_class: piksele maski nakładki + piksele, które
    Scale2x złożonej klatki zmienia względem samego bohatera (zaokrąglenia na granicy)."""
    ha, hb = hero16(ref_class, 0), hero16(ref_class, 1)
    oa, ob = overlay16(slot, rarity, 0), overlay16(slot, rarity, 1)
    hero_c = godot_cells((ha, hb))
    comp_c = godot_cells((compose(ha, oa), compose(hb, ob)))
    masks = mask_cells(mask_of(oa), mask_of(ob))
    out = []
    for hc, cc, mc in zip(hero_c, comp_c, masks):
        out.append([[cc[y][x] if (mc[y][x] or cc[y][x] != hc[y][x]) else 0 for x in range(32)] for y in range(32)])
    return out


def put(base, over):
    return [[o if o else b for b, o in zip(rb, ro)] for rb, ro in zip(base, over)]


def verify():
    """Nakładka z zawodu 0 na każdym zawodzie = Scale2x złożonej klatki tego zawodu? -> liczba różnych pikseli."""
    report = {}
    for c in range(len(pa.WORKERS)):
        bad = 0
        for s in SLOTS:
            for r in range(3):
                cells = overlay_cells(s, r)
                ha, hb = hero16(c, 0), hero16(c, 1)
                hero_c = godot_cells((ha, hb))
                comp_c = godot_cells((compose(ha, overlay16(s, r, 0)), compose(hb, overlay16(s, r, 1))))
                for hc, cc, oc in zip(hero_c, comp_c, cells):
                    got = put(hc, oc)
                    bad += sum(1 for y in range(32) for x in range(32) if got[y][x] != cc[y][x])
                    # GBA 16x16: nakładka bez Scale2x - zawsze dokładna
        report[pa.WORKERS[c][2]] = bad
    return report


def tool_on_hand():
    """Piksele dłoni (rękawic), które w actors.png zasłania narzędzie zawodu (TOOLS w pixel_art.py)."""
    out = {}
    for c, (_h, _v, tool) in enumerate(pa.WORKERS):
        hit = [[x, y] for x, y, _ch in pa.TOOLS[tool] if (x, y) in ((12, 10), (12, 11), (3, 10), (3, 11))]
        out[c] = hit
    return out


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


def mirror(g):
    return [r[::-1] for r in g]


def gba_tiles(g):
    """Niepuste kafle 8x8 w klatce 16x16."""
    return [(tx, ty) for ty in range(2) for tx in range(2)
            if any(g[y][x] for y in range(ty * 8, ty * 8 + 8) for x in range(tx * 8, tx * 8 + 8))]


def main():
    global _REF
    os.makedirs(OUT, exist_ok=True)
    data = json.load(open(GAME_JSON, encoding="utf-8"))
    slots = data["equipment"]["slots"]
    assert [s["name"] for s in slots] == ["Kask", "Rękawice", "Kamizelka", "Buty", "Pas"], [s["name"] for s in slots]
    assert data["equipment"]["rarities"] == RARITIES
    classes = data["classes"]
    assert len(classes) == len(pa.WORKERS) == 12

    # wszystkie zawody: ta sama sylwetka poza narzędziem / przyłbicą (porównanie klatek bez narzędzia)
    ha0, hb0 = hero16(0, 0), hero16(0, 1)
    _REF = {"span16": span(ha0), "span32": span(scale2x(ha0))}
    for c in range(12):
        assert span(hero16(c, 0)) == _REF["span16"], c
        assert span(scale2x(hero16(c, 0))) == _REF["span32"], c

    hero_pal = [tuple(Image.open(os.path.join(REPO, "GBA", "graphics", "actors.bmp")).getpalette()[i:i + 3])
                for i in range(0, 48, 3)]
    full_pal = hero_pal + GEAR_PAL   # dla klatek złożonych (indeksy 16+ = nakładka)

    nrows = len(SLOTS) * 3
    gba = Image.new("RGBA", (16 * 4 * 2, 16 * nrows), (0, 0, 0, 0))
    god = Image.new("RGBA", (32 * 4 * 5, 32 * nrows), (0, 0, 0, 0))
    gba_idx = Image.new("P", (16 * 4 * 2, 16 * nrows), 0)
    gba_idx.putpalette([c for rgb in GEAR_PAL for c in rgb])
    rows = []
    tiles = {}
    for si, s in enumerate(SLOTS):
        for r in range(3):
            row = si * 3 + r
            f16 = [overlay16(s, r, 0), overlay16(s, r, 1)]
            cells = overlay_cells(s, r)
            for di, d in enumerate(DIRS):
                for k in range(2):
                    g = mirror(f16[k]) if d == "left" else f16[k]
                    gba.paste(to_image(g, GEAR_PAL), ((di * 2 + k) * 16, row * 16))
                    for y in range(16):
                        for x in range(16):
                            gba_idx.putpixel(((di * 2 + k) * 16 + x, row * 16 + y), g[y][x])
                for k, cell in enumerate(cells):
                    g = mirror(cell) if d == "left" else cell
                    god.paste(to_image(g, full_pal), ((di * 5 + k) * 32, row * 32))
            tiles[s] = sorted(set(t for k in range(2) for t in gba_tiles(f16[k])))
            item = slots[si]["items"][r][0]
            rows.append({"row": row, "slot": s, "slotName": slots[si]["name"], "rarity": RAR_KEYS[r],
                         "rarityName": RARITIES[r], "item": item, "gbaTiles8x8": len(gba_tiles(f16[0]))})
    gba.save(os.path.join(OUT, "gear_overlay.png"))
    gba_idx.save(os.path.join(OUT, "gear_overlay_gba.bmp"))
    god.save(os.path.join(OUT, "gear_overlay_2x.png"))

    check = verify()
    union = sorted(set(t for v in tiles.values() for t in v))
    manifest = {
        "version": 1,
        "frame": {"gba": 16, "godot": 32},
        "rows": "slot * 3 + rarity",
        "slots": SLOTS, "rarities": RAR_KEYS, "rarityNames": RARITIES,
        "columns": {
            "directions": DIRS,
            "gba": {"frames": ["A", "B"], "column": "dir * 2 + frame",
                    "note": "klatka A/B jak actors.bmp (anim_b w main.cpp)"},
            "godot": {"frames": GODOT_FRAMES, "column": "dir * 5 + frame",
                      "note": "jak actors_anim.png: AnimWalk0..3 (A, A w kroku, B, B w kroku), AnimBreath; "
                              "klatka statyczna A = walkA, B = walkB"},
            "directionNote": "bohater nie ma klatek kierunkowych: down = up = right = klatka bohatera, left = odbicie "
                             "lustrzane. Gdy rysuje się nakładkę tą samą transformacją co bohatera (ActorSprite.Flip), "
                             "wystarczy kolumna down."},
        "drawOrder": DRAW_ORDER,
        "drawOrderNote": "bohater -> buty -> kamizelka -> pas -> rękawice -> kask -> broń w ręce. Przy kierunku up broń "
                         "rysuje się pod bohaterem i całym sprzętem (jak w art/weapons/README.md); sprzęt zawsze nad "
                         "bohaterem (nie ma widoku z tyłu).",
        "offsets": {c["id"]: [0, 0] for c in classes},
        "offsetsNote": "wszystkie 12 zawodów mają tę samą sylwetkę (pixel_art.WORKER_TOP + nogi A/B), przesunięcia 0",
        "verifyGodotMismatchPixels": {classes[c]["id"]: check[pa.WORKERS[c][2]] for c in range(12)},
        "toolCoversHand": {classes[c]["id"]: v for c, v in tool_on_hand().items() if v},
        "toolCoversHandNote": "narzędzie zawodu jest wrysowane w klatkę actors.png na prawej dłoni - tam rękawica "
                              "zasłoni piksel narzędzia (bez znaczenia, gdy w ręce rysowana jest broń)",
        "palette": ["#%02x%02x%02x" % c for c in GEAR_PAL],
        "paletteNames": PAL_NAMES,
        "items": rows,
        "gba": {"tilesPerSlot8x8": {s: len(v) for s, v in tiles.items()},
                "tilesAllSlots8x8": len(union),
                "file": "gear_overlay_gba.bmp (indeksowana, paleta = palette)"},
    }
    with open(os.path.join(OUT, "gear.json"), "w", encoding="utf-8") as fh:
        json.dump(manifest, fh, ensure_ascii=False, indent=1)
        fh.write("\n")
    make_preview(classes, hero_pal, full_pal)
    print("nakładki:", nrows, "| kafle GBA na slot:", manifest["gba"]["tilesPerSlot8x8"], "| razem:", len(union),
          "| niezgodne piksele Godot:", sum(check.values()))


# ------------------------------------------------------------------ podgląd
def font(size):
    for p in ("/System/Library/Fonts/Supplemental/Arial.ttf", "/Library/Fonts/Arial.ttf",
              "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf"):
        if os.path.exists(p):
            return ImageFont.truetype(p, size)
    return ImageFont.load_default()


def full_set(c, rarity, frame):
    """Klatka 16x16 zawodu c w pełnym komplecie jednej rzadkości (kolejność rysowania DRAW_ORDER)."""
    g = hero16(c, frame)
    for s in DRAW_ORDER:
        g = compose(g, overlay16(s, rarity, frame))
    return g


def full_set_cells(c, rarity):
    """5 klatek Godota: bohater (Scale2x) + nakładki z gear_overlay_2x (tak jak w grze)."""
    hero_c = godot_cells((hero16(c, 0), hero16(c, 1)))
    out = []
    for k, hc in enumerate(hero_c):
        g = hc
        for s in DRAW_ORDER:
            g = put(g, overlay_cells(s, rarity)[k])
        out.append(g)
    return out


def make_preview(classes, hero_pal, full_pal):
    from weapon_sprites import deutan
    f_name, f_small, f_head = font(13), font(11), font(14)
    BG, PANEL = (46, 44, 58), (60, 58, 74)
    Z = 3                       # klatka Godota 32x32 powiększona x3
    CW = 32 * Z + 8
    left = 150
    W_ = left + 4 * CW + 20
    H_ = 40 + 12 * (CW + 4) + 60 + 3 * (CW + 4) + 60 + 3 * (CW + 4)
    sheet = Image.new("RGBA", (W_, H_), BG + (255,))
    d = ImageDraw.Draw(sheet)
    for j, t in enumerate(["bez sprzętu"] + RARITIES):
        d.text((left + j * CW + 10, 12), t, font=f_head, fill=(220, 220, 230))
    for c, cl in enumerate(classes):
        y = 36 + c * (CW + 4)
        d.rectangle((4, y, W_ - 4, y + CW - 2), fill=PANEL)
        d.text((10, y + 40), cl["name"], font=f_name, fill=(240, 240, 245))
        cells = [godot_cells((hero16(c, 0), hero16(c, 1)))[0]] + [full_set_cells(c, r)[0] for r in range(3)]
        for j, g in enumerate(cells):
            sheet.alpha_composite(to_image(g, full_pal).resize((32 * Z, 32 * Z), Image.NEAREST),
                                  (left + j * CW, y + 2))
    # jeden zawód w 4 kierunkach (Markowy) + klatki chodu i oddechu (każda rzadkość)
    y0 = 36 + 12 * (CW + 4) + 20
    d.text((10, y0), "Murarz, pełny komplet - kierunki (left = odbicie, jak ActorSprite.Flip)", font=f_head,
           fill=(220, 220, 230))
    y0 += 30
    for r in range(3):
        cells = full_set_cells(1, r)
        y = y0 + r * (CW + 4)
        d.rectangle((4, y, W_ - 4, y + CW - 2), fill=PANEL)
        d.text((10, y + 40), RARITIES[r], font=f_name, fill=(240, 240, 245))
        for di, dname in enumerate(DIRS):
            g = mirror(cells[0]) if dname == "left" else cells[0]
            sheet.alpha_composite(to_image(g, full_pal).resize((32 * Z, 32 * Z), Image.NEAREST),
                                  (left + di * CW, y + 2))
            if r == 0:
                d.text((left + di * CW + 30, y0 - 14), dname, font=f_small, fill=(170, 170, 185))
    y0 += 3 * (CW + 4) + 20
    d.text((10, y0), "Chód i oddech (klatki actors_anim.png): walkA, walkAstep, walkB, walkBstep, breath",
           font=f_head, fill=(220, 220, 230))
    y0 += 30
    Zs = 2
    for r in range(3):
        cells = full_set_cells(4, r)
        y = y0 + r * (CW + 4)
        d.rectangle((4, y, W_ - 4, y + CW - 2), fill=PANEL)
        d.text((10, y + 40), "Hydraulik · " + RARITIES[r], font=f_name, fill=(240, 240, 245))
        for k, g in enumerate(cells):
            sheet.alpha_composite(to_image(g, full_pal).resize((32 * Zs, 32 * Zs), Image.NEAREST),
                                  (left + k * (32 * Zs + 6), y + 16))
    sheet.save(os.path.join(OUT, "preview.png"))
    deutan(sheet).save(os.path.join(OUT, "preview_deutan.png"))


if __name__ == "__main__":
    main()
