using System.Text;

namespace LifeLike.Core.Tests;

/// <summary>
/// v0.23.0: pomiar „odczucia walki” botem na Normalnym (na żądanie, BALANCE_OUT): średnia szansa krytu w chwili trafienia,
/// faktyczny udział krytów, obrażenia otrzymane na etap, średni unik przy ciosie problemu i wygrane. Tabela trafia do
/// BALANCE_OUT + ".walka.md" (do docs/v0.23/balans.md).
/// </summary>
[Trait("Category", "Balance")]
public class CombatFeelTests
{
    private static GameData D => TestData.D;

    private static uint Seed(int k) => (uint)(1000 + k * 7919);

    private sealed class Acc
    {
        public long Hits, Crits, CritPctSum, Strikes, Dodges, DodgePctSum, Taken, Stages, Wins, Runs;
    }

    [BalanceFact]
    public void MeasureCombatFeel()
    {
        var s = Environment.GetEnvironmentVariable("BALANCE_RUNS");
        var runs = int.TryParse(s, out var n) && n > 0 ? n : 300;
        var nc = D.Classes.Length;
        var acc = new Acc[nc];
        Parallel.For(0, nc, c =>
        {
            var a = new Acc();
            for (var k = 0; k < runs; ++k) Play(c, Seed(k), a);
            acc[c] = a;
        });
        var sb = new StringBuilder();
        sb.AppendLine($"Bot, Normalny, {runs} przebiegów na zawód. Kryt: średnia szansa w chwili trafienia bohatera / faktyczny udział krytów; unik: średnia szansa przy ciosie problemu / faktyczny udział uników.");
        sb.AppendLine();
        sb.AppendLine("| Zawód | Wygrane % | Kryt % (szansa / faktycznie) | Obrażenia otrzymane na etap | Unik % (szansa / faktycznie) |");
        sb.AppendLine("|---|---|---|---|---|");
        var t = new Acc();
        for (var c = 0; c < nc; ++c)
        {
            var a = acc[c];
            sb.AppendLine($"| {D.Classes[c].Name} | {Row(a)} |");
            t.Hits += a.Hits;
            t.Crits += a.Crits;
            t.CritPctSum += a.CritPctSum;
            t.Strikes += a.Strikes;
            t.Dodges += a.Dodges;
            t.DodgePctSum += a.DodgePctSum;
            t.Taken += a.Taken;
            t.Stages += a.Stages;
            t.Wins += a.Wins;
            t.Runs += a.Runs;
        }
        sb.AppendLine($"| **Razem** | {Row(t)} |");
        var path = (Environment.GetEnvironmentVariable("BALANCE_OUT") ?? "balans.md") + ".walka.md";
        File.WriteAllText(path, sb.ToString());
        Assert.True(t.Hits > 0 && t.Strikes > 0);
    }

    private static string F(double v) => v.ToString("0.0", System.Globalization.CultureInfo.GetCultureInfo("pl-PL"));

    private static string Row(Acc a) =>
        $"{F(a.Wins * 100.0 / a.Runs)} | {F(a.CritPctSum / (double)Math.Max(1, a.Hits))} / {F(a.Crits * 100.0 / Math.Max(1, a.Hits))} | "
        + $"{F(a.Taken / (double)Math.Max(1, a.Stages))} | {F(a.DodgePctSum / (double)Math.Max(1, a.Strikes))} / {F(a.Dodges * 100.0 / Math.Max(1, a.Strikes))}";

    /// <summary>Rozgrywka jak TestData.PlayOut, z odczytem trafień (Game.Hits) po każdym kroku bota.</summary>
    private static void Play(int cls, uint seed, Acc a)
    {
        var g = TestData.NewGame();
        g.NewRun(cls, seed, D.DefaultDifficulty, RunMods.Default(D));
        ++a.Runs;
        ++a.Stages;
        var stage = g.Stage;
        for (var step = 0; step < 4000; ++step)
        {
            if (g.St == GameStatus.StageClear)
            {
                Bot.Next(g);
                continue;
            }
            if (g.St != GameStatus.Playing) break;
            if (g.Stage != stage)
            {
                stage = g.Stage;
                ++a.Stages;
            }
            var crit = Math.Clamp(g.CritPct(), 0, 100);
            var dodge = g.DodgePct();
            g.HitsCount = 0;
            Bot.Step(g);
            for (var i = 0; i < g.HitsCount; ++i)
            {
                var h = g.Hits[i];
                if (h.OnHero)
                {
                    ++a.Strikes;
                    a.DodgePctSum += dodge;
                    if (h.Kind == HitKind.Dodge) ++a.Dodges;
                    a.Taken += h.Amount;
                }
                else
                {
                    ++a.Hits;
                    a.CritPctSum += crit;
                    if (h.Kind == HitKind.Crit) ++a.Crits;
                }
            }
        }
        if (g.St == GameStatus.Won) ++a.Wins;
    }
}
