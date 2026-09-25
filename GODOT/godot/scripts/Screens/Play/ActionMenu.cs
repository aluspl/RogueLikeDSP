using LifeLike.Game.Audio;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;

namespace LifeLike.Game.Screens.Play;

/// <summary>
/// Menu akcji wokół bohatera pod Enter/START (jak GBA v0.21.42): góra Atak, prawo Moc, dół Termos, lewo Czekaj.
/// Strzałka pierwszy raz wybiera, drugi raz tą samą - wykonuje; A wykonuje wybraną, Enter/B zamyka.
/// </summary>
public sealed class ActionMenu
{
    private static readonly GameAction[] Dirs = [GameAction.Up, GameAction.Right, GameAction.Down, GameAction.Left];
    private readonly App _app;

    public ActionMenu(App app) => _app = app;

    private SceneNodes N => _app.Nodes;

    public bool IsOpen => N.World.MenuSel >= -1;

    /// <summary>Wybrana akcja: -1 brak, 0..3 = góra/prawo/dół/lewo.</summary>
    public int Selected
    {
        get => N.World.MenuSel;
        set
        {
            N.World.MenuSel = value;
            Hint();
        }
    }

    public void Open()
    {
        N.World.MenuSel = -1;
        Hint();
        _app.Refresh();
    }

    public void Close()
    {
        N.World.MenuSel = -2;
        N.Hud.ShowHint(null);
        N.Hud.SilenceLog();
    }

    public bool HandleInput(InputCmd e)
    {
        for (var k = 0; k < 4; k++)
        {
            if (!e.Is(Dirs[k])) continue;
            Pick(k);
            return true;
        }
        if (e.Is(GameAction.A) && N.World.MenuSel >= 0) Execute();
        else if (e.Is(GameAction.Start | GameAction.B | GameAction.Cancel))
        {
            Close();
            _app.Refresh();
        }
        else return false;
        return true;
    }

    /// <summary>Strzałka w menu: pierwszy raz wybiera, drugi raz tą samą - wykonuje.</summary>
    public void Pick(int k)
    {
        if (N.World.MenuSel != k)
        {
            Selected = k;
            Sfx.Play("menu");
            return;
        }
        Execute();
    }

    private void Execute()
    {
        var sel = N.World.MenuSel;
        var g = _app.Session.Game;
        Close();
        var acted = sel switch
        {
            0 => PlayCommands.AttackNearest(g, N.World),
            1 => PlayCommands.UseAbility(g, N.World),
            2 => PlayCommands.Drink(g),
            3 => g.PlayerWait(),
            _ => false,
        };
        _app.AfterAction(acted);
    }

    private void Hint()
    {
        var g = _app.Session.Game;
        var sel = N.World.MenuSel;
        var label = sel switch
        {
            0 => $"Atak: najbliższy cel (z{g.Weapon.Range})",
            1 => $"Moc: {UiText.AbilityLabel(g)}" + (g.AbilityCd > 0 ? $" - za {g.AbilityCd} t." : ""),
            2 => $"Termos {g.Thermos}/{g.ThermosCap()}: kawa +{g.CoffeeHeal()} HP (zużywa turę)",
            3 => "Czekaj turę",
            _ => "Akcje: wybierz strzałką (góra Atak, prawo Moc, dół Termos, lewo Czekaj)",
        };
        N.Hud.ShowHint(label, sel < 0 ? "Enter/Z: zamknij" : "Ta sama strzałka lub Spacja: wykonaj   Enter/Z: zamknij");
    }
}
