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
    constexpr int max_pickups = 10;
    constexpr int log_lines = 3;
    constexpr int log_len = 48;
    constexpr int fov_radius = 7;       // promień widzenia bohatera w polach
    constexpr int max_hits = 8;         // zdarzenia trafień w jednej turze (dla efektów)

    enum class tile : uint8_t { wall, floor, stairs };
    enum class status : uint8_t { playing, stage_clear, dead, won };
    enum sight : uint8_t { unknown = 0, remembered = 1, in_view = 2 };   // mgła wojny
    enum pickup_type : uint8_t { coffee, helmet, plan, tool };

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
        int8_t stun = 0;        // tury ogłuszenia (Odprawa)
    };

    struct temp_wall { int8_t x, y, turns; };   // Ścianka Murarza

    struct pickup { int8_t x, y; uint8_t type; bool active; uint8_t arg = 0; };   // arg: indeks narzędzia
    struct hit { int8_t x, y; int16_t amount; bool on_hero; };   // do liczb obrażeń nad polem

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

    // Premie z meta-progresji (sklep "Szkolenia"), stałe przez całą budowę.
    struct run_mods
    {
        int hp = 0, def = 0, dmg = 0, coffee = 0, pickups = 0;
        int tools = data::start_tools_mask;   // narzędzia, które mogą wypaść z wrogów
    };

    static_assert(sizeof(data::enemies) / sizeof(data::enemies[0]) <= 16);

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
        uint8_t kills_by_type[16] = {};   // pokonane problemy wg rodzaju (zakładka Usterki)
        int stage_damage = 0;        // obrażenia otrzymane na bieżącym etapie (odznaka Bez usterek)
        int stage_kills = 0;         // problemy usunięte na bieżącym etapie (odznaka Seryjny)
        int stage_start_turn = 0;    // tura wejścia na etap (odznaka Przed terminem)
        uint8_t tools_found = 0;     // narzędzia podniesione w tej budowie (odznaka Kolekcjoner)
        run_mods bonus;
        int xp_pct = 0;              // doświadczenie x100 (mnożnik trudności bez gubienia ułamków)
        int xp_banked = 0;           // ile doświadczenia już przeniesiono do profilu
        int run_xp = 0;              // surowe doświadczenie z tej budowy (poziomy postaci)
        int hero_level = 1;               // poziom postaci w trakcie budowy
        int boss = -1;               // indeks w enemies[]
        int stairs_x = -1, stairs_y = -1;
        status st = status::playing;
        message log[log_lines];
        uint8_t fov[map_h][map_w];   // sight: nieznane / zapamiętane / widoczne teraz
        uint32_t turn_events = 0;    // bitmaska: które indeksy przeciwników zostały trafione w tej turze (efekt)
        bool hero_hit = false;
        hit hits[max_hits];
        int hits_count = 0;          // warstwa GBA czyta i zeruje po każdej turze
        int last_target = -1;        // ostatnio trafiony wróg (pasek HP celu)

        void add_hit(int x, int y, int amount, bool on_hero)
        {
            if(hits_count < max_hits) hits[hits_count++] = { int8_t(x), int8_t(y), int16_t(amount), on_hero };
        }

        const class_def& cdef() const { return data::classes[cls]; }
        int ability_cd = 0;          // tury do ponownego użycia mocy (R)
        temp_wall walls[4];
        int walls_count = 0;
        int weapon_override = -1;    // podniesione narzędzie zamiast broni zawodu
        const weapon_def& weapon() const { return data::weapons[weapon_override >= 0 ? weapon_override : cdef().weapon]; }
        const difficulty_def& ddef() const { return data::difficulties[diff]; }

        bool visible(int x, int y) const { return lv.in(x, y) && fov[y][x] == in_view; }
        bool explored(int x, int y) const { return lv.in(x, y) && fov[y][x] != unknown; }

        // Pole widzenia: recursive shadowcasting (8 oktantów), ściany zasłaniają, same są widoczne.
        void update_fov()
        {
            for(auto& row : fov) for(auto& c : row) if(c == in_view) c = remembered;
            fov[hero.y][hero.x] = in_view;
            static constexpr int8_t m[4][8] = { { 1, 0, 0, -1, -1, 0, 0, 1 }, { 0, 1, -1, 0, 0, -1, 1, 0 },
                                                { 0, 1, 1, 0, 0, -1, -1, 0 }, { 1, 0, 0, 1, -1, 0, 0, -1 } };
            for(int o = 0; o < 8; ++o) cast_light(1, 1.0f, 0.0f, m[0][o], m[1][o], m[2][o], m[3][o]);
        }

        void cast_light(int row, float start, float end, int xx, int xy, int yx, int yy)
        {
            if(start < end) return;
            float new_start = 0;
            for(int j = row; j <= fov_radius; ++j)
            {
                bool blocked = false;
                for(int dx = -j, dy = -j; dx <= 0; ++dx)
                {
                    int x = hero.x + dx * xx + dy * xy, y = hero.y + dx * yx + dy * yy;
                    float l_slope = (dx - 0.5f) / (dy + 0.5f), r_slope = (dx + 0.5f) / (dy - 0.5f);
                    if(start < r_slope) continue;
                    if(end > l_slope) break;
                    if(dx * dx + dy * dy <= fov_radius * fov_radius && lv.in(x, y)) fov[y][x] = in_view;
                    bool opaque = ! lv.passable(x, y);
                    if(blocked)
                    {
                        if(opaque) { new_start = r_slope; continue; }
                        blocked = false; start = new_start;
                    }
                    else if(opaque && j < fov_radius)
                    {
                        blocked = true;
                        cast_light(j + 1, start, l_slope, xx, xy, yx, yy);
                        new_start = r_slope;
                    }
                }
                if(blocked) break;
            }
        }

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
        int xp() const { return xp_pct / 100; }
        void gain_xp(int base)
        {
            xp_pct += base * score_pct();
            run_xp += base;
            while(hero_level < data::max_hero_level && run_xp >= data::level_thresholds[hero_level - 1]) level_up();
        }

        // Ile brakuje do kolejnego poziomu; -1 = maksymalny.
        int xp_to_next() const { return hero_level < data::max_hero_level ? data::level_thresholds[hero_level - 1] - run_xp : -1; }

        // Awans: +HP; wybrane poziomy dają +1 obrażenia / +1 obrona (data::dmg/def_levels_mask).
        void level_up()
        {
            ++hero_level;
            hero.max_hp = int16_t(hero.max_hp + data::hp_per_level);
            hero.hp = int16_t(hero.hp + data::hp_per_level);
            if(data::dmg_levels_mask & (1 << hero_level)) ++dmg_bonus;
            if(data::def_levels_mask & (1 << hero_level)) ++def_bonus;
            push(message().add("Awans! Poziom ").add(hero_level));
        }

        void push(const message& m)
        {
            for(int i = 0; i < log_lines - 1; ++i) log[i] = log[i + 1];
            log[log_lines - 1] = m;
        }

        void new_run(int class_index, uint32_t seed, int difficulty = data::default_difficulty,
                     const run_mods& mods = run_mods())
        {
            *this = game();
            cls = class_index;
            diff = difficulty;
            bonus = mods;
            def_bonus = mods.def;
            dmg_bonus = mods.dmg;
            r.seed(seed);
            hero.max_hp = hero.hp = int16_t(cdef().max_health + mods.hp);
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
            walls_count = 0;
            stage_damage = 0; stage_kills = 0; stage_start_turn = turns;
            for(auto& row : fov) for(auto& c : row) c = unknown;
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
            for(int i = 0; i < 3 + bonus.pickups && i < max_pickups && lv.rooms_count > 1; ++i)
            {
                const room& rm = lv.rooms[r.range(1, lv.rooms_count - 1)];
                int x, y; random_free_cell_in_room(rm, x, y);
                pickups[pickups_count++] = { int8_t(x), int8_t(y), uint8_t(i == 0 ? coffee : r.range(0, 2)), true };
            }
            push(message().add("Etap ").add(stage + 1).add(": ").add(sd.name));
            update_fov();
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
            last_target = ei;
            add_hit(e.x, e.y, dmg, false);
            turn_events |= 1u << ei;
            if(e.hp <= 0)
            {
                e.alive = false; ++kills; ++stage_kills;
                if(kills_by_type[e.def_id] < 255) ++kills_by_type[e.def_id]; score += ed.score * score_pct() / 100; gain_xp(data::xp_per_kill);
                maybe_drop(e.x, e.y);
                push(message().add(ed.name).add(" - usunięto!"));
                if(ei == boss) { score += (500 + 100 * (stage + 1)) * score_pct() / 100; gain_xp(data::xp_boss); st = status::won;
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

        bool pickup_at(int x, int y) const
        {
            for(int i = 0; i < pickups_count; ++i) if(pickups[i].active && pickups[i].x == x && pickups[i].y == y) return true;
            return false;
        }

        // Moc zawodu (R). Zwraca true, jeśli zużyła turę; bez celu nic się nie dzieje.
        bool player_ability()
        {
            if(st != status::playing || ability_cd > 0) return false;
            const class_def& c = cdef();
            bool ok = false;
            switch(c.ability)
            {
                case ability_effect::stun:
                    for(int i = 0; i < enemies_count; ++i)
                        if(enemies[i].alive && visible(enemies[i].x, enemies[i].y))
                        { enemies[i].stun = 2; enemies[i].awake = true; ok = true; }
                    if(ok) push(message().add(c.ability_name).add(": problemy wstrzymane"));
                    break;
                case ability_effect::wall:
                {
                    const int d[4][2] = { { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 } };
                    for(auto& v : d)
                    {
                        int x = hero.x + v[0], y = hero.y + v[1];
                        if(walls_count < 4 && lv.at(x, y) == tile::floor && ! occupied(x, y) && ! pickup_at(x, y))
                        { lv.t[y][x] = tile::wall; walls[walls_count++] = { int8_t(x), int8_t(y), 6 }; ok = true; }
                    }
                    if(ok) push(message().add(c.ability_name).add(" postawiona!"));
                    break;
                }
                case ability_effect::volley:
                    for(int i = 0; i < enemies_count && st == status::playing; ++i)
                    {
                        const actor& e = enemies[i];
                        if(e.alive && visible(e.x, e.y) && cheb(hero.x, hero.y, e.x, e.y) <= weapon().range) { hero_attack(i); ok = true; }
                    }
                    break;
                case ability_effect::chain:
                {
                    uint32_t done = 0;
                    int t = nearest_target();
                    for(int k = 0; k < 3 && t >= 0 && st == status::playing; ++k)
                    {
                        int px = enemies[t].x, py = enemies[t].y;
                        hero_attack(t); done |= 1u << t; ok = true;
                        t = -1;
                        for(int i = 0; i < enemies_count; ++i)
                        {
                            const actor& e = enemies[i];
                            if(e.alive && ! (done & (1u << i)) && visible(e.x, e.y) && cheb(px, py, e.x, e.y) <= 2) { t = i; break; }
                        }
                    }
                    break;
                }
                case ability_effect::heal:
                    if(hero.hp < hero.max_hp)
                    {
                        int h = imin(8, hero.max_hp - hero.hp);
                        hero.hp = int16_t(hero.hp + h); ok = true;
                        push(message().add(c.ability_name).add(" zakręcony: +").add(h).add(" HP"));
                    }
                    break;
                case ability_effect::spin:
                    for(int i = 0; i < enemies_count && st == status::playing; ++i)
                        if(enemies[i].alive && cheb(hero.x, hero.y, enemies[i].x, enemies[i].y) == 1) { hero_attack(i); ok = true; }
                    break;
            }
            if(! ok) { push(message().add(c.ability_name).add(": nie teraz")); return false; }
            end_turn();
            ability_cd = c.ability_cooldown;
            return true;
        }

        bool player_wait()
        {
            if(st != status::playing) return false;
            if(hero.hp < hero.max_hp && turns % 4 == 0) ++hero.hp;   // krótki odpoczynek
            end_turn();
            return true;
        }

        // Drop z pokonanego wroga: szansa data::drop_chance_pct, typ losowany wagami.
        void maybe_drop(int x, int y)
        {
            if(r.range(1, 100) > data::drop_chance_pct || pickups_count >= max_pickups) return;
            for(int i = 0; i < pickups_count; ++i) if(pickups[i].active && pickups[i].x == x && pickups[i].y == y) return;
            int total = 0; for(int w : data::drop_weights) total += w;
            int roll = r.range(1, total), type = 0;
            while(roll > data::drop_weights[type]) roll -= data::drop_weights[type++];
            uint8_t arg = 0;
            if(type == tool)
            {
                int n = 0; for(int i = 0; i < data::tools_count; ++i) n += (bonus.tools >> i) & 1;
                if(n == 0) type = coffee;
                else
                {
                    int k = r.range(0, n - 1);
                    for(int i = 0; i < data::tools_count; ++i) if(((bonus.tools >> i) & 1) && k-- == 0) { arg = uint8_t(i); break; }
                }
            }
            pickups[pickups_count++] = { int8_t(x), int8_t(y), uint8_t(type), true, arg };
        }

        void collect()
        {
            for(int i = 0; i < pickups_count; ++i)
            {
                pickup& p = pickups[i];
                if(! p.active || p.x != hero.x || p.y != hero.y) continue;
                p.active = false;
                if(p.type == coffee) { int h = imin(8 + bonus.coffee, hero.max_hp - hero.hp); hero.hp = int16_t(hero.hp + h); push(message().add("Kawa z termosu: +").add(h).add(" HP")); }
                else if(p.type == helmet) { ++def_bonus; push(message().add("Nowy kask: obrona +1")); }
                else if(p.type == plan) { ++dmg_bonus; push(message().add("Projekt wykonawczy: obrażenia +1")); }
                else
                {
                    weapon_override = data::tools[p.arg].weapon;
                    tools_found = uint8_t(tools_found | (1u << p.arg));
                    push(message().add("Narzędzie: ").add(weapon().name).add(" ").add(weapon().min_damage).add("-").add(weapon().max_damage));
                }
            }
        }

        void enemy_act(int i)
        {
            actor& e = enemies[i];
            const enemy_def& ed = data::enemies[e.def_id];
            int d = cheb(e.x, e.y, hero.x, hero.y);
            if(! e.awake) { if(d <= ed.sight) e.awake = true; else return; }
            if(e.stun > 0) { --e.stun; return; }
            // Termin: porusza się co drugą turę, poniżej połowy HP przyspiesza
            if(i == boss && e.hp * 2 > e.max_hp && (turns & 1)) return;
            int manh = iabs(e.x - hero.x) + iabs(e.y - hero.y);
            if(manh == 1)
            {
                int dmg = r.range(ed.min_damage, ed.max_damage) + enemy_dmg_bonus() - (cdef().defense + def_bonus) / 2;
                if(dmg < 1) dmg = 1;
                hero.hp = int16_t(hero.hp - dmg);
                stage_damage += dmg;
                hero_hit = true;
                add_hit(hero.x, hero.y, dmg, true);
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
            if(ability_cd > 0 && --ability_cd == 0) push(message().add("Moc gotowa: ").add(cdef().ability_name));
            for(int i = 0; i < walls_count; )
                if(--walls[i].turns <= 0) { lv.t[walls[i].y][walls[i].x] = tile::floor; walls[i] = walls[--walls_count]; }
                else ++i;
            update_fov();
            if(st == status::playing)
                for(int i = 0; i < enemies_count && st == status::playing; ++i)
                    if(enemies[i].alive) enemy_act(i);
            if(st == status::playing && hero.x == stairs_x && hero.y == stairs_y)
            {
                st = status::stage_clear;
                score += 100 * score_pct() / 100;
                gain_xp(data::xp_per_stage);
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
