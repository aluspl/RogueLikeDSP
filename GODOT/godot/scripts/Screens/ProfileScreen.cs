using LifeLike.Game.Input;
using LifeLike.Game.Phone;
using LifeLike.Game.Phone.ProfileTabs;

namespace LifeLike.Game.Screens;

/// <summary>Telefon profilu na tle tytułu (run_shop na GBA): Odznaki/Zlecenia/Pamiątki, Katalog, Osiedle, Zespół, Koszty = Szkolenia.</summary>
public sealed class ProfileScreen : Screen
{
    private static readonly string[] TabLabels = ["Odznaki", "Katalog", "Osiedle", "Zespół", "Koszty"];
    private int _tab;

    public ProfileScreen(App app) : base(app)
    {
    }

    public override ViewSet Views => ViewSet.Title;
    public override bool UsesPhone => true;
    public override string Music => "title";

    public void Open(int tab = 0, bool instant = false)
    {
        _tab = tab;
        S.Note = "";
        Flow.Go(this, instant);
    }

    public override void Enter(bool instant)
    {
        Flow.Title.Populate();
        var d = S.Data;
        var p = S.Profile;
        PhonePage[] tabs =
        [
            new BadgesTab(d, p),
            new CatalogTab(d, p),
            new EstateTab(d, p),
            new TeamTab(d, p),
            new TrainingTab(d, p, S.Save),
        ];
        N.Phone.OpenTabs(tabs, TabLabels, _tab, instant);
    }

    public override bool HandleInput(InputCmd e)
    {
        if (N.Phone.HandleInput(e)) return true;
        if (!e.Is(GameAction.Profile | GameAction.B | GameAction.Cancel | GameAction.Select | GameAction.Shop | GameAction.Start)) return false;
        S.Note = "";
        N.Phone.Close();
        Flow.Title.Open();
        return true;
    }
}
