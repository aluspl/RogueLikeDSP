using System.Collections.Generic;
using Godot;

namespace LifeLike.Game;

/// <summary>
/// Powiadomienia push jak z aplikacji PlanBudowlany (push_banner na GBA): biała karta zjeżdża z góry, ikona PB,
/// tytuł, treść i „teraz”; kilka banerów układa się w stos, każdy z dźwiękiem powiadomienia.
/// </summary>
public partial class PushBanners : Control
{
    private const float Slide = 0.18f, Hold = 2.8f;
    private const int MaxVisible = 3, WideW = 272, CompactW = 180, H = 36, Gap = 4;

    private sealed class Banner
    {
        public string Title = "";
        public string Body = "";
        public float Age;
        public float Y = -H - 8;
    }

    private readonly Queue<(string, string)> _queue = new();
    private readonly List<Banner> _active = new();

    public bool Busy => _active.Count > 0 || _queue.Count > 0;

    /// <summary>Telefon na ekranie: banery węższe, w kolumnie po lewej stronie telefonu (nie zasłaniają aplikacji).</summary>
    public bool Compact { get; set; }

    private int W => Compact ? CompactW : WideW;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
    }

    public void Push(string title, string body)
    {
        if (_queue.Count >= 6) _queue.Dequeue();
        _queue.Enqueue((title, body));
    }

    public void Clear()
    {
        _queue.Clear();
        _active.Clear();
        QueueRedraw();
    }

    public override void _Process(double delta)
    {
        var dt = (float)delta;
        while (_active.Count < MaxVisible && _queue.Count > 0)
        {
            var (t, b) = _queue.Dequeue();
            _active.Add(new Banner { Title = t, Body = b });
            Sfx.Play("notify", 0.7f);
        }
        for (var i = _active.Count - 1; i >= 0; i--)
        {
            _active[i].Age += dt;
            if (_active[i].Age > Slide * 2 + Hold) _active.RemoveAt(i);
        }
        for (var i = 0; i < _active.Count; i++)
        {
            var b = _active[i];
            var target = (Compact ? 40f : 6f) + i * (H + Gap);
            if (b.Age > Slide + Hold) continue; // znika w miejscu (przezroczystość), nie przejeżdża przez inne
            var speed = dt / Slide * (H + 14);
            b.Y = Mathf.MoveToward(b.Y, target, speed);
        }
        if (_active.Count > 0 || _queue.Count > 0) QueueRedraw();
    }

    public override void _Draw()
    {
        try
        {
            DrawContent();
        }
        catch (System.Exception ex)
        {
            DrawErrors.Record("PushBanners", ex);
        }
    }

    private void DrawContent()
    {
        var f = PixelFont.I;
        var x = Compact ? 8f : Mathf.Round((Size.X - W) / 2);
        foreach (var b in _active)
        {
            var y = Mathf.Round(b.Y);
            // znikanie: przezroczystość w 8 krokach (style kart są buforowane wg koloru)
            var fade = b.Age > Slide + Hold ? Mathf.Round(Mathf.Clamp(1f - (b.Age - Slide - Hold) / Slide, 0f, 1f) * 8f) / 8f : 1f;
            var r = new Rect2(x, y, W, H);
            DrawStyleBox(Ui.Box(new Color(0, 0, 0, 0.35f * fade), 7), new Rect2(r.Position + new Vector2(0, 2), r.Size));
            DrawStyleBox(Ui.Box(new Color(Pal.Card, fade), 7, new Color(Pal.Border, fade)), r);
            Assets.DrawFrame(this, Assets.PhoneIcons, 5, Assets.Icon, new Vector2(x + 8, y + 10), 1, new Color(1, 1, 1, fade));
            if (!Compact) f.Draw(this, new Vector2(x + W - 8, y + 2), "teraz", Ink.Dim.WithAlpha(fade), TextAlign.Right);
            f.Draw(this, new Vector2(x + 30, y + 2), f.Fit(b.Title, W - 30 - (Compact ? 6 : 44)), Ink.Dark.WithAlpha(fade));
            f.Draw(this, new Vector2(x + 30, y + 17), f.Fit(b.Body, W - 38), Ink.Dim.WithAlpha(fade));
        }
    }
}
