using LifeLike.Core.Data;

namespace LifeLike.Core;

/// <summary>
/// Rozpiska obrażeń broni (#26): arytmetyka i wspólne teksty wierszy (port pct_floor, dmg_line, compare_line, versus_line…
/// z core.h). Teksty krótkie – bufor Message ma 48 bajtów jak na GBA.
/// </summary>
public static class DamageHelp
{
    private static string[] SourceNames => [Loc.T("szkolenia"), Loc.T("respekt"), Loc.T("odznaki"), Loc.T("pamiatka")];

    public static int PctFloor(int v, int pct) => pct <= 0 || v <= 0 ? 0 : v * pct / 100;

    public static int PctCeil(int v, int pct) => pct <= 0 || v <= 0 ? 0 : (v * pct + 99) / 100;

    /// <summary>Źródło premii profilu: 0 Szkolenia, 1 Respekt, 2 odznaki, 3 pamiątka.</summary>
    public static string SourceName(int s) => SourceNames[s];

    public static string StatName(Stat s) => s == Stat.Str ? Loc.T("sil") : (s == Stat.Agi ? Loc.T("zre") : "INT");

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
            case GearStat.Def: return m.Add("+").Add(gd.Value).Add(Loc.T("obr_2"));
            case GearStat.Dmg: return m.Add("+").Add(gd.Value).Add(Loc.T("obrazen_2"));
            case GearStat.Dodge: return m.Add(Loc.T("unik_3")).Add(gd.Value).Add("%");
            case GearStat.Thermos: return m.Add(Loc.T("termos_2")).Add(gd.Value);
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
                m.Add(" ").Add(b.WMin).Add("-").Add(b.WMax).Add(Loc.T("zasieg")).Add(b.Range);
                if (b.Range < b.RangeBase) m.Add(Loc.T("wiatr"));
                return true;
            case DmgText.Stat:
                m.Add(StatName(b.Scales)).Add(" ").Add(b.StatValue).Add(": +").Add(b.StatDmg).Add(Loc.T("n1_co_2_pkt"));
                return true;
            case DmgText.StatParts:
                m.Add(StatName(b.Scales)).Add(" ").Add(b.StatValue).Add(Loc.T("zawod")).Add(b.StatClass);
                if (b.StatCraft != 0) m.Add(Loc.T("warsztaty")).Add(b.StatCraft);
                if (b.StatTrait != 0) m.Add(Loc.T("sprzet")).Add(b.StatTrait);
                return b.StatCraft != 0 || b.StatTrait != 0;
            case DmgText.Profile:
            {
                if (b.FlatMods == 0)
                {
                    m.Add(Loc.T("premie_stale_brak"));
                    return false;
                }
                m.Add(Loc.T("premie_stale")).Add(b.FlatMods);
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
                    m.Add(Loc.T("z_budowy_brak"));
                    return false;
                }
                var sum = b.FlatLevel + b.FlatFound + b.FlatEvent;
                m.Add(Loc.T("z_budowy")).Add(sum >= 0 ? "+" : "").Add(sum).Add(":");
                var first = true;
                if (b.FlatLevel != 0)
                {
                    m.Add(Loc.T("poziom")).Add(b.FlatLevel);
                    first = false;
                }
                if (b.FlatFound != 0)
                {
                    m.Add(first ? "" : ",").Add(Loc.T("projekt")).Add(b.FlatFound > 0 ? "+" : "").Add(b.FlatFound);
                    first = false;
                }
                if (b.FlatEvent != 0) m.Add(first ? "" : ",").Add(Loc.T("wydarzenie")).Add(b.FlatEvent > 0 ? "+" : "").Add(b.FlatEvent);
                return true;
            }
            case DmgText.Gear:
                if (b.GearItem < 0 || b.FlatGear == 0)
                {
                    m.Add(Loc.T("sprzet_bez_premii"));
                    return false;
                }
                m.Add(d.Gear[b.GearItem].Name).Add(": +").Add(b.FlatGear);
                return true;
            case DmgText.Pct:
            {
                if (b.Pct <= 0)
                {
                    m.Add(Loc.T("procent_brak"));
                    return false;
                }
                m.Add("+").Add(b.Pct).Add("%");
                if (!b.Split)
                {
                    m.Add(Loc.T("szkolenia_respekt"));
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
                    m.Add(Loc.T("obr_problemu_1_co_2_pkt"));
                    return false;
                }
                m.Add(Loc.T("obr_problemu")).Add(b.EnemyDef);
                if (b.EnemyElite != 0) m.Add("+").Add(b.EnemyElite).Add(Loc.T("elita"));
                if (b.Pierce != 0) m.Add(" -").Add(b.Pierce).Add(Loc.T("przebicie"));
                m.Add(": -").Add(b.DefCut);
                return b.DefCut > 0;
            case DmgText.Total:
                AddRange(m.Add(Loc.T("cios")), b.Min, b.Max).Add(Loc.T("srednio"));
                AddTenths(m, b.Avg10);
                return true;
            case DmgText.Crit:
                m.Add(Loc.T("kryt_x")).Add(b.CritMult).Add(": ");
                AddRange(m, b.CritMin, b.CritMax).Add(Loc.T("szansa")).Add(b.CritChance()).Add("%");
                return true;
            case DmgText.CritParts:
                m.Add(Loc.T("kryt_3")).Add(b.CritChance()).Add("% = ").Add(b.CritBase).Add(Loc.T("szcz_2")).Add(b.Luck).Add(" x ")
                    .Add(d.CritPerLuckPct).Add("%");
                return true;
            case DmgText.CritExtra:
            {
                if (b.CritTrait == 0 && b.CritBonus == 0 && b.CritUpg == 0 && b.CritWeapon == 0)
                {
                    m.Add(Loc.T("kryt_bez_premii"));
                    return false;
                }
                m.Add("+");
                var first = true;
                if (b.CritWeapon != 0)
                {
                    m.Add(Loc.T("bron")).Add(b.CritWeapon).Add("%");
                    first = false;
                }
                if (b.CritTrait != 0)
                {
                    m.Add(Loc.T("cecha")).Add(b.CritTrait).Add("%");
                    first = false;
                }
                if (b.CritUpg != 0)
                {
                    m.Add(first ? " " : ", ").Add(Loc.T("ostrze")).Add(b.CritUpg).Add("%");
                    first = false;
                }
                if (!b.Split)
                {
                    if (b.CritBonus != 0) m.Add(first ? " " : ", ").Add(Loc.T("premie_2")).Add(b.CritBonus).Add("%");
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
                    m.Add(Loc.T("moc_bez_premii_do_ciosu"));
                    return false;
                }
                m.Add(Loc.T("moc_2")).Add(RankNumeral(b.PowerRank)).Add("): +").Add(b.Power).Add(Loc.T("do_ciosu"));
                return true;
            case DmgText.Boon: // v0.21.50: premie wybrane po etapach (#27)
            {
                if (b.FlatBoon == 0 && b.PctBoon == 0 && b.CritBoon == 0)
                {
                    m.Add(Loc.T("premie_etapow_brak"));
                    return false;
                }
                m.Add(Loc.T("premie_etapow"));
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
                if (b.CritBoon != 0) m.Add(first ? Loc.T("kryt_4") : Loc.T("kryt_5")).Add(b.CritBoon).Add("%");
                return true;
            }
            case DmgText.Upgrade: // v0.21.50 cz. 3: ulepszenie narzędzia (#31)
                if (b.UpgLevel == 0)
                {
                    m.Add(Loc.T("ulepszenie_brak"));
                    return false;
                }
                m.Add(Loc.T(Loc.T("ulepszenie_2"))).Add(b.UpgLevel).Add(": +").Add(b.FlatUpgrade).Add(Loc.T("obr_3"));
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
        AddRange(m.Add(Loc.T("teraz")), now.Min, now.Max).Add(" -> ");
        AddRange(m, next.Min, next.Max).Add(shortAvg ? Loc.T("sr") : Loc.T("srednio_2"));
        return AddTenths(m, next.Avg10 - now.Avg10, true).Add(")");
    }

    /// <summary>„kryt 8-14 (11%) -&gt; 10-18 (16%)”.</summary>
    public static Message CompareCrit(Message m, DmgBreakdown now, DmgBreakdown next)
    {
        AddRange(m.Add(Loc.T("kryt_2")), now.CritMin, now.CritMax).Add(" (").Add(now.CritChance()).Add("%) -> ");
        return AddRange(m, next.CritMin, next.CritMax).Add(" (").Add(next.CritChance()).Add("%)");
    }

    /// <summary>„Zadasz 2-5 (kryt 4-10)” – część karty problemu.</summary>
    public static Message VersusHero(Message m, DmgBreakdown b)
    {
        AddRange(m.Add(Loc.T("zadasz")), b.Min, b.Max).Add(Loc.T("kryt_6"));
        return AddRange(m, b.CritMin, b.CritMax).Add(")");
    }

    /// <summary>„on Tobie 1-3” – część karty problemu.</summary>
    public static Message VersusEnemy(Message m, HitRange h) => AddRange(m.Add(Loc.T("on_tobie")), h.Min, h.Max);

    /// <summary>Karta problemu: „Zadasz 2-5 (kryt 4-10), on Tobie 1-3”.</summary>
    public static Message VersusLine(Message m, DmgBreakdown b, HitRange h)
    {
        VersusHero(m, b).Add(", ");
        return VersusEnemy(m, h);
    }
}
