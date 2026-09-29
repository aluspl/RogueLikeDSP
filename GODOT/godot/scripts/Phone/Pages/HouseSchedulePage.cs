using Godot;
using LifeLike.Core;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Phone.Pages;

/// <summary>
/// Harmonogram domu po wygranej (v0.21.48) w stylu aplikacji PlanBudowlany: zdjęcie domu (dom z Osiedla), etapy
/// z datą rozpoczęcia (liczone wstecz od dnia odbioru), dniami trwania, kosztem i statusem Gotowe, suma, na dole
/// spokojny link „Zaplanuj swoją budowę – planbudowlany.online”. Spacja / dotknięcie linku otwiera stronę.
/// </summary>
public sealed class HouseSchedulePage : PhonePage
{
    private readonly CoreGame _g;
    private readonly int _endDay;

    public HouseSchedulePage(CoreGame g, (int Y, int M, int D) end)
    {
        _g = g;
        _endDay = Daily.DaysFromCivil(end.Y, end.M, end.D);
    }

    public override string Title => "Harmonogram domu";
    public override string Sub => $"{_g.StagesInRun()}/{_g.StagesInRun()}";
    public override string Hint => "Tab: planbudowlany.online  Spacja/Enter: dalej";
    public override PageAction[] Actions => [new("Dalej", GameAction.Start), new("Zaplanuj swoją budowę", GameAction.Select)];

    /// <summary>Dotknięcie linku (indeks 0) = to samo co SELECT (v0.21.51: A / Enter zawsze = dalej).</summary>
    public override bool TapRow(int index)
    {
        if (index != 0) return false;
        GameInput.Press(GameAction.Select);
        GameInput.Release(GameAction.Select);
        return true;
    }

    private static string Date(int day)
    {
        var (_, m, d) = Daily.CivilFromDays(day);
        return $"{d:00}.{m:00}";
    }

    public override void Draw(PhonePainter p)
    {
        var d = _g.D;
        var n = _g.StagesInRun();   // bez Aktu 0: od Fundamentów
        var big = PhoneView.Full; // pionowy telefon: większe zdjęcie domu
        var photo = big ? 64 : 32;
        // zdjęcie domu i podsumowanie
        var head = p.CardH(p.Top, Mathf.Max(photo + 12, 3 * 16 + 12));
        var size = Mathf.Min(3, _g.Score / 1000);
        var frame = size * d.Classes.Length + _g.Cls;
        var ph = head.Position + new Vector2(6, (head.Size.Y - photo) / 2);
        p.C.DrawStyleBox(Ui.Box(Pal.Group, 6), new Rect2(ph, new Vector2(photo, photo)));
        p.Icon(Assets.Houses, frame, Assets.Actor, ph, big ? 2 : 1);
        var tx = head.Position.X + photo + 14;
        var right = head.End.X - 6;
        var (ey, em, ed) = Daily.CivilFromDays(_endDay);
        p.Bold(tx, head.Position.Y + 4, "Dom rodziny Nowaków", Ink.Dark);
        p.Text(tx, head.Position.Y + 20, $"Odbiór {ed:00}.{em:00}.{ey}", Ink.Dim, TextAlign.Left, right - tx);
        p.Text(tx, head.Position.Y + 36, $"{HouseSchedule.TotalDays(_g)} dni, {HouseSchedule.TotalCost(_g)} tys. zł", Ink.Done, TextAlign.Left, right - tx);

        var y = p.Section(head.End.Y + 6, "ETAPY", "wszystkie gotowe");
        var link = 2 * PhonePainter.RowH + 8 + 6;
        var room = (int)((p.Bottom - y - link - 8) / PhonePainter.RowH);
        var rows = Mathf.Clamp(room, 3, n);
        var card = p.Card(y, rows);
        var cx = p.TextX(card);
        var cr = card.End.X - 6;
        var first = d.Stages.Length - rows; // mało miejsca: ostatnie etapy (odbiór)
        for (var r = 0; r < rows; r++)
        {
            var s = first + r;
            var ry = p.RowY(card, r);
            if (r > 0) p.Divider(card, r);
            p.Stripe(card, r, Pal.Done);
            var pw = p.Pill(cr, ry, $"{d.Stages[s].Cost} tys.", PillKind.Done);
            var dw = p.Text(cr - pw - 6, ry, $"{HouseSchedule.Days(_g, s)} dni", Ink.Dim, TextAlign.Right);
            p.Text(cx, ry, $"{Date(HouseSchedule.StartDay(_g, s, _endDay))} {d.Stages[s].Name}", Ink.Dark, TextAlign.Left, cr - pw - dw - 12 - cx);
        }
        var lc = p.Card(card.End.Y + 6, 2);
        p.HitRow(lc, 0, 0);
        p.HitRow(lc, 1, 0);
        p.Stripe(lc, 0, Pal.Brand);
        p.Stripe(lc, 1, Pal.Brand);
        p.Text(cx, p.RowY(lc, 0), "Zaplanuj swoją budowę", Ink.Brand, TextAlign.Left, lc.End.X - 6 - cx);
        p.Text(cx, p.RowY(lc, 1), d.ScheduleUrl, Ink.Dim, TextAlign.Left, lc.End.X - 6 - cx);
    }
}
