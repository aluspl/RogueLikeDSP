using LifeLike.Core;
using LifeLike.Game.Input;
using LifeLike.Game.Phone;
using LifeLike.Game.Phone.ProfileTabs;

namespace LifeLike.Game.Screens;

/// <summary>Telefon profilu na tle tytułu (run_shop na GBA): Odznaki/Zlecenia/Pamiątki, Katalog, Osiedle, Zespół, Szkolenia.</summary>
public sealed class ProfileScreen : Screen
{
    private static string[] TabLabels => [Loc.T("odznaki_2"), Loc.T("katalog"), Loc.T("osiedle"), Loc.T("zespol"), Loc.T("szkolenia")]; // v0.22.0 (UX A): „Szkolenia” zamiast „Koszty”
    private int _tab;

    public ProfileScreen(App app) : base(app)
    {
    }

    public override ViewSet Views => ViewSet.Title;
    public override bool UsesPhone => true;
    public override string Music => "title";

    /// <summary>back = ekran, do którego wraca telefon (v0.22.0, UX A: rozmowa między budowami); null = tytuł.</summary>
    public void Open(int tab = 0, bool instant = false, Screen back = null)
    {
        _tab = tab;
        _back = back;
        if (back is null) S.Note = ""; // notatka końca budowy (sekrety, odznaki) zostaje dla rozmowy
        Flow.Go(this, instant);
    }

    private Screen _back;

    public override void Enter(bool instant)
    {
        Flow.Title.Populate();
        var d = S.Data;
        var p = S.Profile;
        if (S.RollTasks()) S.Save(); // v0.21.52 cz. c: zadania nowego dnia / tygodnia
        PhonePage[] tabs =
        [
            new BadgesTab(d, p) { Saved = S.Save, Today = S.TodayNumber },
            new CatalogTab(d, p),
            new EstateTab(d, p, S.Save),
            new TeamTab(d, p),
            new TrainingTab(d, p, S.Save),
        ];
        N.Phone.OpenTabs(tabs, TabLabels, _tab, instant);
    }

    public override bool HandleInput(InputCmd e)
    {
        if (N.Phone.HandleInput(e)) return true;
        if (!e.Is(GameAction.Profile | GameAction.B | GameAction.Cancel | GameAction.Select | GameAction.Shop | GameAction.Start)) return false;
        N.Phone.Close();
        if (_back is not null)
        {
            Flow.Go(_back);
            return true;
        }
        S.Note = "";
        Flow.Title.Open();
        return true;
    }
}
