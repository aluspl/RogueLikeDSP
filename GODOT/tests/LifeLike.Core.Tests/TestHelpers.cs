using LifeLike.Core.Actions;
using LifeLike.Core.Data;

namespace LifeLike.Core.Tests;

/// <summary>Deterministyczny RNG: zawsze zwraca wartość maksymalną (albo stałą).</summary>
public sealed class FixedRandom(int? value = null) : IRandom
{
    public int Next(int min, int max) => value is { } v ? Math.Clamp(v, min, max) : max;
}

/// <summary>Źródło danych w pamięci — testy bez dysku.</summary>
public sealed class InMemorySource(params (string Path, string Json)[] files) : IDataSource
{
    public IEnumerable<(string, string)> ReadFolder(string folder) =>
        files.Where(f => f.Path.StartsWith(folder + "/")).Select(f => (f.Path, f.Json));
}

public static class Paths
{
    /// <summary>Katalog godot/data w repo — testy walidują prawdziwe pliki gry.</summary>
    public static string GameData
    {
        get
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "LifeLike.sln"))) dir = dir.Parent;
            return Path.Combine(dir?.FullName ?? throw new InvalidOperationException("Brak LifeLike.sln"), "godot", "data");
        }
    }
}
