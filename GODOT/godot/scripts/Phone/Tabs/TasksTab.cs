using LifeLike.Core;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game;

/// <summary>Zadania = harmonogram budowy: etapy z pastylkami Gotowe / W trakcie / Do zrob., wydarzenie na placu, postęp (tab_tasks na GBA).</summary>
public sealed class TasksTab : PhonePage
{
    private readonly CoreGame _g;

    public TasksTab(CoreGame g) => _g = g;

    public override string Title => "Zadania";
    public override string Sub => $"Etap {_g.Stage + 1}/{_g.D.Stages.Length}, {_g.Score} pkt";

    public override void Draw(PhonePainter p)
    {
        var d = _g.D;
        var y = p.Section(p.Top, "HARMONOGRAM", $"Akt {UiText.Roman(d.Stages[_g.Stage].Act)}");
        var n = d.Stages.Length;
        var card = p.Card(y, n);
        for (var i = 0; i < n; i++)
        {
            var done = i < _g.Stage || (i == _g.Stage && _g.St == GameStatus.Won);
            var cur = i == _g.Stage && !done;
            var ry = p.RowY(card, i);
            if (i > 0) p.Divider(card, i);
            p.Stripe(card, i, done ? Pal.Done : cur ? Pal.Prog : Pal.Todo);
            var pill = done ? "Gotowe" : cur ? "W trakcie" : "Do zrob.";
            var pw = p.Pill(card.End.X - 6, ry, pill, done ? PillKind.Done : cur ? PillKind.Prog : PillKind.Gray);
            p.Text(p.TextX(card), ry, $"{i + 1}. {d.Stages[i].Name}", done ? Ink.Dim : Ink.Dark, TextAlign.Left, card.End.X - 12 - pw - p.TextX(card));
        }
        y = card.End.Y + 4;
        y = p.Section(y, "PLAC BUDOWY");
        var c2 = p.Card(y, 2);
        var r0 = p.RowY(c2, 0);
        if (_g.CurrentEvent is { } ev)
        {
            p.Stripe(c2, 0, ev.Good ? Pal.Done : Pal.Late);
            var pw = p.Pill(c2.End.X - 6, r0, ev.Short, ev.Good ? PillKind.Done : PillKind.Late);
            p.Text(p.TextX(c2), r0, ev.Name, Ink.Dark, TextAlign.Left, c2.End.X - 12 - pw - p.TextX(c2));
        }
        else
        {
            p.Text(p.TextX(c2), r0, "Plac: bez niespodzianek", Ink.Dim);
        }
        var r1 = p.RowY(c2, 1);
        p.Divider(c2, 1);
        p.Text(p.TextX(c2), r1, "Postęp", Ink.Dim);
        p.Bar(p.TextX(c2) + 46, r1, c2.End.X - 10 - (p.TextX(c2) + 46), _g.Stage, n, Pal.Brand);
    }
}
