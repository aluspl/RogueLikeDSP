namespace LifeLike.Core.Tests;

/// <summary>
/// core_tests.cpp (v0.21.52 cz. d, #47): mapa kariery – kontrakty i trasy etapów, bliźniak, porywy Domu z poddaszem,
/// odblokowanie, nagroda za pierwszą wygraną, kolekcje bossów, profil v16 i zapis budowy ze starszego stanu.
/// </summary>
public class CareerTests
{
    private static GameData D => TestData.D;

    [Fact]
    public void ContractsHaveOwnRoutesAndCanBeFinished()
    {
        Assert.True(D.Career.Length == 5 && D.Career[0].First == 0 && D.Career[0].Count == D.StagesCount && D.Career[0].Prelude == D.PreludeStages);
        var first = 0;
        for (var c = 0; c < D.Career.Length; ++c)
        {
            var k = D.Career[c];
            Assert.True(k.First == first && k.Count is >= 6 and <= Game.MaxStages);
            first += k.Count;
            Assert.True(D.Stages[k.First + k.Count - 1].Boss >= 0);
            if (c > 0)
            {
                Assert.Contains(Enumerable.Range(0, k.Count), s => D.Stages[k.First + s].Boss == k.Boss);
                Assert.True(k.Prelude == 0 && D.Enemies[k.Boss].Slam);
            }
            var g = TestData.NewGame();
            g.NewRun(1, 77 + (uint)c, 0, RunMods.Default(D), c);
            Assert.True(g.Contract == c && g.RouteCount() == k.Count && g.Stage == k.Prelude && ReferenceEquals(g.SDef(), D.Stages[k.First + k.Prelude]));
            int shops = 0, actChanges = 0;
            for (var s = k.Prelude; s < k.Count - 1; ++s) if (D.Stages[k.First + s].Act != D.Stages[k.First + s + 1].Act) ++actChanges;
            for (var guard = 0; guard < 200 && g.St != GameStatus.Won; ++guard)
            {
                if (g.St == GameStatus.StageClear)
                {
                    if (g.ActCleared) ++shops;
                    g.NextStage();
                    continue;
                }
                Assert.Equal(GameStatus.Playing, g.St);
                Assert.True(((g.WDef.StagesMask >> g.StageId(g.Stage)) & 1) != 0);
                g.Hero.Hp = g.Hero.MaxHp;
                g.DebugSkip();
            }
            Assert.True(g.St == GameStatus.Won && g.LastStage() && shops == actChanges);
            Assert.Equal(k.Count - k.Prelude, Career.StagesDone(g));
        }
        Assert.Equal(D.Stages.Length, first);
        var x = TestData.NewGame();
        x.NewRun(1, 5, 1, RunMods.Default(D), 99);
        Assert.Equal(0, x.Contract);
    }

    [Fact]
    public void AtticContractHasStrongerGusts()
    {
        for (var c = 0; c < D.Career.Length; ++c)
        {
            var g = TestData.NewGame();
            g.NewRun(1, 9, 1, RunMods.Default(D), c);
            var s = 0;
            while (D.Acts[g.SDef(s).Act].Mechanic != ActMechanic.Gust) ++s;
            g.StartStage(s);
            var want = D.Career[c].Gust > 0 ? D.Career[c].Gust : D.Acts[g.SDef(s).Act].MechValue;
            Assert.True(g.MechValue() == want && g.GustIn() == want);
        }
    }

    [Fact]
    public void TwinHalfInheritsWeatherEventAndLeftovers()
    {
        var tw = Array.FindIndex(D.Career, k => k.Twins);
        Assert.True(tw > 0);
        var twinS = Enumerable.Range(1, D.Career[tw].Count - 1).First(s => D.Stages[D.Career[tw].First + s].Twin);
        var carried = 0;
        for (uint seed = 1; seed <= 30; ++seed)
        {
            var g = TestData.NewGame();
            g.NewRun(1, seed, 1, RunMods.Default(D), tw);
            g.StartStage(twinS - 1);
            int w = g.Weather, ev = g.StageEvent, baseCount = g.SDef(twinS).EnemyCount;
            var alive = Enumerable.Range(0, g.EnemiesCount).Count(i => g.Enemies[i].Alive && i != g.Boss);
            g.StartStage(twinS);
            Assert.True(g.Weather == w && g.StageEvent == ev);
            Assert.Equal(Math.Min(alive, D.CareerTwinCarryMax), g.TwinCarry);
            var now = Enumerable.Range(0, g.EnemiesCount).Count(i => g.Enemies[i].Alive && i != g.Boss);
            Assert.InRange(now, baseCount + g.TwinCarry, baseCount + g.TwinCarry + 1); // + strażnik magazynu
            carried += g.TwinCarry;
            var h = TestData.NewGame();
            h.NewRun(1, seed, 1, RunMods.Default(D), tw);
            h.StartStage(twinS - 1);
            for (var i = 0; i < h.EnemiesCount; ++i) h.Enemies[i].Alive = false;
            h.StartStage(twinS);
            Assert.Equal(0, h.TwinCarry);
        }
        Assert.True(carried > 0);
    }

    [Fact]
    public void UnlocksRewardsAndBest()
    {
        var p = Meta.NewProfile(D);
        Assert.True(Career.Unlocked(D, p, 0) && Career.UnlockedCount(D, p) == 1 && Career.Selected(D, p) == 0 && Career.Announce(D, p) == 0);
        p.Contract = 2;
        Assert.Equal(0, Career.Selected(D, p));
        p.Wins = 1;
        Assert.True(Career.Unlocked(D, p, 1) && !Career.Unlocked(D, p, 2) && Career.Announce(D, p) == 2 && Career.Announce(D, p) == 0);
        p.Wins = 3;
        Assert.True(Career.Unlocked(D, p, 2) && Career.Selected(D, p) == 2 && Career.Announce(D, p) == 4);
        for (var c = 1; c < D.Career.Length; ++c)
        {
            if (D.Career[c].Unlock != CareerUnlock.Inspector) continue;
            Assert.False(Career.Unlocked(D, p, c));
            var q = Meta.NewProfile(D);
            while (Progress.InspectorLevel(D, q) < D.Career[c].UnlockValue) q.InspectorXp += 50;
            Assert.True(Career.Unlocked(D, q, c));
        }
        int t0 = Titles.OwnedCount(D, p), h0 = Secrets.HelmetsUnlocked(D, p), r0 = p.Respect;
        var g = TestData.NewGame();
        g.NewRun(1, 3, 1, RunMods.Default(D), 1);
        g.St = GameStatus.Won;
        g.Stage = g.RouteCount() - 1;
        Assert.True(Career.Win(D, p, g) && Career.Won(p, 1) && p.CareerWins[1] == 1 && p.CareerBest[1] == D.Career[1].Count);
        Assert.True(p.Respect == r0 + D.Career[1].Respect && Titles.OwnedCount(D, p) == t0 + 1
                    && Secrets.HelmetsUnlocked(D, p) == h0 + (D.Career[1].Helmet >= 0 ? 1 : 0));
        Assert.True(!Career.Win(D, p, g) && p.Respect == r0 + D.Career[1].Respect && p.CareerWins[1] == 2);
        Assert.True(Career.RewardLabel(D, 1).Length > 10);
        Assert.Equal("3 wygrane", Career.UnlockLabel(D, 2));
        var dd = TestData.NewGame();
        dd.NewRun(1, 3, 1, RunMods.Default(D), 3);
        dd.StartStage(2);
        dd.St = GameStatus.Dead;
        Meta.RecordRun(D, p, dd);
        Assert.Equal(2, p.CareerBest[3]);
        // sekrety: wygrana w krótkim domku letniskowym nie liczy się do „wygraj bez kawy”
        var sp = Meta.NewProfile(D);
        var w = TestData.NewGame();
        w.NewRun(1, 4, 1, RunMods.Default(D), 1);
        w.St = GameStatus.Won;
        Assert.Equal(0, Secrets.Check(D, sp, w));
        // fabuła: wątek po bossie kontraktu
        var f = Meta.NewProfile(D);
        Meta.CatalogAdd(f, D.Career[1].Boss);
        Assert.NotEqual(0u, Story.Check(D, f, null));
    }

    [Fact]
    public void BossCollectionsSplitHouseAndCareer()
    {
        var sets = Enumerable.Range(0, D.Collections.Length).Where(i => D.Collections[i].Kind == CollectionKind.Bosses).ToArray();
        Assert.Equal(2, sets.Length);
        var p = Meta.NewProfile(D);
        for (var c = 1; c < D.Career.Length; ++c) p.KillCount[D.Career[c].Boss] = 1;
        Assert.True(CollectionBook.Complete(D, p, sets[1]) && !CollectionBook.Complete(D, p, sets[0]));
        Assert.Equal((0, 5), CollectionBook.Progress(D, p, sets[0]));
    }

    [Fact]
    public void ProfileV16MigrationAndOldRunSave()
    {
        var p = Meta.NewProfile(D);
        p.Wins = 4;
        p.Best = 99;
        p.StreakBest = 3;
        p.Magic = Profile.MagicBytes(Profile.MagicV15);
        var raw = p.ToBytes();
        for (var i = Profile.V15Size; i < raw.Length; ++i) raw[i] = 0xCD;
        var q = Profile.FromBytes(raw);
        Assert.True(Meta.ProfileFix(D, q) && q.MagicIs(Profile.MagicCurrent) && q.Wins == 4 && q.Best == 99 && q.StreakBest == 3);
        Assert.True(q.Contract == 0 && q.CareerSeen == 0 && q.CareerDone == 1 && q.CareerWins[0] == 4 && q.CareerWins[1] == 0);
        Assert.True(q.CareerBest[0] == D.StagesCount - D.PreludeStages && q.CareerBest[5] == 0);
        Assert.True(Career.Announce(D, q) == 2 + 4 && !Meta.ProfileFix(D, q));
        var z = Meta.NewProfile(D);
        z.Magic = Profile.MagicBytes(Profile.MagicV15);
        Assert.True(Meta.ProfileFix(D, z) && z.CareerDone == 0 && z.CareerWins[0] == 0);
        var o = Meta.NewProfile(D);
        o.Wins = 2;
        o.Magic = Profile.MagicBytes(Profile.MagicV14);
        var ro = o.ToBytes();
        for (var i = Profile.V14Size; i < ro.Length; ++i) ro[i] = 0xEE;
        var o2 = Profile.FromBytes(ro);
        Assert.True(Meta.ProfileFix(D, o2) && o2.CareerWins[0] == 2 && o2.CareerDone == 1 && o2.Contract == 0);
        // zapis budowy: kontrakt w stanie; stan sprzed mapy kariery (bez 2 bajtów na końcu) – Dom jednorodzinny
        var g = TestData.NewGame();
        g.NewRun(3, 99, 1, RunMods.Default(D), 2);
        var save = RunSave.Make(g);
        Assert.True(save.Valid(D) && save.Load(D).Contract == 2);
        var old = save.Data[..^RunSave.V15Tail];
        var s15 = new RunSave { Magic = save.Magic, Size = (uint)old.Length, Data = old, Checksum = RunSave.Fnv1a(old) };
        Assert.True(s15.Valid(D) && s15.Load(D).Contract == 0 && s15.Load(D).Cls == 3);
    }
}
