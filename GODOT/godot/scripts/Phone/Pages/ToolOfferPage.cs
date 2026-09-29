using LifeLike.Core;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Phone.Pages;

/// <summary>
/// Nowe narzędzie na polu przy ulepszonym (v0.21.50 cz. 3, tool_offer_dialog na GBA): obecne „Kielnia+2” i nowe,
/// porównanie ciosu i ostrzeżenie, że ulepszenie (i cecha) przepadnie. A zamieniam, B zostaję.
/// </summary>
public sealed class ToolOfferPage : PhonePage
{
    private readonly CoreGame _g;

    public ToolOfferPage(CoreGame g) => _g = g;

    public override string Title => "Nowe narzędzie";
    public override string Sub => _g.ToolOffer >= 0 ? _g.D.Weapons[_g.D.Tools[_g.ToolOffer].Weapon].Name : "";
    public override string Hint => "Spacja: zamieniam  Z: zostaję";
    public override PageAction[] Actions => [new("Zamieniam", GameAction.A), new("Zostaję", GameAction.B)];

    public override void Draw(PhonePainter p)
    {
        if (_g.ToolOffer < 0) return;
        var d = _g.D;
        var w = d.Tools[_g.ToolOffer].Weapon;
        var now = _g.WeaponBreakdown();
        var next = _g.WeaponBreakdown(-1, w);
        var y = p.Section(p.Top, "TERAZ");
        var c0 = p.Card(y, 2);
        var tx = p.TextX(c0);
        var right = c0.End.X - 6;
        p.Stripe(c0, 0, Pal.Brand);
        p.Text(tx, p.RowY(c0, 0), $"{_g.WeaponTitle()} {now.Min}-{now.Max}", Ink.Dark, TextAlign.Left, right - tx);
        p.Divider(c0, 1);
        p.Text(tx, p.RowY(c0, 1), DamageHelp.Text(d, now, DmgText.Upgrade), Ink.Dim, TextAlign.Left, right - tx);
        y = p.Section(c0.End.Y + 4, "NOWE");
        var c1 = p.Card(y, 2);
        p.Stripe(c1, 0, Pal.Prog);
        p.Text(tx, p.RowY(c1, 0), $"{d.Weapons[w].Name} {next.Min}-{next.Max}, zasięg {d.Weapons[w].Range}", Ink.Dark, TextAlign.Left, right - tx);
        p.Divider(c1, 1);
        var cmp = "Cios: " + DamageHelp.CompareLine(new Message(), now, next).Text;
        if (p.F.Measure(cmp) > right - tx) cmp = DamageHelp.CompareLine(new Message(), now, next, true).Text;
        p.Text(tx, p.RowY(c1, 1), cmp, next.Avg10 > now.Avg10 ? Ink.Done : Ink.Late, TextAlign.Left, right - tx);
        var c2 = p.Card(c1.End.Y + 6, 1);
        p.Stripe(c2, 0, Pal.Late);
        p.Text(tx, p.RowY(c2, 0), $"Uwaga: ulepszenie +{_g.WeaponLvl} przepadnie!", Ink.Late, TextAlign.Left, right - tx);
    }
}
