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
    { "Łom", 3, 6, 1, core::stat::str },
    { "Wkrętarka", 2, 5, 2, core::stat::agi },
    { "Poziomica laserowa", 2, 4, 4, core::stat::intel },
    { "Młot wyburzeniowy", 5, 8, 1, core::stat::str },
};

inline constexpr core::class_def classes[] = {
    { "Kierownik budowy", "Trzyma harmonogram w ryzach.", 30, 3, 3, 5, 3, 0, 0, "Odprawa", "Ogłusza widocznych", core::ability_effect::stun, 15 },
    { "Murarz", "Twardy jak pustak.", 32, 5, 2, 1, 4, 1, 1, "Ścianka", "Mur przed wrogiem", core::ability_effect::wall, 15 },
    { "Cieśla-dekarz", "Gwoździe wbija z daleka.", 30, 4, 4, 1, 3, 2, 2, "Seria", "Gwoździe we wszystkich", core::ability_effect::volley, 12 },
    { "Elektryk", "Wie, gdzie jest faza.", 24, 2, 4, 5, 2, 3, 3, "Łańcuch", "Prąd skacze po celach", core::ability_effect::chain, 12 },
    { "Hydraulik", "Żaden przeciek mu nie straszny.", 32, 4, 3, 3, 3, 4, 4, "Zawór", "Odpycha wrogów i leczy", core::ability_effect::flush, 20 },
    { "Glazurnik", "Precyzja co do fugi.", 30, 3, 5, 2, 3, 5, 5, "Wirówka", "Tnie wszystkich dookoła", core::ability_effect::spin, 10 },
};

inline constexpr core::enemy_def enemies[] = {
    { "Przeciek", "Kapie tam, gdzie nie powinno", 6, 1, 3, 0, 6, 10, 6 },
    { "Zwarcie", "Iskrzy przy każdej okazji", 5, 2, 4, 0, 7, 12, 7 },
    { "Pleśń", "Lubi wilgoć i zimne ściany", 9, 1, 2, 1, 4, 10, 8 },
    { "Kornik", "Drąży więźbę po cichu", 7, 1, 3, 1, 5, 10, 9 },
    { "Papierologia", "Brakuje jednej pieczątki", 12, 1, 2, 2, 4, 15, 10 },
    { "Opóźniona dostawa", "Będzie jutro. Na pewno.", 10, 2, 4, 1, 6, 15, 11 },
    { "Ulewa", "Zawsze tuż przed dachem", 8, 2, 3, 0, 8, 12, 12 },
    { "Przekroczony budżet", "Rośnie szybciej niż mury", 14, 2, 5, 2, 6, 25, 13 },
    { "Nieprzekraczalny Termin", "Nieprzesuwalny. Podobno.", 40, 3, 6, 3, 12, 200, 14 },
};

inline constexpr core::stage_def stages[] = {
    { "Fundamenty", { 0, 3, -1, -1 }, 2, 5, -1, 100, 0 },
    { "Stan surowy", { 4, 5, 3, -1 }, 3, 6, -1, 110, 0 },
    { "Dach", { 6, 3, 0, -1 }, 3, 7, -1, 120, 0 },
    { "Instalacje", { 1, 0, 2, -1 }, 3, 8, -1, 130, 0 },
    { "Wykończenie i odbiór", { 2, 7, -1, -1 }, 2, 6, 8, 140, 0 },
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

inline constexpr core::story_msg story_stages[] = {
    { "Anna Nowak", { "Działka nasza! Liczę na", "mocne fundamenty. Uważaj", "na wilgoć w wykopie." } },
    { "Kierownik Marek", { "Stal jest, papierów brak.", "Papierologia już czeka.", "Trzymaj się planu!" } },
    { "Anna Nowak", { "Prognoza: ulewa. Zdążysz", "z dachem przed deszczem?", "Kornik też nie śpi." } },
    { "Kierownik Marek", { "Prąd i woda w jednych", "ścianach. Co może pójść", "nie tak? ;)" } },
    { "Anna Nowak", { "Już widzę nasz salon.", "Tylko ten Termin... Dasz", "radę, mamy harmonogram!" } },
};
inline constexpr core::story_msg story_win = { "Anna Nowak", { "Mamy klucze! Plan", "pokonał chaos budowy.", "Dziękujemy za wszystko!" } };
inline constexpr core::story_msg story_lose = { "Kierownik Marek", { "Budowa stoi. Spokojnie -", "z lepszym planem pójdzie.", "Wracamy na plac?" } };
inline constexpr core::story_msg story_ngplus = { "Anna Nowak", { "Znajomi też chcą dom.", "Bierzesz kolejną budowę?", "Termin już się szykuje." } };

inline constexpr core::badge_def badges[] = {
    { "Bez usterek", "Etap bez żadnych obrażeń", 20 },
    { "Przed terminem", "Termin pokonany w 150 tur", 30 },
    { "Seryjny", "8 problemów na jednym etapie", 15 },
    { "Zawodowiec", "Poziom postaci 5", 20 },
    { "Twardziel", "Wygrana na Trudnym", 40 },
    { "Pełny zespół", "Wygrana każdym zawodem", 60 },
    { "Kolekcjoner", "Znajdź wszystkie narzędzia", 30 },
    { "Katalog usterek", "Pokonaj każdy problem", 30 },
    { "Osiedle", "Zbuduj 5 domów", 50 },
};

inline constexpr int badges_count = 9;
inline constexpr int enemies_count = 9;
inline constexpr int badge_bez_usterek = 0;
inline constexpr int badge_przed_terminem = 1;
inline constexpr int badge_seryjny = 2;
inline constexpr int badge_zawodowiec = 3;
inline constexpr int badge_twardziel = 4;
inline constexpr int badge_pelny_zespol = 5;
inline constexpr int badge_kolekcjoner = 6;
inline constexpr int badge_katalog = 7;
inline constexpr int badge_osiedle = 8;

inline constexpr core::tool_def tools[] = {
    { 6, 0 },
    { 7, 20 },
    { 8, 30 },
    { 9, 40 },
};

inline constexpr int tools_count = 4;
inline constexpr int start_tools_mask = 1;
inline constexpr int drop_chance_pct = 25;
inline constexpr int drop_weights[] = { 50, 20, 15, 15 };

inline constexpr int upgrades_count = 5;
inline constexpr int xp_per_kill = 1;
inline constexpr int xp_per_stage = 5;
inline constexpr int xp_boss = 20;
inline constexpr int start_classes_mask = 19;
inline constexpr int class_cost = 25;
inline constexpr int hard_cost = 40;

inline constexpr int level_thresholds[] = { 10, 25, 45, 70 };
inline constexpr int max_hero_level = 5;
inline constexpr int hp_per_level = 2;
inline constexpr int dmg_levels_mask = 32;
inline constexpr int def_levels_mask = 8;

inline constexpr int classes_count = 6;
inline constexpr int stages_count = 5;
inline constexpr int difficulties_count = 3;
inline constexpr int default_difficulty = 1;
inline constexpr int ng_hp_pct_per_tier = 20;
inline constexpr int ng_dmg_bonus_per_tier = 1;
inline constexpr int ng_score_pct_per_tier = 50;

}
