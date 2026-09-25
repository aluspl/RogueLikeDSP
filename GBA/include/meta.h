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
    static_assert(data::classes_count <= 8);

    constexpr char profile_magic[8] = "PBRL004";
    constexpr char profile_magic_v3[8] = "PBRL003";
    constexpr char profile_magic_v2[8] = "PBRL002";
    constexpr char profile_magic_v1[8] = "PBRL001";
    constexpr int profile_v2_size = 36;   // v3 = v2 + pola motywacji na końcu
    constexpr int profile_v3_size = 56;   // v4 = v3 + zlecenia i pamiątki na końcu
    constexpr int max_keepsakes = 8;
    static_assert(data::contracts_count <= 8 && data::keepsakes_count <= max_keepsakes);
    constexpr int max_houses = 12;        // działki na Osiedlu
    static_assert(data::badges_count <= 16 && data::enemies_count <= 16);

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
    };
    static_assert(offsetof(profile, badges) == profile_v2_size);
    static_assert(offsetof(profile, kills_total) == profile_v3_size);

    enum profile_flag : uint8_t { help_seen = 1, prologue_seen = 2 };

    inline bool has_flag(const profile& p, profile_flag f) { return p.flags & f; }
    inline void set_flag(profile& p, profile_flag f) { p.flags = uint8_t(p.flags | f); }

    inline void profile_reset(profile& p)
    {
        std::memset(&p, 0, sizeof p);
        std::memcpy(p.magic, profile_magic, sizeof p.magic);
        p.classes = uint8_t(data::start_classes_mask);
    }

    // Naprawia wczytany profil. Zwraca true, jeśli trzeba go zapisać (migracja albo pusta pamięć).
    inline bool profile_fix(profile& p)
    {
        if(std::memcmp(p.magic, profile_magic, sizeof p.magic) == 0) return false;
        // v3 -> v4 i v2 -> v4: stare pola zostają, nowe od zera
        int keep = std::memcmp(p.magic, profile_magic_v3, sizeof p.magic) == 0 ? profile_v3_size
                 : (std::memcmp(p.magic, profile_magic_v2, sizeof p.magic) == 0 ? profile_v2_size : 0);
        if(keep > 0)
        {
            std::memset(reinterpret_cast<char*>(&p) + keep, 0, sizeof p - keep);
            std::memcpy(p.magic, profile_magic, sizeof p.magic);
            return true;
        }
        if(std::memcmp(p.magic, profile_magic_v1, sizeof p.magic) == 0)
        {
            int32_t best = p.best, runs = p.runs, wins = p.wins;
            profile_reset(p);
            p.best = best; p.runs = runs; p.wins = wins;
            return true;
        }
        profile_reset(p);
        return true;
    }

    inline bool class_unlocked(const profile& p, int c) { return p.classes & (1u << c); }
    inline bool difficulty_unlocked(const profile& p, int d) { return d < data::difficulties_count - 1 || p.hard; }

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
        if(class_unlocked(p, c) || p.xp < data::class_cost) return false;
        p.xp -= data::class_cost; p.classes = uint8_t(p.classes | (1u << c));
        return true;
    }

    inline int tools_mask(const profile& p) { return p.tools | data::start_tools_mask; }
    inline bool tool_unlocked(const profile& p, int i) { return (tools_mask(p) >> i) & 1; }

    inline bool buy_tool(profile& p, int i)
    {
        if(tool_unlocked(p, i) || p.xp < data::tools[i].cost) return false;
        p.xp -= data::tools[i].cost; p.tools = uint8_t(p.tools | (1u << i));
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

    // Start budowy: licznik budów i budów z wybraną pamiątką (mods() wołać wcześniej - ranga z budów przed tą).
    inline void start_run(profile& p)
    {
        ++p.runs;
        int k = selected_keepsake(p);
        if(k >= 0 && p.keepsake_runs[k] < 255) ++p.keepsake_runs[k];
    }

    inline run_mods mods(const profile& p)
    {
        run_mods m;
        m.tools = tools_mask(p);
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
            }
        }
        for(int i = 0; i < data::badges_count; ++i)   // uprawnienia z zdobytych odznak
            if(p.badges & (1u << i)) add_perk(m, data::badges[i].bonus);
        int k = selected_keepsake(p);   // pamiątka zabrana na budowę
        if(k >= 0) add_perk(m, keepsake_perk(p, k));
        return m;
    }

    // Ekran "Koszty": ile kosztuje cały sklep i ile już wydano (pasek budżetu).
    inline int shop_total_cost()
    {
        int t = data::hard_cost;
        for(int i = 0; i < data::upgrades_count; ++i)
            for(int l = 0; l < data::upgrades[i].levels; ++l) t += data::upgrades[i].costs[l];
        for(int i = 0; i < data::classes_count; ++i) if(! (data::start_classes_mask & (1 << i))) t += data::class_cost;
        for(int i = 0; i < data::tools_count; ++i) t += data::tools[i].cost;
        return t;
    }

    inline int shop_spent(const profile& p)
    {
        int t = p.hard ? data::hard_cost : 0;
        for(int i = 0; i < data::upgrades_count; ++i)
            for(int l = 0; l < p.levels[i]; ++l) t += data::upgrades[i].costs[l];
        for(int i = 0; i < data::classes_count; ++i)
            if(class_unlocked(p, i) && ! (data::start_classes_mask & (1 << i))) t += data::class_cost;
        for(int i = 0; i < data::tools_count; ++i) if(tool_unlocked(p, i)) t += data::tools[i].cost;
        return t;
    }

    // ------------------------------------------------------------------ motywacja: Osiedle, odznaki, katalog
    // Dom na Osiedlu po wygranej budowie; wielkość z wyniku (0-3). Pełne Osiedle: najstarszy dom ustępuje.
    inline bool add_house(profile& p, const game& g)
    {
        if(g.st != status::won) return false;
        uint8_t h = uint8_t((g.cls & 15) | (imin(3, g.score / 1000) << 4));
        if(p.houses_count < max_houses) p.houses[p.houses_count++] = h;
        else { for(int i = 1; i < max_houses; ++i) p.houses[i - 1] = p.houses[i]; p.houses[max_houses - 1] = h; }
        return true;
    }

    inline uint16_t add_sat16(uint16_t a, int d) { return uint16_t(imin(65535, a + imax(0, d))); }
    inline uint8_t add_sat8(uint8_t a, int d) { return uint8_t(imin(255, a + imax(0, d))); }

    // Przenosi do profilu nowe wartości liczników zleceń z budowy (bez podwójnego liczenia).
    inline void bank_counters(profile& p, game& g)
    {
        p.kills_total = add_sat16(p.kills_total, g.kills - g.kills_banked); g.kills_banked = g.kills;
        p.powers_total = add_sat16(p.powers_total, g.powers_used - g.powers_banked); g.powers_banked = g.powers_used;
        p.brand_total = add_sat8(p.brand_total, g.brand_found - g.brand_banked); g.brand_banked = g.brand_found;
        p.clean_bosses = add_sat8(p.clean_bosses, g.clean_bosses - g.clean_banked); g.clean_banked = g.clean_bosses;
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
            case contract_kind::class_wins: return popcount(p.class_wins);
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
            case contract_kind::kills:      return v + g.kills - g.kills_banked;
            case contract_kind::powers:     return v + g.powers_used - g.powers_banked;
            case contract_kind::brand:      return v + g.brand_found - g.brand_banked;
            case contract_kind::clean_boss: return v + g.clean_bosses - g.clean_banked;
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
        for(int d = 0; d < data::enemies_count; ++d) if(g.kills_by_type[d]) p.catalog = uint16_t(p.catalog | (1u << d));
        p.tools_found = uint8_t(p.tools_found | g.tools_found);
        if(g.st == status::won) p.class_wins = uint8_t(p.class_wins | (1u << g.cls));
    }

    // Sprawdza odznaki po ważnym momencie (koniec etapu, koniec budowy). Nowe odznaki dają doświadczenie.
    // Zwraca bitmaskę odznak zdobytych właśnie teraz.
    inline int check_badges(profile& p, game& g)
    {
        record_run(p, g);
        bool cleared = g.st == status::stage_clear || g.st == status::won;
        bool won = g.st == status::won;
        int all_classes = (1 << data::classes_count) - 1, all_tools = (1 << data::tools_count) - 1;
        bool cond[16] = {};
        cond[data::badge_bez_usterek] = cleared && g.stage_damage == 0;
        cond[data::badge_przed_terminem] = won && g.turns - g.stage_start_turn <= 150;
        cond[data::badge_seryjny] = g.stage_kills >= 8;
        cond[data::badge_zawodowiec] = g.hero_level >= data::max_hero_level;
        cond[data::badge_twardziel] = won && g.diff == data::difficulties_count - 1;
        cond[data::badge_pelny_zespol] = (p.class_wins & all_classes) == all_classes;
        cond[data::badge_kolekcjoner] = (p.tools_found & all_tools) == all_tools;
        cond[data::badge_katalog] = p.catalog == (1 << data::enemies_count) - 1;
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

    // ------------------------------------------------------------------ zapis budowy w trakcie
    // Cały stan gry (game jest trywialnie kopiowalny) za profilem w SRAM. Rozmiar i suma kontrolna
    // odrzucają zapisy uszkodzone i z innej wersji gry.
    static_assert(std::is_trivially_copyable_v<game>);
    constexpr char run_magic[8] = "PBRUN03";   // 03: liczniki zleceń, premie z odznak i pamiątek
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
