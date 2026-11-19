using LifeLike.Game.Input;
using LifeLike.Game.Phone.Pages;

namespace LifeLike.Game.Screens;

/// <summary>Podsumowanie budowy (#33): „Szczegóły budowy” z rozmowy między budowami, powrót do niej.</summary>
public sealed class RecapScreen : Screen
{
    public RecapScreen(App app) : base(app)
    {
    }

    public override bool InRun => true;
    public override bool UsesPhone => true;

    public RecapPage Page { get; private set; }

    public void Open(bool instant = false) => Flow.Go(this, instant);

    public override void Enter(bool instant)
    {
        N.Banners.Clear();
        Page = new RecapPage(S);
        N.Phone.OpenSingle(Page, 2, instant);
    }

    public override bool HandleInput(InputCmd e)
    {
        if (N.Phone.HandleInput(e)) return true;
        if (!e.Is(GameAction.Start | GameAction.A | GameAction.B | GameAction.Cancel)) return false;
        Flow.BetweenRuns.Open(); // v0.22.0 (UX A): powrót do rozmowy
        return true;
    }
}
