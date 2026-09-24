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
    // 3. balans: bot gra po 300 runów każdym zawodem
    std::printf("%-18s %6s %6s %6s %8s\n","zawód","wygr.%","śr.etap","śr.tury","śr.wynik");
    for(int c=0;c<data::classes_count;++c)
    {
        int wins=0; long stages=0, turns=0, score=0; const int runs=300;
        for(int k=0;k<runs;++k)
        {
            game g; g.new_run(c, 1000+k*7919);
            for(int step=0; step<4000; ++step)
            {
                if(g.st==status::stage_clear){ g.next_stage(); continue; }
                if(g.st!=status::playing) break;
                bot_step(g);
            }
            wins += g.st==status::won; stages += g.stage+1; turns += g.turns; score += g.score;
        }
        std::printf("%-18s %6d %6.1f %6ld %8ld\n", data::classes[c].name, wins*100/runs, double(stages)/runs, turns/runs, score/runs);
    }
    std::printf(fails ? "\n%d FAIL\n" : "\nOK - wszystkie testy przeszły\n", fails);
    return fails != 0;
}
