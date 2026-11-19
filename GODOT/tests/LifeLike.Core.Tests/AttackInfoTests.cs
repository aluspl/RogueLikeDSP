namespace LifeLike.Core.Tests;

/// <summary>v0.23.0: opis ataku pod przyciskiem Cios / Patent – nazwa ruchu, wzór pól, obrażenia, efekt, odnowienie, profity.</summary>
[Collection("Loc")]
public class AttackInfoTests
{
    private static GameData D => TestData.D;

    private static int Wi(string id) => Array.FindIndex(D.Weapons, w => w.Id == id);

    private static int Perk(string id) => 1 << Array.FindIndex(D.WeaponPerkPool, p => p.Id == id);

    private static Game Arena(string weapon, int rarity = 0, int perks = 0)
    {
        var g = TestData.Arena(0);
        g.WeaponOverride = Wi(weapon);
        g.WeaponRarity = (byte)rarity;
        g.WeaponPerks = (byte)perks;
        return g;
    }

    private static string All(AttackInfo a) => string.Join("\n", a.Lines);

    [Fact]
    public void KielniaPatentNamesMoveStunAndCooldown()
    {
        Loc.English = false;
        var a = AttackInfo.For(Arena("kielnia"), true);
        Assert.Equal("Rzut zaprawą", a.Title);
        Assert.Contains("Patent", a.Header);
        Assert.Contains("Kielnia", a.Header);
        Assert.Contains("Ogłuszenie", All(a));
        Assert.Contains("Odnowienie 7", All(a));
        Assert.Contains("50%", All(a));
    }

    [Fact]
    public void KielniaPatentGridIsShotOfTwoToTheRight()
    {
        var a = AttackInfo.For(Arena("kielnia"), true);
        Assert.Equal(AttackInfo.Hero, a.Cell(0, 0));
        Assert.Equal(AttackInfo.Hit, a.Cell(1, 0));
        Assert.Equal(AttackInfo.Hit, a.Cell(2, 0));
        Assert.NotEqual(AttackInfo.Hit, a.Cell(-1, 0));
        Assert.NotEqual(AttackInfo.Hit, a.Cell(1, 1));
        Assert.True(a.Radius >= 2);
    }

    [Fact]
    public void CiosHasNoCooldownAndArcGrid()
    {
        Loc.English = false;
        var a = AttackInfo.For(Arena("kielnia"), false);
        Assert.Equal(Loc.T("tip_cios"), a.Title);
        Assert.DoesNotContain("Odnowienie", All(a));
        Assert.Equal(AttackInfo.Hit, a.Cell(1, 0));
        Assert.Equal(AttackInfo.Hit, a.Cell(1, 1));
        Assert.Equal(AttackInfo.Hit, a.Cell(1, -1));
        Assert.NotEqual(AttackInfo.Hit, a.Cell(2, 0));
    }

    [Fact]
    public void RarityAndGripShortenPatentCooldownAndAreListed()
    {
        Loc.English = false;
        var g = Arena("kielnia", 2, Perk("chwyt"));
        var a = AttackInfo.For(g, true);
        var cd = Math.Max(1, g.Weapon.ModeB.Cooldown + g.WeaponBCdDelta());
        Assert.True(cd < 7);
        Assert.Contains($"Odnowienie {cd}", All(a));
        Assert.Contains("Chwyt", All(a));
        Assert.Contains(D.WeaponRarities[2].Name, All(a));
    }

    [Fact]
    public void CiosSkipsCooldownPerk()
    {
        var a = AttackInfo.For(Arena("kielnia", 0, Perk("chwyt") | Perk("ostrze")), false);
        Assert.DoesNotContain("Chwyt", All(a));
        Assert.Contains("Ostrze", All(a));
    }

    [Fact]
    public void MasterKnifeShowsTwoThrusts()
    {
        Loc.English = false;
        var a = AttackInfo.For(Arena("noz_majstra", 3), true);
        Assert.Equal("Jeszcze nie emerytura", a.Title);
        Assert.Contains("75%", All(a));
        Assert.Contains(Loc.F("tip_ciosy", 2), All(a));
    }

    [Fact]
    public void CooldownLeftIsShown()
    {
        Loc.English = false;
        var g = Arena("kielnia");
        g.WeaponBCd = 3;
        Assert.Contains(Loc.F("tip_odnowienie_za", 7, 3), All(AttackInfo.For(g, true)));
    }

    [Fact]
    public void EveryWeaponHasShortPatentName()
    {
        foreach (var w in D.Weapons)
        {
            Assert.False(string.IsNullOrEmpty(w.ModeB.Name), w.Id);
            if (w.Id != "noz_majstra") Assert.True(w.ModeB.Name.Length <= 18, w.Id + ": " + w.ModeB.Name);
        }
    }

    [Fact]
    public void EmptyNameFallsBackToPatent()
    {
        var w = D.Weapons[Wi("kielnia")];
        var bare = w with { ModeB = w.ModeB with { Name = "" } };
        Assert.Equal(Loc.T("cios_b"), ModeText.BName(bare));
        Assert.Equal("Rzut zaprawą", ModeText.BName(w));
    }

    [Fact]
    public void AroundGridHitsAllNeighbours()
    {
        var a = AttackInfo.For(Arena("pilarka"), true);
        for (var dy = -1; dy <= 1; dy++)
        {
            for (var dx = -1; dx <= 1; dx++)
            {
                if (dx != 0 || dy != 0) Assert.Equal(AttackInfo.Hit, a.Cell(dx, dy));
            }
        }
    }
    [Fact]
    public void ArtifactProcAndCounterShownInBothModes()
    {
        Loc.English = false;
        var g = Arena("mlot_grom", 3);
        g.ProcCount = 3;
        Assert.Contains("Piorun: co 5. cios (3/5)", All(AttackInfo.For(g, false)));
        Assert.Contains("Piorun: co 5. cios (3/5)", All(AttackInfo.For(g, true)));
        Assert.Contains("Iskry: każdy cios", All(AttackInfo.For(Arena("szlifierka_isk", 3), false)));
        var lom = AttackInfo.For(Arena("lom_deska", 3), true);
        Assert.Contains("Ściana: bez odnowienia", All(lom));
        Assert.Contains("+100% na Drewno", All(lom));
        Assert.DoesNotContain("co 5.", All(AttackInfo.For(Arena("mlot"), false)));
    }
}
