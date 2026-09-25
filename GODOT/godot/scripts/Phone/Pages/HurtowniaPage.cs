using System;
using LifeLike.Game.Audio;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Phone.Pages;

/// <summary>
/// Hurtownia między aktami (run_hurtownia na GBA): budżet budowy w nagłówku, premia za akt, lista towarów
/// z ceną w pastylce (fiolet - stać Cię, szara - za drogo), opis zaznaczonego; Spacja kupuje, Enter - dalej.
/// </summary>
public sealed class HurtowniaPage : PhonePage
{
    private const int Window = 6;
    private readonly CoreGame _g;
    private readonly ListState _list = new();
    private string _note = "";

    public HurtowniaPage(CoreGame g) => _g = g;

    public override string Title => "Hurtownia";
    public override string Sub => $"Budżet: {_g.Cash} zł";
    public override string Hint => "Spacja: kup  Enter: dalej";

    public int Sel
    {
        get => _list.Sel;
        set => _list.Sel = value;
    }

    public void Buy()
    {
        if (_g.HurtowniaBuy(_list.Sel))
        {
            Sfx.Play("buy");
            _note = "Kupione! Enter: dalej";
        }
        else
        {
            _note = "Za mały budżet";
        }
    }

    public override bool Input(InputCmd e)
    {
        var v = e.VDir;
        if (v != 0)
        {
            _list.Move(v, _g.D.Hurtownia.Length, Window);
            _note = "";
            Sfx.Play("menu");
            return true;
        }
        if (e.Is(GameAction.A))
        {
            Buy();
            return true;
        }
        return false;
    }

    public override void Draw(PhonePainter p)
    {
        var d = _g.D;
        var c0 = p.Card(p.Top, 1);
        var tx = p.TextX(c0);
        var right = c0.End.X - 6;
        var bonus = $"Premia za akt {UiText.Roman(d.Stages[_g.Stage].Act)}: +{_g.ActBonus} zł";
        p.Stripe(c0, 0, _note.Length > 0 ? Pal.Brand : Pal.Done);
        p.Text(tx, p.RowY(c0, 0), _note.Length > 0 ? _note : bonus, _note.Length > 0 ? Ink.Brand : Ink.Done, TextAlign.Left, right - tx);
        var n = d.Hurtownia.Length;
        _list.Clamp(n, Window);
        var rows = Math.Min(Window, n - _list.Top);
        var card = p.Card(c0.End.Y + 6, rows);
        for (var r = 0; r < rows; r++)
        {
            var i = _list.Top + r;
            var it = d.Hurtownia[i];
            var y = p.RowY(card, r);
            var sel = i == _list.Sel;
            if (sel) p.Selected(card, r);
            else if (r > 0) p.Divider(card, r);
            var pw = p.Pill(right, y, $"{it.Price} zł", _g.Cash >= it.Price ? PillKind.Group : PillKind.Gray);
            p.Text(tx, y, it.Name, sel ? Ink.Brand : Ink.Dark, TextAlign.Left, right - pw - 4 - tx);
        }
        var dc = p.Card(card.End.Y + 6, 3);
        var lines = p.F.Wrap(d.Hurtownia[_list.Sel].Desc, (int)(right - tx));
        for (var k = 0; k < 2 && k < lines.Count; k++) p.Text(tx, p.RowY(dc, k), lines[k], Ink.Dim);
        p.Divider(dc, 2);
        p.Text(tx, p.RowY(dc, 2), $"HP {_g.Hero.Hp}/{_g.Hero.MaxHp}  {_g.Weapon.Name}", Ink.Dim, TextAlign.Left, right - tx);
    }
}
