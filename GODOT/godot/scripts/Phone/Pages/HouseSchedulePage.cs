using Godot;
using LifeLike.Core;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Phone.Pages;

/// <summary>
/// Harmonogram domu po wygranej (v0.21.48) w stylu aplikacji PlanBudowlany: zdjęcie domu (dom z Osiedla), etapy
/// z datą rozpoczęcia (liczone wstecz od dnia odbioru), dniami trwania, kosztem i statusem Gotowe, suma, na dole
/// spokojny link planbudowlany.online (v0.22.0: z kodem QR, przycisk „Zaplanuj swoją budowę” na dole). Spacja / dotknięcie linku otwiera stronę.
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

    public override string Title => Loc.T("harmonogram_domu");
    public override string Sub => $"{_g.StagesInRun()}/{_g.StagesInRun()}";
    public override string Hint => Loc.T("tab_planbudowlany_online");
    public override PageAction[] Actions => [new(Loc.T("dalej"), GameAction.Start), new(Loc.T("zaplanuj_swoja_budowe"), GameAction.Select)];

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
        p.Bold(tx, head.Position.Y + 4, Loc.T("dom_rodziny_nowakow"), Ink.Dark);
        p.Text(tx, head.Position.Y + 20, Loc.F("odbior_4", ed, em, ey), Ink.Dim, TextAlign.Left, right - tx);
        p.Text(tx, head.Position.Y + 36, Loc.F("dni_tys_zl", HouseSchedule.TotalDays(_g), HouseSchedule.TotalCost(_g)), Ink.Done, TextAlign.Left, right - tx);

        var y = p.Section(head.End.Y + 6, Loc.T("etapy_3"), Loc.T("wszystkie_gotowe"));
        // v0.22.0 (UX A): pionowy telefon – kod QR planbudowlany.online (2x, ostre piksele); mały telefon – sam adres
        var qr = Assets.Tex("ui/qr.png");
        var free = p.Bottom - y - 3 * PhonePainter.RowH - 8 - 6 - 12 - 8; // miejsce pod co najmniej 3 etapami
        var qrScale = !big ? 0 : free >= qr.GetHeight() * 2 ? 2 : free >= qr.GetHeight() ? 1 : 0;
        var link = (qrScale > 0 ? qr.GetHeight() * qrScale + 12 : PhonePainter.RowH + 8) + 6;
        var room = (int)((p.Bottom - y - link - 8) / PhonePainter.RowH);
        var rows = Mathf.Clamp(room, 3, n);
        var card = p.Card(y, rows);
        var cx = p.TextX(card);
        var cr = card.End.X - 6;
        var first = _g.RouteCount() - rows; // mało miejsca: ostatnie etapy (odbiór)
        for (var r = 0; r < rows; r++)
        {
            var s = first + r;
            var ry = p.RowY(card, r);
            if (r > 0) p.Divider(card, r);
            p.Stripe(card, r, Pal.Done);
            var pw = p.Pill(cr, ry, Loc.F("tys_2", _g.SDef(s).Cost), PillKind.Done);
            var dw = p.Text(cr - pw - 6, ry, Loc.F("dni_4", HouseSchedule.Days(_g, s)), Ink.Dim, TextAlign.Right);
            p.Text(cx, ry, $"{Date(HouseSchedule.StartDay(_g, s, _endDay))} {_g.SDef(s).Name}", Ink.Dark, TextAlign.Left, cr - pw - dw - 12 - cx);
        }
        var lc = p.CardH(card.End.Y + 6, link - 6); // przycisk „Zaplanuj swoją budowę” jest na dole strony – tu bez dubla
        p.Hit(lc, 0);
        p.C.DrawRect(new Rect2(lc.Position.X + 4, lc.Position.Y + 6, 3, lc.Size.Y - 12), Pal.Brand);
        var ux = cx;
        if (qrScale > 0)
        {
            var qs = qr.GetSize() * qrScale;
            p.C.DrawTextureRect(qr, new Rect2(cx, lc.Position.Y + 6, qs.X, qs.Y), false);
            ux = cx + qs.X + 10;
        }
        p.Text(ux, Mathf.Round(lc.GetCenter().Y - 8), d.ScheduleUrl, Ink.Brand, TextAlign.Left, lc.End.X - 6 - ux);
    }
}
