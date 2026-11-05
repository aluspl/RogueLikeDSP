using LifeLike.Core;
using LifeLike.Game.Input;
using LifeLike.Game.Phone.Pages;

namespace LifeLike.Game.Screens;

/// <summary>
/// v0.22.0 (UX A): między budowami (po harmonogramie domu albo od razu po porażce) rozmowa z Kierownikiem Markiem
/// w telefonie na tle tytułu: zakup jednym stuknięciem, Szkolenia, kolejna budowa (NG+ po wygranej), szczegóły, menu.
/// Zastępuje SMS końca budowy i planszę końcową z v0.21. Start = kolejna budowa, K = Szkolenia, Esc = menu.
/// </summary>
public sealed class BetweenRunsScreen : Screen
{
    public BetweenRunsScreen(App app) : base(app)
    {
    }

    public override ViewSet Views => ViewSet.Title;
    public override bool UsesPhone => true;
    public override string Music => "title";

    public BetweenRunsPage Page { get; private set; }

    /// <summary>NG+ tylko po wygranej zwykłej budowy (bez budowy dnia i tygodnia).</summary>
    public bool CanContinue => S.Game.St == GameStatus.Won && !S.Game.Daily && S.Game.WeeklyWeek == 0;

    public void Open(bool instant = false) => Flow.Go(this, instant);

    public override void Enter(bool instant)
    {
        N.Banners.Clear(); // to, co pokazywały banery końca budowy, jest w rozmowie („Do tego:”)
        Page = new BetweenRunsPage(S, CanContinue) { OnReply = Reply };
        N.Phone.OpenSingle(Page, 2, instant);
    }

    public override bool HandleInput(InputCmd e)
    {
        if (N.Phone.HandleInput(e)) return true;
        if (e.Is(GameAction.Start)) Reply(QuickReply.Next);
        else if (e.Is(GameAction.Shop)) Reply(QuickReply.Training);
        else if (e.IsBack) Reply(QuickReply.Menu);
        else return false;
        return true;
    }

    public void Reply(QuickReply r)
    {
        switch (r)
        {
            case QuickReply.Buy:
                var d = S.Data;
                var p = S.Profile;
                if (Spend.QuickKind(d, p, out var k, out var i) < 0) return;
                var name = Recap.UnlockName(d, p, k, i);
                if (!Spend.QuickBuy(d, p)) return;
                S.Save();
                Page.Bought(name);
                break;
            case QuickReply.Training:
                Flow.Profile.Open(4);
                break;
            case QuickReply.Next:
                S.Note = "";
                if (Flow.Career.Available) Flow.Career.Open();
                else Flow.ClassSelect.Open();
                break;
            case QuickReply.NextPlus:
                if (!CanContinue) return;
                S.NewGamePlus();
                App.Refresh();
                Flow.StageCard.Open();
                break;
            case QuickReply.Details:
                Flow.Recap.Open();
                break;
            default:
                S.Note = "";
                Flow.Title.Open();
                break;
        }
    }
}
