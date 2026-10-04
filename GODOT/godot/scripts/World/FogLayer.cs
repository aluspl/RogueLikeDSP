using Godot;
using LifeLike.Core;
using LifeLike.Game.Gfx;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.World;

/// <summary>
/// Mgła wojny ze światłem jak 4 palety światła etapu na GBA (pełne przy bohaterze, słabsze na skraju pola widzenia,
/// przyciemnione pola zapamiętane, ciemność nieznanych). v0.21.51 cz. 2: wygląd pikselowy zamiast rozmycia – mała
/// tekstura danych (piksel = pole mapy: R jasność, G pole znane) czytana bez filtrowania, a shader liczy kolor
/// w rozdzielczości piksela grafiki (1/16 pola = piksel GBA): skraj odkrytej części gaśnie w ciemność w 3 stopniach
/// (75% / 50% / 25% kraty Bayera, po 2 piksele grafiki – razem pół pola, także po skosie), a światło między polami
/// przechodzi progami co 0,145 z tym samym ditheringiem (pola zapamiętane trafiają w próg – jednolite) – bez gaussowskiej mgiełki i bez schodków na całe pole.
/// v0.21.54: kanał B = odkryty mur; piksele bryły muru (wierzch i wysokie lico sięgają Proj.WallH w górę) biorą światło i
/// krawędź ciemności od swojego muru, prostokąt mgły zaczyna się nad mapą, a wiersze mają wysokość Proj.RowH (płaski / 3/4).
/// Ciemność nieznanych pól i poza mapą jest lekko przezroczysta (widać przygaszone tło z paralaksą – SkyBackdrop).
/// </summary>
public partial class FogLayer : Node2D
{
    private const string ShaderCode = @"
shader_type canvas_item;
render_mode unshaded;
uniform sampler2D cells : filter_nearest, repeat_disable;
uniform vec2 size;
uniform vec4 fog_col : source_color;
uniform vec4 dark_col : source_color;
uniform float art = 16.0;   // pikseli grafiki na pole
uniform float step_a = 0.145; // próg jasności (0,58 pól zapamiętanych = 4 progi: bez kraty)
uniform float lift = 0.0;   // wysokość bryły muru w wierszach (prostokąt zaczyna się tyle wierszy nad mapą)
const float BAYER[16] = float[16](0.0, 8.0, 2.0, 10.0, 12.0, 4.0, 14.0, 6.0, 3.0, 11.0, 1.0, 9.0, 15.0, 7.0, 13.0, 5.0);
float bayer(vec2 p) {
    ivec2 i = ivec2(mod(p, 4.0));
    return (BAYER[i.x + i.y * 4] + 0.5) / 16.0;
}
bool inside(ivec2 c) { return c.x >= 0 && c.y >= 0 && c.x < int(size.x) && c.y < int(size.y); }
vec2 cell(ivec2 c) {
    if (!inside(c)) return vec2(1.0, 0.0);
    return texelFetch(cells, c, 0).rg;
}
bool wall(ivec2 c) { return inside(c) && texelFetch(cells, c, 0).b > 0.5; }
void fragment() {
    vec2 ap = floor(vec2(UV.x * size.x, UV.y * (size.y + lift) - lift) * art); // piksel grafiki
    vec2 pc = (ap + 0.5) / art;            // jego środek w polach
    ivec2 c = ivec2(floor(pc));
    vec2 f = fract(pc);
    // bryła muru sięga lift wierszy w górę: piksel należy do najbliższego (najniżej leżącego) muru, który go zakrywa
    bool on_wall = false;
    for (int k = 2; k >= 0; k--) {
        int yy = c.y + k;
        if (on_wall || float(yy) - lift > pc.y || !wall(ivec2(c.x, yy))) continue;
        on_wall = true;
        f.y = (pc.y - (float(yy) - lift)) / (1.0 + lift);
        c.y = yy;
    }
    float th = bayer(ap);
    if (cell(c).g < 0.5) {
        COLOR = dark_col;
    } else {
        float best = 9.0;
        for (int oy = -1; oy <= 1; oy++) {
            for (int ox = -1; ox <= 1; ox++) {
                if ((ox == 0 && oy == 0) || cell(c + ivec2(ox, oy)).g > 0.5) continue;
                float dx = ox < 0 ? f.x : (ox > 0 ? 1.0 - f.x : 0.0);
                float dy = oy < 0 ? f.y : (oy > 0 ? 1.0 - f.y : 0.0);
                best = min(best, length(vec2(dx, dy)));
            }
        }
        float d = best * art;              // odległość od nieznanego w pikselach grafiki
        float cov = d < 2.0 ? 0.75 : (d < 4.0 ? 0.5 : (d < 6.0 ? 0.25 : 0.0));
        if (d < 0.75) cov = 1.0;
        if (th < cov) {
            COLOR = dark_col;
        } else {
            vec2 q = vec2(pc.x, on_wall ? float(c.y) + 0.5 : pc.y) - 0.5; // mur: światło jego wiersza
            ivec2 b = ivec2(floor(q));
            vec2 t = fract(q);
            float sum = 0.0, wsum = 0.0;
            for (int k = 0; k < 4; k++) {
                ivec2 o = ivec2(k & 1, k >> 1);
                vec2 v = cell(b + o);
                if (v.g < 0.5) continue;
                float w = (o.x == 1 ? t.x : 1.0 - t.x) * (o.y == 1 ? t.y : 1.0 - t.y) + 0.0001;
                sum += v.r * w;
                wsum += w;
            }
            float l = wsum > 0.0 ? sum / wsum : 1.0;
            float a = clamp(floor(l / step_a + th) * step_a, 0.0, 1.0);
            COLOR = vec4(fog_col.rgb, a);
        }
    }
}";

    private CoreGame _g;
    private ImageTexture _tex;
    private ShaderMaterial _mat;
    private readonly byte[] _px = new byte[Level.W * Level.H * 4];
    private readonly float[] _cur = new float[Level.W * Level.H];
    private readonly float[] _dst = new float[Level.W * Level.H];
    private readonly bool[] _known = new bool[Level.W * Level.H];
    private readonly bool[] _wall = new bool[Level.W * Level.H];
    private bool _dirty = true;
    private Outside _outside;

    public override void _Ready()
    {
        TextureFilter = TextureFilterEnum.Nearest;
        _tex = ImageTexture.CreateFromImage(Image.CreateFromData(Level.W, Level.H, false, Image.Format.Rgba8, _px));
        _mat = new ShaderMaterial { Shader = new Shader { Code = ShaderCode } };
        _mat.SetShaderParameter("cells", _tex);
        _mat.SetShaderParameter("size", new Vector2(Level.W, Level.H));
        _mat.SetShaderParameter("fog_col", Pal.Fog);
        _mat.SetShaderParameter("dark_col", Pal.Void);
        _mat.SetShaderParameter("art", Assets.Cell / 2f);
        Material = _mat;
        _outside = new Outside();
        AddChild(_outside);
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
                _wall[i] = _known[i] && _g.Lv[x, y] == Tile.Wall;
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
        _dirty = true;
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
        for (var i = 0; i < _cur.Length; i++)
        {
            var p = i * 4;
            _px[p] = (byte)Mathf.RoundToInt(Mathf.Clamp(_cur[i], 0f, 1f) * 255);
            _px[p + 1] = (byte)(_known[i] ? 255 : 0);
            _px[p + 2] = (byte)(_wall[i] ? 255 : 0);
            _px[p + 3] = 255;
        }
        _tex.Update(Image.CreateFromData(Level.W, Level.H, false, Image.Format.Rgba8, _px));
        QueueRedraw();
    }

    /// <summary>v0.21.54: ciemność nieznanych pól i poza mapą przepuszcza trochę tła (SkyBackdrop – paralaksa).</summary>
    private const float VoidAlpha = 0.8f, OutsideAlpha = 0.5f;

    public override void _Draw()
    {
        if (_tex is null) return;
        var rh = Proj.RowH;
        var lift = Proj.WallH / (float)rh;
        _mat.SetShaderParameter("lift", lift);
        _mat.SetShaderParameter("dark_col", new Color(Pal.Void, VoidAlpha));
        DrawTextureRect(_tex, new Rect2(0, -Proj.WallH, Level.W * Assets.Cell, Level.H * rh + Proj.WallH), false);
    }

    /// <summary>Po zmianie widoku mapy: prostokąt mgły i tło poza mapą od nowa.</summary>
    public void Reproject()
    {
        QueueRedraw();
        _outside?.QueueRedraw();
    }

    /// <summary>Poza mapą: jednolite tło (osobny węzeł – bez shadera mgły).</summary>
    private sealed partial class Outside : Node2D
    {
        public override void _Draw()
        {
            const int c = Assets.Cell;
            var top = -Proj.WallH;
            var h = Level.H * Proj.RowH - top;
            var col = new Color(Pal.Void, OutsideAlpha);
            DrawRect(new Rect2(-2000, top - 2000, 4000 + Level.W * c, 2000), col);
            DrawRect(new Rect2(-2000, top + h, 4000 + Level.W * c, 2000), col);
            DrawRect(new Rect2(-2000, top, 2000, h), col);
            DrawRect(new Rect2(Level.W * c, top, 2000, h), col);
        }
    }
}
