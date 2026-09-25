using LifeLike.Core.Entities;

namespace LifeLike.Core.Grid;

/// <summary>Logiczna mapa: ściany + zajętość pól przez aktorów.</summary>
public sealed class TileGrid(int width, int height)
{
    private readonly bool[,] _walls = new bool[width, height];
    private readonly Dictionary<GridPos, Actor> _actors = new();

    public int Width { get; } = width;
    public int Height { get; } = height;

    public bool InBounds(GridPos p) => p.X >= 0 && p.Y >= 0 && p.X < Width && p.Y < Height;
    public void SetWall(GridPos p, bool wall = true) => _walls[p.X, p.Y] = wall;
    public bool IsWall(GridPos p) => !InBounds(p) || _walls[p.X, p.Y];
    public bool IsWalkable(GridPos p) => !IsWall(p) && !_actors.ContainsKey(p);
    public Actor? ActorAt(GridPos p) => _actors.GetValueOrDefault(p);

    public void Place(Actor actor, GridPos p)
    {
        if (!IsWalkable(p)) throw new InvalidOperationException($"Pole {p} zajęte");
        _actors[p] = actor;
        actor.Position = p;
    }

    internal void Move(Actor actor, GridPos to)
    {
        _actors.Remove(actor.Position);
        _actors[to] = actor;
        actor.Position = to;
    }

    public void Remove(Actor actor) => _actors.Remove(actor.Position);
}
