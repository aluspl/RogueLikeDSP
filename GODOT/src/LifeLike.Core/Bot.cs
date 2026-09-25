using LifeLike.Core.Data;

namespace LifeLike.Core;

/// <summary>
/// Deterministyczny bot z GBA/tests/core_tests.cpp (bot_step): idzie do najbliższego (po ścieżce) wroga albo schodów,
/// atakuje z dystansu, schodzi z pól zapowiedzianego ciosu bossa w stronę celu, przy paczce sprzętu bierze lepszą
/// (gorszą zostawia), pije z termosu poniżej BotDrinkBelowPct HP. Cel wybiera odległość po ścieżce (do 7 pól drogi;
/// na etapie z bossem każdy problem) - z każdym krokiem maleje, więc bot nie krąży między dwoma celami.
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

    /// <summary>Odległość po ścieżce (4 kierunki, pola przechodnie) od (sx, sy) do każdego pola; -1 = nieosiągalne.</summary>
    private static int[] Distances(Game g, int sx, int sy)
    {
        var dist = new int[Level.W * Level.H];
        Array.Fill(dist, -1);
        var q = new Queue<(int X, int Y)>();
        q.Enqueue((sx, sy));
        dist[sy * Level.W + sx] = 0;
        while (q.Count > 0)
        {
            var (x, y) = q.Dequeue();
            for (var k = 0; k < 4; ++k)
            {
                int nx = x + Dirs[k, 0], ny = y + Dirs[k, 1];
                if (Open(g, nx, ny) && dist[ny * Level.W + nx] < 0)
                {
                    dist[ny * Level.W + nx] = dist[y * Level.W + x] + 1;
                    q.Enqueue((nx, ny));
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
        var hd = Distances(g, g.Hero.X, g.Hero.Y);
        int tx = g.StairsX, ty = g.StairsY, best = 999;
        for (var i = 0; i < g.EnemiesCount; ++i)
        {
            var e = g.Enemies[i];
            var dd = hd[e.Y * Level.W + e.X];
            if (e.Alive && dd >= 0 && dd < best && (dd < 8 || g.StairsX < 0))
            {
                best = dd;
                tx = e.X;
                ty = e.Y;
            }
        }
        var hasTarget = tx >= 0 && hd[ty * Level.W + tx] >= 0;
        var td = hasTarget ? Distances(g, tx, ty) : hd;
        if (g.SlamCell(g.Hero.X, g.Hero.Y)) // zapowiedziany cios bossa: zejdź z czerwonych pól w stronę celu
        {
            int bk = -1, bs = -1000000;
            for (var k = 0; k < 4; ++k)
            {
                int nx = g.Hero.X + Dirs[k, 0], ny = g.Hero.Y + Dirs[k, 1];
                if (!g.Lv.Passable(nx, ny) || g.Occupied(nx, ny)) continue;
                var toTarget = hasTarget && td[ny * Level.W + nx] >= 0 ? td[ny * Level.W + nx] : 500;
                var s = (g.SlamCell(nx, ny) ? 0 : 1000) - toTarget; // najpierw pole poza zasięgiem, potem bliżej celu
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
            if (!Open(g, nx, ny) || td[ny * Level.W + nx] != td[g.Hero.Y * Level.W + g.Hero.X] - 1) continue;
            if (!g.Lv.Passable(nx, ny)) // mur Ścianki na drodze: czekaj, aż zniknie
            {
                g.PlayerWait();
                return;
            }
            if (g.SlamCell(nx, ny) && g.EnemyAt(nx, ny) < 0) // nie wchodzi na czerwone pola przed ciosem
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
        if (!g.SlamCell(g.Hero.X, g.Hero.Y) && g.AbilityCd == 0)
        {
            var t = g.NearestVisibleEnemy();
            if (t >= 0 && Game.Cheb(g.Hero.X, g.Hero.Y, g.Enemies[t].X, g.Enemies[t].Y) <= 2 && g.PlayerAbility()) return;
        }
        if (!g.SlamCell(g.Hero.X, g.Hero.Y))
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
