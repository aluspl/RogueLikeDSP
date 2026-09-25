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
public partial class TitleView : Control
{
    public const string Link = "planbudowlany.online";

    public string[] Items { get; set; } = [];
    public string Version { get; set; } = "";
    public int Best { get; set; }
    public int Runs { get; set; }
    public int Xp { get; set; }
    public int Sel { get; set; }
    public string Note { get; set; } = "";
    public string Info { get; set; } = "";

    private float _clock;
    private readonly List<Rect2> _items = new();
    private Rect2 _link;

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

    public bool LinkAt(Vector2 p) => _link.Grow(4).HasPoint(p);

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

    private void DrawContent()
    {
        var f = PixelFont.I;
        var w = Size.X;
        var h = Size.Y;
        var portrait = h > w;
        var safe = Layout.SafeArea;
        Ui.VioletGradient(this, new Rect2(0, 0, w, h));
        var logo = Assets.Tex("ui/title.png");
        var bob = Mathf.Round(Mathf.Sin(_clock * 1.6f) * 2f);
        var ls = portrait ? Mathf.Min(2f, Mathf.Floor((w - 16) / logo.GetWidth() * 2) / 2) : h >= 440 ? 2f : 1.5f;
        var lsz = logo.GetSize() * ls;
        var logoY = portrait ? safe.Position.Y + 40 : -6 * ls;
        DrawTextureRect(logo, new Rect2(Mathf.Round((w - lsz.X) / 2), logoY + bob, lsz.X, lsz.Y), false);
        Ui.WarningStripe(this, new Rect2(0, h - 14, w, 14), _clock * 12f);

        var top = safe.Position.Y + 4;
        f.Draw(this, new Vector2(safe.Position.X + 6, top), Version, Ink.OnBrand);
        var recordX = safe.End.X - Hud.SettingsButton.Side - 10;
        if (Best > 0) f.Draw(this, new Vector2(recordX, top), $"Rekord: {Best}", Ink.OnBrand, TextAlign.Right);

        // menu: przyciski (pionowo szerokie i wysokie jak cele dotyku), poziomo jak na GBA
        _items.Clear();
        var bottom = portrait ? h - Mathf.Max(Layout.SafeBottom, 14) - 70 : h - 40;
        var y = Mathf.Round(logoY + lsz.Y + (portrait ? 16 : 4));
        var n = Items.Length;
        var ts = 1.5f;
        var rowH = portrait ? Mathf.Max(Layout.TouchTarget + 8, Mathf.Round(PixelFont.LineHeight * ts) + 20) : Mathf.Round(PixelFont.LineHeight * ts) + 4;
        var gap = portrait ? 12f : 4f;
        if (y + n * (rowH + gap) > bottom)
        {
            ts = 1f;
            rowH = portrait ? Layout.TouchTarget : PixelFont.LineHeight + 4;
            gap = Mathf.Max(2, Mathf.Floor((bottom - y - n * rowH) / n));
            gap = Mathf.Min(gap, portrait ? 10 : 4);
        }
        for (var i = 0; i < n; i++)
        {
            var sel = i == Sel;
            var tw = portrait ? w - 64 : f.Measure(Items[i], ts) + 40;
            var r = new Rect2(Mathf.Round((w - tw) / 2), y + i * (rowH + gap), tw, rowH);
            _items.Add(r);
            var ty = Mathf.Round(r.GetCenter().Y - PixelFont.LineHeight * ts / 2);
            if (sel || portrait)
            {
                DrawStyleBox(Ui.Box(new Color(0, 0, 0, 0.25f), 11), new Rect2(r.Position + new Vector2(0, 2), r.Size));
                DrawStyleBox(Ui.Box(sel ? Pal.Card : new Color(Pal.Card, 0.14f), 11, sel ? Pal.Accent : new Color(Pal.Card, 0.35f)), r);
            }
            f.Draw(this, new Vector2(w / 2, ty), Items[i], sel ? Ink.Brand : Ink.OnBrand, TextAlign.Center, ts);
            if (!sel) continue;
            var arrow = ((int)(_clock * 3) & 1) == 1 ? 1 : 0;
            f.Draw(this, new Vector2(r.Position.X + 8 + arrow, ty), ">", new Ink(Pal.Accent, Colors.Transparent), TextAlign.Left, ts);
        }

        // notatka / statystyki i link do planbudowlany.online
        var info = Note.Length > 0 ? Note : Info;
        var ink = Note.Length > 0 ? Ink.NoteOnBrand : Ink.OnBrand;
        var lw = f.Measure(Link) + 30;
        if (portrait)
        {
            var ly = h - Mathf.Max(Layout.SafeBottom, 14) - 48;
            _link = new Rect2(Mathf.Round((w - lw) / 2), ly, lw, 36);
            var lines = f.Wrap(info, (int)w - 24);
            for (var k = 0; k < lines.Count && k < 2; k++)
                f.Draw(this, new Vector2(w / 2, ly - 20 - (Mathf.Min(lines.Count, 2) - 1 - k) * 16), lines[k], ink, TextAlign.Center);
        }
        else
        {
            _link = new Rect2(Mathf.Round(safe.End.X - lw - 6), h - 38, lw, 22);
            f.Draw(this, new Vector2(safe.Position.X + 8, h - 34), f.Fit(info, (int)(_link.Position.X - safe.Position.X - 16)), ink);
        }
        DrawStyleBox(Ui.Box(new Color(Pal.Text, 0.35f), 8, new Color(Pal.Card, 0.5f)), _link);
        Assets.DrawFrame(this, Assets.TouchIcons, TouchIcon.Link, Assets.Icon, new Vector2(_link.Position.X + 6, Mathf.Round(_link.GetCenter().Y - 8)));
        f.Draw(this, new Vector2(_link.Position.X + 26, Mathf.Round(_link.GetCenter().Y - 9)), Link, Ink.OnBrand);
    }
}
