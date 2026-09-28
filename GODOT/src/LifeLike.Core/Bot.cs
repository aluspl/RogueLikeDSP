using LifeLike.Core.Data;

namespace LifeLike.Core;

/// <summary>
/// Deterministyczny bot z GBA/tests/core_tests.cpp (bot_step): idzie do najbliższego (po ścieżce) wroga albo schodów,
/// atakuje z dystansu, schodzi z pól zapowiedzianego ciosu bossa w stronę celu, przy paczce sprzętu bierze lepszą
/// (gorszą zostawia), pije z termosu poniżej BotDrinkBelowPct HP. Cel wybiera koszt drogi (do 7 pól drogi, błoto droższe;
/// na etapie z bossem każdy problem); omija czerwone pola ciosu bossa i wybuchu - z każdym krokiem maleje, więc bot nie krąży między dwoma celami.
/// Używany w testach balansu, teście złotym i teście dymnym Godota.
/// </summary>
public static class Bot
{
    private static readonly int[,] Dirs = { { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 } };

    /// <summary>Pole przechodnie dla bota: także tymczasowy mur (Ścianka) - znika za kilka tur, więc nie zmienia celu.</summary>
    private static bool Open(Game g, int x, int y)
    {
        if (g.Lv.Passable(x, y)) return true;
        for (var i = 0; i < g.WallsCount; ++i)
        {
            if (g.Walls[i].X == x && g.Walls[i].Y == y) return true;
        }
        return false;
    }

    /// <summary>Waga pola dla bota: zwykłe 2, błoto 6 (kosztuje turę).</summary>
    private static int W(Game g, int x, int y) => g.Mud(x, y) ? 6 : 2;

    /// <summary>
    /// Koszt drogi bota (4 kierunki, pola przechodnie) od (sx, sy) do każdego pola; -1 = nieosiągalne. Krok = waga pola,
    /// z którego + waga pola, na które - koszt symetryczny i maleje wzdłuż drogi do celu. Bez błota = 4 x liczba kroków.
    /// </summary>
    private static int[] Costs(Game g, int sx, int sy)
    {
        var dist = new int[Level.W * Level.H];
        Array.Fill(dist, -1);
        var q = new PriorityQueue<int, (int Cost, int Pos)>();
        dist[sy * Level.W + sx] = 0;
        q.Enqueue(sy * Level.W + sx, (0, sy * Level.W + sx));
        while (q.TryDequeue(out var p, out var pr))
        {
            if (pr.Cost != dist[p]) continue;
            int x = p % Level.W, y = p / Level.W;
            for (var k = 0; k < 4; ++k)
            {
                int nx = x + Dirs[k, 0], ny = y + Dirs[k, 1];
                if (!Open(g, nx, ny)) continue;
                var nc = pr.Cost + W(g, x, y) + W(g, nx, ny);
                var ni = ny * Level.W + nx;
                if (dist[ni] < 0 || nc < dist[ni])
                {
                    dist[ni] = nc;
                    q.Enqueue(ni, (nc, ni));
                }
            }
        }
        return dist;
    }

    public static void Step(Game g)
    {
        if (g.HasOffer)
        {
            if (g.OfferIsBetter) g.AcceptOffer();
            else g.DeclineOffer();
        }
        if (g.Thermos > 0 && g.Hero.Hp * 100 < g.Hero.MaxHp * g.D.BotDrinkBelowPct && g.PlayerDrink()) return;
        var hd = Costs(g, g.Hero.X, g.Hero.Y); // koszt drogi (błoto droższe)
        int tx = g.StairsX, ty = g.StairsY, best = 999999;
        if (g.StairsLocked()) // pieczątki (Akt 0): najpierw najbliższy dokument, potem schody
        {
            var bdoc = 999999;
            for (var i = 0; i < g.PickupsCount; ++i)
            {
                var p = g.Pickups[i];
                var dd = hd[p.Y * Level.W + p.X];
                if (p.Active && p.Type == PickupType.Document && dd >= 0 && dd < bdoc)
                {
                    bdoc = dd;
                    tx = p.X;
                    ty = p.Y;
                }
            }
        }
        for (var i = 0; i < g.EnemiesCount; ++i)
        {
            var e = g.Enemies[i];
            var dd = hd[e.Y * Level.W + e.X];
            if (e.Alive && dd >= 0 && dd < best && (dd < 32 || g.StairsX < 0))
            {
                best = dd;
                tx = e.X;
                ty = e.Y;
            }
        }
        var hasTarget = tx >= 0 && hd[ty * Level.W + tx] >= 0;
        var td = hasTarget ? Costs(g, tx, ty) : hd;
        if (g.DangerCell(g.Hero.X, g.Hero.Y)) // zapowiedziany cios bossa / wybuch: zejdź z czerwonych pól w stronę celu
        {
            int bk = -1, bs = -1000000;
            for (var k = 0; k < 4; ++k)
            {
                int nx = g.Hero.X + Dirs[k, 0], ny = g.Hero.Y + Dirs[k, 1];
                if (!g.Lv.Passable(nx, ny) || g.Occupied(nx, ny)) continue;
                var toTarget = hasTarget && td[ny * Level.W + nx] >= 0 ? td[ny * Level.W + nx] : 5000;
                var s = (g.DangerCell(nx, ny) ? 0 : 10000) - toTarget; // najpierw pole poza zasięgiem, potem bliżej celu
                if (s > bs)
                {
                    bs = s;
                    bk = k;
                }
            }
            if (bk >= 0 && g.PlayerMove(Dirs[bk, 0], Dirs[bk, 1])) return;
        }
        if (g.NearestTarget() >= 0 && g.Weapon.Range > 1)
        {
            g.PlayerAttackNearest();
            return;
        }
        if (!hasTarget || (tx == g.Hero.X && ty == g.Hero.Y))
        {
            g.PlayerWait();
            return;
        }
        for (var k = 0; k < 4; ++k) // krok na sąsiednie pole bliżej celu
        {
            int nx = g.Hero.X + Dirs[k, 0], ny = g.Hero.Y + Dirs[k, 1];
            var tn = td[ny * Level.W + nx];
            if (!Open(g, nx, ny) || tn < 0 || tn + W(g, nx, ny) + W(g, g.Hero.X, g.Hero.Y) != td[g.Hero.Y * Level.W + g.Hero.X]) continue;
            if (!g.Lv.Passable(nx, ny)) // mur Ścianki na drodze: czekaj, aż zniknie
            {
                g.PlayerWait();
                return;
            }
            if (g.DangerCell(nx, ny) && g.EnemyAt(nx, ny) < 0) // nie wchodzi na czerwone pola przed ciosem
            {
                g.PlayerWait();
                return;
            }
            if (!g.PlayerMove(Dirs[k, 0], Dirs[k, 1])) g.PlayerWait();
            return;
        }
        g.PlayerWait();
    }

    /// <summary>
    /// Wariant bota z testu złotego: jak Step, ale używa mocy, gdy widoczny wróg jest blisko,
    /// i celuje (PlayerAttack) w najbliższy widoczny cel w zasięgu. Paczkę sprzętu tej samej lub lepszej jakości
    /// zakłada (wymiana cechy), gorszą zostawia; pije z termosu poniżej połowy HP; przy pierwszym problemie na etapie wzywa brygadę.
    /// Musi zgadzać się z GBA/tests/golden_dump.cpp.
    /// </summary>
    public static void StepSmart(Game g)
    {
        if (g.HasOffer)
        {
            if (g.OfferRarity >= g.Equipped[g.OfferSlot]) g.AcceptOffer();
            else g.DeclineOffer();
        }
        if (g.Thermos > 0 && g.Hero.Hp * 2 < g.Hero.MaxHp && g.PlayerDrink()) return;
        for (var k = 0; k < g.D.Repairs.Length; ++k) // naprawy: Kładka przy kałużach; Załataj przy niskim HP (problem w polu widzenia)
        {
            var want = g.D.Repairs[k].Effect == RepairEffect.Bridge || g.Hero.Hp * 3 < g.Hero.MaxHp;
            if (want && g.RepairBlocked(k) == RepairBlock.Ok && g.PlayerRepair(k)) return;
        }
        if (g.HelperCalled < 0 && g.NearestVisibleEnemy() >= 0) // brygada: przy pierwszym problemie na etapie (kolejny fachowiec co etap)
        {
            for (var k = 0; k < g.D.Brigade.Length; ++k)
            {
                var h = (g.Stage + k) % g.D.Brigade.Length;
                if (g.HelperBlocked(h) == HelperBlock.Ok && g.CallHelper(h)) return;
            }
        }
        if (!g.DangerCell(g.Hero.X, g.Hero.Y) && g.AbilityCd == 0)
        {
            var t = g.NearestVisibleEnemy();
            if (t >= 0 && Game.Cheb(g.Hero.X, g.Hero.Y, g.Enemies[t].X, g.Enemies[t].Y) <= 2 && g.PlayerAbility()) return;
        }
        if (!g.DangerCell(g.Hero.X, g.Hero.Y))
        {
            Span<sbyte> targets = stackalloc sbyte[Game.MaxEnemies];
            var n = g.TargetsInRange(targets);
            if (n > 0 && g.PlayerAttack(targets[0])) return;
        }
        Step(g);
    }

    /// <summary>Hurtownia bota: kupuje po kolei wszystko, na co starcza budżetu (deterministycznie).</summary>
    public static void Shop(Game g)
    {
        for (var i = 0; i < g.D.Hurtownia.Length; i++) g.HurtowniaBuy(i);
    }
}
