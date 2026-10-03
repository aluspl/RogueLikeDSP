#!/usr/bin/env python3
"""Karta gry w Google Play: tytuł i opisy (pl-PL, en-US) z Promo/store/listing.json oraz grafiki
(ikona 512, grafika promocyjna 1024x500, zrzuty telefonu) z Promo/store/. Konto serwisowe jak w play_upload.py.

    python3 GODOT/tools/play_listing.py [--no-images]
"""
import argparse, json, pathlib, sys
sys.path.insert(0, str(pathlib.Path(__file__).resolve().parent))
import play_upload as pu

ROOT = pathlib.Path(__file__).resolve().parents[2]
STORE = ROOT / "Promo" / "store"


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--no-images", action="store_true")
    a = ap.parse_args()
    tok = pu.access_token(pu.env("PLAY_SERVICE_ACCOUNT_JSON"))
    base = f"{pu.API}/{pu.PACKAGE}"
    up = f"{pu.UPLOAD}/{pu.PACKAGE}"
    edit = pu.call(tok, "POST", f"{base}/edits", {})["id"]
    texts = json.loads((STORE / "listing.json").read_text())
    for lang, t in texts.items():
        pu.call(tok, "PUT", f"{base}/edits/{edit}/listings/{lang}",
                {"language": lang, "title": t["title"], "shortDescription": t["short"], "fullDescription": t["full"]})
        print(f"opis {lang}")
        if a.no_images:
            continue
        for kind, files in (("icon", [STORE / "icon_512.png"]),
                            ("featureGraphic", [STORE / "feature_graphic_1024x500.png"]),
                            ("phoneScreenshots", sorted((STORE / "play_phone").glob("*.png")))):
            pu.call(tok, "DELETE", f"{base}/edits/{edit}/listings/{lang}/{kind}")
            for f in files:
                pu.call(tok, "POST", f"{up}/edits/{edit}/listings/{lang}/{kind}?uploadType=media",
                        data=f.read_bytes(), ctype="image/png")
            print(f"  {kind}: {len(files)}")
    pu.call(tok, "POST", f"{base}/edits/{edit}:commit")
    print("Zapisano kartę w Google Play")


if __name__ == "__main__":
    main()
