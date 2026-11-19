#!/usr/bin/env python3
"""Żywy przewodnik projektowy gry (bronie, zawody, problemy budowy, etapy) – składany z AKTUALNEGO GBA/data/game.json.

Uruchom z katalogu repozytorium:   python3 docs/przewodnik/make_przewodnik.py
Wynik: docs/przewodnik/przewodnik_gry.html i .pdf (Chrome headless). Najpierw odświeża sprite'y broni
(GBA/tools/weapon_sprites.py -> art/weapons/), obrazki wkleja do HTML jako data URI (działa w PDF).
Styl i siatki wzorów jak w docs/v0.23/make_katalog_broni.py.
"""
import base64
import html
import io
import json
import pathlib
import subprocess
import sys

from PIL import Image

ROOT = pathlib.Path(__file__).resolve().parents[2]
OUT = pathlib.Path(__file__).resolve().parent
ART = ROOT / "art" / "weapons"
ACTORS = ROOT / "GODOT" / "godot" / "assets" / "sprites" / "actors.png"
CHROME = "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome"

subprocess.run([sys.executable, str(ROOT / "GBA" / "tools" / "weapon_sprites.py")], check=True, capture_output=True)
D = json.load(open(ROOT / "GBA" / "data" / "game.json", encoding="utf-8"))
MAN = json.load(open(ART / "weapons.json", encoding="utf-8"))
ICONS = Image.open(ART / "weapons_icons_2x.png")
INHAND = Image.open(ART / "weapons_inhand_2x.png")
ACT_IMG = Image.open(ACTORS) if ACTORS.exists() else None

W = {w["id"]: w for w in D["weapons"]}
WROW = {w["id"]: w for w in MAN["weapons"]}
E = {e["id"]: e for e in D["enemies"]}
STAT = {"strength": "SIŁ", "agility": "ZRĘ", "intelligence": "INT"}
HIT = {"blunt": 0, "cut": 1, "pierce": 2}
HIT_NAMES = D["hitTypes"]["names"]
HIT_COL = ["#6b4eff", "#e0584d", "#0f8a6a"]
ELEM = {"prad": ("prąd", "#d9a400"), "iskra": ("iskra", "#e8772e"), "woda": ("woda", "#2f8fd6"), "ogien": ("ogień", "#d63c3c")}
MATS = [m["name"] for m in D["materials"]["list"]] + ["woda (żywioł)", "bez materiału"]
MAT_NAME = {m["id"]: m["name"] for m in D["materials"]["list"]}
RAR = D["weaponRarities"]
FAM = {"swing": "zamach", "slam": "uderzenie z góry", "stab": "pchnięcie", "shoot": "strzał", "beam": "wiązka", "spray": "natrysk"}


def esc(s):
    return html.escape(str(s))


def png_uri(im, scale=1):
    if scale != 1:
        im = im.resize((im.width * scale, im.height * scale), Image.NEAREST)
    buf = io.BytesIO()
    im.save(buf, "PNG")
    return "data:image/png;base64," + base64.b64encode(buf.getvalue()).decode()


def weapon_icon(wid, r=None, size=32, title=""):
    m = WROW.get(wid)
    if not m:
        return ""
    keys = MAN["rarityKeys"]
    if r is None:
        r = keys.index(m["rarities"][-1]) if m["master"] else 2
    im = ICONS.crop((r * 32, m["row"] * 32, r * 32 + 32, m["row"] * 32 + 32))
    return f'<img class="px" src="{png_uri(im)}" width="{size}" height="{size}" title="{esc(title)}">'


def weapon_inhand(wid, r, d="right", size=32):
    m = WROW[wid]
    di = MAN["directions"].index(d)
    im = INHAND.crop(((r * 4 + di) * 32, m["row"] * 32, (r * 4 + di) * 32 + 32, m["row"] * 32 + 32))
    return f'<img class="px" src="{png_uri(im)}" width="{size}" height="{size}">'


def actor(frame, size=32):
    if ACT_IMG is None or frame is None or (frame + 1) * 32 > ACT_IMG.height:
        return ""
    im = ACT_IMG.crop((0, frame * 32, 32, frame * 32 + 32))
    return f'<img class="px" src="{png_uri(im)}" width="{size}" height="{size}">'


# ---------------------------------------------------------------- wzory pól (bohater w środku, patrzy w górę)
PAT_NAME = {"single": "Pojedynczy", "arc": "Łuk", "around": "Wokół", "cross": "Krzyż", "line": "Linia",
            "lineBoth": "Linia w obie strony", "line4": "Linie w 4 strony", "shot": "Strzał", "throw": "Rzut 3x3",
            "spray": "Natrysk", "push": "Pchnięcie"}
PAT_DESC = {"single": "cel w zasięgu (pole przed bohaterem)", "arc": "przód i oba skosy (3 pola)",
            "around": "8 pól dookoła", "cross": "przód, tył i boki (4 pola)", "line": "N pól w linii, przebija",
            "lineBoth": "linia N pól przed i za bohaterem", "line4": "linie N pól w 4 strony",
            "shot": "pierwszy cel w linii do N pól", "throw": "obszar 3x3 w odległości N",
            "spray": "pole przed + rząd 3 pól dalej", "push": "cel + odrzut"}


def cells(pat, rng):
    """{(x, y): 'X'|'o'} względem bohatera (0, 0), góra = -y."""
    r = max(1, rng)
    c = {}
    if pat in ("single", "push"):
        c[(0, -1)] = "X"
        if pat == "push":
            c[(0, -2)] = "o"
        elif r > 1:
            for y in range(-r, r + 1):
                for x in range(-r, r + 1):
                    if (x, y) != (0, 0) and (x, y) not in c:
                        c[(x, y)] = "r"
    elif pat == "shot":
        c[(0, -1)] = "X"
        for k in range(2, r + 1):
            c[(0, -k)] = "o"
    elif pat == "line":
        for k in range(1, r + 1):
            c[(0, -k)] = "X"
    elif pat == "lineBoth":
        for k in range(1, r + 1):
            c[(0, -k)] = c[(0, k)] = "X"
    elif pat == "line4":
        for k in range(1, r + 1):
            for dx, dy in ((0, -1), (1, 0), (0, 1), (-1, 0)):
                c[(dx * k, dy * k)] = "X"
    elif pat == "arc":
        for x in (-1, 0, 1):
            c[(x, -1)] = "X"
    elif pat == "around":
        for y in (-1, 0, 1):
            for x in (-1, 0, 1):
                if x or y:
                    c[(x, y)] = "X"
    elif pat == "cross":
        for p in ((0, -1), (0, 1), (-1, 0), (1, 0)):
            c[p] = "X"
    elif pat == "spray":
        c[(0, -1)] = "X"
        for x in (-1, 0, 1):
            c[(x, -2)] = "X"
    elif pat == "throw":
        for y in (-r - 1, -r, -r + 1):
            for x in (-1, 0, 1):
                c[(x, y)] = "X"
    return c


def grid(pat, rng, small=False):
    c = cells(pat, rng)
    ext = max([2] + [max(abs(x), abs(y)) for x, y in c])
    ext = min(ext, 4)
    n = ext * 2 + 1
    out = []
    for y in range(-ext, ext + 1):
        for x in range(-ext, ext + 1):
            v = "h" if (x, y) == (0, 0) else {"X": "x", "o": "o", "r": "r"}.get(c.get((x, y)), "e")
            out.append(f'<i class="{v}"></i>')
    sz = 6 if small else 9
    return f'<div class="grid" style="grid-template-columns:repeat({n},{sz}px);width:{n * (sz + 1)}px">' + \
        "".join(out).replace("<i ", f'<i style="width:{sz}px;height:{sz}px" ', ) + "</div>"


# ---------------------------------------------------------------- opisy efektów trybów
STATUS = D["statuses"]


def effect_text(eff, m):
    v, t = m.get("value"), m.get("turns")
    tt = f" ({t} t.)" if t else ""
    return {
        "mark": f"Oznaczony: +{v}% obrażeń{tt}",
        "stun": f"ogłuszenie {v if v else 100}%{tt}",
        "chain": "łańcuch prądu na mokre cele obok",
        "dry": f"osusza / zakręca wodę{tt}",
        "push": f"odrzut o {v or 1}",
        "openWall": "otwiera pęknięte ściany" + (f" ({v} ładunki)" if v else ""),
        "defdown": ("ignoruje OBR" if v == 99 else f"−{v} OBR") + tt,
        "reveal": "odsłania pokój" + (f", kryt +{v}%" if v else "") + tt,
        "swap": "zamiana miejscami z celem",
        "root": f"unieruchomienie{tt}",
        "pull": f"przyciąga o {v}",
        "dot": f"{v} obrażeń/turę{tt}",
        "wet": f"moczy (Mokry){tt}",
        "ignite": "podpala",
    }.get(eff, eff + (f" {v}" if v else "") + tt)


def mode_text(m, w, b=False):
    rng = m.get("range", w["range"])
    parts = [PAT_NAME.get(m["pattern"], m["pattern"]) + (f" {rng}" if m["pattern"] in ("line", "lineBoth", "line4", "shot", "throw") else "")]
    if m.get("dmgPct", 100) != 100:
        parts.append(f"{m['dmgPct']}% obrażeń")
    if m.get("hits"):
        parts.append(f"{m['hits']} ciosy")
    if m.get("effect"):
        parts.append(effect_text(m["effect"], m))
    if m.get("effect2"):
        parts.append(effect_text(m["effect2"], {}))
    if m.get("steelPct"):
        parts.append(f"stal {m['steelPct']}%")
    head = f"<b>{'B' if b else 'A'}</b>" + (f" · <i>{esc(m['name'])}</i>" if m.get("name") else "")
    cost = f'<div class="cost">odnowienie {m["cooldown"]}</div>' if m.get("cooldown") else ""
    return f'<div class="m">{grid(m["pattern"], rng, True)}<div>{head}<br>{esc(", ".join(parts))}{cost}</div></div>'


def hits_of(w):
    h = w.get("hit", [])
    return h if isinstance(h, list) else [h]


def hit_badges(w):
    out = "".join(f'<span class="type" style="background:{HIT_COL[HIT[h]]}">{esc(HIT_NAMES[HIT[h]])}</span> ' for h in hits_of(w))
    el = w.get("element")
    if el:
        n, c = ELEM.get(el, (el, "#555"))
        out += f'<span class="type" style="background:{c}">{esc(n)}</span>'
    return out


# ---------------------------------------------------------------- źródła broni
def sources():
    src = {}
    for c in D["classes"]:
        src.setdefault(c["weapon"], []).append(f"zawód: {c['name']}")
    meta = D["meta"]
    for t in meta["tools"] + meta["newTools"]:
        s = []
        if t.get("shop"):
            s.append("Szkolenia")
        if t.get("reward"):
            s.append("nagroda")
        if t.get("secret"):
            s.append("sekret")
        if t.get("drop"):
            s.append("drop")
        if t.get("hurtownia"):
            s.append("Hurtownia")
        if t.get("artifact"):
            s.append("artefakt (Markowa baza z dropu)")
        if not s:
            s.append("w dropach i Hurtowni od pierwszej budowy (bez odblokowania)")
        src.setdefault(t["weapon"], []).extend(s)
    for sc in D["secrets"]["list"]:
        if sc["reward"].get("kind") == "tool":
            src.setdefault(sc["reward"]["id"], [])
            src[sc["reward"]["id"]] = [x if x != "sekret" else f"sekret: {sc['desc']}" for x in src[sc["reward"]["id"]]]
    return src


SRC = sources()


def weapon_rows():
    rows = []
    for w in D["weapons"]:
        wid = w["id"]
        stats = [f"{w['minDamage']}–{w['maxDamage']} obr.", f"zasięg {w['range']}", STAT.get(w["scalesWith"], w["scalesWith"])]
        if w.get("crit"):
            stats.append(f"kryt +{w['crit']}%")
        if w.get("knockback"):
            stats.append("odrzut")
        if w.get("reveal"):
            stats.append("odsłania")
        if w.get("pierce"):
            stats.append(f"ignoruje {w['pierce']} OBR")
        if w.get("neutral"):
            stats.append("materiał bez znaczenia")
        master = '<span class="tag gold">Mistrzowska</span>' if w.get("master") else ""
        fam = FAM.get(WROW.get(wid, {}).get("family", ""), "")
        desc = f'<div class="dim it">{esc(w["desc"])}</div>' if w.get("desc") else ""
        rows.append(f"""<tr>
<td class="ic">{weapon_icon(wid, size=40)}</td>
<td><b>{esc(w['name'])}</b> {master}<div class="dim mono">{esc(wid)} · anim.: {esc(fam)}</div>{desc}
<div class="dim">{esc(', '.join(SRC.get(wid, ['?'])))}</div></td>
<td>{hit_badges(w)}<div class="stats">{esc(' · '.join(stats))}</div></td>
<td class="mode">{mode_text(w['modeA'], w)}</td>
<td class="mode">{mode_text(w['modeB'], w, True)}</td>
</tr>""")
    return "\n".join(rows)


# ---------------------------------------------------------------- sekcje
def sec_hit():
    ht = D["hitTypes"]
    rows = "".join(
        f"<tr><th style='color:{HIT_COL[i]}'>{esc(n)}</th>" + "".join(
            f'<td class="{"hi" if v > 100 else "lo" if v < 100 else ""}">{v}%</td>' for v in ht["materialPct"][i]) + "</tr>"
        for i, n in enumerate(ht["names"]))
    head = "<tr><th></th>" + "".join(f"<th>{esc(m)}</th>" for m in MATS) + "</tr>"
    count = {i: sum(1 for w in D["weapons"] if any(HIT[h] == i for h in hits_of(w))) for i in range(3)}
    mats = {m: sum(1 for e in D["enemies"] if e.get("material") == m) for m in MAT_NAME}
    rules = f"""<ul>
<li><b>{esc(ht['names'][0])}</b>: {ht['stunChancePct']}% szans na ogłuszenie (cel traci turę). Bronie: {count[0]}.</li>
<li><b>{esc(ht['names'][1])}</b>: +{ht['cutCritPct']}% szans na kryt. Bronie: {count[1]}.</li>
<li><b>{esc(ht['names'][2])}</b>: ignoruje {ht['pierceDef']} OBR (albo własną wartość broni). Bronie: {count[2]}.</li>
<li>Broń z dwoma typami bierze <b>lepszy</b> mnożnik dla danego materiału.</li>
<li>Problemy wg materiału: {", ".join(f"{esc(MAT_NAME[k])} {v}" for k, v in mats.items())}, bez materiału {sum(1 for e in D["enemies"] if not e.get("material"))}.</li>
</ul>"""
    elem = f"""<table class="small"><tr><th>Żywioł broni</th><th>Działanie</th><th>Bronie</th></tr>
<tr><td><span class="type" style="background:{ELEM['iskra'][1]}">iskra</span></td><td>stal ×{ht['sparkSteelPct']}%, w pyle – Wybuch pyłu</td><td>{esc(', '.join(w['name'] for w in D['weapons'] if w.get('element') == 'iskra'))}</td></tr>
<tr><td><span class="type" style="background:{ELEM['prad'][1]}">prąd</span></td><td>woda / Mokry ×{ht['powerWaterPct']}%, combo Porażenie</td><td>{esc(', '.join(w['name'] for w in D['weapons'] if w.get('element') == 'prad'))}</td></tr>
<tr><td><span class="type" style="background:{ELEM['ogien'][1]}">ogień</span></td><td>drewno ×{ht['fireWoodPct']}%, woda ×{ht['fireWaterPct']}%, podpalenie {ht['dotPerTurn']}/turę, osusza Mokry</td><td>{esc(', '.join(w['name'] for w in D['weapons'] if w.get('element') == 'ogien'))}</td></tr>
<tr><td><span class="type" style="background:{ELEM['woda'][1]}">woda</span></td><td>moczy cele (Mokry) – przygotowuje prąd</td><td>{esc(', '.join(w['name'] for w in D['weapons'] if w.get('element') == 'woda'))}</td></tr>
</table>"""
    combos = "".join(f"<li><b>{esc(c['name'])}</b> ({esc(c['short'])}): {esc(c['info'])}</li>" for c in D["combos"]["list"])
    st = "".join(f"<tr><td><b>{esc(v['name'])}</b></td><td class='mono'>{esc(k)}</td><td>{esc(v['effect'])}</td></tr>" for k, v in STATUS.items())
    pats = "".join(f'<div class="pat">{grid(k, 3)}<b>{esc(v)}</b><span>{esc(PAT_DESC[k])}</span></div>' for k, v in PAT_NAME.items())
    return f"""<h2>2. Typy ciosu i skuteczność</h2>
<table><caption>Mnożnik obrażeń wg materiału problemu (<code>hitTypes.materialPct</code>)</caption>{head}{rows}</table>
{rules}
<h3>Żywioły</h3>{elem}
<h3>Kombinacje stanów</h3><ul class="small">{combos}</ul>
<h3>Stany (<code>statuses</code>)</h3><table class="small"><tr><th>Stan</th><th>id</th><th>Działanie</th></tr>{st}</table>
<h3>Wzory pól</h3><div class="pats">{pats}</div>
<p class="dim">Niebieskie kółko – bohater (patrzy w górę), pomarańczowe – trafione pola, jasne – dalszy tor (strzał, odrzut),
szare – zasięg celu pojedynczego. Przykład dla N = 3. Wzory obracają się z kierunkiem, na skos o 45°.</p>"""


def sec_rarity():
    keys = MAN["rarityKeys"]
    cards = ""
    housing = [("ciemna zieleń + czerń"), ("morski niebieski"), ("żółty + czerń"), ("czerwień + biel")]
    for i, r in enumerate(RAR):
        ex = "".join(weapon_icon(w, i, 40) for w in ("wkretarka", "szlifierka", "mlot", "kielnia"))
        inh = weapon_inhand("wkretarka", i, "right", 40) + weapon_inhand("mlot", i, "down", 40)
        pal = MAN["palettes"][keys[i]][12:]
        sw = "".join(f'<span class="chip" style="background:{c}"></span>' for c in pal)
        bonus = []
        if r.get("dmgBonus"):
            bonus.append(f"+{r['dmgBonus']} obrażeń")
        if r.get("cooldownDelta"):
            bonus.append(f"odnowienie B {r['cooldownDelta']:+d}")
        roll = "tylko unikaty (Mistrzowskie bronie)" if r["rollFrom"] > 100 else f"rzut ≥ {r['rollFrom']} na 100"
        cards += f"""<div class="rcard" style="border-top:6px solid {r['color']}">
<h3 style="color:{r['color']}">{esc(r['name'])}</h3>
<p>{esc(', '.join(bonus) or 'bazowe statystyki')} · {esc(roll)}</p>
<p class="dim">Obudowa elektronarzędzi: {esc(housing[i])}. Paleta H, h, A, *: {sw}</p>
<div class="ex">{ex}</div><div class="ex">{inh}</div></div>"""
    return f"""<h2 class="break">3. Rzadkość broni (<code>weaponRarities</code>)</h2>
<div class="rgrid">{cards}</div>
<ul>
<li><b>Elektronarzędzia</b> (wkrętarka, szlifierka, gwoździarka, młot udarowy, pilarka, opalarka, myjka, pistolet do kotew,
spawarka, lasery, lutownica, palnik, agregat, mieszadło, pistolet do silikonu, taker…) – kolor <b>obudowy</b> wg rzadkości.</li>
<li><b>Narzędzia ręczne</b> (młoty, kielnia, łopata, kilof, łom, klucz, siekiera, piła, nożyce, noże, poziomica, tyczka, przecinak) –
drewno, stal, guma; kolor rzadkości tylko na <b>akcentach</b> (pasek rękojeści, owijka, okładka).</li>
<li><b>Wspólny akcent</b>: czarny obrys + 1 px poświaty w kolorze rzadkości; Mistrzowska z iskrą. Ikony mają też znacznik
bez koloru (Solidna 1 kropka, Markowa 2, Mistrzowska iskra) – dla daltonistów.</li>
<li><b>Bez marek</b>: kolory obudów to umowne „klasy jakości”, nigdy logotypy, napisy ani nazwy producentów.</li>
<li>Bronie Mistrzowskie istnieją tylko w tej rzadkości: {esc(', '.join(w['name'] for w in D['weapons'] if w.get('master')))}.</li>
</ul>
<p class="dim">Sprite'y: <code>art/weapons/</code> (opis w README), generator <code>GBA/tools/weapon_sprites.py</code>.</p>"""


def sec_weapons():
    return f"""<h2 class="break">4. Bronie ({len(D['weapons'])})</h2>
<p class="dim">Ikona w rzadkości Markowej (Mistrzowskie – złotej). Obrażenia = rzut min–max + premia statystyki
(SIŁ/ZRĘ/INT +1 co 2 pkt) i inne premie; tryb B ma odnowienie w turach (Markowa −1).</p>
<table class="cat"><tr><th style="width:6%"></th><th style="width:25%">Broń, źródło</th><th style="width:17%">Typ, statystyki</th>
<th style="width:26%">Tryb A</th><th style="width:26%">Tryb B</th></tr>{weapon_rows()}</table>"""


PASSIVE = {"windproof": "pogoda Wiatr nie skraca zasięgu ataków z dystansu (porywy Aktu II dalej spychają)",
           "push": f"cios wręcz odpycha problem o pole ({D['passives'].get('pushChancePct', '?')}% szans, bez bossa); "
                   "wybija pękniętą ścianę magazynu bez klucza",
           "surveyor": "widzi cały plac od początku etapu (także dokumenty Aktu 0)"}


def class_unlock(c, i):
    meta = D["meta"]
    if c["id"] in meta["startClasses"]:
        return "od startu"
    if c.get("reward"):
        return "nagroda za postęp"
    for sc in D["secrets"]["list"]:
        if sc["reward"].get("kind") == "class" and sc["reward"]["id"] == c["id"]:
            return f"sekret: {sc['desc']}"
    buy = [x for x in D["classes"] if x["id"] not in meta["startClasses"] and not x.get("reward") and not x.get("secret")]
    if c in buy:
        k = buy.index(c)
        cost = meta["classCosts"][min(k, len(meta["classCosts"]) - 1)]
        return f"Szkolenia ({cost} XP)"
    return "?"


def sec_classes():
    rows = ""
    for i, c in enumerate(D["classes"]):
        ab = c["ability"]
        w = W.get(c["weapon"], {})
        passive = PASSIVE.get(c.get("passive"), c.get("passive", "")) if c.get("passive") else "–"
        rows += f"""<tr><td class="ic">{actor(c.get('frame'), 40)}</td>
<td><b>{esc(c['name'])}</b><div class="dim it">{esc(c['desc'])}</div><div class="dim">{esc(class_unlock(c, i))}</div></td>
<td class="ic">{weapon_icon(c['weapon'], 0, 36)}</td><td>{esc(w.get('name', c['weapon']))}<div>{hit_badges(w) if w else ''}</div></td>
<td class="num">{c['maxHealth']}</td><td class="num">{c['strength']}</td><td class="num">{c['agility']}</td><td class="num">{c['intelligence']}</td>
<td class="num">{c['defense']}</td><td class="num">{c['luck']}</td>
<td><b>{esc(ab['name'])}</b>: {esc(ab['desc'])}<div class="dim">odnowienie {ab['cooldown']}</div></td><td>{esc(passive)}</td></tr>"""
    return f"""<h2 class="break">5. Zawody ({len(D['classes'])})</h2>
<table class="cls"><tr><th></th><th>Zawód, odblokowanie</th><th colspan="2">Broń domyślna</th><th>HP</th><th>SIŁ</th><th>ZRĘ</th><th>INT</th>
<th>OBR</th><th>SZCZ</th><th>Moc</th><th>Cecha</th></tr>{rows}</table>"""


def enemy_stages():
    where = {}
    acts = D["acts"]
    for s in D["stages"]:
        a = acts[s["act"]]["numeral"] if s["act"] < len(acts) else "?"
        for e in s["enemies"]:
            where.setdefault(e, []).append(f"{a}: {s['name']}")
        if s.get("boss"):
            where.setdefault(s["boss"], []).append(f"{a}: {s['name']} (boss)")
    for c in D["career"]["list"]:
        seen = set()
        for s in c.get("stages", []):
            for e in s.get("enemies", []):
                if e not in seen:
                    seen.add(e)
                    where.setdefault(e, []).append(f"kontrakt {c['short']}")
        if c.get("boss"):
            where.setdefault(c["boss"], []).append(f"kontrakt: {c['short']} (boss)")
    return where


SHAPE = {"cross": "krzyż", None: "wokół"}


def sec_enemies():
    where = enemy_stages()
    beh = D["behaviorNames"]
    rows = ""
    for e in D["enemies"]:
        boss = bool(e.get("phase") or e.get("reward") or e.get("summon")) or any("(boss)" in x for x in where.get(e["id"], []))
        el = e.get("element")
        bits = [beh.get(b, b) for b in e.get("behaviors", [])]
        if e.get("slam"):
            bits.append(f"cios obszarowy „{e.get('slamName', '')}” ({SHAPE.get(e.get('slamShape'), 'wokół')})" if e.get("slamName") else "cios obszarowy")
        if e.get("summon"):
            s = e["summon"]
            bits.append(f"przywołuje {E.get(s['enemy'], {}).get('name', s['enemy'])} co {s['every']} t. (maks. {s['max']})")
        if e.get("phase"):
            p = e["phase"]
            bits.append(f"faza „{p.get('name', '')}” przy {p['atPct']}% HP (leczy {p.get('healPct', 0)}%)")
        if e.get("onHit"):
            o = e["onHit"]
            bits.append(f"trafienie: {STATUS.get(o['status'], {}).get('name', o['status'])} {o.get('chancePct', '')}% / {o.get('turns', '')} t.")
        rew = e.get("reward")
        rewtxt = f"{e['score']} pkt" + (f", {rew.get('cash', 0)} zł, tytuł „{rew.get('title', '')}”" if rew else "")
        mat = MAT_NAME.get(e.get("material"), "–")
        elt = ELEM.get(el, (el or "",))[0]
        rows += f"""<tr class="{'boss' if boss else ''}"><td class="ic">{actor(e.get('frame'), 32)}</td>
<td><b>{esc(e['name'])}</b>{' <span class="tag">boss</span>' if boss else ''}<div class="dim it">{esc(e.get('desc', ''))}</div></td>
<td>{esc(mat)}{('<br>' + esc(elt)) if elt else ''}</td><td class="num">{e['maxHealth']}</td><td class="num">{e['defense']}</td>
<td class="num">{e['minDamage']}–{e['maxDamage']}</td><td class="num">{e['sight']}</td>
<td>{esc('; '.join(bits) or '–')}</td><td class="dim">{esc(rewtxt)}</td><td class="dim">{esc('; '.join(where.get(e['id'], [])) or '– (inne źródło)')}</td></tr>"""
    return f"""<h2 class="break">6. Problemy budowy ({len(D['enemies'])})</h2>
<p class="dim">Przeciwnicy to zawsze <b>problemy budowy</b> (przeciek, zwarcie, termin…), nigdy ludzie ani zawody.</p>
<table class="en"><tr><th></th><th>Problem</th><th>Materiał / żywioł</th><th>HP</th><th>OBR</th><th>Obr.</th><th>Wzrok</th>
<th>Zachowanie, efekty</th><th>Nagroda</th><th>Gdzie (akt: etap)</th></tr>{rows}</table>"""


def sec_stages():
    acts = D["acts"]
    rows = ""
    for s in sorted(D["stages"], key=lambda s: (0 if acts[s["act"]].get("prelude") else 1, s["act"])):
        a = acts[s["act"]]
        boss = E.get(s.get("boss"), {}).get("name", "")
        en = ", ".join(E.get(x, {}).get("name", x) for x in s["enemies"])
        rows += f"""<tr><td>{esc(a['numeral'])}</td><td><b>{esc(s['name'])}</b></td><td>{esc(en)}</td><td class="num">{s['count']}</td>
<td class="num">{s['hpPct']}%</td><td class="num">+{s['dmgBonus']}</td><td>{esc(boss) or '–'}</td></tr>"""
    act_list = "".join(f"<li><b>Akt {esc(a['numeral'])} – {esc(a['name'])}</b>: {esc(a['mechanic']['name'])} – {esc(a['mechanic']['info'])}</li>" for a in acts)
    career = "".join(f"<li><b>{esc(c['name'])}</b> – {esc(c['desc'])}" + (f" (boss: {esc(E.get(c['boss'], {}).get('name', c['boss']))})" if c.get('boss') else "") + "</li>" for c in D["career"]["list"])
    return f"""<h2 class="break">7. Akty i etapy</h2>
<ul>{act_list}</ul>
<table class="small"><tr><th>Akt</th><th>Etap</th><th>Problemy</th><th>Ile</th><th>HP</th><th>Obr.</th><th>Boss</th></tr>{rows}</table>
<h3>Kontrakty kariery</h3><ul class="small">{career}</ul>"""


def sec_perks():
    eq = D["equipment"]
    gear = "".join(f"<tr><td><b>{esc(t['name'])}</b></td><td>{esc(t.get('short', ''))}</td><td class='mono'>{esc(t['effect'])} {esc(t.get('value', ''))}</td></tr>" for t in eq["traits"])
    tu = D["toolUpgrade"]
    tool = "".join(f"<tr><td><b>{esc(t['name'])}</b></td><td>{esc(t['desc'])}</td><td class='mono'>{esc(t['effect'])} {esc(t.get('value', ''))}</td></tr>" for t in tu["traits"])
    brar = {r["id"]: r["name"] for r in D["boons"]["rarities"]}
    bw = {r["id"]: r["weight"] for r in D["boons"]["rarities"]}
    order = {r["id"]: i for i, r in enumerate(D["boons"]["rarities"])}
    tags = {t["id"]: t["name"] for t in D["boons"]["tags"]}
    boons = "".join(
        f"<tr><td><b>{esc(b['name'])}</b>{' <span class=\"tag\">' + esc(b['class']) + '</span>' if b.get('class') else ''}</td><td>{esc(b['desc'])}</td>"
        f"<td>{esc(brar.get(b['rarity'], b['rarity']))}</td><td>{esc(', '.join(tags.get(t, t) for t in b.get('tags', [])))}</td></tr>"
        for b in sorted(D["boons"]["list"], key=lambda b: (order.get(b["rarity"], 9), b["name"])))
    wp = D["weaponPerks"]
    pname = {x["id"]: x["name"] for x in wp["pool"]}
    wpool = "".join(f"<tr><td><b>{esc(x['name'])}</b></td><td>{esc(x['short'])}</td><td>{esc(x['desc'])}</td><td class='mono'>{esc(x['effect'])} {esc(x['value'])}</td></tr>" for x in wp["pool"])
    perrar = esc(", ".join(f"{r['name']} {n if i < len(RAR) - 1 else 'stałe'}" for i, (r, n) in enumerate(zip(RAR, wp["perRarity"]))))
    hitn = dict(zip(["blunt", "cut", "pierce"], D["hitTypes"]["names"]))
    wmaster = "".join(f"<tr><td>{esc(hitn.get(h, h))}</td><td>{esc(', '.join(pname[i] for i in ids))}</td></tr>" for h, ids in wp["master"].items())
    wmaster += "".join(f"<tr><td><b>{esc(w['name'])}</b> (<code>masterPerks</code>)</td><td>{esc(', '.join(pname[i] for i in w['masterPerks']))}</td></tr>"
                       for w in D["weapons"] if w.get("masterPerks"))
    return f"""<h2 class="break">8. Losowe profity</h2>
<h3>Cechy sprzętu (<code>equipment.traits</code>, {len(eq['traits'])})</h3>
<p class="dim">Losowane na sprzęcie z dropów (rzadkości sprzętu: {esc(', '.join(eq['rarities']))}).</p>
<table class="small"><tr><th>Cecha</th><th>Skrót</th><th>Efekt</th></tr>{gear}</table>
<h3>Cechy ulepszenia narzędzia (<code>toolUpgrade.traits</code>, {len(tu['traits'])})</h3>
<p class="dim">Ulepszenie w Hurtowni: +{tu['dmg']} obrażeń za poziom (maks. +{tu['max']}), od poziomu {tu['traitAt']} losowa cecha.</p>
<table class="small"><tr><th>Cecha</th><th>Opis</th><th>Efekt</th></tr>{tool}</table>
<h3>Profity broni (<code>weaponPerks</code>, {len(wp['pool'])})</h3>
<p class="dim">Losowane przy powstaniu broni do podniesienia (drop, oferta wymiany, Hurtownia; broń zawodu na start – z seeda budowy),
bez powtórzeń; widoczne na karcie broni, w ofercie wymiany i w Hurtowni. Ten sam efekt co cecha ulepszenia sumuje się z nią.
Ile profitów wg rzadkości: {perrar}.</p>
<table class="small"><tr><th>Profit</th><th>Skrót</th><th>Opis</th><th>Efekt</th></tr>{wpool}</table>
<p class="dim">Mistrzowska nie losuje – <b>Unikat</b>: 2 stałe profity wg typu ciosu broni (<code>weaponPerks.master</code>),
broń może je nadpisać polem <code>masterPerks</code>.</p>
<table class="small"><tr><th>Typ ciosu / broń</th><th>Stałe profity Mistrzowskiej</th></tr>{wmaster}</table>
<h3>Projekty (<code>boons.list</code>, {len(D['boons']['list'])})</h3>
<p class="dim">Wybór 1 z kilku po etapie; wagi rzadkości: {esc(', '.join(f"{brar[k]} {v}" for k, v in bw.items()))}; od {D['boons']['synergyAt']} projektów z tym samym tagiem – synergia. Znacznik zawodu = projekt tylko dla niego.</p>
<table class="small boons"><tr><th>Projekt</th><th>Działanie</th><th>Rzadkość</th><th>Tagi</th></tr>{boons}</table>"""


def sec_artifacts():
    W = {w["id"]: w for w in D["weapons"]}
    cls = {c["weapon"]: c["name"] for c in D["classes"]}
    rows = ""
    for w in D["weapons"]:
        if not w.get("master"):
            continue
        b = W.get(w.get("artifact", ""))
        base = esc(b["name"]) + (f" <span class='tag'>broń zawodu: {esc(cls[b['id']])}</span>" if b and b["id"] in cls else "") if b else "–"
        pr = w.get("proc")
        mech = []
        if pr:
            mech.append(f"<b>{esc(pr['name'])}</b> (co {pr['every']}. cios): {esc(pr['desc'])}")
        if w.get("wallFree"):
            mech.append("Patent na ścianie bez odnowienia")
        if w.get("bonusPct"):
            mech.append(f"+{w['bonusPct']}% na {esc(w['bonusMaterial'])}")
        if not mech:
            mech.append(f"Patent: {esc(w['modeB'].get('name', ''))}")
        rows += (f"<tr><td>{weapon_icon(w['id'], 3, 24)} <b>{esc(w['name'])}</b><br><span class='dim'>{w['minDamage']}–{w['maxDamage']}</span></td>"
                 f"<td>{base}</td><td>{'<br>'.join(mech)}</td><td>{esc(', '.join(SRC.get(w['id'], [])))}<br><span class='dim'>{esc(w.get('hint', ''))}</span></td></tr>")
    return f"""<h2 class="break">9. Artefakty i Gablota</h2>
<p class="dim">Artefakt = unikatowa broń Mistrzowska (<code>master</code>, stałe profity) z bazą <code>artifact</code>. Gdy z dropu wypada
<b>Markowa</b> broń bazowa (albo dowolna Markowa broń, a baza to broń zawodu bohatera), z szansą {D['artifacts']['chancePct']}%
zamienia się w swój artefakt – najwyżej raz w budowie. Mechanika „co N-ty cios” (<code>proc</code>) liczy ciosy bronią w ręku (Cios
i Patent), zeruje się przy zmianie broni; licznik w HUD (błyskawica) i w opisie ataku. Gablota (Profil &gt; Katalog i Szkolenia)
pokazuje wszystkie artefakty: nieodkryte jako sylwetkę ze wskazówką, odkryte z mechaniką i datą pierwszego znalezienia.</p>
<table class="small"><tr><th>Artefakt</th><th>Baza</th><th>Mechanika</th><th>Źródło</th></tr>{rows}</table>"""


INTRO = f"""<h1>Przewodnik projektowy</h1>
<p class="dim">Plan Budowlany: Roguelike · stan danych {esc(D.get('version', ''))} · wygenerowano z <code>GBA/data/game.json</code>
skryptem <code>docs/przewodnik/make_przewodnik.py</code></p>
<div class="lead">Ten dokument jest <b>punktem odniesienia</b> przy dodawaniu treści: nowej broni, zawodu, problemu budowy czy etapu.
Wszystkie tabele powstają z aktualnych danych – po zmianie <code>game.json</code> uruchom skrypt ponownie. Zanim dodasz nowość,
sprawdź, czy nie dubluje istniejącego wzoru pól, typu ciosu albo efektu, i czy pasuje do zasad poniżej.</div>
<h2>1. Zasady treści</h2>
<ul>
<li><b>Przeciwnicy to problemy budowy</b> (przeciek, zwarcie, pleśń, termin, pieczątka…), <b>nigdy ludzie</b>, zawody ani urzędnicy.
Ludzie występują tylko jako nadawcy SMS-ów, brygada i zawody gracza.</li>
<li><b>Bez marek</b>: żadnych nazw, logotypów ani charakterystycznych barw producentów narzędzi, konsol i sklepów. Kolory rzadkości
to umowne klasy jakości.</li>
<li><b>Teksty dla gracza</b> tylko w <code>GBA/data/game.json</code> (PL) i <code>GBA/data/lang/en.json</code> (EN); <code>gen_data.py --check</code>
pilnuje fontu, szerokości i kompletności.</li>
<li><b>Logika wspólna</b> GBA i Godot: zmiana zasad w obu rdzeniach + test złoty.</li>
<li><b>Nowa broń</b> = mapa sprite'a w <code>GBA/tools/weapon_sprites.py</code> (rękojeść po lewej, maks. 12×9), typ ciosu, wzór pól A i B
(B z odnowieniem), źródło (zawód / Szkolenia / drop / Hurtownia / sekret).</li>
</ul>
<h3>Cele balansu (bot, 300 przebiegów na zawód)</h3>
<table class="small"><tr><th>Tryb</th><th>Wygrane</th><th>Tryb</th><th>Wygrane</th></tr>
<tr><td>Łatwy</td><td>50–60%</td><td>pełne Szkolenia</td><td>50–60%</td></tr>
<tr><td>Normalny</td><td>25–35%</td><td>+ Respekt</td><td>65–75%</td></tr>
<tr><td>Trudny</td><td>8–15%</td><td>Akt 0 + meta</td><td>60–70%</td></tr>
<tr><td>inwestor, wszystkie modyfikatory</td><td>5–15%</td><td>kontrakty kariery</td><td>25–40 / 60–75%</td></tr>
</table>
<p class="dim">Człowiek wygrywa łatwiej niż bot (feedback 2026-10-04, TODO #73) – przy strojeniu celuj w dolną granicę.</p>
<p class="dim">Obrażenia: {esc(' '.join(D['damageHelp']))}</p>"""

CSS = """
@page { size: A4; margin: 12mm 10mm; }
body { font-family: 'Avenir Next', 'Helvetica Neue', Arial, sans-serif; color:#14122a; font-size:9.6pt; }
h1 { color:#6b4eff; margin:0 0 2mm; font-size:24pt; } h2 { color:#6b4eff; border-bottom:2px solid #ff7a3d; padding-bottom:1mm; margin-top:6mm; }
h3 { margin:3mm 0 1mm; } h2, h3 { break-after: avoid; } .dim { color:#6b6a78; font-size:8.4pt; } .it { font-style:italic; } .mono { font-family:Menlo, monospace; font-size:7.6pt; }
p { margin:1.2mm 0; } ul { margin:1mm 0; padding-left:5mm; } li { margin:0.6mm 0; }
.lead { background:#f6f5f8; border-left:5px solid #6b4eff; padding:3mm 4mm; border-radius:2mm; }
.plan { background:#fff6e0; border-left:5px solid #d9a400; padding:2.5mm 3.5mm; border-radius:2mm; margin:3mm 0; }
table { border-collapse:collapse; width:100%; margin:1mm 0; } caption { text-align:left; font-weight:600; padding-bottom:1mm; }
th, td { border:1px solid #e2e0ea; padding:1.2mm 1.6mm; vertical-align:top; text-align:left; }
th { background:#f6f5f8; } td.hi { background:#e3f6ee; font-weight:600; } td.lo { background:#fde9e7; }
table.small td, table.small th { font-size:8.4pt; padding:0.8mm 1.4mm; }
td.num { text-align:center; white-space:nowrap; } td.ic { width:1%; text-align:center; vertical-align:middle; }
img.px { image-rendering: pixelated; image-rendering: crisp-edges; display:inline-block; }
.grid { display:grid; grid-gap:1px; flex:none; } .grid i { display:block; background:#eceaf2; }
.grid i.h { background:#2f6fe0; border-radius:50%; } .grid i.x { background:#ff7a3d; } .grid i.o { background:#ffd3bd; } .grid i.r { background:#dcd9e6; }
.pats { display:grid; grid-template-columns:repeat(6,1fr); gap:3mm; } .pat { display:flex; flex-direction:column; align-items:center; text-align:center; font-size:8pt; gap:1mm; }
.pat span { color:#6b6a78; }
.cat td { font-size:8pt; padding:1mm 1.4mm; } .cat td.mode .m { display:flex; gap:1.6mm; align-items:flex-start; } tr { page-break-inside:avoid; }
.type { color:#fff; border-radius:2mm; padding:0.3mm 1.4mm; font-size:7.6pt; white-space:nowrap; display:inline-block; margin:0 0.5mm 0.6mm 0; }
.stats { font-size:8pt; margin-top:0.6mm; }
.tag { background:#ff7a3d; color:#fff; border-radius:2mm; padding:0 1.4mm; font-size:7.2pt; } .tag.gold { background:#d9a400; }
.cost { color:#6b4eff; font-weight:600; margin-top:0.4mm; }
.rgrid { display:grid; grid-template-columns:1fr 1fr; gap:3mm; } .rcard { border:1px solid #ddd; border-radius:3mm; padding:2.5mm 3mm; page-break-inside:avoid; }
.rcard h3 { margin:0 0 1mm; } .ex { display:flex; gap:2mm; margin-top:1mm; background:#3a3848; padding:1.5mm; border-radius:2mm; width:max-content; }
.chip { display:inline-block; width:3.2mm; height:3.2mm; border-radius:0.6mm; border:1px solid #0003; vertical-align:middle; margin-right:0.6mm; }
.cls td, .cls th { font-size:8pt; padding:0.8mm 1.2mm; } .en td, .en th { font-size:7.6pt; padding:0.7mm 1.1mm; }
tr.boss td { background:#fbf6ea; }
.boons td { font-size:7.8pt; }
.break { page-break-before:always; }
"""


def main():
    doc = f"""<!doctype html><html lang="pl"><head><meta charset="utf-8"><title>Przewodnik projektowy – Plan Budowlany: Roguelike</title>
<style>{CSS}</style></head><body>
{INTRO}
{sec_hit()}
{sec_rarity()}
{sec_weapons()}
{sec_classes()}
{sec_enemies()}
{sec_stages()}
{sec_perks()}
{sec_artifacts()}
</body></html>"""
    html_path = OUT / "przewodnik_gry.html"
    html_path.write_text(doc, encoding="utf-8")
    pdf = OUT / "przewodnik_gry.pdf"
    subprocess.run([CHROME, "--headless=new", "--disable-gpu", "--no-pdf-header-footer",
                    f"--print-to-pdf={pdf}", html_path.as_uri()], check=True, capture_output=True)
    print("ok", pdf.relative_to(ROOT))


if __name__ == "__main__":
    main()
