namespace LifeLike.Core.Tests;

/// <summary>
/// v0.23.0: skąd artefakty i Gablota – Markowa broń bazowa z dropu zamienia się w artefakt z szansą artifacts.chancePct (broń zawodu
/// bohatera też jest bazą), bez powtórki w tej samej budowie; Gablota w profilu (PBRL019): maska odkrytych + dzień znalezienia,
/// migracja v18 -> v19 bez utraty danych (znane już unikaty trafiają do Gabloty bez daty).
/// </summary>
public class ArtifactCaseTests
{
    private static GameData D => TestData.D;

    private static int Wi(string id) => Array.FindIndex(D.Weapons, w => w.Id == id);

    private static int Ti(string weapon) => Array.FindIndex(D.Tools, t => D.Weapons[t.Weapon].Id == weapon);

    private static int Ci(string id) => Array.FindIndex(D.Classes, c => c.Id == id);

    private static int Ai(string weapon) => ArtifactCase.Index(D, Wi(weapon));

    /// <summary>Drop narzędzi same z Kolekcjonera; pula = tylko jedno kupione narzędzie (plus nowe bronie z dropu).</summary>
    private static Game Drops(string cls, string tool)
    {
        var g = TestData.Arena(Ci(cls));
        g.Bonus.ToolPct = 100;
        g.Bonus.Tools = 1UL << Ti(tool);
        return g;
    }

    /// <summary>n dropów narzędzi: (Markowa broń bazowa, artefakt danej bazy, inne artefakty).</summary>
    private static (int Base, int Art, int Other) Sample(Game g, string baseWeapon, string artifact, int n, bool resetRun = true)
    {
        int b = 0, a = 0, o = 0;
        for (var k = 0; k < n; ++k)
        {
            if (resetRun) g.ArtifactsRun = 0;
            g.PickupsCount = 0;
            g.DropAt(g.Hero.X, g.Hero.Y);
            var p = g.Pickups[0];
            if (p.Type != PickupType.Tool) continue;
            var w = D.Tools[p.Arg].Weapon;
            if (w == Wi(artifact))
            {
                ++a;
                Assert.Equal(3, p.Trait); // Mistrzowska
                Assert.Equal(D.MasterPerkMask(w), p.Perks);
            }
            else if (D.Weapons[w].Master) ++o;
            else if (w == Wi(baseWeapon) && p.Trait == 2) ++b;
            else Assert.True(p.Trait < 3);
        }
        return (b, a, o);
    }

    [Fact]
    public void CaseListsAllMasterWeaponsWithBases()
    {
        var list = D.Artifacts;
        Assert.Equal(D.Weapons.Count(w => w.Master), list.Length);
        Assert.Equal(8, list.Length);
        foreach (var w in list)
        {
            Assert.True(D.Weapons[w].Master);
            Assert.False(string.IsNullOrEmpty(D.Weapons[w].Hint));
            Assert.True(Ti(D.Weapons[w].Id) >= 0, D.Weapons[w].Id);
        }
        Assert.Equal(Wi("mlot"), D.ArtifactBase(Wi("mlot_grom")));
        Assert.Equal(Wi("kielnia"), D.ArtifactBase(Wi("kielnia_cegla")));
        Assert.Equal(Wi("szlifierka"), D.ArtifactBase(Wi("szlifierka_isk")));
        Assert.Equal(Wi("lom"), D.ArtifactBase(Wi("lom_deska")));
        Assert.Equal(Wi("noz"), D.ArtifactBase(Wi("noz_majstra")));
        Assert.Equal(-1, D.ArtifactBase(Wi("mlot")));
        Assert.Equal(25, D.ArtifactChancePct);
        // artefakty nie są narzędziami na start, w sklepie ani w zwykłej puli dropu
        foreach (var w in list)
        {
            var t = Ti(D.Weapons[w].Id);
            Assert.Equal(0UL, D.StartToolsMask & (1UL << t));
            Assert.Equal(0UL, D.DropToolsMask & (1UL << t));
            Assert.Equal(0UL, D.HurtowniaToolsMask & (1UL << t));
        }
    }

    [Fact]
    public void BrandedBaseDropBecomesArtifactWithChance()
    {
        var g = Drops("kierownik", "mlot");
        var (b, a, _) = Sample(g, "mlot", "mlot_grom", 20000);
        Assert.True(a > 20, $"artefakty {a}");
        var pct = a * 100.0 / (a + b);
        Assert.InRange(pct, 15, 35);
    }

    [Fact]
    public void ClassWeaponIsBaseToo()
    {
        var g = Drops("murarz", "mlot");
        var (_, a, _) = Sample(g, "mlot", "kielnia_cegla", 20000);
        Assert.True(a > 20, $"kielnie {a}");
        var h = Drops("kierownik", "wkretarka");
        var (_, a2, _) = Sample(h, "wkretarka", "kielnia_cegla", 20000);
        Assert.Equal(0, a2); // Kierownik: Kielnia nie jest jego bronią
    }

    [Fact]
    public void NoArtifactTwiceInOneRun()
    {
        var g = Drops("kierownik", "mlot");
        var (_, a, _) = Sample(g, "mlot", "mlot_grom", 20000, resetRun: false);
        Assert.Equal(1, a);
        Assert.NotEqual(0, g.ArtifactsRun & (1 << Ai("mlot_grom")));
    }

    [Fact]
    public void ArtifactTakenCountsAsFound()
    {
        var g = TestData.Arena(0);
        g.Pickups[g.PickupsCount++] = new Pickup(g.Hero.X, g.Hero.Y, PickupType.Tool, true, Ti("mlot_grom"), 3);
        g.Collect();
        Assert.Equal(Wi("mlot_grom"), g.WeaponOverride);
        Assert.Equal(3, g.WeaponRarity);
        Assert.NotEqual(0UL, g.ToolsFound & (1UL << Ti("mlot_grom")));
        Assert.NotEqual(0, g.ArtifactsRun & (1 << Ai("mlot_grom")));
    }

    [Fact]
    public void BankRecordsFoundArtifactsWithFirstDay()
    {
        var p = Meta.NewProfile(D);
        var g = TestData.Arena(0);
        g.TakeTool(Ti("mlot_grom"), 3, false);
        var got = ArtifactCase.Bank(D, p, g, 20400);
        var i = Ai("mlot_grom");
        Assert.Equal(1 << i, got);
        Assert.True(ArtifactCase.Found(p, i));
        Assert.Equal(20400, ArtifactCase.Day(p, i));
        Assert.Equal(1, ArtifactCase.FoundCount(D, p));
        Assert.Equal(0, ArtifactCase.Bank(D, p, g, 20500));
        Assert.Equal(20400, ArtifactCase.Day(p, i)); // pierwsza data zostaje
        var q = Profile.FromBytes(p.ToBytes());
        Assert.True(ArtifactCase.Found(q, i));
        Assert.Equal(20400, ArtifactCase.Day(q, i));
        Assert.Equal(Profile.Size, p.ToBytes().Length);
    }

    [Fact]
    public void HolsteredArtifactCountsToo()
    {
        var p = Meta.NewProfile(D);
        var g = TestData.Arena(0);
        g.Equip(D.BeltSlot, Game.BeltTier, 1);
        g.Pickups[g.PickupsCount++] = new Pickup(g.Hero.X, g.Hero.Y, PickupType.Tool, true, Ti("kielnia_cegla"), 3);
        g.Collect();
        Assert.Equal(Wi("kielnia_cegla"), g.Weapon2);
        Assert.Equal(1 << Ai("kielnia_cegla"), ArtifactCase.Bank(D, p, g, 100));
    }

    [Fact]
    public void ProfileV18MigratesToV19KeepingDataAndKnownUniques()
    {
        var old = Meta.NewProfile(D);
        old.Best = 4321;
        old.Xp = 77;
        old.Tools = (1UL << 38) | 3;
        old.ToolsFound = (1UL << Ti("noz_majstra")) | 1;
        var raw = old.ToBytes()[..Profile.V18Size];
        Profile.MagicBytes("PBRL018").CopyTo(raw, 0);
        var p = Profile.FromBytes(raw);
        Assert.Equal(old.Tools, p.Tools); // v18 ma już szerokie maski
        Assert.True(Meta.ProfileFix(D, p));
        Assert.True(p.MagicIs(Profile.MagicCurrent) && Profile.MagicCurrent == "PBRL020");
        Assert.True(p.Best == 4321 && p.Xp == 77 && p.Tools == old.Tools && p.ToolsFound == old.ToolsFound);
        var i = Ai("noz_majstra");
        Assert.True(ArtifactCase.Found(p, i));
        Assert.Equal(0, ArtifactCase.Day(p, i)); // bez daty: przed Gablotą
        Assert.Equal(1, ArtifactCase.FoundCount(D, p));
        Assert.Equal(Profile.Size, p.ToBytes().Length);
    }

    /// <summary>Bit Gabloty = pozycja na liście artefaktów: kolejność tylko dopisywana (profil v19, GBA w Task 10 – ta sama).</summary>
    [Fact]
    public void CaseOrderIsPinnedAppendOnly()
    {
        Assert.Equal(new[] { "mlot_zenka", "poziomica_m", "pilarka", "noz_majstra", "mlot_grom", "kielnia_cegla", "szlifierka_isk", "lom_deska" },
            D.Artifacts.Select(w => D.Weapons[w].Id));
    }

    [Fact]
    public void V18MigrationKeepsBytesAndIsIdempotent()
    {
        var old = Meta.NewProfile(D);
        old.Best = 999;
        old.Xp = 1234;
        old.Runs = 17;
        old.Levels[1] = 1;
        old.Tools = (1UL << 30) | 1;
        old.ToolsFound = (1UL << Ti("noz_majstra")) | (1UL << Ti("pilarka")) | 1;
        old.KillCount[3] = 42;
        old.Respect = 77;
        var raw = old.ToBytes()[..Profile.V18Size];
        Profile.MagicBytes("PBRL018").CopyTo(raw, 0);
        var p = Profile.FromBytes(raw);
        Assert.True(Meta.ProfileFix(D, p));
        var b = p.ToBytes();
        Assert.Equal(raw[8..Profile.V18Size], b[8..Profile.V18Size]); // dane v18 bajt w bajt
        Assert.Equal((1 << Ai("noz_majstra")) | (1 << Ai("pilarka")), p.Artifacts);
        var again = Profile.FromBytes(b);
        Assert.False(Meta.ProfileFix(D, again)); // drugi raz nic do naprawy
        Assert.Equal(b, again.ToBytes());
    }

    [Fact]
    public void V17ProfileWithSecretUniqueSeedsCase()
    {
        var old = Meta.NewProfile(D);
        old.Best = 321;
        var s = Array.FindIndex(D.Secrets, x => x.Reward == SecretReward.Tool && x.Index == Ti("mlot_zenka"));
        old.Secrets = (ushort)(1 << s); // Młot Zenka z sekretu (v17: maski broni 8-bitowe, unikaty tylko przez sekret)
        var raw = old.ToBytes()[..Profile.V17Size];
        Profile.MagicBytes("PBRL017").CopyTo(raw, 0);
        var p = Profile.FromBytes(raw);
        Assert.True(Meta.ProfileFix(D, p));
        Assert.True(p.MagicIs(Profile.MagicCurrent));
        Assert.Equal(321, p.Best);
        Assert.True(ArtifactCase.Found(p, Ai("mlot_zenka")));
        Assert.Equal(0, ArtifactCase.Day(p, Ai("mlot_zenka")));
        Assert.Equal(1, ArtifactCase.FoundCount(D, p));
    }

    [Fact]
    public void RecordRunIsNotNeededForCaseButBankIsIdempotent()
    {
        var p = Meta.NewProfile(D);
        var g = TestData.Arena(0);
        Assert.Equal(0, ArtifactCase.Bank(D, p, g, 1));
        Assert.Equal(0, ArtifactCase.FoundCount(D, p));
    }
}
