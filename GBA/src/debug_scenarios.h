#pragma once
// Scenariusze testowe dla playtestera (tools/playtest). Kompilowane tylko z -DPB_SCENARIO=N:
//   make TARGET=scn1 BUILD=build_scn1 USERFLAGS="-DPB_SCENARIO=1"
// Na starcie pierwszej budowy ustawiają sytuację, do której skrypt klawiszy nie dojdzie na ślepo.
//   1 - boss aktu I tuż obok bohatera (zapowiedź ciosu, unik)
//   2 - trzech wrogów w zasięgu (celownik, zmiana celu, karta wroga pod B)
//   3 - wrogowie wokół bohatera (efekty mocy każdego zawodu; wszystkie zawody odblokowane)
//   4 - paczki sprzętu 3 jakości i skrzynka z narzędziem obok bohatera
//   5 - aktywne stany: zatrucie, poślizg, porażenie
#include "core.h"
#include "meta.h"

namespace debug_scenario
{
    // Wolne pole podłogi w odległości [dmin, dmax] od bohatera (najbliższe), -1 gdy brak.
    inline bool free_cell(const core::game& g, int dmin, int dmax, int& ox, int& oy)
    {
        for(int d = dmin; d <= dmax; ++d)
            for(int y = g.hero.y - d; y <= g.hero.y + d; ++y)
                for(int x = g.hero.x - d; x <= g.hero.x + d; ++x)
                    if(core::cheb(x, y, g.hero.x, g.hero.y) == d && g.lv.at(x, y) == core::tile::floor && ! g.occupied(x, y)
                       && ! g.pickup_at(x, y))
                    { ox = x; oy = y; return true; }
        return false;
    }

    inline void place_enemy(core::game& g, int def, int dmin, int dmax, bool awake, int stun)
    {
        int x, y;
        if(g.enemies_count >= core::max_enemies || ! free_cell(g, dmin, dmax, x, y)) return;
        g.spawn(def, x, y);
        g.enemies[g.enemies_count - 1].awake = awake;
        g.enemies[g.enemies_count - 1].stun = int8_t(stun);
    }

    inline void unlock_all(core::profile& p)
    {
        p.classes = uint8_t((1 << data::classes_count) - 1);
        p.hard = 1;
        core::set_flag(p, core::help_seen);
    }

    // Wołane raz, na wejściu na pierwszy etap budowy.
    inline void apply(core::game& g, int scenario)
    {
        switch(scenario)
        {
            case 1:
            {
                int boss_stage = 0;
                while(data::stages[boss_stage].boss < 0) ++boss_stage;
                g.start_stage(boss_stage);
                core::actor& b = g.enemies[g.boss];
                int x, y;
                if(free_cell(g, 3, 4, x, y)) { b.x = int8_t(x); b.y = int8_t(y); }
                b.awake = true;
                g.slam_counter = data::slam_every - 1;   // zapowie cios przy najbliższej turze
                break;
            }
            case 2:
                g.enemies_count = 0;
                place_enemy(g, data::enemy_papierologia, 1, 1, false, 20);
                place_enemy(g, data::enemy_kornik, 2, 2, false, 20);
                place_enemy(g, data::enemy_plesn, 2, 3, false, 20);
                break;
            case 3:
                g.enemies_count = 0;
                for(int i = 0; i < 4; ++i) place_enemy(g, data::enemy_papierologia, 1, 2, false, 40);
                place_enemy(g, data::enemy_kornik, 3, 4, true, 0);
                break;
            case 4:
            {
                g.pickups_count = 0;
                const int args[4] = { 0 * 3 + 0, 1 * 3 + 1, 2 * 3 + 2, 1 };
                for(int i = 0; i < 4; ++i)
                {
                    int x, y;
                    if(! free_cell(g, 1, 2, x, y)) break;
                    g.pickups[g.pickups_count++] = { int8_t(x), int8_t(y), uint8_t(i < 3 ? core::gear_box : core::tool),
                                                     true, uint8_t(args[i]) };
                }
                break;
            }
            case 5:
                g.apply_status(core::status_effect::poison, 6);
                g.apply_status(core::status_effect::slip, 6);
                g.apply_status(core::status_effect::shock, 1);
                break;
            default:
                break;
        }
        g.update_fov();
    }
}
