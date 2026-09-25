using System.Collections.Generic;
using System.Linq;
using LifeLike.Core;
using LifeLike.Core.Data;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.Gfx;

/// <summary>Teksty wspólne dla HUD, telefonu i ekranów (odpowiedniki pomocniczych funkcji z GBA/src/main.cpp).</summary>
public static class UiText
{
    public static string Roman(int i) => i switch { 0 => "I", 1 => "II", 2 => "III", 3 => "IV", 4 => "V", _ => (i + 1).ToString() };

    private static readonly (Stat S, string Name)[] StatNames = [(Stat.Str, "SIŁ"), (Stat.Agi, "ZRĘ"), (Stat.Intel, "INT")];

    public static string StatShort(Stat s) => s == Stat.Str ? "SIŁ" : s == Stat.Agi ? "ZRĘ" : "INT";

    /// <summary>Statystyka z premią, np. „SIŁ 5+2” (baza zawodu + Warsztaty/cechy sprzętu), bez premii „SIŁ 5”.</summary>
    public static string StatText(string name, int baseValue, int bonus) => bonus > 0 ? $"{name} {baseValue}+{bonus}" : $"{name} {baseValue}";

    /// <summary>Statystyki efektywne bohatera: „SIŁ 5+2 ZRĘ 3 INT 4+1”.</summary>
    public static string Stats(CoreGame g) =>
        string.Join(" ", StatNames.Select(x => StatText(x.Name, RunMods.ClassBaseStat(g.D, g.Cls, x.S), g.StatBonus(x.S))));

    /// <summary>Wiersz statystyk jak hero_stats_line na GBA: SIŁ, ZRĘ, INT, SZCZ z premiami.</summary>
    public static string HeroStatsLine(CoreGame g) => Stats(g) + " " + StatText("SZCZ", g.CDef.Luck, g.Luck() - g.CDef.Luck);

    /// <summary>Statystyki zawodu z premią z profilu (Warsztaty na statystykę broni).</summary>
    public static string ClassStat(GameData d, int cls, in RunMods m, Stat s) =>
        StatText(StatShort(s), RunMods.ClassBaseStat(d, cls, s), RunMods.StatBonus(d, m, cls, s));

    /// <summary>Nazwa mocy z rangą, np. „Ścianka II” (ability_label na GBA).</summary>
    public static string AbilityLabel(CoreGame g)
    {
        var r = g.AbilityRank();
        return g.CDef.AbilityName + (r > 1 ? " " + Roman(r - 1) : "");
    }

    public static readonly StatusEffect[] HudStatuses = [StatusEffect.Poison, StatusEffect.Shock, StatusEffect.Slip];

    /// <summary>Aktywne stany z turami: jeden - pełna nazwa i skutek, kilka - skróty (status_line na GBA).</summary>
    public static string StatusLine(CoreGame g, out bool any)
    {
        var active = HudStatuses.Where(s => g.StatusTurns(s) > 0).ToList();
        any = active.Count > 0;
        if (active.Count == 0) return "Stany: brak";
        if (active.Count == 1)
        {
            var sd = g.D.Statuses[(int)active[0]];
            return $"Stany: {sd.Name} {g.StatusTurns(active[0])} t. ({sd.Effect})";
        }
        return "Stany: " + string.Join(", ", active.Select(s => $"{g.D.Statuses[(int)s].Short} {g.StatusTurns(s)}")) + " t.";
    }

    public static string GearStatName(GearStat s) => s == GearStat.Def ? "Obrona" : s == GearStat.Dmg ? "Obrażenia" : "Max HP";

    /// <summary>Założony sprzęt z cechami, np. „Kask budowlany [Kryt+5%]”.</summary>
    public static string Gear(CoreGame g)
    {
        var parts = new List<string>();
        for (var s = 0; s < g.D.GearSlotsCount; s++)
        {
            if (g.Equipped[s] >= 0) parts.Add($"{g.D.Gear[s * 3 + g.Equipped[s]].Name} [{g.D.GearTraits[g.EquippedTrait[s]].Short}]");
        }
        return parts.Count == 0 ? "brak" : string.Join(", ", parts);
    }

    /// <summary>Uprawnienia ze zdobytych odznak (premie na każdą budowę).</summary>
    public static string Perks(GameData d, Profile p) =>
        string.Join(", ", d.Badges.Where((_, i) => (p.Badges & (1 << i)) != 0).Select(b => RunMods.PerkLabel(b.Bonus)).Where(s => s.Length > 0));

    public static int BitCount(int v) => System.Numerics.BitOperations.PopCount((uint)v);
}
