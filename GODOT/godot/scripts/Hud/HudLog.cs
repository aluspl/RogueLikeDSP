using System.Collections.Generic;
using Godot;
using LifeLike.Core;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game;

/// <summary>
/// Dolny pas jak na GBA: nowe komunikaty dziennika pokazują się na chwilę w kolorach rodzaju (zły / dobry / łup)
/// i gasną; w tym samym miejscu podpowiedź menu akcji. Bez komunikatów - dyskretna ściąga sterowania.
/// </summary>
public partial class HudLog : Control
{
    private const float ShowTime = 3.2f, Fade = 0.8f;
    private CoreGame _g;
    private int _seen = -1;
    private float _timer;
    private readonly List<(string Text, Ink Ink)> _lines = new();
    private string[] _hint;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
    }

    public void SetGame(CoreGame g)
    {
        if (!ReferenceEquals(_g, g)) _seen = -1;
        _g = g;
        if (g.LogSerial == _seen) return;
        _seen = g.LogSerial;
        _lines.Clear();
        foreach (var m in g.Log)
        {
            if (m.N == 0) continue;
            var ink = m.Kind switch { LogKind.Bad => Ink.MapBad, LogKind.Good => Ink.MapGood, LogKind.Loot => Ink.MapLoot, _ => Ink.Map };
            _lines.Add((m.Repeat > 1 ? $"{m.Text} x{m.Repeat}" : m.Text, ink));
        }
        _timer = _lines.Count > 0 ? ShowTime + Fade : 0;
        QueueRedraw();
    }

    /// <summary>Podpowiedź (menu akcji): linie w kolorze łupu i zwykłym; null chowa.</summary>
    public void Hint(string first, string second)
    {
        _hint = first is null ? null : [first, second];
        QueueRedraw();
    }

    /// <summary>Wymusza wygaszenie dziennika (np. po zamknięciu menu).</summary>
    public void Silence()
    {
        _timer = 0;
        if (_g is not null) _seen = _g.LogSerial;
        QueueRedraw();
    }

    public override void _Process(double delta)
    {
        if (_timer <= 0) return;
        _timer = Mathf.Max(0, _timer - (float)delta);
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
            DrawErrors.Record("HudLog", ex);
        }
    }

    private void DrawContent()
    {
        if (_g is null) return;
        var f = PixelFont.I;
        var w = Size.X;
        var h = Size.Y;
        if (_hint is not null)
        {
            Strip(h - 38, 38, 1f);
            f.Draw(this, new Vector2(8, h - 36), f.Fit(_hint[0], (int)w - 16), Ink.MapLoot);
            f.Draw(this, new Vector2(8, h - 19), f.Fit(_hint[1] ?? "", (int)w - 16), Ink.Map);
            return;
        }
        if (_timer > 0 && _lines.Count > 0)
        {
            var a = _timer < Fade ? _timer / Fade : 1f;
            var n = _lines.Count;
            var top = h - 4 - n * 16;
            Strip(top - 2, h - top + 2, a);
            for (var i = 0; i < n; i++)
            {
                var age = n - 1 - i; // starsze linie bledsze
                var ink = _lines[i].Ink.WithAlpha(a * (age == 0 ? 1f : age == 1 ? 0.78f : 0.55f));
                f.Draw(this, new Vector2(8, top + i * 16), f.Fit(_lines[i].Text, (int)w - 16), ink);
            }
            return;
        }
        f.Draw(this, new Vector2(w - 6, h - 18), "Tab: telefon  Enter: akcje  R: moc  M: mapa", Ink.MapDim.WithAlpha(0.7f), TextAlign.Right);
    }

    private void Strip(float y, float height, float alpha)
    {
        DrawRect(new Rect2(0, y, Size.X, height), new Color(Pal.Text, 0.62f * alpha));
        DrawRect(new Rect2(0, y - 1, Size.X, 1), new Color(Pal.Brand, 0.55f * alpha));
    }
}
