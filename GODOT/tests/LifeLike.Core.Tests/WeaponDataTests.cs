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

    [Theory]
    [InlineData("dziennik", PatternKind.Shot, ModeEffect.None, PatternKind.Single, ModeEffect.Mark, 4)]
    [InlineData("kielnia", PatternKind.Arc, ModeEffect.None, PatternKind.Shot, ModeEffect.Stun, 3)]
    [InlineData("gwozdziarka", PatternKind.Shot, ModeEffect.None, PatternKind.Line, ModeEffect.None, 3)]
    [InlineData("probnik", PatternKind.Shot, ModeEffect.None, PatternKind.Cross, ModeEffect.Chain, 4)]
    [InlineData("klucz", PatternKind.Single, ModeEffect.None, PatternKind.Single, ModeEffect.Dry, 4)]
    [InlineData("szlifierka", PatternKind.Single, ModeEffect.None, PatternKind.Arc, ModeEffect.None, 3)]
    [InlineData("lom", PatternKind.Single, ModeEffect.None, PatternKind.Push, ModeEffect.Push, 3)]
    [InlineData("wkretarka", PatternKind.Line, ModeEffect.None, PatternKind.Single, ModeEffect.DefDown, 3)]
    [InlineData("laser", PatternKind.Line, ModeEffect.None, PatternKind.LineBoth, ModeEffect.None, 4)]
    [InlineData("mlot", PatternKind.Push, ModeEffect.Push, PatternKind.Around, ModeEffect.OpenWall, 5)]
    [InlineData("tablet", PatternKind.Single, ModeEffect.None, PatternKind.Shot, ModeEffect.Swap, 4)]
    [InlineData("miernik", PatternKind.Shot, ModeEffect.None, PatternKind.Single, ModeEffect.Reveal, 4)]
    [InlineData("dachowka", PatternKind.Shot, ModeEffect.None, PatternKind.Throw3x3, ModeEffect.None, 4)]
    [InlineData("agregat", PatternKind.Spray, ModeEffect.None, PatternKind.Single, ModeEffect.Root, 4)]
    [InlineData("lyzka", PatternKind.Arc, ModeEffect.None, PatternKind.Shot, ModeEffect.Pull, 3)]
    [InlineData("udarowy", PatternKind.Single, ModeEffect.None, PatternKind.Line, ModeEffect.DefDown, 4)]
    [InlineData("kotwy", PatternKind.Shot, ModeEffect.None, PatternKind.Single, ModeEffect.Root, 3)]
    [InlineData("spawarka", PatternKind.Line, ModeEffect.None, PatternKind.Single, ModeEffect.Dot, 3)]
    [InlineData("tyczka", PatternKind.Line, ModeEffect.None, PatternKind.Cross, ModeEffect.Mark, 4)]
    [InlineData("mlotek", PatternKind.Single, ModeEffect.None, PatternKind.Single, ModeEffect.DefDown, 2)]
    [InlineData("mlot_zenka", PatternKind.Push, ModeEffect.Push, PatternKind.Around, ModeEffect.Push, 5)]
    [InlineData("poziomica_m", PatternKind.Shot, ModeEffect.None, PatternKind.LineBoth, ModeEffect.Reveal, 4)]
    [InlineData("lopata", PatternKind.Arc, ModeEffect.Stun, PatternKind.Line, ModeEffect.DefDown, 2)]
    [InlineData("siekiera", PatternKind.Single, ModeEffect.None, PatternKind.Arc, ModeEffect.None, 3)]
    [InlineData("pila", PatternKind.Single, ModeEffect.None, PatternKind.Single, ModeEffect.Dot, 3)]
    [InlineData("kilof", PatternKind.Single, ModeEffect.None, PatternKind.Line, ModeEffect.None, 4)]
    [InlineData("przecinak", PatternKind.Single, ModeEffect.None, PatternKind.Single, ModeEffect.OpenWall, 1)]
    [InlineData("nozyce", PatternKind.Single, ModeEffect.None, PatternKind.Arc, ModeEffect.None, 3)]
    [InlineData("silikon", PatternKind.Shot, ModeEffect.None, PatternKind.Single, ModeEffect.Root, 4)]
    [InlineData("myjka", PatternKind.Line, ModeEffect.None, PatternKind.Push, ModeEffect.Push, 3)]
    [InlineData("palnik", PatternKind.Spray, ModeEffect.Dot, PatternKind.Around, ModeEffect.Dry, 5)]
    [InlineData("taker", PatternKind.Shot, ModeEffect.None, PatternKind.Single, ModeEffect.Root, 3)]
    [InlineData("mlotek_gumowy", PatternKind.Single, ModeEffect.Stun, PatternKind.Cross, ModeEffect.Stun, 4)]
    [InlineData("pilarka", PatternKind.Arc, ModeEffect.None, PatternKind.Around, ModeEffect.None, 5)]
    [InlineData("poziomica_2m", PatternKind.Shot, ModeEffect.None, PatternKind.Cross, ModeEffect.Reveal, 4)]
    [InlineData("mieszadlo", PatternKind.Single, ModeEffect.None, PatternKind.Around, ModeEffect.Push, 5)]
    [InlineData("laser_krzyzowy", PatternKind.Line, ModeEffect.None, PatternKind.Line4, ModeEffect.None, 4)]
    [InlineData("lutownica", PatternKind.Single, ModeEffect.Dot, PatternKind.Line, ModeEffect.Dot, 3)]
    [InlineData("opalarka", PatternKind.Spray, ModeEffect.Dry, PatternKind.Single, ModeEffect.Mark, 3)]
    public void EveryWeaponModesMatchCatalog(string id, PatternKind a, ModeEffect aEffect, PatternKind b, ModeEffect bEffect, int cooldownB)
    {
        var w = W(id);
        Assert.Equal((a, aEffect), (w.ModeA.Pattern, w.ModeA.Effect));
        Assert.Equal((b, bEffect, cooldownB), (w.ModeB.Pattern, w.ModeB.Effect, w.ModeB.Cooldown));
    }

    [Fact]
    public void CatalogTableCoversEveryWeapon()
    {
        var ids = typeof(WeaponDataTests).GetMethod(nameof(EveryWeaponModesMatchCatalog))!
            .GetCustomAttributes(typeof(InlineDataAttribute), false).Cast<InlineDataAttribute>().Select(a => (string)a.GetData(null!).First()[0]).ToHashSet();
        Assert.Equal(D.Weapons.Select(w => w.Id).OrderBy(x => x), ids.OrderBy(x => x));
    }

    [Fact]
    public void MissingModesFallBackToRange()
    {
        static WeaponDef P(int range, bool strict = false)
        {
            using var doc = System.Text.Json.JsonDocument.Parse($"{{\"id\":\"x\",\"name\":\"X\",\"minDamage\":1,\"maxDamage\":2,\"range\":{range},\"scalesWith\":\"strength\"}}");
            return GameData.ParseWeapon(doc.RootElement.Clone(), strict);
        }
        var r1 = P(1);
        Assert.Equal((PatternKind.Single, 1), (r1.ModeA.Pattern, r1.ModeA.Range));
        var r3 = P(3);
        Assert.Equal((PatternKind.Shot, 3), (r3.ModeA.Pattern, r3.ModeA.Range));
        Assert.Equal(1, r3.ModeB.Cooldown);
        Assert.Throws<GameDataException>(() => P(1, strict: true));
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
