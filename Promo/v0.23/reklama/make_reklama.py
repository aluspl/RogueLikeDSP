#!/usr/bin/env python3
"""Reklamówka 9:16 (1080x1920) z ujęć gry: podpisy (PIL), lekki najazd kamery, przejścia (xfade), plansza końcowa,
muzyka z gry. Użycie: make_reklama.py SHOTS.json OUT.mp4  (SHOTS: lista {"clip": plik, "caption": tekst, "sub": tekst,
"len": sekundy, "ss": początek klipu, "zoom0"/"zoom": najazd od/do, "fx"/"fy": punkt najazdu 0..1,
"cap_y": wysokość podpisu, "caption" z "\\n" = dwie linie, "shake": bool, "trans": "fade|slideleft|fadewhite|circleopen"})."""
import json, subprocess, sys, pathlib
from PIL import Image, ImageDraw, ImageFont

W, H, FPS = 1080, 1920, 30
V, O, INK, WH = (107, 78, 255), (255, 122, 61), (20, 18, 40), (255, 255, 255)
AV = '/System/Library/Fonts/Avenir Next.ttc'
F = lambda s, i: ImageFont.truetype(AV, s, index=i)
REPO = pathlib.Path(__file__).resolve().parents[3]  # Promo/v0.23/reklama -> korzeń repo


def caption_png(text, sub, path, y0=150):
    """Pasek podpisu: fioletowa pigułka, biały tekst, pomarańczowa linia (jak infografiki).
    v0.23: tekst może mieć kilka linii ("\\n"), czcionka zmniejsza się, aż wszystko zmieści się w szerokości ekranu."""
    c = Image.new('RGBA', (W, H), (0, 0, 0, 0))
    d = ImageDraw.Draw(c)
    if not text.strip():  # plansza końcowa: bez podpisu
        c.save(path)
        return
    lines = text.split('\n')
    st, ss = 78, 42
    while True:
        ft, fs = F(st, 8), F(ss, 5)
        tws = [d.textlength(l, font=ft) for l in lines]
        sw = d.textlength(sub, font=fs) if sub else 0
        if max(max(tws), sw) + 120 <= W - 40 or st <= 50:
            break
        st, ss = st - 2, max(32, ss - 1)
    lh = int(st * 1.12)
    bw = max(max(tws), sw) + 120
    x0 = (W - bw) / 2
    bh = 52 + lh * len(lines) + (int(ss * 1.6) if sub else 0)
    d.rounded_rectangle([x0, y0, x0 + bw, y0 + bh], radius=40, fill=V + (235,))
    d.rectangle([x0 + 40, y0 + bh - 14, x0 + bw - 40, y0 + bh - 8], fill=O)
    for k, (l, tw) in enumerate(zip(lines, tws)):
        d.text(((W - tw) / 2, y0 + 22 + k * lh), l, font=ft, fill=WH)
    if sub:
        d.text(((W - sw) / 2, y0 + 22 + lh * len(lines) + 4), sub, font=fs, fill=(230, 225, 255))
    c.save(path)


def end_card(path):
    c = Image.new('RGB', (W, H), V)
    d = ImageDraw.Draw(c)
    logo = Image.open(REPO / 'GODOT/godot/assets/ui/title.png').convert('RGBA')
    logo = logo.resize((logo.width * 4, logo.height * 4), Image.NEAREST)
    c.paste(logo, ((W - logo.width) // 2, 380), logo)
    def center(t, f, y, fill):
        tw = d.textlength(t, font=f)
        d.text(((W - tw) / 2, y), t, font=f, fill=fill)
    center('Gra od twórcy PlanBudowlany', F(56, 8), 1010, WH)
    d.rounded_rectangle([120, 1200, W - 120, 1340], radius=40, fill=WH)
    center('planbudowlany.online', F(58, 8), 1232, V)
    center('iPhone · Android · wersja retro', F(42, 5), 1410, WH)
    for x in range(-60, W + 60, 54):
        d.polygon([(x, H), (x + 27, H), (x + 27 + 40, H - 40), (x + 40, H - 40)], fill=O)
    c.save(path)


def segment(shot, i, tmp):
    """Ujęcie → 1080x1920: początek klipu (ss), najazd od zoom0 do zoom w punkt (fx, fy), opcjonalne potrząśnięcie, podpis."""
    ln, z, z0 = shot.get('len', 3.0), shot.get('zoom', 1.06), shot.get('zoom0', 1.0)
    fx, fy = shot.get('fx', 0.5), shot.get('fy', 0.5)
    cap = tmp / f'cap{i}.png'
    caption_png(shot['caption'], shot.get('sub', ''), cap, shot.get('cap_y', 150))
    src = shot['clip']
    inp = (['-loop', '1', '-t', str(ln), '-i', src] if src.endswith('.png')
           else ['-ss', str(shot.get('ss', 0)), '-t', str(ln), '-i', src])
    shake = "+if(lt(mod(n\\,6)\\,3)\\,8\\,-8)*between(t\\,0.3\\,0.9)" if shot.get('shake') else ""
    vf = (f"[0:v]scale={W}:{H}:force_original_aspect_ratio=increase:flags=neighbor,crop={W}:{H},"
          f"scale=w='{W}*({z0}+({z}-{z0})*t/{ln})':h=-2:eval=frame:flags=bicubic,"
          f"crop={W}:{H}:x='(iw-{W})*{fx}{shake}':y='(ih-{H})*{fy}',fps={FPS},setsar=1[v];"
          f"[v][1:v]overlay=0:0:format=auto,format=yuv420p[o]")
    out = tmp / f'seg{i}.mp4'
    subprocess.run(['ffmpeg', '-y', '-loglevel', 'error', *inp, '-i', str(cap), '-filter_complex', vf, '-map', '[o]',
                    '-r', str(FPS), '-c:v', 'libx264', '-crf', '18', '-pix_fmt', 'yuv420p', str(out)], check=True)
    return out, ln


def main():
    shots = json.load(open(sys.argv[1]))
    out = pathlib.Path(sys.argv[2])
    tmp = out.parent / 'tmp'
    tmp.mkdir(parents=True, exist_ok=True)
    end_card(tmp / 'end.png')
    shots = shots + [{'clip': str(tmp / 'end.png'), 'caption': '', 'len': 3.5, 'zoom': 1.03, 'trans': 'fade'}]
    segs = []
    for i, s in enumerate(shots):
        segs.append((*segment(s, i, tmp), s.get('trans', 'fade')))
    # przejścia: xfade 0.35 s między kolejnymi ujęciami
    xd = 0.35
    inputs, chain, t = [], '', 0.0
    for p, _, _ in segs:
        inputs += ['-i', str(p)]
    prev = '[0:v]'
    for k in range(1, len(segs)):
        t += segs[k - 1][1] - xd
        lab = f'[x{k}]'
        chain += f"{prev}[{k}:v]xfade=transition={segs[k][2]}:duration={xd}:offset={t:.2f}{lab};"
        prev = lab
    total = sum(s[1] for s in segs) - xd * (len(segs) - 1)
    # muzyka: utwór z gry, pod planszę końcową przenikanie w melodię tytułową
    game_m, title_m = REPO / 'GODOT/godot/assets/audio/music_game.mp3', REPO / 'GODOT/godot/assets/audio/music_title.mp3'
    gl = min(34.0, total - 3.0)
    n = len(segs)
    chain += (f"[{n}:a]atrim=0:{gl:.2f}[ga];[{n + 1}:a]atrim=0:{total - gl + 2:.2f}[ta];[ga][ta]acrossfade=d=2[m];"
              f"[m]atrim=0:{total:.2f},afade=t=in:d=0.3,afade=t=out:st={total - 1.2:.2f}:d=1.2,volume=0.8[a]")
    subprocess.run(['ffmpeg', '-y', '-loglevel', 'error', *inputs, '-i', str(game_m), '-i', str(title_m), '-filter_complex', chain,
                    '-map', prev, '-map', '[a]', '-c:v', 'libx264', '-crf', '19', '-pix_fmt', 'yuv420p', '-c:a', 'aac',
                    '-b:a', '160k', '-movflags', '+faststart', str(out)], check=True)
    print('ok', out, f'{total:.1f} s')


if __name__ == '__main__':
    main()
