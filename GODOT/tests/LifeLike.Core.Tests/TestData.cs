namespace LifeLike.Core.Tests;

/// <summary>
/// Wspólne dane testów: kopia game.json z zamrożonej migawki GBA (golden/game.json), żeby testy nie zmieniały się
/// razem z bieżącymi pracami nad GBA. Plus pomocnicze funkcje przeniesione z GBA/tests/core_tests.cpp.
/// </summary>
public static class TestData
{
    private static readonly Lazy<GameData> Lazy = new(() => GameData.LoadFile(GoldenPath("game.json")));

    public static GameData D => Lazy.Value;

    public static string GoldenPath(string file) => Path.Combine(AppContext.BaseDirectory, "golden", file);

    public static Game NewGame() => new(D);

    public static Game Run(int cls, uint seed)
    {
        var g = NewGame();
        g.NewRun(cls, seed);
        return g;
    }

    public static Game Run(int cls, uint seed, int diff)
    {
        var g = NewGame();
        g.NewRun(cls, seed, diff);
        return g;
    }

    /// <summary>Otwarta arena 14x14 bez wrogów i znajdziek, bohater na (7,7) – do testów mocy.</summary>
    public static Game Arena(int cls)
    {
        var g = Run(cls, 77);
        g.Lv.Fill(Tile.Wall);
        for (var y = 1; y <= 14; ++y)
            for (var x = 1; x <= 14; ++x)
                g.Lv[x, y] = Tile.Floor;
        g.EnemiesCount = 0;
        g.PickupsCount = 0;
        g.StairsX = g.StairsY = -1;
        g.Weather = 0; // bez pogody (testy mocy i zasięgu)
        g.Hero.X = 7;
        g.Hero.Y = 7;
        g.UpdateFov();
        return g;
    }

    /// <summary>Czy wszystkie przechodnie pola są osiągalne z (sx, sy).</summary>
    public static bool Connected(Level lv, int sx, int sy)
    {
        var seen = new bool[Level.W * Level.H];
        var q = new Queue<(int, int)>();
        q.Enqueue((sx, sy));
        seen[sy * Level.W + sx] = true;
        var n = 1;
        int[,] d = { { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 } };
        while (q.Count > 0)
        {
            var (x, y) = q.Dequeue();
            for (var k = 0; k < 4; k++)
            {
                int nx = x + d[k, 0], ny = y + d[k, 1];
                if (lv.Passable(nx, ny) && !seen[ny * Level.W + nx])
                {
                    seen[ny * Level.W + nx] = true;
                    ++n;
                    q.Enqueue((nx, ny));
                }
            }
        }
        var total = 0;
        for (var y = 0; y < Level.H; ++y)
            for (var x = 0; x < Level.W; ++x)
                if (lv.Passable(x, y)) total++;
        return n == total;
    }

    /// <summary>Rozgrywa budowę botem jak w teście balansu GBA (maks. 4000 kroków).</summary>
    public static Game PlayOut(int cls, uint seed, int diff, RunMods mods)
    {
        var g = NewGame();
        g.NewRun(cls, seed, diff, mods);
        for (var step = 0; step < 4000; ++step)
        {
            if (g.St == GameStatus.StageClear)
            {
                g.NextStage();
                continue;
            }
            if (g.St != GameStatus.Playing) break;
            Bot.Step(g);
        }
        return g;
    }
}
