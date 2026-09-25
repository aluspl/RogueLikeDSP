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

    enum class ability_effect : uint8_t { stun, wall, volley, chain, flush, spin };

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
        const char* ability_name;   // moc zawodu (przycisk R)
        const char* ability_desc;
        ability_effect ability;
        int8_t ability_cooldown;    // w turach
    };

    // Stan nakładany przez problem budowy przy trafieniu bohatera.
    enum class status_effect : uint8_t { none, poison, shock, slip, paper };

    struct enemy_def           // "problem budowy"
    {
        const char* name;
        const char* desc;      // opis w Katalogu usterek
        int8_t max_health;
        int8_t min_damage;
        int8_t max_damage;
        int8_t defense;
        int8_t sight;
        int16_t score;
        int8_t frame;
        bool slam;             // boss: zapowiada uderzenie w obszar (czerwone pola)
        status_effect on_hit;  // stan nakładany przy trafieniu bohatera
        int8_t status_chance;  // szansa w %
        int8_t status_turns;   // ile tur trwa
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
        int8_t act;            // akt budowy (każdy kończy się bossem)
    };

    struct act_def             // akt budowy: kilka etapów zakończonych bossem, potem Hurtownia
    {
        const char* name;
        int8_t bonus_per_stage;   // premia (zł) za ukończenie aktu: za każdy etap aktu
        int8_t bonus_per_kill;    // ... i za każdy usunięty problem w akcie
    };

    enum class shop_effect : uint8_t { heal, gear, tool, maxhp, ability };

    struct shop_item_def       // Hurtownia między aktami (płatne budżetem z budowy)
    {
        const char* name;
        const char* desc;
        int16_t price;
        shop_effect effect;
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

    struct story_msg           // wiadomość w telefonie (fabuła): nadawca + 3 linie dymka
    {
        const char* from;
        const char* lines[3];
    };

    enum class gear_stat : uint8_t { def, dmg, hp };

    struct gear_def            // sprzęt z dropów: slot x jakość
    {
        const char* name;
        gear_stat stat;
        int8_t value;
    };

    struct badge_def           // odznaka (motywacja do kolejnych budów)
    {
        const char* name;
        const char* desc;
        int16_t xp;            // nagroda przy pierwszym zdobyciu
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
