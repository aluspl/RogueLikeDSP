// Testy rdzenia na PC: g++ -std=c++20 -I../include core_tests.cpp && ./a.out
#include <cstdio>
#include <cassert>
#include <queue>
#include "core.h"
#include "meta.h"
using namespace core;
static int fails = 0;
#define CHECK(c) do{ if(!(c)){ std::printf("FAIL %s:%d %s\n", __FILE__, __LINE__, #c); ++fails; } }while(0)

static bool connected(const level& lv, int sx, int sy)
{
    bool seen[map_h][map_w] = {};
    std::queue<std::pair<int,int>> q; q.push({sx, sy}); seen[sy][sx] = true; int n = 1;
    while(!q.empty()){ auto [x,y]=q.front(); q.pop(); int d[4][2]={{1,0},{-1,0},{0,1},{0,-1}};
        for(auto& v:d){int nx=x+v[0],ny=y+v[1]; if(lv.passable(nx,ny)&&!seen[ny][nx]){seen[ny][nx]=true;++n;q.push({nx,ny});}}}
    int total=0; for(int y=0;y<map_h;++y) for(int x=0;x<map_w;++x) total+=lv.passable(x,y);
    return n==total;
}

// Prosty bot: idź do najbliższego wroga/schodów, atakuj z dystansu, gdy się da.
static void bot_step(game& g)
{
    if(g.has_offer()) { if(g.offer_is_better()) g.accept_offer(); else g.decline_offer(); }
    if(g.thermos > 0 && g.hero.hp * 100 < g.hero.max_hp * data::bot_drink_below_pct && g.player_drink()) return;
    if(g.slam_cell(g.hero.x, g.hero.y))   // zapowiedziany cios bossa: zejdź z czerwonych pól (jak człowiek)
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
    // BFS po mapie do celu
    int px[map_h][map_w]; for(auto& r:px) for(auto& c:r) c=-1;
    std::queue<std::pair<int,int>> q; q.push({g.hero.x,g.hero.y}); px[g.hero.y][g.hero.x]=4;
    int d[4][2]={{1,0},{-1,0},{0,1},{0,-1}};
    while(!q.empty()){ auto [x,y]=q.front(); q.pop(); if(x==tx&&y==ty) break;
        for(int k=0;k<4;++k){int nx=x+d[k][0],ny=y+d[k][1]; if(g.lv.passable(nx,ny)&&px[ny][nx]<0){px[ny][nx]=k;q.push({nx,ny});}}}
    if(tx<0||px[ty][tx]<0||(tx==g.hero.x&&ty==g.hero.y)){ g.player_wait(); return; }
    int x=tx,y=ty; while(true){int k=px[y][x]; int bx=x-d[k][0],by=y-d[k][1]; if(bx==g.hero.x&&by==g.hero.y){ if(!g.player_move(x-bx,y-by)) g.player_wait(); return;} x=bx;y=by;}
}

// Otwarta arena 14x14 bez wrogów i znajdziek, bohater na (7,7) - do testów mocy.
static void arena(game& g, int cls)
{
    g.new_run(cls, 77);
    for(auto& row : g.lv.t) for(auto& c : row) c = tile::wall;
    for(int y = 1; y <= 14; ++y) for(int x = 1; x <= 14; ++x) g.lv.t[y][x] = tile::floor;
    g.enemies_count = 0; g.pickups_count = 0; g.stairs_x = g.stairs_y = -1;
    g.hero.x = 7; g.hero.y = 7; g.update_fov();
}

static int g_dummy_crit(int cls) { game g; g.new_run(cls, 1); return g.crit_pct(); }

int main()
{
    // 1. mapy: spójne, pokoje >= 2, start na podłodze, deterministyczne
    for(uint32_t seed = 1; seed <= 500; ++seed)
    {
        game g; g.new_run(seed % data::classes_count, seed);
        CHECK(g.lv.rooms_count >= 2);
        CHECK(connected(g.lv, g.hero.x, g.hero.y));
        CHECK(g.lv.passable(g.hero.x, g.hero.y));
        game h; h.new_run(seed % data::classes_count, seed);
        CHECK(std::memcmp(g.lv.t, h.lv.t, sizeof g.lv.t) == 0);
        for(int i=0;i<g.enemies_count;++i) CHECK(!(g.enemies[i].x==g.hero.x&&g.enemies[i].y==g.hero.y));
    }
    // 2. dziennik: polskie znaki i liczby
    message m; m.add("Kawa: +").add(8).add(" HP"); CHECK(std::strcmp(m.s, "Kawa: +8 HP")==0);
    // 3. trudność: HP i obrażenia wrogów rosną z etapem, poziomem i NG+
    {
        game n; n.new_run(0, 42, 1);
        CHECK(n.diff == 1 && n.tier == 0);
        CHECK(n.enemy_hp_pct() == data::stages[0].hp_pct);
        for(int i=0;i<n.enemies_count;++i)
            CHECK(n.enemies[i].max_hp == data::enemies[n.enemies[i].def_id].max_health * data::stages[0].hp_pct / 100);
        game e; e.new_run(0, 42, 0);
        game h; h.new_run(0, 42, 2);
        CHECK(e.enemy_hp_pct() < n.enemy_hp_pct() && n.enemy_hp_pct() < h.enemy_hp_pct());
        CHECK(e.enemy_dmg_bonus() < n.enemy_dmg_bonus() && n.enemy_dmg_bonus() <= h.enemy_dmg_bonus());
        CHECK(e.score_pct() < n.score_pct() && n.score_pct() < h.score_pct());
        for(int i=0;i<e.enemies_count;++i) CHECK(e.enemies[i].max_hp >= 1);
        // etapy: mnożnik nie maleje
        game s; s.new_run(0, 42, 1);
        int prev_hp = s.enemy_hp_pct(), prev_dmg = s.enemy_dmg_bonus();
        for(int k=1;k<data::stages_count;++k){ s.next_stage(); CHECK(s.enemy_hp_pct() >= prev_hp); CHECK(s.enemy_dmg_bonus() >= prev_dmg); prev_hp=s.enemy_hp_pct(); prev_dmg=s.enemy_dmg_bonus(); }
        // boss też skalowany
        CHECK(s.boss >= 0 && s.enemies[s.boss].max_hp > data::enemies[data::stages[s.stage].boss].max_health);
    }
    // 4. obrażenia wroga uwzględniają premię trudności
    {
        // ten sam seed = ten sam rzut; Termin (3-6) vs obrona Elektryka nie schodzi do minimum 1
        int lost[2];
        for(int k=0;k<2;++k)
        {
            game g; g.new_run(3, 7, k);   // Łatwy (-1) vs Normalny
            g.enemies_count = 0; g.spawn(8, g.hero.x + 1, g.hero.y); g.enemies[0].awake = true;
            int before = g.hero.hp; g.player_wait(); lost[k] = before - g.hero.hp;
        }
        CHECK(lost[0] >= 1 && lost[1] == lost[0] + 1);
    }
    // 5. wynik: zabicie wroga daje score * mnożnik trudności
    {
        game g; g.new_run(1, 7, 2);
        g.enemies_count = 0; g.spawn(0, g.hero.x + 1, g.hero.y); g.enemies[0].hp = 1;
        g.player_move(1, 0);
        CHECK(g.kills == 1 && g.score == data::enemies[0].score * g.score_pct() / 100);
        CHECK(g.kills_by_type[0] == 1 && g.kills_by_type[1] == 0);
    }
    // 6. NG+: tylko po wygranej; zachowuje zawód, premie i wynik, podnosi poziom
    {
        game g; g.new_run(2, 99, 1);
        CHECK(!g.new_game_plus());
        g.dmg_bonus = 2; g.def_bonus = 1; g.score = 1234; g.st = status::won; g.stage = data::stages_count - 1;
        int dmg0 = data::stages[0].dmg_bonus;
        CHECK(g.new_game_plus());
        CHECK(g.tier == 1 && g.stage == 0 && g.st == status::playing && g.cls == 2 && g.diff == 1);
        CHECK(g.dmg_bonus == 2 && g.def_bonus == 1 && g.score == 1234 && g.hero.hp == g.hero.max_hp);
        CHECK(g.enemy_hp_pct() > data::stages[0].hp_pct && g.enemy_dmg_bonus() > dmg0);
    }
    // 7. premie z meta-progresji (run_mods) działają na start budowy
    {
        game a; a.new_run(1, 5);
        run_mods m; m.hp = 8; m.def = 1; m.dmg = 2; m.coffee = 4; m.pickups = 2;
        game b; b.new_run(1, 5, data::default_difficulty, m);
        CHECK(b.hero.max_hp == a.hero.max_hp + 8 && b.hero.hp == b.hero.max_hp);
        CHECK(b.def_bonus == 1 && b.dmg_bonus == 2);
        CHECK(b.pickups_count == a.pickups_count + 2);
        b.hero.hp = 1; b.pickups[0] = { b.hero.x, b.hero.y, coffee, true }; b.collect();
        CHECK(b.hero.hp == 1 && b.thermos == 1);                          // kawa trafia do termosu
        CHECK(b.player_drink() && b.hero.hp == 1 + data::coffee_heal + 4 && b.thermos == 0 && b.turns == 1);
        CHECK(!b.player_drink());                                         // pusty termos: bez tury
        b.thermos = data::thermos_capacity; b.hero.hp = 1;
        b.pickups[0] = { b.hero.x, b.hero.y, coffee, true }; b.collect();
        CHECK(b.hero.hp == 1 + data::coffee_heal + 4 && b.thermos == data::thermos_capacity);   // pełny: pije od razu
        b.hero.hp = b.hero.max_hp; CHECK(!b.player_drink() && b.thermos == data::thermos_capacity);
        b.next_stage(); CHECK(b.pickups_count == a.pickups_count + 2);   // premia trwa w kolejnych etapach
    }
    // 8. doświadczenie: wrogowie, etapy, boss; mnożone przez trudność
    {
        game g; g.new_run(1, 7);
        g.enemies_count = 0; g.spawn(0, g.hero.x + 1, g.hero.y); g.enemies[0].hp = 1;
        g.player_move(1, 0);
        CHECK(g.xp() == data::xp_per_kill);
        g.debug_skip(); CHECK(g.st == status::stage_clear && g.xp() == data::xp_per_kill + data::xp_per_stage);
        game h; h.new_run(1, 7, 2);
        h.enemies_count = 0; h.spawn(0, h.hero.x + 1, h.hero.y); h.enemies[0].hp = 1; h.player_move(1, 0);
        h.debug_skip();
        CHECK(h.xp() == (data::xp_per_kill + data::xp_per_stage) * h.score_pct() / 100);
        game w; w.new_run(1, 7);
        for(int k=0;k<data::stages_count-1;++k){ w.debug_skip(); w.next_stage(); }
        int before = w.xp(); w.debug_skip();
        CHECK(w.st == status::won && w.xp() == before + data::xp_per_kill + data::xp_boss);
    }
    // 9. profil: migracja zapisu v1 (rekord), śmieci -> domyślny, v2 bez zmian
    {
        profile p; std::memset(&p, 0xAB, sizeof p);
        std::memcpy(p.magic, "PBRL001", 8); p.best = 500; p.runs = 3; p.wins = 1;
        CHECK(profile_fix(p));
        CHECK(std::strcmp(p.magic, profile_magic) == 0 && p.best == 500 && p.runs == 3 && p.wins == 1);
        CHECK(p.xp == 0 && p.classes == data::start_classes_mask && !p.hard);
        for(int i=0;i<max_upgrades;++i) CHECK(p.levels[i] == 0);
        profile q; std::memset(&q, 0xFF, sizeof q);
        CHECK(profile_fix(q) && q.best == 0 && q.classes == data::start_classes_mask);
        q.xp = 7; CHECK(!profile_fix(q) && q.xp == 7);
        CHECK(!has_flag(p, help_seen) && !has_flag(q, help_seen));   // instrukcja pokaże się po migracji
        set_flag(q, help_seen); CHECK(has_flag(q, help_seen) && !profile_fix(q) && has_flag(q, help_seen));
    }
    // 10. sklep: koszty, poziomy, zawody, poziom Trudny, premie
    {
        profile p; profile_reset(p);
        CHECK(!class_unlocked(p, 2) && class_unlocked(p, 1));
        CHECK(!difficulty_unlocked(p, 2) && difficulty_unlocked(p, 1));
        CHECK(!buy_upgrade(p, 0));                               // brak doświadczenia
        p.xp = 1000;
        int c0 = upgrade_cost(p, 0);
        CHECK(c0 == data::upgrades[0].costs[0] && buy_upgrade(p, 0) && p.levels[0] == 1 && p.xp == 1000 - c0);
        while(upgrade_cost(p, 0) > 0) CHECK(buy_upgrade(p, 0));
        CHECK(p.levels[0] == data::upgrades[0].levels && !buy_upgrade(p, 0));
        CHECK(buy_class(p, 2) && class_unlocked(p, 2) && !buy_class(p, 2));
        CHECK(buy_hard(p) && difficulty_unlocked(p, 2) && !buy_hard(p));
        run_mods m = mods(p);
        CHECK(m.hp == data::upgrades[0].value * data::upgrades[0].levels && m.def == 0);
        CHECK(shop_spent(p) == 1000 - p.xp);                       // wszystko, co zeszło z konta, to wydatki
        profile e; profile_reset(e); CHECK(shop_spent(e) == 0);
        p.xp = 100000;
        for(int i=0;i<data::upgrades_count;++i) while(buy_upgrade(p, i)) {}
        for(int i=0;i<data::classes_count;++i) buy_class(p, i);
        for(int i=0;i<data::tools_count;++i) buy_tool(p, i);
        buy_hard(p);
        CHECK(shop_spent(p) == shop_total_cost());
    }
    // 11. bankowanie doświadczenia: bez podwójnego liczenia (np. NG+)
    {
        profile p; profile_reset(p);
        game g; g.new_run(1, 7); g.debug_skip();
        int x = g.xp(); CHECK(x > 0);
        CHECK(bank_xp(p, g) == x && p.xp == x);
        CHECK(bank_xp(p, g) == 0 && p.xp == x);
    }
    // 12. pole widzenia (mgła wojny)
    for(uint32_t seed = 1; seed <= 100; ++seed)   // cały pokój startowy widoczny od razu
    {
        game g; g.new_run(0, seed);
        const room& r0 = g.lv.rooms[0];
        CHECK(g.visible(g.hero.x, g.hero.y));
        for(int y = r0.y; y < r0.y + r0.h; ++y) for(int x = r0.x; x < r0.x + r0.w; ++x) CHECK(g.visible(x, y));
        const room& last = g.lv.rooms[g.lv.rooms_count - 1];
        if(cheb(g.hero.x, g.hero.y, last.cx(), last.cy()) > fov_radius) CHECK(!g.explored(last.cx(), last.cy()));
    }
    {
        game g; g.new_run(0, 3);
        for(auto& row : g.lv.t) for(auto& c : row) c = tile::wall;
        for(int y = 1; y <= 12; ++y) for(int x = 1; x <= 12; ++x) g.lv.t[y][x] = tile::floor;
        g.lv.t[4][6] = tile::wall;                        // filar nad bohaterem
        g.enemies_count = 0; g.hero.x = 6; g.hero.y = 6;
        g.update_fov();
        CHECK(g.visible(6, 4));                           // sam filar widać
        CHECK(!g.visible(6, 3) && !g.visible(6, 2));      // za filarem cień
        CHECK(g.visible(9, 6) && g.visible(3, 9));        // otwarta przestrzeń widoczna
        CHECK(!g.visible(6, 6 + fov_radius + 1));         // poza zasięgiem (i tak ściana, ale poza promieniem)
        CHECK(!g.explored(0, 0));                         // narożnik mapy poza promieniem
        // zapamiętane pola: po odejściu przestają być widoczne, ale zostają odkryte
        CHECK(g.visible(1, 6));
        g.lv.t[6][5] = tile::wall; g.lv.t[5][5] = tile::wall; g.lv.t[7][5] = tile::wall;   // ściana na zachód
        g.update_fov();
        CHECK(!g.visible(1, 6) && g.explored(1, 6));
        // wrogowie poza polem widzenia są ukryci
        g.spawn(0, 2, 6); CHECK(!g.visible(g.enemies[0].x, g.enemies[0].y));
        g.spawn(0, 9, 7); CHECK(g.visible(g.enemies[1].x, g.enemies[1].y));
    }
    // 13. zdarzenia trafień (liczby obrażeń, pasek HP celu)
    {
        game g; g.new_run(1, 11);
        g.enemies_count = 0; g.spawn(8, g.hero.x + 1, g.hero.y); g.enemies[0].awake = true;
        int ehp = g.enemies[0].hp, hhp = g.hero.hp;
        g.player_move(1, 0);   // atak w Termin, potem Termin oddaje
        CHECK(g.hits_count == 2);
        CHECK(!g.hits[0].on_hero && g.hits[0].amount == ehp - g.enemies[0].hp && g.hits[0].x == g.enemies[0].x);
        CHECK(g.hits[1].on_hero && g.hits[1].amount == hhp - g.hero.hp && g.hits[1].x == g.hero.x);
        CHECK(g.last_target == 0);
        g.hits_count = 0;
        for(int k = 0; k < 20; ++k) g.player_wait();
        CHECK(g.hits_count <= max_hits);   // bufor się nie przepełnia
    }
    // 14. poziomy postaci w trakcie budowy
    {
        game g; g.new_run(1, 5, 0);   // Łatwy: poziomy liczone z surowego doświadczenia, nie z mnożnika
        int hp0 = g.hero.max_hp, dmg0 = g.dmg_bonus, def0 = g.def_bonus;
        CHECK(g.hero_level == 1);
        g.gain_xp(data::level_thresholds[0] - 1); CHECK(g.hero_level == 1);
        g.gain_xp(1);
        CHECK(g.hero_level == 2 && g.hero.max_hp == hp0 + data::hp_per_level);
        CHECK(g.dmg_bonus == dmg0 + ((data::dmg_levels_mask >> 2) & 1) && g.def_bonus == def0 + ((data::def_levels_mask >> 2) & 1));
        g.gain_xp(data::level_thresholds[1] - data::level_thresholds[0]);
        CHECK(g.hero_level == 3 && g.hero.max_hp == hp0 + 2 * data::hp_per_level);
        g.gain_xp(10000);
        CHECK(g.hero_level == data::max_hero_level);
        CHECK(g.xp_to_next() < 0);
        game h; h.new_run(1, 5); CHECK(h.xp_to_next() == data::level_thresholds[0]);
        // NG+ zachowuje poziom
        g.st = status::won; g.new_game_plus(); CHECK(g.hero_level == data::max_hero_level);
    }
    // 15. dropy: wypadają z wrogów w miejscu śmierci, narzędzia tylko odblokowane
    {
        int drops = 0, tools_seen = 0;
        for(uint32_t seed = 1; seed <= 300; ++seed)
        {
            game g; g.new_run(1, seed);
            int before = g.pickups_count;
            g.enemies_count = 0; g.spawn(0, g.hero.x + 1, g.hero.y); g.enemies[0].hp = 1;
            g.player_move(1, 0);
            if(g.pickups_count > before)
            {
                ++drops;
                const pickup& p = g.pickups[g.pickups_count - 1];
                CHECK(p.active && p.x == g.hero.x + 1 && p.y == g.hero.y);
                if(p.type == tool) { ++tools_seen; CHECK(data::start_tools_mask & (1 << p.arg)); }
            }
        }
        CHECK(drops > 300 * data::drop_chance_pct / 200 && drops < 300 * data::drop_chance_pct * 2 / 100);
        CHECK(tools_seen > 0);
    }
    // 16. narzędzie: podniesienie zmienia broń; odblokowane w sklepie trafiają do puli
    {
        game g; g.new_run(1, 9);
        int lom = data::tools[0].weapon;
        g.pickups[0] = { g.hero.x, g.hero.y, tool, true, 0 }; g.collect();
        CHECK(&g.weapon() == &data::weapons[lom]);
        profile p; profile_reset(p);
        CHECK(tool_unlocked(p, 0) && !tool_unlocked(p, 3) && !buy_tool(p, 3));
        p.xp = 100; CHECK(buy_tool(p, 3) && tool_unlocked(p, 3) && !buy_tool(p, 3) && p.xp == 100 - data::tools[3].cost);
        CHECK(mods(p).tools == (data::start_tools_mask | (1 << 3)));
        // wszystkie narzędzia odblokowane -> każde może wypaść
        run_mods m; m.tools = (1 << data::tools_count) - 1;
        int seen = 0;
        for(uint32_t seed = 1; seed <= 3000 && seen != m.tools; ++seed)
        {
            game h; h.new_run(1, seed, data::default_difficulty, m);
            h.enemies_count = 0; h.spawn(0, h.hero.x + 1, h.hero.y); h.enemies[0].hp = 1; h.player_move(1, 0);
            const pickup& q = h.pickups[h.pickups_count - 1];
            if(q.type == tool) seen |= 1 << q.arg;
        }
        CHECK(seen == m.tools);
        // migracja: stary profil v2 (bajt narzędzi = 0) ma narzędzia startowe
        profile old; profile_reset(old); old.tools = 0; CHECK(tool_unlocked(old, 0));
    }
    // 17. moce zawodów (R)
    {
        game g; arena(g, 4);                                 // Hydraulik: Zawór (strumień: leczy i odpycha)
        CHECK(g.ability_cd == 0 && !g.player_ability());     // pełne HP i nikt obok - nic do zrobienia, tura nie mija
        CHECK(g.turns == 0);
        g.hero.hp = 10; CHECK(g.player_ability() && g.hero.hp == 16 && g.turns == 1);
        CHECK(g.ability_cd == g.ability_cooldown() && !g.player_ability());
        for(int k = 0; k < g.ability_cooldown(); ++k) g.player_wait();
        CHECK(g.ability_cd == 0);
        g.spawn(4, 8, 7); g.enemies[0].awake = true;         // wróg obok: odepchnięty o 1 pole
        CHECK(g.player_ability() && g.enemies[0].x >= 9);
    }
    {
        game g; arena(g, 0);                                 // Kierownik: Odprawa (ogłuszenie)
        CHECK(!g.player_ability());                          // brak widocznych wrogów
        g.spawn(8, 8, 7); g.enemies[0].awake = true;
        int hp = g.hero.hp;
        CHECK(g.player_ability() && g.enemies[0].stun > 0);
        g.player_wait();
        CHECK(g.hero.hp >= hp);                              // ogłuszony nie atakuje (+ ewentualny odpoczynek)
    }
    {
        game g; arena(g, 1);                                 // Murarz: Ścianka - mur w poprzek drogi wroga
        CHECK(!g.player_ability());                          // brak widocznego wroga - nic
        g.spawn(4, 11, 7);                                   // wróg na wschodzie
        CHECK(g.player_ability());
        CHECK(g.lv.at(8, 6) == tile::wall && g.lv.at(8, 7) == tile::wall && g.lv.at(8, 8) == tile::wall);
        CHECK(g.lv.at(6, 7) == tile::floor && g.lv.at(7, 6) == tile::floor && g.lv.at(7, 8) == tile::floor);   // nie zamyka bohatera
        for(int k = 0; k < 8; ++k) g.player_wait();
        CHECK(g.lv.at(8, 6) == tile::floor && g.lv.at(8, 7) == tile::floor && g.lv.at(8, 8) == tile::floor);
        // przy ścianie i w korytarzu też zostaje wyjście
        game c; arena(c, 1);
        for(int y = 1; y <= 14; ++y) for(int x = 1; x <= 14; ++x) if(y != 7) c.lv.t[y][x] = tile::wall;   // korytarz poziomy
        c.update_fov(); c.spawn(4, 10, 7);
        CHECK(c.player_ability() && c.lv.at(8, 7) == tile::wall && c.lv.at(6, 7) == tile::floor);
    }
    {
        game g; arena(g, 3);                                 // rangi mocy: poziom 3 -> II, poziom 5 -> III
        CHECK(g.ability_rank() == 1);
        int cd1 = g.ability_cooldown();
        g.hero_level = 3; CHECK(g.ability_rank() == 2 && g.ability_cooldown() == cd1 - 2);
        g.hero_level = 5; CHECK(g.ability_rank() == 3);
        for(int i = 0; i < 5; ++i) g.spawn(4, 9 + (i % 3) * 2, 7 + (i / 3) * 2);   // cele co 2 pola
        CHECK(g.player_ability());
        int hit = 0; for(int i = 0; i < 5; ++i) hit += g.enemies[i].hp < data::enemies[4].max_health * g.enemy_hp_pct() / 100;
        CHECK(hit == 5);                                     // Łańcuch III: 5 celów
    }
    {
        game g; arena(g, 2);                                 // Cieśla: Seria (zasięg broni 3)
        g.spawn(4, 8, 7); g.spawn(4, 10, 7); g.spawn(4, 13, 7);
        int h0 = g.enemies[0].hp, h1 = g.enemies[1].hp, h2 = g.enemies[2].hp;
        CHECK(g.player_ability());
        CHECK(g.enemies[0].hp < h0 && g.enemies[1].hp < h1 && g.enemies[2].hp == h2);
    }
    {
        game g; arena(g, 3);                                 // Elektryk: Łańcuch (zasięg 2, skoki do 2 pól)
        g.spawn(4, 9, 7); g.spawn(4, 11, 7); g.spawn(4, 13, 7); g.spawn(4, 7, 13);
        int h[4]; for(int i = 0; i < 4; ++i) h[i] = g.enemies[i].hp;
        CHECK(g.player_ability());
        CHECK(g.enemies[0].hp < h[0] && g.enemies[1].hp < h[1] && g.enemies[2].hp < h[2] && g.enemies[3].hp == h[3]);
    }
    {
        game g; arena(g, 5);                                 // Glazurnik: Wirówka (wszyscy obok)
        CHECK(!g.player_ability());
        g.spawn(4, 6, 6); g.spawn(4, 8, 8); g.spawn(4, 7, 9);
        int h0 = g.enemies[0].hp, h1 = g.enemies[1].hp, h2 = g.enemies[2].hp;
        CHECK(g.player_ability());
        CHECK(g.enemies[0].hp < h0 && g.enemies[1].hp < h1 && g.enemies[2].hp == h2);
    }
    // 18. zapis budowy w trakcie (SRAM)
    {
        static game g; g.new_run(2, 555, 2);
        auto play = [](game& x) { if(x.st == status::stage_clear) x.next_stage(); else if(x.st == status::playing) bot_step(x); };
        for(int k = 0; k < 30; ++k) play(g);
        static run_save rs; run_save_make(rs, g);
        CHECK(run_save_valid(rs));
        static game h; std::memcpy(&h, &rs.g, sizeof h);
        for(int k = 0; k < 50; ++k) { play(g); play(h); }            // wznowiona gra przebiega identycznie
        // porównanie stanu (nie całej pamięci - bajty wyrównania struktur mogą się różnić)
        CHECK(g.r.s == h.r.s && g.turns == h.turns && g.score == h.score && g.stage == h.stage && g.st == h.st);
        CHECK(g.hero.x == h.hero.x && g.hero.y == h.hero.y && g.hero.hp == h.hero.hp && g.xp() == h.xp());
        CHECK(std::memcmp(g.lv.t, h.lv.t, sizeof g.lv.t) == 0 && std::memcmp(g.fov, h.fov, sizeof g.fov) == 0);
        for(int i = 0; i < g.enemies_count; ++i)
            CHECK(g.enemies[i].x == h.enemies[i].x && g.enemies[i].y == h.enemies[i].y && g.enemies[i].hp == h.enemies[i].hp);
        reinterpret_cast<unsigned char*>(&rs.g)[100] ^= 0x5A; CHECK(!run_save_valid(rs));   // uszkodzony
        run_save_make(rs, g); rs.size -= 4; CHECK(!run_save_valid(rs));                     // inna wersja gry
        run_save_make(rs, g); run_save_clear(rs); CHECK(!run_save_valid(rs));               // wyczyszczony
    }
    // 19. motywacja: liczniki etapu, odznaki, Osiedle, katalog, migracja profilu v2 -> v3
    {
        profile v2; profile_reset(v2);
        std::memcpy(v2.magic, "PBRL002", 8); v2.xp = 77; v2.levels[0] = 2; v2.classes = 0x3F; v2.hard = 1; v2.flags = 1; v2.tools = 2; v2.best = 900;
        std::memset(reinterpret_cast<char*>(&v2) + 36, 0xEE, sizeof v2 - 36);   // śmieci za starym końcem struktury
        CHECK(profile_fix(v2));
        CHECK(std::strcmp(v2.magic, profile_magic) == 0 && v2.xp == 77 && v2.levels[0] == 2 && v2.classes == 0x3F);
        CHECK(v2.hard == 1 && v2.flags == 1 && v2.tools == 2 && v2.best == 900);
        CHECK(v2.badges == 0 && v2.catalog == 0 && v2.houses_count == 0 && v2.class_wins == 0 && v2.tools_found == 0);
    }
    {
        game g; arena(g, 1);
        CHECK(g.stage_damage == 0 && g.stage_kills == 0 && g.stage_start_turn == 0);
        g.spawn(8, 8, 7); g.enemies[0].awake = true;
        int hp = g.hero.hp; g.player_wait();
        CHECK(g.stage_damage == hp - g.hero.hp && g.stage_damage > 0);
        g.enemies[0].hp = 1; g.player_move(1, 0);
        CHECK(g.stage_kills == 1);
        g.st = status::stage_clear; g.next_stage();
        CHECK(g.stage_damage == 0 && g.stage_kills == 0 && g.stage_start_turn == g.turns);
    }
    {
        profile p; profile_reset(p);
        game g; arena(g, 1);
        g.st = status::stage_clear;                      // etap bez obrażeń
        int xp0 = p.xp;
        int got = check_badges(p, g);
        CHECK(got == (1 << data::badge_bez_usterek) && p.xp == xp0 + data::badges[data::badge_bez_usterek].xp);
        CHECK(check_badges(p, g) == 0);                  // drugi raz nie
        g.stage_damage = 5; g.stage_kills = 8;
        CHECK(check_badges(p, g) == (1 << data::badge_seryjny));
    }
    {
        profile p; profile_reset(p);
        game g; g.new_run(3, 5, data::difficulties_count - 1);
        g.st = status::won; g.score = 2500; g.stage = data::stages_count - 1; g.stage_start_turn = g.turns - 100; g.stage_damage = 1;
        CHECK(add_house(p, g) && p.houses_count == 1 && (p.houses[0] & 15) == 3 && (p.houses[0] >> 4) == 2);
        int got = check_badges(p, g);
        CHECK(got & (1 << data::badge_twardziel));
        CHECK(got & (1 << data::badge_przed_terminem));
        CHECK(p.class_wins == (1 << 3) && !(got & (1 << data::badge_pelny_zespol)));
        for(int i = 0; i < 20; ++i) add_house(p, g);
        CHECK(p.houses_count == max_houses);             // Osiedle ma limit działek
        CHECK(check_badges(p, g) & (1 << data::badge_osiedle));
        for(int c = 0; c < data::classes_count; ++c) { g.cls = c; check_badges(p, g); }
        CHECK(p.badges & (1 << data::badge_pelny_zespol));
    }
    {
        profile p; profile_reset(p);
        game g; arena(g, 1);
        for(int d = 0; d < data::enemies_count; ++d) g.kills_by_type[d] = 1;
        g.tools_found = uint8_t((1 << data::tools_count) - 1);
        g.hero_level = data::max_hero_level;
        int got = check_badges(p, g);
        CHECK(got & (1 << data::badge_katalog) && got & (1 << data::badge_kolekcjoner) && got & (1 << data::badge_zawodowiec));
        CHECK(p.catalog == (1 << data::enemies_count) - 1);
        // zebranie narzędzia zapisuje je w liczniku budowy
        game h; arena(h, 1); h.pickups[0] = { h.hero.x, h.hero.y, tool, true, 2 }; h.pickups_count = 1; h.collect();
        CHECK(h.tools_found == (1 << 2));
    }
    // 20. fabuła: wiadomość etapu, NG+ ma własną
    {
        game g; g.new_run(1, 3);
        CHECK(&g.stage_story() == &data::story_stages[0]);
        g.next_stage(); CHECK(&g.stage_story() == &data::story_stages[1]);
        g.st = status::won; g.stage = data::stages_count - 1; g.new_game_plus();
        CHECK(&g.stage_story() == &data::story_ngplus);
        for(int i = 0; i < data::stages_count; ++i) CHECK(data::story_stages[i].from && data::story_stages[i].lines[0][0]);
    }
    // 21. sprzęt: zakładanie lepszego, premie, drop z jakością zależną od etapu
    {
        game g; arena(g, 1);
        for(int i = 0; i < data::gear_slots_count; ++i) CHECK(g.equipped[i] == -1);
        int hp0 = g.hero.max_hp;
        auto give = [&](int slot, int rarity, int trait = 0) {
            g.pickups[0] = { g.hero.x, g.hero.y, gear_box, true, uint8_t(slot * 3 + rarity), uint8_t(trait) }; g.pickups_count = 1; g.collect();
        };
        give(2, 1, 3);                                                    // kamizelka ocieplana +8 HP: pusty slot - od razu
        CHECK(g.equipped[2] == 1 && g.equipped_trait[2] == 3 && g.hero.max_hp == hp0 + 8 && !g.has_offer());
        int xp0 = g.run_xp;
        give(2, 0);                                                       // zajęty slot: porównanie
        CHECK(g.has_offer() && !g.offer_is_better() && g.equipped[2] == 1);
        give(2, 2); CHECK(g.pickups[0].active);                           // druga paczka czeka, aż zdecydujesz
        g.decline_offer();                                                // zostawiam stary: doświadczenie
        CHECK(!g.has_offer() && g.equipped[2] == 1 && g.hero.max_hp == hp0 + 8 && g.run_xp == xp0 + data::gear_decline_xp);
        give(2, 0, 1); g.accept_offer();                                  // gorszy, ale gracz wybrał
        CHECK(g.equipped[2] == 0 && g.equipped_trait[2] == 1 && g.hero.max_hp == hp0 + 4);
        give(2, 2); CHECK(g.offer_is_better()); g.accept_offer();         // lepsza zastępuje
        CHECK(g.equipped[2] == 2 && g.hero.max_hp == hp0 + 12);
        give(0, 2); give(1, 2);
        CHECK(g.gear_bonus(gear_stat::def) == 3 && g.gear_bonus(gear_stat::dmg) == 3);
    }
    {
        int lost[2], dealt[2];                                            // ten sam seed: kask zmniejsza, rękawice zwiększają
        for(int k = 0; k < 2; ++k)
        {
            game g; g.new_run(3, 9);
            if(k) { g.equipped[0] = 2; g.equipped[1] = 2; }
            g.enemies_count = 0; g.spawn(8, g.hero.x + 1, g.hero.y); g.enemies[0].awake = true;
            int ehp = g.enemies[0].hp, hhp = g.hero.hp;
            g.player_move(1, 0);
            dealt[k] = ehp - g.enemies[0].hp; lost[k] = hhp - g.hero.hp;
        }
        CHECK(dealt[1] == dealt[0] + 3);
        CHECK(lost[1] < lost[0]);
    }
    {
        int brand[2] = {}, gear_drops = 0;
        for(int st = 0; st < 2; ++st)
            for(uint32_t seed = 1; seed <= 1500; ++seed)
            {
                game g; g.new_run(1, seed); g.stage = st == 0 ? 0 : data::stages_count - 1;
                g.enemies_count = 0; g.spawn(0, g.hero.x + 1, g.hero.y); g.enemies[0].hp = 1; g.player_move(1, 0);
                const pickup& p = g.pickups[g.pickups_count - 1];
                if(p.type == gear_box) { CHECK(p.arg < data::gear_slots_count * 3); ++gear_drops; brand[st] += p.arg % 3 == 2; }
            }
        CHECK(gear_drops > 0 && brand[1] > brand[0]);                   // na późnych etapach częściej markowy
    }
    // 22. dziennik: powtórzenia jako licznik, rodzaj komunikatu, numer kolejny
    {
        game g; arena(g, 1);
        int serial = g.log_serial;
        g.push(message().add("Brak celu").as(info));
        g.push(message().add("Brak celu").as(info));
        CHECK(g.log_serial == serial + 2);
        CHECK(std::strcmp(g.log[log_lines - 1].s, "Brak celu") == 0 && g.log[log_lines - 1].repeat == 2);
        CHECK(std::strcmp(g.log[log_lines - 2].s, "Brak celu") != 0);
        g.push(message().add("Awans").as(good));
        CHECK(g.log[log_lines - 1].kind == good && g.log[log_lines - 2].repeat == 2);
        g.spawn(8, 8, 7); g.enemies[0].awake = true; g.player_wait();
        CHECK(g.log[log_lines - 1].kind == bad);                     // obrażenia bohatera na czerwono
    }
    // 23. celowanie: cele w zasięgu od najbliższego, atak tylko w zasięgu
    {
        game g; arena(g, 2);                                             // Cieśla: zasięg 3
        g.spawn(4, 10, 7); g.spawn(4, 8, 8); g.spawn(4, 13, 7);
        int8_t t[8]; int n = g.targets_in_range(t, 8);
        CHECK(n == 2 && t[0] == 1 && t[1] == 0);                         // najbliższy pierwszy, daleki pominięty
        CHECK(!g.player_attack(2) && g.turns == 0);                       // poza zasięgiem - bez tury
        int hp = g.enemies[0].hp;
        CHECK(g.player_attack(0) && g.enemies[0].hp < hp && g.turns == 1);
    }
    // 24. akty: budżet za problemy, boss aktu -> premia i Hurtownia, ostatni boss -> wygrana
    {
        game g; g.new_run(1, 5);
        g.enemies_count = 0; g.spawn(0, g.hero.x + 1, g.hero.y); g.enemies[0].hp = 1; g.player_move(1, 0);
        CHECK(g.cash == data::enemies[0].score / data::cash_per_score);
    }
    {
        game g; g.new_run(1, 5);
        int last_of_act0 = 0;
        while(data::stages[last_of_act0 + 1].act == 0) ++last_of_act0;
        for(int k = 0; k < last_of_act0; ++k) { g.debug_skip(); CHECK(g.st == status::stage_clear && !g.act_cleared); g.next_stage(); }
        CHECK(g.boss >= 0 && data::enemies[g.enemies[g.boss].def_id].slam);
        int c0 = g.cash;
        g.debug_skip();                                                   // boss aktu I
        CHECK(g.st == status::stage_clear && g.act_cleared);
        int stages_in_act = last_of_act0 + 1;
        CHECK(g.act_bonus == data::acts[0].bonus_per_stage * stages_in_act + data::acts[0].bonus_per_kill * 1);
        CHECK(g.cash == c0 + g.act_bonus + data::enemies[g.enemies[g.boss].def_id].score / data::cash_per_score);
        g.next_stage();
        CHECK(!g.act_cleared && data::stages[g.stage].act == 1);
        while(g.stage < data::stages_count - 1) { g.debug_skip(); g.next_stage(); }
        g.debug_skip();
        CHECK(g.st == status::won);
    }
    // 25. uderzenie bossa: zapowiedź 2 tury wcześniej, zejście z czerwonych pól = unik
    for(int dodge = 0; dodge < 2; ++dodge)
    {
        game g; arena(g, 1);
        g.spawn(data::enemy_betoniarka, 10, 7); g.boss = 0; g.enemies[0].awake = true;
        for(int k = 0; k < 12 && g.slam_timer == 0; ++k) g.player_wait();
        CHECK(g.slam_timer == 2 && g.slam_x == g.hero.x && g.slam_y == g.hero.y);
        CHECK(g.slam_cell(g.hero.x, g.hero.y) && g.slam_cell(g.hero.x + 1, g.hero.y + 1) && !g.slam_cell(g.hero.x + 2, g.hero.y));
        int bx = g.enemies[0].x, hp = g.hero.hp;
        if(dodge) { g.player_move(-1, 0); g.player_move(-1, 0); }
        else { g.player_wait(); g.player_wait(); }
        CHECK(g.slam_timer == 0 && g.enemies[0].x <= bx + 0);             // boss stał w miejscu, ładując cios
        if(dodge) CHECK(g.hero.hp >= hp);
        else CHECK(g.hero.hp <= hp - (data::enemies[data::enemy_betoniarka].min_damage + data::slam_damage_bonus
                                      - (g.cdef().defense + g.def_bonus) / 2));
    }
    // 26. Hurtownia: ceny, efekty
    {
        game g; arena(g, 1);
        CHECK(!g.hurtownia_buy(0) && g.cash == 0);
        g.cash = 1000;
        for(int i = 0; i < data::hurtownia_count; ++i)
        {
            const shop_item_def& it = data::hurtownia[i];
            int cash = g.cash, maxhp = g.hero.max_hp;
            g.hero.hp = 3; g.ability_cd = 9;
            CHECK(g.hurtownia_buy(i) && g.cash == cash - it.price);
            switch(it.effect)
            {
                case shop_effect::heal:    CHECK(g.hero.hp == g.hero.max_hp); break;
                case shop_effect::maxhp:   CHECK(g.hero.max_hp == maxhp + 3); break;
                case shop_effect::ability: CHECK(g.ability_cd == 0); break;
                case shop_effect::tool:    CHECK(g.weapon_override >= 0); break;
                case shop_effect::gear:    { bool any = false; for(int s2 = 0; s2 < data::gear_slots_count; ++s2) any |= g.equipped[s2] >= 1; CHECK(any); break; }
            }
        }
    }
    // 27. stany od problemów budowy
    {
        game g; arena(g, 1);
        g.apply_status(status_effect::poison, 3);
        CHECK(std::strcmp(g.log[log_lines - 1].s, "Zatrucie: -1 HP/turę, 3 t.") == 0);   // skutek i czas w komunikacie
        int hp = g.hero.hp; g.player_wait(); g.player_wait();
        CHECK(g.hero.hp <= hp - 2 + 1 && g.status_turns(status_effect::poison) == 1);   // -1 HP na turę (+ew. odpoczynek)
        g.hero.hp = 1; g.player_wait(); CHECK(g.hero.hp == 1 && g.hero.alive);        // zatrucie nie zabija
    }
    {
        game g; arena(g, 1);
        g.apply_status(status_effect::shock, 1);
        int x = g.hero.x;
        CHECK(g.player_move(-1, 0) && g.hero.x == x && g.turns == 1);                 // porażenie: tura stracona
        CHECK(g.player_move(-1, 0) && g.hero.x == x - 1);
    }
    {
        game g; arena(g, 1);
        g.apply_status(status_effect::slip, 2);
        int x = g.hero.x;
        CHECK(g.player_move(-1, 0) && g.hero.x == x - 2);                             // poślizg: 2 pola
        g.hero.x = 2; CHECK(g.player_move(-1, 0) && g.hero.x == 1);                   // przy ścianie tylko 1
    }
    {
        game g; arena(g, 1);
        g.ability_cd = 1; g.apply_status(status_effect::paper, 0);
        CHECK(g.ability_cd == 4);                                                     // papierologia: +3 tury odnowienia
    }
    {
        int applied = 0;                                                              // Pleśń zatruwa przy trafieniu (~35%)
        for(uint32_t seed = 1; seed <= 200; ++seed)
        {
            game g; g.new_run(1, seed);
            g.enemies_count = 0; g.spawn(data::enemy_plesn, g.hero.x + 1, g.hero.y); g.enemies[0].awake = true;
            g.player_wait();
            applied += g.status_turns(status_effect::poison) > 0;
        }
        CHECK(applied > 30 && applied < 120);
    }
    // 27a. cechy sprzętu: szczęście, kryt, odporność, widzenie, odnowienie mocy
    {
        auto trait_of = [](trait_effect e) { for(int i = 0; i < data::gear_traits_count; ++i) if(data::gear_traits[i].effect == e) return i; return -1; };
        game g; arena(g, 1);
        int luck0 = g.luck(), crit0 = g.crit_pct(), cd0 = g.ability_cooldown();
        g.equip(0, 0, trait_of(trait_effect::luck));
        CHECK(g.luck() == luck0 + 1 && g.crit_pct() == crit0 + data::crit_per_luck_pct);
        g.equip(1, 0, trait_of(trait_effect::crit));
        CHECK(g.crit_pct() == crit0 + data::crit_per_luck_pct + 5);
        g.equip(2, 0, trait_of(trait_effect::cooldown));
        CHECK(g.ability_cooldown() == cd0 - 1);
        g.equip(0, 0, trait_of(trait_effect::poison_res));
        g.apply_status(status_effect::poison, 3); CHECK(g.status_turns(status_effect::poison) == 0);
        g.hero.x = 7; g.hero.y = 7; g.update_fov();
        CHECK(!g.visible(7, 7 + fov_radius + 1) || g.lv.at(7, 7 + fov_radius + 1) == tile::wall);
        game w; w.new_run(0, 3);
        for(auto& row : w.lv.t) for(auto& c : row) c = tile::floor;
        w.enemies_count = 0; w.hero.x = 15; w.hero.y = 15; w.update_fov();
        CHECK(!w.visible(15, 15 + fov_radius + 1));
        w.equip(0, 0, trait_of(trait_effect::sight));
        CHECK(w.sight_radius() == fov_radius + 1 && w.visible(15, 15 + fov_radius + 1));
        int seen = 0;                                                     // dropy sprzętu mają różne cechy
        for(uint32_t seed = 1; seed <= 1500; ++seed)
        {
            game h; h.new_run(1, seed);
            h.enemies_count = 0; h.spawn(0, h.hero.x + 1, h.hero.y); h.enemies[0].hp = 1; h.player_move(1, 0);
            const pickup& p = h.pickups[h.pickups_count - 1];
            if(p.type == gear_box) { CHECK(p.trait < data::gear_traits_count); seen |= 1 << p.trait; }
        }
        CHECK(seen == (1 << data::gear_traits_count) - 1);
    }
    // 28a. szczęście: kryt x2, unik, różne szczęście zawodów
    {
        CHECK(data::classes[5].luck > data::classes[1].luck);                          // Glazurnik > Murarz
        for(int c = 0; c < data::classes_count; ++c) CHECK(data::classes[1].luck <= data::classes[c].luck);
        int crits = 0, hits = 0;
        for(uint32_t seed = 1; seed <= 400; ++seed)
        {
            game g; arena(g, 5); g.r.seed(seed);
            g.spawn(8, 8, 7); g.enemies[0].hp = g.enemies[0].max_hp = 500;
            g.player_move(1, 0);
            for(int i = 0; i < g.hits_count; ++i) if(!g.hits[i].on_hero)
            {
                ++hits; crits += g.hits[i].kind == hit_crit;
                if(g.hits[i].kind == hit_crit) CHECK(g.hits[i].amount % data::crit_multiplier == 0);
            }
        }
        int expect = hits * g_dummy_crit(5) / 100;
        CHECK(crits > expect / 2 && crits < expect * 2);
        int dodges = 0;
        for(uint32_t seed = 1; seed <= 400; ++seed)
        {
            game g; arena(g, 5); g.r.seed(seed);
            g.spawn(8, 8, 7); g.enemies[0].awake = true;
            int hp = g.hero.hp; g.player_wait();
            if(g.hits_count > 0 && g.hits[0].kind == hit_dodge) { ++dodges; CHECK(g.hero.hp >= hp); }
        }
        game gl; arena(gl, 5);
        CHECK(dodges > 400 * gl.dodge_pct() / 300 && dodges < 400 * gl.dodge_pct() * 3 / 100);
        game gm; arena(gm, 1); CHECK(gm.dodge_pct() == 0 && gm.crit_pct() == data::crit_base_pct);
    }
    // 29. statystyki: cechy SIŁ/ZRĘ/INT, Warsztaty (statystyka broni zawodu), Kurs BHP II, narzędzia INT
    {
        auto trait_of = [](trait_effect e) { for(int i = 0; i < data::gear_traits_count; ++i) if(data::gear_traits[i].effect == e) return i; return -1; };
        int t_str = trait_of(trait_effect::str), t_int = trait_of(trait_effect::intel);
        CHECK(t_str >= 0 && t_int >= 0 && trait_of(trait_effect::agi) >= 0);
        game g; arena(g, 1);                                          // Murarz: Kielnia skaluje się z SIŁ
        CHECK(g.hero_stat(stat::str) == data::classes[1].strength && g.stat_bonus(stat::str) == 0);
        g.equip(0, 0, t_str); g.equip(1, 0, t_int);
        CHECK(g.hero_stat(stat::str) == data::classes[1].strength + 1 && g.hero_stat(stat::intel) == data::classes[1].intelligence + 1);
        CHECK(g.hero_stat(stat::agi) == data::classes[1].agility);
        run_mods m; m.craft = 2; m.luck = 1;
        game w; w.new_run(1, 9, data::default_difficulty, m);
        CHECK(w.hero_stat(stat::str) == data::classes[1].strength + 2 && w.hero_stat(stat::intel) == data::classes[1].intelligence);
        CHECK(w.luck() == data::classes[1].luck + 1);
        game k; k.new_run(0, 9, data::default_difficulty, m);           // Kierownik: Dziennik skaluje się z INT
        CHECK(k.hero_stat(stat::intel) == data::classes[0].intelligence + 2 && k.hero_stat(stat::str) == data::classes[0].strength);
        CHECK(mods_stat_bonus(m, 0, stat::intel) == 2 && mods_stat_bonus(m, 0, stat::str) == 0);
        // wyższa statystyka broni = większe obrażenia (ten sam rzut)
        game a0, a1; arena(a0, 1); arena(a1, 1); a1.bonus.craft = 2;
        a0.spawn(8, 8, 7); a1.spawn(8, 8, 7); a0.enemies[0].hp = a1.enemies[0].hp = 999;
        a0.hero_attack(0); a1.hero_attack(0);
        CHECK(a1.enemies[0].hp < a0.enemies[0].hp);
        // Szkolenia: Kurs BHP II i Warsztaty w mods()
        profile p; profile_reset(p);
        int i_luck = -1, i_craft = -1;
        for(int i = 0; i < data::upgrades_count; ++i)
        {
            if(data::upgrades[i].effect == upgrade_effect::luck) i_luck = i;
            if(data::upgrades[i].effect == upgrade_effect::craft) i_craft = i;
        }
        CHECK(i_luck >= 0 && i_craft >= 0 && data::upgrades[i_luck].levels == 2 && data::upgrades[i_craft].levels == 2);
        p.levels[i_luck] = 2; p.levels[i_craft] = 1;
        run_mods pm = mods(p);
        CHECK(pm.luck == 2 && pm.craft == 1);
        // co najmniej 2 narzędzia skalowane INT do odblokowania
        int int_tools = 0;
        for(int i = 0; i < data::tools_count; ++i)
            if(data::weapons[data::tools[i].weapon].scales_with == stat::intel && data::tools[i].cost > 0) ++int_tools;
        CHECK(int_tools >= 3);
    }
    // 30. uprawnienia: każda zdobyta odznaka daje trwałą premię (mods), premie działają w budowie
    {
        profile p; profile_reset(p);
        run_mods m0 = mods(p);
        CHECK(m0.hp == 0 && m0.dmg == 0 && m0.cash == 0 && m0.xp_pct == 0);
        p.badges = uint16_t((1 << data::badges_count) - 1);
        run_mods m = mods(p);
        run_mods sum;
        for(int i = 0; i < data::badges_count; ++i) add_perk(sum, data::badges[i].bonus);
        CHECK(m.hp == sum.hp && m.def == sum.def && m.dmg == sum.dmg && m.luck == sum.luck && m.cooldown == sum.cooldown);
        CHECK(m.tool_pct == sum.tool_pct && m.xp_pct == sum.xp_pct && m.cash == sum.cash && m.crit == sum.crit);
        CHECK(data::badges[data::badge_bez_usterek].bonus.effect == perk_effect::hp && m.hp == 2);
        CHECK(m.dmg >= 1 && m.cooldown >= 1 && m.tool_pct >= 10 && m.luck >= 1 && m.xp_pct >= 10);
        for(int i = 0; i < data::badges_count; ++i) { message pm; perk_label(pm, data::badges[i].bonus); CHECK(pm.n > 0 && pm.n < 30); }
        // premie w budowie
        run_mods pm; pm.cash = 20; pm.sight = 1; pm.cooldown = 1; pm.thermos = 1; pm.crit = 5; pm.xp_pct = 50;
        game a; a.new_run(0, 11); game b; b.new_run(0, 11, data::default_difficulty, pm);
        CHECK(b.cash == a.cash + 20 && b.sight_radius() == a.sight_radius() + 1 && b.thermos_cap() == a.thermos_cap() + 1);
        CHECK(b.ability_cooldown() == a.ability_cooldown() - 1 && b.crit_pct() == a.crit_pct() + 5);
        a.gain_xp(10); b.gain_xp(10);
        CHECK(b.xp() == a.xp() * 3 / 2);
        // Kolekcjoner: 100% - każdy drop to narzędzie
        game t; arena(t, 1); t.bonus.tool_pct = 100; t.bonus.tools = data::start_tools_mask;
        int drops = 0, tools_n = 0;
        for(int k = 0; k < 300; ++k) { t.pickups_count = 0; t.maybe_drop(3, 3); if(t.pickups_count) { ++drops; tools_n += t.pickups[0].type == tool; } }
        CHECK(drops > 0 && tools_n == drops);
    }
    // 31. zlecenia: liczniki budowy -> profil (bez podwójnego liczenia), ukończenie daje doświadczenie
    {
        profile p; profile_reset(p);
        CHECK(data::contracts_count >= 5);
        game g; arena(g, 0);
        g.spawn(8, 8, 7); g.enemies[0].awake = true; g.enemies[0].hp = 1;
        CHECK(g.player_ability() && g.powers_used == 1);               // Odprawa: moc użyta
        g.hero_attack(0); CHECK(g.kills == 1);
        g.equip(0, 2, 0); g.equip(1, 1, 0); CHECK(g.brand_found == 1);
        record_run(p, g); record_run(p, g);                             // drugi raz nic nie dodaje
        CHECK(p.kills_total == 1 && p.powers_total == 1 && p.brand_total == 1 && p.clean_bosses == 0);
        int i_pow = -1; for(int i = 0; i < data::contracts_count; ++i) if(data::contracts[i].kind == contract_kind::powers) i_pow = i;
        CHECK(i_pow >= 0 && contract_progress(p, i_pow) == 1);
        p.powers_total = uint16_t(data::contracts[i_pow].target - 1);
        CHECK(check_contracts(p) == 0);
        g.ability_cd = 0; g.spawn(8, 9, 7); g.enemies[1].awake = true;
        CHECK(g.player_ability());
        record_run(p, g);
        int xp0 = p.xp, got = check_contracts(p);
        CHECK(got == (1 << i_pow) && contract_done(p, i_pow) && p.xp == xp0 + data::contracts[i_pow].xp);
        CHECK(check_contracts(p) == 0);                                  // raz
        p.wins = 1000; CHECK(check_contracts(p) != 0);                     // Stały klient itp.
    }
    // 31a. boss aktu bez obrażeń w walce z nim (Czysta robota)
    {
        game g; g.new_run(1, 21);
        int bs = 0; while(data::stages[bs].boss < 0) ++bs;
        g.start_stage(bs); g.stage_damage = 7;                           // obrażenia przed walką się nie liczą
        CHECK(g.boss_wake_damage < 0);
        g.enemies[g.boss].hp = 1; g.hero_attack(g.boss);
        CHECK(g.clean_bosses == 1 && g.boss_wake_damage == 7);
        game h; h.new_run(1, 21); h.start_stage(bs);
        h.hero_attack(h.boss); h.stage_damage += 3;                      // trafiony w walce z bossem
        h.enemies[h.boss].hp = 1; h.hero_attack(h.boss);
        CHECK(h.clean_bosses == 0);
    }
    // 31b. profil v3 -> v4: wszystkie dotychczasowe pola zostają, nowe od zera
    {
        profile v3; profile_reset(v3);
        std::memcpy(v3.magic, "PBRL003", 8); v3.best = 1234; v3.runs = 9; v3.wins = 4; v3.xp = 321; v3.levels[1] = 2;
        v3.classes = 0x1F; v3.hard = 1; v3.flags = 3; v3.tools = 5; v3.badges = 0x0123; v3.catalog = 0x07FF;
        v3.class_wins = 0x05; v3.tools_found = 0x0B; v3.houses_count = 3; v3.houses[0] = 0x21; v3.houses[2] = 0x35;
        std::memset(reinterpret_cast<char*>(&v3) + profile_v3_size, 0xCD, sizeof v3 - profile_v3_size);   // śmieci
        CHECK(profile_fix(v3) && std::strcmp(v3.magic, profile_magic) == 0);
        CHECK(v3.best == 1234 && v3.runs == 9 && v3.wins == 4 && v3.xp == 321 && v3.levels[1] == 2 && v3.classes == 0x1F);
        CHECK(v3.hard == 1 && v3.flags == 3 && v3.tools == 5 && v3.badges == 0x0123 && v3.catalog == 0x07FF);
        CHECK(v3.class_wins == 0x05 && v3.tools_found == 0x0B && v3.houses_count == 3 && v3.houses[0] == 0x21 && v3.houses[2] == 0x35);
        CHECK(v3.kills_total == 0 && v3.powers_total == 0 && v3.brand_total == 0 && v3.clean_bosses == 0);
        CHECK(v3.contracts == 0 && v3.keepsake == 0);
        for(int i = 0; i < max_keepsakes; ++i) CHECK(v3.keepsake_runs[i] == 0);
        CHECK(!profile_fix(v3));
    }
    // 28. balans: bot gra po 300 runów każdym zawodem na każdym poziomie
    std::printf("%-18s %-9s %6s %6s %6s %8s\n","zawód","poziom","wygr.%","śr.etap","śr.tury","śr.wynik");
    int diff_wins[data::difficulties_count] = {};
    for(int df=0;df<data::difficulties_count;++df)
    for(int c=0;c<data::classes_count;++c)
    {
        int wins=0; long stages=0, turns=0, score=0; const int runs=300;
        for(int k=0;k<runs;++k)
        {
            game g; g.new_run(c, 1000+k*7919, df);
            for(int step=0; step<4000; ++step)
            {
                if(g.st==status::stage_clear){ g.next_stage(); continue; }
                if(g.st!=status::playing) break;
                bot_step(g);
            }
            wins += g.st==status::won; stages += g.stage+1; turns += g.turns; score += g.score;
        }
        diff_wins[df] += wins;
        std::printf("%-18s %-9s %6d %6.1f %6ld %8ld\n", data::classes[c].name, data::difficulties[df].name, wins*100/runs, double(stages)/runs, turns/runs, score/runs);
    }
    for(int df=1;df<data::difficulties_count;++df) CHECK(diff_wins[df-1] > diff_wins[df]);   // trudniej = mniej wygranych
    // pełne ulepszenia ze sklepu wyraźnie pomagają (Normalny, wszystkie zawody)
    {
        profile p; profile_reset(p);
        for(int i=0;i<data::upgrades_count;++i) p.levels[i] = uint8_t(data::upgrades[i].levels);
        run_mods m = mods(p);
        int wins=0; const int runs=300;
        for(int c=0;c<data::classes_count;++c)
            for(int k=0;k<runs;++k)
            {
                game g; g.new_run(c, 1000+k*7919, data::default_difficulty, m);
                for(int step=0; step<4000; ++step)
                {
                    if(g.st==status::stage_clear){ g.next_stage(); continue; }
                    if(g.st!=status::playing) break;
                    bot_step(g);
                }
                wins += g.st==status::won;
            }
        int base = diff_wins[data::default_difficulty];
        std::printf("Normalny: bez ulepszeń %d%%, z pełnymi %d%%\n", base*100/(runs*data::classes_count), wins*100/(runs*data::classes_count));
        CHECK(wins > base);
    }
    std::printf(fails ? "\n%d FAIL\n" : "\nOK - wszystkie testy przeszły\n", fails);
    return fails != 0;
}
