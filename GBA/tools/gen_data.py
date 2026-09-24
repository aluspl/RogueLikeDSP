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
    assert ab["effect"] in {"stun", "wall", "volley", "chain", "heal", "spin"}, ab
    L.append(f'    {{ {s(c["name"])}, {s(c["desc"])}, {c["maxHealth"]}, {c["strength"]}, {c["agility"]}, '
             f'{c["intelligence"]}, {c["defense"]}, {wid[c["weapon"]]}, {c["frame"]}, '
             f'{s(ab["name"])}, {s(ab["desc"])}, core::ability_effect::{ab["effect"]}, {ab["cooldown"]} }},')
L.append("};\n")
L.append("inline constexpr core::enemy_def enemies[] = {")
for e in d["enemies"]:
    L.append(f'    {{ {s(e["name"])}, {s(e["desc"])}, {e["maxHealth"]}, {e["minDamage"]}, {e["maxDamage"]}, {e["defense"]}, '
             f'{e["sight"]}, {e["score"]}, {e["frame"]} }},')
L.append("};\n")
L.append("inline constexpr core::stage_def stages[] = {")
for st in d["stages"]:
    pool = [eid[x] for x in st["enemies"]] + [-1] * (4 - len(st["enemies"]))
    boss = eid[st["boss"]] if "boss" in st else -1
    L.append(f'    {{ {s(st["name"])}, {{ {", ".join(map(str, pool))} }}, {len(st["enemies"])}, {st["count"]}, {boss}, '
             f'{st.get("hpPct", 100)}, {st.get("dmgBonus", 0)} }},')
L.append("};\n")
L.append("inline constexpr core::difficulty_def difficulties[] = {")
for df in d["difficulties"]:
    L.append(f'    {{ {s(df["name"])}, {df["hpPct"]}, {df["dmgBonus"]}, {df["scorePct"]} }},')
L.append("};\n")
m = d["meta"]
cid = {c["id"]: i for i, c in enumerate(d["classes"])}
EFF = {"hp", "def", "dmg", "coffee", "pickups"}
L.append("inline constexpr core::upgrade_def upgrades[] = {")
for u in m["upgrades"]:
    assert u["effect"] in EFF and 1 <= len(u["costs"]) <= 4, u
    costs = u["costs"] + [0] * (4 - len(u["costs"]))
    L.append(f'    {{ {s(u["name"])}, {s(u["desc"])}, core::upgrade_effect::{u["effect"]}, {u["value"]}, '
             f'{len(u["costs"])}, {{ {", ".join(map(str, costs))} }} }},')
L.append("};\n")
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
      f"inline constexpr int drop_weights[] = {{ {dr['weights']['coffee']}, {dr['weights']['helmet']}, {dr['weights']['plan']}, {dr['weights']['tool']} }};", ""]
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
L += [f"inline constexpr int classes_count = {len(d['classes'])};",
      f"inline constexpr int stages_count = {len(d['stages'])};",
      f"inline constexpr int difficulties_count = {len(d['difficulties'])};",
      f"inline constexpr int default_difficulty = {d['defaultDifficulty']};",
      f"inline constexpr int ng_hp_pct_per_tier = {d['newGamePlus']['hpPctPerTier']};",
      f"inline constexpr int ng_dmg_bonus_per_tier = {d['newGamePlus']['dmgBonusPerTier']};",
      f"inline constexpr int ng_score_pct_per_tier = {d['newGamePlus']['scorePctPerTier']};", "", "}", ""]
out = "\n".join(L)
path = os.path.join(ROOT, "include", "game_data.h")
if "--check" in sys.argv:
    ok = os.path.exists(path) and open(path, encoding="utf-8").read() == out
    print("game_data.h aktualny" if ok else "game_data.h NIEAKTUALNY - uruchom tools/gen_data.py"); sys.exit(0 if ok else 1)
open(path, "w", encoding="utf-8").write(out); print("zapisano", path)
