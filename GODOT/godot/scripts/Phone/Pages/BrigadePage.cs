using System;
using LifeLike.Core;
using LifeLike.Game.Audio;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Phone.Pages;

/// <summary>
/// Brygada (tab_brigade na GBA, zakładka Zespół): najemni fachowcy raz na etap - lista z ceną w pastylce
/// (fiolet - stać Cię, szara - za drogo albo zablokowany, zielona - już na placu), opis zaznaczonego i stan wezwania.
/// Spacja / dotknięcie zaznaczonego wzywa (zużywa turę), Esc wraca do gry.
/// </summary>
public sealed class BrigadePage : PhonePage
{
    private readonly CoreGame _g;
    private readonly Action<int> _call;
    private readonly ListState _list = new();

    public BrigadePage(CoreGame g, Action<int> call)
    {
        _g = g;
        _call = call;
    }

    public override string Title => "Brygada";
    public override string Sub => $"Budżet: {_g.Cash} zł";
    public override string Hint => "Spacja: wezwij (tura)  Esc: wróć";
    public override PageAction[] Actions => [new("Wezwij", GameAction.A), new("Wróć", GameAction.Cancel)];
    public override bool Closable => true;

    public int Sel
    {
        get => _list.Sel;
        set => _list.Sel = value;
    }

    public override bool TapRow(int index)
    {
        if (index == _list.Sel) _call(index);
        _list.Sel = index;
        return true;
    }

    public override bool Input(InputCmd e)
    {
        var v = e.VDir;
        if (v != 0)
        {
            _list.Move(v, _g.D.Brigade.Length, _g.D.Brigade.Length);
            Sfx.Play("menu");
            return true;
        }
        if (!e.Is(GameAction.A)) return false;
        _call(_list.Sel);
        return true;
    }

    /// <summary>Stan wezwania zaznaczonego fachowca (wiersz pod opisem, jak na GBA).</summary>
    private (string Text, Ink Ink) Status()
    {
        var d = _g.D;
        if (_g.HelperCalled >= 0)
        {
            var t = _g.GuardTurns > 0 ? $" ({_g.GuardTurns} t.)" : _g.AllyTurns > 0 ? $" ({_g.AllyTurns} t.)" : "";
            return ($"Na tym etapie: {d.Brigade[_g.HelperCalled].Name}{t}", Ink.Done);
        }
        return _g.HelperBlocked(_list.Sel) switch
        {
            HelperBlock.Ok => ("Gotowy do wezwania (zużywa turę)", Ink.Brand),
            HelperBlock.Locked => ("Odblokuj w Szkoleniach (Koszty w profilu)", Ink.Dim),
            HelperBlock.Cash => ("Za mały budżet", Ink.Late),
            HelperBlock.NoTarget => ($"Nikogo w zasięgu {d.Brigade[_list.Sel].Reach}", Ink.Late),
            HelperBlock.NoRoom => ("Brak miejsca obok", Ink.Late),
            _ => ("", Ink.Dim),
        };
    }

    public override void Draw(PhonePainter p)
    {
        var d = _g.D;
        var n = d.Brigade.Length;
        _list.Clamp(n, n);
        var y = p.Section(p.Top, "FACHOWCY", "raz na etap");
        var card = p.Card(y, n);
        var tx = p.TextX(card);
        var right = card.End.X - 6;
        for (var i = 0; i < n; i++)
        {
            var h = d.Brigade[i];
            var ry = p.RowY(card, i);
            var sel = i == _list.Sel;
            var unl = ((_g.Bonus.Helpers >> i) & 1) != 0;
            var here = _g.HelperCalled == i;
            if (sel) p.Selected(card, i);
            else if (i > 0) p.Divider(card, i);
            p.HitRow(card, i, i);
            var pill = here ? "Na placu" : unl ? $"{h.Price} zł" : "Zablok.";
            var kind = here ? PillKind.Done : unl && _g.Cash >= h.Price ? PillKind.Group : PillKind.Gray;
            var pw = p.Pill(right, ry, pill, kind);
            p.Text(tx, ry, h.Name, sel ? Ink.Brand : unl ? Ink.Dark : Ink.Dim, TextAlign.Left, right - pw - 4 - tx);
        }
        var dc = p.Card(card.End.Y + 6, 2);
        p.Text(tx, p.RowY(dc, 0), d.Brigade[_list.Sel].Desc, Ink.Dim, TextAlign.Left, right - tx);
        p.Divider(dc, 1);
        var (text, ink) = Status();
        p.Text(tx, p.RowY(dc, 1), text, ink, TextAlign.Left, right - tx);
    }
}
