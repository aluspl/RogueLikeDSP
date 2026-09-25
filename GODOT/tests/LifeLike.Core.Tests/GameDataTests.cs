namespace LifeLike.Core.Tests;

/// <summary>Wczytywanie game.json – te same wartości co w wygenerowanym GBA/include/game_data.h (migawka).</summary>
public class GameDataTests
{
    private static GameData D => TestData.D;

    [Fact]
    public void DerivedConstantsMatchGeneratedHeader()
    {
        Assert.Equal(6, D.Classes.Length);
        Assert.Equal(12, D.Weapons.Length);
        Assert.Equal(11, D.Enemies.Length);
        Assert.Equal(8, D.Stages.Length);
        Assert.Equal(1, D.StartToolsMask);
        Assert.Equal(19, D.StartClassesMask);
        Assert.Equal(32, D.DmgLevelsMask);
        Assert.Equal(8, D.DefLevelsMask);
        Assert.Equal(5, D.MaxHeroLevel);
        Assert.Equal([40, 5, 5, 15, 35], D.DropWeights);
        Assert.Equal(new[] { 0, 3 }, D.Stages[0].Pool);
        Assert.Equal(9, D.Stages[2].Boss);
        Assert.Equal(-1, D.Stages[0].Boss);
        Assert.Equal(1, D.Classes[1].Weapon);
        Assert.Equal(AbilityEffect.Flush, D.Classes[4].Ability);
        Assert.Equal(StatusEffect.Paper, D.Enemies[8].OnHit);
        Assert.Equal(StatusEffect.None, D.Enemies[3].OnHit);
        Assert.Equal(9, D.Gear.Length);
        Assert.Equal(GearStat.Hp, D.Gear[7].Stat);
        Assert.Equal(ShopEffect.MaxHp, D.Hurtownia[3].Effect);
        Assert.Equal(0, D.BadgeBezUsterek);
        Assert.Equal(8, D.BadgeOsiedle);
        Assert.Equal("Anna Nowak", D.StoryStages[0].From);
        Assert.Equal(3, D.StoryStages[0].Lines.Length);
    }

    [Fact]
    public void UnknownFieldsAreIgnored()
    {
        var json = File.ReadAllText(TestData.GoldenPath("game.json"));
        var extended = json.Replace("\"maxHealth\": 30,", "\"maxHealth\": 30, \"luck\": 3, \"traits\": {\"a\": [1, 2]},")
                           .Replace("\"version\":", "\"termos\": {\"heal\": 8}, \"version\":");
        Assert.NotEqual(json, extended);
        var d = GameData.Parse(extended);
        Assert.Equal(D.Classes.Length, d.Classes.Length);
        Assert.Equal(30, d.Classes[0].MaxHealth);
    }

    [Fact]
    public void V42SectionsAreParsed()
    {
        Assert.Equal(new[] { 2, 0, 2, 1, 2, 4 }, D.Classes.Select(c => c.Luck).ToArray());
        Assert.Equal(8, D.GearTraitsCount);
        Assert.Equal(TraitEffect.PoisonRes, D.GearTraits[2].Effect);
        Assert.Equal("Zatrucie", D.Statuses[(int)StatusEffect.Poison].Name);
        Assert.Equal("-1 HP/turę", D.Statuses[(int)StatusEffect.Poison].Effect);
        Assert.Equal(3, D.PaperDelay);
        Assert.True(D.CritBasePct == 5 && D.CritPerLuckPct == 3 && D.CritMultiplier == 2 && D.DodgeMaxPct == 20);
        Assert.True(D.ThermosCapacity == 3 && D.CoffeeHeal == 8 && D.BotDrinkBelowPct == 40 && D.GearDeclineXp == 1);
        Assert.Equal(108, D.Stages[0].HpPct);
        Assert.Equal("v0.21.43", D.Version);
    }

    [Fact]
    public void V43SectionsAreParsed()
    {
        Assert.Equal(TraitEffect.Str, D.GearTraits[5].Effect);
        Assert.Equal(TraitEffect.Intel, D.GearTraits[7].Effect);
        Assert.Equal(7, D.Upgrades.Length);
        Assert.Equal(UpgradeEffect.Luck, D.Upgrades[5].Effect);
        Assert.Equal(UpgradeEffect.Craft, D.Upgrades[6].Effect);
        Assert.Equal(6, D.Tools.Length);
        Assert.Equal(new Perk(PerkEffect.Hp, 2), D.Badges[D.BadgeBezUsterek].Bonus);
        Assert.Equal(new Perk(PerkEffect.ToolPct, 10), D.Badges[D.BadgeKolekcjoner].Bonus);
        Assert.Equal(new Perk(PerkEffect.XpPct, 10), D.Badges[D.BadgeOsiedle].Bonus);
        Assert.Equal(5, D.Keepsakes.Length);
        Assert.Equal(new[] { 3, 8 }, D.KeepsakeRankRuns);
        Assert.True(D.Keepsakes[0].Start && D.Keepsakes[0].Effect == PerkEffect.Thermos && D.Keepsakes[0].Badge == -1);
        Assert.Equal(0, D.Keepsakes[1].Badge);
        Assert.Equal(new[] { 2, 3, 4 }, D.Keepsakes[2].Values);
        Assert.Equal(6, D.Contracts.Length);
        Assert.True(D.Contracts[1].Kind == ContractKind.CleanBoss && D.Contracts[1].Keepsake == 3 && D.Contracts[0].Keepsake == -1);
        Assert.True(D.Contracts[3].Kind == ContractKind.Powers && D.Contracts[3].Target == 100 && D.Contracts[3].Xp == 40);
        Assert.Equal(5, D.SiteEvents.Length);
        Assert.Equal(45, D.SiteEventChancePct);
        Assert.True(D.SiteEvents[0].Effect == EventEffect.FewerPickups && D.SiteEvents[0].Value == 2 && !D.SiteEvents[0].Good);
        Assert.Equal("Kierownik Marek", D.SiteEvents[0].Msg.From);
        Assert.Equal("Mniej materiału na placu.", D.SiteEvents[0].Msg.Lines[2]);
        Assert.Equal("-2 znajdź.", D.SiteEvents[0].Short);
    }

    /// <summary>Dane sprzed v0.21.43 (bez odznak z uprawnieniami, pamiątek, zleceń i wydarzeń) nadal się wczytują.</summary>
    [Fact]
    public void DataWithoutV43SectionsLoads()
    {
        using var doc = System.Text.Json.JsonDocument.Parse(File.ReadAllText(TestData.GoldenPath("game.json")));
        var root = System.Text.Json.Nodes.JsonNode.Parse(doc.RootElement.GetRawText())!.AsObject();
        root.Remove("contracts");
        root.Remove("keepsakes");
        root.Remove("siteEvents");
        foreach (var b in root["badges"]!.AsArray()) b!.AsObject().Remove("perk");
        var d = GameData.Parse(root.ToJsonString());
        Assert.True(d.Keepsakes.Length == 0 && d.Contracts.Length == 0 && d.SiteEvents.Length == 0 && d.SiteEventChancePct == 0);
        Assert.Equal(PerkEffect.Unknown, d.Badges[0].Bonus.Effect);
        var g = new Game(d);
        g.NewRun(0, 5);
        g.NextStage();
        Assert.Equal(-1, g.StageEvent);
        var p = Meta.NewProfile(d);
        p.Badges = 0x1FF;
        p.Keepsake = 1;
        Assert.Equal(0, Meta.Mods(d, p).Hp);
        Assert.Equal(-1, Meta.NextContract(d, p, g));
    }

    /// <summary>Nowsze dane GBA mogą mieć efekty, których port jeszcze nie zna – wczytują się jako Unknown (bez działania).</summary>
    [Fact]
    public void UnknownEffectsLoadAsInert()
    {
        var json = File.ReadAllText(TestData.GoldenPath("game.json"))
            .Replace("\"effect\": \"cooldown\"", "\"effect\": \"nowa_cecha\"")
            .Replace("\"effect\": \"pickups\"", "\"effect\": \"nowe_szkolenie\"")
            .Replace("\"effect\": \"xp_pct\"", "\"effect\": \"nowe_uprawnienie\"");
        var d = GameData.Parse(json);
        Assert.Equal(TraitEffect.Unknown, d.GearTraits[4].Effect);
        Assert.Contains(d.Upgrades, u => u.Effect == UpgradeEffect.Unknown);
        Assert.Equal(PerkEffect.Unknown, d.Badges[d.BadgeOsiedle].Bonus.Effect);
        Assert.Equal(PerkEffect.Unknown, d.Keepsakes[4].Effect); // Notes kierownika: cooldown -> nieznany
    }

    [Fact]
    public void BrokenReferenceIsReported()
    {
        var json = File.ReadAllText(TestData.GoldenPath("game.json")).Replace("\"weapon\": \"kielnia\"", "\"weapon\": \"nie_ma\"");
        Assert.Throws<GameDataException>(() => GameData.Parse(json));
    }

    /// <summary>Wspólny plik z GBA (rozwijany równolegle) musi się dać wczytać – gra w Godot czyta właśnie jego.</summary>
    [Fact]
    public void SharedGbaGameJsonLoadsWhenPresent()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "GBA", "data", "game.json"))) dir = dir.Parent;
        if (dir is null) return; // poza repozytorium (np. sam katalog GODOT) – nie ma czego sprawdzać
        var d = GameData.LoadFile(Path.Combine(dir.FullName, "GBA", "data", "game.json"));
        Assert.NotEmpty(d.Classes);
        var g = new Game(d);
        g.NewRun(0, 1);
        Assert.Equal(GameStatus.Playing, g.St);
    }
}
