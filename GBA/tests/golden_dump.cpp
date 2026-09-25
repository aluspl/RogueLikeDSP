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
static void bot_step(game& g)
{
    if(g.has_offer()) { if(g.offer_is_better()) g.accept_offer(); else g.decline_offer(); }
    if(g.thermos > 0 && g.hero.hp * 100 < g.hero.max_hp * data::bot_drink_below_pct && g.player_drink()) return;
    if(g.slam_cell(g.hero.x, g.hero.y))
    {
        int d[4][2]={{1,0},{-1,0},{0,1},{0,-1}}, best=-1, bd=-1;
        for(int k=0;k<4;++k){ int nx=g.hero.x+d[k][0], ny=g.hero.y+d[k][1];
            if(!g.lv.passable(nx,ny)||g.occupied(nx,ny)) continue;
            int dist=cheb(nx,ny,g.slam_x,g.slam_y); if(dist>bd){bd=dist;best=k;} }
        if(best>=0 && g.player_move(d[best][0],d[best][1])) return;
    }
    if(g.nearest_target() >= 0 && g.weapon().range > 1) { g.player_attack_nearest(); return; }
    int tx = g.stairs_x, ty = g.stairs_y, best = 999;
    for(int i=0;i<g.enemies_count;++i){ auto& e=g.enemies[i]; int d=cheb(g.hero.x,g.hero.y,e.x,e.y); if(e.alive&&d<best&&(d<6||g.stairs_x<0)){best=d;tx=e.x;ty=e.y;} }
    int px[map_h][map_w]; for(auto& r:px) for(auto& c:r) c=-1;
    std::queue<std::pair<int,int>> q; q.push({g.hero.x,g.hero.y}); px[g.hero.y][g.hero.x]=4;
    int d[4][2]={{1,0},{-1,0},{0,1},{0,-1}};
    while(!q.empty()){ auto [x,y]=q.front(); q.pop(); if(x==tx&&y==ty) break;
        for(int k=0;k<4;++k){int nx=x+d[k][0],ny=y+d[k][1]; if(g.lv.passable(nx,ny)&&px[ny][nx]<0){px[ny][nx]=k;q.push({nx,ny});}}}
    if(tx<0||px[ty][tx]<0||(tx==g.hero.x&&ty==g.hero.y)){ g.player_wait(); return; }
    int x=tx,y=ty; while(true){int k=px[y][x]; int bx=x-d[k][0],by=y-d[k][1]; if(bx==g.hero.x&&by==g.hero.y){ if(!g.player_move(x-bx,y-by)) g.player_wait(); return;} x=bx;y=by;}
}

// Wariant: moc, gdy widoczny wróg jest blisko; celowanie w najbliższy widoczny cel w zasięgu (Bot.StepSmart w C#).
// Paczka sprzętu tej samej lub lepszej jakości: zakłada (wymiana cechy), gorsza: zostawia. Termos poniżej połowy HP.
static void bot_step_smart(game& g)
{
    if(g.has_offer()) { if(g.offer_rarity >= g.equipped[g.offer_slot]) g.accept_offer(); else g.decline_offer(); }
    if(g.thermos > 0 && g.hero.hp * 2 < g.hero.max_hp && g.player_drink()) return;
    if(!g.slam_cell(g.hero.x, g.hero.y) && g.ability_cd == 0)
    {
        int t = g.nearest_visible_enemy();
        if(t >= 0 && cheb(g.hero.x, g.hero.y, g.enemies[t].x, g.enemies[t].y) <= 2 && g.player_ability()) return;
    }
    if(!g.slam_cell(g.hero.x, g.hero.y))
    {
        int8_t targets[max_enemies]; int n = g.targets_in_range(targets, max_enemies);
        if(n > 0 && g.player_attack(targets[0])) return;
    }
    bot_step(g);
}

static void bot_shop(game& g) { for(int i = 0; i < data::hurtownia_count; ++i) g.hurtownia_buy(i); }

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
    }
    f.add(g.pickups_count);
    for(int i = 0; i < g.pickups_count; ++i)
    {
        const pickup& p = g.pickups[i];
        f.add(p.x); f.add(p.y); f.add(p.type); f.add(p.active); f.add(p.arg); f.add(p.trait);
    }
    for(int i = 0; i < 5; ++i) f.add(g.hero_status[i]);
    for(int i = 0; i < 4; ++i) f.add(g.equipped[i]);
    for(int i = 0; i < 4; ++i) f.add(g.equipped_trait[i]);
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
    return f.h;
}

// ------------------------------------------------------------------ JSON
static std::string out;
static int event_hits[16];   // statystyka wydarzeń na placu (wypis na końcu)
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
    w(","); key("equipped"); w("["); for(int i = 0; i < 4; ++i) { if(i) w(","); wi(g.equipped[i]); } w("]");
    w(","); key("equippedTrait"); w("["); for(int i = 0; i < 4; ++i) { if(i) w(","); wi(g.equipped_trait[i]); } w("]");
    w(","); key("thermos"); wi(g.thermos); w(","); key("thermosCap"); wi(g.thermos_cap());
    w(","); key("stageEvent"); wi(g.stage_event);
    w(","); key("counters"); w("["); wi(g.kills_banked); w(","); wi(g.powers_used); w(","); wi(g.powers_banked); w(",");
    wi(g.brand_found); w(","); wi(g.brand_banked); w(","); wi(g.clean_bosses); w(","); wi(g.clean_banked); w(","); wi(g.boss_wake_damage); w("]");
    w(","); key("stats"); w("["); wi(g.hero_stat(stat::str)); w(","); wi(g.hero_stat(stat::agi)); w(","); wi(g.hero_stat(stat::intel)); w(",");
    wi(g.luck()); w(","); wi(g.crit_pct()); w(","); wi(g.sight_radius()); w(","); wi(g.ability_cooldown()); w("]");
    w(","); key("offer"); w("["); wi(g.offer_slot); w(","); wi(g.offer_rarity); w(","); wi(g.offer_trait); w("]");
    w(","); key("heroStatus"); w("["); for(int i = 0; i < 5; ++i) { if(i) w(","); wi(g.hero_status[i]); } w("]");
    w(","); key("killsByType"); w("["); for(int i = 0; i < 16; ++i) { if(i) w(","); wi(g.kills_by_type[i]); } w("]");
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
        w("["); wi(e.def_id); w(","); wi(e.x); w(","); wi(e.y); w(","); wi(e.hp); w(","); wi(e.max_hp); w(","); wi(e.alive); w(","); wi(e.awake); w(","); wi(e.stun); w("]");
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
    w(","); key("keepsakeRuns"); w("["); for(int i = 0; i < max_keepsakes; ++i) { if(i) w(","); wi(p.keepsake_runs[i]); } w("]");
    w(","); key("sram"); hex_bytes(reinterpret_cast<const char*>(&p), sizeof p);   // profil bajt po bajcie jak w SRAM
    w("}");
}

// badges/contracts: odznaki i zlecenia w profilu przed budową (uprawnienia, odblokowane pamiątki);
// keepsake: wybrana pamiątka + 1; keepsake_runs: budowy z nią przed tą (ranga)
struct scenario { int cls; uint32_t seed; int diff; bool full_mods; bool smart; bool shop; bool ngplus; int steps;
                  int badges = 0; int contracts = 0; int keepsake = 0; int keepsake_runs = 0; };

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

    for(size_t si = 0; si < sc.size(); ++si)
    {
        const scenario& s = sc[si];
        profile p; profile_reset(p);
        if(s.full_mods)
        {
            for(int i = 0; i < data::upgrades_count; ++i) p.levels[i] = uint8_t(data::upgrades[i].levels);
            p.tools = uint8_t((1 << data::tools_count) - 1);
        }
        p.badges = uint16_t(s.badges); p.contracts = uint8_t(s.contracts); p.keepsake = uint8_t(s.keepsake);
        if(s.keepsake > 0) p.keepsake_runs[s.keepsake - 1] = uint8_t(s.keepsake_runs);
        run_mods m = mods(p);   // przed start_run: ranga pamiątki z budów przed tą
        static game g; g.new_run(s.cls, s.seed, s.diff, m);
        start_run(p);
        out.clear();
        w("{"); key("cls"); wi(s.cls); w(","); key("seed"); wi(s.seed); w(","); key("diff"); wi(s.diff);
        w(","); key("fullMods"); wi(s.full_mods); w(","); key("smart"); wi(s.smart); w(","); key("shop"); wi(s.shop);
        w(","); key("ngplus"); wi(s.ngplus); w(","); key("steps"); wi(s.steps);
        w(","); key("badges"); wi(s.badges); w(","); key("contracts"); wi(s.contracts); w(","); key("keepsake"); wi(s.keepsake);
        w(","); key("keepsakeRuns"); wi(s.keepsake_runs);
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
                if(g.act_cleared && s.shop) bot_shop(g);
                g.next_stage();
                w(","); snapshot(g, step);
                digests.push_back(digest(g)); g.hits_count = 0;
                continue;
            }
            if(g.st == status::won && s.ngplus && ! did_ng)
            {
                if(g.score > p.best) p.best = g.score;
                ++p.wins; add_house(p, g); check_badges(p, g); check_contracts(p); bank_xp(p, g);
                did_ng = true;
                g.new_game_plus();
                w(","); snapshot(g, step);
                digests.push_back(digest(g)); g.hits_count = 0;
                continue;
            }
            if(g.st != status::playing) break;
            if(s.smart) bot_step_smart(g); else bot_step(g);
            if(g.turns == g.stage_start_turn + 1 && g.stage_event >= 0) ++event_hits[g.stage_event];
            digests.push_back(digest(g)); g.hits_count = 0;   // warstwa GBA zeruje trafienia po każdej turze
        }
        if(g.score > p.best) p.best = g.score;
        if(g.st == status::won) { ++p.wins; add_house(p, g); }
        check_badges(p, g); check_contracts(p); bank_xp(p, g);
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
    std::printf("\n");
    return 0;
}
