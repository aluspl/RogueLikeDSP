using System.Text;

namespace LifeLike.Core.Tests;

/// <summary>
/// v0.23.0: pomiar „przed/po” – bot, 300 przebiegów na zawód na Normalnym; wygrane % na zawód i udział porażek na etap.
/// Na żądanie: z BALANCE_OUT=ścieżka zapisuje tabelę markdown, bez niej jest raportowany jako pominięty.
/// </summary>
public class BalanceBaselineTests
{
    private const int Runs = 300;

    private static GameData D => TestData.D;

    [BalanceFact]
    [Trait("Category", "Balance")]
    public void MeasureNormalBaseline()
    {
        var path = Environment.GetEnvironmentVariable("BALANCE_OUT") ?? throw new InvalidOperationException("BALANCE_OUT");
        var nc = D.Classes.Length;
        var wins = new int[nc];
        var deaths = new int[nc, D.Stages.Length];
        Parallel.For(0, nc, c =>
        {
            for (var k = 0; k < Runs; ++k)
            {
                var g = TestData.PlayOut(c, (uint)(1000 + k * 7919), D.DefaultDifficulty, RunMods.Default(D));
                if (g.St == GameStatus.Won) wins[c]++;
                else deaths[c, g.Stage]++;
            }
        });
        var sb = new StringBuilder();
        sb.AppendLine("| Zawód | Wygrane % | Porażki na etapach (etap: udział % wszystkich przebiegów) |");
        sb.AppendLine("|---|---|---|");
        var totW = 0;
        var stageTot = new int[D.Stages.Length];
        for (var c = 0; c < nc; ++c)
        {
            totW += wins[c];
            var parts = new List<string>();
            for (var s = 0; s < D.Stages.Length; ++s)
            {
                stageTot[s] += deaths[c, s];
                if (deaths[c, s] > 0) parts.Add($"{s}: {deaths[c, s] * 100.0 / Runs:F1}");
            }
            sb.AppendLine($"| {D.Classes[c].Name} | {wins[c] * 100.0 / Runs:F1} | {string.Join(", ", parts)} |");
        }
        var all = Runs * nc;
        var tp = new List<string>();
        for (var s = 0; s < D.Stages.Length; ++s) if (stageTot[s] > 0) tp.Add($"{s}: {stageTot[s] * 100.0 / all:F1}");
        sb.AppendLine($"| **Razem** | **{totW * 100.0 / all:F1}** | {string.Join(", ", tp)} |");
        File.WriteAllText(path, sb.ToString());
    }
}
