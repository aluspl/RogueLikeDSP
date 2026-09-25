using LifeLike.Core.Data;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game;

/// <summary>
/// Sprzęt (tab_gear na GBA): narzędzie z obrażeniami i zasięgiem, kask / rękawice / kamizelka / ... z jakością
/// (kolor paska) i cechą (pastylka), premie sprzętu oraz szczęście: kryt, unik, wzrok.
/// </summary>
public sealed class GearTab : PhonePage
{
    private readonly CoreGame _g;

    public GearTab(CoreGame g) => _g = g;

    public override string Title => "Sprzęt";
    public override string Sub => "Na budowie";

    public override void Draw(PhonePainter p)
    {
        var g = _g;
        var d = g.D;
        var y = p.Section(p.Top, "NARZĘDZIE");
        var c0 = p.Card(y, 1);
        var tx = p.TextX(c0);
        var right = c0.End.X - 6;
        var w = g.Weapon;
        p.Stripe(c0, 0, Pal.Brand);
        var pw = p.Pill(right, p.RowY(c0, 0), $"z{w.Range} {UiText.StatShort(w.ScalesWith)}", PillKind.Group);
        p.Text(tx, p.RowY(c0, 0), $"{w.Name} {w.MinDamage}-{w.MaxDamage} +{g.DmgBonus}", Ink.Dark, TextAlign.Left, right - pw - 4 - tx);

        y = p.Section(c0.End.Y + 4, "SPRZĘT", $"{CountEquipped()}/{d.GearSlotsCount}");
        var c1 = p.Card(y, d.GearSlotsCount);
        for (var i = 0; i < d.GearSlotsCount; i++)
        {
            var r = p.RowY(c1, i);
            if (i > 0) p.Divider(c1, i);
            var rar = g.Equipped[i];
            p.Stripe(c1, i, rar < 0 ? Pal.Todo : rar == 2 ? Pal.Prog : rar == 1 ? Pal.Brand : Pal.Done);
            if (rar < 0)
            {
                p.Text(tx, r, $"{d.GearSlots[i]}: brak", Ink.Dim);
                continue;
            }
            var gd = d.Gear[i * 3 + rar];
            pw = p.Pill(right, r, d.GearTraits[g.EquippedTrait[i]].Short, rar == 2 ? PillKind.Prog : rar == 1 ? PillKind.Group : PillKind.Gray);
            p.Text(tx, r, gd.Name, Ink.Dark, TextAlign.Left, right - pw - 4 - tx);
        }

        y = p.Section(c1.End.Y + 4, "PREMIE");
        var c2 = p.Card(y, 3);
        p.Text(tx, p.RowY(c2, 0), $"Obrona +{g.GearBonus(GearStat.Def)}  Obraż. +{g.GearBonus(GearStat.Dmg)}  HP +{g.GearBonus(GearStat.Hp)}", Ink.Dim, TextAlign.Left, right - tx);
        p.Divider(c2, 1);
        p.Text(tx, p.RowY(c2, 1), $"Kryt {g.CritPct()}%  Unik {g.DodgePct()}%  Wzrok {g.SightRadius()}", Ink.Dim, TextAlign.Left, right - tx);
        p.Divider(c2, 2);
        p.Text(tx, p.RowY(c2, 2), $"Szczęście {g.Luck()}  Termos {g.Thermos}/{g.ThermosCap()}", Ink.Dim, TextAlign.Left, right - tx);
    }

    private int CountEquipped()
    {
        var n = 0;
        for (var i = 0; i < _g.D.GearSlotsCount; i++)
        {
            if (_g.Equipped[i] >= 0) n++;
        }
        return n;
    }
}
