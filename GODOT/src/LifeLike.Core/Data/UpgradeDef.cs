namespace LifeLike.Core.Data;

/// <summary>
/// Ulepszenie ze sklepu „Szkolenia” (meta-progresja). Costs = koszt kolejnych poziomów; Refund = zwrot (dośw.) za poziom
/// ponad maksimum w starym profilu; ResetRefund = zwrot za poziom zmienionego ulepszenia w profilu sprzed v8 (poziom od zera).
/// </summary>
public sealed record UpgradeDef(string Id, string Name, string Desc, UpgradeEffect Effect, int Value, int[] Costs, int Refund = 0, int ResetRefund = 0)
{
    public int Levels => Costs.Length;
}
