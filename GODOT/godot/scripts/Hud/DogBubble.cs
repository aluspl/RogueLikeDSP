using System.Linq;
using Godot;
using LifeLike.Game.Gfx;
using LifeLike.Game.Settings;
using LifeLike.Game.World;

namespace LifeLike.Game.Hud;

/// <summary>
/// v0.23.0: easter egg – dymek Kawki nad nią na mapie: imię i sarkastyczna ocena postępów (3 linie z game.json, te same co na
/// GBA), ogonek do psa, serduszka po pogłaskaniu. Rysowany w warstwie banerów (stały rozmiar tekstu niezależnie od kamery),
/// kotwica przeliczana co klatkę z pozycji psa na mapie; zawsze w ekranie. Znika po Life s albo gdy Kawki nie widać.
/// </summary>
public partial class DogBubble : Control
{
    public const float Life = 5.5f;
    private const float PopIn = 0.15f, FadeOut = 0.4f;
    private const float Margin = 12f;   // odstęp dymka od krawędzi ekranu
    private static readonly Color Fill = new("fff6e8"), Edge = new("8a4a1c"), NameInk = new("8a4a1c");

    private WorldView _world;
    private string _name = "";
    private string[] _lines = [];
    private float _age = -1f;

    /// <summary>Zrzuty ekranu: zatrzymany wiek dymku (s; &lt; 0 – normalnie).</summary>
    public float FreezeAt { get; set; } = -1f;

    /// <summary>Dodatkowy warunek pokazania (np. telefon zamknięty).</summary>
    public System.Func<bool> Allowed { get; set; } = () => true;

    public bool Shown => _age >= 0f && _age < Life;

    /// <summary>Ostatnio pokazane linie (test dymny).</summary>
    public string[] Lines => _lines;

    /// <summary>Ile dymków pokazano (test dymny).</summary>
    public int Count { get; private set; }

    public void Bind(WorldView world) => _world = world;

    public void Show(string name, string[] lines)
    {
        _name = name;
        _lines = lines.Where(l => l.Length > 0).ToArray();
        _age = 0f;
        Count++;
        Visible = true;
        QueueRedraw();
    }

    public void HideBubble()
    {
        _age = -1f;
        Visible = false;
    }

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
    }

    public override void _Process(double delta)
    {
        if (_age < 0f) return;
        _age = FreezeAt >= 0f ? FreezeAt : _age + (float)delta;
        if (_age >= Life || _world?.Game is not { DogHere: true })
        {
            HideBubble();
            return;
        }
        QueueRedraw();
    }

    public override void _Draw()
    {
        try
        {
            DrawContent();
        }
        catch (System.Exception ex)
        {
            DrawErrors.Record("DogBubble", ex);
        }
    }

    private void DrawContent()
    {
        var g = _world?.Game;
        if (!Shown || g is null || !g.DogHere || !g.Visible(g.DogX, g.DogY) || !Allowed()) return;
        var head = _world.GridToScreen(g.DogX, g.DogY) + new Vector2(0, Proj.SpriteLift - 14);
        var anchor = (GetGlobalTransformWithCanvas().AffineInverse() * (_world.GetCanvasTransform() * head)).Round();
        var calm = GameSettings.ReduceMotion;
        var f = PixelFont.I;
        var cs = Mathf.Max(1f, Layout.ContentScale);
        var widest = _lines.Length == 0 ? 1 : _lines.Max(l => f.Measure(l));
        var room = Size.X - 2 * Margin;
        var ts = Mathf.Ceil(1.5f * cs - 0.01f) / cs; // ostry piksel fontu (jak dymek Kierownika)
        if (widest * ts + 20 > room) ts = 1f;   // v0.23.0: po zaokrągleniu skali dymek nie może dotknąć krawędzi ekranu
        var lineH = Mathf.Round(16 * ts) + 1;
        var w = Mathf.Round(Mathf.Max(widest * ts, f.Measure(_name)) + 20);
        var h = Mathf.Round(18 + _lines.Length * lineH + 8);
        var tail = 9f;
        var below = anchor.Y - h - tail - 8 < 6; // brak miejsca nad psem: pod nim
        var x = Mathf.Clamp(anchor.X - w / 2, Margin, Mathf.Max(Margin, Size.X - w - Margin));
        var y = below ? anchor.Y + 40 + tail : anchor.Y - h - tail;
        var pop = calm ? 1f : Mathf.Clamp(_age / PopIn, 0f, 1f);
        var fade = calm ? 1f : Mathf.Clamp((Life - _age) / FadeOut, 0f, 1f);
        Modulate = new Color(1, 1, 1, fade);
        var card = new Rect2(x, y, w, h);
        if (pop < 1f) // wyskakuje z psa
        {
            var k = 0.6f + 0.4f * pop;
            var c = card.GetCenter();
            card = new Rect2(c - card.Size * k / 2, card.Size * k);
        }
        // ogonek do psa
        var tx = Mathf.Clamp(anchor.X, card.Position.X + 12, card.End.X - 12);
        var baseY = below ? card.Position.Y + 1 : card.End.Y - 1;
        var tipY = below ? card.Position.Y - tail : card.End.Y + tail;
        var tri = new[] { new Vector2(tx - 7, baseY), new Vector2(tx + 7, baseY), new Vector2(tx + (anchor.X - tx) * 0.3f, tipY) };
        DrawHearts(anchor, calm); // pod dymkiem: serduszka nie zasłaniają tekstu
        DrawStyleBox(Ui.Box(new Color(0, 0, 0, 0.3f), 10), new Rect2(card.Position + new Vector2(0, 3), card.Size));
        DrawColoredPolygon([tri[0] + new Vector2(-2, 0), tri[1] + new Vector2(2, 0), tri[2] + new Vector2(0, below ? -2 : 2)], Edge);
        DrawStyleBox(Ui.Box(Fill, 10, Edge), card);
        DrawColoredPolygon(tri, Fill);
        if (pop >= 1f)
        {
            f.Draw(this, card.Position + new Vector2(10, 3), _name, new Ink(NameInk, Colors.Transparent), TextAlign.Left, 1, true);
            for (var i = 0; i < _lines.Length; i++)
                f.Draw(this, card.Position + new Vector2(10, 19 + i * lineH), _lines[i], Ink.Dark, TextAlign.Left, ts);
        }
    }

    /// <summary>Serduszka unoszą się z boku głowy Kawki przez pierwszą sekundę.</summary>
    private void DrawHearts(Vector2 anchor, bool calm)
    {
        if (calm) return;
        for (var i = 0; i < 3; i++)
        {
            var age = _age - i * 0.22f;
            if (age < 0f || age > 1.2f) continue;
            var s = 14f + 4f * (i % 2);
            var p = anchor + new Vector2(14f + i * 7f + Mathf.Sin(age * 6f + i) * 3f, 10f - age * 26f) - new Vector2(s / 2, s / 2);
            DrawTextureRectRegion(Assets.Actors, new Rect2(p.Round(), new Vector2(s, s)), Assets.Frame(Assets.FrameDogHeart, Assets.Actor),
                new Color(1, 1, 1, Mathf.Clamp((1.2f - age) / 0.4f, 0f, 1f)));
        }
    }
}
