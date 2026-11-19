namespace LifeLike.Core.Tests;

/// <summary>v0.23.0: rzadkość broni (Zwykła, Solidna, Markowa, Mistrzowska): losowanie, Szkolenie, broń mistrza, szeroka maska broni, zapisy.</summary>
public class WeaponRarityTests
{
    private static GameData D => TestData.D;

    private static int Wi(string id) => Array.FindIndex(D.Weapons, w => w.Id == id);

    private static int Ti(string weapon) => Array.FindIndex(D.Tools, t => D.Weapons[t.Weapon].Id == weapon);

    private static int UpgradeIndex() => Array.FindIndex(D.Upgrades, u => u.Id == "narzedzia");

    [Fact]
    public void DropNeverMaster()
    {
        var g = TestData.Run(0, 1234);
        var seen = new int[4];
        for (var i = 0; i < 10000; ++i) ++seen[g.RollWeaponRarity()];
        Assert.Equal(0, seen[3]);
        Assert.True(seen[0] > 0 && seen[1] > 0 && seen[2] > 0, "wszystkie trzy losowane poziomy się zdarzają");
        Assert.True(seen[0] > seen[1] && seen[1] > seen[2], "Zwykła najczęściej, Markowa najrzadziej");
    }

    [Fact]
    public void RollImprovesWithStage()
    {
        var early = TestData.Run(0, 1234);
        var late = TestData.Run(0, 1234);
        late.Stage = D.Stages.Length - 1;
        int a = 0, b = 0;
        for (var i = 0; i < 5000; ++i)
        {
            a += early.RollWeaponRarity();
            b += late.RollWeaponRarity();
        }
        Assert.True(b > a, "późniejszy etap daje lepszą rzadkość");
    }

    [Fact]
    public void TrainingRaisesStartRarity()
    {
        Assert.True(UpgradeIndex() >= 0 && D.Upgrades[UpgradeIndex()].Levels == 2);
        for (var lvl = 0; lvl <= 2; ++lvl)
        {
            var p = Meta.NewProfile(D);
            p.Levels[UpgradeIndex()] = (byte)lvl;
            var g = new Game(D);
            g.NewRun(0, 99, D.DefaultDifficulty, Meta.Mods(D, p));
            Assert.Equal(lvl, g.WeaponRarity);
            Assert.False(g.WeaponMaster());
            Assert.Equal(D.WeaponRarities[lvl].DmgBonus, g.WeaponDmgBonus());
        }
    }

    [Fact]
    public void MasterWeaponIsMaster()
    {
        var m = RunMods.Default(D);
        m.Mastery |= MasteryBit.Weapon;
        var g = new Game(D);
        g.NewRun(0, 5, D.DefaultDifficulty, m);
        Assert.True(g.WeaponMaster());
        Assert.Equal(3, g.WeaponRarity);
        Assert.Equal(D.WeaponRarities[3].DmgBonus, g.WeaponDmgBonus());
        Assert.Equal(0, D.WeaponRarities[3].DmgBonus); // Mistrzowska: tylko złoty kolor, efekty jak dziś (spec 4.7.4)
        Assert.Equal(0, D.WeaponRarities[3].CooldownDelta);

        var u = TestData.Run(0, 5); // unikat z sekretu jest Mistrzowski, zwykła podniesiona broń nie
        u.TakeTool(Ti("mlot_zenka"), 1);
        Assert.True(u.WeaponMaster());
        Assert.Equal(3, u.WeaponRarity);
        u.TakeTool(Ti("lom"), 1);
        Assert.False(u.WeaponMaster());
        Assert.Equal(1, u.WeaponRarity);
    }

    [Fact]
    public void RarityShortensModeBCooldown()
    {
        Assert.True(D.WeaponRarities[2].CooldownDelta < 0);
        for (var r = 0; r <= 3; ++r)
        {
            var g = TestData.Arena(0);
            g.WeaponOverride = Wi("kielnia");
            g.WeaponRarity = (byte)r;
            g.Spawn(Array.FindIndex(D.Enemies, e => e.Id == "plesn"), 9, 7);
            g.Enemies[0].Hp = g.Enemies[0].MaxHp = 5000;
            g.UpdateFov();
            Assert.True(g.PlayerWeaponB(0));
            Assert.Equal(Math.Max(1, D.Weapons[Wi("kielnia")].ModeB.Cooldown + D.WeaponRarities[r].CooldownDelta), g.WeaponBCd);
        }
    }

    [Fact]
    public void DroppedToolCarriesRarity()
    {
        var g = TestData.Run(0, 77);
        g.Bonus.Tools = ulong.MaxValue >> (64 - D.Tools.Length);
        var rolled = new HashSet<int>();
        for (var i = 0; i < 400; ++i)
        {
            g.PickupsCount = 0;
            g.Bonus.ToolPct = 100;
            g.DropAt(3, 3);
            if (g.Pickups[0].Type != PickupType.Tool) continue;
            if (ArtifactCase.Index(D, D.Tools[g.Pickups[0].Arg].Weapon) >= 0) continue; // artefakt z Markowej bazowej jest Mistrzowski
            rolled.Add(g.Pickups[0].Trait);
        }
        Assert.True(rolled.Contains(0) && rolled.Contains(1));
        Assert.DoesNotContain(3, rolled);
    }

    [Fact]
    public void OldRunSaveRejected()
    {
        var g = TestData.Run(0, 5);
        var rs = RunSave.Make(g);
        rs.Magic = Profile.MagicBytes("PBRUN13");
        Assert.False(rs.Valid(D));
        Assert.Null(RunSave.FromBytes(rs.ToBytes()).Load(D));
        Assert.NotNull(RunSave.Make(g).Load(D));
        Assert.Equal("PBRUN20", RunSave.RunMagic);
    }

    [Fact]
    public void RunSaveKeepsRarityAndFoundTools()
    {
        var g = TestData.Run(0, 5);
        g.TakeTool(Ti("laser_krzyzowy"), 2);
        var h = RunSave.FromBytes(RunSave.Make(g).ToBytes()).Load(D);
        Assert.Equal(2, h.WeaponRarity);
        Assert.Equal(g.ToolsFound, h.ToolsFound);
        Assert.Equal(StateDigest.Of(g), StateDigest.Of(h));
    }

    [Fact]
    public void OldProfileMigrates()
    {
        var old = Meta.NewProfile(D);
        old.Best = 1234;
        old.Xp = 99;
        old.Wins = 3;
        old.Levels[0] = 2;
        old.Tools = 0x0E;
        old.ToolsFound = 0x05;
        var raw = old.ToBytes()[..Profile.V17Size];
        Profile.MagicBytes("PBRL017").CopyTo(raw, 0);
        var p = Profile.FromBytes(raw);
        Assert.True(Meta.ProfileFix(D, p));
        Assert.True(p.MagicIs(Profile.MagicCurrent) && Profile.MagicCurrent == "PBRL020");
        Assert.True(p.Best == 1234 && p.Xp == 99 && p.Wins == 3 && p.Levels[0] == 2 && p.Tools == 0x0E && p.ToolsFound == 0x05);
        Assert.Equal(Profile.Size, p.ToBytes().Length);
        var junk = Profile.FromBytes(Enumerable.Repeat((byte)0x7F, 10).ToArray());
        Assert.True(Meta.ProfileFix(D, junk) && junk.MagicIs(Profile.MagicCurrent));
    }

    [Fact]
    public void ToolsMaskHolds39()
    {
        var p = Meta.NewProfile(D);
        p.Tools = (1UL << 38) | (1UL << 9) | 1;
        p.ToolsFound = (1UL << 38) | (1UL << 20);
        var q = Profile.FromBytes(p.ToBytes());
        Assert.Equal(p.Tools, q.Tools);
        Assert.Equal(p.ToolsFound, q.ToolsFound);
        Assert.Equal(33, D.Tools.Length); // v0.23.0 (#84): + 2 noże, + 4 artefakty
        Assert.Equal(1UL << 38, q.Tools & (1UL << 38));
    }

    [Fact]
    public void MergedToolListKeepsSourceFlags()
    {
        Assert.Equal(10 + D.NewTools.Length, D.Tools.Length);
        foreach (var n in D.NewTools)
        {
            var t = D.Tools[Ti(D.Weapons[n.Weapon].Id)];
            Assert.True(t.Weapon == n.Weapon && t.Shop == n.Shop && t.Drop == n.Drop && t.Hurtownia == n.Hurtownia && t.Secret == n.Secret);
        }
        Assert.Equal(D.Tools.Count(t => t.Shop), D.ToolCosts.Length);
    }
}
