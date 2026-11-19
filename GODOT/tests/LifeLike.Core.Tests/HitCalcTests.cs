namespace LifeLike.Core.Tests;

/// <summary>v0.23.0: mnożnik ciosu wg typu broni, materiału i żywiołu celu oraz efekty typów (obuch, cięcie, kłucie, ogień).</summary>
public class HitCalcTests
{
    private static GameData D => TestData.D;

    private static int Wi(string id) => Array.FindIndex(D.Weapons, w => w.Id == id);

    private static int Ei(string id) => Array.FindIndex(D.Enemies, e => e.Id == id);

    [Fact]
    public void BluntCrushesCement() => Assert.Equal(125, HitCalc.Pct(D, D.Weapons[Wi("mlot")], D.Enemies[Ei("kamien")]));

    [Fact]
    public void CutOnSteel75() => Assert.Equal(75, HitCalc.Pct(D, D.Weapons[Wi("siekiera")], D.Enemies[Ei("dostawa")]));

    [Fact]
    public void WaterBeatsMaterial()
    {
        // Przeciek: cement, ale żywioł woda – kolumna woda (75), nie cement (125); v0.23.0 Task 7: tabela ±25%
        Assert.Equal(75, HitCalc.Pct(D, D.Weapons[Wi("mlot")], D.Enemies[Ei("przeciek")]));
    }

    [Fact]
    public void BossNeutral() => Assert.Equal(100, HitCalc.Pct(D, D.Weapons[Wi("mlot")], D.Enemies[Ei("termin")]));

    [Theory]
    [InlineData("nawalnica")]
    [InlineData("strych")]
    public void WaterBossesUseWaterColumn(string boss)
    {
        // bossy żywiołu wody (bez materiału) mają słabość żywiołu: kolumna „woda”, nie 100% – decyzja projektanta v0.23
        var e = D.Enemies[Ei(boss)];
        Assert.Equal(3, HitCalc.Column(D, e));
        Assert.Equal(75, HitCalc.Pct(D, D.Weapons[Wi("mlot")], e)); // obuch
        Assert.Equal(100, HitCalc.Pct(D, D.Weapons[Wi("siekiera")], e)); // cięcie
        Assert.Equal(75, HitCalc.Pct(D, D.Weapons[Wi("noz")], e)); // kłucie
        Assert.Equal(112, HitCalc.Pct(D, D.Weapons[Wi("probnik")], e)); // kłucie 75 × prąd 150%
        Assert.Equal(37, HitCalc.Pct(D, D.Weapons[Wi("lutownica")], e)); // kłucie 75 × ogień 50%
        Assert.Equal(100, HitCalc.Pct(D, D.Weapons[Wi("noz_majstra")], e)); // czyste kłucie
    }

    [Fact]
    public void SparkOnSteel()
    {
        var w = D.Weapons[Wi("szlifierka")];
        Assert.True(HitCalc.Has(w, HitType.Cut));
        Assert.Equal(75 * D.HitTypes.SparkSteelPct / 100, HitCalc.Pct(D, w, D.Enemies[Ei("dostawa")]));
    }

    [Fact]
    public void FireDriesWet()
    {
        var g = TestData.Arena(0);
        g.WeaponOverride = Wi("lutownica");
        g.Spawn(Ei("plesn"), 8, 7);
        g.Enemies[0].Wet = 3;
        g.Enemies[0].Hp = g.Enemies[0].MaxHp = 500;
        g.HeroAttack(0);
        Assert.Equal(0, g.Enemies[0].Wet);
    }

    [Fact]
    public void BluntStunRoll()
    {
        var d = D.WithHitTypes(D.HitTypes with { StunChancePct = 100 });
        var g = new Game(d);
        g.NewRun(0, 77);
        g.Lv.Fill(Tile.Wall);
        for (var y = 1; y <= 14; ++y)
            for (var x = 1; x <= 14; ++x) g.Lv[x, y] = Tile.Floor;
        g.EnemiesCount = 0;
        g.StairsX = g.StairsY = -1;
        g.Hero.X = g.Hero.Y = 7;
        g.WeaponOverride = Wi("mlot");
        g.Spawn(Ei("kamien"), 8, 7);
        g.Enemies[0].Hp = g.Enemies[0].MaxHp = 500;
        g.HeroAttack(0);
        Assert.Equal(1, g.Enemies[0].Stun);
        // boss: bez ogłuszenia
        g.Boss = g.EnemiesCount;
        g.Spawn(Ei("termin"), 8, 8);
        g.Enemies[g.Boss].Hp = g.Enemies[g.Boss].MaxHp = 500;
        g.HeroAttack(g.Boss);
        Assert.Equal(0, g.Enemies[g.Boss].Stun);
    }
}
