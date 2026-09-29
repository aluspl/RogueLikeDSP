using LifeLike.Game.Audio;
using LifeLike.Game.Input;
using LifeLike.Game.Phone;
using LifeLike.Game.Phone.Pages;

namespace LifeLike.Game.Screens;

/// <summary>
/// Wybór cechy ulepszonego narzędzia (v0.21.50 cz. 3): po Hurtowni wraca do Hurtowni, po wydarzeniu na plac.
/// </summary>
public sealed class TraitScreen : Screen
{
    private Screen _back;

    public TraitScreen(App app) : base(app)
    {
    }

    public override bool InRun => true;
    public override bool UsesPhone => true;

    public TraitPage Page { get; private set; }

    /// <summary>back: ekran po wyborze (null = plac budowy).</summary>
    public void Open(Screen back = null, bool instant = false)
    {
        _back = back;
        Flow.Go(this, instant);
    }

    public override void Enter(bool instant)
    {
        Page = new TraitPage(S.Game) { Picked = Pick };
        N.Phone.OpenSingle(Page, PhoneTabs.Gear, instant);
        Sfx.Play("notify", 0.7f);
    }

    public override bool HandleInput(InputCmd e)
    {
        if (N.Phone.HandleInput(e)) return true;
        if (!e.Is(GameAction.A | GameAction.Start)) return false;
        Pick();
        return true;
    }

    public void Pick()
    {
        var g = S.Game;
        if (g.ChooseTrait(Page.Sel))
        {
            Sfx.Play("level", 0.8f);
            var td = S.Data.ToolTraits[g.WeaponTrait];
            N.Banners.Push("Cecha: " + td.Name, td.Desc, PhoneTabs.Gear);
        }
        App.Refresh();
        if (_back is HurtowniaScreen h) h.Open();
        else
        {
            Flow.Game.Open();
            App.AfterAction(true);
        }
    }
}
