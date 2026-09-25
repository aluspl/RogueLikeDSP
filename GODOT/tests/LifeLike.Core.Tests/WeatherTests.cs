namespace LifeLike.Core.Tests;

// core_tests.cpp (v0.21.47): 34 – pogoda dnia (losowanie z listy etapu, upał, wiatr, mróz, deszcz, bez stosu niekorzystnych).
public class WeatherTests
{
    private static GameData D => TestData.D;

    private static int WeatherOf(WeatherEffect e) => Array.FindIndex(D.Weather, w => w.Effect == e);

    [Fact]
    public void WeatherRollsFromStageListWithoutBadStack()
    {
        var seen = new int[D.Stages.Length, D.Weather.Length];
        var badStack = 0;
        for (var k = 0; k < 120; ++k)
        {
            var g = TestData.Run(k % D.Classes.Length, (uint)(900 + k * 17));
            for (var st = 0; st < D.Stages.Length; ++st)
            {
                if (st > 0) g.NextStage();
                Assert.InRange(g.Weather, 0, D.Weather.Length - 1);
                Assert.True((D.Weather[g.Weather].StagesMask & (1 << st)) != 0);
                seen[st, g.Weather]++;
                if (g.StageEvent >= 0 && g.WDef.Bad && !D.SiteEvents[g.StageEvent].Good) badStack++;
            }
        }
        if (D.WeatherNoBadStack) Assert.Equal(0, badStack);
        for (var st = 0; st < D.Stages.Length; ++st)
        {
            for (var w = 0; w < D.Weather.Length; ++w)
            {
                if ((D.Weather[w].StagesMask & (1 << st)) == 0) Assert.Equal(0, seen[st, w]);
            }
        }
        foreach (var e in new[] { WeatherEffect.None, WeatherEffect.Heat, WeatherEffect.Frost, WeatherEffect.Wind, WeatherEffect.Rain })
            Assert.True(WeatherOf(e) >= 0);
    }

    [Fact]
    public void HeatSlowsAbilityAndWindShortensRangedWeapons()
    {
        var h = TestData.Arena(0);
        var cd = h.AbilityCooldown();
        h.Weather = (sbyte)WeatherOf(WeatherEffect.Heat);
        Assert.Equal(cd + D.Weather[h.Weather].Value, h.AbilityCooldown());
        for (var c = 0; c < D.Classes.Length; ++c)
        {
            var w = TestData.Arena(c);
            var rg = w.Weapon.Range;
            w.Weather = (sbyte)WeatherOf(WeatherEffect.Wind);
            Assert.Equal(rg > 1 ? Math.Max(1, rg - D.Weather[w.Weather].Value) : rg, w.WeaponRange());
        }
        var g = TestData.Arena(2);
        var range = g.Weapon.Range;
        Assert.True(range > 1);
        g.Spawn(D.EnemyIndex("kornik"), 7 + range, 7);
        g.UpdateFov();
        Assert.Equal(0, g.NearestTarget());
        g.Weather = (sbyte)WeatherOf(WeatherEffect.Wind);
        Assert.True(g.NearestTarget() < 0 && !g.PlayerAttack(0));
    }

    [Fact]
    public void FrostStopsProblemsEveryFewTurns()
    {
        var f = TestData.Arena(1);
        f.Weather = (sbyte)WeatherOf(WeatherEffect.Frost);
        f.Spawn(D.EnemyIndex("kornik"), 12, 7);
        f.Enemies[0].Awake = true;
        f.Hero.MaxHp = f.Hero.Hp = 999;
        int moved = 0, stood = 0;
        for (var t = 0; t < 12; ++t)
        {
            int ox = f.Enemies[0].X;
            f.PlayerWait();
            var frozen = f.Turns % D.Weather[f.Weather].Value == 0;
            if (Game.Cheb(f.Enemies[0].X, f.Enemies[0].Y, f.Hero.X, f.Hero.Y) <= 1) break;
            if (f.Enemies[0].X == ox) stood++;
            else moved++;
            Assert.Equal(frozen, f.Enemies[0].X == ox);
        }
        Assert.True(stood >= 1 && moved >= 2);
    }

    [Fact]
    public void RainPuddlesOnFloorMakeHeroSlip()
    {
        var r = TestData.Arena(1);
        r.Weather = (sbyte)WeatherOf(WeatherEffect.Rain);
        int puddles = 0, floors = 0;
        for (var y = 0; y < Level.H; ++y)
        {
            for (var x = 0; x < Level.W; ++x)
            {
                if (r.Puddle(x, y))
                {
                    puddles++;
                    Assert.Equal(Tile.Floor, r.Lv.At(x, y));
                }
                if (r.Lv.At(x, y) == Tile.Floor) floors++;
            }
        }
        Assert.True(puddles > 0 && puddles < floors / 3);
        Assert.False(TestData.Arena(1).Puddle(7, 7));
        int px = -1, py = 7;
        for (var x = 2; x <= 13 && px < 0; ++x)
        {
            if (r.Puddle(x, 7) && !r.Puddle(x - 1, 7)) px = x;
        }
        if (px < 0)
        {
            py = 8;
            for (var x = 2; x <= 13 && px < 0; ++x)
            {
                if (r.Puddle(x, 8) && !r.Puddle(x - 1, 8)) px = x;
            }
        }
        Assert.True(px >= 0);
        r.Hero.X = (sbyte)(px - 1);
        r.Hero.Y = (sbyte)py;
        r.UpdateFov();
        Assert.Equal(0, r.StatusTurns(StatusEffect.Slip));
        Assert.True(r.PlayerMove(1, 0) && r.Hero.X == px && r.StatusTurns(StatusEffect.Slip) > 0);
    }
}
