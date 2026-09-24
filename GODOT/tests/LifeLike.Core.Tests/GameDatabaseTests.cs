using LifeLike.Core.Data;

namespace LifeLike.Core.Tests;

public class GameDatabaseTests
{
    [Fact]
    public void ShippedGameData_LoadsWithoutErrors()
    {
        var db = GameDatabase.LoadFromDirectory(Paths.GameData);
        Assert.Contains("warrior", db.Classes.Keys);
        Assert.Contains("rusty_sword", db.Weapons.Keys);
        Assert.All(db.Classes.Values, c =>
        {
            if (c.StartingWeapon is { } w) Assert.True(db.Weapons.ContainsKey(w));
        });
    }

    [Fact]
    public void Load_ParsesEnumsCommentsAndTrailingCommas()
    {
        var db = GameDatabase.Load(new InMemorySource(
            ("weapons/bow.json", """
             { // komentarz
               "id": "bow", "name": "Łuk", "minDamage": 1, "maxDamage": 3, "range": 4, "scalesWith": "agility",
             }
             """)));
        var bow = db.Weapons["bow"];
        Assert.Equal(StatType.Agility, bow.ScalesWith);
        Assert.Equal(4, bow.Range);
    }

    [Fact]
    public void Load_ReportsAllErrorsAtOnce()
    {
        var ex = Assert.Throws<DataLoadException>(() => GameDatabase.Load(new InMemorySource(
            ("weapons/bad.json", """{ "id": "bad", "name": "Zła", "minDamage": 5, "maxDamage": 1 }"""),
            ("weapons/broken.json", "{ nie json"),
            ("classes/x.json", """{ "id": "x", "name": "X", "startingWeapon": "missing" }"""))));

        Assert.Contains("maxDamage", ex.Message);
        Assert.Contains("broken.json", ex.Message);
        Assert.Contains("'missing'", ex.Message);
    }

    [Fact]
    public void Load_DuplicateIdInSameSource_IsError()
    {
        var w = """{ "id": "a", "name": "A" }""";
        Assert.Throws<DataLoadException>(() =>
            GameDatabase.Load(new InMemorySource(("weapons/a.json", w), ("weapons/b.json", w))));
    }

    [Fact]
    public void Load_LaterSourceOverridesById_ForMods()
    {
        var baseData = new InMemorySource(("weapons/a.json", """{ "id": "a", "name": "Bazowa", "maxDamage": 2 }"""));
        var mod = new InMemorySource(("weapons/a.json", """{ "id": "a", "name": "Mod", "maxDamage": 9 }"""));
        var db = GameDatabase.Load(baseData, mod);
        Assert.Equal("Mod", db.Weapons["a"].Name);
        Assert.Equal(9, db.Weapons["a"].MaxDamage);
    }

    [Fact]
    public void SaveThenLoad_RoundTrips()
    {
        var root = Directory.CreateTempSubdirectory().FullName;
        var weapon = new WeaponData { Id = "axe", Name = "Topór", MinDamage = 2, MaxDamage = 5, ScalesWith = StatType.Strength };
        var cls = new CharacterClassData { Id = "barb", Name = "Barbarzyńca", BaseStats = new Stats { MaxHealth = 25, Strength = 4 }, StartingWeapon = "axe" };

        GameDatabase.Save(weapon, root);
        GameDatabase.Save(cls, root);
        var db = GameDatabase.LoadFromDirectory(root);

        Assert.Equal(weapon, db.Weapons["axe"]);
        Assert.Equal(cls, db.Classes["barb"]);
        Assert.True(File.Exists(Path.Combine(root, "classes", "barb.json")));
    }
}
