using LifeLike.Game.Audio;
using LifeLike.Game.Input;
using LifeLike.Game.Phone;
using LifeLike.Game.Phone.Pages;

namespace LifeLike.Game.Screens;

/// <summary>
/// v0.23.2 (UI 2): Postać – sprzęt na bohaterze, 4 kluczowe liczby i plecak (zastępuje Start, Sprzęt i osobny ekran plecaka).
/// Otwierają: pauza, I, dotknięcie paska HUD / broni w HUD, powiadomienia o sprzęcie. Zamiana z plecaka zużywa turę – ekran
/// wraca wtedy na plac; inne akcje (wyrzucenie) zostawiają ekran. B / Esc / I: z powrotem (pauza albo plac).
/// </summary>
public sealed class CharacterScreen : Screen
{
    private Screen _back;

    public CharacterScreen(App app) : base(app)
    {
    }

    public override bool InRun => true;
    public override bool UsesPhone => true;

    public CharacterPage Page { get; private set; }

    /// <summary>back: ekran po wyjściu bez zużytej tury (null = plac budowy).</summary>
    public void Open(Screen back = null, bool instant = false)
    {
        _back = back;
        Flow.Game.Menu.Close();
        N.Banners.Clear();
        Flow.Go(this, instant);
    }

    /// <summary>Powrót z opisu statystyk, rozpiski albo listy premii – ten sam ekran po zamknięciu co wcześniej.</summary>
    public void Return(bool instant = false) => Flow.Go(this, instant);

    public override void Enter(bool instant)
    {
        var focus = Page?.Focus ?? -1;
        Page = new CharacterPage(S.Game, S.Profile)
        {
            Done = Acted,
            Boons = () => Flow.BoonList.Open(),
            Stats = () => Flow.Stats.OpenInRun(0, PhoneTabs.Start),
            Breakdown = () => Flow.Stats.OpenInRun(StatsPage.DamagePage, PhoneTabs.Gear),
            Focus = focus,
        };
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
        if (!e.IsBack && !e.Is(GameAction.Info | GameAction.Select)) return false;
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
        if (!acted && _back is not null && _back != Flow.Game)
        {
            Flow.Go(_back);
            return;
        }
        Flow.Game.Open();
        App.AfterAction(acted);
    }
}
