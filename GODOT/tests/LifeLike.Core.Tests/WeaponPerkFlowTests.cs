namespace LifeLike.Core.Tests;

/// <summary>
/// v0.23.0: profity broni w obiegu – wylosowane przy powstaniu znajdźki (drop, Hurtownia), te same po wzięciu i w ofercie
/// wymiany, jadą z bronią przez kaburę (zamiana, utrata pasa) i przez zapis budowy.
/// </summary>
public class WeaponPerkFlowTests
{
    private static GameData D => TestData.D;

    private static int Wi(string id) => Array.FindIndex(D.Weapons, w => w.Id == id);

    private static int Ti(string weapon) => Array.FindIndex(D.Tools, t => D.Weapons[t.Weapon].Id == weapon);

    private static byte Mask(params string[] ids) =>
        (byte)ids.Aggregate(0, (m, id) => m | 1 << Array.FindIndex(D.WeaponPerkPool, p => p.Id == id));

    private static int Bits(int m) => System.Numerics.BitOperations.PopCount((uint)m);

    private static Game WithBelt()
    {
        var g = TestData.Arena(0);
        g.Equip(D.BeltSlot, Game.BeltTier, 1);
        return g;
    }

    /// <summary>Znajdźka narzędzia z dropu (Kolekcjoner 100%) o rzadkości co najmniej minRarity, broń nie-unikat.</summary>
    private static Pickup DropTool(Game g, int minRarity)
    {
        g.Bonus.ToolPct = 100;
        for (var k = 0; k < 2000; ++k)
        {
            g.PickupsCount = 0;
            g.DropAt(g.Hero.X, g.Hero.Y);
            var p = g.Pickups[0];
            if (p.Type == PickupType.Tool && p.Trait >= minRarity && !D.Weapons[D.Tools[p.Arg].Weapon].Master) return p;
        }
        throw new InvalidOperationException("brak dropu narzędzia");
    }

    [Fact]
    public void DropRollsPerksByRarity()
    {
        var g = TestData.Arena(0);
        for (var k = 0; k < 200; ++k)
        {
            var p = DropTool(g, 0);
            Assert.Equal(D.WeaponPerksPerRarity[p.Trait], Bits(p.Perks));
        }
    }

    [Fact]
    public void PickupPerksSameAfterTaking()
    {
        var g = TestData.Arena(0);
        var p = DropTool(g, 2);
        Assert.Equal(2, Bits(p.Perks));
        g.Collect();
        Assert.Equal(D.Tools[p.Arg].Weapon, g.WeaponOverride);
        Assert.Equal(p.Perks, g.WeaponPerks);
        Assert.Contains(g.Log.TakeLast(2), m => m.Text.Contains(DamageHelp.PerkShorts(D, p.Perks)));
    }

    [Fact]
    public void ToolOfferShowsAndKeepsPerks()
    {
        var g = TestData.Arena(0);
        g.WeaponLvl = 1; // ulepszone narzędzie: najpierw decyzja
        var perks = Mask("glowica", "chwyt");
        g.Pickups[g.PickupsCount++] = new Pickup(g.Hero.X, g.Hero.Y, PickupType.Tool, true, Ti("lom"), 2) { Perks = perks };
        g.Collect();
        Assert.True(g.HasToolOffer);
        Assert.Equal(perks, g.ToolOfferPerks);
        Assert.Equal(perks, g.ToolOfferBreakdown().Perks);
        g.AcceptTool();
        Assert.Equal(perks, g.WeaponPerks);
        Assert.Equal(0, g.ToolOfferPerks);
    }

    [Fact]
    public void UniqueOnGroundHasMasterPerks()
    {
        var g = TestData.Arena(0);
        var t = Ti("noz_majstra");
        g.Pickups[g.PickupsCount++] = new Pickup(g.Hero.X, g.Hero.Y, PickupType.Tool, true, t, 0);
        g.Collect();
        Assert.Equal(D.WeaponRarities.Length - 1, g.WeaponRarity);
        Assert.Equal(Mask("przebicie", "ostrze"), g.WeaponPerks);
    }

    [Fact]
    public void ShopToolRollsPerks()
    {
        var shop = Array.FindIndex(D.Hurtownia, it => it.Effect == ShopEffect.Tool);
        Assert.True(shop >= 0);
        var seen = new int[D.WeaponRarities.Length];
        for (uint s = 1; s <= 80; ++s)
        {
            var g = TestData.Run(0, s);
            g.Cash = 100000;
            for (var i = 0; i < g.Mats.Length; ++i) g.Mats[i] = 99;
            Assert.True(g.HurtowniaBuy(shop));
            var expect = g.WeaponRarity == D.WeaponRarities.Length - 1
                ? D.MasterPerkMask(g.WeaponOverride)
                : -1;
            if (expect >= 0) Assert.Equal(expect, g.WeaponPerks);
            else Assert.Equal(D.WeaponPerksPerRarity[g.WeaponRarity], Bits(g.WeaponPerks));
            ++seen[g.WeaponRarity];
        }
        Assert.True(seen[0] > 0);
    }

    [Fact]
    public void SwapKeepsPerksOfBothWeapons()
    {
        var g = WithBelt();
        g.WeaponOverride = Wi("lom");
        g.WeaponPerks = Mask("ostrze");
        g.Weapon2 = Wi("przecinak");
        g.Weapon2Perks = Mask("glowica", "chwyt");
        Assert.True(g.PlayerSwapWeapon());
        Assert.Equal(Mask("glowica", "chwyt"), g.WeaponPerks);
        Assert.Equal(Mask("ostrze"), g.Weapon2Perks);
        Assert.True(g.PlayerSwapWeapon());
        Assert.Equal(Mask("ostrze"), g.WeaponPerks);
        Assert.Equal(Mask("glowica", "chwyt"), g.Weapon2Perks);
    }

    [Fact]
    public void HolsterStashKeepsPickupPerks()
    {
        var g = WithBelt();
        var perks = Mask("przebicie");
        g.Pickups[g.PickupsCount++] = new Pickup(g.Hero.X, g.Hero.Y, PickupType.Tool, true, Ti("lom"), 1) { Perks = perks };
        g.Collect();
        Assert.Equal(Wi("lom"), g.Weapon2);
        Assert.Equal(perks, g.Weapon2Perks);
    }

    [Fact]
    public void LosingBeltTakesHolsterPerksWithWeapon()
    {
        var g = WithBelt();
        g.Weapon2 = Wi("lom");
        g.Weapon2Rarity = 2;
        g.Weapon2Perks = Mask("ostrze", "wywazenie");
        var before = g.PickupsCount;
        g.Equip(D.BeltSlot, 0, 0);
        Assert.Equal(-1, g.Weapon2);
        Assert.Equal(0, g.Weapon2Perks);
        var p = g.Pickups[before];
        Assert.Equal(Mask("ostrze", "wywazenie"), p.Perks);
        g.Hero.X = p.X;
        g.Hero.Y = p.Y;
        g.Collect();
        Assert.Equal(Mask("ostrze", "wywazenie"), g.WeaponPerks);
    }

    [Fact]
    public void WornOutPrzecinakLosesItsPerks()
    {
        var g = TestData.Arena(0);
        g.TakeTool(Ti("przecinak"), 2, false, 0, Mask("chwyt", "glowica"));
        Assert.Equal(Mask("chwyt", "glowica"), g.WeaponPerks);
        var uses = 0;
        while (g.WeaponOverride == Wi("przecinak") && uses < 10) // otwiera pęknięte ściany, aż ładunki dojdą do 0
        {
            CrackedWall(g, g.Hero.X, g.Hero.Y - 1);
            g.WeaponBCd = 0;
            Assert.True(g.PlayerWeaponBWall());
            Assert.True(g.SecretOpen);
            ++uses;
        }
        Assert.True(uses > 1);
        Assert.Equal(-1, g.WeaponOverride); // zużyty: wraca broń zawodu
        Assert.Equal(0, g.WeaponCharges);
        Assert.Equal(0, g.WeaponPerks); // profity przepadają razem z bronią
    }

    private static void CrackedWall(Game g, int x, int y)
    {
        g.Lv[x, y] = Tile.Wall;
        g.SecretX = (sbyte)x;
        g.SecretY = (sbyte)y;
        g.SecretKind = (sbyte)Array.FindIndex(D.SecretKinds, s => s.Breakable);
        g.SecretOpen = false;
        g.SecretDir = 0;
    }

    [Fact]
    public void RunSaveRoundTripKeepsPerks()
    {
        var g = WithBelt();
        g.WeaponPerks = Mask("ostrze");
        g.Weapon2 = Wi("lom");
        g.Weapon2Perks = Mask("chwyt");
        g.ToolOfferPerks = Mask("glowica");
        g.Pickups[g.PickupsCount++] = new Pickup(3, 3, PickupType.Tool, true, Ti("lom"), 2) { Perks = Mask("przebicie", "wywazenie") };
        g.RollWeaponPerks(Wi("lom"), 2);
        var h = RunSave.FromBytes(RunSave.Make(g).ToBytes()).Load(D);
        Assert.NotNull(h);
        Assert.Equal(g.WeaponPerks, h.WeaponPerks);
        Assert.Equal(g.Weapon2Perks, h.Weapon2Perks);
        Assert.Equal(g.ToolOfferPerks, h.ToolOfferPerks);
        Assert.Equal(g.PerkRolls, h.PerkRolls);
        Assert.Equal(g.Pickups[g.PickupsCount - 1].Perks, h.Pickups[h.PickupsCount - 1].Perks);
        Assert.Equal(StateDigest.Of(g), StateDigest.Of(h));
        // ten sam następny los po wczytaniu
        Assert.Equal(g.RollWeaponPerks(Wi("lom"), 2), h.RollWeaponPerks(Wi("lom"), 2));
        h.Weapon2Perks = 0;
        Assert.NotEqual(StateDigest.Of(g), StateDigest.Of(h));
    }

    [Fact]
    public void OldRunMagicRejectedWithoutException()
    {
        var g = TestData.Run(0, 5);
        var rs = RunSave.Make(g);
        rs.Magic = Profile.MagicBytes("PBRUN17");
        Assert.False(rs.Valid(D));
        Assert.Null(RunSave.FromBytes(rs.ToBytes()).Load(D));
        Assert.Equal("PBRUN19", RunSave.RunMagic);
    }

    [Fact]
    public void BotValuesPerks()
    {
        var g = TestData.Run(0, 5);
        var lom = Wi("lom");
        var bare = BotWeapons.Score(g, lom, 1, 0);
        Assert.True(BotWeapons.Score(g, lom, 1, Mask("ostrze")) > bare, "profit bez wpływu na średni cios też się liczy");
        Assert.True(BotWeapons.Score(g, lom, 1, Mask("ostrze", "chwyt")) > BotWeapons.Score(g, lom, 1, Mask("ostrze")));
        // w ręku Solidna z profitem; na ziemi ta sama broń, Solidna bez profitów – bot jej nie chce
        g.TakeTool(Ti("lom"), 1, false, 0, Mask("chwyt"));
        Assert.True(BotWeapons.Score(g, -1, 0) > bare);
        Assert.False(BotWeapons.WantsTool(g, Ti("lom"), 1, 0));
        // oferta wymiany: bot liczy profity znajdźki
        g.WeaponLvl = 1;
        g.Pickups[g.PickupsCount++] = new Pickup(g.Hero.X, g.Hero.Y, PickupType.Tool, true, Ti("lom"), 2) { Perks = Mask("glowica", "wywazenie") };
        g.Collect();
        Assert.True(g.HasToolOffer);
        Assert.Equal(g.ToolOfferBreakdown().Avg10 > g.WeaponBreakdown().Avg10, g.BotToolAccept());
        Assert.True(g.ToolOfferBreakdown().Avg10 > g.WeaponBreakdown(-1, Wi("lom"), -1, -1, 0, 2).Avg10);
    }
}
