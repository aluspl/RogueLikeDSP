using LifeLike.Core.Data;

namespace LifeLike.Core;

/// <summary>
/// Opis statystyki w prostych słowach z prawdziwym wzorem (port core::stat_effect / stat_rule z core.h).
/// </summary>
public static class StatHelp
{
    public const int Kinds = 6;

    private static readonly string[] Names = ["HP", "SIŁ", "ZRĘ", "INT", "OBR", "SZCZ"];

    public static string Name(StatKind k) => Names[(int)k];

    public static int LuckCritPct(GameData d, int luck) => d.CritBasePct + d.CritPerLuckPct * luck;

    public static int LuckDodgePct(GameData d, int luck) => Math.Min(d.DodgeMaxPct, d.DodgePerLuckPct * Math.Max(0, luck));

    /// <summary>Co daje wartość v statystyki; weaponStat = broń skaluje się z tą statystyką (SIŁ/ZRĘ/INT).</summary>
    public static Message Effect(GameData d, Message m, StatKind k, int v, bool weaponStat)
    {
        switch (k)
        {
            case StatKind.Hp:
                return m.Add("zdrowie (0 = koniec)");
            case StatKind.Str:
            case StatKind.Agi:
            case StatKind.Intel:
                if (weaponStat) return m.Add("+").Add(v / 2).Add(" obrażeń broni");
                return m.Add("nie dla tej broni");
            case StatKind.Def:
                return m.Add("-").Add(v / 2).Add(" obrażeń od problemów");
            case StatKind.Luck:
                return m.Add("kryt ").Add(LuckCritPct(d, v)).Add("%, unik ").Add(LuckDodgePct(d, v)).Add("%");
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
                return m.Add("HP: zdrowie, leczy kawa");
            case StatKind.Str:
                return m.Add("SIŁ: +1 obr. co 2 pkt (broń SIŁ)");
            case StatKind.Agi:
                return m.Add("ZRĘ: +1 obr. co 2 pkt (broń ZRĘ)");
            case StatKind.Intel:
                return m.Add("INT: +1 obr. co 2 pkt (broń INT)");
            case StatKind.Def:
                return m.Add("OBR: -1 obrażeń co 2 pkt");
            case StatKind.Luck:
                if (part == 1) return m.Add("unik +").Add(d.DodgePerLuckPct).Add("%/pkt (maks. ").Add(d.DodgeMaxPct).Add("%)");
                if (part == 2) return m.Add("łupy: +").Add(d.DropPerLuckPct).Add("% szansy/pkt");
                return m.Add("SZCZ: kryt ").Add(d.CritBasePct).Add("% +").Add(d.CritPerLuckPct).Add("%/pkt");
            default:
                return m;
        }
    }
}
