using System;
using LifeLike.Core;
using LifeLike.Core.Data;
using LifeLike.Game.Audio;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;

namespace LifeLike.Game.Phone.Pages;

/// <summary>
/// Tryb inwestora (run_investor na GBA): modyfikatory trudności po pierwszej wygranej - lista z pastylką stawki
/// (zielona „WŁ” = włączony), opis zaznaczonego, suma: stawka, premia doświadczenia i rekord stawki zawodu.
/// Spacja / dotknięcie zaznaczonego włącza i wyłącza (zapis profilu od razu), Esc wraca do wyboru zawodu.
/// </summary>
public sealed class InvestorPage : PhonePage
{
    private readonly GameData _d;
    private readonly Profile _p;
    private readonly int _cls;
    private readonly Action _saved;
    private readonly ListState _list = new();

    public InvestorPage(GameData d, Profile p, int cls, Action saved)
    {
        _d = d;
        _p = p;
        _cls = cls;
        _saved = saved;
    }

    public override string Title => "Tryb inwestora";
    public override string Sub => $"Stawka {Investor.Stake(_d, Meta.InvestorMask(_d, _p))}";
    public override string Hint => "Spacja: wł./wył.  Esc: wróć";
    public override PageAction[] Actions => [new("Wł. / wył.", GameAction.A), new("Gotowe", GameAction.Cancel)];
    public override bool Closable => true;

    public int Sel
    {
        get => _list.Sel;
        set => _list.Sel = value;
    }

    public void Toggle()
    {
        Meta.ToggleInvestor(_p, _list.Sel);
        _saved?.Invoke();
        Sfx.Play("buy");
        Redraw();
    }

    public override bool TapRow(int index)
    {
        _list.Sel = index;
        Toggle();
        return true;
    }

    public override bool Input(InputCmd e)
    {
        var v = e.VDir;
        if (v != 0)
        {
            _list.Move(v, _d.Investor.Length, _d.Investor.Length);
            Sfx.Play("menu");
            return true;
        }
        if (!e.Is(GameAction.A)) return false;
        Toggle();
        return true;
    }

    public override void Draw(PhonePainter p)
    {
        var n = _d.Investor.Length;
        var mask = Meta.InvestorMask(_d, _p);
        _list.Clamp(n, n);
        var y = p.Section(p.Top, "MODYFIKATORY", "za doświadczenie");
        var card = p.Card(y, n);
        var tx = p.TextX(card);
        var right = card.End.X - 6;
        for (var i = 0; i < n; i++)
        {
            var m = _d.Investor[i];
            var on = ((mask >> i) & 1) != 0;
            var sel = i == _list.Sel;
            var ry = p.RowY(card, i);
            if (sel) p.Selected(card, i);
            else if (i > 0) p.Divider(card, i);
            if (on && !sel) p.Stripe(card, i, Pal.Done);
            p.HitRow(card, i, i);
            var pw = p.Pill(right, ry, on ? $"WŁ +{m.Stake}" : $"+{m.Stake}", on ? PillKind.Done : PillKind.Gray);
            p.Text(tx, ry, m.Name, sel ? Ink.Brand : on ? Ink.Dark : Ink.Dim, TextAlign.Left, right - pw - 4 - tx);
        }
        var cur = _d.Investor[_list.Sel];
        var dc = p.Card(card.End.Y + 6, 3);
        p.Text(tx, p.RowY(dc, 0), $"{cur.Desc}, dośw. +{cur.XpPct}%", Ink.Dim, TextAlign.Left, right - tx);
        p.Divider(dc, 1);
        p.Text(tx, p.RowY(dc, 1), $"Stawka {Investor.Stake(_d, mask)}, doświadczenie +{Investor.Xp(_d, mask)}%", Ink.Dark, TextAlign.Left, right - tx);
        p.Divider(dc, 2);
        p.Text(tx, p.RowY(dc, 2), $"Rekord: {_d.Classes[_cls].Name} - stawka {_p.BestStake[_cls]}", Ink.Done, TextAlign.Left, right - tx);
    }
}
