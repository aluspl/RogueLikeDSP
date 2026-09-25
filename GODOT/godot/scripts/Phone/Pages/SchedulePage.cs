using System;
using System.Collections.Generic;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Phone.Pages;

/// <summary>
/// Harmonogram po etapie (run_schedule na GBA): etapy z pastylkami Gotowe / Następny / Do zrob., wynik, dni,
/// doświadczenie budowy, premia za akt, zdobyte odznaki / zlecenia i rada kierownika (game.json „tips”).
/// Gdy wszystko się nie mieści, lista etapów pokazuje okno wokół bieżącego (jak okno 4 etapów na GBA).
/// </summary>
public sealed class SchedulePage : PhonePage
{
    private const int Gap = 6;
    private readonly CoreGame _g;
    private readonly string _note;
    private readonly string _tip;

    public SchedulePage(CoreGame g, string note, string tip = "")
    {
        _g = g;
        _note = note ?? "";
        _tip = tip ?? "";
    }

    public override string Title => "Harmonogram";
    public override string Sub => _g.ActCleared ? $"Akt {UiText.Roman(_g.D.Stages[_g.Stage].Act)} zaliczony!" : "Etap zaliczony";
    public override string Hint => _g.ActCleared && !_g.ShopClosed ? "Enter: do Hurtowni" : $"Enter: dalej{Break}";
    public override PageAction[] Actions => [new(_g.ActCleared && !_g.ShopClosed ? "Do Hurtowni" : $"Dalej{Break}", GameAction.Start)];

    /// <summary>Przerwa na kawę między etapami (tryb inwestora: bez przerwy).</summary>
    private string Break => _g.InvestorHas(LifeLike.Core.Data.InvestorEffect.NoBreak) ? " (bez przerwy)" : " (kawa +5 HP)";

    public override void Draw(PhonePainter p)
    {
        var tx0 = p.Left + 12;
        var width = (int)(p.Right - 6 - tx0);
        var notes = _note.Length > 0 ? p.F.Wrap(_note, width) : new List<string>();
        var tips = _tip.Length > 0 ? p.F.Wrap(_tip, width) : new List<string>();
        var summaryRows = 1 + (_g.ActCleared ? 1 : 0) + Math.Min(notes.Count, 2);
        var tipRows = Math.Min(tips.Count, 2);
        var below = summaryRows * PhonePainter.RowH + 8 + Gap + (tipRows > 0 ? PhonePainter.RowH + tipRows * PhonePainter.RowH + 8 + Gap : 0);
        var n = _g.D.Stages.Length;
        var fit = (int)((p.Bottom - p.Top - below - 8) / PhonePainter.RowH);
        var rows = Math.Clamp(fit, 3, n);
        var card = Stages(p, rows);
        var c2 = Summary(p, card.End.Y + Gap, summaryRows, notes);
        if (tipRows == 0) return;
        var y = p.Section(c2.End.Y + Gap, "RADA KIEROWNIKA");
        var c3 = p.Card(y, tipRows);
        for (var k = 0; k < tipRows; k++)
        {
            p.Stripe(c3, k, Pal.Prog);
            p.Text(p.TextX(c3), p.RowY(c3, k), tips[k], Ink.Prog, TextAlign.Left, c3.End.X - 6 - p.TextX(c3));
        }
    }

    /// <summary>Lista etapów (okno rows wierszy: zaliczony i kolejne).</summary>
    private Godot.Rect2 Stages(PhonePainter p, int rows)
    {
        var d = _g.D;
        var n = d.Stages.Length;
        var first = Math.Clamp(_g.Stage - 1, 0, n - rows);
        var card = p.Card(p.Top, rows);
        var tx = p.TextX(card);
        var right = card.End.X - 6;
        for (var r = 0; r < rows; r++)
        {
            var i = first + r;
            var y = p.RowY(card, r);
            if (r > 0) p.Divider(card, r);
            var done = i <= _g.Stage;
            var next = i == _g.Stage + 1;
            if (next) p.Selected(card, r);
            p.Stripe(card, r, done ? Pal.Done : next ? Pal.Brand : Pal.Todo);
            var pw = p.Pill(right, y, done ? "Gotowe" : next ? "Następny" : "Do zrob.", done ? PillKind.Done : next ? PillKind.Brand : PillKind.Gray);
            p.Text(tx, y, $"{i + 1}. {d.Stages[i].Name}", done ? Ink.Dim : next ? Ink.Brand : Ink.Dark, TextAlign.Left, right - pw - 4 - tx);
        }
        return card;
    }

    /// <summary>Wynik, premia za akt i notatka o odznakach / zleceniach.</summary>
    private Godot.Rect2 Summary(PhonePainter p, float y, int rows, List<string> notes)
    {
        var c2 = p.Card(y, rows);
        var tx = p.TextX(c2);
        var right = c2.End.X - 6;
        var r = 0;
        p.Text(tx, p.RowY(c2, r), $"Wynik {_g.Score}  Dni {_g.Turns}  Dośw. {_g.Xp}", Ink.Dark, TextAlign.Left, right - tx);
        if (_g.ActCleared)
        {
            r++;
            p.Divider(c2, r);
            p.Stripe(c2, r, Pal.Done);
            p.Text(tx, p.RowY(c2, r), $"Premia za akt: +{_g.ActBonus} zł", Ink.Done);
        }
        for (var k = 0; k < notes.Count && k < 2; k++)
        {
            r++;
            p.Text(tx, p.RowY(c2, r), notes[k], Ink.Brand);
        }
        return c2;
    }
}
