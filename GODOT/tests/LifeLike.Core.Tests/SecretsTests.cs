namespace LifeLike.Core.Tests;

/// <summary>core_tests.cpp: 52 (v0.21.51 cz. 2) – sekretne zlecenia #39, migracja v11 -> v12, nowe zawody i narzędzia.</summary>
public class SecretsTests
{
    private static GameData D => TestData.D;

    private static int SecretOf(SecretKind k) => Array.FindIndex(D.Secrets, s => s.Kind == k);

    private static int ClassOf(AbilityEffect e) => Array.FindIndex(D.Classes, c => c.Ability == e);

    private static int ToolOf(bool knockback) =>
        Enumerable.Range(0, D.Tools.Length).First(t => D.Tools[t].Secret && D.Weapons[D.Tools[t].Weapon].Knockback == knockback);

    [Fact]
    public void SecretRewardsLockedUntilDone()
    {
        var p = Meta.NewProfile(D);
        p.Xp = 1 << 20;
        p.Respect = 60000;
        for (var c = D.OpenClassesCount; c < D.Classes.Length; ++c)
        {
            Assert.True(!Meta.ClassUnlocked(D, p, c) && !Meta.BuyClass(D, p, c) && Meta.ClassReward(D, c) && Meta.ClassSecret(D, c));
        }
        var zenka = ToolOf(true);
        Assert.True(!Meta.ToolUnlocked(D, p, zenka) && !Meta.BuyTool(D, p, zenka));
        var vet = Array.FindIndex(D.Respect, r => r.Effect == RespectEffect.Veteran);
        Assert.True(vet >= 16 && !Meta.RespectUnlocked(D, p, vet) && !Meta.BuyRespect(D, p, vet));
        for (var day = 1; day <= 200; ++day) Assert.True(Daily.ClassOf(D, Daily.Seed(day)) < D.OpenClassesCount);
        Assert.Equal(8, D.Secrets.Length);
    }

    [Fact]
    public void RunConditionsAndRewards()
    {
        var p = Meta.NewProfile(D);
        var g = TestData.Run(1, 11);
        g.St = GameStatus.Won;
        g.Hero.Hp = 20;
        for (var s = 0; s < Game.MaxStages; ++s) g.StageDays[s] = 900;
        var noCoffee = SecretOf(SecretKind.NoCoffeeWin);
        Assert.True(Secrets.Condition(D, p, g, noCoffee));
        g.CoffeeDrunk = 1;
        Assert.False(Secrets.Condition(D, p, g, noCoffee));
        g.CoffeeDrunk = 0;
        Assert.Equal(1 << noCoffee, Secrets.Check(D, p, g));
        Assert.True(Meta.ClassUnlocked(D, p, ClassOf(AbilityEffect.Weld)));
        Assert.Equal(0, Secrets.Check(D, p, g)); // raz
        var low = SecretOf(SecretKind.LowHpWin);
        g.Hero.Hp = 3;
        Assert.True(Secrets.Condition(D, p, g, low));
        var fast = SecretOf(SecretKind.FastWin);
        Assert.False(Secrets.Condition(D, p, g, fast));
        Array.Clear(g.StageDays);
        Assert.True(Secrets.Condition(D, p, g, fast));
        var stores = SecretOf(SecretKind.Storerooms);
        g.SecretsFound = (byte)D.Secrets[stores].Value;
        Assert.True(Secrets.Condition(D, p, g, stores));
        // Mokra robota: Elektryk bije mokry problem
        var e = TestData.Arena(3);
        e.Spawn(D.EnemyIndex("przeciek"), 8, 7);
        e.Enemies[0].Hp = e.Enemies[0].MaxHp = 500;
        var shock = SecretOf(SecretKind.ShockCombos);
        for (var k = 0; k < D.Secrets[shock].Value; ++k) e.HeroAttack(0);
        Assert.True(e.ShockCombos == D.Secrets[shock].Value && Secrets.Condition(D, p, e, shock));
        var q = Meta.NewProfile(D);
        Secrets.Check(D, q, e);
        Assert.True(Secrets.CosmeticOn(D, q, D.CosmeticGold) && !Secrets.CosmeticOn(D, q, D.CosmeticStripes));
    }

    [Fact]
    public void HelperKillsFinalBossAndPaperCleanAct0()
    {
        var m = RunMods.Default(D);
        m.Helpers = (1 << D.Brigade.Length) - 1;
        var g = TestData.NewGame();
        g.NewRun(0, 77, D.DefaultDifficulty, m);
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
        g.Stage = D.StagesCount - 1;
        g.UpdateFov();
        g.Spawn(D.SecretHelperBoss, 8, 7);
        g.Boss = 0;
        g.Enemies[0].Hp = 2;
        g.Cash = 500;
        var pump = Array.FindIndex(D.Brigade, h => h.Effect == HelperEffect.Pump);
        Assert.True(g.CallHelper(pump) && g.St == GameStatus.Won && (g.SecretFlags & Game.SecretHelperBossFlag) != 0);
        var p = Meta.NewProfile(D);
        Assert.True(((Secrets.Check(D, p, g) >> SecretOf(SecretKind.HelperBoss)) & 1) != 0 && Meta.ToolUnlocked(D, p, ToolOf(true)));

        var a = RunMods.Default(D);
        a.Act0 = 1;
        var h0 = TestData.NewGame();
        h0.NewRun(1, 21, D.DefaultDifficulty, a);
        h0.DebugSkip();
        h0.NextStage();
        var dirty = TestData.NewGame();
        dirty.CopyFrom(h0);
        h0.DebugSkip();
        Assert.True((h0.SecretFlags & Game.SecretPaperCleanFlag) != 0);
        dirty.LogHit(D.EnemyIndex("podpis"), -1, RecapKind.Melee, 3);
        dirty.DebugSkip();
        Assert.True(dirty.PaperHits == 1 && (dirty.SecretFlags & Game.SecretPaperCleanFlag) == 0);
    }

    [Fact]
    public void MigrationV11ToV12FromClassWins()
    {
        var i = SecretOf(SecretKind.ClassWins);
        var p = Meta.NewProfile(D);
        for (var c = 0; c < D.OpenClassesCount; ++c) Meta.SetClassWon(p, c);
        p.Tutorial = 0xFFFF;
        p.Best = 4321;
        var b = p.ToBytes();
        Array.Fill(b, (byte)0xCD, Profile.V11Size, Profile.Size - Profile.V11Size);
        var v11 = Profile.FromBytes(b);
        v11.Magic = Profile.MagicBytes(Profile.MagicV11);
        Assert.True(Meta.ProfileFix(D, v11) && v11.MagicIs(Profile.MagicCurrent) && v11.Best == 4321);
        Assert.True(v11.Secrets == 1 << i && v11.SecretsNew == 1 << i && v11.Cosmetic == 0 && v11.RespectRanksHi[0] == 0);
        Assert.True(Meta.ClassUnlocked(D, v11, ClassOf(AbilityEffect.Borrow)));
        Assert.True(Meta.PendingUnlock(D, v11, 0, out var cls) == TutorialUnlock.Secret && cls == i);
        Meta.MarkUnlock(v11, TutorialUnlock.Secret, cls);
        Assert.True(v11.SecretsNew == 0 && Meta.PendingUnlock(D, v11, 1, out cls) == TutorialUnlock.Class && cls == ClassOf(AbilityEffect.Borrow));
    }

    [Fact]
    public void NewClassesAndTools()
    {
        var weld = TestData.Arena(ClassOf(AbilityEffect.Weld));
        foreach (var x in new[] { 9, 10, 11 }) weld.Spawn(D.EnemyIndex("kornik"), x, 7);
        for (var k = 0; k < 3; ++k)
        {
            weld.Enemies[k].Hp = weld.Enemies[k].MaxHp = 500;
            weld.Enemies[k].Stun = 9;
        }
        Assert.True(weld.PlayerAbility() && weld.EnemyDusty(0) && weld.EnemyDusty(1) && !weld.EnemyDusty(2));
        weld.ComboEvents = 0;
        weld.HeroAttack(0);
        Assert.True((weld.ComboEvents & 2) != 0); // iskra w dymie: wybuch pyłu
        var mark = TestData.Arena(ClassOf(AbilityEffect.Mark));
        mark.Spawn(D.EnemyIndex("kornik"), 8, 7);
        mark.Enemies[0].Hp = mark.Enemies[0].MaxHp = 500;
        Assert.True(mark.PlayerAbility() && mark.MarkTarget == 0 && mark.MarkBonus(0) == 1 + mark.AbilityRank());
        var geo = TestData.Run(ClassOf(AbilityEffect.Mark), 31);
        var plain = TestData.Run(0, 31);
        Assert.True(geo.Fov.Count(f => f != Sight.Unknown) > plain.Fov.Count(f => f != Sight.Unknown));
        var seen = 0;
        for (uint seed = 1; seed <= 40; ++seed)
        {
            var mj = TestData.Run(ClassOf(AbilityEffect.Borrow), seed);
            Assert.True(mj.BorrowCls >= 0 && mj.BorrowCls < D.OpenClassesCount && mj.PowerCls() == mj.BorrowCls);
            seen |= 1 << mj.BorrowCls;
        }
        Assert.Equal((1 << D.OpenClassesCount) - 1, seen);
        var zenka = TestData.Arena(1);
        zenka.TakeTool(ToolOf(true));
        zenka.Spawn(D.EnemyIndex("kornik"), 8, 7);
        zenka.Enemies[0].Hp = zenka.Enemies[0].MaxHp = 500;
        zenka.HeroAttack(0);
        Assert.Equal(9, zenka.Enemies[0].X);
        var poz = TestData.Run(1, 5);
        var c0 = poz.CritPct();
        poz.TakeTool(ToolOf(false));
        Assert.True(poz.CritPct() == c0 + poz.Weapon.Crit && poz.Weapon.Reveal && poz.WeaponBreakdown().CritPct == poz.CritPct());
    }
}
