using System;
using System.Threading.Tasks;
using Godot;
using LifeLike.Core;
using LifeLike.Game.Audio;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;
using LifeLike.Game.Phone.ProfileTabs;
using LifeLike.Game.Screens;
using LifeLike.Game.Screens.Play;
using LifeLike.Game.Settings;
using LifeLike.Game.Touch;

namespace LifeLike.Game.Debug;

/// <summary>
/// Test dymny: godot --headless --path GODOT/godot -- --smoke. Bot (ten sam co w testach rdzenia) gra kilka etapów
/// przez ekrany (widok, HUD, telefon, harmonogram, Hurtownia, porównanie sprzętu, menu akcji), potem otwiera
/// wszystkie zakładki telefonu i ekrany; kod wyjścia 0 = OK (także brak wyjątków przy rysowaniu).
/// </summary>
public sealed class SmokeTest
{
    private readonly App _app;
    private int _steps, _offers, _drinks, _holds;
    private bool _prologue, _touch, _portrait;

    public SmokeTest(App app) => _app = app;

    private ScreenFlow Flow => _app.Flow;

    public async void Run()
    {
        var s = _app.Session;
        var g = s.Game;
        try
        {
            s.ClassId = 0;
            s.Difficulty = 0;
            s.Seed = s.Seed != 0 ? s.Seed : 424242u;
            _app.StartRun();
            await PlayStages();
            new DebugScenes(_app).AdvanceMessages(); // bot kończy na karcie etapu - dalej na mapę
            if (Flow.Current == Flow.Game && g.St == GameStatus.Playing) ExerciseHolds();
            if (Flow.Current == Flow.Game && g.St == GameStatus.Playing) await ExerciseMenuAndOffer();
            if (Flow.Current == Flow.Game && g.St == GameStatus.Playing) await ExerciseTouchAndSettings();
            var ok = g.Stage >= 5 || g.St is GameStatus.Dead or GameStatus.Won;
            var stage = g.Stage;
            await VisitScreens();
            await ExercisePortrait();
            var missing = Sfx.Missing();
            if (missing.Length > 0) throw new Exception("brak dźwięków: " + missing);
            if (DrawErrors.Count > 0) throw new Exception($"błędy rysowania: {DrawErrors.Count}, ostatni: {DrawErrors.Last}");
            GD.Print($"SMOKE {(ok ? "OK" : "FAIL")}: dane {s.Data.Version}, zawody {s.Data.Classes.Length}, etap {stage + 1}, " +
                     $"dzień {g.Turns}, HP {g.Hero.Hp}/{g.Hero.MaxHp}, wynik {g.Score}, budżet {g.Cash}, kroki {_steps}, " +
                     $"paczki {_offers}, termos {_drinks}, A/B {_holds}, prolog {(_prologue ? "tak" : "nie")}, dotyk {(_touch ? "tak" : "nie")}, pion {(_portrait ? "tak" : "nie")}, zabite w profilu {s.Profile.KillsTotal}, moce {s.Profile.PowersTotal}, " +
                     $"ekran {Flow.Current.GetType().Name}");
            _app.Root.GetTree().Quit(ok ? 0 : 1);
        }
        catch (Exception ex)
        {
            GD.PushError($"SMOKE FAIL: {ex}");
            _app.Root.GetTree().Quit(1);
        }
    }

    /// <summary>Bot gra do 5. etapu: harmonogram, karta etapu, Hurtownia, paczki i termos przez menu akcji.</summary>
    private async Task PlayStages()
    {
        var g = _app.Session.Game;
        for (; _steps < 4000 && g.Stage < 5; _steps++)
        {
            if (Flow.Current == Flow.Prologue) // pierwsza budowa: prolog (kawałek osi czasu), pominięcie, SMS
            {
                Flow.Prologue.Seek(2f);
                Flow.Prologue.HandleInput(InputCmd.Of(GameAction.A));
                if (Flow.Current != Flow.PrologueMessage) throw new Exception("prolog nie przeszedł do SMS-a");
                Flow.PrologueMessage.HandleInput(InputCmd.Of(GameAction.Start));
                if (Flow.Current != Flow.Help) throw new Exception("po prologu brak ekranu Jak grać");
                Flow.Help.HandleInput(InputCmd.Of(GameAction.A));
                if (!_app.Session.Profile.HasFlag(Profile.FlagPrologueSeen)) throw new Exception("prolog nie zapisał się w profilu");
                _prologue = true;
                continue;
            }
            if (Flow.Current == Flow.Schedule)
            {
                Flow.Schedule.Advance();
                continue;
            }
            if (Flow.Current == Flow.StageCard)
            {
                Flow.StageCard.Advance();
                continue;
            }
            if (Flow.Current == Flow.Hurtownia)
            {
                Bot.Shop(g);
                Flow.Hurtownia.Page.Buy();
                Flow.Hurtownia.Advance();
                continue;
            }
            if (Flow.Current == Flow.Offer) // okno porównania: decyzja jak bot z GBA (lepszy - zakładam)
            {
                _offers++;
                Flow.Offer.Decide(g.OfferIsBetter);
                continue;
            }
            if (Flow.Current != Flow.Game) break;
            if (g.Thermos > 0 && g.Hero.Hp * 2 < g.Hero.MaxHp) // termos przez menu akcji (Enter, strzałka w dół x2)
            {
                DrinkViaMenu();
                continue;
            }
            Bot.StepSmart(g);
            _app.AfterAction(true);
            if (_steps % 200 == 0) await DebugRunner.Frames(_app.Root, 1);
        }
    }

    private void DrinkViaMenu()
    {
        Flow.Game.Menu.Open();
        Flow.Game.Menu.Pick(2);
        Flow.Game.Menu.Pick(2);
        _drinks++;
    }

    /// <summary>Trzymane B (podgląd bez tury), krótkie B (czekanie), trzymane A (celowanie, atak po puszczeniu).</summary>
    private void ExerciseHolds()
    {
        var g = _app.Session.Game;
        var game = Flow.Game;
        var turns = g.Turns;
        game.HandleInput(InputCmd.Of(GameAction.B));
        game.Process(EnemyLook.HoldTime + 0.1);
        if (!game.Look.Active || game.Look.Count < 0) throw new Exception("podgląd pod B się nie otworzył");
        game.HandleInput(InputCmd.Release(GameAction.B));
        if (g.Turns != turns || game.Look.Active) throw new Exception("podgląd pod B zużył turę albo się nie zamknął");
        game.HandleInput(InputCmd.Of(GameAction.B));
        game.HandleInput(InputCmd.Release(GameAction.B));
        if (g.St == GameStatus.Playing && g.Turns == turns) throw new Exception("krótkie B nie czeka tury");
        if (Flow.Current != Flow.Game || g.St != GameStatus.Playing) return;
        game.HandleInput(InputCmd.Of(GameAction.A));
        game.Process(Aiming.RevealTime + 0.1);
        if (!game.Aim.Active) throw new Exception("celowanie pod A się nie włączyło");
        game.Aim.Cycle(1);
        game.HandleInput(InputCmd.Release(GameAction.A));
        if (game.Aim.Active) throw new Exception("puszczenie A nie zakończyło celowania");
        _holds++;
    }

    /// <summary>Ścieżki UI, na które bot mógł nie trafić: termos przez menu akcji i okno porównania sprzętu.</summary>
    private async Task ExerciseMenuAndOffer()
    {
        var g = _app.Session.Game;
        g.Thermos = Math.Max(g.Thermos, 1);
        g.Hero.Hp = (short)Math.Max(1, g.Hero.MaxHp / 3);
        int before = g.Thermos, hp = g.Hero.Hp;
        DrinkViaMenu();
        if (Flow.Current == Flow.Game && (g.Thermos != before - 1 || g.Hero.Hp <= hp - 20)) throw new Exception("termos z menu nie zadziałał");
        if (g.St != GameStatus.Playing || Flow.Current != Flow.Game) return;
        g.Equip(0, 0, 0);
        g.Pickups[0] = new Pickup(g.Hero.X, g.Hero.Y, PickupType.GearBox, true, 2, 1);
        g.Collect();
        _app.AfterAction(true);
        if (Flow.Current != Flow.Offer) throw new Exception("brak okna porównania sprzętu");
        await DebugRunner.Frames(_app.Root, 2);
        Flow.Offer.HandleInput(InputCmd.Of(GameAction.A));
        if (Flow.Current != Flow.Game || g.Equipped[0] != 2 || g.EquippedTrait[0] != 1) throw new Exception("zakładanie z porównania nie zadziałało");
        _offers++;
    }

    /// <summary>
    /// Sterowanie dotykiem (gesty jak z GestureTracker): dotknięcie Czekaj = tura, przytrzymanie Czekaj = podgląd
    /// bez tury, Atak, przesunięcie = krok, Telefon; ustawienia bez zużycia tury (zmiana wartości, zamknięcie).
    /// </summary>
    private async Task ExerciseTouchAndSettings()
    {
        var g = _app.Session.Game;
        var game = Flow.Game;
        Layout.Touch = true;
        try
        {
            Flow.Game.Open();
            await DebugRunner.Frames(_app.Root, 2);
            if (!_app.Nodes.Touch.Visible) throw new Exception("pasek akcji niewidoczny przy dotyku");
            Vector2 Btn(BarButton b) => ActionBar.ButtonRect(Array.IndexOf(ActionBar.Order, b)).GetCenter();
            void Send(GestureKind k, Vector2 p) => game.HandleGesture(new Gesture(k, p, p, Vector2I.Zero, 0));
            var turns = g.Turns;
            Send(GestureKind.Down, Btn(BarButton.Wait));
            game.Process(EnemyLook.HoldTime + 0.1);
            Send(GestureKind.Up, Btn(BarButton.Wait));
            if (g.Turns != turns) throw new Exception("przytrzymany Czekaj zużył turę");
            Send(GestureKind.Down, Btn(BarButton.Wait));
            Send(GestureKind.Tap, Btn(BarButton.Wait));
            Send(GestureKind.Up, Btn(BarButton.Wait));
            if (g.St == GameStatus.Playing && g.Turns == turns) throw new Exception("dotknięty Czekaj nie czeka tury");
            if (Flow.Current != Flow.Game || g.St != GameStatus.Playing) return;
            turns = g.Turns;
            var mid = Layout.UiSize / 2;
            foreach (var d in new[] { Vector2I.Left, Vector2I.Right, Vector2I.Up, Vector2I.Down })
            {
                if (Flow.Current != Flow.Game || g.St != GameStatus.Playing) return;
                game.HandleGesture(new Gesture(GestureKind.Down, mid, mid, Vector2I.Zero, 0));
                game.HandleGesture(new Gesture(GestureKind.Swipe, mid + (Vector2)d * 30, mid, d, 0.1f));
                game.HandleGesture(new Gesture(GestureKind.Up, mid + (Vector2)d * 30, mid, d, 0.1f));
            }
            if (Flow.Current == Flow.Game && g.St == GameStatus.Playing && g.Turns == turns) throw new Exception("przesunięcia nie zrobiły kroku");
            if (Flow.Current != Flow.Game || g.St != GameStatus.Playing) return;
            Send(GestureKind.Down, Btn(BarButton.Attack));
            game.Process(Aiming.RevealTime + 0.1);
            if (!game.Aim.Active) throw new Exception("trzymany Atak nie celuje");
            Send(GestureKind.Drag, mid);
            Send(GestureKind.Up, Btn(BarButton.Attack));
            if (game.Aim.Active) throw new Exception("puszczony Atak nie kończy celowania");
            if (Flow.Current != Flow.Game || g.St != GameStatus.Playing) return;
            Send(GestureKind.Down, Btn(BarButton.Phone));
            Send(GestureKind.Tap, Btn(BarButton.Phone));
            Send(GestureKind.Up, Btn(BarButton.Phone));
            if (Flow.Current != Flow.Phone) throw new Exception("Telefon z paska się nie otworzył");
            await DebugRunner.Frames(_app.Root, 2);
            Flow.Phone.HandleInput(InputCmd.Of(GameAction.Cancel));

            turns = g.Turns;
            Flow.Settings.Open(Flow.Game);
            await DebugRunner.Frames(_app.Root, 2);
            var music = GameSettings.Music;
            Flow.Settings.Page.Change(Phone.Pages.SettingsRow.Music, -1);
            if (GameSettings.Music != Math.Max(0, music - 1)) throw new Exception("ustawienia: głośność muzyki się nie zmienia");
            Flow.Settings.HandleInput(InputCmd.Of(GameAction.Cancel));
            if (Flow.Current != Flow.Game || g.Turns != turns) throw new Exception("ustawienia zużyły turę albo nie wróciły do gry");
            GameSettings.Music = music;
            _touch = true;
        }
        finally
        {
            Layout.Touch = false;
        }
    }

    /// <summary>Ekran pionowy (telefon 1290x2796): tytuł, gra z paskiem akcji, telefon na cały ekran - bez błędów rysowania.</summary>
    private async Task ExercisePortrait()
    {
        var root = _app.Root.GetTree().Root;
        var old = root.Size;
        Layout.Touch = true;
        try
        {
            root.Size = new Vector2I(1290, 2796);
            Layout.Refresh();
            await DebugRunner.Frames(_app.Root, 2);
            if (!Layout.Portrait || Layout.UiSize.X > 500) throw new Exception($"brak układu pionowego: UI {Layout.UiSize}");
            Flow.Title.Open();
            await DebugRunner.Frames(_app.Root, 2);
            Flow.ClassSelect.Open();
            await DebugRunner.Frames(_app.Root, 2);
            _app.StartRun();
            new DebugScenes(_app).AdvanceMessages();
            while (Flow.Current != Flow.Game && Flow.Current is not null)
            {
                if (Flow.Current == Flow.Prologue) Flow.Prologue.HandleInput(InputCmd.Of(GameAction.A));
                else if (!Flow.Current.HandleInput(InputCmd.Of(GameAction.Start))) break;
                new DebugScenes(_app).AdvanceMessages();
            }
            await DebugRunner.Frames(_app.Root, 2);
            Flow.Phone.Open(2, true);
            await DebugRunner.Frames(_app.Root, 2);
            if (_app.Nodes.Phone.Size != Layout.UiSize) throw new Exception("telefon pionowo nie jest na cały ekran");
            Flow.Settings.Open(Flow.Game, true);
            await DebugRunner.Frames(_app.Root, 2);
            Flow.Title.Open();
            await DebugRunner.Frames(_app.Root, 2);
            _portrait = true;
        }
        finally
        {
            Layout.Touch = false;
            root.Size = old;
            Layout.Refresh();
        }
    }

    /// <summary>Wszystkie zakładki telefonu w grze i profilu, wybór zawodu i tytuł - rysowanie bez wyjątków.</summary>
    private async Task VisitScreens()
    {
        var s = _app.Session;
        if (s.Game.St == GameStatus.Playing)
        {
            for (var t = 0; t < 5; t++)
            {
                Flow.Phone.Open(t, true);
                await DebugRunner.Frames(_app.Root, 2);
            }
            Flow.Phone.HandleInput(InputCmd.Of(GameAction.Select));
        }
        for (var t = 0; t < 5; t++)
        {
            Flow.Profile.Open(t, true);
            await DebugRunner.Frames(_app.Root, 2);
        }
        if (_app.Nodes.Phone.Current is not TrainingTab) throw new Exception("brak zakładki Koszty w profilu");
        Flow.ClassSelect.Open();
        await DebugRunner.Frames(_app.Root, 2);
        _app.Nodes.ClassSelectView.Move(1);
        s.Profile.Keepsake = 0;
        Meta.CycleKeepsake(s.Data, s.Profile, 1);
        if (Meta.SelectedKeepsake(s.Data, s.Profile) < 0) throw new Exception("brak pamiątki startowej do wyboru");
        Flow.Title.Open();
        await DebugRunner.Frames(_app.Root, 2);
    }
}
