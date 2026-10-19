#!/usr/bin/env python3
"""Pikselowe placeholdery do konceptów UX (bez PIL, czysty Python + zlib).

Generuje w ../assets/generated/:
  hammer.png        – młot wyburzeniowy (jednorazowy), 32x32 -> skala 4 = 128x128
  wall_sheet.png    – 4 klatki pękania ściany, 48x48 każda, w jednym rzędzie
  phone_icons6.png  – 6 ikon aplikacji telefonu 32x32 w jednym rzędzie
Skala całości: nearest-neighbour (ostre piksele).
"""
import os
import struct
import zlib

OUT = os.path.join(os.path.dirname(__file__), "..", "assets", "generated")

# ---------- minimalny zapis PNG (RGBA) ----------

def write_png(path, w, h, px):
    """px: lista wierszy, każdy wiersz lista krotek (r,g,b,a)."""
    raw = bytearray()
    for row in px:
        raw.append(0)  # filtr: none
        for r, g, b, a in row:
            raw += bytes((r, g, b, a))
    def chunk(tag, data):
        c = struct.pack(">I", len(data)) + tag + data
        return c + struct.pack(">I", zlib.crc32(tag + data) & 0xFFFFFFFF)
    png = b"\x89PNG\r\n\x1a\n"
    png += chunk(b"IHDR", struct.pack(">IIBBBBB", w, h, 8, 6, 0, 0, 0))
    png += chunk(b"IDAT", zlib.compress(bytes(raw), 9))
    png += chunk(b"IEND", b"")
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "wb") as f:
        f.write(png)


class Canvas:
    def __init__(self, w, h, bg=(0, 0, 0, 0)):
        self.w, self.h = w, h
        self.p = [[bg] * w for _ in range(h)]

    def put(self, x, y, c):
        if 0 <= x < self.w and 0 <= y < self.h:
            self.p[y][x] = c

    def rect(self, x, y, w, h, c):
        for yy in range(y, y + h):
            for xx in range(x, x + w):
                self.put(xx, yy, c)

    def line(self, x0, y0, x1, y1, c):
        dx, dy = abs(x1 - x0), -abs(y1 - y0)
        sx, sy = (1 if x0 < x1 else -1), (1 if y0 < y1 else -1)
        err = dx + dy
        while True:
            self.put(x0, y0, c)
            if x0 == x1 and y0 == y1:
                break
            e2 = 2 * err
            if e2 >= dy:
                err += dy; x0 += sx
            if e2 <= dx:
                err += dx; y0 += sy

    def tri(self, pts, c):
        xs = [p[0] for p in pts]; ys = [p[1] for p in pts]
        for y in range(min(ys), max(ys) + 1):
            for x in range(min(xs), max(xs) + 1):
                if _in_tri(x, y, pts):
                    self.put(x, y, c)

    def circle(self, cx, cy, r, c):
        for y in range(cy - r, cy + r + 1):
            for x in range(cx - r, cx + r + 1):
                if (x - cx) ** 2 + (y - cy) ** 2 <= r * r:
                    self.put(x, y, c)

    def blit(self, other, ox, oy):
        for y in range(other.h):
            for x in range(other.w):
                c = other.p[y][x]
                if c[3]:
                    self.put(ox + x, oy + y, c)

    def outline(self, c):
        """Czarny kontur wokół nieprzezroczystych pikseli (1 px, na zewnątrz)."""
        src = [row[:] for row in self.p]
        for y in range(self.h):
            for x in range(self.w):
                if src[y][x][3]:
                    continue
                near = any(
                    0 <= x + dx < self.w and 0 <= y + dy < self.h and src[y + dy][x + dx][3]
                    for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1))
                )
                if near:
                    self.p[y][x] = c

    def scaled(self, k):
        out = []
        for row in self.p:
            big = []
            for c in row:
                big.extend([c] * k)
            for _ in range(k):
                out.append(big)
        return out


def _in_tri(x, y, pts):
    (x1, y1), (x2, y2), (x3, y3) = pts
    d1 = (x - x2) * (y1 - y2) - (x1 - x2) * (y - y2)
    d2 = (x - x3) * (y2 - y3) - (x2 - x3) * (y - y3)
    d3 = (x - x1) * (y3 - y1) - (x3 - x1) * (y - y1)
    neg = d1 < 0 or d2 < 0 or d3 < 0
    pos = d1 > 0 or d2 > 0 or d3 > 0
    return not (neg and pos)


# ---------- paleta (zgodna z tonacją gry: złoto, brąz, stal, granat) ----------
BLACK = (14, 14, 22, 255)
STEEL = (154, 160, 166, 255)
STEEL_HI = (214, 218, 222, 255)
STEEL_DK = (92, 98, 106, 255)
WOOD = (122, 85, 48, 255)
WOOD_DK = (74, 50, 28, 255)
AMBER = (224, 165, 38, 255)
AMBER_DK = (160, 110, 20, 255)
BRICK = (150, 90, 58, 255)
BRICK_DK = (104, 60, 38, 255)
MORTAR = (196, 186, 166, 255)
CRACK = (30, 18, 10, 255)
GREY = (120, 124, 130, 255)
GREEN = (60, 160, 110, 255)
RED = (192, 57, 43, 255)
BLUE = (46, 134, 193, 255)
CREAM = (243, 233, 198, 255)


# ---------- młot (32x32) ----------

def make_hammer():
    c = Canvas(32, 32)
    # trzonek: ukośny, od prawego dołu do środka
    c.line(22, 30, 14, 14, WOOD)
    c.line(23, 30, 15, 14, WOOD_DK)
    c.line(21, 30, 13, 14, WOOD)
    # głowica: prostokąt pod kątem (uproszczenie: poziomy blok z połyskiem)
    c.rect(6, 6, 18, 9, STEEL)
    c.rect(6, 6, 18, 2, STEEL_HI)
    c.rect(6, 13, 18, 2, STEEL_DK)
    c.rect(20, 6, 4, 9, STEEL_DK)
    # akcent: złote opaski (jednorazowość)
    c.rect(10, 6, 2, 9, AMBER)
    c.rect(19, 6, 2, 9, AMBER)
    c.outline(BLACK)
    return c


# ---------- ściana: 4 klatki pękania (48x48 każda) ----------

def _brick_wall(c, ox, oy):
    # 3 rzędy cegieł, 24x8 cegły
    for row in range(5):
        y = oy + 4 + row * 8
        off = 0 if row % 2 == 0 else 12
        for col in range(-1, 3):
            x = ox + 2 + col * 24 + off
            c.rect(x, y, 22, 6, BRICK)
            c.rect(x, y + 5, 22, 1, BRICK_DK)
            c.rect(x, y, 22, 1, MORTAR)
    # zaprawa: ramki co cegła widoczne jako jasne linie już w rect'ach


def make_wall_frame(stage):
    c = Canvas(48, 48, bg=(0, 0, 0, 0))
    _brick_wall(c, 0, 0)
    if stage >= 1:
        c.line(10, 10, 16, 22, CRACK); c.line(16, 22, 13, 34, CRACK)
        c.line(30, 6, 36, 18, CRACK)
    if stage >= 2:
        c.line(20, 16, 26, 26, CRACK); c.line(26, 26, 40, 30, CRACK)
        c.line(6, 30, 14, 40, CRACK); c.line(34, 14, 44, 22, CRACK)
        c.rect(18, 22, 4, 4, BRICK_DK); c.rect(33, 30, 3, 3, BRICK_DK)
    if stage >= 3:
        # ściana zniknęła: czysty gruz na dolnej krawędzi (bez resztek cegieł)
        c = Canvas(48, 48, bg=(0, 0, 0, 0))
        c.rect(2, 44, 44, 2, BRICK_DK)  # linia podłoża
        for x, y, w, h in ((4, 38, 8, 6), (13, 40, 6, 4), (20, 36, 9, 8), (30, 39, 7, 5), (38, 41, 6, 3), (16, 33, 5, 4), (27, 33, 6, 5)):
            c.rect(x, y, w, h, BRICK)
            c.rect(x, y + h - 1, w, 1, BRICK_DK)
            c.rect(x, y, w, 1, MORTAR)
    c.outline(BLACK)
    return c


def make_wall_sheet():
    sheet = Canvas(48 * 4 + 6, 48, bg=(40, 36, 30, 255))
    for i in range(4):
        f = make_wall_frame(i)
        sheet.blit(f, i * 50, 0)
    return sheet


# ---------- 6 ikon telefonu (32x32) ----------

def icon_tasks():
    c = Canvas(32, 32)
    c.rect(7, 6, 18, 22, CREAM); c.rect(7, 6, 18, 2, AMBER)
    c.rect(12, 4, 8, 4, GREY)
    for y in (13, 18, 23):
        c.rect(10, y, 12, 2, BRICK_DK)
    c.outline(BLACK); return c


def icon_issues():
    c = Canvas(32, 32)
    c.tri([(16, 4), (28, 26), (4, 26)], AMBER)
    c.rect(15, 11, 2, 9, BLACK); c.rect(15, 22, 2, 2, BLACK)
    c.outline(BLACK); return c


def icon_start():
    c = Canvas(32, 32)
    c.rect(6, 16, 20, 8, AMBER)
    c.rect(4, 22, 24, 4, AMBER_DK)
    c.rect(8, 12, 16, 4, AMBER)
    c.rect(10, 8, 12, 4, AMBER_DK)
    c.outline(BLACK); return c


def icon_gear():
    c = Canvas(32, 32)
    c.rect(4, 12, 24, 14, RED)
    c.rect(12, 6, 8, 6, RED)
    c.rect(4, 12, 24, 3, (150, 40, 32, 255))
    c.rect(14, 16, 4, 4, CREAM)
    c.outline(BLACK); return c


def icon_costs():
    c = Canvas(32, 32)
    for i, y in enumerate((22, 17, 12)):
        c.rect(7, y, 18, 5, AMBER)
        c.rect(7, y + 4, 18, 1, AMBER_DK)
    c.circle(16, 9, 2, AMBER_DK)
    c.outline(BLACK); return c


def icon_badges():
    c = Canvas(32, 32)
    c.rect(10, 5, 12, 12, AMBER)
    c.rect(12, 7, 8, 8, CREAM)
    c.rect(12, 17, 3, 9, BLUE); c.rect(17, 17, 3, 9, RED)
    c.outline(BLACK); return c


def make_phone_icons():
    icons = [icon_tasks(), icon_issues(), icon_start(), icon_gear(), icon_costs(), icon_badges()]
    sheet = Canvas(32 * 6 + 5 * 2, 32, bg=(0, 0, 0, 0))
    for i, ic in enumerate(icons):
        sheet.blit(ic, i * 34, 0)
    return sheet


def main():
    hammer = make_hammer()
    write_png(os.path.join(OUT, "hammer.png"), 32, 32, hammer.p)
    write_png(os.path.join(OUT, "hammer_x4.png"), 128, 128, hammer.scaled(4))
    ws = make_wall_sheet()
    write_png(os.path.join(OUT, "wall_sheet.png"), ws.w, ws.h, ws.p)
    write_png(os.path.join(OUT, "wall_sheet_x4.png"), ws.w * 4, ws.h * 4, ws.scaled(4))
    pi = make_phone_icons()
    write_png(os.path.join(OUT, "phone_icons6.png"), pi.w, pi.h, pi.p)
    write_png(os.path.join(OUT, "phone_icons6_x4.png"), pi.w * 4, pi.h * 4, pi.scaled(4))
    print("ok:", sorted(os.listdir(OUT)))


if __name__ == "__main__":
    main()
