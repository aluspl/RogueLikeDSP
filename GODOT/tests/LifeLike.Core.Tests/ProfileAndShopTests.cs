namespace LifeLike.Core.Tests;

// core_tests.cpp: 9 (migracja profilu), 10 (sklep), 11 (bankowanie), 16 (narzędzia w sklepie), 19 (odznaki, Osiedle, katalog).
public class ProfileAndShopTests
{
    private static GameData D => TestData.D;

    [Fact]
    public void ProfileMigrationFromV1GarbageAndV3()
    {
        var raw = Enumerable.Repeat((byte)0xAB, Profile.Size).ToArray();
        var p = Profile.FromBytes(raw);
        p.Magic = Profile.MagicBytes(Profile.MagicV1);
        p.Best = 500;
        p.Runs = 3;
        p.Wins = 1;
        Assert.True(Meta.ProfileFix(D, p));
        Assert.True(p.MagicIs(Profile.MagicV3) && p.Best == 500 && p.Runs == 3 && p.Wins == 1);
        Assert.True(p.Xp == 0 && p.Classes == D.StartClassesMask && p.Hard == 0);
        Assert.All(p.Levels, l => Assert.Equal(0, l));
        var q = Profile.FromBytes(Enumerable.Repeat((byte)0xFF, Profile.Size).ToArray());
        Assert.True(Meta.ProfileFix(D, q) && q.Best == 0 && q.Classes == D.StartClassesMask);
        q.Xp = 7;
        Assert.True(!Meta.ProfileFix(D, q) && q.Xp == 7);
        Assert.False(p.HasFlag(Profile.FlagHelpSeen));
        Assert.False(q.HasFlag(Profile.FlagHelpSeen));
        q.SetFlag(Profile.FlagHelpSeen);
        Assert.True(q.HasFlag(Profile.FlagHelpSeen) && !Meta.ProfileFix(D, q) && q.HasFlag(Profile.FlagHelpSeen));
    }

    [Fact]
    public void ProfileBytesRoundTrip()
    {
        var p = Meta.NewProfile(D);
        p.Best = 1234;
        p.Xp = 55;
        p.Levels[2] = 1;
        p.Badges = 0x101;
        p.HousesCount = 2;
        p.Houses[1] = 0x23;
        var b = p.ToBytes();
        Assert.Equal(Profile.Size, b.Length);
        var q = Profile.FromBytes(b);
        Assert.Equal(b, q.ToBytes());
        Assert.False(Meta.ProfileFix(D, q));
    }

    [Fact]
    public void ShopCostsLevelsClassesHardAndMods()
    {
        var p = Meta.NewProfile(D);
        Assert.True(!Meta.ClassUnlocked(p, 2) && Meta.ClassUnlocked(p, 1));
        Assert.True(!Meta.DifficultyUnlocked(D, p, 2) && Meta.DifficultyUnlocked(D, p, 1));
        Assert.False(Meta.BuyUpgrade(D, p, 0));
        p.Xp = 1000;
        var c0 = Meta.UpgradeCost(D, p, 0);
        Assert.True(c0 == D.Upgrades[0].Costs[0] && Meta.BuyUpgrade(D, p, 0) && p.Levels[0] == 1 && p.Xp == 1000 - c0);
        while (Meta.UpgradeCost(D, p, 0) > 0) Assert.True(Meta.BuyUpgrade(D, p, 0));
        Assert.True(p.Levels[0] == D.Upgrades[0].Levels && !Meta.BuyUpgrade(D, p, 0));
        Assert.True(Meta.BuyClass(D, p, 2) && Meta.ClassUnlocked(p, 2) && !Meta.BuyClass(D, p, 2));
        Assert.True(Meta.BuyHard(D, p) && Meta.DifficultyUnlocked(D, p, 2) && !Meta.BuyHard(D, p));
        var m = Meta.Mods(D, p);
        Assert.True(m.Hp == D.Upgrades[0].Value * D.Upgrades[0].Levels && m.Def == 0);
        Assert.Equal(1000 - p.Xp, Meta.ShopSpent(D, p));
        Assert.Equal(0, Meta.ShopSpent(D, Meta.NewProfile(D)));
        p.Xp = 100000;
        for (var i = 0; i < D.Upgrades.Length; ++i)
            while (Meta.BuyUpgrade(D, p, i))
            {
            }
        for (var i = 0; i < D.Classes.Length; ++i) Meta.BuyClass(D, p, i);
        for (var i = 0; i < D.Tools.Length; ++i) Meta.BuyTool(D, p, i);
        Meta.BuyHard(D, p);
        Assert.Equal(Meta.ShopTotalCost(D), Meta.ShopSpent(D, p));
    }

    [Fact]
    public void BankXpDoesNotDoubleCount()
    {
        var p = Meta.NewProfile(D);
        var g = TestData.Run(1, 7);
        g.DebugSkip();
        var x = g.Xp;
        Assert.True(x > 0);
        Assert.True(Meta.BankXp(p, g) == x && p.Xp == x);
        Assert.True(Meta.BankXp(p, g) == 0 && p.Xp == x);
    }

    [Fact]
    public void ToolsUnlockedInShopJoinDropPool()
    {
        var g = TestData.Run(1, 9);
        var lom = D.Tools[0].Weapon;
        g.Pickups[0] = new Pickup(g.Hero.X, g.Hero.Y, PickupType.Tool, true, 0);
        g.Collect();
        Assert.Same(D.Weapons[lom], g.Weapon);
        var p = Meta.NewProfile(D);
        Assert.True(Meta.ToolUnlocked(D, p, 0) && !Meta.ToolUnlocked(D, p, 3) && !Meta.BuyTool(D, p, 3));
        p.Xp = 100;
        Assert.True(Meta.BuyTool(D, p, 3) && Meta.ToolUnlocked(D, p, 3) && !Meta.BuyTool(D, p, 3) && p.Xp == 100 - D.Tools[3].Cost);
        Assert.Equal(D.StartToolsMask | (1 << 3), Meta.Mods(D, p).Tools);
        var m = RunMods.Default(D);
        m.Tools = (1 << D.Tools.Length) - 1;
        var seen = 0;
        for (uint seed = 1; seed <= 3000 && seen != m.Tools; ++seed)
        {
            var h = TestData.NewGame();
            h.NewRun(1, seed, D.DefaultDifficulty, m);
            h.EnemiesCount = 0;
            h.Spawn(0, h.Hero.X + 1, h.Hero.Y);
            h.Enemies[0].Hp = 1;
            h.PlayerMove(1, 0);
            var q = h.Pickups[h.PickupsCount - 1];
            if (q.Type == PickupType.Tool) seen |= 1 << q.Arg;
        }
        Assert.Equal(m.Tools, seen);
        var old = Meta.NewProfile(D);
        old.Tools = 0;
        Assert.True(Meta.ToolUnlocked(D, old, 0));
    }

    [Fact]
    public void MigrationV2ToV3ZeroesNewFields()
    {
        var v2 = Meta.NewProfile(D);
        v2.Magic = Profile.MagicBytes(Profile.MagicV2);
        v2.Xp = 77;
        v2.Levels[0] = 2;
        v2.Classes = 0x3F;
        v2.Hard = 1;
        v2.Flags = 1;
        v2.Tools = 2;
        v2.Best = 900;
        var raw = v2.ToBytes();
        for (var i = Profile.V2Size; i < raw.Length; i++) raw[i] = 0xEE; // śmieci za starym końcem struktury
        v2 = Profile.FromBytes(raw);
        Assert.True(Meta.ProfileFix(D, v2));
        Assert.True(v2.MagicIs(Profile.MagicV3) && v2.Xp == 77 && v2.Levels[0] == 2 && v2.Classes == 0x3F);
        Assert.True(v2.Hard == 1 && v2.Flags == 1 && v2.Tools == 2 && v2.Best == 900);
        Assert.True(v2.Badges == 0 && v2.Catalog == 0 && v2.HousesCount == 0 && v2.ClassWins == 0 && v2.ToolsFound == 0);
    }

    [Fact]
    public void StageCountersResetOnNextStage()
    {
        var g = TestData.Arena(1);
        Assert.True(g.StageDamage == 0 && g.StageKills == 0 && g.StageStartTurn == 0);
        g.Spawn(8, 8, 7);
        g.Enemies[0].Awake = true;
        int hp = g.Hero.Hp;
        g.PlayerWait();
        Assert.True(g.StageDamage == hp - g.Hero.Hp && g.StageDamage > 0);
        g.Enemies[0].Hp = 1;
        g.PlayerMove(1, 0);
        Assert.Equal(1, g.StageKills);
        g.St = GameStatus.StageClear;
        g.NextStage();
        Assert.True(g.StageDamage == 0 && g.StageKills == 0 && g.StageStartTurn == g.Turns);
    }

    [Fact]
    public void BadgesAwardedOnceAndGiveXp()
    {
        var p = Meta.NewProfile(D);
        var g = TestData.Arena(1);
        g.St = GameStatus.StageClear;
        var xp0 = p.Xp;
        var got = Meta.CheckBadges(D, p, g);
        Assert.True(got == 1 << D.BadgeBezUsterek && p.Xp == xp0 + D.Badges[D.BadgeBezUsterek].Xp);
        Assert.Equal(0, Meta.CheckBadges(D, p, g));
        g.StageDamage = 5;
        g.StageKills = 8;
        Assert.Equal(1 << D.BadgeSeryjny, Meta.CheckBadges(D, p, g));
    }

    [Fact]
    public void HousesHardWinAndFullTeam()
    {
        var p = Meta.NewProfile(D);
        var g = TestData.Run(3, 5, D.Difficulties.Length - 1);
        g.St = GameStatus.Won;
        g.Score = 2500;
        g.Stage = D.Stages.Length - 1;
        g.StageStartTurn = g.Turns - 100;
        g.StageDamage = 1;
        Assert.True(Meta.AddHouse(p, g) && p.HousesCount == 1 && (p.Houses[0] & 15) == 3 && (p.Houses[0] >> 4) == 2);
        var got = Meta.CheckBadges(D, p, g);
        Assert.True((got & (1 << D.BadgeTwardziel)) != 0);
        Assert.True((got & (1 << D.BadgePrzedTerminem)) != 0);
        Assert.True(p.ClassWins == 1 << 3 && (got & (1 << D.BadgePelnyZespol)) == 0);
        for (var i = 0; i < 20; ++i) Meta.AddHouse(p, g);
        Assert.Equal(Profile.MaxHouses, p.HousesCount);
        Assert.True((Meta.CheckBadges(D, p, g) & (1 << D.BadgeOsiedle)) != 0);
        for (var c = 0; c < D.Classes.Length; ++c)
        {
            g.Cls = c;
            Meta.CheckBadges(D, p, g);
        }
        Assert.True((p.Badges & (1 << D.BadgePelnyZespol)) != 0);
    }

    [Fact]
    public void CatalogCollectorAndProBadges()
    {
        var p = Meta.NewProfile(D);
        var g = TestData.Arena(1);
        for (var e = 0; e < D.Enemies.Length; ++e) g.KillsByType[e] = 1;
        g.ToolsFound = (byte)((1 << D.Tools.Length) - 1);
        g.HeroLevel = D.MaxHeroLevel;
        var got = Meta.CheckBadges(D, p, g);
        Assert.True((got & (1 << D.BadgeKatalog)) != 0 && (got & (1 << D.BadgeKolekcjoner)) != 0 && (got & (1 << D.BadgeZawodowiec)) != 0);
        Assert.Equal((1 << D.Enemies.Length) - 1, p.Catalog);
        var h = TestData.Arena(1);
        h.Pickups[0] = new Pickup(h.Hero.X, h.Hero.Y, PickupType.Tool, true, 2);
        h.PickupsCount = 1;
        h.Collect();
        Assert.Equal(1 << 2, h.ToolsFound);
    }
}
