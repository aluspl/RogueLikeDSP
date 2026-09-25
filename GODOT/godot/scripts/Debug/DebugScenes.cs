using System;
using System.Threading.Tasks;
using LifeLike.Core;
using LifeLike.Core.Data;
using LifeLike.Game.Input;
using LifeLike.Game.Screens;

namespace LifeLike.Game.Debug;

/// <summary>Sceny pokazowe do zrzutów ekranu (--scene): ustawiają stan ręcznie przez ekrany i skrót DebugSkip.</summary>
public sealed class DebugScenes
{
    /// <summary>Sceny zrzutów (--scene): lista dla README i komunikatu o błędzie.</summary>
    public static readonly string[] Names =
    [
        "title", "classselect", "profile", "catalog", "estate", "team", "training", "game", "combat", "offer", "menu",
        "overview", "phone-tasks", "phone-issues", "phone-start", "phone-gear", "phone-costs", "card", "perks", "schedule",
        "hurtownia", "boss", "endmsg", "end", "banners", "map", "aim", "preview", "prologue",
        "schedule-tip", "help", "settings", "settings-title", "walk", "weather-rain", "weather-snow", "weather-wind", "weather-heat",
        "brigade", "ally", "investor",
    ];

    private readonly App _app;
    private readonly DemoStaging _stage;

    public DebugScenes(App app)
    {
        _app = app;
        _stage = new DemoStaging(app);
    }

    private ScreenFlow Flow => _app.Flow;

    public static bool UsesDemoProfile(string scene) =>
        scene is "title" or "classselect" or "profile" or "catalog" or "estate" or "team" or "training" or "card" or "perks" or "investor";

    public async Task Setup(string scene)
    {
        var s = _app.Session;
        switch (scene)
        {
            case "title":
                Flow.Title.Open();
                return;
            case "classselect":
                s.ClassId = 1;
                Flow.ClassSelect.Open();
                return;
            case "profile":
            case "catalog":
            case "estate":
            case "team":
            case "training":
                Flow.Profile.Open(Array.IndexOf(new[] { "profile", "catalog", "estate", "team", "training" }, scene), true);
                return;
            case "prologue": // pierwsza budowa: plac w połowie przejazdu kamery, drugi podpis
                s.ClassId = 1;
                _app.StartRun();
                Flow.Prologue.Seek(4.2f);
                return;
            case "help":
                Flow.Help.Open(true, true);
                return;
            case "investor": // tryb inwestora nad wyborem zawodu: dwa modyfikatory włączone, rekord stawki
                s.ClassId = 1;
                s.Profile.Wins = Math.Max(1, s.Profile.Wins);
                s.Profile.Investor = 0x05;
                s.Profile.BestStake[1] = 3;
                Flow.ClassSelect.Open();
                Flow.Investor.Open(true);
                Flow.Investor.Page.Sel = 2;
                return;
            case "settings-title":
                Flow.Title.Open();
                Flow.Settings.Open(Flow.Title, true);
                return;
            case "card": // karta etapu z wydarzeniem: pierwszy seed, przy którym etap 2 ma wydarzenie
                s.ClassId = 1;
                for (; ; s.Seed++)
                {
                    _app.StartRun();
                    s.Game.NextStage();
                    if (s.Game.StageEvent >= 0) break;
                }
                s.ResetWatch();
                _app.Refresh();
                Flow.StageCard.Open(true);
                return;
        }
        await SetupRun(scene);
    }

    private async Task SetupRun(string scene)
    {
        var s = _app.Session;
        var g = s.Game;
        s.ClassId = scene switch { "game" => 0, "perks" => 1, "aim" => 2, _ => 5 }; // Cieśla: gwoździarka z3
        _app.StartRun();
        Flow.StageCard.Advance(); // karta etapu -> gra
        if (scene == "perks") // Murarz z Warsztatami i cechą SIŁ+1, na etapie z wydarzeniem
        {
            for (var seed = s.Seed; g.StageEvent < 0; seed++)
            {
                s.Seed = seed;
                _app.StartRun();
                g.NextStage();
                Flow.StageCard.Advance();
            }
            g.Equip(1, 2, Array.FindIndex(s.Data.GearTraits, t => t.Effect == TraitEffect.Str));
            _app.Refresh();
        }
        if (scene is "schedule" or "schedule-tip" or "hurtownia" or "endmsg" or "end" or "boss")
        {
            await SkipStages(scene);
            return;
        }
        for (var i = 0; i < 8 && Flow.Current == Flow.Game; i++)
        {
            Bot.StepSmart(g);
            _app.AfterAction(true);
            await DebugRunner.Frames(_app.Root, 1);
        }
        if (Flow.Current == Flow.Game && !_stage.EnemyInView()) _stage.BringEnemies();
        if (scene is "game" or "perks") _app.Nodes.Banners.Clear();
        if (Flow.Current == Flow.Offer) Flow.Offer.Decide(g.OfferIsBetter);
        Showcase(scene);
    }

    private void Showcase(string scene)
    {
        var g = _app.Session.Game;
        var banners = _app.Nodes.Banners;
        switch (scene)
        {
            case "offer": // pokaz: założony kask z jedną cechą, pod nogami paczka z lepszym i inną cechą
                g.Equip(0, 1, 1);
                g.Pickups[0] = new Pickup(g.Hero.X, g.Hero.Y, PickupType.GearBox, true, 0 * 3 + 2, 3);
                g.Collect();
                _app.AfterAction(true);
                break;
            case "menu":
                banners.Clear();
                g.Thermos = 2;
                g.Hero.Hp = (short)(g.Hero.MaxHp / 2);
                g.ApplyStatus(StatusEffect.Poison, 3);
                g.ApplyStatus(StatusEffect.Slip, 2);
                Flow.Game.Menu.Open();
                Flow.Game.Menu.Selected = 2;
                break;
            case "combat":
                banners.Clear();
                _stage.Combat();
                break;
            case "overview":
            case "phone-start":
                Flow.Phone.Open(2, true);
                break;
            case "phone-tasks":
                Flow.Phone.Open(0, true);
                break;
            case "phone-issues":
                Flow.Phone.Open(1, true);
                break;
            case "phone-gear":
                g.Equip(0, 1, 1);
                g.Equip(2, 2, 3);
                Flow.Phone.Open(3, true);
                break;
            case "phone-costs":
                Flow.Phone.Open(4, true);
                break;
            case "banners":
                banners.Push("Awans! Poziom 2", $"+{_app.Session.Data.HpPerLevel} HP");
                banners.Push("Nowe narzędzie", "Młotek 3-6");
                banners.Push("Moc gotowa", g.CDef.AbilityName);
                break;
            case "map":
                banners.Clear();
                Flow.Game.ToggleOverview();
                break;
            case "settings": // ustawienia w trakcie budowy (Zapisz i wyjdź, Porzuć budowę)
                banners.Clear();
                Flow.Settings.Open(Flow.Game, true);
                break;
            case "walk": // dotknięcie pola: marsz w toku (kilka kroków)
                banners.Clear();
                for (var x = Level.W - 1; x >= 0; x--)
                {
                    var cell = new Godot.Vector2I(x, g.Hero.Y);
                    if (Flow.Game.Touch.Walk.Start(cell)) break;
                }
                for (var i = 0; i < 3; i++) Flow.Game.Touch.Walk.Process(1);
                break;
            case "aim": // trzymane A: zasięg, celownik na drugim celu
                banners.Clear();
                _app.Nodes.Touch.Bar.Pressed = Touch.BarButton.Attack;
                Flow.Game.HandleInput(InputCmd.Of(GameAction.A));
                Flow.Game.Aim.Cycle(1);
                break;
            case "weather-rain": // pogoda dnia: nakładka, kałuże (deszcz), ikona w HUD
            case "weather-snow":
            case "weather-wind":
            case "weather-heat":
            {
                banners.Clear();
                var eff = scene switch
                {
                    "weather-rain" => WeatherEffect.Rain,
                    "weather-snow" => WeatherEffect.Frost,
                    "weather-wind" => WeatherEffect.Wind,
                    _ => WeatherEffect.Heat,
                };
                g.Weather = (sbyte)Array.FindIndex(_app.Session.Data.Weather, w => w.Effect == eff);
                _app.Refresh();
                break;
            }
            case "brigade": // telefon: Brygada ze 100 zł, wszyscy fachowcy, zaznaczona pompa
            case "ally": // pomocnik z brygady obok bohatera, baner wezwania
            {
                banners.Clear();
                g.Cash = 100;
                g.Bonus.Helpers = (1 << g.D.Brigade.Length) - 1;
                if (scene == "brigade")
                {
                    Flow.Brigade.Open(true);
                    Flow.Brigade.Page.Sel = Array.FindIndex(g.D.Brigade, h => h.Effect == HelperEffect.Pump);
                }
                else
                {
                    Flow.Brigade.Call(Array.FindIndex(g.D.Brigade, h => h.Effect == HelperEffect.Ally));
                }
                break;
            }
            case "preview": // trzymane B: karta najbliższego problemu
                banners.Clear();
                _app.Nodes.Touch.Bar.Pressed = Touch.BarButton.Wait;
                Flow.Game.HandleInput(InputCmd.Of(GameAction.B));
                Flow.Game.Look.Reveal();
                break;
        }
    }

    /// <summary>Przewija etapy skrótem DebugSkip (L+R+SELECT na GBA) do sceny: harmonogram, Hurtownia, boss, koniec.</summary>
    private async Task SkipStages(string scene)
    {
        var g = _app.Session.Game;
        var d = _app.Session.Data;
        for (var guard = 0; guard < 40; guard++)
        {
            if (scene == "boss" && d.Stages[g.Stage].Boss >= 0 && Flow.Current == Flow.Game)
            {
                _stage.Boss();
                return;
            }
            g.DebugSkip();
            _app.AfterAction(true);
            await DebugRunner.Frames(_app.Root, 1);
            if (Flow.Current == Flow.Schedule && scene == "schedule") return;
            if (Flow.Current == Flow.Schedule && scene == "schedule-tip" && g.Stage >= 1) return; // druga rada, notatka z odznakami
            if (Flow.Current == Flow.EndMessage)
            {
                if (scene == "end") Flow.End.Open();
                return;
            }
            if (Flow.Current == Flow.Schedule && g.ActCleared && scene == "hurtownia")
            {
                Flow.Schedule.Advance();
                return;
            }
            AdvanceMessages();
        }
    }

    /// <summary>Przewija harmonogram, kartę etapu i Hurtownię do mapy (Enter na każdym).</summary>
    public void AdvanceMessages()
    {
        while (true)
        {
            if (Flow.Current == Flow.Schedule) Flow.Schedule.Advance();
            else if (Flow.Current == Flow.StageCard) Flow.StageCard.Advance();
            else if (Flow.Current == Flow.Hurtownia) Flow.Hurtownia.Advance();
            else return;
        }
    }
}
