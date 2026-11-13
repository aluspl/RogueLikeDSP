using System;
using Godot;
using LifeLike.Core;
using LifeLike.Game.Audio;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Phone.Pages;

/// <summary>
/// v0.23.2 (UI 2): pauza w trakcie budowy (Esc, przycisk pauzy w rogu) – przyciemniona mapa, na górze etap i HP, pod nimi
/// duże przyciski jeden pod drugim: Wznów, Postać, Budowa, Ustawienia, Wyjdź do menu (z potwierdzeniem: zapis budowy).
/// Strzałki wybierają, A / Enter zatwierdza, B / Esc wznawia (w potwierdzeniu – wraca do listy).
/// </summary>
public sealed class PausePage : PhonePage
{
    public const int Resume = 0, Character = 1, Site = 2, Settings = 3, Quit = 4, QuitYes = 5, QuitNo = 6;

    /// <summary>Wysokość przycisku i odstęp między przyciskami (piksele telefonu).</summary>
    private const int ButtonH = 38, Gap = 8;

    private readonly CoreGame _g;
    private int _sel;

    public PausePage(CoreGame g) => _g = g;

    /// <summary>Wybór przycisku (PauseScreen).</summary>
    public Action<int> Picked { get; set; }

    /// <summary>Potwierdzenie wyjścia do menu na ekranie.</summary>
    public bool Confirming { get; private set; }

    public int Sel => _sel;

    public override bool Overlay => true;
    public override string Title => Loc.T("pauza");
    public override string Hint => Loc.T("pauza_hint");

    private int[] Items => Confirming ? [QuitYes, QuitNo] : [Resume, Character, Site, Settings, Quit];

    private static string Label(int k) => k switch
    {
        Resume => Loc.T("pauza_wznow"),
        Character => Loc.T("pauza_postac"),
        Site => Loc.T("pauza_budowa"),
        Settings => Loc.T("pauza_ustawienia"),
        Quit => Loc.T("pauza_wyjdz"),
        QuitYes => Loc.T("pauza_wyjdz_tak"),
        _ => Loc.T("bp_anuluj"),
    };

    public override void Enter()
    {
        _sel = 0;
        Confirming = false;
    }

    /// <summary>Potwierdzenie wyjścia (Wyjdź do menu) albo powrót do listy.</summary>
    public void Confirm(bool on)
    {
        Confirming = on;
        _sel = on ? 1 : Array.IndexOf(Items, Quit); // domyślnie bezpieczne „Anuluj”
        Redraw();
    }

    public override bool Input(InputCmd e)
    {
        var v = e.VDir;
        if (v != 0)
        {
            _sel = (_sel + v + Items.Length) % Items.Length;
            Sfx.Play("menu");
            return true;
        }
        if (e.IsConfirm)
        {
            Picked?.Invoke(Items[_sel]);
            return true;
        }
        if (e.IsBack && Confirming)
        {
            Confirm(false);
            return true;
        }
        return false;
    }

    public override bool TapRow(int index)
    {
        var i = Array.IndexOf(Items, index);
        if (i < 0) return false;
        _sel = i;
        Picked?.Invoke(index);
        return true;
    }

    public override void Draw(PhonePainter p)
    {
        var f = p.F;
        var cx = Mathf.Round((p.Left + p.Right) / 2);
        var w = Mathf.Min(p.Width, 260);
        var x = Mathf.Round(cx - w / 2);
        // góra: etap i HP
        var y = p.Top + 4;
        f.Draw(p.C, new Vector2(cx, y), Loc.T("pauza").ToUpperInvariant(), Ink.MapDim, TextAlign.Center);
        y += 24;
        var stage = Loc.F("pauza_etap", _g.StageNumber(), _g.StagesInRun(), _g.SDef().Name);
        f.Draw(p.C, new Vector2(cx, y), f.Fit(stage, (int)p.Width), Ink.Map, TextAlign.Center, 1, true);
        y += 24;
        var hp = $"HP {_g.Hero.Hp}/{_g.Hero.MaxHp}";
        var bar = new Rect2(x + 4, y + 5, w - 8, 8);
        p.C.DrawStyleBox(Ui.Box(new Color(1, 1, 1, 0.18f), 4), bar);
        var fill = Mathf.Round(bar.Size.X * Mathf.Clamp(_g.Hero.Hp / (float)Math.Max(1, (int)_g.Hero.MaxHp), 0f, 1f));
        if (fill >= 2) p.C.DrawStyleBox(Ui.Box(Pal.HpMain[Pal.HpColor(_g.Hero.Hp, _g.Hero.MaxHp)], 4), new Rect2(bar.Position, new Vector2(fill, bar.Size.Y)));
        y += 18;
        f.Draw(p.C, new Vector2(cx, y), hp, Ink.Map, TextAlign.Center);
        y += 16;

        var items = Items;
        var extra = Confirming ? 2 * 20 + 8 : 0;
        var total = items.Length * ButtonH + (items.Length - 1) * Gap + extra;
        var top = Mathf.Max(y + 16, Mathf.Round(p.Top + (p.Bottom - p.Top - total) / 2 + 24));
        if (Confirming)
        {
            f.Draw(p.C, new Vector2(cx, top), Loc.T("pauza_wyjdz_pyt"), Ink.Map, TextAlign.Center, 1, true);
            var lines = f.Wrap(Loc.T("pauza_wyjdz_opis"), (int)w);
            if (lines.Count > 0) f.Draw(p.C, new Vector2(cx, top + 20), lines[0], Ink.MapDim, TextAlign.Center);
            if (lines.Count > 1) f.Draw(p.C, new Vector2(cx, top + 36), lines[1], Ink.MapDim, TextAlign.Center);
            top += extra + 8;
        }
        var focus = !Layout.Touch;
        for (var i = 0; i < items.Length; i++)
        {
            var k = items[i];
            var r = new Rect2(x, top + i * (ButtonH + Gap), w, ButtonH);
            var primary = k is Resume or QuitYes;
            var on = focus && i == _sel;
            var fillC = primary ? Pal.Brand : Pal.Card;
            if (on) p.C.DrawStyleBox(Ui.Box(Colors.Transparent, 12, Pal.EliteGold), r.Grow(3));
            p.C.DrawStyleBox(Ui.Box(fillC, 10, primary ? Pal.Brand : Pal.Border), r);
            var ink = primary ? Ink.White : k == Quit ? Ink.Late : Ink.Dark;
            f.Draw(p.C, new Vector2(r.GetCenter().X, Mathf.Round(r.GetCenter().Y - 8)), f.Fit(Label(k), (int)r.Size.X - 12 - Label(k).Length), ink, TextAlign.Center, 1, true);
            p.Hit(r, k);
            p.Reach(r.End.Y);
        }
    }
}
