using Godot;

namespace LifeLike.Game.World;

/// <summary>Poświata na mapie (awans): rozszerzający się, gasnący krąg światła wokół punktu.</summary>
public sealed class Glow
{
    public Vector2 Pos;
    public Color Color;
    public float Age;
    public float Life = 1.2f;
    public float Radius = 56f;
}
