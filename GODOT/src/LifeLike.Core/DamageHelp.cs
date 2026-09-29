using LifeLike.Core.Data;

namespace LifeLike.Core;

/// <summary>
/// Rozpiska obrażeń broni (#26): arytmetyka i wspólne teksty wierszy (port pct_floor, dmg_line, compare_line, versus_line…
/// z core.h). Teksty krótkie – bufor Message ma 48 bajtów jak na GBA.
/// </summary>
public static class DamageHelp
{
    private static readonly string[] SourceNames = ["Szkolenia", "Respekt", "odznaki", "pamiątka"];

    public static int PctFloor(int v, int pct) => pct <= 0 || v <= 0 ? 0 : v * pct / 100;

    public static int PctCeil(int v, int pct) => pct <= 0 || v <= 0 ? 0 : (v * pct + 99) / 100;

    /// <summary>Źródło premii profilu: 0 Szkolenia, 1 Respekt, 2 odznaki, 3 pamiątka.</summary>
    public static string SourceName(int s) => SourceNames[s];

    public static string StatName(Stat s) => s == Stat.Str ? "SIŁ" : (s == Stat.Agi ? "ZRĘ" : "INT");

    /// <summary>Premia do obrażeń z awansów do poziomu level (DmgLevelsMask).</summary>
    public static int LevelDmg(GameData d, int level)
    {
        var n = 0;
        for (var l = 2; l <= level; ++l) n += (d.DmgLevelsMask >> l) & 1;
        return n;
    }

    /// <summary>Cios problemu w bohatera (EnemyStrike bez losowania): bonus = premia etapu/trudności + wzrost / 2.</summary>
    public static HitRange EnemyHitRange(int dmin, int dmax, int bonus, int heroDef, int takenPct)
    {
        int lo = Math.Max(1, dmin + bonus - heroDef / 2), hi = Math.Max(1, dmax + bonus - heroDef / 2);
        return new HitRange(Math.Max(1, lo - PctCeil(lo, takenPct)), Math.Max(1, hi - PctFloor(hi, takenPct)));
    }

    public static Message AddRange(Message m, int lo, int hi)
    {
        m.Add(lo);
        if (hi != lo) m.Add("-").Add(hi);
        return m;
    }

    /// <summary>Liczba x10 jako „7” albo „7,5” (ze znakiem, gdy sign).</summary>
    public static Message AddTenths(Message m, int v10, bool sign = false)
    {
        if (v10 < 0)
        {
            m.Add("-");
            v10 = -v10;
        }
        else if (sign)
        {
            m.Add("+");
        }
        m.Add(v10 / 10);
        if (v10 % 10 != 0) m.Add(",").Add(v10 % 10);
        return m;
    }

    public static string RankNumeral(int r) => r >= 3 ? "III" : (r == 2 ? "II" : "I");

    /// <summary>Skutek przedmiotu sprzętu, np. „+2 obrażeń”, „+1 OBR”, „+8 HP”, „unik +5%”, „termos +1”.</summary>
    public static Message GearLabel(Message m, GearDef gd)
    {
        switch (gd.Stat)
        {
            case GearStat.Def: return m.Add("+").Add(gd.Value).Add(" OBR");
            case GearStat.Dmg: return m.Add("+").Add(gd.Value).Add(" obrażeń");
            case GearStat.Dodge: return m.Add("unik +").Add(gd.Value).Add("%");
            case GearStat.Thermos: return m.Add("termos +").Add(gd.Value);
            default: return m.Add("+").Add(gd.Value).Add(" HP");
        }
    }

    public static string GearLabel(GearDef gd) => GearLabel(new Message(), gd).Text;

    /// <summary>Wiersz rozpiski k; false = ten składnik nic nie daje (można go pominąć), tekst i tak jest.</summary>
    public static bool Line(GameData d, Message m, DmgBreakdown b, DmgText k)
    {
        switch (k)
        {
            case DmgText.Weapon:
                m.Add(d.Weapons[b.Weapon].Name);
                if (b.UpgLevel != 0) m.Add("+").Add(b.UpgLevel); // ulepszone narzędzie: „Kielnia+2”
                m.Add(" ").Add(b.WMin).Add("-").Add(b.WMax).Add(", zasięg ").Add(b.Range);
                if (b.Range < b.RangeBase) m.Add(" (wiatr)");
                return true;
            case DmgText.Stat:
                m.Add(StatName(b.Scales)).Add(" ").Add(b.StatValue).Add(": +").Add(b.StatDmg).Add(" (+1 co 2 pkt)");
                return true;
            case DmgText.StatParts:
                m.Add(StatName(b.Scales)).Add(" ").Add(b.StatValue).Add(" = zawód ").Add(b.StatClass);
                if (b.StatCraft != 0) m.Add(" + Warsztaty ").Add(b.StatCraft);
                if (b.StatTrait != 0) m.Add(" + sprzęt ").Add(b.StatTrait);
                return b.StatCraft != 0 || b.StatTrait != 0;
            case DmgText.Profile:
            {
                if (b.FlatMods == 0)
                {
                    m.Add("Premie stałe: brak");
                    return false;
                }
                m.Add("Premie stałe +").Add(b.FlatMods);
                if (!b.Split) return true;
                var first = true;
                for (var s = 0; s < DmgBreakdown.Sources; ++s)
                {
                    if (b.SrcDmg[s] == 0) continue;
                    m.Add(first ? ": " : ", ").Add(SourceName(s)).Add(" +").Add(b.SrcDmg[s]);
                    first = false;
                }
                return true;
            }
            case DmgText.Run:
            {
                if (b.FlatLevel == 0 && b.FlatFound == 0 && b.FlatEvent == 0)
                {
                    m.Add("Z budowy: brak");
                    return false;
                }
                var sum = b.FlatLevel + b.FlatFound + b.FlatEvent;
                m.Add("Z budowy ").Add(sum >= 0 ? "+" : "").Add(sum).Add(":");
                var first = true;
                if (b.FlatLevel != 0)
                {
                    m.Add(" poziom +").Add(b.FlatLevel);
                    first = false;
                }
                if (b.FlatFound != 0)
                {
                    m.Add(first ? "" : ",").Add(" projekt ").Add(b.FlatFound > 0 ? "+" : "").Add(b.FlatFound);
                    first = false;
                }
                if (b.FlatEvent != 0) m.Add(first ? "" : ",").Add(" wydarzenie ").Add(b.FlatEvent > 0 ? "+" : "").Add(b.FlatEvent);
                return true;
            }
            case DmgText.Gear:
                if (b.GearItem < 0 || b.FlatGear == 0)
                {
                    m.Add("Sprzęt: bez premii");
                    return false;
                }
                m.Add(d.Gear[b.GearItem].Name).Add(": +").Add(b.FlatGear);
                return true;
            case DmgText.Pct:
            {
                if (b.Pct <= 0)
                {
                    m.Add("Procent: brak");
                    return false;
                }
                m.Add("+").Add(b.Pct).Add("%");
                if (!b.Split)
                {
                    m.Add(" (Szkolenia, Respekt)");
                    return true;
                }
                var first = true;
                for (var s = 0; s < DmgBreakdown.Sources; ++s)
                {
                    if (b.SrcPct[s] == 0) continue;
                    m.Add(first ? ": " : ", ").Add(SourceName(s)).Add(" +").Add(b.SrcPct[s]).Add("%");
                    first = false;
                }
                return true;
            }
            case DmgText.Enemy:
                if (!b.VsEnemy)
                {
                    m.Add("OBR problemu: -1 co 2 pkt");
                    return false;
                }
                m.Add("OBR problemu ").Add(b.EnemyDef);
                if (b.EnemyElite != 0) m.Add("+").Add(b.EnemyElite).Add(" (elita)");
                if (b.Pierce != 0) m.Add(" -").Add(b.Pierce).Add(" (przebicie)");
                m.Add(": -").Add(b.DefCut);
                return b.DefCut > 0;
            case DmgText.Total:
                AddRange(m.Add("Cios "), b.Min, b.Max).Add(", średnio ");
                AddTenths(m, b.Avg10);
                return true;
            case DmgText.Crit:
                m.Add("Kryt x").Add(b.CritMult).Add(": ");
                AddRange(m, b.CritMin, b.CritMax).Add(", szansa ").Add(b.CritChance()).Add("%");
                return true;
            case DmgText.CritParts:
                m.Add("Kryt ").Add(b.CritChance()).Add("% = ").Add(b.CritBase).Add("% + SZCZ ").Add(b.Luck).Add(" x ")
                    .Add(d.CritPerLuckPct).Add("%");
                return true;
            case DmgText.CritExtra:
            {
                if (b.CritTrait == 0 && b.CritBonus == 0 && b.CritUpg == 0 && b.CritWeapon == 0)
                {
                    m.Add("Kryt: bez premii");
                    return false;
                }
                m.Add("+");
                var first = true;
                if (b.CritWeapon != 0)
                {
                    m.Add(" broń ").Add(b.CritWeapon).Add("%");
                    first = false;
                }
                if (b.CritTrait != 0)
                {
                    m.Add(" cecha ").Add(b.CritTrait).Add("%");
                    first = false;
                }
                if (b.CritUpg != 0)
                {
                    m.Add(first ? " " : ", ").Add("ostrze ").Add(b.CritUpg).Add("%");
                    first = false;
                }
                if (!b.Split)
                {
                    if (b.CritBonus != 0) m.Add(first ? " " : ", ").Add("premie ").Add(b.CritBonus).Add("%");
                    return true;
                }
                for (var s = 0; s < DmgBreakdown.Sources; ++s)
                {
                    if (b.SrcCrit[s] == 0) continue;
                    m.Add(first ? " " : ", ").Add(SourceName(s)).Add(" ").Add(b.SrcCrit[s]).Add("%");
                    first = false;
                }
                return true;
            }
            case DmgText.Power:
                if (b.Power == 0)
                {
                    m.Add("Moc: bez premii do ciosu");
                    return false;
                }
                m.Add("Moc (").Add(RankNumeral(b.PowerRank)).Add("): +").Add(b.Power).Add(" do ciosu");
                return true;
            case DmgText.Boon: // v0.21.50: premie wybrane po etapach (#27)
            {
                if (b.FlatBoon == 0 && b.PctBoon == 0 && b.CritBoon == 0)
                {
                    m.Add("Premie etapów: brak");
                    return false;
                }
                m.Add("Premie etapów:");
                var first = true;
                if (b.FlatBoon != 0)
                {
                    m.Add(" +").Add(b.FlatBoon);
                    first = false;
                }
                if (b.PctBoon != 0)
                {
                    m.Add(first ? " +" : ", +").Add(b.PctBoon).Add("%");
                    first = false;
                }
                if (b.CritBoon != 0) m.Add(first ? " kryt +" : ", kryt +").Add(b.CritBoon).Add("%");
                return true;
            }
            case DmgText.Upgrade: // v0.21.50 cz. 3: ulepszenie narzędzia (#31)
                if (b.UpgLevel == 0)
                {
                    m.Add("Ulepszenie: brak");
                    return false;
                }
                m.Add("Ulepszenie +").Add(b.UpgLevel).Add(": +").Add(b.FlatUpgrade).Add(" obr.");
                if (b.UpgTrait >= 0) m.Add(", ").Add(d.ToolTraits[b.UpgTrait].Name).Add(" ").Add(d.ToolTraits[b.UpgTrait].Short);
                return true;
            default:
                return false;
        }
    }

    /// <summary>Tekst wiersza k (bez informacji, czy składnik coś daje).</summary>
    public static string Text(GameData d, DmgBreakdown b, DmgText k)
    {
        var m = new Message();
        Line(d, m, b, k);
        return m.Text;
    }

    /// <summary>Porównanie przy zmianie broni / sprzętu: „teraz 4-7 -&gt; 5-9 (średnio +1,5)” (shortAvg: „śr.”).</summary>
    public static Message CompareLine(Message m, DmgBreakdown now, DmgBreakdown next, bool shortAvg = false)
    {
        AddRange(m.Add("teraz "), now.Min, now.Max).Add(" -> ");
        AddRange(m, next.Min, next.Max).Add(shortAvg ? " (śr. " : " (średnio ");
        return AddTenths(m, next.Avg10 - now.Avg10, true).Add(")");
    }

    /// <summary>„kryt 8-14 (11%) -&gt; 10-18 (16%)”.</summary>
    public static Message CompareCrit(Message m, DmgBreakdown now, DmgBreakdown next)
    {
        AddRange(m.Add("kryt "), now.CritMin, now.CritMax).Add(" (").Add(now.CritChance()).Add("%) -> ");
        return AddRange(m, next.CritMin, next.CritMax).Add(" (").Add(next.CritChance()).Add("%)");
    }

    /// <summary>„Zadasz 2-5 (kryt 4-10)” – część karty problemu.</summary>
    public static Message VersusHero(Message m, DmgBreakdown b)
    {
        AddRange(m.Add("Zadasz "), b.Min, b.Max).Add(" (kryt ");
        return AddRange(m, b.CritMin, b.CritMax).Add(")");
    }

    /// <summary>„on Tobie 1-3” – część karty problemu.</summary>
    public static Message VersusEnemy(Message m, HitRange h) => AddRange(m.Add("on Tobie "), h.Min, h.Max);

    /// <summary>Karta problemu: „Zadasz 2-5 (kryt 4-10), on Tobie 1-3”.</summary>
    public static Message VersusLine(Message m, DmgBreakdown b, HitRange h)
    {
        VersusHero(m, b).Add(", ");
        return VersusEnemy(m, h);
    }
}
