namespace LifeLike.Core.Tests;

// core_tests.cpp: 17 (moce zawodów i ich rangi).
public class AbilityTests
{
    private static GameData D => TestData.D;

    [Fact]
    public void HydraulikFlushHealsAndPushes()
    {
        var g = TestData.Arena(4);
        Assert.True(g.AbilityCd == 0 && !g.PlayerAbility());
        Assert.Equal(0, g.Turns);
        g.Hero.Hp = 10;
        Assert.True(g.PlayerAbility() && g.Hero.Hp == 16 && g.Turns == 1);
        Assert.True(g.AbilityCd == g.AbilityCooldown() && !g.PlayerAbility());
        for (var k = 0; k < g.AbilityCooldown(); ++k) g.PlayerWait();
        Assert.Equal(0, g.AbilityCd);
        g.Spawn(4, 8, 7);
        g.Enemies[0].Awake = true;
        Assert.True(g.PlayerAbility() && g.Enemies[0].X >= 9);
    }

    [Fact]
    public void KierownikStunsVisible()
    {
        var g = TestData.Arena(0);
        Assert.False(g.PlayerAbility());
        g.Spawn(8, 8, 7);
        g.Enemies[0].Awake = true;
        int hp = g.Hero.Hp;
        Assert.True(g.PlayerAbility() && g.Enemies[0].Stun > 0);
        g.PlayerWait();
        Assert.True(g.Hero.Hp >= hp);
    }

    [Fact]
    public void MurarzWallBlocksEnemyNeverHero()
    {
        var g = TestData.Arena(1);
        Assert.False(g.PlayerAbility());
        g.Spawn(4, 11, 7);
        Assert.True(g.PlayerAbility());
        Assert.True(g.Lv.At(8, 6) == Tile.Wall && g.Lv.At(8, 7) == Tile.Wall && g.Lv.At(8, 8) == Tile.Wall);
        Assert.True(g.Lv.At(6, 7) == Tile.Floor && g.Lv.At(7, 6) == Tile.Floor && g.Lv.At(7, 8) == Tile.Floor);
        for (var k = 0; k < 8; ++k) g.PlayerWait();
        Assert.True(g.Lv.At(8, 6) == Tile.Floor && g.Lv.At(8, 7) == Tile.Floor && g.Lv.At(8, 8) == Tile.Floor);
        var c = TestData.Arena(1);
        for (var y = 1; y <= 14; ++y)
            for (var x = 1; x <= 14; ++x)
                if (y != 7) c.Lv[x, y] = Tile.Wall;
        c.UpdateFov();
        c.Spawn(4, 10, 7);
        Assert.True(c.PlayerAbility() && c.Lv.At(8, 7) == Tile.Wall && c.Lv.At(6, 7) == Tile.Floor);
    }

    [Fact]
    public void RanksShortenCooldownAndChainHitsFiveAtRankThree()
    {
        var g = TestData.Arena(3);
        Assert.Equal(1, g.AbilityRank());
        var cd1 = g.AbilityCooldown();
        g.HeroLevel = 3;
        Assert.True(g.AbilityRank() == 2 && g.AbilityCooldown() == cd1 - 2);
        g.HeroLevel = 5;
        Assert.Equal(3, g.AbilityRank());
        for (var i = 0; i < 5; ++i) g.Spawn(4, 9 + (i % 3) * 2, 7 + (i / 3) * 2);
        Assert.True(g.PlayerAbility());
        var hit = 0;
        for (var i = 0; i < 5; ++i)
            if (g.Enemies[i].Hp < D.Enemies[4].MaxHealth * g.EnemyHpPct() / 100) hit++;
        Assert.Equal(5, hit);
    }

    [Fact]
    public void CieslaVolleyHitsAllInRange()
    {
        var g = TestData.Arena(2);
        g.Spawn(4, 8, 7);
        g.Spawn(4, 10, 7);
        g.Spawn(4, 13, 7);
        int h0 = g.Enemies[0].Hp, h1 = g.Enemies[1].Hp, h2 = g.Enemies[2].Hp;
        Assert.True(g.PlayerAbility());
        Assert.True(g.Enemies[0].Hp < h0 && g.Enemies[1].Hp < h1 && g.Enemies[2].Hp == h2);
    }

    [Fact]
    public void ElektrykChainJumpsUpToTwoCells()
    {
        var g = TestData.Arena(3);
        g.Spawn(4, 9, 7);
        g.Spawn(4, 11, 7);
        g.Spawn(4, 13, 7);
        g.Spawn(4, 7, 13);
        var h = new int[4];
        for (var i = 0; i < 4; ++i) h[i] = g.Enemies[i].Hp;
        Assert.True(g.PlayerAbility());
        Assert.True(g.Enemies[0].Hp < h[0] && g.Enemies[1].Hp < h[1] && g.Enemies[2].Hp < h[2] && g.Enemies[3].Hp == h[3]);
    }

    [Fact]
    public void GlazurnikSpinHitsAdjacent()
    {
        var g = TestData.Arena(5);
        Assert.False(g.PlayerAbility());
        g.Spawn(4, 6, 6);
        g.Spawn(4, 8, 8);
        g.Spawn(4, 7, 9);
        int h0 = g.Enemies[0].Hp, h1 = g.Enemies[1].Hp, h2 = g.Enemies[2].Hp;
        Assert.True(g.PlayerAbility());
        Assert.True(g.Enemies[0].Hp < h0 && g.Enemies[1].Hp < h1 && g.Enemies[2].Hp == h2);
    }
}
