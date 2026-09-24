using System.Text.Json;
using System.Text.Json.Serialization;

namespace LifeLike.Core.Data;

public sealed class DataLoadException(string message) : Exception(message);

/// <summary>Źródło plików JSON. Pozwala czytać z dysku (testy, mody) albo z res:// w Godocie (pck).</summary>
public interface IDataSource
{
    /// <summary>Zwraca pary (ścieżka względna, treść JSON) dla plików *.json w podfolderze.</summary>
    IEnumerable<(string Path, string Json)> ReadFolder(string folder);
}

public sealed class DirectoryDataSource(string root) : IDataSource
{
    public IEnumerable<(string, string)> ReadFolder(string folder)
    {
        var dir = System.IO.Path.Combine(root, folder);
        if (!Directory.Exists(dir)) yield break;
        foreach (var f in Directory.EnumerateFiles(dir, "*.json", SearchOption.AllDirectories).Order())
            yield return (System.IO.Path.GetRelativePath(root, f).Replace('\\', '/'), File.ReadAllText(f));
    }
}

/// <summary>
/// Definicje gry w folderach JSON (jeden plik = jedna definicja, liczy się pole "id"):
///   classes/*.json -> CharacterClassData
///   weapons/*.json -> WeaponData
/// Kolejne źródła nadpisują wcześniejsze po id (np. res://data, potem user://mods).
/// </summary>
public sealed class GameDatabase
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public const string ClassesFolder = "classes";
    public const string WeaponsFolder = "weapons";

    public IReadOnlyDictionary<string, CharacterClassData> Classes { get; }
    public IReadOnlyDictionary<string, WeaponData> Weapons { get; }

    private GameDatabase(Dictionary<string, CharacterClassData> c, Dictionary<string, WeaponData> w)
    {
        Classes = c;
        Weapons = w;
    }

    public static GameDatabase LoadFromDirectory(string root) => Load(new DirectoryDataSource(root));

    public static GameDatabase Load(params IDataSource[] sources)
    {
        var errors = new List<string>();
        var weapons = new Dictionary<string, WeaponData>(StringComparer.OrdinalIgnoreCase);
        var classes = new Dictionary<string, CharacterClassData>(StringComparer.OrdinalIgnoreCase);

        foreach (var src in sources)
        {
            LoadFolder(src, WeaponsFolder, weapons, errors);
            LoadFolder(src, ClassesFolder, classes, errors);
        }

        foreach (var c in classes.Values)
            if (c.StartingWeapon is { } w && !weapons.ContainsKey(w))
                errors.Add($"{ClassesFolder}/{c.Id}: startingWeapon '{w}' nie istnieje w {WeaponsFolder}/");

        if (errors.Count > 0)
            throw new DataLoadException("Błędy danych gry:\n - " + string.Join("\n - ", errors));

        return new GameDatabase(classes, weapons);
    }

    private static void LoadFolder<T>(IDataSource src, string folder, Dictionary<string, T> into, List<string> errors)
        where T : class, IGameData
    {
        var seenInSource = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (path, json) in src.ReadFolder(folder))
        {
            T? item;
            try { item = JsonSerializer.Deserialize<T>(json, JsonOptions); }
            catch (JsonException ex) { errors.Add($"{path}: niepoprawny JSON ({ex.Message})"); continue; }

            if (item is null) { errors.Add($"{path}: pusty plik"); continue; }
            if (string.IsNullOrWhiteSpace(item.Id)) { errors.Add($"{path}: brak pola 'id'"); continue; }
            foreach (var e in item.Validate()) errors.Add($"{path}: {e}");
            if (!seenInSource.Add(item.Id)) { errors.Add($"{path}: zduplikowane id '{item.Id}'"); continue; }
            into[item.Id] = item; // późniejsze źródło nadpisuje (mody)
        }
    }

    public static string Serialize<T>(T item) where T : IGameData => JsonSerializer.Serialize(item, JsonOptions);

    public static void Save<T>(T item, string root) where T : IGameData
    {
        var folder = item switch
        {
            CharacterClassData => ClassesFolder,
            WeaponData => WeaponsFolder,
            _ => throw new ArgumentException($"Nieznany typ danych {typeof(T).Name}")
        };
        var dir = System.IO.Path.Combine(root, folder);
        Directory.CreateDirectory(dir);
        File.WriteAllText(System.IO.Path.Combine(dir, item.Id + ".json"), Serialize(item));
    }
}
