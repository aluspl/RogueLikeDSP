namespace LifeLike.Core.Tests;

/// <summary>
/// core_tests.cpp (v0.21.52 cz. b): poziom inspektora (#44), mistrzostwo zawodu (#45), stopnie inwestora (#48), profil v14.
/// </summary>
public class InspectorMasteryTests
{
    private static GameData D => TestData.D;

    [Fact]
    public void DataHasEscalatingLevelsAndEveryReward()
    {
        Assert.InRange(D.InspectorLevels.Length, 30, 40);
        Assert.Equal(10, D.MasteryLevels.Length);
        for (var l = 1; l < D.InspectorLevels.Length; ++l) Assert.True(D.InspectorLevels[l].Xp >= D.InspectorLevels[l - 1].Xp);
        foreach (var r in new[] { ProgressReward.Respect, ProgressReward.Title, ProgressReward.Helmet, ProgressReward.Story, ProgressReward.Decor })
            Assert.Contains(D.InspectorLevels, l => l.Reward == r);
        Assert.Single(D.InspectorLevels, l => l.Reward == ProgressReward.KeepsakeSlot);
        Assert.Equal(3, Progress.MasteryRewardLevel(D, ProgressReward.Power));
        Assert.Equal(5, Progress.MasteryRewardLevel(D, ProgressReward.Weapon));
        Assert.Equal(7, Progress.MasteryRewardLevel(D, ProgressReward.Boon));
        Assert.Equal(10, Progress.MasteryRewardLevel(D, ProgressReward.Helmet));
        Assert.Equal(Investor.Stake(D, (1 << D.Investor.Length) - 1), D.StakeRanks.Length);
    }

    [Fact]
    public void LevelsAndBars()
    {
        var p = Meta.NewProfile(D);
        Progress.InspectorBar(D, p, out var cur, out var need);
        Assert.True(Progress.InspectorLevel(D, p) == 0 && cur == 0 && need == D.InspectorLevels[0].Xp);
        p.InspectorXp = (uint)(D.InspectorLevels[0].Xp + 3);
        Progress.InspectorBar(D, p, out cur, out need);
        Assert.True(Progress.InspectorLevel(D, p) == 1 && cur == 3 && need == D.InspectorLevels[1].Xp);
        p.InspectorXp = (uint)Progress.Floor(D.InspectorLevels, D.InspectorLevels.Length);
        Progress.InspectorBar(D, p, out cur, out need);
        Assert.True(Progress.InspectorLevel(D, p) == D.InspectorLevels.Length && cur == 0 && need == 0);
        p.MasteryXp[2] = (ushort)Progress.Floor(D.MasteryLevels, 3);
        Progress.MasteryBar(D, p, 2, out cur, out need);
        Assert.True(Progress.MasteryLevel(D, p, 2) == 3 && Progress.MasteryLevel(D, p, 1) == 0 && cur == 0 && need == D.MasteryLevels[3].Xp);
    }

    [Fact]
    public void RunGivesProgressOnceWithWatermark()
    {
        var p = Meta.NewProfile(D);
        var g = TestData.NewGame();
        g.NewRun(1, 77, 1, Meta.Mods(D, p, 1));
        Meta.StartRun(D, p);
        var f0 = TestData.F0;
        g.Stage = f0 + 4;
        g.St = GameStatus.Dead;
        g.ElitesKilled = 2;
        g.SecretsFound = 1;
        var bosses = Enumerable.Range(f0, 4).Count(s => D.Stages[s].Boss >= 0);
        var want = (D.InspectorXpRun + 4 * D.InspectorXpStage + bosses * D.InspectorXpBoss + 2 * D.InspectorXpElite + D.InspectorXpStoreroom)
                   * D.InspectorDiffPct[1] / 100;
        Assert.Equal(want, Progress.RunProgressXp(D, g));
        var r = Progress.Bank(D, p, g);
        Assert.True(r.Gained == want && p.InspectorXp == want && p.MasteryXp[1] == want && p.MasteryXp[0] == 0 && r.Cls == 1);
        Assert.Equal(0, Progress.Bank(D, p, g).Gained);
        // wygrana i NG+: druga część tylko nowa
        var q = Meta.NewProfile(D);
        var w = TestData.NewGame();
        w.NewRun(0, 5, 0, Meta.Mods(D, q, 0));
        Meta.StartRun(D, q);
        w.St = GameStatus.Won;
        w.Stage = D.Stages.Length - 1;
        var all = Progress.RunProgressXp(D, w);
        Assert.Equal(all, Progress.Bank(D, q, w).Gained);
        w.Tier = 1;
        w.St = GameStatus.Dead;
        w.Stage = f0 + 1;
        var ng = Progress.Bank(D, q, w).Gained;
        Assert.True(ng == Progress.RunProgressXp(D, w) - all && ng > 0 && ng < all);
        Meta.StartRun(D, q);
        Assert.Equal(0, q.RunProgress);
    }

    [Fact]
    public void InspectorRewardsEveryLevel()
    {
        var p = Meta.NewProfile(D);
        var respect = 0;
        for (var l = 0; l < D.InspectorLevels.Length; ++l)
        {
            var lv = D.InspectorLevels[l];
            if (lv.Reward == ProgressReward.Respect) respect += lv.Value;
            var r = new ProgressGain();
            var r0 = p.Respect;
            Progress.AddInspectorXp(D, p, lv.Xp, r);
            Assert.True(r.InspBefore == l && r.InspAfter == l + 1);
            Assert.Equal(lv.Reward == ProgressReward.Respect ? lv.Value : 0, p.Respect - r0);
            if (lv.Reward == ProgressReward.Title) Assert.True(Titles.Owned(D, p, Titles.ProgressIndex(D, 0, l + 1)));
            if (lv.Reward == ProgressReward.Helmet) Assert.True(Secrets.CosmeticUnlocked(D, p, lv.Index));
            if (lv.Reward == ProgressReward.Decor) Assert.True(Story.DecorUnlocked(D, p, lv.Index));
            Assert.Equal(l + 1 >= Progress.InspectorRewardLevel(D, ProgressReward.KeepsakeSlot), Progress.KeepsakeSlot2(D, p));
        }
        Assert.True(p.Respect == respect && p.RespectTotal == respect);
        var got = Story.Check(D, p, null);
        Assert.Equal(D.InspectorLevels.Count(l => l.Reward == ProgressReward.Story),
            Enumerable.Range(0, D.StoryArc.Length).Count(i => ((got >> i) & 1) != 0 && D.StoryArc[i].Trigger == StoryTrigger.Inspector));
    }

    [Fact]
    public void SecondKeepsakeAddsPerkAndRank()
    {
        var p = Meta.NewProfile(D);
        p.InspectorXp = 1000000;
        p.Badges = (ushort)(1 << D.Keepsakes[1].Badge);
        Meta.CycleKeepsake2(D, p, 1);
        Assert.Equal(1, Meta.SelectedKeepsake2(D, p));
        Meta.CycleKeepsake2(D, p, 1);
        Assert.True(Meta.SelectedKeepsake2(D, p) == -1 && p.Keepsake2 == 0);
        Meta.CycleKeepsake2(D, p, 1);
        var one = Profile.FromBytes(p.ToBytes());
        one.Keepsake2 = 0;
        Assert.Equal(Meta.Mods(D, one).TakenPct + D.Keepsakes[1].Values[0], Meta.Mods(D, p).TakenPct); // cz. c: Kask ojca – mniej obrażeń
        p.KeepsakeRuns[1] = 50; // ranga III – druga pamiątka i tak na randze I
        Assert.Equal(Meta.Mods(D, one).TakenPct + D.Keepsakes[1].Values[0], Meta.Mods(D, p).TakenPct);
        p.KeepsakeRuns[1] = 0;
        var kr = p.KeepsakeRuns[1];
        Meta.StartRun(D, p);
        Assert.Equal(kr + 1, p.KeepsakeRuns[1]);
        p.Keepsake = 2;
        Assert.Equal(-1, Meta.SelectedKeepsake2(D, p));
    }

    [Fact]
    public void MasteryVariantsMatchCombat()
    {
        var p = Meta.NewProfile(D);
        for (var l = 0; l < D.MasteryLevels.Length; ++l) Progress.AddMasteryXp(D, p, 1, D.MasteryLevels[l].Xp, new ProgressGain());
        Assert.True(Progress.MasteryLevel(D, p, 1) == 10 && Progress.PowerVariantOn(D, p, 1) && !Progress.PowerVariantOn(D, p, 0));
        Assert.Equal(MasteryBit.Power | MasteryBit.Weapon | MasteryBit.Boon, Progress.MasteryBits(D, p, 1));
        Progress.TogglePowerVariant(D, p, 1);
        Assert.Equal(MasteryBit.Weapon | MasteryBit.Boon, Meta.Mods(D, p, 1).Mastery);
        var mk = D.MasteryLevels.First(l => l.Reward == ProgressReward.Helmet).Index;
        p.Helmet = (byte)(mk + 1);
        Assert.True(Secrets.HelmetCosmetic(D, p, 1) == mk && Secrets.HelmetCosmetic(D, p, 0) == -1);
        for (var c = 0; c < D.Classes.Length; ++c)
        {
            var a = TestData.Arena(c);
            var b = TestData.Arena(c);
            b.Bonus.Mastery = MasteryBit.Power;
            var mc = D.MasteryClasses[c];
            Assert.Equal(a.BoonPower() + mc.Power, b.BoonPower());
            Assert.True(b.AbilityCooldown() == Math.Max(3, a.AbilityCooldown() + mc.Cooldown) || a.AbilityCooldown() + mc.Cooldown < 3);
        }
        {
            var a = TestData.Arena(0);
            var b = TestData.Arena(0);
            b.Bonus.Mastery = MasteryBit.Power;
            a.Spawn(8, 8, 7);
            b.Spawn(8, 8, 7);
            Assert.True(a.PlayerAbility() && b.PlayerAbility());
            Assert.Equal(a.Enemies[0].Stun + D.MasteryClasses[0].Power, b.Enemies[0].Stun);
            Assert.Equal(a.AbilityCd + D.MasteryClasses[0].Cooldown, b.AbilityCd);
        }
        {
            var a = TestData.Arena(1);
            var b = TestData.Arena(1);
            b.Bonus.Mastery = MasteryBit.Power;
            a.Spawn(4, 11, 7);
            b.Spawn(4, 11, 7);
            Assert.True(a.PlayerAbility() && b.PlayerAbility() && a.WallsCount > 0 && a.WallsCount == b.WallsCount);
            Assert.Equal(a.Walls[0].Turns + D.MasteryClasses[1].Power, b.Walls[0].Turns);
        }
        {
            var a = TestData.Arena(4);
            var b = TestData.Arena(4);
            b.Bonus.Mastery = MasteryBit.Power;
            a.Hero.Hp = b.Hero.Hp = 5;
            Assert.True(a.PlayerAbility() && b.PlayerAbility());
            Assert.Equal(a.Hero.Hp + D.MasteryClasses[4].Power, b.Hero.Hp);
            Assert.True(b.AbilityCd == a.AbilityCd + D.MasteryClasses[4].Cooldown && b.AbilityCd < a.AbilityCd);
        }
    }

    [Fact]
    public void MasterWeaponCritOnlyWithClassWeapon()
    {
        for (var c = 0; c < D.Classes.Length; ++c)
        {
            var a = TestData.NewGame();
            a.NewRun(c, 9);
            var m = RunMods.Default(D);
            m.Mastery = MasteryBit.Weapon;
            var b = TestData.NewGame();
            b.NewRun(c, 9, 1, m);
            Assert.True(b.CritPct() == a.CritPct() + D.MasteryClasses[c].WeaponPerk.Value && b.MasterWeapon() && !a.MasterWeapon());
            Assert.Equal(b.CritPct(), b.WeaponBreakdown().CritChance());
            Assert.Equal(b.CritPct(), DmgBreakdown.ForClass(D, c, m).CritChance());
            a.WeaponOverride = D.Tools[0].Weapon;
            b.WeaponOverride = D.Tools[0].Weapon;
            Assert.Equal(a.CritPct(), b.CritPct());
        }
    }

    [Fact]
    public void MasteryBoonOnlyWithBitAndOwnClass()
    {
        int seen = 0, seenOff = 0;
        for (var k = 0; k < 300; ++k)
        {
            var m = RunMods.Default(D);
            m.Mastery = MasteryBit.Boon;
            var g = TestData.NewGame();
            g.NewRun(5, (uint)(100 + k), 1, m);
            g.RollBoons();
            var h = TestData.NewGame();
            h.NewRun(5, (uint)(100 + k), 1);
            h.RollBoons();
            for (var o = 0; o < 3; ++o)
            {
                if (g.BoonOffer[o] == D.MasteryClasses[5].Boon) ++seen;
                if (h.BoonOffer[o] >= 0 && D.Boons[h.BoonOffer[o]].Mastery) ++seenOff;
                if (g.BoonOffer[o] >= 0 && D.Boons[g.BoonOffer[o]].Mastery) Assert.Equal(D.MasteryClasses[5].Boon, g.BoonOffer[o]);
            }
        }
        Assert.True(seen > 0 && seenOff == 0);
    }

    [Fact]
    public void StakeRanksGrantOnce()
    {
        var p = Meta.NewProfile(D);
        p.Wins = 1;
        var want = D.StakeRanks.Take(3).Where(l => l.Reward == ProgressReward.Respect).Sum(l => l.Value);
        var m = RunMods.Default(D);
        for (var i = 0; i < D.Investor.Length && Investor.Stake(D, m.Investor) < 3; ++i)
        {
            if (Investor.Stake(D, m.Investor | (1 << i)) <= 3) m.Investor |= 1 << i;
        }
        Assert.Equal(3, Investor.Stake(D, m.Investor));
        var g = TestData.NewGame();
        g.NewRun(2, 3, 1, m);
        g.St = GameStatus.Won;
        Meta.RecordRun(D, p, g);
        Assert.True(Progress.MaxStake(D, p) == 3 && Progress.StakeRank(D, p) == 3 && p.Respect == want);
        Meta.RecordRun(D, p, g);
        Assert.Equal(want, p.Respect);
        foreach (var sr in D.StakeRanks)
        {
            if (sr.Reward == ProgressReward.Title) Assert.Equal(sr.Xp <= 3, Titles.Owned(D, p, Titles.ProgressIndex(D, 1, sr.Xp)));
            if (sr.Reward == ProgressReward.Helmet) Assert.Equal(sr.Xp <= 3, Secrets.CosmeticUnlocked(D, p, sr.Index));
        }
    }

    [Fact]
    public void ProfileV13MigratesToV14()
    {
        var v = Meta.NewProfile(D);
        v.Runs = 30;
        v.Wins = 6;
        v.RespectTotal = 200;
        v.Respect = 50;
        v.Xp = 77;
        v.Best = 999;
        v.Title = 2;
        v.Helmet = 3;
        v.HousesCount = 6;
        for (var i = 0; i < 6; ++i) v.Houses[i] = (byte)(i < 4 ? 1 : 2);
        Meta.SetClassWon(v, 1);
        Meta.SetClassWon(v, 2);
        v.Magic = Profile.MagicBytes(Profile.MagicV13);
        var raw = v.ToBytes();
        for (var i = Profile.V13Size; i < raw.Length; i++) raw[i] = 0xAB;
        var p = Profile.FromBytes(raw);
        Assert.True(Meta.ProfileFix(D, p) && p.MagicIs(Profile.MagicCurrent));
        var insp = TestData.InspMigrated(30, 6, 200);
        Assert.Equal(insp, (int)p.InspectorXp);
        Assert.Equal(4 * D.MasteryMigrateWin + D.MasteryMigrateClassWin, p.MasteryXp[1]);
        Assert.Equal(2 * D.MasteryMigrateWin + D.MasteryMigrateClassWin, p.MasteryXp[2]);
        Assert.Equal(0, p.MasteryXp[0]);
        var lr = D.InspectorLevels.Take(Progress.InspectorLevel(D, p)).Where(l => l.Reward == ProgressReward.Respect).Sum(l => l.Value);
        for (var c = 1; c <= 2; ++c) lr += D.MasteryLevels.Take(Progress.MasteryLevel(D, p, c)).Where(l => l.Reward == ProgressReward.Respect).Sum(l => l.Value);
        Assert.True(p.Respect == 50 + lr && p.RespectTotal == 200 + lr && p.Xp == 77 && p.Best == 999 && p.Title == 2 && p.Helmet == 3);
        Assert.True(p.Keepsake2 == 0 && p.RunProgress == 0);
        Assert.Equal(Progress.MasteryLevel(D, p, 1) >= 3, Progress.PowerVariantOn(D, p, 1));
        Assert.False(Meta.ProfileFix(D, p));
        var n = Meta.NewProfile(D);
        n.InspectorXp = 1234;
        n.MasteryXp[11] = 55;
        n.Keepsake2 = 3;
        n.PowerAlt = 0x801;
        var back = Profile.FromBytes(n.ToBytes());
        Assert.True(!Meta.ProfileFix(D, back) && back.InspectorXp == 1234 && back.MasteryXp[11] == 55 && back.Keepsake2 == 3 && back.PowerAlt == 0x801);
    }
}
