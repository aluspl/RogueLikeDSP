using System;
using Godot;
using LifeLike.Core;

namespace LifeLike.Game.Debug;

/// <summary>Zrzut ekranu sceny pokazowej (--screenshot PLIK --scene NAZWA) w 1280x720, potem wyjście.</summary>
public sealed class ScreenshotRunner
{
    private readonly App _app;

    public ScreenshotRunner(App app) => _app = app;

    public async void Run(string path, string scene, float bench = 0f)
    {
        var s = _app.Session;
        s.Persist = false;
        s.Profile = Meta.NewProfile(s.Data);
        s.Seed = s.Seed != 0 ? s.Seed : 424242u;
        var root = _app.Root;
        if (Array.IndexOf(DebugScenes.Names, scene) < 0)
        {
            GD.PushError($"Nieznana scena {scene}; dostępne: {string.Join(", ", DebugScenes.Names)}");
            root.GetTree().Quit(1);
            return;
        }
        if (DebugScenes.UsesDemoProfile(scene)) s.Profile = DemoProfile.Create(s.Data);
        if (!DebugScenes.UsesTutorial(scene)) // samouczek menu i dymki nowości już obejrzane (poza scenami samouczka)
        {
            s.Profile.Tutorial = 0x3F;
            s.Profile.ClassesSeen = 0xFFFF;
        }
        if (scene != "prologue") s.Profile.SetFlag(Profile.FlagPrologueSeen | Profile.FlagHelpSeen); // prolog tylko w swojej scenie
        await new DebugScenes(_app).Setup(scene);
        await DebugRunner.Frames(root, 50);
        if (bench > 0f) await Bench(root, scene, bench);
        var img = root.GetViewport().GetTexture().GetImage();
        if (img.GetWidth() < 1000) img.Resize(img.GetWidth() * 2, img.GetHeight() * 2, Image.Interpolation.Nearest);
        img.SavePng(path);
        GD.Print($"Zrzut ekranu ({scene}): {path}");
        root.GetTree().Quit();
    }

    /// <summary>v0.21.54 (--bench): czas klatki sceny przez podany czas – mediana, 95. centyl, średnia i najdłuższa
    /// (z --disable-vsync przed „--” bez limitu 60 kl./s).</summary>
    private static async System.Threading.Tasks.Task Bench(Node root, string scene, float seconds)
    {
        var times = new System.Collections.Generic.List<double>();
        var start = Time.GetTicksUsec();
        var last = start;
        while ((Time.GetTicksUsec() - start) / 1e6 < seconds)
        {
            await root.ToSignal(root.GetTree(), SceneTree.SignalName.ProcessFrame);
            var now = Time.GetTicksUsec();
            times.Add((now - last) / 1000.0);
            last = now;
        }
        if (times.Count > 1) times.RemoveAt(0);
        times.Sort();
        double P(double q) => times[Math.Min(times.Count - 1, (int)(q * times.Count))];
        var avg = 0.0;
        foreach (var t in times) avg += t;
        avg /= Math.Max(1, times.Count);
        GD.Print($"BENCH {scene} view={(Settings.GameSettings.ThreeQuarter ? "34" : "flat")} lights={(Settings.GameSettings.Lights ? "on" : "off")}: " +
                 $"{times.Count} klatek, mediana {P(0.5):0.00} ms, p95 {P(0.95):0.00} ms, średnio {avg:0.00} ms, max {times[^1]:0.0} ms");
    }
}
