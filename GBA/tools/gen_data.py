#!/usr/bin/env python3
"""data/game.json -> include/game_data.h (constexpr tablice dla GBA i testów na PC).
--check: sprawdza, czy nagłówek jest aktualny (CI)."""
import json, os, sys
ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
d = json.load(open(os.path.join(ROOT, "data", "game.json"), encoding="utf-8"))
STAT = {"strength": "stat::str", "agility": "stat::agi", "intelligence": "stat::intel"}
wid = {w["id"]: i for i, w in enumerate(d["weapons"])}
eid = {e["id"]: i for i, e in enumerate(d["enemies"])}
mid = {x["id"]: i for i, x in enumerate(d["materials"]["list"])}
ELEM = {"none": "none", "woda": "water", "prad": "power", "iskra": "spark"}   # v0.21.50: żywioły (kombinacje stanów)
GEN = {"m": 0, "f": 1, "n": 2}
def s(x): return '"' + x.replace('"', '\\"') + '"'
L = ["// WYGENEROWANE przez tools/gen_data.py z data/game.json - nie edytuj ręcznie.",
     "#pragma once", '#include "core_types.h"', "", "namespace data {", ""]
L.append("inline constexpr core::weapon_def weapons[] = {")
for w in d["weapons"]:
    assert w["minDamage"] <= w["maxDamage"] and w["range"] >= 1, w
    assert w.get("element", "none") in ELEM, w
    assert 0 <= w.get("crit", 0) <= 50, w   # v0.21.51 cz. 2: kryt broni, odpychanie, magazyn na mapie (sekretne narzędzia)
    L.append(f'    {{ {s(w["name"])}, {w["minDamage"]}, {w["maxDamage"]}, {w["range"]}, core::{STAT[w["scalesWith"]]}, '
             f'core::element::{ELEM[w.get("element", "none")]}, {w.get("crit", 0)}, {"true" if w.get("knockback") else "false"}, '
             f'{"true" if w.get("reveal") else "false"} }},')
L.append("};\n")
L.append("inline constexpr core::class_def classes[] = {")
for c in d["classes"]:
    ab = c["ability"]
    assert len(ab["desc"]) <= 23, ab   # mieści się w banerze obok ikony
    assert ab["effect"] in {"stun", "wall", "volley", "chain", "flush", "spin", "line", "splash", "ram", "weld", "mark", "borrow"}, ab
    assert c.get("passive", "none") in {"none", "windproof", "push", "surveyor"}, c
    assert ab["effect"] != "borrow" or c.get("secret"), c   # Złota rączka pożycza moc zwykłych zawodów
    L.append(f'    {{ {s(c["name"])}, {s(c["desc"])}, {c["maxHealth"]}, {c["strength"]}, {c["agility"]}, '
             f'{c["intelligence"]}, {c["defense"]}, {c["luck"]}, {wid[c["weapon"]]}, {c["frame"]}, '
             f'{s(ab["name"])}, {s(ab["desc"])}, core::ability_effect::{ab["effect"]}, {ab["cooldown"]}, '
             f'core::class_passive::{c.get("passive", "none")} }},')
L.append("};\n")
BEH = ["ranged", "splits", "heals", "explodes", "grows", "flees", "stationary", "pushes", "returns"]
def tags(e):
    b = e.get("behaviors", [])
    assert all(x in BEH for x in b) and not (e.get("slam") and b), e   # bossowie bez zachowań
    assert not ("stationary" in b and "flees" in b), e
    return sum(1 << BEH.index(x) for x in b)
L.append("inline constexpr core::enemy_def enemies[] = {")
for e in d["enemies"]:
    sm, rw = e.get("summon", {}), e.get("reward", {})
    assert e.get("slamShape", "square") in {"square", "cross"} and len(e.get("slamName", "")) <= 16, e
    assert not sm or (sm["enemy"] in eid and not d["enemies"][eid[sm["enemy"]]].get("slam") and sm["every"] > 0 and 0 < sm["max"] <= 3), e
    assert len(rw.get("title", "")) <= 24 and 0 <= rw.get("cash", 0) <= 500, e
    ph = e.get("phase", {})   # druga faza bossa (np. Odwołanie): raz przy atPct% HP leczy healPct% i wzywa
    assert not ph or (e.get("slam") and 10 <= ph["atPct"] <= 90 and 0 <= ph["healPct"] <= 60 and len(ph["name"]) <= 16
                      and 0 <= ph.get("summon", 0) <= sm.get("max", 0)), e
    L.append(f'    {{ {s(e["name"])}, {s(e["desc"])}, {e["maxHealth"]}, {e["minDamage"]}, {e["maxDamage"]}, {e["defense"]}, '
             f'{e["sight"]}, {e["score"]}, {e["frame"]}, {"true" if e.get("slam") else "false"}, '
             f'core::status_effect::{e.get("onHit", {}).get("status", "none")}, {e.get("onHit", {}).get("chancePct", 0)}, '
             f'{e.get("onHit", {}).get("turns", 0)}, core::slam_shape::{e.get("slamShape", "square")}, {s(e.get("slamName", ""))}, '
             f'{eid[sm["enemy"]] if sm else -1}, {sm.get("every", 0)}, {sm.get("max", 0)}, {e.get("gearStun", 0)}, '
             f'{rw.get("cash", 0)}, {s(rw.get("title", ""))}, {mid[e["material"]] if "material" in e else -1}, {tags(e)}, '
             f'{ph.get("atPct", 0)}, {ph.get("healPct", 0)}, {ph.get("summon", 0)}, {s(ph.get("name", ""))}, '
             f'core::element::{ELEM[e.get("element", "none")]}, {GEN[e.get("gender", "m")]} }},')
L.append("};\n")
# v0.21.52 cz. d (#47): mapa kariery - kontrakt 0 (Dom jednorodzinny) = etapy "stages", kolejne kontrakty mają własne
# etapy (career.list[].stages); w data::stages wszystkie po kolei (Dom pierwszy), kontrakt = pierwszy etap + liczba.
# Etap: wygląd (paleta etapu, "look"), zestaw kafli ("tiles": domyślnie akt, Akt 0 - 3 + etap), druga połowa bliźniaka ("twin").
car = d["career"]["list"]
assert 2 <= len(car) <= 6 and "stages" not in car[0] and all("stages" in c for c in car[1:])   # profil: 6 kontraktów
dom_n = len(d["stages"])
all_st = list(d["stages"]) + [x for c in car[1:] for x in c["stages"]]
assert len(all_st) <= 64   # pogoda: bitmaska uint64 etapów
pre_n = sum(1 for st in d["stages"] if d["acts"][st["act"]].get("prelude"))
def st_tiles(i, st):
    if "tiles" in st: return st["tiles"]
    return 3 + i if i < pre_n else st["act"]
TILE_SETS = 7   # 0-2 akty, 3-4 Akt 0 (biuro, wykop), 5 drewno (domek letniskowy), 6 kamienica
looks_n = max(dom_n, 1 + max(st.get("look", 0) for st in all_st))
L.append("inline constexpr core::stage_def stages[] = {   // Dom jednorodzinny, potem etapy kolejnych kontraktów (career)")
for i, st in enumerate(all_st):
    pool = [eid[x] for x in st["enemies"]] + [-1] * (4 - len(st["enemies"]))
    boss = eid[st["boss"]] if "boss" in st else -1
    look = i if i < dom_n else st["look"]
    assert (i < dom_n) == ("look" not in st) and 0 <= look < looks_n and 0 <= st_tiles(i, st) < TILE_SETS, st
    assert 1 <= len(st["name"]) <= 20 and len(st["enemies"]) <= 4 and (i >= dom_n or not st.get("twin")), st
    if boss >= 0: assert d["enemies"][boss].get("slam"), st
    L.append(f'    {{ {s(st["name"])}, {{ {", ".join(map(str, pool))} }}, {len(st["enemies"])}, {st["count"]}, {boss}, '
             f'{st.get("hpPct", 100)}, {st.get("dmgBonus", 0)}, {st["act"]}, {st.get("cost", 0)}, {look}, {st_tiles(i, st)}, '
             f'{"true" if st.get("twin") else "false"} }},')
L.append("};\n")
L.append("inline constexpr core::difficulty_def difficulties[] = {")
for df in d["difficulties"]:
    L.append(f'    {{ {s(df["name"])}, {df["hpPct"]}, {df["dmgBonus"]}, {df["scorePct"]} }},')
L.append("};\n")
m = d["meta"]
cid = {c["id"]: i for i, c in enumerate(d["classes"])}
EFF = {"hp", "def", "dmg", "coffee", "pickups", "luck", "craft", "dmg_pct", "taken_pct",
       "crit", "dodge", "thermos", "mats_pct", "gear_pct", "cash"}   # v0.21.52: poziomy Szkoleń z różnym działaniem
TREE_EFF = EFF | {"shop_pct", "brigade_pct", "cooldown", "first_hit"}   # v0.21.52 cz. c: węzły drzewka Szkoleń
L.append("inline constexpr core::upgrade_def upgrades[] = {   // Szkolenia: poziomy (przyrost + koszt), stare koszty do zwrotu")
for u in m["upgrades"]:
    st_ = u["steps"]
    assert u["effect"] in EFF and 1 <= len(st_) <= 5 and all(x["effect"] in EFF for x in st_), u
    assert any(x["effect"] == u["effect"] for x in st_), u   # główne działanie jest wśród poziomów
    assert all(0 < x["value"] < 100 and 0 < x["cost"] < 1000 for x in st_), u
    assert all(st_[i]["cost"] < st_[i + 1]["cost"] for i in range(len(st_) - 1)), u   # cena rośnie z poziomem
    assert 1 <= len(u["legacyCosts"]) <= 4 and len(u["name"]) <= 15, u
    steps = [f'{{ core::upgrade_effect::{x["effect"]}, {x["value"]} }}' for x in st_] + ["{}"] * (5 - len(st_))
    costs = [x["cost"] for x in st_] + [0] * (5 - len(st_))
    legacy = u["legacyCosts"] + [0] * (4 - len(u["legacyCosts"]))
    L.append(f'    {{ {s(u["name"])}, {s(u["desc"])}, core::upgrade_effect::{u["effect"]}, {len(st_)}, {{ {", ".join(steps)} }}, '
             f'{{ {", ".join(map(str, costs))} }}, {{ {", ".join(map(str, legacy))} }}, {len(u["legacyCosts"])}, {u.get("refund", 0)} }},')
L.append("};\n")
def story(m):
    lines = m["text"].split("|")
    assert len(lines) <= 3 and all(len(l) <= 25 for l in lines), m   # dymek: 3 linie x 25 znaków
    lines += [""] * (3 - len(lines))
    return f'{{ {s(m["from"])}, {{ {", ".join(s(l) for l in lines)} }} }}'
st = d["story"]
assert len(st["stages"]) == len(d["stages"])
L.append("inline constexpr core::story_msg story_stages[] = {   // SMS na starcie etapu (indeks jak data::stages)")
L += [f"    {story(m)}," for m in st["stages"] + [x["story"] for c in car[1:] for x in c["stages"]]]
L.append("};")
L += [f"inline constexpr core::story_msg story_{k} = {story(st[k])};" for k in ("win", "lose", "ngplus", "prologue")]
L.append("inline constexpr const char* prologue_captions[] = { " + ", ".join(s(c) for c in st["prologueCaptions"]) + " };")
L.append("")
acts = d["acts"]
path_more = max(0, max(x["enemies"] for x in d["paths"]["list"]))   # ścieżka może dodać problemy
twin_max = d["career"]["twinCarryMax"]
assert 0 <= twin_max <= 3
for st in all_st:   # boss z wezwaniami: etap + boss + wezwani mieszczą się w core::max_enemies (12)
    more = path_more + (twin_max if st.get("twin") else 0)   # bliźniak: problemy z pierwszej połowy przechodzą
    assert st["count"] + more <= 12, st   # core::max_enemies 16: miejsce na dwa podziały (zachowanie "splits")
    if "boss" in st:
        assert st["count"] + more + 1 + d["enemies"][eid[st["boss"]]].get("summon", {}).get("max", 0) <= 12, st
routes = [d["stages"]] + [c["stages"] for c in car[1:]]
for rt in routes:   # w każdym kontrakcie akty po kolei, każdy kończy się etapem z bossem, ostatni etap z bossem
    assert len(rt) <= 12   # core::max_stages: tablice podsumowania w stanie budowy
    seen = []
    for st in rt:
        if not seen or seen[-1] != st["act"]: assert st["act"] not in seen, rt; seen.append(st["act"])
    for ai in seen:
        last = max(i for i, st in enumerate(rt) if st["act"] == ai)
        assert "boss" in rt[last], f"akt {ai} bez bossa"
    assert all(not acts[st["act"]].get("prelude") for st in rt) or rt is d["stages"]   # Akt 0 tylko w Domu
    assert not rt[0].get("twin") and all(not x.get("twin") or not rt[i].get("twin") for i, x in enumerate(rt[1:]))   # bliźniak po pierwszej połowie
# Akt wstępny (Akt 0, "prelude"): jego etapy są na początku listy, bez nagrody za odbiór budowa zaczyna się za nimi.
prelude_stages = sum(1 for st in d["stages"] if acts[st["act"]].get("prelude"))
assert all(acts[st["act"]].get("prelude") for st in d["stages"][:prelude_stages]) and prelude_stages < len(d["stages"])
assert not acts[d["stages"][prelude_stages]["act"]].get("prelude")
docs = d.get("documents", [])
assert len(docs) <= 4 and all(len(x) <= 12 for x in docs)
L.append("inline constexpr core::act_def acts[] = {   // mechanika aktu: błoto, porywy wiatru, pył")
for a in acts:
    mc = a.get("mechanic", {"effect": "none", "value": 0, "name": "", "short": "", "info": ""})
    assert mc["effect"] in {"none", "mud", "gust", "dust", "stamps"} and len(mc["name"]) <= 20 and len(mc["short"]) <= 9 and len(mc["info"]) <= 23, mc   # info = baner (23 znaki)
    assert mc["effect"] != "stamps" or mc["value"] == len(docs), mc   # pieczątki: tyle dokumentów otwiera schody
    assert len(a.get("numeral", "")) <= 3, a
    assert mc["effect"] not in ("mud", "gust") or mc["value"] >= 3, mc
    L.append(f'    {{ {s(a["name"])}, {a["bonusPerStage"]}, {a["bonusPerKill"]}, core::act_mechanic::{mc["effect"]}, {mc["value"]}, '
             f'{s(mc["name"])}, {s(mc["short"])}, {s(mc["info"])}, {s(a.get("numeral", ""))}, {"true" if a.get("prelude") else "false"} }},')
L.append("};")
L += [f"inline constexpr int prelude_stages = {prelude_stages};   // etapy aktu wstępnego (Akt 0) - z nagrody za odbiór",
      "inline constexpr const char* documents[] = { " + ", ".join(s(x) for x in docs or [""]) + " };   // pieczątki: dokumenty etapu",
      f"inline constexpr int documents_count = {len(docs)};"]
bp = d["behaviorParams"]
assert 2 <= bp["rangedReach"] <= 4 and 0 < bp["splitHpPct"] <= 100 and bp["blastDelay"] >= 2 and bp["growEvery"] >= 2
L += [f"inline constexpr int behavior_{k} = {v};" for k, v in [("ranged_reach", bp["rangedReach"]), ("split_hp_pct", bp["splitHpPct"]),
      ("heal_value", bp["healValue"]), ("heal_every", bp["healEvery"]), ("blast_damage", bp["blastDamage"]),
      ("blast_radius", bp["blastRadius"]), ("blast_delay", bp["blastDelay"]), ("grow_every", bp["growEvery"]),
      ("grow_hp", bp["growHp"]), ("grow_max", bp["growMax"]), ("flee_cooldown", bp["fleeCooldown"]),
      ("return_turns", bp["returnTurns"]), ("return_hp_pct", bp["returnHpPct"]), ("push_cooldown", bp["pushCooldown"])]]
bn = d["behaviorNames"]
assert set(bn) == set(BEH) and all(len(v) <= 18 for v in bn.values())
L.append("inline constexpr const char* behavior_names[] = { " + ", ".join(s(bn[k]) for k in BEH) + " };   // indeks = bit zachowania")
L.append("inline constexpr core::shop_item_def hurtownia[] = {")
for it in d["hurtownia"]:
    assert it["effect"] in {"heal", "gear", "tool", "maxhp", "ability", "def", "thermos", "upgrade"} and len(it["desc"]) <= 34, it
    assert it["effect"] == "upgrade" or ("material" in it) == (it["price"] == 0), it   # zł albo materiał (ulepszenie: toolUpgrade)
    assert 0 < it.get("matCost", 1) <= 9 and (it["effect"] != "upgrade" or (it["price"] == 0 and "material" not in it)), it
    L.append(f'    {{ {s(it["name"])}, {s(it["desc"])}, {it["price"]}, core::shop_effect::{it["effect"]}, '
             f'{mid[it["material"]] if "material" in it else -1}, {it.get("matCost", 0)} }},')
L.append("};")
stt = d["statuses"]
L.append("inline constexpr core::status_def statuses[] = {   // indeks = core::status_effect")
L.append('    { "", "", "" },')
for k in ("poison", "shock", "slip", "paper", "wet"):
    x = stt[k]
    assert len(x["short"]) <= 8 and len(x["name"]) + len(x["effect"]) <= 30, x
    L.append(f'    {{ {s(x["name"])}, {s(x["short"])}, {s(x["effect"])} }},')
L.append("};")
L.append(f"inline constexpr int paper_delay = {stt['paper']['delay']};")
sl = d["slam"]
L += [f"inline constexpr int acts_count = {len(acts)};",
      f"inline constexpr int hurtownia_count = {len(d['hurtownia'])};",
      f"inline constexpr int slam_every = {sl['every']};",
      f"inline constexpr int slam_damage_bonus = {sl['damageBonus']};",
      f"inline constexpr int slam_radius = {sl['radius']};",
      f"inline constexpr int slam_delay = {sl['delay']};",
      f"inline constexpr int slam_cross_reach = {sl['crossReach']};",
      f"inline constexpr int slam_cross_delay = {sl['crossDelay']};",
      f"inline constexpr int cash_per_score = {d['cash']['perScore']};"]
L += [f"inline constexpr int enemy_{e['id']} = {i};" for i, e in enumerate(d["enemies"])] + [""]
PERKS = {"hp", "def", "dmg", "luck", "cooldown", "sight", "thermos", "tool_pct", "xp_pct", "cash", "crit", "coffee",
         "taken_pct"}   # v0.21.52 cz. c: Kask ojca - mniej otrzymanych obrażeń
def perk(pk):
    assert pk["effect"] in PERKS and -128 <= pk["value"] <= 127, pk
    return f'{{ core::perk_effect::{pk["effect"]}, {pk["value"]} }}'
L.append("inline constexpr core::badge_def badges[] = {   // perk = uprawnienie: trwała premia na każdą budowę")
coid0 = {x["id"]: i for i, x in enumerate(d["secrets"]["cosmetics"])}   # v0.21.52: wygląd także z odznak i zleceń
def title_cos(x):
    assert 1 <= len(x["title"]) <= 16 and 0 <= x["xp"] <= 50, x   # tytuł w profilu (GBA: wiersz telefonu)
    return f'{s(x["title"])}, {coid0[x["cosmetic"]] if "cosmetic" in x else -1}'
for b in d["badges"]:
    L.append(f'    {{ {s(b["name"])}, {s(b["desc"])}, {b["xp"]}, {perk(b["perk"])}, {title_cos(b)} }},')
L.append("};\n")
bid = {b["id"]: i for i, b in enumerate(d["badges"])}
L += [f"inline constexpr int badges_count = {len(d['badges'])};",
      f"inline constexpr int enemies_count = {len(d['enemies'])};"]
L += [f"inline constexpr int badge_{k} = {v};" for k, v in bid.items()] + [""]
KINDS = {"kills", "powers", "brand", "clean_boss", "class_wins", "wins"}
ks = d["keepsakes"]
kid = {k["id"]: i for i, k in enumerate(ks["list"])}
assert len(ks["rankRuns"]) == 2 and ks["rankRuns"][0] < ks["rankRuns"][1]
L.append("inline constexpr core::keepsake_def keepsakes[] = {   // pamiątki: wybierane na start budowy, ranga rośnie z budowami")
for k in ks["list"]:
    assert k["effect"] in PERKS and len(k["values"]) == 3 and len(k["name"]) <= 20 and len(k["desc"]) <= 34, k
    unlocked_by = [c["id"] for c in d["contracts"] if c.get("keepsake") == k["id"]]
    assert k.get("start") or "badge" in k or unlocked_by or k.get("streak"), f"pamiątka {k['id']} bez sposobu odblokowania"
    assert 0 <= k.get("streak", 0) <= 60, k   # v0.21.52 cz. c: seria dni budowy dnia
    L.append(f'    {{ {s(k["name"])}, {s(k["desc"])}, core::perk_effect::{k["effect"]}, {{ {", ".join(map(str, k["values"]))} }}, '
             f'{bid[k["badge"]] if "badge" in k else -1}, {"true" if k.get("start") else "false"}, {k.get("streak", 0)} }},')
L.append("};")
L += [f"inline constexpr int keepsakes_count = {len(ks['list'])};",
      f"inline constexpr int keepsake_rank_runs[] = {{ {ks['rankRuns'][0]}, {ks['rankRuns'][1]} }};", ""]
L.append("inline constexpr core::contract_def contracts[] = {   // zlecenia: długofalowe cele z licznikami w profilu")
for c in d["contracts"]:
    assert c["kind"] in KINDS and len(c["name"]) <= 16 and len(c["desc"]) <= 30 and 0 < c["target"] < 30000, c
    L.append(f'    {{ {s(c["name"])}, {s(c["desc"])}, core::contract_kind::{c["kind"]}, {c["target"]}, {c["xp"]}, '
             f'{kid[c["keepsake"]] if "keepsake" in c else -1}, {title_cos(c)} }},')
L.append("};")
L += [f"inline constexpr int contracts_count = {len(d['contracts'])};", ""]
se = d["siteEvents"]
L.append("inline constexpr core::site_event_def site_events[] = {   // wydarzenia na placu: SMS na starcie etapu")
for e in se["list"]:
    assert e["effect"] in {"fewer_pickups", "cash", "inspection", "rain", "thermos"} and len(e["short"]) <= 10, e
    assert len(e["name"]) <= 22 and len(e["info"]) <= 30, e
    L.append(f'    {{ {s(e["name"])}, {s(e["short"])}, {s(e["info"])}, {story(e)}, core::event_effect::{e["effect"]}, '
             f'{e["value"]}, {"true" if e["good"] else "false"} }},')
L.append("};")
L += [f"inline constexpr int site_events_count = {len(se['list'])};",
      f"inline constexpr int site_event_chance_pct = {se['chancePct']};", ""]
wt = d["weather"]
all_stages = (1 << len(all_st)) - 1
assert wt["list"][0]["effect"] == "none" and len(d["stages"]) <= 12   # pierwsza = bez skutku (domyślna)
def wmask(w):   # etapy kontraktów: pogoda jak na etapie Domu, który przypominają ("like"; bez - każda)
    if "stages" not in w: return all_stages
    m_ = sum(1 << i for i in w["stages"])
    for i, st in enumerate(all_st[dom_n:]):
        if "like" not in st or st["like"] in w["stages"]: m_ |= 1 << (dom_n + i)
    return m_
L.append("inline constexpr core::weather_def weather[] = {   // pogoda dnia: losowana na starcie etapu")
for w in wt["list"]:
    assert w["effect"] in {"none", "heat", "frost", "wind", "rain"} and len(w["short"]) <= 10, w
    assert len(w["name"]) <= 12 and len(w["info"]) <= 30 and 0 < w["weight"] < 128 and 0 <= w["value"] < 128, w
    assert w["effect"] not in ("frost", "rain") or w["value"] >= 2, w
    L.append(f'    {{ {s(w["name"])}, {s(w["short"])}, {s(w["info"])}, core::weather_effect::{w["effect"]}, {w["value"]}, '
             f'{w["weight"]}, {"true" if w["bad"] else "false"}, {wmask(w)}ull }},')
L.append("};")
for si in range(len(all_st)):   # każdy etap ma jakąś pogodę do wylosowania
    assert any((wmask(w) >> si) & 1 for w in wt["list"]), si
L += [f"inline constexpr int weather_count = {len(wt['list'])};",
      f"inline constexpr bool weather_no_bad_stack = {'true' if wt.get('noBadStack') else 'false'};", ""]
bg = d["brigade"]["list"]
assert 1 <= len(bg) <= 4   # telefon GBA: 4 wiersze listy
L.append("inline constexpr core::helper_def brigade[] = {   // brygada: najemni fachowcy (raz na etap)")
for h in bg:
    assert h["effect"] in {"reveal", "pump", "safety", "ally"} and len(h["name"]) <= 16 and len(h["desc"]) <= 23, h   # opis = baner
    assert 0 < h["price"] <= 200 and 0 <= h["cost"] <= 200 and 0 <= h["value"] < 100 and 0 <= h["turns"] < 100, h
    assert h["effect"] != "pump" or h["reach"] >= 1, h
    assert h["effect"] != "ally" or (h["turns"] > 0 and 0 <= h.get("frame", -1) < 15), h
    L.append(f'    {{ {s(h["name"])}, {s(h["desc"])}, core::helper_effect::{h["effect"]}, {h["value"]}, {h["turns"]}, {h["reach"]}, '
             f'{h["price"]}, {h["cost"]}, {h.get("frame", -1)} }},')
L.append("};")
L += [f"inline constexpr int brigade_count = {len(bg)};",
      f"inline constexpr int start_helpers_mask = {sum(1 << i for i, h in enumerate(bg) if h['cost'] == 0)};", ""]
iv = d["investor"]["list"]
assert 1 <= len(iv) <= 8
L.append("inline constexpr core::investor_def investor[] = {   // tryb inwestora: modyfikatory po pierwszej wygranej")
for x in iv:
    assert x["effect"] in {"cash_pct", "no_break", "enemy_hp", "no_shop", "slam", "enemy_dmg"} and len(x["name"]) <= 20 and len(x["desc"]) <= 30, x
    assert -90 <= x["value"] <= 100 and 0 < x["xpPct"] <= 100 and 0 < x["stake"] <= 9, x
    assert x["effect"] != "slam" or d["slam"]["every"] - x["value"] >= 2, x
    L.append(f'    {{ {s(x["name"])}, {s(x["desc"])}, core::investor_effect::{x["effect"]}, {x["value"]}, {x["xpPct"]}, {x["stake"]} }},')
L.append("};")
L += [f"inline constexpr int investor_count = {len(iv)};", ""]
ma = d["materials"]
assert len(ma["list"]) == 3 and 0 <= ma["dropPct"] <= 100 and 0 < ma["max"] <= 99   # HUD: 3 ikony, 1-2 cyfry
L.append("inline constexpr core::material_def materials[] = {   // materiały: cement, stal, drewno")
for x in ma["list"]:
    assert len(x["name"]) <= 8 and len(x["short"]) <= 6, x
    L.append(f'    {{ {s(x["name"])}, {s(x["short"])} }},')
L.append("};")
L.append("inline constexpr core::repair_def repairs[] = {   // naprawy pola za materiał")
for x in ma["repairs"]:
    assert x["effect"] in {"patch", "bridge"} and len(x["name"]) <= 12 and len(x["desc"]) <= 23 and len(x["info"]) <= 34, x
    assert 0 < x["cost"] <= 9 and 0 < x["value"] <= 20, x
    L.append(f'    {{ {s(x["name"])}, {s(x["desc"])}, {s(x["info"])}, core::repair_effect::{x["effect"]}, {mid[x["material"]]}, '
             f'{x["cost"]}, {x["value"]} }},')
L.append("};")
L += [f"inline constexpr int materials_count = {len(ma['list'])};",
      f"inline constexpr int repairs_count = {len(ma['repairs'])};",
      f"inline constexpr int material_drop_pct = {ma['dropPct']};",
      f"inline constexpr int material_boss_drop = {ma['bossDrop']};",
      f"inline constexpr int material_gear_box = {ma['gearBox']};",
      f"inline constexpr int material_max = {ma['max']};", ""]
assert len(ma["repairs"]) + len(d["brigade"]["list"]) <= 8
pa = d["paths"]["list"]
assert 2 <= len(pa) <= 8
L.append("inline constexpr core::path_def paths[] = {   // wybór ścieżki: wariant kolejnego etapu")
for x in pa:
    assert len(x["name"]) <= 22 and len(x["short"]) <= 9 and len(x["desc"]) <= 36, x
    assert -4 <= x["enemies"] <= 2 and -3 <= x["pickups"] <= 3 and -50 <= x["cash"] <= 50 and 0 <= x["materials"] <= 6, x
    L.append(f'    {{ {s(x["name"])}, {s(x["short"])}, {s(x["desc"])}, {x["enemies"]}, {x["pickups"]}, {x["cash"]}, {x["materials"]}, '
             f'{"true" if x["badWeather"] else "false"}, {"true" if x["noEvent"] else "false"} }},')
L.append("};")
L += [f"inline constexpr int paths_count = {len(pa)};", ""]
dy = d["daily"]
dif = {x["id"]: i for i, x in enumerate(d["difficulties"])}
assert 1 <= dy["history"] <= 5 and 0 <= dy["investorMods"] <= len(d["investor"]["list"])
L += [f"inline constexpr int daily_epoch[] = {{ {', '.join(map(str, dy['epoch']))} }};   // codzienna budowa: dzień nr 1",
      f"inline constexpr int daily_default_date[] = {{ {', '.join(map(str, dy['defaultDate']))} }};",
      f"inline constexpr int daily_difficulty = {dif[dy['difficulty']]};",
      f"inline constexpr int daily_investor_mods = {dy['investorMods']};",
      f"inline constexpr int daily_history = {dy['history']};", ""]
sc = d["schedule"]
L += [f"inline constexpr int schedule_min_days = {sc['minDays']};   // harmonogram domu: dni etapu = min + tury / turnsPerDay",
      f"inline constexpr int schedule_turns_per_day = {sc['turnsPerDay']};",
      f"inline constexpr const char* schedule_url = {s(sc['url'])};", ""]
L.append("inline constexpr core::tool_def tools[] = {")
for t in m["tools"]:
    assert "cost" not in t and sum(bool(t.get(k)) for k in ("shop", "reward", "secret")) <= 1, t   # v0.21.52: cena z meta.toolCosts
    L.append(f'    {{ {wid[t["weapon"]]}, {"true" if t.get("shop") else "false"}, {"true" if t.get("reward") else "false"}, {"true" if t.get("secret") else "false"} }},')
L.append("};\n")
dr = d["drops"]
start_tools = sum(1 << i for i, t in enumerate(m["tools"]) if not t.get("shop") and not t.get("reward") and not t.get("secret"))
shop_tools = sum(1 for t in m["tools"] if t.get("shop"))
assert len(m["toolCosts"]) == shop_tools and all(0 < c < 1000 for c in m["toolCosts"]) and m["toolCosts"] == sorted(m["toolCosts"]), m["toolCosts"]
assert len(m["tools"]) <= 12 and all(i < 8 for i, t in enumerate(m["tools"]) if not t.get("secret")), "narzędzia: bitmaska uint8 w profilu"
L += [f"inline constexpr int tools_count = {len(m['tools'])};",
      f"inline constexpr int secret_tools_mask = {sum(1 << i for i, t in enumerate(m['tools']) if t.get('secret'))};   // z sekretnych zleceń",
      f"inline constexpr int start_tools_mask = {start_tools};",
      f"inline constexpr int tool_costs[] = {{ {', '.join(map(str, m['toolCosts']))} }};   // v0.21.52: cena kolejnego kupionego narzędzia",
      f"inline constexpr int tool_costs_count = {len(m['toolCosts'])};",
      f"inline constexpr int drop_chance_pct = {dr['chancePct']};",
      f"inline constexpr int drop_weights[] = {{ {dr['weights']['coffee']}, {dr['weights']['helmet']}, {dr['weights']['plan']}, {dr['weights']['tool']}, {dr['weights']['gear']} }};", ""]
lk = d["luck"]
L += [f"inline constexpr int crit_base_pct = {lk['critBasePct']};",
      f"inline constexpr int crit_per_luck_pct = {lk['critPerLuckPct']};",
      f"inline constexpr int crit_multiplier = {lk['critMultiplier']};",
      f"inline constexpr int drop_per_luck_pct = {lk['dropPerLuckPct']};",
      f"inline constexpr int rarity_per_luck = {lk['rarityPerLuck']};",
      f"inline constexpr int dodge_per_luck_pct = {lk['dodgePerLuckPct']};",
      f"inline constexpr int dodge_max_pct = {lk['dodgeMaxPct']};", ""]
th = d["thermos"]
L += [f"inline constexpr int thermos_capacity = {th['capacity']};",
      f"inline constexpr int coffee_heal = {th['heal']};",
      f"inline constexpr int bot_drink_below_pct = {th['botDrinkBelowPct']};", ""]
eq = d["equipment"]
L.append("inline constexpr core::gear_def gear[] = {   // indeks = slot * 3 + jakość")
for sl in eq["slots"]:
    assert len(sl["items"]) == len(eq["rarities"]) == 3
    for name, val in sl["items"]:
        assert len(name) <= 20, name
        L.append(f'    {{ {s(name)}, core::gear_stat::{sl["stat"]}, {val} }},')
L.append("};")
L.append("inline constexpr const char* gear_slots[] = { " + ", ".join(s(sl["name"]) for sl in eq["slots"]) + " };")
L.append("inline constexpr const char* gear_rarities[] = { " + ", ".join(s(r) for r in eq["rarities"]) + " };")
L.append("inline constexpr core::trait_def gear_traits[] = {   // cechy sprzętu (losowane do każdego przedmiotu)")
for t in eq["traits"]:
    assert t["effect"] in {"luck", "crit", "poison_res", "sight", "cooldown", "str", "agi", "intel", "slip_res"} and len(t["short"]) <= 9 and len(t["name"]) <= 22, t
    L.append(f'    {{ {s(t["name"])}, {s(t["short"])}, core::trait_effect::{t["effect"]}, {t["value"]} }},')
L.append("};")
L.append(f"inline constexpr int gear_traits_count = {len(eq['traits'])};")
L.append(f"inline constexpr int gear_decline_xp = {eq['declineXp']};")
rr = eq["rarityRoll"]
assert 3 <= len(eq["slots"]) <= 6 and all(sl["stat"] in {"def", "dmg", "hp", "dodge", "thermos"} for sl in eq["slots"])
L += [f"inline constexpr int gear_slots_count = {len(eq['slots'])};",
      f"inline constexpr int gear_reward_mask = {sum(1 << i for i, sl in enumerate(eq['slots']) if sl.get('reward'))};   // sloty z nagród",
      f"inline constexpr int gear_base_mask = {sum(1 << i for i, sl in enumerate(eq['slots']) if not sl.get('reward'))};   // pełny sprzęt (BHP)",
      f"inline constexpr int gear_solid_from = {rr['solidFrom']};",
      f"inline constexpr int gear_brand_from = {rr['brandFrom']};",
      f"inline constexpr int gear_stage_bonus = {rr['stageBonus']};", ""]
start_mask = sum(1 << cid[c] for c in m["startClasses"])
assert all(not d["classes"][cid[c]].get("reward") and not d["classes"][cid[c]].get("secret") for c in m["startClasses"])
assert all(i < 8 for i, c in enumerate(d["classes"]) if not c.get("reward") and not c.get("secret")), "zawody do kupienia: bitmaska uint8 w profilu"
assert len(d["classes"]) <= 12 and all(0 <= c["frame"] < 64 or 127 <= c["frame"] < 160 for c in d["classes"])
open_classes = sum(1 for c in d["classes"] if not c.get("secret"))   # v0.21.51 cz. 2: zawody z sekretów na końcu listy
assert all(c.get("secret") for c in d["classes"][open_classes:]), "zawody z sekretnych zleceń na końcu listy"
L += [f"inline constexpr int upgrades_count = {len(m['upgrades'])};",
      f"inline constexpr int xp_per_kill = {m['xpPerKill']};",
      f"inline constexpr int xp_per_stage = {m['xpPerStage']};",
      f"inline constexpr int xp_boss = {m['xpBoss']};",
      f"inline constexpr int start_classes_mask = {start_mask};",
      f"inline constexpr int reward_classes_mask = {sum(1 << i for i, c in enumerate(d['classes']) if c.get('reward'))};",
      f"inline constexpr int secret_classes_mask = {sum(1 << i for i, c in enumerate(d['classes']) if c.get('secret'))};   // z sekretnych zleceń",
      f"inline constexpr int open_classes_count = {open_classes};   // zawody bez sekretów (budowa dnia, balans, Pełny zespół)",
      f"inline constexpr int class_costs[] = {{ {', '.join(map(str, m['classCosts']))} }};   // v0.21.52: cena kolejnego kupionego zawodu",
      f"inline constexpr int class_costs_count = {len(m['classCosts'])};",
      f"inline constexpr int hard_cost = {m['hardCost']};", ""]
hl = d["heroLevels"]
L += [f"inline constexpr int level_thresholds[] = {{ {', '.join(map(str, hl['thresholds']))} }};",
      f"inline constexpr int max_hero_level = {len(hl['thresholds']) + 1};",
      f"inline constexpr int hp_per_level = {hl['hpPerLevel']};",
      f"inline constexpr int dmg_levels_mask = {sum(1 << l for l in hl['dmgLevels'])};",
      f"inline constexpr int def_levels_mask = {sum(1 << l for l in hl['defLevels'])};", ""]
rs = d["respect"]
REFF = {"dmg_pct", "taken_pct", "gear_pct", "crit", "dodge", "coffee_pct", "thermos", "cooldown", "cash", "xp_pct",
        "brigade_pct", "sight", "shop_pct", "mats_pct", "second_chance", "reroll", "veteran"}
assert 1 <= len(rs["upgrades"]) <= 19 and all(0 < rs[k] < 50 for k in ("stage", "boss", "actBoss", "final"))   # profil: 16 + 3 (v12)
sec = d["secrets"]
sid = {x["id"]: i for i, x in enumerate(sec["list"])}
L.append("inline constexpr core::respect_def respect[] = {   // Respekt: stałe ulepszenia z rangami")
for x in rs["upgrades"]:
    assert x["effect"] in REFF and 1 <= len(x["values"]) == len(x["costs"]) <= 5 and len(x["name"]) <= 17, x
    assert "secret" not in x or sec["list"][sid[x["secret"]]]["reward"] == {"kind": "respect", "id": x["id"]}, x
    assert all(0 < v < 128 for v in x["values"]) and x["values"] == sorted(x["values"]) and all(0 < c < 1000 for c in x["costs"]), x
    vals = x["values"] + [0] * (5 - len(x["values"])); costs = x["costs"] + [0] * (5 - len(x["costs"]))
    L.append(f'    {{ {s(x["name"])}, {s(x["desc"])}, core::respect_effect::{x["effect"]}, {len(x["values"])}, '
             f'{{ {", ".join(map(str, vals))} }}, {{ {", ".join(map(str, costs))} }}, {sid[x["secret"]] if "secret" in x else -1} }},')
L.append("};")
assert 2 <= d['passives']['markTurns'] <= 20
L += [f"inline constexpr int push_chance_pct = {d['passives']['pushChancePct']};   // Operator koparki: cios wręcz odpycha",
      f"inline constexpr int mark_turns = {d['passives']['markTurns']};   // Geodeta: Tyczenie trwa tyle tur",
      f"inline constexpr int respect_count = {len(rs['upgrades'])};",
      f"inline constexpr int respect_stage = {rs['stage']};   // Respekt za etap: zwykły, boss w środku aktu, boss aktu, ostatni",
      f"inline constexpr int respect_boss = {rs['boss']};",
      f"inline constexpr int respect_act_boss = {rs['actBoss']};",
      f"inline constexpr int respect_final = {rs['final']};", ""]
rw = d["rewards"]["list"]
tid = {t["weapon"]: i for i, t in enumerate(m["tools"])}
slid = {sl["name"]: i for i, sl in enumerate(eq["slots"])}
RK = {"tool": "tool", "gear": "gear", "class": "cls", "act": "act", "soon": "soon"}
assert 1 <= len(rw) <= 16
L.append("inline constexpr core::reward_def rewards[] = {   // nagrody za odbiór: każda wygrana odblokowuje kolejną")
seen = set()
for x in rw:
    k = x["kind"]
    assert k in RK and len(x["desc"]) <= 23, x   # baner: 23 znaki
    if k == "tool": idx = tid[x["id"]]; assert m["tools"][idx].get("reward"), x; name = d["weapons"][wid[x["id"]]]["name"]
    elif k == "gear": idx = slid[x["id"]]; assert eq["slots"][idx].get("reward"), x; name = eq["slots"][idx]["name"]
    elif k == "class": idx = cid[x["id"]]; assert d["classes"][idx].get("reward"), x; name = d["classes"][idx]["name"]
    elif k == "act": idx = next(i for i, a in enumerate(acts) if a.get("prelude")); name = x["name"]
    else: idx = -1; name = x["name"]
    assert (k, idx) not in seen; seen.add((k, idx))
    L.append(f'    {{ core::reward_kind::{RK[k]}, {idx}, {s(x.get("name", name))}, {s(x["desc"])} }},')
L.append("};")
for i, t in enumerate(m["tools"]):   # każde narzędzie / slot / zawód z nagrody jest na liście nagród
    assert not t.get("reward") or ("tool", i) in seen, t
for i, sl in enumerate(eq["slots"]):
    assert not sl.get("reward") or ("gear", i) in seen, sl
for i, c in enumerate(d["classes"]):
    assert not c.get("reward") or ("class", i) in seen, c["id"]
assert not prelude_stages or any(k == "act" for k, _ in seen), "Akt 0 bez nagrody za odbiór"
L += [f"inline constexpr int rewards_count = {len(rw)};", ""]
tu = d["tutorial"]
TSCR = {"title": 0, "class": 1}
def tut(x):
    assert x["screen"] in TSCR and len(x["title"]) <= 20 and len(x.get("gba", "")) <= 26 and x.get("requires", "") in ("", "investor"), x
    return (f'    {{ {s(x["id"])}, {s(x["title"])}, {story({"from": tu["from"], "text": x["text"]})}, {s(x.get("gba", ""))}, '
            f'{TSCR[x["screen"]]}, {"true" if x.get("godotOnly") else "false"}, {"true" if x.get("requires") == "investor" else "false"} }},')
L.append("inline constexpr core::tutorial_step tutorial_steps[] = {   // samouczek menu: tytuł (0) i wybór zawodu (1)")
L += [tut(x) for x in tu["steps"]] + ["};"]
UNL = ["respect", "daily", "investor", "act0", "class", "secret"]
assert [x["id"] for x in tu["unlocks"]] == UNL
L.append("inline constexpr core::tutorial_step tutorial_unlocks[] = {   // dymki przy pierwszym odblokowaniu (kolejność = core::tutorial_unlock)")
L += [tut(x) for x in tu["unlocks"]] + ["};"]
L += [f"inline constexpr int tutorial_steps_count = {len(tu['steps'])};", f"inline constexpr int tutorial_unlocks_count = {len(tu['unlocks'])};", ""]
# v0.21.50 cz. 2: premie po etapie (#27), elity (#28), kombinacje stanów (#29)
bo = d["boons"]
BTAG = [x["id"] for x in bo["tags"]]
BEFF = ["dmg", "dmg_pct", "crit", "max_hp", "def", "dodge", "coffee", "thermos", "cooldown", "cash", "mats", "luck", "wet_hits",
        "frost_hits", "electric", "spark", "brigade_pct", "regen_stage", "kill_heal", "status_res", "power", "mats_pct", "shop_pct", "sight"]
RAR = {x["id"]: i for i, x in enumerate(bo["rarities"])}
cid2 = {c["id"]: i for i, c in enumerate(d["classes"])}
assert len(bo["rarities"]) == 3 and len(BTAG) <= 16 and 2 <= len(bo["list"]) <= 64 and 0 < bo["rerollCost"] <= 200
def tagmask(t): assert all(x in BTAG for x in t) and 1 <= len(t) <= 2, t; return sum(1 << BTAG.index(x) for x in t)
L += ["inline constexpr core::boon_rarity_def boon_rarities[] = {   // premie: rzadkość i waga losowania"]
L += [f'    {{ {s(x["name"])}, {x["weight"]} }},' for x in bo["rarities"]] + ["};"]
L += ["inline constexpr const char* boon_tags[] = { " + ", ".join(s(x["name"]) for x in bo["tags"]) + " };   // znaczniki premii"]
L.append("inline constexpr core::boon_def boons[] = {   // premie po etapie: 1 z 3 (rzadkość, znaczniki, skutek, zawód)")
for x in bo["list"]:
    assert x["effect"] in BEFF and len(x["name"]) <= 20 and len(x["desc"]) <= 26 and -128 < x["value"] < 128, x
    assert x["effect"] != "power" or "class" in x, x   # wzmocnienie mocy tylko w premiach zawodów
    assert not x.get("mastery") or "class" in x, x   # v0.21.52 cz. b: premia mistrzostwa zawodu (poziom 7) - tylko zawodu
    L.append(f'    {{ {s(x["name"])}, {s(x["desc"])}, {RAR[x["rarity"]]}, {tagmask(x["tags"])}, core::boon_effect::{x["effect"]}, '
             f'{x["value"]}, {cid2[x["class"]] if "class" in x else -1}, {"true" if x.get("mastery") else "false"} }},')
L.append("};")
for c in range(len(d["classes"])):   # każdy zawód ma 1-2 własne premie
    n = sum(1 for x in bo["list"] if cid2.get(x.get("class"), -1) == c and not x.get("mastery"))
    assert 1 <= n <= 2, (d["classes"][c]["id"], n)
    assert sum(1 for x in bo["list"] if cid2.get(x.get("class"), -1) == c and x.get("mastery")) == 1, d["classes"][c]["id"]
first_mastery = min(i for i, x in enumerate(bo["list"]) if x.get("mastery"))   # premie mistrzostwa na końcu listy (te same oferty bez nich)
assert all(x.get("mastery") for x in bo["list"][first_mastery:]), "premie mistrzostwa na końcu listy"
for r in bo["rarities"]:
    assert any(x["rarity"] == r["id"] and "class" not in x for x in bo["list"]), r
SEFF = ["conduct", "armor", "espresso", "safety", "luck", "brigade", "stock", "sparks"]
L.append("inline constexpr core::synergy_def synergies[] = {   // synergie: 2+ premie z tym samym znacznikiem")
for x in bo["synergies"]:
    assert x["effect"] in SEFF and len(x["name"]) <= 14 and len(x["desc"]) <= 32, x
    L.append(f'    {{ {s(x["name"])}, {s(x["desc"])}, {tagmask(x["tags"])}, core::synergy_effect::{x["effect"]}, {x["value"]} }},')
L.append("};")
L += [f"inline constexpr int boons_count = {len(bo['list'])};", f"inline constexpr int boon_tags_count = {len(BTAG)};",
      f"inline constexpr int synergies_count = {len(bo['synergies'])};", f"inline constexpr int boon_reroll_cost = {bo['rerollCost']};",
      f"inline constexpr int synergy_at = {bo['synergyAt']};", f"inline constexpr int boon_luck_rare = {bo['luckRare']};",
      f"inline constexpr int boon_luck_legend = {bo['luckLegend']};", ""]
el = d["elites"]
EEFF = ["shield", "fast", "regen", "explode", "summon"]
assert len(el["actPct"]) == len(acts) and len(el["diffPct"]) == len(d["difficulties"]) and 100 <= el["hpPct"] <= 300
L.append("inline constexpr core::elite_def elites[] = {   // elity: cecha, przedrostek nazwy wg rodzaju")
for x in el["traits"]:
    assert x["effect"] in EEFF and len(x["prefix"]) == 3 and all(len(p) <= 10 for p in x["prefix"]) and len(x["info"]) <= 24, x
    L.append(f'    {{ {s(x["name"])}, {{ {", ".join(s(p) for p in x["prefix"])} }}, {s(x["info"])}, core::elite_effect::{x["effect"]}, {x["value"]} }},')
L.append("};")
L += [f"inline constexpr int elites_count = {len(el['traits'])};",
      f"inline constexpr int elite_act_pct[] = {{ {', '.join(map(str, el['actPct']))} }};   // szansa na elitę wg aktu (data::acts)",
      f"inline constexpr int elite_diff_pct[] = {{ {', '.join(map(str, el['diffPct']))} }};",
      f"inline constexpr int elite_tier_pct = {el['tierPct']};", f"inline constexpr int elite_hp_pct = {el['hpPct']};",
      f"inline constexpr int elite_dmg = {el['dmg']};", f"inline constexpr int elite_respect = {el['reward']['respect']};",
      f"inline constexpr int elite_mats = {el['reward']['mats']};", f"inline constexpr int elite_gear_min = {el['reward']['gearMin']};", ""]
co = d["combos"]
CEFF = ["shock_area", "dust_blast", "crack"]
assert [x["effect"] for x in co["list"]] == CEFF   # kolejność = core::combo_effect
L.append("inline constexpr core::combo_def combos[] = {   // kombinacje stanów (kolejność = core::combo_effect)")
for x in co["list"]:
    assert len(x["name"]) <= 14 and len(x["short"]) <= 20 and len(x["info"]) <= 36 and len(x["hero"]) <= 40, x
    L.append(f'    {{ {s(x["name"])}, {s(x["short"])}, {s(x["info"])}, {s(x["hero"])}, core::combo_effect::{x["effect"]}, '
             f'{x["value"]}, {x["radius"]}, {x["heroValue"]} }},')
L.append("};")
assert len(co["sources"]) == 4 and all(len(x) <= 36 for x in co["sources"])
L += ["inline constexpr const char* combo_sources[] = { " + ", ".join(s(x) for x in co["sources"]) + " };   // Jak grać: skąd stany",
      f"inline constexpr int combos_count = {len(co['list'])};", f"inline constexpr int wet_turns = {co['wetTurns']};",
      f"inline constexpr int hero_wet_turns = {co['heroWetTurns']};", ""]

# v0.21.50 cz. 3: wydarzenia z wyborem (#30), ulepszanie narzędzia (#31), ukryte pomieszczenia (#32)
ce = d["choiceEvents"]
CHE = ["cash", "xp", "hp", "max_hp", "mats", "stage_dmg", "stage_def", "boon", "gear", "respect", "coffee", "spawn", "status",
       "upgrade", "power"]
STS = ["none", "poison", "shock", "slip", "paper", "wet"]
slot_names = [x["name"] for x in d["equipment"]["slots"]]
def cout(x):
    assert x["effect"] in CHE and -100 < x["value"] < 100 and 1 <= x.get("chance", 100) <= 100, x
    arg = -1
    if x["effect"] == "mats": arg = mid[x["material"]] if "material" in x else -1
    if x["effect"] == "spawn": arg = eid[x["enemy"]]; assert 1 <= x["value"] <= 3 and not d["enemies"][arg].get("slam"), x
    if x["effect"] == "status": arg = STS.index(x["status"]); assert arg > 0, x
    if x["effect"] == "gear": arg = slot_names.index(x["slot"]) if "slot" in x else -1; assert 0 <= x["value"] <= 2, x
    return f'{{ core::choice_effect::{x["effect"]}, {x["value"]}, {x.get("chance", 100)}, {arg} }}'
PAD = "{ core::choice_effect::cash, 0, 0, -1 }"
L.append("inline constexpr core::choice_event_def choice_events[] = {   // wydarzenia z wyborem: pole z SMS-em na etapie")
for e in ce["list"]:
    assert len(e["name"]) <= 22 and 2 <= len(e["choices"]) <= 3, e
    chs = []
    for c in e["choices"]:
        assert len(c["label"]) <= 18 and len(c["result"]) <= 34 and len(c["effects"]) <= 3, c
        outs = [cout(x) for x in c["effects"]] + [PAD] * (3 - len(c["effects"]))
        chs.append(f'{{ {s(c["label"])}, {s(c["result"])}, {{ {", ".join(outs)} }}, {len(c["effects"])} }}')
    chs += ['{ "", "", { ' + ", ".join([PAD] * 3) + ' }, 0 }'] * (3 - len(e["choices"]))
    L.append(f'    {{ {s(e["name"])}, {story(e)}, {{ {", ".join(chs)} }}, {len(e["choices"])} }},')
L.append("};")
assert len(ce["list"]) <= 16 and 0 <= ce["chancePct"] <= 100   # bitmaska widzianych w budowie (uint16)
L += [f"inline constexpr int choice_events_count = {len(ce['list'])};", f"inline constexpr int choice_event_chance_pct = {ce['chancePct']};", ""]
tu2 = d["toolUpgrade"]
TTE = ["pierce", "crit", "steady"]
assert len(tu2["levels"]) == tu2["max"] and 1 <= tu2["traitAt"] <= tu2["max"] and len(tu2["traits"]) == 3
L.append("inline constexpr core::tool_level_def tool_levels[] = {   // koszt kolejnych poziomów ulepszenia narzędzia")
for x in tu2["levels"]:
    assert 0 <= x["cash"] <= 200 and 0 < x["count"] <= 9, x
    L.append(f'    {{ {x["cash"]}, {mid[x["material"]]}, {x["count"]} }},')
L.append("};")
L.append("inline constexpr core::tool_trait_def tool_traits[] = {   // cecha ulepszonego narzędzia (wybór na poziomie traitAt)")
for x in tu2["traits"]:
    assert x["effect"] in TTE and len(x["name"]) <= 12 and len(x["short"]) <= 12 and len(x["desc"]) <= 26, x
    L.append(f'    {{ {s(x["name"])}, {s(x["short"])}, {s(x["desc"])}, core::tool_trait_effect::{x["effect"]}, {x["value"]} }},')
L.append("};")
L += [f"inline constexpr int tool_upgrade_max = {tu2['max']};", f"inline constexpr int tool_upgrade_dmg = {tu2['dmg']};",
      f"inline constexpr int tool_trait_at = {tu2['traitAt']};", f"inline constexpr int tool_traits_count = {len(tu2['traits'])};", ""]
hr = d["hiddenRooms"]
assert 1 <= len(hr["kinds"]) <= 2 and 0 <= hr["chancePct"] <= 100 and 0 <= hr["guardPct"] <= 100
L.append("inline constexpr core::secret_kind_def secret_kinds[] = {   // ukryte pomieszczenie: pęknięta ściana / drzwi")
for x in hr["kinds"]:
    assert len(x["name"]) <= 18 and len(x["info"]) <= 30, x
    L.append(f'    {{ {s(x["name"])}, {s(x["info"])}, {"true" if x["breakable"] else "false"} }},')
L.append("};")
chs2 = hr["chest"]
assert 0 <= chs2["gearMin"] <= 2
L += [f"inline constexpr int secret_kinds_count = {len(hr['kinds'])};", f"inline constexpr int secret_chance_pct = {hr['chancePct']};",
      f"inline constexpr int secret_guard_pct = {hr['guardPct']};", f"inline constexpr int chest_respect = {chs2['respect']};",
      f"inline constexpr int chest_mats = {chs2['mats']};", f"inline constexpr int chest_cash = {chs2['cash']};",
      f"inline constexpr int chest_gear_min = {chs2['gearMin']};", ""]

# v0.21.50 cz. 4: podsumowanie budowy (#33), wyzwania tygodnia (#34), fabuła odkrywana z budowami (#35)
rc = d["recap"]
RTIP = ["shock", "slam", "blast", "coffee", "ranged", "elite", "boss", "no_combo", "won", "any"]
assert len(rc["kinds"]) == 6 and all(len(x) <= 14 for x in rc["kinds"]) and len(rc["verbs"]) == 3
assert rc["tips"][-1]["when"] == "any"   # zawsze jakaś rada
def tip2(t):
    lines = t.split("|"); assert 1 <= len(lines) <= 2 and all(len(l) <= 28 for l in lines), t
    return ", ".join(s(l) for l in lines + [""] * (2 - len(lines)))
L.append("inline constexpr const char* recap_kind_names[] = { " + ", ".join(s(x) for x in rc["kinds"]) + " };   // = core::recap_kind")
L.append("inline constexpr const char* recap_verbs[] = { " + ", ".join(s(x) for x in rc["verbs"]) + " };   // Pokonał / Pokonała / Pokonało Cię")
L.append("inline constexpr core::recap_tip_def recap_tips[] = {   // rada w podsumowaniu: pierwsza pasująca")
for x in rc["tips"]:
    assert x["when"] in RTIP, x
    L.append(f'    {{ core::recap_tip::{x["when"]}, {{ {tip2(x["text"])} }} }},')
L += ["};", f"inline constexpr int recap_tips_count = {len(rc['tips'])};", ""]
wk = d["weekly"]
WRULE = ["cls", "no_coffee", "elite_pct", "weather", "no_shop", "mats_pct", "hp_pct", "dmg_pct", "cash"]
wid2 = {x["id"]: i for i, x in enumerate(d["weather"]["list"])}
assert 1 <= wk["history"] <= 3 and 1 <= len(wk["list"]) <= 16 and 0 <= wk["coffeeCash"] <= 50
def wrule(x):
    r = {"class": "cls"}.get(x["rule"], x["rule"]); assert r in WRULE, x
    v = cid2[x["class"]] if r == "cls" else (wid2[x["weather"]] if r == "weather" else x.get("value", 0))
    assert -90 <= v <= 400, x
    return f'{{ core::weekly_rule::{r}, {v} }}'
L.append("inline constexpr core::weekly_def weekly[] = {   // wyzwania tygodnia: kolejne tygodnie po kolei z listy")
for x in wk["list"]:
    ds = x["desc"].split("|"); assert len(ds) == 2 and all(len(l) <= 28 for l in ds), x
    assert len(x["name"]) <= 26 and len(x["short"]) <= 18 and 1 <= len(x["rules"]) <= 3, x
    assert sum(1 for r in x["rules"] if r["rule"] == "class") <= 1, x
    rules = [wrule(r) for r in x["rules"]] + ["{ core::weekly_rule::cash, 0 }"] * (3 - len(x["rules"]))
    L.append(f'    {{ {s(x["name"])}, {s(x["short"])}, {{ {s(ds[0])}, {s(ds[1])} }}, {{ {", ".join(rules)} }}, {len(x["rules"])} }},')
L += ["};", f"inline constexpr int weekly_count = {len(wk['list'])};",
      f"inline constexpr int weekly_epoch[] = {{ {', '.join(map(str, wk['epoch']))} }};   // tydzień nr 1 (poniedziałek)",
      f"inline constexpr int weekly_difficulty = {dif[wk['difficulty']]};", f"inline constexpr int weekly_history = {wk['history']};",
      f"inline constexpr int weekly_coffee_cash = {wk['coffeeCash']};", ""]
import datetime
assert datetime.date(*wk["epoch"]).weekday() == 0, "epoka tygodni: poniedziałek"
arc = d["story"]["arc"]
STRG = ["runs", "wins", "boss", "elite", "secret", "event", "synergy", "daily", "weekly", "act0", "inspector"]   # v0.21.52 cz. b: poziom inspektora
assert 1 <= len(arc) <= 32   # bity w profilu (uint32)
L.append("inline constexpr core::story_thread story_arc[] = {   // fabuła odkrywana z budowami: wątki SMS-ów (archiwum Wiadomości)")
for x in arc:
    assert x["trigger"] in STRG and len(x["name"]) <= 18 and len(x["hint"]) <= 28 and 1 <= len(x["messages"]) <= 2, x
    v = eid[x["value"]] if x["trigger"] == "boss" else x["value"]
    if x["trigger"] == "boss": assert d["enemies"][v].get("slam"), x
    assert 0 <= v <= 100, x
    msgs = [story(mm) for mm in x["messages"]] + ['{ "", { "", "", "" } }'] * (2 - len(x["messages"]))
    L.append(f'    {{ {s(x["name"])}, {s(x["hint"])}, core::story_trigger::{x["trigger"]}, {v}, {{ {", ".join(msgs)} }}, {len(x["messages"])} }},')
L += ["};", f"inline constexpr int story_arc_count = {len(arc)};", ""]
# v0.21.51 cz. 2: sekretne zlecenia (#39) - ukryte cele profilu z nagrodą (zawód, narzędzie, wygląd, Respekt)
SKIND = ["no_coffee_win", "helper_boss", "storerooms", "class_wins", "paper_clean", "shock_combos", "low_hp_win", "fast_win"]
SREW = {"class": "cls", "tool": "tool", "cosmetic": "cosmetic", "respect": "respect"}
cos = sec["cosmetics"]
coid = {x["id"]: i for i, x in enumerate(cos)}
rsid = {x["id"]: i for i, x in enumerate(rs["upgrades"])}
tid2 = {t["weapon"]: i for i, t in enumerate(m["tools"])}
assert 1 <= len(sec["list"]) <= 16 and 1 <= len(cos) <= 24   # profil: bitmaska uint16; v0.21.52 cz. b: kolory kasku ponad 8 (wybór w p.helmet)
assert all("helmet" in x for x in cos[8:]), "przełączniki wyglądu (bity p.cosmetic, uint8) tylko wśród pierwszych 8"
L.append("inline constexpr core::secret_def secrets[] = {   // sekretne zlecenia: \"???\" z podpowiedzią, nagroda po wykonaniu")
srew = set()
for x in sec["list"]:
    assert x["kind"] in SKIND and len(x["hint"]) <= 28 and len(x["desc"]) <= 30 and len(x["rewardText"]) <= 23, x
    rw_ = x["reward"]; k = rw_["kind"]
    if k == "class": idx = cid[rw_["id"]]; assert d["classes"][idx].get("secret"), x
    elif k == "tool": idx = tid2[rw_["id"]]; assert m["tools"][idx].get("secret"), x
    elif k == "cosmetic": idx = coid[rw_["id"]]
    else: idx = rsid[rw_["id"]]; assert rs["upgrades"][idx].get("secret") == x["id"], x
    assert (k, idx) not in srew; srew.add((k, idx))
    v = eid[x["enemy"]] if x["kind"] == "helper_boss" else x.get("value", 0)
    if x["kind"] == "helper_boss": assert d["enemies"][v].get("slam"), x
    assert 0 <= v <= 1000, x
    L.append(f'    {{ {s(x["hint"])}, {s(x["desc"])}, core::secret_kind::{x["kind"]}, {v}, core::secret_reward::{SREW[k]}, {idx}, '
             f'{s(x["rewardText"])}, {story({"from": tu["from"], "text": x["news"]})} }},')
L.append("};")
for i, c in enumerate(d["classes"]):   # każdy zawód / narzędzie z sekretu ma swoje zlecenie
    assert not c.get("secret") or ("class", i) in srew, c["id"]
for i, t in enumerate(m["tools"]):
    assert not t.get("secret") or ("tool", i) in srew, t
from_meta = {x["cosmetic"] for x in d["badges"] + d["contracts"] if "cosmetic" in x}   # v0.21.52: kolory kasku z odznak i zleceń
GOALS = d["inspector"]["levels"] + d["investor"]["ranks"] + d["mastery"]["levels"] + d["tasks"]["rewards"] + d["daily"]["streak"] \
    + [x["reward"] for x in d["collections"]["sets"]] \
    + [{"reward": "helmet", "id": c["reward"]["helmet"]} for c in car if "helmet" in c.get("reward", {})]   # cz. c: zadania, seria dni, kolekcje; cz. d: kariera
from_meta |= {x["id"] for x in GOALS if x["reward"] == "helmet"}   # cz. b
assert len(from_meta) == sum(1 for x in d["badges"] + d["contracts"] if "cosmetic" in x) + sum(
    1 for x in GOALS if x["reward"] == "helmet"), "kolor kasku z jednego źródła"
for i in range(len(cos)): assert (("cosmetic", i) in srew) != (cos[i]["id"] in from_meta), cos[i]   # jedno źródło
for i in range(len(cos)): assert ("helmet" in cos[i]) == (cos[i]["id"] in from_meta), cos[i]
assert all(p_ in eid for p_ in sec["paper"])
L.append("inline constexpr core::cosmetic_def cosmetics[] = {   // wygląd z sekretnych zleceń (tylko oprawa)")
def rgb555(c): assert len(c) == 3 and all(0 <= v <= 255 for v in c), c; return (c[0] >> 3) | ((c[1] >> 3) << 5) | ((c[2] >> 3) << 10)
for x in cos:
    assert len(x["name"]) <= 16 and len(x["desc"]) <= 30, x
    L.append(f'    {{ {s(x["name"])}, {s(x["desc"])}, {rgb555(x["helmet"]) if "helmet" in x else -1} }},')
L.append("};")
hb = [eid[x["enemy"]] for x in sec["list"] if x["kind"] == "helper_boss"]
L += [f"inline constexpr int secrets_count = {len(sec['list'])};", f"inline constexpr int cosmetics_count = {len(cos)};",
      f"inline constexpr uint64_t secret_paper_mask = {sum(1 << eid[p_] for p_ in sec['paper'])}ull;   // problemy papierowe (Akt 0 bez obrażeń)",
      f"inline constexpr int secret_helper_boss = {hb[0] if hb else -1};   // boss pokonany ciosem brygady (Szef tylko dzwoni)",
      f"inline constexpr int cosmetic_gold = {coid.get('zlota_kielnia', -1)};   // złoty błysk broni przy krycie",
      f"inline constexpr int cosmetic_stripes = {coid.get('kask_paski', -1)};   // kask w paski (wybór zawodu)", ""]
# v0.21.52 cz. d (#47): mapa kariery - kontrakty (pierwszy etap w data::stages, liczba, Akt 0, warunek odblokowania,
# nagroda za pierwszą wygraną: Respekt, tytuł, kolor kasku; porywy aktu II co N tur, bliźniak)
UNL = ["none", "wins", "inspector"]
L.append("inline constexpr core::career_def career[] = {   // kontrakty mapy kariery (0 = Dom jednorodzinny, domyślny)")
first = 0
for i, c in enumerate(car):
    n = dom_n if i == 0 else len(c["stages"])
    u, rw_ = c["unlock"], c.get("reward", {})
    assert u["kind"] in UNL and (i == 0) == (u["kind"] == "none") and 0 <= u["value"] <= 35, c
    assert len(c["name"]) <= 18 and len(c["short"]) <= 10 and len(c["desc"]) <= 26, c
    assert 0 <= c.get("gust", 0) <= 9 and (c.get("gust", 0) == 0 or c["gust"] >= 3) and 0 <= rw_.get("respect", 0) <= 100, c
    assert (i == 0) == ("boss" not in c) and (i == 0 or any(x.get("boss") == c["boss"] for x in c["stages"])), c   # nowy boss kontraktu
    assert bool(c.get("twins")) == any(x.get("twin") for x in c.get("stages", [])), c
    if "title" in rw_: assert 1 <= len(rw_["title"]) <= 16, c
    hel = coid[rw_["helmet"]] if "helmet" in rw_ else -1
    if hel >= 0: assert "helmet" in cos[hel], c
    L.append(f'    {{ {s(c["name"])}, {s(c["short"])}, {s(c["desc"])}, {first}, {n}, {pre_n if i == 0 else 0}, '
             f'core::career_unlock::{u["kind"]}, {u["value"]}, {c.get("gust", 0)}, {"true" if c.get("twins") else "false"}, '
             f'{eid[c["boss"]] if "boss" in c else -1}, {rw_.get("respect", 0)}, {s(rw_.get("title", ""))}, {hel} }},')
    first += n
assert first == len(all_st)
L += ["};", f"inline constexpr int career_count = {len(car)};",
      f"inline constexpr int career_twin_carry_max = {twin_max};   // bliźniak: ile problemów z pierwszej połowy przechodzi", ""]
es = d["estate"]["decor"]
ew = [x for x in es if "wins" in x]
assert 1 <= len(es) <= 12 and all(ew[i]["wins"] < ew[i + 1]["wins"] for i in range(len(ew) - 1))   # 12 miejsc na Osiedlu
assert all(("wins" in x) != ("inspector" in x) and len(x["name"]) <= 22 for x in es), es   # v0.21.52 cz. b: albo z poziomu inspektora
L.append("inline constexpr core::decor_def estate_decor[] = {   // ozdoby Osiedla (klatki w houses.bmp za pustą działką)")
L += [f'    {{ {s(x["name"])}, {x.get("wins", 0)}, {x.get("inspector", 0)} }},' for x in es] + ["};", f"inline constexpr int estate_decor_count = {len(es)};", ""]
# ------------------------------------------------------------------ v0.21.52 cz. b: poziom inspektora (#44), mistrzostwo zawodu (#45),
# stopnie inwestora (#48) - listy nagród za kolejne poziomy (xp = dośw. na poziom; stopnie: xp = stawka)
dcid = {x["id"]: i for i, x in enumerate(es)}
acid = {x["id"]: i for i, x in enumerate(arc)}
PREW = ["respect", "title", "helmet", "story", "decor", "keepsake_slot", "power", "weapon", "boon"]
def plevel(x, allowed):
    k = x["reward"]; assert k in allowed, x
    idx = coid[x["id"]] if k == "helmet" else (acid[x["id"]] if k == "story" else (dcid[x["id"]] if k == "decor" else -1))
    if k == "helmet": assert "helmet" in cos[idx], x
    if k == "title": assert 1 <= len(x["title"]) <= 16, x
    assert 0 <= x.get("value", 0) <= 100 and (k != "respect" or x["value"] > 0), x
    return f'{{ {x["xp"]}, core::progress_reward::{k}, {idx}, {x.get("value", 0)}, {s(x.get("title", ""))} }}'
ins = d["inspector"]; ix = ins["xp"]
assert 20 <= len(ins["levels"]) <= 40 and all(0 < x["xp"] < 2000 for x in ins["levels"])
assert all(ins["levels"][i]["xp"] <= ins["levels"][i + 1]["xp"] for i in range(len(ins["levels"]) - 1)), "progi rosną"
assert sum(1 for x in ins["levels"] if x["reward"] == "keepsake_slot") == 1 and len(ins["diffPct"]) == len(d["difficulties"])
for x in arc:   # wątek inspektora: poziom z nagrodą "story" w liście
    if x["trigger"] == "inspector": assert any(l_["reward"] == "story" and l_["id"] == x["id"] and i_ + 1 == x["value"] for i_, l_ in enumerate(ins["levels"])), x
for x in es:
    if "inspector" in x: assert any(l_["reward"] == "decor" and l_["id"] == x["id"] and i_ + 1 == x["inspector"] for i_, l_ in enumerate(ins["levels"])), x
L.append("inline constexpr core::progress_level inspector_levels[] = {   // poziom inspektora: dośw. na poziom i nagroda")
L += [f"    {plevel(x, PREW[:6])}," for x in ins["levels"]] + ["};"]
L += [f"inline constexpr int inspector_levels_count = {len(ins['levels'])};",
      f"inline constexpr int inspector_xp_run = {ix['run']}, inspector_xp_stage = {ix['stage']}, inspector_xp_boss = {ix['boss']};",
      f"inline constexpr int inspector_xp_elite = {ix['elite']}, inspector_xp_storeroom = {ix['storeroom']}, inspector_xp_win = {ix['win']};",
      f"inline constexpr int inspector_diff_pct[] = {{ {', '.join(map(str, ins['diffPct']))} }};",
      f"inline constexpr int inspector_migrate_run = {ins['migrate']['run']}, inspector_migrate_win = {ins['migrate']['win']};",
      f"inline constexpr int inspector_migrate_respect_pct = {ins['migrate']['respectPct']};", ""]
ma = d["mastery"]
assert len(ma["levels"]) == 10 and [x["reward"] for x in ma["levels"]].count("power") == 1 and len(ma["classes"]) == len(d["classes"])
for k_ in ("weapon", "boon", "helmet"): assert [x["reward"] for x in ma["levels"]].count(k_) == 1, k_
L.append("inline constexpr core::progress_level mastery_levels[] = {   // mistrzostwo zawodu 1-10: dośw. na poziom i nagroda")
L += [f"    {plevel(x, ['respect', 'power', 'weapon', 'boon', 'helmet'])}," for x in ma["levels"]] + ["};"]
bid2 = {x["id"]: i for i, x in enumerate(bo["list"])}
L.append("inline constexpr core::mastery_class_def mastery_classes[] = {   // wariant mocy, broń mistrza (cecha), premia mistrzostwa")
for i, x in enumerate(ma["classes"]):
    assert cid[x["class"]] == i, x
    pw, wp = x["power"], x["weapon"]
    assert len(pw["name"]) <= 18 and len(pw["desc"]) <= 26 and -4 <= pw["power"] <= 4 and -6 <= pw["cooldown"] <= 6 and (pw["power"], pw["cooldown"]) != (0, 0), x
    assert len(wp["name"]) <= 20 and wp["perk"]["effect"] == "crit" and 0 < wp["perk"]["value"] <= 5, x
    b_ = bo["list"][bid2[x["boon"]]]; assert b_.get("mastery") and cid2[b_["class"]] == i, x
    L.append(f'    {{ {s(pw["name"])}, {s(pw["desc"])}, {pw["power"]}, {pw["cooldown"]}, {s(wp["name"])}, {perk(wp["perk"])}, {bid2[x["boon"]]} }},')
L += ["};", f"inline constexpr int mastery_levels_count = {len(ma['levels'])};",
      f"inline constexpr int mastery_migrate_win = {ma['migrate']['win']}, mastery_migrate_class_win = {ma['migrate']['classWin']};", ""]
rk = d["investor"]["ranks"]
assert [x["stake"] for x in rk] == list(range(1, len(rk) + 1)) and len(rk) == sum(x["stake"] for x in d["investor"]["list"]), "stopień na każdą stawkę"
L.append("inline constexpr core::progress_level stake_ranks[] = {   // stopnie inwestora: nagroda za nowy najwyższy próg stawki (xp = stawka)")
L += [f"    {plevel(dict(x, xp=x['stake']), ['respect', 'title', 'helmet'])}," for x in rk] + ["};", f"inline constexpr int stake_ranks_count = {len(rk)};", ""]
pt = [(0, i + 1, x["title"]) for i, x in enumerate(ins["levels"]) if x["reward"] == "title"] + [(1, x["stake"], x["title"]) for x in rk if x["reward"] == "title"]
assert len({t[2] for t in pt} | {x["title"] for x in d["badges"] + d["contracts"]}) == len(pt) + len(d["badges"]) + len(d["contracts"]), "tytuły bez powtórzeń"
# ------------------------------------------------------------------ v0.21.52 cz. c: drzewko Szkoleń (#46), kolekcje (#49),
# zadania dnia i tygodnia (#50), seria dni (#51)
tr = m["tree"]; upid = {u["id"]: i for i, u in enumerate(m["upgrades"])}
brid = {b["id"]: i for i, b in enumerate(tr["branches"])}
assert len(tr["branches"]) == 3 and sorted(sum((b["upgrades"] for b in tr["branches"]), [])) == sorted(upid), "każde Szkolenie w jednej gałęzi"
assert 1 <= len(tr["nodes"]) <= 8 and 0 < tr["respecCost"] < 200   # profil: 2 bity na węzeł (uint16)
L.append("inline constexpr core::tree_branch tree_branches[] = {   // gałęzie drzewka: pień = Szkolenia (bity data::upgrades)")
L += [f'    {{ {s(b["name"])}, {sum(1 << upid[u] for u in b["upgrades"])} }},' for b in tr["branches"]] + ["};"]
L.append("inline constexpr core::tree_node tree_nodes[] = {   // węzły: głębokość (poziomy pnia gałęzi), koszt, 1 z 2 opcji")
for x in tr["nodes"]:
    b = tr["branches"][brid[x["branch"]]]
    assert 0 < x["depth"] <= 4 * len(b["upgrades"]) and 0 < x["cost"] < 1000 and len(x["options"]) == 2, x
    for o in x["options"]: assert o["effect"] in TREE_EFF and 0 < o["value"] <= 30 and len(o["name"]) <= 16 and len(o.get("short", o["name"])) <= 11 and len(o["desc"]) <= 26, o
    assert x["options"][0]["effect"] != x["options"][1]["effect"], x
    if any(o["effect"] == "first_hit" for o in x["options"]): assert all(o["value"] <= 15 for o in x["options"] if o["effect"] == "first_hit")
    op = ", ".join(f'{{ {s(o["name"])}, {s(o.get("short", o["name"]))}, {s(o["desc"])}, core::upgrade_effect::{o["effect"]}, {o["value"]} }}' for o in x["options"])
    L.append(f'    {{ {brid[x["branch"]]}, {x["depth"]}, {x["cost"]}, {{ {op} }} }},')
for i in range(len(tr["nodes"]) - 1):   # w gałęzi kolejne węzły głębiej
    a_, b_ = tr["nodes"][i], tr["nodes"][i + 1]
    assert a_["branch"] != b_["branch"] or a_["depth"] < b_["depth"], (a_, b_)
L += ["};", f"inline constexpr int tree_branches_count = {len(tr['branches'])};", f"inline constexpr int tree_nodes_count = {len(tr['nodes'])};",
      f"inline constexpr int tree_respec_cost = {tr['respecCost']};", ""]
kpid = {k["id"]: i for i, k in enumerate(ks["list"])}
def goal(x, allowed, xp):
    k = x["reward"]; assert k in allowed, x
    idx = coid[x["id"]] if k == "helmet" else (kpid[x["id"]] if k == "keepsake" else -1)
    if k == "helmet": assert "helmet" in cos[idx], x
    if k == "title": assert 1 <= len(x["title"]) <= 16, x
    assert 0 <= x.get("value", 0) <= 100 and (k != "respect" or x["value"] > 0), x
    return f'{{ {xp}, core::progress_reward::{k}, {idx}, {x.get("value", 0)}, {s(x.get("title", ""))} }}'
co = d["collections"]["sets"]
assert 1 <= len(co) <= 8   # profil: bity ogłoszonych kompletów (uint8)
L.append("inline constexpr core::collection_def collections[] = {   // kolekcje: komplet -> stała premia, tytuł albo kolor kasku")
for x in co:
    assert x["kind"] in {"act", "bosses", "decor"} and len(x["name"]) <= 16 and len(x["desc"]) <= 26 and 1 <= x["count"] <= 50, x
    if x["kind"] == "act":
        assert 0 <= x["act"] < len(d["acts"]), x
        mask = 0
        for st in d["stages"]:
            if st["act"] == x["act"]:
                for e in st["enemies"]: mask |= 1 << eid[e]
        kind = "kills"
    elif x["kind"] == "bosses":   # v0.21.52 cz. d: Karty bossów - bossowie Domu; "career" - bossowie nowych kontraktów
        bs = {eid[st["boss"]] for st in d["stages"] if "boss" in st}
        if x.get("career"): bs = {eid[c["boss"]] for c in car[1:]}
        mask = sum(1 << b for b in bs); kind = "bosses"
    else: mask = 0; kind = x["kind"]
    assert kind != "kills" or mask, x
    rw_ = x["reward"]
    pk = perk(rw_["perk"]) if rw_["reward"] == "perk" else "{ core::perk_effect::hp, 0 }"
    L.append(f'    {{ {s(x["name"])}, {s(x["desc"])}, core::collection_kind::{kind}, {mask}ull, {x["count"]}, '
             f'{goal(rw_, ["perk", "title", "helmet"], 0)}, {pk} }},')
L += ["};", f"inline constexpr int collections_count = {len(co)};", ""]
tk = d["tasks"]
TKIND = ["kills", "elites", "bosses", "stages", "brigade", "powers", "coffee", "combos", "storerooms", "events", "win", "win_no_shop"]
assert 3 <= len(tk["daily"]) <= 32 and 2 <= len(tk["weekly"]) <= 32
for lst, nm in ((tk["daily"], "daily_tasks"), (tk["weekly"], "weekly_tasks")):
    L.append(f"inline constexpr core::task_def {nm}[] = {{   // zadania: licznik z budów dnia / tygodnia, nagroda w Respekcie")
    for x in lst:
        assert x["kind"] in TKIND and 1 <= x["target"] <= 255 and 0 < x["respect"] <= 50 and len(x["name"]) <= 22, x   # postęp: uint8
        L.append(f'    {{ {s(x["name"])}, core::task_kind::{x["kind"]}, {x["target"]}, {x["respect"]} }},')
    L += ["};", f"inline constexpr int {nm}_count = {len(lst)};"]
trw = tk["rewards"]; srw = d["daily"]["streak"]
assert all(trw[i]["count"] < trw[i + 1]["count"] for i in range(len(trw) - 1)) and all(srw[i]["days"] < srw[i + 1]["days"] for i in range(len(srw) - 1))
for x in srw:
    if x["reward"] == "keepsake": assert ks["list"][kpid[x["id"]]].get("streak") == x["days"], x
for k_ in ks["list"]:
    if k_.get("streak"): assert any(x["reward"] == "keepsake" and x["id"] == k_["id"] for x in srw), k_
L.append("inline constexpr core::progress_level task_rewards[] = {   // nagrody za wykonane zadania łącznie (xp = liczba zadań)")
L += [f"    {goal(x, ['respect', 'title', 'helmet'], x['count'])}," for x in trw] + ["};", f"inline constexpr int task_rewards_count = {len(trw)};"]
L.append("inline constexpr core::progress_level streak_rewards[] = {   // seria dni budowy dnia (xp = dni)")
L += [f"    {goal(x, ['keepsake', 'title', 'helmet'], x['days'])}," for x in srw] + ["};", f"inline constexpr int streak_rewards_count = {len(srw)};", ""]
pt += [(2, i + 1, x["reward"]["title"]) for i, x in enumerate(co) if x["reward"]["reward"] == "title"]
pt += [(3, x["days"], x["title"]) for x in srw if x["reward"] == "title"] + [(4, x["count"], x["title"]) for x in trw if x["reward"] == "title"]
pt += [(5, i, c["reward"]["title"]) for i, c in enumerate(car) if "title" in c.get("reward", {})]   # cz. d: kontrakt wygrany
assert len({t[2] for t in pt} | {x["title"] for x in d["badges"] + d["contracts"]}) == len(pt) + len(d["badges"]) + len(d["contracts"]), "tytuły bez powtórzeń"
L.append("inline constexpr core::progress_title progress_titles[] = {   // tytuły: inspektor (0), stopnie inwestora (1), kolekcje (2), seria dni (3), zadania (4)")
L += [f"    {{ {s(t[2])}, {t[0]}, {t[1]} }}," for t in pt] + ["};", f"inline constexpr int progress_titles_count = {len(pt)};", ""]

L += [f"inline constexpr const char* version = {s(d['version'])};   // numer wersji (ekran tytułowy, changelog)", ""]
dh = d["damageHelp"]   # v0.21.50: Jak grać, strona Obrażenia (GBA i Godot)
assert len(dh) == 6 and all(len(x) <= 36 for x in dh), dh
L += ["inline constexpr const char* damage_help[] = {   // Jak grać: obrażenia broni w prostych słowach (rozpiska #26)"]
L += [f"    {s(t)}," for t in dh] + ["};", f"inline constexpr int damage_help_count = {len(dh)};", ""]
eh = d["extrasHelp"]   # v0.21.50 cz. 3: Jak grać - wydarzenia, ulepszenia, magazyn (GBA 2 strony po 6 linii, Godot jedna)
assert len(eh) == 12 and all(len(x) <= 31 for x in eh), eh   # GBA: 224 px
L += ["inline constexpr const char* extras_help[] = {   // Jak grać: wydarzenia z wyborem, ulepszanie narzędzia, magazyn"]
L += [f"    {s(t)}," for t in eh] + ["};", f"inline constexpr int extras_help_count = {len(eh)};", ""]
mh = d["metaHelp"]   # v0.21.50 cz. 4: Jak grać - podsumowanie, wyzwanie tygodnia, fabuła (GBA i Godot)
assert len(mh) == 6 and all(len(x) <= 31 for x in mh), mh
L += ["inline constexpr const char* meta_help[] = {   // Jak grać: podsumowanie budowy, wyzwanie tygodnia, fabuła"]
L += [f"    {s(t)}," for t in mh] + ["};", f"inline constexpr int meta_help_count = {len(mh)};", ""]
ch_ = d["careerHelp"]   # v0.21.52 cz. d: Jak grać - mapa kariery
assert len(ch_) == 7 and all(len(x) <= 31 for x in ch_), ch_
L += ["inline constexpr const char* career_help[] = {   // Jak grać: mapa kariery (kontrakty, odblokowanie, nagrody)"]
L += [f"    {s(t)}," for t in ch_] + ["};", f"inline constexpr int career_help_count = {len(ch_)};", ""]
gh_ = d["goalsHelp"]   # v0.21.52 cz. c: Jak grać - drzewko, kolekcje, zadania, seria dni
assert len(gh_) == 7 and all(len(x) <= 31 for x in gh_), gh_
L += ["inline constexpr const char* goals_help[] = {   // Jak grać: drzewko Szkoleń, kolekcje, zadania dnia, seria dni"]
L += [f"    {s(t)}," for t in gh_] + ["};", f"inline constexpr int goals_help_count = {len(gh_)};", ""]
ph_ = d["progressHelp"]   # v0.21.52 cz. b: Jak grać - poziom inspektora, mistrzostwo zawodu, stopnie inwestora
assert len(ph_) == 7 and all(len(x) <= 31 for x in ph_), ph_
L += ["inline constexpr const char* progress_help[] = {   // Jak grać: poziom inspektora i mistrzostwo zawodu"]
L += [f"    {s(t)}," for t in ph_] + ["};", f"inline constexpr int progress_help_count = {len(ph_)};", ""]
sh = d["secretsHelp"]   # v0.21.51 cz. 2: Jak grać - sekretne zlecenia (GBA strona 15, Godot)
assert len(sh) == 7 and all(len(x) <= 31 for x in sh), sh
L += ["inline constexpr const char* secrets_help[] = {   // Jak grać: sekretne zlecenia"]
L += [f"    {s(t)}," for t in sh] + ["};", f"inline constexpr int secrets_help_count = {len(sh)};", ""]
L += ["inline constexpr const char* tips[] = {   // rady kierownika na ekranie harmonogramu między etapami"]
L += [f"    {s(t)}," for t in d["tips"]] + ["};", f"inline constexpr int tips_count = {len(d['tips'])};", ""]
L += [f"inline constexpr int classes_count = {len(d['classes'])};",
      f"inline constexpr int stages_count = {len(d['stages'])};   // etapy Domu jednorodzinnego (kontrakt 0)",
      f"inline constexpr int all_stages_count = {len(all_st)};   // v0.21.52 cz. d: z etapami kolejnych kontraktów",
      f"inline constexpr int stage_looks_count = {looks_n};   // palety etapów (stage_palettes_N)",
      f"inline constexpr int tile_sets_count = {TILE_SETS};",
      f"inline constexpr int difficulties_count = {len(d['difficulties'])};",
      f"inline constexpr int default_difficulty = {d['defaultDifficulty']};",
      f"inline constexpr int ng_hp_pct_per_tier = {d['newGamePlus']['hpPctPerTier']};",
      f"inline constexpr int ng_dmg_bonus_per_tier = {d['newGamePlus']['dmgBonusPerTier']};",
      f"inline constexpr int ng_score_pct_per_tier = {d['newGamePlus']['scorePctPerTier']};", "", "}", ""]
# Każdy tekst gry musi dać się narysować fontem (ASCII + polskie znaki), inaczej Butano zatrzyma grę.
import re
FONT_CHARS = set(chr(c) for c in range(32, 127)) | set("ąćęłńóśźżĄĆĘŁŃÓŚŹŻ")
def check_font(o, where):
    if isinstance(o, str):
        badc = [ch for ch in o if ch not in FONT_CHARS and ch != "|"]
        assert not badc, f"znak spoza fontu {badc} w {where}: {o}"
    elif isinstance(o, dict):
        for k, v in o.items(): check_font(v, where + "." + k)
    elif isinstance(o, list):
        for i, v in enumerate(o): check_font(v, f"{where}[{i}]")
check_font(d, "game.json")
code = "\n".join(l.split("//")[0] for l in open(os.path.join(ROOT, "src", "main.cpp"), encoding="utf-8").read().splitlines())
for lit in re.findall(r'"((?:[^"\\]|\\.)*)"', code):
    check_font(lit, "src/main.cpp")

out = "\n".join(L)
path = os.path.join(ROOT, "include", "game_data.h")
if "--check" in sys.argv:
    ok = os.path.exists(path) and open(path, encoding="utf-8").read() == out
    print("game_data.h aktualny" if ok else "game_data.h NIEAKTUALNY - uruchom tools/gen_data.py"); sys.exit(0 if ok else 1)
open(path, "w", encoding="utf-8").write(out); print("zapisano", path)
