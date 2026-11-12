using LifeLike.Game.Audio;
using LifeLike.Game.Input;
using LifeLike.Game.Phone;
using LifeLike.Game.Phone.Pages;

namespace LifeLike.Game.Screens;

/// <summary>Nowe narzędzie przy ulepszonym (v0.21.50 cz. 3): wybór zaznaczony (domyślnie „Zostaję”), A / Enter zatwierdza
/// (zamiana: ulepszenie przepada), B / Esc zostaję (v0.21.51).</summary>
public sealed class ToolOfferScreen : Screen
{
    public ToolOfferScreen(App app) : base(app)
    {
    }

    public override bool InRun => true;
    public override bool UsesPhone => true;

    public ToolOfferPage Page { get; private set; }

    public void Open(bool instant = false) => Flow.Go(this, instant);

    public override void Enter(bool instant)
    {
        Page = new ToolOfferPage(S.Game) { Decided = Decide };
        N.Phone.OpenSingle(Page, PhoneTabs.Gear, instant);
        Sfx.Play("notify", 0.7f);
    }

    public override bool HandleInput(InputCmd e)
    {
        if (N.Phone.HandleInput(e)) return true;
        if (e.IsConfirm) Decide(Page?.Sel ?? OfferPage.Leave);
        else if (e.IsBack) Decide(OfferPage.Leave);
        else return false;
        return true;
    }

    /// <summary>v0.23.2: Weź (stara broń do plecaka), Do plecaka, Zostaw.</summary>
    public void Decide(int choice)
    {
        var g = S.Game;
        switch (choice)
        {
            case OfferPage.Take:
                g.AcceptTool();
                Sfx.Play("buy");
                break;
            case OfferPage.Store:
                if (!g.ToolToBackpack()) return;
                Sfx.Play("pickup");
                break;
            default:
                g.DeclineTool();
                Sfx.Play("menu");
                break;
        }
        Flow.Game.Open();
        App.AfterAction(true);
    }

    /// <summary>Zgodność (test dymny, małpa): true = Weź, false = Zostaw.</summary>
    public void Decide(bool accept) => Decide(accept ? OfferPage.Take : OfferPage.Leave);
}
