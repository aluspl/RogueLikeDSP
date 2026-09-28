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
    private readonly CoreGame _g;

    public OfferPage(CoreGame g) => _g = g;

    public override string Title => "Paczka sprzętu";
    public override string Sub => _g.OfferSlot >= 0 ? _g.D.GearSlots[_g.OfferSlot] : "";
    public override string Hint => $"Spacja: zakładam  Z: zostawiam (+{_g.D.GearDeclineXp + _g.OfferRarity})";
    public override PageAction[] Actions => [new("Zakładam", GameAction.A), new($"Zostawiam +{_g.D.GearDeclineXp + _g.OfferRarity}", GameAction.B)];

    public override void Draw(PhonePainter p)
    {
        var g = _g;
        var d = g.D;
        var slot = g.OfferSlot;
        if (slot < 0) return;
        var y = p.Section(p.Top, "TERAZ");
        y = Item(p, y, g.Equipped[slot], g.EquippedTrait[slot], false) + 4;
        y = p.Section(y, "NOWY", g.OfferIsBetter ? "lepszy!" : "");
        y = Item(p, y, g.OfferRarity, g.OfferTrait, true) + 6;
        var cmp = Compare(p, (int)(p.Width - 24));
        var c = p.Card(y, 2 + cmp.Count);
        var verdict = g.OfferIsBetter ? "Nowy jest lepszej jakości."
                    : g.OfferRarity == g.Equipped[slot] ? "Ta sama jakość - inna cecha." : "Nowy jest gorszej jakości.";
        p.Stripe(c, 0, g.OfferIsBetter ? Pal.Done : Pal.Todo);
        p.Text(p.TextX(c), p.RowY(c, 0), verdict, g.OfferIsBetter ? Ink.Done : Ink.Dark, TextAlign.Left, c.End.X - 6 - p.TextX(c));
        for (var i = 0; i < cmp.Count; i++)
        {
            p.Divider(c, 1 + i);
            p.Text(p.TextX(c), p.RowY(c, 1 + i), cmp[i], Ink.Brand, TextAlign.Left, c.End.X - 6 - p.TextX(c));
        }
        p.Divider(c, 1 + cmp.Count);
        p.Text(p.TextX(c), p.RowY(c, 1 + cmp.Count), "Cecha: " + d.GearTraits[g.OfferTrait].Name, Ink.Dim, TextAlign.Left, c.End.X - 6 - p.TextX(c));
    }

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
            var line = "Cios: " + DamageHelp.CompareLine(new Message(), now, next).Text;
            list.Add(p.F.Measure(line) <= width ? line : DamageHelp.CompareLine(new Message(), now, next, true).Text);
        }
        if (now.CritChance() != next.CritChance() || now.CritMax != next.CritMax) list.Add(DamageHelp.CompareCrit(new Message(), now, next).Text);
        var old = g.D.Gear[slot * 3 + g.Equipped[slot]];
        var gnew = g.D.Gear[slot * 3 + g.OfferRarity];
        if (old.Stat == LifeLike.Core.Data.GearStat.Def && gnew.Value != old.Value)
        {
            int d0 = g.HeroDefense(), d1 = d0 - old.Value + gnew.Value;
            list.Add($"OBR {d0} -> {d1}: z ciosu -{d0 / 2} -> -{d1 / 2}");
        }
        return list;
    }

    private float Item(PhonePainter p, float y, int rarity, int trait, bool isNew)
    {
        var d = _g.D;
        var card = p.CardH(y, 2 * PhonePainter.RowH + 8);
        if (rarity < 0)
        {
            p.Text(p.TextX(card), p.RowY(card, 0), "brak", Ink.Dim);
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
        p.Text(tx, p.RowY(card, 1), $"{UiText.GearStatName(gd.Stat)} +{gd.Value}, {d.GearTraits[trait].Short}", isNew ? Ink.Brand : Ink.Dim, TextAlign.Left, right - tx);
        return card.End.Y;
    }
}
