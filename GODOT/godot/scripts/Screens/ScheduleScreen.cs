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

    public override void Enter(bool instant) =>
        N.Phone.OpenSingle(new SchedulePage(S.Game, S.Note, ButtonNames.Localize(S.Tip)), 0, instant);

    public override bool HandleInput(InputCmd e)
    {
        if (!e.Is(GameAction.A | GameAction.Start)) return false;
        Advance();
        return true;
    }

    public void Advance()
    {
        if (S.Game.ActCleared)
        {
            S.Note = "";
            Flow.Hurtownia.Open();
            return;
        }
        App.NextStage();
    }
}
