using LifeLike.Core.Data;

namespace LifeLike.Core;

/// <summary>
/// Opis statystyki w prostych słowach z prawdziwym wzorem (port core::stat_effect / stat_rule z core.h).
/// </summary>
public static class StatHelp
{
    public const int Kinds = 6;

    private static string[] Names => ["HP", Loc.T("sil"), Loc.T("zre"), "INT", Loc.T("obr"), Loc.T("szcz")];

    public static string Name(StatKind k) => Names[(int)k];

    public static int LuckCritPct(GameData d, int luck) => d.CritBasePct + d.CritPerLuckPct * luck;

    public static int LuckDodgePct(GameData d, int luck) => Math.Min(d.DodgeMaxPct, d.DodgePerLuckPct * Math.Max(0, luck));

    /// <summary>Co daje wartość v statystyki; weaponStat = broń skaluje się z tą statystyką (SIŁ/ZRĘ/INT).</summary>
    public static Message Effect(GameData d, Message m, StatKind k, int v, bool weaponStat)
    {
        switch (k)
        {
            case StatKind.Hp:
                return m.Add(Loc.T("zdrowie_0_koniec"));
            case StatKind.Str:
            case StatKind.Agi:
            case StatKind.Intel:
                if (weaponStat) return m.Add("+").Add(v / 2).Add(Loc.T("obrazen_broni"));
                return m.Add(Loc.T("nie_dla_tej_broni"));
            case StatKind.Def:
                return m.Add("-").Add(v / 2).Add(Loc.T("obrazen_od_problemow"));
            case StatKind.Luck:
                return m.Add(Loc.T("kryt_2")).Add(LuckCritPct(d, v)).Add(Loc.T("unik_2")).Add(LuckDodgePct(d, v)).Add("%");
            default:
                return m;
        }
    }

    /// <summary>Ogólny wzór statystyki (bez wartości) – strona „Jak działają” / Jak grać. Szczęście ma 3 części (part 0-2).</summary>
    public static Message Rule(GameData d, Message m, StatKind k, int part = 0)
    {
        switch (k)
        {
            case StatKind.Hp:
                return m.Add(Loc.T("hp_zdrowie_leczy_kawa"));
            case StatKind.Str:
                return m.Add(Loc.T("sil_1_obr_co_2_pkt_bron_sil"));
            case StatKind.Agi:
                return m.Add(Loc.T("zre_1_obr_co_2_pkt_bron_zre"));
            case StatKind.Intel:
                return m.Add(Loc.T("int_1_obr_co_2_pkt_bron_int"));
            case StatKind.Def:
                return m.Add(Loc.T("obr_1_obrazen_co_2_pkt"));
            case StatKind.Luck:
                if (part == 1) return m.Add(Loc.T("unik_3")).Add(d.DodgePerLuckPct).Add(Loc.T("pkt_maks")).Add(d.DodgeMaxPct).Add("%)");
                if (part == 2) return m.Add(Loc.T("lupy")).Add(d.DropPerLuckPct).Add(Loc.T("szansy_pkt"));
                return m.Add(Loc.T("szcz_kryt")).Add(d.CritBasePct).Add("% +").Add(d.CritPerLuckPct).Add(Loc.T("pkt"));
            default:
                return m;
        }
    }
}
