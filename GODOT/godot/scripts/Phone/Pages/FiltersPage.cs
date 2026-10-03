using System;
using Godot;
using LifeLike.Core;
using LifeLike.Core.Data;
using LifeLike.Game.Audio;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;
using LifeLike.Game.Settings;

namespace LifeLike.Game.Phone.Pages;

/// <summary>
/// v0.21.53: filtry ekranu (Ustawienia > Filtr ekranu) – lista wszystkich filtrów: wybrany z zieloną pastylką, zablokowany
/// jako „???” z podpowiedzią (warunek po odblokowaniu), tryby dla daltonistów z pastylką „Dostępność”; pod listą siła
/// efektu (-/+), filtr na telefonie i ograniczony ruch. Opis zaznaczonego na dole (Kwas – ostrzeżenie o ruchu).
/// Strzałki: wiersz, lewo/prawo – wartość; Spacja: wybierz / przełącz; dotyk: wiersz albo -/+.
/// </summary>
public sealed class FiltersPage : PhonePage
{
    private const int Minus = 100, Plus = 200;

    private readonly GameData _d;
    private readonly Profile _p;
    private readonly ListState _list = new();
    private int _window = 99;

    public FiltersPage(GameData d, Profile p)
    {
        _d = d;
        _p = p;
        _list.Sel = ScreenFilter.Resolve(d, p);
    }

    private int Count => _d.ScreenFilters.Length;
    private int StrengthRow => Count;
    private int PhoneRow => Count + 1;
    private int MotionRow => Count + 2;
    private int Rows => Count + 3;

    public int Sel
    {
        get => _list.Sel;
        set => _list.Sel = Math.Clamp(value, 0, Rows - 1);
    }

    public override string Title => "Filtr ekranu";
    public override string Sub => $"{ScreenFilters.UnlockedCount(_d, _p)}/{Count}";
    public override string Hint => "Strzałki: wybór i wartość  Spacja: wybierz  Esc: wróć";
    public override PageAction[] Actions => [new("Wróć", GameAction.Cancel), new(_list.Sel < Count ? "Wybierz" : "Zmień", GameAction.A)];
    public override bool Closable => true;

    public override bool Input(InputCmd e)
    {
        var v = e.VDir;
        if (v != 0)
        {
            _list.Move(v, Rows, _window);
            Sfx.Play("menu");
            Redraw();
            return true;
        }
        var h = e.HDir;
        if (h != 0)
        {
            Change(_list.Sel, h);
            return true;
        }
        if (!e.Is(GameAction.A)) return false;
        Activate(_list.Sel);
        return true;
    }

    public override bool TapRow(int index)
    {
        if (index >= Plus) Change(index - Plus, 1);
        else if (index >= Minus) Change(index - Minus, -1);
        else
        {
            _list.Sel = index;
            Activate(index);
        }
        return true;
    }

    private void Change(int row, int d)
    {
        if (row < Count)
        {
            ScreenFilter.Select(_d, ScreenFilters.Next(_d, _p, ScreenFilter.Resolve(_d, _p), d));
            _list.Sel = ScreenFilter.Resolve(_d, _p);
            _list.Clamp(Rows, _window);
        }
        else if (row == StrengthRow) GameSettings.FilterStrength = Math.Clamp(GameSettings.FilterStrength + d, 1, GameSettings.FilterSteps);
        else if (row == PhoneRow) GameSettings.FilterPhone = !GameSettings.FilterPhone;
        else GameSettings.ReduceMotion = !GameSettings.ReduceMotion;
        GameSettings.Save();
        Sfx.Play("menu");
        Redraw();
    }

    private void Activate(int row)
    {
        if (row < Count)
        {
            if (!ScreenFilters.Unlocked(_d, _p, row))
            {
                Sfx.Play("hurt", 0.4f);
                Redraw();
                return;
            }
            ScreenFilter.Select(_d, row);
            Sfx.Play("buy");
            Redraw();
            return;
        }
        if (row == StrengthRow)
        {
            Change(row, GameSettings.FilterStrength >= GameSettings.FilterSteps ? 1 - GameSettings.FilterSteps : 1);
            return;
        }
        if (row == PhoneRow) GameSettings.FilterPhone = !GameSettings.FilterPhone;
        else GameSettings.ReduceMotion = !GameSettings.ReduceMotion;
        GameSettings.Save();
        Sfx.Play("menu");
        Redraw();
    }

    public override void Draw(PhonePainter p)
    {
        var y = p.Section(p.Top, "FILTR EKRANU", "dostępność: zawsze");
        var descH = 2 * PhonePainter.RowH + 8;
        _window = Math.Max(4, (int)((p.Bottom - y - 8 - 6 - descH) / PhonePainter.RowH));
        _list.Clamp(Rows, _window);
        var shown = Math.Min(Rows, _window);
        var card = p.Card(y, shown);
        var tx = p.TextX(card);
        var right = card.End.X - 6;
        var active = ScreenFilter.Resolve(_d, _p);
        for (var k = 0; k < shown; k++)
        {
            var i = _list.Top + k;
            var ry = p.RowY(card, k);
            var sel = i == _list.Sel;
            if (sel) p.Selected(card, k);
            else if (k > 0) p.Divider(card, k);
            p.HitRow(card, k, i);
            if (i < Count)
            {
                var f = _d.ScreenFilters[i];
                var open = ScreenFilters.Unlocked(_d, _p, i);
                var on = i == active;
                if (on && !sel) p.Stripe(card, k, Pal.Done);
                var (pill, kind) = on ? ("Wybrany", PillKind.Done)
                    : !open ? ("???", PillKind.Gray)
                    : f.Kind == FilterKind.Access ? ("Dostępność", PillKind.Group)
                    : ("", PillKind.Gray);
                var pw = pill.Length > 0 ? p.Pill(right, ry, pill, kind) : 0;
                var name = open ? f.Name : "???";
                p.Text(tx, ry, name, sel ? Ink.Brand : open ? Ink.Dark : Ink.Dim, TextAlign.Left, right - pw - 6 - tx);
                continue;
            }
            if (i == StrengthRow)
            {
                var plus = p.Pill(right, ry, "+", PillKind.Group);
                var bw = Mathf.Min(70, (right - tx) * 0.3f);
                var barX = right - plus - 6 - bw;
                p.Bar(barX, ry, bw, GameSettings.FilterStrength, GameSettings.FilterSteps, Pal.Brand);
                var minusR = barX - 6;
                var minus = p.Pill(minusR, ry, "-", PillKind.Group);
                p.Hit(new Rect2(right - plus - 10, ry - 4, plus + 16, PhonePainter.RowH + 8), Plus + i);
                p.Hit(new Rect2(minusR - minus - 6, ry - 4, minus + 14, PhonePainter.RowH + 8), Minus + i);
                p.Text(tx, ry, "Siła efektu", sel ? Ink.Brand : Ink.Dark, TextAlign.Left, minusR - minus - 6 - tx);
                continue;
            }
            var phone = i == PhoneRow;
            var val = phone ? GameSettings.FilterPhone : GameSettings.ReduceMotion;
            var vpw = p.Pill(right, ry, val ? "Wł." : "Wył.", val ? PillKind.Done : PillKind.Gray);
            p.Text(tx, ry, phone ? "Filtr na telefonie" : "Ograniczony ruch", sel ? Ink.Brand : Ink.Dark, TextAlign.Left, right - vpw - 6 - tx);
        }
        var dc = p.Card(card.End.Y + 6, 2);
        var (l1, l2, warn) = Describe(_list.Sel);
        p.Text(tx, p.RowY(dc, 0), l1, Ink.Dark, TextAlign.Left, right - tx);
        p.Divider(dc, 1);
        if (warn) p.Stripe(dc, 1, Pal.Late);
        p.Text(tx, p.RowY(dc, 1), l2, warn ? Ink.Late : Ink.Dim, TextAlign.Left, right - tx);
    }

    /// <summary>Dwie linie opisu zaznaczonego wiersza; warn = ostrzeżenie (ruch).</summary>
    private (string, string, bool) Describe(int row)
    {
        if (row == StrengthRow) return ($"Siła filtra: {GameSettings.FilterStrength * 10}%", "Mniej = łagodniejszy efekt", false);
        if (row == PhoneRow) return ("Filtr także na telefonie i banerach", "Wył.: tylko plac, HUD i plansze", false);
        if (row == MotionRow) return ("Bez falowania, drgań i migania", "Kwas: sama tęcza, bez fal", false);
        var f = _d.ScreenFilters[row];
        if (!ScreenFilters.Unlocked(_d, _p, row)) return ("??? " + f.Hint, "Odblokujesz postępem w grze", false);
        if (f.Motion) return (f.Desc, GameSettings.ReduceMotion ? "Ograniczony ruch: bez fal" : "Uwaga: migająca barwa i fale!", !GameSettings.ReduceMotion);
        return (f.Desc, ScreenFilters.UnlockLabel(_d, row), false);
    }
}
