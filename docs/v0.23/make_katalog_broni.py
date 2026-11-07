#!/usr/bin/env python3
"""v0.23 – katalog broni (PROPOZYCJA do decyzji właściciela): typy ciosu, wzory pól, dwa tryby, rzadkość.
Składa HTML i drukuje PDF przez Chrome (headless):  python3 docs/v0.23/make_katalog_broni.py
Obecne bronie (nazwa, obrażenia, zasięg, statystyka, żywioł) czyta z GBA/data/game.json; reszta to propozycje."""
import json, pathlib, subprocess, html

ROOT = pathlib.Path(__file__).resolve().parents[2]
OUT = pathlib.Path(__file__).resolve().parent
D = json.load(open(ROOT / "GBA/data/game.json"))
W = {w["id"]: w for w in D["weapons"]}
CLS = {c["weapon"]: c["name"] for c in D["classes"]}
STAT = {"strength": "SIŁ", "agility": "ZRĘ", "intelligence": "INT"}
ELEM = {"prad": "prąd", "iskra": "iskra", "woda": "woda"}

# ---------------------------------------------------------------- wzory pól (siatka 5x5, bohater w środku, patrzy w górę)
# H = bohater, X = trafione pole, o = pole „drugiego planu” (przebicie / odrzut / obszar rzutu)
PAT = {
    "jeden":   ("Pojedynczy", "1 pole przed bohaterem", [".....", ".....", "..X..", "..H..", "....."]),
    "luk":     ("Łuk", "3 pola: przód i oba skosy", [".....", ".....", ".XXX.", "..H..", "....."]),
    "wokol":   ("Wokół", "8 pól dookoła", [".....", ".XXX.", ".XHX.", ".XXX.", "....."]),
    "krzyz":   ("Krzyż", "4 pola: przód, tył, boki", [".....", "..X..", ".XHX.", "..X..", "....."]),
    "linia2":  ("Linia 2", "2 pola w linii, przebija", [".....", "..X..", "..X..", "..H..", "....."]),
    "linia3":  ("Linia 3", "3 pola w linii, przebija", ["..X..", "..X..", "..X..", "..H..", "....."]),
    "strzal":  ("Strzał N", "pierwszy cel w linii do N pól", ["..o..", "..o..", "..X..", "..H..", "....."]),
    "rzut":    ("Rzut 3x3", "obszar 3x3 w zasięgu N", [".XXX.", ".XXX.", ".XXX.", "..H..", "....."]),
    "natrysk": ("Natrysk", "stożek: 1 + 3 pola", [".XXX.", ".....", "..X..", "..H..", "....."]),
    "pchniecie": ("Pchnięcie", "cel + odrzut o 1–2 pola", ["..o..", "..o..", "..X..", "..H..", "....."]),
}

def grid(key, small=False):
    rows = PAT[key][2]
    cells = []
    for r in rows:
        for ch in r:
            cls = {"H": "h", "X": "x", "o": "o"}.get(ch, "e")
            cells.append(f'<i class="{cls}"></i>')
    return f'<div class="grid{" sm" if small else ""}">{"".join(cells)}</div>'

# ---------------------------------------------------------------- katalog: obecne bronie (id z danych) i nowe
# (id/nazwa, typ, źródło, tryb A (wzór, opis), tryb B (wzór, opis, koszt))
OLD = [
    ("dziennik", "Kłucie", ("strzal", "Strzał 2 – notatka służbowa"), ("jeden", "Wpis do dziennika: cel bierze +30% obrażeń od wszystkich przez 3 tury", "odnowienie 4")),
    ("kielnia", "Cięcie", ("luk", "Łuk – zamach kielnią"), ("strzal", "Narzut zaprawy (zasięg 2): cel traci turę", "odnowienie 3")),
    ("gwozdziarka", "Kłucie", ("strzal", "Strzał 3 – gwóźdź"), ("linia3", "Seria: 3 gwoździe w linii po 60%, przebijają", "odnowienie 3")),
    ("probnik", "Kłucie + prąd", ("strzal", "Strzał 2 – impuls"), ("krzyz", "Łańcuch: przeskakuje na mokre cele obok (+combo Porażenie)", "odnowienie 4")),
    ("klucz", "Obuch", ("jeden", "Uderzenie"), ("jeden", "Zakręć zawór: problem wodny traci leczenie i turę", "odnowienie 4")),
    ("szlifierka", "Cięcie + iskra", ("jeden", "Tarcza"), ("luk", "Iskry: łuk 3 pól, w pyle – Wybuch", "odnowienie 3")),
    ("lom", "Obuch", ("jeden", "Uderzenie"), ("pchniecie", "Podważ: odrzut o 2 pola, otwiera pęknięte ściany", "odnowienie 3")),
    ("wkretarka", "Kłucie", ("linia2", "Linia 2 – wiercenie"), ("jeden", "Wkręt: cel −3 OBR na 3 tury", "odnowienie 3")),
    ("laser", "Kłucie", ("linia3", "Linia 4 (słabsza), przebija"), ("linia3", "Przód i tył: linia 4 w obie strony", "odnowienie 4")),
    ("mlot", "Obuch", ("pchniecie", "Ciężki cios z odrzutem 1"), ("wokol", "Wyburzenie: 8 pól, kruszy pęknięte ściany", "odnowienie 5")),
    ("tablet", "Obuch", ("jeden", "Uderzenie"), ("jeden", "Zmiana projektu (zasięg 3): zamiana miejscami z celem", "odnowienie 4")),
    ("miernik", "Kłucie", ("strzal", "Strzał 3 – wiązka"), ("jeden", "Pomiar: odsłania pokój, +10% kryt na 3 tury", "odnowienie 4")),
    ("dachowka", "Obuch (rzut)", ("strzal", "Rzut 3 – dachówka"), ("rzut", "Grad dachówek: 3x3 w zasięgu 3 po 50%", "odnowienie 4")),
    ("agregat", "Obuch (natrysk)", ("natrysk", "Natrysk – tynk"), ("jeden", "Zatynkuj: cel unieruchomiony na 2 tury", "odnowienie 4")),
    ("lyzka", "Obuch", ("luk", "Łuk – zamach łyżką"), ("strzal", "Wykop: przyciąga cel o 2 pola do siebie", "odnowienie 3")),
    ("udarowy", "Obuch + kłucie", ("jeden", "Udar"), ("linia2", "Kucie: linia 2, ignoruje OBR", "odnowienie 4")),
    ("kotwy", "Kłucie + iskra", ("strzal", "Strzał 3 – kotwa"), ("jeden", "Przybij: cel nie rusza się 2 tury", "odnowienie 3")),
    ("spawarka", "Cięcie + iskra", ("linia2", "Linia 2 – łuk spawalniczy"), ("jeden", "Spaw: 2 obrażeń przez 3 tury (stal: +50%)", "odnowienie 3")),
    ("tyczka", "Kłucie", ("linia2", "Sztych – linia 2"), ("krzyz", "Wbij tyczkę: wrogowie w krzyżu wokół niej +25% obrażeń", "odnowienie 4")),
    ("mlotek", "Obuch", ("jeden", "Uderzenie"), ("jeden", "Pazur (cięcie): −2 OBR celu i +kryt", "zmiana trybu")),
    ("mlot_zenka", "Obuch", ("pchniecie", "Cios z odrzutem"), ("wokol", "Zamach Zenka: 8 pól z odrzutem", "odnowienie 5")),
    ("poziomica_m", "Kłucie", ("strzal", "Strzał 3, kryt +10%"), ("linia3", "Pion i poziom: linia 3 w 2 strony, odsłania", "odnowienie 4")),
]
NEW = [
    ("Łopata", "Obuch / Kłucie", "drop, Hurtownia", "SIŁ", "1", ("luk", "Płazem: łuk 3 pól, 20% ogłuszenia"), ("linia2", "Sztych: linia 2, kłucie, −2 OBR", "zmiana trybu")),
    ("Siekiera ciesielska", "Cięcie", "Szkolenia (Cieśla)", "SIŁ", "1", ("jeden", "Rąbanie: drewno +50%"), ("luk", "Zamach: łuk 3 pól", "odnowienie 3")),
    ("Piła ręczna", "Cięcie", "drop", "ZRĘ", "1", ("jeden", "Cięcie, +10% kryt"), ("jeden", "Piłowanie: 2 obrażeń przez 3 tury", "odnowienie 3")),
    ("Kilof", "Kłucie", "drop (Akt I)", "SIŁ", "1", ("jeden", "Kucie: ignoruje 2 OBR, cement +50%"), ("linia2", "Przebicie: linia 2", "odnowienie 4")),
    ("Przecinak z młotkiem", "Kłucie", "Hurtownia (3 ładunki)", "SIŁ", "1", ("jeden", "Kucie"), ("jeden", "Przebij ścianę: otwiera pęknięte ściany i magazyny, zużywa ładunek", "ładunek")),
    ("Nożyce do blachy", "Cięcie", "drop (Akt II)", "ZRĘ", "1", ("jeden", "Cięcie – stal bez kary"), ("luk", "Rozcięcie: łuk 3 pól", "odnowienie 3")),
    ("Pistolet do silikonu", "Kłucie", "Hurtownia", "ZRĘ", "3", ("strzal", "Strzał 3 – słaby"), ("jeden", "Uszczelnij: cel unieruchomiony 2 tury, przeciek – zatkany", "odnowienie 4")),
    ("Myjka ciśnieniowa", "Obuch + woda", "drop", "ZRĘ", "3", ("linia3", "Strumień: linia 3, moczy (Mokry)"), ("pchniecie", "Odrzut wodą o 2 pola", "odnowienie 3")),
    ("Palnik dekarski", "Ogień", "Szkolenia (Dekarz)", "INT", "2", ("natrysk", "Płomień: stożek, podpala (2 tury)"), ("wokol", "Opalanie: 8 pól po 50%, osusza", "odnowienie 5")),
    ("Taker", "Kłucie", "drop", "ZRĘ", "2", ("strzal", "Dwa zszywki naraz po 60%"), ("jeden", "Przypnij: cel nie ucieka 3 tury", "odnowienie 3")),
    ("Młotek gumowy", "Obuch", "Hurtownia", "SIŁ", "1", ("jeden", "Klepnięcie: słabe, 50% ogłuszenia"), ("krzyz", "Wyrównanie: krzyż, 30% ogłuszenia", "odnowienie 4")),
    ("Pilarka tarczowa", "Cięcie + iskra", "sekret / Mistrzowska", "ZRĘ", "1", ("luk", "Łuk 3 pól, kryt +10%"), ("wokol", "Obrót: 8 pól, iskry", "odnowienie 5")),
    ("Poziomica 2 m", "Obuch", "drop", "INT", "2", ("linia2", "Płaski cios: linia 2, nie przebija"), ("krzyz", "Sprawdź pion: krzyż, odsłania pokój", "odnowienie 4")),
    ("Laser krzyżowy", "Kłucie", "Szkolenia (Geodeta)", "INT", "4", ("linia3", "Linia 4 (słabsza), przebija"), ("krzyz", "Krzyż: linie w 4 strony", "odnowienie 4")),
    ("Lutownica", "Kłucie + ogień", "drop, Hurtownia", "INT", "1", ("jeden", "Grot: podpala (2 tury)"), ("linia2", "Lut: linia 2, osusza i podpala", "odnowienie 3")),
    ("Opalarka", "Ogień", "drop", "ZRĘ", "2", ("natrysk", "Gorące powietrze: stożek, osusza Mokry"), ("jeden", "Podgrzej: drewno +100% na 1 cios", "odnowienie 3")),
    ("Mieszadło do zaprawy", "Obuch", "Szkolenia (Murarz)", "SIŁ", "1", ("jeden", "Uderzenie"), ("wokol", "Wir: 8 pól, cele odepchnięte o 1", "odnowienie 5")),
]

TYPES = [
    ("Obuch", "#6b4eff", "młoty, klucze, łopata płazem",
     "20% szans na ogłuszenie (cel traci turę); ciężkie bronie odrzucają o 1 pole",
     "cement +50%, stal −25%, woda −50%"),
    ("Cięcie", "#e0584d", "kielnia, piły, siekiera, nożyce",
     "+10% szans na kryt; „krwawienie” w trybie B (obrażenia przez kilka tur)",
     "drewno +50%, cement −25%, stal −25% (nożyce i iskra bez kary)"),
    ("Kłucie", "#0f8a6a", "gwoździe, kotwy, tyczka, kilof",
     "ignoruje 2 OBR; w linii przebija do kolejnego celu",
     "stal +25%, bossowie z wysokim OBR – najlepszy wybór"),
]
MATS = ["stal", "drewno", "cement", "żywioł woda", "bez materiału (bossowie)"]
EFF = {"Obuch": ["75%", "100%", "150%", "50%", "100%"],
       "Cięcie": ["75%", "150%", "75%", "100%", "100%"],
       "Kłucie": ["125%", "100%", "100%", "100%", "100% + −2 OBR"],
       "Iskra (żywioł)": ["150%", "100%", "100%", "100%", "100%"],
       "Prąd (żywioł)": ["100%", "100%", "100%", "150% + Porażenie", "100%"],
       "Ogień (żywioł)": ["100%", "150%", "100%", "50% + osusza", "100% + podpalenie"]}
RAR = [("Zwykła", "#8a8796", "bazowe obrażenia, wygląd jak dziś"),
       ("Solidna", "#5cb4f0", "+1 obrażeń albo 1 cecha (jak sprzęt Solidny), niebieski połysk w ręce"),
       ("Markowa", "#6b4eff", "+2 obrażeń i cecha, fioletowy połysk, lepszy tryb B (−1 tura odnowienia)"),
       ("Mistrzowska", "#d9a400", "unikat z własną nazwą i efektem (np. Młot Zenka, Poziomica mistrza), złota poświata")]
COUNT = {k: 0 for k in ["Obuch", "Cięcie", "Kłucie"]}

def esc(s): return html.escape(s)

def row_old(e):
    wid, typ, a, b = e
    w = W[wid]
    src = f"zawód: {CLS[wid]}" if wid in CLS else ("Mistrzowska" if wid in ("mlot_zenka", "poziomica_m") else "Szkolenia / drop")
    el = ELEM.get(w.get("element", ""), "")
    stats = f'{w["minDamage"]}–{w["maxDamage"]}, zasięg {w["range"]}, {STAT[w["scalesWith"]]}' + (f", {el}" if el else "")
    return row(w["name"], typ, src, stats, a, b)

def row_new(e):
    name, typ, src, st, rng, a, b = e
    return row(name, typ, src, f"zasięg {rng}, {st}", a, b, new=True)

def row(name, typ, src, stats, a, b, new=False):
    base = typ.split()[0].rstrip("/")
    for k in COUNT:
        if k in typ: COUNT[k] += 1
    col = {"Obuch": "#6b4eff", "Cięcie": "#e0584d", "Kłucie": "#0f8a6a", "Iskra": "#d9a400", "Ogień": "#e8772e"}.get(base, "#555")
    return f"""<tr class="{'new' if new else ''}">
<td><b>{esc(name)}</b>{' <span class="tag">nowa</span>' if new else ''}<div class="dim">{esc(src)}</div><div class="dim">{esc(stats)}</div></td>
<td><span class="type" style="background:{col}">{esc(typ)}</span></td>
<td class="mode"><div class="m">{grid(a[0], True)}<div><b>A</b> · {esc(PAT[a[0]][0])}<br>{esc(a[1])}</div></div></td>
<td class="mode"><div class="m">{grid(b[0], True)}<div><b>B</b> · {esc(PAT[b[0]][0])}<br>{esc(b[1])}<div class="cost">{esc(b[2])}</div></div></div></td>
</tr>"""

rows_old = "\n".join(row_old(e) for e in OLD)
rows_new = "\n".join(row_new(e) for e in NEW)
pats = "".join(f'<div class="pat">{grid(k)}<b>{esc(v[0])}</b><span>{esc(v[1])}</span></div>' for k, v in PAT.items())
types = "".join(f'<div class="tcard" style="border-top:6px solid {c}"><h3 style="color:{c}">{esc(n)}</h3><p class="dim">{esc(ex)}</p><p>{esc(ef)}</p><p><b>Materiały:</b> {esc(m)}</p></div>' for n, c, ex, ef, m in TYPES)
eff = "<tr><th></th>" + "".join(f"<th>{esc(m)}</th>" for m in MATS) + "</tr>" + "".join(
    f"<tr><th>{esc(k)}</th>" + "".join(f'<td class="{"hi" if v.startswith(("150", "125")) else "lo" if v.startswith(("75", "50")) else ""}">{esc(v)}</td>' for v in vals) + "</tr>" for k, vals in EFF.items())
rar = "".join(f'<div class="rar"><div class="sw" style="background:{c}"></div><b>{esc(n)}</b><span>{esc(t)}</span></div>' for n, c, t in RAR)
mats = {m: sum(1 for e in D["enemies"] if e.get("material") == m) for m in ("stal", "drewno", "cement")}
nomat = sum(1 for e in D["enemies"] if not e.get("material"))

doc = f"""<!doctype html><html lang="pl"><head><meta charset="utf-8"><title>Katalog broni v0.23 – propozycja</title>
<style>
@page {{ size: A4; margin: 14mm 12mm; }}
body {{ font-family: 'Avenir Next', 'Helvetica Neue', Arial, sans-serif; color:#14122a; font-size:10.5pt; }}
h1 {{ color:#6b4eff; margin:0 0 2mm; font-size:24pt; }} h2 {{ color:#6b4eff; border-bottom:2px solid #ff7a3d; padding-bottom:1mm; margin-top:7mm; }}
h3 {{ margin:0 0 1mm; }} .dim {{ color:#6b6a78; font-size:9pt; }} p {{ margin:1.2mm 0; }}
.lead {{ background:#f6f5f8; border-left:5px solid #6b4eff; padding:3mm 4mm; border-radius:2mm; }}
.types {{ display:grid; grid-template-columns:repeat(3,1fr); gap:4mm; }} .tcard {{ background:#fff; border:1px solid #ddd; border-radius:3mm; padding:3mm; }}
table {{ border-collapse:collapse; width:100%; }} th, td {{ border:1px solid #e2e0ea; padding:1.6mm 2mm; vertical-align:top; text-align:left; }}
th {{ background:#f6f5f8; }} td.hi {{ background:#e3f6ee; font-weight:600; }} td.lo {{ background:#fde9e7; }}
.grid {{ display:grid; grid-template-columns:repeat(5,9px); grid-gap:1px; width:49px; }} .grid i {{ width:9px; height:9px; display:block; background:#eceaf2; }}
.grid.sm {{ grid-template-columns:repeat(5,7px); width:39px; }} .grid.sm i {{ width:7px; height:7px; }}
.grid i.h {{ background:#2f6fe0; border-radius:50%; }} .grid i.x {{ background:#ff7a3d; }} .grid i.o {{ background:#ffd3bd; }}
.pats {{ display:grid; grid-template-columns:repeat(5,1fr); gap:3mm; }} .pat {{ display:flex; flex-direction:column; align-items:center; text-align:center; font-size:8.5pt; gap:1mm; }}
.pat span {{ color:#6b6a78; }}
.cat td {{ font-size:8.6pt; }} .cat td.mode .m {{ display:flex; gap:2mm; align-items:flex-start; }} .cat tr {{ page-break-inside:avoid; }}
.type {{ color:#fff; border-radius:2mm; padding:0.5mm 1.6mm; font-size:8pt; white-space:nowrap; }}
.tag {{ background:#ff7a3d; color:#fff; border-radius:2mm; padding:0 1.4mm; font-size:7.5pt; }} .cost {{ color:#6b4eff; font-weight:600; margin-top:0.6mm; }}
tr.new td:first-child {{ border-left:3px solid #ff7a3d; }}
.rars {{ display:grid; grid-template-columns:1fr 1fr; gap:3mm; }} .rar {{ display:grid; grid-template-columns:10mm auto; grid-template-rows:auto auto; column-gap:2mm; }}
.rar .sw {{ grid-row:span 2; width:10mm; height:10mm; border-radius:2mm; }} .rar span {{ font-size:9pt; color:#4a4958; }}
.q li {{ margin:1.6mm 0; }} .break {{ page-break-before:always; }}
</style></head><body>
<h1>Katalog broni – propozycja v0.23</h1>
<p class="dim">PlanBudowlany Roguelike · dokument roboczy do decyzji · dane obecnych broni z <code>GBA/data/game.json</code> (v{esc(D.get('version','').lstrip('v'))})</p>
<div class="lead">Dziś każda broń różni się głównie liczbami (obrażenia, zasięg, statystyka). Propozycja: <b>3 typy ciosu</b>
(obuch, cięcie, kłucie) związane z <b>materiałem problemu</b>, który gra już zna (stal {mats['stal']}, drewno {mats['drewno']},
cement {mats['cement']}, bez materiału {nomat} – głównie bossowie), <b>2 tryby ataku</b> na broń z różnym wzorem pól i
<b>rzadkość</b> widoczną w rękach postaci. Istniejące żywioły (prąd, iskra, woda) i kombinacje stanów zostają i dokładają się do typu;
nowy żywioł <b>ogień</b> (lutownica, opalarka, palnik) – drewno +50%, osusza Mokry, podpala na 2 tury.</div>

<h2>1. Typy ciosu</h2>
<div class="types">{types}</div>

<h2>2. Skuteczność wg materiału problemu</h2>
<table>{eff}</table>
<p class="dim">Mnożnik obrażeń po rzucie i premiach. Kłucie dodatkowo zdejmuje 2 OBR. Wartości do strojenia botem (cel: balans jak dziś przy broni domyślnej).</p>

<h2>3. Wzory pól</h2>
<div class="pats">{pats}</div>
<p class="dim">Niebieskie kółko – bohater (patrzy w górę), pomarańczowe – trafione pola, jasne – dalszy tor (przebicie, odrzut, lot).</p>

<h2>4. Dwa tryby i rzadkość</h2>
<p><b>Tryb A</b> – zwykły atak (jak dziś, bez kosztu). <b>Tryb B</b> – specjalny: inny wzór pól albo efekt; dwie możliwości do wyboru:
<b>odnowienie</b> (np. 3–5 tur, jak moc zawodu) albo <b>przełączanie trybu</b> (darmowe, ale zmiana kosztuje turę). W katalogu przy każdym trybie B jest koszt w tej pierwszej wersji.</p>
<div class="rars">{rar}</div>
<p class="dim">Rzadkości jak przy sprzęcie (Zwykły / Solidny / Markowy) + Mistrzowska dla unikatów. Wygląd: osobny mały sprite broni w ręce
(4 kierunki, najlepiej widoczny przy patrzeniu w dół), przyciemniony / podbarwiony kolorem rzadkości, Markowa i Mistrzowska z poświatą.
Ten sam kolor w dropach, Hurtowni i na karcie zawodu.</p>

<h2 class="break">5. Obecne bronie ({len(OLD)}) – propozycja typu i trybów</h2>
<table class="cat"><tr><th style="width:27%">Broń</th><th style="width:13%">Typ</th><th style="width:30%">Tryb A</th><th style="width:30%">Tryb B</th></tr>{rows_old}</table>

<h2 class="break">6. Nowe bronie ({len(NEW)}) – pomysły</h2>
<table class="cat"><tr><th style="width:27%">Broń</th><th style="width:13%">Typ</th><th style="width:30%">Tryb A</th><th style="width:30%">Tryb B</th></tr>{rows_new}</table>
<p class="dim">Źródła: zawód (broń domyślna), Szkolenia (odblokowanie za doświadczenie), drop na budowie (z rzadkością), Hurtownia (za zł),
sekret. Przecinak z młotkiem to jednorazowy przedmiot z pomysłu „przecinak/młot do pękniętych ścian” (3 ładunki).</p>

<h2>7. Do decyzji</h2>
<ol class="q">
<li><b>Tryb B:</b> odnowienie (jak moc) czy przełączanie trybu kosztem tury?</li>
<li><b>Ile broni naraz:</b> jedna (zamiana przy znalezieniu, jak dziś narzędzie) czy dwie (główna + zapasowa pod przyciskiem)?</li>
<li><b>Siła różnic:</b> ±50% jak w tabeli czy łagodniej (±25%), żeby broń domyślna nigdy nie była „zła” na danym etapie?</li>
<li><b>Skąd rzadkość:</b> tylko z dropów na budowie, czy także w Szkoleniach / Hurtowni?</li>
<li><b>Które nowe bronie</b> z listy w pierwszej kolejności (propozycja: Łopata, Kilof, Przecinak, Myjka, Młotek gumowy – każda pokazuje inny typ)?</li>
<li><b>GBA:</b> pełny system, uproszczony (typy i rzadkość bez trybu B) czy tylko Godot?</li>
</ol>
<p class="dim">Podsumowanie typów w katalogu: obuch {COUNT['Obuch']}, cięcie {COUNT['Cięcie']}, kłucie {COUNT['Kłucie']} (broń z dwoma typami liczona dwa razy).</p>
</body></html>"""

(OUT / "katalog_broni.html").write_text(doc, encoding="utf-8")
chrome = "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome"
subprocess.run([chrome, "--headless=new", "--disable-gpu", "--no-pdf-header-footer",
                f"--print-to-pdf={OUT / 'katalog_broni.pdf'}", (OUT / "katalog_broni.html").as_uri()],
               check=True, capture_output=True)
print("ok", OUT / "katalog_broni.pdf", COUNT)
