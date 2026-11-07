namespace LifeLike.Core.Tests;

/// <summary>
/// v0.23.0: profity broni w prawdziwej walce – Głowica (+1 do najsilniejszego rzutu), Przebicie profitu sumowane z cechą
/// ulepszenia, Chwyt skraca odnowienie trybu B (najmniej 1). Broń bez rozrzutu, kryt wyłączony – obrażenia dokładnie.
/// </summary>
public class WeaponPerkCombatTests
{
    private static GameData D => TestData.D;

    private static int Wi(string id) => Array.FindIndex(D.Weapons, w => w.Id == id);

    private static byte Mask(params string[] ids) =>
        (byte)ids.Aggregate(0, (m, id) => m | 1 << Array.FindIndex(D.WeaponPerkPool, p => p.Id == id));

    private static Game Arena(string weapon, Func<WeaponDef, WeaponDef> change = null)
    {
        var wi = Wi(weapon);
        var w = D.Weapons[wi] with { MinDamage = 5, MaxDamage = 5, Crit = 0 };
        if (change is not null) w = change(w);
        var g = new Game(D.WithWeapon(wi, w));
        g.NewRun(0, 77);
        g.Lv.Fill(Tile.Wall);
        for (var y = 1; y <= 14; ++y)
            for (var x = 1; x <= 14; ++x)
                g.Lv[x, y] = Tile.Floor;
        g.EnemiesCount = 0;
        g.PickupsCount = 0;
        g.StairsX = g.StairsY = -1;
        g.Weather = 0;
        g.Hero.X = 7;
        g.Hero.Y = 7;
        g.WeaponOverride = wi;
        g.WeaponRarity = 0;
        g.WeaponPerks = 0;
        g.CritBuff = sbyte.MinValue; // bez krytu
        g.UpdateFov();
        return g;
    }

    /// <summary>Stalowy problem obok bohatera z OBR równą def, dużo HP.</summary>
    private static int Foe(Game g, int def)
    {
        var id = Array.FindIndex(D.Enemies, e => e.Material >= 0 && D.Materials[e.Material].Id == "stal" && e.Elem != Element.Water && !e.Slam);
        var i = g.EnemiesCount;
        g.Spawn(id, 8, 7);
        g.Enemies[i].Hp = g.Enemies[i].MaxHp = 1000;
        g.Enemies[i].DefDown = 100;
        g.Enemies[i].DefDownVal = (sbyte)(D.Enemies[id].Defense + g.EnemyEliteDef(i) - def);
        g.UpdateFov();
        Assert.Equal(def, g.EnemyDefense(i));
        return i;
    }

    private static int Loss(Game g, int ei)
    {
        var hp = g.Enemies[ei].Hp;
        g.AttackWith(g.Weapon.ModeA, ei);
        return hp - g.Enemies[ei].Hp;
    }

    [Fact]
    public void HeadRaisesMaxRollByOne()
    {
        // Wyważenie podnosi najsłabszy rzut do maksimum: rzut = maks. broni, więc Głowica daje dokładnie +1
        var a = Arena("noz_majstra");
        a.WeaponPerks = Mask("wywazenie");
        var b = Arena("noz_majstra");
        b.WeaponPerks = Mask("wywazenie", "glowica");
        Assert.Equal(Loss(a, Foe(a, 0)) + 1, Loss(b, Foe(b, 0)));
        var bd = b.WeaponBreakdown();
        Assert.Equal(a.WeaponBreakdown().Max + 1, bd.Max);
        Assert.Equal(6, bd.WMax);
    }

    [Fact]
    public void PiercePerkStacksWithUpgradeTrait()
    {
        var trait = Array.FindIndex(D.ToolTraits, t => t.Effect == ToolTraitEffect.Pierce);
        var tv = D.ToolTraits[trait].Value;
        var plain = Arena("noz_majstra");
        var both = Arena("noz_majstra");
        both.WeaponTrait = (sbyte)trait;
        both.WeaponPerks = Mask("przebicie");
        var baseP = plain.PierceTotal(plain.Weapon);
        Assert.Equal(baseP + tv + 1, both.PierceTotal(both.Weapon));
        Assert.Equal(baseP + tv + 1, both.WeaponBreakdown(-1).Pierce);
        const int def = 12;
        var expect = Math.Max(0, def - baseP) / 2 - Math.Max(0, def - baseP - tv - 1) / 2;
        Assert.True(expect > 0);
        Assert.Equal(Loss(plain, Foe(plain, def)) + expect, Loss(both, Foe(both, def)));
    }

    [Fact]
    public void GripShortensModeBCooldownNotBelowOne()
    {
        var g = Arena("noz_majstra");
        var cd = g.Weapon.ModeB.Cooldown;
        Assert.True(cd >= 2);
        g.WeaponPerks = Mask("chwyt");
        Assert.True(g.PlayerWeaponB(Foe(g, 0)));
        Assert.Equal(cd - 1, g.WeaponBCd);
        Assert.Equal(-1, g.WeaponBCdDelta());
        // odnowienie 1: Chwyt nie schodzi poniżej 1 (także z Markową -1)
        var one = Arena("noz_majstra", w => w with { ModeB = w.ModeB with { Cooldown = 1 } });
        one.WeaponPerks = Mask("chwyt");
        one.WeaponRarity = 2;
        Assert.True(one.PlayerWeaponB(Foe(one, 0)));
        Assert.Equal(1, one.WeaponBCd);
    }

    [Fact]
    public void CritPerkAddsToCritChance()
    {
        var a = Arena("noz_majstra");
        a.CritBuff = 0;
        var b = Arena("noz_majstra");
        b.CritBuff = 0;
        b.WeaponPerks = Mask("ostrze");
        Assert.Equal(a.CritPct() + 5, b.CritPct());
        Assert.Equal(a.WeaponBreakdown().CritPct + 5, b.WeaponBreakdown().CritPct);
    }

    [Fact]
    public void BreakdownOfOtherWeaponCountsItsPerks()
    {
        var g = Arena("noz_majstra");
        var lom = Wi("lom");
        var bare = g.WeaponBreakdown(-1, lom, -1, -1, 0, 1);
        var head = g.WeaponBreakdown(-1, lom, -1, -1, 0, 1, Mask("glowica"));
        Assert.Equal(bare.Max + 1, head.Max);
        Assert.Equal(Mask("glowica"), head.Perks);
        Assert.Equal(g.WeaponPerks, g.WeaponBreakdown().Perks);
    }

    [Fact]
    public void BreakdownTextListsPerks()
    {
        var g = Arena("noz_majstra");
        Assert.False(DamageHelp.Line(D, new Message(), g.WeaponBreakdown(), DmgText.Perks));
        g.WeaponPerks = Mask("przebicie", "glowica");
        var m = new Message();
        Assert.True(DamageHelp.Line(D, m, g.WeaponBreakdown(), DmgText.Perks));
        Assert.Contains("OBR -1", m.Text);
        Assert.Contains("obr. maks +1", m.Text);
        Assert.Equal("OBR -1, obr. maks +1", DamageHelp.PerkShorts(D, g.WeaponPerks));
        g.WeaponRarity = (byte)(D.WeaponRarities.Length - 1);
        Assert.StartsWith(Loc.T("unikat"), DamageHelp.Text(D, g.WeaponBreakdown(), DmgText.Perks));
        // kryt z profitu Ostrze widać w dodatkach krytu
        g.WeaponPerks = Mask("ostrze");
        Assert.Contains("5%", DamageHelp.Text(D, g.WeaponBreakdown(), DmgText.CritExtra));
    }
}
