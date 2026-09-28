using LifeLike.Core;
using LifeLike.Game.Input;
using LifeLike.Game.Phone;
using LifeLike.Game.Phone.Pages;

namespace LifeLike.Game.Screens;

/// <summary>
/// Opis statystyk (#19) w telefonie: z wyboru zawodu (I / przycisk „Statystyki”, jak START na GBA) albo z zakładki
/// Start telefonu w trakcie budowy (Spacja / „Opis”). Powrót tam, skąd otwarto.
/// </summary>
public sealed class StatsScreen : Screen
{
    private bool _inRun;
    private int _cls;

    public StatsScreen(App app) : base(app)
    {
    }

    public override ViewSet Views => _inRun ? base.Views : ViewSet.ClassSelect;
    public override bool InRun => _inRun;
    public override bool UsesPhone => true;
    public override string Music => _inRun ? "game" : "title";

    /// <summary>Z wyboru zawodu: statystyki zawodu cls z premiami profilu.</summary>
    public void OpenClass(int cls)
    {
        _inRun = false;
        _cls = cls;
        Flow.Go(this);
    }

    /// <summary>Z telefonu w trakcie budowy (zakładka Start).</summary>
    public void OpenInRun()
    {
        _inRun = true;
        _cls = S.Game.Cls;
        Flow.Go(this);
    }

    public override void Enter(bool instant)
    {
        var page = _inRun ? new StatsPage(S.Data, _cls, S.Game.Bonus, S.Game) : new StatsPage(S.Data, _cls, Meta.Mods(S.Data, S.Profile), null);
        N.Phone.OpenSingle(page, PhoneTabs.Start, instant);
    }

    public override bool HandleInput(InputCmd e)
    {
        if (N.Phone.HandleInput(e)) return true;
        if (!e.Is(GameAction.B | GameAction.Cancel | GameAction.Start | GameAction.Select | GameAction.Info)) return false;
        if (_inRun) Flow.Phone.Open(PhoneTabs.Start);
        else Flow.ClassSelect.Open();
        return true;
    }
}
