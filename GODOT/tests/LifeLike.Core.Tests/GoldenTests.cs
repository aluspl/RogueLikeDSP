using System.Text.Json;

namespace LifeLike.Core.Tests;

/// <summary>
/// Test złoty: te same przebiegi co GBA/tests/golden_dump.cpp (skompilowany z nagłówkami GBA). Po każdym kroku
/// porównuje skrót stanu, na starcie każdego etapu i na końcu – pełny zrzut pole po polu, plus profil gracza.
/// </summary>
public class GoldenTests
{
    public static IEnumerable<object[]> Runs() =>
        Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "golden"), "run_*.json")
            .Select(Path.GetFileName).OrderBy(f => f, StringComparer.Ordinal).Select(f => new object[] { f });

    [Fact]
    public void GoldenFilesExist() => Assert.True(Runs().Count() >= 10);

    [Theory]
    [MemberData(nameof(Runs))]
    public void ReplayMatchesGba(string file)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(TestData.GoldenPath(file)));
        var j = doc.RootElement;
        var d = TestData.D;
        var cls = j.GetProperty("cls").GetInt32();
        var seed = j.GetProperty("seed").GetUInt32();
        var diff = j.GetProperty("diff").GetInt32();
        bool fullMods = j.GetProperty("fullMods").GetInt32() != 0, smart = j.GetProperty("smart").GetInt32() != 0;
        bool shop = j.GetProperty("shop").GetInt32() != 0, ngplus = j.GetProperty("ngplus").GetInt32() != 0;
        var steps = j.GetProperty("steps").GetInt32();
        var snaps = j.GetProperty("snapshots").EnumerateArray().ToList();
        var digests = j.GetProperty("digests").EnumerateArray().Select(x => x.GetString()).ToList();

        var p = Meta.NewProfile(d);
        var m = RunMods.Default(d);
        if (fullMods)
        {
            for (var i = 0; i < d.Upgrades.Length; i++) p.Levels[i] = (byte)d.Upgrades[i].Levels;
            p.Tools = (byte)((1 << d.Tools.Length) - 1);
            m = Meta.Mods(d, p);
        }
        var g = new Game(d);
        g.NewRun(cls, seed, diff, m);
        ++p.Runs;

        var snapIndex = 0;
        var digestIndex = 0;
        void CheckSnapshot(int step)
        {
            Assert.True(snapIndex < snaps.Count, $"{file}: więcej etapów niż w GBA (krok {step})");
            Compare($"{file} zrzut #{snapIndex} (krok {step})", snaps[snapIndex++], GoldenSnapshot.Of(g, step));
        }
        void CheckDigest(int step)
        {
            Assert.True(digestIndex < digests.Count, $"{file}: więcej kroków niż w GBA");
            var got = StateDigest.Of(g).ToString("x8");
            if (got != digests[digestIndex])
            {
                Assert.Fail($"{file}: rozbieżność stanu w kroku {step} (tura {g.Turns}, etap {g.Stage + 1}): " +
                            $"GBA {digests[digestIndex]}, C# {got}. Ostatni komunikat: {g.Log[Game.LogLines - 1]}");
            }
            digestIndex++;
            g.HitsCount = 0; // jak warstwa GBA po każdej turze
        }

        CheckSnapshot(0);
        var didNg = false;
        var step = 0;
        for (; step < steps; ++step)
        {
            if (g.St == GameStatus.StageClear)
            {
                Meta.CheckBadges(d, p, g);
                Meta.BankXp(p, g);
                if (g.ActCleared && shop) Bot.Shop(g);
                g.NextStage();
                CheckSnapshot(step);
                CheckDigest(step);
                continue;
            }
            if (g.St == GameStatus.Won && ngplus && !didNg)
            {
                if (g.Score > p.Best) p.Best = g.Score;
                ++p.Wins;
                Meta.AddHouse(p, g);
                Meta.CheckBadges(d, p, g);
                Meta.BankXp(p, g);
                didNg = true;
                g.NewGamePlus();
                CheckSnapshot(step);
                CheckDigest(step);
                continue;
            }
            if (g.St != GameStatus.Playing) break;
            if (smart) Bot.StepSmart(g);
            else Bot.Step(g);
            CheckDigest(step);
        }
        if (g.Score > p.Best) p.Best = g.Score;
        if (g.St == GameStatus.Won)
        {
            ++p.Wins;
            Meta.AddHouse(p, g);
        }
        Meta.CheckBadges(d, p, g);
        Meta.BankXp(p, g);

        Assert.Equal(snaps.Count, snapIndex);
        Assert.Equal(digests.Count, digestIndex);
        Assert.Equal(j.GetProperty("endStep").GetInt32(), step);
        Compare($"{file} stan końcowy", j.GetProperty("final"), GoldenSnapshot.Of(g, step));
        Compare($"{file} profil", j.GetProperty("profile"), GoldenSnapshot.OfProfile(p));
    }

    /// <summary>Porównanie pole po polu – komunikat wskazuje pierwszą różnicę.</summary>
    private static void Compare(string where, JsonElement expected, string actualJson)
    {
        using var actualDoc = JsonDocument.Parse(actualJson);
        var actual = actualDoc.RootElement;
        foreach (var prop in expected.EnumerateObject())
        {
            Assert.True(actual.TryGetProperty(prop.Name, out var a), $"{where}: brak pola {prop.Name} w C#");
            var e = prop.Value.GetRawText();
            var got = a.GetRawText();
            if (e != got) Assert.Fail($"{where}: pole '{prop.Name}' różni się\nGBA: {e}\nC#:  {got}");
        }
        Assert.Equal(expected.EnumerateObject().Count(), actual.EnumerateObject().Count());
    }
}
