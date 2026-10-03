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

    /// <summary>v0.21.54 (--bench): średni czas klatki, procesu i renderowania (CPU / GPU) sceny przez podany czas.</summary>
    private static async System.Threading.Tasks.Task Bench(Node root, string scene, float seconds)
    {
        var vp = root.GetViewport().GetViewportRid();
        RenderingServer.ViewportSetMeasureRenderTime(vp, true);
        double frame = 0, proc = 0, cpu = 0, gpu = 0, worst = 0;
        var n = 0;
        var start = Time.GetTicksUsec();
        var last = start;
        while ((Time.GetTicksUsec() - start) / 1e6 < seconds)
        {
            await root.ToSignal(root.GetTree(), SceneTree.SignalName.ProcessFrame);
            var now = Time.GetTicksUsec();
            var dt = (now - last) / 1000.0;
            last = now;
            if (n++ == 0) continue;
            frame += dt;
            worst = Math.Max(worst, dt);
            proc += Performance.GetMonitor(Performance.Monitor.TimeProcess) * 1000.0;
            cpu += RenderingServer.ViewportGetMeasuredRenderTimeCpu(vp) + RenderingServer.GetFrameSetupTimeCpu();
            gpu += RenderingServer.ViewportGetMeasuredRenderTimeGpu(vp);
        }
        var k = Math.Max(1, n - 1);
        GD.Print($"BENCH {scene} view={(Settings.GameSettings.ThreeQuarter ? "34" : "flat")} lights={(Settings.GameSettings.Lights ? "on" : "off")}: " +
                 $"{k} klatek, klatka {frame / k:0.00} ms (max {worst:0.0}), proces {proc / k:0.00} ms, render CPU {cpu / k:0.00} ms, GPU {gpu / k:0.00} ms");
    }
}
