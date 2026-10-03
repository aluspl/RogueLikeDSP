using LifeLike.Core;
using System.Collections.Generic;
using Godot;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;

namespace LifeLike.Game.Guide;

/// <summary>
/// Dymek samouczka menu (#25) nad przyciemnionym ekranem: „dziura” z pulsującą ramką nad omawianym elementem,
/// karta w stylu powiadomienia PlanBudowlany podpisana przez Kierownika Marka (awatar, tytuł, 3 linie, licznik kroków)
/// i przyciski Pomiń / (Statystyki) / Dalej. Rysuje tylko to, co ustawi Coach; prostokąty przycisków do dotyku.
/// </summary>
public partial class CoachView : Control, Touch.ITapTargets
{
    public Rect2 Hole { get; set; }
    public string Title { get; set; } = "";
    public string[] Lines { get; set; } = [];
    public string From { get; set; } = "";
    public string Pill { get; set; } = "";
    public bool PillBrand { get; set; }
    public bool Link { get; set; }
    public bool Last { get; set; }
    /// <summary>Dymek nowości (bez „Pomiń” - jeden krok).</summary>
    public bool Single { get; set; }
    /// <summary>Awatar: klatka postaci z actors.png (Kierownik budowy).</summary>
    public int Avatar { get; set; }

    private float _clock;
    private readonly List<(Rect2 Rect, CoachHit Hit)> _hits = new();

    /// <summary>Prostokąt karty dymka (ostatnio narysowany) - test układu.</summary>
    public Rect2 CardRect { get; private set; }

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
    }

    public override void _Process(double delta)
    {
        if (!Visible) return;
        _clock += (float)delta;
        QueueRedraw();
    }

    public void TapTargets(List<Rect2> into)
    {
        foreach (var (r, _) in _hits) into.Add(r);
    }

    public CoachHit HitAt(Vector2 p)
    {
        foreach (var (r, h) in _hits)
        {
            if (r.Grow(4).HasPoint(p)) return h;
        }
        return CoachHit.None;
    }

    public override void _Draw()
    {
        try
        {
            DrawContent();
        }
        catch (System.Exception ex)
        {
            DrawErrors.Record("CoachView", ex);
        }
    }

    private void DrawContent()
    {
        _hits.Clear();
        var f = PixelFont.I;
        var w = Size.X;
        var h = Size.Y;
        var dim = new Color(0.03f, 0.02f, 0.08f, 0.72f);
        var hasHole = Hole.Size.X > 0 && Hole.Size.Y > 0;
        var hole = hasHole ? Hole.Grow(4).Intersection(new Rect2(0, 0, w, h)) : new Rect2();
        if (hasHole)
        {
            DrawRect(new Rect2(0, 0, w, hole.Position.Y), dim);
            DrawRect(new Rect2(0, hole.End.Y, w, h - hole.End.Y), dim);
            DrawRect(new Rect2(0, hole.Position.Y, hole.Position.X, hole.Size.Y), dim);
            DrawRect(new Rect2(hole.End.X, hole.Position.Y, w - hole.End.X, hole.Size.Y), dim);
            var pulse = 0.5f + 0.5f * Mathf.Sin(_clock * 5f);
            DrawRect(hole.Grow(1 + pulse), new Color(Pal.Accent, 0.9f), false, 2f);
            DrawRect(hole.Grow(4 + 2 * pulse), new Color(Pal.Accent, 0.25f), false, 2f);
        }
        else DrawRect(new Rect2(0, 0, w, h), dim);

        // karta dymka: szerokość jak powiadomienie, pod albo nad podświetleniem (tam, gdzie więcej miejsca)
        var safe = Layout.SafeArea;
        var cw = Mathf.Min(w - 24, 380f);
        var lineH = 16f;
        var btnH = Layout.Touch || Layout.Portrait ? Mathf.Max(34f, Layout.TouchTarget - 6) : 24f;
        var ch = 10 + 34 + 4 + 18 + Lines.Length * lineH + 10 + btnH + 10;
        var cx = Mathf.Round((w - cw) / 2);
        float cy;
        if (!hasHole) cy = Mathf.Round((h - ch) / 2);
        else
        {
            var above = hole.Position.Y - safe.Position.Y;
            var below = safe.End.Y - hole.End.Y;
            var left = hole.Position.X - safe.Position.X;
            var rightRoom = safe.End.X - hole.End.X;
            if (below < ch + 16 && above < ch + 16 && Mathf.Max(left, rightRoom) >= 250)   // obok podświetlenia
            {
                var side = Mathf.Max(left, rightRoom);
                cw = Mathf.Min(cw, side - 16);
                cx = left >= rightRoom ? Mathf.Round(hole.Position.X - 12 - cw) : Mathf.Round(hole.End.X + 12);
                cy = Fit(Mathf.Round(hole.GetCenter().Y - ch / 2), safe.Position.Y + 6, safe.End.Y - ch - 6);
            }
            else
            {
                cy = below >= ch + 16 || below >= above ? hole.End.Y + 12 : hole.Position.Y - 12 - ch;
                cy = Fit(cy, safe.Position.Y + 6, safe.End.Y - ch - 6);
                if (hole.Size.X < w * 0.6f) cx = Fit(Mathf.Round(hole.GetCenter().X - cw / 2), safe.Position.X + 8, safe.End.X - cw - 8);
            }
        }
        var card = new Rect2(cx, Mathf.Round(cy), cw, ch);
        CardRect = card;
        DrawStyleBox(Ui.Box(new Color(0, 0, 0, 0.35f), 14), new Rect2(card.Position + new Vector2(0, 4), card.Size));
        DrawStyleBox(Ui.Box(Pal.Card, 14, Pal.Accent), card);

        // nagłówek jak powiadomienie: awatar, nadawca, pastylka (licznik / nowość)
        var x = card.Position.X + 10;
        var y = card.Position.Y + 10;
        DrawStyleBox(Ui.Box(Pal.Group, 8), new Rect2(x, y, 34, 34));
        DrawTextureRectRegion(Assets.Actors, new Rect2(x + 1, y + 1, 32, 32), Assets.Frame(Avatar, Assets.Actor));
        var right = card.End.X - 10;
        var pw = 0f;
        if (Pill.Length > 0)
        {
            pw = f.Measure(Pill) + 12;
            var pr = new Rect2(right - pw, y + 2, pw, 15);
            DrawStyleBox(Ui.Box(PillBrand ? Pal.Brand : Pal.Group, 7), pr);
            f.Draw(this, new Vector2(pr.GetCenter().X, pr.Position.Y - 1), Pill, PillBrand ? Ink.White : Ink.Brand, TextAlign.Center);
        }
        f.Draw(this, new Vector2(x + 42, y), f.Fit(From, (int)(right - pw - 8 - x - 42)), Ink.Dim);
        f.Draw(this, new Vector2(x + 42, y + 16), f.Fit(Title, (int)(right - x - 42)), Ink.Brand, TextAlign.Left, 1, true);
        y += 34 + 4;

        // dymek wiadomości (3 linie z game.json - te same co na GBA)
        var bubble = new Rect2(x, y, card.Size.X - 20, 12 + Lines.Length * lineH);
        DrawStyleBox(Ui.Box(Pal.Group, 10), bubble);
        for (var i = 0; i < Lines.Length; i++)
            f.Draw(this, new Vector2(x + 8, y + 5 + i * lineH), f.Fit(Lines[i], (int)bubble.Size.X - 14), Ink.Dark);
        y = bubble.End.Y + 10;

        // przyciski
        var gap = 6f;
        var n = Link ? 3 : 2;
        var bw = (card.Size.X - 20 - gap * (n - 1)) / n;
        var bx = x;
        if (!Single)
        {
            Button(new Rect2(bx, y, bw, btnH), Loc.T("pomin") + ButtonNames.Pick(" (Esc)", ""), false, CoachHit.Skip);
            bx += bw + gap;
        }
        else bw = (card.Size.X - 20 - gap * (n - 2)) / (n - 1);
        if (Link)
        {
            Button(new Rect2(bx, y, bw, btnH), Loc.T("statystyki") + ButtonNames.Pick(" (I)", ""), false, CoachHit.Link);
            bx += bw + gap;
        }
        Button(new Rect2(bx, y, card.End.X - 10 - bx, btnH), (Last ? Loc.T("gotowe") : Loc.T("dalej")) + ButtonNames.Pick(Loc.T("spacja_2"), ""), true, CoachHit.Next);
    }

    /// <summary>Clamp odporny na mały ekran (min > max: min).</summary>
    private static float Fit(float v, float min, float max) => Mathf.Max(min, Mathf.Min(v, max));

    private void Button(Rect2 r, string label, bool primary, CoachHit hit)
    {
        var f = PixelFont.I;
        DrawStyleBox(Ui.Box(primary ? Pal.Brand : Pal.Card, 9, primary ? Pal.Accent : Pal.Border), r);
        var k = label.IndexOf(" (");   // wąski przycisk: bez nazwy klawisza
        if (k > 0 && f.Measure(label, 1, true) > r.Size.X - 8) label = label[..k];
        f.Draw(this, new Vector2(r.GetCenter().X, Mathf.Round(r.GetCenter().Y - 9)), f.Fit(label, (int)r.Size.X - 8), primary ? Ink.White : Ink.Brand, TextAlign.Center, 1, true);
        _hits.Add((r, hit));
    }
}
