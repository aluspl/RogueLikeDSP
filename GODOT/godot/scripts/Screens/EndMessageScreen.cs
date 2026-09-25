using LifeLike.Core;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;
using LifeLike.Game.Phone.Pages;

namespace LifeLike.Game.Screens;

/// <summary>Koniec budowy: SMS z odbioru / przerwania (phone_message na GBA), potem plansza końcowa.</summary>
public sealed class EndMessageScreen : Screen
{
    public EndMessageScreen(App app) : base(app)
    {
    }

    public override bool InRun => true;
    public override bool UsesPhone => true;

    public void Open(bool instant = false) => Flow.Go(this, instant);

    public override void Enter(bool instant)
    {
        var won = S.Game.St == GameStatus.Won;
        var page = new MessagePage(won ? "Odbiór" : "Budowa");
        page.Add(won ? S.Data.StoryWin : S.Data.StoryLose);
        page.Info($"Wynik {S.Game.Score}  Dośw. +{S.LastGained}", Ink.Dark);
        page.Info(won ? "Dom na Osiedlu!" : "Doświadczenie zostaje", won ? Ink.Done : Ink.Dim);
        N.Phone.OpenSingle(page, 2, instant);
    }

    public override bool HandleInput(InputCmd e)
    {
        if (!e.Is(GameAction.A | GameAction.Start)) return false;
        Flow.End.Open();
        return true;
    }
}
