using Godot;
using LifeLike.Core;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game;

/// <summary>
/// Mgła wojny z miękkim światłem (odpowiednik 4 palet światła etapu na GBA: pełne przy bohaterze, 80%, 60% na skraju
/// pola widzenia, przyciemnione pola zapamiętane). Jeden piksel tekstury = jedno pole mapy; tekstura jest rysowana
/// z filtrowaniem liniowym, więc granice światła są płynne, a zmiany jasności przechodzą łagodnie przy ruchu.
/// </summary>
public partial class FogLayer : Node2D
{
    private CoreGame _g;
    private Image _img;
    private ImageTexture _tex;
    private readonly float[] _cur = new float[Level.W * Level.H];
    private readonly float[] _dst = new float[Level.W * Level.H];
    private readonly bool[] _known = new bool[Level.W * Level.H];
    private bool _dirty = true;

    public override void _Ready()
    {
        TextureFilter = TextureFilterEnum.Linear;
        _img = Image.CreateEmpty(Level.W, Level.H, false, Image.Format.Rgba8);
        _tex = ImageTexture.CreateFromImage(_img);
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
        _dirty = true;
    }

    public override void _Process(double delta)
    {
        if (_g is null || _img is null) return;
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
        for (var y = 0; y < Level.H; y++)
        {
            for (var x = 0; x < Level.W; x++)
            {
                var i = y * Level.W + x;
                var a = _cur[i];
                // nieznane pola: kolor tła; pola znane: fiolet mgły (jak mieszanie z BRAND_NAVY na GBA)
                var c = a >= 0.99f ? Pal.Void : Pal.Fog;
                _img.SetPixel(x, y, new Color(c, a));
            }
        }
        _tex.Update(_img);
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (_tex is null) return;
        const int c = Assets.Cell;
        // pół pola marginesu: środek teksela = środek pola
        DrawTextureRect(_tex, new Rect2(0, 0, Level.W * c, Level.H * c), false);
        // poza mapą: jednolite tło
        DrawRect(new Rect2(-2000, -2000, 4000 + Level.W * c, 2000), Pal.Void);
        DrawRect(new Rect2(-2000, Level.H * c, 4000 + Level.W * c, 2000), Pal.Void);
        DrawRect(new Rect2(-2000, 0, 2000, Level.H * c), Pal.Void);
        DrawRect(new Rect2(Level.W * c, 0, 2000, Level.H * c), Pal.Void);
    }
}
