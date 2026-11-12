using LifeLike.Game.Audio;
using LifeLike.Game.Input;
using LifeLike.Game.Phone;
using LifeLike.Game.Phone.Pages;

namespace LifeLike.Game.Screens;

/// <summary>
/// Paczka sprzętu przy zajętym slocie (gear_offer_dialog na GBA). v0.23.2: trzy wybory – Załóż (stary do plecaka),
/// Do plecaka, Zostaw; strzałki / dotknięcie wybierają, A / Enter zatwierdza, B / Esc = Zostaw.
/// </summary>
public sealed class OfferScreen : Screen
{
    public OfferScreen(App app) : base(app)
    {
    }

    public override bool InRun => true;
    public override bool UsesPhone => true;

    public OfferPage Page { get; private set; }

    public void Open(bool instant = false) => Flow.Go(this, instant);

    public override void Enter(bool instant)
    {
        Page = new OfferPage(S.Game) { Decided = Decide };
        N.Phone.OpenSingle(Page, PhoneTabs.Gear, instant);
        Sfx.Play("notify", 0.7f);
    }

    public override bool HandleInput(InputCmd e)
    {
        if (N.Phone.HandleInput(e)) return true;
        if (e.IsConfirm) Decide(Page?.Sel ?? OfferPage.Take);
        else if (e.IsBack) Decide(OfferPage.Leave);
        else return false;
        return true;
    }

    /// <summary>Decyzja nie zużywa tury, ale mogła dać awans (doświadczenie) - stąd AfterAction.</summary>
    public void Decide(int choice)
    {
        var g = S.Game;
        switch (choice)
        {
            case OfferPage.Take:
                g.AcceptOffer();
                Sfx.Play("buy");
                break;
            case OfferPage.Store:
                if (!g.OfferToBackpack()) return; // pełny plecak – wybór wyszarzony
                Sfx.Play("pickup");
                break;
            default:
                g.DeclineOffer();
                Sfx.Play("menu");
                break;
        }
        Flow.Game.Open();
        App.AfterAction(true);
    }

    /// <summary>Zgodność (test dymny, małpa): true = Załóż, false = Zostaw.</summary>
    public void Decide(bool accept) => Decide(accept ? OfferPage.Take : OfferPage.Leave);
}
