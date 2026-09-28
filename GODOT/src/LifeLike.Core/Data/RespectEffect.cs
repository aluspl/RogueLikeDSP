namespace LifeLike.Core.Data;

/// <summary>Skutek ulepszenia za Respekt (core::respect_effect). Unknown: z nowszej wersji danych (bez działania).</summary>
public enum RespectEffect : byte
{
    DmgPct, TakenPct, GearPct, Crit, Dodge, CoffeePct, Thermos, Cooldown, Cash, XpPct, BrigadePct, Sight, ShopPct, MatsPct, SecondChance,
    Unknown = 255,
}
