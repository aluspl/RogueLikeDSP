namespace LifeLike.Core.Tests;

/// <summary>
/// core_tests.cpp (v0.21.52 cz. c): drzewko Szkoleń, kolekcje, zadania dnia i tygodnia, seria dni,
/// profil v15 i zapis budowy ze starszego stanu.
/// </summary>
public class GoalsTests
{
    private static GameData D => TestData.D;

    private static int Enemy(string id) => Array.FindIndex(D.Enemies, e => e.Id == id);

    private static Profile FullTrunk()
    {
        var p = Meta.NewProfile(D);
        for (var i = 0; i < D.Upgrades.Length; ++i) p.Levels[i] = (byte)D.Upgrades[i].Levels;
        return p;
    }

    [Fact]
    public void TreeBranchesCoverEveryUpgradeOnce()
    {
        Assert.Equal(3, D.TreeBranches.Length);
        var seen = 0;
        foreach (var b in D.TreeBranches)
        {
            Assert.Equal(0, seen & b.Upgrades);
            seen |= b.Upgrades;
        }
        Assert.Equal((1 << D.Upgrades.Length) - 1, seen);
        foreach (var n in D.TreeNodes) Assert.True(n.Depth <= SkillTree.BranchMax(D, n.Branch));
    }

    [Fact]
    public void TreeNodeOpensChoosesAndRespecs()
    {
        var p = Meta.NewProfile(D);
        p.Xp = 10000;
        var n0 = D.TreeNodes[0];
        Assert.False(SkillTree.Open(D, p, 0));
        Assert.False(SkillTree.Choose(D, p, 0, 0));
        for (var i = 0; i < D.Upgrades.Length && SkillTree.BranchLevels(D, p, n0.Branch) < n0.Depth; ++i)
        {
            if (SkillTree.BranchOf(D, i) != n0.Branch) continue;
            while (SkillTree.BranchLevels(D, p, n0.Branch) < n0.Depth && Meta.BuyUpgrade(D, p, i)) { }
        }
        var xp0 = p.Xp;
        Assert.True(SkillTree.Open(D, p, 0) && SkillTree.Cost(D, p, 0, 0) == n0.Cost && SkillTree.Choose(D, p, 0, 0));
        Assert.True(p.Xp == xp0 - n0.Cost && SkillTree.Pick(p, 0) == 1);
        Assert.True(SkillTree.Cost(D, p, 0, 0) == -1 && !SkillTree.Choose(D, p, 0, 0));
        Assert.True(SkillTree.Cost(D, p, 0, 1) == D.TreeRespecCost && SkillTree.Choose(D, p, 0, 1) && SkillTree.Pick(p, 0) == 2);
        Assert.Equal(xp0 - n0.Cost - D.TreeRespecCost, p.Xp);
        Assert.Equal(10000 - xp0 + n0.Cost, Meta.ShopSpent(D, p)); // opłata za zmianę – nie zakup
        Assert.Equal(1, SkillTree.Picked(D, p));
    }

    [Fact]
    public void EveryTreeOptionAppliesItsEffect()
    {
        for (var n = 0; n < D.TreeNodes.Length; ++n)
        {
            for (var o = 0; o < 2; ++o)
            {
                var a = FullTrunk();
                var b = FullTrunk();
                b.Xp = 1000;
                Assert.True(SkillTree.Choose(D, b, n, o));
                var ma = Meta.Mods(D, a, 0);
                var mb = Meta.Mods(D, b, 0);
                var op = D.TreeNodes[n].Options[o];
                var got = op.Effect switch
                {
                    UpgradeEffect.Crit => mb.Crit - ma.Crit,
                    UpgradeEffect.FirstHit => mb.FirstHitBonus - ma.FirstHitBonus,
                    UpgradeEffect.Craft => mb.Craft - ma.Craft,
                    UpgradeEffect.DmgPct => mb.DmgPct - ma.DmgPct,
                    UpgradeEffect.Cooldown => mb.Cooldown - ma.Cooldown,
                    UpgradeEffect.TakenPct => mb.TakenPct - ma.TakenPct,
                    UpgradeEffect.Coffee => mb.Coffee - ma.Coffee,
                    UpgradeEffect.Dodge => mb.Dodge - ma.Dodge,
                    UpgradeEffect.Hp => mb.Hp - ma.Hp,
                    UpgradeEffect.ShopPct => mb.ShopPct - ma.ShopPct,
                    UpgradeEffect.MatsPct => mb.MatsPct - ma.MatsPct,
                    UpgradeEffect.Thermos => mb.Thermos - ma.Thermos,
                    UpgradeEffect.BrigadePct => mb.BrigadePct - ma.BrigadePct,
                    UpgradeEffect.Cash => mb.Cash - ma.Cash,
                    _ => -999,
                };
                Assert.Equal(op.Value, got);
                Assert.NotEqual("", RunMods.UpgradeLabel(op.Effect, op.Value));
                Assert.Equal(ma.Mastery & 7, mb.Mastery & 7);
            }
        }
    }

    [Fact]
    public void FirstHitBonusOnlyOnUntouchedEnemy()
    {
        var fh = -1;
        for (var n = 0; n < D.TreeNodes.Length; ++n)
        {
            for (var o = 0; o < 2; ++o)
            {
                if (D.TreeNodes[n].Options[o].Effect == UpgradeEffect.FirstHit) fh = n * 2 + o;
            }
        }
        Assert.True(fh >= 0);
        var a = FullTrunk();
        a.Xp = 1000;
        var b = FullTrunk();
        b.Xp = 1000;
        Assert.True(SkillTree.Choose(D, b, fh / 2, fh % 2));
        var bonus = D.TreeNodes[fh / 2].Options[fh % 2].Value;
        var g0 = TestData.NewGame();
        var g1 = TestData.NewGame();
        g0.NewRun(1, 4242, 1, Meta.Mods(D, a, 1));
        g1.NewRun(1, 4242, 1, Meta.Mods(D, b, 1));
        Assert.True(g1.Bonus.FirstHitBonus == bonus && g0.Bonus.FirstHitBonus == 0);
        foreach (var g in new[] { g0, g1 })
        {
            g.Enemies[0].MaxHp = g.Enemies[0].Hp = 120;
            g.Enemies[0].Alive = true;
        }
        g0.HeroAttack(0);
        g1.HeroAttack(0);
        int d0 = 120 - g0.Enemies[0].Hp, d1 = 120 - g1.Enemies[0].Hp;
        Assert.True(d1 - d0 == bonus || d1 - d0 == bonus * D.CritMultiplier);
        int h0 = g0.Enemies[0].Hp, h1 = g1.Enemies[0].Hp;
        g0.HeroAttack(0);
        g1.HeroAttack(0);
        Assert.Equal(h0 - g0.Enemies[0].Hp, h1 - g1.Enemies[0].Hp);
    }

    [Fact]
    public void CollectionsCountWithWatermarkAndAnnounceOnce()
    {
        var p = Meta.NewProfile(D);
        var g = TestData.NewGame();
        g.NewRun(0, 9, 1, Meta.Mods(D, p, 0));
        Meta.StartRun(D, p);
        int woda = Enemy("woda"), beton = Enemy("betoniarka");
        g.KillsByType[woda] = 3;
        g.KillsByType[beton] = 1;
        Meta.RecordRun(D, p, g);
        Meta.RecordRun(D, p, g);
        Assert.True(p.KillCount[woda] == 3 && p.KillCount[beton] == 1 && p.KillMark[woda] == 3);
        g.KillsByType[woda] = 5;
        Meta.RecordRun(D, p, g);
        Assert.Equal(5, p.KillCount[woda]);
        Meta.StartRun(D, p);
        Assert.Equal(0, p.KillMark[woda]);
        var g2 = TestData.NewGame();
        g2.NewRun(0, 10, 1, Meta.Mods(D, p, 0));
        g2.KillsByType[woda] = 2;
        Meta.RecordRun(D, p, g2);
        Assert.Equal(7, p.KillCount[woda]);
        var (have, need) = CollectionBook.Progress(D, p, 0);
        Assert.True(need >= 8 && have == 0 && !CollectionBook.Complete(D, p, 0));
        for (var e = 0; e < D.Enemies.Length; ++e)
        {
            if (((D.Collections[0].Enemies >> e) & 1) == 0) continue;
            Assert.False(CollectionBook.EnemyBoss(D, e));
            p.KillCount[e] = (byte)D.Collections[0].Count;
        }
        Assert.True(CollectionBook.Complete(D, p, 0) && CollectionBook.Check(D, p) == 1 && CollectionBook.Check(D, p) == 0);
        Assert.NotEqual("", CollectionBook.RewardLabel(D, 0));
        Assert.True(CollectionBook.BossesCount(D) == 5 && CollectionBook.BossAt(D, 5) == -1 && CollectionBook.EnemyBoss(D, CollectionBook.BossAt(D, 4)));
    }

    [Fact]
    public void CollectionRewardsPerkTitleHelmet()
    {
        for (var i = 0; i < D.Collections.Length; ++i)
        {
            var q = Meta.NewProfile(D);
            var cd = D.Collections[i];
            var m0 = Meta.Mods(D, q);
            int t0 = Titles.OwnedCount(D, q), h0 = Secrets.HelmetsUnlocked(D, q);
            if (cd.Kind == CollectionKind.Decor)
            {
                q.Wins = 100;
                q.InspectorXp = 1000000;
            }
            else
            {
                for (var e = 0; e < D.Enemies.Length; ++e)
                {
                    if (cd.Kind == CollectionKind.Bosses ? CollectionBook.EnemyBoss(D, e) : ((cd.Enemies >> e) & 1) != 0) q.KillCount[e] = (byte)cd.Count;
                }
            }
            Assert.True(CollectionBook.Complete(D, q, i));
            var m1 = Meta.Mods(D, q);
            if (cd.Reward.Reward == ProgressReward.Perk)
            {
                var want = m0;
                want.AddPerk(cd.Bonus);
                Assert.True(m1.Cash == want.Cash && m1.XpPct == want.XpPct && m1.Hp == want.Hp && m1.Crit == want.Crit);
            }
            if (cd.Reward.Reward == ProgressReward.Title && cd.Kind != CollectionKind.Decor) Assert.Equal(t0 + 1, Titles.OwnedCount(D, q));
            if (cd.Reward.Reward == ProgressReward.Helmet) Assert.True(Secrets.HelmetsUnlocked(D, q) == h0 + 1 && Secrets.CosmeticUnlocked(D, q, cd.Reward.Index));
        }
    }

    [Fact]
    public void TasksAreSeededDistinctAndDaily()
    {
        for (var day = 1; day < 60; ++day)
        {
            var a = Enumerable.Range(0, DailyTasks.Slots).Select(s => DailyTasks.At(D, day, 10 + day / 7, s)).ToArray();
            Assert.True(a[0] != a[1] && a[0] != a[2] && a[1] != a[2] && a[3] != a[4]);
            Assert.Same(DailyTasks.At(D, day, 5, 1), DailyTasks.At(D, day, 9, 1));
        }
        Assert.Same(DailyTasks.At(D, 3, 5, 4), DailyTasks.At(D, 40, 5, 4));
        var differ = Enumerable.Range(1, 29).Count(day => DailyTasks.At(D, day, 1, 0) != DailyTasks.At(D, day + 1, 1, 0));
        Assert.True(differ >= 15);
    }

    [Fact]
    public void TaskMetricsBankingAndRewards()
    {
        var f0 = TestData.F0;
        var p = Meta.NewProfile(D);
        int day = Daily.Number(D, 2026, 10, 3), week = Weekly.Number(D, 2026, 10, 3);
        Assert.True(DailyTasks.Roll(p, day, week) && !DailyTasks.Roll(p, day, week));
        var g = TestData.NewGame();
        g.NewRun(1, 77, 1, Meta.Mods(D, p, 1));
        Meta.StartRun(D, p);
        g.Kills = 41;
        g.ElitesKilled = 3;
        g.HelpersCalled = 2;
        g.PowersUsed = 9;
        g.CoffeeDrunk = 4;
        g.CombosRun = 5;
        g.SecretsFound = 1;
        g.KillsByType[Enemy("betoniarka")] = 1;
        g.KillsByType[Enemy("nawalnica")] = 1;
        g.StageEventLog[0] = 4;
        g.StageEventLog[3] = 9;
        g.Stage = f0 + 3;
        g.St = GameStatus.StageClear;
        Assert.True(DailyTasks.Metric(D, g, TaskKind.Kills) == 41 && DailyTasks.Metric(D, g, TaskKind.Brigade) == 2);
        Assert.True(DailyTasks.Metric(D, g, TaskKind.Bosses) == 2 && DailyTasks.Metric(D, g, TaskKind.Events) == 2 && DailyTasks.Metric(D, g, TaskKind.Stages) == 4);
        Assert.True(DailyTasks.Metric(D, g, TaskKind.Win) == 0 && DailyTasks.Metric(D, g, TaskKind.WinNoShop) == 0);
        int wantDone = 0, wantResp = 0;
        for (var s = 0; s < DailyTasks.Slots; ++s)
        {
            var td = DailyTasks.Of(D, p, s);
            Assert.Equal(Math.Min(td.Target, DailyTasks.Metric(D, g, td.Kind)), DailyTasks.ProgressLive(D, p, g, s));
            if (DailyTasks.Metric(D, g, td.Kind) < td.Target) continue;
            wantDone |= 1 << s;
            wantResp += td.Respect;
        }
        var ns = DailyTasks.Next(D, p, g);
        var hi = Enumerable.Range(0, DailyTasks.Slots).Max(s => DailyTasks.ProgressLive(D, p, g, s) * 100 / DailyTasks.Of(D, p, s).Target);
        Assert.True(ns >= 0 && DailyTasks.ProgressLive(D, p, g, ns) * 100 / DailyTasks.Of(D, p, ns).Target == hi);
        int r0 = p.Respect, got = DailyTasks.Bank(D, p, g, day, week);
        Assert.True(got == wantDone && p.TaskDone == wantDone);
        Assert.Equal(r0 + wantResp + (p.TasksTotal >= D.TaskRewards[0].Xp ? D.TaskRewards[0].Value : 0), p.Respect);
        Assert.Equal(0, DailyTasks.Bank(D, p, g, day, week));
        var wk = p.TaskProgress[3];
        Assert.True(DailyTasks.Roll(p, day + 1, week) && p.TaskProgress[0] == 0 && DailyTasks.DoneToday(p) == 0 && p.TaskProgress[3] == wk);
        // wszystkie zadania naraz: Respekt za każde i za próg liczby zadań
        var r = Meta.NewProfile(D);
        DailyTasks.Roll(r, day, week);
        Meta.StartRun(D, r);
        r.TasksTotal = (ushort)(D.TaskRewards[0].Xp - 1);
        var big = TestData.NewGame();
        big.NewRun(1, 79, 1, Meta.Mods(D, r, 1));
        big.Kills = 255;
        big.ElitesKilled = 255;
        big.HelpersCalled = 255;
        big.PowersUsed = 255;
        big.CoffeeDrunk = 255;
        big.CombosRun = 255;
        big.SecretsFound = 255;
        big.St = GameStatus.Won;
        big.Stage = D.Stages.Length - 1;
        Array.Fill(big.StageEventLog, (byte)1);
        for (var e = 0; e < D.Enemies.Length; ++e)
        {
            if (CollectionBook.EnemyBoss(D, e)) big.KillsByType[e] = 9;
        }
        var rr = r.Respect;
        Assert.Equal((1 << DailyTasks.Slots) - 1, DailyTasks.Bank(D, r, big, day, week));
        var sum = Enumerable.Range(0, DailyTasks.Slots).Sum(s => DailyTasks.Of(D, r, s).Respect);
        Assert.Equal(rr + sum + (D.TaskRewards[0].Reward == ProgressReward.Respect ? D.TaskRewards[0].Value : 0), r.Respect);
        var w = TestData.NewGame();
        w.NewRun(0, 5);
        w.St = GameStatus.Won;
        Assert.Equal(1, DailyTasks.Metric(D, w, TaskKind.WinNoShop));
        w.ShopBuys = 1;
        Assert.True(DailyTasks.Metric(D, w, TaskKind.WinNoShop) == 0 && DailyTasks.Metric(D, w, TaskKind.Win) == 1);
        var q = Meta.NewProfile(D);
        int k0 = Secrets.HelmetsUnlocked(D, q), tt0 = Titles.OwnedCount(D, q);
        q.TasksTotal = (ushort)D.TaskRewards[^1].Xp;
        Assert.Equal(k0 + D.TaskRewards.Count(l => l.Reward == ProgressReward.Helmet), Secrets.HelmetsUnlocked(D, q));
        Assert.Equal(tt0 + D.TaskRewards.Count(l => l.Reward == ProgressReward.Title), Titles.OwnedCount(D, q));
        Assert.Equal(-1, DailyTasks.NextReward(D, q));
    }

    [Fact]
    public void StreakCountsConsecutiveDaysOnly()
    {
        var p = Meta.NewProfile(D);
        Assert.True(DayStreak.Record(D, p, 100) == 0 && p.Streak == 1 && p.StreakDay == 100 && p.StreakBest == 1);
        DayStreak.Record(D, p, 100);
        Assert.Equal(1, p.Streak);
        DayStreak.Record(D, p, 101);
        Assert.Equal(2, p.Streak);
        var k3 = D.StreakRewards[0].Xp;
        var kidx = Array.FindIndex(D.Keepsakes, k => k.Streak == k3);
        Assert.True(kidx >= 0 && !Meta.KeepsakeUnlocked(D, p, kidx));
        Assert.True(DayStreak.Record(D, p, 102) == 1 && p.Streak == 3 && Meta.KeepsakeUnlocked(D, p, kidx));
        DayStreak.Record(D, p, 90);
        Assert.True(p.Streak == 3 && p.StreakDay == 102);
        Assert.True(DayStreak.Now(p, 103) == 3 && DayStreak.Now(p, 104) == 0);
        DayStreak.Record(D, p, 105);
        Assert.True(p.Streak == 1 && p.StreakBest == 3);
        for (var d = 106; d < 106 + 13; ++d) DayStreak.Record(D, p, d);
        Assert.True(p.Streak == 14 && p.StreakBest == 14 && DayStreak.NextReward(D, p) == -1);
        var z = Meta.NewProfile(D);
        z.StreakBest = 1;
        Assert.Equal(Secrets.HelmetsUnlocked(D, z) + D.StreakRewards.Count(l => l.Reward == ProgressReward.Helmet), Secrets.HelmetsUnlocked(D, p));
        Assert.Equal(Titles.OwnedCount(D, z) + D.StreakRewards.Count(l => l.Reward == ProgressReward.Title), Titles.OwnedCount(D, p));
        var q = Meta.NewProfile(D);
        Daily.Record(D, q, 50, 100, false);
        Daily.Record(D, q, 51, 90, true);
        Daily.Record(D, q, 51, 10, false);
        Assert.True(q.Streak == 2 && q.StreakDay == 51 && q.DailyRuns == 3);
    }

    [Fact]
    public void KeepsakeHelmetOfFatherReducesDamageTaken()
    {
        var k = Array.FindIndex(D.Keepsakes, x => x.Id == "kask");
        Assert.Equal(PerkEffect.TakenPct, D.Keepsakes[k].Effect);
        var m = RunMods.Default(D);
        m.AddPerk(new Perk(PerkEffect.TakenPct, 7));
        Assert.Equal(7, m.TakenPct);
        Assert.Equal("-7% otrzym. obr.", RunMods.PerkLabel(new Perk(PerkEffect.TakenPct, 7)));
    }

    [Fact]
    public void ProfileV14MigratesToV15()
    {
        var p = Meta.NewProfile(D);
        p.Best = 321;
        p.Xp = 55;
        p.InspectorXp = 777;
        p.Keepsake2 = 2;
        Meta.CatalogAdd(p, Enemy("woda"));
        Meta.CatalogAdd(p, Enemy("termin"));
        p.DailyDay[0] = 200;
        p.DailyDay[1] = 202;
        p.DailyDay[2] = 201;
        p.DailyDay[3] = 198;
        var bytes = p.ToBytes();
        Profile.MagicBytes(Profile.MagicV14).CopyTo(bytes, 0);
        for (var i = Profile.V14Size; i < bytes.Length; ++i) bytes[i] = 0xCD; // stary zapis budowy pod 256
        var v = Profile.FromBytes(bytes);
        Assert.True(Meta.ProfileFix(D, v));
        Assert.True(v.MagicText == Profile.MagicCurrent && v.Best == 321 && v.Xp == 55 && v.InspectorXp == 777 && v.Keepsake2 == 2);
        Assert.True(v.Tree == 0 && v.KillCount[Enemy("woda")] == 1 && v.KillCount[Enemy("termin")] == 1 && v.KillCount[Enemy("kamien")] == 0);
        Assert.True(v.KillMark[0] == 0 && v.TaskDay == 0 && v.TaskDone == 0 && v.TasksTotal == 0 && v.Collections == 0);
        Assert.True(v.Streak == 3 && v.StreakDay == 202 && v.StreakBest == 3);
        Assert.False(Meta.ProfileFix(D, v));
        Assert.Equal(384, v.ToBytes().Length);
        var o = Meta.NewProfile(D);
        o.Runs = 3;
        var ob = o.ToBytes();
        Profile.MagicBytes(Profile.MagicV13).CopyTo(ob, 0);
        for (var i = Profile.V13Size; i < ob.Length; ++i) ob[i] = 0xEE;
        var ov = Profile.FromBytes(ob);
        Assert.True(Meta.ProfileFix(D, ov) && ov.MagicText == Profile.MagicCurrent && ov.Tree == 0 && ov.Streak == 0 && ov.KillMark[5] == 0);
        var c = Meta.NewProfile(D);
        c.Wins = 100;
        c.InspectorXp = 1000000;
        var cb = c.ToBytes();
        Profile.MagicBytes(Profile.MagicV14).CopyTo(cb, 0);
        var cv = Profile.FromBytes(cb);
        Assert.True(Meta.ProfileFix(D, cv) && CollectionBook.Check(D, cv) == 0); // komplet sprzed migracji – bez banera
    }

    [Fact]
    public void RunSaveWithoutTaskCountersLoads()
    {
        var g = TestData.NewGame();
        g.NewRun(3, 99, 1);
        g.HelpersCalled = 7;
        g.ShopBuys = 2;
        var save = RunSave.Make(g);
        Assert.True(save.Valid(D));
        var old = save.Data[..^RunSave.V14Tail];
        var s14 = new RunSave { Magic = save.Magic, Size = (uint)old.Length, Data = old, Checksum = RunSave.Fnv1a(old) };
        Assert.True(s14.Valid(D));
        var back = s14.Load(D);
        Assert.True(back.Cls == 3 && back.HelpersCalled == 0 && back.ShopBuys == 0);
        Assert.Equal(7, save.Load(D).HelpersCalled);
    }
}
