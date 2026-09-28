// Testy rdzenia na PC: g++ -std=c++20 -I../include core_tests.cpp && ./a.out
#include <cstdio>
#include <cstdlib>
#include <cassert>
#include <queue>
#include "core.h"
#include "meta.h"
using namespace core;
static int fails = 0;
static constexpr int F0 = data::prelude_stages;   // bez nagrody Akt 0 budowa zaczyna się od etapu F0 (Fundamenty)
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
static int bot_drinks = 0;          // statystyka: kawy wypite przez bota (czy przedmioty mają znaczenie)
static bool bot_no_coffee = false;  // wariant bez picia kawy (porównanie)
static void bot_step(game& g)
{
    static const int d[4][2]={{1,0},{-1,0},{0,1},{0,-1}};
    if(g.has_offer()) { if(g.offer_is_better()) g.accept_offer(); else g.decline_offer(); }
    if(! bot_no_coffee && g.thermos > 0 && g.hero.hp * 100 < g.hero.max_hp * data::bot_drink_below_pct && g.player_drink()) { ++bot_drinks; return; }
    static int hd[map_h][map_w], td[map_h][map_w];
    bot_cost(g, g.hero.x, g.hero.y, hd);   // koszt drogi (błoto droższe)
    int tx = g.stairs_x, ty = g.stairs_y, best = 999999;
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

// Otwarta arena 14x14 bez wrogów i znajdziek, bohater na (7,7) - do testów mocy.
static void arena(game& g, int cls)
{
    g.new_run(cls, 77);
    for(auto& row : g.lv.t) for(auto& c : row) c = tile::wall;
    for(int y = 1; y <= 14; ++y) for(int x = 1; x <= 14; ++x) g.lv.t[y][x] = tile::floor;
    g.enemies_count = 0; g.pickups_count = 0; g.stairs_x = g.stairs_y = -1;
    g.weather = 0;   // bez pogody (testy mocy i zasięgu)
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
        CHECK(n.enemy_hp_pct() == data::stages[F0].hp_pct);
        for(int i=0;i<n.enemies_count;++i)
            CHECK(n.enemies[i].max_hp == data::enemies[n.enemies[i].def_id].max_health * data::stages[F0].hp_pct / 100);
        game e; e.new_run(0, 42, 0);
        game h; h.new_run(0, 42, 2);
        CHECK(e.enemy_hp_pct() <= n.enemy_hp_pct() && n.enemy_hp_pct() < h.enemy_hp_pct());
        CHECK(e.enemy_dmg_bonus() < n.enemy_dmg_bonus() && n.enemy_dmg_bonus() <= h.enemy_dmg_bonus());
        CHECK(e.score_pct() < n.score_pct() && n.score_pct() < h.score_pct());
        for(int i=0;i<e.enemies_count;++i) CHECK(e.enemies[i].max_hp >= 1);
        // etapy: mnożnik nie maleje
        game s; s.new_run(0, 42, 1);
        int prev_hp = s.enemy_hp_pct(), prev_dmg = s.enemy_dmg_bonus();
        for(int k=F0+1;k<data::stages_count;++k){ s.next_stage(); CHECK(s.enemy_hp_pct() >= prev_hp); CHECK(s.enemy_dmg_bonus() >= prev_dmg); prev_hp=s.enemy_hp_pct(); prev_dmg=s.enemy_dmg_bonus(); }
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
            g.weather = 0; g.r.seed(7);   // ten sam rzut niezależnie od pogody
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
        int dmg0 = data::stages[F0].dmg_bonus;
        CHECK(g.new_game_plus());
        CHECK(g.tier == 1 && g.stage == F0 && g.st == status::playing && g.cls == 2 && g.diff == 1);
        CHECK(g.dmg_bonus == 2 && g.def_bonus == 1 && g.score == 1234 && g.hero.hp == g.hero.max_hp);
        CHECK(g.enemy_hp_pct() > data::stages[F0].hp_pct && g.enemy_dmg_bonus() > dmg0);
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
        for(int k=F0;k<data::stages_count-1;++k){ w.debug_skip(); w.next_stage(); }
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
        for(int i=0;i<data::brigade_count;++i) buy_helper(p, i);
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
        CHECK(p.catalog == 0xFFFF && catalog_count(p) == data::enemies_count && catalog_has(p, data::enemies_count - 1));
        // zebranie narzędzia zapisuje je w liczniku budowy
        game h; arena(h, 1); h.pickups[0] = { h.hero.x, h.hero.y, tool, true, 2 }; h.pickups_count = 1; h.collect();
        CHECK(h.tools_found == (1 << 2));
    }
    // 20. fabuła: wiadomość etapu, NG+ ma własną
    {
        game g; g.new_run(1, 3);
        CHECK(&g.stage_story() == &data::story_stages[F0]);
        g.next_stage(); CHECK(&g.stage_story() == &data::story_stages[F0 + 1]);
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
                game g; g.new_run(1, seed); g.stage = st == 0 ? F0 : data::stages_count - 1;
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
        const int act1 = data::stages[F0].act;   // pierwszy akt budowy bez Aktu 0 (Stan surowy)
        int last_of_act0 = F0;
        while(data::stages[last_of_act0 + 1].act == act1) ++last_of_act0;
        for(int k = F0; k < last_of_act0; ++k) { g.debug_skip(); CHECK(g.st == status::stage_clear && !g.act_cleared); g.next_stage(); }
        CHECK(g.boss >= 0 && data::enemies[g.enemies[g.boss].def_id].slam);
        int c0 = g.cash;
        g.debug_skip();                                                   // boss aktu I
        CHECK(g.st == status::stage_clear && g.act_cleared);
        int stages_in_act = last_of_act0 - F0 + 1;
        CHECK(g.act_bonus == data::acts[act1].bonus_per_stage * stages_in_act + data::acts[act1].bonus_per_kill * 1);
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
    // 25b. Inspekcja Pracy: Kontrola BHP w krzyż (3 tury), wezwania Papierologii (limit), pełny sprzęt = ogłuszenie,
    //      boss w środku aktu: nagroda, etap zaliczony bez Hurtowni
    {
        int is = 0; while(data::stages[is].boss != data::enemy_inspekcja) ++is;
        const enemy_def& id = data::enemies[data::enemy_inspekcja];
        CHECK(is + 1 < data::stages_count && data::stages[is + 1].act == data::stages[is].act);   // w środku aktu
        CHECK(id.shape == slam_shape::cross && id.summon == data::enemy_papierologia && id.summon_max > 0 && id.reward_cash > 0);
        game g; arena(g, 1);
        g.spawn(data::enemy_inspekcja, 10, 7); g.boss = 0; g.enemies[0].awake = true;
        for(int k = 0; k < id.summon_max; ++k) { g.spawn(id.summon, 10, 7); g.enemies[g.enemies_count - 1].alive = false; }
        for(int k = 0; k < 12 && g.slam_timer == 0; ++k) g.player_wait();
        CHECK(g.slam_timer == data::slam_cross_delay && g.slam_x == g.hero.x && g.slam_y == g.hero.y);
        CHECK(g.slam_cell(g.hero.x + data::slam_cross_reach, g.hero.y) && g.slam_cell(g.hero.x, g.hero.y - data::slam_cross_reach));
        CHECK(!g.slam_cell(g.hero.x + 1, g.hero.y + 1) && !g.slam_cell(g.hero.x + data::slam_cross_reach + 1, g.hero.y));
        int hp = g.hero.hp;
        g.player_move(0, 1); g.player_move(-1, 0); g.player_wait();              // zejście z krzyża po skosie
        CHECK(g.slam_timer == 0 && g.hero.hp >= hp);
        int alive_before = 0;
        for(int k = 0; k < 60 && g.st == status::playing; ++k) { g.hero.hp = g.hero.max_hp; g.player_wait(); }
        for(int i = 1; i < g.enemies_count; ++i) alive_before += g.enemies[i].alive;
        CHECK(g.summons_used == id.summon_max && alive_before == id.summon_max);   // limit wezwań
        for(int i = 1; i < g.enemies_count; ++i) CHECK(g.enemies[i].def_id == data::enemy_papierologia);
    }
    {
        int is = 0; while(data::stages[is].boss != data::enemy_inspekcja) ++is;
        const enemy_def& id = data::enemies[data::enemy_inspekcja];
        for(int gear = 0; gear < 2; ++gear)
        {
            game g; g.new_run(1, 31); g.start_stage(is);
            if(gear) for(int i = 0; i < data::gear_slots_count; ++i) g.equip(i, 0, 0);
            CHECK(g.enemies_count == data::stages[is].enemy_count + 1 + id.summon_max);
            CHECK(!g.enemies[g.boss + 1].alive && g.stairs_x < 0);
            g.enemies[g.boss].awake = true; g.enemies[g.boss].x = int8_t(g.hero.x + 5); g.enemies[g.boss].y = g.hero.y;
            g.enemy_act(g.boss);
            CHECK(g.boss_wake_damage >= 0 && g.enemies[g.boss].stun == (gear ? id.gear_stun - 1 : 0));
        }
        game g; g.new_run(1, 31);
        for(int k = F0; k < is; ++k) { g.debug_skip(); if(g.act_cleared) g.act_cleared = false; g.next_stage(); }
        int cash = g.cash, act_kills = g.act_kills;
        g.debug_skip();
        CHECK(g.st == status::stage_clear && !g.act_cleared);
        CHECK(g.cash == cash + id.reward_cash + id.score / data::cash_per_score && g.act_kills == act_kills + 1);
        g.next_stage();
        CHECK(g.stage == is + 1 && g.stairs_x >= 0);
    }
    // 26. Hurtownia: ceny, efekty
    {
        game g; arena(g, 1);
        CHECK(!g.hurtownia_buy(0) && g.cash == 0);
        g.cash = 1000;
        int mat_items = 0;
        for(int i = 0; i < data::hurtownia_count; ++i)
        {
            const shop_item_def& it = data::hurtownia[i];
            if(it.material >= 0)   // płatne materiałem: bez materiału nie, z materiałem - zł zostają
            {
                ++mat_items;
                g.mats[it.material] = 0; CHECK(!g.hurtownia_can(i) && !g.hurtownia_buy(i));
                g.mats[it.material] = uint8_t(it.mat_cost + 1);
            }
            int cash = g.cash, maxhp = g.hero.max_hp, def = g.def_bonus;
            g.hero.hp = 3; g.ability_cd = 9; g.thermos = 0;
            CHECK(g.hurtownia_can(i) && g.hurtownia_buy(i) && g.cash == cash - it.price);
            if(it.material >= 0) CHECK(g.mats[it.material] == 1);
            switch(it.effect)
            {
                case shop_effect::heal:    CHECK(g.hero.hp == g.hero.max_hp); break;
                case shop_effect::maxhp:   CHECK(g.hero.max_hp == maxhp + 3); break;
                case shop_effect::ability: CHECK(g.ability_cd == 0); break;
                case shop_effect::tool:    CHECK(g.weapon_override >= 0); break;
                case shop_effect::gear:    { bool any = false; for(int s2 = 0; s2 < data::gear_slots_count; ++s2) any |= g.equipped[s2] >= 1; CHECK(any); break; }
                case shop_effect::def:     CHECK(g.def_bonus == def + 1); break;
                case shop_effect::thermos: CHECK(g.thermos == imin(2, g.thermos_cap())); break;
            }
        }
        CHECK(mat_items >= 2);
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
        CHECK(i_luck >= 0 && i_craft >= 0 && data::upgrades[i_luck].levels >= 1 && data::upgrades[i_craft].levels >= 1);
        p.levels[i_luck] = 1; p.levels[i_craft] = 1;
        run_mods pm = mods(p);
        CHECK(pm.luck == data::upgrades[i_luck].value && pm.craft == data::upgrades[i_craft].value);
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
    // 31c. postęp zleceń na żywo w trakcie budowy (telefon, zakładka Koszty)
    {
        profile p; profile_reset(p);
        int i_k = -1; for(int i = 0; i < data::contracts_count; ++i) if(data::contracts[i].kind == contract_kind::kills) i_k = i;
        game g; arena(g, 1); g.kills = 7;
        CHECK(contract_progress_live(p, g, i_k) == 7 && contract_progress(p, i_k) == 0);
        record_run(p, g); CHECK(contract_progress_live(p, g, i_k) == 7 && contract_progress(p, i_k) == 7);
        CHECK(next_contract(p, g) >= 0);
        p.contracts = uint8_t((1 << data::contracts_count) - 1); CHECK(next_contract(p, g) == -1);
    }
    // 31a. boss aktu bez obrażeń w walce z nim (Czysta robota)
    {
        game g; g.new_run(1, 21);
        int bs = F0; while(data::stages[bs].boss < 0) ++bs;
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
        std::memcpy(v3.magic, "PBRL003", 8); v3.best = 1234; v3.runs = 9; v3.wins = 4; v3.xp = 321; v3.levels[0] = 2;
        v3.classes = 0x1F; v3.hard = 1; v3.flags = 3; v3.tools = 5; v3.badges = 0x0123; v3.catalog = 0x07FF;
        v3.class_wins = 0x05; v3.tools_found = 0x0B; v3.houses_count = 3; v3.houses[0] = 0x21; v3.houses[2] = 0x35;
        std::memset(reinterpret_cast<char*>(&v3) + profile_v3_size, 0xCD, sizeof v3 - profile_v3_size);   // śmieci
        CHECK(profile_fix(v3) && std::strcmp(v3.magic, profile_magic) == 0);
        CHECK(v3.best == 1234 && v3.runs == 9 && v3.wins == 4 && v3.xp == 321 && v3.levels[0] == 2 && v3.classes == 0x1F);
        CHECK(v3.hard == 1 && v3.flags == 3 && v3.tools == 5 && v3.badges == 0x0123 && v3.catalog == 0x07FF);
        CHECK(v3.class_wins == 0x05 && v3.tools_found == 0x0B && v3.houses_count == 3 && v3.houses[0] == 0x21 && v3.houses[2] == 0x35);
        CHECK(v3.kills_total == 0 && v3.powers_total == 0 && v3.brand_total == 0 && v3.clean_bosses == 0);
        CHECK(v3.contracts == 0 && selected_keepsake(v3) >= 0 && data::keepsakes[selected_keepsake(v3)].start);   // domyślna pamiątka
        for(int i = 0; i < max_keepsakes; ++i) CHECK(v3.keepsake_runs[i] == 0);
        CHECK(!profile_fix(v3));
    }
    // 31d. profil v4 -> v5: pola zostają, znak wodny liczników od zera; bez pamiątki - pierwsza odblokowana
    {
        profile v4; profile_reset(v4);
        std::memcpy(v4.magic, "PBRL004", 8); v4.best = 77; v4.xp = 12; v4.kills_total = 150; v4.powers_total = 40;
        v4.contracts = 0x03; v4.keepsake = 0; v4.keepsake_runs[0] = 4;
        std::memset(reinterpret_cast<char*>(&v4) + profile_v4_size, 0xEE, sizeof v4 - profile_v4_size);   // śmieci
        CHECK(profile_fix(v4) && std::strcmp(v4.magic, profile_magic) == 0);
        CHECK(v4.best == 77 && v4.xp == 12 && v4.kills_total == 150 && v4.powers_total == 40 && v4.contracts == 0x03);
        CHECK(v4.keepsake_runs[0] == 4 && v4.run_kills == 0 && v4.run_powers == 0 && v4.run_brand == 0 && v4.run_clean == 0);
        CHECK(selected_keepsake(v4) >= 0 && data::keepsakes[selected_keepsake(v4)].start);
        CHECK(!profile_fix(v4));
        profile w; profile_reset(w); std::memcpy(w.magic, "PBRL004", 8); w.keepsake = 0; w.badges = 0x01;
        int kb = -1; for(int k = 0; k < data::keepsakes_count; ++k) if(data::keepsakes[k].badge == 0) kb = k;
        w.keepsake = uint8_t(kb >= 0 ? kb + 1 : 0);   // wybrana pamiątka zostaje
        CHECK(profile_fix(w) && w.keepsake == uint8_t(kb >= 0 ? kb + 1 : 1));
        profile n; profile_reset(n); CHECK(selected_keepsake(n) >= 0 && data::keepsakes[selected_keepsake(n)].start);   // nowy profil
    }
    // 31e. wyłączenie konsoli po zaliczonym etapie i wznowienie z autozapisu na starcie etapu:
    // liczniki zleceń z tego etapu nie liczą się drugi raz
    {
        profile p; profile_reset(p);
        static game g; g.new_run(1, 4242, 0, mods(p)); start_run(p);
        static run_save rs; run_save_make(rs, g);                        // autozapis na starcie etapu
        for(int k = 0; k < 4000 && g.st == status::playing; ++k) bot_step(g);
        CHECK(g.st == status::stage_clear && g.kills > 0);
        g.powers_used = 2;
        check_badges(p, g);                                              // koniec etapu: liczniki do profilu (SRAM)
        int kills1 = p.kills_total, pw1 = p.powers_total;
        CHECK(kills1 == g.kills && pw1 == 2);
        static game h; std::memcpy(&h, &rs.g, sizeof h);                 // wznowienie: stan ze startu etapu
        int i_k = -1; for(int i = 0; i < data::contracts_count; ++i) if(data::contracts[i].kind == contract_kind::kills) i_k = i;
        CHECK(contract_progress_live(p, h, i_k) == kills1);              // telefon nie pokazuje etapu dwa razy
        for(int k = 0; k < 4000 && h.st == status::playing; ++k) bot_step(h);
        CHECK(h.st == status::stage_clear && h.kills == g.kills);        // ten sam etap jeszcze raz
        h.powers_used = 2;
        check_badges(p, h);
        CHECK(p.kills_total == kills1 && p.powers_total == pw1);        // bez podwójnego liczenia
        h.kills += 2; h.powers_used = 3; record_run(p, h);              // powtórka dała więcej: tylko nadwyżka
        CHECK(p.kills_total == kills1 + 2 && p.powers_total == pw1 + 1);
        start_run(p);                                                    // nowa budowa liczy od zera
        game n; n.new_run(1, 5); n.kills = 3; record_run(p, n);
        CHECK(p.kills_total == kills1 + 5 && p.run_kills == 3);
    }
    // 32. pamiątki: odblokowanie (start / odznaka / zlecenie), wybór, ranga po 3 i 8 budowach, premia w mods
    {
        profile p; profile_reset(p);
        CHECK(data::keepsakes_count >= 5);
        int start_k = -1, badge_k = -1, contract_k = -1, contract_i = -1;
        for(int k = 0; k < data::keepsakes_count; ++k)
        {
            if(data::keepsakes[k].start) start_k = k;
            else if(data::keepsakes[k].badge >= 0) badge_k = k;
        }
        for(int i = 0; i < data::contracts_count; ++i) if(data::contracts[i].keepsake >= 0) { contract_i = i; contract_k = data::contracts[i].keepsake; }
        CHECK(start_k >= 0 && badge_k >= 0 && contract_k >= 0);
        CHECK(keepsake_unlocked(p, start_k) && !keepsake_unlocked(p, badge_k) && !keepsake_unlocked(p, contract_k));
        CHECK(selected_keepsake(p) == start_k);                             // nowy profil: pamiątka startowa
        cycle_keepsake(p, 1); CHECK(p.keepsake == 0);                       // zablokowane są pomijane -> "bez pamiątki"
        cycle_keepsake(p, 1); CHECK(selected_keepsake(p) == start_k);
        cycle_keepsake(p, -1); CHECK(p.keepsake == 0);
        p.badges = uint16_t(1 << data::keepsakes[badge_k].badge);
        CHECK(keepsake_unlocked(p, badge_k));
        p.contracts = uint8_t(1 << contract_i);
        CHECK(keepsake_unlocked(p, contract_k));
        p.keepsake = uint8_t(contract_k + 1);
        CHECK(selected_keepsake(p) == contract_k && keepsake_rank(p, contract_k) == 1);
        run_mods m1 = mods(p);
        run_mods e; add_perk(e, { data::keepsakes[contract_k].effect, data::keepsakes[contract_k].values[0] });
        CHECK(m1.luck + m1.sight + m1.cooldown + m1.thermos + m1.def >= e.luck + e.sight + e.cooldown + e.thermos + e.def);
        int runs0 = p.runs;
        for(int r = 0; r < data::keepsake_rank_runs[0]; ++r) start_run(p);
        CHECK(p.runs == runs0 + data::keepsake_rank_runs[0] && keepsake_rank(p, contract_k) == 2);
        CHECK(keepsake_perk(p, contract_k).value == data::keepsakes[contract_k].values[1]);
        for(int r = data::keepsake_rank_runs[0]; r < data::keepsake_rank_runs[1]; ++r) start_run(p);
        CHECK(keepsake_rank(p, contract_k) == 3 && keepsake_perk(p, contract_k).value == data::keepsakes[contract_k].values[2]);
        CHECK(p.keepsake_runs[start_k] == 0);                               // licznik tylko wybranej
        p.contracts = 0; CHECK(selected_keepsake(p) == -1);                  // zablokowana nie działa
        // Termos babci: miejsce w termosie
        profile q; profile_reset(q); q.keepsake = uint8_t(start_k + 1);
        game g; g.new_run(1, 3, data::default_difficulty, mods(q));
        if(data::keepsakes[start_k].effect == perk_effect::thermos) CHECK(g.thermos_cap() == data::thermos_capacity + data::keepsakes[start_k].values[0]);
    }
    // 33. wydarzenia na placu: nie na pierwszym etapie i nie u bossa, efekty
    {
        int counts[data::stages_count] = {};
        for(int k = 0; k < 200; ++k)
        {
            game g; g.new_run(k % data::classes_count, 500 + k * 31);
            for(int st = F0; st < data::stages_count; ++st)
            {
                if(st > F0) g.next_stage();
                if(g.stage_event >= 0) ++counts[st];
                CHECK(g.stage_event < data::site_events_count);
            }
        }
        CHECK(counts[F0] == 0);
        for(int st = F0; st < data::stages_count; ++st)
            if(data::stages[st].boss >= 0) CHECK(counts[st] == 0);
            else if(st > F0) CHECK(counts[st] > 200 * data::site_event_chance_pct / 300 &&   // zła pogoda zabiera złe wydarzenia
                                   counts[st] < 200 * (data::site_event_chance_pct + 20) / 100);
        auto ev_of = [](event_effect e) { for(int i = 0; i < data::site_events_count; ++i) if(data::site_events[i].effect == e) return i; return -1; };
        for(event_effect e : { event_effect::fewer_pickups, event_effect::cash, event_effect::inspection, event_effect::rain, event_effect::thermos })
            CHECK(ev_of(e) >= 0);
        game g; g.new_run(1, 44); g.stage_event = -1;
        int pk = g.pickups_count, cash = g.cash;
        g.apply_event(ev_of(event_effect::fewer_pickups));
        CHECK(g.pickups_count == imax(1, pk - data::site_events[ev_of(event_effect::fewer_pickups)].value) && g.event_active(event_effect::fewer_pickups));
        g.apply_event(ev_of(event_effect::cash)); CHECK(g.cash == cash + data::site_events[ev_of(event_effect::cash)].value);
        g.thermos = 0; g.apply_event(ev_of(event_effect::thermos)); CHECK(g.thermos == g.thermos_cap());
        // inspekcja: etap bez obrażeń = premia doświadczenia; z obrażeniami nic
        game a; a.new_run(1, 44); a.apply_event(ev_of(event_effect::inspection));
        game b; b.new_run(1, 44);
        a.debug_skip(); b.debug_skip();
        CHECK(a.st == status::stage_clear && a.xp() == b.xp() + data::site_events[ev_of(event_effect::inspection)].value);
        game c; c.new_run(1, 44); c.apply_event(ev_of(event_effect::inspection)); c.stage_damage = 1; c.debug_skip();
        CHECK(c.xp() == b.xp());
        // ulewa: ciosy częściej dają poślizg
        int slips_rain = 0, slips_dry = 0;
        for(int k = 0; k < 200; ++k)
            for(int rain = 0; rain < 2; ++rain)
            {
                game h; arena(h, 1); h.hero.max_hp = h.hero.hp = 999;
                if(rain) h.apply_event(ev_of(event_effect::rain));
                h.r.seed(900 + k); h.spawn(data::enemy_kornik, 8, 7); h.enemies[0].awake = true;
                h.player_wait();
                (rain ? slips_rain : slips_dry) += h.status_turns(status_effect::slip) > 0;
            }
        CHECK(slips_dry == 0 && slips_rain > 20);
    }
    // 34. pogoda dnia: losowana na starcie etapu z listy dozwolonych, skutki, bez stosu niekorzystnych
    {
        int seen[data::stages_count][8] = {};
        int bad_stack = 0;
        for(int k = 0; k < 300; ++k)
        {
            game g; g.new_run(k % data::classes_count, 900 + k * 17);
            for(int st = F0; st < data::stages_count; ++st)
            {
                if(st > F0) g.next_stage();
                CHECK(g.weather >= 0 && g.weather < data::weather_count);
                CHECK(data::weather[g.weather].stages & (1u << st));
                ++seen[st][g.weather];
                if(g.stage_event >= 0 && g.wdef().bad && ! data::site_events[g.stage_event].good) ++bad_stack;
            }
        }
        if(data::weather_no_bad_stack) CHECK(bad_stack == 0);
        for(int st = F0; st < data::stages_count; ++st)
        {
            int allowed_weight = 0;
            for(int w = 0; w < data::weather_count; ++w) if(data::weather[w].stages & (1u << st)) allowed_weight += data::weather[w].weight;
            for(int w = 0; w < data::weather_count; ++w)
                if(data::weather[w].stages & (1u << st))   // częstość mniej więcej jak waga (300 prób)
                    CHECK(seen[st][w] > 300 * data::weather[w].weight / allowed_weight / 3);
                else CHECK(seen[st][w] == 0);
        }
        auto wx_of = [](weather_effect e) { for(int i = 0; i < data::weather_count; ++i) if(data::weather[i].effect == e) return i; return -1; };
        for(weather_effect e : { weather_effect::none, weather_effect::heat, weather_effect::frost, weather_effect::wind, weather_effect::rain })
            CHECK(wx_of(e) >= 0);
        // upał: moc odnawia się dłużej
        game h; arena(h, 0); int cd = h.ability_cooldown();
        h.weather = int8_t(wx_of(weather_effect::heat)); CHECK(h.ability_cooldown() == cd + data::weather[h.weather].value);
        // wiatr: broń z dystansu krótsza (min. 1), wręcz bez zmian
        for(int c = 0; c < data::classes_count; ++c)
        {
            game w; arena(w, c); int rg = w.weapon().range;
            w.weather = int8_t(wx_of(weather_effect::wind));
            bool windproof = data::classes[c].passive == class_passive::windproof;   // Dekarz: wiatr mu nie przeszkadza
            CHECK(w.weapon_range() == (rg > 1 && ! windproof ? imax(1, rg - data::weather[w.weather].value) : rg));
        }
        {   // wiatr: cel na granicy zasięgu przestaje być w zasięgu
            game w; arena(w, 2); int rg = w.weapon().range; CHECK(rg > 1);
            w.spawn(data::enemy_kornik, 7 + rg, 7); w.update_fov();
            CHECK(w.nearest_target() == 0);
            w.weather = int8_t(wx_of(weather_effect::wind));
            CHECK(w.nearest_target() < 0 && ! w.player_attack(0));
        }
        // mróz: problemy (nie bossowie) stoją co value tur
        {
            game f; arena(f, 1); f.weather = int8_t(wx_of(weather_effect::frost));
            f.spawn(data::enemy_kornik, 12, 7); f.enemies[0].awake = true; f.hero.max_hp = f.hero.hp = 999;
            int moved = 0, stood = 0;
            for(int t = 0; t < 12; ++t)
            {
                int ox = f.enemies[0].x; f.player_wait();
                bool frozen = f.turns % data::weather[f.weather].value == 0;
                if(cheb(f.enemies[0].x, f.enemies[0].y, f.hero.x, f.hero.y) <= 1) break;
                (f.enemies[0].x == ox ? stood : moved) += 1;
                CHECK(frozen == (f.enemies[0].x == ox));
            }
            CHECK(stood >= 1 && moved >= 2);
        }
        // deszcz: kałuże tylko na podłodze, wejście w kałużę = poślizg
        {
            game r; arena(r, 1); r.weather = int8_t(wx_of(weather_effect::rain));
            int puddles = 0, floors = 0;
            for(int y = 0; y < map_h; ++y) for(int x = 0; x < map_w; ++x)
            {
                if(r.puddle(x, y)) { ++puddles; CHECK(r.lv.at(x, y) == tile::floor); }
                floors += r.lv.at(x, y) == tile::floor;
            }
            CHECK(puddles > 0 && puddles < floors / 3);
            game dry; arena(dry, 1); CHECK(! dry.puddle(7, 7));
            int px = -1, py = 7;
            for(int x = 2; x <= 13; ++x) if(r.puddle(x, 7) && ! r.puddle(x - 1, 7)) { px = x; break; }
            if(px < 0) { py = 8; for(int x = 2; x <= 13; ++x) if(r.puddle(x, 8) && ! r.puddle(x - 1, 8)) { px = x; break; } }
            CHECK(px >= 0);
            r.hero.x = int8_t(px - 1); r.hero.y = int8_t(py); r.update_fov();
            CHECK(r.status_turns(status_effect::slip) == 0);
            CHECK(r.player_move(1, 0) && r.hero.x == px && r.status_turns(status_effect::slip) > 0);
        }
    }
    // 35. brygada: raz na etap, za budżet, odblokowanie w Szkoleniach, skutki fachowców; profil v5 -> v6
    {
        auto hx_of = [](helper_effect e) { for(int i = 0; i < data::brigade_count; ++i) if(data::brigade[i].effect == e) return i; return -1; };
        int geo = hx_of(helper_effect::reveal), pump = hx_of(helper_effect::pump), bhp = hx_of(helper_effect::safety), ally = hx_of(helper_effect::ally);
        CHECK(geo >= 0 && pump >= 0 && bhp >= 0 && ally >= 0);
        // odblokowanie: startowi od razu, reszta za doświadczenie
        profile p; profile_reset(p);
        for(int i = 0; i < data::brigade_count; ++i) CHECK(helper_unlocked(p, i) == (data::brigade[i].cost == 0));
        int locked = -1; for(int i = 0; i < data::brigade_count; ++i) if(! helper_unlocked(p, i)) { locked = i; break; }
        CHECK(locked >= 0 && ! buy_helper(p, locked));
        p.xp = 500; CHECK(buy_helper(p, locked) && helper_unlocked(p, locked) && p.xp == 500 - data::brigade[locked].cost && ! buy_helper(p, locked));
        CHECK(mods(p).helpers == (data::start_helpers_mask | (1 << locked)));
        CHECK(shop_spent(p) == data::brigade[locked].cost);
        // Geodeta: mapa odkryta (podłoga i schody), budżet, raz na etap, tura
        {
            game g; g.new_run(1, 314); g.weather = 0;
            g.cash = 0; CHECK(g.helper_blocked(geo) == game::helper_cash && ! g.call_helper(geo) && g.turns == 0);
            g.cash = 100; CHECK(! g.explored(g.stairs_x, g.stairs_y));
            CHECK(g.helper_blocked(geo) == game::helper_ok && g.call_helper(geo));
            CHECK(g.turns == 1 && g.cash == 100 - data::brigade[geo].price && g.helper_called == geo);
            CHECK(g.explored(g.stairs_x, g.stairs_y));
            for(int y = 0; y < map_h; ++y) for(int x = 0; x < map_w; ++x) if(g.lv.passable(x, y)) CHECK(g.explored(x, y));
            CHECK(g.helper_blocked(bhp) == game::helper_used && ! g.call_helper(bhp) && g.turns == 1);
            g.debug_skip(); CHECK(g.st == status::stage_clear); g.next_stage();
            CHECK(g.helper_called < 0 && g.helper_blocked(geo) == game::helper_ok);   // kolejny etap: znowu można
            game l; l.new_run(1, 314); l.cash = 999;
            if(locked >= 0) CHECK(l.helper_blocked(locked) == game::helper_locked && ! l.call_helper(locked));
        }
        // BHP-owiec: zdejmuje stany, obrona przez kilka tur (mniejsze obrażenia)
        {
            game g; arena(g, 1); g.cash = 100;
            g.apply_status(status_effect::poison, 5); g.apply_status(status_effect::slip, 5);
            int def0 = g.hero_defense();
            CHECK(g.call_helper(bhp));
            CHECK(g.status_turns(status_effect::poison) == 0 && g.status_turns(status_effect::slip) == 0);
            CHECK(g.hero_defense() == def0 + data::brigade[bhp].value && g.guard_turns == data::brigade[bhp].turns);
            for(int t = 0; t < data::brigade[bhp].turns; ++t) g.player_wait();
            CHECK(g.guard_turns == 0 && g.hero_defense() == def0);
        }
        // pompa: beton na problemy w zasięgu (bez rzutu), poza zasięgiem nic; bez celu nie wzywa
        {
            game g; arena(g, 1); g.cash = 100; g.bonus.helpers = (1 << data::brigade_count) - 1;
            CHECK(g.helper_blocked(pump) == game::helper_no_target);
            int reach = data::brigade[pump].reach;
            g.spawn(data::enemy_papierologia, 7 + reach, 7); g.spawn(data::enemy_papierologia, 7 + reach + 2, 7);
            g.enemies[0].hp = g.enemies[0].max_hp = 50; g.enemies[1].hp = g.enemies[1].max_hp = 50;
            CHECK(g.call_helper(pump));
            CHECK(g.enemies[0].hp == 50 - data::brigade[pump].value && g.enemies[1].hp == 50);
            game k; arena(k, 1); k.cash = 100; k.bonus.helpers = (1 << data::brigade_count) - 1; k.spawn(data::enemy_kornik, 8, 7);
            k.enemies[0].hp = 1; int kills = k.kills;
            CHECK(k.call_helper(pump) && ! k.enemies[0].alive && k.kills == kills + 1);   // usunięcie liczy się jak zwykle
        }
        // pomocnik: stoi obok, idzie za bohaterem, bije sąsiadów przez kilka tur, blokuje pole
        {
            game g; arena(g, 1); g.cash = 100; g.bonus.helpers = (1 << data::brigade_count) - 1;
            CHECK(g.call_helper(ally));
            CHECK(g.ally_turns == data::brigade[ally].turns - 1 && cheb(g.ally_x, g.ally_y, g.hero.x, g.hero.y) == 1);
            CHECK(g.occupied(g.ally_x, g.ally_y));
            for(int k = 0; k < 3; ++k) { g.player_move(1, 0); CHECK(cheb(g.ally_x, g.ally_y, g.hero.x, g.hero.y) == 1); }
            int ex = -1, ey = -1;
            for(const auto& o : game::around8) { int x = g.ally_x + o[0], y = g.ally_y + o[1];
                if(g.lv.at(x, y) == tile::floor && ! g.occupied(x, y) && cheb(x, y, g.hero.x, g.hero.y) > 1) { ex = x; ey = y; break; } }
            CHECK(ex >= 0);
            g.spawn(data::enemy_papierologia, ex, ey); int ei = g.enemies_count - 1;
            g.enemies[ei].hp = g.enemies[ei].max_hp = 50; g.enemies[ei].stun = 50;
            g.player_wait();
            CHECK(g.enemies[ei].hp < 50);
            while(g.ally_turns > 0) g.player_wait();
            CHECK(g.ally_x < 0 && g.ally_turns == 0);
        }
        // profil v5 -> v6: stare pola zostają, nowe od zera
        {
            profile v5; profile_reset(v5); std::memcpy(v5.magic, "PBRL005", 8); v5.best = 4321; v5.run_clean = 3; v5.xp = 99;
            std::memset(reinterpret_cast<char*>(&v5) + profile_v5_size, 0xEE, sizeof v5 - profile_v5_size);
            CHECK(profile_fix(v5) && std::strcmp(v5.magic, profile_magic) == 0 && v5.best == 4321 && v5.run_clean == 3 && v5.xp == 99);
            CHECK(v5.brigade == 0 && v5.investor == 0);
            for(int i = 0; i < 8; ++i) CHECK(v5.best_stake[i] == 0);
        }
    }
    // 36. tryb inwestora: odblokowanie po wygranej, stawka i premia doświadczenia, skutki modyfikatorów, rekord stawki
    {
        auto iv_of = [](investor_effect e) { for(int i = 0; i < data::investor_count; ++i) if(data::investor[i].effect == e) return i; return -1; };
        for(investor_effect e : { investor_effect::cash_pct, investor_effect::no_break, investor_effect::enemy_hp, investor_effect::no_shop,
                                  investor_effect::slam, investor_effect::enemy_dmg }) CHECK(iv_of(e) >= 0);
        const int all = (1 << data::investor_count) - 1;
        profile p; profile_reset(p); p.investor = uint8_t(all);
        CHECK(! investor_unlocked(p) && mods(p).investor == 0);            // przed pierwszą wygraną nie działa
        int xp0 = mods(p).xp_pct;
        p.wins = 1;
        run_mods m = mods(p);
        CHECK(m.investor == all && m.xp_pct == xp0 + investor_xp(all) && investor_stake(all) > 0);
        toggle_investor(p, 0); CHECK(mods(p).investor == (all & ~1)); toggle_investor(p, 0);
        game a; a.new_run(1, 55); game b; b.new_run(1, 55, data::default_difficulty, m);
        CHECK(b.enemy_hp_pct() == a.enemy_hp_pct() * (100 + data::investor[iv_of(investor_effect::enemy_hp)].value) / 100);
        CHECK(b.enemy_dmg_bonus() == a.enemy_dmg_bonus() + data::investor[iv_of(investor_effect::enemy_dmg)].value);
        CHECK(b.income(100) == 100 + data::investor[iv_of(investor_effect::cash_pct)].value && a.income(100) == 100);
        CHECK(b.slam_every() == data::slam_every - data::investor[iv_of(investor_effect::slam)].value && a.slam_every() == data::slam_every);
        CHECK(b.shop_closed() && ! a.shop_closed());
        a.hero.hp = 5; b.hero.hp = 5; a.debug_skip(); b.debug_skip(); a.next_stage(); b.next_stage();
        CHECK(a.hero.hp == 10 && b.hero.hp == 5);                           // bez przerwy na kawę
        CHECK(b.xp_pct > 0 && b.bonus.xp_pct == m.xp_pct);
        // rekord stawki tylko za wygraną, per zawód
        profile q; profile_reset(q); q.wins = 1; q.investor = uint8_t(all);
        game w; w.new_run(3, 9, data::default_difficulty, mods(q));
        record_run(q, w); CHECK(q.best_stake[3] == 0);
        w.st = status::won; record_run(q, w); CHECK(q.best_stake[3] == investor_stake(all) && q.best_stake[2] == 0);
        q.investor = 1; game w2; w2.new_run(3, 9, data::default_difficulty, mods(q)); w2.st = status::won; record_run(q, w2);
        CHECK(q.best_stake[3] == investor_stake(all));                      // niższa stawka nie psuje rekordu
        // modyfikatory utrudniają (bot, Normalny)
        int base = 0, hard = 0;
        for(int k = 0; k < 120; ++k)
            for(int mode = 0; mode < 2; ++mode)
            {
                run_mods mm; if(mode) mm.investor = all;
                game g; g.new_run(k % data::classes_count, 3000 + k * 131, data::default_difficulty, mm);
                for(int step = 0; step < 4000; ++step)
                {
                    if(g.st == status::stage_clear) { g.next_stage(); continue; }
                    if(g.st != status::playing) break;
                    bot_step(g);
                }
                (mode ? hard : base) += g.st == status::won;
            }
        std::printf("Tryb inwestora (wszystkie modyfikatory, Normalny): %d%% vs bez %d%%\n", hard * 100 / 120, base * 100 / 120);
        CHECK(hard < base);
    }
    // 37. wybór ścieżki: dwie różne oferty, deterministyczne; skutki wariantu na kolejnym etapie
    {
        auto px_of = [](auto pred) { for(int i = 0; i < data::paths_count; ++i) if(pred(data::paths[i])) return i; return -1; };
        game g; g.new_run(1, 777);
        for(int st = 0; st < data::stages_count - 1; ++st)
        {
            g.stage = st;
            CHECK(g.path_offer(0) != g.path_offer(1) && g.path_offer(0) >= 0 && g.path_offer(1) < data::paths_count);
            game h; h.new_run(1, 777); h.stage = st; CHECK(h.path_offer(0) == g.path_offer(0) && h.path_offer(1) == g.path_offer(1));
        }
        int seen = 0;
        for(uint32_t seed = 1; seed <= 200; ++seed) { game k; k.new_run(0, seed); seen |= (1 << k.path_offer(0)) | (1 << k.path_offer(1)); }
        CHECK(seen == (1 << data::paths_count) - 1);   // każda ścieżka bywa w ofercie
        for(int k = 0; k < 2; ++k)   // wybór 0/1 = pozycja w ofercie; po etapie wybór wraca na pierwszą
        {
            game a; a.new_run(2, 4242); a.debug_skip();
            int want = a.path_offer(k);
            a.choose_path(k); a.next_stage();
            CHECK(a.stage == F0 + 1 && a.stage_path == want && a.next_path == 0);
        }
        game f; f.new_run(2, 99); CHECK(f.stage_path == -1);   // pierwszy etap bez ścieżki
        // skutki: start_stage z konkretną ścieżką vs bez (ten sam seed)
        int more = px_of([](const path_def& p) { return p.enemies > 0; }), fewer = px_of([](const path_def& p) { return p.enemies < 0; });
        int calm = px_of([](const path_def& p) { return p.no_event; }), risky = px_of([](const path_def& p) { return p.bad_weather; });
        int stock = px_of([](const path_def& p) { return p.materials > 0; });
        CHECK(more >= 0 && fewer >= 0 && calm >= 0 && risky >= 0 && stock >= 0);
        int bs = F0; while(data::stages[bs].boss < 0) ++bs;   // etap z bossem: bez wydarzenia na placu (budżet bez premii)
        for(int path : { more, fewer })
        {
            game a; a.new_run(1, 31); a.cash = 50; a.r.seed(1234); a.start_stage(bs);
            game b; b.new_run(1, 31); b.cash = 50; b.r.seed(1234); b.start_stage(bs, path);
            CHECK(b.enemies_count == a.enemies_count + data::paths[path].enemies);
            CHECK(b.cash == imax(0, 50 + data::paths[path].cash));
        }
        int bad_ok = 0, events = 0;
        for(uint32_t seed = 1; seed <= 100; ++seed)
        {
            game w; w.new_run(1, seed); w.start_stage(F0 + 1, risky); bad_ok += data::weather[w.weather].bad;
            game e; e.new_run(1, seed); e.start_stage(F0 + 1, calm); events += e.stage_event >= 0;
        }
        CHECK(bad_ok == 100 && events == 0);
        game m; m.new_run(1, 5); m.start_stage(F0 + 1, stock);
        CHECK(m.mats[0] + m.mats[1] + m.mats[2] == data::paths[stock].materials);
    }
    // 38. materiały: z problemów, bossów i paczek, limit; naprawy: Załataj (mur z desek), Kładka (kałuże bez poślizgu)
    {
        int got = 0;
        for(uint32_t seed = 1; seed <= 400; ++seed)
        {
            game g; arena(g, 1); g.r.seed(seed);
            g.spawn(data::enemy_kornik, 8, 7); g.enemies[0].hp = 1; g.player_move(1, 0);
            int n = g.mats[0] + g.mats[1] + g.mats[2];
            CHECK(n <= 1);
            if(n) { ++got; CHECK(g.mats[data::enemies[data::enemy_kornik].material] == 1); }   // Kornik: drewno
        }
        CHECK(got > 400 * data::material_drop_pct / 200 && got < 400 * data::material_drop_pct * 2 / 100);
        game b; arena(b, 1); b.spawn(data::enemy_betoniarka, 8, 7); b.boss = 0; b.enemies[0].hp = 1; b.player_move(1, 0);
        for(int m = 0; m < data::materials_count; ++m) CHECK(b.mats[m] == data::material_boss_drop);   // boss: każdego po kilka
        game c; arena(c, 1); for(int k = 0; k < 30; ++k) c.add_material(2); CHECK(c.mats[2] == data::material_max);   // limit
        game q; arena(q, 1); q.pickups[0] = { q.hero.x, q.hero.y, gear_box, true, 0, 0 }; q.pickups_count = 1; q.collect();
        CHECK(q.mats[0] + q.mats[1] + q.mats[2] == data::material_gear_box);   // paczka: też materiał
        auto rx_of = [](repair_effect e) { for(int i = 0; i < data::repairs_count; ++i) if(data::repairs[i].effect == e) return i; return -1; };
        int patch = rx_of(repair_effect::patch), bridge = rx_of(repair_effect::bridge);
        CHECK(patch >= 0 && bridge >= 0);
        {   // Załataj: bez drewna / bez celu nic; z celem mur 3 pola przed bohaterem, kosztuje drewno i turę
            game g; arena(g, 1);
            const repair_def& rd = data::repairs[patch];
            CHECK(g.repair_blocked(patch) == game::repair_material && !g.player_repair(patch) && g.turns == 0);
            g.mats[rd.material] = 3;
            CHECK(g.repair_blocked(patch) == game::repair_no_target);
            g.spawn(data::enemy_kornik, 11, 7);
            CHECK(g.repair_blocked(patch) == game::repair_ok && g.player_repair(patch));
            CHECK(g.turns == 1 && g.mats[rd.material] == 3 - rd.cost);
            CHECK(g.lv.at(8, 6) == tile::wall && g.lv.at(8, 7) == tile::wall && g.lv.at(8, 8) == tile::wall && g.walls[0].turns == rd.value - 1);
            for(int k = 0; k < rd.value; ++k) g.player_wait();
            CHECK(g.lv.at(8, 7) == tile::floor);
        }
        {   // Kładka: tylko przy kałużach; kałuże w zasięgu przestają działać, poślizg znika
            const repair_def& rd = data::repairs[bridge];
            game dry; arena(dry, 1); dry.stage = F0 + 4; dry.mats[rd.material] = 3;   // akt II: bez błota (Kładka działa też na błoto)
            CHECK(dry.repair_blocked(bridge) == game::repair_no_puddle && !dry.player_repair(bridge));
            game r; arena(r, 1); r.weather = 0;
            for(int i = 0; i < data::weather_count; ++i) if(data::weather[i].effect == weather_effect::rain) r.weather = int8_t(i);
            r.mats[rd.material] = 3;
            int near0 = 0; for(int y = 7 - rd.value; y <= 7 + rd.value; ++y) for(int x = 7 - rd.value; x <= 7 + rd.value; ++x) near0 += r.puddle(x, y);
            if(near0 > 0)
            {
                r.apply_status(status_effect::slip, 3);
                CHECK(r.player_repair(bridge) && r.mats[rd.material] == 3 - rd.cost && r.status_turns(status_effect::slip) == 0);
                int near1 = 0; for(int y = 7 - rd.value; y <= 7 + rd.value; ++y) for(int x = 7 - rd.value; x <= 7 + rd.value; ++x) near1 += r.puddle(x, y);
                CHECK(near1 == 0);
                r.debug_skip(); r.next_stage(); CHECK(r.bridges == 0);   // kolejny etap: kładki zostają na starym placu
            }
            else CHECK(r.repair_blocked(bridge) == game::repair_no_puddle);
        }
    }
    // 39. codzienna budowa: data -> numer dnia -> seed, zawód i modyfikatory dnia, wyniki w profilu; profil v6 -> v7
    {
        CHECK(daily_number(data::daily_epoch[0], data::daily_epoch[1], data::daily_epoch[2]) == 1);
        CHECK(daily_number(2026, 9, 25) - daily_number(2026, 9, 24) == 1 && daily_number(2027, 1, 1) - daily_number(2026, 12, 31) == 1);
        CHECK(days_in_month(2028, 2) == 29 && days_in_month(2026, 2) == 28 && days_in_month(2026, 12) == 31);
        for(int z = 19000; z < 22000; z += 37) { int y, m, d; civil_from_days(z, y, m, d); CHECK(days_from_civil(y, m, d) == z); }
        int day = daily_number(2026, 9, 25);
        CHECK(daily_seed(day) == daily_seed(day) && daily_seed(day) != daily_seed(day + 1));
        game a; start_daily(a, day); game b; start_daily(b, day);
        CHECK(a.daily && a.daily_day == day && a.cls == daily_class(daily_seed(day)) && a.diff == data::daily_difficulty);
        CHECK(a.bonus.investor == daily_investor(daily_seed(day)) && a.bonus.hp == 0 && a.bonus.def == 0);
        CHECK(std::memcmp(a.lv.t, b.lv.t, sizeof a.lv.t) == 0 && a.hero.x == b.hero.x);   // ten sam dzień = ta sama budowa
        int bits = 0; for(int i = 0; i < data::investor_count; ++i) bits += (a.bonus.investor >> i) & 1;
        CHECK(bits == data::daily_investor_mods);
        int classes_seen = 0; for(int d = 1; d <= 60; ++d) classes_seen |= 1 << daily_class(daily_seed(d));
        CHECK(classes_seen == (1 << data::classes_count) - 1);
        profile p; profile_reset(p);
        int y, m, d; daily_date(p, y, m, d);
        CHECK(y == data::daily_default_date[0] && m == data::daily_default_date[1] && d == data::daily_default_date[2]);
        set_daily_date(p, 2026, 2, 31); daily_date(p, y, m, d); CHECK(d == 28);   // dzień poza miesiącem - obcięty
        CHECK(daily_best(p, day) == -1);
        CHECK(record_daily(p, day, 500, false) && daily_best(p, day) == 500 && !daily_won(p, day));
        CHECK(!record_daily(p, day, 300, true) && daily_best(p, day) == 500 && daily_won(p, day));   // gorszy wynik nie zastępuje
        CHECK(record_daily(p, day, 900, false) && daily_best(p, day) == 900 && daily_won(p, day));
        for(int k = 1; k <= data::daily_history; ++k) record_daily(p, day + k, 100 * k, false);
        CHECK(daily_best(p, day) == -1 && daily_best(p, day + data::daily_history) == 100 * data::daily_history);   // najstarszy wypada
        CHECK(p.daily_runs == 3 + data::daily_history);
        profile v6; profile_reset(v6); std::memcpy(v6.magic, "PBRL006", 8); v6.best = 321; v6.investor = 5; v6.best_stake[2] = 4;
        std::memset(reinterpret_cast<char*>(&v6) + profile_v6_size, 0xEE, sizeof v6 - profile_v6_size);
        CHECK(profile_fix(v6) && std::strcmp(v6.magic, profile_magic) == 0 && v6.best == 321 && v6.investor == 5 && v6.best_stake[2] == 4);
        CHECK(v6.daily_y == 0 && v6.daily_runs == 0 && v6.daily_won == 0);
        for(int i = 0; i < daily_slots; ++i) CHECK(v6.daily_day[i] == 0 && v6.daily_score[i] == 0);
        // Szkolenia z poziomami ponad nowe maksimum: zwrot doświadczenia
        profile c; profile_reset(c); c.xp = 10;
        int ui = -1; for(int i = 0; i < data::upgrades_count; ++i) if(data::upgrades[i].refund > 0) ui = i;
        CHECK(ui >= 0);
        c.levels[ui] = uint8_t(data::upgrades[ui].levels + 1);
        CHECK(profile_fix(c) && c.levels[ui] == data::upgrades[ui].levels && c.xp == 10 + data::upgrades[ui].refund);
        CHECK(!profile_fix(c));
    }
    // 40. harmonogram domu: tury każdego etapu zapisane przy zaliczeniu, dni i daty etapów
    {
        game g; g.new_run(1, 7);
        for(int k = 0; k < 5; ++k) { g.hero.hp = g.hero.max_hp = 999; g.player_wait(); }
        g.debug_skip();
        const int f = g.first_stage;   // bez Aktu 0: od Fundamentów
        CHECK(g.st == status::stage_clear && g.stage_days[f] == g.turns);
        while(g.st == status::stage_clear)
        {
            g.next_stage();
            for(int k = 0; k < 2 + g.stage; ++k) { g.hero.hp = g.hero.max_hp = 999; g.player_wait(); }
            g.debug_skip();
        }
        CHECK(g.st == status::won);
        for(int s2 = f + 1; s2 < data::stages_count; ++s2) CHECK(g.stage_days[s2] >= 2 + s2);
        int total = 0; for(int s2 = f; s2 < data::stages_count; ++s2) { CHECK(schedule_days(g, s2) >= data::schedule_min_days); total += schedule_days(g, s2); }
        CHECK(schedule_total_days(g) == total && schedule_total_cost(g) > 0);
        int end = days_from_civil(2026, 10, 1);
        CHECK(schedule_start_day(g, f, end) == end - total && schedule_start_day(g, f + 1, end) == end - total + schedule_days(g, f));
    }
    // 41. po budowie: najbliższe do kupienia w Szkoleniach
    {
        profile p; profile_reset(p);
        int kind = -1, idx = -1, cost = next_unlock(p, kind, idx);
        CHECK(cost > 0 && kind >= 0);
        int cheapest = 1 << 20; for(int i = 0; i < data::upgrades_count; ++i) cheapest = imin(cheapest, upgrade_cost(p, i));
        CHECK(cost <= cheapest);
        p.xp = 1 << 20;
        for(int i = 0; i < data::upgrades_count; ++i) while(buy_upgrade(p, i)) {}
        for(int i = 0; i < data::classes_count; ++i) buy_class(p, i);
        for(int i = 0; i < data::tools_count; ++i) buy_tool(p, i);
        for(int i = 0; i < data::brigade_count; ++i) buy_helper(p, i);
        buy_hard(p);
        CHECK(next_unlock(p, kind, idx) == -1);
    }
    // 42. Respekt: za każdy ukończony etap (więcej za bossów), od razu w profilu, śmierć go nie zabiera; sklep z rangami
    {
        auto rx_of = [](respect_effect e) { for(int i = 0; i < data::respect_count; ++i) if(data::respect[i].effect == e) return i; return -1; };
        game g; g.new_run(1, 4242);
        profile p; profile_reset(p); start_run(p);
        CHECK(g.respect == 0 && g.stage_respect() == data::respect_stage);
        g.debug_skip();
        CHECK(g.st == status::stage_clear && g.respect == data::respect_stage);
        check_badges(p, g);
        CHECK(p.respect == data::respect_stage && p.respect_total == data::respect_stage && p.run_respect == data::respect_stage);
        check_badges(p, g); CHECK(p.respect == data::respect_stage);   // drugi raz nie dolicza
        int expect = data::respect_stage;
        while(g.st == status::stage_clear)
        {
            g.next_stage();
            int want = g.stage_respect();
            const stage_def& sd = data::stages[g.stage];
            if(g.stage == data::stages_count - 1) CHECK(want == data::respect_final);
            else if(sd.boss >= 0) CHECK(want == (data::stages[g.stage + 1].act == sd.act ? data::respect_boss : data::respect_act_boss));
            else CHECK(want == data::respect_stage);
            g.hero.hp = g.hero.max_hp = 999;
            g.debug_skip();
            expect += want;
            check_badges(p, g);
            CHECK(g.respect == expect && p.respect == expect);
        }
        CHECK(g.st == status::won && expect > 10 * data::respect_stage);
        game e; e.new_run(1, 1, 0); CHECK(e.stage_respect() == imax(1, data::respect_stage * data::difficulties[0].score_pct / 100));   // Łatwy mniej
        // śmierć: Respekt z ukończonych etapów zostaje w profilu, nowa budowa liczy od zera
        profile q; profile_reset(q); start_run(q);
        game d; d.new_run(0, 77); d.debug_skip(); check_badges(q, d); d.next_stage();
        d.hero.hp = 1; d.hero_down(); CHECK(d.st == status::dead);
        check_badges(q, d); CHECK(q.respect == data::respect_stage);
        start_run(q); CHECK(q.run_respect == 0);
        game d2; d2.new_run(0, 78); d2.debug_skip(); check_badges(q, d2); CHECK(q.respect == 2 * data::respect_stage);
        // sklep: rangi po kolei, rosnąca cena, maksimum
        profile s; profile_reset(s);
        CHECK(respect_cost(s, 0) == data::respect[0].costs[0] && ! buy_respect(s, 0));
        s.respect = 60000;
        int spent = 0;
        for(int i = 0; i < data::respect_count; ++i)
        {
            for(int r = 0; r < data::respect[i].ranks; ++r)
            {
                if(r > 0) CHECK(data::respect[i].costs[r] > data::respect[i].costs[r - 1]);
                CHECK(respect_cost(s, i) == data::respect[i].costs[r] && buy_respect(s, i));
                spent += data::respect[i].costs[r];
                CHECK(respect_value(s, i) == data::respect[i].values[r]);
            }
            CHECK(respect_cost(s, i) == -1 && ! buy_respect(s, i));
        }
        CHECK(s.respect == 60000 - spent && respect_spent(s) == respect_total_cost() && spent == respect_total_cost());
        CHECK(respect_total_cost() > 30 * (2 * data::respect_stage + data::respect_final));   // pełny Respekt = wiele budów
        run_mods m = mods(s), m0 = mods(p);
        int dmg = rx_of(respect_effect::dmg_pct), tak = rx_of(respect_effect::taken_pct), cof = rx_of(respect_effect::coffee_pct);
        int dod = rx_of(respect_effect::dodge), brg = rx_of(respect_effect::brigade_pct), shp = rx_of(respect_effect::shop_pct);
        int sec = rx_of(respect_effect::second_chance), gpc = rx_of(respect_effect::gear_pct), mat = rx_of(respect_effect::mats_pct);
        CHECK(dmg >= 0 && tak >= 0 && cof >= 0 && dod >= 0 && brg >= 0 && shp >= 0 && sec >= 0 && gpc >= 0 && mat >= 0);
        CHECK(m.dmg_pct == m0.dmg_pct + respect_value(s, dmg) && m.taken_pct == m0.taken_pct + respect_value(s, tak));
        CHECK(m.second_chance > 0 && m.gear_pct == respect_value(s, gpc) && m.mats_pct == respect_value(s, mat));
        CHECK(daily_mods(1).dmg_pct == 0 && daily_mods(1).second_chance == 0);   // budowa dnia bez Respektu
        // obrażenia +%: średnio dokładnie (reszta przenoszona), bez losowania
        {
            game a; arena(a, 1); game b; arena(b, 1); b.bonus.dmg_pct = 25;
            long sa = 0, sb = 0;
            for(int k = 0; k < 400; ++k)
            {
                a.r.seed(900 + k); b.r.seed(900 + k);
                a.spawn(data::enemy_budzet, 8, 7); b.spawn(data::enemy_budzet, 8, 7);
                a.enemies[a.enemies_count - 1].hp = b.enemies[b.enemies_count - 1].hp = 999;
                a.hero_attack(a.enemies_count - 1); b.hero_attack(b.enemies_count - 1);
                sa += 999 - a.enemies[a.enemies_count - 1].hp; sb += 999 - b.enemies[b.enemies_count - 1].hp;
                a.enemies_count = b.enemies_count = 0;
            }
            CHECK(sb * 100 >= sa * 122 && sb * 100 <= sa * 128);
        }
        {   // otrzymane obrażenia -%: mniej, ale zawsze co najmniej 1
            game a; a.new_run(1, 5); game b; b.new_run(1, 5); b.bonus.taken_pct = 30;
            long ta = 0, tb = 0;
            for(int k = 0; k < 300; ++k) { int d0 = 1 + k % 7; ta += a.taken_damage(d0); tb += b.taken_damage(d0); CHECK(b.taken_damage(1) >= 1); }
            CHECK(tb * 100 >= ta * 66 && tb * 100 <= ta * 80);   // 1 HP nie da się zmniejszyć
        }
        {   // kawa +%, unik z limitem, Druga szansa raz na budowę
            game a; a.new_run(1, 5); a.bonus.coffee_pct = 50;
            CHECK(a.coffee_heal() == div_round(data::coffee_heal * 150, 100));
            game u; u.new_run(5, 5); u.bonus.dodge = 90; CHECK(u.dodge_pct() == data::dodge_max_pct);
            game z; z.new_run(1, 5); z.bonus.second_chance = 1;
            z.hero.hp = 0; z.hero_down(); CHECK(z.st == status::playing && z.hero.hp == 1 && z.hero.alive && z.second_used);
            z.hero.hp = 0; z.hero_down(); CHECK(z.st == status::dead && ! z.hero.alive);
            game n; n.new_run(1, 5); n.hero.hp = 0; n.hero_down(); CHECK(n.st == status::dead);
        }
        {   // brygada i Hurtownia taniej
            game a; a.new_run(1, 5); a.bonus.brigade_pct = 30; a.bonus.shop_pct = 25;
            CHECK(a.helper_price(0) == data::brigade[0].price * 70 / 100);
            for(int i = 0; i < data::hurtownia_count; ++i) CHECK(a.hurtownia_price(i) == data::hurtownia[i].price * 75 / 100);
            a.cash = a.hurtownia_price(0); CHECK(a.hurtownia_can(0) && a.hurtownia_buy(0) && a.cash == 0);
        }
    }
    // 43. nagrody za odbiór: każda wygrana odblokowuje kolejną (narzędzia, sprzęt, zawody); profil v7 -> v8
    {
        profile p; profile_reset(p);
        int avail = rewards_available();
        CHECK(avail > 0 && avail <= data::rewards_count && p.rewards == 0);
        for(int i = 0; i < data::classes_count; ++i) if(class_reward(i)) CHECK(! class_unlocked(p, i) && ! buy_class(p, i));
        for(int i = 0; i < data::tools_count; ++i) if(data::tools[i].reward) CHECK(! tool_unlocked(p, i));
        CHECK(gear_slots_mask(p) == data::gear_base_mask && mods(p).gear_slots == data::gear_base_mask);
        CHECK(reward_win(p, 0) == 1 && reward_win(p, 2) == 3);
        p.xp = 1 << 20;
        for(int i = 0; i < data::tools_count; ++i) if(data::tools[i].reward) CHECK(! buy_tool(p, i));
        for(int k = 0; k < avail; ++k)
        {
            CHECK(record_win(p) == k && p.wins == k + 1 && reward_owned(p, k) && reward_win(p, k) == -1);
            const reward_def& rd = data::rewards[k];
            if(rd.kind == reward_kind::cls) CHECK(class_unlocked(p, rd.index));
            if(rd.kind == reward_kind::tool) CHECK(tool_unlocked(p, rd.index) && (mods(p).tools >> rd.index) & 1);
            if(rd.kind == reward_kind::gear) CHECK((gear_slots_mask(p) >> rd.index) & 1);
        }
        CHECK(record_win(p) == -1 && p.rewards == avail);   // "wkrótce" się nie odblokowuje
        for(int i = 0; i < data::classes_count; ++i) CHECK(class_unlocked(p, i) == (class_reward(i) || (p.classes >> i) & 1));
        // wygrane i stawki zawodów 8+
        profile w; profile_reset(w);
        for(int c = 0; c < data::classes_count; ++c) { CHECK(! class_won(w, c)); set_class_won(w, c); CHECK(class_won(w, c)); set_best_stake(w, c, c + 1); }
        CHECK(classes_won(w) == data::classes_count);
        for(int c = 0; c < data::classes_count; ++c) CHECK(best_stake(w, c) == c + 1);
        game wg; wg.new_run(data::classes_count - 1, 3); wg.st = status::won;
        profile w2; profile_reset(w2); record_run(w2, wg); CHECK(class_won(w2, data::classes_count - 1) && w2.class_wins == 0);
        // v7 -> v8: stare pola zostają, nagrody za dotychczasowe wygrane, zmienione Szkolenia wracają jako doświadczenie
        profile v7; profile_reset(v7); std::memcpy(v7.magic, "PBRL007", 8);
        v7.wins = 3; v7.xp = 11; v7.best = 777; v7.daily_score[4] = 55;
        int refund = 0;
        for(int i = 0; i < data::upgrades_count; ++i)
            if(data::upgrades[i].reset_refund > 0) { v7.levels[i] = 1; refund += data::upgrades[i].reset_refund; }
        CHECK(refund > 0);
        std::memset(reinterpret_cast<char*>(&v7) + profile_v7_size, 0xEE, sizeof v7 - profile_v7_size);
        CHECK(profile_fix(v7) && std::strcmp(v7.magic, profile_magic) == 0);
        CHECK(v7.best == 777 && v7.wins == 3 && v7.daily_score[4] == 55 && v7.rewards == imin(3, avail) && v7.xp == 11 + refund);
        CHECK(v7.respect == 0 && v7.respect_total == 0 && v7.class_wins_hi == 0 && v7.respect_ranks[0] == 0 && v7.best_stake_hi[0] == 0);
        for(int i = 0; i < data::upgrades_count; ++i) if(data::upgrades[i].reset_refund > 0) CHECK(v7.levels[i] == 0);
        CHECK(! profile_fix(v7));
        profile v1; std::memset(&v1, 0, sizeof v1); std::memcpy(v1.magic, "PBRL001", 8); v1.wins = 99;
        CHECK(profile_fix(v1) && v1.wins == 99 && v1.rewards == avail);
    }
    // 44. nowe zawody: Rynna (linia), Narzut (obszar), Taran (szarża, odepchnięcie), cechy: wiatr, odepchnięcie
    {
        auto class_of = [](ability_effect e) { for(int c = 0; c < data::classes_count; ++c) if(data::classes[c].ability == e) return c; return -1; };
        int roofer = class_of(ability_effect::line), plaster = class_of(ability_effect::splash), digger = class_of(ability_effect::ram);
        CHECK(roofer >= 0 && plaster >= 0 && digger >= 0);
        CHECK(data::classes[roofer].passive == class_passive::windproof && data::classes[digger].passive == class_passive::push);
        {   // Rynna: wszyscy na linii (także za celem), mur zatrzymuje
            game g; arena(g, roofer);
            g.spawn(data::enemy_budzet, 9, 7); g.spawn(data::enemy_budzet, 11, 7); g.spawn(data::enemy_budzet, 7, 10);
            for(int i = 0; i < 3; ++i) g.enemies[i].hp = 99;
            g.update_fov();
            CHECK(g.player_ability());
            CHECK(g.enemies[0].hp < 99 && g.enemies[1].hp < 99 && g.enemies[2].hp == 99 && g.ability_cd > 0);
            game w; arena(w, roofer); w.lv.t[7][10] = tile::wall;
            w.spawn(data::enemy_budzet, 9, 7); w.spawn(data::enemy_budzet, 11, 7); w.enemies[0].hp = w.enemies[1].hp = 99; w.update_fov();
            CHECK(w.player_ability() && w.enemies[0].hp < 99 && w.enemies[1].hp == 99);
            game n; arena(n, roofer); CHECK(! n.player_ability() && n.turns == 0);   // bez celu nic
            // wiatr nie skraca zasięgu Dekarza
            game wd; arena(wd, roofer); int rg = wd.weapon_range();
            for(int i = 0; i < data::weather_count; ++i) if(data::weather[i].effect == weather_effect::wind) wd.weather = int8_t(i);
            CHECK(wd.weapon_range() == rg && rg > 1);
        }
        {   // Narzut: cel w zasięgu i sąsiedzi celu, dalszy problem bez zmian; od rangi II ogłusza
            game g; arena(g, plaster);
            g.spawn(data::enemy_budzet, 9, 7); g.spawn(data::enemy_budzet, 10, 8); g.spawn(data::enemy_budzet, 12, 7);
            for(int i = 0; i < 3; ++i) g.enemies[i].hp = 99;
            g.update_fov();
            CHECK(g.player_ability() && g.enemies[0].hp < 99 && g.enemies[1].hp < 99 && g.enemies[2].hp == 99);
            game r2; arena(r2, plaster); r2.hero_level = 3; r2.spawn(data::enemy_budzet, 8, 7); r2.enemies[0].hp = 99; r2.update_fov();
            CHECK(r2.player_ability() && r2.enemies[0].stun >= 0 && r2.enemies[0].hp < 99);
        }
        {   // Taran: szarża do problemu, cios i odepchnięcie o 2
            game g; arena(g, digger);
            g.spawn(data::enemy_budzet, 10, 7); g.enemies[0].hp = 99; g.update_fov();
            CHECK(g.player_ability());
            CHECK(g.hero.x == 9 && g.hero.y == 7 && g.enemies[0].hp < 99 && g.enemies[0].x == 12 && g.enemies[0].stun >= 0);
            game b; arena(b, digger); b.spawn(data::enemy_budzet, 7, 3); b.enemies[0].hp = 99; b.update_fov();
            CHECK(b.player_ability() && b.hero.x == 7 && b.hero.y == 4 && b.enemies[0].hp == 99);   // za daleko: tylko szarża
            game c; arena(c, digger); c.spawn(data::enemy_budzet, 7, 4); c.enemies[0].hp = 99; c.update_fov();
            CHECK(c.player_ability() && c.hero.x == 7 && c.hero.y == 5 && c.enemies[0].y == 2 && c.enemies[0].hp < 99);   // pionowo
        }
        {   // Operator: cios wręcz czasem odpycha (nie bossa)
            int pushed = 0;
            for(uint32_t seed = 1; seed <= 200; ++seed)
            {
                game g; arena(g, digger); g.r.seed(seed);
                g.spawn(data::enemy_budzet, 8, 7); g.enemies[0].hp = 99;
                g.hero_attack(0);
                pushed += g.enemies[0].x == 9;
                CHECK(g.enemies[0].x == 8 || g.enemies[0].x == 9);
            }
            CHECK(pushed > 200 * data::push_chance_pct / 200 && pushed < 200 * data::push_chance_pct * 2 / 100);
            game o; arena(o, 1); o.spawn(data::enemy_budzet, 8, 7); o.enemies[0].hp = 99;
            for(int k = 0; k < 30; ++k) o.hero_attack(0);
            CHECK(o.enemies[0].x == 8);   // inne zawody nie pchają
        }
    }
    // 45. sprzęt z nagród: buty (unik), pas (termos), cecha Bez poślizgu; pełny sprzęt BHP = sloty bazowe
    {
        int boots = -1, belt = -1, slip = -1;
        for(int i = 0; i < data::gear_slots_count; ++i)
        {
            if(data::gear[i * 3].stat == gear_stat::dodge) boots = i;
            if(data::gear[i * 3].stat == gear_stat::thermos) belt = i;
        }
        for(int i = 0; i < data::gear_traits_count; ++i) if(data::gear_traits[i].effect == trait_effect::slip_res) slip = i;
        CHECK(boots >= 0 && belt >= 0 && slip >= 0 && (data::gear_reward_mask >> boots) & 1 && (data::gear_reward_mask >> belt) & 1);
        game g; arena(g, 1);
        int d0 = g.dodge_pct(), cap0 = g.thermos_cap(), plain = 0;
        for(int i = 0; i < data::gear_traits_count; ++i) if(data::gear_traits[i].effect == trait_effect::sight) plain = i;   // cecha bez wpływu na unik
        g.equip(boots, 2, plain); CHECK(g.dodge_pct() == imin(data::dodge_max_pct, d0 + data::gear[boots * 3 + 2].value));
        g.equip(belt, 2, plain); CHECK(g.thermos_cap() == cap0 + data::gear[belt * 3 + 2].value);
        g.thermos = g.thermos_cap(); g.equip(belt, 0, plain); CHECK(g.thermos == g.thermos_cap());   // słabszy pas: kawy ponad limit przepadają
        g.equip(0, 0, slip); g.apply_status(status_effect::slip, 3); CHECK(g.status_turns(status_effect::slip) == 0);
        game f; arena(f, 1);
        for(int i = 0; i < data::gear_slots_count; ++i) if((data::gear_base_mask >> i) & 1) f.equip(i, 0, 0);
        CHECK(f.full_gear());
        // dropy: bez nagród tylko sloty bazowe, z nagrodami też buty i pas
        int seen0 = 0, seen1 = 0;
        for(uint32_t seed = 1; seed <= 300; ++seed)
        {
            game a; arena(a, 1); a.r.seed(seed); seen0 |= 1 << a.random_slot();
            game b; arena(b, 1); b.bonus.gear_slots = (1 << data::gear_slots_count) - 1; b.r.seed(seed); seen1 |= 1 << b.random_slot();
            game c; arena(c, 1); c.r.seed(seed); rng r2; r2.seed(seed);
            CHECK(c.random_slot() == r2.range(0, 2));   // 3 sloty: to samo losowanie co dawniej
        }
        CHECK(seen0 == data::gear_base_mask && seen1 == (1 << data::gear_slots_count) - 1);
    }
    // 40. v0.21.49 cz. 2: zachowania problemów (każdy znacznik), mechaniki aktów, 10 etapów, profil v9
    {
        auto def_with = [](int tag) { for(int d = 0; d < data::enemies_count; ++d) if(data::enemies[d].tags & tag) return d; return -1; };
        auto put = [](game& g, int def, int x, int y) { g.spawn(def, x, y); actor& e = g.enemies[g.enemies_count - 1]; e.awake = true; return g.enemies_count - 1; };
        CHECK(data::stages_count == 12 && data::enemies_count <= max_enemy_types);
        for(int s = 0; s < data::stages_count; ++s)   // każdy etap ma problemy z zachowaniami
        {
            int tagged = 0;
            for(int k = 0; k < data::stages[s].pool_count; ++k) tagged += data::enemies[data::stages[s].pool[k]].tags != 0;
            CHECK(tagged >= 2);
        }
        for(int a = 0; a < data::acts_count; ++a) CHECK(data::acts[a].mechanic != act_mechanic::none);
        for(int t = 1; t <= tag_returns; t <<= 1) CHECK(def_with(t) >= 0);
        {   // ranged: strzał w linii z 3 pól, bez ruchu; mur po drodze blokuje
            game g; arena(g, 1); g.stage = F0 + 4; g.stage_start_turn = -100;   // akt II (bez błota), porywy nie w tej turze
            int d = def_with(tag_ranged); int i = put(g, d, 10, 7);
            int hp = g.hero.hp; g.player_wait();
            CHECK(g.hero.hp < hp && g.enemies[i].x == 10 && (g.shot_events & (1u << i)));
            game h; arena(h, 1); h.stage = F0 + 4; int j = put(h, d, 10, 7); h.lv.t[7][9] = tile::wall; h.lv.t[6][9] = tile::wall; h.lv.t[8][9] = tile::wall;
            hp = h.hero.hp; h.player_wait();
            CHECK(h.hero.hp == hp && !(h.shot_events & (1u << j)));
        }
        {   // splits: dwa dzieci z połową max HP, dzieci się nie dzielą
            game g; arena(g, 1); int d = def_with(tag_splits); int i = put(g, d, 8, 7);
            int mx = g.enemies[i].max_hp; g.enemies[i].hp = 1; g.hero_attack(i);
            int kids = 0, kid = -1;
            for(int k = 0; k < g.enemies_count; ++k) if(g.enemies[k].alive && (g.enemies[k].flags & actor_child)) { ++kids; kid = k; CHECK(g.enemies[k].max_hp == imax(1, mx * data::behavior_split_hp_pct / 100)); }
            CHECK(kids == 2 && g.kills == 1);
            int n = g.enemies_count; g.enemies[kid].hp = 1; g.hero_attack(kid);
            CHECK(g.enemies_count == n && g.kills == 2);
        }
        {   // heals: łata rannego sąsiada (nie obok bohatera), potem odnowienie
            game g; arena(g, 1); int d = def_with(tag_heals); put(g, d, 11, 7); int j = put(g, data::enemy_plesn, 11, 9);
            g.enemies[j].stun = 5; g.enemies[j].hp = int16_t(g.enemies[j].max_hp - 5);
            g.player_wait();
            CHECK(g.enemies[j].hp == g.enemies[j].max_hp - 5 + data::behavior_heal_value);
        }
        {   // explodes: czerwone pola, tura na zejście; zostanie = obrażenia, zejście = bez
            game g; arena(g, 1); int d = def_with(tag_explodes); int i = put(g, d, 8, 7);
            g.enemies[i].hp = 1; g.hero_attack(i); g.end_turn();
            CHECK(g.blast_timer == 1 && g.danger_cell(7, 7) && g.danger_cell(9, 8) && !g.danger_cell(10, 7));
            int hp = g.hero.hp; g.player_wait();
            CHECK(g.hero.hp < hp && g.blast_timer == 0);
            game h; arena(h, 1); int k = put(h, d, 8, 7); h.enemies[k].hp = 1; h.hero_attack(k); h.end_turn();
            h.hero.x = 5; hp = h.hero.hp; h.player_wait();
            CHECK(h.hero.hp == hp);
        }
        {   // grows + stationary: rośnie co kilka tur, stoi w miejscu
            game g; arena(g, 1); int d = -1;
            for(int k = 0; k < data::enemies_count; ++k) if((data::enemies[k].tags & tag_grows) && (data::enemies[k].tags & tag_stationary)) d = k;
            CHECK(d >= 0);
            int i = put(g, d, 11, 11); int mx = g.enemies[i].max_hp;
            for(int t = 0; t < data::behavior_grow_every * 2; ++t) g.player_wait();
            CHECK(g.enemies[i].grow == 2 && g.enemies[i].max_hp == mx + 2 * data::behavior_grow_hp && g.enemies[i].x == 11 && g.enemies[i].y == 11);
            for(int t = 0; t < data::behavior_grow_every * 10; ++t) g.player_wait();
            CHECK(g.enemies[i].grow == data::behavior_grow_max);
        }
        {   // flees: obok bohatera odskakuje, potem odnowienie
            game g; arena(g, 1); int d = def_with(tag_flees); int i = put(g, d, 8, 7);
            g.player_wait();
            CHECK(cheb(g.enemies[i].x, g.enemies[i].y, 7, 7) == 2 && g.enemies[i].timer == data::behavior_flee_cooldown);
        }
        {   // pushes: cios odpycha o pole, potem odnowienie
            game g; arena(g, 1); int d = -1;
            for(int k = 0; k < data::enemies_count; ++k) if(data::enemies[k].tags == tag_pushes) d = k;
            int i = put(g, d, 8, 7);
            g.player_wait();
            CHECK(g.hero.x == 6 && g.hero.y == 7 && g.enemies[i].timer == data::behavior_push_cooldown);
        }
        {   // returns: pierwsze usunięcie - wraca po kilku turach z połową HP; drugie na zawsze
            game g; arena(g, 1); int d = -1;
            for(int k = 0; k < data::enemies_count; ++k) if(data::enemies[k].tags == tag_returns) d = k;
            int i = put(g, d, 11, 11); g.enemies[i].stun = 99;
            g.enemies[i].hp = 1; g.hero_attack(i);
            CHECK(!g.enemies[i].alive && (g.enemies[i].flags & actor_reviving) && g.kills == 0);
            for(int t = 0; t < data::behavior_return_turns; ++t) g.player_wait();
            CHECK(g.enemies[i].alive && g.enemies[i].hp == imax(1, g.enemies[i].max_hp * data::behavior_return_hp_pct / 100));
            g.enemies[i].hp = 1; g.hero_attack(i);
            for(int t = 0; t < data::behavior_return_turns * 2; ++t) g.player_wait();
            CHECK(!g.enemies[i].alive && g.kills == 1);
        }
        {   // akt I: błoto kosztuje turę; Kładka je wyłącza
            game g; arena(g, 1);
            int mx = -1, my = -1;
            for(int y = 2; y <= 13 && mx < 0; ++y) for(int x = 2; x <= 13; ++x) if(g.mud(x, y) && !g.mud(x - 1, y)) { mx = x; my = y; break; }
            CHECK(mx >= 0);
            g.hero.x = int8_t(mx - 1); g.hero.y = int8_t(my); int t0 = g.turns;
            CHECK(g.player_move(1, 0) && g.turns == t0 + 2);
            g.bridges = 1; g.bridge_x[0] = int8_t(mx); g.bridge_y[0] = int8_t(my); CHECK(!g.mud(mx, my));
        }
        {   // akt II: poryw co kilka tur spycha o pole (zapowiedź turę wcześniej)
            game g; arena(g, 1); g.stage = F0 + 4; g.stage_start_turn = g.turns;
            CHECK(g.act_is(act_mechanic::gust));
            int v = data::acts[1].mech_value;
            for(int t = 0; t < v - 1; ++t) g.player_wait();
            CHECK(g.gust_in() == 1);
            int dir = g.gust_dir(), hx = g.hero.x, hy = g.hero.y;
            g.player_wait();
            CHECK(g.hero.x == hx + game::gust_vec[dir][0] && g.hero.y == hy + game::gust_vec[dir][1]);
        }
        {   // akt III: pył - mniejsze pole widzenia
            game g; arena(g, 1); int r0 = g.sight_radius(); g.stage = F0 + 8;
            CHECK(g.act_is(act_mechanic::dust) && g.sight_radius() == r0 - data::acts[2].mech_value);
        }
        {   // profil v8 -> v9: katalog 16-47 od zera, reszta bez zmian; zapis budowy PBRUN09
            profile p; profile_reset(p); p.best = 4321; p.respect = 77; p.catalog = 0x0F0F; p.catalog_hi = 0xDEADBEEF;
            std::memcpy(p.magic, "PBRL008", 8);
            CHECK(profile_fix(p) && std::strcmp(p.magic, profile_magic) == 0 && p.catalog_hi == 0 && p.best == 4321 && p.respect == 77 && p.catalog == 0x0F0F);
            catalog_add(p, 20); catalog_add(p, 31);
            CHECK(catalog_has(p, 20) && catalog_has(p, 31) && !catalog_has(p, 21) && catalog_count(p) == 8 + 2);
            CHECK(std::strcmp(run_magic, "PBRUN10") == 0);
        }
    }
    // 41. v0.21.49 cz. 3: Akt 0 (Papierologia) - nagroda za odbiór, pieczątki zamykają schody, druga faza bossa;
    //     samouczek menu (flagi w profilu, dymki odblokowań), profil v10
    {
        profile p; profile_reset(p);
        CHECK(!act0_unlocked(p) && mods(p).act0 == 0);
        int ai = -1; for(int i = 0; i < data::rewards_count; ++i) if(data::rewards[i].kind == reward_kind::act) ai = i;
        CHECK(ai == data::rewards_count - 1 && rewards_available() == data::rewards_count);
        for(int k = 0; k <= ai; ++k) CHECK(record_win(p) == k);
        CHECK(act0_unlocked(p) && mods(p).act0 == 1);
        game n; n.new_run(1, 11);
        CHECK(n.first_stage == F0 && n.stage == F0 && n.stage_number() == 1 && n.stages_in_run() == data::stages_count - F0);
        game dly; start_daily(dly, 42); CHECK(dly.first_stage == F0);   // budowa dnia bez Aktu 0
        game g; g.new_run(1, 11, data::default_difficulty, mods(p));
        CHECK(g.first_stage == 0 && g.stage == 0 && g.stage_number() == 1 && g.stages_in_run() == data::stages_count);
        CHECK(data::acts[data::stages[0].act].prelude && g.act_is(act_mechanic::stamps) && std::strcmp(g.act_numeral(), "0") == 0);
        CHECK(std::strcmp(data::acts[data::stages[F0].act].numeral, "I") == 0);
        // pieczątki: dokumenty na etapie, schody zamknięte do kompletu
        int docs = 0; for(int i = 0; i < g.pickups_count; ++i) if(g.pickups[i].type == document && g.pickups[i].active) ++docs;
        CHECK(g.docs_needed() == data::documents_count && docs == data::documents_count && g.stairs_locked() && g.stairs_x >= 0);
        g.enemies_count = 0;
        g.hero.x = int8_t(g.stairs_x); g.hero.y = int8_t(g.stairs_y); g.player_wait();
        CHECK(g.st == status::playing && g.stairs_locked());
        for(int i = 0; i < g.pickups_count; ++i)
            if(g.pickups[i].type == document && g.pickups[i].active) { g.hero.x = g.pickups[i].x; g.hero.y = g.pickups[i].y; g.collect(); }
        CHECK(!g.stairs_locked() && g.docs_count() == data::documents_count);
        g.hero.x = int8_t(g.stairs_x); g.hero.y = int8_t(g.stairs_y); g.player_wait();
        CHECK(g.st == status::stage_clear);
        // etap z bossem Aktu 0: bez dokumentów i schodów; druga faza (Odwołanie) raz, przy phase_pct% HP
        g.next_stage();
        CHECK(g.stage == 1 && g.docs_needed() == 0 && g.stairs_x < 0 && g.boss >= 0);
        const enemy_def& bd = data::enemies[g.enemies[g.boss].def_id];
        CHECK(bd.phase_pct > 0 && bd.phase_heal > 0 && bd.phase_summon > 0 && bd.summon >= 0 && bd.slam);
        {
            actor& b = g.enemies[g.boss]; b.awake = true;
            const int mx = b.max_hp, at = mx * bd.phase_pct / 100;
            b.hp = int16_t(at + 2);
            int alive0 = 0; for(int i = 0; i < g.enemies_count; ++i) alive0 += g.enemies[i].alive;
            g.damage_enemy(g.boss, 2, false, "test");
            int alive1 = 0; for(int i = 0; i < g.enemies_count; ++i) alive1 += g.enemies[i].alive;
            CHECK((b.flags & actor_phase) && b.hp == imin(mx, at + mx * bd.phase_heal / 100));
            CHECK(g.summons_used == bd.phase_summon && alive1 == alive0 + bd.phase_summon);
            b.hp = 3; g.damage_enemy(g.boss, 1, false, "test"); CHECK(b.hp == 2 && g.summons_used == bd.phase_summon);   // tylko raz
        }
        {   // cios, który by usunął bossa przed drugą fazą: zostaje z 1 HP + leczenie
            game h; h.new_run(1, 12, data::default_difficulty, mods(p)); h.start_stage(1);
            actor& b = h.enemies[h.boss]; b.awake = true;
            h.damage_enemy(h.boss, 999, false, "test");
            CHECK(b.alive && (b.flags & actor_phase) && b.hp == imin(b.max_hp, 1 + b.max_hp * bd.phase_heal / 100));
            h.debug_skip();   // skrót pokazowy: boss Aktu 0 = boss aktu (premia, Hurtownia, Respekt za akt)
            CHECK(h.st == status::stage_clear && h.act_cleared && h.respect == data::respect_act_boss * h.score_pct() / 100);
            h.next_stage(); CHECK(h.stage == F0 && std::strcmp(h.act_numeral(), "I") == 0);
        }
        {   // bot przechodzi Akt 0 (zbiera dokumenty)
            int won_act0 = 0;
            for(int k = 0; k < 20; ++k)
            {
                game b; b.new_run(k % data::classes_count, 300 + k * 13, 0, mods(p));
                for(int step = 0; step < 3000 && b.st == status::playing && b.stage < F0; ++step) bot_step(b);
                while(b.st == status::stage_clear && b.stage < F0) { b.next_stage(); for(int step = 0; step < 3000 && b.st == status::playing; ++step) bot_step(b); }
                won_act0 += b.stage >= F0 || (b.st == status::stage_clear && b.stage == F0 - 1);
            }
            CHECK(won_act0 >= 14);
        }
        // samouczek: główny na tytule i wyborze zawodu, potem dymki odblokowań (każdy raz)
        profile t; profile_reset(t); int cls = -1;
        CHECK(tutorial_pending(t, 0) && tutorial_pending(t, 1) && pending_unlock(t, 0, cls) == -1);
        int gba = 0, godot = 0; for(int i = 0; i < data::tutorial_steps_count; ++i) { gba += tutorial_step_shown(t, i, false); godot += tutorial_step_shown(t, i, true); }
        CHECK(gba > 5 && godot == gba + 1);   // klucz z opcjami tylko w Godocie; tryb inwestora jeszcze ukryty
        tutorial_done(t, 0); tutorial_done(t, 1);
        CHECK(!tutorial_pending(t, 0) && !tutorial_pending(t, 1) && pending_unlock(t, 0, cls) == -1 && pending_unlock(t, 1, cls) == -1);
        t.respect_total = 2; CHECK(pending_unlock(t, 0, cls) == unlock_respect); mark_unlock(t, unlock_respect, -1);
        t.runs = 1; CHECK(pending_unlock(t, 0, cls) == unlock_daily); mark_unlock(t, unlock_daily, -1);
        CHECK(pending_unlock(t, 0, cls) == -1);
        record_win(t); CHECK(pending_unlock(t, 1, cls) == unlock_investor); mark_unlock(t, unlock_investor, -1);
        int gba2 = 0; for(int i = 0; i < data::tutorial_steps_count; ++i) gba2 += tutorial_step_shown(t, i, false);
        CHECK(gba2 == gba + 1);   // krok o trybie inwestora po odblokowaniu
        while(pending_unlock(t, 1, cls) == -1 && t.rewards < data::rewards_count) record_win(t);
        CHECK(pending_unlock(t, 1, cls) == unlock_class && class_reward(cls) && class_unlocked(t, cls));
        mark_unlock(t, unlock_class, cls); CHECK(pending_unlock(t, 1, cls) != unlock_class || cls >= 0);
        while(t.rewards < data::rewards_count) record_win(t);
        for(int k = 0; k < data::classes_count; ++k) { int c2; if(pending_unlock(t, 1, c2) == unlock_class) mark_unlock(t, unlock_class, c2); }
        CHECK(pending_unlock(t, 1, cls) == -1 && pending_unlock(t, 0, cls) == unlock_act0);
        mark_unlock(t, unlock_act0, -1); CHECK(pending_unlock(t, 0, cls) == -1);
        tutorial_reset(t); CHECK(tutorial_pending(t, 0) && tutorial_pending(t, 1) && (t.tutorial & tut_act0));   // powtórka z Jak grać
        // profil v9 -> v10: kto grał, nie ogląda głównego samouczka; Akt 0 za dotychczasowe wygrane (z dymkiem)
        profile v; profile_reset(v); std::memcpy(v.magic, "PBRL009", 8);
        v.runs = 12; v.wins = 9; v.rewards = uint8_t(ai); v.respect_total = 40; v.best = 999; v.tutorial = 0xABCD; v.classes_seen = 0x1234;
        CHECK(profile_fix(v) && std::strcmp(v.magic, profile_magic) == 0 && v.best == 999 && v.rewards == data::rewards_count && act0_unlocked(v));
        CHECK(!tutorial_pending(v, 0) && !tutorial_pending(v, 1) && pending_unlock(v, 0, cls) == unlock_act0 && pending_unlock(v, 1, cls) == -1);
        profile nv; profile_reset(nv); std::memcpy(nv.magic, "PBRL009", 8);
        CHECK(profile_fix(nv) && tutorial_pending(nv, 0) && nv.rewards == 0 && nv.tutorial == 0);
        CHECK(sizeof(profile) == 160 && std::strcmp(profile_magic, "PBRL010") == 0);
    }
    // 46. v0.21.50: rozpiska obrażeń broni (#26) - zakres z rozpiski = to, co naprawdę zadaje walka (wiele rzutów z seedem)
    {
        // źródła premii profilu: suma części = mods()
        profile p; profile_reset(p);
        for(int i = 0; i < data::upgrades_count; ++i) p.levels[i] = uint8_t(data::upgrades[i].levels);
        for(int i = 0; i < data::respect_count; ++i) p.respect_ranks[i] = uint8_t(data::respect[i].ranks);
        p.badges = 0xFFFFFFFFu & ((1u << data::badges_count) - 1);
        for(int k = 0; k < data::keepsakes_count; ++k) p.keepsake_runs[k] = 9;
        p.keepsake = 3;   // Szczęśliwa kielnia (albo co wybierze select)
        const run_mods full = mods(p);
        run_mods parts[mods_sources]; mods_parts(p, parts);
        int sd = 0, sp = 0, sc = 0, sl = 0, sk = 0, sf = 0, st = 0, sh = 0;
        for(auto& q : parts) { sd += q.dmg; sp += q.dmg_pct; sc += q.crit; sl += q.luck; sk += q.craft; sf += q.def; st += q.taken_pct; sh += q.hp; }
        CHECK(sd == full.dmg && sp == full.dmg_pct && sc == full.crit && sl == full.luck && sk == full.craft && sf == full.def
              && st == full.taken_pct && sh == full.hp);
        CHECK(full.dmg_pct > 0 && full.crit > 0 && parts[1].dmg_pct > 0 && parts[2].crit > 0);   // Respekt i odznaki coś dają
        // wybór zawodu: rozpiska zawodu = rozpiska na starcie budowy
        for(int c = 0; c < data::classes_count; ++c)
        {
            dmg_breakdown a = class_breakdown(c, full);
            game g; g.new_run(c, 5, data::default_difficulty, full);
            dmg_breakdown b = g.weapon_breakdown();
            CHECK(a.min == b.min && a.max == b.max && a.crit_pct == b.crit_pct && a.avg10 == b.avg10 && a.stat_value == g.hero_stat(g.weapon().scales_with));
            CHECK(b.crit_pct == g.crit_pct() && b.range == g.weapon_range());
            b.set_sources(parts); CHECK(b.split);
            game d; d.new_run(c, 5, data::default_difficulty, daily_mods(3));   // budowa dnia: inne premie, bez podziału
            dmg_breakdown bd = d.weapon_breakdown(); bd.set_sources(parts); CHECK(! bd.split || (full.dmg == 0 && daily_mods(3).dmg_pct == full.dmg_pct));
        }
        // walka: min/max ciosu i kryt w zakresie rozpiski, skraje osiągane, częstość kryt ~ szansa
        int configs = 0, reached = 0;
        for(int c = 0; c < data::classes_count; ++c)
            for(int v = 0; v < 6; ++v)
            {
                game g; arena(g, c);
                rng pick; pick.seed(uint32_t(1000 + c * 17 + v));
                g.bonus.dmg_pct = v == 0 ? 0 : pick.range(0, 30);
                g.bonus.crit = pick.range(0, 10);
                g.bonus.dmg = pick.range(0, 2); g.dmg_bonus = g.bonus.dmg;
                for(int l = 0; l < v; ++l) g.gain_xp(20);                          // awanse
                g.dmg_bonus += pick.range(0, 2);                                   // projekty wykonawcze
                for(int s = 0; s < data::gear_slots_count; ++s)
                    if(pick.range(0, 2)) g.equip(s, pick.range(0, 2), pick.range(0, data::gear_traits_count - 1));
                if(v >= 3) g.weapon_override = data::tools[pick.range(0, data::tools_count - 1)].weapon;
                int ed = pick.range(0, data::enemies_count - 1);
                dmg_breakdown b = g.weapon_breakdown(ed);
                CHECK(b.crit_pct == g.crit_pct() && b.stat_value == g.hero_stat(g.weapon().scales_with));
                CHECK(b.flat == g.dmg_bonus + g.gear_bonus(gear_stat::dmg) && b.flat_found >= 0);
                int lo = 999, hi = 0, clo = 999, chi = 0, crits = 0;
                const int n = 3000;
                for(int k = 0; k < n; ++k)
                {
                    g.dmg_carry = pick.range(0, 99);   // reszta z poprzednich ciosów
                    g.hits_count = 0;
                    g.spawn(ed, 8, 7);
                    actor& e = g.enemies[g.enemies_count - 1];
                    e.hp = e.max_hp = 30000;
                    g.hero_attack(g.enemies_count - 1);
                    int dealt = 30000 - g.enemies[g.enemies_count - 1].hp;
                    bool crit = g.hits_count > 0 && g.hits[0].kind == hit_crit;
                    if(crit) { ++crits; clo = imin(clo, dealt); chi = imax(chi, dealt); }
                    else { lo = imin(lo, dealt); hi = imax(hi, dealt); }
                    g.enemies_count = 0;
                }
                CHECK(lo >= b.min && hi <= b.max);
                if(crits) CHECK(clo >= b.crit_min && chi <= b.crit_max);
                if(b.crit_chance() < 100) CHECK(lo == b.min && hi == b.max);   // skraje osiągalne (reszta procentu 0 i 99)
                CHECK(iabs(crits * 100 - b.crit_chance() * n) <= 4 * n);        // +-4 pkt proc.
                ++configs; reached += crits > 0 && clo == b.crit_min && chi == b.crit_max;
                // teksty rozpiski mieszczą się w buforze (bez uciętych znaków)
                b.set_sources(parts);
                for(int t = 0; t < dmg_texts; ++t) { message m; dmg_line(m, b, dmg_text(t)); CHECK(m.n > 0 && m.n < log_len - 1); }
                // cios problemu: w zakresie enemy_hit (bez uniku)
                g.spawn(ed, 8, 7);
                int ei = g.enemies_count - 1;
                g.bonus.taken_pct = pick.range(0, 20);
                hit_range h = g.enemy_hit(ei);
                int tlo = 999, thi = 0;
                for(int k = 0; k < 600; ++k)
                {
                    g.taken_carry = pick.range(0, 99);
                    g.hits_count = 0; g.hero.hp = 30000; g.hero.alive = true; g.st = status::playing;
                    g.enemy_strike(ei, true);
                    if(g.hits_count && g.hits[0].kind == hit_dodge) continue;
                    int got = 30000 - g.hero.hp;
                    tlo = imin(tlo, got); thi = imax(thi, got);
                }
                CHECK(tlo >= h.min && thi <= h.max && tlo == h.min && thi == h.max);
                message vm; versus_line(vm, b, h); CHECK(vm.n < log_len - 1);
            }
        std::printf("Rozpiska obrażeń: %d konfiguracji zgodnych z walką, skraje kryt osiągnięte w %d\n", configs, reached);
        CHECK(reached * 10 >= configs * 8);   // kryt: skraje też osiągane (poza rzadkimi krytami)
        // porównanie: rozpiska z zamianą sprzętu / broni = rozpiska po zamianie
        {
            game g; arena(g, 1);
            for(int s = 0; s < data::gear_slots_count; ++s)
                for(int r = 0; r < 3; ++r)
                    for(int t = 0; t < data::gear_traits_count; ++t)
                    {
                        dmg_breakdown w = g.weapon_breakdown(-1, -1, s, r, t);
                        game h = g; h.equip(s, r, t);
                        dmg_breakdown x = h.weapon_breakdown();
                        CHECK(w.min == x.min && w.max == x.max && w.crit_pct == x.crit_pct && w.avg10 == x.avg10);
                    }
            for(int t = 0; t < data::tools_count; ++t)
            {
                dmg_breakdown w = g.weapon_breakdown(-1, data::tools[t].weapon);
                game h = g; h.weapon_override = data::tools[t].weapon;
                dmg_breakdown x = h.weapon_breakdown();
                CHECK(w.min == x.min && w.max == x.max && w.range == x.range);
            }
            dmg_breakdown a = g.weapon_breakdown(), b = g.weapon_breakdown(-1, -1, 1, 2, 0);   // rękawice markowe
            message m; compare_line(m, a, b);
            CHECK(b.min == a.min + data::gear[3 + 2].value && m.n < log_len - 1);
            message mc; compare_crit(mc, a, b); CHECK(mc.n < log_len - 1);
        }
        {   // przykład: Murarz (Kielnia 4-6, SIŁ 5), bez premii: 6-8, kryt x2 12-16, szansa 5%
            game g; arena(g, 1);
            dmg_breakdown b = g.weapon_breakdown();
            CHECK(b.wmin == 4 && b.wmax == 6 && b.stat_dmg == 2 && b.min == 6 && b.max == 8 && b.avg10 == 70);
            CHECK(b.crit_min == 12 && b.crit_max == 16 && b.crit_chance() == data::crit_base_pct);
            message m; dmg_line(m, b, dmg_text::total); CHECK(std::strcmp(m.s, "Cios 6-8, średnio 7") == 0);
            b.pct = 10; b.finish();   // 6..8 +10%: 6 -> 6 albo 7, 8 -> 8 albo 9
            CHECK(b.min == 6 && b.max == 9 && b.avg10 == 77);
            dmg_breakdown v = g.weapon_breakdown(data::enemy_budzet);   // obrona problemu / 2
            CHECK(v.min == imax(1, 6 - data::enemies[data::enemy_budzet].defense / 2));
        }
    }
    if(std::getenv("PB_NO_BALANCE")) { std::printf(fails ? "\n%d FAIL\n" : "\nOK (bez balansu)\n", fails); return fails != 0; }
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
    {
        int easy = diff_wins[0] * 100 / (300 * data::classes_count), hard = diff_wins[data::difficulties_count - 1] * 100 / (300 * data::classes_count);
        std::printf("Łatwy %d%%, Trudny %d%%\n", easy, hard);
        CHECK(easy >= 50 && easy <= 60 && hard >= 8 && hard <= 15);   // cele balansu v0.21.49
    }
    // Tabela balansu (Normalny, wszystkie zawody): Szkolenia, Respekt, tryb inwestora; kawa bota (czy przedmioty mają znaczenie)
    {
        auto win_rate = [](const run_mods& m, long& drinks, int& drank_runs) {
            int wins = 0; const int runs = 300;
            drinks = 0; drank_runs = 0;
            for(int c = 0; c < data::classes_count; ++c)
                for(int k = 0; k < runs; ++k)
                {
                    game g; g.new_run(c, 1000 + k * 7919, data::default_difficulty, m);
                    bot_drinks = 0;
                    for(int step = 0; step < 4000; ++step)
                    {
                        if(g.st == status::stage_clear) { g.next_stage(); continue; }
                        if(g.st != status::playing) break;
                        bot_step(g);
                    }
                    wins += g.st == status::won; drinks += bot_drinks; drank_runs += bot_drinks > 0;
                }
            return wins * 100 / (runs * data::classes_count);
        };
        const int n = 300 * data::classes_count;
        profile none; profile_reset(none);
        profile szk = none; for(int i = 0; i < data::upgrades_count; ++i) szk.levels[i] = uint8_t(data::upgrades[i].levels);
        profile full = szk; for(int i = 0; i < data::respect_count; ++i) full.respect_ranks[i] = uint8_t(data::respect[i].ranks);
        profile inv = full; inv.wins = 1; inv.investor = uint8_t((1 << data::investor_count) - 1);
        profile fullr = full; fullr.rewards = uint8_t(data::rewards_count);   // + wszystkie nagrody za odbiór, w tym Akt 0
        long dr0, dr1, dr2, dr3, dr4, drx; int k0, k1, k2, k3, k4, kx;
        int w0 = win_rate(mods(none), dr0, k0), w1 = win_rate(mods(szk), dr1, k1), w2 = win_rate(mods(full), dr2, k2), w3 = win_rate(mods(inv), dr3, k3);
        int w4 = win_rate(mods(fullr), dr4, k4);
        bot_no_coffee = true;
        int wx = win_rate(mods(none), drx, kx), wx1 = win_rate(mods(szk), drx, kx);
        bot_no_coffee = false;
        std::printf("Normalny: bez meta %d%%, pełne Szkolenia %d%%, + pełny Respekt %d%%, + wszystkie modyfikatory %d%%, pełne meta z Aktem 0 %d%%\n", w0, w1, w2, w3, w4);
        std::printf("Kawa (bot): %.2f/budowę, pije w %d%% budów (bez meta); bez picia kawy: %d%% (pełne Szkolenia %d%%)\n",
                    double(dr0) / n, k0 * 100 / n, wx, wx1);
        CHECK(w0 >= 25 && w0 <= 35);   // cele balansu v0.21.49
        CHECK(w1 >= 50 && w1 <= 60);
        CHECK(w2 >= 65 && w2 <= 75);
        CHECK(w3 >= 5 && w3 <= 15);
        CHECK(w4 >= 60 && w4 <= 70);   // v0.21.49 cz. 3: dłuższa budowa z Aktem 0
        CHECK(wx < w0 && dr0 > 0);     // kawa ma znaczenie
        (void)dr1; (void)dr2; (void)dr3; (void)dr4; (void)k1; (void)k2; (void)k3; (void)k4;
    }
    std::printf(fails ? "\n%d FAIL\n" : "\nOK - wszystkie testy przeszły\n", fails);
    return fails != 0;
}
