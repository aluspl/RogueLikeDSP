using Godot;
using LifeLike.Core;
using LifeLike.Core.Data;
using LifeLike.Game.Audio;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Phone.Pages;

/// <summary>
/// Wydarzenie z wyborem (v0.21.50 cz. 3, event_dialog na GBA): SMS od nadawcy, potem 2–3 odpowiedzi ze skutkami
/// (ChoiceText.Label – te same słowa co na GBA), na końcu wynik: co zaszło, a co „nie tym razem” (szansa).
/// Strzałki / dotknięcie wybierają odpowiedź, Enter / Spacja / drugie dotknięcie zatwierdza.
/// </summary>
public sealed class EventPage : PhonePage
{
    private readonly CoreGame _g;
    private readonly ChoiceEventDef _ev;

    public EventPage(CoreGame g, int ev)
    {
        _g = g;
        _ev = g.D.ChoiceEvents[ev];
    }

    /// <summary>0 = SMS, 1 = odpowiedzi, 2 = wynik.</summary>
    public int Phase { get; set; }

    public int Sel { get; set; }

    /// <summary>Dalej (A / Enter / dotknięcie wybranej odpowiedzi) – ekran przechodzi do kolejnej fazy.</summary>
    public System.Action Next { get; set; }

    public override string Title => Phase switch { 0 => Loc.T("wiadomosci_2"), 1 => Loc.T("odpowiedz_3"), _ => Loc.T("wynik_4") };
    public override string Sub => _ev.Name;
    public override string Hint => Phase switch { 0 => Loc.T("enter_odpowiedz"), 1 => Loc.T("strzalki_wybor_enter_wybieram"), _ => Loc.T("enter_dalej") };

    public override PageAction[] Actions => Phase switch
    {
        0 => [new(Loc.T("odpowiedz_4"), GameAction.A)],
        1 => [new(Loc.T("wybieram"), GameAction.A)],
        _ => [new(Loc.T("dalej"), GameAction.A)],
    };

    public override bool TapRow(int index)
    {
        if (Phase != 1)
        {
            Next?.Invoke();
            return true;
        }
        if (index < 0 || index >= _ev.Choices.Length) return false;
        if (index == Sel) Next?.Invoke();
        else
        {
            Sel = index;
            Sfx.Play("menu");
        }
        return true;
    }

    public override bool Input(InputCmd e)
    {
        if (Phase != 1) return false;
        var d = e.VDir != 0 ? e.VDir : e.HDir;
        if (d == 0) return false;
        Sel = (Sel + d + _ev.Choices.Length) % _ev.Choices.Length;
        Sfx.Play("menu");
        return true;
    }

    public override void Draw(PhonePainter p)
    {
        if (Phase == 0) DrawSms(p);
        else if (Phase == 1) DrawChoices(p);
        else DrawResult(p);
    }

    private void DrawSms(PhonePainter p)
    {
        var m = _ev.Msg;
        var text = string.Join(" ", m.Lines).Trim();
        var lines = p.F.Wrap(text, (int)p.Width - 34);
        var card = p.CardH(p.Top, 26 + lines.Count * 16 + 8);
        p.Hit(card, 0);
        p.Icon(Assets.PhoneIcons, 5, Assets.Icon, new Vector2(card.Position.X + 6, card.Position.Y + 5));
        p.Text(card.Position.X + 26, card.Position.Y + 3, m.From, Ink.Dark, TextAlign.Left, card.Size.X - 80);
        p.Pill(card.End.X - 6, card.Position.Y + 3, Loc.T("teraz_2"), PillKind.Gray);
        var bubble = new Rect2(card.Position.X + 8, card.Position.Y + 24, card.Size.X - 16, lines.Count * 16 + 6);
        p.C.DrawStyleBox(Ui.Box(Pal.Group, 8), bubble);
        for (var i = 0; i < lines.Count; i++) p.F.Draw(p.C, new Vector2(bubble.Position.X + 8, bubble.Position.Y + 2 + i * 16), lines[i], Ink.Dark);
        var ic = p.Card(card.End.Y + 6, 2);
        p.Stripe(ic, 0, Pal.Brand);
        p.Text(p.TextX(ic), p.RowY(ic, 0), Loc.T("wybierz_odpowiedz"), Ink.Brand, TextAlign.Left, ic.End.X - 6 - p.TextX(ic));
        p.Divider(ic, 1);
        p.Text(p.TextX(ic), p.RowY(ic, 1), Loc.F("odpowiedzi_skutek_od_razu", _ev.Choices.Length), Ink.Dim, TextAlign.Left, ic.End.X - 6 - p.TextX(ic));
    }

    private void DrawChoices(PhonePainter p)
    {
        var y = p.Section(p.Top, Loc.T("odpowiedzi"), _ev.Name);
        for (var k = 0; k < _ev.Choices.Length; k++)
        {
            var c = _ev.Choices[k];
            var sel = k == Sel;
            var tx0 = p.Left + 12;
            var width = (int)(p.Right - 6 - tx0);
            var effects = p.F.Wrap(ChoiceText.Label(_g.D, c), width);
            var rows = 1 + System.Math.Min(2, effects.Count);
            var card = p.Card(y, rows);
            p.C.DrawRect(new Rect2(card.Position, new Vector2(4, card.Size.Y)), sel ? Pal.Brand : Pal.Todo);
            if (sel)
            {
                p.C.DrawRect(new Rect2(card.Position.X + 4, card.Position.Y, card.Size.X - 4, card.Size.Y), Pal.Group);
                p.C.DrawRect(card, Pal.Brand, false, 2);
            }
            p.Hit(card, k);
            var tx = p.TextX(card);
            var right = card.End.X - 6;
            p.Bold(tx, p.RowY(card, 0) + PhonePainter.TextDy, p.F.Fit(c.Label, (int)(right - tx)), sel ? Ink.Brand : Ink.Dark);
            for (var i = 0; i < rows - 1; i++) p.Text(tx, p.RowY(card, 1 + i), effects[i], sel ? Ink.Dark : Ink.Dim, TextAlign.Left, right - tx);
            y = card.End.Y + 5;
        }
    }

    private void DrawResult(PhonePainter p)
    {
        if (_g.StageChoicePick < 0) return;
        var c = _ev.Choices[_g.StageChoicePick];
        var card = p.Card(p.Top, 2 + System.Math.Max(1, c.Outs.Length));
        p.Hit(card, 0);
        var tx = p.TextX(card);
        var right = card.End.X - 6;
        p.Text(tx, p.RowY(card, 0), Loc.T("odpowiedz") + c.Label, Ink.Dark, TextAlign.Left, right - tx);
        p.Divider(card, 1);
        p.Stripe(card, 1, Pal.Done);
        p.Text(tx, p.RowY(card, 1), c.Result, Ink.Done, TextAlign.Left, right - tx);
        if (c.Outs.Length == 0)
        {
            p.Divider(card, 2);
            p.Text(tx, p.RowY(card, 2), Loc.T("bez_skutkow_2"), Ink.Dim);
        }
        for (var i = 0; i < c.Outs.Length; i++)
        {
            var o = c.Outs[i] with { Chance = 100 }; // szansa już rozstrzygnięta
            var done = ((_g.ChoiceDone >> i) & 1) != 0;
            var bad = o.Effect is ChoiceEffect.Spawn or ChoiceEffect.Status || o.Value < 0;
            var label = (done ? "" : Loc.T("nie_tym_razem")) + ChoiceText.OutLabel(_g.D, o);
            p.Divider(card, 2 + i);
            p.Stripe(card, 2 + i, !done ? Pal.Todo : bad ? Pal.Late : Pal.Done);
            p.Text(tx, p.RowY(card, 2 + i), label, !done ? Ink.Dim : bad ? Ink.Late : Ink.Dark, TextAlign.Left, right - tx);
        }
    }
}
