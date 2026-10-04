#!/usr/bin/env python3
"""Wysyłka AAB do Google Play (Google Play Developer API v3) na wybraną ścieżkę testów.

    python3 GODOT/tools/play_upload.py plik.aab [--track internal] [--changelog GBA/CHANGELOG.md --version v0.21.49]

Konto serwisowe: PLAY_SERVICE_ACCOUNT_JSON (z otoczenia albo GODOT/.env.local); w Play Console musi mieć uprawnienia do aplikacji online.planbudowlany.rogue.
Klucz konta służy tylko do podpisania tokenu (nie jest wypisywany). Pierwszy AAB nowej aplikacji Google każe
wgrać ręcznie w Play Console; kolejne może wysyłać ten skrypt.
"""
import argparse, base64, json, os, pathlib, re, sys, time, urllib.parse, urllib.request, urllib.error
from cryptography.hazmat.primitives import hashes, serialization
from cryptography.hazmat.primitives.asymmetric import padding

PACKAGE = "online.planbudowlany.rogue"
API = "https://androidpublisher.googleapis.com/androidpublisher/v3/applications"
UPLOAD = "https://androidpublisher.googleapis.com/upload/androidpublisher/v3/applications"


def env(name):
    """Zmienna z otoczenia albo z GODOT/.env.local (poza gitem; wzór GODOT/.env.example)."""
    if name in os.environ:
        return os.environ[name]
    path = pathlib.Path(__file__).resolve().parent.parent / ".env.local"
    if path.exists():
        for line in path.read_text().splitlines():
            if "=" in line and not line.lstrip().startswith("#"):
                k, v = line.split("=", 1)
                if k.strip() == name:
                    return os.path.expandvars(v.strip())
    return None


def b64(data):
    return base64.urlsafe_b64encode(data).rstrip(b"=").decode()


def access_token(account_path):
    acc = json.loads(pathlib.Path(account_path).read_text())
    now = int(time.time())
    header = b64(json.dumps({"alg": "RS256", "typ": "JWT"}).encode())
    claims = b64(json.dumps({"iss": acc["client_email"], "scope": "https://www.googleapis.com/auth/androidpublisher",
                             "aud": acc["token_uri"], "iat": now, "exp": now + 3000}).encode())
    key = serialization.load_pem_private_key(acc["private_key"].encode(), password=None)
    sig = b64(key.sign(f"{header}.{claims}".encode(), padding.PKCS1v15(), hashes.SHA256()))
    body = urllib.parse.urlencode({"grant_type": "urn:ietf:params:oauth:grant-type:jwt-bearer",
                                   "assertion": f"{header}.{claims}.{sig}"}).encode()
    with urllib.request.urlopen(urllib.request.Request(acc["token_uri"], data=body)) as res:
        return json.load(res)["access_token"]


def call(tok, method, url, body=None, data=None, ctype="application/json"):
    payload = data if data is not None else (json.dumps(body).encode() if body is not None else None)
    req = urllib.request.Request(url, method=method, data=payload,
                                 headers={"Authorization": f"Bearer {tok}", "Content-Type": ctype})
    try:
        with urllib.request.urlopen(req, timeout=600) as res:
            raw = res.read()
            return json.loads(raw) if raw else {}
    except urllib.error.HTTPError as e:
        sys.exit(f"{method} {url.split('?')[0]}: HTTP {e.code} {e.read().decode()[:600]}")


def release_notes(changelog, version):
    """Sekcja wersji z CHANGELOG.md jako zwykły tekst (Play: maks. 500 znaków)."""
    if not changelog or not version or not os.path.exists(changelog):
        return None
    text = pathlib.Path(changelog).read_text()
    m = re.search(rf"^## {re.escape(version)}\b.*?$(.*?)(?=^## |\Z)", text, re.M | re.S)
    if not m:
        return None
    lines = []
    for line in m.group(1).splitlines():
        line = re.sub(r"\*\*|`", "", line).strip()
        if not line or line.startswith("|"):
            continue
        lines.append(line.lstrip("#").strip())
    notes = "\n".join(lines)
    return notes if len(notes) <= 500 else notes[:497].rsplit("\n", 1)[0] + "\n…"


def main():
    home = os.path.expanduser("~")
    ap = argparse.ArgumentParser()
    ap.add_argument("aab")
    ap.add_argument("--track", default="internal")
    ap.add_argument("--status", default="completed", help="completed albo draft (draft dla aplikacji przed publikacją)")
    ap.add_argument("--changelog")
    ap.add_argument("--version")
    ap.add_argument("--account", default=env("PLAY_SERVICE_ACCOUNT_JSON"))
    a = ap.parse_args()

    tok = access_token(a.account)
    edit = call(tok, "POST", f"{API}/{PACKAGE}/edits", {})["id"]
    print(f"Edycja {edit}, wysyłanie {pathlib.Path(a.aab).name}…")
    bundle = call(tok, "POST", f"{UPLOAD}/{PACKAGE}/edits/{edit}/bundles?uploadType=media",
                  data=pathlib.Path(a.aab).read_bytes(), ctype="application/octet-stream")
    code = bundle["versionCode"]
    release = {"versionCodes": [str(code)], "status": a.status, "name": f"{a.version or ''} ({code})".strip()}
    notes = release_notes(a.changelog, a.version)
    if notes:
        release["releaseNotes"] = [{"language": "pl-PL", "text": notes}]
    call(tok, "PUT", f"{API}/{PACKAGE}/edits/{edit}/tracks/{a.track}", {"track": a.track, "releases": [release]})
    call(tok, "POST", f"{API}/{PACKAGE}/edits/{edit}:commit")
    print(f"Wysłano versionCode {code} na ścieżkę {a.track} ({a.status})")


if __name__ == "__main__":
    main()
