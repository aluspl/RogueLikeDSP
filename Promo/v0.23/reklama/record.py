#!/usr/bin/env python3
"""Nagranie ujęcia Godot Movie Maker w pionie: record.py OUT.avi W H [argumenty gry po --].
Na czas nagrania tymczasowy GODOT/godot/override.cfg z rozmiarem okna (project.godot ma 1280x720), potem usuwany."""
import pathlib, subprocess, sys

REPO = pathlib.Path(__file__).resolve().parents[3]  # Promo/v0.23/reklama -> korzeń repo
OVR = REPO / 'GODOT/godot/override.cfg'
out, w, h, game = str(pathlib.Path(sys.argv[1]).resolve()), sys.argv[2], sys.argv[3], sys.argv[4:]
OVR.write_text(f'[display]\nwindow/size/window_width_override={w}\nwindow/size/window_height_override={h}\n')
try:
    r = subprocess.run(['godot-mono', '--path', str(REPO / 'GODOT/godot'), '--write-movie', out, '--fixed-fps', '30', '--', *game],
                       capture_output=True, text=True)
    errs = [l for l in r.stdout.splitlines() + r.stderr.splitlines() if 'Exception' in l or 'SCRIPT ERROR' in l or 'ERROR' in l or 'movie length' in l]
    print('exit', r.returncode, *errs[:3], sep='\n')
finally:
    OVR.unlink(missing_ok=True)
