namespace LifeLike.Core.Data;

/// <summary>Skutek premii po etapie (core::boon_effect). Unknown: z nowszej wersji danych (bez działania).</summary>
public enum BoonEffect : byte
{
    Dmg, DmgPct, Crit, MaxHp, Def, Dodge, Coffee, Thermos, Cooldown, Cash, Mats, Luck, WetHits, FrostHits, Electric, Spark,
    BrigadePct, RegenStage, KillHeal, StatusRes, Power, MatsPct, ShopPct, Sight,
    Unknown = 255,
}
