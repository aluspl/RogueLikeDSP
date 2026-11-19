using System.Text;

namespace LifeLike.Core.Tests;

/// <summary>
/// v0.23.0 (Task 7): cele balansu z bronią (spec. 4.7 pkt 2–3) – bot grający jak gracz (tryb B, pas, znajdźki) i bot tylko
/// z bronią domyślną. Pomiar na żądanie (BALANCE_OUT=plik.md, opcjonalnie BALANCE_RUNS = przebiegi na zawód, domyślnie 300):
/// zapisuje tabelę (do docs/v0.23/balans.md) i sprawdza cele. BALANCE_QUICK=1 – tylko Normalny i bot z bronią domyślną,
/// BALANCE_QUICK=meta – tylko wiersze meta (strojenie). Stałe v0.22.1 z docs/v0.23/balans.md (Task 1).
/// </summary>
[Trait("Category", "Balance")]
public class WeaponBalanceTests
{
    private static GameData D => TestData.D;

    private static readonly Lazy<WeaponBalanceResult> Result = new(Measure);

    /// <summary>v0.22.1: wygrane % (×10) na Normalnym, na zawód (kolejność GameData.Classes).</summary>
    private static readonly int[] BaseWin10 = [367, 160, 487, 353, 140, 233, 570, 367, 230, 350, 367, 230];

    /// <summary>v0.22.1: udział porażek (% wszystkich przebiegów ×10) na zawód i etap (12 etapów).</summary>
    private static readonly int[][] BaseDeath10 =
    [
        [0, 0, 0, 0, 0, 120, 27, 23, 193, 143, 20, 107],
        [0, 0, 0, 0, 0, 73, 20, 57, 307, 210, 33, 140],
        [0, 0, 0, 0, 0, 60, 13, 10, 133, 147, 33, 117],
        [0, 0, 3, 0, 0, 163, 33, 23, 167, 113, 10, 133],
        [0, 0, 0, 0, 3, 97, 33, 43, 270, 250, 33, 130],
        [0, 0, 0, 0, 3, 90, 27, 50, 267, 173, 27, 130],
        [0, 0, 3, 0, 3, 150, 27, 13, 70, 83, 10, 70],
        [0, 0, 0, 0, 0, 97, 37, 23, 187, 140, 17, 133],
        [0, 0, 0, 0, 0, 87, 33, 43, 193, 187, 50, 177],
        [0, 0, 0, 0, 3, 180, 47, 30, 180, 93, 37, 80],
        [0, 0, 0, 0, 0, 200, 23, 33, 167, 87, 30, 93],
        [0, 0, 0, 0, 3, 60, 13, 40, 220, 207, 43, 183],
    ];

    [BalanceFact]
    public void NormalInTarget()
    {
        var r = Result.Value;
        Assert.InRange(r.Normal.OpenPct, 25.0, 35.0);
    }

    [BalanceFact]
    public void DifficultiesInTarget()
    {
        var r = Result.Value;
        Assert.InRange(r.Easy.OpenPct, 50.0, 60.0);
        Assert.InRange(r.Hard.OpenPct, 8.0, 15.0);
        Assert.True(r.Easy.OpenPct > r.Normal.OpenPct && r.Normal.OpenPct > r.Hard.OpenPct);
    }

    [BalanceFact]
    public void MetaInTarget()
    {
        var r = Result.Value;
        Assert.InRange(r.Training, 55.0, 65.0); // v0.23.0: cele meta „pół na pół” (decyzja właściciela)
        Assert.InRange(r.Respect, 70.0, 80.0);
        Assert.InRange(r.Act0, 65.0, 75.0);
        Assert.InRange(r.Investor, 5.0, 15.0);
    }

    [BalanceFact]
    public void ContractsInTarget()
    {
        var r = Result.Value;
        for (var k = 1; k < D.Career.Length; ++k)
        {
            Assert.InRange(r.ContractNone[k], 25.0, 40.0);
            Assert.InRange(r.ContractFull[k], 65.0, 80.0);
        }
    }

    /// <summary>
    /// Spec. 4.7 pkt 2: bot tylko z bronią domyślną – Normalny nie niżej niż bot z bronią tej samej wersji −5 p.p. dla żadnego
    /// zawodu. Bez bazy v0.22.1: właściciel zażyczył sobie trudniejszego Normalnego (25–28%, średnia v0.22.1 to 32,1%), więc spadek
    /// względem v0.22.1 wynika z celu, a nie z broni domyślnej.
    /// </summary>
    [BalanceFact]
    public void DefaultWeaponNotWorse()
    {
        var r = Result.Value;
        for (var c = 0; c < D.Classes.Length; ++c)
        {
            Assert.True(r.DefaultOnly.Win10[c] >= r.Normal.Win10[c] - 50, $"{D.Classes[c].Name}: {r.DefaultOnly.Win10[c] / 10.0} vs bot z bronią {r.Normal.Win10[c] / 10.0}");
        }
    }

    /// <summary>Spec. 4.7 pkt 3: udział porażek bota z bronią domyślną na żadnym etapie nie rośnie o więcej niż 10 p.p.</summary>
    [BalanceFact]
    public void DefaultWeaponStageShare()
    {
        var r = Result.Value;
        for (var c = 0; c < D.Classes.Length; ++c)
        {
            for (var s = 0; s < BaseDeath10[c].Length; ++s)
            {
                Assert.True(r.DefaultOnly.Death10[c][s] <= BaseDeath10[c][s] + 100,
                    $"{D.Classes[c].Name}, etap {s}: {r.DefaultOnly.Death10[c][s] / 10.0} vs v0.22.1 {BaseDeath10[c][s] / 10.0}");
            }
        }
    }

    private static int Runs()
    {
        var s = Environment.GetEnvironmentVariable("BALANCE_RUNS");
        return int.TryParse(s, out var n) && n > 0 ? n : 300;
    }

    private static uint Seed(int k) => (uint)(1000 + k * 7919);

    /// <summary>Wygrane i porażki na etapach na zawód (wszystkie zawody).</summary>
    private static WeaponBalanceTable Table(int diff, RunMods mods, BotOptions o, int runs)
    {
        var nc = D.Classes.Length;
        var t = new WeaponBalanceTable(nc, D.Stages.Length);
        var wins = new int[nc];
        var deaths = new int[nc, D.Stages.Length];
        Parallel.For(0, nc, c =>
        {
            for (var k = 0; k < runs; ++k)
            {
                var g = TestData.PlayOut(c, Seed(k), diff, mods, o);
                if (g.St == GameStatus.Won) wins[c]++;
                else deaths[c, g.StageId(g.Stage)]++;
            }
        });
        int open = 0, all = 0;
        for (var c = 0; c < nc; ++c)
        {
            t.Win10[c] = wins[c] * 1000 / runs;
            for (var s = 0; s < D.Stages.Length; ++s) t.Death10[c][s] = deaths[c, s] * 1000 / runs;
            all += wins[c];
            if (c < D.OpenClassesCount) open += wins[c];
        }
        t.OpenPct = open * 100.0 / (runs * D.OpenClassesCount);
        t.AllPct = all * 100.0 / (runs * nc);
        return t;
    }

    /// <summary>Wygrane % zwykłych zawodów (bez sekretów, jak core_tests.cpp) z premiami profilu i kontraktem.</summary>
    private static double Rate(Profile p, int runs, int contract = 0)
    {
        var m = Meta.Mods(D, p);
        var wins = 0;
        Parallel.For(0, D.OpenClassesCount, c =>
        {
            var w = 0;
            for (var k = 0; k < runs; ++k)
            {
                if (TestData.PlayOut(c, Seed(k), D.DefaultDifficulty, m, BotOptions.Default, contract).St == GameStatus.Won) w++;
            }
            Interlocked.Add(ref wins, w);
        });
        return wins * 100.0 / (runs * D.OpenClassesCount);
    }

    private static WeaponBalanceResult Measure()
    {
        var runs = Runs();
        var mode = Environment.GetEnvironmentVariable("BALANCE_QUICK") ?? ""; // strojenie: "1" – Normalny i domyślna, "meta" – tylko meta
        var quick = mode == "1";
        var metaOnly = mode == "meta";
        var r = new WeaponBalanceResult { Runs = runs };
        r.Normal = metaOnly ? new WeaponBalanceTable(D.Classes.Length, D.Stages.Length) : Table(D.DefaultDifficulty, RunMods.Default(D), BotOptions.Default, runs);
        r.DefaultOnly = metaOnly ? r.Normal : Table(D.DefaultDifficulty, RunMods.Default(D), BotOptions.DefaultOnly, runs);
        r.Easy = quick || metaOnly ? r.Normal : Table(0, RunMods.Default(D), BotOptions.Default, runs);
        r.Hard = quick || metaOnly ? r.Normal : Table(D.Difficulties.Length - 1, RunMods.Default(D), BotOptions.Default, runs);
        r.ContractNone = new double[D.Career.Length];
        r.ContractFull = new double[D.Career.Length];
        if (quick)
        {
            WriteReport(r);
            return r;
        }
        var none = Meta.NewProfile(D);
        var szk = Meta.NewProfile(D);
        TestData.FullTree(szk);
        var full = Meta.NewProfile(D);
        TestData.FullTree(full);
        for (var i = 0; i < D.Respect.Length; ++i)
        {
            if (D.Respect[i].Secret < 0) Meta.SetRespectRank(full, i, D.Respect[i].Ranks);
        }
        var inv = Meta.NewProfile(D);
        TestData.FullTree(inv);
        for (var i = 0; i < D.Respect.Length; ++i)
        {
            if (D.Respect[i].Secret < 0) Meta.SetRespectRank(inv, i, D.Respect[i].Ranks);
        }
        inv.Wins = 1;
        inv.Investor = (byte)((1 << D.Investor.Length) - 1);
        var fullr = Meta.NewProfile(D);
        TestData.FullTree(fullr);
        for (var i = 0; i < D.Respect.Length; ++i)
        {
            if (D.Respect[i].Secret < 0) Meta.SetRespectRank(fullr, i, D.Respect[i].Ranks);
        }
        fullr.Rewards = (byte)D.Rewards.Length;
        r.Training = Rate(szk, runs);
        r.Respect = Rate(full, runs);
        r.Investor = Rate(inv, runs);
        r.Act0 = Rate(fullr, runs);
        if (metaOnly)
        {
            WriteReport(r);
            return r;
        }
        var kr = Math.Max(1, runs / 2); // jak core_tests.cpp: 150 przebiegów na zawód
        for (var k = 1; k < D.Career.Length; ++k)
        {
            r.ContractNone[k] = Rate(none, kr, k);
            r.ContractFull[k] = Rate(fullr, kr, k);
        }
        WriteReport(r);
        return r;
    }

    private static void WriteReport(WeaponBalanceResult r)
    {
        var path = Environment.GetEnvironmentVariable("BALANCE_OUT");
        if (!string.IsNullOrEmpty(path)) File.WriteAllText(path, Report(r));
    }

    private static string P(double v) => v.ToString("F1", System.Globalization.CultureInfo.GetCultureInfo("pl-PL"));

    private static string P10(int v) => P(v / 10.0);

    private static string Report(WeaponBalanceResult r)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Przebiegi na zawód: {r.Runs} (kontrakty {Math.Max(1, r.Runs / 2)}); średnie celów = zwykłe zawody (bez sekretów), jak core_tests.cpp.");
        sb.AppendLine();
        sb.AppendLine("| Cel | Wynik | Zakres |");
        sb.AppendLine("|---|---|---|");
        sb.AppendLine($"| Łatwy | {P(r.Easy.OpenPct)} | 50–60 |");
        sb.AppendLine($"| Normalny | {P(r.Normal.OpenPct)} (wszystkie zawody {P(r.Normal.AllPct)}) | 25–35 |");
        sb.AppendLine($"| Trudny | {P(r.Hard.OpenPct)} | 8–15 |");
        sb.AppendLine($"| Pełne Szkolenia | {P(r.Training)} | 55–65 |");
        sb.AppendLine($"| + Respekt | {P(r.Respect)} | 70–80 |");
        sb.AppendLine($"| Akt 0 + meta | {P(r.Act0)} | 65–75 |");
        sb.AppendLine($"| Inwestor (wszystkie modyfikatory) | {P(r.Investor)} | 5–15 |");
        for (var k = 1; k < D.Career.Length; ++k) sb.AppendLine($"| Kontrakt {D.Career[k].Short} bez meta / pełne meta | {P(r.ContractNone[k])} / {P(r.ContractFull[k])} | 25–40 / 65–80 |");
        sb.AppendLine($"| Tylko broń domyślna, Normalny | {P(r.DefaultOnly.OpenPct)} (wszystkie {P(r.DefaultOnly.AllPct)}) | zawód ≥ bot z bronią −5 p.p. |");
        sb.AppendLine();
        sb.AppendLine("| Zawód | v0.22.1 | Bot (Normalny) | Tylko domyślna | Δ domyślna (vs bot) | Najgorszy etap domyślnej (Δ udziału porażek, p.p.) |");
        sb.AppendLine("|---|---|---|---|---|---|");
        for (var c = 0; c < D.Classes.Length; ++c)
        {
            int ws = -1, wd = int.MinValue;
            for (var s = 0; s < BaseDeath10[c].Length; ++s)
            {
                var dd = r.DefaultOnly.Death10[c][s] - BaseDeath10[c][s];
                if (dd > wd)
                {
                    wd = dd;
                    ws = s;
                }
            }
            sb.AppendLine($"| {D.Classes[c].Name} | {P10(BaseWin10[c])} | {P10(r.Normal.Win10[c])} | {P10(r.DefaultOnly.Win10[c])} | {P10(r.DefaultOnly.Win10[c] - r.Normal.Win10[c])} | {ws}: {P10(wd)} |");
        }
        sb.AppendLine();
        sb.AppendLine("Porażki na etapach (etap: udział % wszystkich przebiegów zawodu), Normalny:");
        sb.AppendLine();
        sb.AppendLine("| Zawód | Bot | Tylko domyślna |");
        sb.AppendLine("|---|---|---|");
        for (var c = 0; c < D.Classes.Length; ++c) sb.AppendLine($"| {D.Classes[c].Name} | {Stages(r.Normal.Death10[c])} | {Stages(r.DefaultOnly.Death10[c])} |");
        return sb.ToString();
    }

    private static string Stages(int[] d)
    {
        var parts = new List<string>();
        for (var s = 0; s < BaseDeath10[0].Length; ++s)
        {
            if (d[s] > 0) parts.Add($"{s}: {P10(d[s])}");
        }
        return string.Join(", ", parts);
    }
}
