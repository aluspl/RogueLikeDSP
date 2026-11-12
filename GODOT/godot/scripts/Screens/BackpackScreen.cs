using LifeLike.Game.Audio;
using LifeLike.Game.Input;
using LifeLike.Game.Phone;
using LifeLike.Game.Phone.Pages;

namespace LifeLike.Game.Screens;

/// <summary>
/// v0.23.2: plecak na placu (I, telefon → Sprzęt → Plecak, w pionie dotknięcie broni w HUD). Zamiana zużywa turę – ekran
/// wraca wtedy na plac; wyrzucenie bez tury (zostaje). B / Esc / I: z powrotem (telefon albo plac).
/// </summary>
public sealed class BackpackScreen : Screen
{
    private Screen _back;

    public BackpackScreen(App app) : base(app)
    {
    }

    public override bool InRun => true;
    public override bool UsesPhone => true;

    public BackpackPage Page { get; private set; }

    /// <summary>back: ekran po wyjściu bez zużytej tury (null = plac budowy).</summary>
    public void Open(Screen back = null, bool instant = false)
    {
        _back = back;
        Flow.Go(this, instant);
    }

    public override void Enter(bool instant)
    {
        var sel = Page?.Sel ?? 0;
        Page = new BackpackPage(S.Game) { Mode = BagMode.Bag, Done = Acted };
        Page.Sel = sel;
        N.Phone.OpenSingle(Page, PhoneTabs.Gear, instant);
        Sfx.Play("menu");
    }

    public override bool HandleInput(InputCmd e)
    {
        if (N.Phone.HandleInput(e))
        {
            N.Hud.ShowGame(S.Game);
            return true;
        }
        if (!e.IsBack && !e.Is(GameAction.Info)) return false;
        Close(false);
        return true;
    }

    private void Acted(bool turn)
    {
        App.Refresh();
        if (turn) Close(true);
    }

    public void Close(bool acted)
    {
        if (!acted && _back is PhoneScreen ph)
        {
            ph.Open(PhoneTabs.Gear);
            return;
        }
        Flow.Game.Open();
        App.AfterAction(acted);
    }
}
