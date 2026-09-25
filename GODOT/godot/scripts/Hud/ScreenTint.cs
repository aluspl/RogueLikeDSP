using Godot;

namespace LifeLike.Game;

/// <summary>
/// Błyski ekranu nad mapą (pod HUD): moc zawodu w jej kolorze, czerwony po obrażeniach, żółty przy krycie
/// (set_fade palet na GBA) oraz pulsująca czerwień przy niskim HP.
/// </summary>
public partial class ScreenTint : Control
{
    private Color _color;
    private float _strength;
    private float _clock;

    public bool LowHp { get; set; }

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
    }

    public void Flash(Color c, float strength)
    {
        if (strength < _strength) return;
        _color = c;
        _strength = strength;
    }

    public override void _Process(double delta)
    {
        _clock += (float)delta;
        if (_strength > 0) _strength = Mathf.Max(0, _strength - (float)delta * 2.2f);
        QueueRedraw();
    }

    public override void _Draw()
    {
        var r = new Rect2(Vector2.Zero, Size);
        if (LowHp)
        {
            var p = 0.5f + 0.5f * Mathf.Sin(_clock * 4f);
            DrawRect(r, new Color(0.9f, 0.05f, 0.05f, 0.05f + 0.12f * p));
        }
        if (_strength > 0) DrawRect(r, new Color(_color, _strength));
    }
}
