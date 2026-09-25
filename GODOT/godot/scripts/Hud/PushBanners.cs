using System.Collections.Generic;
using Godot;
using LifeLike.Game.Audio;
using LifeLike.Game.Gfx;

namespace LifeLike.Game.Hud;

/// <summary>
/// Powiadomienia push jak z aplikacji PlanBudowlany (push_banner na GBA): biała karta zjeżdża z góry, ikona PB,
/// tytuł, treść i „teraz”; kilka banerów układa się w stos, każdy z dźwiękiem powiadomienia. Na mapie stos stoi
/// w prawym górnym rogu pod paskiem HUD (nie zasłania HP ani ostrzeżenia o ciosie bossa); przy telefonie -
/// wąska kolumna po jego lewej stronie. Kliknięcie banera z zakładką otwiera ją w telefonie.
/// </summary>
public partial class PushBanners : Control
{
    private const float Slide = 0.18f, Hold = 2.8f;
    private const int MaxVisible = 3, WideW = 272, CompactW = 180, H = 36, Gap = 4, Margin = 6;

    private readonly Queue<PushBanner> _queue = new();
    private readonly List<PushBanner> _active = new();

    public bool Busy => _active.Count > 0 || _queue.Count > 0;

    /// <summary>Telefon na ekranie: banery węższe, w kolumnie po lewej stronie telefonu (nie zasłaniają aplikacji).</summary>
    public bool Compact { get; set; }

    /// <summary>Górna krawędź stosu (pod paskiem HUD, gdy mapa jest widoczna).</summary>
    public float TopInset { get; set; } = Margin;

    private int W => Compact ? CompactW : WideW;

    private float Left => Compact ? 8f : Mathf.Round(Size.X - W - Margin);

    private float Top => Compact ? 40f : TopInset;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
    }

    /// <summary>Nowe powiadomienie; tab = zakładka telefonu otwierana kliknięciem (-1 brak).</summary>
    public void Push(string title, string body, int tab = -1)
    {
        if (_queue.Count >= 6) _queue.Dequeue();
        _queue.Enqueue(new PushBanner { Title = title, Body = body, Tab = tab });
    }

    public void Clear()
    {
        _queue.Clear();
        _active.Clear();
        QueueRedraw();
    }

    /// <summary>Zakładka banera pod punktem (współrzędne tej warstwy); -1 = pudło albo baner bez zakładki.</summary>
    public int TabAt(Vector2 p)
    {
        foreach (var b in _active)
        {
            if (b.Age <= Slide + Hold && new Rect2(Left, Mathf.Round(b.Y), W, H).HasPoint(p)) return b.Tab;
        }
        return -1;
    }

    /// <summary>Czy punkt trafia w którykolwiek widoczny baner (klik nie idzie wtedy w mapę).</summary>
    public bool Hit(Vector2 p)
    {
        foreach (var b in _active)
        {
            if (new Rect2(Left, Mathf.Round(b.Y), W, H).HasPoint(p)) return true;
        }
        return false;
    }

    public override void _Process(double delta)
    {
        var dt = (float)delta;
        while (_active.Count < MaxVisible && _queue.Count > 0)
        {
            var b = _queue.Dequeue();
            // pierwszy zjeżdża z góry, kolejne pojawiają się na swoim miejscu w stosie (bez przejazdu przez inne)
            b.Y = _active.Count == 0 ? Top - H - 14 : Top + _active.Count * (H + Gap) - 12;
            _active.Add(b);
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
            if (b.Age > Slide + Hold) continue; // znika w miejscu (przezroczystość), nie przejeżdża przez inne
            b.Y = Mathf.MoveToward(b.Y, Top + i * (H + Gap), dt / Slide * (H + 14));
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
        var x = Left;
        foreach (var b in _active)
        {
            var y = Mathf.Round(b.Y);
            // znikanie: przezroczystość w 8 krokach (style kart są buforowane wg koloru)
            var fade = b.Age > Slide + Hold ? Mathf.Clamp(1f - (b.Age - Slide - Hold) / Slide, 0f, 1f) : Mathf.Clamp(b.Age / Slide, 0f, 1f);
            fade = Mathf.Round(fade * 8f) / 8f;
            var r = new Rect2(x, y, W, H);
            DrawStyleBox(Ui.Box(new Color(0, 0, 0, 0.35f * fade), 7), new Rect2(r.Position + new Vector2(0, 2), r.Size));
            DrawStyleBox(Ui.Box(new Color(Pal.Card, fade), 7, new Color(Pal.Border, fade)), r);
            Assets.DrawFrame(this, Assets.PhoneIcons, 5, Assets.Icon, new Vector2(x + 8, y + 10), 1, new Color(1, 1, 1, fade));
            var stamp = b.Tab >= 0 && !Compact ? "otwórz >" : "teraz";
            if (!Compact) f.Draw(this, new Vector2(x + W - 8, y + 2), stamp, (b.Tab >= 0 ? Ink.Brand : Ink.Dim).WithAlpha(fade), TextAlign.Right);
            f.Draw(this, new Vector2(x + 30, y + 2), f.Fit(b.Title, W - 30 - (Compact ? 6 : 14 + f.Measure(stamp))), Ink.Dark.WithAlpha(fade));
            f.Draw(this, new Vector2(x + 30, y + 17), f.Fit(b.Body, W - 38), Ink.Dim.WithAlpha(fade));
        }
    }
}
