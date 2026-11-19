// Bot testów (core_tests.cpp, golden_dump.cpp) - port 1:1 z LifeLike.Core Bot.cs i BotWeapons.cs (v0.23.0: bronie - tryb B,
// wzory pól, pas z kaburą, ocena znalezionych broni). Musi zgadzać się z C# (test złoty).
#pragma once
#include <cstdint>
#include <queue>
#include <unordered_map>
#include <vector>
#include "core.h"

namespace botw
{
    using namespace core;

    // Ustawienia bota (BotOptions): default_only - tylko broń domyślna zawodu; weapons = false - bot jak w v0.22.1.
    struct bot_options
    {
        bool default_only = false;
        bool weapons = true;
    };

    static const int dirs[4][2] = { { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 } };
    constexpr int avoid_w = 40;          // pole z niechcianą bronią (nadepnięcie ją podnosi)
    constexpr int better_pct = 115;      // o ile lepsza broń, by ją wziąć / przełożyć
    constexpr int perk_score_pct = 6;    // premia oceny za każdy profit
    constexpr int tool_goal_cost = 28;   // ile drogi bot zboczy po lepszą broń

    // Pole przechodnie dla bota: także tymczasowy mur (Ścianka) - znika za kilka tur.
    inline bool bot_open(const game& g, int x, int y)
    {
        if(g.lv.passable(x, y)) return true;
        for(int i = 0; i < g.walls_count; ++i) if(g.walls[i].x == x && g.walls[i].y == y) return true;
        return false;
    }
    using avoid_map = bool[map_h][map_w];
    inline int bot_w(const game& g, int x, int y, const avoid_map* avoid = nullptr)
    {
        return avoid && (*avoid)[y][x] ? avoid_w : (g.mud(x, y) ? 6 : 2);
    }
    // Koszt drogi (4 kierunki): krok = waga pola, z którego + waga pola, na które.
    inline void bot_cost(const game& g, int sx, int sy, int (&dist)[map_h][map_w], const avoid_map* avoid = nullptr)
    {
        for(auto& r : dist) for(auto& c : r) c = -1;
        std::priority_queue<std::pair<int, int>, std::vector<std::pair<int, int>>, std::greater<>> q;
        dist[sy][sx] = 0; q.push({ 0, sy * map_w + sx });
        while(! q.empty())
        {
            auto [c, p] = q.top(); q.pop();
            const int x = p % map_w, y = p / map_w;
            if(c != dist[y][x]) continue;
            for(int k = 0; k < 4; ++k)
            {
                const int nx = x + dirs[k][0], ny = y + dirs[k][1];
                if(! bot_open(g, nx, ny)) continue;
                const int nc = c + bot_w(g, x, y, avoid) + bot_w(g, nx, ny, avoid);
                if(dist[ny][nx] < 0 || nc < dist[ny][nx]) { dist[ny][nx] = nc; q.push({ nc, ny * map_w + nx }); }
            }
        }
    }

    // ------------------------------------------------------------------ ocena broni (BotWeapons.Score) z pamięcią na budowę
    inline const game*& cache_game() { static const game* g = nullptr; return g; }
    inline std::unordered_map<long long, int>& cache() { static std::unordered_map<long long, int> c; return c; }
    // Nowa budowa (C#: nowy obiekt Game = nowa pamięć). Wołać przed każdą budową bota.
    inline void reset_cache() { cache().clear(); cache_game() = nullptr; }

    inline int popcount8(int v) { int n = 0; for(unsigned u = unsigned(v); u; u &= u - 1) ++n; return n; }
    inline int avg(const game& g, int weapon, int rarity, int perks, int def_id)
    {
        return weapon < 0 ? g.weapon_breakdown(def_id).avg10 : g.weapon_breakdown(def_id, weapon, -1, -1, 0, rarity, perks).avg10;
    }
    inline int proc_pct(const weapon_def& w)
    {
        const proc_def& p = w.proc;
        if(p.every <= 0) return 0;
        const int n = imax(1, p.every);
        int v = (p.dmg_pct - 100) / n;
        if(p.effect == mode_effect::stun || p.effect == mode_effect::root || p.effect == mode_effect::burn || p.effect == mode_effect::dot) v += 30 / n;
        if(p.effect2 == mode_effect::chain || p.effect2 == mode_effect::spread) v += 5 / n;
        return v;
    }
    inline int shape(const game& g, int weapon)
    {
        const weapon_def& w = weapon < 0 ? g.weapon() : data::weapons[weapon];
        const pattern_kind k = w.mode_a.pattern;
        const bool multi = k == pattern_kind::arc || k == pattern_kind::line || k == pattern_kind::line_both || k == pattern_kind::line4
                        || k == pattern_kind::spray || k == pattern_kind::around || k == pattern_kind::cross || k == pattern_kind::throw3x3;
        return 100 + 25 * (imin(3, w.range) - 1) + (multi ? 10 : 0);
    }
    inline int score_raw(const game& g, int weapon, int rarity, int perks)
    {
        long long sum = 0, n = 0;
        const int last = imin(g.stage + 1, g.route_count() - 1);
        for(int s = g.stage; s <= last; ++s)
        {
            const stage_def& sd = g.sdef(s);
            for(int k = 0; k < sd.pool_count; ++k) { sum += avg(g, weapon, rarity, perks, sd.pool[k]); ++n; }
            if(sd.boss >= 0) { sum += 2 * avg(g, weapon, rarity, perks, sd.boss); n += 2; }
        }
        const int a = n > 0 ? int(sum / n) : avg(g, weapon, rarity, perks, -1);
        return a * shape(g, weapon) / 100 * (100 + proc_pct(weapon < 0 ? g.weapon() : data::weapons[weapon])) / 100;
    }
    inline int score(const game& g, int weapon, int rarity, int perks = 0)
    {
        if(cache_game() != &g) { cache_game() = &g; cache().clear(); }
        if(weapon < 0) perks = g.weapon_perks;
        const long long key = (((((long long)g.stage * 64 + weapon + 1) * 8 + rarity) * 64 + (weapon < 0 ? g.weapon_override + 1 : 0)) * 1024
                               + (weapon < 0 ? g.weapon_lvl * 128 + g.weapon_rarity * 32 : 0) + g.hero_level + g.dmg_bonus * 40) * 256 + perks;
        auto it = cache().find(key);
        if(it != cache().end()) return it->second;
        const int v = score_raw(g, weapon, rarity, perks) * (100 + perk_score_pct * popcount8(perks)) / 100;
        cache()[key] = v;
        return v;
    }
    inline int score_vs(const game& g, int weapon, int rarity, int perks, int def_id)
    {
        if(weapon < 0) perks = g.weapon_perks;
        return avg(g, weapon, rarity, perks, def_id) * shape(g, weapon) / 100 * (100 + perk_score_pct * popcount8(perks)) / 100;
    }
    inline bool wants_tool(const game& g, int t, int rarity, int perks = 0)
    {
        if(t < 0 || t >= data::tools_count) return false;
        const int w = data::tools[t].weapon;
        if(g.has_belt() && g.weapon2 < 0) return true;   // pusta kabura
        const int r = data::weapons[w].master ? data::weapon_rarities_count - 1 : rarity;
        int hand = score(g, -1, 0);
        if(g.has_belt() && g.weapon2 >= 0) hand = imin(hand, score(g, g.weapon2, g.weapon2_rarity, g.weapon2_perks));
        return score(g, w, r, perks) * 100 > hand * better_pct;
    }
    // v0.23.2: karta broni (BotWeapons.ToolOffer) - lepszą bierze, porównywalną (>= backpack_pct% ręki) do plecaka, resztę zostawia.
    constexpr int backpack_pct = 90;
    inline void tool_offer(game& g, const bot_options& o)
    {
        if(! g.has_tool_offer()) return;
        if(o.default_only) { g.decline_tool(); return; }
        if(wants_tool(g, g.tool_offer, g.tool_offer_rarity, g.tool_offer_perks)) { g.accept_tool(); return; }
        const int w = data::tools[g.tool_offer].weapon;
        const int r = data::weapons[w].master ? data::weapon_rarities_count - 1 : g.tool_offer_rarity;
        if(g.backpack_full() || score(g, w, r, g.tool_offer_perks) * 100 < score(g, -1, 0) * backpack_pct || ! g.tool_to_backpack()) g.decline_tool();
    }
    // Pola z bronią, której bot nie chce (tylko domyślna: każda). false = brak takich pól.
    inline bool avoid_cells(const game& g, const bot_options& o, avoid_map& avoid)
    {
        bool any = false;
        for(int i = 0; i < g.pickups_count; ++i)
        {
            const pickup& p = g.pickups[i];
            if(! p.active || p.type != tool) continue;
            if(! o.default_only && wants_tool(g, p.arg, p.trait, p.perks)) continue;
            if(! any) { for(auto& r : avoid) for(auto& c : r) c = false; any = true; }
            avoid[p.y][p.x] = true;
        }
        return any;
    }
    inline bool tool_goal(const game& g, const int (&hd)[map_h][map_w], int& gx, int& gy)
    {
        gx = gy = -1;
        int best = tool_goal_cost + 1;
        for(int i = 0; i < g.pickups_count; ++i)
        {
            const pickup& p = g.pickups[i];
            const int dd = hd[p.y][p.x];
            if(! p.active || p.type != tool || dd < 0 || dd >= best || ! wants_tool(g, p.arg, p.trait, p.perks)) continue;
            best = dd; gx = p.x; gy = p.y;
        }
        return gx >= 0;
    }
    inline int effect_value(const game& g, mode_effect eff, int val, int turns, int t, const enemy_def& ed)
    {
        const bool boss = t == g.boss;
        const actor& e = g.enemies[t];
        switch(eff)
        {
            case mode_effect::stun: return boss || e.stun > 0 ? 0 : (val <= 0 || val >= 100 ? 100 : val) * imax(1, turns) * 6 / 10;
            case mode_effect::root: return boss || e.root > 0 ? 0 : 15 * imax(1, turns);
            case mode_effect::mark: return e.mark_turns > 0 ? 0 : val * imin(3, imax(1, turns)) / 2;
            case mode_effect::def_down: return e.def_down > 0 || ed.defense <= 0 ? 0 : 10 * imin(ed.defense, val) * imin(3, imax(1, turns)) / 2;
            case mode_effect::dot: return e.dot > 0 ? 0 : 12 * imax(1, turns);
            case mode_effect::dry: return ed.elem == element::water && ! boss ? 80 : 0;
            case mode_effect::crit_next: return 40;
            case mode_effect::wet: return e.wet > 0 ? 0 : 10;
            default: return 0;
        }
    }
    inline int mode_value(const game& g, const weapon_mode& m, int ei)
    {
        int8_t hit[pattern_max_cells];
        const int n = g.pattern_targets(m, ei, hit);
        if(n == 0) return 0;
        const weapon_def& w = g.weapon();
        int v = 0;
        for(int i = 0; i < n; ++i)
        {
            const int t = hit[i];
            const enemy_def& ed = data::enemies[g.enemies[t].def_id];
            v += hit_pct(w, ed) * m.dmg_pct / 100;
            v += effect_value(g, m.effect, m.value, m.turns, t, ed) + effect_value(g, m.effect2, m.value2, m.turns2, t, ed);
        }
        return v;
    }
    inline bool try_swap(game& g, int near)
    {
        if(! g.has_belt() || g.weapon2 < 0) return false;
        int hand, other;
        if(near >= 0)
        {
            const int def = g.enemies[near].def_id;
            hand = score_vs(g, -1, 0, 0, def);
            other = score_vs(g, g.weapon2, g.weapon2_rarity, g.weapon2_perks, def);
            const bool adjacent = cheb(g.hero.x, g.hero.y, g.enemies[near].x, g.enemies[near].y) <= 1;
            if(adjacent && g.hero.hp * 2 < g.hero.max_hp) return false;
        }
        else
        {
            hand = score(g, -1, 0);
            other = score(g, g.weapon2, g.weapon2_rarity, g.weapon2_perks);
        }
        return other * 100 > hand * better_pct && g.player_swap_weapon();
    }
    inline bool position(game& g)
    {
        const int rg = g.weapon_range();
        int t = -1, bd = 99;
        for(int i = 0; i < g.enemies_count; ++i)
        {
            const actor& e = g.enemies[i];
            const int d = cheb(g.hero.x, g.hero.y, e.x, e.y);
            if(e.alive && g.visible(e.x, e.y) && d <= rg + 1 && d < bd) { bd = d; t = i; }
        }
        if(t < 0) return false;
        const int hx = g.hero.x, hy = g.hero.y;
        for(int k = 0; k < 4; ++k)
        {
            const int nx = hx + dirs[k][0], ny = hy + dirs[k][1];
            if(! g.lv.passable(nx, ny) || g.occupied(nx, ny) || g.danger_cell(nx, ny) || g.mud(nx, ny)) continue;
            const int d = cheb(nx, ny, g.enemies[t].x, g.enemies[t].y);
            if(d < 2 || d > rg) continue;
            g.hero.x = int8_t(nx); g.hero.y = int8_t(ny);
            const bool ok = g.in_pattern(g.weapon().mode_a, t);
            g.hero.x = int8_t(hx); g.hero.y = int8_t(hy);
            if(ok) return g.player_move(dirs[k][0], dirs[k][1]);
        }
        return false;
    }
    // Tura walki bronią: zamiana z pasa, tryb B, atak trybem A, ustawienie broni dystansowej pod wzór. true = tura zużyta.
    inline bool fight(game& g, const bot_options& o)
    {
        if(g.danger_cell(g.hero.x, g.hero.y)) return false;
        const int near = g.nearest_visible_enemy();
        if(! o.default_only && try_swap(g, near)) return true;
        if(near < 0 && g.can_weapon_b_open_wall() && g.player_weapon_b_wall()) return true;
        int ba = -1, va = 0;
        for(int i = 0; i < g.enemies_count; ++i)
        {
            if(! g.enemies[i].alive || ! g.visible(g.enemies[i].x, g.enemies[i].y)) continue;
            const int v = mode_value(g, g.weapon().mode_a, i);
            if(v > va) { va = v; ba = i; }
        }
        if(g.weapon_b_cd == 0)
        {
            int bb = -1, vb = 0;
            for(int i = 0; i < g.enemies_count; ++i)
            {
                if(! g.enemies[i].alive || ! g.visible(g.enemies[i].x, g.enemies[i].y)) continue;
                const int v = mode_value(g, g.weapon().mode_b, i);
                if(v > vb) { vb = v; bb = i; }
            }
            if(bb >= 0 && vb > va && g.player_weapon_b(bb)) return true;
        }
        if(ba >= 0 && g.weapon().range > 1) return g.player_attack(ba);
        return g.weapon().range > 1 && position(g);
    }

    // Krok bota (Bot.Step). no_coffee / drinks: warianty statystyk z core_tests.cpp (C# ich nie ma - w teście złotym wyłączone).
    inline void step(game& g, const bot_options& o = bot_options(), bool no_coffee = false, int* drinks = nullptr)
    {
        g.bot_gear_offer(false);   // v0.23.2: lepszą zakłada, tej samej jakości do plecaka, gorszą zostawia (pas z bronią w kaburze zostaje)
        if(o.weapons && g.has_tool_offer()) { tool_offer(g, o); return; }   // wg oceny broni; porównywalna do plecaka; tylko domyślna: odmowa
        if(g.bot_pending()) return;   // wydarzenie, premia z wydarzenia, cecha narzędzia, narzędzie
        if(g.can_open_secret() && g.hero.x == g.secret_front_x() && g.hero.y == g.secret_front_y()   // przed magazynem: otwórz
           && g.player_move(g.secret_x - g.hero.x, g.secret_y - g.hero.y)) return;
        if(! no_coffee && g.thermos > 0 && g.hero.hp * 100 < g.hero.max_hp * data::bot_drink_below_pct && g.player_drink())
        {
            if(drinks) ++*drinks;
            return;
        }
        static avoid_map avoid_buf;
        const avoid_map* avoid = o.weapons && avoid_cells(g, o, avoid_buf) ? &avoid_buf : nullptr;
        static int hd[map_h][map_w], td[map_h][map_w];
        bot_cost(g, g.hero.x, g.hero.y, hd, avoid);
        int tx = g.stairs_x, ty = g.stairs_y, best = 999999;
        {
            int gx, gy;
            if(g.bot_goal(gx, gy) && hd[gy][gx] >= 0) { tx = gx; ty = gy; }
            else if(o.weapons && ! o.default_only && tool_goal(g, hd, gx, gy)) { tx = gx; ty = gy; }   // lepsza broń w pobliżu
        }
        if(g.stairs_locked())   // pieczątki (Akt 0): najpierw najbliższy dokument, potem schody
        {
            int bdoc = 999999;
            for(int i = 0; i < g.pickups_count; ++i)
            {
                const pickup& p = g.pickups[i]; const int dd = hd[p.y][p.x];
                if(p.active && p.type == document && dd >= 0 && dd < bdoc) { bdoc = dd; tx = p.x; ty = p.y; }
            }
        }
        for(int i = 0; i < g.enemies_count; ++i)
        {
            const actor& e = g.enemies[i]; const int dd = hd[e.y][e.x];
            if(e.alive && dd >= 0 && dd < best && (dd < 32 || g.stairs_x < 0)) { best = dd; tx = e.x; ty = e.y; }
        }
        const bool has_target = tx >= 0 && hd[ty][tx] >= 0;
        if(has_target) bot_cost(g, tx, ty, td, avoid);
        if(g.danger_cell(g.hero.x, g.hero.y))   // zapowiedziany cios bossa: zejdź z czerwonych pól w stronę celu
        {
            int bk = -1, bs = -1000000;
            for(int k = 0; k < 4; ++k)
            {
                const int nx = g.hero.x + dirs[k][0], ny = g.hero.y + dirs[k][1];
                if(! g.lv.passable(nx, ny) || g.occupied(nx, ny)) continue;
                const int s = (g.danger_cell(nx, ny) ? 0 : 10000) - (has_target && td[ny][nx] >= 0 ? td[ny][nx] : 5000);
                if(s > bs) { bs = s; bk = k; }
            }
            if(bk >= 0 && g.player_move(dirs[bk][0], dirs[bk][1])) return;
        }
        if(o.weapons && fight(g, o)) return;   // v0.23.0: zamiana z pasa, tryb B, atak wzorem A, ustawienie pod wzór
        if(g.nearest_target() >= 0 && g.weapon().range > 1) { g.player_attack_nearest(); return; }
        if(! has_target || (tx == g.hero.x && ty == g.hero.y)) { g.player_wait(); return; }
        for(int k = 0; k < 4; ++k)   // krok na sąsiednie pole bliżej celu
        {
            const int nx = g.hero.x + dirs[k][0], ny = g.hero.y + dirs[k][1];
            if(! bot_open(g, nx, ny) || td[ny][nx] < 0 || td[ny][nx] + bot_w(g, nx, ny, avoid) + bot_w(g, g.hero.x, g.hero.y, avoid) != td[g.hero.y][g.hero.x]) continue;
            if(! g.lv.passable(nx, ny)) { g.player_wait(); return; }   // mur Ścianki na drodze
            if(g.danger_cell(nx, ny) && g.enemy_at(nx, ny) < 0) { g.player_wait(); return; }   // nie wchodzi na czerwone pola
            if(! g.player_move(dirs[k][0], dirs[k][1])) g.player_wait();
            return;
        }
        g.player_wait();
    }

    // Wariant "smart" (Bot.StepSmart): moc przy bliskim problemie, naprawy, brygada, celowanie; v0.23.0: bronie przed celowaniem.
    inline void step_smart(game& g, const bot_options& o = bot_options())
    {
        g.bot_gear_offer(true);   // v0.23.2: tej samej jakości zakłada (wymiana cechy), gorszą zostawia
        if(o.weapons && g.has_tool_offer()) { tool_offer(g, o); return; }
        if(g.bot_pending()) return;
        if(g.thermos > 0 && g.hero.hp * 2 < g.hero.max_hp && g.player_drink()) return;
        for(int k = 0; k < data::repairs_count; ++k)   // naprawy: Kładka przy kałużach; Załataj przy niskim HP
        {
            const bool want = data::repairs[k].effect == repair_effect::bridge || g.hero.hp * 3 < g.hero.max_hp;
            if(want && g.repair_blocked(k) == game::repair_ok && g.player_repair(k)) return;
        }
        if(g.helper_called < 0 && g.nearest_visible_enemy() >= 0)   // brygada: przy pierwszym problemie na etapie
            for(int k = 0; k < data::brigade_count; ++k)
            {
                const int h = (g.stage + k) % data::brigade_count;
                if(g.helper_blocked(h) == game::helper_ok && g.call_helper(h)) return;
            }
        if(! g.danger_cell(g.hero.x, g.hero.y) && g.ability_cd == 0)
        {
            const int t = g.nearest_visible_enemy();
            const bool pointless = t >= 0 && g.cdef().ability == ability_effect::wall
                                   && (g.hero.hp * 2 >= g.hero.max_hp || g.has_tag(g.enemies[t], tag_stationary));
            if(t >= 0 && ! pointless && cheb(g.hero.x, g.hero.y, g.enemies[t].x, g.enemies[t].y) <= 2 && g.player_ability()) return;
        }
        if(o.weapons && fight(g, o)) return;   // v0.23.0: pas, tryb B, wzór A
        if(! g.danger_cell(g.hero.x, g.hero.y))
        {
            int8_t targets[max_enemies];
            const int n = g.targets_in_range(targets, max_enemies);
            if(n > 0 && g.player_attack(targets[0])) return;
        }
        step(g, o);
    }
}
