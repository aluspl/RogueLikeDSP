using System;
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
        Profile p;
        try
        {
            p = FileAccess.FileExists(ProfilePath)
                ? Profile.FromBytes(FileAccess.GetFileAsBytes(ProfilePath))
                : Profile.FromBytes(new byte[Profile.Size]);
        }
        catch (Exception ex)
        {
            // Uszkodzony profil nie może blokować startu gry: zaczynamy od nowego (plik nadpiszemy przy zapisie).
            GD.PushWarning($"Profil {ProfilePath} nie do odczytu, start od nowego profilu: {ex.Message}");
            p = Profile.FromBytes(new byte[Profile.Size]);
        }
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
        catch (Exception ex) when (ex is System.IO.IOException or ArgumentException or IndexOutOfRangeException)
        {
            // Uszkodzony zapis przerwanej budowy: traktujemy jak brak zapisu, nie wywalamy gry.
            GD.PushWarning($"Zapis przerwanej budowy {RunPath} uszkodzony: {ex.Message}");
            return null;
        }
    }

    public static bool SaveRun(RunSave s) => WriteAtomic(RunPath, s.ToBytes());

    public static void DeleteRun()
    {
        if (FileAccess.FileExists(RunPath)) DirAccess.RemoveAbsolute(ProjectSettings.GlobalizePath(RunPath));
    }

    public static bool SaveProfile(Profile p) => WriteAtomic(ProfilePath, p.ToBytes());

    /// <summary>
    /// Zapis przez plik tymczasowy + przeniesienie: przerwanie w trakcie nie zostawia uszkodzonego pliku
    /// zapisu. Zwraca false i loguje błąd, gdy zapis się nie udał (gracz zachowuje poprzedni plik).
    /// </summary>
    private static bool WriteAtomic(string path, byte[] data)
    {
        var tmp = path + ".tmp";
        var ok = false;
        using (var f = FileAccess.Open(tmp, FileAccess.ModeFlags.Write))
        {
            if (f is null)
            {
                GD.PushError($"Zapis {path}: nie można otworzyć pliku tymczasowego (błąd {FileAccess.GetOpenError()})");
            }
            else
            {
                f.StoreBuffer(data);
                var err = f.GetError();
                if (err == Error.Ok) ok = true;
                else GD.PushError($"Zapis {path}: błąd zapisu {err}");
            }
        }
        // Plik tymczasowy zamknięty dopiero tutaj, więc można go bezpiecznie usunąć po błędzie.
        if (!ok)
        {
            DeleteQuietly(tmp);
            return false;
        }
        var rename = DirAccess.RenameAbsolute(ProjectSettings.GlobalizePath(tmp), ProjectSettings.GlobalizePath(path));
        if (rename != Error.Ok)
        {
            GD.PushError($"Zapis {path}: nie można podmienić pliku ({rename})");
            DeleteQuietly(tmp);
            return false;
        }
        return true;
    }

    private static void DeleteQuietly(string path)
    {
        if (FileAccess.FileExists(path)) DirAccess.RemoveAbsolute(ProjectSettings.GlobalizePath(path));
    }
}
