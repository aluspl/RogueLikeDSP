namespace LifeLike.Core.Data;

/// <summary>Skutek ulepszenia za Respekt (core::respect_effect). Unknown: z nowszej wersji danych (bez działania).
/// Veteran (v0.21.51 cz. 2): Zaprawiony w boju – kawy w termosie na start.</summary>
public enum RespectEffect : byte
{
    DmgPct, TakenPct, GearPct, Crit, Dodge, CoffeePct, Thermos, Cooldown, Cash, XpPct, BrigadePct, Sight, ShopPct, MatsPct, SecondChance, Reroll,
    Veteran,
    Unknown = 255,
}
