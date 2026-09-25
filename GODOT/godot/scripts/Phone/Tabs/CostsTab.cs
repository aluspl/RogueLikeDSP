using System;
using LifeLike.Core;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game;

/// <summary>
/// Koszty = Szkolenia w trakcie budowy (tab_costs na GBA): całkowity koszt, pasek budżetu, „Pozostało” = doświadczenie,
/// doświadczenie z tej budowy i najbliższe zlecenie. Kupuje się po budowie (telefon profilu na tytule).
/// </summary>
public sealed class CostsTab : PhonePage
{
    private readonly CoreGame _g;
    private readonly Profile _p;

    public CostsTab(CoreGame g, Profile p)
    {
        _g = g;
        _p = p;
    }

    public override string Title => "Koszty";
    public override string Sub => "Szkolenia";

    public override void Draw(PhonePainter p)
    {
        var d = _g.D;
        int total = Meta.ShopTotalCost(d), spent = Meta.ShopSpent(d, _p);
        var y = p.Section(p.Top, "CAŁKOWITY KOSZT");
        var c0 = p.Card(y, 4);
        var tx = p.TextX(c0);
        var right = c0.End.X - 6;
        var r = p.RowY(c0, 0);
        p.Bold(tx, r + 1, $"{spent} dośw.", Ink.Dark);
        p.Text(right, r, $"{(total > 0 ? spent * 100 / total : 0)}%", Ink.Brand, TextAlign.Right);
        p.Bar(tx, p.RowY(c0, 1), right - tx, spent, total, Pal.Brand, 8);
        r = p.RowY(c0, 2);
        p.Text(tx, r, $"Budżet {total}", Ink.Dim);
        p.Text(right, r, $"Pozostało {_p.Xp}", Ink.Done, TextAlign.Right);
        r = p.RowY(c0, 3);
        p.Divider(c0, 3);
        p.Text(tx, r, $"Z tej budowy: +{_g.Xp - _g.XpBanked}", Ink.Dim);

        y = p.Section(c0.End.Y + 4, "ZLECENIE");
        var c1 = p.Card(y, 2);
        var ci = Meta.NextContract(d, _p, _g);
        r = p.RowY(c1, 0);
        if (ci >= 0)
        {
            var c = d.Contracts[ci];
            p.Stripe(c1, 0, Pal.Prog);
            var pw = p.Pill(right, r, $"{Math.Min(c.Target, Meta.ContractProgressLive(d, _p, _g, ci))}/{c.Target}", PillKind.Prog);
            p.Text(tx, r, c.Name, Ink.Dark, TextAlign.Left, right - pw - 4 - tx);
            p.Divider(c1, 1);
            p.Text(tx, p.RowY(c1, 1), c.Desc, Ink.Dim, TextAlign.Left, right - tx);
        }
        else
        {
            p.Text(tx, r, "Wszystkie zlecenia wykonane", Ink.Done);
        }

        y = p.Section(c1.End.Y + 4, "SZKOLENIA");
        var c2 = p.Card(y, 1);
        p.Text(p.TextX(c2), p.RowY(c2, 0), "Kupisz po budowie (profil: Koszty)", Ink.Dim, TextAlign.Left, right - p.TextX(c2));
    }
}
