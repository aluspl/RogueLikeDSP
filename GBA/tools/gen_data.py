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
    L.append(f'    {{ {s(w["name"])}, {w["minDamage"]}, {w["maxDamage"]}, {w["range"]}, core::{STAT[w["scalesWith"]]}, '
             f'core::element::{ELEM[w.get("element", "none")]} }},')
L.append("};\n")
L.append("inline constexpr core::class_def classes[] = {")
for c in d["classes"]:
    ab = c["ability"]
    assert len(ab["desc"]) <= 23, ab   # mieści się w banerze obok ikony
    assert ab["effect"] in {"stun", "wall", "volley", "chain", "flush", "spin", "line", "splash", "ram"}, ab
    assert c.get("passive", "none") in {"none", "windproof", "push"}, c
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
L.append("inline constexpr core::stage_def stages[] = {")
for st in d["stages"]:
    pool = [eid[x] for x in st["enemies"]] + [-1] * (4 - len(st["enemies"]))
    boss = eid[st["boss"]] if "boss" in st else -1
    L.append(f'    {{ {s(st["name"])}, {{ {", ".join(map(str, pool))} }}, {len(st["enemies"])}, {st["count"]}, {boss}, '
             f'{st.get("hpPct", 100)}, {st.get("dmgBonus", 0)}, {st["act"]}, {st.get("cost", 0)} }},')
L.append("};\n")
L.append("inline constexpr core::difficulty_def difficulties[] = {")
for df in d["difficulties"]:
    L.append(f'    {{ {s(df["name"])}, {df["hpPct"]}, {df["dmgBonus"]}, {df["scorePct"]} }},')
L.append("};\n")
m = d["meta"]
cid = {c["id"]: i for i, c in enumerate(d["classes"])}
EFF = {"hp", "def", "dmg", "coffee", "pickups", "luck", "craft", "dmg_pct", "taken_pct"}
L.append("inline constexpr core::upgrade_def upgrades[] = {")
for u in m["upgrades"]:
    assert u["effect"] in EFF and 1 <= len(u["costs"]) <= 4, u
    costs = u["costs"] + [0] * (4 - len(u["costs"]))
    L.append(f'    {{ {s(u["name"])}, {s(u["desc"])}, core::upgrade_effect::{u["effect"]}, {u["value"]}, '
             f'{len(u["costs"])}, {{ {", ".join(map(str, costs))} }}, {u.get("refund", 0)}, {u.get("resetRefund", 0)} }},')
L.append("};\n")
def story(m):
    lines = m["text"].split("|")
    assert len(lines) <= 3 and all(len(l) <= 25 for l in lines), m   # dymek: 3 linie x 25 znaków
    lines += [""] * (3 - len(lines))
    return f'{{ {s(m["from"])}, {{ {", ".join(s(l) for l in lines)} }} }}'
st = d["story"]
assert len(st["stages"]) == len(d["stages"])
L.append("inline constexpr core::story_msg story_stages[] = {")
L += [f"    {story(m)}," for m in st["stages"]]
L.append("};")
L += [f"inline constexpr core::story_msg story_{k} = {story(st[k])};" for k in ("win", "lose", "ngplus", "prologue")]
L.append("inline constexpr const char* prologue_captions[] = { " + ", ".join(s(c) for c in st["prologueCaptions"]) + " };")
L.append("")
acts = d["acts"]
path_more = max(0, max(x["enemies"] for x in d["paths"]["list"]))   # ścieżka może dodać problemy
for st in d["stages"]:   # boss z wezwaniami: etap + boss + wezwani mieszczą się w core::max_enemies (12)
    assert st["count"] + path_more <= 12, st   # core::max_enemies 16: miejsce na dwa podziały (zachowanie "splits")
    if "boss" in st:
        assert st["count"] + path_more + 1 + d["enemies"][eid[st["boss"]]].get("summon", {}).get("max", 0) <= 12, st
for ai in range(len(acts)):   # każdy akt kończy się etapem z bossem
    last = max(i for i, st in enumerate(d["stages"]) if st["act"] == ai)
    assert "boss" in d["stages"][last], f"akt {ai} bez bossa"
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
PERKS = {"hp", "def", "dmg", "luck", "cooldown", "sight", "thermos", "tool_pct", "xp_pct", "cash", "crit", "coffee"}
def perk(pk):
    assert pk["effect"] in PERKS and -128 <= pk["value"] <= 127, pk
    return f'{{ core::perk_effect::{pk["effect"]}, {pk["value"]} }}'
L.append("inline constexpr core::badge_def badges[] = {   // perk = uprawnienie: trwała premia na każdą budowę")
for b in d["badges"]:
    L.append(f'    {{ {s(b["name"])}, {s(b["desc"])}, {b["xp"]}, {perk(b["perk"])} }},')
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
    assert k.get("start") or "badge" in k or unlocked_by, f"pamiątka {k['id']} bez sposobu odblokowania"
    L.append(f'    {{ {s(k["name"])}, {s(k["desc"])}, core::perk_effect::{k["effect"]}, {{ {", ".join(map(str, k["values"]))} }}, '
             f'{bid[k["badge"]] if "badge" in k else -1}, {"true" if k.get("start") else "false"} }},')
L.append("};")
L += [f"inline constexpr int keepsakes_count = {len(ks['list'])};",
      f"inline constexpr int keepsake_rank_runs[] = {{ {ks['rankRuns'][0]}, {ks['rankRuns'][1]} }};", ""]
L.append("inline constexpr core::contract_def contracts[] = {   // zlecenia: długofalowe cele z licznikami w profilu")
for c in d["contracts"]:
    assert c["kind"] in KINDS and len(c["name"]) <= 16 and len(c["desc"]) <= 30 and 0 < c["target"] < 30000, c
    L.append(f'    {{ {s(c["name"])}, {s(c["desc"])}, core::contract_kind::{c["kind"]}, {c["target"]}, {c["xp"]}, '
             f'{kid[c["keepsake"]] if "keepsake" in c else -1} }},')
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
all_stages = (1 << len(d["stages"])) - 1
assert wt["list"][0]["effect"] == "none" and len(d["stages"]) <= 12   # pierwsza = bez skutku (domyślna)
L.append("inline constexpr core::weather_def weather[] = {   // pogoda dnia: losowana na starcie etapu")
for w in wt["list"]:
    assert w["effect"] in {"none", "heat", "frost", "wind", "rain"} and len(w["short"]) <= 10, w
    assert len(w["name"]) <= 12 and len(w["info"]) <= 30 and 0 < w["weight"] < 128 and 0 <= w["value"] < 128, w
    assert w["effect"] not in ("frost", "rain") or w["value"] >= 2, w
    mask = sum(1 << i for i in w["stages"]) if "stages" in w else all_stages
    L.append(f'    {{ {s(w["name"])}, {s(w["short"])}, {s(w["info"])}, core::weather_effect::{w["effect"]}, {w["value"]}, '
             f'{w["weight"]}, {"true" if w["bad"] else "false"}, {mask} }},')
L.append("};")
for si in range(len(d["stages"])):   # każdy etap ma jakąś pogodę do wylosowania
    assert any(("stages" not in w) or si in w["stages"] for w in wt["list"]), si
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
    assert not t.get("reward") or t["cost"] == 0, t   # narzędzie z nagrody nie jest na sprzedaż
    L.append(f'    {{ {wid[t["weapon"]]}, {t["cost"]}, {"true" if t.get("reward") else "false"} }},')
L.append("};\n")
dr = d["drops"]
start_tools = sum(1 << i for i, t in enumerate(m["tools"]) if t["cost"] == 0 and not t.get("reward"))
assert len(m["tools"]) <= 8   # profil: bitmaska uint8
L += [f"inline constexpr int tools_count = {len(m['tools'])};",
      f"inline constexpr int start_tools_mask = {start_tools};",
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
assert all(not d["classes"][cid[c]].get("reward") for c in m["startClasses"])
assert all(i < 8 for i, c in enumerate(d["classes"]) if not c.get("reward")), "zawody do kupienia: bitmaska uint8 w profilu"
assert len(d["classes"]) <= 12 and all(0 <= c["frame"] < 64 for c in d["classes"])
L += [f"inline constexpr int upgrades_count = {len(m['upgrades'])};",
      f"inline constexpr int xp_per_kill = {m['xpPerKill']};",
      f"inline constexpr int xp_per_stage = {m['xpPerStage']};",
      f"inline constexpr int xp_boss = {m['xpBoss']};",
      f"inline constexpr int start_classes_mask = {start_mask};",
      f"inline constexpr int reward_classes_mask = {sum(1 << i for i, c in enumerate(d['classes']) if c.get('reward'))};",
      f"inline constexpr int class_cost = {m['classCost']};",
      f"inline constexpr int hard_cost = {m['hardCost']};", ""]
hl = d["heroLevels"]
L += [f"inline constexpr int level_thresholds[] = {{ {', '.join(map(str, hl['thresholds']))} }};",
      f"inline constexpr int max_hero_level = {len(hl['thresholds']) + 1};",
      f"inline constexpr int hp_per_level = {hl['hpPerLevel']};",
      f"inline constexpr int dmg_levels_mask = {sum(1 << l for l in hl['dmgLevels'])};",
      f"inline constexpr int def_levels_mask = {sum(1 << l for l in hl['defLevels'])};", ""]
rs = d["respect"]
REFF = {"dmg_pct", "taken_pct", "gear_pct", "crit", "dodge", "coffee_pct", "thermos", "cooldown", "cash", "xp_pct",
        "brigade_pct", "sight", "shop_pct", "mats_pct", "second_chance", "reroll"}
assert 1 <= len(rs["upgrades"]) <= 16 and all(0 < rs[k] < 50 for k in ("stage", "boss", "actBoss", "final"))
L.append("inline constexpr core::respect_def respect[] = {   // Respekt: stałe ulepszenia z rangami")
for x in rs["upgrades"]:
    assert x["effect"] in REFF and 1 <= len(x["values"]) == len(x["costs"]) <= 5 and len(x["name"]) <= 16, x
    assert all(0 < v < 128 for v in x["values"]) and x["values"] == sorted(x["values"]) and all(0 < c < 1000 for c in x["costs"]), x
    vals = x["values"] + [0] * (5 - len(x["values"])); costs = x["costs"] + [0] * (5 - len(x["costs"]))
    L.append(f'    {{ {s(x["name"])}, {s(x["desc"])}, core::respect_effect::{x["effect"]}, {len(x["values"])}, '
             f'{{ {", ".join(map(str, vals))} }}, {{ {", ".join(map(str, costs))} }} }},')
L.append("};")
L += [f"inline constexpr int push_chance_pct = {d['passives']['pushChancePct']};   // Operator koparki: cios wręcz odpycha",
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
UNL = ["respect", "daily", "investor", "act0", "class"]
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
    L.append(f'    {{ {s(x["name"])}, {s(x["desc"])}, {RAR[x["rarity"]]}, {tagmask(x["tags"])}, core::boon_effect::{x["effect"]}, '
             f'{x["value"]}, {cid2[x["class"]] if "class" in x else -1} }},')
L.append("};")
for c in range(len(d["classes"])):   # każdy zawód ma 1-2 własne premie
    n = sum(1 for x in bo["list"] if cid2.get(x.get("class"), -1) == c)
    assert 1 <= n <= 2, (d["classes"][c]["id"], n)
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

L += [f"inline constexpr const char* version = {s(d['version'])};   // numer wersji (ekran tytułowy, changelog)", ""]
dh = d["damageHelp"]   # v0.21.50: Jak grać, strona Obrażenia (GBA i Godot)
assert len(dh) == 6 and all(len(x) <= 36 for x in dh), dh
L += ["inline constexpr const char* damage_help[] = {   // Jak grać: obrażenia broni w prostych słowach (rozpiska #26)"]
L += [f"    {s(t)}," for t in dh] + ["};", f"inline constexpr int damage_help_count = {len(dh)};", ""]
L += ["inline constexpr const char* tips[] = {   // rady kierownika na ekranie harmonogramu między etapami"]
L += [f"    {s(t)}," for t in d["tips"]] + ["};", f"inline constexpr int tips_count = {len(d['tips'])};", ""]
L += [f"inline constexpr int classes_count = {len(d['classes'])};",
      f"inline constexpr int stages_count = {len(d['stages'])};",
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
