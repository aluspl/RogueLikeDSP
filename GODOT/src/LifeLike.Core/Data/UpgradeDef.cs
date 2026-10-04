namespace LifeLike.Core.Data;

/// <summary>
/// Ulepszenie ze sklepu „Szkolenia” (meta-progresja). Effect = główne działanie (opis, wyszukiwanie); Steps = kolejne
/// poziomy (v0.21.52: mniejsze przyrosty, rosnąca cena, różne działania); LegacyCosts = koszty poziomów sprzed v0.21.52
/// (zwrot przy migracji profilu v12); Refund = zwrot (dośw.) za poziom ponad maksimum.
/// </summary>
public sealed record UpgradeDef(string Id, string Name, string Desc, UpgradeEffect Effect, UpgradeStep[] Steps, int[] LegacyCosts, int Refund = 0)
{
    public int Levels => Steps.Length;

    public int Cost(int level) => Steps[level].Cost;
}
