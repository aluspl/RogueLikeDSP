using System;
using Godot;

namespace LifeLike.Game;

/// <summary>
/// Argumenty z linii poleceń (po „--”): --seed N, --smoke, --screenshot PLIK --scene NAZWA, oraz podgląd wersji
/// na telefon na komputerze: --touch (sterowanie dotykiem: pasek akcji, gesty myszą), --portrait (okno pionowe
/// jak iPhone 14 Pro Max w punktach, z symulowaną wyspą i paskiem domowym), --size SZERxWYS (rozmiar okna),
/// --monkey SEED KROKI (test małpy: losowe klawisze i dotknięcia przez prawdziwe wejście, Debug/MonkeyTest),
/// --filter ID (filtr ekranu bez względu na odblokowanie, v0.21.53), --lang pl|en (język, v0.21.53 cz. 2; testy bez
/// --lang: polski), v0.21.54: --view flat|34 (widok mapy), --lights on|off (efekty świetlne), --bench SEKUNDY (ze zrzutem:
/// pomiar czasu klatki sceny przed zapisem zrzutu).
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
    /// <summary>Test małpy: seed generatora akcji (0 = brak testu) i liczba akcji.</summary>
    public uint MonkeySeed { get; private init; }
    public int MonkeySteps { get; private init; }
    /// <summary>v0.21.53: --filter ID – filtr ekranu bez względu na odblokowanie (zrzuty, test).</summary>
    public string Filter { get; private init; } = "";
    /// <summary>v0.21.53 cz. 2 (#40): --lang pl|en – język gry (pusty = ustawienia albo język systemu).</summary>
    public string Lang { get; private init; } = "";
    /// <summary>v0.21.54: --view flat|34 – widok mapy (pusty = ustawienia; testy bez --view: płaski).</summary>
    public string View { get; private init; } = "";
    /// <summary>v0.21.54: --lights on|off – efekty świetlne (pusty = ustawienia; testy: włączone).</summary>
    public string Lights { get; private init; } = "";
    /// <summary>v0.21.54: --bench SEKUNDY – pomiar czasu klatki w scenie zrzutu (0 = bez pomiaru).</summary>
    public float Bench { get; private init; }

    public bool Monkey => MonkeySeed != 0;

    public bool Screenshot => ScreenshotPath.Length > 0;

    /// <summary>Tryb testowy (test dymny albo zrzut): profil tylko w pamięci, bez dźwięku.</summary>
    public bool Harness => Smoke || Screenshot || Monkey;

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
        var mk = Array.IndexOf(args, "--monkey");
        uint mseed = 0;
        var msteps = 0;
        if (mk >= 0)
        {
            mseed = mk + 1 < args.Length && uint.TryParse(args[mk + 1], out var ms) ? Math.Max(1u, ms) : 1u;
            msteps = mk + 2 < args.Length && int.TryParse(args[mk + 2], out var mn) ? mn : 3000;
        }
        if (portrait && size == Vector2I.Zero) size = PortraitWindow;
        if (Array.IndexOf(args, "--monkey") >= 0 && size == Vector2I.Zero) size = new Vector2I(1280, 720); // bez okna (headless) widok ma pełny rozmiar
        return new LaunchOptions
        {
            Seed = uint.TryParse(Arg("--seed"), out var s) ? s : seed,
            Smoke = Array.IndexOf(args, "--smoke") >= 0,
            ScreenshotPath = Arg("--screenshot") ?? "",
            Scene = Arg("--scene") ?? "game",
            Touch = Array.IndexOf(args, "--touch") >= 0,
            Portrait = portrait,
            WindowSize = size,
            MonkeySeed = mseed,
            MonkeySteps = msteps,
            Filter = Arg("--filter") ?? "",
            Lang = Arg("--lang") is "en" or "pl" ? Arg("--lang") : "",
            View = Arg("--view") is "flat" or "34" ? Arg("--view") : "",
            Lights = Arg("--lights") is "on" or "off" ? Arg("--lights") : "",
            Bench = float.TryParse(Arg("--bench"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var b) ? b : 0f,
        };
    }
}
