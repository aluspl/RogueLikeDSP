using System.Collections.Generic;
using Godot;
using LifeLike.Core.Data;

namespace LifeLike.Game;

/// <summary>
/// Czyta JSON-y przez FileAccess/DirAccess, więc działa i w edytorze, i w wyeksportowanej grze (res:// w .pck),
/// i z user://mods. W eksporcie dodaj filtr "data/*.json" (Export > Resources > Filters to export non-resource files).
/// </summary>
public sealed class GodotDataSource(string root) : IDataSource
{
    public IEnumerable<(string Path, string Json)> ReadFolder(string folder)
    {
        var files = new List<(string, string)>();
        Collect($"{root}/{folder}", folder, files);
        files.Sort((a, b) => string.CompareOrdinal(a.Item1, b.Item1));
        return files;
    }

    private static void Collect(string dirPath, string rel, List<(string, string)> into)
    {
        using var dir = DirAccess.Open(dirPath);
        if (dir is null) return;
        foreach (var sub in dir.GetDirectories()) Collect($"{dirPath}/{sub}", $"{rel}/{sub}", into);
        foreach (var file in dir.GetFiles())
            if (file.EndsWith(".json"))
                into.Add(($"{rel}/{file}", FileAccess.GetFileAsString($"{dirPath}/{file}")));
    }
}
