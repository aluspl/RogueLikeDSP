using System;
using Godot;

namespace LifeLike.Game;

/// <summary>
/// Argumenty z linii poleceń (po „--”): --seed N, --smoke, --screenshot PLIK --scene NAZWA, oraz podgląd wersji
/// na telefon na komputerze: --touch (sterowanie dotykiem: pasek akcji, gesty myszą), --portrait (okno pionowe
/// jak iPhone 14 Pro Max w punktach, z symulowaną wyspą i paskiem domowym), --size SZERxWYS (rozmiar okna).
/// </summary>
public sealed class LaunchOptions
{
    /// <summary>Okno pionowe --portrait: 430x932 (punkty iPhone'a 14 Pro Max; UI w skali 1 = układ jak na telefonie).</summary>
    public static readonly Vector2I PortraitWindow = new(430, 932);

    public uint Seed { get; private init; }
    public bool Smoke { get; private init; }
    public string ScreenshotPath { get; private init; } = "";
    public string Scene { get; private init; } = "game";
    public bool Touch { get; private init; }
    public bool Portrait { get; private init; }
    public Vector2I WindowSize { get; private init; }

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

        var size = Vector2I.Zero;
        var sz = Arg("--size")?.Split('x');
        if (sz is { Length: 2 } && int.TryParse(sz[0], out var w) && int.TryParse(sz[1], out var h)) size = new Vector2I(w, h);
        var portrait = Array.IndexOf(args, "--portrait") >= 0;
        if (portrait && size == Vector2I.Zero) size = PortraitWindow;
        return new LaunchOptions
        {
            Seed = uint.TryParse(Arg("--seed"), out var s) ? s : seed,
            Smoke = Array.IndexOf(args, "--smoke") >= 0,
            ScreenshotPath = Arg("--screenshot") ?? "",
            Scene = Arg("--scene") ?? "game",
            Touch = Array.IndexOf(args, "--touch") >= 0,
            Portrait = portrait,
            WindowSize = size,
        };
    }
}
