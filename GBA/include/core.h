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
    constexpr int max_stages = 12;      // v0.21.49: 10 etapów + 2 Aktu 0 (podsumowanie: 12 wartości w tablicach game);
                                        // v0.21.52 cz. d: tyle etapów ma najdłuższy kontrakt mapy kariery
    constexpr int max_gear_slots = 6;
    constexpr int recap_hits_n = 3;     // v0.21.50 cz. 4: ostatnie ciosy w bohatera (podsumowanie budowy)
    // Oś czasu podsumowania (#33): co się działo na etapie (bity w game::stage_flags).
    enum recap_flag : uint8_t { recap_secret = 1, recap_upgrade = 2, recap_elite = 4, recap_boss = 8, recap_combo = 16, recap_synergy = 32,
                                recap_event_boon = 64 };   // kask, rękawice, kamizelka + sloty z nagród (buty, pas)
    static_assert(data::gear_slots_count <= max_gear_slots);

    enum class tile : uint8_t { wall, floor, stairs };
    enum class status : uint8_t { playing, stage_clear, dead, won };
    enum sight : uint8_t { unknown = 0, remembered = 1, in_view = 2 };   // mgła wojny
    enum pickup_type : uint8_t { coffee, helmet, plan, tool, gear_box, document, event_tile, store_key, chest };
    // document: Akt 0 (arg = data::documents); v0.21.50 cz. 3: event_tile - wydarzenie z wyborem (arg = data::choice_events),
    // key - klucz do magazynu, chest - skrzynia w ukrytym pomieszczeniu

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
        int8_t elite = -1;      // v0.21.50: cecha elity (data::elites), -1 = zwykły problem
        int8_t wet = 0;         // v0.21.50: tury mokrego (kałuża, Zawór, Wąż ogrodowy); problem wodny jest mokry zawsze
    };
    // phase: boss w drugiej fazie; v0.21.50: dusty - zapylony (akt III), frozen - zmrożony (Mróz, Suchy lód), called - elita już wezwała
    enum actor_flag : uint8_t { actor_child = 1, actor_returned = 2, actor_reviving = 4, actor_phase = 8, actor_dusty = 16, actor_frozen = 32,
                                actor_called = 64 };

    struct temp_wall { int8_t x, y, turns; };   // Ścianka Murarza

    struct pickup { int8_t x, y; uint8_t type; bool active; uint8_t arg = 0; uint8_t trait = 0; };   // arg: narzędzie / slot*3+jakość; trait: cecha sprzętu
    enum hit_kind : uint8_t { hit_normal, hit_crit, hit_dodge };
    struct hit { int8_t x, y; int16_t amount; bool on_hero; uint8_t kind = hit_normal; };   // do liczb obrażeń nad polem

    // Dziennik budowy (log zdarzeń) - krótkie linie UTF-8
    enum log_kind : uint8_t { info, bad, good, loot };   // kolor komunikatu w dzienniku

    struct message
    {
        char s[log_len] = {};        // v0.21.51 cz. 2: całe zerowane (bez śmieci za końcem tekstu - zapis i porównania stanu)
        int n = 0;
        uint8_t kind = info;
        uint8_t repeat = 1;          // ile razy z rzędu ten sam komunikat (x2, x3...)
        message() {}
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

    // Wiersz podsumowania budowy (#33): tekst, dopisek po prawej (dni, usunięte) i kolor (log_kind).
    struct recap_line
    {
        message text;
        message tail;
        uint8_t ink = info;
    };

    // Rzymska liczba rangi (1-5): Respekt, Szkolenia w podsumowaniu.
    inline const char* roman_numeral(int n)
    {
        static const char* r[6] = { "", "I", "II", "III", "IV", "V" };
        return r[n < 0 ? 0 : (n > 5 ? 5 : n)];
    }

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
        int rerolls = 0;             // v0.21.50: Respekt Druga oferta - darmowe losowanie premii po etapie
        int weekly = -1;             // v0.21.50 cz. 4: wyzwanie tygodnia (data::weekly), -1 = zwykła budowa
        int start_coffee = 0;        // v0.21.51 cz. 2: Respekt Zaprawiony w boju - kawy w termosie na start
        int mastery = 0;             // v0.21.52 cz. b: mistrzostwo zawodu (bity mastery_bit): wariant mocy, broń mistrza, premia
                                     // (dawne wyrównanie - stary zapis budowy ma tu 0 = bez mistrzostwa)
    };

    // v0.21.52 cz. b: mistrzostwo zawodu w budowie (run_mods::mastery) - ustawia profil (meta.h: mastery_bits).
    enum mastery_bit : int { mastery_bit_power = 1, mastery_bit_weapon = 2, mastery_bit_boon = 4 };
    // v0.21.52 cz. c: węzeł drzewka Siła rozpędu - +obrażeń pierwszego ciosu w nietknięty problem; wartość w bitach 8-11
    // run_mods::mastery (bez zmiany rozmiaru run_mods i zapisu budowy).
    constexpr int first_hit_shift = 8;
    inline int first_hit_bonus(const run_mods& m) { return (m.mastery >> first_hit_shift) & 15; }

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
            case respect_effect::reroll:        m.rerolls += v; break;
            case respect_effect::veteran:       m.start_coffee += v; break;
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
            case respect_effect::reroll:        return m.add("Premie: +").add(v).add(" darmowe losowanie");
            case respect_effect::veteran:       return m.add("Na start: ").add(v).add(v == 1 ? " kawa" : " kawy").add(" w termosie");
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
            case perk_effect::taken_pct: m.taken_pct += p.value; break;   // v0.21.52 cz. c: Kask ojca
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
            case perk_effect::taken_pct: return m.add("-").add(v).add("% otrzym. obr.");
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
        int flat_boon = 0, pct_boon = 0, crit_boon = 0;   // v0.21.50: premie po etapach (osobno od premii profilu)
        bool vs_enemy = false;
        int enemy_def = 0;                   // obrona problemu
        int enemy_elite = 0, elite = -1;     // v0.21.50: + obrona elity (Tarcza), cecha elity (data::elites)
        int luck = 0, crit_trait = 0, crit_bonus = 0;   // szczęście, cechy Kryt +%, premie (odznaki, Respekt)
        int power = 0, power_rank = 1;       // moc dodaje do ciosu (Seria, Rynna od II, Taran +ranga)
        // v0.21.50 cz. 3: ulepszenie narzędzia (#31) i premia z wydarzenia (#30)
        int upg_level = 0, flat_upgrade = 0, upg_trait = -1;   // poziom, +obrażeń, cecha (data::tool_traits)
        int pierce = 0, steady = 0, crit_upg = 0;              // cecha: -OBR problemu, +najsłabszy rzut, +kryt
        int crit_weapon = 0;                                    // v0.21.51 cz. 2: kryt broni (Poziomica mistrza)
        int flat_event = 0;                                     // wydarzenie: ciosy +N na etap
        bool split = false;                  // źródła premii profilu znane (src_*)
        int src_dmg[mods_sources] = {}, src_pct[mods_sources] = {}, src_crit[mods_sources] = {};
        // wyliczone w finish()
        int stat_value = 0, stat_dmg = 0, flat = 0, def_cut = 0, roll_min = 0;
        int pct_total = 0;                   // procent łącznie (profil + premie po etapach)
        int base_min = 0, base_max = 0;      // przed procentem
        int min = 0, max = 0;                // zakres ciosu
        int avg10 = 0;                       // średni cios (bez kryt) x10
        int crit_base = 0, crit_luck = 0, crit_pct = 0, crit_mult = 1, crit_min = 0, crit_max = 0;

        void finish()
        {
            stat_value = stat_class + stat_craft + stat_trait;
            stat_dmg = stat_value / 2;
            flat = flat_mods + flat_level + flat_found + flat_gear + flat_boon + flat_upgrade + flat_event;
            def_cut = imax(0, enemy_def + enemy_elite - pierce) / 2;
            pct_total = pct + pct_boon;
            roll_min = imin(wmax, wmin + steady);   // Wyważenie: najsłabszy rzut wyżej
            const int add = stat_dmg + flat - def_cut;
            base_min = imax(1, roll_min + add);
            base_max = imax(1, wmax + add);
            min = base_min + pct_floor(base_min, pct_total);
            max = base_max + pct_ceil(base_max, pct_total);
            int sum = 0, n = 0;
            for(int r = roll_min; r <= wmax; ++r, ++n) sum += imax(1, r + add) * (100 + imax(0, pct_total));
            avg10 = n ? div_round(sum, 10 * n) : 0;
            crit_base = data::crit_base_pct;
            crit_luck = data::crit_per_luck_pct * luck;
            crit_pct = crit_base + crit_luck + crit_trait + crit_bonus + crit_boon + crit_upg + crit_weapon;
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
        b.crit_weapon = w.crit + ((m.mastery & mastery_bit_weapon) ? data::mastery_classes[cls].weapon_perk.value : 0);   // broń mistrza
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
    enum class dmg_text : uint8_t { weapon, stat, stat_parts, profile, run, gear, pct, enemy, total, crit, crit_parts, crit_extra, power, boon,
                                    upgrade };
    constexpr int dmg_texts = 15;

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
                m.add(data::weapons[b.weapon].name);
                if(b.upg_level) m.add("+").add(b.upg_level);   // ulepszone narzędzie: "Kielnia+2"
                m.add(" ").add(b.wmin).add("-").add(b.wmax).add(", zasięg ").add(b.range);
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
            {
                if(! b.flat_level && ! b.flat_found && ! b.flat_event) { m.add("Z budowy: brak"); return false; }
                const int sum = b.flat_level + b.flat_found + b.flat_event;
                m.add("Z budowy ").add(sum >= 0 ? "+" : "").add(sum).add(":");
                bool first = true;
                if(b.flat_level) { m.add(" poziom +").add(b.flat_level); first = false; }
                if(b.flat_found) { m.add(first ? "" : ",").add(" projekt ").add(b.flat_found > 0 ? "+" : "").add(b.flat_found); first = false; }
                if(b.flat_event) m.add(first ? "" : ",").add(" wydarzenie ").add(b.flat_event > 0 ? "+" : "").add(b.flat_event);
                return true;
            }
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
                m.add("OBR problemu ").add(b.enemy_def);
                if(b.enemy_elite) m.add("+").add(b.enemy_elite).add(" (elita)");
                if(b.pierce) m.add(" -").add(b.pierce).add(" (przebicie)");
                m.add(": -").add(b.def_cut);
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
                if(! b.crit_trait && ! b.crit_bonus && ! b.crit_upg && ! b.crit_weapon) { m.add("Kryt: bez premii"); return false; }
                m.add("+");
                bool first = true;
                if(b.crit_weapon) { m.add(" broń ").add(b.crit_weapon).add("%"); first = false; }
                if(b.crit_trait) { m.add(" cecha ").add(b.crit_trait).add("%"); first = false; }
                if(b.crit_upg) { m.add(first ? " " : ", ").add("ostrze ").add(b.crit_upg).add("%"); first = false; }
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
            case dmg_text::boon:   // v0.21.50: premie wybrane po etapach (#27)
            {
                if(! b.flat_boon && ! b.pct_boon && ! b.crit_boon) { m.add("Premie etapów: brak"); return false; }
                m.add("Premie etapów:");
                bool first = true;
                if(b.flat_boon) { m.add(" +").add(b.flat_boon); first = false; }
                if(b.pct_boon) { m.add(first ? " +" : ", +").add(b.pct_boon).add("%"); first = false; }
                if(b.crit_boon) m.add(first ? " kryt +" : ", kryt +").add(b.crit_boon).add("%");
                return true;
            }
            case dmg_text::upgrade:   // v0.21.50 cz. 3: ulepszenie narzędzia (#31)
            {
                if(! b.upg_level) { m.add("Ulepszenie: brak"); return false; }
                m.add("Ulepszenie +").add(b.upg_level).add(": +").add(b.flat_upgrade).add(" obr.");
                if(b.upg_trait >= 0) m.add(", ").add(data::tool_traits[b.upg_trait].name).add(" ").add(data::tool_traits[b.upg_trait].short_name);
                return true;
            }
            default: return false;
        }
    }

    // Porównanie przy zmianie broni / sprzętu: "teraz 4-7 -> 5-9 (średnio +1,5)" (short: "śr.").
    inline message& compare_line(message& m, const dmg_breakdown& now, const dmg_breakdown& next, bool short_avg = false)
    {
        add_range(m.add("teraz "), now.min, now.max).add(" -> ");
        add_range(m, next.min, next.max).add(short_avg ? " (śr. " : " (średnio ");
        return add_tenths(m, next.avg10 - now.avg10, true).add(")");
    }
    // "kryt 8-14 (11%) -> 10-18 (16%)"
    inline message& compare_crit(message& m, const dmg_breakdown& now, const dmg_breakdown& next)
    {
        add_range(m.add("kryt "), now.crit_min, now.crit_max).add(" (").add(now.crit_chance()).add("%) -> ");
        return add_range(m, next.crit_min, next.crit_max).add(" (").add(next.crit_chance()).add("%)");
    }
    // Karta problemu: "Zadasz 2-5 (kryt 4-10), on Tobie 1-3" (na wąskim ekranie osobno: versus_hero, versus_enemy).
    inline message& versus_hero(message& m, const dmg_breakdown& b)
    {
        add_range(m.add("Zadasz "), b.min, b.max).add(" (kryt ");
        return add_range(m, b.crit_min, b.crit_max).add(")");
    }
    inline message& versus_enemy(message& m, const hit_range& h) { return add_range(m.add("on Tobie "), h.min, h.max); }
    inline message& versus_line(message& m, const dmg_breakdown& b, const hit_range& h)
    {
        versus_hero(m, b).add(", ");
        return versus_enemy(m, h);
    }

    // ------------------------------------------------------------------ wydarzenia z wyborem (#30): opis skutków
    // Skutek odpowiedzi słowami, np. "-10 zł", "ciosy +2 na etap", "30%: 2x Pleśń obok" (telefon, Godot, dziennik).
    inline message& choice_out_label(message& m, const choice_out& o)
    {
        const int v = o.value;
        if(o.chance < 100) m.add(o.chance).add("%: ");
        switch(o.effect)
        {
            case choice_effect::cash:      return m.add(v > 0 ? "+" : "").add(v).add(" zł");
            case choice_effect::xp:        return m.add("+").add(v).add(" dośw.");
            case choice_effect::hp:        return m.add(v > 0 ? "+" : "").add(v).add(" HP");
            case choice_effect::max_hp:    return m.add(v > 0 ? "+" : "").add(v).add(" max HP");
            case choice_effect::mats:
                if(o.arg >= 0) return m.add(data::materials[o.arg].name).add(v > 0 ? " +" : " ").add(v);
                return m.add("materiały ").add(v > 0 ? "+" : "").add(v);
            case choice_effect::stage_dmg: return m.add("ciosy ").add(v > 0 ? "+" : "").add(v).add(" na etap");
            case choice_effect::stage_def: return m.add("OBR ").add(v > 0 ? "+" : "").add(v).add(" na etap");
            case choice_effect::boon:      return m.add("premia 1 z 3");
            case choice_effect::gear:
                m.add(o.arg >= 0 ? data::gear_slots[o.arg] : "sprzęt");
                if(v > 0) m.add(" (").add(data::gear_rarities[v]).add(")");
                return m;
            case choice_effect::respect:   return m.add("Respekt +").add(v);
            case choice_effect::coffee:    return m.add("kawa +").add(v);
            case choice_effect::spawn:     return m.add(v).add("x ").add(data::enemies[o.arg].name).add(" obok");
            case choice_effect::status:    return m.add(data::statuses[o.arg].name).add(" ").add(v).add(" t.");
            case choice_effect::upgrade:   return m.add("narzędzie +").add(v);
            case choice_effect::power:     return m.add("moc gotowa");
            default:                       return m;
        }
    }
    // Wszystkie skutki odpowiedzi po przecinku ("bez skutków", gdy brak).
    inline message& choice_label(message& m, const event_choice& c)
    {
        if(c.outs == 0) return m.add("bez skutków");
        for(int i = 0; i < c.outs; ++i) { if(i) m.add(", "); choice_out_label(m, c.out[i]); }
        return m;
    }
    // Koszt kolejnego poziomu ulepszenia narzędzia, np. "20 zł + 2 Stal".
    inline message& tool_level_label(message& m, int level, int cash)
    {
        const tool_level_def& t = data::tool_levels[level];
        return m.add(cash).add(" zł + ").add(t.count).add(" ").add(data::materials[t.material].short_name);
    }

    static_assert(data::enemies_count <= max_enemy_types);
    static_assert(data::materials_count <= 4 && data::stages_count <= max_stages && data::all_stages_count <= 64);
    constexpr bool career_routes_fit()   // v0.21.52 cz. d: każdy kontrakt mieści się w tablicach etapów stanu budowy
    {
        for(int i = 0; i < data::career_count; ++i) if(data::career[i].count > max_stages) return false;
        return data::career_count <= 6;   // profil: wyniki 6 kontraktów
    }
    static_assert(career_routes_fit());

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
        int stage = 0;               // 0..route_count()-1 (etap kontraktu; v0.21.52 cz. d)
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
        // v0.21.52 cz. d (#47): kontrakt mapy kariery (data::career) i problemy przeniesione z pierwszej połowy bliźniaka
        // (w miejscu wyrównania przed shot_events - rozmiar stanu bez zmian; stary zapis budowy ma tu 0 = Dom jednorodzinny)
        int8_t contract = 0;
        uint8_t twin_carry = 0;
        uint32_t shot_events = 0;    // bitmaska: którzy wrogowie strzelili w tej turze (warstwa GBA czyta i zeruje)
        // v0.21.49 (część 3): Akt 0 - pierwszy etap budowy (0 z Aktem 0, inaczej za nim), zebrane dokumenty (pieczątki)
        int8_t first_stage = 0;
        uint8_t docs = 0;            // bitmaska zebranych dokumentów (data::documents)
        // v0.21.52 cz. c: zadania dnia (#50) - wezwania brygady i zakupy w Hurtowni w budowie (w miejscu wyrównania przed
        // boons - rozmiar stanu bez zmian; zapis PBRUN14 wczytuje się z zerami tutaj)
        uint8_t helpers_called = 0;
        uint8_t shop_buys = 0;
        // v0.21.50 cz. 2: premie po etapie (#27), kombinacje stanów (#29)
        uint64_t boons = 0;          // wybrane premie (bity data::boons)
        int8_t boon_offer[3] = { -1, -1, -1 };   // oferta po etapie (1 z 3), -1 = brak
        uint8_t boon_rerolls = 0;    // losowania oferty w tej budowie (1 płatne + darmowe z Respektu)
        uint8_t hit_ctx = 0;         // żywioł ciosu z mocy zawodu (bity 1 prąd, 2 iskra) - tylko w trakcie mocy
        uint8_t combo_events = 0;    // bitmaska: kombinacje w tej turze (bit = combo_effect, +8 = na bohaterze; warstwa GBA czyta i zeruje)
        uint8_t boon_salt = 0;       // v0.21.50 cz. 3: oferta premii z wydarzenia (inna niż po etapie)
        // v0.21.50 cz. 3: wydarzenia z wyborem (#30), ulepszanie narzędzia (#31), ukryte pomieszczenia (#32)
        int8_t pending_event = -1;   // wydarzenie czeka na odpowiedź (data::choice_events), -1 = brak
        int8_t stage_choice = -1, stage_choice_pick = -1;   // wydarzenie etapu i wybrana odpowiedź (telefon: Zadania)
        uint8_t choice_done = 0;     // bity skutków ostatniej odpowiedzi, które zaszły (szansa)
        uint16_t events_seen = 0;    // wydarzenia już wylosowane w tej budowie (bez powtórek)
        int8_t event_dmg = 0, event_def = 0;   // z wydarzenia: ciosy / OBR do końca etapu
        int8_t weapon_lvl = 0;       // ulepszenie narzędzia (+1 obrażeń za poziom), przepada przy zmianie narzędzia
        int8_t weapon_trait = -1;    // cecha ulepszenia (data::tool_traits), -1 = brak
        bool trait_pending = false;  // poziom z cechą: czeka na wybór cechy
        int8_t tool_offer = -1, tool_offer_pickup = -1;   // narzędzie na polu czeka na decyzję (ulepszenia by przepadły)
        int8_t secret_x = -1, secret_y = -1;   // ukryte pomieszczenie: pole pękniętej ściany / drzwi (-1 = brak na etapie)
        int8_t secret_kind = 0, secret_dir = 0;   // rodzaj (data::secret_kinds), kierunek od ściany do wnętrza (gust_vec)
        int8_t secret_rx = 0, secret_ry = 0, secret_rw = 0, secret_rh = 0;   // wnętrze magazynu
        bool secret_open = false;
        int8_t key_holder = -1;      // problem z kluczem do magazynu (indeks w enemies), -1 = brak
        uint8_t keys = 0;            // klucze do magazynu (na etap)
        uint8_t secrets_found = 0;   // otwarte magazyny w budowie
        // v0.21.50 cz. 4: podsumowanie budowy (#33) - ostatnie ciosy, najmocniejsze ciosy, oś czasu etapów; wyzwanie tygodnia (#34)
        recap_hit last_hits[recap_hits_n] = {};   // ciosy w bohatera (0 = ostatni)
        recap_hit worst_hit = {};    // najmocniejszy cios w bohatera w tej budowie
        int16_t best_hit = 0;        // najmocniejszy cios bohatera (obrażenia) ...
        int8_t best_hit_def = -1;    // ... w problem (data::enemies)
        bool best_hit_crit = false;
        int8_t blast_src = -1;       // problem, po którym czeka wybuch (źródło ciosu)
        uint8_t stage_kill_log[max_stages] = {};   // usunięte problemy na etapie
        int8_t stage_boon[max_stages] = { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };   // premia wybrana po etapie
        uint8_t stage_event_log[max_stages] = { 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255 };   // wydarzenie * 4 + odpowiedź
        uint8_t stage_flags[max_stages] = {};      // recap_flag
        uint8_t elites_killed = 0;
        uint16_t combos_run = 0;     // kombinacje stanów wywołane przez bohatera
        uint16_t weekly_week = 0;    // numer tygodnia wyzwania (0 = zwykła budowa)
        // v0.21.51 cz. 2: sekretne zlecenia (#39) - liczniki budowy (profil sprawdza je w check_secrets) i nowe zawody
        uint8_t coffee_drunk = 0;    // kawy wypite w budowie (termos, pełny termos, Hurtownia)
        uint8_t shock_combos = 0;    // mokry + prąd wywołane przez bohatera
        uint8_t paper_hits = 0;      // ciosy problemów papierowych (data::secret_paper_mask) w Akcie 0
        uint8_t secret_flags = 0;    // bity secret_flag: boss pokonany ciosem brygady, Akt 0 bez obrażeń od papierów
        uint8_t helper_ctx = 0;      // cios brygady (pompa, pomocnik) - tylko w trakcie ciosu
        int8_t borrow_cls = -1;      // Majster (Złota rączka): zawód, którego moc ma na tym etapie
        int8_t mark_target = -1, mark_turns = 0;   // Geodeta (Tyczenie): oznaczony problem i tury znaku

        // Numer etapu dla gracza (1..) i liczba etapów tej budowy (bez Aktu 0, gdy nieodblokowany).
        enum secret_flag : uint8_t { secret_helper_boss = 1, secret_paper_clean = 2 };

        // v0.21.52 cz. d (#47): kontrakt budowy - etapy stage = 0..route_count()-1 to kolejne etapy kontraktu w data::stages
        // (Dom jednorodzinny: te same indeksy co wcześniej).
        const career_def& kdef() const { return data::career[contract]; }
        int route_count() const { return kdef().count; }       // etapy kontraktu (z Aktem 0)
        int prelude_count() const { return kdef().prelude; }   // etapy Aktu 0 na początku (tylko Dom)
        int stage_id(int s) const { return kdef().first + s; }  // indeks w data::stages
        const stage_def& sdef(int s) const { return data::stages[stage_id(s)]; }
        const stage_def& sdef() const { return sdef(stage); }
        bool last_stage() const { return stage == route_count() - 1; }
        int stage_number() const { return stage - first_stage + 1; }
        int stages_in_run() const { return route_count() - first_stage; }
        const char* act_numeral() const { return data::acts[sdef().act].numeral; }
        // Etap we wzorach (błoto, kałuże, porywy, oferta ścieżek) liczony od Fundamentów: Akt 0 nie zmienia wzorów
        // etapów budowy (Akt 0 ma ujemne numery).
        int pattern_stage() const { return stage - prelude_count(); }

        // ------------------------------------------------------------------ mechanika aktu: błoto, porywy, pył
        const act_def& adef() const { return data::acts[sdef().act]; }
        // Wartość mechaniki aktu; kontrakt może mieć silniejsze porywy (Dom z poddaszem: co 4 tury).
        int mech_value() const { return adef().mechanic == act_mechanic::gust && kdef().gust > 0 ? kdef().gust : adef().mech_value; }
        bool act_is(act_mechanic m) const { return adef().mechanic == m; }
        // Błoto (akt I): stały wzór na podłodze zależny od etapu; wejście kosztuje dodatkową turę. Kładka też na błoto.
        bool mud(int x, int y) const
        {
            if(! act_is(act_mechanic::mud) || lv.at(x, y) != tile::floor || (x * 5 + y * 11 + pattern_stage() * 3) % mech_value() != 0) return false;
            for(int i = 0; i < bridges; ++i) if(cheb(x, y, bridge_x[i], bridge_y[i]) <= bridge_reach()) return false;
            return true;
        }
        // Porywy (akt II): co mech_value tur od wejścia na etap poryw spycha bohatera o pole; kierunek zmienia się co poryw.
        int gust_in() const   // tury do kolejnego porywu (0 = brak porywów w tym akcie)
        {
            if(! act_is(act_mechanic::gust)) return 0;
            int v = mech_value(), t = turns - stage_start_turn;
            return v - t % v;
        }
        int gust_dir() const   // kierunek kolejnego porywu: 0 prawo, 1 dół, 2 lewo, 3 góra
        {
            int t = turns - stage_start_turn + gust_in();
            return (t / imax(1, mech_value()) + pattern_stage()) & 3;
        }
        static constexpr int8_t gust_vec[4][2] = { { 1, 0 }, { 0, 1 }, { -1, 0 }, { 0, -1 } };
        static const char* dir_name(int d) { static const char* n[4] = { "w prawo", "w dół", "w lewo", "w górę" }; return n[d & 3]; }
        void gust_tick()
        {
            int v = mech_value(), t = turns - stage_start_turn;
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
        int dust_sight() const { return act_is(act_mechanic::dust) ? mech_value() : 0; }
        // Pieczątki (Akt 0): na etapie ze schodami leżą dokumenty; dopóki nie zbierzesz wszystkich, schody są zamknięte.
        int docs_needed() const { return act_is(act_mechanic::stamps) && sdef().boss < 0 ? mech_value() : 0; }
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
        bool shop_closed() const { return investor_has(investor_effect::no_shop) || weekly_has(weekly_rule::no_shop); }
        // Wyzwanie tygodnia (#34): zasada włączona / wartość zasady (0, gdy brak).
        bool weekly_has(weekly_rule w) const
        {
            if(bonus.weekly < 0) return false;
            const weekly_def& wd = data::weekly[bonus.weekly];
            for(int i = 0; i < wd.rules_count; ++i) if(wd.rules[i].rule == w) return true;
            return false;
        }
        int weekly_value(weekly_rule w) const
        {
            if(bonus.weekly < 0) return 0;
            const weekly_def& wd = data::weekly[bonus.weekly];
            for(int i = 0; i < wd.rules_count; ++i) if(wd.rules[i].rule == w) return wd.rules[i].value;
            return 0;
        }
        bool weather_is(weather_effect e) const { return data::weather[weather].effect == e; }

        // Pogoda dnia: losowanie wagami spośród dozwolonych na etapie s (bad_only: tylko niekorzystne, jeśli są).
        bool weather_allowed(int i, int s, bool bad_only) const
        {
            return ((data::weather[i].stages >> stage_id(s)) & 1) && (! bad_only || data::weather[i].bad);
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
        int8_t hero_status[status_slots] = {};  // tury aktywnych stanów bohatera (indeks = status_effect)

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
            if((s == status_effect::poison || s == status_effect::shock) && synergy_on(synergy_effect::safety))   // synergia Pełne BHP
            {
                push(message().add("Pełne BHP: bez stanu").as(good));
                return;
            }
            if(s != status_effect::wet && boon_sum(boon_effect::status_res) > 0)   // Instrukcja BHP: stany krócej
            {
                t -= boon_sum(boon_effect::status_res);
                if(t <= 0) { push(message().add("Instrukcja BHP: bez stanu").as(good)); return; }
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
        // Zawód, którego moc działa (Majster: pożyczony na ten etap) - moc, ikona, odnowienie, premia do ciosu.
        int power_cls() const { return cdef().ability == ability_effect::borrow && borrow_cls >= 0 ? borrow_cls : cls; }
        const class_def& pdef() const { return data::classes[power_cls()]; }
        // Geodeta (Tyczenie): cios w oznaczony problem +1 + ranga mocy (+ premia zawodu).
        int mark_bonus(int ei) const { return ei == mark_target && mark_turns > 0 ? 1 + ability_rank() + boon_power() : 0; }
        // Dni budowy jak w harmonogramie domu, bez Aktu 0 (sekretne zlecenie Szybka ekipa).
        int build_days() const
        {
            int t = 0;
            for(int s = imax(first_stage, prelude_count()); s < route_count(); ++s)
                t += data::schedule_min_days + stage_days[s] / data::schedule_turns_per_day;
            return t;
        }
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
        int luck() const { return cdef().luck + bonus.luck + trait_bonus(trait_effect::luck) + boon_luck(); }
        int crit_pct() const
        {
            return data::crit_base_pct + data::crit_per_luck_pct * luck() + trait_bonus(trait_effect::crit) + bonus.crit + boon_sum(boon_effect::crit)
                   + tool_trait_value(tool_trait_effect::crit) + weapon().crit + master_crit();
        }
        int sight_radius() const   // pył (akt III)
        {
            return imax(3, fov_radius + trait_bonus(trait_effect::sight) + bonus.sight + boon_sum(boon_effect::sight) - dust_sight());
        }
        int thermos_cap() const { return data::thermos_capacity + bonus.thermos + gear_bonus(gear_stat::thermos) + boon_sum(boon_effect::thermos); }

        // ------------------------------------------------------------------ premie po etapie (#27)
        bool has_boon(int b) const { return (boons >> b) & 1u; }
        int boons_owned() const { int n = 0; for(int b = 0; b < data::boons_count; ++b) n += has_boon(b); return n; }
        // Suma wartości wybranych premii danego rodzaju.
        int boon_sum(boon_effect e) const
        {
            int v = 0;
            for(int b = 0; b < data::boons_count && (boons >> b) != 0; ++b)
                if(has_boon(b) && data::boons[b].effect == e) v += data::boons[b].value;
            return v;
        }
        // Ile premii ze zbioru m (bity data::boons) ma znacznik t (data::boon_tags).
        static int tag_count_in(uint64_t m, int t)
        {
            int n = 0;
            for(int b = 0; b < data::boons_count && (m >> b) != 0; ++b) n += ((m >> b) & 1u) && ((data::boons[b].tags >> t) & 1);
            return n;
        }
        int tag_count(int t) const { return tag_count_in(boons, t); }
        // Synergia s: jeden znacznik - 2+ premie z nim; dwa znaczniki - po jednej z każdego (razem 2+).
        static bool synergy_active_in(uint64_t m, int s)
        {
            const uint16_t tg = data::synergies[s].tags;
            int any = 0, tags = 0;
            for(int t = 0; t < data::boon_tags_count; ++t)
            {
                if(! ((tg >> t) & 1)) continue;
                ++tags;
                if(tag_count_in(m, t) == 0) return false;
            }
            for(int b = 0; b < data::boons_count && (m >> b) != 0; ++b) any += ((m >> b) & 1u) && (data::boons[b].tags & tg) != 0;
            return tags > 0 && any >= data::synergy_at;
        }
        bool synergy_active(int s) const { return synergy_active_in(boons, s); }
        static uint16_t synergy_mask_in(uint64_t m)
        {
            uint16_t r = 0;
            for(int s = 0; s < data::synergies_count; ++s) if(synergy_active_in(m, s)) r = uint16_t(r | (1u << s));
            return r;
        }
        uint16_t synergy_mask() const { return synergy_mask_in(boons); }
        // Wartość aktywnych synergii danego rodzaju (0 = żadna).
        int synergy_value(synergy_effect e) const
        {
            int v = 0;
            for(int s = 0; s < data::synergies_count; ++s) if(data::synergies[s].effect == e && synergy_active(s)) v += data::synergies[s].value;
            return v;
        }
        bool synergy_on(synergy_effect e) const
        {
            for(int s = 0; s < data::synergies_count; ++s) if(data::synergies[s].effect == e && synergy_active(s)) return true;
            return false;
        }
        int boon_luck() const { return boon_sum(boon_effect::luck) + synergy_value(synergy_effect::luck); }
        // Obrona z premii: Beton B30 i podobne + Zbrojenie (+1 za każdą premię Beton).
        int boon_defense() const
        {
            int v = boon_sum(boon_effect::def);
            for(int s = 0; s < data::synergies_count; ++s)
                if(data::synergies[s].effect == synergy_effect::armor && synergy_active(s))
                    for(int t = 0; t < data::boon_tags_count; ++t) if((data::synergies[s].tags >> t) & 1) v += data::synergies[s].value * tag_count(t);
            return v;
        }
        int boon_power() const { return boon_sum(boon_effect::power) + mastery_power(); }   // premia zawodu: wzmocnienie mocy (opis w danych)
        // v0.21.52 cz. b: wariant mocy z mistrzostwa zawodu (siła i tury odnowienia z danych; Majster - moc pożyczona)
        int mastery_power() const { return (bonus.mastery & mastery_bit_power) ? data::mastery_classes[cls].power : 0; }
        int mastery_cooldown() const { return (bonus.mastery & mastery_bit_power) ? data::mastery_classes[cls].cooldown : 0; }
        bool master_weapon() const { return bonus.mastery & mastery_bit_weapon; }   // broń mistrza: złoty błysk przy krycie (warstwa GBA / Godot)
        // Broń mistrza: kryt +% tylko z bronią zawodu (podniesione narzędzie jej nie ma).
        int master_crit() const { return master_weapon() && weapon_override < 0 ? data::mastery_classes[cls].weapon_perk.value : 0; }

        bool has_boon_offer() const { return boon_offer[0] >= 0; }
        // Losowania oferty: 1 płatne na budowę + darmowe z Respektu (Druga oferta, zużywane najpierw).
        int rerolls_left() const { return imax(0, 1 + bonus.rerolls - boon_rerolls); }
        int reroll_price() const { return boon_rerolls < bonus.rerolls ? 0 : data::boon_reroll_cost; }
        bool can_reroll() const { return has_boon_offer() && rerolls_left() > 0 && cash >= reroll_price(); }
        bool boon_available(int b) const
        {
            if(has_boon(b) || (data::boons[b].cls >= 0 && data::boons[b].cls != cls)) return false;
            if(data::boons[b].mastery && ! (bonus.mastery & mastery_bit_boon)) return false;   // v0.21.52 cz. b: premia mistrzostwa zawodu
            for(int k = 0; k < 3; ++k) if(boon_offer[k] == b) return false;
            return true;
        }
        // Wagi rzadkości: szczęście przesuwa trochę z zwykłych na rzadkie i legendarne.
        int boon_weight(int rarity) const
        {
            const int l = imax(0, luck());
            if(rarity == 1) return data::boon_rarities[1].weight + l * data::boon_luck_rare;
            if(rarity == 2) return data::boon_rarities[2].weight + l * data::boon_luck_legend;
            return imax(10, data::boon_rarities[0].weight - l * (data::boon_luck_rare + data::boon_luck_legend));
        }
        // Oferta 1 z 3 po etapie: osobny generator z seeda budowy, etapu i losowania (bez wpływu na RNG gry, ta sama dla seeda).
        void roll_boons(int salt = 0)   // salt: oferta z wydarzenia (#30) inna niż po etapie
        {
            for(auto& o : boon_offer) o = -1;
            boon_salt = uint8_t(salt);
            rng br;
            br.seed((run_seed ^ (uint32_t(stage + 1 + tier * 16) * 2654435761u) ^ (uint32_t(boon_rerolls + 1 + boon_salt) * 40503u)) * 2246822519u);
            for(int k = 0; k < 3; ++k)
            {
                int total = boon_weight(0) + boon_weight(1) + boon_weight(2), roll = br.range(1, total), rar = 0;
                while(rar < 2 && roll > boon_weight(rar)) roll -= boon_weight(rar++);
                static constexpr int8_t order[3][3] = { { 0, 1, 2 }, { 1, 0, 2 }, { 2, 1, 0 } };   // brak w rzadkości: najpierw niższa
                for(int o = 0; o < 3 && boon_offer[k] < 0; ++o)
                {
                    int want = order[rar][o], n = 0;
                    for(int b = 0; b < data::boons_count; ++b) n += boon_available(b) && data::boons[b].rarity == want;
                    if(n == 0) continue;
                    int pick = br.range(0, n - 1);
                    for(int b = 0; b < data::boons_count; ++b)
                        if(boon_available(b) && data::boons[b].rarity == want && pick-- == 0) { boon_offer[k] = int8_t(b); break; }
                }
            }
        }
        // Wybór premii k z oferty: skutki natychmiastowe (max HP, budżet, materiały, termos) od razu; nowa synergia - baner.
        bool pick_boon(int k)
        {
            if(k < 0 || k > 2 || boon_offer[k] < 0) return false;
            const int b = boon_offer[k];
            const boon_def& bd = data::boons[b];
            const uint16_t before = synergy_mask();
            boons |= uint64_t(1) << b;
            if(st == status::stage_clear) stage_boon[stage] = int8_t(b);   // podsumowanie: premia po etapie
            else stage_flags[stage] = uint8_t(stage_flags[stage] | recap_event_boon);
            for(auto& o : boon_offer) o = -1;
            switch(bd.effect)
            {
                case boon_effect::max_hp:  hero.max_hp = int16_t(hero.max_hp + bd.value); hero.hp = int16_t(hero.hp + bd.value); break;
                case boon_effect::cash:    cash += income(bd.value); break;
                case boon_effect::mats:    for(int m = 0; m < data::materials_count; ++m) add_material(m, bd.value); break;
                case boon_effect::thermos: thermos = imin(thermos_cap(), thermos + 1); break;
                case boon_effect::sight:   update_fov(); break;
                default: break;
            }
            push(message().add("Premia: ").add(bd.name).as(loot));
            const uint16_t now = synergy_mask();
            for(int s = 0; s < data::synergies_count; ++s)
                if(((now >> s) & 1) && ! ((before >> s) & 1))
                {
                    push(message().add("Synergia: ").add(data::synergies[s].name).add("!").as(good));
                    stage_flags[stage] = uint8_t(stage_flags[stage] | recap_synergy);
                }
            return true;
        }
        bool reroll_boons()
        {
            if(! can_reroll()) return false;
            cash -= reroll_price();
            ++boon_rerolls;
            roll_boons(boon_salt);
            push(message().add("Nowa oferta premii"));
            return true;
        }
        void skip_boons() { for(auto& o : boon_offer) o = -1; }
        // Wybór bota (testy balansu, test złoty): prosta kolejność skutków, rzadkość, nowa synergia.
        int boon_priority(int b) const
        {
            static constexpr int8_t prio[] = { 60, 58, 40, 50, 55, 28, 30, 25, 35, 8, 5, 38, 15, 15, 18, 18, 3, 30, 30, 20, 45, 5, 3, 10 };
            static_assert(sizeof prio == int(boon_effect::sight) + 1);
            const boon_def& bd = data::boons[b];
            int v = bd.rarity * 100 + prio[int(bd.effect)];
            if(synergy_mask_in(boons | (uint64_t(1) << b)) != synergy_mask()) v += 20;   // włączy synergię
            return v;
        }
        int bot_boon_choice() const
        {
            int best = -1, bv = -1;
            for(int k = 0; k < 3; ++k) if(boon_offer[k] >= 0 && boon_priority(boon_offer[k]) > bv) { bv = boon_priority(boon_offer[k]); best = k; }
            return best;
        }
        // Bot losuje ofertę jeszcze raz tylko za darmo (Druga oferta) i gdy same zwykłe premie.
        bool bot_wants_reroll() const
        {
            return has_boon_offer() && reroll_price() == 0 && can_reroll() && boon_priority(boon_offer[bot_boon_choice()]) < 100;
        }

        // ------------------------------------------------------------------ elity (#28)
        bool is_elite(int ei) const { return enemies[ei].elite >= 0; }
        bool elite_is(const actor& e, elite_effect x) const { return e.elite >= 0 && data::elites[e.elite].effect == x; }
        int elite_chance() const
        {
            const int c = imax(0, data::elite_act_pct[sdef().act] + data::elite_diff_pct[diff] + tier * data::elite_tier_pct);
            return weekly_has(weekly_rule::elite_pct) ? c * weekly_value(weekly_rule::elite_pct) / 100 : c;   // wyzwanie: Elity x2
        }
        void make_elite(int i, int trait)
        {
            actor& a = enemies[i];
            a.elite = int8_t(trait);
            a.hp = a.max_hp = int16_t(imax(1, a.max_hp * data::elite_hp_pct / 100));
        }
        // Obrona problemu ei: z danych + Tarcza elity.
        int enemy_elite_def(int ei) const { return elite_is(enemies[ei], elite_effect::shield) ? data::elites[enemies[ei].elite].value : 0; }
        int enemy_defense(int ei) const { return data::enemies[enemies[ei].def_id].defense + enemy_elite_def(ei); }
        // Nazwa problemu z przedrostkiem elity ("Zbrojony Przeciek", "Uparta Pleśń").
        message& enemy_name(message& m, int ei) const
        {
            const actor& e = enemies[ei];
            const enemy_def& ed = data::enemies[e.def_id];
            if(e.elite >= 0) m.add(data::elites[e.elite].prefix[ed.gender]).add(" ");
            return m.add(ed.name);
        }

        // ------------------------------------------------------------------ podsumowanie budowy (#33)
        // Cios w bohatera do podsumowania: ostatnie recap_hits_n (0 = ostatni) i najmocniejszy w budowie.
        void log_hit(int src, int elite, recap_kind k, int amount)
        {
            for(int i = recap_hits_n - 1; i > 0; --i) last_hits[i] = last_hits[i - 1];
            recap_hit& h = last_hits[0];
            h.src = int8_t(src); h.elite = int8_t(elite); h.kind = uint8_t(k); h.stage = int8_t(stage); h.amount = int16_t(imin(32767, amount));
            if(h.amount > worst_hit.amount) worst_hit = h;
            if(stage < prelude_count() && src >= 0 && ((data::secret_paper_mask >> src) & 1) && paper_hits < 255) ++paper_hits;
        }
        void note_combo()
        {
            if(combos_run < 65535) ++combos_run;
            stage_flags[stage] = uint8_t(stage_flags[stage] | recap_combo);
        }
        void clear_timeline()
        {
            for(int s = 0; s < max_stages; ++s) { stage_kill_log[s] = 0; stage_boon[s] = -1; stage_event_log[s] = 255; stage_flags[s] = 0; }
        }
        // Źródło ciosu z przedrostkiem elity ("Zbrojony Przeciek").
        static message& recap_src(message& m, const recap_hit& h)
        {
            if(h.src < 0) return m.add("Wybuch");
            const enemy_def& ed = data::enemies[h.src];
            if(h.elite >= 0) m.add(data::elites[h.elite].prefix[ed.gender]).add(" ");
            return m.add(ed.name);
        }
        // Rodzaj ciosu słowami (cios bossa: nazwa uderzenia, np. "Kontrola BHP").
        static message& recap_kind_name(message& m, const recap_hit& h)
        {
            if(h.kind == uint8_t(recap_kind::slam) && h.src >= 0 && data::enemies[h.src].slam_name[0]) return m.add(data::enemies[h.src].slam_name);
            return m.add(data::recap_kind_names[h.kind < recap_kinds ? h.kind : 0]);
        }
        // Cios w bohatera: "Zbrojony Przeciek: -4 (cios)".
        static message& recap_hit_line(message& m, const recap_hit& h)
        {
            recap_src(m, h).add(": -").add(h.amount).add(" (");
            recap_kind_name(m, h);
            return m.add(")");
        }
        // "Pokonał Cię: Zbrojony Przeciek" (czasownik wg rodzaju nazwy problemu).
        message& recap_killer(message& m) const
        {
            const recap_hit& h = last_hits[0];
            if(h.src < 0 && h.amount == 0) return m.add("Budowa wstrzymana");
            m.add(data::recap_verbs[h.src >= 0 ? data::enemies[h.src].gender : 0]).add(" Cię: ");
            return recap_src(m, h);
        }
        // "3/10, Akt I" (etap budowy i akt)
        message& recap_where(message& m) const
        {
            return m.add(stage_number()).add("/").add(stages_in_run()).add(", Akt ").add(act_numeral());
        }
        // Dni etapu jak w harmonogramie domu (min. + tury / tury na dzień); etap w toku - do teraz.
        bool recap_current(int s) const { return s == stage && (st == status::dead || st == status::playing); }
        int recap_days(int s) const
        {
            return data::schedule_min_days + (recap_current(s) ? turns - stage_start_turn : stage_days[s]) / data::schedule_turns_per_day;
        }
        int recap_kills(int s) const { return recap_current(s) ? stage_kills : stage_kill_log[s]; }
        // Oś czasu: etap (numer, nazwa; dni i usunięte po prawej), pod nim SMS, co się działo, premia; koniec budowy.
        int recap_timeline(recap_line* out, int max) const
        {
            int n = 0;
            auto add = [&](const recap_line& l) { if(n < max) out[n++] = l; };
            static const char* flag_names[7] = { "magazyn", "ulepszenie", "elita", "boss pokonany", "kombinacje", "synergia", "premia z SMS" };
            for(int s = first_stage; s <= stage && s < route_count(); ++s)
            {
                const bool dead_here = recap_current(s) && st == status::dead;
                recap_line l;
                l.text.add(s - first_stage + 1).add(". ").add(sdef(s).name);
                l.tail.add(recap_days(s)).add(" d., ").add(recap_kills(s)).add(" usun.");
                l.ink = uint8_t(dead_here ? bad : info);
                add(l);
                if(stage_event_log[s] != 255)
                {
                    recap_line e; e.text.add("  SMS: ").add(data::choice_events[stage_event_log[s] / 4].name); e.ink = good; add(e);
                }
                recap_line f; int items = 0;
                for(int b = 0; b < 7; ++b)
                {
                    if(! ((stage_flags[s] >> b) & 1)) continue;
                    if(items == 3) { add(f); f = recap_line(); items = 0; }
                    f.text.add(items ? ", " : "  + ").add(flag_names[b]); f.ink = good; ++items;
                }
                if(items) add(f);
                if(stage_boon[s] >= 0) { recap_line b; b.text.add("  Premia: ").add(data::boons[stage_boon[s]].name); b.ink = loot; add(b); }
                if(dead_here) { recap_line d; d.text.add("  Tu stanęła budowa"); d.ink = bad; add(d); }
            }
            return n;
        }

        // ------------------------------------------------------------------ kombinacje stanów (#29)
        bool enemy_wet(int ei) const { const actor& e = enemies[ei]; return e.wet > 0 || data::enemies[e.def_id].elem == element::water; }
        bool enemy_dusty(int ei) const { return enemies[ei].flags & actor_dusty; }
        bool enemy_frozen(int ei) const { return enemies[ei].flags & actor_frozen; }
        bool hero_wet() const { return hero_status[int(status_effect::wet)] > 0; }
        // Bohater mokry (kałuża, cios wody): komunikat tylko, gdy był suchy.
        void soak_hero()
        {
            if(hero_wet()) hero_status[int(status_effect::wet)] = int8_t(imax(hero_status[int(status_effect::wet)], data::hero_wet_turns));
            else apply_status(status_effect::wet, data::hero_wet_turns);
        }
        // Żywioł ciosu bohatera: broń (Próbnik - prąd, Szlifierka - iskra), premie, synergia Przepięcie, moc zawodu.
        bool hit_power() const
        {
            return weapon().elem == element::power || (hit_ctx & 1) || boon_sum(boon_effect::electric) > 0 || synergy_on(synergy_effect::conduct);
        }
        bool hit_spark() const { return weapon().elem == element::spark || (hit_ctx & 2) || boon_sum(boon_effect::spark) > 0; }
        // Mokry + prąd: porażenie celu i mokrych problemów obok (Przepięcie: dalej).
        void combo_shock(int ei, int x, int y)
        {
            const combo_def& c = data::combos[int(combo_effect::shock_area)];
            const int rad = c.radius + synergy_value(synergy_effect::conduct);
            combo_events = uint8_t(combo_events | (1u << int(combo_effect::shock_area)));
            note_combo();
            if(shock_combos < 255) ++shock_combos;   // sekretne zlecenie Mokra robota
            push(message().add(c.short_name).add(" ").add(c.name).as(good));
            for(int i = 0; i < enemies_count && st == status::playing; ++i)
            {
                const actor& e = enemies[i];
                if(e.alive && (i == ei || (enemy_wet(i) && cheb(x, y, e.x, e.y) <= rad))) damage_enemy(i, c.value, false, c.name);
            }
        }
        // Pył + iskra: wybuch pyłu wokół zapylonego celu (pył znika z trafionych).
        void combo_dust(int x, int y)
        {
            const combo_def& c = data::combos[int(combo_effect::dust_blast)];
            const int sp = synergy_value(synergy_effect::sparks), rad = c.radius + (sp > 0 ? 1 : 0), dmg = c.value + sp;
            combo_events = uint8_t(combo_events | (1u << int(combo_effect::dust_blast)));
            note_combo();
            push(message().add(c.short_name).add(" ").add(c.name).as(good));
            for(int i = 0; i < enemies_count; ++i)
                if(cheb(x, y, enemies[i].x, enemies[i].y) <= rad) enemies[i].flags = uint8_t(enemies[i].flags & ~actor_dusty);
            for(int i = 0; i < enemies_count && st == status::playing; ++i)
                if(enemies[i].alive && cheb(x, y, enemies[i].x, enemies[i].y) <= rad) damage_enemy(i, dmg, false, c.name);
            blast_secret(x, y, rad);
        }
        // Zamróz + uderzenie: cios wręcz w zmrożony pęka go (+value% ciosu, osobno).
        void combo_crack(int ei, int dmg)
        {
            const combo_def& c = data::combos[int(combo_effect::crack)];
            enemies[ei].flags = uint8_t(enemies[ei].flags & ~actor_frozen);
            combo_events = uint8_t(combo_events | (1u << int(combo_effect::crack)));
            note_combo();
            push(message().add(c.short_name).add(" ").add(c.name).as(good));
            damage_enemy(ei, imax(1, dmg * c.value / 100), false, c.name);
        }

        // ------------------------------------------------------------------ v0.21.50 cz. 3: wspólny generator etapu
        // Osobny generator z seeda budowy, etapu i soli (wydarzenia, magazyn) - bez wpływu na RNG gry, ten sam dla seeda.
        rng side_rng(uint32_t salt) const
        {
            rng s;
            s.seed((run_seed ^ (uint32_t(stage + 1 + tier * 16) * 2654435761u) ^ (salt * 40503u)) * 2246822519u);
            return s;
        }
        // Wolne pole podłogi w losowym pokoju (bez pierwszego): bez postaci, znajdziek i schodów.
        bool side_free_cell(rng& sr, int& ox, int& oy) const
        {
            for(int t = 0; t < 60; ++t)
            {
                const room& rm = lv.rooms[sr.range(1, lv.rooms_count - 1)];
                int x = sr.range(rm.x, rm.x + rm.w - 1), y = sr.range(rm.y, rm.y + rm.h - 1);
                if(lv.at(x, y) == tile::floor && ! occupied(x, y) && ! pickup_at(x, y)) { ox = x; oy = y; return true; }
            }
            return false;
        }

        // ------------------------------------------------------------------ wydarzenia z wyborem (#30)
        // 0-1 pole wydarzenia na etapie (nie pierwszym budowy i nie z bossem); wydarzenie bez powtórek w budowie.
        void place_event()
        {
            if(sdef().boss >= 0 || stage == first_stage || lv.rooms_count < 2 || pickups_count >= max_pickups) return;
            rng er = side_rng(101);
            if(er.range(1, 100) > data::choice_event_chance_pct) return;
            int n = 0;
            for(int e = 0; e < data::choice_events_count; ++e) n += ! ((events_seen >> e) & 1);
            if(n == 0) return;
            int k = er.range(0, n - 1), ev = 0;
            for(int e = 0; e < data::choice_events_count; ++e) if(! ((events_seen >> e) & 1) && k-- == 0) { ev = e; break; }
            int x, y;
            if(! side_free_cell(er, x, y)) return;
            events_seen = uint16_t(events_seen | (1u << ev));
            pickups[pickups_count++] = { int8_t(x), int8_t(y), uint8_t(event_tile), true, uint8_t(ev) };
        }
        const choice_event_def& pending_def() const { return data::choice_events[pending_event]; }
        // Odpowiedź k na wydarzenie: skutki po kolei (z szansą - osobny generator); premia 1 z 3 - oferta czeka na wybór,
        // ulepszenie z cechą - wybór cechy (warstwa gry pokazuje oba ekrany zaraz po odpowiedzi).
        bool choose_event(int k)
        {
            if(pending_event < 0) return false;
            const choice_event_def& ev = pending_def();
            if(k < 0 || k >= ev.choices_count) return false;
            const event_choice& c = ev.choices[k];
            rng er = side_rng(111 + uint32_t(k));
            stage_event_log[stage] = uint8_t(pending_event * 4 + k);   // podsumowanie: wydarzenie i odpowiedź
            stage_choice = pending_event; stage_choice_pick = int8_t(k); pending_event = -1; choice_done = 0;
            push(message().add("Odpowiedź: ").add(c.label));
            for(int i = 0; i < c.outs; ++i)
            {
                const choice_out& o = c.out[i];
                if(o.chance < 100 && er.range(1, 100) > o.chance) continue;
                choice_done = uint8_t(choice_done | (1u << i));
                apply_choice(o);
            }
            if(c.result[0]) push(message().add(c.result).as(good));
            return true;
        }
        void apply_choice(const choice_out& o)
        {
            const int v = o.value;
            switch(o.effect)
            {
                case choice_effect::cash:
                    cash = imax(0, cash + (v > 0 ? income(v) : v));
                    push(message().add(v > 0 ? "Budżet +" : "Budżet ").add(v > 0 ? income(v) : v).add(" zł").as(v > 0 ? good : bad));
                    break;
                case choice_effect::xp: gain_xp(v); push(message().add("+").add(v).add(" dośw.").as(good)); break;
                case choice_effect::hp:
                    hero.hp = int16_t(v > 0 ? imin(hero.max_hp, hero.hp + v) : imax(1, hero.hp + v));
                    push(message().add(v > 0 ? "+" : "").add(v).add(" HP").as(v > 0 ? good : bad));
                    break;
                case choice_effect::max_hp:
                    hero.max_hp = int16_t(imax(1, hero.max_hp + v)); hero.hp = int16_t(imin(hero.max_hp, imax(1, hero.hp + v)));
                    break;
                case choice_effect::mats:
                    for(int m = 0; m < data::materials_count; ++m) if(o.arg < 0 || o.arg == m) add_material(m, v);
                    break;
                case choice_effect::stage_dmg: event_dmg = int8_t(event_dmg + v); break;
                case choice_effect::stage_def: event_def = int8_t(event_def + v); break;
                case choice_effect::boon: roll_boons(50); break;
                case choice_effect::gear:
                {
                    const int slot = o.arg >= 0 ? o.arg : random_slot();
                    take_gear(slot, imax(v, 0), r.range(0, data::gear_traits_count - 1));
                    break;
                }
                case choice_effect::respect: respect += v; push(message().add("Respekt +").add(v).as(loot)); break;
                case choice_effect::coffee: thermos = imin(thermos_cap(), thermos + v); push(message().add("Kawa do termosu (").add(thermos).add("/").add(thermos_cap()).add(")").as(good)); break;
                case choice_effect::spawn:
                    for(int k = 0; k < v; ++k)
                    {
                        int x, y, slot;
                        if(! free_around(hero.x, hero.y, hero.x, hero.y, x, y) || (slot = free_slot()) < 0) break;
                        actor& a = enemies[slot];
                        a = actor();
                        a.x = int8_t(x); a.y = int8_t(y); a.def_id = o.arg;
                        a.hp = a.max_hp = int16_t(imax(1, data::enemies[o.arg].max_health * enemy_hp_pct() / 100));
                        a.alive = true; a.awake = true; a.stun = 1;
                        if(dust_sight() > 0) a.flags = uint8_t(a.flags | actor_dusty);
                        push(message().add(data::enemies[o.arg].name).add(" wyłazi!").as(bad));
                    }
                    break;
                case choice_effect::status: apply_status(status_effect(o.arg), v); break;
                case choice_effect::upgrade: for(int k = 0; k < v && can_upgrade_weapon(); ++k) upgrade_weapon(); break;
                case choice_effect::power:
                    ability_cd = 0;
                    push(message().add("Moc gotowa: ").add(pdef().ability_name).as(good));
                    break;
                default: break;
            }
        }
        // Wybór bota (testy balansu, test złoty): prosta ocena skutków (wartość x szansa), remis - pierwsza odpowiedź.
        int bot_event_choice() const
        {
            const choice_event_def& ev = pending_def();
            int best = 0, bv = -1000000;
            for(int k = 0; k < ev.choices_count; ++k)
            {
                int v = 0;
                for(int i = 0; i < ev.choices[k].outs; ++i)
                {
                    const choice_out& o = ev.choices[k].out[i];
                    int w = 0;
                    switch(o.effect)
                    {
                        case choice_effect::cash:      w = o.value; break;
                        case choice_effect::xp:        w = o.value; break;
                        case choice_effect::hp:        w = o.value * (hero.hp * 2 < hero.max_hp ? 6 : 3); break;
                        case choice_effect::max_hp:    w = o.value * 5; break;
                        case choice_effect::mats:      w = o.value * (o.arg < 0 ? 9 : 3); break;
                        case choice_effect::stage_dmg: w = o.value * 12; break;
                        case choice_effect::stage_def: w = o.value * 10; break;
                        case choice_effect::boon:      w = 25; break;
                        case choice_effect::gear:      w = 15; break;
                        case choice_effect::respect:   w = o.value * 6; break;
                        case choice_effect::coffee:    w = o.value * 8; break;
                        case choice_effect::spawn:     w = -10 * o.value; break;
                        case choice_effect::status:    w = -8; break;
                        case choice_effect::upgrade:   w = can_upgrade_weapon() ? 20 : 0; break;
                        case choice_effect::power:     w = 4; break;
                        default: break;
                    }
                    v += w * o.chance;
                }
                if(v > bv) { bv = v; best = k; }
            }
            return best;
        }

        // ------------------------------------------------------------------ ulepszanie narzędzia (#31)
        bool can_upgrade_weapon() const { return weapon_lvl < data::tool_upgrade_max && ! trait_pending; }
        // Cena kolejnego poziomu w zł (Rabat z Respektu i premie jak w Hurtowni) i czy stać (zł + materiał).
        int upgrade_price() const
        {
            return data::tool_levels[imin(weapon_lvl, data::tool_upgrade_max - 1)].cash * (100 - imin(90, bonus.shop_pct + boon_sum(boon_effect::shop_pct))) / 100;
        }
        bool upgrade_affordable() const
        {
            if(! can_upgrade_weapon()) return false;
            const tool_level_def& t = data::tool_levels[weapon_lvl];
            return cash >= upgrade_price() && mats[t.material] >= t.count;
        }
        // +1 poziom (za darmo - płaci Hurtownia albo wydarzenie); na poziomie trait_at czeka wybór cechy.
        void upgrade_weapon()
        {
            if(weapon_lvl >= data::tool_upgrade_max) return;
            ++weapon_lvl;
            stage_flags[stage] = uint8_t(stage_flags[stage] | recap_upgrade);
            if(weapon_lvl >= data::tool_trait_at && weapon_trait < 0) trait_pending = true;
            push(message().add("Ulepszenie: ").add(weapon().name).add("+").add(weapon_lvl).as(loot));
        }
        bool choose_trait(int t)
        {
            if(! trait_pending || t < 0 || t >= data::tool_traits_count) return false;
            trait_pending = false;
            weapon_trait = int8_t(t);
            push(message().add("Cecha narzędzia: ").add(data::tool_traits[t].name).as(loot));
            return true;
        }
        int bot_trait_choice() const { return 0; }   // bot: zawsze pierwsza cecha (Przebicie)
        int upgrade_dmg() const { return weapon_lvl * data::tool_upgrade_dmg; }
        int tool_trait_value(tool_trait_effect e) const
        {
            return weapon_trait >= 0 && data::tool_traits[weapon_trait].effect == e ? data::tool_traits[weapon_trait].value : 0;
        }
        void reset_upgrade() { weapon_lvl = 0; weapon_trait = -1; trait_pending = false; }
        // Nazwa narzędzia z poziomem ulepszenia ("Kielnia+2").
        message& weapon_title(message& m) const
        {
            m.add(weapon().name);
            if(weapon_lvl > 0) m.add("+").add(weapon_lvl);
            return m;
        }
        // Narzędzie na polu przy ulepszonym: decyzja gracza (ulepszenia przepadną). Nie zużywa tury.
        bool has_tool_offer() const { return tool_offer >= 0; }
        void accept_tool()
        {
            if(! has_tool_offer()) return;
            pickups[tool_offer_pickup].active = false;
            const int t = tool_offer;
            tool_offer = tool_offer_pickup = -1;
            take_tool(t);
        }
        void decline_tool()
        {
            if(! has_tool_offer()) return;
            tool_offer = tool_offer_pickup = -1;
            push(message().add("Zostajesz przy ulepszonym narzędziu"));
        }
        void take_tool(int t)
        {
            reset_upgrade();
            weapon_override = data::tools[t].weapon;
            tools_found = uint8_t(tools_found | (1u << t));
            push(message().add("Narzędzie: ").add(weapon().name).add(" ").add(weapon().min_damage).add("-").add(weapon().max_damage).as(loot));
        }
        // Bot bierze nowe narzędzie, gdy średni cios (bez problemu) jest wyższy niż ulepszonym obecnym.
        bool bot_tool_accept() const
        {
            return weapon_breakdown(-1, data::tools[tool_offer].weapon).avg10 > weapon_breakdown().avg10;
        }

        // ------------------------------------------------------------------ ukryte pomieszczenia (#32)
        // Magazyn 3x3 za ścianą przy krawędzi pokoju: ściana E graniczy z podłogą pokoju, wnętrze i jego obrys to same
        // mury (poza E), więc bez otwarcia nie ma do niego drogi ani widoku. Klucz ma problem (najpierw elita), czasem
        // w środku śpi elita-strażnik; na środku skrzynia.
        void place_secret()
        {
            if(sdef().boss >= 0 || lv.rooms_count < 2) return;
            rng sr = side_rng(202);
            if(sr.range(1, 100) > data::secret_chance_pct) return;
            for(int a = 0; a < 80 && secret_x < 0; ++a)
            {
                const room& rm = lv.rooms[sr.range(0, lv.rooms_count - 1)];
                const int d = sr.range(0, 3), dx = gust_vec[d][0], dy = gust_vec[d][1];
                int fx, fy;
                if(dx) { fx = dx > 0 ? rm.x + rm.w - 1 : rm.x; fy = sr.range(rm.y, rm.y + rm.h - 1); }
                else { fy = dy > 0 ? rm.y + rm.h - 1 : rm.y; fx = sr.range(rm.x, rm.x + rm.w - 1); }
                const int ex = fx + dx, ey = fy + dy;
                const int ix = dx ? (dx > 0 ? ex + 1 : ex - 3) : ex - 1, iy = dy ? (dy > 0 ? ey + 1 : ey - 3) : ey - 1;
                if(lv.at(fx, fy) != tile::floor || ix - 1 < 0 || iy - 1 < 0 || ix + 3 >= map_w || iy + 3 >= map_h) continue;
                bool ok = true;
                for(int y = iy - 1; y <= iy + 3 && ok; ++y)
                    for(int x = ix - 1; x <= ix + 3; ++x) if(lv.t[y][x] != tile::wall) { ok = false; break; }
                if(! ok) continue;
                for(int y = iy; y < iy + 3; ++y) for(int x = ix; x < ix + 3; ++x) lv.t[y][x] = tile::floor;
                secret_x = int8_t(ex); secret_y = int8_t(ey); secret_dir = int8_t(d);
                secret_rx = int8_t(ix); secret_ry = int8_t(iy); secret_rw = 3; secret_rh = 3;
                secret_kind = int8_t(sr.range(0, data::secret_kinds_count - 1));
            }
            if(secret_x < 0) return;
            const int cx = secret_rx + 1, cy = secret_ry + 1;
            if(pickups_count < max_pickups) pickups[pickups_count++] = { int8_t(cx), int8_t(cy), uint8_t(chest), true };
            const stage_def& sd = sdef();
            if(sr.range(1, 100) <= data::secret_guard_pct && enemies_count < max_enemies)   // strażnik: elita, śpi w kącie
            {
                const int gx = secret_rx + 2 * (sr.range(0, 1)), gy = secret_ry + 2 * (sr.range(0, 1));
                spawn(sd.pool[sr.range(0, sd.pool_count - 1)], gx, gy);
                make_elite(enemies_count - 1, sr.range(0, data::elites_count - 1));
            }
            // klucz: pierwsza elita poza magazynem, inaczej losowy problem (bez bossa i strażnika)
            int outside = 0;
            for(int i = 0; i < enemies_count; ++i) outside += ! in_secret(enemies[i].x, enemies[i].y);
            for(int i = 0; i < enemies_count && key_holder < 0; ++i) if(enemies[i].elite >= 0 && ! in_secret(enemies[i].x, enemies[i].y)) key_holder = int8_t(i);
            if(key_holder < 0 && outside > 0)
            {
                int k = sr.range(0, outside - 1);
                for(int i = 0; i < enemies_count; ++i) if(! in_secret(enemies[i].x, enemies[i].y) && k-- == 0) { key_holder = int8_t(i); break; }
            }
        }
        bool has_secret() const { return secret_x >= 0; }
        bool secret_closed() const { return secret_x >= 0 && ! secret_open; }
        bool secret_is(int x, int y) const { return secret_x >= 0 && x == secret_x && y == secret_y; }
        bool in_secret(int x, int y) const
        {
            return secret_x >= 0 && x >= secret_rx && x < secret_rx + secret_rw && y >= secret_ry && y < secret_ry + secret_rh;
        }
        const secret_kind_def& secret_def() const { return data::secret_kinds[secret_kind]; }
        // Pole przed ścianą magazynu (od strony pokoju) - tam trzeba stanąć, żeby otworzyć.
        int secret_front_x() const { return secret_x - gust_vec[secret_dir][0]; }
        int secret_front_y() const { return secret_y - gust_vec[secret_dir][1]; }
        bool can_open_secret() const { return secret_closed() && (keys > 0 || (secret_def().breakable && has_passive(class_passive::push))); }
        void open_secret(const char* how)
        {
            if(! secret_closed()) return;
            secret_open = true;
            lv.t[secret_y][secret_x] = tile::floor;
            if(secrets_found < 255) ++secrets_found;
            stage_flags[stage] = uint8_t(stage_flags[stage] | recap_secret);
            push(message().add(how).add(" Magazyn otwarty!").as(good));
            update_fov();
        }
        // Wejście w ścianę magazynu: klucz, łyżka Operatora koparki (pęknięta ściana); inaczej podpowiedź, bez tury.
        bool try_open_secret()
        {
            if(keys > 0) { --keys; open_secret("Klucz pasuje!"); return true; }
            if(secret_def().breakable && has_passive(class_passive::push)) { open_secret("Łyżka kruszy ścianę!"); return true; }
            push(message().add(secret_def().name).add(": ").add(secret_def().info));
            return false;
        }
        // Wybuch w promieniu rad od (x, y) kruszy pękniętą ścianę.
        void blast_secret(int x, int y, int rad)
        {
            if(secret_closed() && secret_def().breakable && cheb(x, y, secret_x, secret_y) <= rad) open_secret("Wybuch kruszy ścianę!");
        }
        // Klucz z problemu: na polu usunięcia (albo obok), bez miejsca na znajdźkę - od razu do kieszeni.
        void drop_key(int x, int y)
        {
            key_holder = -1;
            int kx = x, ky = y;
            if((pickup_at(x, y) && ! free_around(x, y, hero.x, hero.y, kx, ky)) || pickups_count >= max_pickups)
            {
                ++keys;
                push(message().add("Klucz do magazynu!").as(loot));
                return;
            }
            pickups[pickups_count++] = { int8_t(kx), int8_t(ky), uint8_t(store_key), true };
            push(message().add("Wypadł klucz do magazynu!").as(loot));
        }
        void open_chest()
        {
            respect += data::chest_respect;
            cash += income(data::chest_cash);
            for(int m = 0; m < data::materials_count; ++m) add_material(m, data::chest_mats);
            push(message().add("Skrzynia! Respekt +").add(data::chest_respect).add(", +").add(income(data::chest_cash)).add(" zł").as(loot));
            take_gear(random_slot(), data::chest_gear_min, r.range(0, data::gear_traits_count - 1));
        }
        // Cel bota zamiast schodów: klucz, pole przed magazynem (gdy da się otworzyć), skrzynia, wydarzenie.
        bool bot_goal(int& gx, int& gy) const
        {
            for(int i = 0; i < pickups_count; ++i) if(pickups[i].active && pickups[i].type == store_key) { gx = pickups[i].x; gy = pickups[i].y; return true; }
            if(can_open_secret()) { gx = secret_front_x(); gy = secret_front_y(); return true; }
            for(int i = 0; i < pickups_count; ++i)
                if(pickups[i].active && (pickups[i].type == chest ? secret_open : pickups[i].type == event_tile)) { gx = pickups[i].x; gy = pickups[i].y; return true; }
            return false;
        }
        // Bot: rozstrzyga oczekujące decyzje (wydarzenie, premia z wydarzenia, cecha narzędzia, narzędzie). true = coś zrobił.
        bool bot_pending()
        {
            if(pending_event >= 0) { choose_event(bot_event_choice()); return true; }
            if(has_boon_offer() && st == status::playing) { pick_boon(bot_boon_choice()); return true; }
            if(trait_pending) { choose_trait(bot_trait_choice()); return true; }
            if(has_tool_offer()) { if(bot_tool_accept()) accept_tool(); else decline_tool(); return true; }
            return false;
        }
        // Bot w Hurtowni (po bossie aktu): ulepsza narzędzie o 1 poziom, jeśli stać (cecha - pierwsza).
        void bot_upgrade()
        {
            if(! act_cleared || shop_closed()) return;
            for(int i = 0; i < data::hurtownia_count; ++i)
                if(data::hurtownia[i].effect == shop_effect::upgrade && hurtownia_buy(i)) bot_pending();
        }
        // Sprzęt z wydarzenia / skrzyni: pusty slot - zakłada, zajęty - porównanie (jak paczka).
        void take_gear(int slot, int rarity, int trait)
        {
            if(equipped[slot] < 0) equip(slot, rarity, trait);
            else if(! has_offer())
            {
                offer_slot = int8_t(slot); offer_rarity = int8_t(rarity); offer_trait = int8_t(trait);
                push(message().add("Paczka: ").add(data::gear[slot * 3 + rarity].name).as(loot));
            }
        }

        // Suma cech założonego sprzętu danego rodzaju.
        int trait_bonus(trait_effect e) const
        {
            int b = 0;
            for(int i = 0; i < data::gear_slots_count; ++i)
                if(equipped[i] >= 0 && data::gear_traits[equipped_trait[i]].effect == e) b += data::gear_traits[equipped_trait[i]].value;
            return b;
        }
        // Unik: szczęście + Respekt + buty, łącznie najwyżej data::dodge_max_pct.
        int dodge_pct() const
        {
            return imin(data::dodge_max_pct, data::dodge_per_luck_pct * luck() + bonus.dodge + gear_bonus(gear_stat::dodge) + boon_sum(boon_effect::dodge));
        }
        bool has_passive(class_passive p) const { return cdef().passive == p; }
        // Obrona bohatera: zawód + premie + sprzęt + ochrona BHP-owca z brygady.
        int hero_defense() const
        {
            return cdef().defense + def_bonus + gear_bonus(gear_stat::def) + (guard_turns > 0 ? data::brigade[helper_called].value : 0) + boon_defense()
                   + event_def;   // wydarzenie (#30): OBR na etap
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
        const story_msg& stage_story() const { return tier > 0 && stage == first_stage ? data::story_ngplus : data::story_stages[stage_id(stage)]; }

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
            return sdef().hp_pct * ddef().hp_pct / 100 * (100 + tier * data::ng_hp_pct_per_tier) / 100
                   * (100 + investor_value(investor_effect::enemy_hp)) / 100;
        }
        int enemy_dmg_bonus() const
        {
            return sdef().dmg_bonus + ddef().dmg_bonus + tier * data::ng_dmg_bonus_per_tier + investor_value(investor_effect::enemy_dmg);
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
                     const run_mods& mods = run_mods(), int contract_index = 0)
        {
            *this = game();
            contract = int8_t(contract_index >= 0 && contract_index < data::career_count ? contract_index : 0);
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
            first_stage = int8_t(mods.act0 ? 0 : prelude_count());   // bez nagrody Akt 0 budowa zaczyna się od Fundamentów
            start_stage(first_stage);
            thermos = imin(thermos_cap(), mods.start_coffee);   // Respekt: Zaprawiony w boju
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
            // v0.21.52 cz. d (#47): bliźniak - druga połowa ma tę samą pogodę i wydarzenie na placu co pierwsza, a problemy
            // niedokończone w pierwszej połowie przechodzą przez wspólną ścianę (do career_twin_carry_max)
            const bool twin = s == stage + 1 && sdef(s).twin;
            const int8_t twin_weather = weather, twin_event = stage_event;
            int carry = 0;
            if(twin) for(int i = 0; i < enemies_count; ++i) carry += enemies[i].alive && i != boss;
            twin_carry = uint8_t(imin(carry, data::career_twin_carry_max));
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
            pending_event = -1; stage_choice = stage_choice_pick = -1; choice_done = 0; event_dmg = event_def = 0;   // v0.21.50 cz. 3
            tool_offer = tool_offer_pickup = -1;
            secret_x = secret_y = -1; secret_open = false; secret_kind = secret_dir = 0; key_holder = -1; keys = 0;
            secret_rx = secret_ry = secret_rw = secret_rh = 0;
            mark_target = -1; mark_turns = 0;   // v0.21.51 cz. 2: Tyczenie tylko na etap
            if(s == 0) paper_hits = 0;          // Akt 0 od nowa (NG+): ciosy od papierów liczone od pierwszego etapu
            if(cdef().ability == ability_effect::borrow)   // Majster: moc innego fachu na ten etap (osobny generator)
            {
                rng br = side_rng(7);
                const bool open = cls < data::open_classes_count;   // zawody z sekretów są za zwykłymi
                int k = br.range(0, data::open_classes_count - (open ? 2 : 1));
                if(open && k >= cls) ++k;   // nigdy własna
                borrow_cls = int8_t(k);
            }
            for(auto& row : fov) for(auto& c : row) c = unknown;
            const stage_def& sd = sdef();
            const room& first = lv.rooms[0];
            const room& last = lv.rooms[lv.rooms_count - 1];
            hero.x = int8_t(first.cx()); hero.y = int8_t(first.cy());
            boss = -1; stairs_x = stairs_y = -1;
            if(sd.boss < 0) { stairs_x = last.cx(); stairs_y = last.cy(); lv.t[stairs_y][stairs_x] = tile::stairs; }

            enemies_count = 0;
            const int count = imax(1, sd.enemy_count + (pd ? pd->enemies : 0) + twin_carry);   // ścieżka: więcej / mniej problemów
            for(int i = 0; i < count && enemies_count < max_enemies; ++i)
            {
                int room_i = 1 + r.range(0, lv.rooms_count - 2 > 0 ? lv.rooms_count - 2 : 0);
                if(room_i >= lv.rooms_count) room_i = lv.rooms_count - 1;
                int x, y; random_free_cell_in_room(lv.rooms[room_i], x, y);
                spawn(sd.pool[r.range(0, sd.pool_count - 1)], x, y);
                if(r.range(1, 100) <= elite_chance()) make_elite(enemies_count - 1, r.range(0, data::elites_count - 1));   // elita (#28)
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
            if(twin_carry > 0) push(message().add("Wspólna ściana: +").add(int(twin_carry)).add(" z 1. połowy").as(bad));
            if(pd)   // ścieżka z harmonogramu: budżet i materiały od razu
            {
                push(message().add("Ścieżka: ").add(pd->name));
                if(pd->cash != 0) cash = imax(0, cash + income(pd->cash));
                for(int k = 0; k < pd->materials; ++k) add_material(r.range(0, data::materials_count - 1));
            }
            weather = twin ? twin_weather : int8_t(roll_weather(s, pd && pd->bad_weather));   // pogoda dnia (bliźniak: ta sama)
            if(weekly_has(weekly_rule::weather)) weather = int8_t(weekly_value(weekly_rule::weather));   // wyzwanie: Mokry tydzień
            if(wdef().effect != weather_effect::none)
                push(message().add("Pogoda: ").add(wdef().name).add(" (").add(wdef().short_name).add(")").as(wdef().bad ? bad : good));
            stage_event = -1;   // wydarzenie na placu: nie na pierwszym etapie i nie u bossa
            if(twin) { if(twin_event >= 0) apply_event(twin_event); }   // bliźniak: to samo wydarzenie na obu połówkach
            else if(s > first_stage && sd.boss < 0 && ! (pd && pd->no_event) && r.range(1, 100) <= data::site_event_chance_pct)
            {
                int e = r.range(0, data::site_events_count - 1);
                // niekorzystna pogoda i niekorzystne wydarzenie naraz to za dużo: wydarzenie przepada
                if(! (data::weather_no_bad_stack && wdef().bad && ! data::site_events[e].good)) apply_event(e);
            }
            place_documents();
            place_event();    // v0.21.50 cz. 3: pole wydarzenia z wyborem (#30)
            place_secret();   // ukryte pomieszczenie (#32): magazyn, skrzynia, klucz, strażnik
            // kombinacje stanów (#29): w pyle (akt III) problemy są zapylone, w Mróz - zmrożone (boss nie)
            for(int i = 0; i < enemies_count; ++i)
            {
                if(i == boss) continue;
                if(dust_sight() > 0) enemies[i].flags = uint8_t(enemies[i].flags | actor_dusty);
                if(weather_is(weather_effect::frost)) enemies[i].flags = uint8_t(enemies[i].flags | actor_frozen);
            }
            update_fov();
            if(has_passive(class_passive::surveyor)) reveal_map();   // Geodeta: cały plac (i dokumenty Aktu 0) od startu
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
            b.crit_weapon = w.crit + (b.weapon == cdef().weapon ? master_crit() : 0);   // v0.21.52 cz. b: broń mistrza
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
            b.flat_boon = boon_sum(boon_effect::dmg);
            b.pct_boon = boon_sum(boon_effect::dmg_pct);
            b.crit_boon = boon_sum(boon_effect::crit);
            b.vs_enemy = enemy_def_id >= 0;
            b.enemy_def = b.vs_enemy ? data::enemies[enemy_def_id].defense : 0;
            b.luck = cdef().luck + bonus.luck + luck_t + boon_luck();
            b.crit_bonus = bonus.crit;
            b.power_rank = ability_rank();
            b.power = power_dmg_bonus();
            b.flat_event = event_dmg;
            if(weapon_idx < 0)   // ulepszenie (#31) tylko obecnego narzędzia - przy zamianie przepada
            {
                b.upg_level = weapon_lvl; b.flat_upgrade = upgrade_dmg(); b.upg_trait = weapon_trait;
                b.pierce = tool_trait_value(tool_trait_effect::pierce);
                b.steady = tool_trait_value(tool_trait_effect::steady);
                b.crit_upg = tool_trait_value(tool_trait_effect::crit);
            }
            b.finish();
            return b;
        }
        // Rozpiska przeciw konkretnemu problemowi na planszy (karta problemu): z obroną elity (Tarcza).
        dmg_breakdown actor_breakdown(int ei) const
        {
            dmg_breakdown b = weapon_breakdown(enemies[ei].def_id);
            b.enemy_elite = enemy_elite_def(ei);
            b.elite = enemies[ei].elite;
            b.finish();
            return b;
        }
        // Premia mocy do ciosu (ability): Seria i Rynna +1 od rangi II, Taran +ranga; premie zawodów: Wirówka, Taran.
        int power_dmg_bonus() const
        {
            int rank = ability_rank();
            switch(pdef().ability)
            {
                case ability_effect::volley:
                case ability_effect::line: return rank >= 2 ? 1 : 0;
                case ability_effect::ram:  return rank + boon_power();
                case ability_effect::spin: return boon_power();
                default:                   return 0;
            }
        }
        // Premia obrażeń problemu: etap, trudność, wzrost, elita.
        int enemy_bonus(const actor& e) const { return enemy_dmg_bonus() + e.grow / 2 + (e.elite >= 0 ? data::elite_dmg : 0); }
        // Cios problemu ei w bohatera (zakres po OBR i -%).
        hit_range enemy_hit(int ei) const
        {
            const actor& e = enemies[ei];
            const enemy_def& ed = data::enemies[e.def_id];
            return enemy_hit_range(ed.min_damage, ed.max_damage, enemy_bonus(e), hero_defense(), bonus.taken_pct);
        }

        // obrażenia = rzut broni + stat/2 + premie - obrona/2, min 1
        void hero_attack(int ei)
        {
            if(ei == boss && boss_wake_damage < 0) boss_engaged();   // walka z bossem trwa
            const int ex = enemies[ei].x, ey = enemies[ei].y;
            const bool was_wet = enemy_wet(ei), was_dusty = enemy_dusty(ei), was_frozen = enemy_frozen(ei);
            const bool melee = cheb(hero.x, hero.y, ex, ey) <= 1;
            const weapon_def& w = weapon();   // ulepszenie (#31): +obrażeń, Wyważenie (rzut), Przebicie (OBR)
            int dmg = r.range(imin(w.max_damage, w.min_damage + tool_trait_value(tool_trait_effect::steady)), w.max_damage)
                    + hero_stat(w.scales_with) / 2 + dmg_bonus + gear_bonus(gear_stat::dmg) + boon_sum(boon_effect::dmg) + upgrade_dmg() + event_dmg
                    + mark_bonus(ei)   // Tyczenie (Geodeta)
                    + (enemies[ei].hp >= enemies[ei].max_hp ? first_hit_bonus(bonus) : 0)   // v0.21.52 cz. c: Siła rozpędu
                    - imax(0, enemy_defense(ei) - tool_trait_value(tool_trait_effect::pierce)) / 2;
            if(dmg < 1) dmg = 1;
            dmg += pct_part(dmg, bonus.dmg_pct + boon_sum(boon_effect::dmg_pct), dmg_carry);   // Kurs fachowy, Respekt, premie: +%
            bool crit = r.range(1, 100) <= crit_pct();
            if(crit) dmg *= data::crit_multiplier;
            damage_enemy(ei, dmg, crit, weapon().name);
            // kombinacje stanów (#29): stan celu sprzed ciosu + żywioł ciosu
            if(st == status::playing && hit_power() && was_wet) combo_shock(ei, ex, ey);
            if(st == status::playing && hit_spark() && was_dusty) combo_dust(ex, ey);
            if(st == status::playing && melee && was_frozen && enemies[ei].alive && enemy_frozen(ei)) combo_crack(ei, dmg);
            actor& e = enemies[ei];
            if(e.alive && boon_sum(boon_effect::wet_hits) > 0) e.wet = int8_t(imax(e.wet, boon_sum(boon_effect::wet_hits)));   // Wąż ogrodowy
            if(e.alive && boon_sum(boon_effect::frost_hits) > 0 && ei != boss) e.flags = uint8_t(e.flags | actor_frozen);      // Suchy lód
            // Operator koparki: cios wręcz czasem odpycha problem o pole (bossa nie)
            if(has_passive(class_passive::push) && e.alive && ei != boss && cheb(hero.x, hero.y, e.x, e.y) == 1
               && r.range(1, 100) <= data::push_chance_pct)
                shove(ei, isign(e.x - hero.x), isign(e.y - hero.y), 1);
            else if(w.knockback && e.alive && ei != boss && melee && cheb(hero.x, hero.y, e.x, e.y) == 1)   // Młot Zenka: zawsze odpycha
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
            if(dmg > best_hit) { best_hit = int16_t(imin(32767, dmg)); best_hit_def = e.def_id; best_hit_crit = crit; }   // podsumowanie
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
                if(ei == mark_target) { mark_target = -1; mark_turns = 0; }
                if(ei == boss && helper_ctx && e.def_id == data::secret_helper_boss) secret_flags = uint8_t(secret_flags | secret_helper_boss);
                cash += income(ed.score / data::cash_per_score);
                if(kills_by_type[e.def_id] < 255) ++kills_by_type[e.def_id];
                score += ed.score * score_pct() / 100; gain_xp(data::xp_per_kill);
                if(e.elite >= 0)   // elita: pewna paczka (lepsza), materiały, Respekt
                {
                    elite_reward(ei);
                    if(elites_killed < 255) ++elites_killed;
                    stage_flags[stage] = uint8_t(stage_flags[stage] | recap_elite);
                }
                maybe_drop(e.x, e.y);
                if(ei == key_holder) drop_key(e.x, e.y);   // klucz do magazynu (#32)
                push(message().add(ed.name).add(" - usunięto!").as(good));
                const int kh = boon_sum(boon_effect::kill_heal);   // Drożdżówka: HP za usunięty problem
                if(kh > 0 && hero.alive && hero.hp < hero.max_hp) hero.hp = int16_t(imin(hero.max_hp, hero.hp + kh));
                if((ed.tags & tag_explodes) || elite_is(e, elite_effect::explode)) arm_blast(e.x, e.y, ed);
                if((ed.tags & tag_splits) && ! (e.flags & actor_child)) split(ei);
                if(ei == boss)   // boss: po kilka sztuk każdego materiału
                    for(int m = 0; m < data::materials_count; ++m) add_material(m, data::material_boss_drop);
                else if(r.range(1, 100) <= data::material_drop_pct * (100 + bonus.mats_pct + boon_sum(boon_effect::mats_pct) + weekly_value(weekly_rule::mats_pct)
                                                                          + synergy_value(synergy_effect::stock)) / 100)   // Respekt: Zapasy
                    add_material(ed.material >= 0 ? ed.material : r.range(0, data::materials_count - 1));
                if(ei == boss)
                {
                    stage_flags[stage] = uint8_t(stage_flags[stage] | recap_boss);
                    if(stage_damage == boss_wake_damage && clean_bosses < 255) ++clean_bosses;   // zlecenie Czysta robota
                    if(stage == prelude_count() - 1 && first_stage == 0 && paper_hits == 0)   // Akt 0 bez ciosu od papierów
                        secret_flags = uint8_t(secret_flags | secret_paper_clean);
                    score += (500 + 100 * imax(0, pattern_stage() + 1)) * score_pct() / 100;
                    gain_xp(data::xp_boss);
                    slam_timer = 0;
                    if(ed.reward_cash > 0)   // nagroda bossa (Inspekcja: Protokół bez uwag)
                    {
                        cash += income(ed.reward_cash);
                        push(message().add(ed.reward_title).add("! +").add(income(ed.reward_cash)).add(" zł").as(good));
                    }
                    finish_stage();
                    if(last_stage())
                    {
                        st = status::won;
                        push(message().add("Odbiór techniczny zaliczony!").as(good));
                    }
                    else if(sdef(stage + 1).act == sdef().act)   // boss w środku aktu: dalej bez Hurtowni
                    {
                        st = status::stage_clear;
                        push(message().add("Etap zakończony: ").add(sdef().name).as(good));
                    }
                    else   // boss aktu: premia za akt, potem Hurtownia
                    {
                        const act_def& ad = adef();
                        int stages_in_act = 0;
                        for(int i = 0; i < route_count(); ++i) stages_in_act += sdef(i).act == sdef().act;
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
            {
                push(message().add(crit ? "KRYT! " : "").add(src).add(": -").add(dmg).add(" (").add(ed.name).add(")").as(crit ? loot : info));
                if(elite_is(e, elite_effect::summon) && ! (e.flags & actor_called) && e.hp * 100 <= e.max_hp * data::elites[e.elite].value)
                    elite_call(ei);
            }
        }

        // Elita "wzywa pomoc": raz, przy value% HP - słabszy problem tego samego rodzaju obok (połowa HP, bez cechy).
        void elite_call(int ei)
        {
            actor& e = enemies[ei];
            e.flags = uint8_t(e.flags | actor_called);
            int x, y;
            if(! free_around(e.x, e.y, hero.x, hero.y, x, y)) return;
            int slot = free_slot();
            if(slot < 0) return;
            actor& c = enemies[slot];
            c = actor();
            c.x = int8_t(x); c.y = int8_t(y); c.def_id = enemies[ei].def_id;
            c.hp = c.max_hp = int16_t(imax(1, data::enemies[c.def_id].max_health * enemy_hp_pct() / 100 / 2));
            c.alive = true; c.awake = true; c.stun = 1; c.flags = actor_child;
            push(message().add(data::enemies[c.def_id].name).add(": wzywa pomoc!").as(bad));
        }

        // Nagroda za elitę: pewny drop (paczka sprzętu co najmniej solidna), materiały, Respekt.
        void elite_reward(int ei)
        {
            const actor& e = enemies[ei];
            respect += data::elite_respect;
            add_material(r.range(0, data::materials_count - 1), data::elite_mats);
            if(pickups_count < max_pickups && ! pickup_at(e.x, e.y)) drop_at(e.x, e.y, data::elite_gear_min);
            push(message().add("Elita usunięta! Respekt +").add(data::elite_respect).as(loot));
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
            else if(secret_closed() && secret_is(nx, ny)) { if(! try_open_secret()) return false; }   // magazyn (#32)
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
                if(puddle(hero.x, hero.y)) soak_hero();   // kałuża moczy (#29)
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
            return imax(3, imax(4, pdef().ability_cooldown - 2 * (ability_rank() - 1)) - trait_bonus(trait_effect::cooldown) - bonus.cooldown
                           - boon_sum(boon_effect::cooldown) + mastery_cooldown())   // v0.21.52 cz. b: wariant mocy
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
            const class_def& c = pdef();   // Majster: moc pożyczona na ten etap
            const int rank = ability_rank();
            bool ok = false;
            switch(c.ability)
            {
                case ability_effect::stun:   // Odprawa: ogłusza widocznych na 2/3/4 tury
                    for(int i = 0; i < enemies_count; ++i)
                        if(enemies[i].alive && visible(enemies[i].x, enemies[i].y))
                        { enemies[i].stun = int8_t(1 + rank + boon_power()); enemies[i].awake = true; ok = true; }
                    if(ok) push(message().add(c.ability_name).add(": problemy wstrzymane"));
                    break;
                case ability_effect::wall:   // Ścianka: mur w poprzek drogi najbliższego wroga (nigdy wokół bohatera)
                    ok = wall_toward_enemy(rank >= 3 ? 2 : 1, 4 + 2 * rank + boon_power());
                    if(ok) push(message().add(c.ability_name).add(" postawiona!"));
                    break;
                case ability_effect::volley:   // Seria: wszyscy widoczni w zasięgu (+1 obrażeń od II, +1 zasięgu na III)
                {
                    int range = weapon_range() + (rank >= 3 ? 1 : 0) + boon_power();
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
                    hit_ctx = 1;   // Łańcuch: prąd (mokry + prąd = porażenie)
                    for(int k = 0; k < 2 + rank + boon_power() && t >= 0 && st == status::playing; ++k)
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
                    hit_ctx = 0;
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
                        e.wet = int8_t(imax(e.wet, data::wet_turns));   // i jest mokry (#29)
                        ok = true;
                    }
                    int h = imin(4 + 2 * rank + boon_power(), hero.max_hp - hero.hp);
                    if(h > 0) { hero.hp = int16_t(hero.hp + h); ok = true; }
                    if(ok) push(message().add(c.ability_name).add(": strumień! +").add(imax(0, h)).add(" HP"));
                    break;
                }
                case ability_effect::spin:   // Wirówka: wszyscy obok (zasięg 2 na III), od II ogłusza na 1 turę
                {
                    int reach = rank >= 3 ? 2 : 1;
                    hit_ctx = 2;   // Wirówka: iskry (pył + iskra = wybuch)
                    dmg_bonus += boon_power();
                    for(int i = 0; i < enemies_count && st == status::playing; ++i)
                    {
                        int d = cheb(hero.x, hero.y, enemies[i].x, enemies[i].y);
                        if(enemies[i].alive && d >= 1 && d <= reach)
                        {
                            hero_attack(i); ok = true;
                            if(rank >= 2 && enemies[i].alive) enemies[i].stun = int8_t(imax(enemies[i].stun, 1));
                        }
                    }
                    dmg_bonus -= boon_power();
                    hit_ctx = 0;
                    break;
                }
                case ability_effect::line:   // Rynna (Dekarz): dachówki lecą linią przez najbliższy widoczny problem (4/5/6 pól)
                {
                    int t = nearest_visible_enemy();
                    if(t < 0) break;
                    int dx = enemies[t].x - hero.x, dy = enemies[t].y - hero.y, len = imax(iabs(dx), iabs(dy));
                    if(rank >= 2) ++dmg_bonus;
                    uint32_t done = 0;
                    for(int k = 1; k <= 3 + rank + boon_power() && st == status::playing; ++k)
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
                            const int stun = (rank >= 2 ? 1 : 0) + boon_power();   // premia Gęsty tynk: dłużej
                            if(stun > 0 && enemies[i].alive) enemies[i].stun = int8_t(imax(enemies[i].stun, stun));
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
                            dmg_bonus += rank + boon_power();
                            hero_attack(ei);
                            dmg_bonus -= rank + boon_power();
                            if(enemies[ei].alive && st == status::playing)
                            {
                                if(ei != boss) shove(ei, dx, dy, 2);
                                enemies[ei].stun = int8_t(imax(enemies[ei].stun, 1));
                            }
                            ok = true;
                            break;
                        }
                        if(secret_closed() && secret_is(nx, ny) && secret_def().breakable) { open_secret("Taran kruszy ścianę!"); ok = true; break; }
                        if(! lv.passable(nx, ny) || occupied(nx, ny)) break;
                        hero.x = int8_t(nx); hero.y = int8_t(ny); moved = ok = true;
                    }
                    if(moved) collect();
                    if(ok) push(message().add(c.ability_name).add("!"));
                    break;
                }
                case ability_effect::weld:   // Spaw (Spawacz): iskry linią przez najbliższy widoczny problem (3/4/5 pól), trafieni
                {                            // w dymie spawalniczym (zapyleni) - kolejna iskra = wybuch pyłu (#29)
                    int t = nearest_visible_enemy();
                    if(t < 0) break;
                    int dx = enemies[t].x - hero.x, dy = enemies[t].y - hero.y, len = imax(iabs(dx), iabs(dy));
                    uint32_t done = 0;
                    hit_ctx = 2;   // iskra
                    for(int k = 1; k <= 2 + rank + boon_power() && st == status::playing; ++k)
                    {
                        int x = hero.x + div_round(dx * k, len), y = hero.y + div_round(dy * k, len);
                        if(! lv.passable(x, y)) break;   // mur zatrzymuje iskry
                        int ei = enemy_at(x, y);
                        if(ei >= 0 && ! (done & (1u << ei)))
                        {
                            hero_attack(ei); done |= 1u << ei; ok = true;
                            if(enemies[ei].alive) enemies[ei].flags = uint8_t(enemies[ei].flags | actor_dusty);   // dym spawalniczy
                        }
                    }
                    hit_ctx = 0;
                    if(ok) push(message().add(c.ability_name).add(": iskry i dym!"));
                    break;
                }
                case ability_effect::mark:   // Tyczenie (Geodeta): najbliższy widoczny problem oznaczony na kilka tur, ogłuszony na turę
                {
                    int t = nearest_visible_enemy();
                    if(t < 0) break;
                    mark_target = int8_t(t); mark_turns = int8_t(data::mark_turns);
                    enemies[t].stun = int8_t(imax(enemies[t].stun, 1)); enemies[t].awake = true;
                    ok = true;
                    push(message().add(c.ability_name).add(": ").add(data::enemies[enemies[t].def_id].name).add(" +").add(mark_bonus(t)));
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
        int helper_price(int h) const
        {
            return data::brigade[h].price * (100 - imin(90, bonus.brigade_pct + boon_sum(boon_effect::brigade_pct) + synergy_value(synergy_effect::brigade))) / 100;
        }

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
            if(helpers_called < 255) ++helpers_called;   // v0.21.52 cz. c: zadania dnia
            push(message().add("Brygada: ").add(hd.name).as(good));
            switch(hd.effect)
            {
                case helper_effect::reveal:   // podłoga, schody i mury przy nich
                    reveal_map();
                    break;
                case helper_effect::pump:
                    for(int i = 0; i < enemies_count && st == status::playing; ++i)
                        if(enemies[i].alive && cheb(hero.x, hero.y, enemies[i].x, enemies[i].y) <= hd.reach)
                        {
                            helper_ctx = 1; damage_enemy(i, hd.value, false, hd.name); helper_ctx = 0;
                        }
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

        // Mapa etapu odkryta (Geodeta z brygady, zawód Geodeta na starcie etapu): podłoga, schody i mury przy nich.
        void reveal_map()
        {
            for(int y = 0; y < map_h; ++y)
                for(int x = 0; x < map_w; ++x)
                {
                    if(fov[y][x] != unknown) continue;
                    bool near = false;
                    for(int dy = -1; dy <= 1 && ! near; ++dy) for(int dx = -1; dx <= 1; ++dx) if(lv.passable(x + dx, y + dy)) { near = true; break; }
                    if(near) fov[y][x] = remembered;
                }
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
                if(enemies[i].alive && cheb(ally_x, ally_y, enemies[i].x, enemies[i].y) == 1)
                {
                    helper_ctx = 1; damage_enemy(i, hd.value, false, hd.name); helper_ctx = 0;
                    break;
                }
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
            if(it.effect == shop_effect::upgrade) return upgrade_affordable();   // ulepszenie narzędzia: zł + materiał
            return it.material >= 0 ? mats[it.material] >= it.mat_cost : cash >= hurtownia_price(i);
        }
        // Cena towaru w zł po rabacie z Respektu.
        int hurtownia_price(int i) const
        {
            if(data::hurtownia[i].effect == shop_effect::upgrade) return upgrade_price();
            return data::hurtownia[i].price * (100 - imin(90, bonus.shop_pct + boon_sum(boon_effect::shop_pct))) / 100;
        }

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
            if(shop_buys < 255) ++shop_buys;   // v0.21.52 cz. c: zadanie "Wygraj bez Hurtowni"
            if(it.effect == shop_effect::upgrade)   // ulepszenie narzędzia (#31): zł i materiał, potem +1 poziom
            {
                const tool_level_def& t = data::tool_levels[weapon_lvl];
                cash -= upgrade_price();
                mats[t.material] = uint8_t(mats[t.material] - t.count);
                upgrade_weapon();
                return true;
            }
            switch(it.effect)
            {
                case shop_effect::heal: hero.hp = hero.max_hp; if(coffee_drunk < 255) ++coffee_drunk; break;   // kawa z ekspresu
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
                        if(((bonus.tools >> t) & 1) && k-- == 0) { reset_upgrade(); weapon_override = data::tools[t].weapon; tools_found = uint8_t(tools_found | (1u << t)); break; }
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
            stage_kill_log[stage] = uint8_t(imin(255, stage_kills));
            int got = stage_respect();
            respect += got;
            push(message().add("Respekt +").add(got).as(loot));
            if(! last_stage()) roll_boons();   // premia 1 z 3 przed harmonogramem (nie po odbiorze)
        }

        // Respekt za bieżący etap: zwykły, boss w środku aktu, boss aktu, ostatni; mnożnik jak wynik (trudność, NG+).
        int stage_respect() const
        {
            const stage_def& sd = sdef();
            int base = last_stage() ? data::respect_final
                     : (sd.boss < 0 ? data::respect_stage : (sdef(stage + 1).act == sd.act ? data::respect_boss : data::respect_act_boss));
            return imax(1, base * score_pct() / 100);
        }

        int coffee_heal() const   // Respekt: Mocna kawa; premia Podwójne espresso
        {
            return div_round((data::coffee_heal + bonus.coffee + boon_sum(boon_effect::coffee)) * (100 + bonus.coffee_pct), 100);
        }

        void drink_coffee()
        {
            int h = imin(coffee_heal(), hero.max_hp - hero.hp);
            hero.hp = int16_t(hero.hp + h);
            if(coffee_drunk < 255) ++coffee_drunk;
            push(message().add("Kawa z termosu: +").add(h).add(" HP").as(good));
            const int es = synergy_value(synergy_effect::espresso);   // synergia Espresso: kawa ładuje moc
            if(es > 0 && ability_cd > 0)
            {
                ability_cd = imax(0, ability_cd - es);
                push(message().add("Espresso: moc -").add(es).add(" t.").as(good));
            }
        }

        // Picie z termosu (menu pod START): leczy, zużywa turę.
        bool player_drink()
        {
            if(st != status::playing) return false;
            if(weekly_has(weekly_rule::no_coffee)) { push(message().add("Tydzień bez kawy!").as(bad)); return false; }
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
            drop_at(x, y);
        }
        // Drop w polu (typ losowany wagami); paczka sprzętu co najmniej jakości min_rarity (elita: solidna).
        void drop_at(int x, int y, int min_rarity = 0)
        {
            int total = 0; for(int w : data::drop_weights) total += w;
            int roll = r.range(1, total), type = 0;
            while(roll > data::drop_weights[type]) roll -= data::drop_weights[type++];
            if(type != tool && bonus.tool_pct > 0 && r.range(1, 100) <= bonus.tool_pct) type = tool;   // uprawnienie Kolekcjoner
            uint8_t arg = 0, trait = 0;
            if(type == gear_box)   // slot losowy, jakość lepsza na późnych etapach
            {
                int q = r.range(1, 100) + imax(0, pattern_stage()) * data::gear_stage_bonus + data::rarity_per_luck * luck() + bonus.gear_pct;
                int rarity = imax(min_rarity, q >= data::gear_brand_from ? 2 : (q >= data::gear_solid_from ? 1 : 0));
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
                if(p.type == tool && (weapon_lvl > 0 || weapon_trait >= 0))   // ulepszone narzędzie: decyzja (ulepszenia przepadną)
                {
                    if(! has_tool_offer())
                    {
                        tool_offer = int8_t(p.arg); tool_offer_pickup = int8_t(i);
                        push(message().add("Narzędzie: ").add(data::weapons[data::tools[p.arg].weapon].name).add(" - zamienić?").as(loot));
                    }
                    continue;
                }
                p.active = false;
                if(p.type == event_tile)   // wydarzenie z wyborem (#30): SMS czeka na odpowiedź
                {
                    pending_event = int8_t(p.arg);
                    push(message().add("SMS: ").add(pending_def().name).as(loot));
                    continue;
                }
                if(p.type == store_key) { ++keys; push(message().add("Klucz do magazynu!").as(loot)); continue; }
                if(p.type == chest) { open_chest(); continue; }
                if(p.type == coffee && weekly_has(weekly_rule::no_coffee))   // wyzwanie: bez kawy - kawa na wynos (zł)
                {
                    cash += income(data::weekly_coffee_cash);
                    push(message().add("Bez kawy: na wynos +").add(income(data::weekly_coffee_cash)).add(" zł").as(loot));
                }
                else if(p.type == coffee)
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
                else take_tool(p.arg);
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
            if(elite_is(e, elite_effect::regen) && e.hp < e.max_hp)   // elita Uparta: +HP co turę
                e.hp = int16_t(imin(e.max_hp, e.hp + data::elites[e.elite].value));
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
            // elita Szybka: drugi krok, jeśli jeszcze nie stoi obok bohatera
            if(chase_step(e) && elite_is(e, elite_effect::fast) && iabs(e.x - hero.x) + iabs(e.y - hero.y) != 1) chase_step(e);
        }

        // Krok problemu prosto w stronę bohatera (najpierw oś z większą odległością).
        bool chase_step(actor& e)
        {
            int dx = isign(hero.x - e.x), dy = isign(hero.y - e.y);
            bool xfirst = iabs(hero.x - e.x) >= iabs(hero.y - e.y);
            int tries[2][2] = { { xfirst ? dx : 0, xfirst ? 0 : dy }, { xfirst ? 0 : dx, xfirst ? dy : 0 } };
            for(auto& t : tries)
            {
                if(t[0] == 0 && t[1] == 0) continue;
                int nx = e.x + t[0], ny = e.y + t[1];
                if(lv.at(nx, ny) == tile::floor && ! occupied(nx, ny)) { e.x = int8_t(nx); e.y = int8_t(ny); return true; }
            }
            return false;
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
            const bool was_wet = hero_wet();
            int dmg = taken_damage(r.range(ed.min_damage, ed.max_damage) + enemy_bonus(e) - hero_defense() / 2);
            hero.hp = int16_t(hero.hp - dmg);
            stage_damage += dmg;
            hero_hit = true;
            log_hit(e.def_id, e.elite, ranged ? recap_kind::ranged : recap_kind::melee, dmg);
            add_hit(hero.x, hero.y, dmg, true);
            push(message().add(ed.name).add(ranged ? " z dystansu: -" : ": -").add(dmg).add(" HP").as(bad));
            if(ed.on_hit != status_effect::none && hero.hp > 0 && r.range(1, 100) <= ed.status_chance)
                apply_status(ed.on_hit, ed.status_turns);
            if(ed.elem == element::water && hero.hp > 0) soak_hero();   // woda moczy
            if(ed.elem == element::power && was_wet && hero.hp > 0)   // mokry + prąd: porażenie bohatera
            {
                const combo_def& c = data::combos[int(combo_effect::shock_area)];
                hero.hp = int16_t(hero.hp - c.hero_value);
                stage_damage += c.hero_value;
                log_hit(e.def_id, e.elite, recap_kind::shock, c.hero_value);
                add_hit(hero.x, hero.y, c.hero_value, true);
                combo_events = uint8_t(combo_events | (8u << int(combo_effect::shock_area)));
                push(message().add(c.short_name).add(" -").add(c.hero_value).add(" HP").as(bad));
                if(hero.hp > 0) apply_status(status_effect::shock, 1);
            }
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
            blast_src = int8_t(&ed - data::enemies);   // podsumowanie: źródło wybuchu
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
            if(ability_cd > 0 && --ability_cd == 0) push(message().add("Moc gotowa: ").add(pdef().ability_name).as(good));
            if(mark_turns > 0 && --mark_turns == 0) mark_target = -1;   // Tyczenie mija
            int8_t& wet = hero_status[int(status_effect::wet)];
            if(wet > 0) --wet;   // mokry schnie
            for(int i = 0; i < enemies_count; ++i)   // problemy: kałuża moczy, poza nią schną
            {
                actor& e = enemies[i];
                if(! e.alive) continue;
                if(puddle(e.x, e.y)) e.wet = int8_t(data::wet_turns);
                else if(e.wet > 0) --e.wet;
            }
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
                    log_hit(enemies[boss].def_id, enemies[boss].elite, recap_kind::slam, dmg);
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
                    const bool dusty = dust_sight() > 0;   // pył + iskra (wybuch) na bohaterze (#29)
                    if(dusty) dmg += data::combos[int(combo_effect::dust_blast)].hero_value;
                    hero.hp = int16_t(hero.hp - dmg);
                    stage_damage += dmg;
                    hero_hit = true;
                    log_hit(blast_src, -1, dusty ? recap_kind::dust : recap_kind::blast, dmg);
                    add_hit(hero.x, hero.y, dmg, true);
                    if(dusty)
                    {
                        combo_events = uint8_t(combo_events | (8u << int(combo_effect::dust_blast)));
                        push(message().add(data::combos[int(combo_effect::dust_blast)].short_name).add(" Wybuch: -").add(dmg).add(" HP").as(bad));
                    }
                    else push(message().add("Wybuch: -").add(dmg).add(" HP").as(bad));
                    if(hero.hp <= 0) hero_down();
                }
                else push(message().add("Wybuch obok - uff!").as(good));
                blast_secret(blast_x, blast_y, data::behavior_blast_radius);   // wybuch kruszy pękniętą ścianę magazynu
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
            else if(st == status::playing && hero.x == stairs_x && hero.y == stairs_y) clear_stage();
        }

        // Wejście na otwarte schody: etap zaliczony (Respekt, oferta premii, wynik, doświadczenie, Inspekcja nadzoru).
        void clear_stage()
        {
            {
                st = status::stage_clear;
                finish_stage();
                score += 100 * score_pct() / 100;
                gain_xp(data::xp_per_stage);
                push(message().add("Etap zakończony: ").add(sdef().name).as(good));
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
            else if(stairs_x >= 0)   // na schody i od razu zaliczony (problem obok mógłby zepchnąć bohatera w tej turze)
            {
                docs = uint8_t((1u << docs_needed()) - 1); hero.x = int8_t(stairs_x); hero.y = int8_t(stairs_y);
                ++turns;
                clear_stage();
            }
        }

        // Przejście do kolejnego etapu (po ekranie harmonogramu) wybraną ścieżką. Przerwa na kawę: +5 HP.
        void next_stage()
        {
            if(! investor_has(investor_effect::no_break)) hero.hp = int16_t(imin(hero.max_hp, hero.hp + 5));   // tryb inwestora: bez przerwy
            if(boon_sum(boon_effect::regen_stage) > 0) hero.hp = int16_t(imin(hero.max_hp, hero.hp + boon_sum(boon_effect::regen_stage)));
            skip_boons();   // oferta bez wyboru przepada
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
            events_seen = 0;   // nowa budowa: wydarzenia od nowa
            clear_timeline();  // podsumowanie: oś czasu nowej budowy
            for(auto& e : enemies) e = actor();
            hero.hp = hero.max_hp;
            start_stage(first_stage);
            push(message().add("Kolejna budowa! Poziom ").add(tier + 1));
            return true;
        }
    };
}
