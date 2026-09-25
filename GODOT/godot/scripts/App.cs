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
        Session = new GameSession(d, profile, persist, seed);
        Nodes = new SceneNodes(root, sound, d.Version);
        Nodes.World.Bind(Session.Game);
        Banners = new BannerFeed(Session, Nodes.Banners);
        SoundCues.Attach(Session.Events);
        Session.Events.LevelUp += (_, _) => Nodes.World.Effects.LevelUp();
        Session.Events.RunEnded += won =>
        {
            if (won) Nodes.World.Effects.Confetti();
        };
        Flow = new ScreenFlow(this);
    }

    /// <summary>Węzeł główny (Main) - dla zadań czekających na klatki (zrzuty, test dymny).</summary>
    public Node Root { get; }
    public GameSession Session { get; }
    public SceneNodes Nodes { get; }
    public ScreenFlow Flow { get; }
    public BannerFeed Banners { get; }

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
        if (!acted) return;
        switch (Session.Resolve())
        {
            case TurnOutcome.StageCleared:
                Flow.Schedule.Open();
                break;
            case TurnOutcome.RunEnded:
                Flow.EndMessage.Open();
                break;
            case TurnOutcome.Offer when Flow.Current == Flow.Game:
                Flow.Offer.Open();
                break;
        }
    }

    /// <summary>Nowa budowa wybranym zawodem: przy pierwszej prolog (wjazd na plac), potem karta etapu z SMS-em.</summary>
    public void StartRun()
    {
        Session.StartRun();
        Refresh();
        if (!Session.Profile.HasFlag(Profile.FlagPrologueSeen)) Flow.Prologue.Open();
        else Flow.StageCard.Open();
    }

    /// <summary>Kolejny etap (harmonogram / Hurtownia -> karta etapu).</summary>
    public void NextStage()
    {
        Session.NextStage();
        Refresh();
        Flow.StageCard.Open();
    }
}
