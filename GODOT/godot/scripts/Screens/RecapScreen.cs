using LifeLike.Game.Input;
using LifeLike.Game.Phone.Pages;

namespace LifeLike.Game.Screens;

/// <summary>Podsumowanie budowy (#33) po SMS-ie końca budowy (i harmonogramie domu po wygranej), przed planszą końcową.</summary>
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
        Flow.End.Open();
        return true;
    }
}
