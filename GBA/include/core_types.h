#pragma once
#include <cstdint>

// Typy danych gry. Czyste C++ (bez Butano) - kompiluje się na GBA i na PC (testy).
namespace core
{
    enum class stat : uint8_t { str, agi, intel };

    // v0.21.50 cz. 2 (#29): żywioł ciosu / problemu - kombinacje stanów (mokry + prąd, pył + iskra).
    enum class element : uint8_t { none, water, power, spark };

    struct weapon_def
    {
        const char* name;
        int8_t min_damage;
        int8_t max_damage;
        int8_t range;          // odległość Czebyszewa w polach; 1 = wręcz
        stat scales_with;
        element elem = element::none;   // prąd (Próbnik), iskra (Szlifierka, Pistolet do kotew)
        int8_t crit = 0;       // v0.21.51 cz. 2: kryt +% (Poziomica mistrza)
        bool knockback = false;   // cios wręcz odpycha problem o pole (Młot Zenka; bossa nie)
        bool reveal = false;   // magazyn widać na podglądzie mapy (Poziomica mistrza)
    };

    enum class ability_effect : uint8_t { stun, wall, volley, chain, flush, spin, line, splash, ram,
                                         weld, mark, borrow };   // v0.21.51 cz. 2: Spaw, Tyczenie, Złota rączka (moc innego fachu)

    // Cecha zawodu działająca cały czas: Dekarz - wiatr nie skraca zasięgu, Operator koparki - cios wręcz czasem odpycha,
    // Geodeta - widzi mapę etapu od startu.
    enum class class_passive : uint8_t { none, windproof, push, surveyor };

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
        int16_t frame;         // klatka w graphics/actors.bmp (v0.21.51 cz. 2: zawody z sekretów od 127)
        const char* ability_name;   // moc zawodu (przycisk R)
        const char* ability_desc;
        ability_effect ability;
        int8_t ability_cooldown;    // w turach
        class_passive passive = class_passive::none;
    };

    // Stan nakładany przez problem budowy przy trafieniu bohatera.
    enum class status_effect : uint8_t { none, poison, shock, slip, paper, wet };   // wet (v0.21.50): mokry - prąd boli bardziej
    constexpr int status_slots = 6;

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
        int8_t material = -1;         // materiał z usuniętego problemu (data::materials), -1 = losowy
        uint16_t tags = 0;            // v0.21.49: zachowania (bity core::behavior), łączone dowolnie
        // druga faza bossa (Decyzja odmowna: Odwołanie) - raz przy phase_pct% HP: +phase_heal% max HP i phase_summon wezwań
        int8_t phase_pct = 0;
        int8_t phase_heal = 0;
        int8_t phase_summon = 0;
        const char* phase_name = "";
        element elem = element::none;   // v0.21.50: woda (zawsze mokry, moczy bohatera), prąd (porażenie mokrego)
        int8_t gender = 0;              // rodzaj nazwy: 0 m, 1 ż, 2 n / l.mn. (przedrostek elity: Zbrojony / Zbrojona / Zbrojone)
    };

    // ------------------------------------------------------------------ v0.21.50 cz. 2
    // Premia po etapie (#27, jak w Hades / Slay the Spire): 1 z 3 po każdym etapie, rzadkość, znaczniki, synergie.
    enum class boon_effect : uint8_t { dmg, dmg_pct, crit, max_hp, def, dodge, coffee, thermos, cooldown, cash, mats, luck,
                                       wet_hits, frost_hits, electric, spark, brigade_pct, regen_stage, kill_heal, status_res,
                                       power, mats_pct, shop_pct, sight };

    struct boon_def
    {
        const char* name;
        const char* desc;
        int8_t rarity;         // 0 zwykła, 1 rzadka, 2 legendarna
        uint16_t tags;         // bity data::boon_tags
        boon_effect effect;
        int8_t value;
        int8_t cls;            // premia zawodu (-1 = dla każdego)
    };

    struct boon_rarity_def
    {
        const char* name;
        int8_t weight;
    };

    // Synergia: 2+ premie z tym samym znacznikiem (albo po jednej z dwóch) włączają dodatkowy skutek.
    enum class synergy_effect : uint8_t { conduct, armor, espresso, safety, luck, brigade, stock, sparks };

    struct synergy_def
    {
        const char* name;
        const char* desc;
        uint16_t tags;         // 1 znacznik: 2+ premie z nim; 2 znaczniki: po jednej z każdego
        synergy_effect effect;
        int8_t value;
    };

    // Elita (#28): wzmocniony problem z jedną cechą, więcej HP i lepszą nagrodą.
    enum class elite_effect : uint8_t { shield, fast, regen, explode, summon };

    struct elite_def
    {
        const char* name;
        const char* prefix[3]; // przedrostek nazwy wg rodzaju (m, ż, n / l.mn.)
        const char* info;
        elite_effect effect;
        int8_t value;
    };

    // Kombinacja stanów (#29): mokry + prąd, pył + iskra, zamróz + uderzenie.
    enum class combo_effect : uint8_t { shock_area, dust_blast, crack };

    struct combo_def
    {
        const char* name;
        const char* short_name;   // zapowiedź, np. "Mokry + prąd!"
        const char* info;         // skutek na problemie
        const char* hero;         // skutek na bohaterze ("" = nie dotyczy)
        combo_effect effect;
        int8_t value;
        int8_t radius;
        int8_t hero_value;
    };

    // Zachowania problemów budowy (pole "behaviors" wroga; parametry w data::behavior_*).
    enum behavior : uint16_t
    {
        tag_ranged = 1,        // strzela z odległości 2-3 w linii (prosto albo po skosie)
        tag_splits = 2,        // po usunięciu dzieli się na dwa słabsze
        tag_heals = 4,         // łata rannych sąsiadów (co kilka tur)
        tag_explodes = 8,      // po usunięciu wybucha: czerwone pola wokół, tura na zejście
        tag_grows = 16,        // rośnie z czasem: +HP, co 2 stopnie +1 obrażeń
        tag_flees = 32,        // trzyma dystans: obok bohatera odskakuje (co kilka tur)
        tag_stationary = 64,   // nie rusza się (za to twardy)
        tag_pushes = 128,      // cios odpycha bohatera o pole
        tag_returns = 256      // raz wraca po usunięciu (po kilku turach, z połową HP)
    };
    constexpr int behaviors_count = 9;

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
        int16_t cost;          // koszt etapu w tys. zł (harmonogram domu po wygranej)
    };

    // Mechanika aktu (v0.21.49): akt I błoto (wejście kosztuje turę), akt II porywy wiatru (spychają o pole),
    // akt III pył (mniejsze pole widzenia), Akt 0 pieczątki (dokumenty na etapie otwierają schody).
    enum class act_mechanic : uint8_t { none, mud, gust, dust, stamps };

    struct act_def             // akt budowy: kilka etapów zakończonych bossem, potem Hurtownia
    {
        const char* name;
        int8_t bonus_per_stage;   // premia (zł) za ukończenie aktu: za każdy etap aktu
        int8_t bonus_per_kill;    // ... i za każdy usunięty problem w akcie
        act_mechanic mechanic = act_mechanic::none;
        int8_t mech_value = 0;    // błoto: 1 pole na tyle; porywy: co tyle tur; pył: -widzenie; pieczątki: dokumenty
        const char* mech_name = "";
        const char* mech_short = "";
        const char* mech_info = "";
        const char* numeral = "";  // numer aktu dla gracza ("0", "I", "II", "III")
        bool prelude = false;      // akt wstępny (Akt 0): w budowie dopiero po nagrodzie za odbiór
    };

    enum class shop_effect : uint8_t { heal, gear, tool, maxhp, ability, def, thermos, upgrade };   // upgrade (v0.21.50): ulepszenie narzędzia

    struct shop_item_def       // Hurtownia między aktami (płatne budżetem z budowy albo materiałami)
    {
        const char* name;
        const char* desc;
        int16_t price;         // zł (0, gdy płatne materiałem)
        shop_effect effect;
        int8_t material = -1;  // płatne materiałem (data::materials), -1 = zł
        int8_t mat_cost = 0;   // ile sztuk materiału
    };

    // Materiały budowy (cement, stal, drewno): wypadają z problemów i paczek, płacą w Hurtowni i za naprawy.
    struct material_def
    {
        const char* name;
        const char* short_name;
    };

    // Naprawa pola za materiał (telefon: Brygada i naprawy): Załataj - deski w poprzek drogi, Kładka - kałuże bez poślizgu.
    enum class repair_effect : uint8_t { patch, bridge };

    struct repair_def
    {
        const char* name;
        const char* desc;      // krótko (baner)
        const char* info;      // skutek (telefon)
        repair_effect effect;
        int8_t material;
        int8_t cost;
        int8_t value;          // Załataj: tury muru; Kładka: zasięg (pola)
    };

    // Wybór ścieżki między etapami: wariant kolejnego etapu (jak rozgałęzienia w Slay the Spire, stała kolejność etapów).
    struct path_def
    {
        const char* name;
        const char* short_name;
        const char* desc;
        int8_t enemies;        // +/- problemów na etapie
        int8_t pickups;        // +/- znajdziek
        int16_t cash;          // zł od razu (+/-)
        int8_t materials;      // losowe materiały na start etapu
        bool bad_weather;      // pogoda tylko z niekorzystnych
        bool no_event;         // bez wydarzenia na placu
    };

    // craft: +statystyka broni zawodu; dmg_pct / taken_pct: +% zadawanych / -% otrzymanych obrażeń
    enum class upgrade_effect : uint8_t { hp, def, dmg, coffee, pickups, luck, craft, dmg_pct, taken_pct };

    struct upgrade_def         // ulepszenie ze sklepu "Szkolenia" (meta-progresja)
    {
        const char* name;
        const char* desc;
        upgrade_effect effect;
        int8_t value;          // premia za każdy poziom
        int8_t levels;
        int16_t costs[4];      // koszt kolejnych poziomów w doświadczeniu
        int16_t refund = 0;    // zwrot (dośw.) za poziom ponad maksimum (profil sprzed zmiany liczby poziomów)
        int16_t reset_refund = 0;   // v0.21.49: ulepszenie zmienione - profil sprzed v8 dostaje zwrot za poziom, poziom od zera
    };

    // Respekt: stała waluta za ukończone etapy, wydawana na stałe ulepszenia procentowe z rangami (telefon profilu).
    enum class respect_effect : uint8_t { dmg_pct, taken_pct, gear_pct, crit, dodge, coffee_pct, thermos, cooldown, cash, xp_pct,
                                          brigade_pct, sight, shop_pct, mats_pct, second_chance, reroll,
                                          veteran };   // v0.21.51 cz. 2: Zaprawiony w boju - kawa w termosie na start

    struct respect_def
    {
        const char* name;
        const char* desc;
        respect_effect effect;
        int8_t ranks;
        int8_t values[5];      // wartość na randze 1..ranks (łącznie, nie przyrost)
        int16_t costs[5];      // koszt kolejnych rang w Respekcie
        int8_t secret = -1;    // v0.21.51 cz. 2: odblokowuje sekretne zlecenie (data::secrets), -1 = od początku
    };

    // Nagroda za odbiór (jak odblokowania w Slay the Spire): każda wygrana odblokowuje kolejną z listy.
    enum class reward_kind : uint8_t { tool, gear, cls, soon, act };

    struct reward_def
    {
        reward_kind kind;
        int8_t index;          // narzędzie (data::tools), slot sprzętu, zawód, akt (data::acts); -1 = wkrótce
        const char* name;
        const char* desc;
    };

    struct story_msg           // wiadomość w telefonie (fabuła): nadawca + 3 linie dymka
    {
        const char* from;
        const char* lines[3];
    };

    // Samouczek menu (#25): dymek Kierownika nad elementem tytułu (screen 0) albo wyboru zawodu (1).
    struct tutorial_step
    {
        const char* id;
        const char* title;
        story_msg msg;         // nadawca + 3 linie dymka
        const char* gba;       // klawisz na GBA ("" = tylko Godot)
        int8_t screen;
        bool godot_only;       // np. klucz z opcjami
        bool needs_investor;   // tylko po odblokowaniu trybu inwestora
    };

    // ------------------------------------------------------------------ v0.21.50 cz. 3
    // Wydarzenie z wyborem (#30): pole z SMS-em na etapie; 2-3 odpowiedzi, każda z 0-3 skutkami (część z szansą).
    enum class choice_effect : uint8_t { cash, xp, hp, max_hp, mats, stage_dmg, stage_def, boon, gear, respect, coffee, spawn, status,
                                         upgrade, power };

    struct choice_out
    {
        choice_effect effect;
        int8_t value;
        int8_t chance;         // % (100 = zawsze)
        int8_t arg;            // materiał (-1 = każdy), problem (spawn), stan (status), slot sprzętu (gear, -1 = losowy)
    };

    struct event_choice
    {
        const char* label;     // odpowiedź, np. "Pożycz za 10 zł"
        const char* result;    // skutek słowami, np. "Betoniarka kręci: mocne ciosy!"
        choice_out out[3];
        int8_t outs;
    };

    struct choice_event_def
    {
        const char* name;
        story_msg msg;         // SMS: nadawca + 3 linie
        event_choice choices[3];
        int8_t choices_count;
    };

    // Ulepszanie narzędzia (#31): +1 obrażeń za poziom, od poziomu trait_at jedna cecha.
    enum class tool_trait_effect : uint8_t { pierce, crit, steady };

    struct tool_trait_def
    {
        const char* name;
        const char* short_name;   // np. "OBR -2" (rozpiska)
        const char* desc;
        tool_trait_effect effect;
        int8_t value;
    };

    struct tool_level_def      // koszt kolejnego poziomu: zł + materiał
    {
        int16_t cash;
        int8_t material;
        int8_t count;
    };

    // Ukryte pomieszczenie (#32): magazyn za pękniętą ścianą (klucz, Operator koparki, wybuch) albo drzwiami (klucz).
    struct secret_kind_def
    {
        const char* name;
        const char* info;
        bool breakable;
    };

    // ------------------------------------------------------------------ v0.21.50 cz. 4
    // Podsumowanie budowy (#33): ostatnie ciosy w bohatera (rodzaj, źródło, etap), rada na koniec budowy.
    enum class recap_kind : uint8_t { melee, ranged, slam, blast, shock, dust };
    constexpr int recap_kinds = 6;

    struct recap_hit
    {
        int8_t src = -1;       // problem (data::enemies), -1 = brak
        int8_t elite = -1;     // cecha elity (data::elites), -1 = zwykły
        uint8_t kind = 0;      // recap_kind
        int8_t stage = -1;     // etap (data::stages)
        int16_t amount = 0;    // obrażenia
    };

    // Rada w podsumowaniu: pierwsza pasująca z listy (kolejność z danych).
    enum class recap_tip : uint8_t { shock, slam, blast, coffee, ranged, elite, boss, no_combo, won, any };

    struct recap_tip_def
    {
        recap_tip when;
        const char* lines[2];
    };

    // Wyzwanie tygodnia (#34): seed z numeru tygodnia i stałe zasady z danych (do 3 na tydzień).
    enum class weekly_rule : uint8_t { cls, no_coffee, elite_pct, weather, no_shop, mats_pct, hp_pct, dmg_pct, cash };

    struct weekly_rule_def
    {
        weekly_rule rule;
        int16_t value;         // zawód (cls), pogoda (weather), procent albo zł
    };

    struct weekly_def
    {
        const char* name;
        const char* short_name;
        const char* desc[2];
        weekly_rule_def rules[3];
        int8_t rules_count;
    };

    // Fabuła odkrywana z budowami (#35): wątek SMS-ów odblokowany kamieniem milowym, archiwum w telefonie profilu.
    enum class story_trigger : uint8_t { runs, wins, boss, elite, secret, event, synergy, daily, weekly, act0 };

    struct story_thread
    {
        const char* name;
        const char* hint;      // jak odblokować (zablokowany wątek w archiwum)
        story_trigger trigger;
        int8_t value;          // liczba budów / wygranych albo boss (data::enemies)
        story_msg msgs[2];
        int8_t msgs_count;
    };

    // Ozdoba Osiedla: pojawia się po tylu wygranych.
    struct decor_def
    {
        const char* name;
        int8_t wins;
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
        uint16_t stages;          // bitmaska etapów, na których może wypaść
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

    enum class gear_stat : uint8_t { def, dmg, hp, dodge, thermos };   // dodge: % uniku; thermos: miejsca w termosie

    struct gear_def            // sprzęt z dropów: slot x jakość
    {
        const char* name;
        gear_stat stat;
        int8_t value;
    };

    enum class trait_effect : uint8_t { luck, crit, poison_res, sight, cooldown, str, agi, intel, slip_res };

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
        bool reward = false;   // odblokowuje nagroda za odbiór (nie Szkolenia)
        bool secret = false;   // v0.21.51 cz. 2: odblokowuje sekretne zlecenie
    };

    // ------------------------------------------------------------------ v0.21.51 cz. 2
    // Sekretne zlecenie (#39): ukryty cel profilu ("???" z podpowiedzią do wykonania), nagroda: zawód, narzędzie,
    // wygląd albo ranga Respektu.
    enum class secret_kind : uint8_t { no_coffee_win, helper_boss, storerooms, class_wins, paper_clean, shock_combos, low_hp_win, fast_win };
    enum class secret_reward : uint8_t { cls, tool, cosmetic, respect };

    struct secret_def
    {
        const char* hint;      // podpowiedź (widoczna od początku)
        const char* desc;      // warunek (po wykonaniu)
        secret_kind kind;
        int16_t value;         // liczba (magazyny, zawody, kombinacje, HP, dni) albo problem (helper_boss)
        secret_reward reward;
        int8_t index;          // zawód, narzędzie (data::tools), wygląd (data::cosmetics), Respekt (data::respect)
        const char* reward_text;   // baner, np. "Nowy zawód: Spawacz"
        story_msg news;        // dymek "Nowość" na tytule
    };

    struct cosmetic_def        // wygląd z sekretnego zlecenia (tylko oprawa)
    {
        const char* name;
        const char* desc;
    };

    struct difficulty_def      // poziom trudności wybierany na starcie
    {
        const char* name;
        int16_t hp_pct;        // mnożnik HP wrogów
        int8_t dmg_bonus;      // premia do obrażeń wrogów (może być ujemna)
        int16_t score_pct;     // mnożnik wyniku
    };
}
