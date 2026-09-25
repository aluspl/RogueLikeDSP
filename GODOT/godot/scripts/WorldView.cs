using Godot;
using LifeLike.Core;

namespace LifeLike.Game;

/// <summary>
/// Prosty render 2D (kolorowe prostokąty + litery, zero assetów). Mgła wojny: nieznane pola czarne,
/// zapamiętane przyciemnione, widoczne w pełnych kolorach; wrogów i znajdźki widać tylko w polu widzenia.
/// Jedyne miejsce, które zna rzutowanie siatka -> ekran: przejście na 2.5D = podmiana tej klasy.
/// </summary>
public partial class WorldView : Node2D
{
    public const int Cell = 20;

    private LifeLike.Core.Game _g;
    private int _marked = -1;

    private static readonly Color WallC = new("3a3548"), FloorC = new("8a8577"), StairsC = new("f59e0b");
    private static readonly Color TempWallC = new("b45309"), HeroC = new("6b4eff"), EnemyC = new("ef4444");
    private static readonly Color BossC = new("991b1b"), SlamC = new(1f, 0.1f, 0.1f, 0.45f), TextC = Colors.White;
    private static readonly Color PickupC = new("10b981"), GearC = new("38bdf8"), ToolC = new("ff7a3d");

    public void Bind(LifeLike.Core.Game g) => _g = g;

    /// <summary>Wróg wskazany celowaniem (ramka).</summary>
    public void Mark(int enemy) => _marked = enemy;

    public Vector2 GridToScreen(int x, int y) => new(x * Cell + Cell / 2f, y * Cell + Cell / 2f);

    public Vector2I ScreenToGrid(Vector2 world) => new(Mathf.FloorToInt(world.X / Cell), Mathf.FloorToInt(world.Y / Cell));

    public override void _Draw()
    {
        if (_g is null) return;
        var font = ThemeDB.FallbackFont;
        DrawRect(new Rect2(-Cell * 4, -Cell * 4, Cell * (Level.W + 8), Cell * (Level.H + 8)), Colors.Black);
        for (var y = 0; y < Level.H; y++)
        {
            for (var x = 0; x < Level.W; x++)
            {
                if (!_g.Explored(x, y)) continue;
                var t = _g.Lv[x, y];
                var c = t switch { Tile.Wall => WallC, Tile.Stairs => StairsC, _ => FloorC };
                if (t == Tile.Wall && IsTempWall(x, y)) c = TempWallC;
                if (!_g.Visible(x, y)) c = c.Darkened(0.65f);
                var r = new Rect2(x * Cell, y * Cell, Cell - 1, Cell - 1);
                DrawRect(r, c);
                if (t == Tile.Stairs) Letter(font, x, y, ">", Colors.Black);
                if (_g.SlamCell(x, y)) DrawRect(r, SlamC);
            }
        }

        for (var i = 0; i < _g.PickupsCount; i++)
        {
            var p = _g.Pickups[i];
            if (!p.Active || !_g.Visible(p.X, p.Y)) continue;
            var (ch, col) = p.Type switch
            {
                PickupType.Coffee => ("K", PickupC),
                PickupType.Helmet => ("H", GearC),
                PickupType.Plan => ("P", GearC),
                PickupType.GearBox => ("S", GearC),
                _ => ("N", ToolC),
            };
            DrawCircle(GridToScreen(p.X, p.Y), Cell * 0.35f, col);
            Letter(font, p.X, p.Y, ch, Colors.Black);
        }

        for (var i = 0; i < _g.EnemiesCount; i++)
        {
            var e = _g.Enemies[i];
            if (!e.Alive || !_g.Visible(e.X, e.Y)) continue;
            var def = _g.D.Enemies[e.DefId];
            DrawRect(new Rect2(e.X * Cell + 2, e.Y * Cell + 2, Cell - 5, Cell - 5), i == _g.Boss ? BossC : EnemyC);
            Letter(font, e.X, e.Y, def.Name[..1], TextC);
            if (e.Stun > 0) DrawRect(new Rect2(e.X * Cell, e.Y * Cell, Cell - 1, Cell - 1), new Color(0.4f, 0.6f, 1f), false, 2);
            Bar(e.X, e.Y, e.Hp, e.MaxHp, Colors.OrangeRed);
            if (i == _marked) DrawRect(new Rect2(e.X * Cell - 1, e.Y * Cell - 1, Cell + 1, Cell + 1), Colors.Yellow, false, 2);
        }

        var h = _g.Hero;
        DrawRect(new Rect2(h.X * Cell + 1, h.Y * Cell + 1, Cell - 3, Cell - 3), h.Alive ? HeroC : Colors.DimGray);
        Letter(font, h.X, h.Y, "@", TextC);
        Bar(h.X, h.Y, h.Hp, h.MaxHp, Colors.LimeGreen);
    }

    private bool IsTempWall(int x, int y)
    {
        for (var i = 0; i < _g.WallsCount; i++)
        {
            if (_g.Walls[i].X == x && _g.Walls[i].Y == y) return true;
        }
        return false;
    }

    private void Letter(Font font, int x, int y, string s, Color c) =>
        DrawString(font, new Vector2(x * Cell, y * Cell + Cell - 5), s, HorizontalAlignment.Center, Cell - 1, 14, c);

    private void Bar(int x, int y, int hp, int max, Color c)
    {
        if (max <= 0) return;
        var w = (Cell - 2) * Mathf.Clamp(hp / (float)max, 0f, 1f);
        DrawRect(new Rect2(x * Cell, y * Cell - 3, Cell - 2, 2), Colors.Black);
        DrawRect(new Rect2(x * Cell, y * Cell - 3, w, 2), c);
    }
}
