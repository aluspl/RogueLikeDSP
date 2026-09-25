using System.Threading.Tasks;
using Godot;

namespace LifeLike.Game.Debug;

/// <summary>
/// Wejście do trybów testowych z linii poleceń: --smoke (SmokeTest) i --screenshot PLIK --scene NAZWA
/// (ScreenshotRunner). Kod produkcyjny woła tylko TryStart - sceny pokazowe i test żyją w tym katalogu.
/// </summary>
public static class DebugRunner
{
    /// <summary>true = uruchomiono tryb testowy (gra nie pokazuje tytułu).</summary>
    public static bool TryStart(App app, LaunchOptions opts)
    {
        if (opts.Smoke)
        {
            new SmokeTest(app).Run();
            return true;
        }
        if (opts.Screenshot)
        {
            new ScreenshotRunner(app).Run(opts.ScreenshotPath, opts.Scene);
            return true;
        }
        return false;
    }

    /// <summary>Czeka n klatek drzewa sceny.</summary>
    public static async Task Frames(Node root, int n)
    {
        for (var i = 0; i < n; i++) await root.ToSignal(root.GetTree(), SceneTree.SignalName.ProcessFrame);
    }
}
