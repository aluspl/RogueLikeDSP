using System;
using LifeLike.Core;
using LifeLike.Core.Data;
using LifeLike.Game.Audio;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;

namespace LifeLike.Game.Phone.Pages;

/// <summary>
/// v0.21.52 cz. d (#47): Mapa kariery (run_career na GBA) – kontrakty z ikoną bossa (zablokowane: ciemna sylwetka),
/// pastylka: wygrane / ukończone etapy / warunek odblokowania; pod listą opis zaznaczonego, liczba etapów, boss i nagroda
/// za pierwszą wygraną. Strzałki / stuknięcie zaznacza, Spacja / drugie stuknięcie wybiera (zablokowany – dźwięk).
/// </summary>
public sealed class CareerPage : PhonePage
{
    private readonly GameData _d;
    private readonly Profile _p;
    private readonly Action<int> _pick;
    private readonly ListState _list = new();

    public CareerPage(GameData d, Profile p, Action<int> pick)
    {
        _d = d;
        _p = p;
        _pick = pick;
        _list.Sel = Career.Selected(d, p);
    }

    public override string Title => Loc.T("mapa_kariery");
    public override string Sub => Loc.F("wygrane_3", Career.WonCount(_d, _p), _d.Career.Length);
    public override string Hint => Loc.T("spacja_wybierz_esc_wroc");
    public override PageAction[] Actions => [new(Loc.T("wybierz"), GameAction.A), new(Loc.T("wroc"), GameAction.Cancel)];
    public override bool Closable => true;

    public int Sel
    {
        get => _list.Sel;
        set => _list.Sel = value;
    }

    public override bool TapRow(int index)
    {
        if (index == _list.Sel) _pick(index);
        _list.Sel = index;
        Redraw();
        return true;
    }

    public override bool Input(InputCmd e)
    {
        var v = e.VDir;
        if (v != 0)
        {
            _list.Move(v, _d.Career.Length, _d.Career.Length);
            Sfx.Play("menu");
            Redraw();
            return true;
        }
        if (!e.Is(GameAction.A | GameAction.Start)) return false;
        _pick(_list.Sel);
        return true;
    }

    private string PillText(int k)
    {
        var kd = _d.Career[k];
        if (!Career.Unlocked(_d, _p, k)) return Career.UnlockLabel(_d, k);
        if (_p.CareerWins[k] > 0) return Loc.F("wygrane_4", _p.CareerWins[k]);
        return Loc.F("etapy_2", _p.CareerBest[k], kd.Count - kd.Prelude);
    }

    public override void Draw(PhonePainter p)
    {
        var n = _d.Career.Length;
        _list.Clamp(n, n);
        var y = p.Section(p.Top, Loc.T("kontrakty"), Loc.T("budynki_z_wlasnymi_etapami"));
        var card = p.Card(y, n);
        var tx = p.TextX(card);
        var right = card.End.X - 6;
        for (var k = 0; k < n; k++)
        {
            var kd = _d.Career[k];
            var ry = p.RowY(card, k);
            var sel = k == _list.Sel;
            var unl = Career.Unlocked(_d, _p, k);
            var won = Career.Won(_p, k);
            if (sel) p.Selected(card, k);
            else if (k > 0) p.Divider(card, k);
            p.HitRow(card, k, k);
            var pw = p.Pill(right, ry, PillText(k), !unl ? PillKind.Gray : won ? PillKind.Done : PillKind.Prog);
            var boss = kd.Boss >= 0 ? kd.Boss : _d.EnemyIndex("termin");
            var ic = new Godot.Rect2(tx - 2, ry + (PhonePainter.RowH - 16) / 2f, 16, 16);
            p.C.DrawTextureRectRegion(Assets.Actors, ic, Assets.Frame(_d.Enemies[boss].Frame, Assets.Actor),
                unl ? Godot.Colors.White : new Godot.Color(0, 0, 0, 0.8f));
            p.Text(tx + 18, ry, kd.Name, sel ? Ink.Brand : unl ? Ink.Dark : Ink.Dim, TextAlign.Left, right - pw - 4 - tx - 18);
        }
        var s = _d.Career[_list.Sel];
        var dc = p.Card(card.End.Y + 6, 3);
        p.Text(tx, p.RowY(dc, 0), s.Desc, Ink.Dark, TextAlign.Left, right - tx);
        p.Divider(dc, 1);
        var bossLine = s.Boss >= 0 ? Loc.F("etapow_boss", s.Count - s.Prelude, _d.Enemies[s.Boss].Name) : Loc.F("etapow_akt_0_tez_budowa_dnia", s.Count - s.Prelude);
        p.Text(tx, p.RowY(dc, 1), bossLine, Ink.Dim, TextAlign.Left, right - tx);
        p.Divider(dc, 2);
        string reward;
        Ink ink;
        if (!Career.Unlocked(_d, _p, _list.Sel))
        {
            reward = Loc.F("odblokujesz", Career.UnlockLabel(_d, _list.Sel));
            ink = Ink.Late;
        }
        else if (s.Boss >= 0 && !Career.Won(_p, _list.Sel))
        {
            reward = Loc.F("nagroda_2", Career.RewardLabel(_d, _list.Sel));
            ink = Ink.Brand;
        }
        else
        {
            reward = _p.CareerWins[_list.Sel] > 0 ? Loc.F("wygrany_raz_y", _p.CareerWins[_list.Sel]) : Loc.T("jeszcze_bez_wygranej");
            ink = Ink.Done;
        }
        p.Text(tx, p.RowY(dc, 2), reward, ink, TextAlign.Left, right - tx);
    }
}
