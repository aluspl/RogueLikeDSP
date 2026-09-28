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
/// Przed kolejnym etapem wybór ścieżki (v0.21.48): mapka z bieżącym etapem i dwiema gałęziami do następnego
/// (Game.PathOffer), strzałki / dotknięcie wybierają, Enter potwierdza (Game.ChoosePath).
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

    /// <summary>Wybrana ścieżka (0/1 = pozycja w ofercie).</summary>
    public int Sel { get; set; }

    /// <summary>Czy jest wybór ścieżki (kolejny etap istnieje, dane mają ścieżki).</summary>
    public bool HasChoice => _g.D.Paths.Length >= 2 && _g.Stage + 1 < _g.D.Stages.Length;

    public override bool TapRow(int index)
    {
        if (!HasChoice) return false;
        Sel = index;
        LifeLike.Game.Audio.Sfx.Play("menu");
        return true;
    }

    public override bool Input(InputCmd e)
    {
        if (!HasChoice) return false;
        var d = e.HDir != 0 ? e.HDir : e.VDir;
        if (d == 0) return false;
        Sel = d > 0 ? 1 : 0;
        LifeLike.Game.Audio.Sfx.Play("menu");
        return true;
    }

    public override string Title => "Harmonogram";
    public override string Sub => _g.ActCleared ? $"Akt {_g.ActNumeral()} zaliczony!" : "Etap zaliczony";
    public override string Hint => (HasChoice ? "Strzałki: ścieżka  " : "") + (_g.ActCleared && !_g.ShopClosed ? "Enter: do Hurtowni" : HasChoice ? "Enter: dalej" : $"Enter: dalej{Break}");
    public override PageAction[] Actions => [new(_g.ActCleared && !_g.ShopClosed ? "Do Hurtowni" : HasChoice ? $"Dalej: {_g.D.Paths[_g.PathOffer(Sel)].Short}" : $"Dalej{Break}", GameAction.Start)];

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
        var n = _g.StagesInRun();
        var top = p.Top;
        if (HasChoice)
        {
            top = Paths(p, top) + Gap;
            if (p.Bottom - top - below < 4 * PhonePainter.RowH) // mało miejsca: bez rady kierownika
            {
                below -= tipRows > 0 ? PhonePainter.RowH + tipRows * PhonePainter.RowH + 8 + Gap : 0;
                tipRows = 0;
            }
        }
        var fit = (int)((p.Bottom - top - below - 8) / PhonePainter.RowH);
        var rows = Math.Clamp(fit, 2, n);
        var card = Stages(p, rows, top);
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
    private Godot.Rect2 Stages(PhonePainter p, int rows, float top)
    {
        var d = _g.D;
        var f0 = _g.FirstStage;   // bez Aktu 0: od Fundamentów
        var n = d.Stages.Length;
        var first = Math.Clamp(_g.Stage - (rows > 3 ? 1 : 0), f0, n - rows);
        var card = p.Card(top, rows);
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
            p.Text(tx, y, $"{i - f0 + 1}. {d.Stages[i].Name}", done ? Ink.Dim : next ? Ink.Brand : Ink.Dark, TextAlign.Left, right - pw - 4 - tx);
        }
        return card;
    }

    /// <summary>
    /// Mapka ścieżek: węzeł zaliczonego etapu po lewej, dwie gałęzie do kolejnego etapu (nazwa wariantu, pastylka
    /// i skutek); wybrana w kolorze marki. Zwraca dół karty.
    /// </summary>
    private float Paths(PhonePainter p, float top)
    {
        var d = _g.D;
        var next = d.Stages[_g.Stage + 1];
        var y = p.Section(top, "WYBIERZ ŚCIEŻKĘ", $"{_g.StageNumber() + 1}. {next.Name}");
        var rowH = PhonePainter.RowH;
        var tx0 = p.Left + 38 + 12;
        var descW = (int)(p.Right - 6 - tx0);
        var descs = new System.Collections.Generic.List<string>[2];
        for (var k = 0; k < 2; k++)
        {
            var l = p.F.Wrap(d.Paths[_g.PathOffer(k)].Desc, descW);
            descs[k] = l.Count > 2 ? l.GetRange(0, 2) : l;
        }
        var h0 = 1 + descs[0].Count;
        var card = p.Card(y, h0 + 1 + descs[1].Count);
        var nodeX = card.Position.X + 14;
        var branchX = card.Position.X + 38;
        var midY = card.Position.Y + 4 + h0 * rowH;
        var tx = branchX + 12;
        var right = card.End.X - 6;
        for (var k = 0; k < 2; k++)
        {
            var path = d.Paths[_g.PathOffer(k)];
            var sel = k == Sel;
            var rows = 1 + descs[k].Count;
            var ry = card.Position.Y + 4 + (k == 0 ? 0 : h0 * rowH);
            var by = ry + rows * rowH / 2f;
            if (sel) p.C.DrawRect(new Godot.Rect2(branchX - 8, ry, card.End.X - 2 - (branchX - 8), rows * rowH), Pal.Group);
            p.Hit(new Godot.Rect2(card.Position.X, ry, card.Size.X, rows * rowH), k);
            var c = sel ? Pal.Brand : Pal.Border;
            p.C.DrawLine(new Godot.Vector2(nodeX, midY), new Godot.Vector2(branchX, by), c, sel ? 3 : 2);
            p.C.DrawCircle(new Godot.Vector2(branchX, by), sel ? 6 : 5, c);
            p.C.DrawCircle(new Godot.Vector2(branchX, by), sel ? 3 : 2, Pal.Card);
            var pw = p.Pill(right, ry, path.Short, sel ? PillKind.Brand : PillKind.Gray);
            p.Text(tx, ry, path.Name, sel ? Ink.Brand : Ink.Dark, TextAlign.Left, right - pw - 4 - tx);
            for (var i = 0; i < descs[k].Count; i++) p.Text(tx, ry + (1 + i) * rowH, descs[k][i], Ink.Dim, TextAlign.Left, right - tx);
        }
        p.C.DrawCircle(new Godot.Vector2(nodeX, midY), 7, Pal.Done);
        p.C.DrawCircle(new Godot.Vector2(nodeX, midY), 3, Pal.Card);
        return card.End.Y;
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
