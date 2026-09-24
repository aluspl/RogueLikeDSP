using System.Collections.Generic;
using Godot;
using LifeLike.Core.Entities;
using LifeLike.Core.Grid;

namespace LifeLike.Game;

/// <summary>
/// Render placeholder (prostokąty, zero assetów). Jedyne miejsce, które zna rzutowanie siatka -> ekran:
/// przejście na 2.5D/iso = podmiana tej klasy, bez ruszania logiki (patrz ADR-0003).
/// </summary>
public partial class WorldView : Node2D
{
    public const int Tile = 16;
    private TileGrid? _map;
    private Actor? _hero;
    private IReadOnlyList<Actor> _enemies = [];

    private static readonly Color Wall = new("2b2b3a"), Floor = new("4a4a5e"), Hero = new("e8c547"), Enemy = new("c94c4c");

    public void Bind(TileGrid map, Actor hero, IReadOnlyList<Actor> enemies)
    {
        _map = map; _hero = hero; _enemies = enemies;
    }

    public Vector2 GridToScreen(GridPos p) => new(p.X * Tile + Tile / 2f, p.Y * Tile + Tile / 2f);
    public GridPos ScreenToGrid(Vector2 world) => new(Mathf.FloorToInt(world.X / Tile), Mathf.FloorToInt(world.Y / Tile));

    public override void _Draw()
    {
        if (_map is null || _hero is null) return;
        for (var x = 0; x < _map.Width; x++)
            for (var y = 0; y < _map.Height; y++)
            {
                var p = new GridPos(x, y);
                DrawRect(new Rect2(x * Tile, y * Tile, Tile - 1, Tile - 1), _map.IsWall(p) ? Wall : Floor);
            }

        foreach (var e in _enemies)
            if (!e.IsDead) DrawActor(e, Enemy);
        DrawActor(_hero, _hero.IsDead ? Colors.DimGray : Hero);
    }

    private void DrawActor(Actor a, Color c)
    {
        var center = GridToScreen(a.Position);
        DrawCircle(center, Tile * 0.4f, c);
        var hp = (float)a.Health / a.Stats.MaxHealth;
        DrawRect(new Rect2(center.X - Tile / 2f, center.Y - Tile / 2f - 3, Tile * hp, 2), Colors.LimeGreen);
    }
}
