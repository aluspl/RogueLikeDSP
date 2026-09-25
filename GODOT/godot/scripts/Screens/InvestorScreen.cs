using LifeLike.Core;
using LifeLike.Game.Input;
using LifeLike.Game.Phone.Pages;

namespace LifeLike.Game.Screens;

/// <summary>Tryb inwestora nad wyborem zawodu (SELECT na GBA, tu Tab / dotknięcie wiersza „Stawka”); Esc wraca.</summary>
public sealed class InvestorScreen : Screen
{
    public InvestorScreen(App app) : base(app)
    {
    }

    public override ViewSet Views => ViewSet.ClassSelect;
    public override bool UsesPhone => true;
    public override string Music => "title";

    public InvestorPage Page { get; private set; }

    public void Open(bool instant = false)
    {
        if (!Meta.InvestorUnlocked(S.Profile)) return;
        Flow.Go(this, instant);
    }

    public override void Enter(bool instant)
    {
        Page = new InvestorPage(S.Data, S.Profile, N.ClassSelectView.Selected, S.Save);
        N.Phone.OpenSingle(Page, 4, instant);
    }

    public override bool HandleInput(InputCmd e)
    {
        if (N.Phone.HandleInput(e)) return true;
        if (!e.Is(GameAction.B | GameAction.Cancel | GameAction.Select | GameAction.Start)) return false;
        N.Phone.Close();
        Flow.ClassSelect.Open();
        return true;
    }
}
