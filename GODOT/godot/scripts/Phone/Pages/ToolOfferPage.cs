using System;
using Godot;
using LifeLike.Core;
using LifeLike.Game.Audio;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Phone.Pages;

/// <summary>
/// Nowe narzędzie na polu przy ulepszonym (v0.21.50 cz. 3, tool_offer_dialog na GBA): obecne „Kielnia+2” i nowe,
/// porównanie ciosu i ostrzeżenie, że ulepszenie (i cecha) przepadnie. v0.21.51: dwa wiersze wyboru – zaznaczony na
/// start „Zostaję” – strzałki / dotknięcie zaznacza, A / Enter / „Wybierz” (albo drugie dotknięcie) zatwierdza,
/// B / Esc / „Zostaję” zostawia – zamiana nigdy nie dzieje się jednym przypadkowym dotknięciem.
/// </summary>
public sealed class ToolOfferPage : PhonePage
{
    private readonly CoreGame _g;

    public ToolOfferPage(CoreGame g) => _g = g;

    public override string Title => Loc.T("nowe_narzedzie");
    public override string Sub => _g.ToolOffer >= 0 ? _g.D.Weapons[_g.D.Tools[_g.ToolOffer].Weapon].Name : "";
    /// <summary>Zaznaczony wybór: 0 = zostaję przy ulepszonym (domyślnie), 1 = zamieniam.</summary>
    public int Sel { get; set; }

    /// <summary>Zatwierdzenie: true = zamiana (ToolOfferScreen.Decide).</summary>
    public Action<bool> Decided;

    public override string Hint => Loc.T("strzalki_wybor_spacja_enter");
    public override PageAction[] Actions => [new(Loc.T("wybierz"), GameAction.A), new(Loc.T("zostaje_2"), GameAction.B)];

    public override void Enter() => Sel = 0;

    public override bool Input(InputCmd e)
    {
        var d = e.VDir != 0 ? e.VDir : e.HDir;
        if (d == 0) return false;
        Sel = 1 - Sel;
        Sfx.Play("menu");
        return true;
    }

    /// <summary>Dotknięcie wiersza: pierwsze zaznacza, drugie na zaznaczonym zatwierdza.</summary>
    public override bool TapRow(int index)
    {
        if (index is < 0 or > 1) return false;
        if (index == Sel) Decided?.Invoke(Sel == 1);
        else Sel = index;
        return true;
    }

    public override void Draw(PhonePainter p)
    {
        if (_g.ToolOffer < 0) return;
        var d = _g.D;
        var w = d.Tools[_g.ToolOffer].Weapon;
        var now = _g.WeaponBreakdown();
        var next = _g.WeaponBreakdown(-1, w);
        var y = p.Section(p.Top, Loc.T("teraz_5"));
        var c0 = p.Card(y, 2);
        var tx = p.TextX(c0);
        var right = c0.End.X - 6;
        p.Stripe(c0, 0, Pal.Brand);
        p.Text(tx, p.RowY(c0, 0), $"{_g.WeaponTitle()} {now.Min}-{now.Max}", Ink.Dark, TextAlign.Left, right - tx);
        p.Divider(c0, 1);
        p.Text(tx, p.RowY(c0, 1), DamageHelp.Text(d, now, DmgText.Upgrade), Ink.Dim, TextAlign.Left, right - tx);
        y = p.Section(c0.End.Y + 4, Loc.T("nowe_4"));
        var c1 = p.Card(y, 2);
        p.Stripe(c1, 0, Pal.Prog);
        p.Text(tx, p.RowY(c1, 0), Loc.F("zasieg_3", d.Weapons[w].Name, next.Min, next.Max, d.Weapons[w].Range), Ink.Dark, TextAlign.Left, right - tx);
        p.Divider(c1, 1);
        var cmp = Loc.T("cios_2") + DamageHelp.CompareLine(new Message(), now, next).Text;
        if (p.F.Measure(cmp) > right - tx) cmp = DamageHelp.CompareLine(new Message(), now, next, true).Text;
        p.Text(tx, p.RowY(c1, 1), cmp, next.Avg10 > now.Avg10 ? Ink.Done : Ink.Late, TextAlign.Left, right - tx);
        y = p.Section(c1.End.Y + 4, Loc.T("wybierz_3"));
        var c2 = p.Card(y, 2);
        string[] rows = [Loc.F("zostaje_3", _g.WeaponTitle()), Loc.F("zamieniam_przepadnie", d.Weapons[w].Name, _g.WeaponLvl)];
        for (var k = 0; k < 2; k++)
        {
            if (k > 0) p.Divider(c2, k);
            if (k == Sel) p.Selected(c2, k);
            else p.Stripe(c2, k, k == 1 ? Pal.Late : Pal.Todo);
            p.HitRow(c2, k, k);
            p.Text(tx, p.RowY(c2, k), rows[k], k == Sel ? Ink.Brand : k == 1 ? Ink.Late : Ink.Dark, TextAlign.Left, right - tx);
        }
    }
}
