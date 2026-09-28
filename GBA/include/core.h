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
    constexpr int max_enemies = 16;     // v0.21.49: 12 z etapu (z bossem i wezwanymi) + miejsce na podziały
    constexpr int max_pickups = 10;
    constexpr int log_lines = 3;
    constexpr int log_len = 48;
    constexpr int fov_radius = 7;       // promień widzenia bohatera w polach
    constexpr int max_hits = 8;         // zdarzenia trafień w jednej turze (dla efektów)
    constexpr int max_walls = 5;        // tymczasowe mury (Ścianka III ma 5 pól)
    constexpr int max_enemy_types = 48;  // rodzaje problemów (katalog: 16 + 32 bity w profilu)
    constexpr int max_stages = 12;      // v0.21.49: 10 etapów + 2 Aktu 0
    constexpr int max_gear_slots = 6;   // kask, rękawice, kamizelka + sloty z nagród (buty, pas)
    static_assert(data::gear_slots_count <= max_gear_slots);

    enum class tile : uint8_t { wall, floor, stairs };
    enum class status : uint8_t { playing, stage_clear, dead, won };
    enum sight : uint8_t { unknown = 0, remembered = 1, in_view = 2 };   // mgła wojny
    enum pickup_type : uint8_t { coffee, helmet, plan, tool, gear_box, document };   // document: Akt 0 (arg = data::documents)

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
        uint8_t flags = 0;      // actor_flag: dziecko z podziału, już wrócił, czeka na powrót
        int8_t grow = 0;        // stopnie wzrostu (zachowanie "grows")
        int8_t timer = 0;       // odnowienie ucieczki / łatania / odepchnięcia; u czekającego na powrót - tury do powrotu
    };
    enum actor_flag : uint8_t { actor_child = 1, actor_returned = 2, actor_reviving = 4, actor_phase = 8 };   // phase: boss w drugiej fazie

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
        int helpers = data::start_helpers_mask;   // brygada: fachowcy do wezwania (Szkolenia)
        int investor = 0;            // tryb inwestora: włączone modyfikatory (bitmaska data::investor)
        // v0.21.49: procentowe premie (Szkolenia BHP i Kurs fachowy, Respekt), nagrody za odbiór
        int dmg_pct = 0;             // +% zadawanych obrażeń
        int taken_pct = 0;           // -% otrzymanych obrażeń
        int gear_pct = 0;            // + do rzutu na jakość sprzętu z paczek
        int dodge = 0;               // unik +% (łącznie maks. data::dodge_max_pct)
        int coffee_pct = 0;          // kawa leczy +%
        int brigade_pct = 0;         // brygada taniej o %
        int shop_pct = 0;            // Hurtownia (zł) taniej o %
        int mats_pct = 0;            // materiały z problemów częściej o %
        int second_chance = 0;       // Druga szansa: raz na budowę 1 HP zamiast końca
        int gear_slots = data::gear_base_mask;   // sloty sprzętu w dropach (nagrody: buty, pas)
        int act0 = 0;                // v0.21.49: Akt 0 (Papierologia) z nagrody za odbiór - budowa zaczyna się od niego
    };

    // Tryb inwestora: stawka i premia doświadczenia za zestaw modyfikatorów.
    inline int investor_stake(int mask)
    {
        int s = 0;
        for(int i = 0; i < data::investor_count; ++i) if(mask & (1 << i)) s += data::investor[i].stake;
        return s;
    }
    inline int investor_xp(int mask)
    {
        int s = 0;
        for(int i = 0; i < data::investor_count; ++i) if(mask & (1 << i)) s += data::investor[i].xp_pct;
        return s;
    }

    // Premia z rangi Respektu (wartość łączna rangi).
    inline void add_respect(run_mods& m, respect_effect e, int v)
    {
        switch(e)
        {
            case respect_effect::dmg_pct:       m.dmg_pct += v; break;
            case respect_effect::taken_pct:     m.taken_pct += v; break;
            case respect_effect::gear_pct:      m.gear_pct += v; break;
            case respect_effect::crit:          m.crit += v; break;
            case respect_effect::dodge:         m.dodge += v; break;
            case respect_effect::coffee_pct:    m.coffee_pct += v; break;
            case respect_effect::thermos:       m.thermos += v; break;
            case respect_effect::cooldown:      m.cooldown += v; break;
            case respect_effect::cash:          m.cash += v; break;
            case respect_effect::xp_pct:        m.xp_pct += v; break;
            case respect_effect::brigade_pct:   m.brigade_pct += v; break;
            case respect_effect::sight:         m.sight += v; break;
            case respect_effect::shop_pct:      m.shop_pct += v; break;
            case respect_effect::mats_pct:      m.mats_pct += v; break;
            case respect_effect::second_chance: m.second_chance += v; break;
            default: break;
        }
    }

    // Skutek rangi Respektu dla gracza, np. "+8% obrażeń", "Moc -1 t." (telefon profilu).
    inline message& respect_label(message& m, respect_effect e, int v)
    {
        switch(e)
        {
            case respect_effect::dmg_pct:       return m.add("+").add(v).add("% obrażeń");
            case respect_effect::taken_pct:     return m.add("-").add(v).add("% otrzymanych obrażeń");
            case respect_effect::gear_pct:      return m.add("+").add(v).add(" do jakości sprzętu");
            case respect_effect::crit:          return m.add("Kryt +").add(v).add("%");
            case respect_effect::dodge:         return m.add("Unik +").add(v).add("%");
            case respect_effect::coffee_pct:    return m.add("Kawa leczy +").add(v).add("%");
            case respect_effect::thermos:       return m.add("Termos +").add(v).add(v == 1 ? " miejsce" : " miejsca");
            case respect_effect::cooldown:      return m.add("Moc -").add(v).add(" t. odnowienia");
            case respect_effect::cash:          return m.add("+").add(v).add(" zł na start");
            case respect_effect::xp_pct:        return m.add("+").add(v).add("% doświadczenia");
            case respect_effect::brigade_pct:   return m.add("Brygada -").add(v).add("% ceny");
            case respect_effect::sight:         return m.add("Widzenie +").add(v);
            case respect_effect::shop_pct:      return m.add("Hurtownia -").add(v).add("% ceny");
            case respect_effect::mats_pct:      return m.add("Materiały +").add(v).add("% częściej");
            case respect_effect::second_chance: return m.add("Raz na budowę: 1 HP zamiast końca");
            default:                            return m;
        }
    }

    // Część procentowa z przeniesieniem reszty (bez losowania, dokładna średnio): (v * pct + carry) / 100.
    inline int pct_part(int v, int pct, int& carry)
    {
        if(pct <= 0 || v <= 0) return 0;
        int t = v * pct + carry;
        carry = t % 100;
        return t / 100;
    }

    // Zaokrąglenie ilorazu a / b (b > 0) do najbliższej, połówki od zera (tak samo w C#).
    inline int div_round(int a, int b) { return a >= 0 ? (2 * a + b) / (2 * b) : -((-2 * a + b) / (2 * b)); }

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

    // Opis statystyki w prostych słowach z prawdziwym wzorem (wybór zawodu, telefon: Start, Jak grać).
    // v = wartość statystyki; weapon_stat = broń skaluje się z tą statystyką (SIŁ/ZRĘ/INT).
    enum class stat_kind : uint8_t { hp, str, agi, intel, def, luck };
    constexpr int stat_kinds = 6;
    inline const char* stat_kind_name(stat_kind k)
    {
        static const char* n[stat_kinds] = { "HP", "SIŁ", "ZRĘ", "INT", "OBR", "SZCZ" };
        return n[int(k)];
    }
    inline int luck_crit_pct(int luck) { return data::crit_base_pct + data::crit_per_luck_pct * luck; }
    inline int luck_dodge_pct(int luck) { return imin(data::dodge_max_pct, data::dodge_per_luck_pct * imax(0, luck)); }
    inline message& stat_effect(message& m, stat_kind k, int v, bool weapon_stat)
    {
        switch(k)
        {
            case stat_kind::hp:   return m.add("zdrowie (0 = koniec)");
            case stat_kind::str:
            case stat_kind::agi:
            case stat_kind::intel:
                if(weapon_stat) return m.add("+").add(v / 2).add(" obrażeń broni");
                return m.add("nie dla tej broni");
            case stat_kind::def:  return m.add("-").add(v / 2).add(" obrażeń od problemów");
            case stat_kind::luck: return m.add("kryt ").add(luck_crit_pct(v)).add("%, unik ").add(luck_dodge_pct(v)).add("%");
            default:              return m;
        }
    }
    // Ogólny wzór statystyki (bez wartości) - strona "Jak działają" / Jak grać. Szczęście ma 3 części (part 0-2).
    inline message& stat_rule(message& m, stat_kind k, int part = 0)
    {
        switch(k)
        {
            case stat_kind::hp:   return m.add("HP: zdrowie, leczy kawa");
            case stat_kind::str:  return m.add("SIŁ: +1 obr. co 2 pkt (broń SIŁ)");
            case stat_kind::agi:  return m.add("ZRĘ: +1 obr. co 2 pkt (broń ZRĘ)");
            case stat_kind::intel:return m.add("INT: +1 obr. co 2 pkt (broń INT)");
            case stat_kind::def:  return m.add("OBR: -1 obrażeń co 2 pkt");
            case stat_kind::luck:
                if(part == 1) return m.add("unik +").add(data::dodge_per_luck_pct).add("%/pkt (maks. ").add(data::dodge_max_pct).add("%)");
                if(part == 2) return m.add("łupy: +").add(data::drop_per_luck_pct).add("% szansy/pkt");
                return m.add("SZCZ: kryt ").add(data::crit_base_pct).add("% +").add(data::crit_per_luck_pct).add("%/pkt");
            default:              return m;
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

    // ------------------------------------------------------------------ rozpiska obrażeń broni (#26, jak w D&D / BG3)
    // Zakres ciosu liczony TYMI SAMYMI wzorami co hero_attack, bez losowania (liczby na ekranie = walka):
    //   cios = rzut broni (min..max) + statystyka broni / 2 (w dół) + premie płaskie - obrona problemu / 2 (w dół), min. 1;
    //   potem +% (Kurs fachowy, Respekt) przez pct_part: część procentowa w dół, a reszta przechodzi na następny cios -
    //   pojedynczy cios dostaje ją w dół albo w górę (średnio dokładnie), więc zakres ma oba skraje (min w dół, max w górę);
    //   kryt (szansa = baza + SZCZ x %/pkt + cechy sprzętu + premie, r.range(1, 100) <= szansa) mnoży wynik po procencie.
    // Cios problemu (enemy_strike) tak samo: rzut + premia etapu + wzrost / 2 - OBR bohatera / 2, min. 1, potem -%
    // (Szkolenie BHP, Respekt) z resztą, znów min. 1.
    inline int pct_floor(int v, int pct) { return pct <= 0 || v <= 0 ? 0 : v * pct / 100; }
    inline int pct_ceil(int v, int pct) { return pct <= 0 || v <= 0 ? 0 : (v * pct + 99) / 100; }

    // Źródła premii z profilu (meta::mods_part): Szkolenia, Respekt, odznaki (uprawnienia), pamiątka.
    constexpr int mods_sources = 4;
    inline const char* mods_source_name(int s)
    {
        static const char* n[mods_sources] = { "Szkolenia", "Respekt", "odznaki", "pamiątka" };
        return n[s];
    }
    inline const char* stat_name(stat s) { return s == stat::str ? "SIŁ" : (s == stat::agi ? "ZRĘ" : "INT"); }
    // Premia do obrażeń z awansów do poziomu level (data::dmg_levels_mask).
    inline int level_dmg(int level)
    {
        int n = 0;
        for(int l = 2; l <= level; ++l) n += (data::dmg_levels_mask >> l) & 1;
        return n;
    }

    struct dmg_breakdown
    {
        int weapon = 0;                      // indeks w data::weapons
        int wmin = 0, wmax = 0;              // rzut broni
        int range = 1, range_base = 1;       // zasięg (z pogodą) i zasięg broni
        stat scales = stat::str;             // statystyka broni
        int stat_class = 0, stat_craft = 0, stat_trait = 0;   // statystyka = zawód + Warsztaty + cechy sprzętu
        int flat_mods = 0;                   // premie stałe z profilu (Szkolenia, odznaki: run_mods.dmg)
        int flat_level = 0;                  // awanse (data::dmg_levels_mask)
        int flat_found = 0;                  // z budowy: projekty wykonawcze (reszta dmg_bonus)
        int flat_gear = 0, gear_item = -1;   // sprzęt +obrażenia (rękawice), przedmiot w data::gear
        int pct = 0;                         // +% (Kurs fachowy, Respekt)
        bool vs_enemy = false;
        int enemy_def = 0;                   // obrona problemu
        int luck = 0, crit_trait = 0, crit_bonus = 0;   // szczęście, cechy Kryt +%, premie (odznaki, Respekt)
        int power = 0, power_rank = 1;       // moc dodaje do ciosu (Seria, Rynna od II, Taran +ranga)
        bool split = false;                  // źródła premii profilu znane (src_*)
        int src_dmg[mods_sources] = {}, src_pct[mods_sources] = {}, src_crit[mods_sources] = {};
        // wyliczone w finish()
        int stat_value = 0, stat_dmg = 0, flat = 0, def_cut = 0;
        int base_min = 0, base_max = 0;      // przed procentem
        int min = 0, max = 0;                // zakres ciosu
        int avg10 = 0;                       // średni cios (bez kryt) x10
        int crit_base = 0, crit_luck = 0, crit_pct = 0, crit_mult = 1, crit_min = 0, crit_max = 0;

        void finish()
        {
            stat_value = stat_class + stat_craft + stat_trait;
            stat_dmg = stat_value / 2;
            flat = flat_mods + flat_level + flat_found + flat_gear;
            def_cut = enemy_def / 2;
            const int add = stat_dmg + flat - def_cut;
            base_min = imax(1, wmin + add);
            base_max = imax(1, wmax + add);
            min = base_min + pct_floor(base_min, pct);
            max = base_max + pct_ceil(base_max, pct);
            int sum = 0, n = 0;
            for(int r = wmin; r <= wmax; ++r, ++n) sum += imax(1, r + add) * (100 + imax(0, pct));
            avg10 = n ? div_round(sum, 10 * n) : 0;
            crit_base = data::crit_base_pct;
            crit_luck = data::crit_per_luck_pct * luck;
            crit_pct = crit_base + crit_luck + crit_trait + crit_bonus;
            crit_mult = data::crit_multiplier;
            crit_min = min * crit_mult;
            crit_max = max * crit_mult;
        }
        int crit_chance() const { return imax(0, imin(100, crit_pct)); }
        // Źródła premii profilu (parts = meta::mods_part dla każdego źródła); tylko gdy sumy się zgadzają (nie budowa dnia).
        void set_sources(const run_mods (&parts)[mods_sources])
        {
            int d = 0, p = 0, c = 0;
            for(int s = 0; s < mods_sources; ++s)
            {
                src_dmg[s] = parts[s].dmg; src_pct[s] = parts[s].dmg_pct; src_crit[s] = parts[s].crit;
                d += parts[s].dmg; p += parts[s].dmg_pct; c += parts[s].crit;
            }
            split = d == flat_mods && p == pct && c == crit_bonus;
        }
    };

    // Rozpiska dla zawodu przed budową (wybór zawodu): broń zawodu, premie z profilu, bez sprzętu i awansów.
    inline dmg_breakdown class_breakdown(int cls, const run_mods& m, int enemy_def = -1)
    {
        const class_def& c = data::classes[cls];
        const weapon_def& w = data::weapons[c.weapon];
        dmg_breakdown b;
        b.weapon = c.weapon;
        b.wmin = w.min_damage; b.wmax = w.max_damage;
        b.range = b.range_base = w.range;
        b.scales = w.scales_with;
        b.stat_class = class_base_stat(cls, w.scales_with);
        b.stat_craft = mods_stat_bonus(m, cls, w.scales_with);
        b.flat_mods = m.dmg;
        b.pct = m.dmg_pct;
        b.vs_enemy = enemy_def >= 0;
        b.enemy_def = imax(0, enemy_def);
        b.luck = c.luck + m.luck;
        b.crit_bonus = m.crit;
        b.finish();
        return b;
    }

    struct hit_range { int min = 0, max = 0; };
    // Cios problemu w bohatera (enemy_strike bez losowania): bonus = premia etapu/trudności + wzrost / 2.
    inline hit_range enemy_hit_range(int dmin, int dmax, int bonus, int hero_def, int taken_pct)
    {
        int lo = imax(1, dmin + bonus - hero_def / 2), hi = imax(1, dmax + bonus - hero_def / 2);
        return { imax(1, lo - pct_ceil(lo, taken_pct)), imax(1, hi - pct_floor(hi, taken_pct)) };
    }

    // Teksty rozpiski (GBA: strona Obrażenia, Godot: podpowiedzi) - wspólne, krótkie (bufor message 48 bajtów).
    enum class dmg_text : uint8_t { weapon, stat, stat_parts, profile, run, gear, pct, enemy, total, crit, crit_parts, crit_extra, power };
    constexpr int dmg_texts = 13;

    inline message& add_range(message& m, int lo, int hi) { m.add(lo); if(hi != lo) m.add("-").add(hi); return m; }
    // Liczba x10 jako "7" albo "7,5" (ze znakiem, gdy sign).
    inline message& add_tenths(message& m, int v10, bool sign = false)
    {
        if(v10 < 0) { m.add("-"); v10 = -v10; }
        else if(sign) m.add("+");
        m.add(v10 / 10);
        if(v10 % 10) m.add(",").add(v10 % 10);
        return m;
    }
    inline const char* rank_numeral(int r) { return r >= 3 ? "III" : (r == 2 ? "II" : "I"); }

    // Skutek przedmiotu sprzętu, np. "+2 obrażeń", "+1 OBR", "+8 HP", "unik +5%", "termos +1".
    inline message& gear_label(message& m, const gear_def& gd)
    {
        switch(gd.stat)
        {
            case gear_stat::def:     return m.add("+").add(gd.value).add(" OBR");
            case gear_stat::dmg:     return m.add("+").add(gd.value).add(" obrażeń");
            case gear_stat::dodge:   return m.add("unik +").add(gd.value).add("%");
            case gear_stat::thermos: return m.add("termos +").add(gd.value);
            default:                 return m.add("+").add(gd.value).add(" HP");
        }
    }

    // Wiersz rozpiski k; false = ten składnik nic nie daje (warstwa gry może go pominąć), tekst i tak jest.
    inline bool dmg_line(message& m, const dmg_breakdown& b, dmg_text k)
    {
        switch(k)
        {
            case dmg_text::weapon:
                m.add(data::weapons[b.weapon].name).add(" ").add(b.wmin).add("-").add(b.wmax).add(", zasięg ").add(b.range);
                if(b.range < b.range_base) m.add(" (wiatr)");
                return true;
            case dmg_text::stat:
                m.add(stat_name(b.scales)).add(" ").add(b.stat_value).add(": +").add(b.stat_dmg).add(" (+1 co 2 pkt)");
                return true;
            case dmg_text::stat_parts:
                m.add(stat_name(b.scales)).add(" ").add(b.stat_value).add(" = zawód ").add(b.stat_class);
                if(b.stat_craft) m.add(" + Warsztaty ").add(b.stat_craft);
                if(b.stat_trait) m.add(" + sprzęt ").add(b.stat_trait);
                return b.stat_craft || b.stat_trait;
            case dmg_text::profile:
            {
                if(! b.flat_mods) { m.add("Premie stałe: brak"); return false; }
                m.add("Premie stałe +").add(b.flat_mods);
                if(! b.split) return true;
                bool first = true;
                for(int s = 0; s < mods_sources; ++s)
                    if(b.src_dmg[s]) { m.add(first ? ": " : ", ").add(mods_source_name(s)).add(" +").add(b.src_dmg[s]); first = false; }
                return true;
            }
            case dmg_text::run:
                if(! b.flat_level && ! b.flat_found) { m.add("Z budowy: brak"); return false; }
                m.add("Z budowy +").add(b.flat_level + b.flat_found).add(":");
                if(b.flat_level) m.add(" poziom +").add(b.flat_level);
                if(b.flat_found) m.add(b.flat_level ? "," : "").add(" projekty ").add(b.flat_found > 0 ? "+" : "").add(b.flat_found);
                return true;
            case dmg_text::gear:
                if(b.gear_item < 0 || ! b.flat_gear) { m.add("Sprzęt: bez premii"); return false; }
                m.add(data::gear[b.gear_item].name).add(": +").add(b.flat_gear);
                return true;
            case dmg_text::pct:
            {
                if(b.pct <= 0) { m.add("Procent: brak"); return false; }
                m.add("+").add(b.pct).add("%");
                if(! b.split) { m.add(" (Szkolenia, Respekt)"); return true; }
                bool first = true;
                for(int s = 0; s < mods_sources; ++s)
                    if(b.src_pct[s]) { m.add(first ? ": " : ", ").add(mods_source_name(s)).add(" +").add(b.src_pct[s]).add("%"); first = false; }
                return true;
            }
            case dmg_text::enemy:
                if(! b.vs_enemy) { m.add("OBR problemu: -1 co 2 pkt"); return false; }
                m.add("OBR problemu ").add(b.enemy_def).add(": -").add(b.def_cut);
                return b.def_cut > 0;
            case dmg_text::total:
                add_range(m.add("Cios "), b.min, b.max).add(", średnio ");
                add_tenths(m, b.avg10);
                return true;
            case dmg_text::crit:
                m.add("Kryt x").add(b.crit_mult).add(": ");
                add_range(m, b.crit_min, b.crit_max).add(", szansa ").add(b.crit_chance()).add("%");
                return true;
            case dmg_text::crit_parts:
                m.add("Kryt ").add(b.crit_chance()).add("% = ").add(b.crit_base).add("% + SZCZ ").add(b.luck).add(" x ")
                 .add(data::crit_per_luck_pct).add("%");
                return true;
            case dmg_text::crit_extra:
            {
                if(! b.crit_trait && ! b.crit_bonus) { m.add("Kryt: bez premii"); return false; }
                m.add("+");
                bool first = true;
                if(b.crit_trait) { m.add(" cecha ").add(b.crit_trait).add("%"); first = false; }
                if(! b.split) { if(b.crit_bonus) m.add(first ? " " : ", ").add("premie ").add(b.crit_bonus).add("%"); return true; }
                for(int s = 0; s < mods_sources; ++s)
                    if(b.src_crit[s]) { m.add(first ? " " : ", ").add(mods_source_name(s)).add(" ").add(b.src_crit[s]).add("%"); first = false; }
                return true;
            }
            case dmg_text::power:
            {
                if(! b.power) { m.add("Moc: bez premii do ciosu"); return false; }
                m.add("Moc (").add(rank_numeral(b.power_rank)).add("): +").add(b.power).add(" do ciosu");
                return true;
            }
            default: return false;
        }
    }

    // Porównanie przy zmianie broni / sprzętu: "teraz 4-7 -> 5-9 (średnio +1,5)".
    inline message& compare_line(message& m, const dmg_breakdown& now, const dmg_breakdown& next)
    {
        add_range(m.add("teraz "), now.min, now.max).add(" -> ");
        add_range(m, next.min, next.max).add(" (średnio ");
        return add_tenths(m, next.avg10 - now.avg10, true).add(")");
    }
    // "kryt 8-14 (11%) -> 10-18 (16%)"
    inline message& compare_crit(message& m, const dmg_breakdown& now, const dmg_breakdown& next)
    {
        add_range(m.add("kryt "), now.crit_min, now.crit_max).add(" (").add(now.crit_chance()).add("%) -> ");
        return add_range(m, next.crit_min, next.crit_max).add(" (").add(next.crit_chance()).add("%)");
    }
    // Karta problemu: "Zadasz 2-5 (kryt 4-10), on Tobie 1-3".
    inline message& versus_line(message& m, const dmg_breakdown& b, const hit_range& h)
    {
        add_range(m.add("Zadasz "), b.min, b.max).add(" (kryt ");
        add_range(m, b.crit_min, b.crit_max).add("), on Tobie ");
        return add_range(m, h.min, h.max);
    }

    static_assert(data::enemies_count <= max_enemy_types);
    static_assert(data::materials_count <= 4 && data::stages_count <= max_stages);

    // Kładka: zasięg (pola) z danych naprawy "bridge".
    constexpr int bridge_reach()
    {
        for(int i = 0; i < data::repairs_count; ++i) if(data::repairs[i].effect == repair_effect::bridge) return data::repairs[i].value;
        return 0;
    }
    constexpr int max_bridges = 3;

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
        uint8_t kills_by_type[max_enemy_types] = {};   // pokonane problemy wg rodzaju (zakładka Usterki)
        int stage_damage = 0;        // obrażenia otrzymane na bieżącym etapie (odznaka Bez usterek)
        int stage_kills = 0;         // problemy usunięte na bieżącym etapie (odznaka Seryjny)
        int stage_start_turn = 0;    // tura wejścia na etap (odznaka Przed terminem)
        uint8_t tools_found = 0;     // narzędzia podniesione w tej budowie (odznaka Kolekcjoner)
        // liczniki zleceń (przenoszone do profilu przez bank_counters; ile już przeniesiono - w profilu, run_*)
        uint16_t powers_used = 0;    // użycia mocy
        uint8_t brand_found = 0;     // założone markowe przedmioty
        uint8_t clean_bosses = 0;    // bossowie aktu bez obrażeń w walce z nimi
        int boss_wake_damage = -1;   // stage_damage w chwili dołączenia bossa do walki (-1 = jeszcze nie)
        int8_t stage_event = -1;     // wydarzenie na placu na bieżącym etapie (data::site_events, -1 = brak)
        int8_t weather = 0;          // pogoda dnia na bieżącym etapie (data::weather)
        // brygada: fachowiec wezwany na tym etapie (-1 = jeszcze nie), ochrona BHP-owca, pomocnik obok bohatera
        int8_t helper_called = -1;
        int8_t guard_turns = 0;
        int8_t ally_turns = 0, ally_x = -1, ally_y = -1;
        // v0.21.48: wybór ścieżki, materiały, naprawy, codzienna budowa, harmonogram domu
        uint32_t run_seed = 0;       // seed budowy (oferta ścieżek na harmonogramie)
        int8_t stage_path = -1;      // ścieżka bieżącego etapu (data::paths), -1 = bez wyboru (pierwszy etap)
        int8_t next_path = 0;        // wybór na harmonogramie: 0/1 = pozycja w ofercie
        uint8_t mats[4] = {};        // materiały: cement, stal, drewno (data::materials)
        int8_t bridges = 0;          // Kładki na etapie (kałuże w zasięgu bez poślizgu)
        int8_t bridge_x[max_bridges] = {}, bridge_y[max_bridges] = {};
        bool daily = false;          // codzienna budowa (seed dnia)
        uint16_t daily_day = 0;      // numer dnia codziennej budowy
        uint16_t stage_days[max_stages] = {}; // tury na każdym etapie (harmonogram domu po wygranej)
        // v0.21.49: Respekt za etapy, reszty procentów obrażeń, Druga szansa
        int respect = 0;             // Respekt zdobyty w tej budowie (profil: bank_respect)
        int dmg_carry = 0, taken_carry = 0;   // reszty z procentowych premii obrażeń (pct_part)
        bool second_used = false;    // Druga szansa zużyta
        // v0.21.49 (część 2): wybuch po usunięciu problemu (czerwone pola), strzały z dystansu (efekty warstwy GBA)
        int8_t blast_x = -1, blast_y = -1, blast_timer = 0, blast_dmg = 0;
        uint32_t shot_events = 0;    // bitmaska: którzy wrogowie strzelili w tej turze (warstwa GBA czyta i zeruje)
        // v0.21.49 (część 3): Akt 0 - pierwszy etap budowy (0 z Aktem 0, inaczej za nim), zebrane dokumenty (pieczątki)
        int8_t first_stage = 0;
        uint8_t docs = 0;            // bitmaska zebranych dokumentów (data::documents)

        // Numer etapu dla gracza (1..) i liczba etapów tej budowy (bez Aktu 0, gdy nieodblokowany).
        int stage_number() const { return stage - first_stage + 1; }
        int stages_in_run() const { return data::stages_count - first_stage; }
        const char* act_numeral() const { return data::acts[data::stages[stage].act].numeral; }
        // Etap we wzorach (błoto, kałuże, porywy, oferta ścieżek) liczony od Fundamentów: Akt 0 nie zmienia wzorów
        // etapów budowy (Akt 0 ma ujemne numery).
        int pattern_stage() const { return stage - data::prelude_stages; }

        // ------------------------------------------------------------------ mechanika aktu: błoto, porywy, pył
        const act_def& adef() const { return data::acts[data::stages[stage].act]; }
        bool act_is(act_mechanic m) const { return adef().mechanic == m; }
        // Błoto (akt I): stały wzór na podłodze zależny od etapu; wejście kosztuje dodatkową turę. Kładka też na błoto.
        bool mud(int x, int y) const
        {
            if(! act_is(act_mechanic::mud) || lv.at(x, y) != tile::floor || (x * 5 + y * 11 + pattern_stage() * 3) % adef().mech_value != 0) return false;
            for(int i = 0; i < bridges; ++i) if(cheb(x, y, bridge_x[i], bridge_y[i]) <= bridge_reach()) return false;
            return true;
        }
        // Porywy (akt II): co mech_value tur od wejścia na etap poryw spycha bohatera o pole; kierunek zmienia się co poryw.
        int gust_in() const   // tury do kolejnego porywu (0 = brak porywów w tym akcie)
        {
            if(! act_is(act_mechanic::gust)) return 0;
            int v = adef().mech_value, t = turns - stage_start_turn;
            return v - t % v;
        }
        int gust_dir() const   // kierunek kolejnego porywu: 0 prawo, 1 dół, 2 lewo, 3 góra
        {
            int t = turns - stage_start_turn + gust_in();
            return (t / imax(1, adef().mech_value) + pattern_stage()) & 3;
        }
        static constexpr int8_t gust_vec[4][2] = { { 1, 0 }, { 0, 1 }, { -1, 0 }, { 0, -1 } };
        static const char* dir_name(int d) { static const char* n[4] = { "w prawo", "w dół", "w lewo", "w górę" }; return n[d & 3]; }
        void gust_tick()
        {
            int v = adef().mech_value, t = turns - stage_start_turn;
            if(t <= 0) return;
            if(t % v == v - 1) { push(message().add("Poryw wiatru za 1 t. ").add(dir_name(gust_dir())).as(bad)); return; }
            if(t % v != 0) return;
            int d = (t / v + pattern_stage()) & 3, nx = hero.x + gust_vec[d][0], ny = hero.y + gust_vec[d][1];
            if(lv.passable(nx, ny) && ! occupied(nx, ny))
            {
                hero.x = int8_t(nx); hero.y = int8_t(ny);
                collect(); update_fov();
                push(message().add("Poryw! Spycha cię ").add(dir_name(d)).as(bad));
            }
            else push(message().add("Poryw - trzymasz się muru").as(good));
        }
        int dust_sight() const { return act_is(act_mechanic::dust) ? adef().mech_value : 0; }
        // Pieczątki (Akt 0): na etapie ze schodami leżą dokumenty; dopóki nie zbierzesz wszystkich, schody są zamknięte.
        int docs_needed() const { return act_is(act_mechanic::stamps) && data::stages[stage].boss < 0 ? adef().mech_value : 0; }
        int docs_count() const { int n = 0; for(int i = 0; i < data::documents_count; ++i) n += (docs >> i) & 1; return n; }
        bool stairs_locked() const { return docs_count() < docs_needed(); }

        // ------------------------------------------------------------------ zachowania problemów
        bool has_tag(const actor& e, int t) const { return e.def_id >= 0 && (data::enemies[e.def_id].tags & t) != 0; }
        // Wybuch po usunięciu problemu: pola w promieniu wokół miejsca (czerwone), tura na zejście.
        bool blast_cell(int x, int y) const { return blast_timer > 0 && cheb(x, y, blast_x, blast_y) <= data::behavior_blast_radius; }
        // Pole zagrożone: zapowiedziany cios bossa albo wybuch.
        bool danger_cell(int x, int y) const { return slam_cell(x, y) || blast_cell(x, y); }

        bool event_active(event_effect e) const { return stage_event >= 0 && data::site_events[stage_event].effect == e; }
        const weather_def& wdef() const { return data::weather[weather]; }

        // Tryb inwestora: suma wartości włączonych modyfikatorów danego rodzaju / czy któryś włączony.
        int investor_value(investor_effect e) const
        {
            int v = 0;
            for(int i = 0; i < data::investor_count; ++i) if((bonus.investor >> i) & 1 && data::investor[i].effect == e) v += data::investor[i].value;
            return v;
        }
        bool investor_has(investor_effect e) const
        {
            for(int i = 0; i < data::investor_count; ++i) if((bonus.investor >> i) & 1 && data::investor[i].effect == e) return true;
            return false;
        }
        // Przychód budowy (zł) z modyfikatorem budżetu.
        int income(int v) const { return v * (100 + investor_value(investor_effect::cash_pct)) / 100; }
        int slam_every() const { return imax(2, data::slam_every - investor_value(investor_effect::slam)); }
        bool shop_closed() const { return investor_has(investor_effect::no_shop); }
        bool weather_is(weather_effect e) const { return data::weather[weather].effect == e; }

        // Pogoda dnia: losowanie wagami spośród dozwolonych na etapie s (bad_only: tylko niekorzystne, jeśli są).
        bool weather_allowed(int i, int s, bool bad_only) const
        {
            return (data::weather[i].stages & (1u << s)) && (! bad_only || data::weather[i].bad);
        }
        int roll_weather(int s, bool bad_only = false)
        {
            int total = 0;
            for(int i = 0; i < data::weather_count; ++i) if(weather_allowed(i, s, bad_only)) total += data::weather[i].weight;
            if(total == 0) return roll_weather(s, false);
            int roll = r.range(1, total);
            for(int i = 0; i < data::weather_count; ++i)
            {
                if(! weather_allowed(i, s, bad_only)) continue;
                if(roll <= data::weather[i].weight) return i;
                roll -= data::weather[i].weight;
            }
            return 0;
        }

        // Deszcz: kałuże na części pól podłogi (stały wzór zależny od etapu); wejście w kałużę = poślizg.
        bool puddle(int x, int y) const
        {
            if(! weather_is(weather_effect::rain) || lv.at(x, y) != tile::floor || (x * 7 + y * 13 + pattern_stage() * 5) % wdef().value != 0) return false;
            for(int i = 0; i < bridges; ++i) if(cheb(x, y, bridge_x[i], bridge_y[i]) <= bridge_reach()) return false;   // Kładka
            return true;
        }

        // Wydarzenie na placu: SMS na starcie etapu, efekt od razu (znajdźki, budżet, termos) albo w trakcie etapu.
        void apply_event(int e)
        {
            stage_event = int8_t(e);
            const site_event_def& ev = data::site_events[e];
            switch(ev.effect)
            {
                case event_effect::fewer_pickups: pickups_count = imax(imin(1, pickups_count), pickups_count - ev.value); break;
                case event_effect::cash:          cash += income(ev.value); break;
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
        int summon_counter = 0;      // boss z wezwaniami: tury do kolejnego wezwania
        int summons_used = 0;        // ilu wezwano w tej walce (uśpione miejsca za bossem w enemies[])
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
            if(s == status_effect::slip && trait_bonus(trait_effect::slip_res) > 0)
            {
                push(message().add("Odporność: bez poślizgu").as(good));
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

        // Pole w zasięgu zapowiedzianego uderzenia bossa (czerwone pola na mapie): kwadrat albo krzyż (Kontrola BHP).
        bool slam_cell(int x, int y) const { return slam_timer > 0 && slam_cell_at(x, y); }
        bool slam_cell_at(int x, int y) const
        {
            if(slam_x < 0) return false;
            if(boss >= 0 && data::enemies[enemies[boss].def_id].shape == slam_shape::cross)
                return (x == slam_x && iabs(y - slam_y) <= data::slam_cross_reach) || (y == slam_y && iabs(x - slam_x) <= data::slam_cross_reach);
            return cheb(x, y, slam_x, slam_y) <= data::slam_radius;
        }

        // Pełny sprzęt: założony przedmiot w każdym slocie (kask, rękawice, kamizelka).
        bool full_gear() const
        {
            for(int i = 0; i < data::gear_slots_count; ++i) if(((data::gear_base_mask >> i) & 1) && equipped[i] < 0) return false;
            return true;
        }

        // Boss dołącza do walki (raz na etap): licznik Czystej roboty; Inspekcja przy pełnym sprzęcie traci turę.
        void boss_engaged()
        {
            boss_wake_damage = stage_damage;
            const enemy_def& bd = data::enemies[enemies[boss].def_id];
            if(bd.gear_stun > 0 && full_gear())
            {
                enemies[boss].stun = int8_t(imax(enemies[boss].stun, bd.gear_stun));
                push(message().add("Wszystko zgodnie z BHP!").as(good));
            }
        }
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
        int8_t equipped[max_gear_slots] = { -1, -1, -1, -1, -1, -1 };   // sprzęt: jakość w slocie (kask, rękawice, kamizelka, buty, pas), -1 = brak
        int8_t equipped_trait[max_gear_slots] = {};                      // cecha przedmiotu w slocie (data::gear_traits)
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
        int sight_radius() const { return imax(3, fov_radius + trait_bonus(trait_effect::sight) + bonus.sight - dust_sight()); }   // pył (akt III)
        int thermos_cap() const { return data::thermos_capacity + bonus.thermos + gear_bonus(gear_stat::thermos); }

        // Suma cech założonego sprzętu danego rodzaju.
        int trait_bonus(trait_effect e) const
        {
            int b = 0;
            for(int i = 0; i < data::gear_slots_count; ++i)
                if(equipped[i] >= 0 && data::gear_traits[equipped_trait[i]].effect == e) b += data::gear_traits[equipped_trait[i]].value;
            return b;
        }
        // Unik: szczęście + Respekt + buty, łącznie najwyżej data::dodge_max_pct.
        int dodge_pct() const { return imin(data::dodge_max_pct, data::dodge_per_luck_pct * luck() + bonus.dodge + gear_bonus(gear_stat::dodge)); }
        bool has_passive(class_passive p) const { return cdef().passive == p; }
        // Obrona bohatera: zawód + premie + sprzęt + ochrona BHP-owca z brygady.
        int hero_defense() const
        {
            return cdef().defense + def_bonus + gear_bonus(gear_stat::def) + (guard_turns > 0 ? data::brigade[helper_called].value : 0);
        }
        const weapon_def& weapon() const { return data::weapons[weapon_override >= 0 ? weapon_override : cdef().weapon]; }
        // Zasięg broni z pogodą: wiatr skraca zasięg broni dalekiego zasięgu (nie mniej niż 1).
        int weapon_range() const { return range_of(weapon()); }
        int range_of(const weapon_def& w) const
        {
            int rg = w.range;   // Dekarz: wiatr mu nie przeszkadza
            return weather_is(weather_effect::wind) && rg > 1 && ! has_passive(class_passive::windproof) ? imax(1, rg - wdef().value) : rg;
        }
        const difficulty_def& ddef() const { return data::difficulties[diff]; }

        // Wiadomość fabularna na wejściu etapu (przy NG+ pierwszy etap ma własną).
        const story_msg& stage_story() const { return tier > 0 && stage == first_stage ? data::story_ngplus : data::story_stages[stage]; }

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
            return data::stages[stage].hp_pct * ddef().hp_pct / 100 * (100 + tier * data::ng_hp_pct_per_tier) / 100
                   * (100 + investor_value(investor_effect::enemy_hp)) / 100;
        }
        int enemy_dmg_bonus() const
        {
            return data::stages[stage].dmg_bonus + ddef().dmg_bonus + tier * data::ng_dmg_bonus_per_tier + investor_value(investor_effect::enemy_dmg);
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
            run_seed = seed;
            hero.max_hp = hero.hp = int16_t(cdef().max_health + mods.hp);
            hero.alive = true;
            cash = mods.cash;
            first_stage = int8_t(mods.act0 ? 0 : data::prelude_stages);   // bez nagrody Akt 0 budowa zaczyna się od Fundamentów
            start_stage(first_stage);
        }

        bool occupied(int x, int y) const
        {
            if(hero.alive && hero.x == x && hero.y == y) return true;
            if(ally_turns > 0 && ally_x == x && ally_y == y) return true;   // pomocnik z brygady
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

        void start_stage(int s, int path = -1)
        {
            stage = s;
            st = status::playing;
            stage_path = int8_t(path);
            const path_def* pd = path >= 0 ? &data::paths[path] : nullptr;
            lv.generate(r);
            walls_count = 0;
            bridges = 0;
            stage_damage = 0; stage_kills = 0; stage_start_turn = turns; boss_wake_damage = -1;
            act_cleared = false; slam_timer = 0; slam_x = slam_y = -1; slam_counter = 0; summon_counter = 0; summons_used = 0;
            helper_called = -1; guard_turns = 0; ally_turns = 0; ally_x = ally_y = -1;   // brygada: raz na etap
            blast_timer = 0; blast_x = blast_y = -1; shot_events = 0; docs = 0;
            for(auto& row : fov) for(auto& c : row) c = unknown;
            const stage_def& sd = data::stages[stage];
            const room& first = lv.rooms[0];
            const room& last = lv.rooms[lv.rooms_count - 1];
            hero.x = int8_t(first.cx()); hero.y = int8_t(first.cy());
            boss = -1; stairs_x = stairs_y = -1;
            if(sd.boss < 0) { stairs_x = last.cx(); stairs_y = last.cy(); lv.t[stairs_y][stairs_x] = tile::stairs; }

            enemies_count = 0;
            const int count = imax(1, sd.enemy_count + (pd ? pd->enemies : 0));   // ścieżka: więcej / mniej problemów
            for(int i = 0; i < count && enemies_count < max_enemies; ++i)
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
                const enemy_def& bd = data::enemies[sd.boss];
                for(int k = 0; k < bd.summon_max && enemies_count < max_enemies; ++k)   // uśpione miejsca na wezwanych
                {
                    spawn(bd.summon, x, y);
                    enemies[enemies_count - 1].alive = false;
                }
            }

            pickups_count = 0;
            const int pickups_n = imax(1, 3 + bonus.pickups + (pd ? pd->pickups : 0));
            for(int i = 0; i < pickups_n && i < max_pickups && lv.rooms_count > 1; ++i)
            {
                const room& rm = lv.rooms[r.range(1, lv.rooms_count - 1)];
                int x, y; random_free_cell_in_room(rm, x, y);
                pickups[pickups_count++] = { int8_t(x), int8_t(y), uint8_t(i == 0 ? coffee : r.range(0, 2)), true };
            }
            push(message().add("Etap ").add(stage_number()).add(": ").add(sd.name));
            if(pd)   // ścieżka z harmonogramu: budżet i materiały od razu
            {
                push(message().add("Ścieżka: ").add(pd->name));
                if(pd->cash != 0) cash = imax(0, cash + income(pd->cash));
                for(int k = 0; k < pd->materials; ++k) add_material(r.range(0, data::materials_count - 1));
            }
            weather = int8_t(roll_weather(s, pd && pd->bad_weather));   // pogoda dnia
            if(wdef().effect != weather_effect::none)
                push(message().add("Pogoda: ").add(wdef().name).add(" (").add(wdef().short_name).add(")").as(wdef().bad ? bad : good));
            stage_event = -1;   // wydarzenie na placu: nie na pierwszym etapie i nie u bossa
            if(s > first_stage && sd.boss < 0 && ! (pd && pd->no_event) && r.range(1, 100) <= data::site_event_chance_pct)
            {
                int e = r.range(0, data::site_events_count - 1);
                // niekorzystna pogoda i niekorzystne wydarzenie naraz to za dużo: wydarzenie przepada
                if(! (data::weather_no_bad_stack && wdef().bad && ! data::site_events[e].good)) apply_event(e);
            }
            place_documents();
            update_fov();
        }

        // Pieczątki (Akt 0): dokumenty w różnych pokojach (bez pierwszego), na wolnych polach bez znajdziek.
        void place_documents()
        {
            const int n = docs_needed();
            if(n == 0 || lv.rooms_count < 2) return;
            const int span = lv.rooms_count - 1, base = r.range(0, span - 1);
            for(int k = 0; k < n && pickups_count < max_pickups; ++k)
            {
                const room& rm = lv.rooms[1 + (base + k * imax(1, span / n)) % span];
                int x = rm.cx(), y = rm.cy();
                for(int t = 0; t < 40; ++t)
                {
                    int cx = r.range(rm.x, rm.x + rm.w - 1), cy = r.range(rm.y, rm.y + rm.h - 1);
                    if(lv.at(cx, cy) == tile::floor && ! occupied(cx, cy) && ! pickup_at(cx, cy)) { x = cx; y = cy; break; }
                }
                pickups[pickups_count++] = { int8_t(x), int8_t(y), uint8_t(document), true, uint8_t(k) };
            }
            push(message().add("Pieczątki: zbierz ").add(n).add(" dokumenty").as(bad));
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

        // Rozpiska obrażeń broni (#26) - wzory jak hero_attack niżej (opis przy dmg_breakdown). enemy_def_id: problem
        // (jego obrona; -1 = bez), weapon_idx: inna broń (-1 = obecna), swap_*: sprzęt w slocie po zamianie (paczka).
        dmg_breakdown weapon_breakdown(int enemy_def_id = -1, int weapon_idx = -1, int swap_slot = -1, int swap_rarity = -1,
                                       int swap_trait = 0) const
        {
            dmg_breakdown b;
            b.weapon = weapon_idx >= 0 ? weapon_idx : (weapon_override >= 0 ? weapon_override : cdef().weapon);
            const weapon_def& w = data::weapons[b.weapon];
            b.wmin = w.min_damage; b.wmax = w.max_damage;
            b.range_base = w.range; b.range = range_of(w);
            b.scales = w.scales_with;
            const trait_effect st_tr = stat_trait(w.scales_with);
            int luck_t = 0;
            for(int i = 0; i < data::gear_slots_count; ++i)
            {
                int rar = i == swap_slot ? swap_rarity : equipped[i], tr = i == swap_slot ? swap_trait : equipped_trait[i];
                if(rar < 0) continue;
                const gear_def& gd = data::gear[i * 3 + rar];
                if(gd.stat == gear_stat::dmg) { b.flat_gear += gd.value; b.gear_item = i * 3 + rar; }
                const trait_def& td = data::gear_traits[tr];
                if(td.effect == trait_effect::luck) luck_t += td.value;
                else if(td.effect == trait_effect::crit) b.crit_trait += td.value;
                else if(td.effect == st_tr) b.stat_trait += td.value;
            }
            b.stat_class = class_base_stat(cls, w.scales_with);
            b.stat_craft = mods_stat_bonus(bonus, cls, w.scales_with);
            b.flat_mods = bonus.dmg;
            b.flat_level = level_dmg(hero_level);
            b.flat_found = dmg_bonus - bonus.dmg - b.flat_level;
            b.pct = bonus.dmg_pct;
            b.vs_enemy = enemy_def_id >= 0;
            b.enemy_def = b.vs_enemy ? data::enemies[enemy_def_id].defense : 0;
            b.luck = cdef().luck + bonus.luck + luck_t;
            b.crit_bonus = bonus.crit;
            b.power_rank = ability_rank();
            b.power = power_dmg_bonus();
            b.finish();
            return b;
        }
        // Premia mocy do ciosu (ability): Seria i Rynna +1 od rangi II, Taran +ranga.
        int power_dmg_bonus() const
        {
            int rank = ability_rank();
            switch(cdef().ability)
            {
                case ability_effect::volley:
                case ability_effect::line: return rank >= 2 ? 1 : 0;
                case ability_effect::ram:  return rank;
                default:                   return 0;
            }
        }
        // Cios problemu ei w bohatera (zakres po OBR i -%).
        hit_range enemy_hit(int ei) const
        {
            const actor& e = enemies[ei];
            const enemy_def& ed = data::enemies[e.def_id];
            return enemy_hit_range(ed.min_damage, ed.max_damage, enemy_dmg_bonus() + e.grow / 2, hero_defense(), bonus.taken_pct);
        }

        // obrażenia = rzut broni + stat/2 + premie - obrona/2, min 1
        void hero_attack(int ei)
        {
            const enemy_def& ed = data::enemies[enemies[ei].def_id];
            if(ei == boss && boss_wake_damage < 0) boss_engaged();   // walka z bossem trwa
            int dmg = r.range(weapon().min_damage, weapon().max_damage) + hero_stat(weapon().scales_with) / 2 + dmg_bonus
                    + gear_bonus(gear_stat::dmg) - ed.defense / 2;
            if(dmg < 1) dmg = 1;
            dmg += pct_part(dmg, bonus.dmg_pct, dmg_carry);   // Kurs fachowy, Respekt: +% obrażeń
            bool crit = r.range(1, 100) <= crit_pct();
            if(crit) dmg *= data::crit_multiplier;
            damage_enemy(ei, dmg, crit, weapon().name);
            // Operator koparki: cios wręcz czasem odpycha problem o pole (bossa nie)
            actor& e = enemies[ei];
            if(has_passive(class_passive::push) && e.alive && ei != boss && cheb(hero.x, hero.y, e.x, e.y) == 1
               && r.range(1, 100) <= data::push_chance_pct)
                shove(ei, isign(e.x - hero.x), isign(e.y - hero.y), 1);
        }

        // Odepchnięcie problemu o n pól w kierunku (dx, dy), dopóki pole wolne; zwraca, o ile przesunięto.
        int shove(int ei, int dx, int dy, int n)
        {
            actor& e = enemies[ei];
            int moved = 0;
            for(int k = 0; k < n; ++k)
            {
                int nx = e.x + dx, ny = e.y + dy;
                if(lv.at(nx, ny) != tile::floor || occupied(nx, ny)) break;
                e.x = int8_t(nx); e.y = int8_t(ny); ++moved;
            }
            return moved;
        }

        // Obrażenia dla bohatera po obronie: Szkolenie BHP i Respekt zmniejszają je o %, najmniej 1.
        int taken_damage(int dmg)
        {
            if(dmg < 1) dmg = 1;
            dmg -= pct_part(dmg, bonus.taken_pct, taken_carry);
            return dmg < 1 ? 1 : dmg;
        }

        // Bohater bez HP: Druga szansa (Respekt) raz na budowę zostawia 1 HP; inaczej koniec budowy.
        void hero_down()
        {
            if(bonus.second_chance > 0 && ! second_used)
            {
                second_used = true;
                hero.hp = 1;
                push(message().add("Druga szansa! Zostaje 1 HP").as(good));
                return;
            }
            hero.hp = 0; hero.alive = false; st = status::dead;
            push(message().add("Budowa wstrzymana...").as(bad));
        }

        // Obrażenia dla problemu (broń bohatera albo brygada; src = nazwa w dzienniku): trafienie, usunięcie, nagrody,
        // koniec etapu po bossie.
        void damage_enemy(int ei, int dmg, bool crit, const char* src)
        {
            actor& e = enemies[ei];
            const enemy_def& ed = data::enemies[e.def_id];
            if(ei == boss && boss_wake_damage < 0) boss_engaged();
            e.hp = int16_t(e.hp - dmg);
            e.awake = true;
            last_target = ei;
            if(ei == boss && ed.phase_pct > 0 && ! (e.flags & actor_phase) && e.hp * 100 <= e.max_hp * ed.phase_pct) boss_phase(ei);
            add_hit(e.x, e.y, dmg, false, crit ? hit_crit : hit_normal);
            turn_events |= 1u << ei;
            if(e.hp <= 0 && ei != boss && (ed.tags & tag_returns) && ! (e.flags & actor_returned))   // wraca raz
            {
                e.alive = false; e.hp = 0;
                e.flags = uint8_t(e.flags | actor_returned | actor_reviving);
                e.timer = int8_t(data::behavior_return_turns);
                push(message().add(ed.name).add(" - wróci za ").add(data::behavior_return_turns).add(" t.!").as(bad));
                return;
            }
            if(e.hp <= 0)
            {
                e.alive = false; ++kills; ++stage_kills; ++act_kills;
                cash += income(ed.score / data::cash_per_score);
                if(kills_by_type[e.def_id] < 255) ++kills_by_type[e.def_id];
                score += ed.score * score_pct() / 100; gain_xp(data::xp_per_kill);
                maybe_drop(e.x, e.y);
                push(message().add(ed.name).add(" - usunięto!").as(good));
                if(ed.tags & tag_explodes) arm_blast(e.x, e.y, ed);
                if((ed.tags & tag_splits) && ! (e.flags & actor_child)) split(ei);
                if(ei == boss)   // boss: po kilka sztuk każdego materiału
                    for(int m = 0; m < data::materials_count; ++m) add_material(m, data::material_boss_drop);
                else if(r.range(1, 100) <= data::material_drop_pct * (100 + bonus.mats_pct) / 100)   // Respekt: Zapasy
                    add_material(ed.material >= 0 ? ed.material : r.range(0, data::materials_count - 1));
                if(ei == boss)
                {
                    if(stage_damage == boss_wake_damage && clean_bosses < 255) ++clean_bosses;   // zlecenie Czysta robota
                    score += (500 + 100 * imax(0, pattern_stage() + 1)) * score_pct() / 100;
                    gain_xp(data::xp_boss);
                    slam_timer = 0;
                    if(ed.reward_cash > 0)   // nagroda bossa (Inspekcja: Protokół bez uwag)
                    {
                        cash += income(ed.reward_cash);
                        push(message().add(ed.reward_title).add("! +").add(income(ed.reward_cash)).add(" zł").as(good));
                    }
                    finish_stage();
                    if(stage == data::stages_count - 1)
                    {
                        st = status::won;
                        push(message().add("Odbiór techniczny zaliczony!").as(good));
                    }
                    else if(data::stages[stage + 1].act == data::stages[stage].act)   // boss w środku aktu: dalej bez Hurtowni
                    {
                        st = status::stage_clear;
                        push(message().add("Etap zakończony: ").add(data::stages[stage].name).as(good));
                    }
                    else   // boss aktu: premia za akt, potem Hurtownia
                    {
                        const act_def& ad = data::acts[data::stages[stage].act];
                        int stages_in_act = 0;
                        for(int i = 0; i < data::stages_count; ++i) stages_in_act += data::stages[i].act == data::stages[stage].act;
                        act_bonus = income(ad.bonus_per_stage * stages_in_act + ad.bonus_per_kill * act_kills);
                        cash += act_bonus;
                        act_kills = 0;
                        act_cleared = true;
                        st = status::stage_clear;
                        push(message().add("Akt zaliczony! Premia ").add(act_bonus).add(" zł").as(good));
                    }
                }
            }
            else
                push(message().add(crit ? "KRYT! " : "").add(src).add(": -").add(dmg).add(" (").add(ed.name).add(")").as(crit ? loot : info));
        }

        // Druga faza bossa (Decyzja odmowna: Odwołanie): raz, gdy HP spadnie do phase_pct% (także ciosem, który by go
        // usunął) - odzyskuje phase_heal% max HP i od razu wzywa phase_summon problemów.
        void boss_phase(int ei)
        {
            actor& e = enemies[ei];
            const enemy_def& ed = data::enemies[e.def_id];
            e.flags = uint8_t(e.flags | actor_phase);
            int heal = e.max_hp * ed.phase_heal / 100;
            e.hp = int16_t(imin(e.max_hp, imax(1, e.hp) + heal));
            push(message().add(ed.phase_name).add("! ").add(ed.name).add(" +").add(heal).add(" HP").as(bad));
            for(int k = 0; k < ed.phase_summon && summons_used < ed.summon_max; ++k) summon_near(e.x, e.y);
        }

        // Akcje gracza. Zwracają true, jeśli zużyły turę.
        bool player_move(int dx, int dy)
        {
            if(st != status::playing) return false;
            if(shocked_turn()) return true;
            int nx = hero.x + dx, ny = hero.y + dy;
            int ei = enemy_at(nx, ny);
            bool stuck = false;
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
                else if(puddle(hero.x, hero.y))   // deszcz: kałuża = poślizg
                    apply_status(status_effect::slip, 2);
                if(mud(hero.x, hero.y) && ! puddle(hero.x, hero.y))   // akt I: błoto - grzęźniesz, tura przepada
                {
                    stuck = true;
                    push(message().add("Błoto! Grzęźniesz - tura stracona").as(bad));
                }
            }
            else return false;
            end_turn();
            if(stuck && st == status::playing) end_turn();
            return true;
        }

        int nearest_target() const
        {
            int best = -1, bd = 99;
            for(int i = 0; i < enemies_count; ++i)
            {
                const actor& e = enemies[i];
                int d = cheb(hero.x, hero.y, e.x, e.y);
                if(e.alive && d <= weapon_range() && d < bd) { bd = d; best = i; }
            }
            return best;
        }

        // Cele w zasięgu broni (widoczni, żywi), posortowane od najbliższego. Zwraca liczbę.
        int targets_in_range(int8_t* out, int max) const
        {
            int n = 0;
            for(int d = 1; d <= weapon_range(); ++d)
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
            if(! e.alive || ! visible(e.x, e.y) || cheb(hero.x, hero.y, e.x, e.y) > weapon_range()) return false;
            hero_attack(ei);
            end_turn();
            return true;
        }

        bool player_attack_nearest()
        {
            if(st != status::playing) return false;
            if(shocked_turn()) return true;
            int t = nearest_target();
            if(t < 0) { push(message().add("Brak celu w zasięgu ").add(weapon_range())); return false; }
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
            return imax(3, imax(4, cdef().ability_cooldown - 2 * (ability_rank() - 1)) - trait_bonus(trait_effect::cooldown) - bonus.cooldown)
                   + (weather_is(weather_effect::heat) ? wdef().value : 0);   // upał: moc odnawia się dłużej
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

        bool can_place_wall(int x, int y) const
        {
            return walls_count < max_walls && lv.at(x, y) == tile::floor && ! occupied(x, y) && ! pickup_at(x, y);
        }

        bool place_wall(int x, int y, int turns_left)
        {
            if(! can_place_wall(x, y)) return false;
            lv.t[y][x] = tile::wall;
            walls[walls_count++] = { int8_t(x), int8_t(y), int8_t(turns_left) };
            return true;
        }

        // Mur (2*half+1 pól) w poprzek drogi najbliższego widocznego wroga, na polu przed bohaterem (Ścianka, Załataj).
        // Zwraca liczbę pól linii (0 = brak widocznego wroga).
        int wall_line(int half, int8_t* xs, int8_t* ys) const
        {
            int t = nearest_visible_enemy();
            if(t < 0) return 0;
            int dx = isign(enemies[t].x - hero.x), dy = isign(enemies[t].y - hero.y);
            if(iabs(enemies[t].x - hero.x) >= iabs(enemies[t].y - hero.y)) dy = 0; else dx = 0;
            int cx = hero.x + dx, cy = hero.y + dy;          // środek muru: pole przed bohaterem
            int px = dy != 0 ? 1 : 0, py = dx != 0 ? 1 : 0;   // kierunek muru: prostopadle
            int n = 0;
            for(int k = -half; k <= half; ++k) { xs[n] = int8_t(cx + px * k); ys[n] = int8_t(cy + py * k); ++n; }
            return n;
        }
        bool wall_possible(int half) const
        {
            int8_t xs[5], ys[5];
            int n = wall_line(half, xs, ys);
            for(int i = 0; i < n; ++i) if(can_place_wall(xs[i], ys[i])) return true;
            return false;
        }
        bool wall_toward_enemy(int half, int dur)
        {
            int8_t xs[5], ys[5];
            int n = wall_line(half, xs, ys);
            bool ok = false;
            for(int i = 0; i < n; ++i) ok |= place_wall(xs[i], ys[i], dur);
            return ok;
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
                    ok = wall_toward_enemy(rank >= 3 ? 2 : 1, 4 + 2 * rank);
                    if(ok) push(message().add(c.ability_name).add(" postawiona!"));
                    break;
                case ability_effect::volley:   // Seria: wszyscy widoczni w zasięgu (+1 obrażeń od II, +1 zasięgu na III)
                {
                    int range = weapon_range() + (rank >= 3 ? 1 : 0);
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
                case ability_effect::line:   // Rynna (Dekarz): dachówki lecą linią przez najbliższy widoczny problem (4/5/6 pól)
                {
                    int t = nearest_visible_enemy();
                    if(t < 0) break;
                    int dx = enemies[t].x - hero.x, dy = enemies[t].y - hero.y, len = imax(iabs(dx), iabs(dy));
                    if(rank >= 2) ++dmg_bonus;
                    uint32_t done = 0;
                    for(int k = 1; k <= 3 + rank && st == status::playing; ++k)
                    {
                        int x = hero.x + div_round(dx * k, len), y = hero.y + div_round(dy * k, len);
                        if(! lv.passable(x, y)) break;   // mur zatrzymuje dachówki
                        int ei = enemy_at(x, y);
                        if(ei >= 0 && ! (done & (1u << ei))) { hero_attack(ei); done |= 1u << ei; ok = true; }
                    }
                    if(rank >= 2) --dmg_bonus;
                    if(ok) push(message().add(c.ability_name).add(": dachówki w linii!"));
                    break;
                }
                case ability_effect::splash:   // Narzut (Tynkarz): tynk na obszar wokół celu w zasięgu (3x3, 5x5 na III), od II ogłusza
                {
                    int t = nearest_target();
                    if(t < 0) break;
                    int cx = enemies[t].x, cy = enemies[t].y, rad = rank >= 3 ? 2 : 1;
                    for(int i = 0; i < enemies_count && st == status::playing; ++i)
                        if(enemies[i].alive && cheb(cx, cy, enemies[i].x, enemies[i].y) <= rad)
                        {
                            hero_attack(i); ok = true;
                            if(rank >= 2 && enemies[i].alive) enemies[i].stun = int8_t(imax(enemies[i].stun, 1));
                        }
                    break;
                }
                case ability_effect::ram:   // Taran (Operator koparki): szarża 3/4/5 pól do problemu, cios +ranga, odepchnięcie o 2
                {
                    int t = nearest_visible_enemy();
                    if(t < 0) break;
                    int ex = enemies[t].x - hero.x, ey = enemies[t].y - hero.y;
                    int dx = iabs(ey) >= 2 * iabs(ex) ? 0 : isign(ex), dy = iabs(ex) >= 2 * iabs(ey) ? 0 : isign(ey);
                    bool moved = false;
                    for(int k = 0; k < 2 + rank; ++k)
                    {
                        int nx = hero.x + dx, ny = hero.y + dy;
                        int ei = enemy_at(nx, ny);
                        if(ei >= 0)
                        {
                            dmg_bonus += rank;
                            hero_attack(ei);
                            dmg_bonus -= rank;
                            if(enemies[ei].alive && st == status::playing)
                            {
                                if(ei != boss) shove(ei, dx, dy, 2);
                                enemies[ei].stun = int8_t(imax(enemies[ei].stun, 1));
                            }
                            ok = true;
                            break;
                        }
                        if(! lv.passable(nx, ny) || occupied(nx, ny)) break;
                        hero.x = int8_t(nx); hero.y = int8_t(ny); moved = ok = true;
                    }
                    if(moved) collect();
                    if(ok) push(message().add(c.ability_name).add("!"));
                    break;
                }
                default: break;
            }
            if(! ok) { push(message().add(c.ability_name).add(": nie teraz")); return false; }
            if(powers_used < 65535) ++powers_used;
            end_turn();
            ability_cd = ability_cooldown();
            return true;
        }

        // ------------------------------------------------------------------ brygada (raz na etap, z telefonu)
        static constexpr int8_t around8[8][2] = { { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 }, { 1, 1 }, { -1, 1 }, { 1, -1 }, { -1, -1 } };

        // Wolne pole podłogi obok (x, y), najbliższe (nx, ny); false, gdy brak.
        bool free_around(int x, int y, int nx, int ny, int& ox, int& oy) const
        {
            int bd = 99;
            for(const auto& o : around8)
            {
                int cx = x + o[0], cy = y + o[1];
                if(lv.at(cx, cy) != tile::floor || occupied(cx, cy) || pickup_at(cx, cy)) continue;
                int d = cheb(cx, cy, nx, ny);
                if(d < bd) { bd = d; ox = cx; oy = cy; }
            }
            return bd < 99;
        }

        enum helper_block : uint8_t { helper_ok, helper_busy, helper_used, helper_locked, helper_cash, helper_no_target, helper_no_room };

        // Cena fachowca (zł) po rabacie z Respektu (Znajomości).
        int helper_price(int h) const { return data::brigade[h].price * (100 - bonus.brigade_pct) / 100; }

        // Czy fachowca h można teraz wezwać (bez skutków ubocznych - telefon i bot).
        int helper_blocked(int h) const
        {
            const helper_def& hd = data::brigade[h];
            if(st != status::playing) return helper_busy;
            if(helper_called >= 0) return helper_used;
            if(! ((bonus.helpers >> h) & 1)) return helper_locked;
            if(cash < helper_price(h)) return helper_cash;
            if(hd.effect == helper_effect::pump)
            {
                for(int i = 0; i < enemies_count; ++i)
                    if(enemies[i].alive && cheb(hero.x, hero.y, enemies[i].x, enemies[i].y) <= hd.reach) return helper_ok;
                return helper_no_target;
            }
            int x, y;
            if(hd.effect == helper_effect::ally && ! free_around(hero.x, hero.y, hero.x, hero.y, x, y)) return helper_no_room;
            return helper_ok;
        }

        // Wezwanie fachowca (zużywa turę i budżet). Geodeta: mapa etapu; pompa: beton na problemy wokół;
        // BHP-owiec: zdejmuje stany i daje obronę; pomocnik: idzie za bohaterem i bije sąsiadów przez kilka tur.
        bool call_helper(int h)
        {
            const helper_def& hd = data::brigade[h];
            switch(helper_blocked(h))
            {
                case helper_ok: break;
                case helper_used: push(message().add("Brygada już była na tym etapie")); return false;
                case helper_cash: push(message().add("Brygada: za mały budżet (").add(helper_price(h)).add(" zł)")); return false;
                case helper_no_target: push(message().add(hd.name).add(": nikogo w zasięgu")); return false;
                case helper_no_room: push(message().add(hd.name).add(": brak miejsca obok")); return false;
                default: return false;
            }
            if(shocked_turn()) return true;
            cash -= helper_price(h);
            helper_called = int8_t(h);
            push(message().add("Brygada: ").add(hd.name).as(good));
            switch(hd.effect)
            {
                case helper_effect::reveal:   // podłoga, schody i mury przy nich
                    for(int y = 0; y < map_h; ++y)
                        for(int x = 0; x < map_w; ++x)
                        {
                            if(fov[y][x] != unknown) continue;
                            bool near = false;
                            for(int dy = -1; dy <= 1 && ! near; ++dy) for(int dx = -1; dx <= 1; ++dx) if(lv.passable(x + dx, y + dy)) { near = true; break; }
                            if(near) fov[y][x] = remembered;
                        }
                    break;
                case helper_effect::pump:
                    for(int i = 0; i < enemies_count && st == status::playing; ++i)
                        if(enemies[i].alive && cheb(hero.x, hero.y, enemies[i].x, enemies[i].y) <= hd.reach) damage_enemy(i, hd.value, false, hd.name);
                    break;
                case helper_effect::safety:
                    hero_status[int(status_effect::poison)] = hero_status[int(status_effect::shock)] = hero_status[int(status_effect::slip)] = 0;
                    guard_turns = int8_t(hd.turns + 1);   // + tura wezwania
                    break;
                case helper_effect::ally:
                {
                    int x = -1, y = -1;
                    free_around(hero.x, hero.y, hero.x, hero.y, x, y);
                    ally_x = int8_t(x); ally_y = int8_t(y); ally_turns = hd.turns;
                    break;
                }
                default: break;
            }
            end_turn();
            return true;
        }

        // Pomocnik: trzyma się obok bohatera i bije problem obok siebie (bez rzutu - stałe obrażenia).
        void ally_act()
        {
            const helper_def& hd = data::brigade[helper_called];
            if(cheb(ally_x, ally_y, hero.x, hero.y) != 1)
            {
                int x, y;
                if(free_around(hero.x, hero.y, ally_x, ally_y, x, y)) { ally_x = int8_t(x); ally_y = int8_t(y); }
            }
            for(int i = 0; i < enemies_count; ++i)
                if(enemies[i].alive && cheb(ally_x, ally_y, enemies[i].x, enemies[i].y) == 1) { damage_enemy(i, hd.value, false, hd.name); break; }
            if(--ally_turns == 0)
            {
                ally_x = ally_y = -1;
                push(message().add(hd.name).add(": koniec pomocy"));
            }
        }

        // Hurtownia między aktami: zakup za budżet budowy.
        // Czy stać na towar z Hurtowni (zł albo materiał).
        bool hurtownia_can(int i) const
        {
            const shop_item_def& it = data::hurtownia[i];
            return it.material >= 0 ? mats[it.material] >= it.mat_cost : cash >= hurtownia_price(i);
        }
        // Cena towaru w zł po rabacie z Respektu.
        int hurtownia_price(int i) const { return data::hurtownia[i].price * (100 - bonus.shop_pct) / 100; }

        // Losowy slot sprzętu spośród dostępnych (nagrody dokładają buty i pas); przy 3 slotach jak dawniej.
        int random_slot()
        {
            int n = 0;
            for(int i = 0; i < data::gear_slots_count; ++i) n += (bonus.gear_slots >> i) & 1;
            int k = r.range(0, imax(1, n) - 1);
            for(int i = 0; i < data::gear_slots_count; ++i) if(((bonus.gear_slots >> i) & 1) && k-- == 0) return i;
            return 0;
        }

        bool hurtownia_buy(int i)
        {
            const shop_item_def& it = data::hurtownia[i];
            if(! hurtownia_can(i)) return false;
            switch(it.effect)
            {
                case shop_effect::heal: hero.hp = hero.max_hp; break;
                case shop_effect::maxhp: hero.max_hp = int16_t(hero.max_hp + 3); hero.hp = int16_t(hero.hp + 3); break;
                case shop_effect::ability: ability_cd = 0; break;
                case shop_effect::def: ++def_bonus; break;
                case shop_effect::thermos: thermos = imin(thermos_cap(), thermos + 2); break;
                case shop_effect::gear:
                {
                    int slot = random_slot(), rarity = r.range(1, 2);
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
                default: break;
            }
            if(it.material >= 0) mats[it.material] = uint8_t(mats[it.material] - it.mat_cost);
            else cash -= hurtownia_price(i);
            push(message().add("Hurtownia: ").add(it.name).as(loot));
            return true;
        }

        // ------------------------------------------------------------------ materiały i naprawy pola
        void add_material(int m, int n = 1)
        {
            int v = imin(data::material_max, mats[m] + n), got = v - mats[m];
            mats[m] = uint8_t(v);
            if(got > 0) push(message().add(data::materials[m].name).add(" +").add(got).as(loot));
        }

        enum repair_block : uint8_t { repair_ok, repair_busy, repair_material, repair_no_target, repair_no_room, repair_no_puddle };

        bool puddle_near(int reach) const   // kałuże albo błoto (Kładka działa na oba)
        {
            for(int y = hero.y - reach; y <= hero.y + reach; ++y)
                for(int x = hero.x - reach; x <= hero.x + reach; ++x) if(puddle(x, y) || mud(x, y)) return true;
            return false;
        }

        // Czy naprawę k można teraz zrobić (bez skutków ubocznych - telefon i bot).
        int repair_blocked(int k) const
        {
            const repair_def& rd = data::repairs[k];
            if(st != status::playing) return repair_busy;
            if(mats[rd.material] < rd.cost) return repair_material;
            if(rd.effect == repair_effect::patch)
            {
                if(nearest_visible_enemy() < 0) return repair_no_target;
                if(! wall_possible(1)) return repair_no_room;
            }
            if(rd.effect == repair_effect::bridge && (bridges >= max_bridges || ! puddle_near(rd.value))) return repair_no_puddle;
            return repair_ok;
        }

        // Naprawa za materiał (zużywa turę): Załataj - mur z desek przed najbliższym problemem, Kładka - kałuże wokół
        // bez poślizgu do końca etapu (i koniec poślizgu).
        bool player_repair(int k)
        {
            const repair_def& rd = data::repairs[k];
            switch(repair_blocked(k))
            {
                case repair_ok: break;
                case repair_material: push(message().add(rd.name).add(": brak - ").add(data::materials[rd.material].name)); return false;
                case repair_no_target: push(message().add(rd.name).add(": brak problemu w polu widzenia")); return false;
                case repair_no_room: push(message().add(rd.name).add(": nie ma gdzie")); return false;
                case repair_no_puddle: push(message().add(rd.name).add(": brak kałuż obok")); return false;
                default: return false;
            }
            if(shocked_turn()) return true;
            mats[rd.material] = uint8_t(mats[rd.material] - rd.cost);
            if(rd.effect == repair_effect::patch) wall_toward_enemy(1, rd.value);
            else
            {
                bridge_x[bridges] = hero.x; bridge_y[bridges] = hero.y; ++bridges;
                hero_status[int(status_effect::slip)] = 0;
            }
            push(message().add(rd.name).add(": ").add(rd.desc).as(good));
            end_turn();
            return true;
        }

        // ------------------------------------------------------------------ wybór ścieżki (harmonogram)
        // Oferta na kolejny etap: dwie różne ścieżki zależne od seeda budowy i etapu (bez losowania z RNG gry).
        int path_offer(int k) const
        {
            uint32_t h = (run_seed ^ (uint32_t(pattern_stage() + 1 + tier * 16) * 2654435761u)) * 2246822519u;
            h ^= h >> 15;
            int a = int(h % uint32_t(data::paths_count));
            if(k == 0) return a;
            return (a + 1 + int((h >> 8) % uint32_t(data::paths_count - 1))) % data::paths_count;
        }
        void choose_path(int k) { next_path = int8_t(k & 1); }

        // Etap zaliczony: ile tur trwał (harmonogram domu po wygranej).
        void finish_stage()
        {
            stage_days[stage] = uint16_t(imin(65535, turns - stage_start_turn));
            int got = stage_respect();
            respect += got;
            push(message().add("Respekt +").add(got).as(loot));
        }

        // Respekt za bieżący etap: zwykły, boss w środku aktu, boss aktu, ostatni; mnożnik jak wynik (trudność, NG+).
        int stage_respect() const
        {
            const stage_def& sd = data::stages[stage];
            int base = stage == data::stages_count - 1 ? data::respect_final
                     : (sd.boss < 0 ? data::respect_stage : (data::stages[stage + 1].act == sd.act ? data::respect_boss : data::respect_act_boss));
            return imax(1, base * score_pct() / 100);
        }

        int coffee_heal() const { return div_round((data::coffee_heal + bonus.coffee) * (100 + bonus.coffee_pct), 100); }   // Respekt: Mocna kawa

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
                int q = r.range(1, 100) + imax(0, pattern_stage()) * data::gear_stage_bonus + data::rarity_per_luck * luck() + bonus.gear_pct;
                int rarity = q >= data::gear_brand_from ? 2 : (q >= data::gear_solid_from ? 1 : 0);
                arg = uint8_t(random_slot() * 3 + rarity);
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
                int delta = nw.value - (equipped[slot] >= 0 ? data::gear[slot * 3 + equipped[slot]].value : 0);
                hero.max_hp = int16_t(hero.max_hp + delta);
                hero.hp = int16_t(imax(1, hero.hp + delta));
            }
            equipped[slot] = int8_t(rarity);
            equipped_trait[slot] = int8_t(trait);
            thermos = imin(thermos, thermos_cap());   // słabszy pas: kawy ponad miejsca przepadają
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
                else if(p.type == document)   // pieczątki: komplet otwiera schody
                {
                    docs = uint8_t(docs | (1u << p.arg));
                    push(message().add("Dokument: ").add(data::documents[p.arg]).add(" (").add(docs_count()).add("/").add(docs_needed()).add(")").as(loot));
                    if(! stairs_locked()) push(message().add("Komplet pieczątek! Schody otwarte").as(good));
                }
                else if(p.type == gear_box)
                {
                    add_material(r.range(0, data::materials_count - 1), data::material_gear_box);   // w paczce też materiał
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

        // Wezwanie (Inspekcja: Papierologia): budzi kolejne uśpione miejsce za bossem na wolnym polu obok niego.
        bool summon_near(int bx, int by)
        {
            int slot = boss + 1 + summons_used;
            if(slot >= enemies_count) return false;
            static constexpr int8_t around[8][2] = { { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 }, { 1, 1 }, { -1, 1 }, { 1, -1 }, { -1, -1 } };
            for(const auto& o : around)
            {
                int x = bx + o[0], y = by + o[1];
                if(lv.at(x, y) != tile::floor || occupied(x, y)) continue;
                actor& m = enemies[slot];
                m.x = int8_t(x); m.y = int8_t(y); m.hp = m.max_hp; m.alive = true; m.awake = true; m.stun = 0;
                ++summons_used;
                push(message().add("Wezwanie: ").add(data::enemies[m.def_id].name).as(bad));
                return true;
            }
            return false;
        }

        void enemy_act(int i)
        {
            actor& e = enemies[i];
            const enemy_def& ed = data::enemies[e.def_id];
            int d = cheb(e.x, e.y, hero.x, hero.y);
            if(! e.awake) { if(d <= ed.sight) e.awake = true; else return; }
            if(i == boss && boss_wake_damage < 0) boss_engaged();
            if(e.stun > 0) { --e.stun; return; }
            if(ed.slam && i == boss)   // boss: co kilka tur zapowiada uderzenie w obszar wokół bohatera
            {
                if(slam_timer > 0) return;   // ładuje cios, stoi w miejscu
                if(++slam_counter >= slam_every() && d <= 4)
                {
                    bool cross = ed.shape == slam_shape::cross;
                    slam_counter = 0;
                    slam_timer = cross ? data::slam_cross_delay : data::slam_delay;
                    slam_x = hero.x; slam_y = hero.y;
                    push(message().add(ed.slam_name[0] ? ed.slam_name : "Cios bossa").add(" za ").add(slam_timer).add(" tury!").as(bad));
                    return;
                }
            }
            if(i == boss && ed.summon >= 0 && summons_used < ed.summon_max && ++summon_counter >= ed.summon_every && d <= 6)
            {
                summon_counter = 0;
                if(summon_near(e.x, e.y)) return;   // wezwanie zużywa turę bossa
            }
            // Termin: porusza się co drugą turę, poniżej połowy HP przyspiesza
            if(i == boss && e.hp * 2 > e.max_hp && (turns & 1)) return;
            const uint16_t tg = ed.tags;
            if(tg & tag_grows) grow_tick(i);
            if(e.timer > 0) --e.timer;   // odnowienie ucieczki / łatania
            int manh = iabs(e.x - hero.x) + iabs(e.y - hero.y);
            if((tg & tag_heals) && manh != 1 && e.timer == 0 && heal_near(i)) { e.timer = int8_t(data::behavior_heal_every); return; }
            if((tg & tag_flees) && manh == 1 && e.timer == 0 && flee_step(i)) { e.timer = int8_t(data::behavior_flee_cooldown); return; }
            if(manh == 1) { enemy_strike(i, false); return; }
            if((tg & tag_ranged) && shot_line(e.x, e.y)) { shot_events |= 1u << i; enemy_strike(i, true); return; }
            if(tg & tag_stationary) return;
            if(weather_is(weather_effect::frost) && i != boss && turns % wdef().value == 0) return;   // mróz: problemy stoją
            if((tg & tag_ranged) && ranged_step(i)) return;
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

        // Cios problemu (wręcz albo z dystansu): unik ze szczęścia, obrażenia po obronie, stan, odepchnięcie.
        void enemy_strike(int i, bool ranged)
        {
            actor& e = enemies[i];
            const enemy_def& ed = data::enemies[e.def_id];
            if(dodge_pct() > 0 && r.range(1, 100) <= dodge_pct())   // szczęście: unik
            {
                add_hit(hero.x, hero.y, 0, true, hit_dodge);
                push(message().add("Unik! ").add(ed.name).add(" chybia").as(good));
                return;
            }
            int dmg = taken_damage(r.range(ed.min_damage, ed.max_damage) + enemy_dmg_bonus() + e.grow / 2 - hero_defense() / 2);
            hero.hp = int16_t(hero.hp - dmg);
            stage_damage += dmg;
            hero_hit = true;
            add_hit(hero.x, hero.y, dmg, true);
            push(message().add(ed.name).add(ranged ? " z dystansu: -" : ": -").add(dmg).add(" HP").as(bad));
            if(ed.on_hit != status_effect::none && hero.hp > 0 && r.range(1, 100) <= ed.status_chance)
                apply_status(ed.on_hit, ed.status_turns);
            if(event_active(event_effect::rain) && hero.hp > 0 && r.range(1, 100) <= data::site_events[stage_event].value)
                apply_status(status_effect::slip, 2);   // Ulewa w nocy: błoto na placu
            if((ed.tags & tag_pushes) && ! ranged && hero.hp > 0 && e.timer == 0)   // odepchnięcie o pole (co kilka tur)
            {
                e.timer = int8_t(data::behavior_push_cooldown);
                int nx = hero.x + isign(hero.x - e.x), ny = hero.y + isign(hero.y - e.y);
                if(lv.passable(nx, ny) && ! occupied(nx, ny))
                {
                    hero.x = int8_t(nx); hero.y = int8_t(ny);
                    collect(); update_fov();
                    push(message().add(ed.name).add(" odpycha cię!").as(bad));
                }
            }
            if(hero.hp <= 0) hero_down();
        }

        // Linia strzału z (x, y) do bohatera: odległość 2..zasięg, prosto albo po skosie, bez murów i postaci po drodze.
        bool shot_line(int x, int y) const
        {
            int dx = hero.x - x, dy = hero.y - y, d = cheb(x, y, hero.x, hero.y);
            if(d < 2 || d > data::behavior_ranged_reach || ! (dx == 0 || dy == 0 || iabs(dx) == iabs(dy))) return false;
            for(int k = 1; k < d; ++k)
            {
                int cx = x + isign(dx) * k, cy = y + isign(dy) * k;
                if(! lv.passable(cx, cy) || occupied(cx, cy)) return false;
            }
            return true;
        }

        // Strzelec ustawia się w linii: krok na pole, z którego ma czysty strzał.
        bool ranged_step(int i)
        {
            actor& e = enemies[i];
            for(const auto& o : around8)
            {
                if(o[0] != 0 && o[1] != 0) continue;   // problemy chodzą tylko prosto
                int nx = e.x + o[0], ny = e.y + o[1];
                if(lv.at(nx, ny) == tile::floor && ! occupied(nx, ny) && shot_line(nx, ny)) { e.x = int8_t(nx); e.y = int8_t(ny); return true; }
            }
            return false;
        }

        // Ucieczka: krok prosto na pole dalej od bohatera (bez miejsca - nie ucieka).
        bool flee_step(int i)
        {
            actor& e = enemies[i];
            int bd = cheb(e.x, e.y, hero.x, hero.y), bx = -1, by = -1;
            for(const auto& o : around8)
            {
                if(o[0] != 0 && o[1] != 0) continue;
                int nx = e.x + o[0], ny = e.y + o[1];
                if(lv.at(nx, ny) != tile::floor || occupied(nx, ny)) continue;
                int d = cheb(nx, ny, hero.x, hero.y);
                if(d > bd) { bd = d; bx = nx; by = ny; }
            }
            if(bx < 0) return false;
            e.x = int8_t(bx); e.y = int8_t(by);
            if(visible(bx, by)) push(message().add(data::enemies[e.def_id].name).add(" ucieka"));
            return true;
        }

        // Łatanie: najbardziej ranny problem w zasięgu 2 (bez bossa) dostaje HP.
        bool heal_near(int i)
        {
            const actor& e = enemies[i];
            int best = -1, lack = 0;
            for(int j = 0; j < enemies_count; ++j)
            {
                const actor& o = enemies[j];
                if(j == i || j == boss || ! o.alive || cheb(e.x, e.y, o.x, o.y) > 2 || o.max_hp - o.hp <= lack) continue;
                best = j; lack = o.max_hp - o.hp;
            }
            if(best < 0) return false;
            actor& o = enemies[best];
            int h = imin(data::behavior_heal_value, lack);
            o.hp = int16_t(o.hp + h);
            if(visible(e.x, e.y) || visible(o.x, o.y))
                push(message().add(data::enemies[e.def_id].name).add(" łata: ").add(data::enemies[o.def_id].name).add(" +").add(h).as(bad));
            return true;
        }

        // Wzrost: co kilka tur (w walce) +HP, co drugi stopień +1 obrażeń.
        void grow_tick(int i)
        {
            actor& e = enemies[i];
            if(e.grow >= data::behavior_grow_max || turns % data::behavior_grow_every != 0) return;
            ++e.grow;
            e.max_hp = int16_t(e.max_hp + data::behavior_grow_hp);
            e.hp = int16_t(e.hp + data::behavior_grow_hp);
            if(visible(e.x, e.y)) push(message().add(data::enemies[e.def_id].name).add(" rośnie!").as(bad));
        }

        // Wybuch po usunięciu: czerwone pola wokół, spada po data::behavior_blast_delay turach (tura na zejście).
        void arm_blast(int x, int y, const enemy_def& ed)
        {
            blast_x = int8_t(x); blast_y = int8_t(y);
            blast_timer = int8_t(data::behavior_blast_delay);
            blast_dmg = int8_t(data::behavior_blast_damage + enemy_dmg_bonus());
            push(message().add(ed.name).add(": wybuch za ").add(data::behavior_blast_delay - 1).add(" t.! Odejdź").as(bad));
        }

        // Miejsce na nowy problem (podział): wolny slot na końcu albo po usuniętym (nie boss, nie wezwani, nie czekający).
        int free_slot()
        {
            if(enemies_count < max_enemies) return enemies_count++;
            int reserve = boss >= 0 ? data::enemies[enemies[boss].def_id].summon_max : 0;
            for(int j = 0; j < enemies_count; ++j)
                if(! enemies[j].alive && ! (enemies[j].flags & actor_reviving) && j != boss && ! (boss >= 0 && j > boss && j <= boss + reserve))
                    return j;
            return -1;
        }

        // Podział: dwa słabsze problemy (połowa max HP) na polu usuniętego i obok; same się już nie dzielą.
        void split(int ei)
        {
            const actor p = enemies[ei];
            int made = 0;
            for(int k = 0; k < 2; ++k)
            {
                int x = p.x, y = p.y;
                if(k == 1 || occupied(x, y)) { if(! free_around(p.x, p.y, hero.x, hero.y, x, y)) break; }
                int slot = free_slot();
                if(slot < 0) break;
                actor& c = enemies[slot];
                c = actor();
                c.x = int8_t(x); c.y = int8_t(y); c.def_id = p.def_id;
                c.hp = c.max_hp = int16_t(imax(1, p.max_hp * data::behavior_split_hp_pct / 100));
                c.alive = true; c.awake = true; c.stun = 1; c.flags = actor_child;
                ++made;
            }
            if(made) push(message().add(data::enemies[p.def_id].name).add(" dzieli się!").as(bad));
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
            if(guard_turns > 0) --guard_turns;   // ochrona BHP-owca mija
            for(int i = 0; i < walls_count; )
                if(--walls[i].turns <= 0) { lv.t[walls[i].y][walls[i].x] = tile::floor; walls[i] = walls[--walls_count]; }
                else ++i;
            update_fov();
            if(ally_turns > 0 && st == status::playing) ally_act();   // pomocnik z brygady
            if(slam_timer > 0 && --slam_timer == 0 && boss >= 0 && enemies[boss].alive)   // cios bossa spada
            {
                const enemy_def& bd = data::enemies[enemies[boss].def_id];
                if(slam_cell_at(hero.x, hero.y))
                {
                    int dmg = taken_damage(r.range(bd.min_damage, bd.max_damage) + enemy_dmg_bonus() + data::slam_damage_bonus - hero_defense() / 2);
                    hero.hp = int16_t(hero.hp - dmg);
                    stage_damage += dmg;
                    hero_hit = true;
                    add_hit(hero.x, hero.y, dmg, true);
                    push(message().add(bd.slam_name[0] ? bd.slam_name : "Uderzenie").add(": -").add(dmg).add(" HP").as(bad));
                    if(hero.hp <= 0) hero_down();
                }
                else push(message().add("Unik! Cios poszedł obok").as(good));
                slam_x = slam_y = -1;
            }
            if(blast_timer > 0 && --blast_timer == 0 && st == status::playing)   // wybuch po usuniętym problemie
            {
                if(cheb(hero.x, hero.y, blast_x, blast_y) <= data::behavior_blast_radius)
                {
                    int dmg = taken_damage(blast_dmg - hero_defense() / 2);
                    hero.hp = int16_t(hero.hp - dmg);
                    stage_damage += dmg;
                    hero_hit = true;
                    add_hit(hero.x, hero.y, dmg, true);
                    push(message().add("Wybuch: -").add(dmg).add(" HP").as(bad));
                    if(hero.hp <= 0) hero_down();
                }
                else push(message().add("Wybuch obok - uff!").as(good));
                blast_x = blast_y = -1;
            }
            if(st == status::playing && act_is(act_mechanic::gust)) gust_tick();   // akt II: porywy wiatru
            for(int i = 0; i < enemies_count && st == status::playing; ++i)   // "wraca raz": powrót po kilku turach
            {
                actor& e = enemies[i];
                if(e.alive || ! (e.flags & actor_reviving) || --e.timer > 0) continue;
                if(occupied(e.x, e.y)) { e.timer = 1; continue; }
                e.alive = true; e.awake = true;
                e.hp = int16_t(imax(1, e.max_hp * data::behavior_return_hp_pct / 100));
                e.flags = uint8_t(e.flags & ~actor_reviving);
                push(message().add(data::enemies[e.def_id].name).add(" wraca!").as(bad));
            }
            if(st == status::playing)
                for(int i = 0; i < enemies_count && st == status::playing; ++i)
                    if(enemies[i].alive) enemy_act(i);
            if(st == status::playing && hero.x == stairs_x && hero.y == stairs_y && stairs_locked())
                push(message().add("Schody zamknięte: dokumenty ").add(docs_count()).add("/").add(docs_needed()).as(bad));
            else if(st == status::playing && hero.x == stairs_x && hero.y == stairs_y)
            {
                st = status::stage_clear;
                finish_stage();
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
            if(boss >= 0 && enemies[boss].alive) { enemies[boss].hp = 1; enemies[boss].flags = uint8_t(enemies[boss].flags | actor_phase); hero_attack(boss); }
            else if(stairs_x >= 0) { docs = uint8_t((1u << docs_needed()) - 1); hero.x = int8_t(stairs_x); hero.y = int8_t(stairs_y); end_turn(); }
        }

        // Przejście do kolejnego etapu (po ekranie harmonogramu) wybraną ścieżką. Przerwa na kawę: +5 HP.
        void next_stage()
        {
            if(! investor_has(investor_effect::no_break)) hero.hp = int16_t(imin(hero.max_hp, hero.hp + 5));   // tryb inwestora: bez przerwy
            int path = path_offer(next_path);
            next_path = 0;
            start_stage(stage + 1, path);
        }

        // NG+ ("Kolejna budowa"): po wygranej ten sam zawód i poziom, premie i wynik zostają,
        // wrogowie mocniejsi o kolejny stopień. Zwraca false, jeśli budowa nie została ukończona.
        bool new_game_plus()
        {
            if(st != status::won) return false;
            ++tier;
            for(auto& e : enemies) e = actor();
            hero.hp = hero.max_hp;
            start_stage(first_stage);
            push(message().add("Kolejna budowa! Poziom ").add(tier + 1));
            return true;
        }
    };
}
