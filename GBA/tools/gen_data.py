#!/usr/bin/env python3
"""data/game.json -> include/game_data.h (constexpr tablice dla GBA i testów na PC).
--check: sprawdza, czy nagłówek jest aktualny (CI)."""
import json, os, sys
ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
d = json.load(open(os.path.join(ROOT, "data", "game.json"), encoding="utf-8"))
STAT = {"strength": "stat::str", "agility": "stat::agi", "intelligence": "stat::intel"}
wid = {w["id"]: i for i, w in enumerate(d["weapons"])}
eid = {e["id"]: i for i, e in enumerate(d["enemies"])}
def s(x): return '"' + x.replace('"', '\\"') + '"'
L = ["// WYGENEROWANE przez tools/gen_data.py z data/game.json - nie edytuj ręcznie.",
     "#pragma once", '#include "core_types.h"', "", "namespace data {", ""]
L.append("inline constexpr core::weapon_def weapons[] = {")
for w in d["weapons"]:
    assert w["minDamage"] <= w["maxDamage"] and w["range"] >= 1, w
    L.append(f'    {{ {s(w["name"])}, {w["minDamage"]}, {w["maxDamage"]}, {w["range"]}, core::{STAT[w["scalesWith"]]} }},')
L.append("};\n")
L.append("inline constexpr core::class_def classes[] = {")
for c in d["classes"]:
    ab = c["ability"]
    assert len(ab["desc"]) <= 23, ab   # mieści się w banerze obok ikony
    assert ab["effect"] in {"stun", "wall", "volley", "chain", "flush", "spin"}, ab
    L.append(f'    {{ {s(c["name"])}, {s(c["desc"])}, {c["maxHealth"]}, {c["strength"]}, {c["agility"]}, '
             f'{c["intelligence"]}, {c["defense"]}, {c["luck"]}, {wid[c["weapon"]]}, {c["frame"]}, '
             f'{s(ab["name"])}, {s(ab["desc"])}, core::ability_effect::{ab["effect"]}, {ab["cooldown"]} }},')
L.append("};\n")
L.append("inline constexpr core::enemy_def enemies[] = {")
for e in d["enemies"]:
    L.append(f'    {{ {s(e["name"])}, {s(e["desc"])}, {e["maxHealth"]}, {e["minDamage"]}, {e["maxDamage"]}, {e["defense"]}, '
             f'{e["sight"]}, {e["score"]}, {e["frame"]}, {"true" if e.get("slam") else "false"}, '
             f'core::status_effect::{e.get("onHit", {}).get("status", "none")}, {e.get("onHit", {}).get("chancePct", 0)}, '
             f'{e.get("onHit", {}).get("turns", 0)} }},')
L.append("};\n")
L.append("inline constexpr core::stage_def stages[] = {")
for st in d["stages"]:
    pool = [eid[x] for x in st["enemies"]] + [-1] * (4 - len(st["enemies"]))
    boss = eid[st["boss"]] if "boss" in st else -1
    L.append(f'    {{ {s(st["name"])}, {{ {", ".join(map(str, pool))} }}, {len(st["enemies"])}, {st["count"]}, {boss}, '
             f'{st.get("hpPct", 100)}, {st.get("dmgBonus", 0)}, {st["act"]} }},')
L.append("};\n")
L.append("inline constexpr core::difficulty_def difficulties[] = {")
for df in d["difficulties"]:
    L.append(f'    {{ {s(df["name"])}, {df["hpPct"]}, {df["dmgBonus"]}, {df["scorePct"]} }},')
L.append("};\n")
m = d["meta"]
cid = {c["id"]: i for i, c in enumerate(d["classes"])}
EFF = {"hp", "def", "dmg", "coffee", "pickups", "luck", "craft"}
L.append("inline constexpr core::upgrade_def upgrades[] = {")
for u in m["upgrades"]:
    assert u["effect"] in EFF and 1 <= len(u["costs"]) <= 4, u
    costs = u["costs"] + [0] * (4 - len(u["costs"]))
    L.append(f'    {{ {s(u["name"])}, {s(u["desc"])}, core::upgrade_effect::{u["effect"]}, {u["value"]}, '
             f'{len(u["costs"])}, {{ {", ".join(map(str, costs))} }} }},')
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
for ai in range(len(acts)):   # każdy akt kończy się etapem z bossem
    last = max(i for i, st in enumerate(d["stages"]) if st["act"] == ai)
    assert "boss" in d["stages"][last], f"akt {ai} bez bossa"
L.append("inline constexpr core::act_def acts[] = {")
L += [f'    {{ {s(a["name"])}, {a["bonusPerStage"]}, {a["bonusPerKill"]} }},' for a in acts]
L.append("};")
L.append("inline constexpr core::shop_item_def hurtownia[] = {")
for it in d["hurtownia"]:
    assert it["effect"] in {"heal", "gear", "tool", "maxhp", "ability"} and len(it["desc"]) <= 34, it
    L.append(f'    {{ {s(it["name"])}, {s(it["desc"])}, {it["price"]}, core::shop_effect::{it["effect"]} }},')
L.append("};")
stt = d["statuses"]
L.append("inline constexpr core::status_def statuses[] = {   // indeks = core::status_effect")
L.append('    { "", "", "" },')
for k in ("poison", "shock", "slip", "paper"):
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
      f"inline constexpr int cash_per_score = {d['cash']['perScore']};"]
L += [f"inline constexpr int enemy_{e['id']} = {i};" for i, e in enumerate(d["enemies"])] + [""]
L.append("inline constexpr core::badge_def badges[] = {")
for b in d["badges"]:
    L.append(f'    {{ {s(b["name"])}, {s(b["desc"])}, {b["xp"]} }},')
L.append("};\n")
bid = {b["id"]: i for i, b in enumerate(d["badges"])}
L += [f"inline constexpr int badges_count = {len(d['badges'])};",
      f"inline constexpr int enemies_count = {len(d['enemies'])};"]
L += [f"inline constexpr int badge_{k} = {v};" for k, v in bid.items()] + [""]
L.append("inline constexpr core::tool_def tools[] = {")
for t in m["tools"]:
    L.append(f'    {{ {wid[t["weapon"]]}, {t["cost"]} }},')
L.append("};\n")
dr = d["drops"]
start_tools = sum(1 << i for i, t in enumerate(m["tools"]) if t["cost"] == 0)
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
    assert t["effect"] in {"luck", "crit", "poison_res", "sight", "cooldown", "str", "agi", "intel"} and len(t["short"]) <= 9 and len(t["name"]) <= 22, t
    L.append(f'    {{ {s(t["name"])}, {s(t["short"])}, core::trait_effect::{t["effect"]}, {t["value"]} }},')
L.append("};")
L.append(f"inline constexpr int gear_traits_count = {len(eq['traits'])};")
L.append(f"inline constexpr int gear_decline_xp = {eq['declineXp']};")
rr = eq["rarityRoll"]
L += [f"inline constexpr int gear_slots_count = {len(eq['slots'])};",
      f"inline constexpr int gear_solid_from = {rr['solidFrom']};",
      f"inline constexpr int gear_brand_from = {rr['brandFrom']};",
      f"inline constexpr int gear_stage_bonus = {rr['stageBonus']};", ""]
start_mask = sum(1 << cid[c] for c in m["startClasses"])
L += [f"inline constexpr int upgrades_count = {len(m['upgrades'])};",
      f"inline constexpr int xp_per_kill = {m['xpPerKill']};",
      f"inline constexpr int xp_per_stage = {m['xpPerStage']};",
      f"inline constexpr int xp_boss = {m['xpBoss']};",
      f"inline constexpr int start_classes_mask = {start_mask};",
      f"inline constexpr int class_cost = {m['classCost']};",
      f"inline constexpr int hard_cost = {m['hardCost']};", ""]
hl = d["heroLevels"]
L += [f"inline constexpr int level_thresholds[] = {{ {', '.join(map(str, hl['thresholds']))} }};",
      f"inline constexpr int max_hero_level = {len(hl['thresholds']) + 1};",
      f"inline constexpr int hp_per_level = {hl['hpPerLevel']};",
      f"inline constexpr int dmg_levels_mask = {sum(1 << l for l in hl['dmgLevels'])};",
      f"inline constexpr int def_levels_mask = {sum(1 << l for l in hl['defLevels'])};", ""]
L += [f"inline constexpr const char* version = {s(d['version'])};   // numer wersji (ekran tytułowy, changelog)", ""]
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
