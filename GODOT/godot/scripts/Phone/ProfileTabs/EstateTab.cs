using System;
using Godot;
using LifeLike.Core;
using LifeLike.Core.Data;
using LifeLike.Game.Audio;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;

namespace LifeLike.Game.Phone.ProfileTabs;

/// <summary>
/// Osiedle (zakładka 2 profilu na GBA): domy z wygranych budów na działkach (dach w kolorze kasku zawodu,
/// wielkość wg wyniku), puste działki czekają na kolejne odbiory; ozdoby rosną z wygranymi (#35). Spacja / „Wiadomości”
/// otwiera archiwum fabuły: wątki SMS odblokowane kamieniami milowymi (nowe z pastylką), zablokowane z podpowiedzią.
/// </summary>
public sealed class EstateTab : PhonePage
{
    private readonly GameData _d;
    private readonly Profile _p;
    private readonly Action _save;
    private readonly ListState _list = new();
    private int _window = 8;

    public EstateTab(GameData d, Profile p, Action save = null)
    {
        _d = d;
        _p = p;
        _save = save;
    }

    /// <summary>0 = Osiedle, 1 = lista Wiadomości, 2 = wątek.</summary>
    public int Mode { get; private set; }

    public int Sel => _list.Sel;

    public override string Title => Mode == 0 ? "Osiedle" : Mode == 1 ? "Wiadomości" : _d.StoryArc[_list.Sel].Name;
    public override string Sub => Mode == 0 ? $"Domy: {_p.HousesCount}/{Profile.MaxHouses}" : $"{Story.Count(_d, _p)}/{_d.StoryArc.Length}";
    public override string Hint => Mode == 0 ? "Spacja: Wiadomości  Q/E: zakładki  Esc: wróć" : Mode == 1 ? "Spacja: czytaj  Esc: Osiedle" : "Esc: lista";
    public override PageAction[] Actions => Mode == 0 ? [new("Wiadomości", GameAction.A)] : Mode == 1 ? [new("Czytaj", GameAction.A), new("Osiedle", GameAction.B)] : [new("Lista", GameAction.B)];

    /// <summary>Otwarcie archiwum (test dymny, zrzuty): pierwszy nieprzeczytany wątek zaznaczony.</summary>
    public void OpenMessages()
    {
        Mode = 1;
        _list.Reset();
        for (var i = 0; i < _d.StoryArc.Length; i++)
        {
            if (!Story.Unread(_p, i)) continue;
            _list.Sel = i;
            break;
        }
        _list.Clamp(_d.StoryArc.Length, _window);
        Redraw();
    }

    /// <summary>Otwarcie wątku (odblokowany): oznacza jako przeczytany i zapisuje profil.</summary>
    public bool OpenThread(int i)
    {
        if (!Story.Unlocked(_p, i)) return false;
        _list.Sel = i;
        Mode = 2;
        if (Story.Unread(_p, i))
        {
            Story.MarkRead(_p, i);
            _save?.Invoke();
        }
        Redraw();
        return true;
    }

    public override bool TapRow(int index)
    {
        if (index == 1000)
        {
            OpenMessages();
            return true;
        }
        if (Mode != 1) return false;
        if (_list.Sel == index) OpenThread(index);
        else _list.Sel = index;
        return true;
    }

    public override bool Input(InputCmd e)
    {
        if (Mode == 0)
        {
            if (!e.Is(GameAction.A)) return false;
            Sfx.Play("menu");
            OpenMessages();
            return true;
        }
        if (e.Is(GameAction.B | GameAction.Cancel))
        {
            Mode = Mode == 2 ? 1 : 0;
            Sfx.Play("menu");
            Redraw();
            return true;
        }
        if (Mode == 1 && e.VDir != 0)
        {
            _list.Move(e.VDir, _d.StoryArc.Length, _window);
            Sfx.Play("menu");
            return true;
        }
        if (Mode == 1 && e.Is(GameAction.A | GameAction.Start))
        {
            Sfx.Play(OpenThread(_list.Sel) ? "notify" : "hurt");
            return true;
        }
        if (Mode == 2 && e.Is(GameAction.A | GameAction.Start))
        {
            Mode = 1;
            Redraw();
            return true;
        }
        return Mode != 0 && e.HDir != 0; // lista i wątek: bez przełączania zakładek w bok
    }

    public override void Draw(PhonePainter p)
    {
        if (Mode == 1) DrawList(p);
        else if (Mode == 2) DrawThread(p);
        else DrawEstate(p);
    }

    private void DrawEstate(PhonePainter p)
    {
        var y = p.Section(p.Top, "TWOJE UKOŃCZONE BUDOWY");
        const int cols = 4, rows = 3, cellW = 48, cellH = 40;
        var card = p.CardH(y, rows * cellH + 8);
        var x0 = card.Position.X + (card.Size.X - cols * cellW) / 2;
        for (var i = 0; i < Profile.MaxHouses; i++)
        {
            var cx = x0 + (i % cols) * cellW;
            var cy = card.Position.Y + 4 + (i / cols) * cellH;
            p.C.DrawRect(new Rect2(cx + 4, cy + 34, cellW - 8, 3), i < _p.HousesCount ? Pal.EstateBar : Pal.Border);
            var frame = Assets.HouseEmpty(_d.Classes.Length);
            if (i < _p.HousesCount) frame = Assets.HouseFrame(_p.Houses[i], _d.Classes.Length);
            p.Icon(Assets.Houses, frame, Assets.Actor, new Vector2(cx + (cellW - 32) / 2, cy + 2));
        }
        var mc = p.Card(card.End.Y + 6, 1); // archiwum fabuły (#35)
        var tx = p.TextX(mc);
        var unread = Story.UnreadCount(_d, _p);
        p.Stripe(mc, 0, unread > 0 ? Pal.Brand : Pal.Todo);
        var pw = p.Pill(mc.End.X - 6, p.RowY(mc, 0), unread > 0 ? $"{unread} nowe" : "czytaj", unread > 0 ? PillKind.Brand : PillKind.Gray);
        p.Text(tx, p.RowY(mc, 0), $"Wiadomości {Story.Count(_d, _p)}/{_d.StoryArc.Length}", Ink.Dark, TextAlign.Left, mc.End.X - 10 - pw - tx);
        p.HitRow(mc, 0, 1000);
        var decor = Story.EstateDecor(_d, _p); // ozdoby rosną z wygranymi
        y = p.Section(mc.End.Y + 6, "OSIEDLE ROŚNIE", decor < _d.EstateDecor.Length ? $"kolejna: {_d.EstateDecor[decor].Wins} wygr." : "komplet!");
        var dc = p.CardH(y, 40);
        var step = Math.Min(40f, (dc.Size.X - 12) / Math.Max(1, _d.EstateDecor.Length));
        var x1 = dc.Position.X + (dc.Size.X - step * _d.EstateDecor.Length) / 2;
        for (var k = 0; k < _d.EstateDecor.Length; k++)
        {
            var pos = new Vector2(x1 + k * step + (step - 32) / 2, dc.Position.Y + 4);
            var f = Assets.HouseEmpty(_d.Classes.Length) + 1 + k;
            if (k < decor) p.Icon(Assets.Houses, f, Assets.Actor, pos);
            else p.IconTinted(Assets.Houses, f, Assets.Actor, pos, 1, new Color(0.2f, 0.2f, 0.25f, 0.25f));
        }
        if (dc.End.Y + 6 + PhonePainter.RowH + 8 > p.Bottom) return; // wąski telefon: bez wiersza statystyk
        var c2 = p.Card(dc.End.Y + 6, 1);
        var ti = Titles.Selected(_d, _p); // v0.21.52: wybrany tytuł (Odznaki > Tytuły)
        if (ti >= 0) p.Text(p.TextX(c2), p.RowY(c2, 0), $"„{Titles.Name(_d, ti)}”  Rekord: {_p.Best}  Wygrane: {_p.Wins}", Ink.Brand, TextAlign.Left, c2.Size.X - 18);
        else p.Text(p.TextX(c2), p.RowY(c2, 0), $"Najlepszy wynik: {_p.Best}  Budowy: {_p.Runs}  Wygrane: {_p.Wins}", Ink.Dim, TextAlign.Left, c2.Size.X - 18);
    }

    private void DrawList(PhonePainter p)
    {
        var y = p.Section(p.Top, "FABUŁA Z KOLEJNYCH BUDÓW", $"nowe: {Story.UnreadCount(_d, _p)}");
        _window = Math.Max(4, (int)((p.Bottom - y - 8 - 2 * PhonePainter.RowH - 14) / PhonePainter.RowH));
        _list.Clamp(_d.StoryArc.Length, _window);
        var n = Math.Min(_window, _d.StoryArc.Length - _list.Top);
        var card = p.Card(y, n);
        var tx = p.TextX(card);
        var right = card.End.X - 6;
        for (var r = 0; r < n; r++)
        {
            var i = _list.Top + r;
            if (r > 0) p.Divider(card, r);
            if (i == _list.Sel) p.Selected(card, r);
            p.HitRow(card, r, i);
            var t = _d.StoryArc[i];
            if (Story.Unlocked(_p, i))
            {
                var nw = Story.Unread(_p, i);
                p.Stripe(card, r, nw ? Pal.Brand : Pal.Done);
                var pw = p.Pill(right, p.RowY(card, r), nw ? "Nowa" : t.Messages[0].From.Split(' ')[0], nw ? PillKind.Brand : PillKind.Gray);
                p.Text(tx, p.RowY(card, r), t.Name, i == _list.Sel ? Ink.Brand : Ink.Dark, TextAlign.Left, right - pw - 4 - tx);
            }
            else
            {
                var pw = p.Pill(right, p.RowY(card, r), "Zablok.", PillKind.Gray);
                p.Text(tx, p.RowY(card, r), "???", Ink.Dim, TextAlign.Left, right - pw - 4 - tx);
            }
        }
        var sel = _d.StoryArc[_list.Sel];
        var dc = p.Card(card.End.Y + 6, 2);
        var unl = Story.Unlocked(_p, _list.Sel);
        p.Text(p.TextX(dc), p.RowY(dc, 0), unl ? $"{sel.Messages.Length} SMS: {string.Join(", ", Array.ConvertAll(sel.Messages, m => m.From))}" : "Jak odblokować:", Ink.Dim, TextAlign.Left, dc.Size.X - 18);
        p.Text(p.TextX(dc), p.RowY(dc, 1), unl ? "Spacja / dotknięcie: czytaj" : sel.Hint, unl ? Ink.Dim : Ink.Brand, TextAlign.Left, dc.Size.X - 18);
    }

    private void DrawThread(PhonePainter p)
    {
        var y = p.Top;
        foreach (var m in _d.StoryArc[_list.Sel].Messages)
        {
            var text = string.Join(" ", m.Lines).Trim();
            var lines = p.F.Wrap(text, (int)p.Width - 34);
            var card = p.CardH(y, 26 + lines.Count * 16 + 8);
            p.Icon(Assets.PhoneIcons, 5, Assets.Icon, new Vector2(card.Position.X + 6, card.Position.Y + 5));
            p.Text(card.Position.X + 26, card.Position.Y + 3, m.From, Ink.Dark, TextAlign.Left, card.Size.X - 80);
            p.Pill(card.End.X - 6, card.Position.Y + 3, "SMS", PillKind.Gray);
            var bubble = new Rect2(card.Position.X + 8, card.Position.Y + 24, card.Size.X - 16, lines.Count * 16 + 6);
            p.C.DrawStyleBox(Ui.Box(Pal.Group, 8), bubble);
            for (var i = 0; i < lines.Count; i++) p.F.Draw(p.C, new Vector2(bubble.Position.X + 8, bubble.Position.Y + 2 + i * 16), lines[i], Ink.Dark);
            y = card.End.Y + 6;
        }
    }
}
