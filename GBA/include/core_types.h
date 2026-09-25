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
        int8_t luck;           // szczęście: kryt, unik, dropy
        int8_t weapon;
        int8_t frame;          // klatka w graphics/actors.bmp
        const char* ability_name;   // moc zawodu (przycisk R)
        const char* ability_desc;
        ability_effect ability;
        int8_t ability_cooldown;    // w turach
    };

    // Stan nakładany przez problem budowy przy trafieniu bohatera.
    enum class status_effect : uint8_t { none, poison, shock, slip, paper };

    struct status_def          // opis stanu (komunikat przy nałożeniu, HUD, telefon)
    {
        const char* name;
        const char* short_name;
        const char* effect;    // skutek, np. "-1 HP/turę"
    };

    // Kształt zapowiedzianego uderzenia bossa: kwadrat wokół bohatera albo krzyż (wiersz i kolumna).
    enum class slam_shape : uint8_t { square, cross };

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
        // mechaniki bossów (domyślnie wyłączone)
        slam_shape shape = slam_shape::square;
        const char* slam_name = "";   // nazwa uderzenia w komunikatach ("" = "Cios bossa" / "Uderzenie")
        int8_t summon = -1;           // wzywany problem (indeks w data::enemies), -1 = brak
        int8_t summon_every = 0;      // co ile tur boss wzywa
        int8_t summon_max = 0;        // ile razy na walkę
        int8_t gear_stun = 0;         // pełny sprzęt (wszystkie sloty): boss ogłuszony na tyle tur na starcie walki
        int16_t reward_cash = 0;      // premia (zł) za pokonanie bossa
        const char* reward_title = "";   // baner nagrody, np. "Protokół bez uwag"
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

    enum class upgrade_effect : uint8_t { hp, def, dmg, coffee, pickups, luck, craft };   // craft: +statystyka broni zawodu

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

    // Wydarzenie na placu: losowy SMS na starcie etapu (nie pierwszego i nie z bossem) z modyfikatorem etapu.
    enum class event_effect : uint8_t { fewer_pickups, cash, inspection, rain, thermos };

    struct site_event_def
    {
        const char* name;
        const char* short_name;   // pastylka w telefonie
        const char* info;         // skutek dla gracza
        story_msg msg;            // SMS: nadawca + 3 linie
        event_effect effect;
        int8_t value;
        bool good;                // korzystne (kolor w telefonie)
    };

    // Pogoda dnia: losowana na starcie każdego etapu (tylko z listy dozwolonych dla etapu).
    enum class weather_effect : uint8_t { none, heat, frost, wind, rain };

    struct weather_def
    {
        const char* name;
        const char* short_name;   // pastylka w telefonie
        const char* info;         // skutek dla gracza
        weather_effect effect;
        int8_t value;             // upał: +tury mocy; mróz: co ile tur problemy stoją; wiatr: -zasięg; deszcz: 1 kałuża na tyle pól
        int8_t weight;            // waga losowania
        bool bad;                 // niekorzystna (z niekorzystnym wydarzeniem na placu się nie łączy)
        uint8_t stages;           // bitmaska etapów, na których może wypaść
    };

    // Brygada: najemny fachowiec wzywany raz na etap z telefonu (płatny budżetem budowy).
    enum class helper_effect : uint8_t { reveal, pump, safety, ally };

    struct helper_def
    {
        const char* name;
        const char* desc;
        helper_effect effect;
        int8_t value;          // pompa: obrażenia; BHP-owiec: +obrona; pomocnik: obrażenia ciosu
        int8_t turns;          // BHP-owiec: tury ochrony; pomocnik: tury pomocy
        int8_t reach;          // pompa: zasięg (pola)
        int16_t price;         // zł z budżetu budowy
        int16_t cost;          // odblokowanie w Szkoleniach (doświadczenie), 0 = od początku
        int8_t frame;          // pomocnik: klatka postaci w actors.bmp
    };

    // Tryb inwestora (jak Heat w Hadesie): modyfikatory trudności po pierwszej wygranej, każdy za % doświadczenia i stawkę.
    enum class investor_effect : uint8_t { cash_pct, no_break, enemy_hp, no_shop, slam, enemy_dmg };

    struct investor_def
    {
        const char* name;
        const char* desc;
        investor_effect effect;
        int8_t value;          // budżet: % zł; problemy: +% HP; kontrola: -tury między ciosami bossa; termin: +obrażenia
        int8_t xp_pct;         // premia doświadczenia
        int8_t stake;          // punkty stawki
    };

    enum class gear_stat : uint8_t { def, dmg, hp };

    struct gear_def            // sprzęt z dropów: slot x jakość
    {
        const char* name;
        gear_stat stat;
        int8_t value;
    };

    enum class trait_effect : uint8_t { luck, crit, poison_res, sight, cooldown, str, agi, intel };

    struct trait_def           // cecha przedmiotu sprzętu
    {
        const char* name;
        const char* short_name;   // do pastylki w telefonie
        trait_effect effect;
        int8_t value;
    };

    // Trwała premia na budowę: uprawnienie z odznaki, pamiątka (run_mods).
    enum class perk_effect : uint8_t { hp, def, dmg, luck, cooldown, sight, thermos, tool_pct, xp_pct, cash, crit, coffee };

    struct perk
    {
        perk_effect effect;
        int8_t value;
    };

    struct badge_def           // odznaka (motywacja do kolejnych budów)
    {
        const char* name;
        const char* desc;
        int16_t xp;            // nagroda przy pierwszym zdobyciu
        perk bonus;            // uprawnienie: premia na każdą kolejną budowę (jak w Hades)
    };

    // Pamiątka (jak keepsake w Hades): zabierana na budowę, premia rośnie z rangą (I/II/III).
    struct keepsake_def
    {
        const char* name;
        const char* desc;
        perk_effect effect;
        int8_t values[3];      // premia na rangę I, II, III
        int8_t badge;          // odblokowuje odznaka (-1 = nie)
        bool start;            // dostępna od początku
    };

    // Zlecenie (jak lista przepowiedni): cel z licznikiem w profilu, nagroda przy ukończeniu.
    enum class contract_kind : uint8_t { kills, powers, brand, clean_boss, class_wins, wins };

    struct contract_def
    {
        const char* name;
        const char* desc;
        contract_kind kind;
        int16_t target;
        int16_t xp;            // nagroda w doświadczeniu
        int8_t keepsake;       // odblokowana pamiątka (-1 = brak)
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
