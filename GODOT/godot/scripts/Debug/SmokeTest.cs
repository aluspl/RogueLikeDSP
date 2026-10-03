using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using LifeLike.Core;
using LifeLike.Game.Audio;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;
using LifeLike.Game.Phone;
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
    private string _secrets = "-";
    private readonly App _app;
    private int _steps, _offers, _drinks, _holds, _weathers, _helpers, _repairs, _levelUps;
    private int _pathWanted = -1;
    private bool _pathOk, _daily, _house;
    private bool _investor;
    private int _respectBought = -1, _reward = -1, _newClasses;
    private bool _prologue, _touch, _portrait;
    private int _acts, _splits, _blasts, _shots;
    private bool _stats, _help, _damage;
    private int _tutorial, _unlocks, _docs, _phases;
    private bool _act0;
    private int _boons, _boonList;
    private bool _inputLock;

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
            s.Events.LevelUp += (_, _) => _levelUps++;
            await ExerciseTutorial();   // świeży profil: samouczek menu, dymki nowości, powtórka z Jak grać
            await ExerciseAct0();       // Akt 0 z nagrody: pieczątki, Decyzja odmowna z drugą fazą, dalej Fundamenty
            s.Profile = Meta.NewProfile(s.Data);
            s.Profile.Tutorial = 0x3F;  // dalej bez dymków (obejrzane)
            s.Profile.ClassesSeen = 0xFFFF;
            s.ClassId = 0;
            s.Difficulty = 0;
            _app.StartRun();
            await PlayStages();
            new DebugScenes(_app).AdvanceMessages(); // bot kończy na karcie etapu - dalej na mapę
            if (Flow.Current == Flow.Game && g.St == GameStatus.Playing) ExerciseHolds();
            if (Flow.Current == Flow.Game && g.St == GameStatus.Playing) await ExerciseWeather();
            if (Flow.Current == Flow.Game && g.St == GameStatus.Playing) await ExerciseBrigade();
            if (Flow.Current == Flow.Game && g.St == GameStatus.Playing) await ExerciseRepairs();
            if (Flow.Current == Flow.Game && g.St == GameStatus.Playing) await ExerciseMenuAndOffer();
            if (Flow.Current == Flow.Game && g.St == GameStatus.Playing) await ExerciseTouchAndSettings();
            if (Flow.Current == Flow.Game && g.St == GameStatus.Playing) await ExerciseActsAndBehaviors();
            var ok = g.Stage >= 5 || g.St is GameStatus.Dead or GameStatus.Won;
            var stage = g.Stage;
            await VisitScreens();
            await ExerciseInvestor();
            await ExerciseDaily();
            await ExerciseWeeklyAndStory();
            await ExerciseRespectAndRewards();
            var secrets = new SmokeSecrets(_app);
            await secrets.Run();
            _secrets = $"{secrets.Done} wykonane, banery {secrets.Banners}, zawody {secrets.Classes}";
            await ExerciseStatsAndHelp();
            await ExerciseDamageRun();
            await ExerciseExtras();
            if (!_pathOk) throw new Exception("wybór ścieżki: druga oferta nie trafiła na etap");
            if (_boons == 0 || _boonList == 0) throw new Exception($"premie po etapie: wybrane {_boons}, lista w telefonie {_boonList}");
            if (!_inputLock) throw new Exception("blokada wejścia: nie sprawdzona");
            await ExercisePortrait();
            var missing = Sfx.Missing();
            if (missing.Length > 0) throw new Exception("brak dźwięków: " + missing);
            if (DrawErrors.Count > 0) throw new Exception($"błędy rysowania: {DrawErrors.Count}, ostatni: {DrawErrors.Last}");
            GD.Print($"SMOKE {(ok ? "OK" : "FAIL")}: dane {s.Data.Version}, zawody {s.Data.Classes.Length}, etap {stage + 1}, " +
                     $"dzień {g.Turns}, HP {g.Hero.Hp}/{g.Hero.MaxHp}, wynik {g.Score}, budżet {g.Cash}, kroki {_steps}, " +
                     $"paczki {_offers}, termos {_drinks}, A/B {_holds}, pogoda {_weathers}, brygada {_helpers}, naprawy {_repairs}, awanse {_levelUps}, ścieżka {(_pathOk ? "tak" : "nie")}, budowa dnia {(_daily ? "tak" : "nie")}, tydzień {(_weekly ? "tak" : "nie")}, podsumowanie {_recapRows} wierszy, fabuła {_story} wątków, harmonogram domu {(_house ? "tak" : "nie")}, inwestor {(_investor ? "tak" : "nie")}, Respekt {s.Profile.RespectTotal} (ranga {_respectBought}), nagroda {(_reward >= 0 ? s.Data.Rewards[_reward].Name : "-")}, nowe zawody {_newClasses}, akty {_acts}, podziały {_splits}, wybuchy {_blasts}, strzały {_shots}, statystyki {(_stats ? "tak" : "nie")}, rozpiska obrażeń {(_damage ? "tak" : "nie")}, samouczek {_tutorial} dymków + nowości {_unlocks}, Akt 0 {(_act0 ? "tak" : "nie")} (dokumenty {_docs}, druga faza {_phases}), premie {_boons} (lista {_boonList}, synergie {g.SynergyMask()}, blokada wejścia {(_inputLock ? "tak" : "nie")}), wydarzenia {_events} (ekran {(_extras ? "tak" : "nie")}), Jak grać {(_help ? "tak" : "nie")}, sekrety {_secrets}, prolog {(_prologue ? "tak" : "nie")}, dotyk {(_touch ? "tak" : "nie")}, pion {(_portrait ? "tak" : "nie")}, zabite w profilu {s.Profile.KillsTotal}, moce {s.Profile.PowersTotal}, " +
                     $"ekran {Flow.Current.GetType().Name}");
            _app.Root.GetTree().Quit(ok ? 0 : 1);
        }
        catch (Exception ex)
        {
            GD.PushError($"SMOKE FAIL: {ex}");
            _app.Root.GetTree().Quit(1);
        }
    }

    /// <summary>
    /// Samouczek menu (#25) na świeżym profilu: wszystkie dymki tytułu i wyboru zawodu (A), w kroku o statystykach
    /// I otwiera ich opis i po powrocie samouczek trwa dalej; potem po jednym dymku nowości (Respekt, budowa dnia,
    /// Akt 0, tryb inwestora, nowe zawody) - każdy raz; „Samouczek jeszcze raz” w Jak grać (SELECT) i pominięcie (B).
    /// </summary>
    private async Task ExerciseTutorial()
    {
        var s = _app.Session;
        var d = s.Data;
        var coach = _app.Coach;
        s.Profile.SetFlag(Profile.FlagPrologueSeen | Profile.FlagHelpSeen);
        for (var screen = 0; screen < 2; screen++)
        {
            if (screen == 0) Flow.Title.Open();
            else Flow.ClassSelect.Open();
            Screen cur = screen == 0 ? Flow.Title : Flow.ClassSelect;
            var want = 0;
            for (var i = 0; i < d.TutorialSteps.Length; i++) want += d.TutorialSteps[i].Screen == screen && Meta.TutorialStepShown(d, s.Profile, i, true) ? 1 : 0;
            if (!coach.Active || coach.StepsCount != want) throw new Exception($"samouczek ekranu {screen}: {coach.StepsCount} kroków zamiast {want}");
            var seen = 0;
            for (var k = 0; k < 40 && coach.Active; k++)
            {
                await DebugRunner.Frames(_app.Root, 2);
                if (!_app.Nodes.Coach.Visible) throw new Exception("samouczek: dymek niewidoczny");
                if (_app.Nodes.Coach.Hole.Size.X <= 0 && _app.Nodes.ClassSelectView.Size.X >= 320) // bez okna (headless) widoki bywają malutkie
                    throw new Exception($"samouczek: brak podświetlenia dla {coach.CurrentId}");
                seen++;
                if (coach.CurrentId == "stats")   // link: opis statystyk i powrót
                {
                    cur.HandleInput(InputCmd.Of(GameAction.Info));
                    if (Flow.Current != Flow.Stats) throw new Exception("samouczek: I nie otwiera opisu statystyk");
                    Flow.Stats.HandleInput(InputCmd.Of(GameAction.B));
                    if (Flow.Current != Flow.ClassSelect || coach.CurrentId != "stats") throw new Exception("samouczek: po opisie statystyk dymek nie wrócił");
                }
                cur.HandleInput(InputCmd.Of(GameAction.A));
            }
            if (coach.Active || seen != want || Meta.TutorialPending(s.Profile, screen)) throw new Exception($"samouczek ekranu {screen} niedokończony ({seen}/{want})");
            _tutorial += seen;
        }
        // dymki nowości: każdy raz, po jednym
        s.Profile.RespectTotal = 4;
        s.Profile.Runs = 1;
        s.Profile.Wins = d.Rewards.Length;
        s.Profile.Rewards = (byte)d.Rewards.Length;
        for (var pass = 0; pass < 2; pass++)
        {
            var got = 0;
            foreach (var screen in new[] { 0, 1 })
            {
                if (screen == 0) Flow.Title.Open();
                else Flow.ClassSelect.Open();
                Screen cur = screen == 0 ? Flow.Title : Flow.ClassSelect;
                for (var k = 0; k < 20 && coach.Active; k++)
                {
                    await DebugRunner.Frames(_app.Root, 1);
                    got++;
                    cur.HandleInput(InputCmd.Of(k % 2 == 0 ? GameAction.A : GameAction.B));
                }
            }
            var reward = 0;
            foreach (var c in d.Classes) reward += c.Reward ? 1 : 0;
            if (pass == 0 && got != 4 + reward) throw new Exception($"dymki nowości: {got} zamiast {4 + reward}");
            if (pass == 1 && got != 0) throw new Exception("dymki nowości pokazują się drugi raz");
            _unlocks += got;
        }
        // Jak grać z tytułu: SELECT = samouczek od nowa, B pomija resztę
        Flow.Help.Open(true, true);
        Flow.Help.HandleInput(InputCmd.Of(GameAction.Select));
        if (Flow.Current != Flow.Title || !coach.Active || coach.CurrentId != d.TutorialSteps[0].Id) throw new Exception("Jak grać: samouczek nie wrócił");
        Flow.Title.HandleInput(InputCmd.Of(GameAction.B));
        if (coach.Active || Meta.TutorialPending(s.Profile, 0)) throw new Exception("samouczek: B nie pomija");
        await DebugRunner.Frames(_app.Root, 2);
    }

    /// <summary>
    /// Akt 0 (nagroda za odbiór): budowa od Działki; bot zbiera dokumenty (schody zamknięte do kompletu), na Przyłączach
    /// Decyzja odmowna wchodzi w drugą fazę (Odwołanie), po niej Hurtownia i etap Fundamenty.
    /// </summary>
    private async Task ExerciseAct0()
    {
        var s = _app.Session;
        var g = s.Game;
        s.Profile = Meta.NewProfile(s.Data);
        s.Profile.Rewards = (byte)s.Data.Rewards.Length;
        s.Profile.Tutorial = 0x3F;
        s.Profile.ClassesSeen = 0xFFFF;
        s.Profile.SetFlag(Profile.FlagPrologueSeen | Profile.FlagHelpSeen);
        s.Events.DocumentFound += (_, _) => _docs++;
        s.Events.BossPhase += _ => _phases++;
        s.ClassId = 1;
        _app.StartRun();
        if (g.Stage != 0 || g.FirstStage != 0 || !g.StairsLocked() || g.ActNumeral() != "0") throw new Exception("Akt 0: budowa nie zaczyna się od Papierologii");
        for (var step = 0; step < 4000 && g.Stage < s.Data.PreludeStages; step++)
        {
            if (Flow.Current == Flow.Offer)
            {
                Flow.Offer.Decide(g.OfferIsBetter);
                continue;
            }
            if (Flow.Current != Flow.Game)
            {
                new DebugScenes(_app).AdvanceMessages();
                if (Flow.Current != Flow.Game && Flow.Current != Flow.Offer) break;
                continue;
            }
            g.Hero.Hp = g.Hero.MaxHp;   // test przejścia, nie balansu
            Bot.Step(g);
            _app.AfterAction(true);
            if (step % 100 == 0) await DebugRunner.Frames(_app.Root, 1);
        }
        new DebugScenes(_app).AdvanceMessages();
        if (g.Stage != s.Data.PreludeStages || g.ActNumeral() != "I") throw new Exception($"Akt 0: bot nie doszedł do Fundamentów (etap {g.Stage}, {g.St})");
        if (_docs != s.Data.Documents.Length || _phases != 1) throw new Exception($"Akt 0: dokumenty {_docs}, druga faza {_phases}");
        _act0 = true;
        await DebugRunner.Frames(_app.Root, 2);
    }

    /// <summary>
    /// Premia 1 z 3 po etapie (v0.21.50 cz. 2): strzałka zmienia kartę, R losuje raz (budżet), Enter bierze premię,
    /// potem harmonogram; za pierwszym razem telefon > Sprzęt > R = lista premii i synergii (strzałka: synergie).
    /// </summary>
    private async Task ExerciseBoons()
    {
        var g = _app.Session.Game;
        var page = Flow.Boons.Page;
        if (page is null || !g.HasBoonOffer) throw new Exception("premia po etapie: brak oferty");
        if (_boons == 0)
        {
            page = await ExerciseInputLock();
            if (!g.HasBoonOffer) throw new Exception("blokada wejścia: premia wybrana przed zatwierdzeniem");
            g.Cash = Math.Max(g.Cash, g.D.BoonRerollCost);
            var before = g.BoonOffer.ToArray();
            Flow.Boons.HandleInput(InputCmd.Of(GameAction.R));
            if (g.BoonRerolls != 1 || before.SequenceEqual(g.BoonOffer)) throw new Exception("premia po etapie: R nie losuje nowej oferty");
            Flow.Boons.HandleInput(InputCmd.Of(GameAction.Down));
            if (page.Sel != 1) throw new Exception("premia po etapie: strzałka nie zmienia karty");
            await DebugRunner.Frames(_app.Root, 1);
        }
        var want = g.BoonOffer[page.Sel];
        var owned = g.BoonsOwned();
        var midStage = g.St == GameStatus.Playing; // premia z projektu (wydarzenie) – powrót na plac
        Flow.Boons.HandleInput(InputCmd.Of(GameAction.A));
        if (!g.HasBoon(want) || g.BoonsOwned() != owned + 1 || (!midStage && Flow.Current != Flow.Schedule)) throw new Exception("premia po etapie: wybór nie działa");
        if (midStage) return;
        _boons++;
        if (_boonList > 0) return;
        Flow.Phone.Open(Phone.PhoneTabs.Gear, true);
        Flow.Phone.HandleInput(InputCmd.Of(GameAction.R));
        if (Flow.Current != Flow.BoonList) throw new Exception("telefon > Sprzęt: R nie otwiera listy premii");
        await DebugRunner.Frames(_app.Root, 1);
        Flow.BoonList.HandleInput(InputCmd.Of(GameAction.Right));
        if (Flow.BoonList.Page.Mode != 1) throw new Exception("lista premii: strzałka nie przełącza na synergie");
        await DebugRunner.Frames(_app.Root, 1);
        Flow.BoonList.HandleInput(InputCmd.Of(GameAction.B));
        if (Flow.Current != Flow.Phone) throw new Exception("lista premii: B nie wraca do telefonu");
        _boonList++;
        Flow.Schedule.Open();
    }

    /// <summary>
    /// Blokada wejścia (v0.21.51): świeżo otwarta premia ignoruje A z klawiatury i stuknięcie w kartę (jak stuknięcie
    /// w mapę tuż przed końcem etapu); po LockSeconds pierwsze stuknięcie w kartę tylko ją zaznacza (bez wyboru).
    /// </summary>
    private async Task<Phone.Pages.BoonPickPage> ExerciseInputLock()
    {
        var g = _app.Session.Game;
        var main = (Main)_app.Root;
        await DebugRunner.Frames(_app.Root, 2); // wiersze kart narysowane (cele dotyku)
        Flow.Boons.Open(); // świeże otwarcie: blokada od teraz (telefon już na miejscu, te same cele dotyku)
        var page = Flow.Boons.Page;
        var card = _app.Nodes.Phone.RowRect(2) ?? throw new Exception("premia po etapie: brak celu dotyku karty 3");
        var owned = g.BoonsOwned();
        if (!Flow.InputLocked) throw new Exception("blokada wejścia: okno premii otwarte bez blokady");
        GameInput.Press(GameAction.A);
        GameInput.Release(GameAction.A);
        GameInput.Press(GameAction.Start);
        GameInput.Release(GameAction.Start);
        var tap = new Gesture(GestureKind.Tap, card.GetCenter(), card.GetCenter(), Vector2I.Zero, 0.05f);
        main.InjectGesture(tap);
        main.InjectGesture(tap);
        if (g.BoonsOwned() != owned || page.Sel != 0) throw new Exception("blokada wejścia: A / stuknięcie tuż po otwarciu wybrało premię");
        await _app.Root.ToSignal(_app.Root.GetTree().CreateTimer(ScreenFlow.LockSeconds + 0.15f), SceneTreeTimer.SignalName.Timeout);
        if (Flow.InputLocked) throw new Exception("blokada wejścia: nie mija");
        main.InjectGesture(tap);
        if (page.Sel != 2 || g.BoonsOwned() != owned) throw new Exception("premia: pierwsze stuknięcie w kartę powinno ją tylko zaznaczyć");
        page.Sel = 0;
        _inputLock = true;
        return page;
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
                for (var k = 0; k < 9 && Flow.Current == Flow.Help; k++) Flow.Help.HandleInput(InputCmd.Of(GameAction.A)); // 9 stron Jak grać (v0.21.52 cz. b)
                if (Flow.Current == Flow.Help) throw new Exception("Jak grać: A nie przechodzi dalej");
                if (!_app.Session.Profile.HasFlag(Profile.FlagPrologueSeen)) throw new Exception("prolog nie zapisał się w profilu");
                _prologue = true;
                continue;
            }
            if (Flow.Current == Flow.Boons)
            {
                await ExerciseBoons();
                continue;
            }
            if (Flow.Current == Flow.Schedule)
            {
                if (_pathWanted < 0 && Flow.Schedule.Page.HasChoice) // raz: druga ścieżka strzałką w prawo
                {
                    Flow.Schedule.HandleInput(InputCmd.Of(GameAction.Right));
                    if (Flow.Schedule.Page.Sel != 1) throw new Exception("harmonogram: strzałka nie wybiera drugiej ścieżki");
                    _pathWanted = g.PathOffer(1);
                    await DebugRunner.Frames(_app.Root, 1);
                }
                Flow.Schedule.Advance();
                continue;
            }
            if (Flow.Current == Flow.StageCard)
            {
                if (_pathWanted >= 0 && !_pathOk)
                {
                    if (g.StagePath != _pathWanted) throw new Exception($"ścieżka etapu {g.StagePath}, oczekiwana {_pathWanted}");
                    _pathOk = true;
                }
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
            if (Flow.Current == Flow.Event) // wydarzenie z wyborem: SMS -> odpowiedź bota -> wynik
            {
                if (Flow.Event.Page.Phase == 1) Flow.Event.Page.Sel = g.BotEventChoice();
                Flow.Event.Advance();
                _events++;
                continue;
            }
            if (Flow.Current == Flow.Trait)
            {
                Flow.Trait.Pick();
                continue;
            }
            if (Flow.Current == Flow.ToolOffer)
            {
                Flow.ToolOffer.Decide(g.BotToolAccept());
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

    /// <summary>Każda pogoda dnia na mapie (nakładka deszczu/śniegu/wiatru/upału, kałuże, ikona w HUD) bez błędów rysowania.</summary>
    private async Task ExerciseWeather()
    {
        var g = _app.Session.Game;
        var old = g.Weather;
        for (var w = 0; w < g.D.Weather.Length; w++)
        {
            g.Weather = (sbyte)w;
            _app.Refresh();
            await DebugRunner.Frames(_app.Root, 2);
            if (g.D.Weather[w].Effect == Core.Data.WeatherEffect.Wind && g.Weapon.Range > 1 && g.WeaponRange() >= g.Weapon.Range)
                throw new Exception("wiatr nie skraca zasięgu");
            _weathers++;
        }
        g.Weather = old;
        _app.Refresh();
    }

    /// <summary>
    /// Brygada: z menu akcji (A bez kierunku) do telefonu, wezwanie zablokowanego (zostaje w telefonie), potem każdy
    /// fachowiec na kolejnych etapach (wezwanie wraca na mapę, zużywa turę, pomocnik rysuje się obok bohatera).
    /// </summary>
    private async Task ExerciseBrigade()
    {
        var g = _app.Session.Game;
        var d = g.D;
        Flow.Game.Menu.Open();
        Flow.Game.Menu.HandleInput(InputCmd.Of(GameAction.A));
        if (Flow.Current != Flow.Brigade) throw new Exception("menu akcji: A bez kierunku nie otwiera Brygady");
        await DebugRunner.Frames(_app.Root, 2);
        g.Cash = 0;
        Flow.Brigade.Call(0);
        if (Flow.Current != Flow.Brigade || g.HelperCalled >= 0) throw new Exception("brygada bez budżetu nie powinna przyjść");
        Flow.Brigade.HandleInput(InputCmd.Of(GameAction.Cancel));
        if (Flow.Current != Flow.Game) throw new Exception("Brygada: Esc nie wraca do gry");
        g.Bonus.Helpers = (1 << d.Brigade.Length) - 1;
        for (var h = d.Brigade.Length - 1; h >= 0 && Flow.Current == Flow.Game && g.St == GameStatus.Playing; h--)
        {
            g.HelperCalled = -1;
            g.Cash = 100;
            if (d.Brigade[h].Effect == Core.Data.HelperEffect.Pump && g.HelperBlocked(h) == HelperBlock.NoTarget) new DemoStaging(_app).BringEnemies();
            if (g.HelperBlocked(h) != HelperBlock.Ok) continue;
            var turns = g.Turns;
            Flow.Brigade.Open();
            Flow.Brigade.Page.Sel = h;
            Flow.Brigade.HandleInput(InputCmd.Of(GameAction.A));
            if (g.Turns == turns || g.HelperCalled != h) throw new Exception($"brygada: {d.Brigade[h].Name} nie przyszedł");
            await DebugRunner.Frames(_app.Root, 2);
            _helpers++;
        }
    }

    /// <summary>Naprawy z telefonu (Brygada i naprawy): Załataj za drewno – mur przed problemem, tura, materiał zużyty.</summary>
    private async Task ExerciseRepairs()
    {
        var g = _app.Session.Game;
        var d = g.D;
        var k = Array.FindIndex(d.Repairs, r => r.Effect == Core.Data.RepairEffect.Patch);
        if (k < 0) return;
        var rd = d.Repairs[k];
        g.Mats[rd.Material] = 0;
        Flow.Brigade.Open();
        Flow.Brigade.Page.Sel = d.Brigade.Length + k;
        await DebugRunner.Frames(_app.Root, 2);
        Flow.Brigade.HandleInput(InputCmd.Of(GameAction.A));
        if (Flow.Current != Flow.Brigade) throw new Exception("naprawa bez materiału nie powinna wyjść z telefonu");
        Flow.Brigade.HandleInput(InputCmd.Of(GameAction.Cancel));
        g.Mats[rd.Material] = 3;
        if (g.RepairBlocked(k) == RepairBlock.NoTarget) new DemoStaging(_app).BringEnemies();
        if (g.RepairBlocked(k) != RepairBlock.Ok) return;
        var turns = g.Turns;
        Flow.Brigade.Open();
        Flow.Brigade.Page.Sel = d.Brigade.Length + k;
        Flow.Brigade.HandleInput(InputCmd.Of(GameAction.A));
        if (g.Turns == turns || g.Mats[rd.Material] != 3 - rd.Cost) throw new Exception("Załataj: brak tury albo materiał niezużyty");
        await DebugRunner.Frames(_app.Root, 2);
        _repairs++;
    }

    /// <summary>
    /// Codzienna budowa z tytułu (stała data): ekran dnia, „Wyślij wynik” (lokalna zaślepka), start budowy dnia,
    /// kilka kroków bota, porażka – rekord dnia w profilu, bez NG+; potem wygrana – harmonogram domu z linkiem.
    /// </summary>
    private bool _weekly;
    private int _recapRows;
    private int _story;

    /// <summary>
    /// v0.21.50 cz. 4: wyzwanie tygodnia (strona, wyślij wynik – tabela tygodnia, start z zasadą), porażka -> SMS ->
    /// podsumowanie (przewijanie) -> plansza końcowa bez NG+; wynik tygodnia i wątki fabuły w profilu; Osiedle ->
    /// Wiadomości -> wątek (przeczytany) -> wstecz.
    /// </summary>
    private async Task ExerciseWeeklyAndStory()
    {
        var s = _app.Session;
        var g = s.Game;
        s.FixedToday = Tuple.Create(2026, 9, 29);
        Flow.Title.Open();
        Flow.Weekly.Open(true);
        await DebugRunner.Frames(_app.Root, 2);
        var week = Flow.Weekly.Page.Week;
        if (week != Weekly.Number(s.Data, 2026, 9, 29)) throw new Exception("tydzień: zły numer");
        Flow.Weekly.HandleInput(InputCmd.Of(GameAction.A));
        if (g.WeeklyWeek != week || g.Bonus.Weekly != Weekly.Index(s.Data, week) || g.Cls != Weekly.ClassOf(s.Data, week))
            throw new Exception("wyzwanie tygodnia nie wystartowało");
        new DebugScenes(_app).AdvanceMessages();
        for (var i = 0; i < 20 && Flow.Current == Flow.Game && g.St == GameStatus.Playing; i++)
        {
            Bot.StepSmart(g);
            _app.AfterAction(true);
            new DebugScenes(_app).AdvanceMessages();
        }
        if (g.St == GameStatus.Playing)
        {
            var zw = Array.FindIndex(s.Data.Enemies, e => e.Id == "zwarcie");
            g.Spawn(zw, g.Hero.X, g.Hero.Y);
            g.Hero.Hp = 1;
            g.Bonus.SecondChance = 0;
            g.EnemyStrike(g.EnemiesCount - 1, false);
            _app.AfterAction(true);
        }
        if (Weekly.Best(s.Data, s.Profile, week) != g.Score || s.Profile.WeeklyRuns == 0) throw new Exception("tydzień: brak wyniku w profilu");
        if (Flow.Current == Flow.EndMessage) Flow.EndMessage.HandleInput(InputCmd.Of(GameAction.A));
        if (Flow.Current != Flow.Recap) throw new Exception("porażka: brak podsumowania budowy");
        var page = Flow.Recap.Page;
        _recapRows = page.Count;
        await DebugRunner.Frames(_app.Root, 2);
        Flow.Recap.HandleInput(InputCmd.Of(GameAction.Down));
        await DebugRunner.Frames(_app.Root, 1);
        if (page.Count > 12 && page.Top == 0) throw new Exception("podsumowanie: nie przewija się");
        Flow.Recap.HandleInput(InputCmd.Of(GameAction.Start));
        if (Flow.Current != Flow.End || _app.Nodes.EndView.CanContinue) throw new Exception("tydzień: plansza końcowa z NG+");
        Flow.Weekly.Open(true);
        Flow.Weekly.Submit();
        if (!Flow.Weekly.Page.Note.Contains("pb.weekly.")) throw new Exception("tydzień: brak tabeli tygodnia (zaślepka)");
        _weekly = true;
        // fabuła: wątek „Pierwsza budowa” jest już odblokowany (budowy > 0); archiwum w telefonie profilu
        _story = Story.Count(s.Data, s.Profile);
        if (_story == 0) throw new Exception("fabuła: brak wątków po budowach");
        Flow.Profile.Open(2, true);
        await DebugRunner.Frames(_app.Root, 2);
        if (_app.Nodes.Phone.Current is not Phone.ProfileTabs.EstateTab et) throw new Exception("Osiedle: brak zakładki");
        _app.Nodes.Phone.HandleInput(InputCmd.Of(GameAction.A));
        if (et.Mode != 1) throw new Exception("Osiedle: A nie otwiera Wiadomości");
        var first = Enumerable.Range(0, s.Data.StoryArc.Length).First(i => Story.Unlocked(s.Profile, i));
        et.OpenThread(first);
        await DebugRunner.Frames(_app.Root, 2);
        if (Story.Unread(s.Profile, first) || et.Mode != 2) throw new Exception("Wiadomości: wątek nie został przeczytany");
        _app.Nodes.Phone.HandleInput(InputCmd.Of(GameAction.B));
        _app.Nodes.Phone.HandleInput(InputCmd.Of(GameAction.B));
        if (et.Mode != 0) throw new Exception("Wiadomości: B nie wraca na Osiedle");
        Flow.Title.Open();
    }

    private async Task ExerciseDaily()
    {
        var s = _app.Session;
        var g = s.Game;
        s.FixedToday = Tuple.Create(2026, 9, 25);
        Flow.Title.Open();
        Flow.Daily.Open(true);
        await DebugRunner.Frames(_app.Root, 2);
        Flow.Daily.Submit();
        var day = Flow.Daily.Page.Day;
        Flow.Daily.HandleInput(InputCmd.Of(GameAction.A));
        if (!g.Daily || g.DailyDay != day || g.Cls != Daily.ClassOf(s.Data, Daily.Seed(day))) throw new Exception("codzienna budowa nie wystartowała");
        new DebugScenes(_app).AdvanceMessages();
        for (var i = 0; i < 30 && Flow.Current == Flow.Game && g.St == GameStatus.Playing; i++)
        {
            Bot.StepSmart(g);
            _app.AfterAction(true);
            new DebugScenes(_app).AdvanceMessages();
        }
        if (g.St == GameStatus.Playing)
        {
            g.Hero.Hp = 0;
            g.Hero.Alive = false;
            g.St = GameStatus.Dead;
            _app.AfterAction(true);
        }
        if (Daily.Best(s.Data, s.Profile, day) != g.Score) throw new Exception("codzienna budowa: brak wyniku dnia w profilu");
        if (Flow.Current == Flow.EndMessage) Flow.EndMessage.HandleInput(InputCmd.Of(GameAction.A));
        if (Flow.Current == Flow.Recap) Flow.Recap.HandleInput(InputCmd.Of(GameAction.Start)); // podsumowanie budowy (#33)
        if (Flow.Current == Flow.End && _app.Nodes.EndView.CanContinue) throw new Exception("codzienna budowa: NG+ nie powinno być");
        await DebugRunner.Frames(_app.Root, 2);
        _daily = true;
        // wygrana: SMS -> harmonogram domu -> link (bez przeglądarki) -> plansza końcowa
        s.ClassId = 0;
        _app.StartRun();
        new DebugScenes(_app).AdvanceMessages();
        for (var guard = 0; guard < 60 && g.St != GameStatus.Won; guard++)
        {
            g.DebugSkip();
            _app.AfterAction(true);
            new DebugScenes(_app).AdvanceMessages();
        }
        if (Flow.Current != Flow.EndMessage) throw new Exception("wygrana: brak SMS-a z odbioru");
        Flow.EndMessage.HandleInput(InputCmd.Of(GameAction.A));
        if (Flow.Current != Flow.HouseSchedule) throw new Exception("wygrana: brak harmonogramu domu");
        Flow.HouseSchedule.OpenBrowser = false;
        await DebugRunner.Frames(_app.Root, 2);
        Flow.HouseSchedule.HandleInput(InputCmd.Of(GameAction.Select));
        if (Flow.HouseSchedule.LinkOpened != 1) throw new Exception("harmonogram domu: link nie działa");
        Flow.HouseSchedule.HandleInput(InputCmd.Of(GameAction.Start));
        if (Flow.Current != Flow.Recap || Flow.Recap.Page.Count < 10) throw new Exception("wygrana: brak podsumowania budowy");
        Flow.Recap.HandleInput(InputCmd.Of(GameAction.Start));
        if (Flow.Current != Flow.End || !_app.Nodes.EndView.CanContinue) throw new Exception("po harmonogramie domu brak planszy końcowej z NG+");
        await DebugRunner.Frames(_app.Root, 2);
        _house = true;
        Flow.Title.Open();
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
            if (ActionBar.HitTest(mid) != BarButton.None || VirtualStick.Hit(mid)) mid = new Vector2(mid.X, Layout.UiSize.Y * 0.3f); // środek pod paskiem akcji: wyżej
            foreach (var d in new[] { Vector2I.Left, Vector2I.Right, Vector2I.Up, Vector2I.Down })
            {
                if (Flow.Current != Flow.Game || g.St != GameStatus.Playing) return;
                game.HandleGesture(new Gesture(GestureKind.Down, mid, mid, Vector2I.Zero, 0));
                game.HandleGesture(new Gesture(GestureKind.Swipe, mid + (Vector2)d * 30, mid, d, 0.1f));
                game.HandleGesture(new Gesture(GestureKind.Up, mid + (Vector2)d * 30, mid, d, 0.1f));
            }
            var degenerate = ActionBar.HitTest(mid) != BarButton.None; // headless: okno 64x64, pasek akcji zasłania wszystko
            if (!degenerate && Flow.Current == Flow.Game && g.St == GameStatus.Playing && g.Turns == turns) throw new Exception("przesunięcia nie zrobiły kroku");
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

    /// <summary>Tryb inwestora: zablokowany przed wygraną, po wygranej Tab na wyborze zawodu, przełączanie, Mods, powrót.</summary>
    /// <summary>
    /// Respekt z ukończonych etapów w profilu, zakup rangi na stronie Respekt (Tab w Kosztach), wygrana odblokowuje
    /// nagrodę za odbiór, każdy nowy zawód (Dekarz, Tynkarz, Operator koparki) używa mocy na mapie.
    /// </summary>
    private async Task ExerciseRespectAndRewards()
    {
        var s = _app.Session;
        var p = s.Profile;
        var d = s.Data;
        if (p.RespectTotal <= 0) throw new Exception("brak Respektu w profilu po zaliczonych etapach");
        Flow.Profile.Open(4, true);
        await DebugRunner.Frames(_app.Root, 2);
        if (_app.Nodes.Phone.Current is not TrainingTab tt) throw new Exception("brak zakładki Koszty");
        Flow.Profile.HandleInput(InputCmd.Of(GameAction.Select));
        if (tt.Page != 1) throw new Exception("Tab w Kosztach nie otwiera strony Respekt");
        p.Respect = (ushort)Math.Max((int)p.Respect, 500);
        var rank = Meta.RespectRank(d, p, 0);
        Flow.Profile.HandleInput(InputCmd.Of(GameAction.A));
        _respectBought = Meta.RespectRank(d, p, 0);
        if (_respectBought != rank + 1) throw new Exception("zakup rangi Respektu nie zadziałał");
        Flow.Profile.HandleInput(InputCmd.Of(GameAction.Select));
        await DebugRunner.Frames(_app.Root, 2);
        if (tt.Page != 2) throw new Exception("Tab nie otwiera strony Nagrody");
        Flow.Profile.HandleInput(InputCmd.Of(GameAction.Select));
        Flow.Title.Open();

        // wygrana (skrót DebugSkip) - kolejna nagroda za odbiór
        var before = p.Rewards;
        s.ClassId = 1;
        _app.StartRun();
        var scenes = new DebugScenes(_app);
        for (var guard = 0; guard < 60 && Flow.Current != Flow.EndMessage; guard++)
        {
            scenes.AdvanceMessages();
            if (Flow.Current == Flow.Prologue) Flow.Prologue.HandleInput(InputCmd.Of(GameAction.A));
            if (Flow.Current == Flow.Offer) Flow.Offer.Decide(true);
            if (Flow.Current != Flow.Game) continue;
            s.Game.DebugSkip();
            _app.AfterAction(true);
            await DebugRunner.Frames(_app.Root, 1);
        }
        if (Flow.Current != Flow.EndMessage || s.Game.St != GameStatus.Won) throw new Exception("skrót nie doprowadził do odbioru");
        if (before < Meta.RewardsAvailable(d))
        {
            if (p.Rewards != before + 1 || s.LastReward != before) throw new Exception("wygrana nie odblokowała nagrody za odbiór");
            _reward = s.LastReward;
        }
        await DebugRunner.Frames(_app.Root, 2);

        // nowe zawody: moc na mapie i kilka tur bota
        var staging = new DemoStaging(_app);
        foreach (var eff in new[] { LifeLike.Core.Data.AbilityEffect.Line, LifeLike.Core.Data.AbilityEffect.Splash, LifeLike.Core.Data.AbilityEffect.Ram })
        {
            s.ClassId = Array.FindIndex(d.Classes, c => c.Ability == eff);
            _app.StartRun();
            scenes.AdvanceMessages();
            if (Flow.Current != Flow.Game) throw new Exception($"nowy zawód {d.Classes[s.ClassId].Name}: brak mapy");
            staging.NewClassShowcase(eff);
            if (!PlayCommands.UseAbility(s.Game, _app.Nodes.World)) throw new Exception($"{d.Classes[s.ClassId].Name}: moc nie zadziałała");
            _app.AfterAction(true);
            for (var k = 0; k < 20 && Flow.Current == Flow.Game && s.Game.St == GameStatus.Playing; k++)
            {
                Bot.StepSmart(s.Game);
                _app.AfterAction(true);
            }
            await DebugRunner.Frames(_app.Root, 2);
            _newClasses++;
        }
        Flow.Title.Open();
        await DebugRunner.Frames(_app.Root, 2);
    }

    private async Task ExerciseInvestor()
    {
        var s = _app.Session;
        var p = s.Profile;
        int wins = p.Wins, inv = p.Investor;
        try
        {
            p.Wins = 0;
            Flow.ClassSelect.Open();
            Flow.ClassSelect.HandleInput(InputCmd.Of(GameAction.Select));
            if (Flow.Current != Flow.ClassSelect) throw new Exception("tryb inwestora dostępny przed pierwszą wygraną");
            p.Wins = 1;
            p.Investor = 0;
            Flow.ClassSelect.HandleInput(InputCmd.Of(GameAction.Select));
            if (Flow.Current != Flow.Investor) throw new Exception("Tab na wyborze zawodu nie otwiera trybu inwestora");
            await DebugRunner.Frames(_app.Root, 2);
            Flow.Investor.Page.Sel = 3;
            Flow.Investor.HandleInput(InputCmd.Of(GameAction.A));
            if (Meta.Mods(s.Data, p).Investor != 1 << 3) throw new Exception("modyfikator nie włączył się");
            Flow.Investor.HandleInput(InputCmd.Of(GameAction.Cancel));
            if (Flow.Current != Flow.ClassSelect) throw new Exception("Esc nie wraca do wyboru zawodu");
            await DebugRunner.Frames(_app.Root, 2);
            _investor = true;
        }
        finally
        {
            p.Wins = wins;
            p.Investor = (byte)inv;
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

    /// <summary>
    /// Mechaniki aktów (błoto, porywy, pył) na etapach każdego aktu i zachowania problemów (strzał, podział, wybuch):
    /// kilka tur bota z rysowaniem; potem statystyki z zakładki Start telefonu.
    /// </summary>
    private async Task ExerciseActsAndBehaviors()
    {
        var g = _app.Session.Game;
        var d = g.D;
        var staging = new DemoStaging(_app);
        foreach (var mech in new[] { Core.Data.ActMechanic.Mud, Core.Data.ActMechanic.Gust, Core.Data.ActMechanic.Dust })
        {
            var st = Array.FindIndex(d.Stages, x => d.Acts[x.Act].Mechanic == mech && x.Boss < 0);
            if (st < 0) throw new Exception("brak etapu z mechaniką " + mech);
            g.Hero.Hp = g.Hero.MaxHp = 300;
            staging.ActShowcase(st);
            if (!g.ActIs(mech)) throw new Exception("etap bez mechaniki aktu " + mech);
            if (mech == Core.Data.ActMechanic.Dust && g.DustSight() <= 0) throw new Exception("pył nie zmniejsza widzenia");
            for (var k = 0; k < 8 && g.St == GameStatus.Playing && Flow.Current == Flow.Game; k++)
            {
                Bot.StepSmart(g);
                _app.AfterAction(true);
                await DebugRunner.Frames(_app.Root, 1);
            }
            _acts++;
            if (Flow.Current == Flow.Offer) Flow.Offer.Decide(g.OfferIsBetter);
        }
        if (g.St != GameStatus.Playing || Flow.Current != Flow.Game) return;
        g.Hero.Hp = g.Hero.MaxHp = 300;
        var before = g.EnemiesCount;
        staging.BehaviorShowcase();
        foreach (var m in g.Log) if (m.Text.Contains("z dystansu")) _shots++;   // strzał Mostka termicznego w pokazie
        _splits = g.EnemiesCount > 3 ? 1 : 0;
        _blasts = g.BlastTimer > 0 || g.DangerCell(g.Hero.X, g.Hero.Y) ? 1 : 0;
        for (var k = 0; k < 4 && g.St == GameStatus.Playing; k++)
        {
            var acted = g.PlayerWait();
            if (g.ShotEvents != 0) _shots++; // strzał z dystansu w tej turze (warstwa Godota zeruje po narysowaniu)
            _app.AfterAction(acted);
            await DebugRunner.Frames(_app.Root, 2);
            if (g.ShotEvents != 0) throw new Exception("warstwa Godota nie wyzerowała strzałów po turze");
        }
        if (_splits == 0 || _blasts == 0) throw new Exception($"pokaz zachowań: podział {_splits}, wybuch {_blasts} (problemów {before} -> {g.EnemiesCount}) etap {g.Stage} klasa {g.Cls} log: {string.Join(" | ", System.Linq.Enumerable.Select(g.Log, m => m.Text))}");
        Flow.Phone.Open(PhoneTabs.Start, true);
        await DebugRunner.Frames(_app.Root, 1);
        Flow.Phone.HandleInput(InputCmd.Of(GameAction.A));
        if (Flow.Current != Flow.Stats) throw new Exception("Start > A nie otwiera opisu statystyk");
        Flow.Stats.HandleInput(InputCmd.Of(GameAction.A));
        await DebugRunner.Frames(_app.Root, 2);
        Flow.Stats.HandleInput(InputCmd.Of(GameAction.B));
        if (Flow.Current != Flow.Phone) throw new Exception("statystyki: B nie wraca do telefonu");
        Flow.Phone.HandleInput(InputCmd.Of(GameAction.Select));
        await DebugRunner.Frames(_app.Root, 1);
    }

    /// <summary>Nowa budowa Murarzem tylko dla rozpiski obrażeń (niezależnie od tego, jak skończyły się wcześniejsze).</summary>
    private int _events;
    private bool _extras;

    /// <summary>
    /// v0.21.50 cz. 3: wydarzenie z wyborem (SMS, strzałka, odpowiedź, wynik), ulepszenie w Hurtowni z cechą (+2),
    /// nowe narzędzie przy ulepszonym (zostaję), magazyn: drzwi bez klucza, pęknięta ściana z kluczem, skrzynia.
    /// </summary>
    private async Task ExerciseExtras()
    {
        var s = _app.Session;
        s.ClassId = 1;
        _app.StartRun();
        new DebugScenes(_app).AdvanceMessages();
        var g = s.Game;
        var st = new DemoStaging(_app);
        st.EventTiles("Stal przed czasem");
        _app.AfterAction(g.PlayerMove(1, 0));
        if (Flow.Current != Flow.Event || Flow.Event.Page.Phase != 0) throw new Exception("wydarzenie: pole nie otwiera SMS-a");
        Flow.Event.HandleInput(InputCmd.Of(GameAction.A));
        await DebugRunner.Frames(_app.Root, 1);
        Flow.Event.HandleInput(InputCmd.Of(GameAction.Down));
        if (Flow.Event.Page.Sel != 1) throw new Exception("wydarzenie: strzałka nie zmienia odpowiedzi");
        var cash = g.Cash;
        Flow.Event.HandleInput(InputCmd.Of(GameAction.A));
        await DebugRunner.Frames(_app.Root, 1);
        if (Flow.Event.Page.Phase != 2 || g.StageChoicePick != 1 || g.Cash <= cash) throw new Exception("wydarzenie: odpowiedź nie zadziałała");
        Flow.Event.HandleInput(InputCmd.Of(GameAction.A));
        if (Flow.Current != Flow.Game) throw new Exception("wydarzenie: wynik nie wraca na plac");
        _events++;
        g.WeaponLvl = 1;
        g.Cash = 300;
        g.Mats[1] = 9;
        g.ActCleared = true;
        Flow.Hurtownia.Open(true);
        await DebugRunner.Frames(_app.Root, 1);
        Flow.Hurtownia.Page.Sel = Array.FindIndex(g.D.Hurtownia, it => it.Effect == LifeLike.Core.Data.ShopEffect.Upgrade);
        Flow.Hurtownia.HandleInput(InputCmd.Of(GameAction.A));
        if (g.WeaponLvl != 2 || Flow.Current != Flow.Trait) throw new Exception("Hurtownia: ulepszenie +2 bez wyboru cechy");
        await DebugRunner.Frames(_app.Root, 1);
        Flow.Trait.HandleInput(InputCmd.Of(GameAction.Down));
        Flow.Trait.HandleInput(InputCmd.Of(GameAction.A));
        if (g.WeaponTrait != 1 || Flow.Current != Flow.Hurtownia) throw new Exception("cecha narzędzia: wybór nie wrócił do Hurtowni");
        await DebugRunner.Frames(_app.Root, 1);
        Flow.Game.Open(true);
        st.EventTiles();
        g.Pickups[g.PickupsCount++] = new Pickup(g.Hero.X + 1, g.Hero.Y, PickupType.Tool, true, 0);
        _app.AfterAction(g.PlayerMove(1, 0));
        if (Flow.Current != Flow.ToolOffer) throw new Exception("narzędzie przy ulepszonym: brak okna decyzji");
        await DebugRunner.Frames(_app.Root, 1);
        Flow.ToolOffer.HandleInput(InputCmd.Of(GameAction.A)); // v0.21.51: zaznaczone na start „Zostaję” - A nie zamienia
        if (g.WeaponLvl != 2 || Flow.Current != Flow.Game) throw new Exception("narzędzie: A bez zaznaczenia zamiany nie może zabrać ulepszenia");
        st.SecretStage(1);
        var turns = g.Turns;
        _app.AfterAction(g.PlayerMove(1, 0));
        if (g.SecretOpen || g.Turns != turns) throw new Exception("drzwi magazynu otwarte bez klucza");
        st.SecretStage(0);
        g.Keys = 1;
        _app.AfterAction(g.PlayerMove(1, 0));
        if (!g.SecretOpen || g.Keys != 0) throw new Exception("magazyn: klucz nie otwiera pękniętej ściany");
        var respect = g.Respect;
        for (var k = 0; k < 4 && Flow.Current == Flow.Game; k++) _app.AfterAction(g.PlayerMove(1, 0));
        if (g.Respect != respect + g.D.ChestRespect) throw new Exception("magazyn: skrzynia nie dała Respektu");
        if (Flow.Current == Flow.Offer) Flow.Offer.Decide(true);
        await DebugRunner.Frames(_app.Root, 2);
        Flow.Phone.Open(PhoneTabs.Tasks, true);
        await DebugRunner.Frames(_app.Root, 1);
        _extras = true;
        Flow.Title.Open();
        await DebugRunner.Frames(_app.Root, 2);
    }

    private async Task ExerciseDamageRun()
    {
        _app.Session.ClassId = 1;
        _app.StartRun();
        new DebugScenes(_app).AdvanceMessages();
        for (var guard = 0; guard < 20 && Flow.Current != Flow.Game && Flow.Current is not null; guard++)
        {
            if (Flow.Current == Flow.Prologue) Flow.Prologue.HandleInput(InputCmd.Of(GameAction.A));
            else if (!Flow.Current.HandleInput(InputCmd.Of(GameAction.Start))) break;
            new DebugScenes(_app).AdvanceMessages();
        }
        await DebugRunner.Frames(_app.Root, 2);
        if (Flow.Current != Flow.Game) throw new Exception("rozpiska: nie udało się wejść na plac");
        await ExerciseDamage(_app.Session.Game);
        Flow.Title.Open();
        await DebugRunner.Frames(_app.Root, 2);
    }

    /// <summary>
    /// Rozpiska obrażeń (#26): Sprzęt > I = strona Obrażenia (zakres jak w rdzeniu), Kryt i obrona, powrót na Sprzęt;
    /// karta problemu (trzymane B) z obrażeniami w obie strony.
    /// </summary>
    private async Task ExerciseDamage(LifeLike.Core.Game g)
    {
        Flow.Phone.Open(PhoneTabs.Gear, true);
        await DebugRunner.Frames(_app.Root, 1);
        Flow.Phone.HandleInput(InputCmd.Of(GameAction.Info));
        if (Flow.Current != Flow.Stats || _app.Nodes.Phone.Current is not Phone.Pages.StatsPage sp || sp.Page != Phone.Pages.StatsPage.DamagePage)
            throw new Exception("Sprzęt > I nie otwiera rozpiski obrażeń");
        var b = g.WeaponBreakdown();
        if (b.Min < 1 || b.Max < b.Min || b.CritMin != b.Min * g.D.CritMultiplier || b.CritPct != g.CritPct())
            throw new Exception($"rozpiska: zakres {b.Min}-{b.Max}, kryt {b.CritMin} / {b.CritPct} vs {g.CritPct()}");
        await DebugRunner.Frames(_app.Root, 2);
        Flow.Stats.HandleInput(InputCmd.Of(GameAction.A));
        if (sp.Page != Phone.Pages.StatsPage.DamagePage + 1) throw new Exception("rozpiska: A nie przechodzi do Kryt i obrona");
        await DebugRunner.Frames(_app.Root, 2);
        Flow.Stats.HandleInput(InputCmd.Of(GameAction.B));
        if (Flow.Current != Flow.Phone || _app.Nodes.Phone.TabIndex != PhoneTabs.Gear) throw new Exception("rozpiska: B nie wraca na Sprzęt");
        await DebugRunner.Frames(_app.Root, 1);
        _damage = true;
    }

    /// <summary>Opis statystyk na wyborze zawodu (I, dymek nad wierszem) i 3 strony Jak grać z tytułu.</summary>
    private async Task ExerciseStatsAndHelp()
    {
        Flow.ClassSelect.Open();
        await DebugRunner.Frames(_app.Root, 1);
        _app.Nodes.ClassSelectView.TipStat = 1;
        await DebugRunner.Frames(_app.Root, 2);
        _app.Nodes.ClassSelectView.TipStat = Screens.Views.ClassSelectView.WeaponTip;   // rozpiska obrażeń broni (#26)
        await DebugRunner.Frames(_app.Root, 2);
        _app.Nodes.ClassSelectView.TipStat = -1;
        Flow.ClassSelect.HandleInput(InputCmd.Of(GameAction.Info));
        if (Flow.Current != Flow.Stats) throw new Exception("wybór zawodu: I nie otwiera opisu statystyk");
        Flow.Stats.HandleInput(InputCmd.Of(GameAction.A));
        await DebugRunner.Frames(_app.Root, 2);
        Flow.Stats.HandleInput(InputCmd.Of(GameAction.B));
        if (Flow.Current != Flow.ClassSelect) throw new Exception("statystyki: B nie wraca na wybór zawodu");
        _stats = true;
        Flow.Help.Open(true, true);
        for (var k = 0; k < 9; k++) // 9 stron: v0.21.50 cz. 4 - po budowie, tydzień, fabuła; v0.21.51 cz. 2 - sekrety; v0.21.52 cz. b - inspektor
        {
            await DebugRunner.Frames(_app.Root, 1);
            Flow.Help.HandleInput(InputCmd.Of(GameAction.A));
        }
        if (Flow.Current != Flow.Title) throw new Exception("Jak grać: po 9 stronach brak powrotu na tytuł");
        _help = true;
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
