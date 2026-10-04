using System.Linq;
using Godot;
using LifeLike.Core;
using LifeLike.Core.Data;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;

namespace LifeLike.Game.Phone.Pages;

/// <summary>
/// Wyzwanie tygodnia (#34): tydzień z daty systemu (poniedziałek–niedziela), zasady tygodnia z danych (te same dla
/// wszystkich), zawód, najlepszy wynik tygodnia i poprzednich tygodni z profilu, notatka po „Wyślij wynik” (tabela
/// tygodnia – zaślepka ILeaderboard). Spacja: start, Tab: wyślij, Esc: wróć.
/// </summary>
public sealed class WeeklyPage : PhonePage
{
    private readonly GameData _d;
    private readonly Profile _p;

    public WeeklyPage(GameData d, Profile p, int week)
    {
        _d = d;
        _p = p;
        Week = week;
    }

    public int Week { get; }

    /// <summary>Komunikat pod listą (np. po wysłaniu wyniku).</summary>
    public string Note { get; set; } = "";

    public override string Title => Loc.T("wyzwanie_tygodnia");
    public override string Sub => Range(Week);
    public override string Hint => Loc.T("spacja_start_tab_wyslij_esc");
    public override PageAction[] Actions => [new(Loc.T("start"), GameAction.A), new(Loc.T("wyslij_wynik"), GameAction.Select), new(Loc.T("wroc"), GameAction.Cancel)];
    public override bool Closable => true;

    private string Range(int week)
    {
        var (_, m0, d0) = Daily.CivilFromDays(Weekly.FirstDay(_d, week));
        var (_, m1, d1) = Daily.CivilFromDays(Weekly.FirstDay(_d, week) + 6);
        return $"{d0:00}.{m0:00} - {d1:00}.{m1:00}";
    }

    public override void Draw(PhonePainter p)
    {
        var wd = _d.Weekly[Weekly.Index(_d, Week)];
        var cls = _d.Classes[Weekly.ClassOf(_d, Week)];
        var y = p.Section(p.Top, Loc.F("tydzien_nr_2", Week));
        var card = p.Card(y, 4);
        var tx = p.TextX(card);
        var right = card.End.X - 6;
        p.Stripe(card, 0, Pal.Late);
        p.Text(tx, p.RowY(card, 0), wd.Name, Ink.Late, TextAlign.Left, right - tx);
        for (var i = 0; i < 2; i++)
        {
            p.Stripe(card, 1 + i, Pal.Late);
            p.Text(tx, p.RowY(card, 1 + i), wd.Desc[i], Ink.Dark, TextAlign.Left, right - Assets.Actor - 4 - tx);
        }
        p.Icon(Assets.Actors, cls.Frame, Assets.Actor, new Vector2(right - Assets.Actor, card.Position.Y + 4 + PhonePainter.RowH + (2 * PhonePainter.RowH - Assets.Actor) / 2));
        p.Divider(card, 3);
        p.Text(tx, p.RowY(card, 3), Loc.F("zawod_bez_szkolen", cls.Name, _d.Difficulties[_d.WeeklyDifficulty].Name), Ink.Dim, TextAlign.Left, right - tx);

        var best = Weekly.Best(_d, _p, Week);
        y = p.Section(card.End.Y + 6, Loc.T("wyniki"), _p.WeeklyRuns > 0 ? Loc.F("wyzwania", _p.WeeklyRuns) : "");
        var weeks = Enumerable.Range(0, _d.WeeklyHistory).Where(i => _p.WeeklyWeek[i] > 0 && _p.WeeklyWeek[i] != Week).OrderByDescending(i => _p.WeeklyWeek[i]).ToArray();
        var notes = Note.Length > 0 ? p.F.Wrap(Note, (int)(right - tx)) : new System.Collections.Generic.List<string>();
        if (notes.Count > 2) notes = notes.GetRange(0, 2);
        var rc = p.Card(y, 1 + weeks.Length);
        p.Stripe(rc, 0, best >= 0 ? Pal.Done : Pal.Todo);
        var bw = p.Pill(right, p.RowY(rc, 0), best >= 0 ? Loc.F("pkt_3", best) : Loc.T("brak_2"), best >= 0 ? PillKind.Done : PillKind.Gray);
        p.Text(tx, p.RowY(rc, 0), Weekly.Won(_d, _p, Week) ? Loc.T("ten_tydzien_odbior_zaliczony") : Loc.T("ten_tydzien_najlepszy_wynik"), Ink.Dark, TextAlign.Left, right - bw - 4 - tx);
        for (var k = 0; k < weeks.Length; k++)
        {
            var i = weeks[k];
            p.Divider(rc, 1 + k);
            var won = ((_p.WeeklyWon >> i) & 1) != 0;
            var pw = p.Pill(right, p.RowY(rc, 1 + k), Loc.F("pkt_3", _p.WeeklyScore[i]), won ? PillKind.Group : PillKind.Gray);
            var name = _d.Weekly[Weekly.Index(_d, _p.WeeklyWeek[i])].Short;
            p.Text(tx, p.RowY(rc, 1 + k), Loc.F("tydz_2", _p.WeeklyWeek[i], name), Ink.Dim, TextAlign.Left, right - pw - 4 - tx);
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
