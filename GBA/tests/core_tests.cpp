// Testy rdzenia na PC: g++ -std=c++20 -I../include core_tests.cpp && ./a.out
#include <cstdio>
#include <cstdlib>
#include <cassert>
#include <queue>
#include <cstring>
#include "core.h"
#include "meta.h"
using namespace core;
static int fails = 0;
static constexpr int F0 = data::prelude_stages;   // bez nagrody Akt 0 budowa zaczyna się od etapu F0 (Fundamenty)
#define CHECK(c) do{ if(!(c)){ std::printf("FAIL %s:%d %s\n", __FILE__, __LINE__, #c); ++fails; } }while(0)

// Spójność: cała podłoga osiągalna ze startu (g: bez zamkniętego magazynu - v0.21.50 cz. 3, #32).
static bool connected(const level& lv, int sx, int sy, const game* g = nullptr)
{
    bool seen[map_h][map_w] = {};
    std::queue<std::pair<int,int>> q; q.push({sx, sy}); seen[sy][sx] = true; int n = 1;
    while(!q.empty()){ auto [x,y]=q.front(); q.pop(); int d[4][2]={{1,0},{-1,0},{0,1},{0,-1}};
        for(auto& v:d){int nx=x+v[0],ny=y+v[1]; if(lv.passable(nx,ny)&&!seen[ny][nx]){seen[ny][nx]=true;++n;q.push({nx,ny});}}}
    int total=0; for(int y=0;y<map_h;++y) for(int x=0;x<map_w;++x) total+=lv.passable(x,y) && !(g && g->secret_closed() && g->in_secret(x,y));
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
    if(g.bot_pending()) return;   // v0.21.50 cz. 3: wydarzenie, premia z wydarzenia, cecha narzędzia, narzędzie
    if(g.can_open_secret() && g.hero.x == g.secret_front_x() && g.hero.y == g.secret_front_y()   // przed magazynem: otwórz
       && g.player_move(g.secret_x - g.hero.x, g.secret_y - g.hero.y)) return;
    if(! bot_no_coffee && g.thermos > 0 && g.hero.hp * 100 < g.hero.max_hp * data::bot_drink_below_pct && g.player_drink()) { ++bot_drinks; return; }
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

// Po etapie: bot wybiera premię (kolejność skutków z core: bot_boon_choice) i idzie dalej.
static bool bot_no_boons = false;   // wariant bez premii (porównanie)
static void bot_next(game& g)
{
    if(g.bot_wants_reroll() && ! bot_no_boons) g.reroll_boons();
    if(g.has_boon_offer() && ! bot_no_boons) g.pick_boon(g.bot_boon_choice());
    g.bot_upgrade();   // v0.21.50 cz. 3: Hurtownia - bot ulepsza narzędzie, jeśli stać (nic więcej nie kupuje)
    g.next_stage();
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

// Pełny Respekt (secret = false: bez rang z sekretnych zleceń - tabela balansu porównywalna z poprzednimi wersjami).
static void full_respect(profile& p, bool secret = true)
{
    for(int i = 0; i < data::respect_count; ++i) if(secret || data::respect[i].secret < 0) set_respect_rank(p, i, data::respect[i].ranks);
}

// Zwrot po starej cenie za `levels` poziomów Szkolenia i (migracja v13).
static int legacy_refund_of(int i, int levels) { profile p; profile_reset(p); p.levels[i] = uint8_t(levels); return legacy_refund(p, i); }

// Respekt z poziomów inspektora osiągniętych z dośw. xp (migracja v14 daje go starym profilom).
static int insp_respect(int xp)
{
    profile p; profile_reset(p); progress_gain r; add_inspector_xp(p, xp, r); return p.respect;
}
static int insp_migrated(int runs, int wins, int respect_total)
{
    return runs * data::inspector_migrate_run + wins * data::inspector_migrate_win + respect_total * data::inspector_migrate_respect_pct / 100;
}

static int g_dummy_crit(int cls) { game g; g.new_run(cls, 1); return g.crit_pct(); }

int main()
{
    // 1. mapy: spójne, pokoje >= 2, start na podłodze, deterministyczne
    for(uint32_t seed = 1; seed <= 500; ++seed)
    {
        game g; g.new_run(seed % data::classes_count, seed);
        CHECK(g.lv.rooms_count >= 2);
        CHECK(connected(g.lv, g.hero.x, g.hero.y, &g));
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
        auto regular = [](const game& x) { int n = 0; for(int i = 0; i < x.pickups_count; ++i) n += x.pickups[i].type <= plan; return n; };
        b.next_stage(); CHECK(regular(b) == regular(a) + 2);   // premia trwa w kolejnych etapach (bez wydarzeń i skrzyń)
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
        CHECK(m.hp == upgrade_total(0, data::upgrades[0].levels, upgrade_effect::hp) && m.hp > 0 && m.def == 0);
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
            g.enemies_count = 0; g.key_holder = -1; g.spawn(0, g.hero.x + 1, g.hero.y); g.enemies[0].hp = 1;
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
        p.xp = 100; CHECK(buy_tool(p, 3) && tool_unlocked(p, 3) && !buy_tool(p, 3) && p.xp == 100 - data::tool_costs[0]);
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
        const int r2 = data::upgrades[0].legacy_costs[0] + data::upgrades[0].legacy_costs[1];   // v13: Szkolenia wracają jako doświadczenie
        CHECK(std::strcmp(v2.magic, profile_magic) == 0 && v2.xp == 77 + r2 && v2.levels[0] == 0 && v2.classes == 0x3F);
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
            if(it.effect == shop_effect::upgrade) continue;   // ulepszenie narzędzia: test 48
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
                case shop_effect::upgrade: break;   // test 48
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
        p.levels[i_luck] = uint8_t(data::upgrades[i_luck].levels); p.levels[i_craft] = uint8_t(data::upgrades[i_craft].levels);
        run_mods pm = mods(p);
        CHECK(pm.luck == upgrade_total(i_luck, 9, upgrade_effect::luck) && pm.luck >= 1);
        CHECK(pm.craft == upgrade_total(i_craft, 9, upgrade_effect::craft) && pm.craft >= 1);
        // co najmniej 2 narzędzia skalowane INT do odblokowania
        int int_tools = 0;
        for(int i = 0; i < data::tools_count; ++i)
            if(data::weapons[data::tools[i].weapon].scales_with == stat::intel && data::tools[i].shop) ++int_tools;
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
        CHECK(v3.best == 1234 && v3.runs == 9 && v3.wins == 4 && v3.xp == 321 + legacy_refund_of(0, 2) && v3.levels[0] == 0 && v3.classes == 0x1F);
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
                game g; g.new_run(k % data::open_classes_count, 3000 + k * 131, data::default_difficulty, mm);
                for(int step = 0; step < 4000; ++step)
                {
                    if(g.st == status::stage_clear) { bot_next(g); continue; }
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
        CHECK(classes_seen == (1 << data::open_classes_count) - 1);   // bez zawodów z sekretów
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
        s.secrets = uint16_t((1 << data::secrets_count) - 1);   // Respekt z sekretnego zlecenia (Zaprawiony w boju) odblokowany
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
            int first = -1;
            for(int i = 0; i < data::hurtownia_count; ++i)
            {
                if(data::hurtownia[i].effect == shop_effect::upgrade) { CHECK(a.hurtownia_price(i) == data::tool_levels[0].cash * 75 / 100); continue; }
                CHECK(a.hurtownia_price(i) == data::hurtownia[i].price * 75 / 100);
                if(first < 0 && data::hurtownia[i].material < 0) first = i;
            }
            a.cash = a.hurtownia_price(first); CHECK(a.hurtownia_can(first) && a.hurtownia_buy(first) && a.cash == 0);
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
        for(int i = 0; i < data::classes_count; ++i) CHECK(class_unlocked(p, i) == ((class_reward(i) && ! class_secret(i)) || (p.classes >> i) & 1));
        // wygrane i stawki zawodów 8+
        profile w; profile_reset(w);
        for(int c = 0; c < data::classes_count; ++c) { CHECK(! class_won(w, c)); set_class_won(w, c); CHECK(class_won(w, c)); set_best_stake(w, c, c + 1); }
        CHECK(classes_won(w) == data::classes_count);
        for(int c = 0; c < data::classes_count; ++c) CHECK(best_stake(w, c) == c + 1);
        game wg; wg.new_run(data::classes_count - 1, 3); wg.st = status::won;
        profile w2; profile_reset(w2); record_run(w2, wg); CHECK(class_won(w2, data::classes_count - 1) && w2.class_wins == 0);
        // v7 -> v8: stare pola zostają, nagrody za dotychczasowe wygrane, Szkolenia wracają jako doświadczenie (v13: stara cena)
        profile v7; profile_reset(v7); std::memcpy(v7.magic, "PBRL007", 8);
        v7.wins = 3; v7.xp = 11; v7.best = 777; v7.daily_score[4] = 55;
        int refund = 0;
        for(int i = 0; i < data::upgrades_count; ++i) { v7.levels[i] = 1; refund += data::upgrades[i].legacy_costs[0]; }
        CHECK(refund > 0);
        std::memset(reinterpret_cast<char*>(&v7) + profile_v7_size, 0xEE, sizeof v7 - profile_v7_size);
        CHECK(profile_fix(v7) && std::strcmp(v7.magic, profile_magic) == 0);
        CHECK(v7.best == 777 && v7.wins == 3 && v7.daily_score[4] == 55 && v7.rewards == imin(3, avail) && v7.xp == 11 + refund);
        const int r7 = insp_respect(insp_migrated(0, 3, 0));   // v0.21.52 cz. b: Respekt z poziomów inspektora (migracja v14)
        CHECK(v7.respect == r7 && v7.respect_total == r7 && v7.class_wins_hi == 0 && v7.respect_ranks[0] == 0 && v7.best_stake_hi[0] == 0);
        for(int i = 0; i < data::upgrades_count; ++i) CHECK(v7.levels[i] == 0);
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
            CHECK(std::strcmp(run_magic, "PBRUN14") == 0);
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
                game b; b.new_run(k % data::open_classes_count, 300 + k * 13, 0, mods(p));
                for(int step = 0; step < 3000 && b.st == status::playing && b.stage < F0; ++step) bot_step(b);
                while(b.st == status::stage_clear && b.stage < F0) { bot_next(b); for(int step = 0; step < 3000 && b.st == status::playing; ++step) bot_step(b); }
                won_act0 += b.stage >= F0 || (b.st == status::stage_clear && b.stage == F0 - 1);
            }
            std::printf("Akt 0 (bot, Łatwy): %d/20\n", won_act0);
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
        CHECK(sizeof(profile) == 240 && std::strcmp(profile_magic, "PBRL014") == 0);
    }
    // 46. v0.21.50: rozpiska obrażeń broni (#26) - zakres z rozpiski = to, co naprawdę zadaje walka (wiele rzutów z seedem)
    {
        // źródła premii profilu: suma części = mods()
        profile p; profile_reset(p);
        for(int i = 0; i < data::upgrades_count; ++i) p.levels[i] = uint8_t(data::upgrades[i].levels);
        full_respect(p);
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
                    int dealt = g.hits[0].amount;   // pierwszy wpis = cios (kombinacje stanów to osobne trafienia)
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
            message mw; dmg_line(mw, b, dmg_text::weapon); CHECK(std::strcmp(mw.s, "Kielnia 4-6, zasięg 1") == 0);
            message ms; dmg_line(ms, b, dmg_text::stat); CHECK(std::strcmp(ms.s, "SIŁ 5: +2 (+1 co 2 pkt)") == 0);
            message mk; dmg_line(mk, b, dmg_text::crit); CHECK(std::strcmp(mk.s, "Kryt x2: 12-16, szansa 5%") == 0);
            message mg; compare_line(mg, b, g.weapon_breakdown(-1, -1, 1, 2, 0)); CHECK(std::strcmp(mg.s, "teraz 6-8 -> 9-11 (średnio +3)") == 0);
            message ml; gear_label(ml, data::gear[5]); CHECK(std::strcmp(ml.s, "+3 obrażeń") == 0);
            b.pct = 10; b.finish();   // 6..8 +10%: 6 -> 6 albo 7, 8 -> 8 albo 9
            CHECK(b.min == 6 && b.max == 9 && b.avg10 == 77);
            dmg_breakdown v = g.weapon_breakdown(data::enemy_budzet);   // obrona problemu / 2
            CHECK(v.min == imax(1, 6 - data::enemies[data::enemy_budzet].defense / 2));
        }
    }
    // 47. v0.21.50 cz. 2: premie po etapie (#27), elity (#28), kombinacje stanów (#29)
    {
        auto boon_idx = [](const char* n) { for(int b = 0; b < data::boons_count; ++b) if(std::strcmp(data::boons[b].name, n) == 0) return b; return -1; };
        auto syn_idx = [](const char* n) { for(int s = 0; s < data::synergies_count; ++s) if(std::strcmp(data::synergies[s].name, n) == 0) return s; return -1; };
        auto clear_stage = [](game& g) { g.debug_skip(); };
        // oferta: po etapie 3 różne premie, dostępne dla zawodu, deterministyczne dla seeda (osobny generator - RNG gry bez zmian)
        int differ = 0;
        for(uint32_t seed = 1; seed <= 60; ++seed)
        {
            game a; a.new_run(int(seed % data::classes_count), seed); clear_stage(a);
            game b; b.new_run(int(seed % data::classes_count), seed); clear_stage(b);
            game c; c.new_run(int(seed % data::classes_count), seed + 1000); clear_stage(c);
            CHECK(a.st == status::stage_clear && a.has_boon_offer());
            CHECK(std::memcmp(a.boon_offer, b.boon_offer, 3) == 0 && a.r.s == b.r.s);
            differ += std::memcmp(a.boon_offer, c.boon_offer, 3) != 0;
            for(int k = 0; k < 3; ++k)
            {
                int bo = a.boon_offer[k];
                CHECK(bo >= 0 && bo < data::boons_count && (data::boons[bo].cls < 0 || data::boons[bo].cls == a.cls) && ! a.has_boon(bo));
                for(int j = 0; j < k; ++j) CHECK(a.boon_offer[j] != bo);
            }
            uint32_t rs = a.r.s;
            game p = a; p.pick_boon(0); CHECK(p.r.s == rs && ! p.has_boon_offer() && p.has_boon(a.boon_offer[0]) && p.boons_owned() == 1);
            game n = a; n.next_stage(); CHECK(! n.has_boon_offer() && n.boons == 0);   // bez wyboru oferta przepada
        }
        CHECK(differ >= 50);
        {   // ostatni etap (odbiór): bez oferty
            game g; g.new_run(0, 5); g.start_stage(data::stages_count - 1);
            g.enemies[g.boss].hp = 1; g.hero_attack(g.boss);
            if(g.st != status::won) { g.enemies[g.boss].hp = 1; g.debug_skip(); }
            CHECK(g.st == status::won && ! g.has_boon_offer());
        }
        // rzadkość: wagi z danych, szczęście przesuwa ku rzadkim i legendarnym
        {
            int cnt[2][3] = {};
            for(int lk = 0; lk < 2; ++lk)
                for(uint32_t seed = 1; seed <= 400; ++seed)
                {
                    game g; g.new_run(1, seed * 13); if(lk) g.bonus.luck = 5;
                    clear_stage(g);
                    for(int k = 0; k < 3; ++k) ++cnt[lk][data::boons[g.boon_offer[k]].rarity];
                }
            std::printf("Premie: rzadkość bez szczęścia %d/%d/%d, SZCZ 5: %d/%d/%d\n", cnt[0][0], cnt[0][1], cnt[0][2], cnt[1][0], cnt[1][1], cnt[1][2]);
            CHECK(cnt[0][0] > cnt[0][1] && cnt[0][1] > cnt[0][2] && cnt[0][2] > 0);
            CHECK(cnt[1][1] + cnt[1][2] > cnt[0][1] + cnt[0][2] && cnt[1][2] > cnt[0][2]);
            game g; g.new_run(1, 3); CHECK(g.boon_weight(0) == data::boon_rarities[0].weight && g.boon_weight(2) == data::boon_rarities[2].weight);
        }
        // losowanie: raz płatne na budowę, Druga oferta (Respekt) daje darmowe; nowa oferta inna
        {
            game g; g.new_run(2, 77); clear_stage(g);
            int8_t before[3]; std::memcpy(before, g.boon_offer, 3);
            g.cash = data::boon_reroll_cost - 1; CHECK(! g.can_reroll() && ! g.reroll_boons());
            g.cash = data::boon_reroll_cost + 5; CHECK(g.reroll_price() == data::boon_reroll_cost && g.reroll_boons());
            CHECK(g.cash == 5 && g.rerolls_left() == 0 && ! g.reroll_boons() && std::memcmp(before, g.boon_offer, 3) != 0 && g.has_boon_offer());
            run_mods m; m.rerolls = 1;
            game f; f.new_run(2, 77, data::default_difficulty, m); clear_stage(f);
            f.cash = 0; CHECK(f.reroll_price() == 0 && f.reroll_boons() && f.cash == 0 && f.rerolls_left() == 1);
            f.cash = 100; CHECK(f.reroll_price() == data::boon_reroll_cost && f.reroll_boons() && f.cash == 100 - data::boon_reroll_cost);
            profile p; profile_reset(p); full_respect(p);
            CHECK(mods(p).rerolls == 1);
        }
        // skutki premii: natychmiastowe i stałe; rozpiska = walka z premiami i elitą (Tarcza)
        {
            game g; arena(g, 1);
            int hp = g.hero.max_hp, cash = g.cash, def = g.hero_defense(), cool = g.ability_cooldown(), cap = g.thermos_cap();
            auto give = [&](const char* n) { int b = boon_idx(n); CHECK(b >= 0); g.boon_offer[0] = int8_t(b); CHECK(g.pick_boon(0)); };
            auto val = [&](const char* n) { return int(data::boons[boon_idx(n)].value); };
            give("Płyta warstwowa"); CHECK(g.hero.max_hp == hp + val("Płyta warstwowa"));
            give("Premia od inwestora"); CHECK(g.cash == cash + g.income(val("Premia od inwestora")));
            give("Paleta materiałów"); for(int mm = 0; mm < data::materials_count; ++mm) CHECK(g.mats[mm] == val("Paleta materiałów"));
            int zb = syn_idx("Zbrojenie");
            give("Beton B30"); CHECK(g.hero_defense() == def + val("Beton B30") && ! g.synergy_active(zb));
            give("Druga zmiana"); CHECK(g.ability_cooldown() == imax(3, cool - val("Druga zmiana")));
            give("Termos z bufetu"); CHECK(g.thermos_cap() == cap + 1 && g.thermos == 1);
            dmg_breakdown b0 = g.weapon_breakdown();
            give("Hartowana kielnia"); give("Zbrojona rękawica"); give("Hydrofor"); give("Szczęśliwa moneta");
            dmg_breakdown b1 = g.weapon_breakdown();
            const int fb = val("Hartowana kielnia") + val("Zbrojona rękawica"), pb = val("Hydrofor"), cb = val("Szczęśliwa moneta");
            CHECK(b1.flat_boon == fb && b1.pct_boon == pb && b1.crit_boon == cb && b1.crit_pct == g.crit_pct() && b1.min > b0.min);
            message ml, me; me.add("Premie etapów: +").add(fb).add(", +").add(pb).add("%, kryt +").add(cb).add("%");
            CHECK(dmg_line(ml, b1, dmg_text::boon) && std::strcmp(ml.s, me.s) == 0);
            // Zbrojenie: 2+ premie Beton (B30, Hartowana kielnia) -> +1 OBR za każdą
            CHECK(zb >= 0 && g.synergy_active(zb) && g.tag_count(2) == 2);
            CHECK(g.hero_defense() == def + val("Beton B30") + 2 * data::synergies[zb].value);
            // walka: z premiami i elitą (Tarcza) w zakresie rozpiski
            for(int t = 0; t < data::elites_count; ++t)
            {
                g.enemies_count = 0; g.spawn(data::enemy_kornik, 8, 7); g.make_elite(0, t);
                g.enemies[0].hp = g.enemies[0].max_hp = 30000;
                dmg_breakdown be = g.actor_breakdown(0);
                CHECK(be.enemy_elite == g.enemy_elite_def(0) && be.def_cut == g.enemy_defense(0) / 2);
                int lo = 999, hi = 0;
                for(int k = 0; k < 1500; ++k)
                {
                    g.dmg_carry = k % 100; g.hits_count = 0;
                    g.hero_attack(0);
                    if(g.hits[0].kind == hit_crit) continue;
                    lo = imin(lo, g.hits[0].amount); hi = imax(hi, g.hits[0].amount);
                }
                CHECK(lo == be.min && hi == be.max);
                hit_range h = g.enemy_hit(0); hit_range h0 = enemy_hit_range(1, 3, g.enemy_dmg_bonus() + data::elite_dmg, g.hero_defense(), 0);
                CHECK(h.min == h0.min && h.max == h0.max);
                message m; dmg_line(m, be, dmg_text::enemy); CHECK(m.n < log_len - 1);
                message nm; g.enemy_name(nm, 0); CHECK(std::strncmp(nm.s, data::elites[t].prefix[0], std::strlen(data::elites[t].prefix[0])) == 0);
            }
        }
        // synergie: Przepięcie (woda + prąd), nowa synergia w dzienniku
        {
            game g; arena(g, 1);
            int pr = syn_idx("Przepięcie");
            g.boon_offer[0] = int8_t(boon_idx("Wąż ogrodowy")); g.pick_boon(0); CHECK(! g.synergy_active(pr) && ! g.hit_power());
            g.boon_offer[0] = int8_t(boon_idx("Przedłużacz")); g.pick_boon(0); CHECK(g.synergy_active(pr) && g.hit_power());
            CHECK(std::strstr(g.log[log_lines - 1].s, "Synergia: Przepięcie") != nullptr);
            int bhp = syn_idx("Pełne BHP");
            g.boon_offer[0] = int8_t(boon_idx("Szelki asekuracyjne")); g.pick_boon(0);
            g.boon_offer[0] = int8_t(boon_idx("Kask z latarką")); g.pick_boon(0); CHECK(g.synergy_active(bhp));
            g.apply_status(status_effect::shock, 1); g.apply_status(status_effect::poison, 3);
            CHECK(g.status_turns(status_effect::shock) == 0 && g.status_turns(status_effect::poison) == 0);
            // Espresso: kawa ładuje moc
            game e; arena(e, 1);
            e.boon_offer[0] = int8_t(boon_idx("Podwójne espresso")); e.pick_boon(0);
            e.boon_offer[0] = int8_t(boon_idx("Termos z bufetu")); e.pick_boon(0);
            e.ability_cd = 10; e.hero.hp = 5; e.thermos = 1; CHECK(e.player_drink() && e.ability_cd <= 10 - 3);
            // premie zawodów: tylko własny zawód
            for(int b = 0; b < data::boons_count; ++b)
                if(data::boons[b].cls >= 0) { game q; q.new_run(data::boons[b].cls == 0 ? 1 : 0, 3); CHECK(! q.boon_available(b)); }
        }
        // elity: szansa rośnie z aktem i trudnością, więcej HP, cechy, nagroda
        {
            game a; a.new_run(0, 9, 0); game h; h.new_run(0, 9, 2);
            CHECK(a.elite_chance() < h.elite_chance());
            game l; l.new_run(0, 9); int c1 = l.elite_chance(); l.start_stage(data::stages_count - 2); CHECK(l.elite_chance() > c1);
            int elites = 0, all = 0;
            for(uint32_t seed = 1; seed <= 300; ++seed)
            {
                game g; g.new_run(0, seed); g.start_stage(F0 + 8);
                for(int i = 0; i < g.enemies_count; ++i) if(i != g.boss && g.enemies[i].alive) { ++all; elites += g.enemies[i].elite >= 0; }
            }
            std::printf("Elity: %d z %d problemów (%d%%) na etapie %d\n", elites, all, elites * 100 / imax(1, all), F0 + 9);
            CHECK(elites > 0 && iabs(elites * 100 / all - l.elite_chance()) <= 6);
            auto trait = [](elite_effect e) { for(int t = 0; t < data::elites_count; ++t) if(data::elites[t].effect == e) return t; return -1; };
            game g; arena(g, 1);
            g.spawn(data::enemy_kornik, 10, 7); int base_hp = g.enemies[0].max_hp; g.make_elite(0, trait(elite_effect::fast));
            CHECK(g.enemies[0].max_hp == base_hp * data::elite_hp_pct / 100 && g.is_elite(0));
            g.enemies[0].awake = true; g.player_wait(); CHECK(g.enemies[0].x == 8);   // szybka: 2 pola na turę
            game rg; arena(rg, 1); rg.spawn(data::enemy_kamien, 12, 12); rg.make_elite(0, trait(elite_effect::regen));
            rg.enemies[0].awake = true; rg.enemies[0].hp = 5; rg.player_wait(); CHECK(rg.enemies[0].hp == 5 + data::elites[trait(elite_effect::regen)].value);
            game ex; arena(ex, 1); ex.spawn(data::enemy_kornik, 8, 7); ex.make_elite(0, trait(elite_effect::explode));
            ex.enemies[0].hp = 1; ex.hero_attack(0); CHECK(ex.blast_timer > 0);
            int resp = ex.respect; (void)resp;
            CHECK(ex.respect == data::elite_respect);
            bool box = false;   // pewny drop w polu elity (paczka co najmniej solidna)
            for(int i = 0; i < ex.pickups_count; ++i)
                box |= ex.pickups[i].x == 8 && ex.pickups[i].y == 7 && (ex.pickups[i].type != gear_box || ex.pickups[i].arg % 3 >= data::elite_gear_min);
            CHECK(box);
            game sm; arena(sm, 1); sm.spawn(data::enemy_kornik, 8, 7); sm.make_elite(0, trait(elite_effect::summon));
            sm.enemies[0].hp = 3; sm.damage_enemy(0, 1, false, "t"); CHECK(sm.enemies_count == 2 && sm.enemies[1].alive && (sm.enemies[0].flags & actor_called));
            sm.damage_enemy(0, 1, false, "t"); CHECK(sm.enemies_count == 2);   // tylko raz
        }
        // kombinacje: mokry + prąd (porażenie obok), pył + iskra (wybuch), zamróz + uderzenie (pęknięcie), na bohaterze
        {
            game g; arena(g, 3);   // Elektryk: Próbnik (prąd)
            CHECK(g.hit_power() && ! g.hit_spark());
            g.spawn(data::enemy_przeciek, 8, 7); g.spawn(data::enemy_kornik, 9, 7); g.spawn(data::enemy_kornik, 9, 8);
            for(int i = 0; i < 3; ++i) g.enemies[i].hp = g.enemies[i].max_hp = 500;
            g.enemies[1].wet = 3;   // mokry obok celu; [2] suchy
            g.hero_attack(0);
            CHECK(g.combo_events & 1);
            CHECK(g.enemies[1].hp == 500 - data::combos[0].value && g.enemies[2].hp == 500);
            game d; arena(d, 5);   // Glazurnik: Szlifierka (iskra)
            CHECK(d.hit_spark());
            d.spawn(data::enemy_kornik, 8, 7); d.spawn(data::enemy_kornik, 9, 8); d.spawn(data::enemy_kornik, 11, 7);
            for(int i = 0; i < 3; ++i) { d.enemies[i].hp = d.enemies[i].max_hp = 500; d.enemies[i].flags = actor_dusty; }
            d.hero_attack(0);
            CHECK((d.combo_events & 2) && d.enemies[1].hp == 500 - data::combos[1].value && d.enemies[2].hp == 500);
            CHECK(! d.enemy_dusty(0) && ! d.enemy_dusty(1) && d.enemy_dusty(2));
            game f; arena(f, 1);   // Murarz: wręcz
            f.spawn(data::enemy_kornik, 8, 7); f.enemies[0].hp = f.enemies[0].max_hp = 500; f.enemies[0].flags = actor_frozen;
            f.hero_attack(0);
            CHECK((f.combo_events & 4) && ! f.enemy_frozen(0) && f.hits_count == 2 && f.hits[1].amount == imax(1, f.hits[0].amount * data::combos[2].value / 100));
            // na starcie etapu: akt III - zapyleni, Mróz - zmrożeni
            game t; t.new_run(0, 4); t.start_stage(F0 + 8);
            for(int i = 0; i < t.enemies_count; ++i) if(i != t.boss && t.enemies[i].alive) CHECK(t.enemy_dusty(i));
            // bohater: mokry (kałuża, cios wody) + prąd = porażenie
            game hh; arena(hh, 1);
            hh.spawn(data::enemy_przeciek, 8, 7); hh.spawn(data::enemy_zwarcie, 6, 7);
            hh.hero.hp = hh.hero.max_hp = 500;
            int guard = 0;
            while(! hh.hero_wet() && guard++ < 50) hh.enemy_strike(0, false);
            CHECK(hh.hero_wet());
            int before = hh.hero.hp;
            for(guard = 0; guard < 50 && hh.hero.hp == before; ++guard) hh.enemy_strike(1, false);
            CHECK(hh.combo_events & 8);
            CHECK(hh.status_turns(status_effect::shock) > 0 || (hh.bonus.second_chance && false));
            // Zawór moczy odepchniętych, Wąż ogrodowy moczy cel, kałuża moczy problem
            game z; arena(z, 4); z.spawn(data::enemy_kornik, 8, 7); z.enemies[0].hp = 99; z.hero.hp = 1;
            CHECK(z.player_ability() && z.enemies[0].wet > 0);
            game w; arena(w, 1); w.boon_offer[0] = int8_t(boon_idx("Wąż ogrodowy")); w.pick_boon(0);
            w.spawn(data::enemy_kornik, 8, 7); w.enemies[0].hp = 99; w.hero_attack(0); CHECK(w.enemy_wet(0));
            // teksty mieszczą się w buforze
            for(int c = 0; c < data::combos_count; ++c) { message m; m.add(data::combos[c].short_name).add(" ").add(data::combos[c].name); CHECK(m.n < log_len - 1); }
            for(int b = 0; b < data::boons_count; ++b) { message m; m.add("Premia: ").add(data::boons[b].name); CHECK(m.n < log_len - 1); }
            for(int e = 0; e < data::enemies_count; ++e)
                for(int t2 = 0; t2 < data::elites_count; ++t2)
                { message m; m.add(data::elites[t2].prefix[data::enemies[e].gender]).add(" ").add(data::enemies[e].name); CHECK(m.n < log_len - 1); }
        }
        // zapis budowy: premie, oferta i elity w stanie gry (nowa magia)
        {
            game g; g.new_run(3, 42); clear_stage(g); g.pick_boon(1);
            run_save* sv = new run_save(); run_save_make(*sv, g);
            CHECK(run_save_valid(*sv) && sv->g.boons == g.boons && std::memcmp(sv->magic, "PBRUN14", 7) == 0);
            delete sv;
        }
    }
    // 48. v0.21.50 cz. 3: wydarzenia z wyborem (#30), ulepszanie narzędzia (#31), ukryte pomieszczenia (#32)
    {
        // wydarzenia: pole tylko poza pierwszym etapem i bossem, deterministyczne z seeda, bez powtórek w budowie
        int tiles = 0, stages_n = 0;
        for(uint32_t seed = 1; seed <= 60; ++seed)
        {
            game a, b; a.new_run(int(seed % data::classes_count), seed * 131u); b.new_run(int(seed % data::classes_count), seed * 131u);
            uint16_t seen = 0;
            for(int st = a.first_stage; st < data::stages_count; ++st)
            {
                if(st != a.first_stage) { a.start_stage(st); b.start_stage(st); }
                int ev = -1, n = 0;
                for(int i = 0; i < a.pickups_count; ++i) if(a.pickups[i].type == event_tile) { ev = a.pickups[i].arg; ++n; }
                int evb = -1; for(int i = 0; i < b.pickups_count; ++i) if(b.pickups[i].type == event_tile) evb = b.pickups[i].arg;
                CHECK(ev == evb && n <= 1);
                if(ev >= 0)
                {
                    CHECK(data::stages[st].boss < 0 && st != a.first_stage && ! ((seen >> ev) & 1));
                    seen = uint16_t(seen | (1u << ev)); ++tiles;
                    for(int i = 0; i < a.pickups_count; ++i)
                        if(a.pickups[i].type == event_tile) CHECK(a.lv.at(a.pickups[i].x, a.pickups[i].y) == tile::floor && ! a.occupied(a.pickups[i].x, a.pickups[i].y));
                }
                ++stages_n;
            }
        }
        CHECK(tiles > stages_n / 5 && tiles < stages_n * 3 / 4);
        // każda odpowiedź każdego wydarzenia: skutki bez błędów, ten sam seed = ten sam wynik (szansa z osobnego generatora)
        for(int e = 0; e < data::choice_events_count; ++e)
            for(int k = 0; k < data::choice_events[e].choices_count; ++k)
            {
                static game a, b;
                a.new_run(1, 777u + uint32_t(e)); a.start_stage(F0 + 1); a.enemies_count = 1;
                a.hero.hp = a.hero.max_hp - 5; a.cash = 50; for(auto& m : a.mats) m = 3;
                b = a;
                const int cash0 = a.cash, hp0 = a.hero.hp, n0 = a.enemies_count;
                a.pending_event = int8_t(e); b.pending_event = int8_t(e);
                CHECK(a.choose_event(k) && b.choose_event(k));
                CHECK(a.choice_done == b.choice_done && a.cash == b.cash && a.hero.hp == b.hero.hp && a.enemies_count == b.enemies_count);
                CHECK(a.pending_event < 0 && a.stage_choice == e && a.stage_choice_pick == k && a.hero.hp >= 1);
                const event_choice& c = data::choice_events[e].choices[k];
                for(int i = 0; i < c.outs; ++i)
                {
                    const choice_out& o = c.out[i];
                    const bool done = (a.choice_done >> i) & 1;
                    CHECK(done || o.chance < 100);
                    if(! done) continue;
                    if(o.effect == choice_effect::cash && c.outs == 1) CHECK(a.cash == imax(0, cash0 + (o.value > 0 ? a.income(o.value) : o.value)));
                    if(o.effect == choice_effect::hp && c.outs == 1) CHECK(a.hero.hp == imin(a.hero.max_hp, imax(1, hp0 + o.value)));
                    if(o.effect == choice_effect::spawn) CHECK(a.enemies_count > n0 && a.enemies[a.enemies_count - 1].def_id == o.arg);
                    if(o.effect == choice_effect::boon) CHECK(a.has_boon_offer());
                    if(o.effect == choice_effect::stage_dmg) CHECK(a.event_dmg == o.value && a.weapon_breakdown().flat_event == o.value);
                    if(o.effect == choice_effect::stage_def) CHECK(a.event_def == o.value);
                    if(o.effect == choice_effect::upgrade) CHECK(a.weapon_lvl == 1);
                }
                message m; choice_label(m, c); CHECK(m.n > 0 && m.n < log_len - 1);
                a.bot_pending(); a.bot_pending();
                CHECK(! a.has_boon_offer() && ! a.trait_pending);
            }
        {   // premia z wydarzenia: inna oferta niż po etapie, wybór czyści ofertę; etap zeruje premie z wydarzenia
            game g; g.new_run(0, 4242); g.start_stage(F0 + 1);
            g.roll_boons(); int8_t o1[3]; for(int k = 0; k < 3; ++k) o1[k] = g.boon_offer[k];
            g.roll_boons(50); CHECK(o1[0] != g.boon_offer[0] || o1[1] != g.boon_offer[1] || o1[2] != g.boon_offer[2]);
            g.skip_boons();
            g.event_dmg = 2; g.event_def = 2; const int d0 = g.hero_defense();
            g.start_stage(F0 + 2); CHECK(g.event_dmg == 0 && g.event_def == 0 && g.hero_defense() == d0 - 2);
            // bot: wybór wg wartości (Betoniarka: pożycza)
            g.pending_event = 0; CHECK(g.bot_event_choice() == 0);
        }
        // ulepszenie narzędzia: Hurtownia (zł + stal), maks. +3, od +2 cecha; rozpiska = walka (przebicie, ostrze, wyważenie)
        {
            game g; arena(g, 1);
            int up = -1; for(int i = 0; i < data::hurtownia_count; ++i) if(data::hurtownia[i].effect == shop_effect::upgrade) up = i;
            CHECK(up >= 0);
            g.cash = 0; g.mats[1] = 0; CHECK(! g.hurtownia_can(up));
            g.cash = 500; g.mats[1] = 9;
            const dmg_breakdown b0 = g.weapon_breakdown();
            CHECK(g.hurtownia_price(up) == data::tool_levels[0].cash && g.hurtownia_buy(up) && g.weapon_lvl == 1);
            CHECK(g.cash == 500 - data::tool_levels[0].cash && g.mats[1] == 9 - data::tool_levels[0].count);
            const dmg_breakdown b1 = g.weapon_breakdown();
            CHECK(b1.min == b0.min + 1 && b1.max == b0.max + 1 && b1.upg_level == 1);
            { message m; dmg_line(m, b1, dmg_text::weapon); CHECK(std::strstr(m.s, "+1 ") != nullptr); }
            CHECK(g.hurtownia_buy(up) && g.weapon_lvl == 2 && g.trait_pending && ! g.hurtownia_can(up));   // cecha czeka
            CHECK(! g.choose_trait(9) && g.choose_trait(0) && g.weapon_trait == 0 && ! g.trait_pending);
            g.mats[1] = 9; CHECK(g.hurtownia_buy(up) && g.weapon_lvl == 3 && ! g.hurtownia_can(up));
            { message m; g.weapon_title(m); CHECK(std::strstr(m.s, "+3") != nullptr); }
            // każda cecha: zakres z rozpiski = walka (także z premią z wydarzenia), teksty w buforze
            for(int t = 0; t < data::tool_traits_count; ++t)
                for(int ed : { data::enemy_kamien, data::enemy_przeciek, data::enemy_zbrojenie })
                {
                    g.weapon_trait = int8_t(t); g.event_dmg = int8_t(t);
                    dmg_breakdown b = g.weapon_breakdown(ed);
                    CHECK(b.crit_pct == g.crit_pct());
                    rng pick; pick.seed(uint32_t(9 + t * 7 + ed));
                    int lo = 999, hi = 0;
                    for(int k = 0; k < 2000; ++k)
                    {
                        g.dmg_carry = pick.range(0, 99); g.hits_count = 0;
                        g.spawn(ed, 8, 7);
                        actor& e = g.enemies[g.enemies_count - 1]; e.hp = e.max_hp = 30000;
                        g.hero_attack(g.enemies_count - 1);
                        if(g.hits[0].kind != hit_crit) { lo = imin(lo, g.hits[0].amount); hi = imax(hi, g.hits[0].amount); }
                        g.enemies_count = 0;
                    }
                    CHECK(lo == b.min && hi == b.max);
                    for(int x = 0; x < dmg_texts; ++x) { message m; dmg_line(m, b, dmg_text(x)); CHECK(m.n > 0 && m.n < log_len - 1); }
                }
            g.event_dmg = 0;
            // narzędzie na polu przy ulepszonym: decyzja; zostawiam - ulepszenie zostaje, biorę - przepada
            g.pickups[0] = { int8_t(g.hero.x + 1), int8_t(g.hero.y), uint8_t(tool), true, 0 }; g.pickups_count = 1;
            g.player_move(1, 0);
            CHECK(g.has_tool_offer() && g.pickups[0].active && g.weapon_lvl == 3);
            const dmg_breakdown cur = g.weapon_breakdown(), nw = g.weapon_breakdown(-1, data::tools[0].weapon);
            CHECK(nw.upg_level == 0 && cur.upg_level == 3 && g.bot_tool_accept() == (nw.avg10 > cur.avg10));
            g.decline_tool(); CHECK(! g.has_tool_offer() && g.weapon_lvl == 3 && g.pickups[0].active);
            g.player_move(-1, 0); g.player_move(1, 0); CHECK(g.has_tool_offer());
            g.accept_tool(); CHECK(g.weapon_lvl == 0 && g.weapon_trait < 0 && g.weapon_override == data::tools[0].weapon && ! g.pickups[0].active);
            // nowe narzędzie z Hurtowni też kasuje ulepszenie
            g.weapon_lvl = 2; g.weapon_trait = 1;
            for(int i = 0; i < data::hurtownia_count; ++i) if(data::hurtownia[i].effect == shop_effect::tool) { g.cash = 500; g.hurtownia_buy(i); }
            CHECK(g.weapon_lvl == 0 && g.weapon_trait < 0);
            // wydarzenie z ulepszeniem (Stara ostrzałka) i bot w Hurtowni
            game h; h.new_run(2, 99); h.act_cleared = true; h.cash = 300; for(auto& m : h.mats) m = 9;
            h.bot_upgrade(); h.bot_upgrade(); CHECK(h.weapon_lvl == 2 && h.weapon_trait == h.bot_trait_choice() && ! h.trait_pending);
        }
        // ukryte pomieszczenia: tylko poza bossem, ściana graniczy z pokojem, wnętrze niedostępne bez otwarcia, skrzynia,
        // klucz ma problem poza magazynem; otwarcie kluczem / Operatorem / wybuchem (drzwi - tylko klucz)
        {
            int found = 0, guards = 0, kinds[2] = {}, stages_n = 0;
            for(uint32_t seed = 1; seed <= 80; ++seed)
                for(int st = F0; st < data::stages_count; ++st)
                {
                    static game g; g.new_run(int(seed % data::classes_count), seed * 7u + 3u); g.start_stage(st);
                    ++stages_n;
                    if(! g.has_secret()) continue;
                    ++found; ++kinds[g.secret_kind];
                    CHECK(data::stages[st].boss < 0 && g.lv.at(g.secret_x, g.secret_y) == tile::wall);
                    CHECK(g.lv.at(g.secret_front_x(), g.secret_front_y()) == tile::floor && connected(g.lv, g.hero.x, g.hero.y) == false);
                    int chest_i = -1;
                    for(int i = 0; i < g.pickups_count; ++i) if(g.pickups[i].type == chest) chest_i = i;
                    CHECK(chest_i >= 0 && g.in_secret(g.pickups[chest_i].x, g.pickups[chest_i].y));
                    CHECK(g.key_holder >= 0 && g.key_holder < g.enemies_count && ! g.in_secret(g.enemies[g.key_holder].x, g.enemies[g.key_holder].y));
                    for(int i = 0; i < g.enemies_count; ++i) if(g.in_secret(g.enemies[i].x, g.enemies[i].y)) { ++guards; CHECK(g.enemies[i].elite >= 0); }
                    // przed otwarciem: magazyn nieosiągalny; po: cała podłoga spójna
                    bool seen[map_h][map_w] = {};
                    std::queue<std::pair<int,int>> q; q.push({g.hero.x, g.hero.y}); seen[g.hero.y][g.hero.x] = true;
                    while(! q.empty()) { auto [x, y] = q.front(); q.pop(); const int dd[4][2] = {{1,0},{-1,0},{0,1},{0,-1}};
                        for(auto& v : dd) { int nx = x + v[0], ny = y + v[1]; if(g.lv.passable(nx, ny) && ! seen[ny][nx]) { seen[ny][nx] = true; q.push({nx, ny}); } } }
                    CHECK(seen[g.secret_front_y()][g.secret_front_x()] && ! seen[g.pickups[chest_i].y][g.pickups[chest_i].x]);
                    game o = g; o.keys = 1; o.hero.x = int8_t(o.secret_front_x()); o.hero.y = int8_t(o.secret_front_y());
                    for(int i = 0; i < o.enemies_count; ++i) if(o.enemies[i].alive && cheb(o.enemies[i].x, o.enemies[i].y, o.hero.x, o.hero.y) <= 1) o.enemies[i].alive = false;
                    CHECK(o.can_open_secret() && o.player_move(o.secret_x - o.hero.x, o.secret_y - o.hero.y));
                    CHECK(o.secret_open && o.keys == 0 && connected(o.lv, o.hero.x, o.hero.y) && o.secrets_found == 1);
                    if(seed <= 6)   // klucz z problemu, wybuch, Operator
                    {
                        game k = g; const int kh = k.key_holder; const int kx = k.enemies[kh].x, ky = k.enemies[kh].y;
                        k.damage_enemy(kh, 30000, false, "test");
                        if(k.enemies[kh].alive || (k.enemies[kh].flags & actor_reviving)) continue;   // wraca raz: klucz przy ostatecznym usunięciu
                        int key_i = -1; for(int i = 0; i < k.pickups_count; ++i) if(k.pickups[i].type == store_key && k.pickups[i].active) key_i = i;
                        CHECK((key_i >= 0 && cheb(k.pickups[key_i].x, k.pickups[key_i].y, kx, ky) <= 1) || k.keys == 1);
                        CHECK(k.key_holder < 0);
                        game b = g; b.blast_secret(b.secret_x, b.secret_y + 1, 1); CHECK(b.secret_open == b.secret_def().breakable);
                        game op = g; op.cls = 8; op.hero.x = int8_t(op.secret_front_x()); op.hero.y = int8_t(op.secret_front_y());
                        CHECK(op.can_open_secret() == op.secret_def().breakable);
                    }
                }
            std::printf("magazyny: %d/%d etapów (strażnik %d, pęknięcie %d, drzwi %d)\n", found, stages_n, guards, kinds[0], kinds[1]);
            CHECK(found > stages_n / 8 && kinds[0] > 0 && kinds[1] > 0 && guards > 0);
            // skrzynia: Respekt, zł, materiały, sprzęt markowy
            game g; arena(g, 0); g.pickups[0] = { 8, 7, uint8_t(chest), true }; g.pickups_count = 1;
            const int r0 = g.respect, c0 = g.cash;
            g.player_move(1, 0);
            CHECK(g.respect == r0 + data::chest_respect && g.cash == c0 + g.income(data::chest_cash) && g.mats[0] == data::chest_mats);
            bool gear = false; for(int s = 0; s < data::gear_slots_count; ++s) gear |= g.equipped[s] == data::chest_gear_min;
            CHECK(gear);
            // bot: cel - klucz, potem magazyn, potem skrzynia
            game t = g; t.pickups[1] = { 3, 3, uint8_t(store_key), true }; t.pickups_count = 2; int gx, gy;
            CHECK(t.bot_goal(gx, gy) && gx == 3 && gy == 3);
        }
    }

    // 49. v0.21.50 cz. 4: podsumowanie budowy (#33), wyzwania tygodnia (#34), fabuła odkrywana z budowami (#35)
    {
        auto str_eq = [](const message& m, const char* s) { return std::strcmp(m.s, s) == 0; };
        // ostatnie ciosy: najnowszy pierwszy, najmocniejszy zapamiętany, źródło z przedrostkiem elity
        {
            game g; arena(g, 1);   // Murarz: SZCZ 0 = bez uniku
            CHECK(g.dodge_pct() == 0 && g.last_hits[0].src < 0 && g.worst_hit.amount == 0);
            int ea = -1, eb = -1;   // bez żywiołu i stanów (inaczej mokry + prąd dopisuje osobny cios)
            for(int e = 0; e < data::enemies_count; ++e)
                if(data::enemies[e].elem == element::none && data::enemies[e].on_hit == status_effect::none && ! data::enemies[e].slam && data::enemies[e].tags == 0) { if(ea < 0) ea = e; else if(eb < 0) eb = e; }
            g.spawn(ea, 8, 7); g.spawn(eb, 6, 7); g.make_elite(1, 0);
            int hp = g.hero.hp;
            g.enemy_strike(0, false); const int d0 = hp - g.hero.hp; hp = g.hero.hp;
            g.enemy_strike(1, true); const int d1 = hp - g.hero.hp; hp = g.hero.hp;
            g.enemy_strike(0, false); const int d2 = hp - g.hero.hp;
            CHECK(d0 > 0 && d1 > 0 && d2 > 0);
            CHECK(g.last_hits[0].src == ea && g.last_hits[0].amount == d2 && g.last_hits[0].kind == uint8_t(recap_kind::melee));
            CHECK(g.last_hits[1].src == eb && g.last_hits[1].amount == d1 && g.last_hits[1].kind == uint8_t(recap_kind::ranged) && g.last_hits[1].elite == 0);
            CHECK(g.last_hits[2].src == ea && g.last_hits[2].amount == d0 && g.last_hits[2].stage == g.stage);
            CHECK(g.worst_hit.amount == imax(d0, imax(d1, d2)));
            message m; game::recap_hit_line(m, g.last_hits[1]);
            message want; want.add(data::elites[0].prefix[data::enemies[eb].gender]).add(" ").add(data::enemies[eb].name).add(": -").add(d1).add(" (z dystansu)");
            CHECK(str_eq(m, want.s));
            g.hero.hp = 1; g.bonus.second_chance = 0;
            g.enemy_strike(0, false);
            CHECK(g.st == status::dead);
            message k; g.recap_killer(k);
            message kw; kw.add(data::recap_verbs[data::enemies[ea].gender]).add(" Cię: ").add(data::enemies[ea].name);
            CHECK(str_eq(k, kw.s));
            message w; g.recap_where(w); CHECK(std::strncmp(w.s, "1/", 2) == 0 && std::strstr(w.s, ", Akt ") != nullptr);
            // cios bossa (nazwa uderzenia) i wybuch (źródło: problem wybuchowy)
            game b; arena(b, 1);
            int ins = -1; for(int e = 0; e < data::enemies_count; ++e) if(data::enemies[e].slam && data::enemies[e].slam_name[0]) ins = e;
            b.spawn(ins, 12, 12); b.boss = 0; b.slam_timer = 1; b.slam_x = b.hero.x; b.slam_y = b.hero.y;
            b.enemies[0].stun = 5; b.end_turn();
            if(b.last_hits[0].amount > 0)
            {
                CHECK(b.last_hits[0].kind == uint8_t(recap_kind::slam) && b.last_hits[0].src == ins);
                message sm; game::recap_hit_line(sm, b.last_hits[0]); CHECK(std::strstr(sm.s, data::enemies[ins].slam_name) != nullptr);
            }
            game x; arena(x, 1);
            int boom = -1; for(int e = 0; e < data::enemies_count; ++e) if(data::enemies[e].tags & tag_explodes) boom = e;
            x.arm_blast(8, 7, data::enemies[boom]); x.blast_timer = 1; x.end_turn();
            CHECK(x.last_hits[0].kind == uint8_t(recap_kind::blast) && x.last_hits[0].src == boom && x.last_hits[0].amount > 0);
        }
        // oś czasu: dni i usunięte jak w harmonogramie, premie i wydarzenia z etapu, etap porażki na końcu
        {
            int checked = 0;
            for(uint32_t seed = 1; seed <= 40; ++seed)
            {
                game g; g.new_run(int(seed % data::classes_count), seed * 977u);
                for(int step = 0; step < 4000; ++step)
                {
                    if(g.st == status::stage_clear) { bot_next(g); continue; }
                    if(g.st != status::playing) break;
                    bot_step(g);
                }
                int sum = 0;
                for(int s = g.first_stage; s <= g.stage; ++s) sum += g.recap_kills(s);
                CHECK(sum == g.kills);
                for(int s = g.first_stage; s < g.stage; ++s) CHECK(g.recap_days(s) == schedule_days(g, s));
                recap_line lines[64]; const int n = g.recap_timeline(lines, 64);
                int stage_rows = 0, boon_rows = 0, sms_rows = 0;
                for(int i = 0; i < n; ++i)
                {
                    stage_rows += lines[i].text.s[0] != ' ';
                    boon_rows += std::strncmp(lines[i].text.s, "  Premia: ", 10) == 0;
                    sms_rows += std::strncmp(lines[i].text.s, "  SMS: ", 7) == 0;
                }
                CHECK(stage_rows == g.stage - g.first_stage + 1);
                int boons_n = 0, events_n = 0;
                for(int s = 0; s < max_stages; ++s) { boons_n += g.stage_boon[s] >= 0; events_n += g.stage_event_log[s] != 255; }
                CHECK(boon_rows == boons_n && sms_rows == events_n);
                CHECK(std::strncmp(lines[0].text.s, "1. ", 3) == 0 && std::strstr(lines[0].text.s, data::stages[g.first_stage].name) != nullptr);
                if(g.st == status::dead) { CHECK(lines[n - 1].ink == bad && g.last_hits[0].amount > 0 && g.last_hits[0].stage == g.stage); ++checked; }
                CHECK(g.worst_hit.amount >= g.last_hits[0].amount && g.best_hit > 0 && g.best_hit_def >= 0);
                if(g.secrets_found) { bool f = false; for(int s = 0; s < max_stages; ++s) f |= (g.stage_flags[s] & recap_secret) != 0; CHECK(f); }
            }
            CHECK(checked > 5);
            // NG+: oś czasu od nowa
            game w; w.new_run(1, 5); w.stage_boon[w.stage] = 3; w.st = status::won; w.new_game_plus(); CHECK(w.stage_boon[w.first_stage] == -1);
        }
        // rada i najbliższy cel
        {
            game g; arena(g, 1); g.spawn(0, 8, 7); g.hero.hp = 1; g.thermos = 1; g.enemy_strike(0, false);
            CHECK(g.st == status::dead && data::recap_tips[recap_tip_index(g)].when == recap_tip::coffee);
            g.thermos = 0; CHECK(data::recap_tips[recap_tip_index(g)].when == recap_tip::no_combo);
            g.combos_run = 1; CHECK(data::recap_tips[recap_tip_index(g)].when == recap_tip::any);
            g.last_hits[0].kind = uint8_t(recap_kind::shock); CHECK(data::recap_tips[recap_tip_index(g)].when == recap_tip::shock);
            g.st = status::won; CHECK(data::recap_tips[recap_tip_index(g)].when == recap_tip::won);
            profile p; profile_reset(p); message lead, name;
            CHECK(recap_goal(p, lead, name) && std::strncmp(lead.s, "Jeszcze ", 8) == 0 && std::strstr(name.s, " I") != nullptr);
            p.respect = 9999; message l2, n2; CHECK(recap_goal(p, l2, n2) && std::strncmp(l2.s, "Stać Cię", 9) == 0);
            full_respect(p);
            message l3, n3; CHECK(recap_goal(p, l3, n3) && std::strstr(l3.s, "dośw.") != nullptr);
        }
        // wyzwanie tygodnia: numer tygodnia od poniedziałku, seed i zasady deterministyczne, zasady działają
        {
            CHECK(weekly_number(data::weekly_epoch[0], data::weekly_epoch[1], data::weekly_epoch[2]) == 1);
            int y, mo, d; civil_from_days(weekly_first_day(1) + 6, y, mo, d); CHECK(weekly_number(y, mo, d) == 1);
            civil_from_days(weekly_first_day(1) + 7, y, mo, d); CHECK(weekly_number(y, mo, d) == 2);
            civil_from_days(weekly_first_day(40) + 3, y, mo, d); CHECK(weekly_number(y, mo, d) == 40);
            CHECK(weekly_seed(40) == weekly_seed(40) && weekly_seed(40) != weekly_seed(41) && weekly_seed(40) != daily_seed(40));
            for(int w = 1; w <= data::weekly_count * 2; ++w)
            {
                static game a, b; start_weekly(a, w); start_weekly(b, w);
                CHECK(std::memcmp(&a, &b, sizeof a) == 0 && a.bonus.weekly == weekly_index(w) && a.weekly_week == w && ! a.daily);
                CHECK(a.cls == weekly_class(w) && a.diff == data::weekly_difficulty);
                const weekly_def& wd = data::weekly[weekly_index(w)];
                for(int r = 0; r < wd.rules_count; ++r)
                {
                    const weekly_rule_def& rd = wd.rules[r];
                    if(rd.rule == weekly_rule::cls) CHECK(a.cls == rd.value);
                    if(rd.rule == weekly_rule::no_shop) CHECK(a.shop_closed());
                    if(rd.rule == weekly_rule::hp_pct) CHECK(a.hero.max_hp == data::classes[a.cls].max_health * (100 + rd.value) / 100
                                                             || a.hero.max_hp == data::classes[a.cls].max_health + data::classes[a.cls].max_health * rd.value / 100);
                    if(rd.rule == weekly_rule::dmg_pct) CHECK(a.bonus.dmg_pct == rd.value);
                    if(rd.rule == weekly_rule::cash) CHECK(a.cash == rd.value);
                    if(rd.rule == weekly_rule::weather)
                        for(int s = a.first_stage; s < data::stages_count; ++s) { a.start_stage(s); CHECK(a.weather == rd.value); }
                    if(rd.rule == weekly_rule::elite_pct)
                    {
                        game n; n.new_run(a.cls, a.run_seed, data::weekly_difficulty);
                        CHECK(a.elite_chance() == n.elite_chance() * rd.value / 100);
                    }
                    if(rd.rule == weekly_rule::no_coffee)
                    {
                        game c = a; c.pickups[0] = { int8_t(c.hero.x + 1), c.hero.y, uint8_t(coffee), true }; c.pickups_count = 1;
                        for(int i = 0; i < c.enemies_count; ++i) c.enemies[i].alive = false;
                        c.lv.t[c.hero.y][c.hero.x + 1] = tile::floor;
                        const int cash0 = c.cash, th0 = c.thermos;
                        c.player_move(1, 0);
                        CHECK(c.cash == cash0 + c.income(data::weekly_coffee_cash) && c.thermos == th0);
                        c.thermos = 1; c.hero.hp = 1; CHECK(! c.player_drink() && c.thermos == 1);
                    }
                }
                game z; z.new_run(a.cls, a.run_seed); CHECK(! z.shop_closed() || z.investor_has(investor_effect::no_shop));
            }
            profile p; profile_reset(p);
            CHECK(weekly_best(p, 5) < 0 && record_weekly(p, 5, 100, false) && weekly_best(p, 5) == 100 && ! weekly_won(p, 5));
            CHECK(! record_weekly(p, 5, 50, true) && weekly_best(p, 5) == 100 && weekly_won(p, 5));
            CHECK(record_weekly(p, 6, 10, false) && record_weekly(p, 7, 10, false) && record_weekly(p, 8, 10, false));
            CHECK(weekly_best(p, 5) < 0 && weekly_best(p, 8) == 10 && p.weekly_runs == 5);   // najstarszy tydzień ustępuje
        }
        // fabuła: wątki z kamieni milowych, raz; nieprzeczytane do otwarcia; ozdoby Osiedla z wygranymi
        {
            profile p; profile_reset(p);
            CHECK(story_check(p, nullptr) == 0 && story_count(p) == 0 && estate_decor(p) == 0);
            auto idx = [](story_trigger t, int v) { for(int i = 0; i < data::story_arc_count; ++i) if(data::story_arc[i].trigger == t && data::story_arc[i].value == v) return i; return -1; };
            p.runs = 1; uint32_t got = story_check(p, nullptr);
            CHECK(got == (1u << idx(story_trigger::runs, 1)) && story_unread(p, idx(story_trigger::runs, 1)));
            CHECK(story_check(p, nullptr) == 0);
            story_mark_read(p, idx(story_trigger::runs, 1)); CHECK(story_unread_count(p) == 0 && story_count(p) == 1);
            game g; g.new_run(1, 3); g.elites_killed = 1; g.secrets_found = 1;
            got = story_check(p, &g);
            CHECK(((got >> idx(story_trigger::elite, 0)) & 1) && ((got >> idx(story_trigger::secret, 0)) & 1));
            p.wins = 3; catalog_add(p, data::stages[data::stages_count - 1].boss);
            got = story_check(p, nullptr);
            CHECK(((got >> idx(story_trigger::wins, 1)) & 1) && ((got >> idx(story_trigger::wins, 3)) & 1) && ! ((got >> idx(story_trigger::wins, 5)) & 1));
            CHECK((got >> idx(story_trigger::boss, data::stages[data::stages_count - 1].boss)) & 1);
            CHECK(estate_decor(p) == 2);
            int wd = 0; for(int k = 0; k < data::estate_decor_count; ++k) wd += data::estate_decor[k].inspector == 0;
            p.wins = 100; CHECK(estate_decor(p) == wd);   // v0.21.52 cz. b: reszta ozdób z poziomu inspektora
            p.inspector_xp = 1000000; CHECK(estate_decor(p) == data::estate_decor_count);
        }
        // profil v10 -> v11: stare pola zostają, wyzwania od zera, wątki za dotychczasowe osiągnięcia (jako nowe)
        {
            profile p; profile_reset(p);
            p.best = 777; p.runs = 6; p.wins = 1; p.xp = 55; p.respect = 12; p.tutorial = 5;
            std::memcpy(p.magic, profile_magic_v10, sizeof p.magic);
            std::memset(reinterpret_cast<char*>(&p) + profile_v10_size, 0xAB, sizeof p - profile_v10_size);
            CHECK(profile_fix(p));
            CHECK(std::strcmp(p.magic, profile_magic) == 0 && p.best == 777 && p.runs == 6 && p.wins == 1 && p.xp == 55 && p.respect == 12 + insp_respect(insp_migrated(6, 1, 0)) && p.tutorial == 5);
            CHECK(p.weekly_runs == 0 && p.weekly_week[0] == 0 && p.weekly_score[2] == 0);
            int want = 0; for(int i = 0; i < data::story_arc_count; ++i) want += story_condition(p, nullptr, i);
            CHECK(story_count(p) == want && story_unread_count(p) == want && want >= 3);   // 1 i 5 budów, 1 wygrana
            CHECK(! profile_fix(p));
            profile q; std::memset(&q, 0, sizeof q); std::memcpy(q.magic, profile_magic_v9, sizeof q.magic); q.runs = 2;
            CHECK(profile_fix(q) && std::strcmp(q.magic, profile_magic) == 0 && story_count(q) == 1);
        }
        // wyzwania tygodnia nie są niemożliwe: bot na każdej zasadzie (tabela w CHANGELOG)
        if(! std::getenv("PB_NO_BALANCE"))
        {
            std::printf("Wyzwania tygodnia (bot, 100 przebiegów na zawód albo 900 jednym zawodem):\n");
            for(int wi = 0; wi < data::weekly_count; ++wi)
            {
                const int week = wi + 1;
                const bool one = weekly_rule_value(wi, weekly_rule::cls, -1) >= 0;
                int wins = 0, runs = 0; long drinks = 0;
                for(int c = 0; c < data::open_classes_count; ++c)   // bez zawodów z sekretów (wyniki jak w poprzednich wersjach)
                    for(int k = 0; k < 100; ++k)
                    {
                        const int cls = one ? weekly_class(week) : c;
                        run_mods m = weekly_mods(week);
                        m.hp = data::classes[cls].max_health * weekly_rule_value(wi, weekly_rule::hp_pct, 0) / 100;
                        game g; g.new_run(cls, 1000 + uint32_t(k * data::open_classes_count + c) * 7919u, data::weekly_difficulty, m);
                        bot_drinks = 0;
                        for(int step = 0; step < 4000; ++step)
                        {
                            if(g.st == status::stage_clear) { bot_next(g); continue; }
                            if(g.st != status::playing) break;
                            bot_step(g);
                        }
                        wins += g.st == status::won; ++runs; drinks += bot_drinks;
                    }
                std::printf("  %-28s %3d%% (kawa %.2f/budowę)\n", data::weekly[wi].name, wins * 100 / runs, double(drinks) / runs);
                CHECK(wins * 100 / runs >= 5);   // do przejścia (cel: 5-40%)
            }
        }
    }

    // 52. v0.21.51 cz. 2: sekretne zlecenia (#39) - warunki, nagrody, migracja v11 -> v12, nowe zawody i narzędzia
    {
        auto secret_idx = [](secret_kind k) { for(int i = 0; i < data::secrets_count; ++i) if(data::secrets[i].kind == k) return i; return -1; };
        auto class_idx = [](ability_effect e) { for(int c = 0; c < data::classes_count; ++c) if(data::classes[c].ability == e) return c; return -1; };
        auto tool_of = [](bool knock) { for(int t = 0; t < data::tools_count; ++t) if(data::tools[t].secret && data::weapons[data::tools[t].weapon].knockback == knock) return t; return -1; };
        const int spawacz = class_idx(ability_effect::weld), geodeta = class_idx(ability_effect::mark), majster = class_idx(ability_effect::borrow);
        CHECK(spawacz >= data::open_classes_count && geodeta >= data::open_classes_count && majster >= data::open_classes_count);
        CHECK(data::classes_count == 12 && data::open_classes_count == 9 && data::secrets_count == 8);
        for(int k = 0; k < 8; ++k) CHECK(secret_idx(secret_kind(k)) >= 0);
        // nowy profil: nic z sekretów, zawody i narzędzia zablokowane, nie do kupienia
        profile p; profile_reset(p); p.xp = 1 << 20; p.respect = 60000;
        for(int c = data::open_classes_count; c < data::classes_count; ++c) CHECK(! class_unlocked(p, c) && ! buy_class(p, c) && class_reward(c) && class_secret(c));
        const int zenka = tool_of(true), poz = tool_of(false);
        CHECK(zenka >= 0 && poz >= 0 && ! tool_unlocked(p, zenka) && ! buy_tool(p, zenka) && ! ((mods(p).tools >> poz) & 1));
        const int vet = [] { for(int i = 0; i < data::respect_count; ++i) if(data::respect[i].effect == respect_effect::veteran) return i; return -1; }();
        CHECK(vet >= 16 && ! respect_unlocked(p, vet) && ! buy_respect(p, vet) && respect_rank(p, vet) == 0);
        CHECK(! cosmetic_unlocked(p, data::cosmetic_gold) && ! cosmetic_on(p, data::cosmetic_stripes));
        { int kind = -1, idx = -1; next_unlock(p, kind, idx); CHECK(! (kind == 1 && class_secret(idx))); }
        // budowa dnia: zawody bez sekretów
        for(int d = 1; d <= 200; ++d) CHECK(daily_class(daily_seed(d)) < data::open_classes_count);

        // 1. wygrana bez kawy (Spawacz); kawa z termosu, pełny termos i Hurtownia liczą się jako wypita
        {
            game g; g.new_run(1, 11); g.st = status::won;
            const int i = secret_idx(secret_kind::no_coffee_win);
            CHECK(secret_condition(p, &g, i));
            g.st = status::playing; g.hero.hp = 3; g.thermos = 1; CHECK(g.player_drink() && g.coffee_drunk == 1);
            g.st = status::won; CHECK(! secret_condition(p, &g, i));
            game h; h.new_run(1, 11); h.cash = 999;
            int heal = -1; for(int k = 0; k < data::hurtownia_count; ++k) if(data::hurtownia[k].effect == shop_effect::heal) heal = k;
            h.hero.hp = 1; CHECK(heal >= 0 && h.hurtownia_buy(heal) && h.coffee_drunk == 1);
            game n; n.new_run(1, 11); CHECK(! secret_condition(p, &n, i));   // bez wygranej nic
            profile q; profile_reset(q); CHECK(! ((check_secrets(q, &g) >> i) & 1) && ! class_unlocked(q, spawacz));
            g.coffee_drunk = 0; CHECK(check_secrets(q, &g) == (1 << i) && secret_done(q, i) && class_unlocked(q, spawacz));
            CHECK(check_secrets(q, &g) == 0);   // raz
        }
        // 2. Termin pokonany ciosem brygady (Młot Zenka): pompa dobija bossa ostatniego etapu
        {
            run_mods m; m.helpers = (1 << data::brigade_count) - 1;
            game g; arena(g, 0);
            g.new_run(0, 77, data::default_difficulty, m);
            for(auto& row : g.lv.t) for(auto& c : row) c = tile::wall;
            for(int y = 1; y <= 14; ++y) for(int x = 1; x <= 14; ++x) g.lv.t[y][x] = tile::floor;
            g.enemies_count = 0; g.pickups_count = 0; g.stairs_x = g.stairs_y = -1; g.weather = 0;
            g.hero.x = 7; g.hero.y = 7; g.stage = data::stages_count - 1; g.update_fov();
            g.spawn(data::secret_helper_boss, 8, 7); g.boss = 0; g.enemies[0].hp = 2; g.cash = 500;
            int pump = -1; for(int h = 0; h < data::brigade_count; ++h) if(data::brigade[h].effect == helper_effect::pump) pump = h;
            CHECK(pump >= 0 && g.call_helper(pump));
            CHECK(! g.enemies[0].alive && g.st == status::won && (g.secret_flags & game::secret_helper_boss));
            profile q; profile_reset(q); const int i = secret_idx(secret_kind::helper_boss);
            CHECK((check_secrets(q, &g) >> i) & 1);
            CHECK(tool_unlocked(q, zenka) && (mods(q).tools >> zenka) & 1 && ! tool_unlocked(q, poz));
            game b; arena(b, 0); b.stage = data::stages_count - 1; b.spawn(data::secret_helper_boss, 8, 7); b.boss = 0; b.enemies[0].hp = 2;
            b.hero_attack(0); CHECK(b.st == status::won && ! (b.secret_flags & game::secret_helper_boss));   // cios bohatera się nie liczy
        }
        // 3. 5 magazynów w budowie (Poziomica mistrza: kryt +, magazyn na podglądzie mapy)
        {
            game g; g.new_run(1, 5); const int i = secret_idx(secret_kind::storerooms);
            g.secrets_found = uint8_t(data::secrets[i].value - 1); CHECK(! secret_condition(p, &g, i));
            g.secrets_found = uint8_t(data::secrets[i].value); CHECK(secret_condition(p, &g, i));
            const weapon_def& w = data::weapons[data::tools[poz].weapon];
            CHECK(w.reveal && w.crit > 0);
            game t; t.new_run(1, 5); int c0 = t.crit_pct(); t.take_tool(poz); CHECK(t.crit_pct() == c0 + w.crit && t.weapon().reveal);
            CHECK(t.weapon_breakdown().crit_pct == t.crit_pct());
        }
        // 4. wygrana każdym z 9 zawodów (Majster) - też z migracji v11 -> v12 (dane w profilu)
        {
            const int i = secret_idx(secret_kind::class_wins);
            profile q; profile_reset(q);
            for(int c = 0; c < data::open_classes_count - 1; ++c) set_class_won(q, c);
            CHECK(! secret_condition(q, nullptr, i));
            set_class_won(q, data::open_classes_count - 1); CHECK(secret_condition(q, nullptr, i));
            profile v11 = q; std::memcpy(v11.magic, profile_magic_v11, sizeof v11.magic);
            std::memset(reinterpret_cast<char*>(&v11) + profile_v11_size, 0xCD, sizeof v11 - profile_v11_size);
            v11.best = 4321; v11.wins = 9; v11.story = 7; v11.tutorial = 0xFFFF;   // samouczek obejrzany
            CHECK(profile_fix(v11) && std::strcmp(v11.magic, profile_magic) == 0 && v11.best == 4321 && (v11.story & 7) == 7);
            CHECK(v11.secrets == (1 << i) && v11.secrets_new == (1 << i) && v11.cosmetic == 0 && v11.respect_ranks_hi[0] == 0);
            CHECK(class_unlocked(v11, majster) && ! class_unlocked(v11, spawacz));
            int cls = -1; CHECK(pending_unlock(v11, 0, cls) == unlock_secret && cls == i);
            mark_unlock(v11, unlock_secret, cls); CHECK(v11.secrets_new == 0 && pending_unlock(v11, 0, cls) == -1);
            CHECK(pending_unlock(v11, 1, cls) == unlock_class && cls == majster);   // dymek nowego zawodu na wyborze zawodu
            CHECK(! profile_fix(v11));
            profile old; profile_reset(old); std::memcpy(old.magic, profile_magic_v11, sizeof old.magic);   // bez wygranych: nic
            CHECK(profile_fix(old) && old.secrets == 0);
            profile v10; profile_reset(v10); std::memcpy(v10.magic, profile_magic_v10, sizeof v10.magic);
            for(int c = 0; c < data::open_classes_count; ++c) set_class_won(v10, c);
            CHECK(profile_fix(v10) && secret_done(v10, i));   // starsze profile też przez migrate_v12
            // Pełny zespół (odznaka) nadal tylko zwykłe zawody
            game w; w.new_run(0, 3); w.st = status::won; profile b; profile_reset(b);
            for(int c = 0; c < data::open_classes_count; ++c) set_class_won(b, c);
            check_badges(b, w); CHECK(b.badges & (1u << data::badge_pelny_zespol));
        }
        // 5. Akt 0 bez ciosu od papierów (Geodeta)
        {
            const int i = secret_idx(secret_kind::paper_clean);
            run_mods m; m.act0 = 1;
            game g; g.new_run(1, 21, data::default_difficulty, m); CHECK(g.first_stage == 0 && g.stage == 0);
            g.debug_skip(); CHECK(g.st == status::stage_clear); g.next_stage(); CHECK(g.stage == data::prelude_stages - 1);
            game h = g;
            g.debug_skip(); CHECK(g.st == status::stage_clear && (g.secret_flags & game::secret_paper_clean) && secret_condition(p, &g, i));
            h.log_hit(data::enemy_podpis, -1, recap_kind::melee, 3); CHECK(h.paper_hits == 1);
            h.debug_skip(); CHECK(! (h.secret_flags & game::secret_paper_clean));
            game r = g; r.paper_hits = 0; r.log_hit(data::enemy_rura, -1, recap_kind::melee, 3); CHECK(r.paper_hits == 0);   // rura to nie papier
            game f; f.new_run(1, 21); f.log_hit(data::enemy_papierologia, -1, recap_kind::melee, 3); CHECK(f.paper_hits == 0);   // poza Aktem 0
            profile q; profile_reset(q); CHECK((check_secrets(q, &g) >> i) & 1); CHECK(class_unlocked(q, geodeta));
        }
        // 6. 20x mokry + prąd w budowie (Złota kielnia)
        {
            const int i = secret_idx(secret_kind::shock_combos);
            game g; arena(g, 3);
            g.spawn(data::enemy_przeciek, 8, 7); g.enemies[0].hp = g.enemies[0].max_hp = 500;
            for(int k = 0; k < data::secrets[i].value; ++k) { CHECK(! secret_condition(p, &g, i)); g.hero_attack(0); }
            CHECK(g.shock_combos == data::secrets[i].value && secret_condition(p, &g, i));
            profile q; profile_reset(q); check_secrets(q, &g);
            CHECK(cosmetic_unlocked(q, data::cosmetic_gold) && cosmetic_on(q, data::cosmetic_gold));   // złota kielnia: zawsze po odblokowaniu
        }
        // 7. wygrana na 1-3 HP (Kask w paski - wybór na ekranie zawodu)
        {
            const int i = secret_idx(secret_kind::low_hp_win);
            game g; g.new_run(1, 5); g.st = status::won; g.hero.hp = 4; CHECK(! secret_condition(p, &g, i));
            g.hero.hp = 3; CHECK(secret_condition(p, &g, i)); g.hero.hp = 1; CHECK(secret_condition(p, &g, i));
            profile q; profile_reset(q); toggle_cosmetic(q, data::cosmetic_stripes); CHECK(q.cosmetic == 0);   // zablokowany
            check_secrets(q, &g); CHECK(cosmetic_unlocked(q, data::cosmetic_stripes) && ! cosmetic_on(q, data::cosmetic_stripes));
            toggle_cosmetic(q, data::cosmetic_stripes); CHECK(cosmetic_on(q, data::cosmetic_stripes));
            toggle_cosmetic(q, data::cosmetic_stripes); CHECK(! cosmetic_on(q, data::cosmetic_stripes));
        }
        // 8. szybka wygrana (Zaprawiony w boju: Respekt - kawa w termosie na start); dni bez Aktu 0
        {
            const int i = secret_idx(secret_kind::fast_win);
            game g; g.new_run(1, 5); g.st = status::won;
            for(int s2 = 0; s2 < data::stages_count; ++s2) g.stage_days[s2] = 0;
            const int n = data::stages_count - data::prelude_stages;
            CHECK(g.build_days() == n * data::schedule_min_days);
            g.stage_days[0] = 3000; CHECK(g.build_days() == n * data::schedule_min_days);   // Akt 0 się nie liczy
            CHECK(secret_condition(p, &g, i) == (n * data::schedule_min_days <= data::secrets[i].value));
            g.stage_days[F0] = uint16_t((data::secrets[i].value + 1) * data::schedule_turns_per_day); CHECK(! secret_condition(p, &g, i));
            profile q; profile_reset(q); q.respect = 500;
            g.stage_days[F0] = 0; check_secrets(q, &g);
            CHECK(respect_unlocked(q, vet) && buy_respect(q, vet) && respect_rank(q, vet) == 1 && q.respect_ranks_hi[vet - 16] == 1);
            game s; s.new_run(1, 5, data::default_difficulty, mods(q)); CHECK(s.thermos == data::respect[vet].values[0]);
            message l; respect_label(l, respect_effect::veteran, 2); CHECK(std::strstr(l.s, "kawy") != nullptr);
        }
        // nowe zawody: Spaw (iskry w linii i dym -> wybuch pyłu), Tyczenie (+ do ciosu w cel), Geodeta widzi plac, Majster pożycza moc
        {
            game g; arena(g, spawacz);
            CHECK(g.weapon().elem == element::spark && g.hit_spark());
            g.spawn(data::enemy_kornik, 9, 7); g.spawn(data::enemy_kornik, 10, 7); g.spawn(data::enemy_kornik, 11, 7);
            for(int k = 0; k < 3; ++k) { g.enemies[k].hp = g.enemies[k].max_hp = 500; g.enemies[k].stun = 9; }
            CHECK(g.player_ability());
            CHECK(g.enemies[0].hp < 500 && g.enemies[1].hp < 500 && g.enemies[2].hp == 500);   // Spaw I: 3 pola
            CHECK(g.enemy_dusty(0) && g.enemy_dusty(1) && ! g.enemy_dusty(2) && g.ability_cd > 0);
            g.combo_events = 0; g.hero_attack(0); CHECK(g.combo_events & 2);                   // iskra w dymie: wybuch pyłu
            game t; arena(t, geodeta);
            t.spawn(data::enemy_kornik, 8, 7); t.enemies[0].hp = t.enemies[0].max_hp = 500;
            const int thp = t.hero.hp;
            CHECK(t.mark_bonus(0) == 0 && t.player_ability() && t.mark_target == 0 && t.mark_turns > 0 && t.hero.hp == thp);   // ogłuszony nie bije
            CHECK(t.mark_bonus(0) == 1 + t.ability_rank());
            int turns = t.mark_turns; for(int k = 0; k < turns; ++k) t.player_wait();
            CHECK(t.mark_turns == 0 && t.mark_target == -1);
            game a; a.new_run(geodeta, 31); game b; b.new_run(0, 31);
            int ea = 0, eb = 0; for(int y = 0; y < map_h; ++y) for(int x = 0; x < map_w; ++x) { ea += a.explored(x, y); eb += b.explored(x, y); }
            CHECK(ea > eb && a.explored(a.stairs_x, a.stairs_y));
            run_mods m0; m0.act0 = 1; game d; d.new_run(geodeta, 31, data::default_difficulty, m0);
            for(int k = 0; k < d.pickups_count; ++k) if(d.pickups[k].type == document) CHECK(d.explored(d.pickups[k].x, d.pickups[k].y));
            int seen = 0;
            for(uint32_t seed = 1; seed <= 40; ++seed)
            {
                game mj; mj.new_run(majster, seed); game mj2; mj2.new_run(majster, seed);
                CHECK(mj.borrow_cls >= 0 && mj.borrow_cls < data::open_classes_count && mj.borrow_cls == mj2.borrow_cls);
                CHECK(mj.power_cls() == mj.borrow_cls && &mj.pdef() == &data::classes[mj.borrow_cls]);
                seen |= 1 << mj.borrow_cls;
                int prev = mj.borrow_cls; mj.start_stage(mj.stage + 1); CHECK(mj.borrow_cls >= 0);
                (void)prev;
            }
            CHECK(seen == (1 << data::open_classes_count) - 1);   // każdy fach się trafia
            game o; o.new_run(0, 3); CHECK(o.borrow_cls == -1 && o.power_cls() == 0);
            game mh; arena(mh, majster); mh.borrow_cls = int8_t(class_idx(ability_effect::flush)); mh.hero.hp = 5;
            CHECK(mh.player_ability() && mh.hero.hp > 5 && mh.ability_cd == mh.ability_cooldown());   // Zawór Hydraulika
            CHECK(mh.ability_cooldown() <= data::classes[mh.borrow_cls].ability_cooldown);
        }
        // Młot Zenka: cios wręcz odpycha (bossa nie)
        {
            game g; arena(g, 1); g.take_tool(zenka); CHECK(g.weapon().knockback);
            g.spawn(data::enemy_kornik, 8, 7); g.enemies[0].hp = g.enemies[0].max_hp = 500;
            g.hero_attack(0); CHECK(g.enemies[0].x == 9);
            g.spawn(data::enemy_termin, 7, 8); g.boss = 1; g.enemies[1].hp = g.enemies[1].max_hp = 500;
            g.hero_attack(1); CHECK(g.enemies[1].x == 7 && g.enemies[1].y == 8);
            profile q; profile_reset(q); q.tools_found = 0xFF; game w; w.new_run(0, 3); w.tools_found = uint8_t((1 << 8) - 1);
            q.tools_found = w.tools_found; check_badges(q, w); CHECK(q.badges & (1u << data::badge_kolekcjoner));   // sekretne się nie liczą
        }
        // zapis budowy: nowe pola w PBRUN14
        {
            game g; g.new_run(majster, 9); g.coffee_drunk = 2; g.shock_combos = 7; g.secret_flags = 3;
            run_save* sv = new run_save(); run_save_make(*sv, g);
            CHECK(run_save_valid(*sv) && sv->g.shock_combos == 7 && sv->g.borrow_cls == g.borrow_cls && std::memcmp(sv->magic, "PBRUN14", 7) == 0);
            delete sv;
        }
    }

    // 53. v0.21.52 cz. a: tempo postępu - Szkolenia z poziomami, ceny rosnące z zakupem, tytuły i kolory kasku, profil v13
    {
        // Szkolenia: 4-5 poziomów, cena rośnie, główne działanie wśród poziomów, opis poziomu i sumy
        for(int i = 0; i < data::upgrades_count; ++i)
        {
            const upgrade_def& u = data::upgrades[i];
            CHECK(u.levels >= 4 && u.levels <= 5);
            for(int l = 1; l < u.levels; ++l) CHECK(u.costs[l] > u.costs[l - 1]);
            CHECK(upgrade_total(i, u.levels, u.effect) > 0);
            for(int l = 0; l < u.levels; ++l) { message m; upgrade_label(m, u.steps[l].effect, u.steps[l].value); CHECK(m.n > 3 && m.n < 30); }
            message all; upgrade_summary(all, i, u.levels); CHECK(all.n > 3 && all.n < log_len - 1);   // bez obcięcia
            message none; upgrade_summary(none, i, 0); CHECK(none.n == 0);
        }
        // pełne Szkolenia: co najmniej to, co dawały stare (v0.21.51: +4 HP, -2%, +2%, kawa +1, +1 znajdźka, +1 SZCZ, +1 stat. broni)
        {
            profile f; profile_reset(f);
            for(int i = 0; i < data::upgrades_count; ++i) f.levels[i] = uint8_t(data::upgrades[i].levels);
            run_mods m = mods(f);
            CHECK(m.hp >= 4 && m.taken_pct >= 2 && m.dmg_pct >= 2 && m.coffee >= 1 && m.pickups >= 1 && m.luck >= 1 && m.craft >= 1);
            CHECK(m.hp <= 6 && m.taken_pct <= 5 && m.dmg_pct <= 5 && m.luck <= 1 && m.craft <= 1 && m.pickups <= 1);   // bez dużego wzrostu mocy
            // rozpiska obrażeń: część Szkoleń = premie bojowe z mods()
            run_mods s0 = mods_part(f, 0);
            CHECK(s0.hp == m.hp && s0.dmg_pct == m.dmg_pct && s0.taken_pct == m.taken_pct && s0.crit == m.crit && s0.dodge == m.dodge && s0.luck == m.luck);
        }
        // zawody i narzędzia drożeją z każdym zakupem; Trudny 100
        {
            profile p; profile_reset(p); p.xp = 100000;
            int prev = 0, bought = 0;
            for(int c = 0; c < data::classes_count; ++c)
            {
                if(! class_for_sale(c)) { CHECK(! buy_class(p, c) || class_unlocked(p, c)); continue; }
                const int price = class_cost(p), xp0 = p.xp;
                CHECK(price == data::class_costs[imin(bought, data::class_costs_count - 1)] && price > prev);
                CHECK(buy_class(p, c) && p.xp == xp0 - price && classes_bought(p) == ++bought);
                prev = price;
            }
            CHECK(bought == data::class_costs_count && class_cost(p) == data::class_costs[data::class_costs_count - 1]);
            prev = 0; bought = 0;
            for(int i = 0; i < data::tools_count; ++i)
            {
                if(! data::tools[i].shop) { CHECK(! buy_tool(p, i)); continue; }
                const int price = tool_cost(p), xp0 = p.xp;
                CHECK(price == data::tool_costs[bought] && price > prev && buy_tool(p, i) && p.xp == xp0 - price && tools_bought(p) == ++bought);
                prev = price;
            }
            CHECK(bought == data::tool_costs_count && data::hard_cost == 100);
            for(int i = 0; i < data::upgrades_count; ++i) while(buy_upgrade(p, i)) {}
            for(int i = 0; i < data::brigade_count; ++i) buy_helper(p, i);
            buy_hard(p);
            CHECK(shop_spent(p) == shop_total_cost() && p.xp == 100000 - shop_total_cost());
            int kind = -1, idx = -1; CHECK(next_unlock(p, kind, idx) < 0);
            // najbliższy cel bierze cenę rosnącą
            profile q; profile_reset(q); q.xp = 100000;
            for(int c = 0; c < data::classes_count; ++c) if(class_for_sale(c)) { buy_class(q, c); break; }
            for(int i = 0; i < data::upgrades_count; ++i) while(buy_upgrade(q, i)) {}
            for(int i = 0; i < data::tools_count; ++i) buy_tool(q, i);
            for(int i = 0; i < data::brigade_count; ++i) buy_helper(q, i);
            buy_hard(q);
            CHECK(next_unlock(q, kind, idx) == data::class_costs[1] && kind == 1);
        }
        // odznaki i zlecenia: mało doświadczenia, w zamian tytuł (i czasem kolor kasku)
        int helmets_from = 0;
        for(int i = 0; i < data::badges_count; ++i) { CHECK(data::badges[i].xp <= 15 && data::badges[i].title[0] != 0); helmets_from += data::badges[i].cosmetic >= 0; }
        for(int i = 0; i < data::contracts_count; ++i) { CHECK(data::contracts[i].xp <= 15 && data::contracts[i].title[0] != 0); helmets_from += data::contracts[i].cosmetic >= 0; }
        for(int l = 0; l < data::inspector_levels_count; ++l) helmets_from += data::inspector_levels[l].reward == progress_reward::helmet;   // cz. b
        for(int l = 0; l < data::stake_ranks_count; ++l) helmets_from += data::stake_ranks[l].reward == progress_reward::helmet;
        for(int l = 0; l < data::mastery_levels_count; ++l) helmets_from += data::mastery_levels[l].reward == progress_reward::helmet;
        int helmets = 0; for(int k = 0; k < data::cosmetics_count; ++k) helmets += cosmetic_helmet(k);
        CHECK(helmets >= 3 && helmets_from == helmets && ! cosmetic_helmet(data::cosmetic_stripes) && ! cosmetic_helmet(data::cosmetic_gold));
        // tytuły: tylko zdobyte, wybór w kółko z "bez tytułu"
        {
            profile p; profile_reset(p);
            CHECK(selected_title(p) == -1 && titles_owned(p) == 0);
            cycle_title(p, 1); CHECK(p.title == 0);   // nic do wyboru
            p.badges = uint16_t(1 << data::badge_seryjny); p.contracts = 1 << 1;
            cycle_title(p, 1); CHECK(selected_title(p) == data::badge_seryjny && std::strcmp(title_name(selected_title(p)), data::badges[data::badge_seryjny].title) == 0);
            cycle_title(p, 1); CHECK(selected_title(p) == data::badges_count + 1 && std::strcmp(title_name(selected_title(p)), data::contracts[1].title) == 0);
            cycle_title(p, 1); CHECK(p.title == 0 && selected_title(p) == -1);
            cycle_title(p, -1); CHECK(selected_title(p) == data::badges_count + 1);
            p.contracts = 0; CHECK(selected_title(p) == -1);   // utracony (np. inny profil) - bez tytułu
            CHECK(titles_owned(p) == 1);
        }
        // kolory kasku: z odznaki / zlecenia, wybór w kółko, kask w paski osobno
        {
            int bi = -1, k = -1;
            for(int i = 0; i < data::badges_count; ++i) if(data::badges[i].cosmetic >= 0) { bi = i; k = data::badges[i].cosmetic; break; }
            CHECK(bi >= 0 && cosmetic_helmet(k));
            profile p; profile_reset(p);
            CHECK(! cosmetic_unlocked(p, k) && helmet_cosmetic(p) == -1 && helmets_unlocked(p) == 0);
            cycle_helmet(p, 1); CHECK(p.helmet == 0);
            p.helmet = uint8_t(k + 1); CHECK(helmet_cosmetic(p) == -1);   // zablokowany nie działa
            p.helmet = 0; p.badges = uint16_t(1 << bi);
            CHECK(cosmetic_unlocked(p, k) && helmets_unlocked(p) == 1);
            cycle_helmet(p, 1); CHECK(helmet_cosmetic(p) == k);
            cycle_helmet(p, 1); CHECK(helmet_cosmetic(p) == -1 && p.helmet == 0);
            int ci = -1; for(int i = 0; i < data::contracts_count; ++i) if(data::contracts[i].cosmetic >= 0) ci = i;
            CHECK(ci >= 0); p.contracts = uint8_t(1 << ci); CHECK(cosmetic_unlocked(p, data::contracts[ci].cosmetic) && helmets_unlocked(p) == 2);
            toggle_cosmetic(p, k); CHECK(! cosmetic_on(p, k) || k == data::cosmetic_gold);   // kolor kasku to nie przełącznik
        }
        // profil v12 -> v13: Szkolenia wracają po starej cenie (poziomy od zera), reszta zostaje; tytuł i kask od zera
        {
            profile v; profile_reset(v);
            v.xp = 33; v.best = 4444; v.wins = 7; v.classes = 0x3F; v.tools = 0x0E; v.hard = 1; v.brigade = 3; v.secrets = 0x15;
            v.respect = 77; v.respect_ranks_hi[0] = 1;
            int want = 33;
            for(int i = 0; i < data::upgrades_count; ++i) { v.levels[i] = uint8_t(data::upgrades[i].legacy_levels); for(int l = 0; l < data::upgrades[i].legacy_levels; ++l) want += data::upgrades[i].legacy_costs[l]; }
            std::memcpy(v.magic, profile_magic_v12, sizeof v.magic);
            std::memset(reinterpret_cast<char*>(&v) + profile_v12_size, 0xAB, sizeof v - profile_v12_size);
            CHECK(profile_fix(v) && std::strcmp(v.magic, profile_magic) == 0);
            CHECK(v.xp == want && want > 33 && v.best == 4444 && v.wins == 7 && v.classes == 0x3F && v.tools == 0x0E && v.hard == 1);
            CHECK(v.brigade == 3 && v.secrets == 0x15 && v.respect == 77 + insp_respect(insp_migrated(0, 7, 0)) && v.respect_ranks_hi[0] == 1 && v.title == 0 && v.helmet == 0);
            for(int i = 0; i < max_upgrades; ++i) CHECK(v.levels[i] == 0);
            CHECK(! profile_fix(v) && v.xp == want);   // raz
            // stary profil z poziomem ponad stare maksimum: zwrot `refund` za nadmiar
            profile o; profile_reset(o); o.levels[0] = uint8_t(data::upgrades[0].legacy_levels + 1); o.xp = 0;
            std::memcpy(o.magic, profile_magic_v12, sizeof o.magic);
            int w0 = data::upgrades[0].refund; for(int l = 0; l < data::upgrades[0].legacy_levels; ++l) w0 += data::upgrades[0].legacy_costs[l];
            CHECK(profile_fix(o) && o.xp == w0 && o.levels[0] == 0);
            // starsze profile też dostają zwrot (v11 przez tę samą migrację)
            profile e; profile_reset(e); e.levels[1] = 1; e.xp = 5; std::memcpy(e.magic, profile_magic_v11, sizeof e.magic);
            CHECK(profile_fix(e) && e.xp == 5 + data::upgrades[1].legacy_costs[0] && e.levels[1] == 0);
            // bieżący profil: nowe pola przeżywają zapis
            profile n; profile_reset(n); n.title = 3; n.helmet = 2; CHECK(! profile_fix(n) && n.title == 3 && n.helmet == 2);
        }
    }

    // v0.21.52 cz. b: poziom inspektora (#44), mistrzostwo zawodu (#45), stopnie inwestora (#48), profil v14
    {
        // dane: 30-40 poziomów inspektora z rosnącymi progami, każda nagroda co najmniej raz, jeden slot pamiątki
        CHECK(data::inspector_levels_count >= 30 && data::inspector_levels_count <= 40 && data::mastery_levels_count == 10);
        int kinds[9] = {};
        for(int l = 0; l < data::inspector_levels_count; ++l)
        {
            ++kinds[int(data::inspector_levels[l].reward)];
            if(l > 0) CHECK(data::inspector_levels[l].xp >= data::inspector_levels[l - 1].xp);
        }
        for(int k = 0; k <= int(progress_reward::keepsake_slot); ++k) CHECK(kinds[k] >= 1);
        CHECK(kinds[int(progress_reward::keepsake_slot)] == 1);
        CHECK(mastery_reward_level(progress_reward::power) == 3 && mastery_reward_level(progress_reward::weapon) == 5);
        CHECK(mastery_reward_level(progress_reward::boon) == 7 && mastery_reward_level(progress_reward::helmet) == 10);
        CHECK(data::stake_ranks_count == investor_stake((1 << data::investor_count) - 1));   // stopień na każdą stawkę
        // progi: poziom = ile progów mieści się w dośw.; pasek do następnego
        {
            profile p; profile_reset(p);
            int cur = -1, need = -1; inspector_bar(p, cur, need);
            CHECK(inspector_level(p) == 0 && cur == 0 && need == data::inspector_levels[0].xp);
            p.inspector_xp = uint32_t(data::inspector_levels[0].xp - 1); CHECK(inspector_level(p) == 0);
            p.inspector_xp = uint32_t(data::inspector_levels[0].xp + 3); inspector_bar(p, cur, need);
            CHECK(inspector_level(p) == 1 && cur == 3 && need == data::inspector_levels[1].xp);
            p.inspector_xp = uint32_t(progress_floor(data::inspector_levels, data::inspector_levels_count)); inspector_bar(p, cur, need);
            CHECK(inspector_level(p) == data::inspector_levels_count && need == 0 && cur == 0);
            p.mastery_xp[2] = uint16_t(progress_floor(data::mastery_levels, 3)); int mc, mn; mastery_bar(p, 2, mc, mn);
            CHECK(mastery_level(p, 2) == 3 && mastery_level(p, 1) == 0 && mc == 0 && mn == data::mastery_levels[3].xp);
        }
        // dośw. z budowy: budowa, etapy, bossowie, elity, magazyny, wygrane x trudność; znak wodny (NG+, drugi raz nic)
        {
            profile p; profile_reset(p);
            game g; g.new_run(1, 77, 1, mods(p, 1)); start_run(p);
            g.stage = F0 + 4; g.st = status::dead; g.elites_killed = 2; g.secrets_found = 1;
            int bosses = 0; for(int s = F0; s < F0 + 4; ++s) bosses += data::stages[s].boss >= 0;
            const int want = (data::inspector_xp_run + 4 * data::inspector_xp_stage + bosses * data::inspector_xp_boss
                              + 2 * data::inspector_xp_elite + data::inspector_xp_storeroom) * data::inspector_diff_pct[1] / 100;
            CHECK(bosses >= 1 && run_progress_xp(g) == want);
            progress_gain r = bank_progress(p, g);
            CHECK(r.gained == want && int(p.inspector_xp) == want && p.mastery_xp[1] == want && p.mastery_xp[0] == 0 && r.cls == 1);
            CHECK(bank_progress(p, g).gained == 0 && int(p.inspector_xp) == want);   // drugi raz nic
            // wygrana i "Kolejna budowa": druga wygrana dolicza tylko nową część
            profile q; profile_reset(q);
            game w; w.new_run(0, 5, 0, mods(q, 0)); start_run(q);
            w.st = status::won; w.stage = data::stages_count - 1;
            const int all = run_progress_xp(w);
            int allb = 0; for(int s = F0; s < data::stages_count; ++s) allb += data::stages[s].boss >= 0;
            CHECK(all == (data::inspector_xp_run + (data::stages_count - F0) * data::inspector_xp_stage + allb * data::inspector_xp_boss
                          + data::inspector_xp_win) * data::inspector_diff_pct[0] / 100);
            CHECK(bank_progress(q, w).gained == all);
            w.tier = 1; w.st = status::dead; w.stage = F0 + 1;   // po NG+: padł na drugim etapie
            const int ng = bank_progress(q, w).gained;
            CHECK(ng == run_progress_xp(w) - all && ng > 0 && ng < all);   // etap po NG+ (wygrana z tier już policzona)
            CHECK(int(q.inspector_xp) == run_progress_xp(w) && q.run_progress == run_progress_xp(w));
            start_run(q); CHECK(q.run_progress == 0);   // nowa budowa: znak wodny od zera
        }
        // nagrody inspektora: Respekt raz za poziom, tytuły, kolory kasku, wątki SMS, ozdoby, slot pamiątki
        {
            profile p; profile_reset(p);
            int respect = 0, titles = 0, helmets = 0, stories = 0, decor = 0;
            for(int l = 0; l < data::inspector_levels_count; ++l)
            {
                const progress_level& lv = data::inspector_levels[l];
                if(lv.reward == progress_reward::respect) respect += lv.value;
                if(lv.reward == progress_reward::title) { ++titles; CHECK(progress_title_index(0, l + 1) >= 0 && ! title_owned(p, progress_title_index(0, l + 1))); }
                if(lv.reward == progress_reward::helmet) { ++helmets; CHECK(! cosmetic_unlocked(p, lv.index)); }
                if(lv.reward == progress_reward::story) { ++stories; CHECK(data::story_arc[lv.index].trigger == story_trigger::inspector && data::story_arc[lv.index].value == l + 1); }
                if(lv.reward == progress_reward::decor) { ++decor; CHECK(! decor_unlocked(p, lv.index) && data::estate_decor[lv.index].inspector == l + 1); }
            }
            CHECK(! keepsake_slot2(p) && estate_decor(p) == 0);
            // poziom po poziomie (jak budowa po budowie): Respekt dokładnie raz
            for(int l = 0; l < data::inspector_levels_count; ++l)
            {
                progress_gain r; const int r0 = p.respect;
                add_inspector_xp(p, data::inspector_levels[l].xp, r);
                CHECK(r.insp_before == l && r.insp_after == l + 1);
                CHECK(p.respect - r0 == (data::inspector_levels[l].reward == progress_reward::respect ? data::inspector_levels[l].value : 0));
                const bool slot = l + 1 >= inspector_reward_level(progress_reward::keepsake_slot);
                CHECK(keepsake_slot2(p) == slot);
            }
            CHECK(p.respect == respect && p.respect_total == respect && inspector_level(p) == data::inspector_levels_count);
            { progress_gain r; add_inspector_xp(p, 100000, r); CHECK(r.insp_before == r.insp_after && p.respect == respect); }   // maksimum
            int own_t = 0; for(int t = progress_titles_from; t < titles_count; ++t) own_t += title_owned(p, t);
            CHECK(own_t == titles && helmets_unlocked(p) == helmets);
            uint32_t got = story_check(p, nullptr); int got_n = 0; for(int i = 0; i < data::story_arc_count; ++i) got_n += (got >> i) & 1 && data::story_arc[i].trigger == story_trigger::inspector;
            CHECK(got_n == stories);
            int dn = 0; for(int k = 0; k < data::estate_decor_count; ++k) dn += data::estate_decor[k].inspector > 0 && decor_unlocked(p, k);
            CHECK(dn == decor && estate_decor(p) == decor);   // bez wygranych: tylko ozdoby inspektora
            // druga pamiątka: inna niż pierwsza, premia w mods, budowy z nią liczą się do rangi
            p.badges = uint16_t(1 << data::keepsakes[1].badge);   // Kask ojca (z odznaki)
            CHECK(keepsake_unlocked(p, 1) && selected_keepsake(p) == 0);
            cycle_keepsake2(p, 1); CHECK(selected_keepsake2(p) == 1);
            cycle_keepsake2(p, 1); CHECK(selected_keepsake2(p) == -1 && p.keepsake2 == 0);   // bez drugiej (pierwsza pominięta)
            cycle_keepsake2(p, 1);
            profile one = p; one.keepsake2 = 0;
            const run_mods m1 = mods(one), m2 = mods(p);
            p.keepsake_runs[1] = 50;   // ranga III - druga pamiątka i tak na randze I
            const run_mods m3 = mods(p);
            CHECK(m2.def == m1.def + data::keepsakes[1].values[0] && m3.def == m2.def && mods_part(p, 3).def == data::keepsakes[1].values[0]);
            p.keepsake_runs[1] = 0;
            const int kr = p.keepsake_runs[1]; start_run(p); CHECK(p.keepsake_runs[1] == kr + 1);
            p.keepsake = 2; CHECK(selected_keepsake2(p) == -1);   // ta sama co pierwsza - nie działa podwójnie
            profile low; profile_reset(low); low.keepsake2 = 2; low.badges = p.badges; CHECK(selected_keepsake2(low) == -1);   // bez slotu
        }
        // mistrzostwo: wariant mocy (sam się włącza na poziomie 3), broń mistrza (kryt + złoty błysk), premia w ofercie,
        // kask mistrza tylko zawodem z poziomem 10; Respekt raz za poziom
        {
            profile p; profile_reset(p);
            int mresp = 0; for(int l = 0; l < data::mastery_levels_count; ++l) if(data::mastery_levels[l].reward == progress_reward::respect) mresp += data::mastery_levels[l].value;
            for(int l = 0; l < data::mastery_levels_count; ++l) { progress_gain r; add_mastery_xp(p, 1, data::mastery_levels[l].xp, r); CHECK(r.mastery_after == l + 1); }
            CHECK(mastery_level(p, 1) == 10 && p.respect == mresp && power_variant_on(p, 1) && ! power_variant_on(p, 0));
            CHECK(mastery_bits(p, 1) == (mastery_bit_power | mastery_bit_weapon | mastery_bit_boon) && mastery_bits(p, 0) == 0);
            toggle_power_variant(p, 1); CHECK(! power_variant_on(p, 1) && mastery_bits(p, 1) == (mastery_bit_weapon | mastery_bit_boon));
            toggle_power_variant(p, 1); toggle_power_variant(p, 0); CHECK(power_variant_on(p, 1) && ! power_variant_on(p, 0));
            int mk = -1; for(int l = 0; l < data::mastery_levels_count; ++l) if(data::mastery_levels[l].reward == progress_reward::helmet) mk = data::mastery_levels[l].index;
            CHECK(mk >= 0 && mastery_helmet(mk) && cosmetic_unlocked(p, mk));
            p.helmet = uint8_t(mk + 1);
            CHECK(helmet_cosmetic(p, 1) == mk && helmet_cosmetic(p, 0) == -1 && helmet_cosmetic(p) == mk);
            CHECK(mods(p, 1).mastery == mastery_bits(p, 1) && mods(p).mastery == 0);
        }
        // wariant mocy zgodny z walką: siła mocy i tury odnowienia z danych (każdy zawód), Odprawa, Ścianka, Zawór
        for(int c = 0; c < data::classes_count; ++c)
        {
            game a; arena(a, c); game b; arena(b, c); b.bonus.mastery = mastery_bit_power;
            const mastery_class_def& mc = data::mastery_classes[c];
            CHECK(b.boon_power() == a.boon_power() + mc.power);
            CHECK(b.ability_cooldown() == imax(3, a.ability_cooldown() + mc.cooldown) || a.ability_cooldown() + mc.cooldown < 3);
        }
        {
            game a; arena(a, 0); game b; arena(b, 0); b.bonus.mastery = mastery_bit_power;   // Odprawa: ogłusza dłużej
            a.spawn(8, 8, 7); b.spawn(8, 8, 7);
            CHECK(a.player_ability() && b.player_ability() && b.enemies[0].stun == a.enemies[0].stun + data::mastery_classes[0].power);
            CHECK(b.ability_cd == a.ability_cd + data::mastery_classes[0].cooldown);
        }
        {
            game a; arena(a, 1); game b; arena(b, 1); b.bonus.mastery = mastery_bit_power;   // Ścianka: stoi dłużej
            a.spawn(4, 11, 7); b.spawn(4, 11, 7);
            CHECK(a.player_ability() && b.player_ability() && a.walls_count > 0 && b.walls_count == a.walls_count);
            CHECK(b.walls[0].turns == a.walls[0].turns + data::mastery_classes[1].power);
        }
        {
            game a; arena(a, 4); game b; arena(b, 4); b.bonus.mastery = mastery_bit_power;   // Zawór: szybciej, leczy mniej
            a.hero.hp = b.hero.hp = 5;
            CHECK(a.player_ability() && b.player_ability() && b.hero.hp == a.hero.hp + data::mastery_classes[4].power);
            CHECK(b.ability_cd == a.ability_cd + data::mastery_classes[4].cooldown && b.ability_cd < a.ability_cd);
        }
        // broń mistrza: kryt tylko z bronią zawodu, ten sam w rozpisce (wybór zawodu i budowa)
        for(int c = 0; c < data::classes_count; ++c)
        {
            game a; a.new_run(c, 9); game b; run_mods m; m.mastery = mastery_bit_weapon; b.new_run(c, 9, 1, m);
            const int pk = data::mastery_classes[c].weapon_perk.value;
            CHECK(b.crit_pct() == a.crit_pct() + pk && b.master_weapon() && ! a.master_weapon());
            CHECK(b.weapon_breakdown().crit_chance() == b.crit_pct() && class_breakdown(c, m).crit_chance() == b.crit_pct());
            b.weapon_override = data::tools[0].weapon; a.weapon_override = data::tools[0].weapon;
            CHECK(b.crit_pct() == a.crit_pct());
        }
        // premia mistrzostwa: tylko z bitem i tylko swoim zawodem; oferty bez mistrzostwa bez zmian
        {
            int seen = 0, seen_off = 0;
            for(int k = 0; k < 300; ++k)
            {
                run_mods m; m.mastery = mastery_bit_boon;
                game g; g.new_run(5, 100 + k, 1, m); g.roll_boons();
                game h; h.new_run(5, 100 + k, 1); h.roll_boons();
                for(int o = 0; o < 3; ++o)
                {
                    seen += g.boon_offer[o] == data::mastery_classes[5].boon;
                    seen_off += h.boon_offer[o] >= 0 && data::boons[h.boon_offer[o]].mastery;
                    if(g.boon_offer[o] >= 0 && data::boons[g.boon_offer[o]].mastery) CHECK(g.boon_offer[o] == data::mastery_classes[5].boon);
                }
            }
            CHECK(seen > 0 && seen_off == 0);
        }
        // stopnie inwestora: wygrana z wyższą stawką - Respekt za każdy nowy próg, tytuły i kolory kasku od progu
        {
            profile p; profile_reset(p); p.wins = 1;
            int want = 0; for(int l = 0; l < 3; ++l) if(data::stake_ranks[l].reward == progress_reward::respect) want += data::stake_ranks[l].value;
            game g; run_mods m; m.investor = 0; for(int i = 0; i < data::investor_count && investor_stake(m.investor) < 3; ++i) if(investor_stake(m.investor | (1 << i)) <= 3) m.investor |= 1 << i;
            CHECK(investor_stake(m.investor) == 3);
            g.new_run(2, 3, 1, m); g.st = status::won;
            record_run(p, g);
            CHECK(max_stake(p) == 3 && stake_rank(p) == 3 && p.respect == want);
            record_run(p, g); CHECK(p.respect == want);   // ten sam rekord - nic
            for(int l = 0; l < data::stake_ranks_count; ++l)
            {
                const progress_level& sr = data::stake_ranks[l];
                if(sr.reward == progress_reward::title) CHECK(title_owned(p, progress_title_index(1, sr.xp)) == (sr.xp <= 3));
                if(sr.reward == progress_reward::helmet) CHECK(cosmetic_unlocked(p, sr.index) == (sr.xp <= 3));
            }
            game d = g; d.st = status::dead; d.bonus.investor = (1 << data::investor_count) - 1; record_run(p, d);
            CHECK(max_stake(p) == 3);   // porażka nie podnosi stawki
        }
        // profil v13 -> v14: inspektor z budów, wygranych i Respektu, mistrzostwo z domów i wygranych zawodów; reszta zostaje
        {
            profile v; profile_reset(v);
            v.runs = 30; v.wins = 6; v.respect_total = 200; v.respect = 50; v.xp = 77; v.best = 999; v.title = 2; v.helmet = 3;
            v.houses_count = 6; for(int i = 0; i < 6; ++i) v.houses[i] = uint8_t(i < 4 ? 1 : 2);   // 4 domy Murarza, 2 Cieśli
            set_class_won(v, 1); set_class_won(v, 2);
            std::memcpy(v.magic, profile_magic_v13, sizeof v.magic);
            std::memset(reinterpret_cast<char*>(&v) + profile_v13_size, 0xAB, sizeof v - profile_v13_size);
            CHECK(profile_fix(v) && std::strcmp(v.magic, profile_magic) == 0);
            const int insp = 30 * data::inspector_migrate_run + 6 * data::inspector_migrate_win + 200 * data::inspector_migrate_respect_pct / 100;
            CHECK(int(v.inspector_xp) == insp && v.mastery_xp[1] == 4 * data::mastery_migrate_win + data::mastery_migrate_class_win);
            CHECK(v.mastery_xp[2] == 2 * data::mastery_migrate_win + data::mastery_migrate_class_win && v.mastery_xp[0] == 0);
            int lr = 0; for(int l = 0; l < inspector_level(v); ++l) if(data::inspector_levels[l].reward == progress_reward::respect) lr += data::inspector_levels[l].value;
            for(int c = 1; c <= 2; ++c) for(int l = 0; l < mastery_level(v, c); ++l) if(data::mastery_levels[l].reward == progress_reward::respect) lr += data::mastery_levels[l].value;
            CHECK(inspector_level(v) > 0 && v.respect == 50 + lr && v.respect_total == 200 + lr);
            CHECK(v.xp == 77 && v.best == 999 && v.title == 2 && v.helmet == 3 && v.keepsake2 == 0 && v.run_progress == 0);
            CHECK(power_variant_on(v, 1) == (mastery_level(v, 1) >= 3));
            for(int i = 0; i < data::story_arc_count; ++i)
                if(data::story_arc[i].trigger == story_trigger::inspector) CHECK(story_unlocked(v, i) == (inspector_level(v) >= data::story_arc[i].value));
            CHECK(! profile_fix(v) && int(v.inspector_xp) == insp);   // raz
            // v12 i starsze: przez v13 do v14
            profile e; profile_reset(e); e.runs = 4; std::memcpy(e.magic, profile_magic_v12, sizeof e.magic);
            CHECK(profile_fix(e) && int(e.inspector_xp) == 4 * data::inspector_migrate_run);
            profile n; profile_reset(n); n.inspector_xp = 1234; n.mastery_xp[11] = 55; n.keepsake2 = 3; n.power_alt = 0x801;
            CHECK(! profile_fix(n) && n.inspector_xp == 1234 && n.mastery_xp[11] == 55 && n.keepsake2 == 3 && n.power_alt == 0x801);
        }
    }

    if(std::getenv("PB_NO_BALANCE")) { std::printf(fails ? "\n%d FAIL\n" : "\nOK (bez balansu)\n", fails); return fails != 0; }
    // 28. balans: bot gra po 300 runów każdym zawodem na każdym poziomie
    std::printf("%-18s %-9s %6s %6s %6s %8s\n","zawód","poziom","wygr.%","śr.etap","śr.tury","śr.wynik");
    int diff_wins[data::difficulties_count] = {};
    int class_rate[data::difficulties_count][data::classes_count] = {};   // v0.21.51 cz. 2: nowe zawody w rozrzucie zwykłych
    for(int df=0;df<data::difficulties_count;++df)
    for(int c=0;c<data::classes_count;++c)
    {
        int wins=0; long stages=0, turns=0, score=0; const int runs=300;
        for(int k=0;k<runs;++k)
        {
            game g; g.new_run(c, 1000+k*7919, df);
            for(int step=0; step<4000; ++step)
            {
                if(g.st==status::stage_clear){ bot_next(g); continue; }
                if(g.st!=status::playing) break;
                bot_step(g);
            }
            wins += g.st==status::won; stages += g.stage+1; turns += g.turns; score += g.score;
        }
        class_rate[df][c] = wins * 100 / runs;
        if(c < data::open_classes_count) diff_wins[df] += wins;   // średnie: zawody bez sekretów (porównywalne z v0.21.51 cz. 1)
        std::printf("%-18s %-9s %6d %6.1f %6ld %8ld\n", data::classes[c].name, data::difficulties[df].name, wins*100/runs, double(stages)/runs, turns/runs, score/runs);
    }
    for(int df=1;df<data::difficulties_count;++df) CHECK(diff_wins[df-1] > diff_wins[df]);   // trudniej = mniej wygranych
    {
        const int nd = data::default_difficulty;
        int lo = 100, hi = 0;
        for(int c = 0; c < data::open_classes_count; ++c) { lo = imin(lo, class_rate[nd][c]); hi = imax(hi, class_rate[nd][c]); }
        std::printf("Zawody z sekretów (Normalny):");
        for(int c = data::open_classes_count; c < data::classes_count; ++c)
        {
            std::printf(" %s %d%%", data::classes[c].name, class_rate[nd][c]);
            CHECK(class_rate[nd][c] >= lo && class_rate[nd][c] <= hi);   // inne, nie mocniejsze: w rozrzucie zwykłych zawodów
        }
        std::printf(" (zwykłe %d-%d%%)\n", lo, hi);
    }
    {
        int easy = diff_wins[0] * 100 / (300 * data::open_classes_count), hard = diff_wins[data::difficulties_count - 1] * 100 / (300 * data::open_classes_count);
        std::printf("Łatwy %d%%, Trudny %d%%\n", easy, hard);
        CHECK(easy >= 50 && easy <= 60 && hard >= 8 && hard <= 15);   // cele balansu v0.21.49
    }
    // Tabela balansu (Normalny, wszystkie zawody): Szkolenia, Respekt, tryb inwestora; kawa bota (czy przedmioty mają znaczenie)
    {
        const profile* per_class = nullptr;   // v0.21.52 cz. b: premie zależne od zawodu (mistrzostwo) - mods(p, c)
        auto win_rate = [&per_class](const run_mods& m, long& drinks, int& drank_runs) {
            int wins = 0; const int runs = 300;
            drinks = 0; drank_runs = 0;
            for(int c = 0; c < data::open_classes_count; ++c)
                for(int k = 0; k < runs; ++k)
                {
                    game g; g.new_run(c, 1000 + k * 7919, data::default_difficulty, per_class ? mods(*per_class, c) : m);
                    bot_drinks = 0;
                    for(int step = 0; step < 4000; ++step)
                    {
                        if(g.st == status::stage_clear) { bot_next(g); continue; }
                        if(g.st != status::playing) break;
                        bot_step(g);
                    }
                    wins += g.st == status::won; drinks += bot_drinks; drank_runs += bot_drinks > 0;
                }
            return wins * 100 / (runs * data::open_classes_count);
        };
        const int n = 300 * data::open_classes_count;
        profile none; profile_reset(none);
        profile szk = none; for(int i = 0; i < data::upgrades_count; ++i) szk.levels[i] = uint8_t(data::upgrades[i].levels);
        profile full = szk; full_respect(full, false);
        profile inv = full; inv.wins = 1; inv.investor = uint8_t((1 << data::investor_count) - 1);
        profile fullr = full; fullr.rewards = uint8_t(data::rewards_count);   // + wszystkie nagrody za odbiór, w tym Akt 0
        long dr0, dr1, dr2, dr3, dr4, drx; int k0, k1, k2, k3, k4, kx;
        int w0 = win_rate(mods(none), dr0, k0), w1 = win_rate(mods(szk), dr1, k1), w2 = win_rate(mods(full), dr2, k2), w3 = win_rate(mods(inv), dr3, k3);
        int w4 = win_rate(mods(fullr), dr4, k4);
        // v0.21.52 cz. b: + mistrzostwo 10 każdym zawodem (broń mistrza, premia mistrzostwa; bot nie używa mocy) i maksymalny
        // poziom inspektora (druga pamiątka: Kask ojca z odznaki Bez usterek)
        // (jak inne wiersze - bez odznak; druga pamiątka wymaga odblokowanej pamiątki: osobny wiersz z Kaskiem ojca i jego odznaką)
        profile maxp = fullr; maxp.inspector_xp = 1000000;
        for(int c = 0; c < data::classes_count; ++c) maxp.mastery_xp[c] = 60000;
        profile badge = fullr; badge.badges = uint16_t(1 << data::badge_bez_usterek);
        profile maxk = maxp; maxk.badges = badge.badges; maxk.keepsake2 = 2;
        long dr5; int k5; per_class = &maxp;
        int w5 = win_rate(mods(maxp), dr5, k5);
        per_class = nullptr;
        int w6 = win_rate(mods(badge), dr5, k5);
        per_class = &maxk;
        int w7 = win_rate(mods(maxk), dr5, k5);
        per_class = nullptr; (void)dr5; (void)k5;
        bot_no_coffee = true;
        int wx = win_rate(mods(none), drx, kx), wx1 = win_rate(mods(szk), drx, kx);
        bot_no_coffee = false;
        std::printf("Normalny: bez meta %d%%, pełne Szkolenia %d%%, + pełny Respekt %d%%, + wszystkie modyfikatory %d%%, pełne meta z Aktem 0 %d%%,\n"
                    "  + mistrzostwo 10 i maks. inspektor %d%%; z odznaką Bez usterek %d%%, + 2. pamiątka Kask ojca (I) %d%%\n",
                    w0, w1, w2, w3, w4, w5, w6, w7);
        std::printf("Kawa (bot): %.2f/budowę, pije w %d%% budów (bez meta); bez picia kawy: %d%% (pełne Szkolenia %d%%)\n",
                    double(dr0) / n, k0 * 100 / n, wx, wx1);
        CHECK(w0 >= 25 && w0 <= 35);   // cele balansu v0.21.49
        CHECK(w1 >= 50 && w1 <= 60);
        CHECK(w2 >= 65 && w2 <= 75);
        CHECK(w3 >= 5 && w3 <= 15);
        CHECK(w4 >= 60 && w4 <= 70);   // v0.21.49 cz. 3: dłuższa budowa z Aktem 0
        CHECK(w5 >= 60 && w5 <= 72 && w5 >= w4);   // v0.21.52 cz. b: mistrzostwo i inspektor - drobne premie
        CHECK(wx < w0 && dr0 > 0);     // kawa ma znaczenie
        (void)dr1; (void)dr2; (void)dr3; (void)dr4; (void)k1; (void)k2; (void)k3; (void)k4;
    }
    // v0.21.52 cz. a: tempo postępu (#41-#43) - doświadczenie z budowy bez meta (wygrane / przegrane) i ile budów do
    // wykupienia wszystkiego w Szkoleniach: kariera od pustego profilu, bot na Normalnym kolejno odblokowanymi zawodami,
    // odznaki, zlecenia i sekrety jak w grze, po każdej budowie kupuje najtańsze, na co go stać (tabela w CHANGELOG)
    {
        long xw = 0, xl = 0; int nw = 0, nl = 0;
        for(int c = 0; c < data::open_classes_count; ++c)
            for(int k = 0; k < 100; ++k)
            {
                game g; g.new_run(c, 1000 + k * 7919);
                for(int step = 0; step < 4000; ++step)
                {
                    if(g.st == status::stage_clear) { bot_next(g); continue; }
                    if(g.st != status::playing) break;
                    bot_step(g);
                }
                if(g.st == status::won) { xw += g.xp(); ++nw; } else { xl += g.xp(); ++nl; }
            }
        const int careers = 40;
        int total = 0, lo = 1000, hi = 0; long gained = 0, runs_all = 0, wins_all = 0;
        // v0.21.52 cz. b: tempo poziomu inspektora i mistrzostwa (kariera gra dalej po wykupieniu Szkoleń)
        long insp_total = 0, insp_xp = 0, insp_runs = 0, m10_total = 0, m10_n = 0, insp_at_shop = 0; int insp_lo = 1000, insp_hi = 0;
        long m3_total = 0, m3_n = 0, m5_total = 0, m5_n = 0, m7_total = 0, m7_n = 0;
        for(int t = 0; t < careers; ++t)
        {
            profile p; profile_reset(p);
            int runs = 0, done = -1, insp_done = -1;
            int with[max_classes] = {}, m10[max_classes], m3[max_classes], m5[max_classes], m7[max_classes];
            for(int c = 0; c < max_classes; ++c) m10[c] = m3[c] = m5[c] = m7[c] = -1;
            while(runs < 300)   // v0.21.52 cz. b: 300 budów - też tempo mistrzostwa (budowy danym zawodem)
            {
                int list[max_classes], n = 0;
                for(int c = 0; c < data::classes_count; ++c) if(class_unlocked(p, c)) list[n++] = c;
                const int cls = list[(runs * 7 + t) % n];
                const run_mods m = mods(p, cls);
                start_run(p);
                const int x0 = p.xp;
                game g; g.new_run(cls, 5000 + uint32_t(t) * 100000u + uint32_t(runs) * 7919u, data::default_difficulty, m);
                for(int step = 0; step < 4000; ++step)
                {
                    if(g.st == status::stage_clear) { check_badges(p, g); bank_xp(p, g); bot_next(g); continue; }
                    if(g.st != status::playing) break;
                    bot_step(g);
                }
                check_badges(p, g); bank_xp(p, g); check_contracts(p); check_secrets(p, &g);
                if(g.st == status::won) { record_win(p); add_house(p, g); ++wins_all; }
                const progress_gain pg = bank_progress(p, g);
                insp_xp += pg.gained; ++insp_runs;
                ++with[cls];
                if(mastery_level(p, cls) >= 3 && m3[cls] < 0) m3[cls] = with[cls];
                if(mastery_level(p, cls) >= 5 && m5[cls] < 0) m5[cls] = with[cls];
                if(mastery_level(p, cls) >= 7 && m7[cls] < 0) m7[cls] = with[cls];
                if(mastery_level(p, cls) >= 10 && m10[cls] < 0) m10[cls] = with[cls];
                ++runs; gained += p.xp - x0;
                if(insp_done < 0 && inspector_level(p) >= data::inspector_levels_count) insp_done = runs;
                for(;;)
                {
                    int kind = -1, idx = -1;
                    const int c = next_unlock(p, kind, idx);
                    if(c < 0 || p.xp < c) break;
                    if(kind == 0) buy_upgrade(p, idx); else if(kind == 1) buy_class(p, idx); else if(kind == 2) buy_tool(p, idx);
                    else if(kind == 3) buy_helper(p, idx); else buy_hard(p);
                }
                int kind = -1, idx = -1;
                if(done < 0 && next_unlock(p, kind, idx) < 0) { done = runs; insp_at_shop += inspector_level(p); }
            }
            CHECK(done > 0 && insp_done > 0);
            total += done; lo = imin(lo, done); hi = imax(hi, done); runs_all += runs;
            insp_total += insp_done; insp_lo = imin(insp_lo, insp_done); insp_hi = imax(insp_hi, insp_done);
            for(int c = 0; c < max_classes; ++c)
            {
                if(m10[c] > 0) { m10_total += m10[c]; ++m10_n; }
                if(m3[c] > 0) { m3_total += m3[c]; ++m3_n; }
                if(m5[c] > 0) { m5_total += m5[c]; ++m5_n; }
                if(m7[c] > 0) { m7_total += m7[c]; ++m7_n; }
            }
        }
        const int avg = total / careers;
        std::printf("Tempo postępu: dośw. z budowy bez meta %ld (wygrana %ld, porażka %ld); Szkolenia razem %d dośw.;\n"
                    "  wszystko wykupione po %d budowach (min %d, maks %d; kariera: %ld dośw./budowę, wygrane %ld%%)\n",
                    (xw + xl) / (nw + nl), xw / imax(1, nw), xl / imax(1, nl), shop_total_cost(), avg, lo, hi,
                    gained / runs_all, wins_all * 100 / runs_all);
        CHECK(avg >= 20 && avg <= 30);   // cel #41: pełne odblokowanie ~20-30 budów
        const int insp_avg = int(insp_total / careers);
        std::printf("Inspektor: %ld dośw./budowę, maks. poziom %d po %d budowach (min %d, maks %d), po wykupieniu Szkoleń poziom %ld;\n"
                    "  mistrzostwo (budowy danym zawodem): poziom 3 po %ld, 5 po %ld, 7 po %ld, 10 po %ld (zawodów z 10: %ld)\n",
                    insp_xp / imax(1, int(insp_runs)), data::inspector_levels_count, insp_avg, insp_lo, insp_hi, insp_at_shop / careers,
                    m3_total / imax(1, int(m3_n)), m5_total / imax(1, int(m5_n)), m7_total / imax(1, int(m7_n)), m10_total / imax(1, int(m10_n)), m10_n);
        CHECK(insp_avg >= 60 && insp_avg <= 160);   // cel #44: długi cel (nagroda co poziom, pierwsze poziomy co budowę)
    }
    std::printf(fails ? "\n%d FAIL\n" : "\nOK - wszystkie testy przeszły\n", fails);
    return fails != 0;
}
