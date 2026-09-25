using System.Linq;
using Godot;
using LifeLike.Core;
using LifeLike.Core.Data;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;

namespace LifeLike.Game.Phone.Pages;

/// <summary>
/// Codzienna budowa (v0.21.48): dzisiejsza data z systemu, „Budowa dnia nr N”, zawód i modyfikatory dnia (te same
/// dla wszystkich), najlepszy wynik dziś i ostatnie dni z profilu, notatka po „Wyślij wynik” (na razie lokalnie).
/// Spacja: start, Tab: wyślij wynik, Esc: wróć.
/// </summary>
public sealed class DailyPage : PhonePage
{
    private readonly GameData _d;
    private readonly Profile _p;
    private readonly (int Y, int M, int D) _date;
    private readonly int _day;

    public DailyPage(GameData d, Profile p, (int Y, int M, int D) date)
    {
        _d = d;
        _p = p;
        _date = date;
        _day = Daily.Number(d, date.Y, date.M, date.D);
    }

    public int Day => _day;

    /// <summary>Komunikat pod listą (np. po wysłaniu wyniku).</summary>
    public string Note { get; set; } = "";

    public override string Title => "Codzienna budowa";
    public override string Sub => $"{_date.D:00}.{_date.M:00}.{_date.Y}";
    public override string Hint => "Spacja: start  Tab: wyślij  Esc: wróć";
    public override PageAction[] Actions => [new("Start", GameAction.A), new("Wyślij wynik", GameAction.Select), new("Wróć", GameAction.Cancel)];
    public override bool Closable => true;

    private static string DateOf(GameData d, int day)
    {
        var (y, m, dd) = Daily.CivilFromDays(Daily.DaysFromCivil(d.DailyEpoch[0], d.DailyEpoch[1], d.DailyEpoch[2]) + day - 1);
        return $"{dd:00}.{m:00}" + (y != d.DailyEpoch[0] ? $".{y}" : "");
    }

    public override void Draw(PhonePainter p)
    {
        var seed = Daily.Seed(_day);
        var cls = _d.Classes[Daily.ClassOf(_d, seed)];
        var mask = Daily.InvestorOf(_d, seed);
        var mods = Enumerable.Range(0, _d.Investor.Length).Where(i => ((mask >> i) & 1) != 0).Select(i => _d.Investor[i]).ToArray();
        var y = p.Section(p.Top, $"BUDOWA DNIA NR {_day}");
        var card = p.Card(y, 2 + mods.Length);
        var tx = p.TextX(card);
        var right = card.End.X - 6;
        p.Stripe(card, 0, Pal.Brand);
        p.Stripe(card, 1, Pal.Brand);
        p.Icon(Assets.Actors, cls.Frame, Assets.Actor, new Vector2(right - Assets.Actor, card.Position.Y + 4 + (2 * PhonePainter.RowH - Assets.Actor) / 2));
        p.Text(tx, p.RowY(card, 0), $"Zawód dnia: {cls.Name}", Ink.Dark, TextAlign.Left, right - Assets.Actor - 4 - tx);
        p.Text(tx, p.RowY(card, 1), $"{_d.Difficulties[_d.DailyDifficulty].Name}, bez Szkoleń", Ink.Dim, TextAlign.Left, right - Assets.Actor - 4 - tx);
        for (var i = 0; i < mods.Length; i++)
        {
            var r = 2 + i;
            p.Divider(card, r);
            p.Stripe(card, r, Pal.Late);
            var pw = p.Pill(right, p.RowY(card, r), $"+{mods[i].XpPct}%", PillKind.Late);
            p.Text(tx, p.RowY(card, r), mods[i].Name, Ink.Dark, TextAlign.Left, right - pw - 4 - tx);
        }

        var best = Daily.Best(_d, _p, _day);
        y = p.Section(card.End.Y + 6, "WYNIKI", _p.DailyRuns > 0 ? $"budowy dnia: {_p.DailyRuns}" : "");
        var days = Enumerable.Range(0, _d.DailyHistory).Where(i => _p.DailyDay[i] > 0).OrderByDescending(i => _p.DailyDay[i]).ToArray();
        var rows = 1 + days.Count(i => _p.DailyDay[i] != _day);
        var notes = Note.Length > 0 ? p.F.Wrap(Note, (int)(right - tx)) : new System.Collections.Generic.List<string>();
        if (notes.Count > 2) notes = notes.GetRange(0, 2);
        var room = (int)((p.Bottom - y - 8 - (notes.Count > 0 ? notes.Count * PhonePainter.RowH + 14 : 0)) / PhonePainter.RowH);
        rows = System.Math.Max(1, System.Math.Min(rows, room));
        var rc = p.Card(y, rows);
        p.Stripe(rc, 0, best >= 0 ? Pal.Done : Pal.Todo);
        var bw = p.Pill(right, p.RowY(rc, 0), best >= 0 ? $"{best} pkt" : "brak", best >= 0 ? PillKind.Done : PillKind.Gray);
        p.Text(tx, p.RowY(rc, 0), Daily.Won(_d, _p, _day) ? "Dziś: odbiór zaliczony!" : "Dziś: najlepszy wynik", Ink.Dark, TextAlign.Left, right - bw - 4 - tx);
        var r0 = 1;
        foreach (var i in days)
        {
            if (_p.DailyDay[i] == _day) continue;
            if (r0 >= rows) break;
            p.Divider(rc, r0);
            var won = ((_p.DailyWon >> i) & 1) != 0;
            var pw = p.Pill(right, p.RowY(rc, r0), $"{_p.DailyScore[i]} pkt", won ? PillKind.Group : PillKind.Gray);
            p.Text(tx, p.RowY(rc, r0), $"Nr {_p.DailyDay[i]} ({DateOf(_d, _p.DailyDay[i])})" + (won ? ", odbiór" : ""), Ink.Dim, TextAlign.Left, right - pw - 4 - tx);
            r0++;
        }
        if (notes.Count == 0) return;
        var nc = p.Card(rc.End.Y + 6, notes.Count);
        for (var k = 0; k < notes.Count; k++)
        {
            p.Stripe(nc, k, Pal.Brand);
            p.Text(tx, p.RowY(nc, k), notes[k], Ink.Brand, TextAlign.Left, right - tx);
        }
    }
}
