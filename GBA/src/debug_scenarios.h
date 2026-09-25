#pragma once
// Scenariusze testowe dla playtestera (tools/playtest). Kompilowane tylko z -DPB_SCENARIO=N:
//   make TARGET=scn1 BUILD=build_scn1 USERFLAGS="-DPB_SCENARIO=1"
// Na starcie pierwszej budowy ustawiają sytuację, do której skrypt klawiszy nie dojdzie na ślepo.
//   1 - boss aktu I tuż obok bohatera (zapowiedź ciosu, unik)
//   2 - trzech wrogów w zasięgu (celownik, zmiana celu, karta wroga pod B)
//   3 - wrogowie wokół bohatera (efekty mocy każdego zawodu; wszystkie zawody odblokowane)
//   4 - paczki sprzętu 3 jakości i skrzynka z narzędziem obok bohatera
//   5 - aktywne stany: zatrucie, poślizg, porażenie
//   6 - porównanie sprzętu: założony kask i rękawice, obok (w prawo) paczki: lepszy kask i gorsze rękawice
//   7 - termos: 2 kawy w termosie, ranny bohater (35% HP), kawa na polu w prawo
//   8 - kryt i unik: markowy sprzęt z cechą Szczęście +1, obok (w lewo) wytrzymała Pleśń atakująca bohatera
//   9 - statystyki: Szkolenia Warsztaty i Kurs BHP II kupione, sprzęt z cechami SIŁ/ZRĘ/INT +1,
//       obok (w prawo) skrzynka z Tabletem z projektem (narzędzie INT)
//  10 - uprawnienia i zlecenia: część odznak zdobyta (premie na budowę), zlecenia w toku,
//       Mocarz o krok od ukończenia (moc R + L+R+SELECT = baner), Stały klient o jedną wygraną (baner na końcu)
//  11 - pamiątki: wszystkie odblokowane, Termos babci po 8 budowach (ranga III, wybrany), Kask ojca po 3 (II)
//  12 - wydarzenia na placu: start od etapu 2 z wydarzeniem; każdy kolejny etap bez bossa ma wydarzenie
//       (po kolei z listy; L+R+SELECT przechodzi dalej)
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

    // Profil scenariusza (po unlock_all, przed ekranem tytułowym).
    inline void setup_profile(core::profile& p, int scenario)
    {
        if(scenario < 9) return;
        core::set_flag(p, core::prologue_seen);
        for(int i = 0; i < data::upgrades_count; ++i)
            if(data::upgrades[i].effect == core::upgrade_effect::luck || data::upgrades[i].effect == core::upgrade_effect::craft)
                p.levels[i] = uint8_t(data::upgrades[i].levels);
        p.tools = uint8_t((1 << data::tools_count) - 1);
        if(scenario == 10)
        {
            p.badges = uint16_t((1 << data::badge_bez_usterek) | (1 << data::badge_seryjny) | (1 << data::badge_kolekcjoner)
                                | (1 << data::badge_osiedle));
            p.kills_total = 150; p.brand_total = 2; p.class_wins = 3;
            for(int i = 0; i < data::contracts_count; ++i)
            {
                if(data::contracts[i].kind == core::contract_kind::powers) p.powers_total = uint16_t(data::contracts[i].target - 1);
                if(data::contracts[i].kind == core::contract_kind::wins) p.wins = data::contracts[i].target - 1;   // wygrana = baner na końcu
            }
        }
        if(scenario == 11)
        {
            p.badges = uint16_t(p.badges | (1 << data::badge_bez_usterek));
            for(int i = 0; i < data::contracts_count; ++i) if(data::contracts[i].keepsake >= 0) p.contracts = uint8_t(p.contracts | (1 << i));
            p.keepsake_runs[0] = 8; p.keepsake_runs[1] = 3; p.keepsake_runs[3] = 1;
            p.keepsake = 1;
        }
    }

    inline int trait_index(core::trait_effect e)
    {
        for(int i = 0; i < data::gear_traits_count; ++i) if(data::gear_traits[i].effect == e) return i;
        return 0;
    }

    inline int tool_index(const char* weapon_name)
    {
        for(int i = 0; i < data::tools_count; ++i)
        {
            const char* a = data::weapons[data::tools[i].weapon].name;
            const char* b = weapon_name;
            while(*a && *a == *b) { ++a; ++b; }
            if(*a == *b) return i;
        }
        return 0;
    }

    // Scenariusz 12: wymusza wydarzenie na placu (kolejne z listy), jeśli los go nie dał.
    inline void force_event(core::game& g)
    {
        static int next = 0;
        if(g.stage_event >= 0 || g.stage == 0 || data::stages[g.stage].boss >= 0) return;
        g.apply_event(next++ % data::site_events_count);
    }

    // Wołane po przejściu na kolejny etap (harmonogram, Hurtownia).
    inline void after_next_stage(core::game& g, int scenario)
    {
        if(scenario == 12) force_event(g);
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
            case 6:
            {
                g.pickups_count = 0;
                g.equip(0, 0, 0);                                   // kask budowlany, Szczęście +1
                g.equip(1, 2, 4);                                   // rękawice markowe, Odnowienie mocy -1
                g.pickups[g.pickups_count++] = { int8_t(g.hero.x + 1), g.hero.y, core::gear_box, true, uint8_t(0 * 3 + 2), 3 };
                if(g.lv.at(g.hero.x + 2, g.hero.y) == core::tile::floor)
                    g.pickups[g.pickups_count++] = { int8_t(g.hero.x + 2), g.hero.y, core::gear_box, true, uint8_t(1 * 3 + 1), 1 };
                break;
            }
            case 7:
                g.pickups_count = 0;
                g.thermos = 2;
                g.hero.hp = int16_t(g.hero.max_hp * 35 / 100);
                g.pickups[g.pickups_count++] = { int8_t(g.hero.x + 1), g.hero.y, core::coffee, true };
                break;
            case 8:
            {
                for(int i = 0; i < data::gear_slots_count; ++i) g.equip(i, 2, 0);   // szczęście +3: kryt i unik
                g.enemies_count = 0;
                g.spawn(data::enemy_plesn, g.hero.x - 1, g.hero.y);
                core::actor& e = g.enemies[0];
                e.hp = e.max_hp = 300; e.awake = true;
                g.hero.max_hp = g.hero.hp = 300;
                break;
            }
            case 9:
                g.pickups_count = 0;
                g.equip(0, 1, trait_index(core::trait_effect::str));
                g.equip(1, 1, trait_index(core::trait_effect::agi));
                g.equip(2, 1, trait_index(core::trait_effect::intel));
                g.pickups[g.pickups_count++] = { int8_t(g.hero.x + 1), g.hero.y, core::tool, true, uint8_t(tool_index("Tablet z projektem")) };
                break;
            case 10:
                g.enemies_count = 0;
                place_enemy(g, data::enemy_kornik, 2, 3, true, 0);
                break;
            case 12:
                g.start_stage(1);
                force_event(g);
                break;
            default:
                break;
        }
        g.update_fov();
    }
}
