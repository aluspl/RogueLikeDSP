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
    constexpr int max_classes = 12;       // zawody 0-7: bitmaska classes (Szkolenia), 8-11: z nagród za odbiór i sekretnych zleceń
    constexpr int max_respect = 19;       // 16 w respect_ranks + 3 w respect_ranks_hi (v12)
    static_assert(data::classes_count <= max_classes && data::respect_count <= max_respect && data::rewards_count <= 255);

    constexpr char profile_magic[8] = "PBRL017";
    constexpr char profile_magic_v16[8] = "PBRL016";
    constexpr char profile_magic_v15[8] = "PBRL015";
    constexpr char profile_magic_v14[8] = "PBRL014";
    constexpr char profile_magic_v13[8] = "PBRL013";
    constexpr char profile_magic_v12[8] = "PBRL012";
    constexpr char profile_magic_v11[8] = "PBRL011";
    constexpr char profile_magic_v10[8] = "PBRL010";
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
    constexpr int profile_v10_size = 160; // v11 = v10 + wyzwania tygodnia (#34) i fabuła odkrywana z budowami (#35)
    constexpr int profile_v11_size = 188; // v12 = v11 + sekretne zlecenia (#39), wygląd, Respekt 16-18
    constexpr int profile_v12_size = 196; // v13 = v12 + tytuł i kolor kasku (v0.21.52), poziomy Szkoleń od nowa
    constexpr int profile_v13_size = 200; // v14 = v13 + poziom inspektora, mistrzostwo zawodów, druga pamiątka (v0.21.52 cz. b)
    constexpr int profile_v14_size = 240; // v15 = v14 + drzewko Szkoleń, kolekcje, zadania dnia, seria dni (v0.21.52 cz. c)
    constexpr int profile_v15_size = 360; // v16 = v15 + mapa kariery (v0.21.52 cz. d): kontrakty, wygrane, najlepszy etap
    // v17 (v0.21.53) = v16 + filtr ekranu i ogłoszone filtry - w dawnym wyrównaniu v16 (rozmiar bez zmian, 384 B)
    static_assert(data::screen_filters_count <= 16);   // ogłoszone filtry: bity uint16
    constexpr int max_contracts = 6;      // kontrakty mapy kariery (data::career)
    static_assert(data::career_count <= max_contracts);
    constexpr int task_slots = 5;         // v0.21.52 cz. c: 3 zadania dnia + 2 tygodnia
    constexpr int daily_task_slots = 3;
    static_assert(data::tree_nodes_count <= 8 && data::collections_count <= 8 && data::upgrades_count <= 8);
    static_assert(data::secrets_count <= 16 && data::cosmetics_count <= 255);   // wygląd: przełączniki tylko 0-7 (p.cosmetic), kolory kasku w p.helmet
    constexpr int weekly_slots = 3;
    static_assert(data::weekly_history <= weekly_slots && data::story_arc_count <= 32);
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
        uint8_t respect_ranks[16];     // kupione rangi Respektu 0-15
        uint8_t best_stake_hi[4];      // rekord stawki zawodów 8-11
        // --- v9: katalog usterek - rodzaje problemów 16-47 (dalszy ciąg catalog)
        uint32_t catalog_hi;
        // --- v10: samouczek menu - obejrzane dymki (bity tutorial_flag) i zawody z nagród, o których już był dymek
        uint16_t tutorial;
        uint16_t classes_seen;
        // --- v11: wyzwania tygodnia (najlepszy wynik ostatnich tygodni) i fabuła (odblokowane / nieprzeczytane wątki SMS)
        uint16_t weekly_week[weekly_slots];   // numery tygodni z wynikiem (0 = pusty)
        uint8_t weekly_won;            // bity: wygrana w tym tygodniu
        uint8_t weekly_runs;           // rozegrane wyzwania tygodnia (do 255)
        int32_t weekly_score[weekly_slots];
        uint32_t story;                // odblokowane wątki (bity data::story_arc)
        uint32_t story_new;            // jeszcze nieprzeczytane
        // --- v12: sekretne zlecenia (#39) - wykonane i jeszcze nieogłoszone (dymek "Nowość"), wybrany wygląd, Respekt 16-18
        uint16_t secrets;              // wykonane (bity data::secrets)
        uint16_t secrets_new;          // wykonane, dymek na tytule jeszcze nie pokazany
        uint8_t cosmetic;              // wybrany wygląd (bity data::cosmetics; tylko odblokowane działają)
        uint8_t respect_ranks_hi[3];   // kupione rangi Respektu 16-18 (dalszy ciąg respect_ranks)
        // --- v13 (v0.21.52): tytuł z odznaki / zlecenia i kolor kasku (wygląd z odznak i zleceń); Szkolenia z poziomami
        uint8_t title;                 // wybrany tytuł + 1 (0 = bez tytułu): odznaki 0..badges_count-1, potem zlecenia
        uint8_t helmet;                // wybrany kolor kasku: wygląd + 1 (0 = kask zawodu)
        uint8_t reserved13[2];         // wyrównanie do 200 B (profil bez dziur: zapis SRAM bajt po bajcie)
        // --- v14 (v0.21.52 cz. b): poziom inspektora (#44), mistrzostwo zawodów (#45), druga pamiątka (nagroda inspektora)
        uint32_t inspector_xp;         // dośw. inspektora łącznie (poziom z progów data::inspector_levels)
        uint16_t mastery_xp[max_classes];   // dośw. mistrzostwa każdego zawodu (poziom 1-10 z data::mastery_levels)
        uint16_t power_alt;            // bity: wariant mocy włączony (zawód; działa od poziomu mistrzostwa z nagrodą "power")
        uint16_t run_progress;         // ile dośw. inspektora z bieżącej budowy już przeniesiono (znak wodny jak run_kills)
        uint8_t keepsake2;             // druga pamiątka + 1 (0 = bez; slot z poziomu inspektora)
        uint8_t reserved14[7];         // wyrównanie do 240 B
        // --- v15 (v0.21.52 cz. c): drzewko Szkoleń (#46), kolekcje (#49), zadania dnia i tygodnia (#50), seria dni (#51)
        uint16_t tree;                 // wybór w węzłach drzewka: 2 bity na węzeł (0 brak, 1 = opcja A, 2 = opcja B)
        uint8_t kill_count[max_enemy_types];   // kolekcje: pokonane każdego rodzaju (do 255)
        uint8_t kill_mark[max_enemy_types];    // ile z bieżącej budowy już doliczono (znak wodny jak run_kills)
        uint16_t task_day;             // dzień zadań dnia (daily_number), 0 = jeszcze żadnych
        uint16_t task_week;            // tydzień zadań tygodnia (weekly_number)
        uint8_t task_progress[task_slots];   // postęp: 3 zadania dnia, 2 tygodnia
        uint8_t task_mark[task_slots];       // ile z bieżącej budowy już doliczono (znak wodny)
        uint8_t task_done;             // bity: zadanie wykonane (Respekt wydany)
        uint8_t streak;                // seria dni budowy dnia (kolejne dni)
        uint16_t tasks_total;          // wykonane zadania łącznie (nagrody za liczbę)
        uint16_t streak_day;           // ostatni dzień serii (numer budowy dnia)
        uint8_t streak_best;           // najdłuższa seria (nagrody za 3 / 7 / 14 dni)
        uint8_t collections;           // bity: ogłoszone komplety kolekcji (baner raz)
        // --- v16 (v0.21.52 cz. d): mapa kariery (#47)
        uint8_t contract;              // wybrany kontrakt (data::career)
        uint8_t career_seen;           // bity: odblokowanie kontraktu już ogłoszone (baner raz)
        uint8_t career_done;           // bity: kontrakt wygrany (nagroda za pierwszą wygraną wydana)
        uint8_t filter;                // v17 (v0.21.53): wybrany filtr ekranu (data::screen_filters; zablokowany = klasyczny)
        uint8_t career_wins[max_contracts];   // wygrane w każdym kontrakcie (do 255)
        uint8_t career_best[max_contracts];   // najwięcej ukończonych etapów kontraktu w jednej budowie (bez Aktu 0)
        // --- v17 (v0.21.53): filtry ekranu (#53, #54)
        uint16_t filters_seen;         // bity: odblokowanie filtra już ogłoszone (baner raz)
        uint8_t reserved17[6];         // wyrównanie do 384 B (zapis budowy od 512)
    };
    static_assert(offsetof(profile, badges) == profile_v2_size);
    static_assert(offsetof(profile, kills_total) == profile_v3_size);
    static_assert(offsetof(profile, run_kills) == profile_v4_size);
    static_assert(offsetof(profile, brigade) == profile_v5_size);
    static_assert(offsetof(profile, daily_d) == profile_v6_size && offsetof(profile, daily_score) == 104);
    static_assert(offsetof(profile, respect) == profile_v7_size && offsetof(profile, respect_ranks) == 132);
    static_assert(offsetof(profile, catalog_hi) == profile_v8_size && offsetof(profile, tutorial) == profile_v9_size);
    static_assert(offsetof(profile, weekly_week) == profile_v10_size && offsetof(profile, weekly_score) == 168);
    static_assert(offsetof(profile, secrets) == profile_v11_size && offsetof(profile, respect_ranks_hi) == 193);
    static_assert(offsetof(profile, title) == profile_v12_size && offsetof(profile, inspector_xp) == profile_v13_size);
    static_assert(offsetof(profile, power_alt) == 228 && offsetof(profile, keepsake2) == 232 && offsetof(profile, tree) == profile_v14_size);
    static_assert(offsetof(profile, kill_count) == 242 && offsetof(profile, kill_mark) == 290 && offsetof(profile, task_day) == 338);
    static_assert(offsetof(profile, task_progress) == 342 && offsetof(profile, task_mark) == 347 && offsetof(profile, task_done) == 352);
    static_assert(offsetof(profile, tasks_total) == 354 && offsetof(profile, streak_day) == 356 && offsetof(profile, collections) == 359);
    static_assert(offsetof(profile, contract) == profile_v15_size && offsetof(profile, career_wins) == 364 && offsetof(profile, career_best) == 370);
    static_assert(offsetof(profile, filter) == 363 && offsetof(profile, filters_seen) == 376);
    static_assert(sizeof(profile) == 384);

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
    inline int open_classes_won(const profile& p) { int n = 0; for(int c = 0; c < data::open_classes_count; ++c) n += class_won(p, c); return n; }
    inline int best_stake(const profile& p, int c) { return c < 8 ? p.best_stake[c] : p.best_stake_hi[c - 8]; }
    inline void set_best_stake(profile& p, int c, int v) { (c < 8 ? p.best_stake[c] : p.best_stake_hi[c - 8]) = uint8_t(v); }

    // ------------------------------------------------------------------ v0.21.52 cz. b: poziom inspektora (#44), mistrzostwo
    // zawodu (#45), stopnie inwestora (#48). Poziom = ile kolejnych progów (dośw. na poziom z listy) mieści się w dośw.
    inline int progress_level_of(const progress_level* lv, int n, int xp)
    {
        int l = 0, need = 0;
        while(l < n && xp >= need + lv[l].xp) need += lv[l++].xp;
        return l;
    }
    inline int progress_floor(const progress_level* lv, int level) { int t = 0; for(int l = 0; l < level; ++l) t += lv[l].xp; return t; }
    inline int inspector_xp_int(const profile& p) { return p.inspector_xp > 1000000000u ? 1000000000 : int(p.inspector_xp); }   // uszkodzony zapis: nie ujemne
    inline int inspector_level(const profile& p) { return progress_level_of(data::inspector_levels, data::inspector_levels_count, inspector_xp_int(p)); }
    inline int mastery_xp(const profile& p, int c) { return c >= 0 && c < max_classes ? p.mastery_xp[c] : 0; }
    inline int mastery_level(const profile& p, int c) { return progress_level_of(data::mastery_levels, data::mastery_levels_count, mastery_xp(p, c)); }
    // Pasek do kolejnego poziomu: dośw. w poziomie (cur) i potrzebne (need; 0 = maksimum).
    inline void inspector_bar(const profile& p, int& cur, int& need)
    {
        const int l = inspector_level(p);
        need = l < data::inspector_levels_count ? data::inspector_levels[l].xp : 0;
        cur = need ? inspector_xp_int(p) - progress_floor(data::inspector_levels, l) : 0;
    }
    inline void mastery_bar(const profile& p, int c, int& cur, int& need)
    {
        const int l = mastery_level(p, c);
        need = l < data::mastery_levels_count ? data::mastery_levels[l].xp : 0;
        cur = need ? mastery_xp(p, c) - progress_floor(data::mastery_levels, l) : 0;
    }
    // Poziom mistrzostwa, od którego działa nagroda r (99 = żaden).
    inline int mastery_reward_level(progress_reward r)
    {
        for(int l = 0; l < data::mastery_levels_count; ++l) if(data::mastery_levels[l].reward == r) return l + 1;
        return 99;
    }
    inline bool mastery_has(const profile& p, int c, progress_reward r) { return mastery_level(p, c) >= mastery_reward_level(r); }
    // Wariant mocy: odblokowany na poziomie "power", włączany na wyborze zawodu (SELECT: Wygląd / Godot: wiersz Moc).
    inline bool power_variant_on(const profile& p, int c) { return mastery_has(p, c, progress_reward::power) && ((p.power_alt >> c) & 1); }
    inline void toggle_power_variant(profile& p, int c) { if(mastery_has(p, c, progress_reward::power)) p.power_alt = uint16_t(p.power_alt ^ (1u << c)); }
    // Bity mistrzostwa do budowy zawodem c (run_mods::mastery).
    inline int mastery_bits(const profile& p, int c)
    {
        int b = 0;
        if(power_variant_on(p, c)) b |= mastery_bit_power;
        if(mastery_has(p, c, progress_reward::weapon)) b |= mastery_bit_weapon;
        if(mastery_has(p, c, progress_reward::boon)) b |= mastery_bit_boon;
        return b;
    }
    // Stopnie inwestora: najwyższa stawka wygranej budowy (dowolnym zawodem) i ile progów z data::stake_ranks osiągnięto.
    inline int max_stake(const profile& p) { int m = 0; for(int c = 0; c < data::classes_count; ++c) m = imax(m, best_stake(p, c)); return m; }
    inline int stake_rank(const profile& p) { int n = 0; while(n < data::stake_ranks_count && max_stake(p) >= data::stake_ranks[n].xp) ++n; return n; }
    // Poziom inspektora, od którego działa nagroda r (np. slot drugiej pamiątki; 99 = żaden).
    inline int inspector_reward_level(progress_reward r)
    {
        for(int l = 0; l < data::inspector_levels_count; ++l) if(data::inspector_levels[l].reward == r) return l + 1;
        return 99;
    }
    inline bool keepsake_slot2(const profile& p) { return inspector_level(p) >= inspector_reward_level(progress_reward::keepsake_slot); }

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
    // v0.21.49 (profil sprzed v8): nagrody za odbiór za dotychczasowe wygrane (zwrot za zmienione Szkolenia BHP i Kurs
    // fachowy robi teraz migrate_v13 - ta sama kwota: stary koszt poziomu).
    inline void migrate_v8(profile& p)
    {
        p.rewards = uint8_t(imin(imax(0, p.wins), rewards_available()));
    }

    // v12 -> v13 (v0.21.52): Szkolenia mają 4 poziomy z mniejszymi krokami i wyższą ceną - kupione poziomy wracają jako
    // doświadczenie po starej cenie (poziom ponad stare maksimum: refund), poziomy od zera. Zawody, narzędzia, Trudny
    // i brygada zostają. Tytuł i kolor kasku - bez wyboru.
    inline int legacy_refund(const profile& p, int i)
    {
        const upgrade_def& u = data::upgrades[i];
        int t = 0;
        for(int l = 0; l < p.levels[i]; ++l) t += l < u.legacy_levels ? u.legacy_costs[l] : u.refund;
        return t;
    }
    inline void migrate_v13(profile& p)
    {
        for(int i = 0; i < max_upgrades; ++i)
        {
            if(i < data::upgrades_count) p.xp += legacy_refund(p, i);
            p.levels[i] = 0;
        }
        p.title = 0; p.helmet = 0; p.reserved13[0] = p.reserved13[1] = 0;
    }

    inline void migrate_v10(profile& p);
    inline void migrate_v11(profile& p);
    inline void migrate_v12(profile& p);
    inline void migrate_v14(profile& p);
    inline void migrate_v15(profile& p);
    inline void migrate_v16(profile& p);
    inline void migrate_v17(profile& p);
    inline int next_unlock(const profile& p, int& kind, int& index);

    // v0.21.52 cz. c: każda ścieżka migracji kończy się migrate_v15 (drzewko, kolekcje, zadania, seria dni);
    // cz. d: potem migrate_v16 (mapa kariery); v0.21.53: migrate_v17 (filtry ekranu).
    inline bool profile_fix(profile& p)
    {
        if(std::memcmp(p.magic, profile_magic, sizeof p.magic) == 0) return clamp_levels(p);
        if(std::memcmp(p.magic, profile_magic_v16, sizeof p.magic) == 0)   // v16 -> v17: filtr ekranu klasyczny, filtry do ogłoszenia
        {
            std::memcpy(p.magic, profile_magic, sizeof p.magic);
            migrate_v17(p);
            return true;
        }
        if(std::memcmp(p.magic, profile_magic_v15, sizeof p.magic) == 0)   // v15 -> v16: Dom jednorodzinny z dotychczasowych wygranych
        {
            std::memset(reinterpret_cast<char*>(&p) + profile_v15_size, 0, sizeof p - profile_v15_size);
            std::memcpy(p.magic, profile_magic, sizeof p.magic);
            migrate_v16(p);
            migrate_v17(p);
            return true;
        }
        if(std::memcmp(p.magic, profile_magic_v14, sizeof p.magic) == 0)   // v14 -> v15: kolekcje z Katalogu, seria z wyników dni
        {
            std::memset(reinterpret_cast<char*>(&p) + profile_v14_size, 0, sizeof p - profile_v14_size);
            std::memcpy(p.magic, profile_magic, sizeof p.magic);
            migrate_v15(p);
            migrate_v16(p);
            migrate_v17(p);
            return true;
        }
        if(std::memcmp(p.magic, profile_magic_v13, sizeof p.magic) == 0)   // v13 -> v14: inspektor i mistrzostwo z dotychczasowych statystyk
        {
            std::memset(reinterpret_cast<char*>(&p) + profile_v13_size, 0, sizeof p - profile_v13_size);
            std::memcpy(p.magic, profile_magic, sizeof p.magic);
            migrate_v14(p);
            migrate_v15(p);
            migrate_v16(p);
            migrate_v17(p);
            return true;
        }
        if(std::memcmp(p.magic, profile_magic_v12, sizeof p.magic) == 0)   // v12 -> v13: zwrot za Szkolenia, tytuł i kask od zera
        {
            std::memset(reinterpret_cast<char*>(&p) + profile_v12_size, 0, sizeof p - profile_v12_size);
            std::memcpy(p.magic, profile_magic, sizeof p.magic);
            migrate_v13(p);
            migrate_v14(p);
            migrate_v15(p);
            migrate_v16(p);
            migrate_v17(p);
            return true;
        }
        if(std::memcmp(p.magic, profile_magic_v11, sizeof p.magic) == 0)   // v11 -> v12: sekretne zlecenia z tego, co już widać w profilu
        {
            std::memset(reinterpret_cast<char*>(&p) + profile_v11_size, 0, sizeof p - profile_v11_size);
            std::memcpy(p.magic, profile_magic, sizeof p.magic);
            migrate_v13(p);
            migrate_v12(p);
            migrate_v14(p);
            migrate_v15(p);
            migrate_v16(p);
            migrate_v17(p);
            return true;
        }
        if(std::memcmp(p.magic, profile_magic_v10, sizeof p.magic) == 0)   // v10 -> v11: wyzwania tygodnia i fabuła od zera
        {
            std::memset(reinterpret_cast<char*>(&p) + profile_v10_size, 0, sizeof p - profile_v10_size);
            std::memcpy(p.magic, profile_magic, sizeof p.magic);
            migrate_v13(p);
            migrate_v11(p);
            migrate_v12(p);
            migrate_v14(p);
            migrate_v15(p);
            migrate_v16(p);
            migrate_v17(p);
            return true;
        }
        bool v9 = std::memcmp(p.magic, profile_magic_v9, sizeof p.magic) == 0;
        if(v9 || std::memcmp(p.magic, profile_magic_v8, sizeof p.magic) == 0)   // v8 -> v9: katalog 16-47 od zera; v9 -> v10: samouczek
        {
            int keep = v9 ? profile_v9_size : profile_v8_size;
            std::memset(reinterpret_cast<char*>(&p) + keep, 0, sizeof p - keep);
            std::memcpy(p.magic, profile_magic, sizeof p.magic);
            migrate_v13(p);
            migrate_v10(p);
            migrate_v11(p);
            migrate_v12(p);
            migrate_v14(p);
            migrate_v15(p);
            migrate_v16(p);
            migrate_v17(p);
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
            migrate_v13(p);
            migrate_v8(p);
            migrate_v10(p);
            migrate_v11(p);
            migrate_v12(p);
            migrate_v14(p);
            migrate_v15(p);
            migrate_v16(p);
            migrate_v17(p);
            return true;
        }
        if(std::memcmp(p.magic, profile_magic_v1, sizeof p.magic) == 0)
        {
            int32_t best = p.best, runs = p.runs, wins = p.wins;
            profile_reset(p);
            p.best = best; p.runs = runs; p.wins = wins;
            migrate_v8(p);
            migrate_v10(p);
            migrate_v11(p);
            migrate_v12(p);
            migrate_v14(p);
            migrate_v15(p);
            migrate_v16(p);
            migrate_v17(p);
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
    enum tutorial_unlock : int { unlock_respect, unlock_daily, unlock_investor, unlock_act0, unlock_class, unlock_secret };   // = data::tutorial_unlocks
    static_assert(data::tutorial_unlocks_count == 6);

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
    // Dymek odblokowania do pokazania na ekranie (-1 = brak); cls = nowy zawód z nagrody (unlock_class) albo wykonane
    // sekretne zlecenie (unlock_secret, v0.21.51 cz. 2).
    inline int pending_unlock(const profile& p, int screen, int& cls)
    {
        cls = -1;
        if(tutorial_pending(p, screen)) return -1;   // najpierw główny samouczek
        if(screen == 0)
        {
            if(p.respect_total > 0 && ! (p.tutorial & tut_respect)) return unlock_respect;
            if(p.runs > 0 && ! (p.tutorial & tut_daily)) return unlock_daily;
            if(act0_unlocked(p) && ! (p.tutorial & tut_act0)) return unlock_act0;
            for(int i = 0; i < data::secrets_count; ++i) if((p.secrets_new >> i) & 1) { cls = i; return unlock_secret; }
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
        if(u == unlock_secret && cls >= 0) p.secrets_new = uint16_t(p.secrets_new & ~(1u << cls));
    }

    // ------------------------------------------------------------------ sekretne zlecenia (#39, v0.21.51 cz. 2)
    inline bool secret_done(const profile& p, int i) { return (p.secrets >> i) & 1; }
    // Nagroda z wykonanego sekretnego zlecenia (zawód, narzędzie, wygląd, Respekt).
    inline bool secret_owned(const profile& p, secret_reward k, int index)
    {
        for(int i = 0; i < data::secrets_count; ++i)
            if(data::secrets[i].reward == k && data::secrets[i].index == index && secret_done(p, i)) return true;
        return false;
    }
    // Sekretne zlecenie, które daje tę nagrodę (-1 = żadne).
    inline int secret_of(secret_reward k, int index)
    {
        for(int i = 0; i < data::secrets_count; ++i) if(data::secrets[i].reward == k && data::secrets[i].index == index) return i;
        return -1;
    }
    inline bool collection_complete(const profile& p, int i);   // v0.21.52 cz. c (niżej)
    // Nagroda z listy progress_level za osiągnięty próg (zadania łącznie, seria dni): wygląd k / tytuł / pamiątka.
    inline bool goal_helmet(const progress_level* lv, int n, int have, int k)
    {
        for(int l = 0; l < n; ++l) if(lv[l].reward == progress_reward::helmet && lv[l].index == k && have >= lv[l].xp) return true;
        return false;
    }

    // Wygląd: z sekretnego zlecenia albo (v0.21.52) z odznaki / zlecenia - kolory kasku.
    inline bool cosmetic_unlocked(const profile& p, int k)
    {
        if(k < 0) return false;
        if(secret_owned(p, secret_reward::cosmetic, k)) return true;
        // v0.21.52 cz. c: kolekcje, zadania łącznie, seria dni
        for(int i = 0; i < data::collections_count; ++i)
            if(data::collections[i].reward.reward == progress_reward::helmet && data::collections[i].reward.index == k && collection_complete(p, i)) return true;
        if(goal_helmet(data::task_rewards, data::task_rewards_count, p.tasks_total, k)) return true;
        if(goal_helmet(data::streak_rewards, data::streak_rewards_count, p.streak_best, k)) return true;
        for(int i = 0; i < data::badges_count; ++i) if(data::badges[i].cosmetic == k && (p.badges & (1u << i))) return true;
        for(int i = 0; i < data::contracts_count; ++i) if(data::contracts[i].cosmetic == k && (p.contracts & (1u << i))) return true;
        for(int c = 0; c < data::career_count; ++c) if(data::career[c].helmet == k && ((p.career_done >> c) & 1)) return true;   // cz. d
        // v0.21.52 cz. b: poziom inspektora, stopnie inwestora, kask mistrza (dowolny zawód na poziomie z nagrodą "helmet")
        for(int l = 0; l < data::inspector_levels_count; ++l)
            if(data::inspector_levels[l].reward == progress_reward::helmet && data::inspector_levels[l].index == k) return inspector_level(p) > l;
        for(int l = 0; l < data::stake_ranks_count; ++l)
            if(data::stake_ranks[l].reward == progress_reward::helmet && data::stake_ranks[l].index == k) return stake_rank(p) > l;
        for(int l = 0; l < data::mastery_levels_count; ++l)
            if(data::mastery_levels[l].reward == progress_reward::helmet && data::mastery_levels[l].index == k)
            {
                for(int c = 0; c < data::classes_count; ++c) if(mastery_level(p, c) > l) return true;
                return false;
            }
        return false;
    }
    inline bool cosmetic_helmet(int k) { return k >= 0 && k < data::cosmetics_count && data::cosmetics[k].helmet >= 0; }
    // Kask mistrza zawodu (nagroda mistrzostwa): działa tylko zawodem z tym poziomem.
    inline bool mastery_helmet(int k)
    {
        for(int l = 0; l < data::mastery_levels_count; ++l)
            if(data::mastery_levels[l].reward == progress_reward::helmet && data::mastery_levels[l].index == k) return true;
        return false;
    }
    // Kolor kasku na budowie: wybrany i odblokowany wygląd (-1 = kask zawodu). Kask w paski ma pierwszeństwo.
    // cls >= 0: kask mistrza tylko zawodem, który ma ten poziom mistrzostwa (inny zawód - kask zawodu).
    inline int helmet_cosmetic(const profile& p, int cls = -1)
    {
        const int k = int(p.helmet) - 1;
        if(! cosmetic_helmet(k) || ! cosmetic_unlocked(p, k)) return -1;
        if(cls >= 0 && mastery_helmet(k) && ! mastery_has(p, cls, progress_reward::helmet)) return -1;
        return k;
    }
    // Wybór koloru kasku (wybór zawodu, Wygląd): kolejny odblokowany albo kask zawodu.
    inline void cycle_helmet(profile& p, int dir)
    {
        const int n = data::cosmetics_count + 1;
        int k = p.helmet;
        for(int i = 0; i < n; ++i)
        {
            k = (k + dir + n) % n;
            if(k == 0 || (cosmetic_helmet(k - 1) && cosmetic_unlocked(p, k - 1))) break;
        }
        p.helmet = uint8_t(k);
    }
    inline int helmets_unlocked(const profile& p) { int n = 0; for(int k = 0; k < data::cosmetics_count; ++k) n += cosmetic_helmet(k) && cosmetic_unlocked(p, k); return n; }

    // ------------------------------------------------------------------ tytuły (v0.21.52, #43): z odznak i zleceń
    // Tytuł t: 0..badges_count-1 = odznaki, dalej zlecenia. Wybór w profilu, widać go w profilu i na końcu budowy.
    // v0.21.52 cz. b: dalej tytuły z poziomu inspektora i stopni inwestora (data::progress_titles).
    constexpr int titles_count = data::badges_count + data::contracts_count + data::progress_titles_count;
    constexpr int progress_titles_from = data::badges_count + data::contracts_count;
    static_assert(titles_count < 255);
    inline const char* title_name(int t)
    {
        if(t >= progress_titles_from) return data::progress_titles[t - progress_titles_from].name;
        return t < data::badges_count ? data::badges[t].title : data::contracts[t - data::badges_count].title;
    }
    inline bool title_owned(const profile& p, int t)
    {
        if(t >= progress_titles_from)
        {
            const progress_title& pt = data::progress_titles[t - progress_titles_from];
            switch(pt.source)
            {
                case 0:  return inspector_level(p) >= pt.level;
                case 1:  return max_stake(p) >= pt.level;
                case 2:  return collection_complete(p, pt.level - 1);   // v0.21.52 cz. c: kolekcje, seria dni, zadania
                case 3:  return p.streak_best >= pt.level;
                case 5:  return (p.career_done >> pt.level) & 1;   // v0.21.52 cz. d: kontrakt mapy kariery wygrany
                default: return p.tasks_total >= pt.level;
            }
        }
        return t < data::badges_count ? (p.badges >> t) & 1 : (p.contracts >> (t - data::badges_count)) & 1;
    }
    // Tytuł z nagrody poziomu inspektora (level 1..) albo stopnia inwestora (stawka) - indeks t (-1 = brak).
    inline int progress_title_index(int source, int level)
    {
        for(int i = 0; i < data::progress_titles_count; ++i)
            if(data::progress_titles[i].source == source && data::progress_titles[i].level == level) return progress_titles_from + i;
        return -1;
    }
    inline int titles_owned(const profile& p) { int n = 0; for(int t = 0; t < titles_count; ++t) n += title_owned(p, t); return n; }
    // Wybrany tytuł (-1 = bez tytułu albo już nie należy do gracza).
    inline int selected_title(const profile& p)
    {
        const int t = int(p.title) - 1;
        return t >= 0 && t < titles_count && title_owned(p, t) ? t : -1;
    }
    inline void cycle_title(profile& p, int dir)
    {
        const int n = titles_count + 1;
        int t = p.title;
        for(int i = 0; i < n; ++i)
        {
            t = (t + dir + n) % n;
            if(t == 0 || title_owned(p, t - 1)) break;
        }
        p.title = uint8_t(t);
    }
    // Wygląd na budowie: wybrany i odblokowany (kask w paski - wybór zawodu; złota kielnia - zawsze po odblokowaniu).
    // Kolory kasku (v0.21.52) nie są przełącznikami - wybór w p.helmet (helmet_cosmetic).
    inline bool cosmetic_on(const profile& p, int k) { return ! cosmetic_helmet(k) && cosmetic_unlocked(p, k) && (k == data::cosmetic_gold || ((p.cosmetic >> k) & 1)); }
    inline void toggle_cosmetic(profile& p, int k) { if(! cosmetic_helmet(k) && cosmetic_unlocked(p, k)) p.cosmetic = uint8_t(p.cosmetic ^ (1u << k)); }

    // Zawód: startowy / kupiony w Szkoleniach (bitmaska), z nagrody za odbiór albo z sekretnego zlecenia.
    inline bool class_secret(int c) { return (data::secret_classes_mask >> c) & 1; }
    inline bool class_reward(int c) { return ((data::reward_classes_mask | data::secret_classes_mask) >> c) & 1; }   // nie na sprzedaż
    inline bool class_unlocked(const profile& p, int c)
    {
        if(class_secret(c)) return secret_owned(p, secret_reward::cls, c);
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

    // v0.21.52: zawody i narzędzia drożeją z każdym zakupem - cena kolejnego z listy (ostatnia, gdy kupiono więcej).
    inline bool class_for_sale(int c) { return ! (data::start_classes_mask & (1 << c)) && ! class_reward(c); }
    inline int classes_bought(const profile& p)
    {
        int n = 0;
        for(int c = 0; c < data::classes_count; ++c) n += class_for_sale(c) && (p.classes & (1u << c));
        return n;
    }
    inline int class_price(int bought) { return data::class_costs[imin(bought, data::class_costs_count - 1)]; }
    inline int class_cost(const profile& p) { return class_price(classes_bought(p)); }
    inline int tools_bought(const profile& p)
    {
        int n = 0;
        for(int i = 0; i < data::tools_count; ++i) n += data::tools[i].shop && ((p.tools >> i) & 1);
        return n;
    }
    inline int tool_price(int bought) { return data::tool_costs[imin(bought, data::tool_costs_count - 1)]; }
    inline int tool_cost(const profile& p) { return tool_price(tools_bought(p)); }

    inline bool buy_upgrade(profile& p, int i)
    {
        int c = upgrade_cost(p, i);
        if(c < 0 || p.xp < c) return false;
        p.xp -= c; ++p.levels[i];
        return true;
    }

    // ------------------------------------------------------------------ v0.21.52 cz. c (#46): drzewko Szkoleń
    // Gałęzie Fach / BHP / Logistyka: pień = Szkolenia (poziomy kupowane jak dotąd), węzeł otwiera się po depth poziomach
    // pnia gałęzi; w węźle wybór 1 z 2 opcji za dośw. (cost), zmiana wyboru za data::tree_respec_cost.
    inline void add_upgrade(run_mods& m, upgrade_effect e, int v);
    inline int tree_branch_levels(const profile& p, int b)
    {
        int n = 0;
        for(int i = 0; i < data::upgrades_count; ++i) if((data::tree_branches[b].upgrades >> i) & 1) n += p.levels[i];
        return n;
    }
    inline int tree_branch_max(int b)
    {
        int n = 0;
        for(int i = 0; i < data::upgrades_count; ++i) if((data::tree_branches[b].upgrades >> i) & 1) n += data::upgrades[i].levels;
        return n;
    }
    inline int tree_branch_of(int upgrade) { for(int b = 0; b < data::tree_branches_count; ++b) if((data::tree_branches[b].upgrades >> upgrade) & 1) return b; return 0; }
    // Wybrana opcja węzła n: 0 = brak, 1 = A, 2 = B.
    inline int tree_pick(const profile& p, int n) { const int v = (p.tree >> (2 * n)) & 3; return v <= 2 ? v : 0; }
    inline bool tree_open(const profile& p, int n) { const tree_node& tn = data::tree_nodes[n]; return tree_branch_levels(p, tn.branch) >= tn.depth; }
    // Koszt wybrania opcji o (0/1) węzła n: pierwszy wybór - cost, zmiana - opłata; -1 = już wybrana.
    inline int tree_cost(const profile& p, int n, int o)
    {
        const int pk = tree_pick(p, n);
        if(pk == o + 1) return -1;
        return pk == 0 ? data::tree_nodes[n].cost : data::tree_respec_cost;
    }
    inline bool tree_choose(profile& p, int n, int o)
    {
        const int c = tree_cost(p, n, o);
        if(n < 0 || n >= data::tree_nodes_count || o < 0 || o > 1 || c < 0 || ! tree_open(p, n) || p.xp < c) return false;
        p.xp -= c;
        p.tree = uint16_t((p.tree & ~(3u << (2 * n))) | (unsigned(o + 1) << (2 * n)));
        return true;
    }
    inline int tree_picked(const profile& p) { int k = 0; for(int n = 0; n < data::tree_nodes_count; ++n) k += tree_pick(p, n) > 0; return k; }
    inline void add_tree(run_mods& m, const profile& p)
    {
        for(int n = 0; n < data::tree_nodes_count; ++n)
        {
            const int pk = tree_pick(p, n);
            if(pk > 0) add_upgrade(m, data::tree_nodes[n].options[pk - 1].effect, data::tree_nodes[n].options[pk - 1].value);
        }
    }

    inline bool buy_class(profile& p, int c)
    {
        const int cost = class_cost(p);
        if(class_reward(c) || class_unlocked(p, c) || p.xp < cost) return false;
        p.xp -= cost; p.classes = uint8_t(p.classes | (1u << c));
        return true;
    }

    inline int tools_mask(const profile& p)
    {
        int m = p.tools | data::start_tools_mask;
        for(int i = 0; i < data::tools_count; ++i)
        {
            if(data::tools[i].reward) m = reward_unlocked(p, reward_kind::tool, i) ? m | (1 << i) : m & ~(1 << i);
            if(data::tools[i].secret) m = secret_owned(p, secret_reward::tool, i) ? m | (1 << i) : m & ~(1 << i);
        }
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
        const int cost = tool_cost(p);
        if(! data::tools[i].shop || tool_unlocked(p, i) || p.xp < cost) return false;
        p.xp -= cost; p.tools = uint8_t(p.tools | (1u << i));
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
        if(kd.streak > 0 && p.streak_best >= kd.streak) return true;   // v0.21.52 cz. c: seria dni budowy dnia
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

    // v0.21.52 cz. b: druga pamiątka (slot z poziomu inspektora) - inna niż pierwsza, działa na randze I; -1 = brak.
    inline perk keepsake2_perk(int k) { return { data::keepsakes[k].effect, data::keepsakes[k].values[0] }; }
    inline int selected_keepsake2(const profile& p)
    {
        const int k = int(p.keepsake2) - 1;
        return keepsake_slot2(p) && k >= 0 && k < data::keepsakes_count && keepsake_unlocked(p, k) && k != selected_keepsake(p) ? k : -1;
    }
    inline void cycle_keepsake2(profile& p, int dir)
    {
        if(! keepsake_slot2(p)) return;
        int n = data::keepsakes_count + 1, k = p.keepsake2;
        for(int i = 0; i < n; ++i)
        {
            k = (k + dir + n) % n;
            if(k == 0 || (keepsake_unlocked(p, k - 1) && k - 1 != selected_keepsake(p))) break;
        }
        p.keepsake2 = uint8_t(k);
    }

    // Start budowy: licznik budów i budów z wybraną pamiątką (mods() wołać wcześniej - ranga z budów przed tą),
    // nowa budowa nie ma jeszcze nic przeniesionego do liczników zleceń.
    inline void start_run(profile& p)
    {
        ++p.runs;
        p.run_kills = 0; p.run_powers = 0; p.run_brand = 0; p.run_clean = 0; p.run_respect = 0;
        p.run_progress = 0;   // v0.21.52 cz. b: dośw. inspektora z nowej budowy - nic jeszcze nie przeniesiono
        for(auto& k : p.kill_mark) k = 0;   // v0.21.52 cz. c: kolekcje i zadania - znak wodny nowej budowy
        for(auto& k : p.task_mark) k = 0;
        int k = selected_keepsake(p);
        if(k >= 0 && p.keepsake_runs[k] < 255) ++p.keepsake_runs[k];
        int k2 = selected_keepsake2(p);
        if(k2 >= 0 && p.keepsake_runs[k2] < 255) ++p.keepsake_runs[k2];
    }

    // ------------------------------------------------------------------ Respekt (telefon profilu, strona Respekt)
    inline uint8_t respect_slot(const profile& p, int i) { return i < 16 ? p.respect_ranks[i] : p.respect_ranks_hi[i - 16]; }
    inline void set_respect_rank(profile& p, int i, int r) { (i < 16 ? p.respect_ranks[i] : p.respect_ranks_hi[i - 16]) = uint8_t(r); }
    // Ranga z sekretnego zlecenia (Zaprawiony w boju) - dopiero po jego wykonaniu.
    inline bool respect_unlocked(const profile& p, int i) { return data::respect[i].secret < 0 || secret_done(p, data::respect[i].secret); }
    inline int respect_rank(const profile& p, int i) { return imin(respect_slot(p, i), data::respect[i].ranks); }
    // Koszt kolejnej rangi; -1 = maksymalna.
    inline int respect_cost(const profile& p, int i)
    {
        int r = respect_rank(p, i);
        return r < data::respect[i].ranks ? data::respect[i].costs[r] : -1;
    }
    inline bool buy_respect(profile& p, int i)
    {
        int c = respect_cost(p, i);
        if(c < 0 || p.respect < c || ! respect_unlocked(p, i)) return false;
        p.respect = uint16_t(p.respect - c); set_respect_rank(p, i, respect_slot(p, i) + 1);
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

    // Premia jednego poziomu Szkolenia (v0.21.52: poziomy mogą mieć różne działanie).
    inline void add_upgrade(run_mods& m, upgrade_effect e, int v)
    {
        switch(e)
        {
            case upgrade_effect::hp:        m.hp += v; break;
            case upgrade_effect::def:       m.def += v; break;
            case upgrade_effect::dmg:       m.dmg += v; break;
            case upgrade_effect::coffee:    m.coffee += v; break;
            case upgrade_effect::pickups:   m.pickups += v; break;
            case upgrade_effect::luck:      m.luck += v; break;
            case upgrade_effect::craft:     m.craft += v; break;
            case upgrade_effect::dmg_pct:   m.dmg_pct += v; break;
            case upgrade_effect::taken_pct: m.taken_pct += v; break;
            case upgrade_effect::crit:      m.crit += v; break;
            case upgrade_effect::dodge:     m.dodge += v; break;
            case upgrade_effect::thermos:   m.thermos += v; break;
            case upgrade_effect::mats_pct:  m.mats_pct += v; break;
            case upgrade_effect::gear_pct:  m.gear_pct += v; break;
            case upgrade_effect::cash:      m.cash += v; break;
            case upgrade_effect::shop_pct:    m.shop_pct += v; break;      // v0.21.52 cz. c: węzły drzewka
            case upgrade_effect::brigade_pct: m.brigade_pct += v; break;
            case upgrade_effect::cooldown:    m.cooldown += v; break;
            case upgrade_effect::first_hit:   m.mastery += imin(15, v) << first_hit_shift; break;   // Siła rozpędu (jeden węzeł)
            default: break;
        }
    }
    // Suma kupionych poziomów Szkolenia i (premie wszystkich poziomów 1..levels).
    inline void add_upgrade_levels(run_mods& m, int i, int levels)
    {
        const upgrade_def& u = data::upgrades[i];
        for(int l = 0; l < levels && l < u.levels; ++l) add_upgrade(m, u.steps[l].effect, u.steps[l].value);
    }
    // Łączna wartość działania e z poziomów 1..levels Szkolenia i (opis w sklepie).
    inline int upgrade_total(int i, int levels, upgrade_effect e)
    {
        const upgrade_def& u = data::upgrades[i];
        int t = 0;
        for(int l = 0; l < levels && l < u.levels; ++l) if(u.steps[l].effect == e) t += u.steps[l].value;
        return t;
    }

    // Skutek poziomu Szkolenia, np. "+1 HP", "Kryt +1%" (sklep Szkolenia, rada końca budowy).
    inline message& upgrade_label(message& m, upgrade_effect e, int v)
    {
        switch(e)
        {
            case upgrade_effect::hp:        return m.add("+").add(v).add(" HP na start");
            case upgrade_effect::def:       return m.add("+").add(v).add(" OBR");
            case upgrade_effect::dmg:       return m.add("+").add(v).add(" obrażeń");
            case upgrade_effect::coffee:    return m.add("Kawa leczy +").add(v).add(" HP");
            case upgrade_effect::pickups:   return m.add("+").add(v).add(v == 1 ? " znajdźka" : " znajdźki");
            case upgrade_effect::luck:      return m.add("+").add(v).add(" szczęścia");
            case upgrade_effect::craft:     return m.add("+").add(v).add(" stat. broni");
            case upgrade_effect::dmg_pct:   return m.add("+").add(v).add("% obrażeń");
            case upgrade_effect::taken_pct: return m.add("-").add(v).add("% otrzym. obr.");
            case upgrade_effect::crit:      return m.add("Kryt +").add(v).add("%");
            case upgrade_effect::dodge:     return m.add("Unik +").add(v).add("%");
            case upgrade_effect::thermos:   return m.add("Termos +").add(v).add(v == 1 ? " miejsce" : " miejsca");
            case upgrade_effect::mats_pct:  return m.add("Materiały +").add(v).add("%");
            case upgrade_effect::gear_pct:  return m.add("Sprzęt +").add(v);
            case upgrade_effect::cash:      return m.add("Budżet +").add(v).add(" zł");
            case upgrade_effect::shop_pct:    return m.add("Hurtownia -").add(v).add("%");
            case upgrade_effect::brigade_pct: return m.add("Brygada -").add(v).add("%");
            case upgrade_effect::cooldown:    return m.add("Moc -").add(v).add(" t.");
            case upgrade_effect::first_hit:   return m.add("Pierwszy cios +").add(v);
            default:                        return m;
        }
    }
    // Razem z poziomów 1..levels Szkolenia i, działania w kolejności poziomów ("+2 HP na start"; "Kryt +1%, +1 szczęścia").
    inline message& upgrade_summary(message& m, int i, int levels)
    {
        const upgrade_def& u = data::upgrades[i];
        bool any = false;
        for(int l = 0; l < levels && l < u.levels; ++l)
        {
            bool seen = false;
            for(int k = 0; k < l; ++k) seen |= u.steps[k].effect == u.steps[l].effect;
            if(seen) continue;
            if(any) m.add(", ");
            upgrade_label(m, u.steps[l].effect, upgrade_total(i, levels, u.steps[l].effect));
            any = true;
        }
        return m;
    }

    inline run_mods mods(const profile& p)
    {
        run_mods m;
        m.tools = tools_mask(p);
        m.helpers = helpers_mask(p);
        for(int i = 0; i < data::upgrades_count; ++i) add_upgrade_levels(m, i, p.levels[i]);
        add_tree(m, p);   // v0.21.52 cz. c: wybrane węzły drzewka Szkoleń
        for(int i = 0; i < data::collections_count; ++i)   // v0.21.52 cz. c: komplety kolekcji ze stałą premią
            if(data::collections[i].reward.reward == progress_reward::perk && collection_complete(p, i)) add_perk(m, data::collections[i].bonus);
        for(int i = 0; i < data::respect_count; ++i)   // Respekt: kupione rangi
            if(respect_rank(p, i) > 0) add_respect(m, data::respect[i].effect, respect_value(p, i));
        m.gear_slots = gear_slots_mask(p);   // nagrody za odbiór: buty, pas
        m.act0 = act0_unlocked(p) ? 1 : 0;   // nagroda za odbiór: Akt 0 przed budową
        for(int i = 0; i < data::badges_count; ++i)   // uprawnienia z zdobytych odznak
            if(p.badges & (1u << i)) add_perk(m, data::badges[i].bonus);
        int k = selected_keepsake(p);   // pamiątka zabrana na budowę
        if(k >= 0) add_perk(m, keepsake_perk(p, k));
        int k2 = selected_keepsake2(p);   // v0.21.52 cz. b: druga pamiątka (poziom inspektora) - zawsze na randze I
        if(k2 >= 0) add_perk(m, keepsake2_perk(k2));
        m.investor = investor_mask(p);   // tryb inwestora: modyfikatory i premia doświadczenia
        m.xp_pct += investor_xp(m.investor);
        return m;
    }
    // Premie na budowę zawodem cls: mods() + mistrzostwo zawodu (wariant mocy, broń mistrza, premia w ofercie).
    inline run_mods mods(const profile& p, int cls)
    {
        run_mods m = mods(p);
        m.mastery |= mastery_bits(p, cls);   // v0.21.52 cz. c: bity 8-11 - Siła rozpędu z drzewka
        return m;
    }

    // Premie profilu z jednego źródła (rozpiska obrażeń #26): 0 Szkolenia, 1 Respekt, 2 odznaki, 3 pamiątka
    // (core::mods_source_name). Suma premii bojowych = mods() (bez narzędzi, brygady, slotów i trybu inwestora).
    inline run_mods mods_part(const profile& p, int src)
    {
        run_mods m;
        if(src == 0)
            for(int i = 0; i <= data::upgrades_count; ++i)
            {
                run_mods u;
                if(i < data::upgrades_count) add_upgrade_levels(u, i, p.levels[i]);   // premie bojowe (bez kawy, termosu, znajdziek, materiałów, sprzętu, zł)
                else add_tree(u, p);   // v0.21.52 cz. c: węzły drzewka (Szkolenia)
                m.hp += u.hp; m.def += u.def; m.dmg += u.dmg; m.luck += u.luck; m.craft += u.craft;
                m.dmg_pct += u.dmg_pct; m.taken_pct += u.taken_pct; m.crit += u.crit; m.dodge += u.dodge;
            }
        else if(src == 1)
        {
            for(int i = 0; i < data::respect_count; ++i)
                if(respect_rank(p, i) > 0) add_respect(m, data::respect[i].effect, respect_value(p, i));
        }
        else if(src == 2)
        {
            for(int i = 0; i < data::badges_count; ++i)
                if(p.badges & (1u << i)) add_perk(m, data::badges[i].bonus);
        }
        else
        {
            int k = selected_keepsake(p);
            if(k >= 0) add_perk(m, keepsake_perk(p, k));
            int k2 = selected_keepsake2(p);
            if(k2 >= 0) add_perk(m, keepsake2_perk(k2));
        }
        return m;
    }
    inline void mods_parts(const profile& p, run_mods (&out)[mods_sources])
    {
        for(int s = 0; s < mods_sources; ++s) out[s] = mods_part(p, s);
    }

    // Ekran "Koszty": ile kosztuje cały sklep i ile już wydano (pasek budżetu).
    // v0.21.52: zawody i narzędzia liczone po cenach rosnących (kolejność zakupu nie zmienia sumy).
    inline int shop_total_cost()
    {
        int t = data::hard_cost, nc = 0, nt = 0;
        for(int i = 0; i < data::upgrades_count; ++i)
            for(int l = 0; l < data::upgrades[i].levels; ++l) t += data::upgrades[i].costs[l];
        for(int i = 0; i < data::classes_count; ++i) if(class_for_sale(i)) t += class_price(nc++);
        for(int i = 0; i < data::tools_count; ++i) if(data::tools[i].shop) t += tool_price(nt++);
        for(int i = 0; i < data::brigade_count; ++i) t += data::brigade[i].cost;
        for(int n = 0; n < data::tree_nodes_count; ++n) t += data::tree_nodes[n].cost;   // v0.21.52 cz. c: węzły drzewka
        return t;
    }

    inline int shop_spent(const profile& p)
    {
        int t = p.hard ? data::hard_cost : 0;
        for(int i = 0; i < data::upgrades_count; ++i)
            for(int l = 0; l < p.levels[i]; ++l) t += data::upgrades[i].costs[l];
        for(int k = 0, n = classes_bought(p); k < n; ++k) t += class_price(k);
        for(int k = 0, n = tools_bought(p); k < n; ++k) t += tool_price(k);
        for(int i = 0; i < data::brigade_count; ++i) if(helper_unlocked(p, i)) t += data::brigade[i].cost;
        for(int n = 0; n < data::tree_nodes_count; ++n) if(tree_pick(p, n) > 0) t += data::tree_nodes[n].cost;   // bez opłat za zmianę
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

    // ------------------------------------------------------------------ v0.21.52 cz. b: nagrody za poziomy (#44, #45, #48)
    // Respekt przychodzi w chwili osiągnięcia poziomu (raz - poziom tylko rośnie); tytuły, kolory kasku, wątki SMS, ozdoby,
    // slot pamiątki, wariant mocy, broń mistrza i premia mistrzostwa działają od poziomu (liczone z dośw.).
    inline void grant_level(profile& p, const progress_level& l)
    {
        if(l.reward == progress_reward::respect) { p.respect = add_sat16(p.respect, l.value); p.respect_total = add_sat16(p.respect_total, l.value); }
    }
    inline int grant_stake_ranks(profile& p, int before)
    {
        const int now = stake_rank(p);
        for(int l = before; l < now; ++l) grant_level(p, data::stake_ranks[l]);
        return now - before;
    }

    inline void bank_collections(profile& p, const game& g);   // v0.21.52 cz. c (niżej)
    inline void career_record(profile& p, const game& g);      // v0.21.52 cz. d (niżej)

    // Przenosi do profilu trwałe osiągnięcia budowy (katalog, narzędzia, wygrane zawody, liczniki zleceń).
    // Można wołać wielokrotnie.
    inline void record_run(profile& p, game& g)
    {
        bank_counters(p, g);
        bank_collections(p, g);   // v0.21.52 cz. c: liczniki kolekcji (znak wodny kill_mark)
        career_record(p, g);      // v0.21.52 cz. d: najlepszy etap kontraktu
        for(int d = 0; d < data::enemies_count; ++d) if(g.kills_by_type[d]) catalog_add(p, d);
        p.tools_found = uint8_t(p.tools_found | g.tools_found);
        if(g.st == status::won) set_class_won(p, g.cls);
        int stake = investor_stake(g.bonus.investor);   // rekord stawki zawodu (wygrana w trybie inwestora)
        if(g.st == status::won && stake > best_stake(p, g.cls))
        {
            const int before = stake_rank(p);
            set_best_stake(p, g.cls, stake);
            grant_stake_ranks(p, before);   // v0.21.52 cz. b (#48): stopnie inwestora - Respekt za nowe progi
        }
    }

    // Sprawdza odznaki po ważnym momencie (koniec etapu, koniec budowy). Nowe odznaki dają doświadczenie.
    // Zwraca bitmaskę odznak zdobytych właśnie teraz.
    inline int check_badges(profile& p, game& g)
    {
        record_run(p, g);
        bool cleared = g.st == status::stage_clear || g.st == status::won;
        bool won = g.st == status::won;
        int all_tools = ((1 << data::tools_count) - 1) & ~data::secret_tools_mask;   // sekretne narzędzia się nie liczą
        bool cond[16] = {};
        cond[data::badge_bez_usterek] = cleared && g.stage_damage == 0;
        cond[data::badge_przed_terminem] = won && g.turns - g.stage_start_turn <= 150;
        cond[data::badge_seryjny] = g.stage_kills >= 8;
        cond[data::badge_zawodowiec] = g.hero_level >= data::max_hero_level;
        cond[data::badge_twardziel] = won && g.diff == data::difficulties_count - 1;
        cond[data::badge_pelny_zespol] = open_classes_won(p) == data::open_classes_count;   // zawody z sekretów się nie liczą
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

    // v0.21.52 cz. b: dośw. inspektora z budowy (łącznie od startu, z budowami NG+): budowa, ukończone etapy, bossowie,
    // elity, magazyny, wygrane; x procent trudności. Dośw. mistrzostwa zawodu = to samo.
    inline int run_progress_xp(const game& g)
    {
        int stages = 0, bosses = 0;
        auto count = [&](int to) { for(int s = g.first_stage; s < to; ++s) { ++stages; bosses += g.sdef(s).boss >= 0; } };
        for(int t = 0; t < g.tier; ++t) count(g.route_count());   // budowy ukończone przed "Kolejną budową"
        const bool won = g.st == status::won;
        count(won ? g.route_count() : (g.st == status::stage_clear ? g.stage + 1 : g.stage));
        const int wins = g.tier + (won ? 1 : 0);
        const int xp = data::inspector_xp_run + stages * data::inspector_xp_stage + bosses * data::inspector_xp_boss
                     + g.elites_killed * data::inspector_xp_elite + g.secrets_found * data::inspector_xp_storeroom + wins * data::inspector_xp_win;
        return xp * data::inspector_diff_pct[g.diff] / 100;
    }

    struct progress_gain
    {
        int gained = 0;                        // dośw. inspektora i mistrzostwa z tej części budowy
        int cls = -1;
        int insp_before = 0, insp_after = 0;   // poziom inspektora przed / po
        int mastery_before = 0, mastery_after = 0;
        int respect = 0;                       // Respekt z nowych poziomów
    };
    // Poziomy, nagrody (Respekt, wariant mocy włącza się sam) - wspólne dla budowy i migracji.
    inline void add_inspector_xp(profile& p, int d, progress_gain& r)
    {
        r.insp_before = inspector_level(p);
        p.inspector_xp = uint32_t(imin(1000000000, inspector_xp_int(p) + imax(0, d)));
        r.insp_after = inspector_level(p);
        for(int l = r.insp_before; l < r.insp_after; ++l) grant_level(p, data::inspector_levels[l]);
    }
    inline void add_mastery_xp(profile& p, int cls, int d, progress_gain& r)
    {
        if(cls < 0 || cls >= data::classes_count) return;
        r.cls = cls;
        r.mastery_before = mastery_level(p, cls);
        p.mastery_xp[cls] = add_sat16(p.mastery_xp[cls], d);
        r.mastery_after = mastery_level(p, cls);
        for(int l = r.mastery_before; l < r.mastery_after; ++l)
        {
            grant_level(p, data::mastery_levels[l]);
            if(data::mastery_levels[l].reward == progress_reward::power) p.power_alt = uint16_t(p.power_alt | (1u << cls));
        }
    }
    // Przenosi nowe dośw. inspektora i mistrzostwa z budowy (bez podwójnego liczenia - znak wodny run_progress, jak NG+).
    // Wołać na końcu budowy (śmierć, wygrana, porzucenie), przed story_check (wątki inspektora).
    inline progress_gain bank_progress(profile& p, const game& g)
    {
        progress_gain r;
        const int total = run_progress_xp(g);
        const int d = imax(0, total - int(p.run_progress));
        p.run_progress = uint16_t(imin(65535, imax(int(p.run_progress), total)));
        const int r0 = p.respect_total;
        r.gained = d;
        add_inspector_xp(p, d, r);
        add_mastery_xp(p, g.cls, d, r);
        r.respect = p.respect_total - r0;
        return r;
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
    inline int daily_class(uint32_t seed) { return int(seed % uint32_t(data::open_classes_count)); }   // bez zawodów z sekretów
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

    inline int streak_record(profile& p, int day);   // v0.21.52 cz. c (niżej)
    // Wynik codziennej budowy: najlepszy dnia zostaje; nowy dzień zastępuje najstarszy. Zwraca true = nowy rekord dnia.
    // v0.21.52 cz. c: liczy też serię dni (streak_record).
    inline bool record_daily(profile& p, int day, int score, bool won)
    {
        if(p.daily_runs < 255) ++p.daily_runs;
        streak_record(p, day);
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

    // ------------------------------------------------------------------ wyzwanie tygodnia (#34)
    // Tydzień nr 1 zaczyna się w poniedziałek data::weekly_epoch; seed z numeru tygodnia, zasady z listy po kolei.
    // GBA: data z ekranu budowy dnia (ustawiana ręcznie), Godot: z systemu. Bez Szkoleń i pamiątek - równo dla wszystkich.
    inline int weekly_number(int y, int m, int d)
    {
        const int t = days_from_civil(y, m, d) - days_from_civil(data::weekly_epoch[0], data::weekly_epoch[1], data::weekly_epoch[2]);
        return imax(1, (t >= 0 ? t / 7 : -((-t + 6) / 7)) + 1);
    }
    // Poniedziałek tygodnia (dni od 1970-01-01).
    inline int weekly_first_day(int week)
    {
        return days_from_civil(data::weekly_epoch[0], data::weekly_epoch[1], data::weekly_epoch[2]) + (week - 1) * 7;
    }
    inline int weekly_index(int week) { return ((week - 1) % data::weekly_count + data::weekly_count) % data::weekly_count; }
    inline uint32_t weekly_seed(int week)
    {
        uint32_t h = uint32_t(week) * 2246822519u + 0x85EBCA6Bu;
        h ^= h >> 15; h *= 2654435761u; h ^= h >> 13;
        return h ? h : 1u;
    }
    inline int weekly_rule_value(int wi, weekly_rule w, int fallback)
    {
        const weekly_def& wd = data::weekly[wi];
        for(int i = 0; i < wd.rules_count; ++i) if(wd.rules[i].rule == w) return wd.rules[i].value;
        return fallback;
    }
    // Zawód tygodnia: z zasady albo z seeda (jak budowa dnia).
    inline int weekly_class(int week) { return weekly_rule_value(weekly_index(week), weekly_rule::cls, daily_class(weekly_seed(week))); }
    // Premie z zasad (obrażenia %, budżet, HP %) - bez meta-progresji.
    inline run_mods weekly_mods(int week)
    {
        const int wi = weekly_index(week);
        run_mods m;
        m.weekly = wi;
        m.dmg_pct = weekly_rule_value(wi, weekly_rule::dmg_pct, 0);
        m.cash = weekly_rule_value(wi, weekly_rule::cash, 0);
        const int hp = data::classes[weekly_class(week)].max_health;
        m.hp = hp * weekly_rule_value(wi, weekly_rule::hp_pct, 0) / 100;
        return m;
    }
    inline void start_weekly(game& g, int week)
    {
        g.new_run(weekly_class(week), weekly_seed(week), data::weekly_difficulty, weekly_mods(week));
        g.weekly_week = uint16_t(week);
    }
    // Najlepszy wynik tygodnia (-1 = brak) i wygrana.
    inline int weekly_best(const profile& p, int week)
    {
        for(int i = 0; i < data::weekly_history; ++i) if(p.weekly_week[i] == week && week > 0) return p.weekly_score[i];
        return -1;
    }
    inline bool weekly_won(const profile& p, int week)
    {
        for(int i = 0; i < data::weekly_history; ++i) if(p.weekly_week[i] == week && week > 0) return (p.weekly_won >> i) & 1;
        return false;
    }
    // Wynik wyzwania: najlepszy tygodnia zostaje, nowy tydzień zastępuje najstarszy. true = nowy rekord tygodnia.
    inline bool record_weekly(profile& p, int week, int score, bool won)
    {
        if(p.weekly_runs < 255) ++p.weekly_runs;
        int slot = -1, oldest = 0;
        for(int i = 0; i < data::weekly_history; ++i)
        {
            if(p.weekly_week[i] == week) { slot = i; break; }
            if(p.weekly_week[i] < p.weekly_week[oldest]) oldest = i;
        }
        if(slot < 0)
        {
            slot = oldest;
            p.weekly_week[slot] = uint16_t(week); p.weekly_score[slot] = score;
            p.weekly_won = uint8_t((p.weekly_won & ~(1u << slot)) | (won ? 1u << slot : 0u));
            return true;
        }
        if(won) p.weekly_won = uint8_t(p.weekly_won | (1u << slot));
        if(score <= p.weekly_score[slot]) return false;
        p.weekly_score[slot] = score;
        return true;
    }

    // ------------------------------------------------------------------ fabuła odkrywana z budowami (#35)
    // Wątek SMS-ów odblokowuje się, gdy warunek jest spełniony (profil po budowie + ta budowa); archiwum w telefonie
    // profilu (Osiedle -> A = Wiadomości), nowe wątki z kropką aż do przeczytania. g = nullptr: tylko profil (migracja).
    inline bool story_condition(const profile& p, const game* g, int i)
    {
        const story_thread& t = data::story_arc[i];
        switch(t.trigger)
        {
            case story_trigger::runs:    return p.runs >= t.value;
            case story_trigger::wins:    return p.wins >= t.value;
            case story_trigger::boss:    return catalog_has(p, t.value);
            case story_trigger::elite:   return g && g->elites_killed > 0;
            case story_trigger::secret:  return g && g->secrets_found > 0;
            case story_trigger::event:
                if(g) for(int s = 0; s < max_stages; ++s) if(g->stage_event_log[s] != 255) return true;
                return false;
            case story_trigger::synergy: return g && g->synergy_mask() != 0;
            case story_trigger::daily:   return p.daily_runs > 0;
            case story_trigger::weekly:  return p.weekly_runs > 0;
            case story_trigger::act0:    return act0_unlocked(p);
            case story_trigger::inspector: return inspector_level(p) >= t.value;   // v0.21.52 cz. b
            default:                     return false;
        }
    }
    // Sprawdza wątki (koniec budowy). Zwraca bitmaskę odblokowanych właśnie teraz.
    inline uint32_t story_check(profile& p, const game* g)
    {
        uint32_t got = 0;
        for(int i = 0; i < data::story_arc_count; ++i)
            if(! ((p.story >> i) & 1) && story_condition(p, g, i)) got |= 1u << i;
        p.story |= got;
        p.story_new |= got;
        return got;
    }
    inline bool story_unlocked(const profile& p, int i) { return (p.story >> i) & 1; }
    inline bool story_unread(const profile& p, int i) { return (p.story_new >> i) & 1; }
    inline void story_mark_read(profile& p, int i) { p.story_new &= ~(1u << i); }
    inline int story_count(const profile& p) { int n = 0; for(int i = 0; i < data::story_arc_count; ++i) n += story_unlocked(p, i); return n; }
    inline int story_unread_count(const profile& p) { int n = 0; for(int i = 0; i < data::story_arc_count; ++i) n += story_unread(p, i); return n; }
    // v10 -> v11: wątki za to, co już osiągnięte (liczniki, bossowie z Katalogu, Akt 0) - czekają jako nowe.
    inline void migrate_v11(profile& p) { p.story = 0; p.story_new = 0; story_check(p, nullptr); }

    // ------------------------------------------------------------------ sekretne zlecenia (#39): warunki i sprawdzanie
    // g = nullptr: tylko profil (migracja v11 -> v12: np. wygrane każdym zawodem); reszta liczy się w budowie.
    inline bool secret_condition(const profile& p, const game* g, int i)
    {
        const secret_def& sd = data::secrets[i];
        // v0.21.52 cz. d: wygrana liczy się w pełnym budynku (co najmniej 10 etapów - nie w krótkim Domku letniskowym)
        const bool won = g && g->st == status::won && g->route_count() - g->prelude_count() >= 10;
        switch(sd.kind)
        {
            case secret_kind::no_coffee_win: return won && g->coffee_drunk == 0;
            case secret_kind::helper_boss:   return g && (g->secret_flags & game::secret_helper_boss);
            case secret_kind::storerooms:    return g && g->secrets_found >= sd.value;
            case secret_kind::class_wins:    return open_classes_won(p) >= sd.value;
            case secret_kind::paper_clean:   return g && (g->secret_flags & game::secret_paper_clean);
            case secret_kind::shock_combos:  return g && g->shock_combos >= sd.value;
            case secret_kind::low_hp_win:    return won && g->hero.hp >= 1 && g->hero.hp <= sd.value;
            case secret_kind::fast_win:      return won && g->build_days() <= sd.value;
            default:                         return false;
        }
    }
    // Postęp do pokazania po wykonaniu nie jest potrzebny ("???" do końca); zwraca bitmaskę wykonanych właśnie teraz.
    inline int check_secrets(profile& p, const game* g)
    {
        int got = 0;
        for(int i = 0; i < data::secrets_count; ++i)
            if(! secret_done(p, i) && secret_condition(p, g, i)) got |= 1 << i;
        p.secrets = uint16_t(p.secrets | got);
        p.secrets_new = uint16_t(p.secrets_new | got);
        return got;
    }
    inline int secrets_done_count(const profile& p) { int n = 0; for(int i = 0; i < data::secrets_count; ++i) n += secret_done(p, i); return n; }
    // v11 -> v12: sekrety za to, co już jest w profilu (wygrane każdym zawodem) - czekają na dymek jak nowe.
    inline void migrate_v12(profile& p)
    {
        p.secrets = 0; p.secrets_new = 0; p.cosmetic = 0;
        for(auto& r : p.respect_ranks_hi) r = 0;
        check_secrets(p, nullptr);
    }

    // Nagroda poziomu słowami (banery, listy w profilu): "Respekt +10", "Tytuł: Praktykant", "SMS: Pierwsza kontrola"...
    // cls - zawód (mistrzostwo: wariant mocy, broń mistrza, premia).
    inline message& progress_reward_label(message& m, const progress_level& l, int cls)
    {
        const mastery_class_def& mc = data::mastery_classes[cls >= 0 && cls < data::classes_count ? cls : 0];
        switch(l.reward)
        {
            case progress_reward::respect:       return m.add("Respekt +").add(l.value);
            case progress_reward::title:         return m.add("Tytuł: ").add(l.title);
            case progress_reward::helmet:        return m.add(data::cosmetics[l.index].name);
            case progress_reward::story:         return m.add("SMS: ").add(data::story_arc[l.index].name);
            case progress_reward::decor:         return m.add("Ozdoba: ").add(data::estate_decor[l.index].name);
            case progress_reward::keepsake_slot: return m.add("Druga pamiątka");
            case progress_reward::power:         return m.add("Moc: ").add(mc.power_name);
            case progress_reward::weapon:        return m.add(mc.weapon_name);
            case progress_reward::boon:          return m.add("Premia: ").add(data::boons[mc.boon].name);
            case progress_reward::keepsake:      return m.add("Pamiątka: ").add(data::keepsakes[l.index].name);   // v0.21.52 cz. c
            default:                             return m;
        }
    }

    // v13 -> v14 (v0.21.52 cz. b): dośw. inspektora i mistrzostwa z dotychczasowych statystyk (budowy, wygrane, Respekt;
    // mistrzostwo - wygrane zawodem z domów na Osiedlu i zawody z wygraną); Respekt za osiągnięte poziomy, wątki SMS.
    inline void migrate_v14(profile& p)
    {
        p.inspector_xp = 0; p.power_alt = 0; p.run_progress = 0; p.keepsake2 = 0;
        for(auto& m : p.mastery_xp) m = 0;
        for(auto& r : p.reserved14) r = 0;
        const int insp = imin(10000, imax(0, p.runs)) * data::inspector_migrate_run + imin(10000, imax(0, p.wins)) * data::inspector_migrate_win
                       + int(p.respect_total) * data::inspector_migrate_respect_pct / 100;
        progress_gain r;
        add_inspector_xp(p, insp, r);
        for(int c = 0; c < data::classes_count; ++c)
        {
            int houses = 0;
            for(int i = 0; i < p.houses_count; ++i) houses += (p.houses[i] & 15) == c;
            add_mastery_xp(p, c, houses * data::mastery_migrate_win + (class_won(p, c) ? data::mastery_migrate_class_win : 0), r);
        }
        story_check(p, nullptr);   // wątki od inspektora za osiągnięty poziom
    }

    // Osiedle rośnie z wygranymi: ile ozdób już stoi (data::estate_decor po progach wygranych).
    // v0.21.52 cz. b: ozdoba z wygranych albo z poziomu inspektora - każda ma swoje miejsce (k).
    inline bool decor_unlocked(const profile& p, int k)
    {
        const decor_def& dd = data::estate_decor[k];
        return dd.inspector > 0 ? inspector_level(p) >= dd.inspector : p.wins >= dd.wins;
    }
    inline int estate_decor(const profile& p)
    {
        int n = 0;
        for(int k = 0; k < data::estate_decor_count; ++k) n += decor_unlocked(p, k);
        return n;
    }

    // ------------------------------------------------------------------ v0.21.52 cz. c (#49): kolekcje
    // Liczniki pokonanych problemów każdego rodzaju (profil, do 255; znak wodny kill_mark jak run_kills); komplet:
    // każdy problem aktu x count, każdy boss (karty bossów) albo wszystkie ozdoby Osiedla (album).
    inline void bank_collections(profile& p, const game& g)
    {
        for(int d = 0; d < data::enemies_count; ++d)
        {
            const int v = g.kills_by_type[d], add = v - p.kill_mark[d];
            if(add > 0) p.kill_count[d] = add_sat8(p.kill_count[d], add);
            p.kill_mark[d] = uint8_t(imax(p.kill_mark[d], v));
        }
    }
    inline bool enemy_boss(int d) { return data::enemies[d].slam; }
    // Postęp kompletu: have / need (rodzaje problemów z licznikiem >= count, bossowie, ozdoby).
    inline void collection_progress(const profile& p, int i, int& have, int& need)
    {
        const collection_def& cd = data::collections[i];
        have = need = 0;
        if(cd.kind == collection_kind::decor) { have = estate_decor(p); need = data::estate_decor_count; return; }
        for(int d = 0; d < data::enemies_count; ++d)
        {
            const bool in = ((cd.enemies >> d) & 1) != 0;   // v0.21.52 cz. d: bossowie też z maski (Dom / kariera)
            if(! in) continue;
            ++need;
            have += p.kill_count[d] >= cd.count;
        }
    }
    inline bool collection_complete(const profile& p, int i) { int h = 0, n = 0; collection_progress(p, i, h, n); return n > 0 && h >= n; }
    inline int collections_done(const profile& p) { int k = 0; for(int i = 0; i < data::collections_count; ++i) k += collection_complete(p, i); return k; }
    // Komplety ukończone, a jeszcze nieogłoszone (baner na końcu budowy): zaznacza je i zwraca bity.
    inline int check_collections(profile& p)
    {
        int got = 0;
        for(int i = 0; i < data::collections_count; ++i)
            if(collection_complete(p, i) && ! ((p.collections >> i) & 1)) got |= 1 << i;
        p.collections = uint8_t(p.collections | got);
        return got;
    }
    inline int bosses_count() { int n = 0; for(int d = 0; d < data::enemies_count; ++d) n += enemy_boss(d); return n; }
    // Boss nr k (karty bossów, kolejność z listy problemów) - indeks w data::enemies.
    inline int boss_at(int k) { for(int d = 0; d < data::enemies_count; ++d) if(enemy_boss(d) && k-- == 0) return d; return -1; }
    // Nagroda kompletu słowami: "+10 zł na start", "Tytuł: Urzędnik", "Ceglasty kask".
    inline message& collection_reward_label(message& m, int i)
    {
        const collection_def& cd = data::collections[i];
        if(cd.reward.reward == progress_reward::perk) return perk_label(m, cd.bonus);
        return progress_reward_label(m, cd.reward, -1);
    }

    // ------------------------------------------------------------------ v0.21.52 cz. c (#50): zadania dnia i tygodnia
    // 3 zadania dnia (z puli data::daily_tasks) i 2 tygodnia (data::weekly_tasks) z seeda numeru dnia / tygodnia - te same
    // dla wszystkich. Dzień: Godot - data z systemu, GBA - data ustawiona dla budowy dnia. Postęp z każdej budowy (też dnia
    // i tygodnia), w profilu ze znakiem wodnym task_mark; wykonane = Respekt od razu, wykonane łącznie - nagrody za liczbę.
    inline void pick_tasks(uint32_t seed, int pool, int n, int* out)
    {
        uint32_t h = seed;
        for(int k = 0; k < n; ++k)
        {
            h = h * 1664525u + 1013904223u;
            int i = int((h >> 16) % uint32_t(pool));
            for(int guard = 0; guard < pool; ++guard)
            {
                bool used = false;
                for(int j = 0; j < k; ++j) used |= out[j] == i;
                if(! used) break;
                i = (i + 1) % pool;
            }
            out[k] = i;
        }
    }
    // Zadanie w slocie s (0-2 dnia, 3-4 tygodnia) dla dnia / tygodnia z profilu.
    inline const task_def& task_at(int day, int week, int s)
    {
        int ids[daily_task_slots];
        if(s < daily_task_slots)
        {
            pick_tasks(daily_seed(day) ^ 0x7A5Bu, data::daily_tasks_count, daily_task_slots, ids);
            return data::daily_tasks[ids[s]];
        }
        pick_tasks(weekly_seed(week) ^ 0x7A5Bu, data::weekly_tasks_count, task_slots - daily_task_slots, ids);
        return data::weekly_tasks[ids[s - daily_task_slots]];
    }
    inline const task_def& task_of(const profile& p, int s) { return task_at(p.task_day, p.task_week, s); }
    // Etapy ukończone w budowie (z budowami NG+), bossowie pokonani, wygrane - jak run_progress_xp.
    inline int run_stages_done(const game& g)
    {
        const int per = g.route_count() - g.first_stage;
        const bool won = g.st == status::won;
        return g.tier * per + (won ? per : (g.st == status::stage_clear ? g.stage + 1 : g.stage) - g.first_stage);
    }
    inline int task_metric(const game& g, task_kind k)
    {
        const bool won = g.st == status::won;
        switch(k)
        {
            case task_kind::kills:       return g.kills;
            case task_kind::elites:      return g.elites_killed;
            case task_kind::bosses:      { int n = 0; for(int d = 0; d < data::enemies_count; ++d) if(enemy_boss(d)) n += g.kills_by_type[d]; return n; }
            case task_kind::stages:      return run_stages_done(g);
            case task_kind::brigade:     return g.helpers_called;
            case task_kind::powers:      return g.powers_used;
            case task_kind::coffee:      return g.coffee_drunk;
            case task_kind::combos:      return g.combos_run;
            case task_kind::storerooms:  return g.secrets_found;
            case task_kind::events:      { int n = 0; for(int s = 0; s < max_stages; ++s) n += g.stage_event_log[s] != 255; return n; }
            case task_kind::win:         return g.tier + (won ? 1 : 0);
            case task_kind::win_no_shop: return g.shop_buys == 0 ? g.tier + (won ? 1 : 0) : 0;
            default:                     return 0;
        }
    }
    // Nowy dzień / tydzień: zadania od zera (postęp, znak wodny, wykonane). Zwraca true, jeśli coś zmieniono.
    inline bool tasks_roll(profile& p, int day, int week)
    {
        bool changed = false;
        if(day > 0 && p.task_day != day)
        {
            p.task_day = uint16_t(day);
            for(int s = 0; s < daily_task_slots; ++s) { p.task_progress[s] = 0; p.task_mark[s] = 0; p.task_done = uint8_t(p.task_done & ~(1u << s)); }
            changed = true;
        }
        if(week > 0 && p.task_week != week)
        {
            p.task_week = uint16_t(week);
            for(int s = daily_task_slots; s < task_slots; ++s) { p.task_progress[s] = 0; p.task_mark[s] = 0; p.task_done = uint8_t(p.task_done & ~(1u << s)); }
            changed = true;
        }
        return changed;
    }
    inline bool task_done(const profile& p, int s) { return (p.task_done >> s) & 1; }
    // Postęp w trakcie budowy: profil + licznik budowy jeszcze nieprzeniesiony (do celu).
    inline int task_progress_live(const profile& p, const game* g, int s)
    {
        const task_def& td = task_of(p, s);
        int v = p.task_progress[s];
        if(g && ! task_done(p, s)) v += imax(0, task_metric(*g, td.kind) - p.task_mark[s]);
        return imin(v, td.target);
    }
    // Przenosi postęp zadań z budowy (koniec etapu, koniec budowy); wykonane dają Respekt. Zwraca bity wykonanych teraz.
    inline int bank_tasks(profile& p, const game& g, int day, int week)
    {
        tasks_roll(p, day, week);
        int got = 0;
        for(int s = 0; s < task_slots; ++s)
        {
            const task_def& td = task_of(p, s);
            const int v = imin(255, task_metric(g, td.kind)), add = v - p.task_mark[s];
            p.task_mark[s] = uint8_t(imax(p.task_mark[s], v));
            if(task_done(p, s) || add <= 0) continue;
            p.task_progress[s] = uint8_t(imin(255, p.task_progress[s] + add));
            if(p.task_progress[s] >= td.target)
            {
                p.task_done = uint8_t(p.task_done | (1u << s));
                p.respect = add_sat16(p.respect, td.respect); p.respect_total = add_sat16(p.respect_total, td.respect);
                const int before = p.tasks_total;
                p.tasks_total = add_sat16(p.tasks_total, 1);
                for(int l = 0; l < data::task_rewards_count; ++l)   // nagroda za liczbę zadań: Respekt raz
                    if(before < data::task_rewards[l].xp && p.tasks_total >= data::task_rewards[l].xp) grant_level(p, data::task_rewards[l]);
                got |= 1 << s;
            }
        }
        return got;
    }
    // Najbliższe wykonania zadanie (największy % postępu na żywo, przy remisie wcześniejszy slot); -1 = wszystkie wykonane.
    inline int next_task(const profile& p, const game* g)
    {
        int best = -1, best_pct = -1;
        for(int s = 0; s < task_slots; ++s)
        {
            if(task_done(p, s)) continue;
            const int pct = task_progress_live(p, g, s) * 100 / imax(1, task_of(p, s).target);
            if(pct > best_pct) { best_pct = pct; best = s; }
        }
        return best;
    }
    inline int tasks_done_today(const profile& p) { int n = 0; for(int s = 0; s < daily_task_slots; ++s) n += task_done(p, s); return n; }
    inline int tasks_done_week(const profile& p) { int n = 0; for(int s = daily_task_slots; s < task_slots; ++s) n += task_done(p, s); return n; }
    // Kolejna nagroda za zadania łącznie (-1 = wszystkie).
    inline int next_task_reward(const profile& p) { for(int l = 0; l < data::task_rewards_count; ++l) if(p.tasks_total < data::task_rewards[l].xp) return l; return -1; }

    // ------------------------------------------------------------------ v0.21.52 cz. c (#51): seria dni budowy dnia
    // Dzień budowy dnia (numer) zaraz po ostatnim = seria +1; dalszy = od nowa (1); ten sam albo wcześniejszy - bez zmian
    // (GBA: data wpisana ręcznie - liczy się tylko kolejny dzień). Nagrody za najdłuższą serię (pamiątka, kask, tytuł).
    // Zwraca bity nagród osiągniętych teraz.
    inline int streak_record(profile& p, int day)
    {
        if(day <= 0) return 0;
        if(p.streak_day != 0 && day <= p.streak_day) return 0;
        p.streak = p.streak_day != 0 && day == p.streak_day + 1 ? uint8_t(imin(255, p.streak + 1)) : uint8_t(1);
        p.streak_day = uint16_t(day);
        const int before = p.streak_best;
        p.streak_best = uint8_t(imax(p.streak_best, p.streak));
        int got = 0;
        for(int l = 0; l < data::streak_rewards_count; ++l)
            if(before < data::streak_rewards[l].xp && p.streak_best >= data::streak_rewards[l].xp) got |= 1 << l;
        return got;
    }
    // Seria widoczna dnia today: przerwana (dzień przerwy) = 0.
    inline int streak_now(const profile& p, int today) { return p.streak_day != 0 && today <= p.streak_day + 1 ? p.streak : 0; }
    inline int next_streak_reward(const profile& p) { for(int l = 0; l < data::streak_rewards_count; ++l) if(p.streak_best < data::streak_rewards[l].xp) return l; return -1; }

    // v14 -> v15 (v0.21.52 cz. c): drzewko bez wyborów, kolekcje - rodzaje z Katalogu jako 1 pokonany, zadania od zera,
    // seria dni z wyników ostatnich dni budowy dnia (kolejne dni do najnowszego).
    inline void migrate_v15(profile& p)
    {
        p.tree = 0;
        for(int d = 0; d < max_enemy_types; ++d) { p.kill_count[d] = d < data::enemies_count && catalog_has(p, d) ? 1 : 0; p.kill_mark[d] = 0; }
        p.task_day = p.task_week = 0;
        for(int s = 0; s < task_slots; ++s) { p.task_progress[s] = 0; p.task_mark[s] = 0; }
        p.task_done = 0; p.tasks_total = 0; p.collections = 0;
        int last = 0;
        for(int i = 0; i < daily_slots; ++i) last = imax(last, p.daily_day[i]);
        int n = 0;
        if(last > 0)
            for(n = 1; n < daily_slots; ++n)
            {
                bool has = false;
                for(int i = 0; i < daily_slots; ++i) has |= p.daily_day[i] == last - n;
                if(! has || last - n <= 0) break;
            }
        p.streak = uint8_t(n); p.streak_best = uint8_t(n); p.streak_day = uint16_t(last);
        p.collections = uint8_t(0);
        for(int i = 0; i < data::collections_count; ++i) if(collection_complete(p, i)) p.collections = uint8_t(p.collections | (1u << i));   // bez banera za stare
    }

    // ------------------------------------------------------------------ v0.21.52 cz. d (#47): mapa kariery
    // Kontrakty: Dom jednorodzinny zawsze, kolejne po wygranych albo na poziomie inspektora; wybór przed wyborem zawodu
    // (budowa dnia i tygodnia - zawsze Dom). Pierwsza wygrana kontraktu: Respekt (raz), tytuł i kolor kasku (od wygranej).
    inline bool career_unlocked(const profile& p, int c)
    {
        if(c < 0 || c >= data::career_count) return false;
        const career_def& k = data::career[c];
        switch(k.unlock)
        {
            case career_unlock::wins:      return p.wins >= k.unlock_value;
            case career_unlock::inspector: return inspector_level(p) >= k.unlock_value;
            default:                       return true;
        }
    }
    inline bool career_won(const profile& p, int c) { return c >= 0 && c < max_contracts && ((p.career_done >> c) & 1); }
    inline int careers_unlocked(const profile& p) { int n = 0; for(int c = 0; c < data::career_count; ++c) n += career_unlocked(p, c); return n; }
    inline int careers_won(const profile& p) { int n = 0; for(int c = 0; c < data::career_count; ++c) n += career_won(p, c); return n; }
    // Wybrany kontrakt (zablokowany albo spoza danych - Dom jednorodzinny).
    inline int selected_career(const profile& p) { return career_unlocked(p, p.contract) ? p.contract : 0; }
    // Odblokowane, a jeszcze nieogłoszone kontrakty (baner "Nowy kontrakt" raz) - zwraca bity i zapamiętuje je.
    inline int career_announce(profile& p)
    {
        int got = 0;
        for(int c = 1; c < data::career_count; ++c)
            if(career_unlocked(p, c) && ! ((p.career_seen >> c) & 1)) got |= 1 << c;
        p.career_seen = uint8_t(p.career_seen | got);
        return got;
    }
    // Ukończone etapy kontraktu w tej budowie (bez Aktu 0; NG+ - do końca budynku).
    inline int career_stages_done(const game& g)
    {
        if(g.tier > 0 || g.st == status::won) return g.route_count() - g.prelude_count();
        const int to = g.st == status::stage_clear ? g.stage + 1 : g.stage;
        return imax(0, to - imax(int(g.first_stage), g.prelude_count()));
    }
    // Koniec etapu / budowy: najlepszy wynik kontraktu (można wołać wielokrotnie).
    inline void career_record(profile& p, const game& g)
    {
        const int c = g.contract;
        if(c < 0 || c >= max_contracts) return;
        p.career_best[c] = uint8_t(imax(p.career_best[c], imin(255, career_stages_done(g))));
    }
    // Wygrana budowa w kontrakcie (raz na wygraną, obok record_win). Zwraca true przy pierwszej wygranej kontraktu
    // (Respekt z nagrody już w profilu; tytuł i kask - od teraz).
    inline bool career_win(profile& p, const game& g)
    {
        const int c = g.contract;
        if(c < 0 || c >= data::career_count) return false;
        career_record(p, g);
        p.career_wins[c] = add_sat8(p.career_wins[c], 1);
        if(career_won(p, c)) return false;
        p.career_done = uint8_t(p.career_done | (1u << c));
        const int r = data::career[c].respect;
        if(r > 0) { p.respect = add_sat16(p.respect, r); p.respect_total = add_sat16(p.respect_total, r); }
        return true;
    }
    // Nagroda za pierwszą wygraną słowami: "Respekt +20, tytuł Letnik, Sosnowy kask".
    inline message& career_reward_label(message& m, int c)
    {
        const career_def& k = data::career[c];
        bool any = false;
        if(k.respect > 0) { m.add("Respekt +").add(k.respect); any = true; }
        if(k.title[0]) { m.add(any ? ", " : "").add(k.title); any = true; }
        if(k.helmet >= 0) m.add(any ? ", " : "").add(data::cosmetics[k.helmet].name);
        return m;
    }
    // Warunek odblokowania słowami: "1 wygrana", "3 wygrane", "Inspektor 8".
    inline message& career_unlock_label(message& m, int c)
    {
        const career_def& k = data::career[c];
        if(k.unlock == career_unlock::wins)
        {
            const int v = k.unlock_value;
            return m.add(v).add(v == 1 ? " wygrana" : (v % 10 >= 2 && v % 10 <= 4 && (v % 100 < 10 || v % 100 >= 20) ? " wygrane" : " wygranych"));
        }
        if(k.unlock == career_unlock::inspector) return m.add("Inspektor ").add(k.unlock_value);
        return m;
    }
    // v15 -> v16: Dom jednorodzinny z dotychczasowych wygranych (wygrany, gdy była wygrana; najlepszy etap - cała budowa).
    // Kontrakty już odblokowane czekają na baner jak nowe.
    inline void migrate_v16(profile& p)
    {
        p.contract = 0; p.career_seen = 0; p.career_done = 0;
        for(int c = 0; c < max_contracts; ++c) { p.career_wins[c] = 0; p.career_best[c] = 0; }
        if(p.wins > 0)
        {
            p.career_wins[0] = uint8_t(imin(255, p.wins));
            p.career_best[0] = uint8_t(data::career[0].count - data::career[0].prelude);
            p.career_done = 1;
        }
    }

    // ------------------------------------------------------------------ v0.21.53 (#53, #54): filtry ekranu
    // Klasyczny i tryby dla daltonistów (Protanopia, Deuteranopia, Tritanopia, Wysoki kontrast) - zawsze; zabawowe
    // (Noir, Retro LCD, Neon nocy, Kwas) - gdy spełniony dowolny z warunków z danych. Zablokowany: "???" z podpowiedzią.
    inline bool filter_cond_met(const profile& p, const filter_cond& c)
    {
        switch(c.kind)
        {
            case filter_unlock::inspector:  return inspector_level(p) >= c.value;
            case filter_unlock::collection: return collection_complete(p, c.value);
            case filter_unlock::career:     return career_won(p, c.value);
            case filter_unlock::secret:     return c.value >= 0 && c.value < data::secrets_count && secret_done(p, c.value);
            case filter_unlock::wins:       return p.wins >= c.value;
            default:                        return false;
        }
    }
    inline bool filter_unlocked(const profile& p, int f)
    {
        if(f < 0 || f >= data::screen_filters_count) return false;
        const screen_filter_def& d = data::screen_filters[f];
        if(d.kind != filter_kind::fun) return true;
        for(const filter_cond& c : d.unlock) if(filter_cond_met(p, c)) return true;
        return false;
    }
    inline int filters_unlocked(const profile& p) { int n = 0; for(int f = 0; f < data::screen_filters_count; ++f) n += filter_unlocked(p, f); return n; }
    // Wybrany filtr (zablokowany albo spoza danych - klasyczny).
    inline int selected_filter(const profile& p) { return filter_unlocked(p, p.filter) ? p.filter : 0; }
    // Wybór (Wygląd, Ustawienia): kolejny odblokowany filtr w kierunku dir.
    inline void cycle_filter(profile& p, int dir)
    {
        int f = selected_filter(p);
        for(int i = 0; i < data::screen_filters_count; ++i)
        {
            f = (f + (dir < 0 ? -1 : 1) + data::screen_filters_count) % data::screen_filters_count;
            if(filter_unlocked(p, f)) break;
        }
        p.filter = uint8_t(f);
    }
    // Odblokowane, a jeszcze nieogłoszone filtry zabawowe (baner "Nowy filtr ekranu" raz) - zwraca bity i zapamiętuje je.
    inline int filter_announce(profile& p)
    {
        int got = 0;
        for(int f = 0; f < data::screen_filters_count; ++f)
            if(data::screen_filters[f].kind == filter_kind::fun && filter_unlocked(p, f) && ! ((p.filters_seen >> f) & 1)) got |= 1 << f;
        p.filters_seen = uint16_t(p.filters_seen | got);
        return got;
    }
    // Warunek odblokowania słowami (po odblokowaniu): "Inspektor 5", "Kolekcja: Stan surowy", "Wygrana: Kamienica",
    // "Sekret: Szybka ekipa", "10 wygranych"; dwa warunki - "... albo ...".
    inline message& filter_unlock_label(message& m, int f)
    {
        const screen_filter_def& d = data::screen_filters[f];
        if(d.kind == filter_kind::access) return m.add(data::filter_ui_always_access);   // teksty z danych (screenFilters.ui)
        if(d.kind == filter_kind::classic) return m.add(data::filter_ui_always);
        for(int i = 0; i < 2; ++i)
        {
            const filter_cond& c = d.unlock[i];
            if(c.kind == filter_unlock::none) break;
            if(i > 0) m.add(data::filter_ui_or);
            switch(c.kind)
            {
                case filter_unlock::inspector:  m.add(data::filter_ui_inspector).add(c.value); break;
                case filter_unlock::collection: m.add(data::filter_ui_collection).add(data::collections[c.value].name); break;
                case filter_unlock::career:     m.add(data::filter_ui_career).add(data::career[c.value].short_name); break;
                case filter_unlock::secret:     m.add(data::filter_ui_secret).add(data::secrets[c.value].hint); break;
                default:                        m.add(c.value).add(data::filter_ui_wins); break;
            }
        }
        return m;
    }
    // v16 -> v17: filtr klasyczny; odblokowane już filtry czekają na baner jak nowe.
    inline void migrate_v17(profile& p)
    {
        p.filter = 0;
        p.filters_seen = 0;
        for(auto& r : p.reserved17) r = 0;
    }

    // ------------------------------------------------------------------ podsumowanie budowy (#33): rada i najbliższy cel
    // Rada: pierwsza pasująca z data::recap_tips (co zabiło, niewypita kawa, bez kombinacji, wygrana...).
    inline int recap_tip_index(const game& g)
    {
        const recap_hit& h = g.last_hits[0];
        const bool dead = g.st == status::dead;
        for(int i = 0; i < data::recap_tips_count; ++i)
        {
            bool ok = false;
            switch(data::recap_tips[i].when)
            {
                case recap_tip::shock:    ok = dead && h.kind == uint8_t(recap_kind::shock); break;
                case recap_tip::slam:     ok = dead && h.kind == uint8_t(recap_kind::slam); break;
                case recap_tip::blast:    ok = dead && (h.kind == uint8_t(recap_kind::blast) || h.kind == uint8_t(recap_kind::dust)); break;
                case recap_tip::coffee:   ok = dead && g.thermos > 0 && ! g.weekly_has(weekly_rule::no_coffee); break;
                case recap_tip::ranged:   ok = dead && h.kind == uint8_t(recap_kind::ranged); break;
                case recap_tip::elite:    ok = dead && h.elite >= 0; break;
                case recap_tip::boss:     ok = dead && h.src >= 0 && data::enemies[h.src].slam; break;
                case recap_tip::no_combo: ok = g.combos_run == 0; break;
                case recap_tip::won:      ok = g.st == status::won; break;
                case recap_tip::any:      ok = true; break;
                default: break;
            }
            if(ok) return i;
        }
        return data::recap_tips_count - 1;
    }
    // Najbliższy cel: najtańsza ranga Respektu ("Jeszcze 3 Respektu do:" + "Pewna ręka II"); wszystko kupione -
    // najbliższe Szkolenie za doświadczenie. false = nic nie zostało.
    inline bool recap_goal(const profile& p, message& lead, message& name)
    {
        int best = -1, bc = 0;
        for(int i = 0; i < data::respect_count; ++i)
        {
            const int c = respect_unlocked(p, i) ? respect_cost(p, i) : -1;
            if(c >= 0 && (best < 0 || c < bc)) { best = i; bc = c; }
        }
        if(best >= 0)
        {
            if(p.respect >= bc) lead.add("Stać Cię (Respekt):");
            else lead.add("Jeszcze ").add(bc - int(p.respect)).add(" Respektu do:");
            name.add(data::respect[best].name).add(" ").add(roman_numeral(respect_rank(p, best) + 1));
            return true;
        }
        int kind = -1, idx = -1;
        const int cost = next_unlock(p, kind, idx);
        if(cost < 0) return false;
        if(p.xp >= cost) lead.add("Stać Cię (Szkolenia):");
        else lead.add("Jeszcze ").add(cost - p.xp).add(" dośw. do:");
        if(kind == 0) name.add(data::upgrades[idx].name).add(" ").add(roman_numeral(p.levels[idx] + 1));
        else if(kind == 1) name.add(data::classes[idx].name);
        else if(kind == 2) name.add(data::weapons[data::tools[idx].weapon].name);
        else if(kind == 3) name.add(data::brigade[idx].name);
        else if(kind == 5) name.add("Drzewko: ").add(data::tree_branches[data::tree_nodes[idx].branch].name);
        else name.add(data::difficulties[data::difficulties_count - 1].name);
        return true;
    }

    // ------------------------------------------------------------------ harmonogram domu po wygranej
    // Dni etapu z liczby tur (min + tury / turnsPerDay) i data końca etapu, licząc wstecz od daty odbioru.
    inline int schedule_days(const game& g, int s) { return data::schedule_min_days + g.stage_days[s] / data::schedule_turns_per_day; }
    inline int schedule_total_days(const game& g) { int t = 0; for(int s = g.first_stage; s < g.route_count(); ++s) t += schedule_days(g, s); return t; }
    inline int schedule_total_cost(const game& g) { int t = 0; for(int s = g.first_stage; s < g.route_count(); ++s) t += g.sdef(s).cost; return t; }
    // Dzień (days_from_civil) rozpoczęcia etapu s, gdy odbiór był w dniu end_day.
    inline int schedule_start_day(const game& g, int s, int end_day)
    {
        int d = end_day - schedule_total_days(g);
        for(int i = g.first_stage; i < s; ++i) d += schedule_days(g, i);
        return d;
    }

    // ------------------------------------------------------------------ po budowie: co najbliżej do kupienia (motywacja)
    // Najtańsze niekupione w Szkoleniach: kind 0 ulepszenie, 1 zawód, 2 narzędzie, 3 brygada, 4 poziom Trudny, 5 węzeł
    // drzewka (otwarty, bez wyboru; v0.21.52 cz. c); -1 = wszystko.
    inline int next_unlock(const profile& p, int& kind, int& index)
    {
        int best = -1;
        auto take = [&](int k, int i, int c) { if(c >= 0 && (best < 0 || c < best)) { best = c; kind = k; index = i; } };
        for(int i = 0; i < data::upgrades_count; ++i) take(0, i, upgrade_cost(p, i));
        for(int i = 0; i < data::classes_count; ++i) if(! class_unlocked(p, i) && ! class_reward(i)) take(1, i, class_cost(p));
        for(int i = 0; i < data::tools_count; ++i) if(! tool_unlocked(p, i) && data::tools[i].shop) take(2, i, tool_cost(p));
        for(int i = 0; i < data::brigade_count; ++i) if(! helper_unlocked(p, i)) take(3, i, data::brigade[i].cost);
        if(! p.hard) take(4, 0, data::hard_cost);
        for(int n = 0; n < data::tree_nodes_count; ++n)   // v0.21.52 cz. c: otwarty, niewybrany węzeł drzewka (kind 5)
            if(tree_pick(p, n) == 0 && tree_open(p, n)) take(5, n, data::tree_nodes[n].cost);
        return best;
    }

    // ------------------------------------------------------------------ zapis budowy w trakcie
    // Cały stan gry (game jest trywialnie kopiowalny) za profilem w SRAM. Rozmiar i suma kontrolna
    // odrzucają zapisy uszkodzone i z innej wersji gry.
    static_assert(std::is_trivially_copyable_v<game>);
    // v0.21.52 cz. c: PBRUN15 - liczniki zadań (wezwania brygady, zakupy) w miejscu wyrównania; rozmiar stanu bez zmian, więc
    // zapis PBRUN14 też się wczytuje (z zerami w nowych polach). Profil v15 (384 B) nie mieści się przed 256 - zapis budowy
    // od 512; przy migracji profilu warstwa GBA przenosi przerwaną budowę spod 256 (run_save_offset_v14).
    constexpr char run_magic[8] = "PBRUN16";
    constexpr char run_magic_v15[8] = "PBRUN15";   // 16 (v0.21.52 cz. d): kontrakt mapy kariery i problemy z 1. połowy bliźniaka w wyrównaniu; 15: liczniki zadań
    constexpr char run_magic_v14[8] = "PBRUN14";   // 14: sekretne zlecenia (liczniki budowy), nowe zawody; 13: podsumowanie budowy (ciosy, oś czasu), wyzwanie tygodnia; 12: wydarzenia z wyborem, ulepszenie narzędzia, magazyn; 11: premie po etapie, elity, kombinacje stanów; 10: Akt 0; 09: 10 etapów, zachowania
    constexpr int run_save_offset = 512;
    constexpr int run_save_offset_v14 = 256;   // profil v14 i starsze (240 B)
    static_assert(sizeof(profile) <= run_save_offset);
    static_assert(sizeof(game) == 3248);   // v0.21.52 cz. c: liczniki zadań w wyrównaniu - stary zapis budowy pasuje

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

    // Ważny zapis (bieżący albo PBRUN14 / PBRUN15 - ten sam układ stanu). Po wczytaniu starszego wołać run_save_upgrade.
    inline bool run_save_valid(const run_save& s)
    {
        const bool magic = std::memcmp(s.magic, run_magic, sizeof s.magic) == 0 || std::memcmp(s.magic, run_magic_v15, sizeof s.magic) == 0
                        || std::memcmp(s.magic, run_magic_v14, sizeof s.magic) == 0;
        return magic && s.size == sizeof(game) && s.checksum == run_checksum(s.g);
    }
    // PBRUN14 -> PBRUN16: pola w dawnym wyrównaniu (liczniki zadań; v0.21.52 cz. d: kontrakt i bliźniak) od zera.
    inline void run_save_upgrade(run_save& s)
    {
        const bool v14 = std::memcmp(s.magic, run_magic_v14, sizeof s.magic) == 0;
        if(! v14 && std::memcmp(s.magic, run_magic_v15, sizeof s.magic) != 0) return;
        if(v14) { s.g.helpers_called = 0; s.g.shop_buys = 0; }
        s.g.contract = 0; s.g.twin_carry = 0;   // przerwana budowa sprzed mapy kariery: Dom jednorodzinny
        std::memcpy(s.magic, run_magic, sizeof s.magic);
        s.checksum = run_checksum(s.g);
    }

    inline void run_save_clear(run_save& s) { std::memset(s.magic, 0, sizeof s.magic); }
}
