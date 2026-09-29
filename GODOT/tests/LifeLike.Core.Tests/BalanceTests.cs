namespace LifeLike.Core.Tests;

/// <summary>
/// core_tests.cpp: 28 (balans na botach). GBA gra 300 budów na zawód i poziom; tu mniej (szybciej w CI),
/// ale asercje te same: trudniej = mniej wygranych, pełne Szkolenia wyraźnie pomagają.
/// </summary>
public class BalanceTests
{
    private const int Runs = 100;

    private static GameData D => TestData.D;

    private static uint Seed(int k) => (uint)(1000 + k * 7919);

    [Fact]
    public void HarderDifficultyMeansFewerWinsAndUpgradesHelp()
    {
        var diffWins = new int[D.Difficulties.Length];
        Parallel.For(0, D.Difficulties.Length * D.Classes.Length, job =>
        {
            int df = job / D.Classes.Length, c = job % D.Classes.Length, wins = 0;
            for (var k = 0; k < Runs; ++k)
            {
                if (TestData.PlayOut(c, Seed(k), df, RunMods.Default(D)).St == GameStatus.Won) wins++;
            }
            Interlocked.Add(ref diffWins[df], wins);
        });
        for (var df = 1; df < D.Difficulties.Length; ++df) Assert.True(diffWins[df - 1] > diffWins[df], $"poziom {df}: {string.Join(",", diffWins)}");

        var p = Meta.NewProfile(D);
        for (var i = 0; i < D.Upgrades.Length; ++i) p.Levels[i] = (byte)D.Upgrades[i].Levels;
        var m = Meta.Mods(D, p);
        var upgraded = 0;
        Parallel.For(0, D.Classes.Length, c =>
        {
            var wins = 0;
            for (var k = 0; k < Runs; ++k)
            {
                if (TestData.PlayOut(c, Seed(k), D.DefaultDifficulty, m).St == GameStatus.Won) wins++;
            }
            Interlocked.Add(ref upgraded, wins);
        });
        Assert.True(upgraded > diffWins[D.DefaultDifficulty], $"bez ulepszeń {diffWins[D.DefaultDifficulty]}, z pełnymi {upgraded}");
        // pełny Respekt na pełnych Szkoleniach pomaga dalej (cele v0.21.49: 25-35% / 50-60% / 65-75%)
        for (var i = 0; i < D.Respect.Length; ++i) Meta.SetRespectRank(p, i, D.Respect[i].Ranks);
        var mr = Meta.Mods(D, p);
        var respected = 0;
        Parallel.For(0, D.Classes.Length, c =>
        {
            var wins = 0;
            for (var k = 0; k < Runs; ++k)
            {
                if (TestData.PlayOut(c, Seed(k), D.DefaultDifficulty, mr).St == GameStatus.Won) wins++;
            }
            Interlocked.Add(ref respected, wins);
        });
        Assert.True(respected > upgraded, $"pełne Szkolenia {upgraded}, + Respekt {respected}");
    }
}
