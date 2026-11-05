namespace LifeLike.Core.Data;

/// <summary>v0.23.0: parametry typów ciosu. MaterialPct[typ, kolumna]: kolumny 0..2 = materiały wg GameData.Materials
/// (cement, stal, drewno), 3 = żywioł woda, 4 = bez materiału; wiersze wg HitType. Names = nazwy typów dla gracza.</summary>
public sealed record HitTypesDef(int[,] MaterialPct, int StunChancePct, int CutCritPct, int PierceDef, int SparkSteelPct, int PowerWaterPct,
    int FireWoodPct, int FireWaterPct, int DotPerTurn, string[] Names);
