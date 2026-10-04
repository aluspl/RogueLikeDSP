using System.Collections.Generic;
using Godot;
using LifeLike.Core;
using LifeLike.Core.Data;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Gfx;

/// <summary>
/// Rozpiska obrażeń broni (#26, jak w BG3) jako lista wierszy – ta sama kolejność i te same teksty co breakdown_rows
/// na GBA (DamageHelp z rdzenia): broń, statystyka, premie (tylko te, które coś dają), cios, kryt z częściami, moc,
/// a w trakcie budowy obrona i unik.
/// </summary>
public static class DamageRows
{
    private static readonly DmgText[] Order =
    [
        DmgText.Weapon, DmgText.Stat, DmgText.StatParts, DmgText.Profile, DmgText.Run, DmgText.Gear, DmgText.Upgrade, DmgText.Pct, DmgText.Boon, DmgText.Total,
        DmgText.Power, DmgText.Crit, DmgText.CritParts, DmgText.CritExtra,
    ];

    /// <summary>Rozpiska z bieżącej budowy (g) albo dla zawodu przed budową (m = premie profilu); źródła premii z profilu.</summary>
    public static DmgBreakdown ForHero(GameData d, Profile p, CoreGame g, int cls, RunMods m)
    {
        var b = g is not null ? g.WeaponBreakdown() : DmgBreakdown.ForClass(d, cls, m);
        if (p is not null) b.SetSources(Meta.ModsParts(d, p));
        return b;
    }

    public static List<DamageRow> Build(GameData d, DmgBreakdown b, CoreGame g)
    {
        var rows = new List<DamageRow>();
        foreach (var k in Order)
        {
            var m = new Message();
            var shown = DamageHelp.Line(d, m, b, k);
            var always = k is DmgText.Weapon or DmgText.Stat or DmgText.Total or DmgText.Crit or DmgText.CritParts;
            if (!shown && !always) continue;
            var ink = k switch
            {
                DmgText.Weapon => Ink.Brand,
                DmgText.Total => Ink.Done,
                DmgText.Crit => Ink.Prog,
                DmgText.Stat or DmgText.Profile or DmgText.Run or DmgText.Gear or DmgText.Pct or DmgText.Upgrade => Ink.Dark,
                DmgText.Boon => Ink.Rare,
                _ => Ink.Dim,
            };
            var stripe = k switch
            {
                DmgText.Weapon => Pal.Brand,
                DmgText.Total => Pal.Done,
                DmgText.Crit => Pal.Prog,
                _ => Colors.Transparent,
            };
            rows.Add(new DamageRow(m.Text, ink, stripe, k, false));
        }
        if (g is null) return rows;
        var def = g.HeroDefense();
        var ds = Loc.F("obr_z_ciosu_problemu", def, def / 2) + (g.Bonus.TakenPct > 0 ? $", -{g.Bonus.TakenPct}%" : "");
        rows.Add(new DamageRow(ds, Ink.Dark, Colors.Transparent, DmgText.Enemy, true));
        var us = Loc.F("unik_szcz_x", g.DodgePct(), g.Luck(), d.DodgePerLuckPct);
        if (g.GearBonus(GearStat.Dodge) > 0) us += Loc.F("buty_2", g.GearBonus(GearStat.Dodge));
        if (g.Bonus.Dodge > 0) us += Loc.F("premie_5", g.Bonus.Dodge);
        rows.Add(new DamageRow(us + ")", Ink.Dim, Colors.Transparent, DmgText.Enemy, true));
        return rows;
    }

    /// <summary>Wiersz „cios”: wszystko do średniego ciosu i moc (strona Obrażenia).</summary>
    public static bool IsHit(DamageRow r) => !r.Defense && r.Kind is not (DmgText.Crit or DmgText.CritParts or DmgText.CritExtra);
}
