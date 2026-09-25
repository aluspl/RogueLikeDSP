using LifeLike.Core;
using LifeLike.Game.Audio;
using LifeLike.Game.Input;
using LifeLike.Game.Phone.Pages;

namespace LifeLike.Game.Screens;

/// <summary>
/// Brygada w telefonie (zakładka Zespół na GBA): wybór fachowca, wezwanie wraca na mapę, zużywa turę,
/// pokazuje baner i efekt. Otwierana ze Sprzętu w telefonie (A / przycisk) i z menu akcji (A bez kierunku).
/// </summary>
public sealed class BrigadeScreen : Screen
{
    public BrigadeScreen(App app) : base(app)
    {
    }

    public override bool InRun => true;
    public override bool UsesPhone => true;

    public BrigadePage Page { get; private set; }

    public void Open(bool instant = false)
    {
        Flow.Game.Menu.Close();
        N.Banners.Clear();
        Flow.Go(this, instant);
    }

    public override void Enter(bool instant)
    {
        Page = new BrigadePage(S.Game, Call);
        N.Phone.OpenSingle(Page, 3, instant);
    }

    public override bool HandleInput(InputCmd e)
    {
        if (N.Phone.HandleInput(e)) return true;
        if (!e.Is(GameAction.B | GameAction.Cancel | GameAction.Select | GameAction.Start)) return false;
        Flow.Game.Open();
        App.Refresh();
        return true;
    }

    /// <summary>Wezwanie fachowca h: zablokowany - dźwięk i zostaje w telefonie; inaczej mapa, tura, baner, efekt.</summary>
    public void Call(int h)
    {
        var g = S.Game;
        if (g.HelperBlocked(h) != HelperBlock.Ok)
        {
            Sfx.Play("hurt");
            N.Phone.QueueRedraw();
            return;
        }
        Flow.Game.Open();
        var acted = g.CallHelper(h);
        if (acted)
        {
            var hd = S.Data.Brigade[h];
            N.Banners.Push(hd.Name, hd.Desc);
            Sfx.Play("ability");
            N.World.Sync();
            N.World.Effects.Brigade(hd.Effect);
        }
        App.AfterAction(acted);
    }
}
