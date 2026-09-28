using LifeLike.Game.Input;
using LifeLike.Game.Phone;
using LifeLike.Game.Phone.Pages;

namespace LifeLike.Game.Screens;

/// <summary>Telefon > Sprzęt > Premie (R / przycisk „Premie”): lista premii i synergii; powrót na zakładkę Sprzęt.</summary>
public sealed class BoonListScreen : Screen
{
    private int _mode;

    public BoonListScreen(App app) : base(app)
    {
    }

    public override bool InRun => true;
    public override bool UsesPhone => true;

    public BoonListPage Page { get; private set; }

    public void Open(int mode = 0, bool instant = false)
    {
        _mode = mode;
        Flow.Go(this, instant);
    }

    public override void Enter(bool instant)
    {
        Page = new BoonListPage(S.Game, _mode);
        N.Phone.OpenSingle(Page, PhoneTabs.Gear, instant);
    }

    public override bool HandleInput(InputCmd e)
    {
        if (N.Phone.HandleInput(e)) return true;
        if (!e.Is(GameAction.B | GameAction.Cancel | GameAction.Start | GameAction.Select | GameAction.R)) return false;
        Flow.Phone.Open(PhoneTabs.Gear);
        return true;
    }
}
