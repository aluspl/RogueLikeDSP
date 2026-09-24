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
    };
}
