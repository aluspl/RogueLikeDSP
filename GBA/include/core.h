#pragma once
// Rdzeń gry PlanBudowlany RogueLike. Port logiki z LifeLike.Core (C#):
// siatka + generator pokoje/korytarze z seedem, bump-to-attack, AI Idle/Chase, tury, dziennik.
// Zero alokacji, zero Butano: kompiluje się na GBA i na PC (tests/).
#include <cstdint>
#include <cstring>
#include "game_data.h"

namespace core
{
    constexpr int map_w = 32;
    constexpr int map_h = 32;
    constexpr int max_rooms = 10;
    constexpr int max_enemies = 12;
    constexpr int max_pickups = 4;
    constexpr int log_lines = 3;
    constexpr int log_len = 48;

    enum class tile : uint8_t { wall, floor, stairs };
    enum class status : uint8_t { playing, stage_clear, dead, won };
    enum pickup_type : uint8_t { coffee, helmet, plan };

    inline int iabs(int v) { return v < 0 ? -v : v; }
    inline int imax(int a, int b) { return a > b ? a : b; }
    inline int imin(int a, int b) { return a < b ? a : b; }
    inline int isign(int v) { return (v > 0) - (v < 0); }
    inline int cheb(int ax, int ay, int bx, int by) { return imax(iabs(ax - bx), iabs(ay - by)); }

    // xorshift32 - identyczny algorytm można zaimplementować w C# (testy złote seed -> mapa)
    struct rng
    {
        uint32_t s = 2463534242u;
        void seed(uint32_t v) { s = v ? v : 2463534242u; }
        uint32_t next() { s ^= s << 13; s ^= s >> 17; s ^= s << 5; return s; }
        int range(int lo, int hi) { return lo + int(next() % uint32_t(hi - lo + 1)); }   // [lo, hi]
    };

    struct room
    {
        int8_t x, y, w, h;
        int cx() const { return x + w / 2; }
        int cy() const { return y + h / 2; }
        bool intersects(const room& o) const
        {
            return x - 1 < o.x + o.w && x + w + 1 > o.x && y - 1 < o.y + o.h && y + h + 1 > o.y;
        }
    };

    struct level
    {
        tile t[map_h][map_w];
        room rooms[max_rooms];
        int rooms_count = 0;

        bool in(int x, int y) const { return x >= 0 && y >= 0 && x < map_w && y < map_h; }
        tile at(int x, int y) const { return in(x, y) ? t[y][x] : tile::wall; }
        bool passable(int x, int y) const { return at(x, y) != tile::wall; }

        void carve(int x, int y) { if(in(x, y) && t[y][x] == tile::wall) t[y][x] = tile::floor; }

        void generate(rng& r)
        {
            for(auto& row : t) for(auto& c : row) c = tile::wall;
            rooms_count = 0;
            for(int attempt = 0; attempt < max_rooms * 12 && rooms_count < max_rooms; ++attempt)
            {
                room rm;
                rm.w = int8_t(r.range(4, 8));
                rm.h = int8_t(r.range(4, 7));
                rm.x = int8_t(r.range(1, map_w - rm.w - 2));
                rm.y = int8_t(r.range(1, map_h - rm.h - 2));
                bool ok = true;
                for(int i = 0; i < rooms_count; ++i) if(rooms[i].intersects(rm)) { ok = false; break; }
                if(! ok) continue;
                for(int y = rm.y; y < rm.y + rm.h; ++y) for(int x = rm.x; x < rm.x + rm.w; ++x) carve(x, y);
                if(rooms_count > 0)  // korytarz w kształcie L do poprzedniego pokoju
                {
                    const room& p = rooms[rooms_count - 1];
                    int ax = p.cx(), ay = p.cy(), bx = rm.cx(), by = rm.cy();
                    bool hfirst = r.next() & 1;
                    int kx = hfirst ? bx : ax, ky = hfirst ? ay : by;
                    for(int x = ax, y = ay;; ) { carve(x, y); if(x == kx && y == ky) break; x += isign(kx - x); y += isign(ky - y); }
                    for(int x = kx, y = ky;; ) { carve(x, y); if(x == bx && y == by) break; x += isign(bx - x); y += isign(by - y); }
                }
                rooms[rooms_count++] = rm;
            }
        }
    };

    struct actor
    {
        int8_t x = 0, y = 0;
        int16_t hp = 0, max_hp = 0;
        int8_t def_id = -1;     // indeks w data::enemies; -1 = gracz
        bool alive = false;
        bool awake = false;
    };

    struct pickup { int8_t x, y; uint8_t type; bool active; };

    // Dziennik budowy (log zdarzeń) - krótkie linie UTF-8
    struct message
    {
        char s[log_len];
        int n = 0;
        message() { s[0] = 0; }
        message& add(const char* t) { while(*t && n < log_len - 1) s[n++] = *t++; s[n] = 0; return *this; }
        message& add(int v)
        {
            char b[12]; int k = 0; bool neg = v < 0; unsigned u = neg ? unsigned(-v) : unsigned(v);
            do { b[k++] = char('0' + u % 10); u /= 10; } while(u);
            if(neg) b[k++] = '-';
            while(k && n < log_len - 1) s[n++] = b[--k];
            s[n] = 0; return *this;
        }
    };

    struct game
    {
        level lv;
        rng r;
        actor hero;
        actor enemies[max_enemies];
        int enemies_count = 0;
        pickup pickups[max_pickups];
        int pickups_count = 0;
        int cls = 0;
        int stage = 0;               // 0..stages_count-1
        int diff = data::default_difficulty;   // indeks w data::difficulties
        int tier = 0;                // NG+: ile razy budowa została już ukończona
        int def_bonus = 0, dmg_bonus = 0;
        int turns = 0, kills = 0, score = 0;
        int boss = -1;               // indeks w enemies[]
        int stairs_x = -1, stairs_y = -1;
        status st = status::playing;
        message log[log_lines];
        uint32_t turn_events = 0;    // bitmaska: które indeksy przeciwników zostały trafione w tej turze (efekt)
        bool hero_hit = false;

        const class_def& cdef() const { return data::classes[cls]; }
        const weapon_def& weapon() const { return data::weapons[cdef().weapon]; }
        const difficulty_def& ddef() const { return data::difficulties[diff]; }

        // Trudność = etap x poziom x NG+. Mnożniki w procentach, premie sumowane.
        int enemy_hp_pct() const
        {
            return data::stages[stage].hp_pct * ddef().hp_pct / 100 * (100 + tier * data::ng_hp_pct_per_tier) / 100;
        }
        int enemy_dmg_bonus() const
        {
            return data::stages[stage].dmg_bonus + ddef().dmg_bonus + tier * data::ng_dmg_bonus_per_tier;
        }
        int score_pct() const { return ddef().score_pct * (100 + tier * data::ng_score_pct_per_tier) / 100; }

        void push(const message& m)
        {
            for(int i = 0; i < log_lines - 1; ++i) log[i] = log[i + 1];
            log[log_lines - 1] = m;
        }

        void new_run(int class_index, uint32_t seed, int difficulty = data::default_difficulty)
        {
            *this = game();
            cls = class_index;
            diff = difficulty;
            r.seed(seed);
            hero.max_hp = hero.hp = cdef().max_health;
            hero.alive = true;
            start_stage(0);
        }

        bool occupied(int x, int y) const
        {
            if(hero.alive && hero.x == x && hero.y == y) return true;
            for(int i = 0; i < enemies_count; ++i)
                if(enemies[i].alive && enemies[i].x == x && enemies[i].y == y) return true;
            return false;
        }

        void random_free_cell_in_room(const room& rm, int& ox, int& oy)
        {
            for(int k = 0; k < 40; ++k)
            {
                int x = r.range(rm.x, rm.x + rm.w - 1), y = r.range(rm.y, rm.y + rm.h - 1);
                if(lv.at(x, y) == tile::floor && ! occupied(x, y)) { ox = x; oy = y; return; }
            }
            ox = rm.cx(); oy = rm.cy();
        }

        void start_stage(int s)
        {
            stage = s;
            st = status::playing;
            lv.generate(r);
            const stage_def& sd = data::stages[stage];
            const room& first = lv.rooms[0];
            const room& last = lv.rooms[lv.rooms_count - 1];
            hero.x = int8_t(first.cx()); hero.y = int8_t(first.cy());
            boss = -1; stairs_x = stairs_y = -1;
            if(sd.boss < 0) { stairs_x = last.cx(); stairs_y = last.cy(); lv.t[stairs_y][stairs_x] = tile::stairs; }

            enemies_count = 0;
            for(int i = 0; i < sd.enemy_count && enemies_count < max_enemies; ++i)
            {
                int room_i = 1 + r.range(0, lv.rooms_count - 2 > 0 ? lv.rooms_count - 2 : 0);
                if(room_i >= lv.rooms_count) room_i = lv.rooms_count - 1;
                int x, y; random_free_cell_in_room(lv.rooms[room_i], x, y);
                spawn(sd.pool[r.range(0, sd.pool_count - 1)], x, y);
            }
            if(sd.boss >= 0)
            {
                int x = last.cx(), y = last.cy();
                if(occupied(x, y)) random_free_cell_in_room(last, x, y);
                boss = enemies_count;
                spawn(sd.boss, x, y);
            }

            pickups_count = 0;
            for(int i = 0; i < 3 && lv.rooms_count > 1; ++i)
            {
                const room& rm = lv.rooms[r.range(1, lv.rooms_count - 1)];
                int x, y; random_free_cell_in_room(rm, x, y);
                pickups[pickups_count++] = { int8_t(x), int8_t(y), uint8_t(i == 0 ? coffee : r.range(0, 2)), true };
            }
            push(message().add("Etap ").add(stage + 1).add(": ").add(sd.name));
        }

        void spawn(int def_id, int x, int y)
        {
            const enemy_def& ed = data::enemies[def_id];
            actor& a = enemies[enemies_count++];
            a = actor();
            a.x = int8_t(x); a.y = int8_t(y); a.def_id = int8_t(def_id);
            a.hp = a.max_hp = int16_t(imax(1, ed.max_health * enemy_hp_pct() / 100)); a.alive = true;
        }

        int enemy_at(int x, int y) const
        {
            for(int i = 0; i < enemies_count; ++i)
                if(enemies[i].alive && enemies[i].x == x && enemies[i].y == y) return i;
            return -1;
        }

        int hero_stat(stat s) const
        {
            switch(s) { case stat::str: return cdef().strength; case stat::agi: return cdef().agility; default: return cdef().intelligence; }
        }

        // obrażenia = rzut broni + stat/2 + premie - obrona/2, min 1
        void hero_attack(int ei)
        {
            actor& e = enemies[ei];
            const enemy_def& ed = data::enemies[e.def_id];
            int dmg = r.range(weapon().min_damage, weapon().max_damage) + hero_stat(weapon().scales_with) / 2 + dmg_bonus - ed.defense / 2;
            if(dmg < 1) dmg = 1;
            e.hp = int16_t(e.hp - dmg);
            e.awake = true;
            turn_events |= 1u << ei;
            if(e.hp <= 0)
            {
                e.alive = false; ++kills; score += ed.score * score_pct() / 100;
                push(message().add(ed.name).add(" - usunięto!"));
                if(ei == boss) { score += (500 + 100 * (stage + 1)) * score_pct() / 100; st = status::won;
                    push(message().add("Odbiór techniczny zaliczony!")); }
            }
            else
                push(message().add(weapon().name).add(": -").add(dmg).add(" (").add(ed.name).add(")"));
        }

        // Akcje gracza. Zwracają true, jeśli zużyły turę.
        bool player_move(int dx, int dy)
        {
            if(st != status::playing) return false;
            int nx = hero.x + dx, ny = hero.y + dy;
            int ei = enemy_at(nx, ny);
            if(ei >= 0) hero_attack(ei);
            else if(lv.passable(nx, ny)) { hero.x = int8_t(nx); hero.y = int8_t(ny); collect(); }
            else return false;
            end_turn();
            return true;
        }

        int nearest_target() const
        {
            int best = -1, bd = 99;
            for(int i = 0; i < enemies_count; ++i)
            {
                const actor& e = enemies[i];
                int d = cheb(hero.x, hero.y, e.x, e.y);
                if(e.alive && d <= weapon().range && d < bd) { bd = d; best = i; }
            }
            return best;
        }

        bool player_attack_nearest()
        {
            if(st != status::playing) return false;
            int t = nearest_target();
            if(t < 0) { push(message().add("Brak celu w zasięgu ").add(weapon().range)); return false; }
            hero_attack(t);
            end_turn();
            return true;
        }

        bool player_wait()
        {
            if(st != status::playing) return false;
            if(hero.hp < hero.max_hp && turns % 4 == 0) ++hero.hp;   // krótki odpoczynek
            end_turn();
            return true;
        }

        void collect()
        {
            for(int i = 0; i < pickups_count; ++i)
            {
                pickup& p = pickups[i];
                if(! p.active || p.x != hero.x || p.y != hero.y) continue;
                p.active = false;
                if(p.type == coffee) { int h = imin(8, hero.max_hp - hero.hp); hero.hp = int16_t(hero.hp + h); push(message().add("Kawa z termosu: +").add(h).add(" HP")); }
                else if(p.type == helmet) { ++def_bonus; push(message().add("Nowy kask: obrona +1")); }
                else { ++dmg_bonus; push(message().add("Projekt wykonawczy: obrażenia +1")); }
            }
        }

        void enemy_act(int i)
        {
            actor& e = enemies[i];
            const enemy_def& ed = data::enemies[e.def_id];
            int d = cheb(e.x, e.y, hero.x, hero.y);
            if(! e.awake) { if(d <= ed.sight) e.awake = true; else return; }
            // Termin: porusza się co drugą turę, poniżej połowy HP przyspiesza
            if(i == boss && e.hp * 2 > e.max_hp && (turns & 1)) return;
            int manh = iabs(e.x - hero.x) + iabs(e.y - hero.y);
            if(manh == 1)
            {
                int dmg = r.range(ed.min_damage, ed.max_damage) + enemy_dmg_bonus() - (cdef().defense + def_bonus) / 2;
                if(dmg < 1) dmg = 1;
                hero.hp = int16_t(hero.hp - dmg);
                hero_hit = true;
                push(message().add(ed.name).add(": -").add(dmg).add(" HP"));
                if(hero.hp <= 0) { hero.hp = 0; hero.alive = false; st = status::dead;
                    push(message().add("Budowa wstrzymana...")); }
                return;
            }
            int dx = isign(hero.x - e.x), dy = isign(hero.y - e.y);
            bool xfirst = iabs(hero.x - e.x) >= iabs(hero.y - e.y);
            int tries[2][2] = { { xfirst ? dx : 0, xfirst ? 0 : dy }, { xfirst ? 0 : dx, xfirst ? dy : 0 } };
            for(auto& t : tries)
            {
                if(t[0] == 0 && t[1] == 0) continue;
                int nx = e.x + t[0], ny = e.y + t[1];
                if(lv.at(nx, ny) == tile::floor && ! occupied(nx, ny)) { e.x = int8_t(nx); e.y = int8_t(ny); return; }
            }
        }

        void end_turn()
        {
            ++turns;
            if(st == status::playing)
                for(int i = 0; i < enemies_count && st == status::playing; ++i)
                    if(enemies[i].alive) enemy_act(i);
            if(st == status::playing && hero.x == stairs_x && hero.y == stairs_y)
            {
                st = status::stage_clear;
                score += 100 * score_pct() / 100;
                push(message().add("Etap zakończony: ").add(data::stages[stage].name));
            }
        }

        // Skrót pokazowy (L+R+SELECT): zalicza etap albo pokonuje bossa.
        void debug_skip()
        {
            if(st != status::playing) return;
            if(boss >= 0 && enemies[boss].alive) { enemies[boss].hp = 1; hero_attack(boss); }
            else if(stairs_x >= 0) { hero.x = int8_t(stairs_x); hero.y = int8_t(stairs_y); end_turn(); }
        }

        // Przejście do kolejnego etapu (po ekranie harmonogramu). Przerwa na kawę: +5 HP.
        void next_stage()
        {
            hero.hp = int16_t(imin(hero.max_hp, hero.hp + 5));
            start_stage(stage + 1);
        }

        // NG+ ("Kolejna budowa"): po wygranej ten sam zawód i poziom, premie i wynik zostają,
        // wrogowie mocniejsi o kolejny stopień. Zwraca false, jeśli budowa nie została ukończona.
        bool new_game_plus()
        {
            if(st != status::won) return false;
            ++tier;
            for(auto& e : enemies) e = actor();
            hero.hp = hero.max_hp;
            start_stage(0);
            push(message().add("Kolejna budowa! Poziom ").add(tier + 1));
            return true;
        }
    };
}
