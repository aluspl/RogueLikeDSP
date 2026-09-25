namespace LifeLike.Core.Tests;

// core_tests.cpp: 28a (szczęście: kryt x2, unik, różne szczęście zawodów).
public class LuckTests
{
    private static GameData D => TestData.D;

    [Fact]
    public void ClassesDifferInLuck()
    {
        Assert.True(D.Classes[5].Luck > D.Classes[1].Luck); // Glazurnik > Murarz
        for (var c = 0; c < D.Classes.Length; ++c) Assert.True(D.Classes[1].Luck <= D.Classes[c].Luck);
        var gm = TestData.Arena(1);
        Assert.True(gm.DodgePct() == 0 && gm.CritPct() == D.CritBasePct);
    }

    [Fact]
    public void CritsDoubleDamageAtExpectedRate()
    {
        int crits = 0, hits = 0;
        for (uint seed = 1; seed <= 400; ++seed)
        {
            var g = TestData.Arena(5);
            g.R.Seed(seed);
            g.Spawn(8, 8, 7);
            g.Enemies[0].Hp = g.Enemies[0].MaxHp = 500;
            g.PlayerMove(1, 0);
            for (var i = 0; i < g.HitsCount; ++i)
            {
                if (g.Hits[i].OnHero) continue;
                ++hits;
                if (g.Hits[i].Kind == HitKind.Crit)
                {
                    ++crits;
                    Assert.Equal(0, g.Hits[i].Amount % D.CritMultiplier);
                }
            }
        }
        var expect = hits * TestData.Run(5, 1).CritPct() / 100;
        Assert.True(crits > expect / 2 && crits < expect * 2, $"kryty {crits}, oczekiwane ~{expect}");
    }

    [Fact]
    public void LuckyHeroSometimesDodges()
    {
        var dodges = 0;
        for (uint seed = 1; seed <= 400; ++seed)
        {
            var g = TestData.Arena(5);
            g.R.Seed(seed);
            g.Spawn(8, 8, 7);
            g.Enemies[0].Awake = true;
            int hp = g.Hero.Hp;
            g.PlayerWait();
            if (g.HitsCount > 0 && g.Hits[0].Kind == HitKind.Dodge)
            {
                ++dodges;
                Assert.True(g.Hero.Hp >= hp);
            }
        }
        var gl = TestData.Arena(5);
        Assert.True(dodges > 400 * gl.DodgePct() / 300 && dodges < 400 * gl.DodgePct() * 3 / 100, $"uniki {dodges}");
    }

    [Fact]
    public void CritMessageAndDodgeMessage()
    {
        var g = TestData.Arena(5);
        for (uint seed = 1; seed <= 200 && !(g.HitsCount > 0 && g.Hits[0].Kind == HitKind.Crit); ++seed)
        {
            g = TestData.Arena(5);
            g.R.Seed(seed);
            g.Spawn(8, 8, 7);
            g.Enemies[0].Hp = g.Enemies[0].MaxHp = 500;
            g.HeroAttack(0);
        }
        Assert.Equal(HitKind.Crit, g.Hits[0].Kind);
        Assert.StartsWith("KRYT! ", g.Log[Game.LogLines - 1].Text);
        Assert.Equal(LogKind.Loot, g.Log[Game.LogLines - 1].Kind);
    }
}
