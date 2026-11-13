using LifeLike.Game.Input;
using LifeLike.Game.Phone;
using LifeLike.Game.Phone.Pages;
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
    private Screen _back;

    public PhoneScreen(App app) : base(app)
    {
    }

    public override bool InRun => true;
    public override bool UsesPhone => true;
    public override bool ShowsTarget => true;

    /// <summary>Otwórz na zakładce (-1 = ostatnio otwarta); back – ekran po zamknięciu (v0.23.2: pauza), null = plac budowy.</summary>
    public void Open(int tab = -1, bool instant = false, Screen back = null)
    {
        _back = back;
        Return(tab, instant);
    }

    /// <summary>Powrót z ekranu otwartego z telefonu (rozpiska, premie) – ten sam ekran po zamknięciu co wcześniej.</summary>
    public void Return(int tab = -1, bool instant = false)
    {
        _tab = tab;
        Flow.Game.Menu.Close();
        N.Banners.Clear(); // telefon zasłania mapę - banery znikają jak na GBA (banner.hide)
        Flow.Go(this, instant);
    }

    public override void Enter(bool instant)
    {
        var g = S.Game;
        PhonePage[] tabs = [new TasksTab(g), new IssuesTab(g), new HomeTab(g, S.Profile, () => Flow.Stats.OpenInRun()), new GearTab(g, () => Flow.Brigade.Open(), () => Flow.Stats.OpenInRun(StatsPage.DamagePage, PhoneTabs.Gear), () => Flow.BoonList.Open(), () => Flow.Backpack.Open(this)), new CostsTab(g, S.Profile)];
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
        if (_back is not null && _back != Flow.Game)
        {
            Flow.Go(_back);
            return true;
        }
        Flow.Game.Open();
        App.Refresh();
        return true;
    }
}
