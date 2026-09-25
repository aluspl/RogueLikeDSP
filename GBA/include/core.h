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
    constexpr int max_walls = 5;        // tymczasowe mury (Ścianka III ma 5 pól)

    enum class tile : uint8_t { wall, floor, stairs };
    enum class status : uint8_t { playing, stage_clear, dead, won };
    enum sight : uint8_t { unknown = 0, remembered = 1, in_view = 2 };   // mgła wojny
    enum pickup_type : uint8_t { coffee, helmet, plan, tool, gear_box };

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

    struct pickup { int8_t x, y; uint8_t type; bool active; uint8_t arg = 0; uint8_t trait = 0; };   // arg: narzędzie / slot*3+jakość; trait: cecha sprzętu
    enum hit_kind : uint8_t { hit_normal, hit_crit, hit_dodge };
    struct hit { int8_t x, y; int16_t amount; bool on_hero; uint8_t kind = hit_normal; };   // do liczb obrażeń nad polem

    // Dziennik budowy (log zdarzeń) - krótkie linie UTF-8
    enum log_kind : uint8_t { info, bad, good, loot };   // kolor komunikatu w dzienniku

    struct message
    {
        char s[log_len];
        int n = 0;
        uint8_t kind = info;
        uint8_t repeat = 1;          // ile razy z rzędu ten sam komunikat (x2, x3...)
        message() { s[0] = 0; }
        message& as(log_kind k) { kind = k; return *this; }
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
        int luck = 0;                // Kurs BHP II
        int craft = 0;               // Warsztaty: + do statystyki, z którą skaluje się broń zawodu
        // uprawnienia z odznak i pamiątka (perk_effect)
        int cooldown = 0;            // odnowienie mocy krótsze o tyle tur
        int sight = 0;               // widzenie +
        int thermos = 0;             // dodatkowe miejsca w termosie
        int tool_pct = 0;            // % szansy, że drop zamieni się w narzędzie
        int xp_pct = 0;              // % więcej doświadczenia
        int cash = 0;                // budżet na start budowy (zł)
        int crit = 0;                // kryt +%
        int tools = data::start_tools_mask;   // narzędzia, które mogą wypaść z wrogów
    };

    inline void add_perk(run_mods& m, const perk& p)
    {
        switch(p.effect)
        {
            case perk_effect::hp:       m.hp += p.value; break;
            case perk_effect::def:      m.def += p.value; break;
            case perk_effect::dmg:      m.dmg += p.value; break;
            case perk_effect::luck:     m.luck += p.value; break;
            case perk_effect::cooldown: m.cooldown += p.value; break;
            case perk_effect::sight:    m.sight += p.value; break;
            case perk_effect::thermos:  m.thermos += p.value; break;
            case perk_effect::tool_pct: m.tool_pct += p.value; break;
            case perk_effect::xp_pct:   m.xp_pct += p.value; break;
            case perk_effect::cash:     m.cash += p.value; break;
            case perk_effect::crit:     m.crit += p.value; break;
            case perk_effect::coffee:   m.coffee += p.value; break;
            default: break;
        }
    }

    // Opis premii dla gracza, np. "+2 max HP", "Moc -1 t. odnowienia" (telefon, wybór zawodu).
    inline message& perk_label(message& m, const perk& p)
    {
        int v = p.value;
        switch(p.effect)
        {
            case perk_effect::hp:       return m.add("+").add(v).add(" max HP");
            case perk_effect::def:      return m.add("+").add(v).add(" obrony");
            case perk_effect::dmg:      return m.add("+").add(v).add(" obrażeń");
            case perk_effect::luck:     return m.add("+").add(v).add(" szczęścia");
            case perk_effect::cooldown: return m.add("Moc -").add(v).add(" t. odnowienia");
            case perk_effect::sight:    return m.add("Widzenie +").add(v);
            case perk_effect::thermos:  return m.add("Termos +").add(v).add(v == 1 ? " miejsce" : " miejsca");
            case perk_effect::tool_pct: return m.add("+").add(v).add("% szans na narzędzie");
            case perk_effect::xp_pct:   return m.add("+").add(v).add("% doświadczenia");
            case perk_effect::cash:     return m.add("+").add(v).add(" zł na start");
            case perk_effect::crit:     return m.add("Kryt +").add(v).add("%");
            case perk_effect::coffee:   return m.add("Kawa +").add(v).add(" HP");
            default:                    return m;
        }
    }

    inline int class_base_stat(int cls, stat s)
    {
        const class_def& c = data::classes[cls];
        return s == stat::str ? c.strength : (s == stat::agi ? c.agility : c.intelligence);
    }

    // Premia z meta-progresji do statystyki zawodu (Warsztaty działają na statystykę broni zawodu).
    inline int mods_stat_bonus(const run_mods& m, int cls, stat s)
    {
        return data::weapons[data::classes[cls].weapon].scales_with == s ? m.craft : 0;
    }

    inline trait_effect stat_trait(stat s)
    {
        return s == stat::str ? trait_effect::str : (s == stat::agi ? trait_effect::agi : trait_effect::intel);
    }

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
        // liczniki zleceń (przenoszone do profilu przez bank_counters; *_banked = już przeniesione)
        int kills_banked = 0;
        uint16_t powers_used = 0, powers_banked = 0;   // użycia mocy
        uint8_t brand_found = 0, brand_banked = 0;     // założone markowe przedmioty
        uint8_t clean_bosses = 0, clean_banked = 0;    // bossowie aktu bez obrażeń w walce z nimi
        int boss_wake_damage = -1;   // stage_damage w chwili dołączenia bossa do walki (-1 = jeszcze nie)
        int8_t stage_event = -1;     // wydarzenie na placu na bieżącym etapie (data::site_events, -1 = brak)

        bool event_active(event_effect e) const { return stage_event >= 0 && data::site_events[stage_event].effect == e; }

        // Wydarzenie na placu: SMS na starcie etapu, efekt od razu (znajdźki, budżet, termos) albo w trakcie etapu.
        void apply_event(int e)
        {
            stage_event = int8_t(e);
            const site_event_def& ev = data::site_events[e];
            switch(ev.effect)
            {
                case event_effect::fewer_pickups: pickups_count = imax(imin(1, pickups_count), pickups_count - ev.value); break;
                case event_effect::cash:          cash += ev.value; break;
                case event_effect::thermos:       thermos = thermos_cap(); break;
                default: break;   // inspekcja: premia na koniec etapu; ulewa: poślizg przy ciosach
            }
            push(message().add("SMS: ").add(ev.name).as(ev.good ? good : bad));
        }
        int cash = 0;                // budżet budowy (zł) - za usunięte problemy i premie aktów, wydawany w Hurtowni
        int act_kills = 0;           // problemy usunięte w bieżącym akcie (premia)
        int act_bonus = 0;           // ostatnia premia za akt (do pokazania w Hurtowni)
        bool act_cleared = false;    // pokonano bossa aktu - przed kolejnym etapem jest Hurtownia
        int slam_timer = 0;          // uderzenie bossa: tury do ciosu (0 = brak zapowiedzi)
        int8_t slam_x = -1, slam_y = -1;
        int slam_counter = 0;
        int8_t hero_status[5] = {};  // tury aktywnych stanów bohatera (indeks = status_effect)

        int status_turns(status_effect s) const { return hero_status[int(s)]; }

        // Nakłada stan; komunikat mówi skutek i czas, np. "Zatrucie: -1 HP/turę, 3 t.".
        void apply_status(status_effect s, int t)
        {
            if(s == status_effect::none) return;
            const status_def& sd = data::statuses[int(s)];
            if(s == status_effect::paper)
            {
                ability_cd = imin(ability_cooldown() + data::paper_delay, ability_cd + data::paper_delay);
                push(message().add(sd.name).add(": moc +").add(data::paper_delay).add(" t.").as(bad));
                return;
            }
            if(s == status_effect::poison && trait_bonus(trait_effect::poison_res) > 0)
            {
                push(message().add("Odporność: bez zatrucia").as(good));
                return;
            }
            hero_status[int(s)] = int8_t(imax(hero_status[int(s)], t));
            push(message().add(sd.name).add(": ").add(sd.effect).add(", ").add(hero_status[int(s)]).add(" t.").as(bad));
        }

        // Porażenie: akcja bohatera przepada, mija tura.
        bool shocked_turn()
        {
            if(hero_status[int(status_effect::shock)] <= 0) return false;
            --hero_status[int(status_effect::shock)];
            push(message().add("Porażenie: tura stracona").as(bad));
            end_turn();
            return true;
        }

        // Pole w zasięgu zapowiedzianego uderzenia bossa (czerwone pola na mapie).
        bool slam_cell(int x, int y) const { return slam_timer > 0 && slam_cell_at(x, y); }
        bool slam_cell_at(int x, int y) const { return slam_x >= 0 && cheb(x, y, slam_x, slam_y) <= data::slam_radius; }
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

        void add_hit(int x, int y, int amount, bool on_hero, int kind = hit_normal)
        {
            if(hits_count < max_hits) hits[hits_count++] = { int8_t(x), int8_t(y), int16_t(amount), on_hero, uint8_t(kind) };
        }

        const class_def& cdef() const { return data::classes[cls]; }
        int ability_cd = 0;          // tury do ponownego użycia mocy (R)
        temp_wall walls[max_walls];
        int walls_count = 0;
        int8_t equipped[4] = { -1, -1, -1, -1 };   // sprzęt: jakość w slocie (kask, rękawice, kamizelka), -1 = brak
        int8_t equipped_trait[4] = {};              // cecha przedmiotu w slocie (data::gear_traits)
        int thermos = 0;                            // kawy w termosie (pije się z menu pod START)
        int8_t offer_slot = -1, offer_rarity = 0, offer_trait = 0;   // paczka czeka na decyzję: zakładam / zostawiam
        int weapon_override = -1;    // podniesione narzędzie zamiast broni zawodu

        int gear_bonus(gear_stat s) const
        {
            int b = 0;
            for(int i = 0; i < data::gear_slots_count; ++i)
                if(equipped[i] >= 0 && data::gear[i * 3 + equipped[i]].stat == s) b += data::gear[i * 3 + equipped[i]].value;
            return b;
        }
        // Szczęście: kryt (x2), mały unik przed ciosem wroga, częstsze i lepsze dropy.
        int luck() const { return cdef().luck + bonus.luck + trait_bonus(trait_effect::luck); }
        int crit_pct() const { return data::crit_base_pct + data::crit_per_luck_pct * luck() + trait_bonus(trait_effect::crit) + bonus.crit; }
        int sight_radius() const { return fov_radius + trait_bonus(trait_effect::sight) + bonus.sight; }
        int thermos_cap() const { return data::thermos_capacity + bonus.thermos; }

        // Suma cech założonego sprzętu danego rodzaju.
        int trait_bonus(trait_effect e) const
        {
            int b = 0;
            for(int i = 0; i < data::gear_slots_count; ++i)
                if(equipped[i] >= 0 && data::gear_traits[equipped_trait[i]].effect == e) b += data::gear_traits[equipped_trait[i]].value;
            return b;
        }
        int dodge_pct() const { return imin(data::dodge_max_pct, data::dodge_per_luck_pct * luck()); }
        const weapon_def& weapon() const { return data::weapons[weapon_override >= 0 ? weapon_override : cdef().weapon]; }
        const difficulty_def& ddef() const { return data::difficulties[diff]; }

        // Wiadomość fabularna na wejściu etapu (przy NG+ pierwszy etap ma własną).
        const story_msg& stage_story() const { return tier > 0 && stage == 0 ? data::story_ngplus : data::story_stages[stage]; }

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
            const int radius = sight_radius();
            for(int j = row; j <= radius; ++j)
            {
                bool blocked = false;
                for(int dx = -j, dy = -j; dx <= 0; ++dx)
                {
                    int x = hero.x + dx * xx + dy * xy, y = hero.y + dx * yx + dy * yy;
                    float l_slope = (dx - 0.5f) / (dy + 0.5f), r_slope = (dx + 0.5f) / (dy - 0.5f);
                    if(start < r_slope) continue;
                    if(end > l_slope) break;
                    if(dx * dx + dy * dy <= radius * radius && lv.in(x, y)) fov[y][x] = in_view;
                    bool opaque = ! lv.passable(x, y);
                    if(blocked)
                    {
                        if(opaque) { new_start = r_slope; continue; }
                        blocked = false; start = new_start;
                    }
                    else if(opaque && j < radius)
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
            xp_pct += base * score_pct() * (100 + bonus.xp_pct) / 100;
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
            push(message().add("Awans! Poziom ").add(hero_level).as(good));
        }

        int log_serial = 0;          // rośnie przy każdym komunikacie (warstwa GBA pokazuje świeże)

        void push(const message& m)
        {
            ++log_serial;
            message& last = log[log_lines - 1];
            if(last.n == m.n && last.kind == m.kind && std::memcmp(last.s, m.s, size_t(m.n)) == 0)
            {
                if(last.repeat < 99) ++last.repeat;   // ten sam komunikat: licznik zamiast nowej linii
                return;
            }
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
            cash = mods.cash;
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
            stage_damage = 0; stage_kills = 0; stage_start_turn = turns; boss_wake_damage = -1;
            act_cleared = false; slam_timer = 0; slam_x = slam_y = -1; slam_counter = 0;
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
            stage_event = -1;   // wydarzenie na placu: nie na pierwszym etapie i nie u bossa
            if(s > 0 && sd.boss < 0 && r.range(1, 100) <= data::site_event_chance_pct) apply_event(r.range(0, data::site_events_count - 1));
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

        // Statystyka efektywna: zawód + Warsztaty + cechy sprzętu (SIŁ/ZRĘ/INT +1).
        int hero_stat(stat s) const { return class_base_stat(cls, s) + stat_bonus(s); }
        int stat_bonus(stat s) const { return mods_stat_bonus(bonus, cls, s) + trait_bonus(stat_trait(s)); }

        // obrażenia = rzut broni + stat/2 + premie - obrona/2, min 1
        void hero_attack(int ei)
        {
            actor& e = enemies[ei];
            const enemy_def& ed = data::enemies[e.def_id];
            if(ei == boss && boss_wake_damage < 0) boss_wake_damage = stage_damage;   // walka z bossem trwa
            int dmg = r.range(weapon().min_damage, weapon().max_damage) + hero_stat(weapon().scales_with) / 2 + dmg_bonus
                    + gear_bonus(gear_stat::dmg) - ed.defense / 2;
            if(dmg < 1) dmg = 1;
            bool crit = r.range(1, 100) <= crit_pct();
            if(crit) dmg *= data::crit_multiplier;
            e.hp = int16_t(e.hp - dmg);
            e.awake = true;
            last_target = ei;
            add_hit(e.x, e.y, dmg, false, crit ? hit_crit : hit_normal);
            turn_events |= 1u << ei;
            if(e.hp <= 0)
            {
                e.alive = false; ++kills; ++stage_kills; ++act_kills;
                cash += ed.score / data::cash_per_score;
                if(kills_by_type[e.def_id] < 255) ++kills_by_type[e.def_id]; score += ed.score * score_pct() / 100; gain_xp(data::xp_per_kill);
                maybe_drop(e.x, e.y);
                push(message().add(ed.name).add(" - usunięto!").as(good));
                if(ei == boss)
                {
                    if(stage_damage == boss_wake_damage && clean_bosses < 255) ++clean_bosses;   // zlecenie Czysta robota
                    score += (500 + 100 * (stage + 1)) * score_pct() / 100;
                    gain_xp(data::xp_boss);
                    slam_timer = 0;
                    if(stage == data::stages_count - 1)
                    {
                        st = status::won;
                        push(message().add("Odbiór techniczny zaliczony!").as(good));
                    }
                    else   // boss aktu: premia za akt, potem Hurtownia
                    {
                        const act_def& ad = data::acts[data::stages[stage].act];
                        int stages_in_act = 0;
                        for(int i = 0; i < data::stages_count; ++i) stages_in_act += data::stages[i].act == data::stages[stage].act;
                        act_bonus = ad.bonus_per_stage * stages_in_act + ad.bonus_per_kill * act_kills;
                        cash += act_bonus;
                        act_kills = 0;
                        act_cleared = true;
                        st = status::stage_clear;
                        push(message().add("Akt zaliczony! Premia ").add(act_bonus).add(" zł").as(good));
                    }
                }
            }
            else
                push(message().add(crit ? "KRYT! " : "").add(weapon().name).add(": -").add(dmg).add(" (").add(ed.name).add(")").as(crit ? loot : info));
        }

        // Akcje gracza. Zwracają true, jeśli zużyły turę.
        bool player_move(int dx, int dy)
        {
            if(st != status::playing) return false;
            if(shocked_turn()) return true;
            int nx = hero.x + dx, ny = hero.y + dy;
            int ei = enemy_at(nx, ny);
            if(ei >= 0) hero_attack(ei);
            else if(lv.passable(nx, ny))
            {
                hero.x = int8_t(nx); hero.y = int8_t(ny); collect();
                int8_t& slip = hero_status[int(status_effect::slip)];
                if(slip > 0)   // poślizg: jeszcze jedno pole w tę samą stronę
                {
                    --slip;
                    int sx = hero.x + dx, sy = hero.y + dy;
                    if(lv.passable(sx, sy) && ! occupied(sx, sy)) { hero.x = int8_t(sx); hero.y = int8_t(sy); collect(); }
                }
            }
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

        // Cele w zasięgu broni (widoczni, żywi), posortowane od najbliższego. Zwraca liczbę.
        int targets_in_range(int8_t* out, int max) const
        {
            int n = 0;
            for(int d = 1; d <= weapon().range; ++d)
                for(int i = 0; i < enemies_count && n < max; ++i)
                {
                    const actor& e = enemies[i];
                    if(e.alive && visible(e.x, e.y) && cheb(hero.x, hero.y, e.x, e.y) == d) out[n++] = int8_t(i);
                }
            return n;
        }

        // Atak wybranego celu (celowanie przytrzymaniem A). Cel musi być w zasięgu i widoczny.
        bool player_attack(int ei)
        {
            if(st != status::playing || ei < 0 || ei >= enemies_count) return false;
            if(shocked_turn()) return true;
            const actor& e = enemies[ei];
            if(! e.alive || ! visible(e.x, e.y) || cheb(hero.x, hero.y, e.x, e.y) > weapon().range) return false;
            hero_attack(ei);
            end_turn();
            return true;
        }

        bool player_attack_nearest()
        {
            if(st != status::playing) return false;
            if(shocked_turn()) return true;
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

        // Ranga mocy rośnie z poziomem postaci: II od 3., III od 5. poziomu. Każda ranga skraca odnowienie o 2 tury.
        int ability_rank() const { return 1 + (hero_level >= 3) + (hero_level >= 5); }
        int ability_cooldown() const
        {
            return imax(3, imax(4, cdef().ability_cooldown - 2 * (ability_rank() - 1)) - trait_bonus(trait_effect::cooldown) - bonus.cooldown);
        }

        int nearest_visible_enemy() const
        {
            int best = -1, bd = 99;
            for(int i = 0; i < enemies_count; ++i)
            {
                const actor& e = enemies[i];
                int d = cheb(hero.x, hero.y, e.x, e.y);
                if(e.alive && visible(e.x, e.y) && d < bd) { bd = d; best = i; }
            }
            return best;
        }

        bool place_wall(int x, int y, int turns_left)
        {
            if(walls_count >= max_walls || lv.at(x, y) != tile::floor || occupied(x, y) || pickup_at(x, y)) return false;
            lv.t[y][x] = tile::wall;
            walls[walls_count++] = { int8_t(x), int8_t(y), int8_t(turns_left) };
            return true;
        }

        // Moc zawodu (R). Zwraca true, jeśli zużyła turę; bez celu nic się nie dzieje.
        bool player_ability()
        {
            if(st != status::playing || ability_cd > 0) return false;
            const class_def& c = cdef();
            const int rank = ability_rank();
            bool ok = false;
            switch(c.ability)
            {
                case ability_effect::stun:   // Odprawa: ogłusza widocznych na 2/3/4 tury
                    for(int i = 0; i < enemies_count; ++i)
                        if(enemies[i].alive && visible(enemies[i].x, enemies[i].y))
                        { enemies[i].stun = int8_t(1 + rank); enemies[i].awake = true; ok = true; }
                    if(ok) push(message().add(c.ability_name).add(": problemy wstrzymane"));
                    break;
                case ability_effect::wall:   // Ścianka: mur w poprzek drogi najbliższego wroga (nigdy wokół bohatera)
                {
                    int t = nearest_visible_enemy();
                    if(t < 0) break;
                    int dx = isign(enemies[t].x - hero.x), dy = isign(enemies[t].y - hero.y);
                    if(iabs(enemies[t].x - hero.x) >= iabs(enemies[t].y - hero.y)) dy = 0; else dx = 0;
                    int cx = hero.x + dx, cy = hero.y + dy;          // środek muru: pole przed bohaterem
                    int px = dy != 0 ? 1 : 0, py = dx != 0 ? 1 : 0;   // kierunek muru: prostopadle
                    int half = rank >= 3 ? 2 : 1, dur = 4 + 2 * rank;
                    for(int k = -half; k <= half; ++k) ok |= place_wall(cx + px * k, cy + py * k, dur);
                    if(ok) push(message().add(c.ability_name).add(" postawiona!"));
                    break;
                }
                case ability_effect::volley:   // Seria: wszyscy widoczni w zasięgu (+1 obrażeń od II, +1 zasięgu na III)
                {
                    int range = weapon().range + (rank >= 3 ? 1 : 0);
                    if(rank >= 2) ++dmg_bonus;
                    for(int i = 0; i < enemies_count && st == status::playing; ++i)
                    {
                        const actor& e = enemies[i];
                        if(e.alive && visible(e.x, e.y) && cheb(hero.x, hero.y, e.x, e.y) <= range) { hero_attack(i); ok = true; }
                    }
                    if(rank >= 2) --dmg_bonus;
                    break;
                }
                case ability_effect::chain:   // Łańcuch: 3/4/5 celów, skoki do 2 pól
                {
                    uint32_t done = 0;
                    int t = nearest_target();
                    for(int k = 0; k < 2 + rank && t >= 0 && st == status::playing; ++k)
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
                case ability_effect::flush:   // Zawór: strumień odpycha sąsiadów o 1/2 pola (tracą turę) i leczy 6/8/10 HP
                {
                    int push_by = rank >= 2 ? 2 : 1;
                    for(int i = 0; i < enemies_count; ++i)
                    {
                        actor& e = enemies[i];
                        if(! e.alive || cheb(hero.x, hero.y, e.x, e.y) != 1) continue;
                        int dx = isign(e.x - hero.x), dy = isign(e.y - hero.y);
                        for(int k = 0; k < push_by; ++k)
                        {
                            int nx = e.x + dx, ny = e.y + dy;
                            if(lv.at(nx, ny) != tile::floor || occupied(nx, ny)) break;
                            e.x = int8_t(nx); e.y = int8_t(ny);
                        }
                        e.awake = true;
                        e.stun = int8_t(imax(e.stun, 1));   // zalany traci turę, inaczej od razu by wrócił
                        ok = true;
                    }
                    int h = imin(4 + 2 * rank, hero.max_hp - hero.hp);
                    if(h > 0) { hero.hp = int16_t(hero.hp + h); ok = true; }
                    if(ok) push(message().add(c.ability_name).add(": strumień! +").add(imax(0, h)).add(" HP"));
                    break;
                }
                case ability_effect::spin:   // Wirówka: wszyscy obok (zasięg 2 na III), od II ogłusza na 1 turę
                {
                    int reach = rank >= 3 ? 2 : 1;
                    for(int i = 0; i < enemies_count && st == status::playing; ++i)
                    {
                        int d = cheb(hero.x, hero.y, enemies[i].x, enemies[i].y);
                        if(enemies[i].alive && d >= 1 && d <= reach)
                        {
                            hero_attack(i); ok = true;
                            if(rank >= 2 && enemies[i].alive) enemies[i].stun = int8_t(imax(enemies[i].stun, 1));
                        }
                    }
                    break;
                }
            }
            if(! ok) { push(message().add(c.ability_name).add(": nie teraz")); return false; }
            if(powers_used < 65535) ++powers_used;
            end_turn();
            ability_cd = ability_cooldown();
            return true;
        }

        // Hurtownia między aktami: zakup za budżet budowy.
        bool hurtownia_buy(int i)
        {
            const shop_item_def& it = data::hurtownia[i];
            if(cash < it.price) return false;
            switch(it.effect)
            {
                case shop_effect::heal: hero.hp = hero.max_hp; break;
                case shop_effect::maxhp: hero.max_hp = int16_t(hero.max_hp + 3); hero.hp = int16_t(hero.hp + 3); break;
                case shop_effect::ability: ability_cd = 0; break;
                case shop_effect::gear:
                {
                    int slot = r.range(0, data::gear_slots_count - 1), rarity = r.range(1, 2);
                    if(rarity <= equipped[slot]) rarity = imin(2, equipped[slot] + 1);
                    if(rarity > equipped[slot]) equip(slot, rarity, r.range(0, data::gear_traits_count - 1)); else gain_xp(3);
                    break;
                }
                case shop_effect::tool:
                {
                    int n = 0; for(int t = 0; t < data::tools_count; ++t) n += (bonus.tools >> t) & 1;
                    int k = r.range(0, imax(0, n - 1));
                    for(int t = 0; t < data::tools_count; ++t)
                        if(((bonus.tools >> t) & 1) && k-- == 0) { weapon_override = data::tools[t].weapon; tools_found = uint8_t(tools_found | (1u << t)); break; }
                    break;
                }
            }
            cash -= it.price;
            push(message().add("Hurtownia: ").add(it.name).as(loot));
            return true;
        }

        int coffee_heal() const { return data::coffee_heal + bonus.coffee; }

        void drink_coffee()
        {
            int h = imin(coffee_heal(), hero.max_hp - hero.hp);
            hero.hp = int16_t(hero.hp + h);
            push(message().add("Kawa z termosu: +").add(h).add(" HP").as(good));
        }

        // Picie z termosu (menu pod START): leczy, zużywa turę.
        bool player_drink()
        {
            if(st != status::playing) return false;
            if(thermos <= 0) { push(message().add("Termos pusty")); return false; }
            if(hero.hp >= hero.max_hp) { push(message().add("HP pełne - kawa poczeka")); return false; }
            if(shocked_turn()) return true;
            --thermos;
            drink_coffee();
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

        // Drop z pokonanego wroga: szansa data::drop_chance_pct, typ losowany wagami.
        void maybe_drop(int x, int y)
        {
            if(r.range(1, 100) > data::drop_chance_pct + data::drop_per_luck_pct * luck() || pickups_count >= max_pickups) return;
            for(int i = 0; i < pickups_count; ++i) if(pickups[i].active && pickups[i].x == x && pickups[i].y == y) return;
            int total = 0; for(int w : data::drop_weights) total += w;
            int roll = r.range(1, total), type = 0;
            while(roll > data::drop_weights[type]) roll -= data::drop_weights[type++];
            if(type != tool && bonus.tool_pct > 0 && r.range(1, 100) <= bonus.tool_pct) type = tool;   // uprawnienie Kolekcjoner
            uint8_t arg = 0, trait = 0;
            if(type == gear_box)   // slot losowy, jakość lepsza na późnych etapach
            {
                int q = r.range(1, 100) + stage * data::gear_stage_bonus + data::rarity_per_luck * luck();
                int rarity = q >= data::gear_brand_from ? 2 : (q >= data::gear_solid_from ? 1 : 0);
                arg = uint8_t(r.range(0, data::gear_slots_count - 1) * 3 + rarity);
                trait = uint8_t(r.range(0, data::gear_traits_count - 1));
            }
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
            pickups[pickups_count++] = { int8_t(x), int8_t(y), uint8_t(type), true, arg, trait };
        }

        // Zakłada przedmiot w slocie (zastępuje obecny; kamizelka od razu zmienia max HP).
        void equip(int slot, int rarity, int trait)
        {
            const gear_def& nw = data::gear[slot * 3 + rarity];
            if(nw.stat == gear_stat::hp)
            {
                int diff = nw.value - (equipped[slot] >= 0 ? data::gear[slot * 3 + equipped[slot]].value : 0);
                hero.max_hp = int16_t(hero.max_hp + diff);
                hero.hp = int16_t(imax(1, hero.hp + diff));
            }
            equipped[slot] = int8_t(rarity);
            equipped_trait[slot] = int8_t(trait);
            if(rarity == 2 && brand_found < 255) ++brand_found;   // zlecenie Markowy styl
            update_fov();   // cecha Widzenie zmienia pole widzenia
            push(message().add("Sprzęt: ").add(nw.name).add(" +").add(nw.value).as(loot));
        }

        bool has_offer() const { return offer_slot >= 0; }
        bool offer_is_better() const { return has_offer() && offer_rarity > equipped[offer_slot]; }

        // Paczka sprzętu przy zajętym slocie: gracz porównuje (A zakładam, B zostawiam). Nie zużywa tury.
        void accept_offer()
        {
            if(! has_offer()) return;
            int slot = offer_slot; offer_slot = -1;
            equip(slot, offer_rarity, offer_trait);
        }

        void decline_offer()
        {
            if(! has_offer()) return;
            int xp = data::gear_decline_xp + offer_rarity;
            offer_slot = -1;
            gain_xp(xp);
            push(message().add("Zostawiasz stary sprzęt: +").add(xp).add(" dośw.").as(loot));
        }

        void collect()
        {
            for(int i = 0; i < pickups_count; ++i)
            {
                pickup& p = pickups[i];
                if(! p.active || p.x != hero.x || p.y != hero.y) continue;
                if(p.type == gear_box && has_offer()) continue;   // najpierw decyzja o poprzedniej paczce
                p.active = false;
                if(p.type == coffee)
                {
                    if(thermos < thermos_cap())   // kawa do termosu; pełny termos - pije od razu
                    {
                        ++thermos;
                        push(message().add("Kawa do termosu (").add(thermos).add("/").add(thermos_cap()).add(")").as(good));
                    }
                    else drink_coffee();
                }
                else if(p.type == helmet) { ++def_bonus; push(message().add("Nowy kask: obrona +1").as(loot)); }
                else if(p.type == plan) { ++dmg_bonus; push(message().add("Projekt wykonawczy: obrażenia +1").as(loot)); }
                else if(p.type == gear_box)
                {
                    int slot = p.arg / 3;
                    if(equipped[slot] < 0) equip(slot, p.arg % 3, p.trait);   // pusty slot: zakłada od razu
                    else
                    {
                        offer_slot = int8_t(slot); offer_rarity = int8_t(p.arg % 3); offer_trait = int8_t(p.trait);
                        push(message().add("Paczka: ").add(data::gear[p.arg].name).as(loot));
                    }
                }
                else
                {
                    weapon_override = data::tools[p.arg].weapon;
                    tools_found = uint8_t(tools_found | (1u << p.arg));
                    push(message().add("Narzędzie: ").add(weapon().name).add(" ").add(weapon().min_damage).add("-").add(weapon().max_damage).as(loot));
                }
            }
        }

        void enemy_act(int i)
        {
            actor& e = enemies[i];
            const enemy_def& ed = data::enemies[e.def_id];
            int d = cheb(e.x, e.y, hero.x, hero.y);
            if(! e.awake) { if(d <= ed.sight) e.awake = true; else return; }
            if(i == boss && boss_wake_damage < 0) boss_wake_damage = stage_damage;
            if(e.stun > 0) { --e.stun; return; }
            if(ed.slam && i == boss)   // boss: co kilka tur zapowiada uderzenie w obszar wokół bohatera
            {
                if(slam_timer > 0) return;   // ładuje cios, stoi w miejscu
                if(++slam_counter >= data::slam_every && d <= 4)
                {
                    slam_counter = 0;
                    slam_timer = 2;
                    slam_x = hero.x; slam_y = hero.y;
                    push(message().add("Cios bossa za 2 tury!").as(bad));
                    return;
                }
            }
            // Termin: porusza się co drugą turę, poniżej połowy HP przyspiesza
            if(i == boss && e.hp * 2 > e.max_hp && (turns & 1)) return;
            int manh = iabs(e.x - hero.x) + iabs(e.y - hero.y);
            if(manh == 1)
            {
                if(dodge_pct() > 0 && r.range(1, 100) <= dodge_pct())   // szczęście: unik
                {
                    add_hit(hero.x, hero.y, 0, true, hit_dodge);
                    push(message().add("Unik! ").add(ed.name).add(" chybia").as(good));
                    return;
                }
                int dmg = r.range(ed.min_damage, ed.max_damage) + enemy_dmg_bonus() - (cdef().defense + def_bonus + gear_bonus(gear_stat::def)) / 2;
                if(dmg < 1) dmg = 1;
                hero.hp = int16_t(hero.hp - dmg);
                stage_damage += dmg;
                hero_hit = true;
                add_hit(hero.x, hero.y, dmg, true);
                push(message().add(ed.name).add(": -").add(dmg).add(" HP").as(bad));
                if(ed.on_hit != status_effect::none && hero.hp > 0 && r.range(1, 100) <= ed.status_chance)
                    apply_status(ed.on_hit, ed.status_turns);
                if(event_active(event_effect::rain) && hero.hp > 0 && r.range(1, 100) <= data::site_events[stage_event].value)
                    apply_status(status_effect::slip, 2);   // Ulewa w nocy: błoto na placu
                if(hero.hp <= 0) { hero.hp = 0; hero.alive = false; st = status::dead;
                    push(message().add("Budowa wstrzymana...").as(bad)); }
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
            int8_t& poison = hero_status[int(status_effect::poison)];
            if(poison > 0 && st == status::playing)   // zatrucie: -1 HP na turę, ale nie zabija
            {
                --poison;
                if(hero.hp > 1) { hero.hp = int16_t(hero.hp - 1); stage_damage += 1; add_hit(hero.x, hero.y, 1, true); }
            }
            if(ability_cd > 0 && --ability_cd == 0) push(message().add("Moc gotowa: ").add(cdef().ability_name).as(good));
            for(int i = 0; i < walls_count; )
                if(--walls[i].turns <= 0) { lv.t[walls[i].y][walls[i].x] = tile::floor; walls[i] = walls[--walls_count]; }
                else ++i;
            update_fov();
            if(slam_timer > 0 && --slam_timer == 0 && boss >= 0 && enemies[boss].alive)   // cios bossa spada
            {
                const enemy_def& bd = data::enemies[enemies[boss].def_id];
                if(slam_cell_at(hero.x, hero.y))
                {
                    int dmg = r.range(bd.min_damage, bd.max_damage) + enemy_dmg_bonus() + data::slam_damage_bonus
                            - (cdef().defense + def_bonus + gear_bonus(gear_stat::def)) / 2;
                    if(dmg < 1) dmg = 1;
                    hero.hp = int16_t(hero.hp - dmg);
                    stage_damage += dmg;
                    hero_hit = true;
                    add_hit(hero.x, hero.y, dmg, true);
                    push(message().add("Uderzenie: -").add(dmg).add(" HP").as(bad));
                    if(hero.hp <= 0) { hero.hp = 0; hero.alive = false; st = status::dead; push(message().add("Budowa wstrzymana...").as(bad)); }
                }
                else push(message().add("Unik! Cios poszedł obok").as(good));
                slam_x = slam_y = -1;
            }
            if(st == status::playing)
                for(int i = 0; i < enemies_count && st == status::playing; ++i)
                    if(enemies[i].alive) enemy_act(i);
            if(st == status::playing && hero.x == stairs_x && hero.y == stairs_y)
            {
                st = status::stage_clear;
                score += 100 * score_pct() / 100;
                gain_xp(data::xp_per_stage);
                push(message().add("Etap zakończony: ").add(data::stages[stage].name).as(good));
                if(event_active(event_effect::inspection) && stage_damage == 0)   // Inspekcja nadzoru: etap bez obrażeń
                {
                    gain_xp(data::site_events[stage_event].value);
                    push(message().add("Inspekcja: +").add(data::site_events[stage_event].value).add(" dośw.").as(good));
                }
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
