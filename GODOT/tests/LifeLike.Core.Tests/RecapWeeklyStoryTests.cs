namespace LifeLike.Core.Tests;

/// <summary>
/// core_tests.cpp: 49 (v0.21.50 cz. 4) – podsumowanie budowy (ciosy, oś czasu, rada), wyzwania tygodnia (seed, zasady,
/// wyniki), fabuła (wątki za kamienie milowe), migracja profilu v10 -> v11.
/// </summary>
public class RecapWeeklyStoryTests
{
    private static GameData D => TestData.D;

    private static (int A, int B) PlainEnemies()
    {
        int a = -1, b = -1;
        for (var e = 0; e < D.Enemies.Length; ++e)
        {
            var ed = D.Enemies[e];
            if (ed.Elem != Element.None || ed.OnHit != StatusEffect.None || ed.Slam || ed.Tags != 0) continue;
            if (a < 0) a = e;
            else if (b < 0) b = e;
        }
        return (a, b);
    }

    [Fact]
    public void LastHitsNewestFirstWithEliteName()
    {
        var g = TestData.Arena(1);
        var (ea, eb) = PlainEnemies();
        g.Spawn(ea, 8, 7);
        g.Spawn(eb, 6, 7);
        g.MakeElite(1, 0);
        int hp = g.Hero.Hp;
        g.EnemyStrike(0, false);
        var d0 = hp - g.Hero.Hp;
        hp = g.Hero.Hp;
        g.EnemyStrike(1, true);
        var d1 = hp - g.Hero.Hp;
        hp = g.Hero.Hp;
        g.EnemyStrike(0, false);
        var d2 = hp - g.Hero.Hp;
        Assert.True(d0 > 0 && d1 > 0 && d2 > 0);
        Assert.Equal(ea, g.LastHits[0].Src);
        Assert.Equal(d2, g.LastHits[0].Amount);
        Assert.Equal((byte)RecapKind.Ranged, g.LastHits[1].Kind);
        Assert.Equal(0, g.LastHits[1].Elite);
        Assert.Equal(d0, g.LastHits[2].Amount);
        Assert.Equal(Math.Max(d0, Math.Max(d1, d2)), g.WorstHit.Amount);
        Assert.Equal($"{D.Elites[0].Prefix[D.Enemies[eb].Gender]} {D.Enemies[eb].Name}: -{d1} (z dystansu)", g.RecapHitText(g.LastHits[1]));
        g.Hero.Hp = 1;
        g.EnemyStrike(0, false);
        Assert.Equal(GameStatus.Dead, g.St);
        Assert.Equal($"{D.RecapVerbs[D.Enemies[ea].Gender]} Cię: {D.Enemies[ea].Name}", g.RecapKiller(new Message()).Text);
        Assert.StartsWith("1/", g.RecapWhere(new Message()).Text);
    }

    [Fact]
    public void TimelineMatchesRun()
    {
        var dead = 0;
        for (uint seed = 1; seed <= 25; ++seed)
        {
            var g = TestData.Run((int)(seed % (uint)D.Classes.Length), seed * 977u);
            for (var step = 0; step < 4000; ++step)
            {
                if (g.St == GameStatus.StageClear)
                {
                    Bot.Next(g);
                    continue;
                }
                if (g.St != GameStatus.Playing) break;
                Bot.Step(g);
            }
            var sum = 0;
            for (var s = g.FirstStage; s <= g.Stage; ++s) sum += g.RecapKills(s);
            Assert.Equal(g.Kills, sum);
            var lines = g.RecapTimeline();
            Assert.Equal(g.Stage - g.FirstStage + 1, lines.Count(l => !l.Text.Text.StartsWith(' ')));
            Assert.Equal(g.StageBoon.Count(b => b >= 0), lines.Count(l => l.Text.Text.StartsWith("  Premia: ")));
            Assert.Equal(g.StageEventLog.Count(e => e != 255), lines.Count(l => l.Text.Text.StartsWith("  SMS: ")));
            if (g.St != GameStatus.Dead) continue;
            Assert.Equal(LogKind.Bad, lines[^1].Ink);
            Assert.Equal(g.Stage, g.LastHits[0].Stage);
            ++dead;
        }
        Assert.True(dead > 3);
    }

    [Fact]
    public void TipAndGoal()
    {
        var g = TestData.Arena(1);
        g.Spawn(PlainEnemies().A, 8, 7);
        g.Hero.Hp = 1;
        g.Thermos = 1;
        g.EnemyStrike(0, false);
        Assert.Equal(RecapTip.Coffee, D.RecapTips[Recap.TipIndex(D, g)].When);
        g.Thermos = 0;
        Assert.Equal(RecapTip.NoCombo, D.RecapTips[Recap.TipIndex(D, g)].When);
        var p = Meta.NewProfile(D);
        Assert.True(Recap.Goal(D, p, out var lead, out var name) && lead.StartsWith("Jeszcze ") && name.EndsWith(" I"));
        p.Respect = 9999;
        Assert.True(Recap.Goal(D, p, out lead, out _) && lead.StartsWith("Stać Cię"));
    }

    [Fact]
    public void WeeklySeedAndRules()
    {
        Assert.Equal(1, Weekly.Number(D, D.WeeklyEpoch[0], D.WeeklyEpoch[1], D.WeeklyEpoch[2]));
        var (y, m, dd) = Daily.CivilFromDays(Weekly.FirstDay(D, 40) + 6);
        Assert.Equal(40, Weekly.Number(D, y, m, dd));
        Assert.NotEqual(Weekly.Seed(40), Weekly.Seed(41));
        for (var w = 1; w <= D.Weekly.Length; ++w)
        {
            var a = new Game(D);
            var b = new Game(D);
            Weekly.Start(a, w);
            Weekly.Start(b, w);
            Assert.Equal(a.ToBytes(), b.ToBytes());
            Assert.Equal(Weekly.Index(D, w), a.Bonus.Weekly);
            Assert.Equal(w, a.WeeklyWeek);
            foreach (var r in D.Weekly[Weekly.Index(D, w)].Rules)
            {
                if (r.Rule == WeeklyRule.Cls) Assert.Equal(r.Value, a.Cls);
                if (r.Rule == WeeklyRule.NoShop) Assert.True(a.ShopClosed);
                if (r.Rule == WeeklyRule.DmgPct) Assert.Equal(r.Value, a.Bonus.DmgPct);
                if (r.Rule == WeeklyRule.Weather) Assert.Equal(r.Value, a.Weather);
                if (r.Rule == WeeklyRule.NoCoffee)
                {
                    a.Thermos = 1;
                    a.Hero.Hp = 1;
                    Assert.False(a.PlayerDrink());
                }
            }
        }
        var p = Meta.NewProfile(D);
        Assert.True(Weekly.Best(D, p, 5) < 0 && Weekly.Record(D, p, 5, 100, false) && Weekly.Best(D, p, 5) == 100);
        Assert.True(!Weekly.Record(D, p, 5, 50, true) && Weekly.Won(D, p, 5));
    }

    [Fact]
    public void StoryUnlocksOnceAndMigrationFromV10()
    {
        var p = Meta.NewProfile(D);
        Assert.Equal(0u, Story.Check(D, p, null));
        p.Runs = 1;
        var got = Story.Check(D, p, null);
        Assert.True(got != 0 && Story.UnreadCount(D, p) == 1);
        Assert.Equal(0u, Story.Check(D, p, null));
        var g = TestData.Run(1, 3);
        g.ElitesKilled = 1;
        Assert.NotEqual(0u, Story.Check(D, p, g));
        p.Wins = 4;
        Assert.Equal(3, Story.EstateDecor(D, p));

        var v = Meta.NewProfile(D);
        v.Best = 777;
        v.Runs = 6;
        v.Wins = 1;
        v.Magic = Profile.MagicBytes(Profile.MagicV10);
        var raw = v.ToBytes();
        Array.Fill(raw, (byte)0xAB, Profile.V10Size, Profile.Size - Profile.V10Size);
        var q = Profile.FromBytes(raw);
        Assert.True(Meta.ProfileFix(D, q) && q.MagicIs(Profile.MagicV12) && q.Best == 777 && q.WeeklyRuns == 0 && q.WeeklyScore[2] == 0);
        Assert.True(Story.Count(D, q) >= 3 && Story.Count(D, q) == Story.UnreadCount(D, q));
        Assert.False(Meta.ProfileFix(D, q));
    }
}
