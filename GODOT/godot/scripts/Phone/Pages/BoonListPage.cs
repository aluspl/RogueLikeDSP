using LifeLike.Core;
using System;
using System.Collections.Generic;
using LifeLike.Game.Audio;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Phone.Pages;

/// <summary>
/// Telefon > Sprzęt > Premie (v0.21.50 cz. 2): wybrane premie po etapach (pasek w kolorze rzadkości, nazwa, skutek;
/// zaznaczona – opis ze znacznikami) i strona Synergie (aktywne na zielono, pozostałe z postępem znaczników).
/// Góra/dół – lista, lewo/prawo – premie / synergie.
/// </summary>
public sealed class BoonListPage : PhonePage
{
    private readonly CoreGame _g;
    private readonly ListState _list = new();

    public BoonListPage(CoreGame g, int mode = 0)
    {
        _g = g;
        Mode = mode;
    }

    /// <summary>0 = premie, 1 = synergie.</summary>
    public int Mode { get; set; }

    public override string Title => Mode == 0 ? Loc.T("premie_3") : Loc.T("synergie");
    public override string Sub => Mode == 0 ? Loc.F("z_etapow", _g.BoonsOwned()) : Loc.F("aktywne", Count(_g.SynergyMask()));
    public override string Hint => Loc.T("l_p_premie_synergie_esc_wroc");
    public override bool Closable => true;
    public override PageAction[] Actions => [new(Mode == 0 ? Loc.T("synergie") : Loc.T("premie_3"), GameAction.Right)];

    private List<int> Owned()
    {
        var l = new List<int>();
        for (var b = 0; b < _g.D.Boons.Length; b++)
        {
            if (_g.HasBoon(b)) l.Add(b);
        }
        return l;
    }

    private static int Count(int m)
    {
        var n = 0;
        for (; m != 0; m &= m - 1) n++;
        return n;
    }

    public override bool TapRow(int index)
    {
        _list.Sel = index;
        return true;
    }

    public override bool Input(InputCmd e)
    {
        if (e.HDir != 0)
        {
            Mode = 1 - Mode;
            _list.Reset();
            Sfx.Play("menu");
            return true;
        }
        if (e.VDir != 0)
        {
            _list.Move(e.VDir, Mode == 0 ? Owned().Count : _g.D.Synergies.Length, Window);
            Sfx.Play("menu");
            return true;
        }
        return false;
    }

    private int _window = 7;

    private int Window => _window;

    /// <summary>Ile wierszy listy mieści się nad kartą opisu (detailRows wierszy).</summary>
    private int Fit(PhonePainter p, float y, int detailRows) =>
        Math.Max(3, (int)((p.Bottom - y - 8 - 6 - (detailRows * PhonePainter.RowH + 8) - 4) / PhonePainter.RowH));

    public override void Draw(PhonePainter p)
    {
        if (Mode == 0) DrawBoons(p);
        else DrawSynergies(p);
    }

    private void DrawBoons(PhonePainter p)
    {
        var d = _g.D;
        var owned = Owned();
        var y = p.Section(p.Top, Loc.T("premie_z_etapow"), Loc.T("n1_z_3_po_etapie"));
        _window = Fit(p, y, 3);
        if (owned.Count == 0)
        {
            var c = p.Card(y, 2);
            p.Text(p.TextX(c), p.RowY(c, 0), Loc.T("brak_premii_pierwsza_po_etapie"), Ink.Dim);
            p.Text(p.TextX(c), p.RowY(c, 1), Loc.T("n3_do_wyboru_zwykla_rzadka"), Ink.Dim);
            return;
        }
        _list.Clamp(owned.Count, Window);
        var rows = Math.Min(Window, owned.Count - _list.Top);
        var card = p.Card(y, rows);
        var tx = p.TextX(card);
        var right = card.End.X - 6;
        for (var r = 0; r < rows; r++)
        {
            var i = _list.Top + r;
            var bd = d.Boons[owned[i]];
            var ry = p.RowY(card, r);
            var sel = i == _list.Sel;
            if (sel) p.Selected(card, r);
            else if (r > 0) p.Divider(card, r);
            p.Stripe(card, r, BoonLook.RarityColor(bd.Rarity));
            p.HitRow(card, r, i);
            var nw = p.Text(tx, ry, p.F.Fit(BoonLook.CueName(d, bd.Name, bd.Rarity), (int)((right - tx) * 0.55f)), sel ? Ink.Brand : Ink.Dark);
            p.Text(tx + nw + 6, ry, bd.Desc, BoonLook.RarityInk(bd.Rarity), TextAlign.Left, right - tx - nw - 6);
        }
        var b = d.Boons[owned[Math.Clamp(_list.Sel, 0, owned.Count - 1)]];
        var dc = p.Card(card.End.Y + 6, 3);
        var dtx = p.TextX(dc);
        var dr = dc.End.X - 6;
        var pw = p.Pill(dr, p.RowY(dc, 0), BoonLook.RarityName(d, b.Rarity), BoonLook.RarityPill(b.Rarity));
        p.Text(dtx, p.RowY(dc, 0), b.Name, Ink.Dark, TextAlign.Left, dr - pw - 6 - dtx);
        p.Text(dtx, p.RowY(dc, 1), b.Desc, BoonLook.RarityInk(b.Rarity), TextAlign.Left, dr - dtx);
        p.Divider(dc, 2);
        var tags = string.Join(", ", BoonLook.Tags(d, b.Tags)) + (b.Cls >= 0 ? Loc.F("tylko", d.Classes[b.Cls].Name) : "");
        p.Text(dtx, p.RowY(dc, 2), Loc.T("znaczniki") + tags, Ink.Dim, TextAlign.Left, dr - dtx);
    }

    private void DrawSynergies(PhonePainter p)
    {
        var d = _g.D;
        var n = d.Synergies.Length;
        var y = p.Section(p.Top, Loc.T("synergie_3"), Loc.F("premie_z_jednym_znacznikiem", d.SynergyAt));
        _window = Fit(p, y, 2);
        _list.Clamp(n, Window);
        var rows = Math.Min(Window, n - _list.Top);
        var card = p.Card(y, rows);
        var tx = p.TextX(card);
        var right = card.End.X - 6;
        for (var r = 0; r < rows; r++)
        {
            var s = _list.Top + r;
            var sd = d.Synergies[s];
            var on = _g.SynergyActive(s);
            var ry = p.RowY(card, r);
            var sel = s == _list.Sel;
            if (sel) p.Selected(card, r);
            else if (r > 0) p.Divider(card, r);
            p.Stripe(card, r, on ? Pal.Done : Pal.Todo);
            p.HitRow(card, r, s);
            var progress = new List<string>();
            for (var t = 0; t < d.BoonTags.Length; t++)
            {
                if (((sd.Tags >> t) & 1) != 0) progress.Add($"{d.BoonTags[t]} {_g.TagCount(t)}");
            }
            var pw = p.Pill(right, ry, on ? Loc.T("aktywna") : string.Join("+", progress), on ? PillKind.Done : PillKind.Gray);
            p.Text(tx, ry, sd.Name, on ? Ink.Done : sel ? Ink.Brand : Ink.Dark, TextAlign.Left, right - pw - 6 - tx);
        }
        var sel2 = d.Synergies[Math.Clamp(_list.Sel, 0, n - 1)];
        var dc = p.Card(card.End.Y + 6, 2);
        var dtx = p.TextX(dc);
        p.Text(dtx, p.RowY(dc, 0), sel2.Desc, Ink.Dark, TextAlign.Left, dc.End.X - 6 - dtx);
        var need = string.Join(" + ", BoonLook.Tags(d, sel2.Tags));
        p.Text(dtx, p.RowY(dc, 1), Loc.T("znaczniki") + need, Ink.Dim, TextAlign.Left, dc.End.X - 6 - dtx);
    }
}
