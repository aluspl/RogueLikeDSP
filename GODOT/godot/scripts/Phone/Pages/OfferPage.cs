using System.Collections.Generic;
using Godot;
using LifeLike.Core;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Phone.Pages;

/// <summary>
/// Paczka sprzętu przy zajętym slocie (gear_offer_dialog na GBA): obecny i nowy przedmiot z jakością (pastylka),
/// premią i cechą; porównanie ciosu, krytu i obrony (rozpiska #26: „teraz 4-7 -&gt; 5-9 (średnio +1,5)”);
/// A zakładam, B zostawiam (+doświadczenie).
/// </summary>
public sealed class OfferPage : PhonePage
{
    /// <summary>v0.23.2: wybory karty – Załóż, Do plecaka, Zostaw.</summary>
    public const int Take = 0, Store = 1, Leave = 2;

    private const int ChoiceBase = 50;
    private readonly CoreGame _g;

    public OfferPage(CoreGame g) => _g = g;

    /// <summary>Zaznaczony wybór (Take / Store / Leave).</summary>
    public int Sel { get; set; }

    /// <summary>Zatwierdzenie wyboru (OfferScreen.Decide).</summary>
    public System.Action<int> Decided { get; set; }

    public override string Title => Loc.T("paczka_sprzetu");
    public override string Sub => _g.OfferSlot >= 0 ? _g.D.GearSlots[_g.OfferSlot] : "";
    public override string Hint => Loc.T("karta_hint");
    public override PageAction[] Actions => [new(Loc.T("zostaw"), GameAction.B), new(Loc.T("wybierz"), GameAction.A)];

    public override void Enter() => Sel = _g.OfferIsBetter ? Take : !_g.BackpackFull() ? Store : Leave;

    public override bool Input(InputCmd e)
    {
        var d = e.VDir;
        if (d == 0) return false;
        Sel = (Sel + d + 3) % 3;
        if (Sel == Store && _g.BackpackFull()) Sel = (Sel + d + 3) % 3; // pełny plecak – pomija wyszarzony wybór
        Audio.Sfx.Play("menu");
        return true;
    }

    /// <summary>Dotknięcie wyboru: pierwsze zaznacza, drugie zatwierdza.</summary>
    public override bool TapRow(int index)
    {
        var k = index - ChoiceBase;
        if (k is < 0 or > 2 || (k == Store && _g.BackpackFull())) return false;
        if (k == Sel) Decided?.Invoke(k);
        else Sel = k;
        return true;
    }

    /// <summary>v0.23.2: trzy wybory karty (Załóż / Do plecaka / Zostaw) od y – wspólne z kartą broni.</summary>
    public static float Choices(PhonePainter p, float y, string[] rows, int sel, bool storeOff, int hitBase)
    {
        var c = p.Card(y, rows.Length);
        var tx = p.TextX(c);
        for (var k = 0; k < rows.Length; k++)
        {
            var off = k == Store && storeOff;
            if (k == sel) p.Selected(c, k);
            else
            {
                if (k > 0) p.Divider(c, k);
                p.Stripe(c, k, off ? Pal.Border : k == Take ? Pal.Done : k == Store ? Pal.Prog : Pal.Todo);
            }
            if (!off) p.HitRow(c, k, hitBase + k);
            p.Text(tx, p.RowY(c, k), rows[k], off ? Ink.Dim : k == sel ? Ink.Brand : Ink.Dark, TextAlign.Left, c.End.X - 6 - tx);
        }
        return c.End.Y;
    }

    public override void Draw(PhonePainter p)
    {
        var g = _g;
        var d = g.D;
        var slot = g.OfferSlot;
        if (slot < 0) return;
        var y = p.Section(p.Top, Loc.T("teraz_5"));
        y = Item(p, y, g.Equipped[slot], g.EquippedTrait[slot], false) + 4;
        y = p.Section(y, Loc.T("nowy_2"), g.OfferIsBetter ? Loc.T("lepszy") : "");
        y = Item(p, y, g.OfferRarity, g.OfferTrait, true) + 6;
        var cmp = Compare(p, (int)(p.Width - 24));
        var tw = (int)(p.Width - 24);
        var traitText = g.Equipped[slot] >= 0 && g.EquippedTrait[slot] != g.OfferTrait // v0.23.0 (#91): wprost „Cecha: stara -> nowa”
            ? Loc.F("cecha_zmiana", d.GearTraits[g.EquippedTrait[slot]].Name, d.GearTraits[g.OfferTrait].Name)
            : Loc.T("cecha_2") + d.GearTraits[g.OfferTrait].Name;
        var traitLines = p.F.Wrap(traitText, tw);
        var warnLines = g.OfferDropsHolster ? p.F.Wrap(HolsterWarning(), tw) : new List<string>(); // v0.23.0: pas bez kabury zdejmie kaburę
        var c = p.Card(y, 1 + cmp.Count + traitLines.Count + warnLines.Count);
        var verdict = g.OfferIsBetter ? Loc.T("nowy_jest_lepszej_jakosci")
                    : g.OfferRarity == g.Equipped[slot] ? Loc.T("ta_sama_jakosc_inna_cecha") : Loc.T("nowy_jest_gorszej_jakosci");
        p.Stripe(c, 0, g.OfferIsBetter ? Pal.Done : Pal.Todo);
        p.Text(p.TextX(c), p.RowY(c, 0), verdict, g.OfferIsBetter ? Ink.Done : Ink.Dark, TextAlign.Left, c.End.X - 6 - p.TextX(c));
        for (var i = 0; i < cmp.Count; i++)
        {
            p.Divider(c, 1 + i);
            p.Text(p.TextX(c), p.RowY(c, 1 + i), cmp[i], Ink.Brand, TextAlign.Left, c.End.X - 6 - p.TextX(c));
        }
        for (var i = 0; i < traitLines.Count; i++)
        {
            p.Divider(c, 1 + cmp.Count + i);
            p.Text(p.TextX(c), p.RowY(c, 1 + cmp.Count + i), traitLines[i], g.Equipped[slot] >= 0 && g.EquippedTrait[slot] != g.OfferTrait ? Ink.Brand : Ink.Dim, TextAlign.Left, c.End.X - 6 - p.TextX(c));
        }
        var row = 1 + cmp.Count + traitLines.Count;
        for (var i = 0; i < warnLines.Count; i++)
        {
            p.Divider(c, row + i);
            p.Stripe(c, row + i, Pal.Late);
            p.Text(p.TextX(c), p.RowY(c, row + i), warnLines[i], Ink.Late, TextAlign.Left, c.End.X - 6 - p.TextX(c));
        }
        // v0.23.2: trzy wybory
        var full = g.BackpackFull();
        var xp = g.OfferDeclineXp();
        string[] rows =
        [
            full ? Loc.T("karta_zaloz_ziemia") : Loc.T("karta_zaloz_plecak"),
            full ? Loc.F("karta_plecak_pelny", g.BackpackCount(), g.BackpackSlots()) : Loc.F("karta_do_plecaka", g.BackpackCount(), g.BackpackSlots()),
            xp > 0 ? Loc.F("karta_zostaw_xp", xp) : Loc.T("karta_zostaw"),
        ];
        Choices(p, c.End.Y + 4, rows, Sel, full, ChoiceBase);
    }

    /// <summary>v0.23.0: ostrzeżenie, że pas bez kabury zdejmie pas z kaburą (broń z kabury wypadnie na ziemię).</summary>
    private string HolsterWarning() =>
        _g.Weapon2 >= 0 ? Loc.F("uwaga_kabura", _g.D.Weapons[_g.Weapon2].Name) : Loc.T("uwaga_kabura_pusta");

    /// <summary>Co się zmieni po założeniu: cios (średnio), kryt, obrona - tylko to, co się zmienia (jak na GBA).</summary>
    private List<string> Compare(PhonePainter p, int width)
    {
        var g = _g;
        var slot = g.OfferSlot;
        var now = g.WeaponBreakdown();
        var next = g.WeaponBreakdown(-1, -1, slot, g.OfferRarity, g.OfferTrait);
        var list = new List<string>();
        if (now.Min != next.Min || now.Max != next.Max || now.Avg10 != next.Avg10)
        {
            var line = Loc.T("cios_2") + DamageHelp.CompareLine(new Message(), now, next).Text;
            list.Add(p.F.Measure(line) <= width ? line : DamageHelp.CompareLine(new Message(), now, next, true).Text);
        }
        if (now.CritChance() != next.CritChance() || now.CritMax != next.CritMax) list.Add(DamageHelp.CompareCrit(new Message(), now, next).Text);
        var old = g.D.Gear[slot * 3 + g.Equipped[slot]];
        var gnew = g.D.Gear[slot * 3 + g.OfferRarity];
        if (old.Stat == LifeLike.Core.Data.GearStat.Def && gnew.Value != old.Value)
        {
            int d0 = g.HeroDefense(), d1 = d0 - old.Value + gnew.Value;
            list.Add(Loc.F("obr_z_ciosu", d0, d1, d0 / 2, d1 / 2));
        }
        return list;
    }

    private float Item(PhonePainter p, float y, int rarity, int trait, bool isNew)
    {
        var d = _g.D;
        var gdName = rarity < 0 ? null : d.Gear[_g.OfferSlot * 3 + rarity];
        var statText = gdName is null ? "" : $"{UiText.GearStatName(gdName.Stat)} +{gdName.Value}, ";
        var traitName = d.GearTraits[trait].Name;
        var wide = gdName is not null && p.F.Measure(statText + traitName) > p.Width - 12 - 46 - 6; // v0.23.0 (#91): cecha pełną nazwą w osobnym wierszu, gdy nie mieści się obok premii
        var card = p.CardH(y, (wide ? 3 : 2) * PhonePainter.RowH + 8);
        if (rarity < 0)
        {
            p.Text(p.TextX(card), p.RowY(card, 0), Loc.T("brak_2"), Ink.Dim);
            return card.End.Y;
        }
        var gd = d.Gear[_g.OfferSlot * 3 + rarity];
        var right = card.End.X - 6;
        var tx = p.TextX(card);
        var stripe = isNew ? Pal.Prog : Pal.Todo;
        p.C.DrawRect(new Rect2(card.Position.X + 4, card.Position.Y + 6, 3, 2 * PhonePainter.RowH - 4), stripe);
        var photo = new Vector2(tx - 2, card.Position.Y + 4);
        p.Icon(Assets.Actors, Assets.FrameGear + rarity, Assets.Actor, photo);
        tx += 34;
        var pw = p.Pill(right, p.RowY(card, 0), d.GearRarities[rarity], rarity == 2 ? PillKind.Prog : rarity == 1 ? PillKind.Group : PillKind.Gray);
        p.Text(tx, p.RowY(card, 0), gd.Name, Ink.Dark, TextAlign.Left, right - pw - 4 - tx);
        var ink = isNew ? Ink.Brand : Ink.Dim;
        if (wide)
        {
            p.Text(tx, p.RowY(card, 1), $"{UiText.GearStatName(gd.Stat)} +{gd.Value},", ink, TextAlign.Left, right - tx);
            p.Text(tx, p.RowY(card, 2), traitName, ink, TextAlign.Left, right - tx);
        }
        else p.Text(tx, p.RowY(card, 1), statText + traitName, ink, TextAlign.Left, right - tx);
        return card.End.Y;
    }
}
