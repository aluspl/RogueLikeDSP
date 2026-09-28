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
//  13 - Inspekcja Pracy (boss w środku aktu III, sam) kilka pól od bohatera w pełnym sprzęcie: baner "Wszystko zgodnie
//       z BHP!" (2 tury ogłuszenia), potem Kontrola BHP w krzyż (3 tury na zejście), wezwanie Papierologii;
//       L+R+SELECT = pokonanie, baner "Protokół bez uwag" i harmonogram bez Hurtowni
//  14 - pogoda dnia: pierwszy etap w deszczu (kałuże obok bohatera), każdy kolejny etap z kolejną pogodą z listy
//       (Słonecznie, Upał, Mróz, Wiatr, Deszcz...; L+R+SELECT przechodzi dalej)
//  15 - brygada: 100 zł budżetu, wszyscy fachowcy odblokowani, trzy problemy obok bohatera (ogłuszone na chwilę),
//       zatrucie; telefon -> Zespół -> A = Brygada; kolejne etapy też ze 100 zł (L+R+SELECT)
//  16 - tryb inwestora: profil po pierwszej wygranej, włączone Budżet -30% i Problemy +20% HP, rekord stawki Murarza 3;
//       na wyborze zawodu SELECT = lista modyfikatorów
//  17 - wybór ścieżki: L+R+SELECT kończy etap, harmonogram z dwoma wariantami (lewo/prawo, A); kolejne etapy też
//  18 - materiały i naprawy: po 5 cementu/stali/drewna, deszcz (kałuże obok), bohater o 1 dośw. od awansu, obok
//       Kornik z 1 HP (A = usunięty, awans z napisem "AWANS!"), dalej ogłuszona Pleśń; telefon -> Sprzęt -> A:
//       naprawy Załataj i Kładka
//  19 - codzienna budowa: profil z datą 25.09.2026 i wynikami trzech wcześniejszych dni (R na tytule)
//  20 - wygrana: ostatni etap z bossem, dni poprzednich etapów wypełnione (L+R+SELECT = odbiór, harmonogram domu)
//  21 - porażka: 1 HP, obok przebudzony problem (B = czekaj, koniec budowy z motywacją: rekord, zlecenie, Szkolenia)
//  22 - Respekt: profil z Respektem i częścią rang, 3 wygrane (3 nagrody); tytuł -> SELECT -> Koszty -> SELECT = Respekt,
//       A kupuje, SELECT = Nagrody za odbiór
//  23 - Respekt za etap: profil z 12 Respektu; L+R+SELECT = etap zaliczony, baner "Respekt +2"; telefon -> Koszty
//  24 - nagroda za odbiór: profil bez wygranych, ostatni etap z bossem (L+R+SELECT = odbiór, "Nagroda: Młot udarowy")
//  25 - Dekarz: trzy problemy w linii w prawo (ogłuszone), wiatr (zasięg bez zmian); R = Rynna
//  26 - Tynkarz: trzy problemy w grupie 2 pola w prawo; R = Narzut
//  27 - Operator koparki: wytrzymały problem 3 pola w prawo; R = Taran, potem D-pad w prawo = ciosy (czasem odpychają)
//  28 - nowe narzędzia i sprzęt: obok bohatera (w prawo) skrzynki Młot udarowy i Pistolet do kotew, paczki Buty i Pas;
//       telefon -> Sprzęt (5 slotów)
//  29 - strzelcy (etap Mury parteru): Mostek termiczny 3 pola w prawo (strzela i ucieka), Przeciekająca papa 3 pola w dół
//  30 - dzieli się i wybucha: Woda gruntowa (1 HP) w prawo, Pęknięty pustak (1 HP) w lewo - A w prawo = podział,
//       D-pad w lewo = wybuch (czerwone pola, tura na zejście)
//  31 - łata, rośnie, stoi (etap Tynki, akt III - pył): Wilgoć 3 pola w prawo przy rannej Pleśni, Ugięcie stropu 2 w lewo,
//       Kamień w wykopie 3 w górę
//  32 - odpycha i wraca: Osuwisko skarpy obok w prawo, Dziurawa folia (1 HP, ogłuszona) w lewo
//  33 - akt I, błoto (etap Izolacja fundamentów): pole w prawo to błoto (wejście = tura)
//  34 - akt II, porywy (etap Ściany działowe): poryw za 2 tury (licznik w HUD), B = czekaj
//  35 - akt III, pył (etap Tynki i wylewki): mniejsze pole widzenia, pył w powietrzu, płytki i tynk
//  36 - Katalog usterek: wszystkie problemy znane (tytuł -> SELECT -> Katalog, zachowania pod listą)
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
        p.classes = uint8_t(((1 << data::classes_count) - 1) & ~data::reward_classes_mask);
        p.rewards = uint8_t(core::rewards_available());   // nagrody za odbiór: nowe zawody, narzędzia, buty i pas
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
        if(scenario == 19)
        {
            core::set_daily_date(p, 2026, 9, 25);
            int day = core::daily_number(2026, 9, 25);
            core::record_daily(p, day - 1, 3120, true);
            core::record_daily(p, day - 2, 1480, false);
            core::record_daily(p, day - 4, 2210, false);
        }
        if(scenario == 21) { p.best = 4200; p.xp = 12; p.kills_total = 180; }
        if(scenario == 22)
        {
            p.respect = 180; p.respect_total = 420; p.wins = 3; p.rewards = 3;
            p.respect_ranks[0] = 1; p.respect_ranks[5] = 2; p.respect_ranks[8] = 3; p.respect_ranks[12] = 1;
        }
        if(scenario == 23) { p.respect = 12; p.respect_total = 12; }
        if(scenario == 36) { p.catalog = 0xFFFF; p.catalog_hi = 0xFFFFFFFFu; }
        if(scenario == 24) { p.wins = 0; p.rewards = 0; p.respect = 30; }
        if(scenario == 16)
        {
            p.wins = 1;
            p.investor = 0x05;
            p.best_stake[1] = 3;
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

    // Scenariusz 14: wymusza pogodę (kolejna z listy, od deszczu).
    inline void force_weather(core::game& g)
    {
        static int next = data::weather_count - 1;
        g.weather = int8_t(next % data::weather_count);
        next = (next + 1) % data::weather_count;
        g.push(core::message().add("Pogoda: ").add(g.wdef().name).add(" (").add(g.wdef().short_name).add(")").as(g.wdef().bad ? core::bad : core::good));
    }

    // Wołane po przejściu na kolejny etap (harmonogram, Hurtownia).
    inline void after_next_stage(core::game& g, int scenario)
    {
        if(scenario == 12) force_event(g);
        if(scenario == 14) { force_weather(g); g.update_fov(); }
        if(scenario == 15) g.cash = 100;
    }

    // Zawód z mocą e (nowe zawody z nagród za odbiór).
    inline int class_of(core::ability_effect e)
    {
        for(int c = 0; c < data::classes_count; ++c) if(data::classes[c].ability == e) return c;
        return 0;
    }

    // Podłoga w prostokącie wokół bohatera (scenariusze mocy potrzebują miejsca w linii).
    inline void clear_area(core::game& g, int dx0, int dy0, int dx1, int dy1)
    {
        for(int y = g.hero.y + dy0; y <= g.hero.y + dy1; ++y)
            for(int x = g.hero.x + dx0; x <= g.hero.x + dx1; ++x)
                if(x >= 1 && y >= 1 && x < core::map_w - 1 && y < core::map_h - 1 && g.lv.t[y][x] == core::tile::wall) g.lv.t[y][x] = core::tile::floor;
    }

    inline void place_at(core::game& g, int def, int dx, int dy, int stun)
    {
        int x = g.hero.x + dx, y = g.hero.y + dy;
        if(g.enemies_count >= core::max_enemies || g.lv.at(x, y) != core::tile::floor || g.occupied(x, y)) return;
        g.spawn(def, x, y);
        g.enemies[g.enemies_count - 1].awake = true;
        g.enemies[g.enemies_count - 1].stun = int8_t(stun);
    }

    // Wołane raz, na wejściu na pierwszy etap budowy.
    inline void apply(core::game& g, int scenario)
    {
        if(scenario >= 25 && scenario <= 27)   // nowy zawód niezależnie od wyboru na ekranie zawodu
        {
            const core::ability_effect e[3] = { core::ability_effect::line, core::ability_effect::splash, core::ability_effect::ram };
            core::run_mods m = g.bonus;
            g.new_run(class_of(e[scenario - 25]), g.run_seed, g.diff, m);
        }
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
            case 14:
                force_weather(g);   // deszcz: kałuże widać wokół bohatera
                break;
            case 15:
                g.cash = 100;
                g.bonus.helpers = (1 << data::brigade_count) - 1;
                g.enemies_count = 0;
                place_enemy(g, data::enemy_papierologia, 1, 1, true, 3);
                place_enemy(g, data::enemy_kornik, 2, 2, true, 3);
                place_enemy(g, data::enemy_plesn, 2, 2, true, 3);
                g.apply_status(core::status_effect::poison, 6);
                break;
            case 13:
            {
                int st = 0;
                while(data::stages[st].boss != data::enemy_inspekcja) ++st;
                g.start_stage(st);
                for(int i = 0; i < g.boss; ++i) g.enemies[i].alive = false;          // sam boss (bez problemów etapu)
                for(int i = 0; i < data::gear_slots_count; ++i) g.equip(i, 0, 0);   // pełny sprzęt: kask, rękawice, kamizelka
                core::actor& b = g.enemies[g.boss];
                int x, y;
                if(free_cell(g, 3, 3, x, y)) { b.x = int8_t(x); b.y = int8_t(y); }
                b.awake = true;
                g.slam_counter = data::slam_every - 1;   // Kontrola BHP zaraz po ogłuszeniu
                g.summon_counter = data::enemies[data::enemy_inspekcja].summon_every - 1;
                break;
            }
            case 18:
            {
                for(int m = 0; m < data::materials_count; ++m) g.mats[m] = 5;
                for(int i = 0; i < data::weather_count; ++i) if(data::weather[i].effect == core::weather_effect::rain) g.weather = int8_t(i);
                g.run_xp = data::level_thresholds[0] - 1;
                g.enemies_count = 0;
                place_enemy(g, data::enemy_kornik, 1, 1, true, 5);
                g.enemies[0].hp = 1;
                place_enemy(g, data::enemy_plesn, 3, 3, true, 30);
                break;
            }
            case 20:
            {
                g.start_stage(data::stages_count - 1);
                for(int s = 0; s < data::stages_count - 1; ++s) g.stage_days[s] = uint16_t(18 + (s * 7) % 11);
                g.score = 4800;
                g.turns = 240;
                g.stage_start_turn = 200;
                break;
            }
            case 23:
                g.enemies_count = 0;
                place_enemy(g, data::enemy_kornik, 3, 4, false, 0);
                break;
            case 24:
            {
                g.start_stage(data::stages_count - 1);
                for(int s = 0; s < data::stages_count - 1; ++s) g.stage_days[s] = uint16_t(15 + (s * 5) % 9);
                g.score = 3900;
                g.respect = 24;
                break;
            }
            case 25:
            {
                clear_area(g, -1, -2, 6, 2);
                g.enemies_count = 0;
                for(int k = 2; k <= 4; ++k) place_at(g, k == 3 ? data::enemy_kornik : data::enemy_przeciek, k, 0, 30);
                for(int i = 0; i < data::weather_count; ++i) if(data::weather[i].effect == core::weather_effect::wind) g.weather = int8_t(i);
                g.push(core::message().add("Pogoda: ").add(g.wdef().name).add(" (").add(g.wdef().short_name).add(")").as(core::bad));
                break;
            }
            case 26:
            {
                clear_area(g, -1, -2, 5, 2);
                g.enemies_count = 0;
                place_at(g, data::enemy_kornik, 2, 0, 30);
                place_at(g, data::enemy_przeciek, 3, 1, 30);
                place_at(g, data::enemy_plesn, 3, -1, 30);
                place_at(g, data::enemy_kornik, -1, 2, 30);
                break;
            }
            case 27:
            {
                clear_area(g, -1, -2, 6, 2);
                g.enemies_count = 0;
                place_at(g, data::enemy_budzet, 3, 0, 30);
                if(g.enemies_count > 0) g.enemies[0].hp = g.enemies[0].max_hp = 80;   // wytrzyma szarżę i kilka ciosów (odepchnięcie)
                place_at(g, data::enemy_kornik, -3, 2, 40);   // dalej niż cel szarży
                break;
            }
            case 28:
            {
                clear_area(g, -1, -1, 5, 1);
                g.pickups_count = 0;
                int udarowy = -1, kotwy = -1, boots = -1, belt = -1, slip = trait_index(core::trait_effect::slip_res);
                for(int i = 0; i < data::tools_count; ++i) if(data::tools[i].reward) (udarowy < 0 ? udarowy : kotwy) = i;
                for(int i = 0; i < data::gear_slots_count; ++i)
                {
                    if(data::gear[i * 3].stat == core::gear_stat::dodge) boots = i;
                    if(data::gear[i * 3].stat == core::gear_stat::thermos) belt = i;
                }
                g.pickups[g.pickups_count++] = { int8_t(g.hero.x + 1), g.hero.y, core::tool, true, uint8_t(udarowy) };
                g.pickups[g.pickups_count++] = { int8_t(g.hero.x + 2), g.hero.y, core::gear_box, true, uint8_t(boots * 3 + 2), uint8_t(slip) };
                g.pickups[g.pickups_count++] = { int8_t(g.hero.x + 3), g.hero.y, core::gear_box, true, uint8_t(belt * 3 + 1), 0 };
                g.pickups[g.pickups_count++] = { int8_t(g.hero.x + 4), g.hero.y, core::tool, true, uint8_t(kotwy) };
                g.enemies_count = 0;
                break;
            }
            case 29:
                g.start_stage(2);
                g.enemies_count = 0;
                clear_area(g, -1, -1, 4, 4);
                place_at(g, data::enemy_mostek, 3, 0, 0);
                place_at(g, data::enemy_papa, 0, 3, 0);
                break;
            case 30:
                g.enemies_count = 0;
                clear_area(g, -3, -2, 3, 2);
                place_at(g, data::enemy_woda, 1, 0, 6);
                place_at(g, data::enemy_pustak, -1, 0, 6);
                for(int i = 0; i < g.enemies_count; ++i) g.enemies[i].hp = 1;
                break;
            case 31:
                g.start_stage(8);
                g.enemies_count = 0;
                clear_area(g, -3, -3, 4, 3);
                place_at(g, data::enemy_wilgoc, 3, 0, 0);
                place_at(g, data::enemy_plesn, 3, 2, 40);
                if(g.enemies_count > 1) g.enemies[1].hp = int16_t(g.enemies[1].max_hp - 7);
                place_at(g, data::enemy_ugiecie, -2, 0, 0);
                place_at(g, data::enemy_kamien, 0, -3, 0);
                break;
            case 32:
                g.enemies_count = 0;
                clear_area(g, -2, -1, 3, 1);
                place_at(g, data::enemy_osuwisko, 1, 0, 0);
                place_at(g, data::enemy_folia, -1, 0, 8);
                if(g.enemies_count > 1) g.enemies[1].hp = 1;
                break;
            case 33:
            {
                g.start_stage(1);
                g.enemies_count = 0;
                int best = 999, hx = g.hero.x, hy = g.hero.y;
                for(int y = 1; y < core::map_h - 1; ++y)   // najbliższe błoto z wolnym polem po lewej
                    for(int x = 2; x < core::map_w - 1; ++x)
                        if(g.mud(x, y) && g.lv.at(x - 1, y) == core::tile::floor && ! g.mud(x - 1, y) && core::cheb(x, y, hx, hy) < best)
                        { best = core::cheb(x, y, hx, hy); g.hero.x = int8_t(x - 1); g.hero.y = int8_t(y); }
                break;
            }
            case 34:
                g.start_stage(5);
                g.enemies_count = 0;
                g.stage_start_turn = g.turns - (data::acts[1].mech_value - 2);   // poryw za 2 tury
                break;
            case 35:
                g.start_stage(8);
                for(int i = 0; i < g.enemies_count; ++i) g.enemies[i].stun = 60;
                break;
            case 21:
            {
                g.hero.hp = 1;
                g.score = 900;
                g.enemies_count = 0;
                place_enemy(g, data::enemy_budzet, 1, 1, true, 0);
                break;
            }
            default:
                break;
        }
        g.update_fov();
    }
}
