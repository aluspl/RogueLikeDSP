using LifeLike.Game.Gfx;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Phone.Pages;

/// <summary>
/// Harmonogram po etapie (run_schedule na GBA): etapy z pastylkami Gotowe / Następny / Do zrob., wynik, dni,
/// doświadczenie budowy, premia za akt i zdobyte odznaki / zlecenia.
/// </summary>
public sealed class SchedulePage : PhonePage
{
    private readonly CoreGame _g;
    private readonly string _note;

    public SchedulePage(CoreGame g, string note)
    {
        _g = g;
        _note = note ?? "";
    }

    public override string Title => "Harmonogram";
    public override string Sub => _g.ActCleared ? $"Akt {UiText.Roman(_g.D.Stages[_g.Stage].Act)} zaliczony!" : "Etap zaliczony";
    public override string Hint => _g.ActCleared ? "Enter: do Hurtowni" : "Enter: dalej (kawa +5 HP)";

    public override void Draw(PhonePainter p)
    {
        var d = _g.D;
        var n = d.Stages.Length;
        var card = p.Card(p.Top, n);
        var tx = p.TextX(card);
        var right = card.End.X - 6;
        for (var i = 0; i < n; i++)
        {
            var y = p.RowY(card, i);
            if (i > 0) p.Divider(card, i);
            var done = i <= _g.Stage;
            var next = i == _g.Stage + 1;
            if (next) p.Selected(card, i);
            p.Stripe(card, i, done ? Pal.Done : next ? Pal.Brand : Pal.Todo);
            var pw = p.Pill(right, y, done ? "Gotowe" : next ? "Następny" : "Do zrob.", done ? PillKind.Done : next ? PillKind.Brand : PillKind.Gray);
            p.Text(tx, y, $"{i + 1}. {d.Stages[i].Name}", done ? Ink.Dim : next ? Ink.Brand : Ink.Dark, TextAlign.Left, right - pw - 4 - tx);
        }
        var lines = _note.Length > 0 ? p.F.Wrap(_note, (int)(right - tx)) : new System.Collections.Generic.List<string>();
        var rows = 1 + (_g.ActCleared ? 1 : 0) + System.Math.Min(lines.Count, 2);
        var c2 = p.Card(card.End.Y + 6, rows);
        var r = 0;
        p.Text(tx, p.RowY(c2, r), $"Wynik {_g.Score}  Dni {_g.Turns}  Dośw. {_g.Xp}", Ink.Dark, TextAlign.Left, right - tx);
        if (_g.ActCleared)
        {
            r++;
            p.Divider(c2, r);
            p.Stripe(c2, r, Pal.Done);
            p.Text(tx, p.RowY(c2, r), $"Premia za akt: +{_g.ActBonus} zł", Ink.Done);
        }
        for (var k = 0; k < lines.Count && k < 2; k++)
        {
            r++;
            p.Text(tx, p.RowY(c2, r), lines[k], Ink.Brand);
        }
    }
}
