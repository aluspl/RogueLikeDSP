using LifeLike.Game.Input;
using LifeLike.Game.Phone.Pages;

namespace LifeLike.Game.Screens;

/// <summary>Harmonogram po etapie (run_schedule na GBA); po zaliczonym akcie - Hurtownia, inaczej kolejny etap.</summary>
public sealed class ScheduleScreen : Screen
{
    public ScheduleScreen(App app) : base(app)
    {
    }

    public override bool InRun => true;
    public override bool UsesPhone => true;

    public void Open(bool instant = false) => Flow.Go(this, instant);

    public SchedulePage Page { get; private set; }

    public override void Enter(bool instant)
    {
        Page = new SchedulePage(S.Game, S.Note, ButtonNames.Localize(S.Tip));
        N.Phone.OpenSingle(Page, 0, instant);
    }

    public override bool HandleInput(InputCmd e)
    {
        if (N.Phone.HandleInput(e)) return true;
        if (!e.Is(GameAction.A | GameAction.Start)) return false;
        Advance();
        return true;
    }

    /// <summary>Dalej wybraną ścieżką: wybór zostaje w grze (Game.NextPath) także przez Hurtownię.</summary>
    public void Advance()
    {
        if (Page is not null && Page.HasChoice) S.Game.ChoosePath(Page.Sel);
        if (S.Game.ActCleared && !S.Game.ShopClosed) // tryb inwestora: Hurtownia zamknięta
        {
            S.Note = "";
            Flow.Hurtownia.Open();
            return;
        }
        App.NextStage();
    }
}
