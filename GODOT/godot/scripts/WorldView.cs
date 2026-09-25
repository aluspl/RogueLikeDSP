using System.Collections.Generic;
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

    /// <summary>Liczby nad polami (obrażenia, KRYT!, Unik!) – znikają po FloatLife sekundach.</summary>
    private readonly List<Floater> _floaters = new();
    private const float FloatLife = 1.2f;

    private sealed class Floater
    {
        public Vector2 Pos;
        public string Text = "";
        public Color Col;
        public int Size;
        public float Age;
    }

    /// <summary>Menu akcji pod Enter/START: -2 zamknięte, -1 otwarte bez wyboru, 0..3 = góra/prawo/dół/lewo.</summary>
    public int MenuSel { get; set; } = -2;

    public static readonly Vector2I[] MenuDirs = [new(0, -1), new(1, 0), new(0, 1), new(-1, 0)];
    public static readonly string[] MenuIcons = ["A", "M", "T", "C"]; // Atak, Moc, Termos, Czekaj
    private static readonly Color MenuC = new("fde68a"), MenuSelC = new("f59e0b");
    public static readonly Color CritC = new("facc15"), DodgeC = new("34d399"), HurtC = new("ef4444");

    private static readonly Color WallC = new("3a3548"), FloorC = new("8a8577"), StairsC = new("f59e0b");
    private static readonly Color TempWallC = new("b45309"), HeroC = new("6b4eff"), EnemyC = new("ef4444");
    private static readonly Color BossC = new("991b1b"), SlamC = new(1f, 0.1f, 0.1f, 0.45f), TextC = Colors.White;
    private static readonly Color PickupC = new("10b981"), GearC = new("38bdf8"), ToolC = new("ff7a3d");

    public void Bind(LifeLike.Core.Game g) => _g = g;

    /// <summary>Wróg wskazany celowaniem (ramka).</summary>
    public void Mark(int enemy) => _marked = enemy;

    /// <summary>
    /// Przenosi trafienia z rdzenia na liczby nad polami i zeruje listę (jak warstwa GBA po każdej turze):
    /// „KRYT! -N” na żółto, „Unik!” na zielono, obrażenia bohatera na czerwono.
    /// </summary>
    public void TakeHits(LifeLike.Core.Game g)
    {
        for (var i = 0; i < g.HitsCount; i++)
        {
            var h = g.Hits[i];
            var f = new Floater { Pos = GridToScreen(h.X, h.Y) + new Vector2(0, -Cell * 0.7f - 9 * (i % 3)), Size = 13 };
            switch (h.Kind)
            {
                case HitKind.Dodge:
                    f.Text = "Unik!";
                    f.Col = DodgeC;
                    break;
                case HitKind.Crit:
                    f.Text = $"KRYT! -{h.Amount}";
                    f.Col = CritC;
                    f.Size = 15;
                    break;
                default:
                    f.Text = $"-{h.Amount}";
                    f.Col = h.OnHero ? HurtC : Colors.White;
                    break;
            }
            _floaters.Add(f);
        }
        g.HitsCount = 0;
        QueueRedraw();
    }

    public override void _Process(double delta)
    {
        if (_floaters.Count == 0) return;
        foreach (var f in _floaters) f.Age += (float)delta;
        _floaters.RemoveAll(f => f.Age > FloatLife);
        QueueRedraw();
    }

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

        if (MenuSel >= -1)
        {
            for (var k = 0; k < 4; k++)
            {
                var c = GridToScreen(h.X, h.Y) + new Vector2(MenuDirs[k].X, MenuDirs[k].Y) * (Cell * 1.15f);
                if (k == MenuSel) DrawCircle(c, Cell * 0.55f, MenuSelC);
                DrawCircle(c, Cell * 0.42f, new Color(0.08f, 0.06f, 0.14f));
                DrawArc(c, Cell * 0.42f, 0, Mathf.Tau, 20, k == MenuSel ? MenuSelC : MenuC, 1.5f);
                DrawString(font, c + new Vector2(-Cell / 2f, 5), MenuIcons[k], HorizontalAlignment.Center, Cell, 13, k == MenuSel ? MenuSelC : MenuC);
            }
        }

        foreach (var f in _floaters)
        {
            var t = f.Age / FloatLife;
            var col = new Color(f.Col, 1f - t * t);
            var pos = f.Pos + new Vector2(0, -14 * t);
            DrawString(font, pos + new Vector2(-40 + 1, 1), f.Text, HorizontalAlignment.Center, 80, f.Size, new Color(0, 0, 0, col.A));
            DrawString(font, pos + new Vector2(-40, 0), f.Text, HorizontalAlignment.Center, 80, f.Size, col);
        }
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
