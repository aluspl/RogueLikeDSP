using LifeLike.Core;
using LifeLike.Core.Data;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Phone.Tabs;

/// <summary>
/// Sprzęt (tab_gear na GBA): narzędzie z zakresem ciosu i krytem (rozpiska obrażeń #26: I / dotknięcie narzędzia),
/// kask / rękawice / kamizelka / ... z jakością (kolor paska), skutkiem (np. „+2 OBR”) i cechą (pastylka), premie
/// sprzętu oraz szczęście: kryt, unik, wzrok. A / przycisk: Brygada.
/// </summary>
public sealed class GearTab : PhonePage
{
    private const int WeaponRow = 100;
    private readonly CoreGame _g;
    private readonly System.Action _brigade;
    private readonly System.Action _breakdown;
    private readonly System.Action _boons;

    public GearTab(CoreGame g, System.Action brigade = null, System.Action breakdown = null, System.Action boons = null)
    {
        _g = g;
        _brigade = brigade;
        _breakdown = breakdown;
        _boons = boons;
    }

    public override string Title => Loc.T("sprzet_5");
    public override string Sub => _brigade is null ? Loc.T("na_budowie") : Loc.T("spacja_brygada");
    public override PageAction[] Actions =>
        _brigade is null ? [] : _breakdown is null ? [new(Loc.T("brygada_3"), GameAction.A)]
        : _boons is null ? [new(Loc.T("obrazenia"), GameAction.Info), new(Loc.T("brygada_3"), GameAction.A)]
        : [new(Loc.T("obrazenia"), GameAction.Info), new(Loc.T("premie_3"), GameAction.R), new(Loc.T("brygada_3"), GameAction.A)];

    public override bool Input(InputCmd e)
    {
        if (_breakdown is not null && e.Is(GameAction.Info))
        {
            _breakdown();
            return true;
        }
        if (_boons is not null && e.Is(GameAction.R)) // premie po etapach i synergie
        {
            _boons();
            return true;
        }
        if (_brigade is null || !e.Is(GameAction.A)) return false;
        _brigade();
        return true;
    }

    /// <summary>Dotknięcie / klik narzędzia: rozpiska obrażeń.</summary>
    public override bool TapRow(int index)
    {
        if (index != WeaponRow || _breakdown is null) return false;
        _breakdown();
        return true;
    }

    public override void Draw(PhonePainter p)
    {
        var g = _g;
        var d = g.D;
        var y = p.Section(p.Top, Loc.T("narzedzie_4"), _breakdown is null ? "" : ButtonNames.Pick(Loc.T("i_rozpiska"), Loc.T("dotknij_rozpiska")));
        var c0 = p.Card(y, 2);
        var tx = p.TextX(c0);
        var right = c0.End.X - 6;
        var w = g.Weapon;
        var b = g.WeaponBreakdown();   // zakres ciosu jak w walce (rozpiska #26)
        p.Stripe(c0, 0, Pal.Brand);
        var pw = p.Pill(right, p.RowY(c0, 0), $"z{g.WeaponRange()} {UiText.StatShort(w.ScalesWith)}", PillKind.Group);
        p.Text(tx, p.RowY(c0, 0), $"{g.WeaponTitle()} {b.Min}-{b.Max}", Ink.Dark, TextAlign.Left, right - pw - 4 - tx);
        p.Divider(c0, 1);
        var avg = DamageHelp.AddTenths(new Message(), b.Avg10).Text;
        p.Text(tx, p.RowY(c0, 1), Loc.F("kryt_srednio", b.CritMin, b.CritMax, b.CritChance(), avg), Ink.Prog, TextAlign.Left, right - tx);
        p.HitRow(c0, 0, WeaponRow);
        p.HitRow(c0, 1, WeaponRow);

        // sloty: kask, rękawice, kamizelka + buty i pas z nagród za odbiór (jeśli odebrane albo założone)
        var slots = new System.Collections.Generic.List<int>();
        for (var i = 0; i < d.GearSlotsCount; i++)
        {
            if (((g.Bonus.GearSlots >> i) & 1) != 0 || g.Equipped[i] >= 0) slots.Add(i);
        }
        y = p.Section(c0.End.Y + 4, Loc.T("sprzet_8"), $"{CountEquipped()}/{slots.Count}");
        var c1 = p.Card(y, slots.Count);
        for (var k = 0; k < slots.Count; k++)
        {
            var i = slots[k];
            var r = p.RowY(c1, k);
            if (k > 0) p.Divider(c1, k);
            var rar = g.Equipped[i];
            p.Stripe(c1, k, rar < 0 ? Pal.Todo : rar == 2 ? Pal.Prog : rar == 1 ? Pal.Brand : Pal.Done);
            if (rar < 0)
            {
                p.Text(tx, r, Loc.F("brak_6", d.GearSlots[i]), Ink.Dim);
                continue;
            }
            var gd = d.Gear[i * 3 + rar];
            pw = p.Pill(right, r, d.GearTraits[g.EquippedTrait[i]].Short, rar == 2 ? PillKind.Prog : rar == 1 ? PillKind.Group : PillKind.Gray);
            // co daje (np. „+2 OBR”, „+3 obrażeń”); nazwa przedmiotu, a gdy się nie mieści - nazwa slotu
            var eff = DamageHelp.GearLabel(gd);
            var room = right - pw - 8 - tx - p.F.Measure(eff) - 6;
            var name = p.F.Measure(gd.Name) <= room ? gd.Name : p.F.Fit(d.GearSlots[i], (int)room);
            var nx = tx + p.Text(tx, r, name, Ink.Dark) + 6;
            p.Text(nx, r, eff, Ink.Brand);
        }

        var boonsRight = _boons is null ? "" : ButtonNames.Pick(Loc.F("r_premie", g.BoonsOwned()), Loc.F("premie_7", g.BoonsOwned()));
        y = p.Section(c1.End.Y + 4, Loc.T("premie_i_materialy"), boonsRight);
        var c2 = p.Card(y, d.Materials.Length > 0 ? 4 : 3);
        p.Text(tx, p.RowY(c2, 0), Loc.F("obrona_obraz_hp", g.GearBonus(GearStat.Def), g.GearBonus(GearStat.Dmg), g.GearBonus(GearStat.Hp)), Ink.Dim, TextAlign.Left, right - tx);
        p.Divider(c2, 1);
        p.Text(tx, p.RowY(c2, 1), Loc.F("kryt_unik_wzrok", g.CritPct(), g.DodgePct(), g.SightRadius()), Ink.Dim, TextAlign.Left, right - tx);
        p.Divider(c2, 2);
        p.Text(tx, p.RowY(c2, 2), Loc.F("szczescie_termos", g.Luck(), g.Thermos, g.ThermosCap()), Ink.Dim, TextAlign.Left, right - tx);
        if (d.Materials.Length == 0) return;
        p.Divider(c2, 3); // materiały: Hurtownia i naprawy (Brygada)
        var mx = tx;
        for (var m = 0; m < d.Materials.Length; m++)
        {
            MaterialIcon.Draw(p.C, m, new Godot.Vector2(mx, p.RowY(c2, 3) + (PhonePainter.RowH - MaterialIcon.Size) / 2));
            mx += MaterialIcon.Size + 3;
            mx += p.Text(mx, p.RowY(c2, 3), $"{d.Materials[m].Name} {g.Mats[m]}", g.Mats[m] > 0 ? Ink.Dark : Ink.Dim) + 8;
        }
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
