using Godot;

namespace LifeLike.Game.World;

/// <summary>Cząsteczka (pył, iskra, konfetti, gwiazdka, cegła, gwóźdź, piorun, kropla...) jak particle w main.cpp.</summary>
public sealed class Particle
{
    public Vector2 Pos;
    public Vector2 Vel;
    public float Gravity;
    public int Age;
    public int Life;
    public int Frame;
    public int Frames = 1;
}
