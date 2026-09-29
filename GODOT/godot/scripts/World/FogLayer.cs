using Godot;
using LifeLike.Core;
using LifeLike.Game.Gfx;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.World;

/// <summary>
/// Mgła wojny z miękkim światłem (odpowiednik 4 palet światła etapu na GBA: pełne przy bohaterze, 80%, 60% na skraju
/// pola widzenia, przyciemnione pola zapamiętane). Tekstura ma Sub x Sub tekseli na pole mapy i jest rysowana
/// z filtrowaniem liniowym, więc granice światła są płynne, a zmiany jasności przechodzą łagodnie przy ruchu.
/// v0.21.51: pola znane przy nieznanych gasną w ciemność na szerokości ~pół pola (odległość od najbliższego
/// nieznanego pola, także po skosie) - zamiast twardych schodków na granicy odkrytej części etapu.
/// </summary>
public partial class FogLayer : Node2D
{
    /// <summary>Teksele na pole mapy (rozdzielczość miękkiej krawędzi).</summary>
    private const int Sub = 4;

    /// <summary>Szerokość zanikania w ciemność przy nieznanym polu (w polach mapy).</summary>
    private const float EdgeFade = 0.55f;

    private const int TW = Level.W * Sub, TH = Level.H * Sub;

    private CoreGame _g;
    private ImageTexture _tex;
    private readonly byte[] _px = new byte[TW * TH * 4];
    private readonly float[] _cur = new float[Level.W * Level.H];
    private readonly float[] _dst = new float[Level.W * Level.H];
    private readonly bool[] _known = new bool[Level.W * Level.H];
    private readonly float[] _edge = new float[TW * TH];   // 0 = daleko od nieznanego, 1 = na granicy
    private bool _dirty = true;

    public override void _Ready()
    {
        TextureFilter = TextureFilterEnum.Linear;
        _tex = ImageTexture.CreateFromImage(Image.CreateFromData(TW, TH, false, Image.Format.Rgba8, _px));
    }

    public void Bind(CoreGame g) => _g = g;

    /// <summary>Nowe cele jasności po turze; snap = od razu (nowy etap).</summary>
    public void Sync(bool snap)
    {
        if (_g is null) return;
        var sight = Mathf.Max(3, _g.SightRadius());
        for (var y = 0; y < Level.H; y++)
        {
            for (var x = 0; x < Level.W; x++)
            {
                var i = y * Level.W + x;
                _known[i] = _g.Explored(x, y);
                float a;
                if (!_known[i]) a = 1f;
                else if (!_g.Visible(x, y)) a = 0.58f;
                else
                {
                    float dx = x - _g.Hero.X, dy = y - _g.Hero.Y;
                    var d = Mathf.Sqrt(dx * dx + dy * dy);
                    a = Mathf.Clamp((d - 2.2f) / (sight - 2.2f), 0f, 1f) * 0.45f;
                }
                _dst[i] = a;
                if (snap) _cur[i] = a;
            }
        }
        BuildEdges();
        _dirty = true;
    }

    private bool Known(int x, int y) => x >= 0 && y >= 0 && x < Level.W && y < Level.H && _known[y * Level.W + x];

    /// <summary>Zanikanie przy nieznanych polach: dla każdego teksela pola znanego odległość do najbliższego nieznanego
    /// sąsiada (prostokąt pola, 8 kierunków; poza mapą = nieznane).</summary>
    private void BuildEdges()
    {
        for (var cy = 0; cy < Level.H; cy++)
        {
            for (var cx = 0; cx < Level.W; cx++)
            {
                var known = _known[cy * Level.W + cx];
                for (var sy = 0; sy < Sub; sy++)
                {
                    for (var sx = 0; sx < Sub; sx++)
                    {
                        var ti = (cy * Sub + sy) * TW + cx * Sub + sx;
                        if (!known)
                        {
                            _edge[ti] = 1f;
                            continue;
                        }
                        // pozycja środka teksela wewnątrz pola (0..1)
                        float fx = (sx + 0.5f) / Sub, fy = (sy + 0.5f) / Sub;
                        var best = 9f;
                        for (var oy = -1; oy <= 1; oy++)
                        {
                            for (var ox = -1; ox <= 1; ox++)
                            {
                                if ((ox == 0 && oy == 0) || Known(cx + ox, cy + oy)) continue;
                                var ddx = ox < 0 ? fx : ox > 0 ? 1 - fx : 0f;
                                var ddy = oy < 0 ? fy : oy > 0 ? 1 - fy : 0f;
                                best = Mathf.Min(best, Mathf.Sqrt(ddx * ddx + ddy * ddy));
                            }
                        }
                        var t = Mathf.Clamp(1f - best / EdgeFade, 0f, 1f);
                        _edge[ti] = t * t * (3 - 2 * t); // smoothstep
                    }
                }
            }
        }
    }

    public override void _Process(double delta)
    {
        if (_g is null || _tex is null) return;
        var step = (float)delta * 5f;
        for (var i = 0; i < _cur.Length; i++)
        {
            var d = _dst[i] - _cur[i];
            if (Mathf.Abs(d) < 0.001f) continue;
            _cur[i] += Mathf.Clamp(d, -step, step);
            _dirty = true;
        }
        if (!_dirty) return;
        _dirty = false;
        Color fog = Pal.Fog, dark = Pal.Void;
        for (var ty = 0; ty < TH; ty++)
        {
            var row = ty / Sub * Level.W;
            for (var tx = 0; tx < TW; tx++)
            {
                var ti = ty * TW + tx;
                var e = _edge[ti];
                var a = Mathf.Max(_cur[row + tx / Sub], e);
                // nieznane pola i skraj odkrytej części: kolor tła; pola znane: fiolet mgły (jak mieszanie z BRAND_NAVY na GBA)
                var c = fog.Lerp(dark, Mathf.Max(e, a >= 0.99f ? 1f : 0f));
                var p = ti * 4;
                _px[p] = (byte)(c.R * 255);
                _px[p + 1] = (byte)(c.G * 255);
                _px[p + 2] = (byte)(c.B * 255);
                _px[p + 3] = (byte)(a * 255);
            }
        }
        _tex.Update(Image.CreateFromData(TW, TH, false, Image.Format.Rgba8, _px));
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (_tex is null) return;
        const int c = Assets.Cell;
        DrawTextureRect(_tex, new Rect2(0, 0, Level.W * c, Level.H * c), false);
        // poza mapą: jednolite tło
        DrawRect(new Rect2(-2000, -2000, 4000 + Level.W * c, 2000), Pal.Void);
        DrawRect(new Rect2(-2000, Level.H * c, 4000 + Level.W * c, 2000), Pal.Void);
        DrawRect(new Rect2(-2000, 0, 2000, Level.H * c), Pal.Void);
        DrawRect(new Rect2(Level.W * c, 0, 2000, Level.H * c), Pal.Void);
    }
}
