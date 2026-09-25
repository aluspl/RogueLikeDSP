using Godot;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game;

/// <summary>
/// Paczka sprzętu przy zajętym slocie (gear_offer_dialog na GBA): obecny i nowy przedmiot z jakością (pastylka),
/// premią i cechą; A zakładam, B zostawiam (+doświadczenie).
/// </summary>
public sealed class OfferPage : PhonePage
{
    private readonly CoreGame _g;

    public OfferPage(CoreGame g) => _g = g;

    public override string Title => "Paczka sprzętu";
    public override string Sub => _g.OfferSlot >= 0 ? _g.D.GearSlots[_g.OfferSlot] : "";
    public override string Hint => $"Spacja: zakładam  Z: zostawiam (+{_g.D.GearDeclineXp + _g.OfferRarity})";

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
        var c = p.Card(y, 2);
        var verdict = g.OfferIsBetter ? "Nowy jest lepszej jakości."
                    : g.OfferRarity == g.Equipped[slot] ? "Ta sama jakość - inna cecha." : "Nowy jest gorszej jakości.";
        p.Stripe(c, 0, g.OfferIsBetter ? Pal.Done : Pal.Todo);
        p.Text(p.TextX(c), p.RowY(c, 0), verdict, g.OfferIsBetter ? Ink.Done : Ink.Dark, TextAlign.Left, c.End.X - 6 - p.TextX(c));
        p.Divider(c, 1);
        p.Text(p.TextX(c), p.RowY(c, 1), "Cecha: " + d.GearTraits[g.OfferTrait].Name, Ink.Dim, TextAlign.Left, c.End.X - 6 - p.TextX(c));
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
