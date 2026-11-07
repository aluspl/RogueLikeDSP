namespace LifeLike.Core.Tests;

/// <summary>v0.23.0: kryt rzadszy – 2% za punkt szczęścia, cięcie +5%, sufit szansy krytu luck.critMaxPct (35%).</summary>
public class CritCapTests
{
    private static GameData D => TestData.D;

    [Fact]
    public void CritDataValues()
    {
        Assert.Equal(2, D.CritPerLuckPct);
        Assert.Equal(5, D.HitTypes.CutCritPct);
        Assert.Equal(35, D.CritMaxPct);
    }

    [Fact]
    public void CritChanceIsCapped()
    {
        var g = TestData.Arena(0);
        g.CritBuff = 0;
        var free = g.CritPct();
        Assert.True(free < D.CritMaxPct);
        g.Bonus.Crit = 90; // premie ponad sufit
        Assert.Equal(D.CritMaxPct, g.CritPct());
        Assert.Equal(D.CritMaxPct, g.WeaponBreakdown().CritChance());
        g.CritBuff = 60; // pomiar (Miernik) też nie przebija sufitu
        Assert.Equal(D.CritMaxPct, g.CritPct());
        g.Bonus.Crit = 0;
        g.CritBuff = 0;
        Assert.Equal(free, g.CritPct());
    }

    [Fact]
    public void CapAppliesToClassBreakdown()
    {
        var m = RunMods.Default(D);
        m.Crit = 90;
        Assert.Equal(D.CritMaxPct, DmgBreakdown.ForClass(D, 0, m).CritChance());
    }

    [Fact]
    public void MissingCapMeansNoCap()
    {
        var json = File.ReadAllText(TestData.GoldenPath("game.json")).Replace("\"critMaxPct\"", "\"critMaxPctOld\"");
        var d = GameData.Parse(json);
        Assert.Equal(100, d.CritMaxPct);
    }
}
