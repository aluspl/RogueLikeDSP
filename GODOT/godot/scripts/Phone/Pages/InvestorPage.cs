using System;
using Godot;
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
/// v0.21.51 cz. 2: za modyfikatorami wiersz wyglądu „Kask w paski” (po sekretnym zleceniu Na styk; tylko wygląd).
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

    /// <summary>Wiersz wyglądu „Kask w paski” (odblokowany sekretnym zleceniem).</summary>
    private bool Stripes => _d.CosmeticStripes >= 0 && Secrets.CosmeticUnlocked(_d, _p, _d.CosmeticStripes);

    private int Rows => _d.Investor.Length + (Stripes ? 1 : 0);

    public void Toggle()
    {
        if (_list.Sel >= _d.Investor.Length) Secrets.ToggleCosmetic(_d, _p, _d.CosmeticStripes);
        else Meta.ToggleInvestor(_p, _list.Sel);
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
            _list.Move(v, Rows, Rows);
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
        _list.Clamp(Rows, Rows);
        var y = p.Section(p.Top, "MODYFIKATORY", "za doświadczenie");
        var card = p.Card(y, Rows);
        var tx = p.TextX(card);
        var right = card.End.X - 6;
        for (var i = 0; i < Rows; i++)
        {
            if (i >= n) // wygląd: kask w paski
            {
                var son = Secrets.CosmeticOn(_d, _p, _d.CosmeticStripes);
                var ssel = i == _list.Sel;
                var sy = p.RowY(card, i);
                if (ssel) p.Selected(card, i);
                else p.Divider(card, i);
                p.HitRow(card, i, i);
                var spw = p.Pill(right, sy, son ? "WŁ" : "Wygląd", son ? PillKind.Done : PillKind.Gray);
                p.Icon(Assets.UiMenu, Assets.MenuStripes, Assets.Icon, new Vector2(tx - 2, sy + (PhonePainter.RowH - 16) / 2f));
                p.Text(tx + 18, sy, _d.Cosmetics[_d.CosmeticStripes].Name, ssel ? Ink.Brand : son ? Ink.Dark : Ink.Dim, TextAlign.Left, right - spw - 4 - tx - 18);
                continue;
            }
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
        var dc = p.Card(card.End.Y + 6, 3);
        var desc = _list.Sel >= n ? "Wygląd: " + _d.Cosmetics[_d.CosmeticStripes].Desc
                 : $"{_d.Investor[_list.Sel].Desc}, dośw. +{_d.Investor[_list.Sel].XpPct}%";
        p.Text(tx, p.RowY(dc, 0), desc, Ink.Dim, TextAlign.Left, right - tx);
        p.Divider(dc, 1);
        p.Text(tx, p.RowY(dc, 1), $"Stawka {Investor.Stake(_d, mask)}, doświadczenie +{Investor.Xp(_d, mask)}%", Ink.Dark, TextAlign.Left, right - tx);
        p.Divider(dc, 2);
        p.Text(tx, p.RowY(dc, 2), $"Rekord: {_d.Classes[_cls].Name} - stawka {Meta.BestStake(_p, _cls)}", Ink.Done, TextAlign.Left, right - tx);
    }
}
