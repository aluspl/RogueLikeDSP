using LifeLike.Game.Input;
using LifeLike.Game.Phone;
using LifeLike.Game.Phone.Tabs;

namespace LifeLike.Game.Screens;

/// <summary>
/// Telefon w trakcie budowy (SELECT na GBA): Zadania, Usterki, Start, Sprzęt, Koszty - poza czasem gry.
/// Pamięta ostatnią zakładkę (phone_tab na GBA).
/// </summary>
public sealed class PhoneScreen : Screen
{
    private int _lastTab = PhoneTabs.Start;
    private int _tab = -1;

    public PhoneScreen(App app) : base(app)
    {
    }

    public override bool InRun => true;
    public override bool UsesPhone => true;
    public override bool ShowsTarget => true;

    /// <summary>Otwórz na zakładce (-1 = ostatnio otwarta).</summary>
    public void Open(int tab = -1, bool instant = false)
    {
        _tab = tab;
        Flow.Game.Menu.Close();
        N.Banners.Clear(); // telefon zasłania mapę - banery znikają jak na GBA (banner.hide)
        Flow.Go(this, instant);
    }

    public override void Enter(bool instant)
    {
        var g = S.Game;
        PhonePage[] tabs = [new TasksTab(g), new IssuesTab(g), new HomeTab(g, S.Profile), new GearTab(g, () => Flow.Brigade.Open()), new CostsTab(g, S.Profile)];
        N.Phone.OnTabChanged = i => _lastTab = i;
        N.Phone.OpenTabs(tabs, PhoneTabs.Labels, _tab >= 0 ? _tab : _lastTab, instant);
    }

    public override void Exit() => N.Phone.OnTabChanged = null;

    public override bool HandleInput(InputCmd e)
    {
        if (e.IsTap && N.Banners.Hit(e.Pointer)) // baner obok telefonu: jego zakładka
        {
            var tab = N.Banners.TabAt(e.Pointer);
            if (tab >= 0) N.Phone.ShowTab(tab);
            return true;
        }
        if (N.Phone.HandleInput(e)) return true;
        if (!e.Is(GameAction.Select | GameAction.Cancel | GameAction.B | GameAction.Start)) return false;
        Flow.Game.Open();
        App.Refresh();
        return true;
    }
}
