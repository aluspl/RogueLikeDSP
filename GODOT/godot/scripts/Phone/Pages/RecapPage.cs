using System;
using System.Collections.Generic;
using Godot;
using LifeLike.Core;
using LifeLike.Game.Audio;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;
using LifeLike.Game.Session;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Phone.Pages;

/// <summary>
/// Podsumowanie budowy (#33) – jedna przewijana strona w telefonie (na GBA 3 strony): co zatrzymało budowę (albo
/// odbiór), ostatnie ciosy, najmocniejsze ciosy, oś czasu etapów (dni, usunięte, SMS, magazyn, premie), nagrody z budowy
/// (doświadczenie, Respekt, zlecenie, rekord dnia / tygodnia), najbliższy cel i rada. Góra/dół albo dotknięcie górnej /
/// dolnej połowy przewija, Enter – dalej.
/// </summary>
public sealed class RecapPage : PhonePage
{
    private readonly List<RecapRow> _rows;
    private int _top;
    private int _window = 10;

    public RecapPage(GameSession s)
    {
        _rows = Build(s);
    }

    public int Top => _top;
    public int Count => _rows.Count;

    public override string Title => "Podsumowanie";
    public override string Sub => _rows.Count > _window ? $"{Math.Min(_rows.Count, _top + _window)}/{_rows.Count}" : "";
    public override string Hint => "Góra/dół: przewiń  Enter: dalej";
    public override PageAction[] Actions => [new("Wyżej", GameAction.Up), new("Niżej", GameAction.Down), new("Dalej", GameAction.Start)];

    private static Ink InkOf(LogKind k) => k switch
    {
        LogKind.Bad => Ink.Late,
        LogKind.Good => Ink.Done,
        LogKind.Loot => Ink.Brand,
        _ => Ink.Dark,
    };

    private static List<RecapRow> Build(GameSession s)
    {
        var g = s.Game;
        var d = s.Data;
        var p = s.Profile;
        var won = g.St == GameStatus.Won;
        var none = Colors.Transparent;
        var rows = new List<RecapRow>();
        void Head(string t) => rows.Add(new RecapRow(t, "", Ink.Dim, none, true));
        void Row(string t, string tail, Ink ink, Color stripe) => rows.Add(new RecapRow(t, tail, ink, stripe));

        var where = g.RecapWhere(new Message()).Text;
        if (won) Row("Odbiór zaliczony!", $"{HouseSchedule.TotalDays(g)} dni", Ink.Done, Pal.Done);
        else Row(g.RecapKiller(new Message()).Text, "", Ink.Late, Pal.Late);
        Row(d.Stages[g.Stage].Name, where, Ink.Dark, won ? Pal.Done : Pal.Late);
        Row($"Usunięte {g.Kills}, elity {g.ElitesKilled}, kombinacje {g.CombosRun}", "", Ink.Dim, none);
        if (!won)
        {
            Head("OSTATNIE CIOSY");
            for (var i = 0; i < CoreGame.RecapHitsN; i++)
            {
                var h = g.LastHits[i];
                if (h.Amount <= 0) break;
                Row(g.RecapHitText(h), "", i == 0 ? Ink.Late : Ink.Dark, i == 0 ? Pal.Late : none);
            }
        }
        Head("NAJMOCNIEJSZE CIOSY");
        if (g.WorstHit.Amount > 0) Row("W Ciebie: " + g.RecapHitText(g.WorstHit), "", Ink.Dark, none);
        if (g.BestHit > 0) Row($"Twój: {g.BestHit}{(g.BestHitCrit ? " (kryt)" : "")} w {d.Enemies[g.BestHitDef].Name}", "", Ink.Brand, none);

        Head("OŚ CZASU");
        foreach (var l in g.RecapTimeline())
        {
            var head = !l.Text.Text.StartsWith(' ');
            rows.Add(new RecapRow(l.Text.Text.TrimStart(), l.Tail.Text, InkOf(l.Ink), head ? (l.Ink == LogKind.Bad ? Pal.Late : Pal.Done) : none));
        }

        Head("NAGRODY Z BUDOWY");
        Row($"Doświadczenie +{s.LastGained}", $"masz {p.Xp}", Ink.Dark, none);
        Row($"Respekt +{g.Respect}", $"masz {p.Respect}", Ink.Brand, none);
        var ci = Meta.NextContract(d, p, g);
        if (ci >= 0)
        {
            var c = d.Contracts[ci];
            Row($"Zlecenie {c.Name}", $"{Math.Min(c.Target, Meta.ContractProgress(d, p, ci))}/{c.Target}", Ink.Dark, Pal.Prog);
        }
        if (g.Daily) Row(s.DailyRecord ? "Rekord dnia!" : $"Budowa dnia nr {g.DailyDay}", "", s.DailyRecord ? Ink.Done : Ink.Dim, none);
        if (g.WeeklyWeek != 0) Row(s.WeeklyRecord ? "Rekord tygodnia!" : $"Wyzwanie: {d.Weekly[g.Bonus.Weekly].Name}", "", s.WeeklyRecord ? Ink.Done : Ink.Dim, none);

        Head("NAJBLIŻSZY CEL");
        if (Recap.Goal(d, p, out var lead, out var name)) Row($"{lead} {name}", "", Ink.Brand, Pal.Brand);
        var tip = d.RecapTips[Recap.TipIndex(d, g)];
        Row(tip.Lines[0], "", Ink.Prog, Pal.Prog);
        if (tip.Lines[1].Length > 0) Row(tip.Lines[1], "", Ink.Prog, Pal.Prog);
        return rows;
    }

    public override bool TapRow(int index)
    {
        Scroll(index == -1 ? -_window / 2 : _window / 2);
        return true;
    }

    public override bool Input(InputCmd e)
    {
        if (e.VDir == 0) return false;
        Scroll(e.VDir * 3);
        return true;
    }

    private void Scroll(int d)
    {
        var top = Math.Clamp(_top + d, 0, Math.Max(0, _rows.Count - _window));
        if (top == _top) return;
        _top = top;
        Sfx.Play("menu");
        Redraw();
    }

    public override void Draw(PhonePainter p)
    {
        _window = Math.Max(4, (int)((p.Bottom - p.Top - 12) / PhonePainter.RowH));
        _top = Math.Clamp(_top, 0, Math.Max(0, _rows.Count - _window));
        var n = Math.Min(_window, _rows.Count - _top);
        var card = p.Card(p.Top, n);
        var tx = p.TextX(card);
        var right = card.End.X - 6;
        for (var r = 0; r < n; r++)
        {
            var row = _rows[_top + r];
            var y = p.RowY(card, r);
            if (row.Header)
            {
                if (r > 0) p.Divider(card, r);
                p.Text(card.Position.X + 6, y, row.Text, Ink.Dim);
                continue;
            }
            if (row.Stripe.A > 0) p.Stripe(card, r, row.Stripe);
            var room = right - tx;
            if (row.Tail.Length > 0) room -= p.Pill(right, y, row.Tail, PillKind.Gray) + 4;
            p.Text(tx, y, row.Text, row.Ink, TextAlign.Left, room);
        }
        var half = card.Size.Y / 2;
        p.Hit(new Rect2(card.Position, new Vector2(card.Size.X, half)), -1);
        p.Hit(new Rect2(card.Position + new Vector2(0, half), new Vector2(card.Size.X, half)), -2);
        if (_top > 0) p.Text(right, card.Position.Y - 2, "^", Ink.Dim, TextAlign.Right);
        if (_top + n < _rows.Count) p.Text(right, card.End.Y - 14, "v", Ink.Dim, TextAlign.Right);
    }
}
