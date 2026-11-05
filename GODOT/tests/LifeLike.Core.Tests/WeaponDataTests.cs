namespace LifeLike.Core.Tests;

/// <summary>v0.23.0: dane broni – typy ciosu, tryby A/B, rzadkość, 17 nowych broni, ogień, pas monterski.</summary>
public class WeaponDataTests
{
    private static GameData D => TestData.D;

    private static WeaponDef W(string id) => D.Weapons.First(w => w.Id == id);

    [Fact]
    public void AllWeaponsHaveHitAndModes()
    {
        Assert.Equal(39, D.Weapons.Length);
        Assert.All(D.Weapons, w =>
        {
            Assert.NotEmpty(w.Hit);
            Assert.True(w.ModeB.Cooldown > 0, w.Id);
        });
    }

    [Fact]
    public void LegacyRangeBecomesModeA()
    {
        Assert.Equal(PatternKind.Arc, W("kielnia").ModeA.Pattern);
        Assert.Equal((PatternKind.Shot, 3), (W("gwozdziarka").ModeA.Pattern, W("gwozdziarka").ModeA.Range));
    }

    [Fact]
    public void LasersModeB()
    {
        Assert.Equal(PatternKind.LineBoth, W("laser").ModeB.Pattern);
        Assert.Equal(PatternKind.Line4, W("laser_krzyzowy").ModeB.Pattern);
    }

    [Fact]
    public void FireWeapons() => Assert.All(new[] { "lutownica", "opalarka", "palnik" }, id => Assert.Equal(Element.Fire, W(id).Elem));

    [Fact]
    public void RaritiesAndBelt()
    {
        Assert.Equal(new[] { "Zwykła", "Solidna", "Markowa", "Mistrzowska" }, D.WeaponRarities.Select(r => r.Name));
        Assert.Equal(new[] { "#8a8796", "#2f8fd6", "#6b4eff", "#d9a400" }, D.WeaponRarities.Select(r => r.Color));
        Assert.Contains(D.Gear, i => i.Name == "Pas monterski z kaburą");
    }

    [Fact]
    public void NewWeaponsHaveSourcesAndStayOutsideProfileToolMask()
    {
        Assert.Equal(17, D.NewTools.Length);
        Assert.All(D.NewTools, t => Assert.True(t.Weapon >= 22 && (t.Shop || t.Drop || t.Hurtownia || t.Secret)));
        Assert.Equal(10, D.Tools.Length);
    }

    [Fact]
    public void HitTypesTableUsesMaterialOrder()
    {
        var h = D.HitTypes;
        var stal = Array.FindIndex(D.Materials, m => m.Id == "stal");
        var drewno = Array.FindIndex(D.Materials, m => m.Id == "drewno");
        var cement = Array.FindIndex(D.Materials, m => m.Id == "cement");
        Assert.Equal(75, h.MaterialPct[(int)HitType.Blunt, stal]);
        Assert.Equal(150, h.MaterialPct[(int)HitType.Blunt, cement]);
        Assert.Equal(150, h.MaterialPct[(int)HitType.Cut, drewno]);
        Assert.Equal(125, h.MaterialPct[(int)HitType.Pierce, stal]);
        Assert.Equal(50, h.MaterialPct[(int)HitType.Blunt, 3]);
        Assert.Equal((20, 10, 2, 150, 150, 150, 50, 2), (h.StunChancePct, h.CutCritPct, h.PierceDef, h.SparkSteelPct, h.PowerWaterPct, h.FireWoodPct, h.FireWaterPct, h.DotPerTurn));
    }

    [Fact]
    public void ModesAndNewStatuses()
    {
        Assert.Equal(new[] { HitType.Blunt, HitType.Pierce }, W("udarowy").Hit);
        Assert.Equal((ModeEffect.OpenWall, 3, 1), (W("przecinak").ModeB.Effect, W("przecinak").ModeB.Value, W("przecinak").ModeB.Cooldown));
        Assert.Equal((ModeEffect.Stun, 20), (W("lopata").ModeA.Effect, W("lopata").ModeA.Value));
        Assert.Equal(new[] { "mlot_zenka", "poziomica_m", "pilarka" }, D.Weapons.Where(w => w.Master).Select(w => w.Id));
        Assert.Equal("Podpalenie", D.Statuses[(int)StatusEffect.Burn].Name);
        Assert.Equal("Oznaczony", D.Statuses[(int)StatusEffect.Mark].Name);
    }
}
