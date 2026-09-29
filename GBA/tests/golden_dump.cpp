// Zrzut "złotych" przebiegów gry dla testu zgodności wersji Godot (C#) z GBA.
// Kompilacja (z katalogu GBA): g++ -std=c++20 -O2 -Iinclude tests/golden_dump.cpp -o /tmp/golden_dump
// (nagłówki muszą pochodzić z tej samej wersji GBA co kopia GODOT/tests/LifeLike.Core.Tests/golden/game.json)
// Uruchomienie: /tmp/golden_dump <katalog_wyjściowy>   -> <katalog>/run_XX.json
// Każdy przebieg: deterministyczny bot (kopia bot_step z core_tests.cpp + wariant "smart" z mocą i celowaniem),
// po każdym kroku skrót FNV stanu (digest), na starcie każdego etapu i na końcu pełny zrzut stanu.
// Test C#: GODOT/tests/LifeLike.Core.Tests/GoldenTests.cs odtwarza to samo i porównuje pole po polu.
#include <cstdio>
#include <queue>
#include <string>
#include <vector>
#include "core.h"
#include "meta.h"
using namespace core;

// ------------------------------------------------------------------ bot (kopia z core_tests.cpp)
// Pole przechodnie dla bota: także tymczasowy mur (Ścianka) - znika za kilka tur, więc nie zmienia wybranego celu.
static bool bot_open(const game& g, int x, int y)
{
    if(g.lv.passable(x, y)) return true;
    for(int i = 0; i < g.walls_count; ++i) if(g.walls[i].x == x && g.walls[i].y == y) return true;
    return false;
}

// Koszt drogi bota: krok = waga pola, z którego + waga pola, na które (zwykłe 2, błoto 6 - kosztuje turę), więc koszt
// jest symetryczny i maleje wzdłuż drogi do celu (bez przeskakiwania między celami). Bez błota = 4 x liczba kroków.
static int bot_w(const game& g, int x, int y) { return g.mud(x, y) ? 6 : 2; }
static void bot_cost(const game& g, int sx, int sy, int (&dist)[map_h][map_w])
{
    static const int d[4][2]={{1,0},{-1,0},{0,1},{0,-1}};
    for(auto& r:dist) for(auto& c:r) c=-1;
    std::priority_queue<std::pair<int,int>, std::vector<std::pair<int,int>>, std::greater<>> q;
    dist[sy][sx]=0; q.push({0, sy*map_w+sx});
    while(!q.empty()){ auto [c,p]=q.top(); q.pop(); int x=p%map_w, y=p/map_w; if(c!=dist[y][x]) continue;
        for(int k=0;k<4;++k){int nx=x+d[k][0],ny=y+d[k][1]; if(!bot_open(g,nx,ny)) continue;
            int nc=c+bot_w(g,x,y)+bot_w(g,nx,ny); if(dist[ny][nx]<0||nc<dist[ny][nx]){dist[ny][nx]=nc;q.push({nc,ny*map_w+nx});}}}
}

// Prosty bot: idź do najbliższego (po ścieżce) wroga albo schodów, atakuj z dystansu, gdy się da.
// Cel wybiera odległość po ścieżce (do 7 pól drogi; na etapie z bossem każdy problem), więc z każdym krokiem do celu
// jego odległość maleje i bot nie przeskakuje między dwoma celami (dawniej odległość w linii prostej przez ścianę:
// krok w stronę celu oddalał go, bot wracał do schodów i kręcił się do limitu kroków). Przed ciosem bossa schodzi
// z czerwonych pól w stronę celu (dawniej zawsze w tę samą stronę - w wąskim korytarzu cofał się bez końca).
static void bot_step(game& g)
{
    static const int d[4][2]={{1,0},{-1,0},{0,1},{0,-1}};
    if(g.has_offer()) { if(g.offer_is_better()) g.accept_offer(); else g.decline_offer(); }
    if(g.bot_pending()) return;   // v0.21.50 cz. 3: wydarzenie, premia z wydarzenia, cecha narzędzia, narzędzie
    if(g.can_open_secret() && g.hero.x == g.secret_front_x() && g.hero.y == g.secret_front_y()   // przed magazynem: otwórz
       && g.player_move(g.secret_x - g.hero.x, g.secret_y - g.hero.y)) return;
    if(g.thermos > 0 && g.hero.hp * 100 < g.hero.max_hp * data::bot_drink_below_pct && g.player_drink()) return;
    static int hd[map_h][map_w], td[map_h][map_w];
    bot_cost(g, g.hero.x, g.hero.y, hd);   // koszt drogi (błoto droższe)
    int tx = g.stairs_x, ty = g.stairs_y, best = 999999;
    { int gx, gy; if(g.bot_goal(gx, gy) && hd[gy][gx] >= 0) { tx = gx; ty = gy; } }   // klucz, magazyn, skrzynia, wydarzenie
    if(g.stairs_locked())   // pieczątki (Akt 0): najpierw najbliższy dokument, potem schody
    {
        int bdoc = 999999;
        for(int i=0;i<g.pickups_count;++i){ const pickup& p=g.pickups[i]; int dd=hd[p.y][p.x];
            if(p.active&&p.type==document&&dd>=0&&dd<bdoc){bdoc=dd;tx=p.x;ty=p.y;} }
    }
    for(int i=0;i<g.enemies_count;++i){ auto& e=g.enemies[i]; int dd=hd[e.y][e.x]; if(e.alive&&dd>=0&&dd<best&&(dd<32||g.stairs_x<0)){best=dd;tx=e.x;ty=e.y;} }
    bool has_target = tx>=0 && hd[ty][tx]>=0;
    if(has_target) bot_cost(g, tx, ty, td);
    if(g.danger_cell(g.hero.x, g.hero.y))   // zapowiedziany cios bossa: zejdź z czerwonych pól (jak człowiek; nie wraca na nie)
    {
        int bk=-1, bs=-1000000;
        for(int k=0;k<4;++k){ int nx=g.hero.x+d[k][0], ny=g.hero.y+d[k][1];
            if(!g.lv.passable(nx,ny)||g.occupied(nx,ny)) continue;
            int s=(g.danger_cell(nx,ny)?0:10000)-(has_target&&td[ny][nx]>=0?td[ny][nx]:5000); if(s>bs){bs=s;bk=k;} }
        if(bk>=0 && g.player_move(d[bk][0],d[bk][1])) return;
    }
    if(g.nearest_target() >= 0 && g.weapon().range > 1) { g.player_attack_nearest(); return; }
    if(!has_target||(tx==g.hero.x&&ty==g.hero.y)){ g.player_wait(); return; }
    for(int k=0;k<4;++k)   // krok na sąsiednie pole bliżej celu
    {
        int nx=g.hero.x+d[k][0], ny=g.hero.y+d[k][1];
        if(!bot_open(g,nx,ny)||td[ny][nx]<0||td[ny][nx]+bot_w(g,nx,ny)+bot_w(g,g.hero.x,g.hero.y)!=td[g.hero.y][g.hero.x]) continue;
        if(!g.lv.passable(nx,ny)){ g.player_wait(); return; }   // mur Ścianki na drodze: czekaj, aż zniknie
        if(g.danger_cell(nx,ny)&&g.enemy_at(nx,ny)<0){ g.player_wait(); return; }   // nie wchodzi na czerwone pola przed ciosem
        if(!g.player_move(d[k][0],d[k][1])) g.player_wait();
        return;
    }
    g.player_wait();
}

// Wariant: moc, gdy widoczny wróg jest blisko; celowanie w najbliższy widoczny cel w zasięgu (Bot.StepSmart w C#).
// Paczka sprzętu tej samej lub lepszej jakości: zakłada (wymiana cechy), gorsza: zostawia. Termos poniżej połowy HP.
static void bot_step_smart(game& g)
{
    if(g.has_offer()) { if(g.offer_rarity >= g.equipped[g.offer_slot]) g.accept_offer(); else g.decline_offer(); }
    if(g.bot_pending()) return;
    if(g.thermos > 0 && g.hero.hp * 2 < g.hero.max_hp && g.player_drink()) return;
    for(int k = 0; k < data::repairs_count; ++k)   // naprawy: Kładka przy kałużach; Załataj przy niskim HP (problem w polu widzenia)
    {
        bool want = data::repairs[k].effect == repair_effect::bridge || g.hero.hp * 3 < g.hero.max_hp;
        if(want && g.repair_blocked(k) == game::repair_ok && g.player_repair(k)) return;
    }
    if(g.helper_called < 0 && g.nearest_visible_enemy() >= 0)   // brygada: przy pierwszym problemie na etapie (kolejny fachowiec co etap)
        for(int k = 0; k < data::brigade_count; ++k)
        {
            int h = (g.stage + k) % data::brigade_count;
            if(g.helper_blocked(h) == game::helper_ok && g.call_helper(h)) return;
        }
    if(!g.danger_cell(g.hero.x, g.hero.y) && g.ability_cd == 0)
    {
        int t = g.nearest_visible_enemy();
        // Ścianka tylko w obronie (HP poniżej połowy) i nie na problem, który stoi w miejscu - mur zasłania drogę do celu,
        // a problem, który nie podchodzi (stoi, strzela z dystansu), zostaje za nim: bot czekałby na zniknięcie muru bez końca
        const bool pointless = t >= 0 && g.cdef().ability == ability_effect::wall
                               && (g.hero.hp * 2 >= g.hero.max_hp || g.has_tag(g.enemies[t], tag_stationary));
        if(t >= 0 && ! pointless && cheb(g.hero.x, g.hero.y, g.enemies[t].x, g.enemies[t].y) <= 2 && g.player_ability()) return;
    }
    if(!g.danger_cell(g.hero.x, g.hero.y))
    {
        int8_t targets[max_enemies]; int n = g.targets_in_range(targets, max_enemies);
        if(n > 0 && g.player_attack(targets[0])) return;
    }
    bot_step(g);
}

static void bot_shop(game& g) { for(int i = 0; i < data::hurtownia_count; ++i) { g.hurtownia_buy(i); g.bot_pending(); } }

// ------------------------------------------------------------------ skrót stanu (StateDigest.cs)
struct fnv { uint32_t h = 2166136261u; void add(int v) { uint32_t u = uint32_t(v); for(int i = 0; i < 4; ++i) { h ^= (u >> (8 * i)) & 0xFF; h *= 16777619u; } } };

static uint32_t digest(const game& g)
{
    fnv f;
    f.add(g.hero.x); f.add(g.hero.y); f.add(g.hero.hp); f.add(g.hero.max_hp); f.add(g.hero.alive);
    f.add(g.turns); f.add(g.score); f.add(g.cash); f.add(g.xp_pct); f.add(g.run_xp); f.add(g.hero_level); f.add(int(g.st));
    f.add(int(g.r.s)); f.add(g.log_serial); f.add(g.ability_cd); f.add(g.def_bonus); f.add(g.dmg_bonus); f.add(g.stage); f.add(g.tier);
    f.add(g.kills); f.add(g.slam_timer); f.add(g.slam_x); f.add(g.slam_y); f.add(g.slam_counter); f.add(g.weapon_override);
    f.add(g.walls_count); f.add(g.enemies_count);
    for(int i = 0; i < g.enemies_count; ++i)
    {
        const actor& e = g.enemies[i];
        f.add(e.x); f.add(e.y); f.add(e.hp); f.add(e.max_hp); f.add(e.def_id); f.add(e.alive); f.add(e.awake); f.add(e.stun);
        f.add(e.flags); f.add(e.grow); f.add(e.timer);   // v0.21.49 cz. 2: zachowania problemów
        f.add(e.elite); f.add(e.wet);                     // v0.21.50 cz. 2: elity, mokry
    }
    f.add(g.pickups_count);
    for(int i = 0; i < g.pickups_count; ++i)
    {
        const pickup& p = g.pickups[i];
        f.add(p.x); f.add(p.y); f.add(p.type); f.add(p.active); f.add(p.arg); f.add(p.trait);
    }
    for(int i = 0; i < status_slots; ++i) f.add(g.hero_status[i]);
    for(int i = 0; i < max_gear_slots; ++i) f.add(g.equipped[i]);
    for(int i = 0; i < max_gear_slots; ++i) f.add(g.equipped_trait[i]);
    f.add(g.thermos); f.add(g.offer_slot); f.add(g.offer_rarity); f.add(g.offer_trait);
    f.add(g.hits_count);
    for(int i = 0; i < g.hits_count; ++i) { const hit& h = g.hits[i]; f.add(h.x); f.add(h.y); f.add(h.amount); f.add(h.on_hero); f.add(h.kind); }
    f.add(g.act_cleared); f.add(g.act_bonus); f.add(g.stage_damage); f.add(g.stage_kills); f.add(g.tools_found);
    for(const message& m : g.log)
    {
        f.add(m.n); f.add(m.kind); f.add(m.repeat);
        for(int i = 0; i < m.n; ++i) f.add((unsigned char)m.s[i]);
    }
    for(int y = 0; y < map_h; ++y) for(int x = 0; x < map_w; ++x) f.add(int(g.lv.t[y][x]));
    // v0.21.43: wydarzenie na placu, liczniki zleceń
    f.add(g.stage_event); f.add(g.boss_wake_damage); f.add(g.powers_used); f.add(g.brand_found); f.add(g.clean_bosses);
    // v0.21.46: wezwania bossa (Inspekcja Pracy)
    f.add(g.summon_counter); f.add(g.summons_used);
    // v0.21.47: pogoda dnia, brygada
    f.add(g.weather);
    f.add(g.helper_called); f.add(g.guard_turns); f.add(g.ally_turns); f.add(g.ally_x); f.add(g.ally_y);
    // v0.21.48: ścieżka, materiały, kładki, codzienna budowa, dni etapów
    f.add(g.stage_path); f.add(g.next_path);
    for(int i = 0; i < 3; ++i) f.add(g.mats[i]);
    f.add(g.bridges);
    for(int i = 0; i < g.bridges; ++i) { f.add(g.bridge_x[i]); f.add(g.bridge_y[i]); }
    f.add(g.daily); f.add(g.daily_day);
    for(int i = 0; i < max_stages; ++i) f.add(g.stage_days[i]);
    // v0.21.49: Respekt, reszty procentów obrażeń, Druga szansa
    f.add(g.respect); f.add(g.dmg_carry); f.add(g.taken_carry); f.add(g.second_used);
    // v0.21.49 cz. 2: wybuch, porywy (kolejny poryw), pole widzenia z pyłem
    f.add(g.blast_x); f.add(g.blast_y); f.add(g.blast_timer); f.add(g.blast_dmg); f.add(g.gust_in()); f.add(g.sight_radius());
    // v0.21.49 cz. 3: Akt 0 - pierwszy etap, dokumenty (pieczątki)
    f.add(g.first_stage); f.add(g.docs); f.add(g.stairs_locked());
    for(int i = 0; i < max_enemy_types; ++i) f.add(g.kills_by_type[i]);
    // v0.21.50 cz. 2: premie po etapie, synergie, kombinacje stanów
    f.add(int(uint32_t(g.boons))); f.add(int(uint32_t(g.boons >> 32)));
    for(int i = 0; i < 3; ++i) f.add(g.boon_offer[i]);
    f.add(g.boon_rerolls); f.add(g.hit_ctx); f.add(g.combo_events); f.add(g.synergy_mask());
    f.add(g.hero_defense()); f.add(g.crit_pct()); f.add(g.thermos_cap()); f.add(g.coffee_heal());
    // v0.21.50 cz. 3: wydarzenia z wyborem, ulepszenie narzędzia, ukryte pomieszczenia
    f.add(g.boon_salt); f.add(g.pending_event); f.add(g.stage_choice); f.add(g.stage_choice_pick); f.add(g.choice_done);
    f.add(g.events_seen); f.add(g.event_dmg); f.add(g.event_def);
    f.add(g.weapon_lvl); f.add(g.weapon_trait); f.add(g.trait_pending); f.add(g.tool_offer); f.add(g.tool_offer_pickup);
    f.add(g.secret_x); f.add(g.secret_y); f.add(g.secret_kind); f.add(g.secret_dir); f.add(g.secret_rx); f.add(g.secret_ry);
    f.add(g.secret_rw); f.add(g.secret_rh); f.add(g.secret_open); f.add(g.key_holder); f.add(g.keys); f.add(g.secrets_found);
    f.add(g.upgrade_price()); f.add(g.can_open_secret());
    // v0.21.50 cz. 4: podsumowanie budowy (ciosy, oś czasu), wyzwanie tygodnia
    for(const recap_hit& h : g.last_hits) { f.add(h.src); f.add(h.elite); f.add(h.kind); f.add(h.stage); f.add(h.amount); }
    f.add(g.worst_hit.src); f.add(g.worst_hit.elite); f.add(g.worst_hit.kind); f.add(g.worst_hit.stage); f.add(g.worst_hit.amount);
    f.add(g.best_hit); f.add(g.best_hit_def); f.add(g.best_hit_crit); f.add(g.blast_src);
    for(int i = 0; i < max_stages; ++i) { f.add(g.stage_kill_log[i]); f.add(g.stage_boon[i]); f.add(g.stage_event_log[i]); f.add(g.stage_flags[i]); }
    f.add(g.elites_killed); f.add(g.combos_run); f.add(g.weekly_week); f.add(g.bonus.weekly);
    return f.h;
}

// ------------------------------------------------------------------ JSON
static std::string out;
static int event_hits[16];   // statystyka wydarzeń na placu (wypis na końcu)
static int helper_hits[8];   // statystyka wezwań brygady
static void w(const char* s) { out += s; }
static void wi(long v) { out += std::to_string(v); }
static void key(const char* k) { out += '"'; out += k; out += "\":"; }
static void hex_bytes(const char* s, int n)
{
    static const char* hx = "0123456789abcdef";
    out += '"';
    for(int i = 0; i < n; ++i) { unsigned char c = (unsigned char)s[i]; out += hx[c >> 4]; out += hx[c & 15]; }
    out += '"';
}

static void snapshot(const game& g, int step)
{
    w("{"); key("step"); wi(step);
    w(","); key("stage"); wi(g.stage); w(","); key("tier"); wi(g.tier); w(","); key("st"); wi(int(g.st));
    w(","); key("turns"); wi(g.turns); w(","); key("score"); wi(g.score); w(","); key("cash"); wi(g.cash);
    w(","); key("xpPct"); wi(g.xp_pct); w(","); key("xpBanked"); wi(g.xp_banked); w(","); key("runXp"); wi(g.run_xp);
    w(","); key("heroLevel"); wi(g.hero_level); w(","); key("rng"); wi(g.r.s);
    w(","); key("hero"); w("["); wi(g.hero.x); w(","); wi(g.hero.y); w(","); wi(g.hero.hp); w(","); wi(g.hero.max_hp); w(","); wi(g.hero.alive); w("]");
    w(","); key("defBonus"); wi(g.def_bonus); w(","); key("dmgBonus"); wi(g.dmg_bonus);
    w(","); key("kills"); wi(g.kills); w(","); key("abilityCd"); wi(g.ability_cd); w(","); key("boss"); wi(g.boss);
    w(","); key("stairs"); w("["); wi(g.stairs_x); w(","); wi(g.stairs_y); w("]");
    w(","); key("weaponOverride"); wi(g.weapon_override); w(","); key("actBonus"); wi(g.act_bonus);
    w(","); key("actCleared"); wi(g.act_cleared); w(","); key("actKills"); wi(g.act_kills);
    w(","); key("toolsFound"); wi(g.tools_found); w(","); key("logSerial"); wi(g.log_serial);
    w(","); key("stageDamage"); wi(g.stage_damage); w(","); key("stageKills"); wi(g.stage_kills); w(","); key("stageStartTurn"); wi(g.stage_start_turn);
    w(","); key("slam"); w("["); wi(g.slam_timer); w(","); wi(g.slam_x); w(","); wi(g.slam_y); w(","); wi(g.slam_counter); w("]");
    w(","); key("equipped"); w("["); for(int i = 0; i < max_gear_slots; ++i) { if(i) w(","); wi(g.equipped[i]); } w("]");
    w(","); key("equippedTrait"); w("["); for(int i = 0; i < max_gear_slots; ++i) { if(i) w(","); wi(g.equipped_trait[i]); } w("]");
    w(","); key("thermos"); wi(g.thermos); w(","); key("thermosCap"); wi(g.thermos_cap());
    w(","); key("stageEvent"); wi(g.stage_event);
    w(","); key("weather"); wi(g.weather); w(","); key("weaponRange"); wi(g.weapon_range());
    w(","); key("investor"); w("["); wi(g.bonus.investor); w(","); wi(g.income(100)); w(","); wi(g.slam_every()); w(","); wi(g.shop_closed()); w("]");
    w(","); key("brigade"); w("["); wi(g.helper_called); w(","); wi(g.guard_turns); w(","); wi(g.ally_turns); w(","); wi(g.ally_x);
    w(","); wi(g.ally_y); w(","); wi(g.hero_defense()); w("]");
    w(","); key("counters"); w("["); wi(g.powers_used); w(","); wi(g.brand_found); w(","); wi(g.clean_bosses); w(",");
    wi(g.boss_wake_damage); w("]");
    w(","); key("stats"); w("["); wi(g.hero_stat(stat::str)); w(","); wi(g.hero_stat(stat::agi)); w(","); wi(g.hero_stat(stat::intel)); w(",");
    wi(g.luck()); w(","); wi(g.crit_pct()); w(","); wi(g.sight_radius()); w(","); wi(g.ability_cooldown()); w("]");
    w(","); key("path"); w("["); wi(g.stage_path); w(","); wi(g.path_offer(0)); w(","); wi(g.path_offer(1)); w("]");
    w(","); key("mats"); w("["); for(int i = 0; i < 3; ++i) { if(i) w(","); wi(g.mats[i]); } w("]");
    w(","); key("bridges"); w("["); for(int i = 0; i < g.bridges; ++i) { if(i) w(","); w("["); wi(g.bridge_x[i]); w(","); wi(g.bridge_y[i]); w("]"); } w("]");
    w(","); key("daily"); w("["); wi(g.daily); w(","); wi(g.daily_day); w("]");
    w(","); key("stageDays"); w("["); for(int i = 0; i < max_stages; ++i) { if(i) w(","); wi(g.stage_days[i]); } w("]");
    w(","); key("blast"); w("["); wi(g.blast_x); w(","); wi(g.blast_y); w(","); wi(g.blast_timer); w(","); wi(g.blast_dmg); w(",");
    wi(g.gust_in()); w(","); wi(g.gust_dir()); w("]");
    w(","); key("offer"); w("["); wi(g.offer_slot); w(","); wi(g.offer_rarity); w(","); wi(g.offer_trait); w("]");
    w(","); key("respect"); w("["); wi(g.respect); w(","); wi(g.stage_respect()); w(","); wi(g.dmg_carry); w(","); wi(g.taken_carry); w(",");
    wi(g.second_used); w(","); wi(g.dodge_pct()); w(","); wi(g.coffee_heal()); w(","); wi(g.bonus.gear_slots); w(","); wi(g.bonus.tools); w("]");
    w(","); key("act0"); w("["); wi(g.first_stage); w(","); wi(g.docs); w(","); wi(g.docs_needed()); w(","); wi(g.stairs_locked()); w(",");
    wi(g.stage_number()); w(","); wi(g.stages_in_run()); w("]");
    w(","); key("heroStatus"); w("["); for(int i = 0; i < status_slots; ++i) { if(i) w(","); wi(g.hero_status[i]); } w("]");
    w(","); key("boons"); w("["); wi(long(uint32_t(g.boons))); w(","); wi(long(uint32_t(g.boons >> 32))); w(",");
    wi(g.boon_offer[0]); w(","); wi(g.boon_offer[1]); w(","); wi(g.boon_offer[2]); w(","); wi(g.boon_rerolls); w(",");
    wi(g.synergy_mask()); w(","); wi(g.rerolls_left()); w(","); wi(g.reroll_price()); w(","); wi(g.hero_defense()); w(",");
    wi(g.luck()); w(","); wi(g.dodge_pct()); w("]");
    w(","); key("part3"); w("["); wi(g.boon_salt); w(","); wi(g.pending_event); w(","); wi(g.stage_choice); w(","); wi(g.stage_choice_pick);
    w(","); wi(g.choice_done); w(","); wi(g.events_seen); w(","); wi(g.event_dmg); w(","); wi(g.event_def); w(","); wi(g.weapon_lvl);
    w(","); wi(g.weapon_trait); w(","); wi(g.trait_pending); w(","); wi(g.tool_offer); w(","); wi(g.tool_offer_pickup); w(",");
    wi(g.secret_x); w(","); wi(g.secret_y); w(","); wi(g.secret_kind); w(","); wi(g.secret_dir); w(","); wi(g.secret_rx); w(",");
    wi(g.secret_ry); w(","); wi(g.secret_rw); w(","); wi(g.secret_rh); w(","); wi(g.secret_open); w(","); wi(g.key_holder); w(",");
    wi(g.keys); w(","); wi(g.secrets_found); w(","); wi(g.upgrade_price()); w(","); wi(g.can_open_secret()); w("]");
    w(","); key("part4"); w("[");
    for(const recap_hit& h : g.last_hits) { wi(h.src); w(","); wi(h.elite); w(","); wi(h.kind); w(","); wi(h.stage); w(","); wi(h.amount); w(","); }
    wi(g.worst_hit.src); w(","); wi(g.worst_hit.elite); w(","); wi(g.worst_hit.kind); w(","); wi(g.worst_hit.stage); w(","); wi(g.worst_hit.amount);
    w(","); wi(g.best_hit); w(","); wi(g.best_hit_def); w(","); wi(g.best_hit_crit); w(","); wi(g.blast_src);
    for(int i = 0; i < max_stages; ++i) { w(","); wi(g.stage_kill_log[i]); w(","); wi(g.stage_boon[i]); w(","); wi(g.stage_event_log[i]); w(","); wi(g.stage_flags[i]); }
    w(","); wi(g.elites_killed); w(","); wi(g.combos_run); w(","); wi(g.weekly_week); w(","); wi(g.bonus.weekly);
    w(","); wi(g.elite_chance()); w(","); wi(g.shop_closed()); w(","); wi(recap_tip_index(g)); w("]");
    w(","); key("killsByType"); w("["); for(int i = 0; i < max_enemy_types; ++i) { if(i) w(","); wi(g.kills_by_type[i]); } w("]");
    w(","); key("rooms"); w("[");
    for(int i = 0; i < g.lv.rooms_count; ++i) { if(i) w(","); const room& r = g.lv.rooms[i]; w("["); wi(r.x); w(","); wi(r.y); w(","); wi(r.w); w(","); wi(r.h); w("]"); }
    w("]");
    w(","); key("map"); w("[");
    for(int y = 0; y < map_h; ++y)
    {
        if(y) w(",");
        w("\"");
        for(int x = 0; x < map_w; ++x) out += g.lv.t[y][x] == tile::wall ? '#' : (g.lv.t[y][x] == tile::floor ? '.' : '>');
        w("\"");
    }
    w("]");
    w(","); key("fov"); w("[");
    for(int y = 0; y < map_h; ++y)
    {
        if(y) w(",");
        w("\""); for(int x = 0; x < map_w; ++x) out += char('0' + g.fov[y][x]); w("\"");
    }
    w("]");
    w(","); key("enemies"); w("[");
    for(int i = 0; i < g.enemies_count; ++i)
    {
        const actor& e = g.enemies[i];
        if(i) w(",");
        w("["); wi(e.def_id); w(","); wi(e.x); w(","); wi(e.y); w(","); wi(e.hp); w(","); wi(e.max_hp); w(","); wi(e.alive); w(","); wi(e.awake); w(","); wi(e.stun);
        w(","); wi(e.flags); w(","); wi(e.grow); w(","); wi(e.timer); w(","); wi(e.elite); w(","); wi(e.wet); w("]");
    }
    w("]");
    w(","); key("pickups"); w("[");
    for(int i = 0; i < g.pickups_count; ++i)
    {
        const pickup& p = g.pickups[i];
        if(i) w(",");
        w("["); wi(p.x); w(","); wi(p.y); w(","); wi(p.type); w(","); wi(p.active); w(","); wi(p.arg); w(","); wi(p.trait); w("]");
    }
    w("]");
    w(","); key("walls"); w("[");
    for(int i = 0; i < g.walls_count; ++i) { if(i) w(","); w("["); wi(g.walls[i].x); w(","); wi(g.walls[i].y); w(","); wi(g.walls[i].turns); w("]"); }
    w("]");
    w(","); key("log"); w("[");
    for(int i = 0; i < log_lines; ++i)
    {
        if(i) w(",");
        w("{"); key("hex"); hex_bytes(g.log[i].s, g.log[i].n); w(","); key("kind"); wi(g.log[i].kind); w(","); key("repeat"); wi(g.log[i].repeat); w("}");
    }
    w("]}");
}

static void profile_json(const profile& p)
{
    w("{"); key("best"); wi(p.best); w(","); key("runs"); wi(p.runs); w(","); key("wins"); wi(p.wins); w(","); key("xp"); wi(p.xp);
    w(","); key("badges"); wi(p.badges); w(","); key("catalog"); wi(p.catalog); w(","); key("classWins"); wi(p.class_wins);
    w(","); key("toolsFound"); wi(p.tools_found); w(","); key("houses"); w("[");
    for(int i = 0; i < p.houses_count; ++i) { if(i) w(","); wi(p.houses[i]); }
    w("]"); w(","); key("killsTotal"); wi(p.kills_total); w(","); key("powersTotal"); wi(p.powers_total);
    w(","); key("brandTotal"); wi(p.brand_total); w(","); key("cleanBosses"); wi(p.clean_bosses);
    w(","); key("contracts"); wi(p.contracts); w(","); key("keepsake"); wi(p.keepsake);
    w(","); key("brigade"); wi(p.brigade); w(","); key("investor"); wi(p.investor);
    w(","); key("bestStake"); w("["); for(int i = 0; i < 8; ++i) { if(i) w(","); wi(p.best_stake[i]); } w("]");
    w(","); key("keepsakeRuns"); w("["); for(int i = 0; i < max_keepsakes; ++i) { if(i) w(","); wi(p.keepsake_runs[i]); } w("]");
    w(","); key("daily"); w("["); wi(p.daily_won); w(","); wi(p.daily_runs); w("]");
    w(","); key("dailyDay"); w("["); for(int i = 0; i < daily_slots; ++i) { if(i) w(","); wi(p.daily_day[i]); } w("]");
    w(","); key("dailyScore"); w("["); for(int i = 0; i < daily_slots; ++i) { if(i) w(","); wi(p.daily_score[i]); } w("]");
    w(","); key("respect"); w("["); wi(p.respect); w(","); wi(p.respect_total); w(","); wi(p.run_respect); w(","); wi(p.rewards); w(",");
    wi(p.class_wins_hi); w("]");
    w(","); key("catalogHi"); wi(p.catalog_hi);
    w(","); key("tutorial"); w("["); wi(p.tutorial); w(","); wi(p.classes_seen); w("]");
    w(","); key("weekly"); w("["); wi(p.weekly_won); w(","); wi(p.weekly_runs);
    for(int i = 0; i < weekly_slots; ++i) { w(","); wi(p.weekly_week[i]); w(","); wi(p.weekly_score[i]); } w("]");
    w(","); key("story"); w("["); wi(long(p.story)); w(","); wi(long(p.story_new)); w(","); wi(estate_decor(p)); w("]");
    w(","); key("sram"); hex_bytes(reinterpret_cast<const char*>(&p), sizeof p);   // profil bajt po bajcie jak w SRAM
    w("}");
}

// badges/contracts: odznaki i zlecenia w profilu przed budową (uprawnienia, odblokowane pamiątki);
// keepsake: wybrana pamiątka + 1; keepsake_runs: budowy z nią przed tą (ranga)
// paths: wybór ścieżki na harmonogramie (0 = zawsze pierwsza z oferty, 1 = na przemian: stage & 1);
// daily: numer dnia codziennej budowy (0 = zwykła budowa; zawód i seed z dnia, bez Szkoleń)
// respect: rangi Respektu (0 = brak, 1 = wszystkie maksymalne); rewards: odebrane nagrody za odbiór (narzędzia, buty, pas, zawody)
struct scenario { int cls; uint32_t seed; int diff; bool full_mods; bool smart; bool shop; bool ngplus; int steps;
                  int badges = 0; int contracts = 0; int keepsake = 0; int keepsake_runs = 0; int investor = 0; int paths = 0; int daily = 0;
                  int respect = 0; int rewards = 0; int weekly = 0; };

int main(int argc, char** argv)
{
    const char* dir = argc > 1 ? argv[1] : ".";
    std::vector<scenario> sc;
    for(int c = 0; c < data::classes_count; ++c)                         // bot z core_tests.cpp, Normalny
        sc.push_back({ c, 1000u + uint32_t(c) * 7919u, 1, false, false, false, false, 4000 });
    for(int c = 0; c < data::classes_count; ++c)                         // bot z mocą i celowaniem, różne poziomy, Hurtownia
        sc.push_back({ c, 424242u + uint32_t(c) * 104729u, c % 3, false, true, true, true, 4000 });
    sc.push_back({ 2, 7u, 0, true, true, true, true, 6000 });            // pełne Szkolenia, Łatwy, NG+
    sc.push_back({ 3, 99u, 0, true, true, true, true, 6000 });
    sc.push_back({ 0, 123456789u, 2, true, true, true, true, 4000 });
    // v0.21.43: uprawnienia z odznak, pamiątki (rangi I-III), wydarzenia na placu, liczniki zleceń
    const int all_badges = (1 << data::badges_count) - 1, all_contracts = (1 << data::contracts_count) - 1;
    sc.push_back({ 1, 31337u, 1, false, true, true, true, 6000, all_badges, 0, 1, 0 });            // Termos babci I, wszystkie odznaki
    sc.push_back({ 4, 2024u, 1, true, true, true, true, 6000, 0x41, all_contracts, 3, 8 });        // Szczęśliwa kielnia III, Kolekcjoner
    sc.push_back({ 5, 1234u, 2, true, true, true, false, 5000, all_badges, all_contracts, 5, 3 });  // Notes kierownika II
    sc.push_back({ 0, 55555u, 1, false, false, false, false, 4000, 0x108, all_contracts, 4, 0 });  // Stara poziomica I, bot z testów
    sc.push_back({ 2, 9001u, 0, true, true, true, true, 6000, 0x01, 0, 2, 2 });                    // Kask ojca I (z odznaki)
    // v0.21.47: tryb inwestora (wszystkie modyfikatory; budżet i Hurtownia zamknięta)
    const int all_investor = (1 << data::investor_count) - 1;
    sc.push_back({ 2, 780u, 0, true, true, true, true, 6000, all_badges, all_contracts, 1, 8, all_investor });
    sc.push_back({ 4, 4242u, 1, true, true, true, true, 6000, 0, 0, 0, 0, 0x09 });
    // v0.21.48: wybór ścieżki na przemian (materiały, naprawy, Hurtownia za materiały), codzienna budowa
    sc.push_back({ 1, 48048u, 1, true, true, true, true, 6000, 0, 0, 1, 0, 0, 1 });
    sc.push_back({ 3, 20260925u, 0, false, true, true, false, 5000, 0, 0, 0, 0, 0, 1 });
    sc.push_back({ 0, 0u, 1, false, true, true, false, 5000, 0, 0, 0, 0, 0, 1, daily_number(2026, 9, 25) });
    // v0.21.49: pełny Respekt (Druga szansa, procenty obrażeń, kawa, rabaty), nagrody za odbiór (buty, pas, nowe narzędzia),
    // nowe zawody z Respektem; bot z testów i "smart" (moce Rynna, Narzut, Taran)
    const int all_rewards = rewards_available();
    sc.push_back({ 6, 4949u, 1, true, true, true, true, 6000, 0, 0, 1, 0, 0, 1, 0, 1, all_rewards });
    sc.push_back({ 7, 5050u, 1, false, true, true, false, 5000, 0, 0, 0, 0, 0, 0, 0, 1, all_rewards });
    sc.push_back({ 8, 5151u, 2, true, true, true, true, 6000, all_badges, all_contracts, 2, 3, 0x21, 1, 0, 1, all_rewards });
    sc.push_back({ 1, 5252u, 1, true, false, false, false, 4000, 0, 0, 1, 0, 0, 0, 0, 1, 3 });
    sc.push_back({ 4, 5353u, 0, false, true, true, true, 6000, 0, 0, 0, 0, 0, 1, 0, 0, all_rewards });
    sc.push_back({ 1, 5454u, 2, false, false, false, false, 4000, 0, 0, 1, 0, 0, 0, 0, 1, 0 });            // Trudny: Druga szansa, potem koniec
    // v0.21.49 cz. 3: Akt 0 (nagroda za odbiór) - pieczątki, Decyzja odmowna z drugą fazą; bot z testów i "smart"
    sc.push_back({ 0, 4900u, 1, false, false, false, false, 4000, 0, 0, 1, 0, 0, 0, 0, 0, all_rewards });
    sc.push_back({ 5, 4901u, 0, true, true, true, false, 5000, 0, 0, 0, 0, 0, 1, 0, 1, all_rewards });
    // v0.21.50 cz. 4: wyzwania tygodnia (każda zasada: zawód, bez kawy, elity, deszcz, bez Hurtowni, HP / ciosy, budżet)
    for(int wk = 1; wk <= data::weekly_count; ++wk)
        sc.push_back({ 0, 0u, 1, false, wk % 2 == 0, true, false, 5000, 0, 0, 0, 0, 0, wk % 3 == 0, 0, 0, 0, wk });

    for(size_t si = 0; si < sc.size(); ++si)
    {
        const scenario& s = sc[si];
        profile p; profile_reset(p);
        if(s.full_mods)
        {
            for(int i = 0; i < data::upgrades_count; ++i) p.levels[i] = uint8_t(data::upgrades[i].levels);
            p.tools = uint8_t((1 << data::tools_count) - 1);
            p.brigade = uint8_t((1 << data::brigade_count) - 1);
        }
        p.badges = uint16_t(s.badges); p.contracts = uint8_t(s.contracts); p.keepsake = uint8_t(s.keepsake);
        if(s.investor) { p.wins = 1; p.investor = uint8_t(s.investor); }   // tryb inwestora po pierwszej wygranej
        if(s.keepsake > 0) p.keepsake_runs[s.keepsake - 1] = uint8_t(s.keepsake_runs);
        if(s.respect) for(int i = 0; i < data::respect_count; ++i) p.respect_ranks[i] = uint8_t(data::respect[i].ranks);
        p.rewards = uint8_t(s.rewards);
        run_mods m = mods(p);   // przed start_run: ranga pamiątki z budów przed tą
        static game g;
        if(s.weekly > 0) start_weekly(g, s.weekly);
        else if(s.daily > 0) start_daily(g, s.daily);
        else g.new_run(s.cls, s.seed, s.diff, m);
        start_run(p);
        out.clear();
        w("{"); key("cls"); wi(s.cls); w(","); key("seed"); wi(s.seed); w(","); key("diff"); wi(s.diff);
        w(","); key("fullMods"); wi(s.full_mods); w(","); key("smart"); wi(s.smart); w(","); key("shop"); wi(s.shop);
        w(","); key("ngplus"); wi(s.ngplus); w(","); key("steps"); wi(s.steps);
        w(","); key("badges"); wi(s.badges); w(","); key("contracts"); wi(s.contracts); w(","); key("keepsake"); wi(s.keepsake);
        w(","); key("keepsakeRuns"); wi(s.keepsake_runs); w(","); key("investor"); wi(s.investor);
        w(","); key("paths"); wi(s.paths); w(","); key("daily"); wi(s.daily);
        w(","); key("respect"); wi(s.respect); w(","); key("rewards"); wi(s.rewards); w(","); key("weekly"); wi(s.weekly);
        w(","); key("snapshots"); w("[");
        snapshot(g, 0);
        std::vector<uint32_t> digests;
        bool did_ng = false;
        int step = 0;
        for(; step < s.steps; ++step)
        {
            if(g.st == status::stage_clear)
            {
                check_badges(p, g); check_contracts(p); bank_xp(p, g);
                if(g.act_cleared && s.shop && ! g.shop_closed()) bot_shop(g);
                g.bot_upgrade();   // v0.21.50 cz. 3: jak bot balansu - ulepszenie narzędzia, jeśli stać
                if(s.paths) g.choose_path(g.stage & 1);
                // v0.21.50 cz. 2: premia 1 z 3 - bot z rdzenia; ścieżki na przemian: też losowanie (płatne) i wybór wg etapu
                if(g.bot_wants_reroll()) g.reroll_boons();
                if(s.paths && g.stage % 4 == 1 && g.can_reroll()) g.reroll_boons();
                if(g.has_boon_offer()) g.pick_boon(s.paths ? g.stage % 3 : g.bot_boon_choice());
                g.next_stage();
                w(","); snapshot(g, step);
                digests.push_back(digest(g)); g.hits_count = 0; g.combo_events = 0;
                continue;
            }
            if(g.st == status::won && s.ngplus && ! did_ng)
            {
                if(g.score > p.best) p.best = g.score;
                record_win(p); add_house(p, g); check_badges(p, g); check_contracts(p); bank_xp(p, g);
                did_ng = true;
                g.new_game_plus();
                w(","); snapshot(g, step);
                digests.push_back(digest(g)); g.hits_count = 0; g.combo_events = 0;
                continue;
            }
            if(g.st != status::playing) break;
            int called = g.helper_called;
            if(s.smart) bot_step_smart(g); else bot_step(g);
            if(called < 0 && g.helper_called >= 0) ++helper_hits[g.helper_called];
            if(g.turns == g.stage_start_turn + 1 && g.stage_event >= 0) ++event_hits[g.stage_event];
            digests.push_back(digest(g)); g.hits_count = 0; g.combo_events = 0;   // warstwa GBA zeruje trafienia po każdej turze
        }
        if(g.score > p.best) p.best = g.score;
        if(g.st == status::won) { record_win(p); add_house(p, g); }
        check_badges(p, g); check_contracts(p); bank_xp(p, g);
        if(g.daily) record_daily(p, g.daily_day, g.score, g.st == status::won);
        if(g.weekly_week) record_weekly(p, g.weekly_week, g.score, g.st == status::won);
        story_check(p, &g);   // v0.21.50 cz. 4: fabuła - wątki za kamienie milowe
        w("]"); w(","); key("endStep"); wi(step);
        w(","); key("final"); snapshot(g, step);
        w(","); key("profile"); profile_json(p);
        w(","); key("digests"); w("[");
        char buf[16];
        for(size_t i = 0; i < digests.size(); ++i) { if(i) w(","); std::snprintf(buf, sizeof buf, "\"%08x\"", digests[i]); w(buf); }
        w("]}\n");
        char path[512]; std::snprintf(path, sizeof path, "%s/run_%02d.json", dir, int(si));
        FILE* f = std::fopen(path, "wb"); if(! f) { std::perror(path); return 1; }
        std::fwrite(out.data(), 1, out.size(), f); std::fclose(f);
        std::printf("%s: zawód %d seed %u poziom %d -> %s etap %d tier %d tury %d wynik %d kroki %d\n", path, s.cls, s.seed, s.diff,
                    g.st == status::won ? "WYGRANA" : (g.st == status::dead ? "porażka" : "w toku"), g.stage + 1, g.tier, g.turns, g.score, step);
    }
    std::printf("wydarzenia na placu (etapy):");
    for(int i = 0; i < data::site_events_count; ++i) std::printf(" %s=%d", data::site_events[i].name, event_hits[i]);
    std::printf("\nbrygada (wezwania):");
    for(int i = 0; i < data::brigade_count; ++i) std::printf(" %s=%d", data::brigade[i].name, helper_hits[i]);
    std::printf("\n");
    return 0;
}
