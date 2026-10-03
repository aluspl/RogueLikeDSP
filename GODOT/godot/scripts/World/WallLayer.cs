using Godot;
using LifeLike.Core;
using LifeLike.Game.Gfx;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.World;

/// <summary>
/// v0.21.54: mury jako bryły 1,5 pola (wyższe ściany, oba widoki). Każdy wiersz mapy to osobny węzeł w warstwie postaci
/// z sortowaniem po Y (klucz tuż przed środkiem wiersza), więc postać za murem jest zasłonięta, a przed murem – nie.
/// Wierzch muru (kafel 6 z nakładkami krawędzi jak autokafle v0.21.51) leży Proj.WallH wyżej niż podstawa, pod nim
/// wysokie lico (stage_N_wall.png) – tylko gdy poniżej nie ma pokazanego muru. Czytelność: mur, który zasłania widoczną
/// podłogę (x, y-1), płynnie robi się półprzezroczysty (np. południowa ściana pokoju, w którym jest bohater), a jeszcze
/// bardziej przed bohaterem, pomocnikiem, problemem, znajdźką, polem ciosu i schodami (także postać w polu (x, y-2)
/// zasłonięta do połowy). Pęknięcie / drzwi magazynu są na licu.
/// </summary>
public sealed partial class WallLayer
{
    /// <summary>Przezroczystość muru przed czymś ważnym i przed zwykłą widoczną podłogą.</summary>
    public const float SeeThrough = 0.38f, SeeFloor = 0.55f;

    private readonly WallRow[] _rows = new WallRow[Level.H];
    private readonly float[] _alpha = new float[Level.W * Level.H];
    private readonly float[] _full = new float[Level.W * Level.H];   // pole za murem poniżej: najwyższa dozwolona nieprzezroczystość muru
    private readonly float[] _half = new float[Level.W * Level.H];   // postać w polu: mur dwa wiersze niżej zasłania jej dół
    private CoreGame _g;
    private float _clock;

    public WallLayer()
    {
        System.Array.Fill(_alpha, 1f);
        ClearMarks();
    }

    /// <summary>Węzły wierszy w warstwie postaci (YSortEnabled).</summary>
    public void Attach(Node2D actors)
    {
        for (var y = 0; y < Level.H; y++)
        {
            _rows[y] = new WallRow { Layer = this, Row = y };
            actors.AddChild(_rows[y]);
        }
        Relayout();
    }

    public void Bind(CoreGame g) => _g = g;

    /// <summary>Bieżąca nieprzezroczystość muru w polu (test dymny).</summary>
    public float Alpha(int x, int y) => Level.In(x, y) ? _alpha[y * Level.W + x] : 1f;

    public static float RowKey(int y) => y * Proj.RowH + Proj.RowH / 2 - 1;

    /// <summary>Po zmianie widoku: klucze sortowania i rysunek od nowa.</summary>
    public void Relayout()
    {
        for (var y = 0; y < Level.H; y++)
        {
            if (_rows[y] is null) continue;
            _rows[y].Position = new Vector2(0, RowKey(y));
            _rows[y].QueueRedraw();
        }
    }

    public void Redraw()
    {
        foreach (var r in _rows) r?.QueueRedraw();
    }

    /// <summary>Nowy etap: mury od razu pełne.</summary>
    public void ResetFade()
    {
        System.Array.Fill(_alpha, 1f);
        Redraw();
    }

    public void ClearMarks()
    {
        System.Array.Fill(_full, 1f);
        System.Array.Fill(_half, 1f);
    }

    /// <summary>Pole, którego mur nie może całkiem zasłonić (actor = postać: liczy się też zasłonięcie do połowy);
    /// alpha – nieprzezroczystość muru przed nim.</summary>
    public void Mark(int x, int y, bool actor, float alpha = SeeThrough)
    {
        if (!Level.In(x, y)) return;
        var i = y * Level.W + x;
        _full[i] = Mathf.Min(_full[i], alpha);
        if (actor) _half[i] = Mathf.Min(_half[i], alpha);
    }

    /// <summary>Co klatkę: półprzezroczystość murów zasłaniających zaznaczone pola (płynnie), przerysowanie zmienionych wierszy.</summary>
    public void Update(float dt)
    {
        if (_g is null) return;
        _clock += dt;
        var step = dt * 5f;
        for (var y = 0; y < Level.H; y++)
        {
            var changed = false;
            for (var x = 0; x < Level.W; x++)
            {
                var i = y * Level.W + x;
                var dst = 1f;
                if (_g.Lv[x, y] == Tile.Wall)
                {
                    if (y >= 1) dst = _full[i - Level.W];
                    if (y >= 2) dst = Mathf.Min(dst, _half[i - 2 * Level.W]);
                }
                if (Mathf.IsEqualApprox(_alpha[i], dst)) continue;
                _alpha[i] = Mathf.MoveToward(_alpha[i], dst, step);
                changed = true;
            }
            if (changed || (y == _g.SecretY && _g.SecretClosed())) _rows[y].QueueRedraw();
        }
    }

    private bool Solid(int x, int y) => _g.Lv.At(x, y) == Tile.Wall;

    /// <summary>Mur pokazany na mapie (odkryty) – jego wierzch zakrywa lico muru powyżej.</summary>
    private bool Shown(int x, int y) => Solid(x, y) && _g.Explored(x, y);

    private bool IsTempWall(int x, int y)
    {
        for (var i = 0; i < _g.WallsCount; i++)
        {
            if (_g.Walls[i].X == x && _g.Walls[i].Y == y && _g.Walls[i].Turns > 0) return true;
        }
        return false;
    }

    /// <summary>Prostokąt lica muru w polu (x, y) w pikselach świata.</summary>
    public static Rect2 FaceRect(int x, int y) => new(x * Proj.W, (y + 1) * Proj.RowH - Proj.WallH, Proj.W, Proj.WallH);

    public static Rect2 TopRect(int x, int y) => new(x * Proj.W, y * Proj.RowH - Proj.WallH, Proj.W, Proj.RowH);

    private void DrawRow(CanvasItem ci, int y)
    {
        if (_g is null) return;
        var q = Proj.ThreeQuarter;
        var rh = Proj.RowH;
        var wh = Proj.WallH;
        var look = _g.SDef().Look;
        var tops = q ? Assets.StageTiles34(look) : Assets.StageTiles(look);
        var faces = Assets.StageWalls(look);
        var brickTops = q ? Assets.StageTiles34(1) : Assets.StageTiles(1);
        var brickFaces = Assets.StageWalls(1);
        var col = q ? Assets.WallSheet34 : 0;
        ci.DrawSetTransform(new Vector2(0, -RowKey(y)));
        for (var x = 0; x < Level.W; x++)
        {
            if (!_g.Explored(x, y) || _g.Lv[x, y] != Tile.Wall) continue;
            var mod = new Color(1, 1, 1, _alpha[y * Level.W + x]);
            var temp = IsTempWall(x, y);
            var tt = temp ? brickTops : tops;
            var top = TopRect(x, y);
            void Top(int idx) => ci.DrawTextureRectRegion(tt, top, new Rect2(idx * Proj.W, 0, Proj.W, rh), mod);
            Top(Assets.TileWall);
            bool n = Solid(x, y - 1), w = Solid(x - 1, y), e = Solid(x + 1, y);
            if (!temp)
            {
                if (!n) Top(Assets.TileEdgeTop);
                if (!w) Top(Assets.TileEdgeLeft);
                if (!e) Top(Assets.TileEdgeRight);
                if (n && w && !Solid(x - 1, y - 1)) Top(Assets.TileInnerCorner);
            }
            var face = !Shown(x, y + 1);
            if (face)
            {
                var fr = FaceRect(x, y);
                var ff = temp ? brickFaces : faces;
                void Face(int k) => ci.DrawTextureRectRegion(ff, fr, new Rect2((col + k) * Proj.W, 0, Proj.W, wh), mod);
                Face(Assets.WallFace);
                if (!temp && !w) Face(Assets.WallFaceLeft);
                if (!temp && !e) Face(Assets.WallFaceRight);
            }
            if (x == _g.SecretX && y == _g.SecretY && _g.SecretClosed()) DrawSecret(ci, x, y, face);
        }
        ci.DrawSetTransform(Vector2.Zero);
    }

    /// <summary>Magazyn: pęknięcie albo drzwi u dołu lica (bez lica – na wierzchu), złota ramka, gdy da się otworzyć.</summary>
    private void DrawSecret(CanvasItem ci, int x, int y, bool face)
    {
        var r = face ? new Rect2(x * Proj.W, (y + 1) * Proj.RowH - Assets.Actor, Assets.Actor, Assets.Actor) : TopRect(x, y);
        var frame = _g.SecretDef.Breakable ? Assets.FrameCrack : Assets.FrameDoor;
        ci.DrawTextureRectRegion(Assets.Actors, r, Assets.Frame(frame, Assets.Actor), new Color(1, 1, 1, _g.Visible(x, y) ? 1f : 0.6f));
        if (_g.Keys > 0 || _g.CanOpenSecret())
        {
            var pulse = 0.5f + 0.5f * Mathf.Sin(_clock * 7f);
            ci.DrawRect(r.Grow(-1), new Color(1f, 0.85f, 0.3f, 0.35f + 0.4f * pulse), false, 2f);
        }
    }

    /// <summary>Wiersz murów (węzeł sortowany po Y razem z postaciami).</summary>
    private sealed partial class WallRow : Node2D
    {
        public WallLayer Layer;
        public int Row;

        public override void _Draw()
        {
            try
            {
                Layer.DrawRow(this, Row);
            }
            catch (System.Exception ex)
            {
                DrawErrors.Record("WallRow", ex);
            }
        }
    }
}
