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
        "schedule-path", "materials", "repairs", "hurtownia-mats", "daily", "house", "levelup", "death",
        "respect", "rewards", "classselect-locked", "class-dekarz", "class-tynkarz", "class-operator", "gear5", "respect-banner",
        "second-chance", "behaviors", "act-mud", "act-gust", "act-dust", "stats-class", "stats-tip", "stats-phone", "catalog-tags",
        "help-acts", "help-stats",
        "tutorial-title", "tutorial-class", "tutorial-stats", "tutorial-unlock", "tutorial-act0", "help-tutorial",
        "act0-card", "act0-stamps", "act0-stairs-open", "act0-boss-phase",
        "dmg-class", "dmg-stats", "dmg-gear", "dmg-phone", "dmg-crit", "dmg-offer", "dmg-tool", "dmg-enemy", "help-dmg",
        "boon-pick", "boon-synergy", "boon-phone", "boon-synergies", "elite-map", "elite-card", "combo-shock", "combo-dust", "combo-crack",
        "help-combos",
        "event-map", "event-sms", "event-choices", "event-result", "event-boon", "upgrade-shop", "upgrade-trait", "upgrade-gear",
        "tool-swap", "secret-crack", "secret-card", "secret-open", "secret-door", "secret-map", "tasks-extras", "help-extras",
        "recap-endmsg", "recap-death", "recap-death-scroll", "recap-win", "recap-end", "weekly", "weekly-card", "weekly-run",
        "recap-progress", "titles", "looks", "inspector", "help-progress",
        "story-archive", "story-thread", "estate-grow", "help-meta",
        .. SecretStaging.Names,
        .. GoalsStaging.Names,
        .. CareerStaging.Names,
        .. FilterStaging.Names,
    ];

    private readonly App _app;
    private readonly DemoStaging _stage;
    private readonly RecapStaging _recap;
    private readonly SecretStaging _secrets;
    private readonly GoalsStaging _goals;
    private readonly CareerStaging _career;
    private readonly FilterStaging _filters;

    public DebugScenes(App app)
    {
        _app = app;
        _stage = new DemoStaging(app);
        _recap = new RecapStaging(app);
        _secrets = new SecretStaging(app);
        _goals = new GoalsStaging(app);
        _career = new CareerStaging(app);
        _filters = new FilterStaging(app);
    }

    private ScreenFlow Flow => _app.Flow;

    public static bool UsesDemoProfile(string scene) =>
        scene is "title" or "classselect" or "profile" or "catalog" or "estate" or "team" or "training" or "card" or "perks" or "investor"
            or "daily" or "death" or "respect" or "rewards" or "classselect-locked" or "stats-class" or "stats-tip" or "catalog-tags"
            or "tutorial-unlock" or "tutorial-act0" or "dmg-class" or "dmg-stats" or "titles" or "looks" or "recap-progress" or "recap-end" or "inspector"
            || SecretStaging.UsesDemoProfile(scene) || GoalsStaging.Handles(scene) || CareerStaging.Handles(scene);

    /// <summary>Sceny samouczka menu: profil bez obejrzanych dymków (inne sceny - samouczek już obejrzany).</summary>
    public static bool UsesTutorial(string scene) => scene.StartsWith("tutorial");

    public async Task Setup(string scene)
    {
        var s = _app.Session;
        if (scene == "filter-boon") scene = "boon-pick"; // v0.21.53: oferta premii w filtrze (litery rzadkości)
        if (FilterStaging.Handles(scene)) // v0.21.53: filtry ekranu
        {
            await _filters.Setup(scene);
            return;
        }
        if (RecapStaging.Handles(scene)) // v0.21.50 cz. 4: podsumowanie, wyzwanie tygodnia, fabuła
        {
            await _recap.Setup(scene);
            return;
        }
        if (SecretStaging.Handles(scene)) // v0.21.51 cz. 2: sekretne zlecenia
        {
            await _secrets.Setup(scene);
            return;
        }
        if (CareerStaging.Handles(scene)) // v0.21.52 cz. d: mapa kariery
        {
            await _career.Setup(scene);
            return;
        }
        if (GoalsStaging.Handles(scene)) // v0.21.52 cz. c: drzewko, kolekcje, zadania, seria dni
        {
            await _goals.Setup(scene);
            return;
        }
        switch (scene)
        {
            case "title":
                Flow.Title.Open();
                return;
            case "tutorial-title": // pierwsze uruchomienie: trzeci dymek (Szkolenia) nad przyciemnionym tytułem
                Flow.Title.Open();
                _app.Coach.Next();
                _app.Coach.Next();
                return;
            case "tutorial-class": // wybór zawodu: dymek o trudności
            case "tutorial-stats": // dymek o statystykach z przyciskiem do ich opisu
                Meta.TutorialDone(s.Profile, 0);
                s.ClassId = 1;
                Flow.ClassSelect.Open();
                while (_app.Coach.Active && _app.Coach.CurrentId != (scene == "tutorial-class" ? "difficulty" : "stats")) _app.Coach.Next();
                return;
            case "tutorial-unlock": // dymek nowości: nowy zawód z nagrody (Dekarz) na wyborze zawodu
                s.Profile.Tutorial = (ushort)(Tutorial.Title | Tutorial.Class | Tutorial.Investor);
                s.Profile.ClassesSeen = 0;
                s.ClassId = 1;
                Flow.ClassSelect.Open();
                return;
            case "tutorial-act0": // dymek nowości na tytule: Akt 0 po ósmej wygranej
                s.Profile.Wins = s.Data.Rewards.Length;
                s.Profile.Rewards = (byte)s.Data.Rewards.Length;
                s.Profile.Tutorial = (ushort)(0x3F & ~Tutorial.Act0);
                Flow.Title.Open();
                return;
            case "help-tutorial": // Jak grać z tytułu: przycisk „Samouczek jeszcze raz”
                Flow.Help.Open(true, true);
                return;
            case "act0-card":
            case "act0-stamps":
            case "act0-stairs-open":
            case "act0-boss-phase":
                s.Profile.Rewards = (byte)s.Data.Rewards.Length;   // nagroda Akt 0: budowa zaczyna się od Papierologii
                s.ClassId = 1;
                _app.StartRun();
                if (scene == "act0-card") return;
                Flow.StageCard.Advance();
                _app.Nodes.Banners.Clear();
                if (scene == "act0-stamps") _app.Banners.ActHint();
                if (scene == "act0-boss-phase") _stage.Act0BossPhase();
                else _stage.Act0Stamps(scene == "act0-stairs-open");
                await DebugRunner.Frames(_app.Root, 4);
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
            case "respect": // telefon profilu: Koszty, strona Respekt (część rang kupiona)
            case "rewards": // telefon profilu: Koszty, strona Nagrody za odbiór (3 odebrane)
            {
                Flow.Profile.Open(4, true);
                if (_app.Nodes.Phone.Current is Phone.ProfileTabs.TrainingTab tt)
                {
                    tt.Page = scene == "respect" ? Phone.ProfileTabs.TrainingTab.RespectPage : Phone.ProfileTabs.TrainingTab.RewardsPage;
                    tt.Select(scene == "respect" ? 5 : 3);
                }
                _app.Nodes.Phone.QueueRedraw();
                return;
            }
            case "classselect-locked": // zawód z nagrody za odbiór, jeszcze zablokowany
                s.ClassId = Array.FindIndex(s.Data.Classes, c => c.Ability == AbilityEffect.Splash);
                Flow.ClassSelect.Open();
                return;
            case "prologue": // pierwsza budowa: plac w połowie przejazdu kamery, drugi podpis
                s.ClassId = 1;
                _app.StartRun();
                Flow.Prologue.Seek(4.2f);
                return;
            case "help":
            case "help-acts":
            case "help-stats":
            case "help-dmg":
            case "help-combos":
            case "help-extras":
            case "help-meta":
            case "help-progress": // v0.21.52 cz. b: strona 9 – inspektor i mistrzostwo
                Flow.Help.Open(true, true);
                if (_app.Nodes.Phone.Current is Phone.Pages.HelpPage hp)
                    hp.Page = scene == "help" ? 0 : scene == "help-acts" ? 1 : scene == "help-stats" ? 2 : scene == "help-dmg" ? 3 : scene == "help-combos" ? 4 : scene == "help-extras" ? 5 : scene == "help-progress" ? 8 : 6;
                _app.Nodes.Phone.QueueRedraw();
                return;
            case "dmg-class": // rozpiska obrażeń broni (#26): dymek nad narzędziem na karcie zawodu
                s.ClassId = 1;
                Flow.ClassSelect.Open();
                _app.Nodes.ClassSelectView.TipStat = Screens.Views.ClassSelectView.WeaponTip;
                return;
            case "dmg-stats": // strona Obrażenia broni w opisie statystyk zawodu
                s.ClassId = 1;
                Flow.ClassSelect.Open();
                Flow.Stats.OpenClass(1, Phone.Pages.StatsPage.DamagePage);
                return;
            case "stats-class": // opis statystyk zawodu (I na wyborze zawodu, START na GBA)
            case "stats-tip":   // dymek nad statystyką (mysz / dotknięcie)
                s.ClassId = 1;
                Flow.ClassSelect.Open();
                if (scene == "stats-tip") _app.Nodes.ClassSelectView.TipStat = 1;
                else Flow.Stats.OpenClass(1);
                return;
            case "catalog-tags": // Katalog: wszystko poznane, zaznaczony Mostek termiczny z zachowaniami
            {
                s.Profile.Catalog = 0xFFFF;
                s.Profile.CatalogHi = 0xFFFFFFFFu;
                Flow.Profile.Open(1, true);
                if (_app.Nodes.Phone.Current is Phone.ProfileTabs.CatalogTab ct) ct.Select(Array.FindIndex(s.Data.Enemies, e => e.Id == "mostek"));
                _app.Nodes.Phone.QueueRedraw();
                return;
            }
            case "daily": // codzienna budowa: stała data, wyniki kilku dni w profilu, notatka po „Wyślij wynik”
            {
                s.FixedToday = Tuple.Create(2026, 9, 25);
                var day = s.TodayNumber;
                Daily.Record(s.Data, s.Profile, day - 3, 2140, false);
                Daily.Record(s.Data, s.Profile, day - 2, 5320, true);
                Daily.Record(s.Data, s.Profile, day - 1, 1870, false);
                Daily.Record(s.Data, s.Profile, day, 3410, false);
                Flow.Title.Open();
                Flow.Daily.Open(true);
                Flow.Daily.Submit();
                return;
            }
            case "investor": // tryb inwestora nad wyborem zawodu: dwa modyfikatory włączone, rekord stawki
                s.ClassId = 1;
                s.Profile.Wins = Math.Max(1, s.Profile.Wins);
                s.Profile.Investor = 0x05;
                s.Profile.BestStake[1] = 3;
                Flow.ClassSelect.Open();
                Flow.Investor.Open(true);
                Flow.Investor.Page.Sel = 2;
                return;
            case "titles": // v0.21.52: Odznaki > Tytuły, zaznaczony wybrany tytuł
                Flow.Profile.Open(0, true);
                if (_app.Nodes.Phone.Current is Phone.ProfileTabs.BadgesTab tb)
                {
                    tb.Page = Phone.ProfileTabs.BadgesTab.TitlesPage;
                    tb.Select(Math.Max(0, Titles.Selected(s.Data, s.Profile)));
                }
                _app.Nodes.Phone.QueueRedraw();
                return;
            case "inspector": // v0.21.52 cz. b: Odznaki > Inspektor (poziomy z nagrodami, od bieżącego)
                Flow.Profile.Open(0, true);
                if (_app.Nodes.Phone.Current is Phone.ProfileTabs.BadgesTab ib) ib.Page = Phone.ProfileTabs.BadgesTab.InspectorPage;
                _app.Nodes.Phone.QueueRedraw();
                return;
            case "looks": // v0.21.52: Wygląd – kolor kasku (wiersz za modyfikatorami inwestora)
                s.ClassId = 1;
                s.Profile.Wins = Math.Max(1, s.Profile.Wins);
                Flow.ClassSelect.Open();
                Flow.Investor.Open(true);
                Flow.Investor.Page.Sel = s.Data.Investor.Length + (Secrets.CosmeticUnlocked(s.Data, s.Profile, s.Data.CosmeticStripes) ? 1 : 0);
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
        var newClass = scene switch
        {
            "class-dekarz" => AbilityEffect.Line,
            "class-tynkarz" => AbilityEffect.Splash,
            "class-operator" => AbilityEffect.Ram,
            _ => AbilityEffect.Stun,
        };
        s.ClassId = newClass != AbilityEffect.Stun ? Array.FindIndex(s.Data.Classes, c => c.Ability == newClass)
                  : scene switch
                  {
                      "game" => 0, "perks" => 1, "aim" => 2, "behaviors" => 1, "combo-shock" => 3, "combo-crack" => 1, "elite-card" => 1,
                      "elite-map" => 1, "boon-pick" => 1, "boon-synergy" => 1, "boon-phone" => 1, "boon-synergies" => 1,
                      _ => 5,
                  }; // domyślnie Glazurnik
        _app.StartRun();
        Flow.StageCard.Advance(); // karta etapu -> gra
        if (scene.StartsWith("event-") || scene.StartsWith("upgrade-") || scene.StartsWith("secret-") || scene is "tool-swap" or "tasks-extras")
        {
            await ExtrasScene(scene);
            return;
        }
        if (newClass != AbilityEffect.Stun) // nowy zawód: problemy pod moc i moc (efekt w trakcie)
        {
            _app.Nodes.Banners.Clear();
            _stage.NewClassShowcase(newClass);
            var acted = Screens.Play.PlayCommands.UseAbility(g, _app.Nodes.World);
            _app.AfterAction(acted);
            await DebugRunner.Frames(_app.Root, 6);
            return;
        }
        if (scene is "behaviors" or "act-mud" or "act-gust" or "act-dust" or "stats-phone")
        {
            _app.Nodes.Banners.Clear();
            if (scene == "behaviors") _stage.BehaviorShowcase();
            else if (scene == "stats-phone")
            {
                g.Equip(1, 2, Array.FindIndex(s.Data.GearTraits, t => t.Effect == TraitEffect.Str));
                Flow.Stats.OpenInRun();
            }
            else _stage.ActShowcase(s.Data.PreludeStages + (scene == "act-mud" ? 1 : scene == "act-gust" ? 5 : 8));
            await DebugRunner.Frames(_app.Root, 4);
            return;
        }
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
        if (scene.StartsWith("boon-"))
        {
            await BoonScene(scene);
            return;
        }
        if (scene is "schedule" or "schedule-tip" or "hurtownia" or "endmsg" or "end" or "boss" or "schedule-path" or "hurtownia-mats" or "house")
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

    /// <summary>v0.21.50 cz. 3: wydarzenia z wyborem, ulepszanie narzędzia, magazyn.</summary>
    private async Task ExtrasScene(string scene)
    {
        var g = _app.Session.Game;
        var banners = _app.Nodes.Banners;
        banners.Clear();
        switch (scene)
        {
            case "event-map":
            case "event-sms":
            case "event-choices":
            case "event-result":
            case "event-boon":
            case "tasks-extras":
                _stage.EventTiles(scene == "event-boon" ? "projekt" : "dostawca", "ostrzalka");
                if (scene == "event-map") break;
                _app.AfterAction(g.PlayerMove(1, 0));
                if (scene == "event-sms") break;
                Flow.Event.Advance();
                if (scene == "event-choices") break;
                Flow.Event.Advance();
                if (scene == "event-result") break;
                Flow.Event.Advance(); // wynik -> plac (premia z projektu albo zakładka Zadania)
                if (scene == "tasks-extras")
                {
                    banners.Clear();
                    Flow.Phone.Open(0, true);
                }
                break;
            case "upgrade-shop":
                g.WeaponLvl = 1;
                g.Cash = 200;
                g.Mats[1] = 9;
                g.ActCleared = true;
                Flow.Hurtownia.Open(true);
                break;
            case "upgrade-trait":
                g.WeaponLvl = 2;
                g.TraitPending = true;
                Flow.Trait.Open(null, true);
                break;
            case "upgrade-gear":
                g.WeaponLvl = 2;
                g.WeaponTrait = 0;
                _app.Refresh();
                Flow.Phone.Open(3, true);
                break;
            case "tool-swap":
                _stage.EventTiles();
                g.WeaponLvl = 2;
                g.WeaponTrait = 0;
                g.Pickups[g.PickupsCount++] = new Pickup(g.Hero.X + 1, g.Hero.Y, PickupType.Tool, true,
                    Array.FindIndex(g.D.Tools, t => g.D.Weapons[t.Weapon].Id == "udarowy"));
                _app.AfterAction(g.PlayerMove(1, 0));
                break;
            case "secret-crack":
            case "secret-card":
            case "secret-open":
                _stage.SecretStage(0);
                g.Keys = 1;
                if (scene == "secret-card")
                {
                    int kx = g.Hero.X - 1, ky = g.Hero.Y;
                    foreach (var (dx, dy) in new[] { (-1, 0), (0, -1), (0, 1), (-1, -1), (-1, 1), (-2, 0) })
                    {
                        if (g.Lv.At(g.Hero.X + dx, g.Hero.Y + dy) != Tile.Floor || g.Occupied(g.Hero.X + dx, g.Hero.Y + dy)) continue;
                        kx = g.Hero.X + dx;
                        ky = g.Hero.Y + dy;
                        break;
                    }
                    g.Spawn(g.D.EnemyIndex("kornik"), kx, ky);
                    g.Enemies[g.EnemiesCount - 1].Stun = 90;
                    g.Enemies[g.EnemiesCount - 1].Awake = true;
                    g.KeyHolder = (sbyte)(g.EnemiesCount - 1);
                    g.UpdateFov();
                    _app.Nodes.World.Sync();
                    _app.Refresh();
                    _app.Nodes.Touch.Bar.Pressed = Touch.BarButton.Wait;
                    Flow.Game.HandleInput(InputCmd.Of(GameAction.B));
                    Flow.Game.Look.Reveal();
                }
                if (scene == "secret-open")
                {
                    for (var k = 0; k < 4 && Flow.Current == Flow.Game; k++) _app.AfterAction(g.PlayerMove(1, 0));
                }
                break;
            case "secret-door":
            case "secret-map":
                _stage.SecretStage(1);
                _app.AfterAction(g.PlayerMove(1, 0)); // bez klucza: podpowiedź w dzienniku, bez tury
                if (scene == "secret-map") Flow.Game.ToggleOverview();
                break;
        }
        await DebugRunner.Frames(_app.Root, 4);
    }

    private void Showcase(string scene)
    {
        var g = _app.Session.Game;
        var banners = _app.Nodes.Banners;
        switch (scene)
        {
            case "materials": // HUD z materiałami (ikony i liczby w drugim rzędzie)
                banners.Clear();
                g.Mats[0] = 2;
                g.Mats[1] = 1;
                g.Mats[2] = 4;
                _app.Refresh();
                break;
            case "repairs": // Brygada i naprawy: drewno i stal, zaznaczone Załataj
                banners.Clear();
                g.Cash = 30;
                g.Mats[1] = 2;
                g.Mats[2] = 3;
                Flow.Brigade.Open(true);
                Flow.Brigade.Page.Sel = g.D.Brigade.Length;
                break;
            case "levelup": // wyraźny awans: poświata, gwiazdki, napis nad bohaterem
                banners.Clear();
                g.GainXp(g.XpToNext());
                _app.Refresh();
                break;
            case "death": // budowa wstrzymana: SMS z tym, co zostaje (dośw., rekord, zlecenie, najbliższy zakup)
                banners.Clear();
                g.Hero.Hp = 0;
                g.Hero.Alive = false;
                g.St = GameStatus.Dead;
                _app.AfterAction(true);
                break;
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
                g.Mats[0] = 2;
                g.Mats[2] = 4;
                g.Equip(0, 1, 1);
                g.Equip(2, 2, 3);
                Flow.Phone.Open(3, true);
                break;
            case "phone-costs":
                Flow.Phone.Open(4, true);
                break;
            case "gear5": // telefon: Sprzęt z butami i pasem (nagrody za odbiór)
                g.Bonus.GearSlots = (1 << g.D.GearSlotsCount) - 1;
                g.Mats[0] = 2;
                g.Mats[2] = 4;
                g.Equip(0, 1, 1);
                g.Equip(2, 2, 3);
                g.Equip(3, 2, Array.FindIndex(g.D.GearTraits, t => t.Effect == TraitEffect.SlipRes));
                g.Equip(4, 1, 0);
                Flow.Phone.Open(3, true);
                break;
            case "respect-banner": // etap zaliczony: baner Respektu (jak po schodach)
                banners.Clear();
                _app.Session.Events.RaiseStageCleared();
                _app.Session.Events.RaiseRespectGained(g.StageRespect(), _app.Session.Profile.Respect + g.StageRespect());
                break;
            case "second-chance": // Druga szansa z Respektu: 1 HP zamiast końca budowy
                banners.Clear();
                g.Bonus.SecondChance = 1;
                g.Hero.Hp = 0;
                g.HeroDown();
                _app.AfterAction(true);
                break;
            case "dmg-gear": // Sprzęt: narzędzie z zakresem i krytem, przedmioty z tym, co dają
            case "dmg-phone": // rozpiska obrażeń w trakcie budowy (Sprzęt > I): cios
            case "dmg-crit":  // ... i druga strona: kryt i obrona
                banners.Clear();
                DamageStage(g);
                _app.Session.ResetWatch();   // bez banerów awansu i sprzętu z przygotowania sceny
                _app.Refresh();
                if (scene == "dmg-gear") Flow.Phone.Open(Phone.PhoneTabs.Gear, true);
                else Flow.Stats.OpenInRun(Phone.Pages.StatsPage.DamagePage + (scene == "dmg-crit" ? 1 : 0), Phone.PhoneTabs.Gear);
                break;
            case "dmg-offer": // paczka: markowe rękawice z SIŁ+1 zamiast wzmacnianych z Kryt+5% - porównanie ciosu i krytu
                banners.Clear();
                DamageStage(g);
                _app.Session.ResetWatch();   // bez banerów awansu i sprzętu z przygotowania sceny
                _app.Refresh();
                g.Pickups[0] = new Pickup(g.Hero.X, g.Hero.Y, PickupType.GearBox, true, 1 * 3 + 2, Array.FindIndex(g.D.GearTraits, t => t.Effect == TraitEffect.Str));
                g.Collect();
                _app.AfterAction(true);
                break;
            case "dmg-tool": // skrzynka z Młotem udarowym: baner „teraz -> po zmianie”
                banners.Clear();
                DamageStage(g);
                _app.Session.ResetWatch();   // bez banerów awansu i sprzętu z przygotowania sceny
                _app.Refresh();
                g.Pickups[0] = new Pickup(g.Hero.X, g.Hero.Y, PickupType.Tool, true, Array.FindIndex(g.D.Tools, t => g.D.Weapons[t.Weapon].Id == "udarowy"), 0);
                g.Collect();
                _app.AfterAction(true);
                break;
            case "dmg-enemy": // karta problemu: obrażenia w obie strony
                banners.Clear();
                DamageStage(g);
                _app.Session.ResetWatch();   // bez banerów awansu i sprzętu z przygotowania sceny
                _app.Refresh();
                _app.Refresh();
                _app.Nodes.Touch.Bar.Pressed = Touch.BarButton.Wait;
                Flow.Game.HandleInput(InputCmd.Of(GameAction.B));
                Flow.Game.Look.Reveal();
                break;
            case "elite-map": // elity: złote ramki, poświata, przedrostki
            case "elite-card": // karta elity: „Zbrojony Przeciek”, OBR z Tarczą, cecha
                banners.Clear();
                _stage.EliteShowcase();
                _app.Session.ResetWatch();
                if (scene == "elite-card")
                {
                    _app.Nodes.Touch.Bar.Pressed = Touch.BarButton.Wait;
                    Flow.Game.HandleInput(InputCmd.Of(GameAction.B));
                    Flow.Game.Look.Reveal();
                }
                break;
            case "combo-shock": // mokry + prąd: Elektryk w mokry Przeciek, porażenie mokrego obok
            case "combo-dust":  // pył + iskra: Szlifierka w zapylonego, wybuch pyłu
            case "combo-crack": // zamróz + uderzenie: Kielnia w zmrożonego, pęknięcie
                banners.Clear();
                _app.Session.ResetWatch();
                _stage.ComboShowcase(scene == "combo-shock" ? 0 : scene == "combo-dust" ? 1 : 2);
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

    /// <summary>
    /// Sceny premii (v0.21.50 cz. 2): boon-pick – oferta z każdą rzadkością (jedna włącza synergię), boon-synergy – baner
    /// synergii po wyborze, boon-phone / boon-synergies – telefon > Sprzęt > Premie (lista i synergie).
    /// </summary>
    private async Task BoonScene(string scene)
    {
        var g = _app.Session.Game;
        var d = g.D;
        int Idx(string id) => Array.FindIndex(d.Boons, b => b.Id == id); // v0.21.53 cz. 2: po identyfikatorze (nazwy zależą od języka)
        void Give(string n)
        {
            g.BoonOffer[0] = (sbyte)Idx(n);
            g.PickBoon(0);
        }
        _app.Nodes.Banners.Clear();
        if (scene is "boon-phone" or "boon-synergies")
        {
            foreach (var n in new[] { "beton_b30", "kielnia_hart", "waz", "przedluzacz", "koniczyna", "mlot_mistrza", "espresso" }) Give(n);
            _app.Session.ResetWatch();
            _app.Refresh();
            Flow.BoonList.Open(scene == "boon-synergies" ? 1 : 0, true);
            return;
        }
        Give("waz");
        g.DebugSkip();
        _app.Session.ResetWatch();
        _app.AfterAction(true);
        await DebugRunner.Frames(_app.Root, 1);
        if (Flow.Current != Flow.Boons) return;
        g.BoonOffer[0] = (sbyte)Idx("przedluzacz");   // zwykła: włączy Przepięcie (woda + prąd)
        g.BoonOffer[1] = (sbyte)Idx("instrukcja"); // rzadka
        g.BoonOffer[2] = (sbyte)Idx("mlot_mistrza");   // legendarna
        _app.Nodes.Banners.Clear();
        if (scene == "boon-synergy")
        {
            Flow.Boons.Page.Sel = 0;
            Flow.Boons.Pick();
            return;
        }
        Flow.Boons.Page.Sel = 2;
        _app.Nodes.Phone.QueueRedraw();
    }

    /// <summary>Rozpiska obrażeń (sceny dmg-*): poziom 5, projekt wykonawczy, kask ze Szczęściem, rękawice z Kryt +5%.</summary>
    private static void DamageStage(LifeLike.Core.Game g)
    {
        while (g.HeroLevel < 5) g.GainXp(10);
        g.DmgBonus++;
        g.Equip(0, 0, Array.FindIndex(g.D.GearTraits, t => t.Effect == TraitEffect.Luck));
        g.Equip(1, 1, Array.FindIndex(g.D.GearTraits, t => t.Effect == TraitEffect.Crit));
    }

    /// <summary>Przewija etapy skrótem DebugSkip (L+R+SELECT na GBA) do sceny: harmonogram, Hurtownia, boss, koniec.</summary>
    private async Task SkipStages(string scene)
    {
        var g = _app.Session.Game;
        var d = _app.Session.Data;
        for (var guard = 0; guard < 40; guard++)
        {
            if (scene == "boss" && g.SDef().Boss >= 0 && Flow.Current == Flow.Game)
            {
                _stage.Boss();
                return;
            }
            g.DebugSkip();
            _app.AfterAction(true);
            await DebugRunner.Frames(_app.Root, 1);
            if (Flow.Current == Flow.Boons) Flow.Boons.Pick(); // premia po etapie: pierwsza z oferty
            if (Flow.Current == Flow.Schedule && scene == "schedule") return;
            if (Flow.Current == Flow.Schedule && scene == "schedule-path" && g.Stage >= 1)
            {
                Flow.Schedule.Page.Sel = 1;
                _app.Nodes.Phone.QueueRedraw();
                return;
            }
            if (Flow.Current == Flow.Schedule && scene == "schedule-tip" && g.Stage >= 1) return; // druga rada, notatka z odznakami
            if (Flow.Current == Flow.EndMessage)
            {
                if (scene == "end") Flow.End.Open();
                if (scene == "house")
                {
                    _app.Session.FixedToday = Tuple.Create(2026, 9, 25);
                    Flow.HouseSchedule.OpenBrowser = false;
                    Flow.HouseSchedule.Open(true);
                }
                return;
            }
            if (Flow.Current == Flow.Schedule && g.ActCleared && scene is "hurtownia" or "hurtownia-mats")
            {
                Flow.Schedule.Advance();
                if (scene == "hurtownia-mats")
                {
                    g.Mats[0] = 4;
                    g.Mats[1] = 5;
                    g.Mats[2] = 1;
                    Flow.Hurtownia.Page.Sel = Array.FindIndex(g.D.Hurtownia, it => it.Material >= 0);
                }
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
            if (Flow.Current == Flow.Boons) Flow.Boons.Pick();
            else if (Flow.Current == Flow.Event) Flow.Event.Advance();
            else if (Flow.Current == Flow.Trait) Flow.Trait.Pick();
            else if (Flow.Current == Flow.ToolOffer) Flow.ToolOffer.Decide(false);
            else if (Flow.Current == Flow.Schedule) Flow.Schedule.Advance();
            else if (Flow.Current == Flow.StageCard) Flow.StageCard.Advance();
            else if (Flow.Current == Flow.Hurtownia) Flow.Hurtownia.Advance();
            else return;
        }
    }
}
