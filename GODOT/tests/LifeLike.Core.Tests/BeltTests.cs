namespace LifeLike.Core.Tests;

/// <summary>v0.23.0: pas monterski z kaburą – druga broń, zamiana za turę, rzadkość pasa i zapis.</summary>
public class BeltTests
{
    private static GameData D => TestData.D;

    private static int Wi(string id) => Array.FindIndex(D.Weapons, w => w.Id == id);

    private static int Ti(string weapon) => Array.FindIndex(D.Tools, t => D.Weapons[t.Weapon].Id == weapon);

    private static Game WithBelt()
    {
        var g = TestData.Arena(0);
        g.Equip(D.BeltSlot, Game.BeltTier, 1);
        return g;
    }

    private static void DropToolAtHero(Game g, int tool, int rarity)
    {
        g.Pickups[g.PickupsCount++] = new Pickup(g.Hero.X, g.Hero.Y, PickupType.Tool, true, tool, rarity);
        g.Collect();
    }

    [Fact]
    public void BeltItemIsTheMiddleBeltTier()
    {
        Assert.True(D.BeltSlot >= 0 && D.GearSlots[D.BeltSlot] == "Pas");
        Assert.Equal("Pas monterski z kaburą", D.Gear[D.BeltSlot * 3 + Game.BeltTier].Name);
        Assert.Equal(15, D.BeltChancePct);
        var g = TestData.Arena(0);
        Assert.False(g.HasBelt());
        g.Equip(D.BeltSlot, 0, 0);
        Assert.False(g.HasBelt());
        g.Equip(D.BeltSlot, 2, 0);
        Assert.False(g.HasBelt());
        g.Equip(D.BeltSlot, Game.BeltTier, 0);
        Assert.True(g.HasBelt());
    }

    [Fact]
    public void SwapNeedsBeltAndCostsTurn()
    {
        var g = TestData.Arena(0);
        g.Weapon2 = Wi("lom");
        var turns = g.Turns;
        Assert.False(g.PlayerSwapWeapon()); // bez pasa
        Assert.Equal(turns, g.Turns);
        Assert.Equal(-1, g.WeaponOverride);
        g.Equip(D.BeltSlot, Game.BeltTier, 1);
        g.Weapon2 = -1;
        Assert.False(g.PlayerSwapWeapon()); // pas, ale pusta kabura
        Assert.Equal(turns, g.Turns);
        g.Weapon2 = Wi("lom");
        Assert.True(g.PlayerSwapWeapon());
        Assert.Equal(turns + 1, g.Turns);
        Assert.Equal(Wi("lom"), g.WeaponOverride);
        Assert.Equal(g.CDef.Weapon, g.Weapon2); // broń zawodu poszła do kabury
        Assert.True(g.PlayerSwapWeapon()); // i z powrotem
        Assert.Equal(-1, g.WeaponOverride);
        Assert.Equal(Wi("lom"), g.Weapon2);
        Assert.Equal(turns + 2, g.Turns);
    }

    [Fact]
    public void SwapKeepsRarities()
    {
        var g = WithBelt();
        g.WeaponRarity = 1;
        g.Weapon2 = Wi("lom");
        g.Weapon2Rarity = 2;
        Assert.True(g.PlayerSwapWeapon());
        Assert.Equal(2, g.WeaponRarity);
        Assert.Equal(1, g.Weapon2Rarity);
        Assert.True(g.PlayerSwapWeapon());
        Assert.Equal(1, g.WeaponRarity);
        Assert.Equal(2, g.Weapon2Rarity);
    }

    [Fact]
    public void SwapKeepsCharges()
    {
        var g = WithBelt();
        g.WeaponOverride = Wi("przecinak");
        g.WeaponCharges = 2;
        g.Weapon2 = Wi("lom");
        Assert.True(g.PlayerSwapWeapon());
        Assert.Equal(0, g.WeaponCharges); // Łom nie ma ładunków
        Assert.Equal(2, g.Weapon2Charges); // ładunki zostały przy Przecinaku w kaburze
        Assert.True(g.PlayerSwapWeapon());
        Assert.Equal(Wi("przecinak"), g.WeaponOverride);
        Assert.Equal(2, g.WeaponCharges);
        Assert.Equal(0, g.Weapon2Charges);
    }

    [Fact]
    public void FoundWeaponGoesToSecondSlot()
    {
        var g = WithBelt();
        g.WeaponLvl = 1; // ulepszona broń w ręku nie wywołuje pytania o wymianę
        var t = Ti("lom");
        DropToolAtHero(g, t, 1);
        Assert.False(g.HasToolOffer);
        Assert.Equal(Wi("lom"), g.Weapon2);
        Assert.Equal(1, g.Weapon2Rarity);
        Assert.Equal(-1, g.WeaponOverride);
        Assert.Equal(1, g.WeaponLvl);
        // kabura pełna: dotychczasowy przebieg (ulepszone narzędzie – pytanie o wymianę)
        DropToolAtHero(g, Ti("przecinak"), 0);
        Assert.True(g.HasToolOffer);
        Assert.Equal(Wi("lom"), g.Weapon2);
        // bez pasa nic się nie zmienia: broń trafia do ręki
        var h = TestData.Arena(0);
        DropToolAtHero(h, t, 0);
        Assert.Equal(Wi("lom"), h.WeaponOverride);
        Assert.Equal(-1, h.Weapon2);
    }

    [Fact]
    public void FoundUniqueInHolsterIsMaster()
    {
        var g = WithBelt();
        var unique = Array.FindIndex(D.Tools, tl => D.Weapons[tl.Weapon].Master);
        if (unique < 0) return;
        DropToolAtHero(g, unique, 0);
        Assert.Equal(D.WeaponRarities.Length - 1, g.Weapon2Rarity);
    }

    [Fact]
    public void BeltIsRare()
    {
        var g = TestData.Arena(0);
        g.Bonus.GearSlots = 1 << D.BeltSlot; // tylko Pas, żeby mierzyć jego rozkład
        int boxes = 0, belts = 0;
        for (var i = 0; i < 20000; ++i)
        {
            g.PickupsCount = 0;
            g.DropAt(1, 1);
            if (g.PickupsCount == 0 || g.Pickups[0].Type != PickupType.GearBox) continue;
            ++boxes;
            if (g.Pickups[0].Arg == D.BeltSlot * 3 + Game.BeltTier) ++belts;
        }
        Assert.True(boxes > 1000, "paczek sprzętu: " + boxes);
        var share = belts / (double)boxes;
        // bez filtra ~24% paczek to jakość środkowa; 15% z tego to ~3,6%
        Assert.InRange(share, 0.015, 0.06);
    }

    [Fact]
    public void ShopGearBeltIsRare()
    {
        var g = TestData.Arena(0);
        g.Bonus.GearSlots = 1 << D.BeltSlot;
        int belts = 0, n = 3000;
        for (var i = 0; i < n; ++i)
        {
            g.Equipped[D.BeltSlot] = (sbyte)(i % 2 == 0 ? -1 : 0); // pusty albo Zwykły pas
            g.Cash = 100000;
            var gi = Array.FindIndex(D.Hurtownia, h => h.Effect == ShopEffect.Gear);
            Assert.True(g.HurtowniaBuy(gi));
            if (g.HasBelt()) ++belts;
        }
        var share = belts / (double)n;
        Assert.InRange(share, 0.04, 0.11); // połowa losowań to jakość środkowa, z niej 15% = ok. 7,5%; reszta Markowy
    }

    [Fact]
    public void RewardStartsRunWithBelt()
    {
        var idx = Array.FindIndex(D.Rewards, r => r.Kind == RewardKind.Belt);
        Assert.True(idx >= 0);
        Assert.Equal(D.BeltSlot, D.Rewards[idx].Index);
        var p = new Profile();
        Assert.Equal(0, Meta.Mods(D, p).StartBelt);
        p.Rewards = (byte)(idx + 1);
        var mods = Meta.Mods(D, p);
        Assert.Equal(1, mods.StartBelt);
        var g = new Game(D);
        g.NewRun(0, 5, 0, mods);
        Assert.True(g.HasBelt());
        var plain = TestData.Run(0, 5);
        Assert.False(plain.HasBelt());
    }

    [Fact]
    public void SecondWeaponSurvivesSave()
    {
        var g = WithBelt();
        g.Weapon2 = Wi("przecinak");
        g.Weapon2Rarity = 2;
        g.Weapon2Charges = 3;
        var h = new Game(D);
        h.CopyFrom(g);
        Assert.Equal(g.Weapon2, h.Weapon2);
        Assert.Equal(2, h.Weapon2Rarity);
        Assert.Equal(3, h.Weapon2Charges);
        Assert.Equal(StateDigest.Of(g), StateDigest.Of(h));
        g.Weapon2Charges = 1;
        Assert.NotEqual(StateDigest.Of(g), StateDigest.Of(h));
    }

    [Fact]
    public void SwapKeepsUpgrades()
    {
        var g = WithBelt();
        g.WeaponOverride = Wi("przecinak");
        g.WeaponLvl = 2;
        g.WeaponTrait = 0;
        g.Weapon2 = Wi("lom");
        g.Weapon2Lvl = 1;
        g.Weapon2Trait = -1;
        Assert.True(g.PlayerSwapWeapon());
        Assert.Equal(1, g.WeaponLvl);
        Assert.Equal(-1, g.WeaponTrait);
        Assert.Equal(2, g.Weapon2Lvl); // ulepszenia Przecinaka zostały w kaburze
        Assert.Equal(0, g.Weapon2Trait);
        Assert.True(g.PlayerSwapWeapon());
        Assert.Equal(2, g.WeaponLvl);
        Assert.Equal(0, g.WeaponTrait);
        Assert.Equal(1, g.Weapon2Lvl);
        Assert.Equal(-1, g.Weapon2Trait);
        // oczekujący wybór cechy należy do broni, na której zdobyto poziom
        g.TraitPending = true;
        Assert.True(g.PlayerSwapWeapon());
        Assert.False(g.TraitPending);
        Assert.True(g.Weapon2TraitPending);
    }

    [Fact]
    public void ChestAndEventGearObeyBeltRule()
    {
        var g = TestData.Arena(0);
        int belts = 0, n = 3000;
        for (var i = 0; i < n; ++i)
        {
            g.Equipped[D.BeltSlot] = -1;
            g.TakeGear(D.BeltSlot, Game.BeltTier, 0); // skrzynia i wydarzenia wołają TakeGear
            if (g.HasBelt()) ++belts;
        }
        Assert.InRange(belts / (double)n, 0.10, 0.20);
        g.Equipped[D.BeltSlot] = -1;
        g.TakeGear(D.BeltSlot, 2, 0); // inne jakości bez zmian
        Assert.Equal(2, g.Equipped[D.BeltSlot]);
    }

    [Fact]
    public void LosingBeltDropsHolsterWeapon()
    {
        var g = WithBelt();
        g.Weapon2 = Wi("przecinak");
        g.Weapon2Rarity = 2;
        g.Weapon2Charges = 2;
        g.Weapon2Lvl = 2;
        g.Weapon2Trait = 0;
        var before = g.PickupsCount;
        g.Equip(D.BeltSlot, 0, 0);
        Assert.False(g.HasBelt());
        Assert.Equal(-1, g.Weapon2);
        Assert.Equal(before + 1, g.PickupsCount);
        var p = g.Pickups[before];
        Assert.True(p.Active && p.Type == PickupType.Tool);
        Assert.Equal(Ti("przecinak"), p.Arg);
        Assert.Equal(2, p.Trait);
        Assert.Contains(g.Log.TakeLast(3), m => m.Text.Contains("wypada"));
        // podniesienie przywraca rzadkość, ładunki i ulepszenia
        g.Hero.X = p.X;
        g.Hero.Y = p.Y;
        g.Collect();
        Assert.Equal(Wi("przecinak"), g.WeaponOverride);
        Assert.Equal(2, g.WeaponRarity);
        Assert.Equal(2, g.WeaponCharges);
        Assert.Equal(2, g.WeaponLvl);
        Assert.Equal(0, g.WeaponTrait);
    }

    [Fact]
    public void DroppedHolsterWeaponSurvivesSave()
    {
        var g = WithBelt();
        g.Weapon2 = Wi("lom");
        g.Weapon2Lvl = 3;
        g.Equip(D.BeltSlot, 2, 0);
        var h = new Game(D);
        h.CopyFrom(g);
        Assert.Equal(g.Pickups[0].Extra, h.Pickups[0].Extra);
        Assert.Equal(StateDigest.Of(g), StateDigest.Of(h));
    }

    [Fact]
    public void LosingBeltWithClassWeaponInHolsterDoesNotSayLost()
    {
        // v0.23.0: broń zawodu w kaburze nie „przepada” – wraca zawsze (wpis: odkładasz); zawód, którego broń nie jest
        // narzędziem (nie wypada jako znajdźka), wybrany na stałe – pozytywna asercja zawsze sprawdzana
        var cls = Array.FindIndex(D.Classes, c => !c.Secret && Array.FindIndex(D.Tools, t => t.Weapon == c.Weapon) < 0);
        Assert.True(cls >= 0);
        var g = TestData.Arena(cls);
        g.Equip(D.BeltSlot, Game.BeltTier, 1);
        g.WeaponOverride = Wi("przecinak");
        g.Weapon2 = g.CDef.Weapon;
        g.Equip(D.BeltSlot, 0, 0);
        Assert.Equal(-1, g.Weapon2);
        Assert.DoesNotContain(g.Log.TakeLast(3), m => m.Text.Contains(Loc.T("bron_z_kabury_przepada").Trim()));
        Assert.Contains(g.Log.TakeLast(3), m => m.Text.Contains(Loc.T("bron_z_kabury_domyslna").Trim()));
    }
}
