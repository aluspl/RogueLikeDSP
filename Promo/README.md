# Promo

Materiały do social mediów i sklepów, po jednym folderze na wersję.

- `v0.21.51/` – 2 infografiki 1080×1350, tekst posta (`post.md`), zrzuty z Godota w pionie (`zrzuty/`)
  i skrypt, który składa grafiki (`make_infografiki.py`, Pillow + font Avenir Next z macOS).
- `v0.22/` – seria 0.22 (v0.22.0 + v0.22.1): 2 infografiki 1080×1350, tekst posta PL/EN, zrzuty w pionie, skrypt.
- `v0.23/` – bronie: 2 infografiki, post PL/EN, zrzuty; `reklama/` – reklamówka 9:16 (`nagraj.sh`: ujęcia Godot Movie Maker
  w oknie 540×960 + montaż `make_reklama.py`, lista ujęć `shots_v023.json`). MP4 (~48 MB) poza repo – kopia w Organizacji.

Zrzuty robi tryb zrzutów Godota, np.:

    godot-mono --path GODOT/godot -- --touch --portrait --size 860x1864 --screenshot Promo/v0.21.51/zrzuty/boss.png --scene boss

Zasady: bez znaków i nazw konsol w brandingu (starsza wersja = „wersja retro”), przeciwnicy to problemy budowy,
teksty do wklejenia bez ramek cytatu i z minimum emoji.
