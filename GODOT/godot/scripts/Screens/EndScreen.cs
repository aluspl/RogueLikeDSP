using System.Collections.Generic;
using Godot;

namespace LifeLike.Game;

/// <summary>
/// Ekran końcowy jak na GBA: gradient fioletu, plansza z kodem QR do planbudowlany.online (ui/end.png w 2x),
/// wynik budowy, doświadczenie i rekord; przy odbiorze sypie się konfetti.
/// </summary>
public partial class EndScreen : Control
{
    public bool Won { get; set; }
    public string Line1 { get; set; } = "";
    public string Line2 { get; set; } = "";
    public string Note { get; set; } = "";

    private float _clock;
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
            _confetti.Add(new Vector4(_rnd.RandfRange(0, 640), -10, _rnd.RandfRange(40, 90), Assets.PConfetti + _rnd.RandiRange(0, 3)));
        for (var i = _confetti.Count - 1; i >= 0; i--)
        {
            var c = _confetti[i];
            c.Y += c.Z * dt;
            c.X += Mathf.Sin(_clock * 3 + i) * 12 * dt;
            _confetti[i] = c;
            if (c.Y > 370) _confetti.RemoveAt(i);
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
            DrawErrors.Record("EndScreen", ex);
        }
    }

    private void DrawContent()
    {
        var f = PixelFont.I;
        var w = Size.X;
        var h = Size.Y;
        Ui.VioletGradient(this, new Rect2(0, 0, w, h));
        DrawTextureRect(Assets.Tex("ui/end.png"), new Rect2((w - 480) / 2, 0, 480, 208), false);
        DrawRect(new Rect2(0, 210, w, 2), Pal.Accent);
        var title = Won ? "ODBIÓR ZALICZONY!" : "BUDOWA WSTRZYMANA";
        f.Draw(this, new Vector2(w / 2, 220), title, new Ink(Won ? new Color("b9f5c9") : new Color("ffd0c0"), new Color("2a1d80")), TextAlign.Center, 2);
        f.Draw(this, new Vector2(w / 2, 256), Line1, Ink.OnBrand, TextAlign.Center);
        f.Draw(this, new Vector2(w / 2, 273), Line2, Ink.OnBrand, TextAlign.Center);
        var y = 292f;
        foreach (var line in f.Wrap(Note, (int)w - 40))
        {
            if (y > 316) break;
            f.Draw(this, new Vector2(w / 2, y), line, new Ink(new Color("ffe08a"), new Color("2a1d80")), TextAlign.Center);
            y += 16;
        }
        var hint = Won ? "Spacja: kolejna budowa (NG+)   Enter: menu" : "Enter: menu";
        if (((int)(_clock * 1.5f) & 1) == 0) f.Draw(this, new Vector2(w / 2, h - 20), hint, Ink.OnBrand, TextAlign.Center);
        foreach (var c in _confetti)
            Assets.DrawFrame(this, Assets.Particles, (int)c.W, Assets.Particle, new Vector2(Mathf.Round(c.X), Mathf.Round(c.Y)));
    }
}
