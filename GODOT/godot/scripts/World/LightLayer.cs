using System.Collections.Generic;
using Godot;
using LifeLike.Core;
using LifeLike.Game.Gfx;
using LifeLike.Game.Settings;

namespace LifeLike.Game.World;

/// <summary>
/// v0.21.54: światło dynamiczne (tylko wygląd – pole widzenia liczy rdzeń jak dotąd). Warstwa addytywna pod mgłą:
/// latarka czołowa bohatera (miękkie koło światła w stopniach jak mgła, lekko przesunięte w stronę patrzenia, subtelne
/// migotanie – bez niego przy „ograniczonym ruchu”), odblaski latarki w widocznych kałużach blisko bohatera, poświata
/// iskier i piorunów z FxLayer oraz krótkie błyski (wybuch, kombinacje stanów). Wyłączana w ustawieniach
/// („Efekty świetlne”) – wtedy węzeł jest ukryty i nic nie liczy. W widoku 3/4 koła światła są spłaszczone jak wiersz.
/// </summary>
public partial class LightLayer : Node2D
{
    private const int MaxFlashes = 24;
    private static readonly Color Lamp = new(1f, 0.86f, 0.62f);
    private static readonly Color Glint = new(0.78f, 0.9f, 1f);
    private static readonly Color SparkGlow = new(1f, 0.7f, 0.35f);

    private sealed class LightFlash
    {
        public Vector2 Pos;
        public Color Color;
        public float Radius;
        public float Life;
        public float Age;
    }

    private readonly List<LightFlash> _flashes = new();
    private WorldView _w;
    private ImageTexture _soft;
    private float _clock;

    public int FlashCount => _flashes.Count;

    public void Bind(WorldView w) => _w = w;

    public override void _Ready()
    {
        Material = new CanvasItemMaterial { BlendMode = CanvasItemMaterial.BlendModeEnum.Add };
        TextureFilter = TextureFilterEnum.Nearest;
        _soft = SoftDisc(64);
    }

    /// <summary>Koło światła w 6 stopniach jasności (pikselowe, jak progi mgły), białe – kolor nadaje modulacja.</summary>
    private static ImageTexture SoftDisc(int size)
    {
        var img = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
        var h = size / 2f;
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var d = new Vector2(x + 0.5f - h, y + 0.5f - h).Length() / h;
                var a = Mathf.Clamp(1f - d, 0f, 1f);
                a = Mathf.Floor(Mathf.Pow(a, 1.4f) * 6f) / 6f;
                img.SetPixel(x, y, new Color(1, 1, 1, a));
            }
        }
        return ImageTexture.CreateFromImage(img);
    }

    /// <summary>Krótki błysk światła (wybuch, kombinacja): promień w pikselach świata, czas w sekundach.</summary>
    public void Flash(Vector2 pos, Color color, float radius, float life)
    {
        if (!GameSettings.Lights) return;
        if (_flashes.Count >= MaxFlashes) _flashes.RemoveAt(0);
        _flashes.Add(new LightFlash { Pos = pos, Color = color, Radius = radius, Life = life });
    }

    public void Clear() => _flashes.Clear();

    public override void _Process(double delta)
    {
        Visible = GameSettings.Lights;
        if (!Visible)
        {
            _flashes.Clear();
            return;
        }
        var dt = (float)delta;
        _clock += dt;
        for (var i = _flashes.Count - 1; i >= 0; i--)
        {
            _flashes[i].Age += dt;
            if (_flashes[i].Age >= _flashes[i].Life) _flashes.RemoveAt(i);
        }
        QueueRedraw();
    }

    private void Light(Vector2 center, float radius, Color color, float strength)
    {
        var ry = radius * Proj.RowH / Assets.Cell;
        DrawTextureRect(_soft, new Rect2(center.X - radius, center.Y - ry, radius * 2, ry * 2), false, new Color(color * strength, 1f));
    }

    public override void _Draw()
    {
        var g = _w?.Game;
        var hero = _w?.HeroSprite;
        if (g is null || hero is null || !hero.Visible || _soft is null) return;
        var flicker = GameSettings.ReduceMotion ? 1f : 1f + 0.035f * Mathf.Sin(_clock * 11.3f) + 0.025f * Mathf.Sin(_clock * 17.9f + 1.3f);
        var dir = hero.Flip ? -1f : 1f;
        var lift = new Vector2(0, Proj.SpriteLift);
        var c = Assets.Cell;
        if (g.Hero.Alive)
        {
            Light(hero.Position + lift + new Vector2(dir * 10, -2), c * 3.4f * flicker, Lamp, 0.16f * flicker);
            Light(hero.Position + lift + new Vector2(dir * 6, -6), c * 1.4f, Lamp, 0.10f);
        }
        // odblask latarki w kałużach: im bliżej bohatera, tym jaśniej; błysk po stronie dalszej od bohatera
        int hx = g.Hero.X, hy = g.Hero.Y;
        for (var y = hy - 4; y <= hy + 4; y++)
        {
            for (var x = hx - 4; x <= hx + 4; x++)
            {
                if (!Level.In(x, y) || !g.Visible(x, y) || !g.Puddle(x, y)) continue;
                var p = Proj.Center(x, y) + new Vector2(0, 2 * Proj.RowH / (float)c);
                var away = (p - hero.Position).Normalized();
                var k = 1f - new Vector2(x - hx, y - hy).Length() / 5f;
                if (k <= 0) continue;
                var shimmer = GameSettings.ReduceMotion ? 1f : 0.8f + 0.2f * Mathf.Sin(_clock * 3f + x * 1.7f + y);
                Light(p + away * 4, 12f, Glint, 0.55f * k * shimmer);
                var s = p + away * 5 + new Vector2(-4, -1);
                DrawLine(s, s + new Vector2(7, 0), new Color(Glint * (0.5f * k * shimmer), 1f), 2f);
            }
        }
        foreach (var pt in _w.Fx.Particles) // iskry i pioruny świecą
        {
            if (pt.Frame is < Assets.PSpark or > Assets.PSpark + 1 && pt.Frame != Assets.PBolt) continue;
            var fade = 1f - pt.Age / (float)pt.Life;
            Light(pt.Pos, 14f, pt.Frame == Assets.PBolt ? Glint : SparkGlow, 0.35f * fade);
        }
        foreach (var f in _flashes)
        {
            var t = f.Age / f.Life;
            var a = t < 0.15f ? t / 0.15f : 1f - (t - 0.15f) / 0.85f;
            Light(f.Pos, f.Radius * (0.7f + 0.3f * t), f.Color, 0.45f * a);
        }
    }
}
