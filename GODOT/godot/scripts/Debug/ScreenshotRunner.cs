using System;
using Godot;
using LifeLike.Core;

namespace LifeLike.Game.Debug;

/// <summary>Zrzut ekranu sceny pokazowej (--screenshot PLIK --scene NAZWA) w 1280x720, potem wyjście.</summary>
public sealed class ScreenshotRunner
{
    private readonly App _app;

    public ScreenshotRunner(App app) => _app = app;

    public async void Run(string path, string scene)
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
        if (scene != "prologue") s.Profile.SetFlag(Profile.FlagPrologueSeen | Profile.FlagHelpSeen); // prolog tylko w swojej scenie
        await new DebugScenes(_app).Setup(scene);
        await DebugRunner.Frames(root, 50);
        var img = root.GetViewport().GetTexture().GetImage();
        if (img.GetWidth() < 1000) img.Resize(img.GetWidth() * 2, img.GetHeight() * 2, Image.Interpolation.Nearest);
        img.SavePng(path);
        GD.Print($"Zrzut ekranu ({scene}): {path}");
        root.GetTree().Quit();
    }
}
