using Godot;
using LifeLike.Game.Input;
using LifeLike.Game.Phone.Pages;

namespace LifeLike.Game.Screens;

/// <summary>
/// Po wygranej, przed planszą końcową: harmonogram gotowego domu w telefonie (odbiór dziś – data systemu). A / link
/// otwiera planbudowlany.online, Enter – plansza końcowa.
/// </summary>
public sealed class HouseScheduleScreen : Screen
{
    public HouseScheduleScreen(App app) : base(app)
    {
    }

    public override bool InRun => true;
    public override bool UsesPhone => true;

    /// <summary>Ile razy otwarto stronę (test dymny zamiast przeglądarki).</summary>
    public int LinkOpened { get; private set; }

    /// <summary>false = bez otwierania przeglądarki (test dymny, zrzuty).</summary>
    public bool OpenBrowser { get; set; } = true;

    public void Open(bool instant = false) => Flow.Go(this, instant);

    public override void Enter(bool instant)
    {
        N.Banners.Clear(); // odznaki z końca budowy były już na SMS-ie - nie zasłaniają zdjęcia domu
        N.Phone.OpenSingle(new HouseSchedulePage(S.Game, S.Today), 0, instant);
    }

    public override bool HandleInput(InputCmd e)
    {
        if (N.Phone.HandleInput(e)) return true;
        if (e.Is(GameAction.A))
        {
            LinkOpened++;
            if (OpenBrowser) OS.ShellOpen(SettingsPage.Url);
            return true;
        }
        if (!e.Is(GameAction.Start | GameAction.B | GameAction.Cancel)) return false;
        Flow.End.Open();
        return true;
    }
}
