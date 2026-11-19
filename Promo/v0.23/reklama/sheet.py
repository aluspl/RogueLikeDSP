#!/usr/bin/env python3
"""Arkusz kontrolny: sheet.py KLIP OUT.png [N=6] [SZER=360] – N klatek równo z całego klipu obok siebie, z czasem."""
import io, json, subprocess, sys
from PIL import Image, ImageDraw
clip, out = sys.argv[1], sys.argv[2]
n = int(sys.argv[3]) if len(sys.argv) > 3 else 6
w = int(sys.argv[4]) if len(sys.argv) > 4 else 360
dur = float(json.loads(subprocess.run(['ffprobe', '-v', 'error', '-show_entries', 'format=duration', '-of', 'json', clip],
                                      capture_output=True, text=True).stdout)['format']['duration'])
ims = []
for k in range(n):
    t = dur * (k + 0.5) / n
    png = subprocess.run(['ffmpeg', '-v', 'error', '-ss', f'{t:.3f}', '-i', clip, '-frames:v', '1', '-vf', f'scale={w}:-2',
                          '-f', 'image2pipe', '-vcodec', 'png', '-'], capture_output=True, check=True).stdout
    im = Image.open(io.BytesIO(png)).convert('RGB')
    d = ImageDraw.Draw(im)
    d.rectangle([0, 0, 70, 22], fill=(0, 0, 0))
    d.text((4, 4), f'{t:.1f}s', fill=(255, 255, 0))
    ims.append(im)
sheet = Image.new('RGB', (sum(i.width for i in ims) + 4 * (n - 1), max(i.height for i in ims)), (255, 0, 255))
x = 0
for i in ims:
    sheet.paste(i, (x, 0))
    x += i.width + 4
sheet.save(out)
print(out, f'{dur:.2f} s')
