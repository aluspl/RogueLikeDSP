namespace LifeLike.Core.Tests;

/// <summary>v0.23.2: plecak – zbieranie z karty, pełny plecak, zamiana za turę, wyrzucenie, Skup, PSZOK, zapis PBRUN19 → PBRUN20.</summary>
public class BackpackTests
{
    private static GameData D => TestData.D;

    private static int Wi(string id) => Array.FindIndex(D.Weapons, w => w.Id == id);

    private static int Ti(string weapon) => Array.FindIndex(D.Tools, t => D.Weapons[t.Weapon].Id == weapon);

    private static void Fill(Game g)
    {
        for (var k = 0; k < g.BackpackSlots(); k++) g.Backpack[k] = BackpackItem.Gear(0, 0, 0);
    }

    private static Game WithHelmetOffer(int rarity)
    {
        var g = TestData.Arena(0);
        g.Equip(0, 1, 0);
        g.Pickups[g.PickupsCount++] = new Pickup(g.Hero.X, g.Hero.Y, PickupType.GearBox, true, 0 * 3 + rarity, 2);
        g.Collect();
        Assert.True(g.HasOffer);
        return g;
    }

    private static void Hurtownia(Game g)
    {
        g.St = GameStatus.StageClear;
        g.ActCleared = true;
    }

    [Fact]
    public void DataHasSixSlotsAndSellTable()
    {
        Assert.Equal(6, D.BackpackSlots);
        Assert.Equal([1, 2, 3, 5], D.BackpackSell);
        Assert.True(D.BackpackWeaponMat >= 0 && D.BackpackGearMat >= 0 && D.BackpackAltMat >= 0);
    }

    [Fact]
    public void OfferToBackpackAndFullBackpack()
    {
        var g = WithHelmetOffer(2);
        Assert.True(g.OfferToBackpack());
        Assert.False(g.HasOffer);
        Assert.Equal(1, g.BackpackCount());
        Assert.Equal(BackpackKind.Gear, g.Backpack[0].Kind);
        Assert.Equal(2, g.Backpack[0].Rarity);
        Assert.Equal(1, g.Equipped[0]); // noszony bez zmian
        var h = WithHelmetOffer(2);
        Fill(h);
        Assert.True(h.BackpackFull());
        Assert.False(h.OfferToBackpack()); // pełny: „Do plecaka” wyszarzone
        Assert.True(h.HasOffer);
    }

    [Fact]
    public void AcceptOfferPutsOldIntoBackpackOrOnGroundWhenFull()
    {
        var g = WithHelmetOffer(2);
        g.AcceptOffer();
        Assert.Equal(2, g.Equipped[0]);
        Assert.Equal(1, g.BackpackCount());
        Assert.Equal(1, g.Backpack[0].Rarity);
        var h = WithHelmetOffer(2);
        Fill(h);
        var before = h.PickupsCount;
        h.AcceptOffer();
        Assert.Equal(2, h.Equipped[0]);
        Assert.Equal(before, h.PickupsCount); // znajdźka paczki wraca na pole ze starym kaskiem
        var p = h.Pickups[before - 1];
        Assert.True(p.Active && p.Type == PickupType.GearBox && p.Arg == 1 && p.X == h.Hero.X && p.Y == h.Hero.Y);
        Assert.Equal(Game.GearBoxUsed, p.Extra & Game.GearBoxUsed);
    }

    [Fact]
    public void DeclineLeavesBoxOnGroundAndGivesXpOnce()
    {
        var g = WithHelmetOffer(0);
        var xp = g.RunXp;
        g.DeclineOffer();
        Assert.True(g.RunXp > xp);
        Assert.True(g.Pickups[0].Active);
        var mats = g.Mats.Sum(m => m);
        xp = g.RunXp;
        g.Collect(); // drugie wejście: znowu karta, bez materiału i bez dośw. za Zostaw
        Assert.True(g.HasOffer);
        Assert.Equal(mats, g.Mats.Sum(m => m));
        g.DeclineOffer();
        Assert.Equal(xp, g.RunXp);
    }

    [Fact]
    public void WeaponCardTakeStoresOldWeaponWithUpgrades()
    {
        var g = TestData.Arena(0);
        g.WeaponLvl = 2;
        g.Pickups[g.PickupsCount++] = new Pickup(g.Hero.X, g.Hero.Y, PickupType.Tool, true, Ti("lom"), 1);
        g.Collect();
        Assert.True(g.HasToolOffer);
        g.AcceptTool();
        Assert.Equal(Wi("lom"), g.WeaponOverride);
        Assert.Equal(BackpackKind.Weapon, g.Backpack[0].Kind);
        Assert.Equal(g.CDef.Weapon, g.Backpack[0].Id);
        Assert.Equal(2, g.Backpack[0].Extra & 15);
    }

    [Fact]
    public void WeaponCardToBackpackKeepsHand()
    {
        var g = TestData.Arena(0);
        g.Pickups[g.PickupsCount++] = new Pickup(g.Hero.X, g.Hero.Y, PickupType.Tool, true, Ti("lom"), 2) { Perks = 1 };
        g.Collect();
        Assert.True(g.ToolToBackpack());
        Assert.Equal(-1, g.WeaponOverride);
        Assert.False(g.Pickups[0].Active);
        Assert.Equal(Wi("lom"), g.Backpack[0].Id);
        Assert.Equal(2, g.Backpack[0].Rarity);
        Assert.Equal(1, g.Backpack[0].Perks);
        Assert.NotEqual(0UL, g.ToolsFound & (1UL << Ti("lom")));
        var h = TestData.Arena(0);
        Fill(h);
        h.Pickups[h.PickupsCount++] = new Pickup(h.Hero.X, h.Hero.Y, PickupType.Tool, true, Ti("lom"), 2);
        h.Collect();
        Assert.False(h.ToolToBackpack());
        Assert.True(h.HasToolOffer);
    }

    [Fact]
    public void SwapCostsTurnOnSiteButNotInHurtownia()
    {
        var g = TestData.Arena(0);
        g.Backpack[0] = BackpackItem.Weapon(Wi("lom"), 2, 0, 3);
        var turns = g.Turns;
        Assert.True(g.BackpackEquip(0));
        Assert.Equal(turns + 1, g.Turns);
        Assert.Equal(Wi("lom"), g.WeaponOverride);
        Assert.Equal(3, g.WeaponLvl);
        Assert.Equal(g.CDef.Weapon, g.Backpack[0].Id); // broń zawodu w plecaku
        Assert.True(g.BackpackEquip(0)); // z powrotem
        Assert.Equal(-1, g.WeaponOverride);
        Assert.Equal(turns + 2, g.Turns);
        var h = TestData.Arena(0);
        h.Backpack[0] = BackpackItem.Gear(2, 2, 0);
        Hurtownia(h);
        turns = h.Turns;
        var hp = h.Hero.MaxHp;
        Assert.True(h.BackpackEquip(0));
        Assert.Equal(turns, h.Turns);
        Assert.Equal(2, h.Equipped[2]);
        Assert.True(h.Hero.MaxHp > hp || D.Gear[2 * 3 + 2].Stat != Data.GearStat.Hp);
        Assert.True(h.Backpack[0].Empty); // slot był pusty
    }

    [Fact]
    public void CompareArrows()
    {
        var g = TestData.Arena(0);
        g.Equip(0, 1, 0);
        g.Backpack[0] = BackpackItem.Gear(0, 2, 0);
        g.Backpack[1] = BackpackItem.Gear(0, 0, 0);
        g.Backpack[2] = BackpackItem.Gear(0, 1, 3);
        Assert.Equal(1, g.BackpackCompare(0));
        Assert.Equal(-1, g.BackpackCompare(1));
        Assert.Equal(0, g.BackpackCompare(2));
    }

    [Fact]
    public void DropPutsItemOnGroundClassWeaponIsLost()
    {
        var g = TestData.Arena(0);
        g.Backpack[0] = BackpackItem.Weapon(Wi("lom"), 1, 0, 0);
        g.Backpack[1] = BackpackItem.Weapon(g.CDef.Weapon, 0, 0, 0);
        var turns = g.Turns;
        Assert.True(g.BackpackDrop(0));
        Assert.Equal(turns, g.Turns);
        Assert.Equal(1, g.PickupsCount);
        Assert.True(g.Pickups[0].Type == PickupType.Tool && g.Pickups[0].Arg == Ti("lom") && g.Pickups[0].X == g.Hero.X);
        Assert.True(g.BackpackDrop(1));
        Assert.Equal(1, g.PickupsCount); // broń zawodu nie leży na ziemi
        Assert.Equal(0, g.BackpackCount());
    }

    [Fact]
    public void SkupGivesMaterialsByTier()
    {
        var g = TestData.Arena(0);
        Array.Clear(g.Mats);
        g.Backpack[0] = BackpackItem.Weapon(Wi("lom"), 2, 0, 0); // Markowa: 3 = 2 stal + 1 cement
        g.Backpack[1] = BackpackItem.Gear(0, 0, 0);               // Zwykły: 1 drewno
        g.Backpack[2] = BackpackItem.Gear(1, 1, 0);               // Solidny: 1 drewno + 1 cement
        Assert.False(g.BackpackSell(0)); // tylko w Hurtowni
        Hurtownia(g);
        Assert.True(g.BackpackSell(0) && g.BackpackSell(1) && g.BackpackSell(2));
        Assert.Equal(2, g.Mats[D.BackpackWeaponMat]);
        Assert.Equal(2, g.Mats[D.BackpackGearMat]);
        Assert.Equal(2, g.Mats[D.BackpackAltMat]);
        Assert.Equal(0, g.BackpackCount());
        var unique = Array.FindIndex(D.Weapons, w => w.Master);
        Assert.Equal(5, g.SellCount(BackpackItem.Weapon(unique, 3, 0, 0)));
    }

    [Fact]
    public void PszokGivesXpAndHp()
    {
        var g = TestData.Arena(0);
        g.Hero.Hp = 5;
        g.Backpack[3] = BackpackItem.Gear(0, 2, 0);
        Hurtownia(g);
        var xp = g.RunXp;
        Assert.Equal(3 + 2 * 2, g.PszokXpFor(g.Backpack[3]));
        Assert.True(g.BackpackRecycle(3));
        Assert.True(g.RunXp > xp);
        Assert.Equal(7, g.Hero.Hp);
        Assert.True(g.Backpack[3].Empty);
    }

    [Fact]
    public void BotSkupKeepsBestSpareWeapon()
    {
        var g = TestData.Arena(0);
        g.Backpack[0] = BackpackItem.Weapon(Wi("lom"), 0, 0, 0);
        g.Backpack[1] = BackpackItem.Weapon(Wi("lom"), 2, 0, 2);
        g.Backpack[2] = BackpackItem.Gear(0, 1, 0);
        Hurtownia(g);
        g.BotSkup();
        Assert.Equal(1, g.BackpackCount());
        Assert.Equal(2, g.Backpack[1].Rarity);
    }

    [Fact]
    public void HolsterWeaponGoesToBackpackWhenBeltLost()
    {
        var g = TestData.Arena(0);
        g.Equip(D.BeltSlot, Game.BeltTier, 1);
        g.Weapon2 = Wi("lom");
        g.Weapon2Rarity = 2;
        var before = g.PickupsCount;
        g.Equip(D.BeltSlot, 0, 0);
        Assert.Equal(-1, g.Weapon2);
        Assert.Equal(before, g.PickupsCount);
        Assert.Equal(Wi("lom"), g.Backpack[0].Id);
        Assert.Equal(2, g.Backpack[0].Rarity);
    }

    [Fact]
    public void RunSaveKeepsBackpackAndMigratesPbrun19()
    {
        var g = TestData.Run(2, 9);
        g.Backpack[2] = BackpackItem.Weapon(Wi("lom"), 2, 3, 0x21);
        g.Backpack[5] = BackpackItem.Gear(1, 1, 4);
        g.OfferPickup = 3;
        var s = RunSave.Make(g);
        Assert.Equal("PBRUN20", RunSave.RunMagic);
        Assert.True(s.Valid(D));
        var l = RunSave.FromBytes(s.ToBytes()).Load(D);
        Assert.Equal(g.Backpack[2].Extra, l.Backpack[2].Extra);
        Assert.Equal(g.Backpack[5].Trait, l.Backpack[5].Trait);
        Assert.Equal(3, l.OfferPickup);
        // PBRUN19: stan bez ogona plecaka – pusty plecak, oferta bez znajdźki
        var data = g.ToBytes()[..^RunSave.BackpackTail];
        var o = new RunSave { Magic = Profile.MagicBytes(RunSave.PrevMagic), Size = (uint)data.Length, Data = data, Checksum = RunSave.Fnv1a(data) };
        Assert.True(o.Valid(D));
        var lo = RunSave.FromBytes(o.ToBytes()).Load(D);
        Assert.NotNull(lo);
        Assert.Equal(0, lo.BackpackCount());
        Assert.Equal(-1, lo.OfferPickup);
        Assert.Equal(g.Stage, lo.Stage);
        Assert.Equal(g.Hero.Hp, lo.Hero.Hp);
        // PBRUN20 o rozmiarze PBRUN19 – odrzucony
        var bad = new RunSave { Magic = Profile.MagicBytes(RunSave.RunMagic), Size = (uint)data.Length, Data = data, Checksum = RunSave.Fnv1a(data) };
        Assert.False(bad.Valid(D));
    }
}
