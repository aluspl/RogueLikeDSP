using System.Linq;
using System.Text.Json;
using Godot;
using LifeLike.Core;
using LifeLike.Core.Data;

namespace LifeLike.Game.Session;

/// <summary>
/// Wczytuje dane gry i profil gracza przez FileAccess, więc działa w edytorze i w wyeksportowanej grze.
/// res://data/game.json to kopia wspólnego GBA/data/game.json robiona przy każdym buildzie (target
/// CopySharedGameData w LifeLike.Game.csproj). W eksporcie dodaj filtr "data/*.json" (Export > Resources).
/// </summary>
public static class GodotDataSource
{
    public const string GameJson = "res://data/game.json";
    /// <summary>v0.21.53 cz. 2 (#40): angielska warstwa tekstów (kopia GBA/data/lang/en.json).</summary>
    public const string LangEnJson = "res://data/lang_en.json";
    public const string ProfilePath = "user://profile.sav";
    public const string RunPath = "user://run.sav";

    public static GameData LoadGameData()
    {
        if (!FileAccess.FileExists(GameJson))
            throw new GameDataException($"brak {GameJson} – zbuduj projekt (dotnet build), żeby skopiować GBA/data/game.json");
        var json = FileAccess.GetFileAsString(GameJson);
        var en = FileAccess.FileExists(LangEnJson) ? FileAccess.GetFileAsString(LangEnJson) : null;
        var d = GameData.Parse(json, en, Loc.English);
        DataEnglish = Loc.English;
        _text = Loc.English && en is not null ? LangOverlay.Apply(json, en) : json;   // rady i strony Jak grać w tym samym języku
        return d;
    }

    private static string _text;

    /// <summary>v0.21.53 cz. 2 (#40): język, w którym wczytano dane (zmiana w ustawieniach - przeładowanie na tytule).</summary>
    public static bool DataEnglish { get; private set; }

    /// <summary>game.json w bieżącym języku (po LoadGameData; wcześniej - plik).</summary>
    private static string Text => _text ??= FileAccess.FileExists(GameJson) ? FileAccess.GetFileAsString(GameJson) : null;

    /// <summary>
    /// Rady kierownika (game.json „tips”, ekran harmonogramu na GBA). GameData z rdzenia ich nie czyta
    /// (to tekst tylko dla prezentacji), więc bierzemy je wprost z tego samego pliku.
    /// </summary>
    public static string[] LoadTips()
    {
        if (Text is null) return [];
        using var doc = JsonDocument.Parse(Text);
        if (!doc.RootElement.TryGetProperty("tips", out var tips) || tips.ValueKind != JsonValueKind.Array) return [];
        return tips.EnumerateArray().Select(t => t.GetString() ?? "").Where(t => t.Length > 0).ToArray();
    }

    /// <summary>Tablica napisów z game.json (np. "secretsHelp" – strona Jak grać); brak = pusta.</summary>
    public static string[] LoadStrings(string key)
    {
        if (Text is null) return [];
        using var doc = JsonDocument.Parse(Text);
        if (!doc.RootElement.TryGetProperty(key, out var arr) || arr.ValueKind != JsonValueKind.Array) return [];
        return arr.EnumerateArray().Select(t => t.GetString() ?? "").Where(t => t.Length > 0).ToArray();
    }

    public static Profile LoadProfile(GameData d)
    {
        var p = FileAccess.FileExists(ProfilePath)
            ? Profile.FromBytes(FileAccess.GetFileAsBytes(ProfilePath))
            : Profile.FromBytes(new byte[Profile.Size]);
        if (Meta.ProfileFix(d, p)) SaveProfile(p);
        return p;
    }

    public static RunSave LoadRun()
    {
        if (!FileAccess.FileExists(RunPath)) return null;
        var raw = FileAccess.GetFileAsBytes(RunPath);
        if (raw.Length < 16) return null;
        try
        {
            return RunSave.FromBytes(raw);
        }
        catch (System.IO.EndOfStreamException)
        {
            return null;
        }
    }

    public static void SaveRun(RunSave s)
    {
        using var f = FileAccess.Open(RunPath, FileAccess.ModeFlags.Write);
        f?.StoreBuffer(s.ToBytes());
    }

    public static void DeleteRun()
    {
        if (FileAccess.FileExists(RunPath)) DirAccess.RemoveAbsolute(ProjectSettings.GlobalizePath(RunPath));
    }

    public static void SaveProfile(Profile p)
    {
        using var f = FileAccess.Open(ProfilePath, FileAccess.ModeFlags.Write);
        f?.StoreBuffer(p.ToBytes());
    }
}
