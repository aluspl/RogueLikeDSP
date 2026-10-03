namespace LifeLike.Core.Tests;

/// <summary>
/// core_tests.cpp: 53 (v0.21.52 cz. a, tempo postępu): Szkolenia z poziomami (mniejsze kroki, rosnąca cena), zawody
/// i narzędzia drożeją z każdym zakupem, odznaki i zlecenia dają tytuły i kolory kasku zamiast dużego doświadczenia,
/// profil v13 (zwrot za Szkolenia po starej cenie).
/// </summary>
public class ProgressionPaceTests
{
    private static GameData D => TestData.D;

    [Fact]
    public void UpgradesHaveRisingLevelsAndLabels()
    {
        for (var i = 0; i < D.Upgrades.Length; ++i)
        {
            var u = D.Upgrades[i];
            Assert.InRange(u.Levels, 4, 5);
            for (var l = 1; l < u.Levels; ++l) Assert.True(u.Cost(l) > u.Cost(l - 1), u.Name);
            Assert.True(Meta.UpgradeTotal(D, i, u.Levels, u.Effect) > 0, u.Name);
            foreach (var st in u.Steps) Assert.InRange(RunMods.UpgradeLabel(new Message(), st.Effect, st.Value).N, 4, 29);
            Assert.InRange(Meta.UpgradeSummary(D, new Message(), i, u.Levels).N, 4, Message.Len - 2);
            Assert.Equal(0, Meta.UpgradeSummary(D, new Message(), i, 0).N);
        }
        var f = Meta.NewProfile(D);
        TestData.FullTree(f); // v0.21.52 cz. c: pień + wybory w węzłach (kawa i stat. broni w węzłach Apteczka i Rzemieślnik)
        var m = Meta.Mods(D, f);
        Assert.True(m.Hp >= 4 && m.TakenPct >= 2 && m.DmgPct >= 2 && m.Coffee >= 1 && m.Pickups >= 1 && m.Luck >= 1 && m.Craft == 0);
        Assert.True(m.Hp <= 6 && m.TakenPct <= 5 && m.DmgPct <= 5 && m.Luck <= 1 && m.Craft <= 1 && m.Pickups <= 1);
        var s0 = Meta.ModsPart(D, f, 0);
        Assert.True(s0.Hp == m.Hp && s0.DmgPct == m.DmgPct && s0.TakenPct == m.TakenPct && s0.Crit == m.Crit && s0.Dodge == m.Dodge && s0.Luck == m.Luck);
    }

    [Fact]
    public void ClassesAndToolsGetPricierWithEachPurchase()
    {
        var p = Meta.NewProfile(D);
        p.Xp = 100000;
        int prev = 0, bought = 0;
        for (var c = 0; c < D.Classes.Length; ++c)
        {
            if (!Meta.ClassForSale(D, c)) continue;
            int price = Meta.ClassCost(D, p), xp0 = p.Xp;
            Assert.True(price == D.ClassCosts[Math.Min(bought, D.ClassCosts.Length - 1)] && price > prev);
            Assert.True(Meta.BuyClass(D, p, c) && p.Xp == xp0 - price && Meta.ClassesBought(D, p) == ++bought);
            prev = price;
        }
        Assert.Equal(D.ClassCosts.Length, bought);
        prev = 0;
        bought = 0;
        for (var i = 0; i < D.Tools.Length; ++i)
        {
            if (!D.Tools[i].Shop)
            {
                Assert.False(Meta.BuyTool(D, p, i));
                continue;
            }
            int price = Meta.ToolCost(D, p), xp0 = p.Xp;
            Assert.True(price == D.ToolCosts[bought] && price > prev && Meta.BuyTool(D, p, i) && p.Xp == xp0 - price && Meta.ToolsBought(D, p) == ++bought);
            prev = price;
        }
        Assert.True(bought == D.ToolCosts.Length && D.HardCost == 100);
        for (var i = 0; i < D.Upgrades.Length; ++i)
        {
            while (Meta.BuyUpgrade(D, p, i))
            {
            }
        }
        for (var i = 0; i < D.Brigade.Length; ++i) Meta.BuyHelper(D, p, i);
        Meta.BuyHard(D, p);
        for (var n = 0; n < D.TreeNodes.Length; ++n) Assert.True(SkillTree.Choose(D, p, n, 1)); // v0.21.52 cz. c: drzewko
        Assert.True(Meta.ShopSpent(D, p) == Meta.ShopTotalCost(D) && p.Xp == 100000 - Meta.ShopTotalCost(D));
        Assert.True(Meta.NextUnlock(D, p, out _, out _) < 0);
    }

    [Fact]
    public void BadgesAndContractsGiveTitlesAndHelmets()
    {
        Assert.All(D.Badges, b => Assert.True(b.Xp <= 15 && b.Title.Length > 0));
        Assert.All(D.Contracts, c => Assert.True(c.Xp <= 15 && c.Title.Length > 0));
        var helmets = Enumerable.Range(0, D.Cosmetics.Length).Count(k => Secrets.CosmeticHelmet(D, k));
        var fromProgress = D.InspectorLevels.Concat(D.StakeRanks).Concat(D.MasteryLevels).Concat(D.TaskRewards).Concat(D.StreakRewards)
            .Concat(D.Collections.Select(c => c.Reward)).Count(l => l.Reward == ProgressReward.Helmet)
            + D.Career.Count(c => c.Helmet >= 0); // cz. b, cz. c, cz. d (mapa kariery)
        Assert.True(helmets >= 3 && helmets == D.Badges.Count(b => b.Cosmetic >= 0) + D.Contracts.Count(c => c.Cosmetic >= 0) + fromProgress);
        Assert.False(Secrets.CosmeticHelmet(D, D.CosmeticGold));

        var p = Meta.NewProfile(D);
        Assert.Equal(-1, Titles.Selected(D, p));
        Titles.Cycle(D, p, 1);
        Assert.Equal(0, p.Title);
        var seryjny = Array.FindIndex(D.Badges, b => b.Id == "seryjny");
        p.Badges = (ushort)(1 << seryjny);
        p.Contracts = 1 << 1;
        Titles.Cycle(D, p, 1);
        Assert.True(Titles.Selected(D, p) == seryjny && Titles.Name(D, seryjny) == D.Badges[seryjny].Title);
        Titles.Cycle(D, p, 1);
        Assert.True(Titles.Selected(D, p) == D.Badges.Length + 1 && Titles.Name(D, D.Badges.Length + 1) == D.Contracts[1].Title);
        Titles.Cycle(D, p, 1);
        Assert.True(p.Title == 0 && Titles.Selected(D, p) == -1);
        Titles.Cycle(D, p, -1);
        Assert.Equal(D.Badges.Length + 1, Titles.Selected(D, p));
        p.Contracts = 0;
        Assert.True(Titles.Selected(D, p) == -1 && Titles.OwnedCount(D, p) == 1);

        var bi = Array.FindIndex(D.Badges, b => b.Cosmetic >= 0);
        var k = D.Badges[bi].Cosmetic;
        var q = Meta.NewProfile(D);
        Assert.True(!Secrets.CosmeticUnlocked(D, q, k) && Secrets.HelmetCosmetic(D, q) == -1);
        q.Helmet = (byte)(k + 1);
        Assert.Equal(-1, Secrets.HelmetCosmetic(D, q));
        q.Helmet = 0;
        q.Badges = (ushort)(1 << bi);
        Assert.True(Secrets.CosmeticUnlocked(D, q, k) && Secrets.HelmetsUnlocked(D, q) == 1);
        Secrets.CycleHelmet(D, q, 1);
        Assert.Equal(k, Secrets.HelmetCosmetic(D, q));
        Secrets.CycleHelmet(D, q, 1);
        Assert.True(Secrets.HelmetCosmetic(D, q) == -1 && q.Helmet == 0);
        Secrets.ToggleCosmetic(D, q, k);
        Assert.False(Secrets.CosmeticOn(D, q, k));
    }

    [Fact]
    public void ProfileV12MigratesWithRefundAtOldPrices()
    {
        var v = Meta.NewProfile(D);
        v.Xp = 33;
        v.Best = 4444;
        v.Wins = 7;
        v.Classes = 0x3F;
        v.Tools = 0x0E;
        v.Hard = 1;
        v.Brigade = 3;
        v.Secrets = 0x15;
        v.Respect = 77;
        v.RespectRanksHi[0] = 1;
        var want = 33;
        for (var i = 0; i < D.Upgrades.Length; ++i)
        {
            v.Levels[i] = (byte)D.Upgrades[i].LegacyCosts.Length;
            want += D.Upgrades[i].LegacyCosts.Sum();
        }
        v.Magic = Profile.MagicBytes(Profile.MagicV12);
        var raw = v.ToBytes();
        for (var i = Profile.V12Size; i < raw.Length; i++) raw[i] = 0xAB;
        var p = Profile.FromBytes(raw);
        Assert.True(Meta.ProfileFix(D, p) && p.MagicIs(Profile.MagicCurrent));
        Assert.True(p.Xp == want && want > 33 && p.Best == 4444 && p.Wins == 7 && p.Classes == 0x3F && p.Tools == 0x0E && p.Hard == 1);
        Assert.True(p.Brigade == 3 && p.Secrets == 0x15 && p.Respect == 77 + TestData.InspRespect(TestData.InspMigrated(0, 7, 0)) && p.RespectRanksHi[0] == 1 && p.Title == 0 && p.Helmet == 0);
        Assert.All(p.Levels, l => Assert.Equal(0, l));
        Assert.False(Meta.ProfileFix(D, p));
        var o = Meta.NewProfile(D);
        o.Levels[0] = (byte)(D.Upgrades[0].LegacyCosts.Length + 1);
        o.Magic = Profile.MagicBytes(Profile.MagicV12);
        Assert.True(Meta.ProfileFix(D, o) && o.Xp == D.Upgrades[0].LegacyCosts.Sum() + D.Upgrades[0].Refund && o.Levels[0] == 0);
        var e = Meta.NewProfile(D);
        e.Levels[1] = 1;
        e.Xp = 5;
        e.Magic = Profile.MagicBytes(Profile.MagicV11);
        Assert.True(Meta.ProfileFix(D, e) && e.Xp == 5 + D.Upgrades[1].LegacyCosts[0] && e.Levels[1] == 0);
        var n = Meta.NewProfile(D);
        n.Title = 3;
        n.Helmet = 2;
        var back = Profile.FromBytes(n.ToBytes());
        Assert.True(!Meta.ProfileFix(D, back) && back.Title == 3 && back.Helmet == 2);
    }
}
