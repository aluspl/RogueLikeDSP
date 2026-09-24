// Testy rdzenia na PC: g++ -std=c++20 -I../include core_tests.cpp && ./a.out
#include <cstdio>
#include <cassert>
#include <queue>
#include "core.h"
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
    if(g.nearest_target() >= 0 && g.weapon().range > 1) { g.player_attack_nearest(); return; }
    int tx = g.stairs_x, ty = g.stairs_y, best = 999;
    for(int i=0;i<g.enemies_count;++i){ auto& e=g.enemies[i]; int d=cheb(g.hero.x,g.hero.y,e.x,e.y); if(e.alive&&d<best&&(d<6||g.stairs_x<0)){best=d;tx=e.x;ty=e.y;} }
    // BFS po mapie do celu
    int px[map_h][map_w]; for(auto& r:px) for(auto& c:r) c=-1;
    std::queue<std::pair<int,int>> q; q.push({g.hero.x,g.hero.y}); px[g.hero.y][g.hero.x]=4;
    int d[4][2]={{1,0},{-1,0},{0,1},{0,-1}};
    while(!q.empty()){ auto [x,y]=q.front(); q.pop(); if(x==tx&&y==ty) break;
        for(int k=0;k<4;++k){int nx=x+d[k][0],ny=y+d[k][1]; if(g.lv.passable(nx,ny)&&px[ny][nx]<0){px[ny][nx]=k;q.push({nx,ny});}}}
    if(tx<0||px[ty][tx]<0){ g.player_wait(); return; }
    int x=tx,y=ty; while(true){int k=px[y][x]; int bx=x-d[k][0],by=y-d[k][1]; if(bx==g.hero.x&&by==g.hero.y){ if(!g.player_move(x-bx,y-by)) g.player_wait(); return;} x=bx;y=by;}
}

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
    // 7. balans: bot gra po 300 runów każdym zawodem na każdym poziomie
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
    std::printf(fails ? "\n%d FAIL\n" : "\nOK - wszystkie testy przeszły\n", fails);
    return fails != 0;
}
