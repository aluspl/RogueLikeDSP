using LifeLike.Game.Audio;
using LifeLike.Game.Input;
using LifeLike.Game.Phone.Pages;

namespace LifeLike.Game.Screens;

/// <summary>Paczka sprzętu przy zajętym slocie (gear_offer_dialog na GBA): A zakładam, B zostawiam.</summary>
public sealed class OfferScreen : Screen
{
    public OfferScreen(App app) : base(app)
    {
    }

    public override bool InRun => true;
    public override bool UsesPhone => true;

    public void Open(bool instant = false) => Flow.Go(this, instant);

    public override void Enter(bool instant)
    {
        N.Phone.OpenSingle(new OfferPage(S.Game), 3, instant);
        Sfx.Play("notify", 0.7f);
    }

    public override bool HandleInput(InputCmd e)
    {
        if (e.Is(GameAction.A)) Decide(true);
        else if (e.Is(GameAction.B | GameAction.Cancel)) Decide(false);
        else return false;
        return true;
    }

    /// <summary>Decyzja nie zużywa tury, ale mogła dać awans (doświadczenie) - stąd AfterAction.</summary>
    public void Decide(bool accept)
    {
        if (accept)
        {
            S.Game.AcceptOffer();
            Sfx.Play("buy");
        }
        else
        {
            S.Game.DeclineOffer();
            Sfx.Play("menu");
        }
        Flow.Game.Open();
        App.AfterAction(true);
    }
}
