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
    L.append(f'    {{ {s(c["name"])}, {s(c["desc"])}, {c["maxHealth"]}, {c["strength"]}, {c["agility"]}, '
             f'{c["intelligence"]}, {c["defense"]}, {wid[c["weapon"]]}, {c["frame"]} }},')
L.append("};\n")
L.append("inline constexpr core::enemy_def enemies[] = {")
for e in d["enemies"]:
    L.append(f'    {{ {s(e["name"])}, {e["maxHealth"]}, {e["minDamage"]}, {e["maxDamage"]}, {e["defense"]}, '
             f'{e["sight"]}, {e["score"]}, {e["frame"]} }},')
L.append("};\n")
L.append("inline constexpr core::stage_def stages[] = {")
for st in d["stages"]:
    pool = [eid[x] for x in st["enemies"]] + [-1] * (4 - len(st["enemies"]))
    boss = eid[st["boss"]] if "boss" in st else -1
    L.append(f'    {{ {s(st["name"])}, {{ {", ".join(map(str, pool))} }}, {len(st["enemies"])}, {st["count"]}, {boss} }},')
L.append("};\n")
L += [f"inline constexpr int classes_count = {len(d['classes'])};",
      f"inline constexpr int stages_count = {len(d['stages'])};", "", "}", ""]
out = "\n".join(L)
path = os.path.join(ROOT, "include", "game_data.h")
if "--check" in sys.argv:
    ok = os.path.exists(path) and open(path, encoding="utf-8").read() == out
    print("game_data.h aktualny" if ok else "game_data.h NIEAKTUALNY - uruchom tools/gen_data.py"); sys.exit(0 if ok else 1)
open(path, "w", encoding="utf-8").write(out); print("zapisano", path)
