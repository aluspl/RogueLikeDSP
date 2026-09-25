namespace LifeLike.Core.Data;

/// <summary>Etap budowy = piętro lochu. Pool = indeksy wrogów, Boss = -1 gdy brak; Cost = koszt w tys. zł (harmonogram domu).</summary>
public sealed record StageDef(string Name, int[] Pool, int EnemyCount, int Boss, int HpPct, int DmgBonus, int Act, int Cost = 0);
