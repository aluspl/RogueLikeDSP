using System.Collections.Generic;
using Godot;
using LifeLike.Game.Gfx;

namespace LifeLike.Game.Screens.Views;

/// <summary>
/// Ekran końcowy jak na GBA: gradient fioletu, plansza z kodem QR do planbudowlany.online (ui/end.png w 2x),
/// wynik budowy, doświadczenie i rekord; przy odbiorze sypie się konfetti.
/// </summary>
public partial class EndView : Control
{
    public bool Won { get; set; }
    public string Line1 { get; set; } = "";
    public string Line2 { get; set; } = "";
    public string Note { get; set; } = "";

    private float _clock;
    private Rect2 _next, _menu;

    /// <summary>Przycisk pod punktem: 1 = kolejna budowa (NG+), 2 = menu, 0 = nic.</summary>
    public int ButtonAt(Vector2 p) => Won && _next.HasPoint(p) ? 1 : _menu.HasPoint(p) ? 2 : 0;
    private readonly List<Vector4> _confetti = new(); // x, y, prędkość, klatka
    private readonly RandomNumberGenerator _rnd = new();

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        _rnd.Seed = 7;
    }

    public override void _Process(double delta)
    {
        if (!Visible) return;
        var dt = (float)delta;
        _clock += dt;
        if (Won && _confetti.Count < 60 && _rnd.Randf() < 0.5f)
            _confetti.Add(new Vector4(_rnd.RandfRange(0, Size.X), -10, _rnd.RandfRange(40, 90), Assets.PConfetti + _rnd.RandiRange(0, 3)));
        for (var i = _confetti.Count - 1; i >= 0; i--)
        {
            var c = _confetti[i];
            c.Y += c.Z * dt;
            c.X += Mathf.Sin(_clock * 3 + i) * 12 * dt;
            _confetti[i] = c;
            if (c.Y > Size.Y + 10) _confetti.RemoveAt(i);
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
            DrawErrors.Record("EndView", ex);
        }
    }

    private void DrawContent()
    {
        var f = PixelFont.I;
        var w = Size.X;
        var h = Size.Y;
        var portrait = h > w;
        Ui.VioletGradient(this, new Rect2(0, 0, w, h));
        var es = portrait ? Mathf.Min(2f, Mathf.Floor((w - 16) / 240f * 2) / 2) : 2f;
        var top = portrait ? Layout.SafeTop + 24 : 0;
        DrawTextureRect(Assets.Tex("ui/end.png"), new Rect2(Mathf.Round((w - 240 * es) / 2), top, 240 * es, 104 * es), false);
        var y = top + 104 * es + 2;
        DrawRect(new Rect2(0, y, w, 2), Pal.Accent);
        var title = Won ? "ODBIÓR ZALICZONY!" : "BUDOWA WSTRZYMANA";
        var ts = f.Measure(title, 2) <= w - 16 ? 2f : 1.5f;
        f.Draw(this, new Vector2(w / 2, y + 10), title, Won ? Ink.EndWin : Ink.EndLose, TextAlign.Center, ts);
        y += 14 + PixelFont.LineHeight * ts + 8;
        var body = new List<(string, Ink)>();
        foreach (var l in portrait ? f.Wrap(Line1, (int)w - 24) : [Line1]) body.Add((l, Ink.OnBrand));
        foreach (var l in portrait ? f.Wrap(Line2, (int)w - 24) : [Line2]) body.Add((l, Ink.OnBrand));
        foreach (var (l, ink) in body)
        {
            f.Draw(this, new Vector2(w / 2, y), l, ink, TextAlign.Center);
            y += 17;
        }
        y += 2;
        var noteEnd = portrait ? h - Layout.SafeBottom - 140 : 316;
        foreach (var line in f.Wrap(Note, (int)w - 40))
        {
            if (y > noteEnd) break;
            f.Draw(this, new Vector2(w / 2, y), line, Ink.EndNote, TextAlign.Center);
            y += 16;
        }
        _next = _menu = new Rect2();
        if (Layout.Touch || portrait)
        {
            var bh = portrait ? 52f : 26f;
            var by = h - Mathf.Max(Layout.SafeBottom, 8) - bh - (portrait ? 12 : 4);
            if (Won)
            {
                _next = new Rect2(16, by - bh - 10, w - 32, bh);
                DrawButton(_next, "Kolejna budowa (NG+)", true);
            }
            _menu = new Rect2(16, by, w - 32, bh);
            DrawButton(_menu, "Menu", !Won);
        }
        else
        {
            var hint = Won ? "Spacja: kolejna budowa (NG+)   Enter: menu" : "Enter: menu";
            if (((int)(_clock * 1.5f) & 1) == 0) f.Draw(this, new Vector2(w / 2, h - 20), hint, Ink.OnBrand, TextAlign.Center);
        }
        foreach (var c in _confetti)
            Assets.DrawFrame(this, Assets.Particles, (int)c.W, Assets.Particle, new Vector2(Mathf.Round(c.X), Mathf.Round(c.Y)));
    }

    private void DrawButton(Rect2 r, string label, bool primary)
    {
        var f = PixelFont.I;
        DrawStyleBox(Ui.Box(primary ? Pal.Card : new Color(Pal.Card, 0.14f), 11, primary ? Pal.Accent : new Color(Pal.Card, 0.5f)), r);
        f.Draw(this, new Vector2(r.GetCenter().X, Mathf.Round(r.GetCenter().Y - 9)), label, primary ? Ink.Brand : Ink.OnBrand, TextAlign.Center, 1, true);
    }
}
