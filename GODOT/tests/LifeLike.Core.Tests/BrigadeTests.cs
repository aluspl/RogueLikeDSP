namespace LifeLike.Core.Tests;

// core_tests.cpp (v0.21.47): 35 – brygada (raz na etap, budżet, odblokowanie, skutki fachowców), migracja profilu v5 -> v6.
public class BrigadeTests
{
    private static GameData D => TestData.D;

    private static int HelperOf(HelperEffect e) => Array.FindIndex(D.Brigade, h => h.Effect == e);

    private static int AllHelpers => (1 << D.Brigade.Length) - 1;

    [Fact]
    public void UnlockInTrainingAndMods()
    {
        foreach (var e in new[] { HelperEffect.Reveal, HelperEffect.Pump, HelperEffect.Safety, HelperEffect.Ally }) Assert.True(HelperOf(e) >= 0);
        var p = Meta.NewProfile(D);
        for (var i = 0; i < D.Brigade.Length; ++i) Assert.Equal(D.Brigade[i].Cost == 0, Meta.HelperUnlocked(D, p, i));
        var locked = Array.FindIndex(D.Brigade, h => h.Cost > 0);
        Assert.True(locked >= 0 && !Meta.BuyHelper(D, p, locked));
        p.Xp = 500;
        Assert.True(Meta.BuyHelper(D, p, locked) && Meta.HelperUnlocked(D, p, locked) && p.Xp == 500 - D.Brigade[locked].Cost);
        Assert.False(Meta.BuyHelper(D, p, locked));
        Assert.Equal(D.StartHelpersMask | (1 << locked), Meta.Mods(D, p).Helpers);
        Assert.Equal(D.Brigade[locked].Cost, Meta.ShopSpent(D, p));
    }

    [Fact]
    public void SurveyorRevealsMapOncePerStage()
    {
        var geo = HelperOf(HelperEffect.Reveal);
        var bhp = HelperOf(HelperEffect.Safety);
        var g = TestData.Run(1, 314);
        g.Weather = 0;
        g.Cash = 0;
        Assert.Equal(HelperBlock.Cash, g.HelperBlocked(geo));
        Assert.True(!g.CallHelper(geo) && g.Turns == 0);
        g.Cash = 100;
        Assert.False(g.Explored(g.StairsX, g.StairsY));
        Assert.True(g.CallHelper(geo));
        Assert.True(g.Turns == 1 && g.Cash == 100 - D.Brigade[geo].Price && g.HelperCalled == geo);
        for (var y = 0; y < Level.H; ++y)
        {
            for (var x = 0; x < Level.W; ++x)
            {
                if (g.Lv.Passable(x, y)) Assert.True(g.Explored(x, y));
            }
        }
        Assert.Equal(HelperBlock.Used, g.HelperBlocked(bhp));
        Assert.True(!g.CallHelper(bhp) && g.Turns == 1);
        g.DebugSkip();
        Assert.Equal(GameStatus.StageClear, g.St);
        g.NextStage();
        Assert.True(g.HelperCalled < 0 && g.HelperBlocked(geo) == HelperBlock.Ok);
        var l = TestData.Run(1, 314);
        l.Cash = 999;
        var locked = Array.FindIndex(D.Brigade, h => h.Cost > 0);
        Assert.True(l.HelperBlocked(locked) == HelperBlock.Locked && !l.CallHelper(locked));
    }

    [Fact]
    public void SafetyOfficerCleansAndGuards()
    {
        var bhp = HelperOf(HelperEffect.Safety);
        var g = TestData.Arena(1);
        g.Cash = 100;
        g.ApplyStatus(StatusEffect.Poison, 5);
        g.ApplyStatus(StatusEffect.Slip, 5);
        var def0 = g.HeroDefense();
        Assert.True(g.CallHelper(bhp));
        Assert.True(g.StatusTurns(StatusEffect.Poison) == 0 && g.StatusTurns(StatusEffect.Slip) == 0);
        Assert.True(g.HeroDefense() == def0 + D.Brigade[bhp].Value && g.GuardTurns == D.Brigade[bhp].Turns);
        for (var t = 0; t < D.Brigade[bhp].Turns; ++t) g.PlayerWait();
        Assert.True(g.GuardTurns == 0 && g.HeroDefense() == def0);
    }

    [Fact]
    public void ConcretePumpHitsProblemsInReach()
    {
        var pump = HelperOf(HelperEffect.Pump);
        var g = TestData.Arena(1);
        g.Cash = 100;
        g.Bonus.Helpers = AllHelpers;
        Assert.Equal(HelperBlock.NoTarget, g.HelperBlocked(pump));
        var reach = D.Brigade[pump].Reach;
        g.Spawn(D.EnemyIndex("papierologia"), 7 + reach, 7);
        g.Spawn(D.EnemyIndex("papierologia"), 7 + reach + 2, 7);
        g.Enemies[0].Hp = g.Enemies[0].MaxHp = 50;
        g.Enemies[1].Hp = g.Enemies[1].MaxHp = 50;
        Assert.True(g.CallHelper(pump));
        Assert.True(g.Enemies[0].Hp == 50 - D.Brigade[pump].Value && g.Enemies[1].Hp == 50);
        var k = TestData.Arena(1);
        k.Cash = 100;
        k.Bonus.Helpers = AllHelpers;
        k.Spawn(D.EnemyIndex("kornik"), 8, 7);
        k.Enemies[0].Hp = 1;
        var kills = k.Kills;
        Assert.True(k.CallHelper(pump) && !k.Enemies[0].Alive && k.Kills == kills + 1);
    }

    [Fact]
    public void AllyFollowsHitsAndLeaves()
    {
        var ally = HelperOf(HelperEffect.Ally);
        var g = TestData.Arena(1);
        g.Cash = 100;
        g.Bonus.Helpers = AllHelpers;
        Assert.True(g.CallHelper(ally));
        Assert.True(g.AllyTurns == D.Brigade[ally].Turns - 1 && Game.Cheb(g.AllyX, g.AllyY, g.Hero.X, g.Hero.Y) == 1);
        Assert.True(g.Occupied(g.AllyX, g.AllyY));
        for (var s = 0; s < 3; ++s)
        {
            g.PlayerMove(1, 0);
            Assert.Equal(1, Game.Cheb(g.AllyX, g.AllyY, g.Hero.X, g.Hero.Y));
        }
        int ex = -1, ey = -1;
        int[,] around = { { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 }, { 1, 1 }, { -1, 1 }, { 1, -1 }, { -1, -1 } };
        for (var k = 0; k < 8 && ex < 0; ++k)
        {
            int x = g.AllyX + around[k, 0], y = g.AllyY + around[k, 1];
            if (g.Lv.At(x, y) != Tile.Floor || g.Occupied(x, y) || Game.Cheb(x, y, g.Hero.X, g.Hero.Y) <= 1) continue;
            ex = x;
            ey = y;
        }
        Assert.True(ex >= 0);
        g.Spawn(D.EnemyIndex("papierologia"), ex, ey);
        var ei = g.EnemiesCount - 1;
        g.Enemies[ei].Hp = g.Enemies[ei].MaxHp = 50;
        g.Enemies[ei].Stun = 50;
        g.PlayerWait();
        Assert.True(g.Enemies[ei].Hp < 50);
        while (g.AllyTurns > 0) g.PlayerWait();
        Assert.True(g.AllyX < 0 && g.AllyTurns == 0);
    }

    [Fact]
    public void ProfileV5MigratesToV6()
    {
        var p = Meta.NewProfile(D);
        p.Best = 4321;
        p.RunClean = 3;
        p.Xp = 99;
        var raw = p.ToBytes();
        Profile.MagicBytes(Profile.MagicV5).CopyTo(raw, 0);
        for (var i = Profile.V5Size; i < raw.Length; i++) raw[i] = 0xEE; // śmieci
        var v5 = Profile.FromBytes(raw);
        Assert.True(Meta.ProfileFix(D, v5) && v5.MagicIs(Profile.MagicV6));
        Assert.True(v5.Best == 4321 && v5.RunClean == 3 && v5.Xp == 99 && v5.Brigade == 0 && v5.Investor == 0);
        Assert.All(v5.BestStake, b => Assert.Equal(0, b));
    }
}
