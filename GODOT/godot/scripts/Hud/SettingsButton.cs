using Godot;
using LifeLike.Game.Gfx;
using LifeLike.Game.Touch;

namespace LifeLike.Game.Hud;

/// <summary>
/// Klucz ustawień w prawym górnym rogu (tytuł i mapa): pikselowa ikona na ciemnej plakietce, w bezpiecznym obszarze
/// (pod wyspą na telefonie). Kliknięcie / dotknięcie otwiera ustawienia (obsługuje Main), Esc na klawiaturze też.
/// </summary>
public partial class SettingsButton : Control
{
    /// <summary>Bok przycisku w pikselach UI: przy dotyku cel 44 pt, z myszą mniejszy.</summary>
    public static float Side => Layout.Touch || Layout.Portrait ? 40f : 32f;

    public static Rect2 Rect => new(Mathf.Round(Layout.SafeArea.End.X - Side - 2), Mathf.Round(Layout.SafeTop + 1), Side, Side);

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        Layout.Changed += QueueRedraw;
    }

    public override void _ExitTree() => Layout.Changed -= QueueRedraw;

    public bool Hit(Vector2 p) => IsVisibleInTree() && Rect.Grow(6).HasPoint(p);

    public override void _Draw()
    {
        var r = Rect;
        var inner = r.Grow(-3);
        DrawStyleBox(Ui.Box(new Color(Pal.Text, 0.55f), 9, new Color(Pal.Brand, 0.8f)), inner);
        var scale = inner.Size.X >= 34 ? 2 : 1;
        var icon = Assets.Icon * scale;
        Assets.DrawFrame(this, Assets.TouchIcons, TouchIcon.Wrench, Assets.Icon, (inner.GetCenter() - new Vector2(icon, icon) / 2).Round(), scale);
    }
}
