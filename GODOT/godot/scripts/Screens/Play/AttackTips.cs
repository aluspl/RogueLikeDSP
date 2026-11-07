using System;
using Godot;
using LifeLike.Game.Input;
using LifeLike.Game.Touch;

namespace LifeLike.Game.Screens.Play;

/// <summary>
/// v0.23.0: kiedy pokazać opis ataku (AttackTip) bez dotyku: najechanie myszą na przycisk Cios / Patent paska akcji (komputer
/// z --touch) albo na pole broni w górnym pasku (komputer), przytrzymanie LB na padzie (Q na klawiaturze) – opis Patentu.
/// Przytrzymanie palcem obsługuje TouchPlay (Holding = dymek należy do palca). Zjechanie myszą / puszczenie chowa dymek.
/// </summary>
public sealed class AttackTips
{
    /// <summary>Po tylu sekundach przytrzymania (palec, LB) pokazuje się opis.</summary>
    public const float HoldTime = 0.4f;

    private readonly App _app;
    private readonly GameScreen _screen;
    private float _padHeld;
    private BarButton _shown = BarButton.None;

    public AttackTips(App app, GameScreen screen)
    {
        _app = app;
        _screen = screen;
    }

    /// <summary>Dymek pokazany przytrzymaniem palca (TouchPlay) – tu go nie chowamy.</summary>
    public bool Holding { get; set; }

    private SceneNodes N => _app.Nodes;

    private static bool Desktop => !OS.HasFeature("mobile");

    /// <summary>Opis trybu nad przyciskiem paska akcji.</summary>
    public void ShowOverButton(BarButton b)
    {
        var slot = Array.IndexOf(ActionBar.Order, b);
        if (slot < 0) return;
        _shown = b;
        N.Hud.ShowAttackTip(_app.Session.Game, b == BarButton.WeaponB, ActionBar.ButtonRect(slot), N.Touch.Bar);
    }

    public void Hide()
    {
        Holding = false;
        _padHeld = 0;
        _shown = BarButton.None;
        N.Hud.HideAttackTip();
    }

    public void Process(double delta)
    {
        if (Holding) // dymek palca: ten sam przycisk, przeliczony po zmianie układu (odnowienie, obrót ekranu)
        {
            if (_shown != BarButton.None && N.Hud.Tip.Shown) ShowOverButton(_shown);
            return;
        }
        if (_screen.Menu.IsOpen || _screen.Aim.Active || _screen.Look.Active || N.World.OverviewOn || _app.Session.Game is null)
        {
            _padHeld = 0;
            N.Hud.HideAttackTip();
            return;
        }
        if (GameInput.IsHeld(GameAction.TabPrev)) // pad LB (klawiatura Q) – modyfikator Patentu
        {
            _padHeld += (float)delta;
            if (_padHeld < HoldTime) return;
            if (N.Touch.Visible) ShowOverButton(BarButton.WeaponB);
            else N.Hud.ShowAttackTipAtHud(_app.Session.Game, true);
            return;
        }
        _padHeld = 0;
        if (Desktop && !Godot.Input.IsMouseButtonPressed(MouseButton.Left))
        {
            if (N.Touch.Visible)
            {
                var b = ActionBar.HitTest(N.Touch.Bar.GetLocalMousePosition());
                if (b is BarButton.Attack or BarButton.WeaponB)
                {
                    ShowOverButton(b);
                    return;
                }
            }
            else if (N.Hud.MouseOverWeapon())
            {
                N.Hud.ShowAttackTipAtHud(_app.Session.Game, true);
                return;
            }
        }
        N.Hud.HideAttackTip();
    }
}
