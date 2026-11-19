using System;
using Godot;
using LifeLike.Core;
using LifeLike.Core.Data;
using LifeLike.Game.Audio;
using LifeLike.Game.Hud;
using LifeLike.Game.Screens;
using LifeLike.Game.Session;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game;

/// <summary>
/// Kompozycja gry: sesja (LifeLike.Core + profil), węzły prezentacji, ekrany i obserwatorzy zdarzeń
/// (banery, dźwięki, efekty mapy). Wspólne kroki ekranów: odświeżenie widoku po zmianie stanu i przejścia po turze.
/// </summary>
public sealed class App
{
    public App(Node root, GameData d, Profile profile, bool persist, bool sound, uint seed)
    {
        Root = root;
        Session = new GameSession(d, profile, persist, seed) { Tips = GodotDataSource.LoadTips() };
        Nodes = new SceneNodes(root, sound, d.Version);
        Nodes.World.Bind(Session.Game);
        Nodes.World.ProfileSource = () => Session.Profile;
        Gfx.ScreenFilter.Changed += () => Nodes.Phone.QueueRedraw(); // wzory (litery rzadkości) zależą od filtra
        Nodes.Filter.Source = () => (Session.Data, Session.Profile);
        Nodes.Touch.Bind(Session.Game);
        Banners = new BannerFeed(Session, Nodes.Banners);
        Announcements = new AnnounceFeed(Session, Nodes.Announce);
        SoundCues.Attach(Session.Events);
        Session.Events.LevelUp += (level, abilityUp) => Nodes.World.Effects.LevelUp(level, abilityUp);
        Session.Events.SecondChance += () => Nodes.World.Effects.SecondChance();
        Session.Events.BossPhase += _ => Nodes.World.Effects.BossPhase();
        Session.Events.DogPetted += _ => ShowDogQuip(); // v0.23.0: easter egg – Kawka: ocena postępów w dymku nad nią
        Session.Events.RunStarted += Nodes.DogBubble.HideBubble;
        Nodes.DogBubble.Allowed = () => !Nodes.Phone.IsOpen && Nodes.World.IsVisibleInTree();
        Session.Events.RunEnded += won =>
        {
            if (won) Nodes.World.Effects.Confetti();
        };
        Flow = new ScreenFlow(this);
        Coach = new Guide.Coach(this);
    }

    /// <summary>v0.23.0: Kawka pogłaskana – zadowolona mina na mapie i dymek z oceną postępów (Dog.Quip: stan budowy i profilu).</summary>
    public void ShowDogQuip()
    {
        var d = Session.Data;
        if (d.DogQuips.Length == 0) return;
        var q = d.DogQuips[Dog.Quip(d, Session.Profile, Session.Game)];
        Nodes.World.DogPetted();
        Nodes.DogBubble.Show(d.DogName, q.Lines);
    }

    /// <summary>Węzeł główny (Main) - dla zadań czekających na klatki (zrzuty, test dymny).</summary>
    public Node Root { get; }
    public GameSession Session { get; }
    public SceneNodes Nodes { get; }
    public ScreenFlow Flow { get; }
    public BannerFeed Banners { get; }
    /// <summary>v0.23.0: ogłoszenia „z przytupem”.</summary>
    public AnnounceFeed Announcements { get; }
    /// <summary>Samouczek menu (#25): dymki na tytule i wyborze zawodu.</summary>
    public Guide.Coach Coach { get; }

    /// <summary>
    /// Po każdej zmianie stanu gry: znacznik celu, widok mapy (efekty trafień), zdarzenia tury, HUD, telefon.
    /// </summary>
    public void Refresh()
    {
        var g = Session.Game;
        Span<sbyte> t = stackalloc sbyte[CoreGame.MaxEnemies];
        var marks = Flow.Current is not null && Flow.Current.ShowsTarget;
        Nodes.World.Mark(marks && g.St == GameStatus.Playing && g.TargetsInRange(t) > 0 ? t[0] : -1);
        Nodes.World.Sync();
        Session.Watch();
        Nodes.Hud.ShowGame(g);
        if (Nodes.Phone.IsOpen) Nodes.Phone.QueueRedraw();
    }

    /// <summary>Po akcji gracza: odświeżenie, a gdy zużyła turę - przejścia jak main.cpp na GBA.</summary>
    public void AfterAction(bool acted)
    {
        Refresh();
        BackpackTip();
        if (!acted) return;
        if (OpenPending()) return;
        switch (Session.Resolve())
        {
            case TurnOutcome.StageCleared: // premia 1 z 3, potem harmonogram (i Hurtownia po akcie)
                if (Session.Game.HasBoonOffer) Flow.Boons.Open();
                else Flow.Schedule.Open();
                break;
            case TurnOutcome.RunEnded:
                // v0.22.0 (UX A): po wygranej harmonogram domu (z kodem QR), potem rozmowa z Kierownikiem Markiem
                if (Session.Game.St == GameStatus.Won && Session.Game.RouteCount() > 0) Flow.HouseSchedule.Open();
                else Flow.BetweenRuns.Open();
                break;
            case TurnOutcome.Offer when Flow.Current == Flow.Game:
                Flow.Offer.Open();
                break;
        }
    }

    /// <summary>v0.23.2: pierwszy przedmiot w plecaku – jednorazowy dymek Marka (bit Tutorial.Backpack w profilu).</summary>
    private void BackpackTip()
    {
        var p = Session.Profile;
        if (p is null || Session.Game.BackpackCount() == 0 || (p.Tutorial & Tutorial.Backpack) != 0) return;
        p.Tutorial |= Tutorial.Backpack;
        Session.Save();
        Nodes.Banners.Push(Loc.T("tut_plecak_tytul"), Loc.T("tut_plecak"), Phone.PhoneTabs.Gear);
    }

    /// <summary>
    /// Decyzje w trakcie etapu (v0.21.50 cz. 3): SMS wydarzenia, premia z projektu, cecha narzędzia, nowe narzędzie
    /// przy ulepszonym. true = otwarto ekran decyzji.
    /// </summary>
    public bool OpenPending()
    {
        var g = Session.Game;
        if (g.St != GameStatus.Playing) return false;
        if (g.PendingEvent >= 0) Flow.Event.Open();
        else if (g.HasBoonOffer) Flow.Boons.Open();
        else if (g.TraitPending) Flow.Trait.Open();
        else if (g.HasToolOffer) Flow.ToolOffer.Open();
        else return false;
        return true;
    }

    /// <summary>Nowa budowa wybranym zawodem: przy pierwszej prolog (wjazd na plac), potem karta etapu z SMS-em.</summary>
    public void StartRun()
    {
        Session.StartRun();
        Refresh();
        if (!Session.Profile.HasFlag(Profile.FlagPrologueSeen)) Flow.Prologue.Open();
        else Flow.StageCard.Open();
    }

    /// <summary>Codzienna budowa dnia `day` (bez prologu): karta etapu z SMS-em.</summary>
    public void StartDaily(int day)
    {
        Session.StartDaily(day);
        Refresh();
        Flow.StageCard.Open();
    }

    /// <summary>Wyzwanie tygodnia (bez prologu): karta etapu z SMS-em.</summary>
    public void StartWeekly(int week)
    {
        Session.StartWeekly(week);
        Refresh();
        Flow.StageCard.Open();
    }

    /// <summary>Kolejny etap (harmonogram / Hurtownia -> karta etapu).</summary>
    public void NextStage()
    {
        Session.NextStage();
        Refresh();
        Flow.StageCard.Open();
    }
}
