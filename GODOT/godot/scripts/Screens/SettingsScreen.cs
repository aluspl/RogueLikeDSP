using LifeLike.Core;
using LifeLike.Game.Input;
using LifeLike.Game.Phone.Pages;

namespace LifeLike.Game.Screens;

/// <summary>
/// Ustawienia (klucz w rogu tytułu i mapy, Esc): strona SettingsPage w telefonie nad ekranem, z którego je otwarto
/// (tytuł albo budowa - bez zużycia tury). Z budowy także Zapisz i wyjdź oraz Porzuć budowę. v0.21.53: Filtr ekranu
/// otwiera listę filtrów (FiltersPage), Esc z niej wraca do ustawień.
/// </summary>
public sealed class SettingsScreen : Screen
{
    private Screen _back;

    public SettingsScreen(App app) : base(app)
    {
    }

    public override ViewSet Views => _back?.Views ?? ViewSet.Title;
    public override bool InRun => _back?.InRun ?? false;
    public override bool UsesPhone => true;
    public override string Music => _back?.Music ?? "title";

    public SettingsPage Page { get; private set; }

    /// <summary>v0.21.53: lista filtrów ekranu (Ustawienia > Filtr ekranu); null = strona ustawień.</summary>
    public FiltersPage Filters { get; private set; }

    /// <summary>Otwórz nad ekranem back (powrót tam po zamknięciu).</summary>
    public void Open(Screen back, bool instant = false)
    {
        _back = back;
        Flow.Go(this, instant);
    }

    /// <summary>Powrót z „Jak grać” do ustawień.</summary>
    public void Reopen() => Flow.Go(this);

    public override void Enter(bool instant)
    {
        N.Banners.Clear();
        ShowSettings(instant);
    }

    private void ShowSettings(bool instant)
    {
        Filters = null;
        Page = new SettingsPage(InRun, S.Data.Version, S.Data, S.Profile) { OnAction = Act };
        N.Phone.OpenSingle(Page, -1, instant);
    }

    /// <summary>Lista filtrów ekranu nad ustawieniami; Esc / krzyżyk wraca do ustawień.</summary>
    public void OpenFilters()
    {
        Filters = new FiltersPage(S.Data, S.Profile);
        N.Phone.OpenSingle(Filters, -1, true);
    }

    public override bool HandleInput(InputCmd e)
    {
        if (N.Phone.HandleInput(e)) return true;
        if (!e.Is(GameAction.Cancel | GameAction.B | GameAction.Start | GameAction.Select)) return false;
        if (Filters is not null)
        {
            ShowSettings(true);
            Page.SelectRow(SettingsRow.Filter);
            return true;
        }
        Close();
        return true;
    }

    public void Close()
    {
        if (_back == Flow.Game)
        {
            Flow.Game.Open();
            App.Refresh();
        }
        else if (_back is null || _back == Flow.Title)
        {
            Flow.Title.Open();
        }
        else
        {
            Flow.Go(_back);
        }
    }

    private void Act(SettingsRow row)
    {
        switch (row)
        {
            case SettingsRow.Help:
                Flow.Help.OpenFromSettings();
                break;
            case SettingsRow.Filter:
                OpenFilters();
                break;
            case SettingsRow.SaveExit:
                S.SaveRun();
                S.Note = Loc.T("budowa_zapisana_kontynuuj");
                Flow.Title.Open();
                break;
            case SettingsRow.Abandon:
                S.AbandonRun();
                Flow.Title.Open();
                break;
        }
    }
}
