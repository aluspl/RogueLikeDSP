namespace LifeLike.Core.Tests;

// core_tests.cpp (v0.21.47): 36 – tryb inwestora (odblokowanie po wygranej, stawka, skutki modyfikatorów, rekord stawki).
public class InvestorTests
{
    private static GameData D => TestData.D;

    private static int Of(InvestorEffect e) => Array.FindIndex(D.Investor, x => x.Effect == e);

    private static int All => (1 << D.Investor.Length) - 1;

    [Fact]
    public void UnlockedAfterFirstWinWithXpBonus()
    {
        foreach (var e in Enum.GetValues<InvestorEffect>()) Assert.True(Of(e) >= 0);
        var p = Meta.NewProfile(D);
        p.Investor = (byte)All;
        Assert.True(!Meta.InvestorUnlocked(p) && Meta.Mods(D, p).Investor == 0);
        var xp0 = Meta.Mods(D, p).XpPct;
        p.Wins = 1;
        var m = Meta.Mods(D, p);
        Assert.True(m.Investor == All && m.XpPct == xp0 + Investor.Xp(D, All) && Investor.Stake(D, All) > 0);
        Meta.ToggleInvestor(p, 0);
        Assert.Equal(All & ~1, Meta.Mods(D, p).Investor);
    }

    [Fact]
    public void ModifiersChangeTheRun()
    {
        var p = Meta.NewProfile(D);
        p.Wins = 1;
        p.Investor = (byte)All;
        var m = Meta.Mods(D, p);
        var a = TestData.Run(1, 55);
        var b = new Game(D);
        b.NewRun(1, 55, D.DefaultDifficulty, m);
        Assert.Equal(a.EnemyHpPct() * (100 + D.Investor[Of(InvestorEffect.EnemyHp)].Value) / 100, b.EnemyHpPct());
        Assert.Equal(a.EnemyDmgBonus() + D.Investor[Of(InvestorEffect.EnemyDmg)].Value, b.EnemyDmgBonus());
        Assert.True(b.Income(100) == 100 + D.Investor[Of(InvestorEffect.CashPct)].Value && a.Income(100) == 100);
        Assert.True(b.SlamEvery() == D.SlamEvery - D.Investor[Of(InvestorEffect.Slam)].Value && a.SlamEvery() == D.SlamEvery);
        Assert.True(b.ShopClosed && !a.ShopClosed);
        a.Hero.Hp = 5;
        b.Hero.Hp = 5;
        a.DebugSkip();
        b.DebugSkip();
        a.NextStage();
        b.NextStage();
        Assert.True(a.Hero.Hp == 10 && b.Hero.Hp == 5);
    }

    [Fact]
    public void BestStakeOnlyForWinsPerClass()
    {
        var q = Meta.NewProfile(D);
        q.Wins = 1;
        q.Investor = (byte)All;
        var w = new Game(D);
        w.NewRun(3, 9, D.DefaultDifficulty, Meta.Mods(D, q));
        Meta.RecordRun(D, q, w);
        Assert.Equal(0, q.BestStake[3]);
        w.St = GameStatus.Won;
        Meta.RecordRun(D, q, w);
        Assert.True(q.BestStake[3] == Investor.Stake(D, All) && q.BestStake[2] == 0);
        q.Investor = 1;
        var w2 = new Game(D);
        w2.NewRun(3, 9, D.DefaultDifficulty, Meta.Mods(D, q));
        w2.St = GameStatus.Won;
        Meta.RecordRun(D, q, w2);
        Assert.Equal(Investor.Stake(D, All), q.BestStake[3]);
    }
}
