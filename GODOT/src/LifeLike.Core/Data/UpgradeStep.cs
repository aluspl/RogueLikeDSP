namespace LifeLike.Core.Data;

/// <summary>Jeden poziom Szkolenia (v0.21.52, core::upgrade_step): przyrost premii i koszt w doświadczeniu.</summary>
public sealed record UpgradeStep(UpgradeEffect Effect, int Value, int Cost);
