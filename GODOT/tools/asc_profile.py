#!/usr/bin/env python3
"""Profil App Store (IOS_APP_STORE) dla PB Rogue przez App Store Connect API – tworzy go, jeśli nie istnieje,
i instaluje w ~/Library/MobileDevice/Provisioning Profiles. Klucz .p8 służy tylko do podpisania tokenu JWT
(nie jest nigdzie wypisywany ani kopiowany).

    python3 GODOT/tools/asc_profile.py [--name PB-Rogue-AppStore] [--bundle online.planbudowlany.rogue]
"""
import argparse, base64, json, os, pathlib, time, urllib.request, urllib.error
from cryptography.hazmat.primitives import hashes, serialization
from cryptography.hazmat.primitives.asymmetric import ec
from cryptography.hazmat.primitives.asymmetric.utils import decode_dss_signature

API = "https://api.appstoreconnect.apple.com/v1"


def b64(data):
    return base64.urlsafe_b64encode(data).rstrip(b"=").decode()


def token(key_path, key_id, issuer):
    key = serialization.load_pem_private_key(pathlib.Path(key_path).read_bytes(), password=None)
    header = b64(json.dumps({"alg": "ES256", "kid": key_id, "typ": "JWT"}).encode())
    now = int(time.time())
    payload = b64(json.dumps({"iss": issuer, "iat": now, "exp": now + 1000, "aud": "appstoreconnect-v1"}).encode())
    r, s = decode_dss_signature(key.sign(f"{header}.{payload}".encode(), ec.ECDSA(hashes.SHA256())))
    return f"{header}.{payload}.{b64(r.to_bytes(32, 'big') + s.to_bytes(32, 'big'))}"


def call(tok, method, path, body=None):
    req = urllib.request.Request(API + path, method=method, data=json.dumps(body).encode() if body else None,
                                 headers={"Authorization": f"Bearer {tok}", "Content-Type": "application/json"})
    try:
        with urllib.request.urlopen(req) as res:
            return json.load(res)
    except urllib.error.HTTPError as e:
        raise SystemExit(f"{method} {path}: HTTP {e.code} {e.read().decode()[:400]}")


def main():
    home = os.path.expanduser("~")
    ap = argparse.ArgumentParser()
    ap.add_argument("--name", default="PB-Rogue-AppStore")
    ap.add_argument("--bundle", default="online.planbudowlany.rogue")
    ap.add_argument("--key", default=os.environ.get("ASC_KEY", f"{home}/Dev/PlanerBudowlany/Organizacja/Mobile/certs/AuthKey_REDACTED_KEY_ID.p8"))
    ap.add_argument("--key-id", default=os.environ.get("ASC_KEY_ID", "REDACTED_KEY_ID"))
    ap.add_argument("--issuer", default=os.environ.get("ASC_ISSUER_ID", "REDACTED_ISSUER_ID"))
    a = ap.parse_args()
    tok = token(a.key, a.key_id, a.issuer)

    bundles = call(tok, "GET", f"/bundleIds?filter[identifier]={a.bundle}&limit=5")["data"]
    bundle = next((b for b in bundles if b["attributes"]["identifier"] == a.bundle), None)
    if not bundle:
        raise SystemExit(f"Brak bundle id {a.bundle} w App Store Connect")

    profiles = call(tok, "GET", f"/profiles?filter[name]={a.name}&limit=10")["data"]
    prof = next((p for p in profiles if p["attributes"]["profileState"] == "ACTIVE"), None)
    if not prof:
        certs = [c for c in call(tok, "GET", "/certificates?limit=50")["data"]
                 if c["attributes"]["certificateType"] in ("DISTRIBUTION", "IOS_DISTRIBUTION")]
        if not certs:
            raise SystemExit("Brak certyfikatu dystrybucyjnego w zespole")
        prof = call(tok, "POST", "/profiles", {"data": {"type": "profiles",
            "attributes": {"name": a.name, "profileType": "IOS_APP_STORE"},
            "relationships": {"bundleId": {"data": {"type": "bundleIds", "id": bundle["id"]}},
                              "certificates": {"data": [{"type": "certificates", "id": c["id"]} for c in certs]}}}})["data"]
        print(f"Utworzono profil {a.name}")
    attrs = prof["attributes"]
    content = base64.b64decode(attrs["profileContent"])
    for d in (f"{home}/Library/MobileDevice/Provisioning Profiles", f"{home}/Library/Developer/Xcode/UserData/Provisioning Profiles"):
        pathlib.Path(d).mkdir(parents=True, exist_ok=True)
        pathlib.Path(d, f"{attrs['uuid']}.mobileprovision").write_bytes(content)
    print(f"Profil {a.name} ({attrs['uuid']}) zainstalowany, ważny do {attrs['expirationDate']}")


if __name__ == "__main__":
    main()
