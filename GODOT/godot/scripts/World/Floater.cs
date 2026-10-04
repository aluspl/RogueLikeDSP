using Godot;
using LifeLike.Game.Gfx;

namespace LifeLike.Game.World;

/// <summary>Unosząca się liczba obrażeń / „KRYT! -N” / „Unik!” nad polem (floater w main.cpp).</summary>
public sealed class Floater
{
    public Vector2 Pos;
    public string Text = "";
    public Ink Ink;
    public float Age;
    public float Life = 0.75f;
    public int Scale = 1;
}
