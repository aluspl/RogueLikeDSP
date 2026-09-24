namespace LifeLike.Core.Grid;

/// <summary>Pozycja logiczna na siatce. Render (2D/2.5D/3D) tylko ją tłumaczy na ekran.</summary>
public readonly record struct GridPos(int X, int Y)
{
    public static GridPos operator +(GridPos a, GridPos b) => new(a.X + b.X, a.Y + b.Y);
    public int ChebyshevDistance(GridPos o) => Math.Max(Math.Abs(X - o.X), Math.Abs(Y - o.Y));

    public static readonly GridPos Up = new(0, -1), Down = new(0, 1), Left = new(-1, 0), Right = new(1, 0);
}
