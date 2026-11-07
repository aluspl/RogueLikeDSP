namespace LifeLike.Core.Tests;

/// <summary>
/// v0.23.0: losowe profity broni wg rzadkości – Zwykła 0, Solidna 1, Markowa 2 z puli (bez powtórzeń), Mistrzowska
/// 2 stałe wg typu ciosu (albo masterPerks broni); deterministyczne z seeda budowy.
/// </summary>
public class WeaponPerkTests
{
    private static GameData D => TestData.D;

    private static int Wi(string id) => Array.FindIndex(D.Weapons, w => w.Id == id);

    private static int Pi(string id) => Array.FindIndex(D.WeaponPerkPool, p => p.Id == id);

    private static int Mask(params string[] ids) => ids.Aggregate(0, (m, id) => m | 1 << Pi(id));

    private static int Master => D.WeaponRarities.Length - 1;

    [Fact]
    public void PerkDataIsParsed()
    {
        Assert.Equal(["przebicie", "ostrze", "wywazenie", "glowica", "chwyt"], D.WeaponPerkPool.Select(p => p.Id));
        Assert.Equal([0, 1, 2, 0], D.WeaponPerksPerRarity);
        Assert.Equal(ToolTraitEffect.Pierce, D.WeaponPerkPool[Pi("przebicie")].Effect);
        Assert.Equal(1, D.WeaponPerkPool[Pi("przebicie")].Value);
        Assert.Equal(5, D.WeaponPerkPool[Pi("ostrze")].Value);
        Assert.Equal(ToolTraitEffect.MaxDmg, D.WeaponPerkPool[Pi("glowica")].Effect);
        Assert.Equal(ToolTraitEffect.CooldownB, D.WeaponPerkPool[Pi("chwyt")].Effect);
        Assert.All(D.WeaponPerkPool, p => Assert.False(string.IsNullOrEmpty(p.Short)));
    }

    [Fact]
    public void RolledCountFollowsRarityWithoutRepeats()
    {
        for (uint seed = 1; seed <= 60; ++seed)
        {
            var g = TestData.Run(0, seed);
            for (var r = 0; r < Master; ++r)
            {
                for (var k = 0; k < 10; ++k)
                {
                    var m = g.RollWeaponPerks(Wi("lom"), r);
                    Assert.Equal(D.WeaponPerksPerRarity[r], System.Numerics.BitOperations.PopCount((uint)m)); // bity = bez powtórzeń
                    Assert.Equal(0, m >> D.WeaponPerkPool.Length);
                }
            }
        }
    }

    [Fact]
    public void RolledPerksCoverWholePool()
    {
        var g = TestData.Run(0, 9);
        var seen = 0;
        for (var i = 0; i < 300; ++i) seen |= g.RollWeaponPerks(Wi("lom"), 2);
        Assert.Equal((1 << D.WeaponPerkPool.Length) - 1, seen);
    }

    [Fact]
    public void MasterGetsFixedPerksByHitType()
    {
        var g = TestData.Run(0, 3);
        int Of(HitType h) => Array.FindIndex(D.Weapons, w => w.Hit[0] == h && w.MasterPerks.Length == 0);
        Assert.Equal(Mask("wywazenie", "glowica"), g.RollWeaponPerks(Of(HitType.Blunt), Master));
        Assert.Equal(Mask("ostrze", "glowica"), g.RollWeaponPerks(Of(HitType.Cut), Master));
        Assert.Equal(Mask("przebicie", "ostrze"), g.RollWeaponPerks(Of(HitType.Pierce), Master));
        Assert.Equal(Mask("przebicie", "ostrze"), g.RollWeaponPerks(Wi("noz_majstra"), Master));
        Assert.Equal(["przebicie", "ostrze"], D.Weapons[Wi("noz_majstra")].MasterPerks);
        // nadpisanie masterPerks w danych broni wygrywa z typem ciosu
        var blunt = Of(HitType.Blunt);
        var h = new Game(D.WithWeapon(blunt, D.Weapons[blunt] with { MasterPerks = ["chwyt", "ostrze"] }));
        h.NewRun(0, 3);
        Assert.Equal(Mask("chwyt", "ostrze"), h.RollWeaponPerks(blunt, Master));
    }

    [Fact]
    public void SameSeedSamePerks()
    {
        var a = TestData.Run(0, 4242);
        var b = TestData.Run(0, 4242);
        var c = TestData.Run(0, 4243);
        var diff = false;
        for (var i = 0; i < 20; ++i)
        {
            var x = a.RollWeaponPerks(Wi("lom"), 2);
            Assert.Equal(x, b.RollWeaponPerks(Wi("lom"), 2));
            diff |= x != c.RollWeaponPerks(Wi("lom"), 2);
        }
        Assert.True(diff, "inny seed – inne profity");
    }

    [Fact]
    public void PerkRollDoesNotMoveGameRng()
    {
        var a = TestData.Run(0, 77);
        var b = TestData.Run(0, 77);
        a.RollWeaponPerks(Wi("lom"), 2);
        Assert.Equal(a.R.S, b.R.S);
    }

    [Fact]
    public void StartWeaponPerksFollowStartRarity()
    {
        var m = RunMods.Default(D);
        m.WeaponRarity = 2;
        for (uint seed = 1; seed <= 20; ++seed)
        {
            var g = new Game(D);
            g.NewRun(0, seed, D.DefaultDifficulty, m);
            Assert.Equal(2, g.WeaponRarity);
            Assert.Equal(2, System.Numerics.BitOperations.PopCount(g.WeaponPerks));
            var h = new Game(D);
            h.NewRun(0, seed, D.DefaultDifficulty, m);
            Assert.Equal(g.WeaponPerks, h.WeaponPerks);
        }
        var plain = TestData.Run(0, 5);
        Assert.Equal(0, plain.WeaponPerks);
        var mm = RunMods.Default(D);
        mm.Mastery = MasteryBit.Weapon;
        var master = new Game(D);
        master.NewRun(0, 5, D.DefaultDifficulty, mm);
        Assert.Equal(Master, master.WeaponRarity);
        Assert.Equal(D.MasterPerkMask(D.Classes[0].Weapon), master.WeaponPerks);
        Assert.Equal(2, System.Numerics.BitOperations.PopCount(master.WeaponPerks));
    }
}
