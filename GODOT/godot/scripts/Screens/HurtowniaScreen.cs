using LifeLike.Game.Audio;
using LifeLike.Game.Input;
using LifeLike.Game.Phone.Pages;

namespace LifeLike.Game.Screens;

/// <summary>Hurtownia między aktami (run_hurtownia na GBA): zakupy za budżet budowy, potem kolejny etap.</summary>
public sealed class HurtowniaScreen : Screen
{
    public HurtowniaScreen(App app) : base(app)
    {
    }

    public override bool InRun => true;
    public override bool UsesPhone => true;

    public HurtowniaPage Page { get; private set; }

    public void Open(bool instant = false) => Flow.Go(this, instant);

    public override void Enter(bool instant)
    {
        Page = new HurtowniaPage(S.Game);
        N.Phone.OpenSingle(Page, 4, instant);
        Sfx.Play("notify", 0.7f);
    }

    public override bool HandleInput(InputCmd e)
    {
        if (N.Phone.HandleInput(e))
        {
            N.Hud.ShowGame(S.Game);
            return true;
        }
        if (!e.Is(GameAction.Start | GameAction.B | GameAction.Cancel)) return false;
        Advance();
        return true;
    }

    public void Advance() => App.NextStage();
}
