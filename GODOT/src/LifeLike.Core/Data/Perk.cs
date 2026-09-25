namespace LifeLike.Core.Data;

/// <summary>
/// Rodzaj trwałej premii na budowę (core::perk_effect): uprawnienie z odznaki albo pamiątka.
/// Unknown: efekt z nowszej wersji danych GBA, jeszcze nieprzeniesiony (bez działania).
/// </summary>
public enum PerkEffect : byte { Hp, Def, Dmg, Luck, Cooldown, Sight, Thermos, ToolPct, XpPct, Cash, Crit, Coffee, Unknown = 255 }

/// <summary>Trwała premia na budowę (core::perk).</summary>
public readonly record struct Perk(PerkEffect Effect, int Value);
