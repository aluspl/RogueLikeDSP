namespace LifeLike.Core.Tests;

/// <summary>v0.23.0: dane broni – typy ciosu, tryby A/B, rzadkość, 17 nowych broni, ogień, pas monterski.</summary>
public class WeaponDataTests
{
    private static GameData D => TestData.D;

    private static WeaponDef W(string id) => D.Weapons.First(w => w.Id == id);

    [Fact]
    public void AllWeaponsHaveHitAndModes()
    {
        Assert.Equal(45, D.Weapons.Length); // v0.23.0 (#84): + Nóż do tapet, Nóż majstra seniora; + 4 artefakty
        Assert.All(D.Weapons, w =>
        {
            Assert.NotEmpty(w.Hit);
            Assert.True(w.ModeB.Cooldown > 0, w.Id);
        });
    }

    [Theory]
    [InlineData("dziennik", PatternKind.Shot, ModeEffect.None, PatternKind.Single, ModeEffect.Mark, 6)]
    [InlineData("kielnia", PatternKind.Arc, ModeEffect.None, PatternKind.Shot, ModeEffect.Stun, 7)]
    [InlineData("gwozdziarka", PatternKind.Shot, ModeEffect.None, PatternKind.Line, ModeEffect.None, 5)]
    [InlineData("probnik", PatternKind.Shot, ModeEffect.None, PatternKind.Cross, ModeEffect.Chain, 4)]
    [InlineData("klucz", PatternKind.Single, ModeEffect.None, PatternKind.Single, ModeEffect.Dry, 4)]
    [InlineData("szlifierka", PatternKind.Single, ModeEffect.None, PatternKind.Arc, ModeEffect.None, 3)]
    [InlineData("lom", PatternKind.Single, ModeEffect.None, PatternKind.Push, ModeEffect.Push, 5)]
    [InlineData("wkretarka", PatternKind.Line, ModeEffect.None, PatternKind.Single, ModeEffect.DefDown, 5)]
    [InlineData("laser", PatternKind.Line, ModeEffect.None, PatternKind.LineBoth, ModeEffect.None, 6)]
    [InlineData("mlot", PatternKind.Push, ModeEffect.Push, PatternKind.Around, ModeEffect.OpenWall, 7)]
    [InlineData("tablet", PatternKind.Single, ModeEffect.None, PatternKind.Shot, ModeEffect.Swap, 6)]
    [InlineData("miernik", PatternKind.Shot, ModeEffect.None, PatternKind.Single, ModeEffect.Reveal, 6)]
    [InlineData("dachowka", PatternKind.Shot, ModeEffect.None, PatternKind.Throw3x3, ModeEffect.None, 2)]
    [InlineData("agregat", PatternKind.Spray, ModeEffect.None, PatternKind.Single, ModeEffect.Root, 6)]
    [InlineData("lyzka", PatternKind.Arc, ModeEffect.None, PatternKind.Shot, ModeEffect.Pull, 5)]
    [InlineData("udarowy", PatternKind.Single, ModeEffect.None, PatternKind.Line, ModeEffect.DefDown, 6)]
    [InlineData("kotwy", PatternKind.Shot, ModeEffect.None, PatternKind.Single, ModeEffect.Root, 5)]
    [InlineData("spawarka", PatternKind.Line, ModeEffect.None, PatternKind.Single, ModeEffect.Dot, 4)]
    [InlineData("tyczka", PatternKind.Line, ModeEffect.None, PatternKind.Cross, ModeEffect.Mark, 4)]
    [InlineData("mlotek", PatternKind.Single, ModeEffect.None, PatternKind.Single, ModeEffect.DefDown, 2)]
    [InlineData("mlot_zenka", PatternKind.Push, ModeEffect.Push, PatternKind.Around, ModeEffect.Push, 7)]
    [InlineData("poziomica_m", PatternKind.Shot, ModeEffect.None, PatternKind.LineBoth, ModeEffect.Reveal, 6)]
    [InlineData("lopata", PatternKind.Arc, ModeEffect.Stun, PatternKind.Line, ModeEffect.DefDown, 4)]
    [InlineData("siekiera", PatternKind.Single, ModeEffect.None, PatternKind.Arc, ModeEffect.None, 5)]
    [InlineData("pila", PatternKind.Single, ModeEffect.None, PatternKind.Single, ModeEffect.Dot, 5)]
    [InlineData("kilof", PatternKind.Single, ModeEffect.None, PatternKind.Line, ModeEffect.None, 6)]
    [InlineData("przecinak", PatternKind.Single, ModeEffect.None, PatternKind.Single, ModeEffect.OpenWall, 1)]
    [InlineData("nozyce", PatternKind.Single, ModeEffect.None, PatternKind.Arc, ModeEffect.None, 5)]
    [InlineData("silikon", PatternKind.Shot, ModeEffect.None, PatternKind.Single, ModeEffect.Root, 6)]
    [InlineData("myjka", PatternKind.Line, ModeEffect.Wet, PatternKind.Push, ModeEffect.Push, 5)]
    [InlineData("palnik", PatternKind.Spray, ModeEffect.Dot, PatternKind.Around, ModeEffect.Dry, 7)]
    [InlineData("taker", PatternKind.Shot, ModeEffect.None, PatternKind.Single, ModeEffect.Root, 5)]
    [InlineData("mlotek_gumowy", PatternKind.Single, ModeEffect.Stun, PatternKind.Cross, ModeEffect.Stun, 6)]
    [InlineData("pilarka", PatternKind.Arc, ModeEffect.None, PatternKind.Around, ModeEffect.None, 7)]
    [InlineData("poziomica_2m", PatternKind.Shot, ModeEffect.None, PatternKind.Cross, ModeEffect.Reveal, 6)]
    [InlineData("mieszadlo", PatternKind.Single, ModeEffect.None, PatternKind.Around, ModeEffect.Push, 7)]
    [InlineData("laser_krzyzowy", PatternKind.Line, ModeEffect.None, PatternKind.Line4, ModeEffect.None, 6)]
    [InlineData("lutownica", PatternKind.Single, ModeEffect.Dot, PatternKind.Line, ModeEffect.Dot, 5)]
    [InlineData("opalarka", PatternKind.Spray, ModeEffect.Dry, PatternKind.Single, ModeEffect.Mark, 5)]
    [InlineData("noz", PatternKind.Single, ModeEffect.None, PatternKind.Line, ModeEffect.None, 3)]
    [InlineData("noz_majstra", PatternKind.Single, ModeEffect.None, PatternKind.Single, ModeEffect.None, 3)]
    [InlineData("mlot_grom", PatternKind.Push, ModeEffect.Push, PatternKind.Around, ModeEffect.OpenWall, 7)]
    [InlineData("kielnia_cegla", PatternKind.Arc, ModeEffect.None, PatternKind.Shot, ModeEffect.Stun, 7)]
    [InlineData("szlifierka_isk", PatternKind.Single, ModeEffect.None, PatternKind.Arc, ModeEffect.None, 3)]
    [InlineData("lom_deska", PatternKind.Single, ModeEffect.None, PatternKind.Push, ModeEffect.Push, 5)]
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
        Assert.Equal(new[] { "#8a8796", "#5cb4f0", "#6b4eff", "#d9a400" }, D.WeaponRarities.Select(r => r.Color));
        Assert.Contains(D.Gear, i => i.Name == "Pas monterski z kaburą");
    }

    [Fact]
    public void NewWeaponsHaveSourcesAndJoinToolList()
    {
        Assert.Equal(23, D.NewTools.Length); // v0.23.0 (#84): + dwa noże, + 4 artefakty
        Assert.All(D.NewTools, t => Assert.True(t.Weapon >= 22 && (t.Shop || t.Drop || t.Hurtownia || t.Secret || t.Artifact)));
        Assert.Equal(33, D.Tools.Length); // v0.23.0 zad. 5 (+ 4 artefakty): 10 + 17 (+ 2 noże, #84) w jednej liście (maska 64 bity)
    }

    [Fact]
    public void HitTypesTableUsesMaterialOrder()
    {
        var h = D.HitTypes;
        var stal = Array.FindIndex(D.Materials, m => m.Id == "stal");
        var drewno = Array.FindIndex(D.Materials, m => m.Id == "drewno");
        var cement = Array.FindIndex(D.Materials, m => m.Id == "cement");
        Assert.Equal(75, h.MaterialPct[(int)HitType.Blunt, stal]);
        // v0.23.0 Task 7: tabela złagodzona do ±25% (spec. 4.7 pkt 3), kłucie słabsze na cement i wodę
        Assert.Equal(125, h.MaterialPct[(int)HitType.Blunt, cement]);
        Assert.Equal(125, h.MaterialPct[(int)HitType.Cut, drewno]);
        Assert.Equal(125, h.MaterialPct[(int)HitType.Pierce, stal]);
        Assert.Equal(75, h.MaterialPct[(int)HitType.Pierce, cement]);
        Assert.Equal(75, h.MaterialPct[(int)HitType.Blunt, 3]);
        Assert.Equal((20, 5, 1, 125, 150, 150, 50, 2), (h.StunChancePct, h.CutCritPct, h.PierceDef, h.SparkSteelPct, h.PowerWaterPct, h.FireWoodPct, h.FireWaterPct, h.DotPerTurn));
    }

    [Fact]
    public void ModesAndNewStatuses()
    {
        Assert.Equal(new[] { HitType.Blunt, HitType.Pierce }, W("udarowy").Hit);
        Assert.Equal((ModeEffect.OpenWall, 3, 1), (W("przecinak").ModeB.Effect, W("przecinak").ModeB.Value, W("przecinak").ModeB.Cooldown));
        Assert.Equal((ModeEffect.Stun, 20), (W("lopata").ModeA.Effect, W("lopata").ModeA.Value));
        Assert.Equal(new[] { "mlot_zenka", "poziomica_m", "pilarka", "noz_majstra", "mlot_grom", "kielnia_cegla", "szlifierka_isk", "lom_deska" },
            D.Weapons.Where(w => w.Master).Select(w => w.Id));
        Assert.Equal("Podpalenie", D.Statuses[(int)StatusEffect.Burn].Name);
        Assert.Equal("Oznaczony", D.Statuses[(int)StatusEffect.Mark].Name);
    }

    [Fact]
    public void KnivesData()
    {
        // v0.23.0 (#84): Nóż do tapet – zwykła broń z dropu i Hurtowni; Nóż majstra seniora – unikat, czyste kłucie
        var noz = Array.FindIndex(D.Tools, t => D.Weapons[t.Weapon].Id == "noz");
        Assert.True(D.Tools[noz].Drop && D.Tools[noz].Hurtownia && !D.Tools[noz].Secret);
        Assert.Equal(new[] { HitType.Pierce }, W("noz").Hit);
        Assert.False(W("noz").Master);
        var k = W("noz_majstra");
        var t = Array.FindIndex(D.Tools, x => D.Weapons[x.Weapon].Id == "noz_majstra");
        Assert.True(D.Tools[t].Secret && !D.Tools[t].Drop && !D.Tools[t].Hurtownia);
        Assert.Equal(new[] { HitType.Pierce }, k.Hit);
        Assert.True(k.Master && k.Neutral);
        Assert.Equal((4, 15, 2, 3), (k.Pierce, k.Crit, k.ModeB.Hits, k.ModeB.Cooldown));
        Assert.Equal("Jeszcze nie emerytura", k.ModeB.Name);
        Assert.Equal("Ostrze prosi o emeryturę. Majster: nie!", k.Desc);
        foreach (var e in D.Enemies) Assert.Equal(100, HitCalc.Pct(D, k, e)); // bez kar i premii za materiał
        Assert.Equal(4, HitCalc.PierceDef(D, k));
        Assert.Equal(D.HitTypes.PierceDef, HitCalc.PierceDef(D, W("noz")));
    }
}
