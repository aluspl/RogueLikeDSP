// WYGENEROWANE przez tools/gen_data.py z data/game.json - nie edytuj ręcznie.
#pragma once
#include "core_types.h"

namespace data {

inline constexpr core::weapon_def weapons[] = {
    { "Dziennik budowy", 2, 4, 2, core::stat::intel },
    { "Kielnia", 3, 5, 1, core::stat::str },
    { "Gwoździarka", 2, 3, 3, core::stat::agi },
    { "Próbnik napięcia", 3, 5, 2, core::stat::intel },
    { "Klucz nastawny", 3, 6, 1, core::stat::str },
    { "Szlifierka", 3, 7, 1, core::stat::agi },
};

inline constexpr core::class_def classes[] = {
    { "Kierownik budowy", "Trzyma harmonogram w ryzach.", 30, 3, 3, 5, 3, 0, 0 },
    { "Murarz", "Twardy jak pustak.", 32, 5, 2, 1, 4, 1, 1 },
    { "Cieśla-dekarz", "Gwoździe wbija z daleka.", 30, 4, 4, 1, 3, 2, 2 },
    { "Elektryk", "Wie, gdzie jest faza.", 24, 2, 4, 5, 2, 3, 3 },
    { "Hydraulik", "Żaden przeciek mu nie straszny.", 32, 4, 3, 3, 3, 4, 4 },
    { "Glazurnik", "Precyzja co do fugi.", 30, 3, 5, 2, 3, 5, 5 },
};

inline constexpr core::enemy_def enemies[] = {
    { "Przeciek", 6, 1, 3, 0, 6, 10, 6 },
    { "Zwarcie", 5, 2, 4, 0, 7, 12, 7 },
    { "Pleśń", 9, 1, 2, 1, 4, 10, 8 },
    { "Kornik", 7, 1, 3, 1, 5, 10, 9 },
    { "Papierologia", 12, 1, 2, 2, 4, 15, 10 },
    { "Opóźniona dostawa", 10, 2, 4, 1, 6, 15, 11 },
    { "Ulewa", 8, 2, 3, 0, 8, 12, 12 },
    { "Przekroczony budżet", 14, 2, 5, 2, 6, 25, 13 },
    { "Nieprzekraczalny Termin", 40, 3, 6, 3, 12, 200, 14 },
};

inline constexpr core::stage_def stages[] = {
    { "Fundamenty", { 0, 3, -1, -1 }, 2, 5, -1, 100, 0 },
    { "Stan surowy", { 4, 5, 3, -1 }, 3, 6, -1, 105, 0 },
    { "Dach", { 6, 3, 0, -1 }, 3, 7, -1, 110, 0 },
    { "Instalacje", { 1, 0, 2, -1 }, 3, 8, -1, 115, 0 },
    { "Wykończenie i odbiór", { 2, 7, -1, -1 }, 2, 6, 8, 120, 0 },
};

inline constexpr core::difficulty_def difficulties[] = {
    { "Łatwy", 80, -1, 50 },
    { "Normalny", 100, 0, 100 },
    { "Trudny", 125, 0, 150 },
};

inline constexpr core::upgrade_def upgrades[] = {
    { "Kondycja", "+4 HP na start", core::upgrade_effect::hp, 4, 3, { 10, 20, 35, 0 } },
    { "Szkolenie BHP", "+1 obrona", core::upgrade_effect::def, 1, 2, { 15, 30, 0, 0 } },
    { "Kurs fachowy", "+1 obrażenia", core::upgrade_effect::dmg, 1, 2, { 20, 40, 0, 0 } },
    { "Lepszy termos", "Kawa leczy +4 HP", core::upgrade_effect::coffee, 4, 2, { 10, 20, 0, 0 } },
    { "Dostawy", "+1 znajdźka na etap", core::upgrade_effect::pickups, 1, 2, { 15, 30, 0, 0 } },
};

inline constexpr int upgrades_count = 5;
inline constexpr int xp_per_kill = 1;
inline constexpr int xp_per_stage = 5;
inline constexpr int xp_boss = 20;
inline constexpr int start_classes_mask = 19;
inline constexpr int class_cost = 25;
inline constexpr int hard_cost = 40;

inline constexpr int classes_count = 6;
inline constexpr int stages_count = 5;
inline constexpr int difficulties_count = 3;
inline constexpr int default_difficulty = 1;
inline constexpr int ng_hp_pct_per_tier = 20;
inline constexpr int ng_dmg_bonus_per_tier = 1;
inline constexpr int ng_score_pct_per_tier = 50;

}
