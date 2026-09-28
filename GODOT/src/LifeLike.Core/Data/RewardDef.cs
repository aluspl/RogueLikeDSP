namespace LifeLike.Core.Data;

/// <summary>
/// Nagroda za odbiór: każda wygrana odblokowuje kolejną z listy. Index = narzędzie (GameData.Tools), slot sprzętu
/// albo zawód; -1 = wkrótce.
/// </summary>
public sealed record RewardDef(RewardKind Kind, int Index, string Name, string Desc);
