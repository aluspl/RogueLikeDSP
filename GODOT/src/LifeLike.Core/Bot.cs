namespace LifeLike.Core;

/// <summary>
/// Deterministyczny bot z GBA/tests/core_tests.cpp (bot_step): idzie do najbliższego wroga albo schodów,
/// atakuje z dystansu, schodzi z pól zapowiedzianego ciosu bossa, przy paczce sprzętu bierze lepszą (gorszą zostawia),
/// pije z termosu poniżej BotDrinkBelowPct HP. Używany w testach balansu, teście złotym
/// i teście dymnym Godota.
/// </summary>
public static class Bot
{
    private static readonly int[,] Dirs = { { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 } };

    public static void Step(Game g)
    {
        if (g.HasOffer)
        {
            if (g.OfferIsBetter) g.AcceptOffer();
            else g.DeclineOffer();
        }
        if (g.Thermos > 0 && g.Hero.Hp * 100 < g.Hero.MaxHp * g.D.BotDrinkBelowPct && g.PlayerDrink()) return;
        if (g.SlamCell(g.Hero.X, g.Hero.Y)) // zapowiedziany cios bossa: zejdź z czerwonych pól (jak człowiek; nie wraca na nie)
        {
            int best = -1, bd = -1;
            for (var k = 0; k < 4; ++k)
            {
                int nx = g.Hero.X + Dirs[k, 0], ny = g.Hero.Y + Dirs[k, 1];
                if (!g.Lv.Passable(nx, ny) || g.Occupied(nx, ny)) continue;
                var dist = Game.Cheb(nx, ny, g.SlamX, g.SlamY) + (g.SlamCell(nx, ny) ? 0 : 10); // najpierw pole poza zasięgiem
                if (dist > bd)
                {
                    bd = dist;
                    best = k;
                }
            }
            if (best >= 0 && g.PlayerMove(Dirs[best, 0], Dirs[best, 1])) return;
        }
        if (g.NearestTarget() >= 0 && g.Weapon.Range > 1)
        {
            g.PlayerAttackNearest();
            return;
        }
        int tx = g.StairsX, ty = g.StairsY, bestD = 999;
        for (var i = 0; i < g.EnemiesCount; ++i)
        {
            var e = g.Enemies[i];
            var d = Game.Cheb(g.Hero.X, g.Hero.Y, e.X, e.Y);
            if (e.Alive && d < bestD && (d < 6 || g.StairsX < 0))
            {
                bestD = d;
                tx = e.X;
                ty = e.Y;
            }
        }
        // BFS po mapie do celu
        var px = new int[Level.W * Level.H];
        Array.Fill(px, -1);
        var q = new Queue<(int X, int Y)>();
        q.Enqueue((g.Hero.X, g.Hero.Y));
        px[g.Hero.Y * Level.W + g.Hero.X] = 4;
        while (q.Count > 0)
        {
            var (x, y) = q.Dequeue();
            if (x == tx && y == ty) break;
            for (var k = 0; k < 4; ++k)
            {
                int nx = x + Dirs[k, 0], ny = y + Dirs[k, 1];
                if (g.Lv.Passable(nx, ny) && px[ny * Level.W + nx] < 0)
                {
                    px[ny * Level.W + nx] = k;
                    q.Enqueue((nx, ny));
                }
            }
        }
        if (tx < 0 || px[ty * Level.W + tx] < 0 || (tx == g.Hero.X && ty == g.Hero.Y))
        {
            g.PlayerWait();
            return;
        }
        int cx = tx, cy = ty;
        while (true)
        {
            var k = px[cy * Level.W + cx];
            int bx = cx - Dirs[k, 0], by = cy - Dirs[k, 1];
            if (bx == g.Hero.X && by == g.Hero.Y)
            {
                if (g.SlamCell(cx, cy) && g.EnemyAt(cx, cy) < 0) // nie wchodzi na czerwone pola przed ciosem
                {
                    g.PlayerWait();
                    return;
                }
                if (!g.PlayerMove(cx - bx, cy - by)) g.PlayerWait();
                return;
            }
            cx = bx;
            cy = by;
        }
    }

    /// <summary>
    /// Wariant bota z testu złotego: jak Step, ale używa mocy, gdy widoczny wróg jest blisko,
    /// i celuje (PlayerAttack) w najbliższy widoczny cel w zasięgu. Paczkę sprzętu tej samej lub lepszej jakości
    /// zakłada (wymiana cechy), gorszą zostawia; pije z termosu poniżej połowy HP.
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
