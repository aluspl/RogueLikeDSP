using LifeLike.Game.Audio;
using LifeLike.Game.Input;
using LifeLike.Game.Phone;
using LifeLike.Game.Phone.Pages;

namespace LifeLike.Game.Screens;

/// <summary>Nowe narzędzie przy ulepszonym (v0.21.50 cz. 3): A zamieniam (ulepszenie przepada), B zostaję.</summary>
public sealed class ToolOfferScreen : Screen
{
    public ToolOfferScreen(App app) : base(app)
    {
    }

    public override bool InRun => true;
    public override bool UsesPhone => true;

    public void Open(bool instant = false) => Flow.Go(this, instant);

    public override void Enter(bool instant)
    {
        N.Phone.OpenSingle(new ToolOfferPage(S.Game), PhoneTabs.Gear, instant);
        Sfx.Play("notify", 0.7f);
    }

    public override bool HandleInput(InputCmd e)
    {
        if (e.Is(GameAction.A)) Decide(true);
        else if (e.Is(GameAction.B | GameAction.Cancel)) Decide(false);
        else return false;
        return true;
    }

    public void Decide(bool accept)
    {
        if (accept)
        {
            S.Game.AcceptTool();
            Sfx.Play("buy");
        }
        else
        {
            S.Game.DeclineTool();
            Sfx.Play("menu");
        }
        Flow.Game.Open();
        App.AfterAction(true);
    }
}
