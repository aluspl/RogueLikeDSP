using System;

namespace LifeLike.Game;

/// <summary>Argumenty z linii poleceń (po „--”): --seed N, --smoke, --screenshot PLIK --scene NAZWA.</summary>
public sealed class LaunchOptions
{
    public uint Seed { get; private init; }
    public bool Smoke { get; private init; }
    public string ScreenshotPath { get; private init; } = "";
    public string Scene { get; private init; } = "game";

    public bool Screenshot => ScreenshotPath.Length > 0;

    /// <summary>Tryb testowy (test dymny albo zrzut): profil tylko w pamięci, bez dźwięku.</summary>
    public bool Harness => Smoke || Screenshot;

    public static LaunchOptions Parse(string[] args, uint seed)
    {
        string Arg(string name)
        {
            var i = Array.IndexOf(args, name);
            return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
        }

        return new LaunchOptions
        {
            Seed = uint.TryParse(Arg("--seed"), out var s) ? s : seed,
            Smoke = Array.IndexOf(args, "--smoke") >= 0,
            ScreenshotPath = Arg("--screenshot") ?? "",
            Scene = Arg("--scene") ?? "game",
        };
    }
}
