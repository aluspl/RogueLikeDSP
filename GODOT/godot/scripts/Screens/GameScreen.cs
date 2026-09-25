using LifeLike.Game.Input;
using LifeLike.Game.Screens.Play;

namespace LifeLike.Game.Screens;

/// <summary>
/// Budowa (run_game na GBA): mapa z HUD, ruch, A atak, B czekaj, R moc, START menu akcji, SELECT telefon,
/// L podgląd mapy, mysz. Po każdej akcji App.AfterAction decyduje o kolejnym ekranie.
/// </summary>
public sealed class GameScreen : Screen
{
    public GameScreen(App app) : base(app)
    {
        Menu = new ActionMenu(app);
        Aim = new Aiming(app);
        Look = new EnemyLook(app);
    }

    public ActionMenu Menu { get; }
    public Aiming Aim { get; }
    public EnemyLook Look { get; }

    public override bool InRun => true;
    public override bool ShowsTarget => !Menu.IsOpen && !Aim.Active;

    public override void Exit()
    {
        Aim.Cancel();
        Look.Cancel();
    }

    public override void Process(double delta)
    {
        Aim.Process(delta);
        Look.Process(delta);
    }

    public void Open(bool instant = false) => Flow.Go(this, instant);

    /// <summary>Podgląd całego etapu (L na GBA) z podpowiedzią w dolnym pasie.</summary>
    public void ToggleOverview()
    {
        N.World.ToggleOverview();
        if (N.World.OverviewOn) N.Hud.ShowHint("Podgląd mapy etapu", "Dowolny klawisz: wróć");
        else N.Hud.ShowHint(null);
    }

    public override bool HandleInput(InputCmd e)
    {
        if (Menu.IsOpen) return Menu.HandleInput(e);
        if (Aim.Active) return Aim.HandleInput(e);
        if (Look.Active) return Look.HandleInput(e);
        if (e.IsTap && N.Banners.Hit(e.Pointer)) // dotknięcie powiadomienia: jego zakładka w telefonie
        {
            var tab = N.Banners.TabAt(e.Pointer);
            if (tab >= 0) Flow.Phone.Open(tab);
            return true;
        }
        var g = S.Game;
        var view = N.World;
        if (view.OverviewOn)
        {
            if (!e.AnyKey) return false;
            ToggleOverview();
            return true;
        }
        if (e.Is(GameAction.Select))
        {
            Flow.Phone.Open();
            return true;
        }
        if (e.Is(GameAction.L))
        {
            ToggleOverview();
            return true;
        }
        if (e.Is(GameAction.Start))
        {
            Menu.Open();
            return true;
        }
        int dx = 0, dy = 0;
        if (e.Is(GameAction.Up, true)) dy = -1;
        else if (e.Is(GameAction.Down, true)) dy = 1;
        else if (e.Is(GameAction.Left, true)) dx = -1;
        else if (e.Is(GameAction.Right, true)) dx = 1;
        bool acted;
        if (dx != 0 || dy != 0) acted = g.PlayerMove(dx, dy);
        else if (e.Is(GameAction.A))
        {
            Aim.Begin(); // atak po puszczeniu A (celowanie jak na GBA)
            return true;
        }
        else if (e.Is(GameAction.B))
        {
            Look.Begin(); // krótko - czekaj, przytrzymane - podgląd problemów
            return true;
        }
        else if (e.Is(GameAction.R)) acted = PlayCommands.UseAbility(g, view);
        else if (e.IsClick) acted = PlayCommands.Mouse(g, view, e.Click);
        else return false;
        App.AfterAction(acted);
        return true;
    }
}
