#pragma once
// Meta-progresja: profil gracza w SRAM (rekord, doświadczenie, zakupy) i sklep "Szkolenia".
// Czyste C++ (bez Butano) - testowalne na PC.
#include <cstddef>
#include <type_traits>
#include "core.h"

namespace core
{
    constexpr int max_upgrades = 8;
    static_assert(data::upgrades_count <= max_upgrades);
    constexpr int max_classes = 12;       // zawody 0-7: bitmaska classes (Szkolenia), 8-11: tylko z nagród za odbiór
    constexpr int max_respect = 16;
    static_assert(data::classes_count <= max_classes && data::respect_count <= max_respect && data::rewards_count <= 255);

    constexpr char profile_magic[8] = "PBRL010";
    constexpr char profile_magic_v9[8] = "PBRL009";
    constexpr char profile_magic_v8[8] = "PBRL008";
    constexpr char profile_magic_v7[8] = "PBRL007";
    constexpr char profile_magic_v6[8] = "PBRL006";
    constexpr char profile_magic_v5[8] = "PBRL005";
    constexpr char profile_magic_v4[8] = "PBRL004";
    constexpr char profile_magic_v3[8] = "PBRL003";
    constexpr char profile_magic_v2[8] = "PBRL002";
    constexpr char profile_magic_v1[8] = "PBRL001";
    constexpr int profile_v2_size = 36;   // v3 = v2 + pola motywacji na końcu
    constexpr int profile_v3_size = 56;   // v4 = v3 + zlecenia i pamiątki na końcu
    constexpr int profile_v4_size = 72;   // v5 = v4 + liczniki zleceń przeniesione z bieżącej budowy
    constexpr int profile_v5_size = 78;   // v6 = v5 + brygada i tryb inwestora
    constexpr int profile_v6_size = 88;   // v7 = v6 + codzienna budowa (data, najlepsze wyniki dni)
    constexpr int profile_v7_size = 124;  // v8 = v7 + Respekt, nagrody za odbiór, wygrane i stawki zawodów 8-11
    constexpr int profile_v8_size = 152;  // v9 = v8 + katalog usterek 16-47 (nowe problemy etapów)
    constexpr int profile_v9_size = 156;  // v10 = v9 + samouczek menu (#25): obejrzane dymki
    constexpr int daily_slots = 5;
    static_assert(data::daily_history <= daily_slots);
    constexpr int max_keepsakes = 8;
    static_assert(data::contracts_count <= 8 && data::keepsakes_count <= max_keepsakes);
    constexpr int max_houses = 12;        // działki na Osiedlu
    static_assert(data::badges_count <= 16 && data::enemies_count <= 48);

    // Pierwsze pola jak w zapisie v1 (magic, best, runs, wins) - migracja zachowuje rekord.
    struct profile
    {
        char magic[8];
        int32_t best;
        int32_t runs;
        int32_t wins;
        int32_t xp;                    // doświadczenie do wydania
        uint8_t levels[max_upgrades];  // kupione poziomy ulepszeń
        uint8_t classes;               // bitmaska odblokowanych zawodów
        uint8_t hard;                  // odblokowany poziom Trudny
        uint8_t flags;                 // bity profile_flag (dawny bajt wyrównania = 0 w starych zapisach)
        uint8_t tools;                 // kupione narzędzia (bitmaska; startowe zawsze dostępne)
        // --- v3: motywacja do kolejnych budów
        uint16_t badges;               // zdobyte odznaki (bitmaska data::badges)
        uint16_t catalog;              // rodzaje problemów kiedykolwiek pokonane (Katalog usterek)
        uint8_t class_wins;            // zawody, którymi wygrano (odznaka Pełny zespół)
        uint8_t tools_found;           // narzędzia kiedykolwiek znalezione (odznaka Kolekcjoner)
        uint8_t houses_count;
        uint8_t houses[max_houses];    // Osiedle: zawód (4 bity) | wielkość domu << 4
        // --- v4: zlecenia i pamiątki
        uint16_t kills_total;          // problemy usunięte we wszystkich budowach
        uint16_t powers_total;         // użycia mocy zawodu
        uint8_t brand_total;           // założone markowe przedmioty
        uint8_t clean_bosses;          // bossowie aktu pokonani bez obrażeń w walce z nimi
        uint8_t contracts;             // ukończone zlecenia (bitmaska data::contracts)
        uint8_t keepsake;              // wybrana pamiątka + 1 (0 = bez pamiątki)
        uint8_t keepsake_runs[max_keepsakes];   // budowy z każdą pamiątką (ranga)
        // --- v5: ile liczników zleceń bieżącej budowy już przeniesiono do *_total (znak wodny).
        // Jest w profilu, bo zapisuje się razem z sumami jednym zapisem SRAM: wznowienie budowy
        // z autozapisu na starcie etapu nie doliczy drugi raz etapu, który już raz przeniesiono.
        uint16_t run_kills;
        uint16_t run_powers;
        uint8_t run_brand;
        uint8_t run_clean;
        // --- v6: brygada (odblokowani fachowcy) i tryb inwestora
        uint8_t brigade;               // kupieni w Szkoleniach fachowcy (bitmaska; startowi zawsze dostępni)
        uint8_t investor;              // włączone modyfikatory trybu inwestora (bitmaska data::investor)
        uint8_t best_stake[8];         // najwyższa stawka wygranej budowy na każdy zawód
        // --- v7: codzienna budowa (GBA nie ma zegara: data wpisana ręcznie; Godot: data z systemu)
        uint8_t daily_d, daily_m;      // ostatnio wpisana data (0 = domyślna z danych)
        uint16_t daily_y;
        uint16_t daily_day[daily_slots];   // numery dni z wynikiem (0 = pusty)
        uint8_t daily_won;             // bity: wygrana tego dnia
        uint8_t daily_runs;            // rozegrane codzienne budowy (licznik, do 255)
        int32_t daily_score[daily_slots];  // najlepszy wynik dnia
        // --- v8: Respekt (stała waluta za etapy) i nagrody za odbiór (każda wygrana odblokowuje kolejną)
        uint16_t respect;              // Respekt do wydania
        uint16_t respect_total;        // zdobyty łącznie
        uint16_t run_respect;          // ile Respektu bieżącej budowy już przeniesiono (znak wodny jak run_kills)
        uint8_t rewards;               // odblokowane nagrody za odbiór (pierwsze N z data::rewards)
        uint8_t class_wins_hi;         // zawody 8-15, którymi wygrano (dalszy ciąg class_wins)
        uint8_t respect_ranks[max_respect];   // kupione rangi Respektu
        uint8_t best_stake_hi[4];      // rekord stawki zawodów 8-11
        // --- v9: katalog usterek - rodzaje problemów 16-47 (dalszy ciąg catalog)
        uint32_t catalog_hi;
        // --- v10: samouczek menu - obejrzane dymki (bity tutorial_flag) i zawody z nagród, o których już był dymek
        uint16_t tutorial;
        uint16_t classes_seen;
    };
    static_assert(offsetof(profile, badges) == profile_v2_size);
    static_assert(offsetof(profile, kills_total) == profile_v3_size);
    static_assert(offsetof(profile, run_kills) == profile_v4_size);
    static_assert(offsetof(profile, brigade) == profile_v5_size);
    static_assert(offsetof(profile, daily_d) == profile_v6_size && offsetof(profile, daily_score) == 104);
    static_assert(offsetof(profile, respect) == profile_v7_size && offsetof(profile, respect_ranks) == 132);
    static_assert(offsetof(profile, catalog_hi) == profile_v8_size && offsetof(profile, tutorial) == profile_v9_size && sizeof(profile) == 160);

    // Katalog usterek: rodzaje 0-15 w catalog, 16-47 w catalog_hi.
    inline bool catalog_has(const profile& p, int d) { return d < 16 ? (p.catalog >> d) & 1 : (p.catalog_hi >> (d - 16)) & 1; }
    inline void catalog_add(profile& p, int d)
    {
        if(d < 16) p.catalog = uint16_t(p.catalog | (1u << d));
        else p.catalog_hi |= 1u << (d - 16);
    }
    inline int catalog_count(const profile& p) { int n = 0; for(int d = 0; d < data::enemies_count; ++d) n += catalog_has(p, d); return n; }

    enum profile_flag : uint8_t { help_seen = 1, prologue_seen = 2 };

    inline bool has_flag(const profile& p, profile_flag f) { return p.flags & f; }
    inline void set_flag(profile& p, profile_flag f) { p.flags = uint8_t(p.flags | f); }

    inline bool keepsake_unlocked(const profile& p, int k);

    // ------------------------------------------------------------------ nagrody za odbiór
    // Nagroda i jest odebrana, gdy i < p.rewards (każda wygrana odblokowuje kolejną; "wkrótce" nie odblokowuje się).
    inline bool reward_owned(const profile& p, int i) { return i < p.rewards; }
    inline bool reward_unlocked(const profile& p, reward_kind k, int index)
    {
        for(int i = 0; i < data::rewards_count && i < p.rewards; ++i)
            if(data::rewards[i].kind == k && data::rewards[i].index == index) return true;
        return false;
    }
    // Ile nagród da się odebrać (bez "wkrótce" na końcu listy).
    inline int rewards_available()
    {
        int n = 0;
        while(n < data::rewards_count && data::rewards[n].kind != reward_kind::soon) ++n;
        return n;
    }
    // Numer wygranej (licząc od 1), która odblokuje nagrodę i (dla odebranych i "wkrótce": -1).
    inline int reward_win(const profile& p, int i)
    {
        if(reward_owned(p, i) || i >= rewards_available()) return -1;
        return p.wins + (i - p.rewards) + 1;
    }
    // Wygrana budowa: licznik i kolejna nagroda. Zwraca indeks odblokowanej nagrody albo -1.
    inline int record_win(profile& p)
    {
        ++p.wins;
        if(p.rewards >= rewards_available()) return -1;
        return p.rewards++;
    }
    // Zawód wygrany (odznaka Pełny zespół, zlecenie Trzy fachy): bity 0-7 w class_wins, 8-15 w class_wins_hi.
    inline bool class_won(const profile& p, int c) { return c < 8 ? (p.class_wins >> c) & 1 : (p.class_wins_hi >> (c - 8)) & 1; }
    inline void set_class_won(profile& p, int c)
    {
        if(c < 8) p.class_wins = uint8_t(p.class_wins | (1u << c));
        else p.class_wins_hi = uint8_t(p.class_wins_hi | (1u << (c - 8)));
    }
    inline int classes_won(const profile& p) { int n = 0; for(int c = 0; c < data::classes_count; ++c) n += class_won(p, c); return n; }
    inline int best_stake(const profile& p, int c) { return c < 8 ? p.best_stake[c] : p.best_stake_hi[c - 8]; }
    inline void set_best_stake(profile& p, int c, int v) { (c < 8 ? p.best_stake[c] : p.best_stake_hi[c - 8]) = uint8_t(v); }

    // Bez wybranej pamiątki: pierwsza odblokowana (nowy profil zaczyna z Termosem babci).
    inline void default_keepsake(profile& p)
    {
        if(p.keepsake != 0) return;
        for(int k = 0; k < data::keepsakes_count; ++k)
            if(keepsake_unlocked(p, k)) { p.keepsake = uint8_t(k + 1); return; }
    }

    inline void profile_reset(profile& p)
    {
        std::memset(&p, 0, sizeof p);
        std::memcpy(p.magic, profile_magic, sizeof p.magic);
        p.classes = uint8_t(data::start_classes_mask);
        default_keepsake(p);
    }

    // Szkolenia z mniejszą liczbą poziomów niż w starym profilu (np. po zmianie balansu): poziomy ponad maksimum
    // wracają jako doświadczenie (refund z danych). Zwraca true, jeśli coś zmieniono.
    inline bool clamp_levels(profile& p)
    {
        bool changed = false;
        for(int i = 0; i < data::upgrades_count; ++i)
            while(p.levels[i] > data::upgrades[i].levels)
            {
                --p.levels[i];
                p.xp += data::upgrades[i].refund;
                changed = true;
            }
        return changed;
    }

    // Naprawia wczytany profil. Zwraca true, jeśli trzeba go zapisać (migracja albo pusta pamięć).
    // v0.21.49 (profil sprzed v8): ulepszenia ze zmienionym działaniem (BHP, Kurs fachowy) wracają jako doświadczenie,
    // nagrody za odbiór za dotychczasowe wygrane.
    inline void migrate_v8(profile& p)
    {
        for(int i = 0; i < data::upgrades_count; ++i)
            if(data::upgrades[i].reset_refund > 0 && p.levels[i] > 0)
            {
                p.xp += data::upgrades[i].reset_refund * p.levels[i];
                p.levels[i] = 0;
            }
        p.rewards = uint8_t(imin(imax(0, p.wins), rewards_available()));
    }

    inline void migrate_v10(profile& p);

    inline bool profile_fix(profile& p)
    {
        if(std::memcmp(p.magic, profile_magic, sizeof p.magic) == 0) return clamp_levels(p);
        bool v9 = std::memcmp(p.magic, profile_magic_v9, sizeof p.magic) == 0;
        if(v9 || std::memcmp(p.magic, profile_magic_v8, sizeof p.magic) == 0)   // v8 -> v9: katalog 16-47 od zera; v9 -> v10: samouczek
        {
            int keep = v9 ? profile_v9_size : profile_v8_size;
            std::memset(reinterpret_cast<char*>(&p) + keep, 0, sizeof p - keep);
            std::memcpy(p.magic, profile_magic, sizeof p.magic);
            clamp_levels(p);
            migrate_v10(p);
            return true;
        }
        // v7/v6/v5/v4/v3/v2 -> v9: stare pola zostają, nowe od zera; bez wybranej pamiątki - pierwsza odblokowana
        int keep = std::memcmp(p.magic, profile_magic_v7, sizeof p.magic) == 0 ? profile_v7_size
                 : std::memcmp(p.magic, profile_magic_v6, sizeof p.magic) == 0 ? profile_v6_size
                 : std::memcmp(p.magic, profile_magic_v5, sizeof p.magic) == 0 ? profile_v5_size
                 : std::memcmp(p.magic, profile_magic_v4, sizeof p.magic) == 0 ? profile_v4_size
                 : (std::memcmp(p.magic, profile_magic_v3, sizeof p.magic) == 0 ? profile_v3_size
                 : (std::memcmp(p.magic, profile_magic_v2, sizeof p.magic) == 0 ? profile_v2_size : 0));
        if(keep > 0)
        {
            std::memset(reinterpret_cast<char*>(&p) + keep, 0, sizeof p - keep);
            std::memcpy(p.magic, profile_magic, sizeof p.magic);
            default_keepsake(p);
            clamp_levels(p);
            migrate_v8(p);
            migrate_v10(p);
            return true;
        }
        if(std::memcmp(p.magic, profile_magic_v1, sizeof p.magic) == 0)
        {
            int32_t best = p.best, runs = p.runs, wins = p.wins;
            profile_reset(p);
            p.best = best; p.runs = runs; p.wins = wins;
            migrate_v8(p);
            migrate_v10(p);
            return true;
        }
        profile_reset(p);
        return true;
    }

    // Akt 0 (Papierologia) z nagrody za odbiór: budowa zaczyna się od jego etapów.
    inline bool act0_unlocked(const profile& p)
    {
        for(int i = 0; i < data::rewards_count && i < p.rewards; ++i) if(data::rewards[i].kind == reward_kind::act) return true;
        return false;
    }

    // ------------------------------------------------------------------ samouczek menu (#25)
    // Pierwsze uruchomienie: dymki po kolei na tytule i wyborze zawodu; potem jeden dymek przy pierwszym odblokowaniu
    // (Respekt, codzienna budowa, tryb inwestora, Akt 0, nowy zawód). "Pokaż samouczek jeszcze raz" w Jak grać.
    enum tutorial_flag : uint16_t { tut_title = 1, tut_class = 2, tut_respect = 4, tut_daily = 8, tut_investor = 16, tut_act0 = 32 };
    enum tutorial_unlock : int { unlock_respect, unlock_daily, unlock_investor, unlock_act0, unlock_class };   // = data::tutorial_unlocks
    static_assert(data::tutorial_unlocks_count == 5);

    inline bool class_reward(int c);
    inline bool class_unlocked(const profile& p, int c);
    inline bool investor_unlocked(const profile& p);

    // Główny samouczek ekranu (0 tytuł, 1 wybór zawodu) jeszcze nieobejrzany.
    inline bool tutorial_pending(const profile& p, int screen) { return ! (p.tutorial & (screen == 0 ? tut_title : tut_class)); }
    inline void tutorial_done(profile& p, int screen) { p.tutorial = uint16_t(p.tutorial | (screen == 0 ? tut_title : tut_class)); }
    inline void tutorial_reset(profile& p) { p.tutorial = uint16_t(p.tutorial & ~(tut_title | tut_class)); }
    // Kroki samouczka widoczne na ekranie (bez kroków tylko dla Godota na GBA; tryb inwestora dopiero po odblokowaniu).
    inline bool tutorial_step_shown(const profile& p, int i, bool godot)
    {
        const tutorial_step& t = data::tutorial_steps[i];
        return (godot || ! t.godot_only) && (! t.needs_investor || investor_unlocked(p));
    }
    // Dymek odblokowania do pokazania na ekranie (-1 = brak); cls = nowy zawód z nagrody (unlock_class).
    inline int pending_unlock(const profile& p, int screen, int& cls)
    {
        cls = -1;
        if(tutorial_pending(p, screen)) return -1;   // najpierw główny samouczek
        if(screen == 0)
        {
            if(p.respect_total > 0 && ! (p.tutorial & tut_respect)) return unlock_respect;
            if(p.runs > 0 && ! (p.tutorial & tut_daily)) return unlock_daily;
            if(act0_unlocked(p) && ! (p.tutorial & tut_act0)) return unlock_act0;
            return -1;
        }
        if(investor_unlocked(p) && ! (p.tutorial & tut_investor)) return unlock_investor;
        for(int c = 0; c < data::classes_count; ++c)
            if(class_reward(c) && class_unlocked(p, c) && ! ((p.classes_seen >> c) & 1)) { cls = c; return unlock_class; }
        return -1;
    }
    inline void mark_unlock(profile& p, int u, int cls)
    {
        static constexpr uint16_t bits[4] = { tut_respect, tut_daily, tut_investor, tut_act0 };
        if(u >= 0 && u < 4) p.tutorial = uint16_t(p.tutorial | bits[u]);
        if(u == unlock_class && cls >= 0) p.classes_seen = uint16_t(p.classes_seen | (1u << cls));
    }

    // Zawód: startowy / kupiony w Szkoleniach (bitmaska) albo z nagrody za odbiór.
    inline bool class_reward(int c) { return (data::reward_classes_mask >> c) & 1; }
    inline bool class_unlocked(const profile& p, int c)
    {
        return class_reward(c) ? reward_unlocked(p, reward_kind::cls, c) : (p.classes & (1u << c)) != 0;
    }
    inline bool difficulty_unlocked(const profile& p, int d) { return d < data::difficulties_count - 1 || p.hard; }

    // v9 -> v10: nagroda Akt 0 za dotychczasowe wygrane (wcześniej "wkrótce"); samouczek - kto już grał, nie ogląda
    // głównego samouczka ani dymków o tym, co już zna (Akt 0 z migracji dostaje dymek).
    inline void migrate_v10(profile& p)
    {
        p.rewards = uint8_t(imax(p.rewards, imin(imax(0, p.wins), rewards_available())));
        p.tutorial = 0; p.classes_seen = 0;
        if(p.runs > 0) p.tutorial = uint16_t(tut_title | tut_class | tut_daily);
        if(p.respect_total > 0) p.tutorial = uint16_t(p.tutorial | tut_respect);
        if(p.wins > 0) p.tutorial = uint16_t(p.tutorial | tut_investor);
        for(int c = 0; c < data::classes_count; ++c)
            if(class_reward(c) && class_unlocked(p, c)) p.classes_seen = uint16_t(p.classes_seen | (1u << c));
    }

    // Koszt kolejnego poziomu ulepszenia; -1 = maksymalny poziom.
    inline int upgrade_cost(const profile& p, int i)
    {
        const upgrade_def& u = data::upgrades[i];
        return p.levels[i] < u.levels ? u.costs[p.levels[i]] : -1;
    }

    inline bool buy_upgrade(profile& p, int i)
    {
        int c = upgrade_cost(p, i);
        if(c < 0 || p.xp < c) return false;
        p.xp -= c; ++p.levels[i];
        return true;
    }

    inline bool buy_class(profile& p, int c)
    {
        if(class_reward(c) || class_unlocked(p, c) || p.xp < data::class_cost) return false;
        p.xp -= data::class_cost; p.classes = uint8_t(p.classes | (1u << c));
        return true;
    }

    inline int tools_mask(const profile& p)
    {
        int m = p.tools | data::start_tools_mask;
        for(int i = 0; i < data::tools_count; ++i) if(data::tools[i].reward) m = reward_unlocked(p, reward_kind::tool, i) ? m | (1 << i) : m & ~(1 << i);
        return m;
    }
    inline bool tool_unlocked(const profile& p, int i) { return (tools_mask(p) >> i) & 1; }
    // Sloty sprzętu w dropach: kask, rękawice, kamizelka + odebrane w nagrodach (buty, pas).
    inline int gear_slots_mask(const profile& p)
    {
        int m = data::gear_base_mask;
        for(int i = 0; i < data::gear_slots_count; ++i) if(reward_unlocked(p, reward_kind::gear, i)) m |= 1 << i;
        return m;
    }

    inline bool buy_tool(profile& p, int i)
    {
        if(data::tools[i].reward || tool_unlocked(p, i) || p.xp < data::tools[i].cost) return false;
        p.xp -= data::tools[i].cost; p.tools = uint8_t(p.tools | (1u << i));
        return true;
    }

    // Brygada: fachowcy do wezwania (startowi zawsze, reszta za doświadczenie w Szkoleniach).
    inline int helpers_mask(const profile& p) { return p.brigade | data::start_helpers_mask; }
    inline bool helper_unlocked(const profile& p, int i) { return (helpers_mask(p) >> i) & 1; }

    inline bool buy_helper(profile& p, int i)
    {
        if(helper_unlocked(p, i) || p.xp < data::brigade[i].cost) return false;
        p.xp -= data::brigade[i].cost; p.brigade = uint8_t(p.brigade | (1u << i));
        return true;
    }

    inline bool buy_hard(profile& p)
    {
        if(p.hard || p.xp < data::hard_cost) return false;
        p.xp -= data::hard_cost; p.hard = 1;
        return true;
    }

    // ------------------------------------------------------------------ pamiątki
    inline bool keepsake_unlocked(const profile& p, int k)
    {
        const keepsake_def& kd = data::keepsakes[k];
        if(kd.start || (kd.badge >= 0 && (p.badges & (1u << kd.badge)))) return true;
        for(int i = 0; i < data::contracts_count; ++i)
            if(data::contracts[i].keepsake == k && (p.contracts & (1u << i))) return true;
        return false;
    }

    // Ranga 1-3: rośnie po data::keepsake_rank_runs budowach z tą pamiątką.
    inline int keepsake_rank(const profile& p, int k)
    {
        return 1 + (p.keepsake_runs[k] >= data::keepsake_rank_runs[0]) + (p.keepsake_runs[k] >= data::keepsake_rank_runs[1]);
    }

    inline perk keepsake_perk(const profile& p, int k)
    {
        return { data::keepsakes[k].effect, data::keepsakes[k].values[keepsake_rank(p, k) - 1] };
    }

    // Wybrana pamiątka (indeks) albo -1.
    inline int selected_keepsake(const profile& p)
    {
        int k = int(p.keepsake) - 1;
        return k >= 0 && k < data::keepsakes_count && keepsake_unlocked(p, k) ? k : -1;
    }

    // Wybór na ekranie zawodu (L/R): kolejna odblokowana pamiątka albo "bez pamiątki".
    inline void cycle_keepsake(profile& p, int dir)
    {
        int n = data::keepsakes_count + 1, k = p.keepsake;
        for(int i = 0; i < n; ++i)
        {
            k = (k + dir + n) % n;
            if(k == 0 || keepsake_unlocked(p, k - 1)) break;
        }
        p.keepsake = uint8_t(k);
    }

    // Start budowy: licznik budów i budów z wybraną pamiątką (mods() wołać wcześniej - ranga z budów przed tą),
    // nowa budowa nie ma jeszcze nic przeniesionego do liczników zleceń.
    inline void start_run(profile& p)
    {
        ++p.runs;
        p.run_kills = 0; p.run_powers = 0; p.run_brand = 0; p.run_clean = 0; p.run_respect = 0;
        int k = selected_keepsake(p);
        if(k >= 0 && p.keepsake_runs[k] < 255) ++p.keepsake_runs[k];
    }

    // ------------------------------------------------------------------ Respekt (telefon profilu, strona Respekt)
    inline int respect_rank(const profile& p, int i) { return imin(p.respect_ranks[i], data::respect[i].ranks); }
    // Koszt kolejnej rangi; -1 = maksymalna.
    inline int respect_cost(const profile& p, int i)
    {
        int r = respect_rank(p, i);
        return r < data::respect[i].ranks ? data::respect[i].costs[r] : -1;
    }
    inline bool buy_respect(profile& p, int i)
    {
        int c = respect_cost(p, i);
        if(c < 0 || p.respect < c) return false;
        p.respect = uint16_t(p.respect - c); ++p.respect_ranks[i];
        return true;
    }
    // Wartość kupionej rangi (0 = nic nie kupiono).
    inline int respect_value(const profile& p, int i) { int r = respect_rank(p, i); return r > 0 ? data::respect[i].values[r - 1] : 0; }
    inline int respect_total_cost()
    {
        int t = 0;
        for(int i = 0; i < data::respect_count; ++i) for(int r = 0; r < data::respect[i].ranks; ++r) t += data::respect[i].costs[r];
        return t;
    }
    inline int respect_spent(const profile& p)
    {
        int t = 0;
        for(int i = 0; i < data::respect_count; ++i) for(int r = 0; r < respect_rank(p, i); ++r) t += data::respect[i].costs[r];
        return t;
    }

    // Tryb inwestora: odblokowany po pierwszej wygranej; wybór na ekranie zawodu (SELECT).
    inline bool investor_unlocked(const profile& p) { return p.wins > 0; }
    inline int investor_mask(const profile& p) { return investor_unlocked(p) ? p.investor & ((1 << data::investor_count) - 1) : 0; }
    inline void toggle_investor(profile& p, int i) { p.investor = uint8_t(p.investor ^ (1u << i)); }

    inline run_mods mods(const profile& p)
    {
        run_mods m;
        m.tools = tools_mask(p);
        m.helpers = helpers_mask(p);
        for(int i = 0; i < data::upgrades_count; ++i)
        {
            int v = data::upgrades[i].value * p.levels[i];
            switch(data::upgrades[i].effect)
            {
                case upgrade_effect::hp:      m.hp += v; break;
                case upgrade_effect::def:     m.def += v; break;
                case upgrade_effect::dmg:     m.dmg += v; break;
                case upgrade_effect::coffee:  m.coffee += v; break;
                case upgrade_effect::pickups: m.pickups += v; break;
                case upgrade_effect::luck:    m.luck += v; break;
                case upgrade_effect::craft:   m.craft += v; break;
                case upgrade_effect::dmg_pct: m.dmg_pct += v; break;
                case upgrade_effect::taken_pct: m.taken_pct += v; break;
                default: break;
            }
        }
        for(int i = 0; i < data::respect_count; ++i)   // Respekt: kupione rangi
            if(respect_rank(p, i) > 0) add_respect(m, data::respect[i].effect, respect_value(p, i));
        m.gear_slots = gear_slots_mask(p);   // nagrody za odbiór: buty, pas
        m.act0 = act0_unlocked(p) ? 1 : 0;   // nagroda za odbiór: Akt 0 przed budową
        for(int i = 0; i < data::badges_count; ++i)   // uprawnienia z zdobytych odznak
            if(p.badges & (1u << i)) add_perk(m, data::badges[i].bonus);
        int k = selected_keepsake(p);   // pamiątka zabrana na budowę
        if(k >= 0) add_perk(m, keepsake_perk(p, k));
        m.investor = investor_mask(p);   // tryb inwestora: modyfikatory i premia doświadczenia
        m.xp_pct += investor_xp(m.investor);
        return m;
    }

    // Ekran "Koszty": ile kosztuje cały sklep i ile już wydano (pasek budżetu).
    inline int shop_total_cost()
    {
        int t = data::hard_cost;
        for(int i = 0; i < data::upgrades_count; ++i)
            for(int l = 0; l < data::upgrades[i].levels; ++l) t += data::upgrades[i].costs[l];
        for(int i = 0; i < data::classes_count; ++i) if(! (data::start_classes_mask & (1 << i)) && ! class_reward(i)) t += data::class_cost;
        for(int i = 0; i < data::tools_count; ++i) t += data::tools[i].cost;
        for(int i = 0; i < data::brigade_count; ++i) t += data::brigade[i].cost;
        return t;
    }

    inline int shop_spent(const profile& p)
    {
        int t = p.hard ? data::hard_cost : 0;
        for(int i = 0; i < data::upgrades_count; ++i)
            for(int l = 0; l < p.levels[i]; ++l) t += data::upgrades[i].costs[l];
        for(int i = 0; i < data::classes_count; ++i)
            if(class_unlocked(p, i) && ! (data::start_classes_mask & (1 << i)) && ! class_reward(i)) t += data::class_cost;
        for(int i = 0; i < data::tools_count; ++i) if(tool_unlocked(p, i)) t += data::tools[i].cost;
        for(int i = 0; i < data::brigade_count; ++i) if(helper_unlocked(p, i)) t += data::brigade[i].cost;
        return t;
    }

    // ------------------------------------------------------------------ motywacja: Osiedle, odznaki, katalog
    // Dom na Osiedlu po wygranej budowie; wielkość z wyniku (0-3). Pełne Osiedle: najstarszy dom ustępuje.
    inline bool add_house(profile& p, const game& g)
    {
        if(g.st != status::won) return false;
        uint8_t h = uint8_t((g.cls & 15) | (imin(3, g.score / 1000) << 4));   // klatka domu: wielkość * data::classes_count + zawód
        if(p.houses_count < max_houses) p.houses[p.houses_count++] = h;
        else { for(int i = 1; i < max_houses; ++i) p.houses[i - 1] = p.houses[i]; p.houses[max_houses - 1] = h; }
        return true;
    }

    inline uint16_t add_sat16(uint16_t a, int d) { return uint16_t(imin(65535, a + imax(0, d))); }
    inline uint8_t add_sat8(uint8_t a, int d) { return uint8_t(imin(255, a + imax(0, d))); }

    // Przenosi do profilu nowe wartości liczników zleceń z budowy (bez podwójnego liczenia): dolicza tylko
    // nadwyżkę ponad znak wodny run_* w profilu. Po wznowieniu budowy z wcześniejszego autozapisu liczniki
    // gry są mniejsze niż znak wodny - powtórzony etap dolicza się dopiero, gdy go przebije.
    inline void bank_counters(profile& p, const game& g)
    {
        int rd = g.respect - p.run_respect;   // Respekt za ukończone etapy - od razu w profilu (śmierć go nie zabiera)
        if(rd > 0) { p.respect = add_sat16(p.respect, rd); p.respect_total = add_sat16(p.respect_total, rd); }
        p.run_respect = uint16_t(imax(p.run_respect, imin(65535, g.respect)));
        p.kills_total = add_sat16(p.kills_total, g.kills - p.run_kills); p.run_kills = uint16_t(imax(p.run_kills, imin(65535, g.kills)));
        p.powers_total = add_sat16(p.powers_total, g.powers_used - p.run_powers); p.run_powers = uint16_t(imax(p.run_powers, g.powers_used));
        p.brand_total = add_sat8(p.brand_total, g.brand_found - p.run_brand); p.run_brand = uint8_t(imax(p.run_brand, g.brand_found));
        p.clean_bosses = add_sat8(p.clean_bosses, g.clean_bosses - p.run_clean); p.run_clean = uint8_t(imax(p.run_clean, g.clean_bosses));
    }

    inline int popcount(unsigned v) { int n = 0; for(; v; v &= v - 1) ++n; return n; }

    // Postęp zlecenia (licznik z profilu).
    inline int contract_progress(const profile& p, int i)
    {
        switch(data::contracts[i].kind)
        {
            case contract_kind::kills:      return p.kills_total;
            case contract_kind::powers:     return p.powers_total;
            case contract_kind::brand:      return p.brand_total;
            case contract_kind::clean_boss: return p.clean_bosses;
            case contract_kind::class_wins: return classes_won(p);
            case contract_kind::wins:       return p.wins;
            default:                        return 0;
        }
    }

    inline bool contract_done(const profile& p, int i) { return p.contracts & (1u << i); }

    // Postęp zlecenia w trakcie budowy: profil + liczniki budowy jeszcze nieprzeniesione do profilu.
    inline int contract_progress_live(const profile& p, const game& g, int i)
    {
        int v = contract_progress(p, i);
        switch(data::contracts[i].kind)
        {
            case contract_kind::kills:      return v + imax(0, g.kills - p.run_kills);
            case contract_kind::powers:     return v + imax(0, g.powers_used - p.run_powers);
            case contract_kind::brand:      return v + imax(0, g.brand_found - p.run_brand);
            case contract_kind::clean_boss: return v + imax(0, g.clean_bosses - p.run_clean);
            default:                        return v;
        }
    }

    // Najbliższe ukończenia (największy % postępu) nieukończone zlecenie; -1 gdy wszystkie wykonane.
    inline int next_contract(const profile& p, const game& g)
    {
        int best = -1, best_pct = -1;
        for(int i = 0; i < data::contracts_count; ++i)
        {
            if(contract_done(p, i)) continue;
            int pct = imin(100, contract_progress_live(p, g, i) * 100 / data::contracts[i].target);
            if(pct > best_pct) { best_pct = pct; best = i; }
        }
        return best;
    }

    // Sprawdza zlecenia: ukończone dają doświadczenie (i pamiątkę). Zwraca bitmaskę ukończonych właśnie teraz.
    inline int check_contracts(profile& p)
    {
        int got = 0;
        for(int i = 0; i < data::contracts_count; ++i)
            if(! contract_done(p, i) && contract_progress(p, i) >= data::contracts[i].target)
            {
                p.contracts = uint8_t(p.contracts | (1u << i));
                p.xp += data::contracts[i].xp;
                got |= 1 << i;
            }
        return got;
    }

    // Przenosi do profilu trwałe osiągnięcia budowy (katalog, narzędzia, wygrane zawody, liczniki zleceń).
    // Można wołać wielokrotnie.
    inline void record_run(profile& p, game& g)
    {
        bank_counters(p, g);
        for(int d = 0; d < data::enemies_count; ++d) if(g.kills_by_type[d]) catalog_add(p, d);
        p.tools_found = uint8_t(p.tools_found | g.tools_found);
        if(g.st == status::won) set_class_won(p, g.cls);
        int stake = investor_stake(g.bonus.investor);   // rekord stawki zawodu (wygrana w trybie inwestora)
        if(g.st == status::won && stake > best_stake(p, g.cls)) set_best_stake(p, g.cls, stake);
    }

    // Sprawdza odznaki po ważnym momencie (koniec etapu, koniec budowy). Nowe odznaki dają doświadczenie.
    // Zwraca bitmaskę odznak zdobytych właśnie teraz.
    inline int check_badges(profile& p, game& g)
    {
        record_run(p, g);
        bool cleared = g.st == status::stage_clear || g.st == status::won;
        bool won = g.st == status::won;
        int all_tools = (1 << data::tools_count) - 1;
        bool cond[16] = {};
        cond[data::badge_bez_usterek] = cleared && g.stage_damage == 0;
        cond[data::badge_przed_terminem] = won && g.turns - g.stage_start_turn <= 150;
        cond[data::badge_seryjny] = g.stage_kills >= 8;
        cond[data::badge_zawodowiec] = g.hero_level >= data::max_hero_level;
        cond[data::badge_twardziel] = won && g.diff == data::difficulties_count - 1;
        cond[data::badge_pelny_zespol] = classes_won(p) == data::classes_count;
        cond[data::badge_kolekcjoner] = (p.tools_found & all_tools) == all_tools;
        cond[data::badge_katalog] = catalog_count(p) == data::enemies_count;
        cond[data::badge_osiedle] = p.houses_count >= 5;
        int got = 0;
        for(int i = 0; i < data::badges_count; ++i)
            if(cond[i] && ! (p.badges & (1u << i)))
            {
                p.badges = uint16_t(p.badges | (1u << i));
                p.xp += data::badges[i].xp;
                got |= 1 << i;
            }
        return got;
    }

    // Przenosi nowe doświadczenie z budowy do profilu. Zwraca, ile dodano.
    inline int bank_xp(profile& p, game& g)
    {
        int d = g.xp() - g.xp_banked;
        g.xp_banked = g.xp();
        p.xp += d;
        return d;
    }

    // ------------------------------------------------------------------ codzienna budowa (seed z daty)
    inline bool leap_year(int y) { return (y % 4 == 0 && y % 100 != 0) || y % 400 == 0; }
    inline int days_in_month(int y, int m)
    {
        static constexpr int8_t dm[12] = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
        return m == 2 && leap_year(y) ? 29 : dm[(m - 1) % 12];
    }

    // Dni od 1970-01-01 (kalendarz gregoriański; algorytm days_from_civil).
    inline int days_from_civil(int y, int m, int d)
    {
        y -= m <= 2;
        int era = (y >= 0 ? y : y - 399) / 400;
        int yoe = y - era * 400;
        int doy = (153 * (m + (m > 2 ? -3 : 9)) + 2) / 5 + d - 1;
        int doe = yoe * 365 + yoe / 4 - yoe / 100 + doy;
        return era * 146097 + doe - 719468;
    }

    inline void civil_from_days(int z, int& y, int& m, int& d)
    {
        z += 719468;
        int era = (z >= 0 ? z : z - 146096) / 146097;
        int doe = z - era * 146097;
        int yoe = (doe - doe / 1460 + doe / 36524 - doe / 146096) / 365;
        int doy = doe - (365 * yoe + yoe / 4 - yoe / 100);
        int mp = (5 * doy + 2) / 153;
        d = doy - (153 * mp + 2) / 5 + 1;
        m = mp < 10 ? mp + 3 : mp - 9;
        y = yoe + era * 400 + (m <= 2);
    }

    // Numer "Budowy dnia" (1 = data::daily_epoch).
    inline int daily_number(int y, int m, int d)
    {
        return days_from_civil(y, m, d) - days_from_civil(data::daily_epoch[0], data::daily_epoch[1], data::daily_epoch[2]) + 1;
    }

    inline uint32_t daily_seed(int day)
    {
        uint32_t h = uint32_t(day) * 2654435761u + 0x9E3779B9u;
        h ^= h >> 16; h *= 2246822519u; h ^= h >> 13;
        return h ? h : 1u;
    }

    // Zawód dnia i modyfikatory dnia (tryb inwestora) z seeda - dla wszystkich takie same.
    inline int daily_class(uint32_t seed) { return int(seed % uint32_t(data::classes_count)); }
    inline int daily_investor(uint32_t seed)
    {
        int mask = 0;
        uint32_t h = seed;
        for(int k = 0; k < data::daily_investor_mods; ++k)
        {
            h = h * 1664525u + 1013904223u;
            int i = int((h >> 16) % uint32_t(data::investor_count));
            for(int guard = 0; (mask >> i) & 1 && guard < data::investor_count; ++guard) i = (i + 1) % data::investor_count;
            mask |= 1 << i;
        }
        return mask;
    }

    // Codzienna budowa jest równa dla wszystkich: bez Szkoleń, odznak i pamiątek, tylko modyfikatory dnia.
    inline run_mods daily_mods(uint32_t seed)
    {
        run_mods m;
        m.investor = daily_investor(seed);
        m.xp_pct = investor_xp(m.investor);
        return m;
    }

    inline void start_daily(game& g, int day)
    {
        uint32_t seed = daily_seed(day);
        g.new_run(daily_class(seed), seed, data::daily_difficulty, daily_mods(seed));
        g.daily = true;
        g.daily_day = uint16_t(day);
    }

    // Data codziennej budowy z profilu (GBA: wpisana ręcznie; 0 = domyślna z danych).
    inline void daily_date(const profile& p, int& y, int& m, int& d)
    {
        if(p.daily_y == 0 || p.daily_m < 1 || p.daily_m > 12 || p.daily_d < 1) { y = data::daily_default_date[0]; m = data::daily_default_date[1]; d = data::daily_default_date[2]; }
        else { y = p.daily_y; m = p.daily_m; d = imin(p.daily_d, days_in_month(p.daily_y, p.daily_m)); }
    }

    inline void set_daily_date(profile& p, int y, int m, int d)
    {
        p.daily_y = uint16_t(y); p.daily_m = uint8_t(m); p.daily_d = uint8_t(d);
    }

    // Najlepszy wynik dnia (-1 = brak).
    inline int daily_best(const profile& p, int day)
    {
        for(int i = 0; i < data::daily_history; ++i) if(p.daily_day[i] == day && day > 0) return p.daily_score[i];
        return -1;
    }
    inline bool daily_won(const profile& p, int day)
    {
        for(int i = 0; i < data::daily_history; ++i) if(p.daily_day[i] == day && day > 0) return (p.daily_won >> i) & 1;
        return false;
    }

    // Wynik codziennej budowy: najlepszy dnia zostaje; nowy dzień zastępuje najstarszy. Zwraca true = nowy rekord dnia.
    inline bool record_daily(profile& p, int day, int score, bool won)
    {
        if(p.daily_runs < 255) ++p.daily_runs;
        int slot = -1, oldest = 0;
        for(int i = 0; i < data::daily_history; ++i)
        {
            if(p.daily_day[i] == day) { slot = i; break; }
            if(p.daily_day[i] < p.daily_day[oldest]) oldest = i;
        }
        if(slot < 0)
        {
            slot = oldest;
            p.daily_day[slot] = uint16_t(day); p.daily_score[slot] = score;
            p.daily_won = uint8_t((p.daily_won & ~(1u << slot)) | (won ? 1u << slot : 0u));
            return true;
        }
        if(won) p.daily_won = uint8_t(p.daily_won | (1u << slot));
        if(score <= p.daily_score[slot]) return false;
        p.daily_score[slot] = score;
        return true;
    }

    // ------------------------------------------------------------------ harmonogram domu po wygranej
    // Dni etapu z liczby tur (min + tury / turnsPerDay) i data końca etapu, licząc wstecz od daty odbioru.
    inline int schedule_days(const game& g, int s) { return data::schedule_min_days + g.stage_days[s] / data::schedule_turns_per_day; }
    inline int schedule_total_days(const game& g) { int t = 0; for(int s = g.first_stage; s < data::stages_count; ++s) t += schedule_days(g, s); return t; }
    inline int schedule_total_cost(const game& g) { int t = 0; for(int s = g.first_stage; s < data::stages_count; ++s) t += data::stages[s].cost; return t; }
    // Dzień (days_from_civil) rozpoczęcia etapu s, gdy odbiór był w dniu end_day.
    inline int schedule_start_day(const game& g, int s, int end_day)
    {
        int d = end_day - schedule_total_days(g);
        for(int i = g.first_stage; i < s; ++i) d += schedule_days(g, i);
        return d;
    }

    // ------------------------------------------------------------------ po budowie: co najbliżej do kupienia (motywacja)
    // Najtańsze niekupione w Szkoleniach: kind 0 ulepszenie, 1 zawód, 2 narzędzie, 3 brygada, 4 poziom Trudny; -1 = wszystko.
    inline int next_unlock(const profile& p, int& kind, int& index)
    {
        int best = -1;
        auto take = [&](int k, int i, int c) { if(c >= 0 && (best < 0 || c < best)) { best = c; kind = k; index = i; } };
        for(int i = 0; i < data::upgrades_count; ++i) take(0, i, upgrade_cost(p, i));
        for(int i = 0; i < data::classes_count; ++i) if(! class_unlocked(p, i) && ! class_reward(i)) take(1, i, data::class_cost);
        for(int i = 0; i < data::tools_count; ++i) if(! tool_unlocked(p, i) && ! data::tools[i].reward) take(2, i, data::tools[i].cost);
        for(int i = 0; i < data::brigade_count; ++i) if(! helper_unlocked(p, i)) take(3, i, data::brigade[i].cost);
        if(! p.hard) take(4, 0, data::hard_cost);
        return best;
    }

    // ------------------------------------------------------------------ zapis budowy w trakcie
    // Cały stan gry (game jest trywialnie kopiowalny) za profilem w SRAM. Rozmiar i suma kontrolna
    // odrzucają zapisy uszkodzone i z innej wersji gry.
    static_assert(std::is_trivially_copyable_v<game>);
    constexpr char run_magic[8] = "PBRUN10";   // 10: Akt 0 (pieczątki, druga faza bossa); 09: 10 etapów, zachowania, mechaniki aktów
    constexpr int run_save_offset = 256;
    static_assert(sizeof(profile) <= run_save_offset);

    struct run_save
    {
        char magic[8];
        uint32_t size;
        uint32_t checksum;
        game g;
    };

    inline uint32_t run_checksum(const game& g)
    {
        uint32_t h = 2166136261u;   // FNV-1a
        const unsigned char* b = reinterpret_cast<const unsigned char*>(&g);
        for(unsigned i = 0; i < sizeof g; ++i) { h ^= b[i]; h *= 16777619u; }
        return h;
    }

    inline void run_save_make(run_save& s, const game& g)
    {
        std::memcpy(s.magic, run_magic, sizeof s.magic);
        s.size = sizeof(game);
        std::memcpy(&s.g, &g, sizeof g);
        s.checksum = run_checksum(s.g);
    }

    inline bool run_save_valid(const run_save& s)
    {
        return std::memcmp(s.magic, run_magic, sizeof s.magic) == 0 && s.size == sizeof(game) && s.checksum == run_checksum(s.g);
    }

    inline void run_save_clear(run_save& s) { std::memset(s.magic, 0, sizeof s.magic); }
}
