namespace LifeLike.Core.Data;

/// <summary>Ulepszenie ze sklepu „Szkolenia” (meta-progresja). Costs = koszt kolejnych poziomów.</summary>
public sealed record UpgradeDef(string Id, string Name, string Desc, UpgradeEffect Effect, int Value, int[] Costs)
{
    public int Levels => Costs.Length;
}
