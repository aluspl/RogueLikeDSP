#pragma once
#include <cstdint>

// Typy danych gry. Czyste C++ (bez Butano) - kompiluje się na GBA i na PC (testy).
namespace core
{
    enum class stat : uint8_t { str, agi, intel };

    struct weapon_def
    {
        const char* name;
        int8_t min_damage;
        int8_t max_damage;
        int8_t range;          // odległość Czebyszewa w polach; 1 = wręcz
        stat scales_with;
    };

    struct class_def           // zawód budowlany
    {
        const char* name;
        const char* desc;
        int8_t max_health;
        int8_t strength;
        int8_t agility;
        int8_t intelligence;
        int8_t defense;
        int8_t weapon;
        int8_t frame;          // klatka w graphics/actors.bmp
    };

    struct enemy_def           // "problem budowy"
    {
        const char* name;
        int8_t max_health;
        int8_t min_damage;
        int8_t max_damage;
        int8_t defense;
        int8_t sight;
        int16_t score;
        int8_t frame;
    };

    struct stage_def           // etap budowy = piętro lochu
    {
        const char* name;
        int8_t pool[4];
        int8_t pool_count;
        int8_t enemy_count;
        int8_t boss;           // -1 = brak
        int16_t hp_pct;        // mnożnik HP wrogów w etapie (100 = bez zmian)
        int8_t dmg_bonus;      // premia do obrażeń wrogów w etapie
    };

    enum class upgrade_effect : uint8_t { hp, def, dmg, coffee, pickups };

    struct upgrade_def         // ulepszenie ze sklepu "Szkolenia" (meta-progresja)
    {
        const char* name;
        const char* desc;
        upgrade_effect effect;
        int8_t value;          // premia za każdy poziom
        int8_t levels;
        int16_t costs[4];      // koszt kolejnych poziomów w doświadczeniu
    };

    struct tool_def            // narzędzie do znalezienia (drop), odblokowywane w sklepie
    {
        int8_t weapon;         // indeks w data::weapons
        int16_t cost;          // 0 = dostępne od początku
    };

    struct difficulty_def      // poziom trudności wybierany na starcie
    {
        const char* name;
        int16_t hp_pct;        // mnożnik HP wrogów
        int8_t dmg_bonus;      // premia do obrażeń wrogów (może być ujemna)
        int16_t score_pct;     // mnożnik wyniku
    };
}
