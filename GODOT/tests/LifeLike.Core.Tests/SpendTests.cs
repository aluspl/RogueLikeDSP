namespace LifeLike.Core.Tests;

// v0.22.0 (UX A): szybki zakup w rozmowie między budowami i nagroda za następny odbiór.
public class SpendTests
{
    private static GameData D => TestData.D;

    [Fact]
    public void QuickKindNothingWhenBroke()
    {
        var p = Meta.NewProfile(D);
        p.Xp = 0;
        Assert.Equal(-1, Spend.QuickKind(D, p, out _, out _));
        Assert.False(Spend.QuickBuy(D, p));
        Assert.Equal(0, p.Xp);
    }

    [Fact]
    public void QuickBuyTwiceBuysNextCheapest()
    {
        var p = Meta.NewProfile(D);
        p.Xp = 10000;
        var c1 = Spend.QuickKind(D, p, out var k1, out var i1);
        Assert.True(c1 >= 0 && k1 is >= 0 and <= 4);
        Assert.True(Spend.QuickBuy(D, p));
        Assert.Equal(10000 - c1, p.Xp);
        var c2 = Spend.QuickKind(D, p, out var k2, out var i2);
        Assert.True(c2 >= c1 || (k2, i2) != (k1, i1));
        Assert.True(Spend.QuickBuy(D, p));
        Assert.Equal(10000 - c1 - c2, p.Xp);
    }

    [Fact]
    public void QuickKindSkipsTreeNode()
    {
        // Rodzaje 0-4 wykupione, otwarty węzeł drzewka tani: brak szybkiego zakupu, jest TreePick.
        var p = Meta.NewProfile(D);
        p.Xp = 1000000;
        while (Spend.QuickBuy(D, p)) { }
        p.Xp = 1000000;
        var node = Spend.TreePick(D, p);
        Assert.True(node >= 0);
        p.Xp = D.TreeNodes[node].Cost;
        Assert.Equal(-1, Spend.QuickKind(D, p, out _, out _));
        Assert.True(Meta.NextUnlock(D, p, out var k, out _) >= 0);
        Assert.Equal(5, k);
    }

    [Fact]
    public void TreePickNothingWhenBroke()
    {
        var p = Meta.NewProfile(D);
        p.Xp = 0;
        Assert.Equal(-1, Spend.TreePick(D, p));
    }

    [Fact]
    public void NextRewardToolHasWeapon()
    {
        var p = Meta.NewProfile(D);
        var r = Spend.NextReward(D, p, out var w);
        Assert.Equal(0, r);
        Assert.Equal(D.Rewards[0].Kind == RewardKind.Tool ? D.Tools[D.Rewards[0].Index].Weapon : -1, w);
        p.Rewards = (byte)Meta.RewardsAvailable(D);
        Assert.Equal(-1, Spend.NextReward(D, p, out _));
    }
}
