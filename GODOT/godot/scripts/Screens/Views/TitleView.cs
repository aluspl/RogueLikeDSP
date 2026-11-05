using LifeLike.Core;
using System.Collections.Generic;
using Godot;
using LifeLike.Game.Gfx;
using LifeLike.Game.Touch;

namespace LifeLike.Game.Screens.Views;

/// <summary>
/// Ekran tytułowy jak na GBA: gradient fioletu marki, logo PB z napisem (ui/title.png z GBA), pas ostrzegawczy
/// placu budowy na dole, wersja z game.json w lewym górnym rogu, rekord, menu z wyborem (przy przerwanej budowie
/// najpierw „Kontynuuj budowę”), link planbudowlany.online. Pionowo (telefon) menu to duże przyciski pod logo.
/// Prostokąty przycisków i linku są zapamiętywane do dotyku / kliknięcia.
/// </summary>
public partial class TitleView : Control, Touch.ITapTargets
{
    public const string Link = "planbudowlany.online";

    public string[] Items { get; set; } = [];

    /// <summary>v0.22.0 (UX A): wiersz każdej pozycji (dwie pozycje w wierszu = dwa przyciski obok siebie).</summary>
    public int[] ItemRows { get; set; } = [];

    /// <summary>v0.22.0 (UX A): nagłówek grupy nad wierszem ("" = bez nagłówka).</summary>
    public string[] RowGroups { get; set; } = [];

    /// <summary>v0.22.0 (UX A): karta „Twoje postępy” (od pierwszej budowy): linia, przycisk, czy przycisk aktywny.</summary>
    public bool CardOn { get; set; }
    public string CardLine { get; set; } = "";
    public string CardButton { get; set; } = "";
    public bool CardActive { get; set; }
    public string Version { get; set; } = "";
    public int Best { get; set; }
    public int Runs { get; set; }
    public int Xp { get; set; }
    public int Sel { get; set; }
    public string Note { get; set; } = "";
    public string Info { get; set; } = "";
    /// <summary>v0.21.52 cz. b (#44): poziom inspektora (-1 = ukryty: przed pierwszą budową) i pasek do kolejnego 0..1.</summary>
    public int InspLevel { get; set; } = -1;
    public float InspFill { get; set; }
    public bool InspMax { get; set; }
    /// <summary>v0.21.52 cz. c: zadania dnia („Zadania 1/3”) i seria dni pod rekordem ("" = ukryte).</summary>
    public string Goals { get; set; } = "";
    public string Streak { get; set; } = "";

    private float _clock;
    private readonly List<Rect2> _items = new();
    private Rect2 _link;
    private Rect2 _card;

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

    /// <summary>Pozycja menu pod punktem (piksele UI) albo -1.</summary>
    public int ItemAt(Vector2 p)
    {
        for (var i = 0; i < _items.Count; i++)
        {
            if (_items[i].Grow(2).HasPoint(p)) return i;
        }
        return -1;
    }

    public void TapTargets(List<Rect2> into)
    {
        into.AddRange(_items);
        into.Add(_link);
        if (CardOn && Note.Length == 0) into.Add(_card);
    }

    public bool LinkAt(Vector2 p) => _link.Grow(4).HasPoint(p);

    /// <summary>v0.22.0 (UX A): przycisk „Wydaj doświadczenie” w karcie postępów pod punktem.</summary>
    public bool CardAt(Vector2 p) => CardOn && Note.Length == 0 && _card.Grow(2).HasPoint(p);

    /// <summary>Prostokąt pozycji menu (samouczek: podświetlenie), pusty przed pierwszym rysowaniem.</summary>
    public Rect2 ItemRect(int i) => i >= 0 && i < _items.Count ? _items[i] : new Rect2();

    public override void _Draw()
    {
        try
        {
            DrawContent();
        }
        catch (System.Exception ex)
        {
            DrawErrors.Record("TitleView", ex);
        }
    }

    /// <summary>v0.22.0 (UX A): dolna krawędź bloku rekord / zadania / seria i klucza ustawień (prawa góra) – pod nią banery.</summary>
    public static float StatsBottom => Mathf.Max(Layout.SafeTop + 4 + 3 * (PixelFont.LineHeight + 2), Hud.SettingsButton.Rect.End.Y);

    private void DrawContent()
    {
        var f = PixelFont.I;
        var w = Size.X;
        var h = Size.Y;
        var portrait = Layout.Portrait; // v0.22.0 (UX A): także duży ekran prawie kwadratowy
        var safe = Layout.SafeArea;
        Ui.VioletGradient(this, new Rect2(0, 0, w, h));
        var logo = Assets.Tex(Loc.English ? "ui/title_en.png" : "ui/title.png"); // v0.21.53 cz. 2: hasło po angielsku
        var bob = Mathf.Round(Mathf.Sin(_clock * 1.6f) * 2f);
        var ls = portrait ? Mathf.Min(2f, Mathf.Floor((w - 16) / logo.GetWidth() * 2) / 2) : h >= 440 ? 2f : 1.5f;
        var menu = MenuLayout(portrait, w, h, safe);
        while (portrait && ls > 0.5f && safe.Position.Y + 40 + logo.GetHeight() * ls + 16 + menu.Need > menu.Limit) ls -= 0.5f; // niski ekran (składak): mniejsze logo
        var lsz = logo.GetSize() * ls;
        var logoY = portrait ? safe.Position.Y + 40 : -6 * ls;
        DrawTextureRect(logo, new Rect2(Mathf.Round((w - lsz.X) / 2), logoY + bob, lsz.X, lsz.Y), false);
        Ui.WarningStripe(this, new Rect2(0, h - 14, w, 14), _clock * 12f);

        var top = safe.Position.Y + 4;
        f.Draw(this, new Vector2(safe.Position.X + 6, top), Version, Ink.OnBrand);
        if (InspLevel >= 0) // poziom inspektora pod wersją: napis i pasek (żółty jak na GBA)
        {
            var ix = safe.Position.X + 6;
            var iy = top + PixelFont.LineHeight + 2;
            var label = Loc.F("inspektor_3", InspLevel) + (InspMax ? " MAX" : "");
            f.Draw(this, new Vector2(ix, iy), label, Ink.OnBrand);
            var bar = new Rect2(ix, iy + PixelFont.LineHeight + 2, Mathf.Max(64, f.Measure(label)), 7);
            DrawRect(bar.Grow(1), Pal.HpEdge);
            DrawRect(bar, Pal.HpBack);
            DrawRect(new Rect2(bar.Position, new Vector2(Mathf.Round(bar.Size.X * Mathf.Clamp(InspFill, 0f, 1f)), bar.Size.Y)), Pal.HpMain[1]);
        }
        var recordX = safe.End.X - Hud.SettingsButton.Side - 10;
        if (Best > 0) f.Draw(this, new Vector2(recordX, top), Loc.F("rekord_5", Best), Ink.OnBrand, TextAlign.Right);
        if (Goals.Length > 0) f.Draw(this, new Vector2(recordX, top + PixelFont.LineHeight + 2), Goals, Ink.OnBrand, TextAlign.Right);
        if (Streak.Length > 0) f.Draw(this, new Vector2(recordX, top + 2 * (PixelFont.LineHeight + 2)), Streak, Ink.OnBrand, TextAlign.Right);

        // menu (v0.22.0, UX A): wiersze w grupach, pionowo przyciski jak cele dotyku, poziomo jak na GBA
        _items.Clear();
        for (var i = 0; i < Items.Length; i++) _items.Add(new Rect2());
        var y = Mathf.Round(logoY + lsz.Y + (portrait ? 16 : 4));
        var (rowH, gap, hdr, ts) = (menu.RowH, menu.Gap, menu.Hdr, menu.Ts);
        var colW = portrait ? Mathf.Min(w - 64, 420f) : w;
        var colX = Mathf.Round((w - colW) / 2);
        var rows = RowCount();
        for (var row = 0; row < rows; row++)
        {
            var group = row < RowGroups.Length ? RowGroups[row] : "";
            if (group.Length > 0 && hdr > 0)
            {
                f.Draw(this, new Vector2(portrait ? colX + 4 : w / 2, y), group.ToUpperInvariant(), Ink.OnBrand.WithAlpha(0.7f), portrait ? TextAlign.Left : TextAlign.Center);
                y += hdr;
            }
            var idx = new List<int>();
            for (var i = 0; i < Items.Length; i++)
            {
                if (ItemRows[i] == row) idx.Add(i);
            }
            if (portrait)
            {
                var bw = (colW - (idx.Count - 1) * 8) / idx.Count;
                for (var k = 0; k < idx.Count; k++) DrawItem(idx[k], new Rect2(colX + k * (bw + 8), y, bw, rowH), ts, true);
            }
            else
            {
                var tws = idx.ConvertAll(i => f.Measure(Items[i], ts) + 40f);
                var total = 0f;
                foreach (var t in tws) total += t;
                var x = Mathf.Round((w - total - (idx.Count - 1) * 8) / 2);
                for (var k = 0; k < idx.Count; k++)
                {
                    DrawItem(idx[k], new Rect2(x, y, tws[k], rowH), ts, false);
                    x += tws[k] + 8;
                }
            }
            y += rowH + gap;
        }

        // karta „Twoje postępy” (notatka ma pierwszeństwo) i link do planbudowlany.online
        var lw = f.Measure(Link) + 30;
        var card = CardOn && Note.Length == 0;
        if (portrait)
        {
            var ly = h - Mathf.Max(Layout.SafeBottom, 14) - 48;
            _link = new Rect2(Mathf.Round((w - lw) / 2), ly, lw, 36);
            if (card)
            {
                var cr = new Rect2(colX, ly - 8 - CardH, colW, CardH);
                DrawStyleBox(Ui.Box(new Color(0, 0, 0, 0.25f), 12), new Rect2(cr.Position + new Vector2(0, 2), cr.Size));
                DrawStyleBox(Ui.Box(Pal.Card, 12), cr);
                var tw = _slim ? cr.Size.X * 0.45f : cr.Size.X - 24;
                f.Draw(this, cr.Position + new Vector2(12, 8), f.Fit(Loc.T("twoje_postepy"), (int)tw), Ink.Dark, TextAlign.Left, 1, true);
                f.Draw(this, cr.Position + new Vector2(12, 26), f.Fit(CardLine, (int)tw), Ink.Dim);
                _card = _slim ? new Rect2(cr.Position.X + 24 + tw, cr.Position.Y + 6, cr.Size.X - 30 - tw, Layout.TouchTarget)
                    : new Rect2(cr.Position.X + 10, cr.End.Y - 10 - Layout.TouchTarget, cr.Size.X - 20, Layout.TouchTarget);
                DrawStyleBox(Ui.Box(CardActive ? Pal.Brand : Pal.Group, 10), _card);
                f.Draw(this, new Vector2(_card.GetCenter().X, Mathf.Round(_card.GetCenter().Y - 8)), f.Fit(CardButton, (int)_card.Size.X - 12), CardActive ? Ink.OnBrand : Ink.Dim, TextAlign.Center);
            }
            else if (Note.Length > 0)
            {
                var lines = f.Wrap(Note, (int)w - 24);
                for (var k = 0; k < lines.Count && k < 2; k++)
                    f.Draw(this, new Vector2(w / 2, ly - 20 - (Mathf.Min(lines.Count, 2) - 1 - k) * 16), lines[k], Ink.NoteOnBrand, TextAlign.Center);
            }
        }
        else
        {
            _link = new Rect2(Mathf.Round(safe.End.X - lw - 6), h - 38, lw, 22);
            var room = (int)(_link.Position.X - safe.Position.X - 16);
            if (card)
            {
                var bw = f.Measure(CardButton) + 20;
                var tw = Mathf.Min(f.Measure(CardLine), room - bw - 10);
                f.Draw(this, new Vector2(safe.Position.X + 8, h - 34), f.Fit(CardLine, (int)tw), Ink.OnBrand);
                _card = new Rect2(safe.Position.X + 8 + tw + 10, h - 38, bw, 22);
                DrawStyleBox(Ui.Box(CardActive ? Pal.Card : new Color(Pal.Card, 0.14f), 8, new Color(Pal.Card, 0.5f)), _card);
                f.Draw(this, new Vector2(_card.GetCenter().X, h - 34), CardButton, CardActive ? Ink.Brand : Ink.OnBrand, TextAlign.Center);
            }
            else if (Note.Length > 0) f.Draw(this, new Vector2(safe.Position.X + 8, h - 34), f.Fit(Note, room), Ink.NoteOnBrand);
        }
        DrawStyleBox(Ui.Box(new Color(Pal.Text, 0.35f), 8, new Color(Pal.Card, 0.5f)), _link);
        Assets.DrawFrame(this, Assets.TouchIcons, TouchIcon.Link, Assets.Icon, new Vector2(_link.Position.X + 6, Mathf.Round(_link.GetCenter().Y - 8)));
        f.Draw(this, new Vector2(_link.Position.X + 26, Mathf.Round(_link.GetCenter().Y - 9)), Link, Ink.OnBrand);
    }

    private const float CardFullH = 54f + 44f + 10f, CardSlimH = 56f;

    /// <summary>Niski ekran pionowy (składak): karta w jednym wierszu (tekst po lewej, przycisk po prawej).</summary>
    private bool _slim;

    private float CardH => _slim ? CardSlimH : CardFullH;

    private int RowCount()
    {
        var n = 0;
        foreach (var r in ItemRows) n = Mathf.Max(n, r + 1);
        return n;
    }

    /// <summary>Wysokość wiersza, odstęp, nagłówek grupy, skala tekstu i miejsce: najpierw duże przyciski, potem ciaśniej, bez nagłówków.</summary>
    private (float RowH, float Gap, float Hdr, float Ts, float Need, float Limit) MenuLayout(bool portrait, float w, float h, Rect2 safe)
    {
        var rows = RowCount();
        var groups = 0;
        foreach (var g in RowGroups) groups += g.Length > 0 ? 1 : 0;
        _slim = portrait && h < 640;
        var limit = portrait ? h - Mathf.Max(Layout.SafeBottom, 14) - 48 - 8 - (CardOn && Note.Length == 0 ? CardH + 8 : Note.Length > 0 ? 40 : 0) : h - 42;
        var logoBottom = portrait ? 0 : h >= 440 ? 128 * 2 - 12 : 128 * 1.5f - 9 + 4;
        float RowNeed(float rh, float gp, float hd) => rows * (rh + gp) + groups * hd;
        var ts = 1.5f;
        var rowH = portrait ? Mathf.Max(Layout.TouchTarget + 8, Mathf.Round(PixelFont.LineHeight * ts) + 20) : Mathf.Round(PixelFont.LineHeight * ts) + 4;
        var gap = portrait ? 10f : 4f;
        var hdr = portrait ? 20f : 16f;
        var room = portrait ? limit - (safe.Position.Y + 40 + 128 + 16) : limit - logoBottom;
        if (RowNeed(rowH, gap, hdr) > room)
        {
            ts = 1f;
            rowH = portrait ? Layout.TouchTarget : PixelFont.LineHeight + 4;
            gap = portrait ? 6f : 2f;
        }
        if (RowNeed(rowH, gap, hdr) > room) hdr = 0; // dalej za mało miejsca: bez nagłówków grup (pionowo też mniejsze logo)
        return (rowH, gap, hdr, ts, RowNeed(rowH, gap, hdr), limit);
    }

    private void DrawItem(int i, Rect2 r, float ts, bool boxed)
    {
        var f = PixelFont.I;
        _items[i] = r;
        var sel = i == Sel;
        var ty = Mathf.Round(r.GetCenter().Y - PixelFont.LineHeight * ts / 2);
        if (sel || boxed)
        {
            DrawStyleBox(Ui.Box(new Color(0, 0, 0, 0.25f), 11), new Rect2(r.Position + new Vector2(0, 2), r.Size));
            DrawStyleBox(Ui.Box(sel ? Pal.Card : new Color(Pal.Card, 0.14f), 11, sel ? Pal.Accent : new Color(Pal.Card, 0.35f)), r);
        }
        f.Draw(this, new Vector2(r.GetCenter().X, ty), f.Fit(Items[i], (int)r.Size.X - 20, ts), sel ? Ink.Brand : Ink.OnBrand, TextAlign.Center, ts);
        if (!sel) return;
        var arrow = ((int)(_clock * 3) & 1) == 1 ? 1 : 0;
        f.Draw(this, new Vector2(r.Position.X + 8 + arrow, ty), ">", new Ink(Pal.Accent, Colors.Transparent), TextAlign.Left, ts);
    }
}
