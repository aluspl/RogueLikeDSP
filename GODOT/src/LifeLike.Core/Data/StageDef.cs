namespace LifeLike.Core.Data;

/// <summary>
/// Etap budowy = piętro lochu. Pool = indeksy wrogów, Boss = -1 gdy brak; Cost = koszt w tys. zł (harmonogram domu).
/// v0.21.52 cz. d (#47): Look – paleta etapu (tiles/stage_N.png), Tiles – zestaw kafli GBA (0-2 akty, 3-4 Akt 0, 5 drewno,
/// 6 kamienica), Twin – druga połowa bliźniaka (pogoda, wydarzenie i niedokończone problemy z pierwszej).
/// </summary>
public sealed record StageDef(string Name, int[] Pool, int EnemyCount, int Boss, int HpPct, int DmgBonus, int Act, int Cost = 0,
    int Look = 0, int Tiles = 0, bool Twin = false);
