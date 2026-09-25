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
    public const string ProfilePath = "user://profile.sav";

    public static GameData LoadGameData()
    {
        if (!FileAccess.FileExists(GameJson))
            throw new GameDataException($"brak {GameJson} – zbuduj projekt (dotnet build), żeby skopiować GBA/data/game.json");
        return GameData.Parse(FileAccess.GetFileAsString(GameJson));
    }

    public static Profile LoadProfile(GameData d)
    {
        var p = FileAccess.FileExists(ProfilePath)
            ? Profile.FromBytes(FileAccess.GetFileAsBytes(ProfilePath))
            : Profile.FromBytes(new byte[Profile.Size]);
        if (Meta.ProfileFix(d, p)) SaveProfile(p);
        return p;
    }

    public static void SaveProfile(Profile p)
    {
        using var f = FileAccess.Open(ProfilePath, FileAccess.ModeFlags.Write);
        f?.StoreBuffer(p.ToBytes());
    }
}
