using System;
using Godot;
using LifeLike.Core;
using LifeLike.Core.Data;
using LifeLike.Game.Debug;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;
using LifeLike.Game.Session;
using LifeLike.Game.Settings;
using LifeLike.Game.Touch;

namespace LifeLike.Game;

/// <summary>
/// Korzeń sceny (scenes/Main.tscn): wczytuje dane (game.json wspólny z GBA) i profil, składa App (sesja, węzły,
/// ekrany) i przekazuje ekranowi bieżącemu wejście (jako akcje gry) i czas. Cała logika gry siedzi
/// w LifeLike.Core (port 1:1 z GBA), przejścia ekranów w Screens/, zrzuty i test dymny w Debug/.
/// Przy sterowaniu dotykiem (telefon albo --touch) dotyk / lewy przycisk idzie przez GestureTracker jako gesty;
/// klucz ustawień w rogu obsługuje Main przed ekranem. Gdy system usypia aplikację w trakcie budowy - zapis budowy.
/// </summary>
public partial class Main : Node2D
{
    [Export] public uint Seed { get; set; } // 0 = losowy

    private App _app;
    private readonly GestureTracker _gestures = new();
    private bool _wrenchDown;

    public override void _Ready()
    {
        GameInput.Register();
        RenderingServer.SetDefaultClearColor(Pal.Void);
        var opts = LaunchOptions.Parse(OS.GetCmdlineUserArgs(), Seed);
        if (!opts.Harness) GameSettings.Load();
        Layout.Touch = OS.HasFeature("mobile") || opts.Touch;
        if (opts.Portrait) Layout.SimulatedInsets = new Vector2(59, 34); // wyspa i pasek domowy iPhone'a 14 Pro Max (pt)
        if (opts.WindowSize != Vector2I.Zero && !OS.HasFeature("mobile"))
        {
            DisplayServer.WindowSetSize(opts.WindowSize);
            GetTree().Root.Size = opts.WindowSize;
        }
        Layout.Track(GetTree().Root);
        GameData d;
        try
        {
            d = GodotDataSource.LoadGameData();
        }
        catch (Exception ex)
        {
            GD.PushError($"Dane gry: {ex.Message}");
            GetTree().Quit(1);
            return;
        }
        var profile = opts.Harness ? Meta.NewProfile(d) : GodotDataSource.LoadProfile(d);
        _app = new App(this, d, profile, !opts.Harness, !opts.Harness, opts.Seed);
        GameInput.Injected += OnInjected;
        _gestures.Emit = OnGesture;
        if (DebugRunner.TryStart(_app, opts)) return;
        _app.Flow.Title.Open();
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (Layout.Touch && _app is not null && _gestures.Feed(e))
        {
            GetViewport().SetInputAsHandled();
            return;
        }
        if (e is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left } mb && WrenchHit(mb.Position))
        {
            OpenSettings();
            GetViewport().SetInputAsHandled();
            return;
        }
        if (Dispatch(GameInput.Translate(e))) GetViewport().SetInputAsHandled();
    }

    /// <summary>Gest dotyku: klucz ustawień (cały gest od dotknięcia do puszczenia), potem ekran bieżący.</summary>
    private void OnGesture(Gesture g)
    {
        if (_app?.Flow.Current is null) return;
        if (g.Kind == GestureKind.Down) _wrenchDown = WrenchHit(g.Pos);
        if (_wrenchDown)
        {
            if (g.Kind == GestureKind.Tap && WrenchHit(g.Pos)) OpenSettings();
            if (g.Kind == GestureKind.Up) _wrenchDown = false;
            return;
        }
        _app.Flow.Current.HandleGesture(g);
    }

    private bool WrenchHit(Vector2 p) => _app is not null && _app.Flow.Current is { ShowsSettings: true } && _app.Nodes.Settings.Hit(p);

    private void OpenSettings()
    {
        var cur = _app.Flow.Current;
        if (cur == _app.Flow.Game) _app.Flow.Game.Touch.Reset();
        _app.Flow.Settings.Open(cur);
    }

    public override void _Notification(int what)
    {
        if (what is not ((int)NotificationApplicationPaused or (int)NotificationWMCloseRequest or (int)NotificationApplicationFocusOut)) return;
        if (_app?.Flow.Current is { InRun: true }) _app.Session.SaveRun();
    }

    /// <summary>Akcja gracza (klawiatura, pad, mysz albo wirtualny kontroler) do ekranu bieżącego.</summary>
    private bool Dispatch(InputCmd cmd) => _app?.Flow.Current is not null && _app.Flow.Current.HandleInput(cmd);

    private void OnInjected(InputCmd cmd) => Dispatch(cmd);

    public override void _Process(double delta)
    {
        _gestures.Process(delta);
        _app?.Flow.Current?.Process(delta);
    }

    public override void _ExitTree()
    {
        GameInput.Injected -= OnInjected;
        Assets.ClearCache();
        Ui.ClearCache();
        PixelFont.Release();
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }
}
