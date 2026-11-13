using System;
using LifeLike.Core;
using LifeLike.Game.Input;
using LifeLike.Game.Phone;
using LifeLike.Game.Phone.Tabs;

namespace LifeLike.Game.Screens;

/// <summary>
/// Telefon w trakcie budowy – aplikacja Budowa (SELECT na GBA, Tab, przycisk Telefon, pauza): v0.23.2 (UI 2) zakładki
/// Zadania, Usterki, Koszty (Start i Sprzęt zastąpił ekran Postać – PhoneTabs.Start / Gear otwierają Postać), przycisk
/// Brygada (A / Spacja) na każdej zakładce. Poza czasem gry, pamięta ostatnią zakładkę (phone_tab na GBA).
/// </summary>
public sealed class PhoneScreen : Screen
{
    /// <summary>Zakładki aplikacji (logiczne PhoneTabs) w kolejności paska.</summary>
    private static readonly int[] Order = [PhoneTabs.Tasks, PhoneTabs.Issues, PhoneTabs.Costs];

    private int _lastTab;
    private int _tab = -1;
    private Screen _back;

    public PhoneScreen(App app) : base(app)
    {
    }

    public override bool InRun => true;
    public override bool UsesPhone => true;
    public override bool ShowsTarget => true;

    /// <summary>Otwórz na zakładce (PhoneTabs, -1 = ostatnio otwarta); back – ekran po zamknięciu (v0.23.2: pauza), null = plac budowy.
    /// Start / Sprzęt otwierają ekran Postać.</summary>
    public void Open(int tab = -1, bool instant = false, Screen back = null)
    {
        if (tab is PhoneTabs.Start or PhoneTabs.Gear)
        {
            Flow.Character.Open(back, instant);
            return;
        }
        _back = back;
        Return(tab, instant);
    }

    /// <summary>Powrót z ekranu otwartego z telefonu – ten sam ekran po zamknięciu co wcześniej (Start / Sprzęt: Postać).</summary>
    public void Return(int tab = -1, bool instant = false)
    {
        if (tab is PhoneTabs.Start or PhoneTabs.Gear)
        {
            Flow.Character.Return(instant);
            return;
        }
        _tab = tab;
        Flow.Game.Menu.Close();
        N.Banners.Clear(); // telefon zasłania mapę - banery znikają jak na GBA (banner.hide)
        Flow.Go(this, instant);
    }

    public override void Enter(bool instant)
    {
        var g = S.Game;
        PhonePage[] tabs = [new TasksTab(g), new IssuesTab(g), new CostsTab(g, S.Profile)];
        string[] labels = [PhoneTabs.Labels[PhoneTabs.Tasks], PhoneTabs.Labels[PhoneTabs.Issues], PhoneTabs.Labels[PhoneTabs.Costs]];
        N.Phone.OnTabChanged = i => _lastTab = i;
        var at = _tab >= 0 ? Math.Max(0, Array.IndexOf(Order, _tab)) : _lastTab;
        N.Phone.OpenTabs(tabs, labels, at, instant, Order, [new(Loc.T("brygada_3"), GameAction.A, PageRole.Primary)], Loc.T("budowa_hint"));
    }

    public override void Exit() => N.Phone.OnTabChanged = null;

    public override bool HandleInput(InputCmd e)
    {
        if (e.IsTap && N.Banners.Hit(e.Pointer)) // baner obok telefonu: jego zakładka
        {
            var tab = N.Banners.TabAt(e.Pointer);
            if (tab >= 0) Show(tab);
            return true;
        }
        if (N.Phone.HandleInput(e)) return true;
        if (e.Is(GameAction.A)) // v0.23.2: Brygada (dawniej z zakładki Sprzęt)
        {
            Flow.Brigade.Open();
            return true;
        }
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

    /// <summary>Zakładka logiczna (PhoneTabs) na otwartym telefonie; Start / Sprzęt – Postać.</summary>
    public void Show(int tab)
    {
        var i = Array.IndexOf(Order, tab);
        if (i >= 0) N.Phone.ShowTab(i);
        else Open(tab, false, _back);
    }
}
