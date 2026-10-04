using LifeLike.Game.Audio;
using LifeLike.Game.Input;
using LifeLike.Game.Phone;
using LifeLike.Game.Phone.Pages;

namespace LifeLike.Game.Screens;

/// <summary>
/// Wydarzenie z wyborem na polu etapu (v0.21.50 cz. 3, event_dialog na GBA): SMS -> odpowiedzi -> wynik, potem
/// z powrotem na plac (App.AfterAction: premia z projektu, cecha narzędzia, paczka sprzętu).
/// </summary>
public sealed class EventScreen : Screen
{
    public EventScreen(App app) : base(app)
    {
    }

    public override bool InRun => true;
    public override bool UsesPhone => true;

    public EventPage Page { get; private set; }

    public void Open(bool instant = false) => Flow.Go(this, instant);

    public override void Enter(bool instant)
    {
        Page = new EventPage(S.Game, S.Game.PendingEvent) { Next = Advance };
        N.Phone.OpenSingle(Page, PhoneTabs.Tasks, instant);
        Sfx.Play("notify", 0.7f);
    }

    public override bool HandleInput(InputCmd e)
    {
        if (N.Phone.HandleInput(e)) return true;
        if (!e.Is(GameAction.A | GameAction.Start)) return false;
        Advance();
        return true;
    }

    /// <summary>Kolejna faza: SMS -> odpowiedzi -> (odpowiedź) wynik -> plac.</summary>
    public void Advance()
    {
        var g = S.Game;
        if (Page.Phase == 0)
        {
            Page.Phase = 1;
            Sfx.Play("menu");
        }
        else if (Page.Phase == 1)
        {
            if (!g.ChooseEvent(Page.Sel)) return;
            Page.Phase = 2;
            Sfx.Play("buy");
            N.Hud.ShowGame(g);
        }
        else
        {
            Flow.Game.Open();
            App.AfterAction(true);
            return;
        }
        N.Phone.QueueRedraw();
    }
}
