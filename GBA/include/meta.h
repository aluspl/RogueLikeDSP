#pragma once
// Meta-progresja: profil gracza w SRAM (rekord, doświadczenie, zakupy) i sklep "Szkolenia".
// Czyste C++ (bez Butano) - testowalne na PC.
#include "core.h"

namespace core
{
    constexpr int max_upgrades = 8;
    static_assert(data::upgrades_count <= max_upgrades);
    static_assert(data::classes_count <= 8);

    constexpr char profile_magic[8] = "PBRL002";
    constexpr char profile_magic_v1[8] = "PBRL001";

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
        uint8_t pad[2];
    };

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

    inline bool buy_hard(profile& p)
    {
        if(p.hard || p.xp < data::hard_cost) return false;
        p.xp -= data::hard_cost; p.hard = 1;
        return true;
    }

    inline run_mods mods(const profile& p)
    {
        run_mods m;
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
            }
        }
        return m;
    }

    // Przenosi nowe doświadczenie z budowy do profilu. Zwraca, ile dodano.
    inline int bank_xp(profile& p, game& g)
    {
        int d = g.xp() - g.xp_banked;
        g.xp_banked = g.xp();
        p.xp += d;
        return d;
    }
}
