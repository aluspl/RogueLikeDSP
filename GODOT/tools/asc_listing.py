#!/usr/bin/env python3
"""Karta gry w App Store Connect (aplikacja online.planbudowlany.rogue) z Promo/store/:
nazwa, podtytuł, opis, słowa kluczowe, tekst promocyjny (pl, en-US), kategoria Gry / RPG i zrzuty iPhone 6,9" i 6,5".
Klucz API jak w asc_profile.py (zmienne z GODOT/.env.local).

    python3 GODOT/tools/asc_listing.py [--version 0.21.53] [--no-screens] [--privacy-url URL] [--support-url URL]
"""
import argparse, hashlib, json, pathlib, sys, urllib.request, urllib.error
sys.path.insert(0, str(pathlib.Path(__file__).resolve().parent))
import asc_profile as asc

ROOT = pathlib.Path(__file__).resolve().parents[2]
STORE = ROOT / "Promo" / "store"
LOCALES = {"pl-PL": "pl", "en-US": "en-US"}       # listing.json -> App Store locale
SCREENS = {"APP_IPHONE_67": "ios_6.9", "APP_IPHONE_65": "ios_6.5"}


class Api:
    def __init__(self):
        self.tok = asc.token(asc.env("ASC_KEY"), asc.env("ASC_KEY_ID"), asc.env("ASC_ISSUER_ID"))

    def __call__(self, method, path, body=None):
        url = path if path.startswith("http") else asc.API + path
        req = urllib.request.Request(url, method=method, data=json.dumps(body).encode() if body else None,
                                     headers={"Authorization": f"Bearer {self.tok}", "Content-Type": "application/json"})
        try:
            with urllib.request.urlopen(req) as r:
                raw = r.read()
                return json.loads(raw) if raw else {}
        except urllib.error.HTTPError as e:
            sys.exit(f"{method} {path}: HTTP {e.code} {e.read().decode()[:500]}")


def upsert_loc(api, kind, parent_rel, parent_id, existing, locale, attrs):
    cur = next((x for x in existing if x["attributes"]["locale"] == locale), None)
    if cur:
        api("PATCH", f"/{kind}/{cur['id']}", {"data": {"type": kind, "id": cur["id"], "attributes": attrs}})
        return cur["id"]
    attrs = dict(attrs, locale=locale)
    rel_type = {"appInfoLocalizations": "appInfos", "appStoreVersionLocalizations": "appStoreVersions"}[kind]
    return api("POST", f"/{kind}", {"data": {"type": kind, "attributes": attrs,
               "relationships": {parent_rel: {"data": {"type": rel_type, "id": parent_id}}}}})["data"]["id"]


def upload_screens(api, loc_id, display, files):
    sets = api("GET", f"/appStoreVersionLocalizations/{loc_id}/appScreenshotSets")["data"]
    s = next((x for x in sets if x["attributes"]["screenshotDisplayType"] == display), None)
    if s:
        for shot in api("GET", f"/appScreenshotSets/{s['id']}/appScreenshots")["data"]:
            api("DELETE", f"/appScreenshots/{shot['id']}")
    else:
        s = api("POST", "/appScreenshotSets", {"data": {"type": "appScreenshotSets",
                "attributes": {"screenshotDisplayType": display},
                "relationships": {"appStoreVersionLocalization": {"data": {"type": "appStoreVersionLocalizations", "id": loc_id}}}}})["data"]
    for f in files:
        data = f.read_bytes()
        shot = api("POST", "/appScreenshots", {"data": {"type": "appScreenshots",
                   "attributes": {"fileName": f.name, "fileSize": len(data)},
                   "relationships": {"appScreenshotSet": {"data": {"type": "appScreenshotSets", "id": s["id"]}}}}})["data"]
        for op in shot["attributes"]["uploadOperations"]:
            part = data[op["offset"]:op["offset"] + op["length"]]
            req = urllib.request.Request(op["url"], method=op["method"], data=part,
                                         headers={h["name"]: h["value"] for h in op["requestHeaders"]})
            urllib.request.urlopen(req).read()
        api("PATCH", f"/appScreenshots/{shot['id']}", {"data": {"type": "appScreenshots", "id": shot["id"],
            "attributes": {"uploaded": True, "sourceFileChecksum": hashlib.md5(data).hexdigest()}}})
    return len(files)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--version")
    ap.add_argument("--no-screens", action="store_true")
    ap.add_argument("--privacy-url")
    ap.add_argument("--support-url")
    ap.add_argument("--bundle", default="online.planbudowlany.rogue")
    a = ap.parse_args()
    version = a.version or json.loads((ROOT / "GBA/data/game.json").read_text())["version"].lstrip("v")
    texts = json.loads((STORE / "listing.json").read_text())
    api = Api()
    app = api("GET", f"/apps?filter[bundleId]={a.bundle}")["data"][0]
    print(f"Aplikacja: {app['attributes']['name']} (główny język {app['attributes']['primaryLocale']})")

    # Informacje o aplikacji (nazwa, podtytuł, prywatność) + kategoria
    info = api("GET", f"/apps/{app['id']}/appInfos")["data"][0]
    api("PATCH", f"/appInfos/{info['id']}", {"data": {"type": "appInfos", "id": info["id"], "relationships": {
        "primaryCategory": {"data": {"type": "appCategories", "id": "GAMES"}},
        "primarySubcategoryOne": {"data": {"type": "appCategories", "id": "GAMES_ROLE_PLAYING"}},
        "primarySubcategoryTwo": {"data": {"type": "appCategories", "id": "GAMES_STRATEGY"}}}}})
    info_locs = api("GET", f"/appInfos/{info['id']}/appInfoLocalizations")["data"]
    for src, loc in LOCALES.items():
        t = texts[src]
        attrs = {"name": t["title"], "subtitle": t["subtitle"]}
        if a.privacy_url:
            attrs["privacyPolicyUrl"] = a.privacy_url
        upsert_loc(api, "appInfoLocalizations", "appInfo", info["id"], info_locs, loc, attrs)
        print(f"informacje {loc}")

    # Wersja w App Store (opis, słowa kluczowe, zrzuty)
    vers = api("GET", f"/apps/{app['id']}/appStoreVersions?filter[platform]=IOS")["data"]
    ver = next((v for v in vers if v["attributes"]["appStoreState"] in
                ("PREPARE_FOR_SUBMISSION", "DEVELOPER_REJECTED", "REJECTED", "METADATA_REJECTED")), None)
    if ver is None:
        ver = api("POST", "/appStoreVersions", {"data": {"type": "appStoreVersions",
                  "attributes": {"platform": "IOS", "versionString": version},
                  "relationships": {"app": {"data": {"type": "apps", "id": app["id"]}}}}})["data"]
        print(f"utworzono wersję {version}")
    elif ver["attributes"]["versionString"] != version:
        api("PATCH", f"/appStoreVersions/{ver['id']}", {"data": {"type": "appStoreVersions", "id": ver["id"],
            "attributes": {"versionString": version}}})
    ver_locs = api("GET", f"/appStoreVersions/{ver['id']}/appStoreVersionLocalizations")["data"]
    for src, loc in LOCALES.items():
        t = texts[src]
        attrs = {"description": t["full"], "keywords": t["keywords"], "promotionalText": t["promo"],
                 "marketingUrl": "https://planbudowlany.online"}
        if a.support_url:
            attrs["supportUrl"] = a.support_url
        loc_id = upsert_loc(api, "appStoreVersionLocalizations", "appStoreVersion", ver["id"], ver_locs, loc, attrs)
        print(f"opis {loc}")
        if not a.no_screens:
            for display, folder in SCREENS.items():
                n = upload_screens(api, loc_id, display, sorted((STORE / folder).glob("*.png")))
                print(f"  zrzuty {display}: {n}")
    print("Gotowe – karta w App Store Connect zaktualizowana (bez wysyłki do recenzji)")


if __name__ == "__main__":
    main()
