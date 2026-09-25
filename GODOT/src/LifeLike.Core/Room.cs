namespace LifeLike.Core;

/// <summary>Prostokątny pokój etapu (pola jak int8_t w wersji GBA).</summary>
public struct Room
{
    public sbyte X, Y, W, H;

    public readonly int Cx => X + W / 2;
    public readonly int Cy => Y + H / 2;

    public readonly bool Intersects(in Room o) =>
        X - 1 < o.X + o.W && X + W + 1 > o.X && Y - 1 < o.Y + o.H && Y + H + 1 > o.Y;
}
