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
        int Opt(string k) => j.TryGetProperty(k, out var v) ? v.GetInt32() : 0;
        int badges = Opt("badges"), contracts = Opt("contracts"), keepsake = Opt("keepsake"), keepsakeRuns = Opt("keepsakeRuns");
        var investor = Opt("investor");
        int paths = Opt("paths"), daily = Opt("daily");
        int respect = Opt("respect"), rewards = Opt("rewards"), weekly = Opt("weekly"), secrets = Opt("secrets");
        int mastery = Opt("mastery"), insp = Opt("insp"), keepsake2 = Opt("keepsake2"); // v0.21.52 cz. b
        var tree = Opt("tree"); // v0.21.52 cz. c: wybory w drzewku (1 + bity opcji)
        var contract = Opt("contract"); // v0.21.52 cz. d: kontrakt mapy kariery
        int goldenDay = Daily.Number(d, 2026, 10, 3), goldenWeek = Weekly.Number(d, 2026, 10, 3); // zadania dnia i tygodnia
        var snaps = j.GetProperty("snapshots").EnumerateArray().ToList();
        var digests = j.GetProperty("digests").EnumerateArray().Select(x => x.GetString()).ToList();

        var p = Meta.NewProfile(d);
        if (fullMods)
        {
            for (var i = 0; i < d.Upgrades.Length; i++) p.Levels[i] = (byte)d.Upgrades[i].Levels;
            p.Tools = (byte)((1 << d.Tools.Length) - 1);
            p.Brigade = (byte)((1 << d.Brigade.Length) - 1);
        }
        p.Badges = (ushort)badges;
        p.Contracts = (byte)contracts;
        p.Keepsake = (byte)keepsake;
        if (investor != 0) // tryb inwestora po pierwszej wygranej
        {
            p.Wins = 1;
            p.Investor = (byte)investor;
        }
        if (keepsake > 0) p.KeepsakeRuns[keepsake - 1] = (byte)keepsakeRuns;
        p.Secrets = (ushort)secrets; // v0.21.51 cz. 2: przed Respektem – Zaprawiony w boju tylko po sekrecie
        if (respect != 0)
        {
            for (var i = 0; i < d.Respect.Length; i++)
            {
                if (Meta.RespectUnlocked(d, p, i)) Meta.SetRespectRank(p, i, d.Respect[i].Ranks);
            }
        }
        p.Rewards = (byte)rewards;
        p.InspectorXp = (uint)insp;
        p.Keepsake2 = (byte)keepsake2;
        if (mastery > 0)
        {
            p.MasteryXp[cls] = (ushort)Progress.Floor(d.MasteryLevels, mastery);
            if (mastery >= 3) p.PowerAlt = (ushort)(1 << cls);
        }
        if (tree != 0)
        {
            p.Xp = 100000;
            for (var n = 0; n < d.TreeNodes.Length; ++n) SkillTree.Choose(d, p, n, ((tree - 1) >> n) & 1);
            p.Xp = 0;
        }
        if (daily == Daily.Number(d, 2026, 10, 2)) Daily.Record(d, p, Daily.Number(d, 2026, 10, 1), 10, false); // seria: poprzedni dzień
        var m = Meta.Mods(d, p, cls); // przed StartRun: ranga pamiątki z budów przed tą; v0.21.52 cz. b: mistrzostwo zawodu
        var g = new Game(d);
        if (weekly > 0) Weekly.Start(g, weekly); // v0.21.50 cz. 4: wyzwanie tygodnia – zawód, seed i zasady z tygodnia
        else if (daily > 0) Daily.Start(g, daily); // codzienna budowa: zawód i seed z dnia, bez Szkoleń
        else g.NewRun(cls, seed, diff, m, contract);
        Meta.StartRun(d, p);

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
            g.ComboEvents = 0;
        }

        CheckSnapshot(0);
        var didNg = false;
        var step = 0;
        for (; step < steps; ++step)
        {
            if (g.St == GameStatus.StageClear)
            {
                Meta.CheckBadges(d, p, g);
                Meta.CheckContracts(d, p);
                Secrets.Check(d, p, g);
                Meta.BankXp(p, g);
                DailyTasks.Bank(d, p, g, goldenDay, goldenWeek); // v0.21.52 cz. c: zadania na końcu etapu (znak wodny)
                if (g.ActCleared && shop && !g.ShopClosed) Bot.Shop(g);
                g.BotUpgrade(); // v0.21.50 cz. 3: jak bot balansu – ulepszenie narzędzia, jeśli stać
                if (paths != 0) g.ChoosePath(g.Stage & 1);
                // v0.21.50 cz. 2: premia 1 z 3 – bot z rdzenia; ścieżki na przemian: też losowanie (płatne) i wybór wg etapu
                if (g.BotWantsReroll()) g.RerollBoons();
                if (paths != 0 && g.Stage % 4 == 1 && g.CanReroll()) g.RerollBoons();
                if (g.HasBoonOffer) g.PickBoon(paths != 0 ? g.Stage % 3 : g.BotBoonChoice());
                g.NextStage();
                CheckSnapshot(step);
                CheckDigest(step);
                continue;
            }
            if (g.St == GameStatus.Won && ngplus && !didNg)
            {
                if (g.Score > p.Best) p.Best = g.Score;
                Meta.RecordWin(d, p);
                Career.Win(d, p, g);
                Meta.AddHouse(p, g);
                Meta.CheckBadges(d, p, g);
                Meta.CheckContracts(d, p);
                Secrets.Check(d, p, g);
                Meta.BankXp(p, g);
                Progress.Bank(d, p, g); // v0.21.52 cz. b: inspektor i mistrzostwo (NG+ – dalej znak wodny)
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
            Meta.RecordWin(d, p);
            Career.Win(d, p, g);
            Meta.AddHouse(p, g);
        }
        Meta.CheckBadges(d, p, g);
        Meta.CheckContracts(d, p);
        Secrets.Check(d, p, g);
        Meta.BankXp(p, g);
        if (g.Daily) Daily.Record(d, p, g.DailyDay, g.Score, g.St == GameStatus.Won);
        if (g.WeeklyWeek != 0) Weekly.Record(d, p, g.WeeklyWeek, g.Score, g.St == GameStatus.Won);
        Progress.Bank(d, p, g); // v0.21.52 cz. b: poziom inspektora i mistrzostwo (przed fabułą: wątki inspektora)
        DailyTasks.Bank(d, p, g, goldenDay, goldenWeek); // v0.21.52 cz. c: zadania dnia i tygodnia, kolekcje
        CollectionBook.Check(d, p);
        Story.Check(d, p, g); // v0.21.50 cz. 4: fabuła – wątki za kamienie milowe

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
