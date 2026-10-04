using Godot;
using LifeLike.Game.Gfx;
using LifeLike.Game.Settings;

namespace LifeLike.Game.Touch;

/// <summary>
/// Wirtualna gałka (ustawienia: Sterowanie - gałka) nad paskiem akcji po stronie ręki: przeciągnięcie kciuka
/// poza martwą strefę daje kierunek (4 kierunki jak na GBA), trzymanie - kolejne kroki.
/// </summary>
public partial class VirtualStick : Control
{
    public const float Radius = 54f;
    private const float Dead = 16f;

    private Vector2 _knob;

    public bool Active { get; private set; }

    /// <summary>Kierunek wychylenia albo (0, 0).</summary>
    public Vector2I Dir { get; private set; }

    public static bool Enabled => Layout.Touch && GameSettings.Controls == ControlScheme.Joystick;

    public static Vector2 Center
    {
        get
        {
            var bar = ActionBar.BarRect;
            var x = GameSettings.LeftHanded ? Layout.SafeLeft + Radius + 26 : Layout.UiSize.X - Layout.SafeRight - Radius - 26;
            return new Vector2(Mathf.Round(x), Mathf.Round(bar.Position.Y - Radius - 24));
        }
    }

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
    }

    public static bool Hit(Vector2 p) => Enabled && p.DistanceTo(Center) <= Radius + 22;

    public void Begin(Vector2 p)
    {
        Active = true;
        Move(p);
    }

    /// <summary>Nowa pozycja palca; true = zmienił się kierunek.</summary>
    public bool Move(Vector2 p)
    {
        var d = p - Center;
        if (d.Length() > Radius) d = d.Normalized() * Radius;
        _knob = d;
        var dir = d.Length() < Dead ? Vector2I.Zero : GestureTracker.Dir(d);
        var changed = dir != Dir;
        Dir = dir;
        QueueRedraw();
        return changed;
    }

    public void End()
    {
        Active = false;
        Dir = Vector2I.Zero;
        _knob = Vector2.Zero;
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (!Enabled) return;
        var c = Center;
        var a = Active ? 0.55f : 0.32f;
        DrawCircle(c, Radius, new Color(Pal.Text, a), true, -1, false);
        DrawCircle(c, Radius, new Color(Pal.Brand, a + 0.2f), false, 2, false);
        for (var k = 0; k < 4; k++) // znaczniki kierunków
        {
            var v = k switch { 0 => Vector2.Up, 1 => Vector2.Right, 2 => Vector2.Down, _ => Vector2.Left };
            DrawCircle(c + v * (Radius - 12), 3, new Color(Colors.White, a + 0.2f), true, -1, false);
        }
        DrawCircle(c + _knob, 22, new Color(Pal.Brand, 0.85f), true, -1, false);
        DrawCircle(c + _knob, 22, new Color(Colors.White, 0.8f), false, 2, false);
    }
}
