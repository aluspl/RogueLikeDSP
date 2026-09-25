namespace LifeLike.Core.Data;

/// <summary>Poziom trudności wybierany na starcie.</summary>
public sealed record DifficultyDef(string Id, string Name, int HpPct, int DmgBonus, int ScorePct);
