namespace LifeLike.Core.Tests;

/// <summary>Wczytywanie game.json – te same wartości co w wygenerowanym GBA/include/game_data.h (migawka).</summary>
public class GameDataTests
{
    private static GameData D => TestData.D;

    [Fact]
    public void DerivedConstantsMatchGeneratedHeader()
    {
        Assert.Equal(6, D.Classes.Length);
        Assert.Equal(10, D.Weapons.Length);
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
