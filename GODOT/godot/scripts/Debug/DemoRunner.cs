using System;
using Godot;
using LifeLike.Core;
using LifeLike.Game.Input;

namespace LifeLike.Game.Debug;

/// <summary>
/// v0.23.0 (reklamówka): --demo SEKUNDY – bot sam gra budowę (Bot.StepSmart co --step klatek), ekrany między
/// etapami stoją chwilę i przechodzą dalej; po czasie wyjście. Do nagrań z Godot Movie Maker
/// (`godot-mono --path GODOT/godot --write-movie klip.avi --fixed-fps 30 -- --demo 6 --class 1 [--stage 3] [--filter noir]`).
/// Profil pokazowy w pamięci (jak zrzuty), samouczek i prolog obejrzane.
/// </summary>
public sealed class DemoRunner
{
    private const int HoldFrames = 40; // ekran poza mapą (harmonogram, premia, SMS): tyle klatek widać, potem dalej

    /// <summary>Klatki na sekundę nagrania (--fixed-fps przy --write-movie).</summary>
    private const int DemoFps = 30;

    private readonly App _app;

    public DemoRunner(App app) => _app = app;

    private Screens.ScreenFlow Flow => _app.Flow;

    public async void Run(float seconds, int cls, int stage, int step)
    {
        var s = _app.Session;
        var root = _app.Root;
        s.Persist = false;
        s.Profile = DemoProfile.Create(s.Data);
        s.Seed = s.Seed != 0 ? s.Seed : 424242u;
        s.Profile.Tutorial = 0x3F;
        s.Profile.ClassesSeen = 0xFFFF;
        s.Profile.SetFlag(Profile.FlagPrologueSeen | Profile.FlagHelpSeen);
        s.ClassId = Math.Clamp(cls, 0, s.Data.Classes.Length - 1);
        _app.StartRun();
        var scenes = new DebugScenes(_app);
        for (var k = 0; k < stage && s.Game.St == GameStatus.Playing; k++) // od etapu N (skrót L+R+SELECT)
        {
            s.Game.DebugSkip();
            _app.AfterAction(true);
            scenes.AdvanceMessages();
            Skip(true);
        }
        _app.Banners.ActHint();
        var total = (int)(seconds * DemoFps); // Movie Maker (--fixed-fps 30): czas filmu liczony klatkami, nie zegarem
        var frames = 0;
        var held = 0;
        while (frames < total)
        {
            await DebugRunner.Frames(root, 1);
            frames++;
            var g = s.Game;
            if (Flow.Current == Flow.Game && g.St == GameStatus.Playing)
            {
                held = 0;
                if (frames % Math.Max(1, step) != 0) continue;
                Bot.StepSmart(g);
                _app.AfterAction(true);
                continue;
            }
            if (Flow.Current == Flow.BetweenRuns || g.St != GameStatus.Playing) continue; // koniec budowy: rozmowa zostaje
            if (++held < HoldFrames) continue;
            held = 0;
            Skip(false);
        }
        root.GetTree().Quit();
    }

    /// <summary>Ekran poza mapą: wybór jak w teście dymnym (premia, oferta, prolog) albo dalej.</summary>
    private void Skip(bool instant)
    {
        if (Flow.Current == Flow.Boons) Flow.Boons.Pick();
        else if (Flow.Current == Flow.Offer) Flow.Offer.Decide(true);
        else if (Flow.Current == Flow.Prologue) Flow.Prologue.HandleInput(InputCmd.Of(GameAction.A));
        else if (Flow.Current != Flow.Game) Flow.Current?.HandleInput(InputCmd.Of(GameAction.Start));
        if (instant) new DebugScenes(_app).AdvanceMessages();
    }
}
