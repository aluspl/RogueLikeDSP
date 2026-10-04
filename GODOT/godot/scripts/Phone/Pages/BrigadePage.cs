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
/// Spacja / dotknięcie zaznaczonego wzywa (zużywa turę), Esc wraca do gry. Pod fachowcami naprawy za materiały
/// (Załataj – drewno, Kładka – stal): indeksy listy za fachowcami, stan z Game.RepairBlocked.
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

    public override string Title => _g.D.Repairs.Length > 0 ? Loc.T("brygada_i_naprawy") : Loc.T("brygada_3");
    public override string Sub => Loc.F("budzet_zl", _g.Cash);
    public override string Hint => IsRepair ? Loc.T("spacja_napraw_tura_esc_wroc") : Loc.T("spacja_wezwij_tura_esc_wroc");
    public override PageAction[] Actions => [new(IsRepair ? Loc.T("napraw") : Loc.T("wezwij"), GameAction.A), new(Loc.T("wroc"), GameAction.Cancel)];

    private int Count => _g.D.Brigade.Length + _g.D.Repairs.Length;

    /// <summary>Zaznaczona jest naprawa (indeks za fachowcami).</summary>
    private bool IsRepair => _list.Sel >= _g.D.Brigade.Length;
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
            _list.Move(v, Count, Count);
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
        if (IsRepair)
        {
            var k = _list.Sel - d.Brigade.Length;
            var rd = d.Repairs[k];
            return _g.RepairBlocked(k) switch
            {
                RepairBlock.Ok => (Loc.T("gotowe_do_naprawy_zuzywa_ture"), Ink.Brand),
                RepairBlock.Material => (Loc.F("brak_materialu", d.Materials[rd.Material].Name, _g.Mats[rd.Material], rd.Cost), Ink.Late),
                RepairBlock.NoTarget => (Loc.T("brak_problemu_w_polu_widzenia_2"), Ink.Late),
                RepairBlock.NoRoom => (Loc.T("nie_ma_gdzie_postawic_desek"), Ink.Late),
                RepairBlock.NoPuddle => (Loc.T("brak_kaluz_obok_deszcz"), Ink.Late),
                _ => ("", Ink.Dim),
            };
        }
        if (_g.HelperCalled >= 0)
        {
            var t = _g.GuardTurns > 0 ? $" ({_g.GuardTurns} t.)" : _g.AllyTurns > 0 ? $" ({_g.AllyTurns} t.)" : "";
            return (Loc.F("na_tym_etapie_2", d.Brigade[_g.HelperCalled].Name, t), Ink.Done);
        }
        return _g.HelperBlocked(_list.Sel) switch
        {
            HelperBlock.Ok => (Loc.T("gotowy_do_wezwania_zuzywa_ture"), Ink.Brand),
            HelperBlock.Locked => (Loc.T("odblokuj_w_szkoleniach_koszty"), Ink.Dim),
            HelperBlock.Cash => (Loc.T("za_maly_budzet"), Ink.Late),
            HelperBlock.NoTarget => (Loc.F("nikogo_w_zasiegu_3", d.Brigade[_list.Sel].Reach), Ink.Late),
            HelperBlock.NoRoom => (Loc.T("brak_miejsca_obok_2"), Ink.Late),
            _ => ("", Ink.Dim),
        };
    }

    public override void Draw(PhonePainter p)
    {
        var d = _g.D;
        var n = d.Brigade.Length;
        _list.Clamp(Count, Count);
        var y = p.Section(p.Top, Loc.T("fachowcy"), Loc.T("raz_na_etap"));
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
            var pill = here ? Loc.T("na_placu") : unl ? Loc.F("zl_5", _g.HelperPrice(i)) : Loc.T("zablok");
            var kind = here ? PillKind.Done : unl && _g.Cash >= _g.HelperPrice(i) ? PillKind.Group : PillKind.Gray;
            var pw = p.Pill(right, ry, pill, kind);
            p.Text(tx, ry, h.Name, sel ? Ink.Brand : unl ? Ink.Dark : Ink.Dim, TextAlign.Left, right - pw - 4 - tx);
        }
        var bottom = card.End.Y;
        if (d.Repairs.Length > 0)
        {
            var ry0 = p.Section(bottom + 4, Loc.T("naprawy_2"), Loc.T("za_materialy"));
            var rc = p.Card(ry0, d.Repairs.Length);
            for (var k = 0; k < d.Repairs.Length; k++)
            {
                var rd = d.Repairs[k];
                var ry = p.RowY(rc, k);
                var idx = n + k;
                var sel = idx == _list.Sel;
                var ok = _g.RepairBlocked(k) == RepairBlock.Ok;
                if (sel) p.Selected(rc, k);
                else if (k > 0) p.Divider(rc, k);
                p.HitRow(rc, k, idx);
                var have = _g.Mats[rd.Material] >= rd.Cost;
                var pw = p.Pill(right, ry, $"{rd.Cost}x {d.Materials[rd.Material].Short} ({_g.Mats[rd.Material]})", ok ? PillKind.Group : have ? PillKind.Gray : PillKind.Late);
                MaterialIcon.Draw(p.C, rd.Material, new Godot.Vector2(tx, ry + (PhonePainter.RowH - MaterialIcon.Size) / 2));
                var nx = tx + MaterialIcon.Size + 4;
                p.Text(nx, ry, rd.Name, sel ? Ink.Brand : ok ? Ink.Dark : Ink.Dim, TextAlign.Left, right - pw - 4 - nx);
            }
            bottom = rc.End.Y;
        }
        var dc = p.Card(bottom + 6, 2);
        var desc = IsRepair ? d.Repairs[_list.Sel - n].Info : d.Brigade[_list.Sel].Desc;
        p.Text(tx, p.RowY(dc, 0), desc, Ink.Dim, TextAlign.Left, right - tx);
        p.Divider(dc, 1);
        var (text, ink) = Status();
        p.Text(tx, p.RowY(dc, 1), text, ink, TextAlign.Left, right - tx);
    }
}
