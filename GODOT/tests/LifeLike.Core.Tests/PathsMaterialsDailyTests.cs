namespace LifeLike.Core.Tests;

// core_tests.cpp (v0.21.48): 37 – wybór ścieżki, 38 – materiały i naprawy, 39 – codzienna budowa i profil v7,
// 40 – harmonogram domu, 41 – najbliższe do kupienia po budowie.
public class PathsMaterialsDailyTests
{
    private static GameData D => TestData.D;

    private static int PathWhere(Func<PathDef, bool> pred) => Array.FindIndex(D.Paths, x => pred(x));

    [Fact]
    public void PathOfferIsDeterministicAndApplied()
    {
        var g = TestData.Run(1, 777);
        for (var st = 0; st < D.Stages.Length - 1; ++st)
        {
            g.Stage = st;
            Assert.NotEqual(g.PathOffer(0), g.PathOffer(1));
            var h = TestData.Run(1, 777);
            h.Stage = st;
            Assert.True(h.PathOffer(0) == g.PathOffer(0) && h.PathOffer(1) == g.PathOffer(1));
        }
        var seen = 0;
        for (var seed = 1u; seed <= 200; ++seed)
        {
            var k = TestData.Run(0, seed);
            seen |= (1 << k.PathOffer(0)) | (1 << k.PathOffer(1));
        }
        Assert.Equal((1 << D.Paths.Length) - 1, seen);
        for (var k = 0; k < 2; ++k)
        {
            var a = TestData.Run(2, 4242);
            a.DebugSkip();
            var want = a.PathOffer(k);
            a.ChoosePath(k);
            a.NextStage();
            Assert.True(a.Stage == TestData.F0 + 1 && a.StagePath == want && a.NextPath == 0);
        }
        Assert.Equal(-1, TestData.Run(2, 99).StagePath);
        int more = PathWhere(p => p.Enemies > 0), fewer = PathWhere(p => p.Enemies < 0);
        int calm = PathWhere(p => p.NoEvent), risky = PathWhere(p => p.BadWeather), stock = PathWhere(p => p.Materials > 0);
        Assert.True(more >= 0 && fewer >= 0 && calm >= 0 && risky >= 0 && stock >= 0);
        var bs = Array.FindIndex(D.Stages, TestData.F0, s => s.Boss >= 0);
        foreach (var path in new[] { more, fewer })
        {
            var a = TestData.Run(1, 31);
            a.Cash = 50;
            a.R.Seed(1234);
            a.StartStage(bs);
            var b = TestData.Run(1, 31);
            b.Cash = 50;
            b.R.Seed(1234);
            b.StartStage(bs, path);
            Assert.Equal(a.EnemiesCount + D.Paths[path].Enemies, b.EnemiesCount);
            Assert.Equal(Math.Max(0, 50 + D.Paths[path].Cash), b.Cash);
        }
        int badOk = 0, events = 0;
        for (var seed = 1u; seed <= 100; ++seed)
        {
            var w = TestData.Run(1, seed);
            w.StartStage(TestData.F0 + 1, risky);
            badOk += w.WDef.Bad ? 1 : 0;
            var e = TestData.Run(1, seed);
            e.StartStage(TestData.F0 + 1, calm);
            events += e.StageEvent >= 0 ? 1 : 0;
        }
        Assert.True(badOk == 100 && events == 0);
        var m = TestData.Run(1, 5);
        m.StartStage(TestData.F0 + 1, stock);
        Assert.Equal(D.Paths[stock].Materials, m.Mats[0] + m.Mats[1] + m.Mats[2]);
    }

    [Fact]
    public void MaterialsDropFromProblemsBossesAndBoxes()
    {
        var kornik = D.EnemyIndex("kornik");
        var got = 0;
        for (var seed = 1u; seed <= 400; ++seed)
        {
            var g = TestData.Arena(1);
            g.R.Seed(seed);
            g.Spawn(kornik, 8, 7);
            g.Enemies[0].Hp = 1;
            g.PlayerMove(1, 0);
            var n = g.Mats[0] + g.Mats[1] + g.Mats[2];
            Assert.True(n <= 1);
            if (n == 0) continue;
            ++got;
            Assert.Equal(1, g.Mats[D.Enemies[kornik].Material]);
        }
        Assert.True(got > 400 * D.MaterialDropPct / 200 && got < 400 * D.MaterialDropPct * 2 / 100);
        var b = TestData.Arena(1);
        b.Spawn(D.EnemyIndex("betoniarka"), 8, 7);
        b.Boss = 0;
        b.Enemies[0].Hp = 1;
        b.PlayerMove(1, 0);
        for (var m = 0; m < D.Materials.Length; ++m) Assert.Equal(D.MaterialBossDrop, b.Mats[m]);
        var c = TestData.Arena(1);
        for (var k = 0; k < 30; ++k) c.AddMaterial(2);
        Assert.Equal(D.MaterialMax, c.Mats[2]);
        var q = TestData.Arena(1);
        q.Pickups[0] = new Pickup(q.Hero.X, q.Hero.Y, PickupType.GearBox, true);
        q.PickupsCount = 1;
        q.Collect();
        Assert.Equal(D.MaterialGearBox, q.Mats[0] + q.Mats[1] + q.Mats[2]);
    }

    [Fact]
    public void RepairsPatchAndBridge()
    {
        int patch = Array.FindIndex(D.Repairs, r => r.Effect == RepairEffect.Patch), bridge = Array.FindIndex(D.Repairs, r => r.Effect == RepairEffect.Bridge);
        Assert.True(patch >= 0 && bridge >= 0);
        var g = TestData.Arena(1);
        var rd = D.Repairs[patch];
        Assert.True(g.RepairBlocked(patch) == RepairBlock.Material && !g.PlayerRepair(patch) && g.Turns == 0);
        g.Mats[rd.Material] = 3;
        Assert.Equal(RepairBlock.NoTarget, g.RepairBlocked(patch));
        g.Spawn(D.EnemyIndex("kornik"), 11, 7);
        Assert.True(g.RepairBlocked(patch) == RepairBlock.Ok && g.PlayerRepair(patch));
        Assert.True(g.Turns == 1 && g.Mats[rd.Material] == 3 - rd.Cost);
        Assert.True(g.Lv.At(8, 6) == Tile.Wall && g.Lv.At(8, 7) == Tile.Wall && g.Lv.At(8, 8) == Tile.Wall && g.Walls[0].Turns == rd.Value - 1);
        for (var k = 0; k < rd.Value; ++k) g.PlayerWait();
        Assert.Equal(Tile.Floor, g.Lv.At(8, 7));

        var br = D.Repairs[bridge];
        var dry = TestData.Arena(1);
        dry.Stage = TestData.F0 + 4; // akt II: bez błota (Kładka działa też na błoto)
        dry.Mats[br.Material] = 3;
        Assert.True(dry.RepairBlocked(bridge) == RepairBlock.NoPuddle && !dry.PlayerRepair(bridge));
        var r = TestData.Arena(1);
        r.Weather = (sbyte)Array.FindIndex(D.Weather, w => w.Effect == WeatherEffect.Rain);
        r.Mats[br.Material] = 3;
        int Near()
        {
            var n = 0;
            for (var y = 7 - br.Value; y <= 7 + br.Value; ++y)
                for (var x = 7 - br.Value; x <= 7 + br.Value; ++x)
                    n += r.Puddle(x, y) ? 1 : 0;
            return n;
        }
        if (Near() == 0)
        {
            Assert.Equal(RepairBlock.NoPuddle, r.RepairBlocked(bridge));
            return;
        }
        r.ApplyStatus(StatusEffect.Slip, 3);
        Assert.True(r.PlayerRepair(bridge) && r.Mats[br.Material] == 3 - br.Cost && r.StatusTurns(StatusEffect.Slip) == 0);
        Assert.Equal(0, Near());
        r.DebugSkip();
        r.NextStage();
        Assert.Equal(0, r.Bridges);
    }

    [Fact]
    public void DailySeedClassModsAndResults()
    {
        Assert.Equal(1, Daily.Number(D, D.DailyEpoch[0], D.DailyEpoch[1], D.DailyEpoch[2]));
        Assert.Equal(1, Daily.Number(D, 2026, 9, 25) - Daily.Number(D, 2026, 9, 24));
        Assert.Equal(1, Daily.Number(D, 2027, 1, 1) - Daily.Number(D, 2026, 12, 31));
        Assert.True(Daily.DaysInMonth(2028, 2) == 29 && Daily.DaysInMonth(2026, 2) == 28);
        for (var z = 19000; z < 22000; z += 37)
        {
            var (y, m, d) = Daily.CivilFromDays(z);
            Assert.Equal(z, Daily.DaysFromCivil(y, m, d));
        }
        var day = Daily.Number(D, 2026, 9, 25);
        Assert.NotEqual(Daily.Seed(day), Daily.Seed(day + 1));
        var a = TestData.NewGame();
        Daily.Start(a, day);
        var b = TestData.NewGame();
        Daily.Start(b, day);
        Assert.True(a.Daily && a.DailyDay == day && a.Cls == Daily.ClassOf(D, Daily.Seed(day)) && a.Diff == D.DailyDifficulty);
        Assert.True(a.Bonus.Investor == Daily.InvestorOf(D, Daily.Seed(day)) && a.Bonus.Hp == 0 && a.Bonus.Def == 0);
        Assert.Equal(StateDigest.Of(a), StateDigest.Of(b));
        var bits = 0;
        for (var i = 0; i < D.Investor.Length; ++i) bits += (a.Bonus.Investor >> i) & 1;
        Assert.Equal(D.DailyInvestorMods, bits);
        var p = Meta.NewProfile(D);
        Assert.Equal((D.DailyDefaultDate[0], D.DailyDefaultDate[1], D.DailyDefaultDate[2]), Daily.DateOf(D, p));
        Daily.SetDate(p, 2026, 2, 31);
        Assert.Equal(28, Daily.DateOf(D, p).D);
        Assert.Equal(-1, Daily.Best(D, p, day));
        Assert.True(Daily.Record(D, p, day, 500, false) && Daily.Best(D, p, day) == 500 && !Daily.Won(D, p, day));
        Assert.True(!Daily.Record(D, p, day, 300, true) && Daily.Best(D, p, day) == 500 && Daily.Won(D, p, day));
        Assert.True(Daily.Record(D, p, day, 900, false) && Daily.Best(D, p, day) == 900);
        for (var k = 1; k <= D.DailyHistory; ++k) Daily.Record(D, p, day + k, 100 * k, false);
        Assert.True(Daily.Best(D, p, day) == -1 && Daily.Best(D, p, day + D.DailyHistory) == 100 * D.DailyHistory);
        Assert.Equal(3 + D.DailyHistory, p.DailyRuns);
    }

    [Fact]
    public void ProfileV6MigratesToV7AndRefundsRemovedLevels()
    {
        var v6 = Meta.NewProfile(D);
        v6.Magic = Profile.MagicBytes(Profile.MagicV6);
        v6.Best = 321;
        v6.Investor = 5;
        v6.BestStake[2] = 4;
        var bytes = v6.ToBytes();
        for (var i = Profile.V6Size; i < bytes.Length; i++) bytes[i] = 0xEE;
        var p = Profile.FromBytes(bytes);
        Assert.True(Meta.ProfileFix(D, p) && p.MagicIs(Profile.MagicCurrent) && p.Best == 321 && p.Investor == 5 && p.BestStake[2] == 4);
        Assert.True(p.DailyY == 0 && p.DailyRuns == 0 && p.DailyWon == 0 && p.DailyDay.All(x => x == 0) && p.DailyScore.All(x => x == 0));
        var c = Meta.NewProfile(D);
        c.Xp = 10;
        var ui = Array.FindIndex(D.Upgrades, u => u.Refund > 0);
        Assert.True(ui >= 0);
        c.Levels[ui] = (byte)(D.Upgrades[ui].Levels + 1);
        Assert.True(Meta.ProfileFix(D, c) && c.Levels[ui] == D.Upgrades[ui].Levels && c.Xp == 10 + D.Upgrades[ui].Refund);
        Assert.False(Meta.ProfileFix(D, c));
    }

    [Fact]
    public void HouseScheduleDaysAndDates()
    {
        var g = TestData.Run(1, 7);
        for (var k = 0; k < 5; ++k)
        {
            g.Hero.Hp = g.Hero.MaxHp = 999;
            g.PlayerWait();
        }
        g.DebugSkip();
        var f = g.FirstStage; // bez Aktu 0: od Fundamentów
        Assert.True(g.St == GameStatus.StageClear && g.StageDays[f] == g.Turns);
        while (g.St == GameStatus.StageClear)
        {
            g.NextStage();
            for (var k = 0; k < 2 + g.Stage; ++k)
            {
                g.Hero.Hp = g.Hero.MaxHp = 999;
                g.PlayerWait();
            }
            g.DebugSkip();
        }
        Assert.Equal(GameStatus.Won, g.St);
        for (var s = f + 1; s < D.Stages.Length; ++s) Assert.True(g.StageDays[s] >= 2 + s);
        var total = 0;
        for (var s = f; s < D.Stages.Length; ++s)
        {
            Assert.True(HouseSchedule.Days(g, s) >= D.ScheduleMinDays);
            total += HouseSchedule.Days(g, s);
        }
        Assert.True(HouseSchedule.TotalDays(g) == total && HouseSchedule.TotalCost(g) > 0);
        var end = Daily.DaysFromCivil(2026, 10, 1);
        Assert.Equal(end - total, HouseSchedule.StartDay(g, f, end));
        Assert.Equal(end - total + HouseSchedule.Days(g, f), HouseSchedule.StartDay(g, f + 1, end));
    }

    [Fact]
    public void NextUnlockIsCheapestUntilEverythingBought()
    {
        var p = Meta.NewProfile(D);
        var cost = Meta.NextUnlock(D, p, out var kind, out _);
        Assert.True(cost > 0 && kind >= 0);
        var cheapest = Enumerable.Range(0, D.Upgrades.Length).Min(i => Meta.UpgradeCost(D, p, i));
        Assert.True(cost <= cheapest);
        p.Xp = 1 << 20;
        for (var i = 0; i < D.Upgrades.Length; ++i)
        {
            while (Meta.BuyUpgrade(D, p, i)) { }
        }
        for (var i = 0; i < D.Classes.Length; ++i) Meta.BuyClass(D, p, i);
        for (var i = 0; i < D.Tools.Length; ++i) Meta.BuyTool(D, p, i);
        for (var i = 0; i < D.Brigade.Length; ++i) Meta.BuyHelper(D, p, i);
        Meta.BuyHard(D, p);
        Assert.Equal(-1, Meta.NextUnlock(D, p, out _, out _));
    }
}
