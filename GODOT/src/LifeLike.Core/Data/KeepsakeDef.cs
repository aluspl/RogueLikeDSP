namespace LifeLike.Core.Data;

/// <summary>
/// Pamiątka (jak keepsake w Hades): zabierana na budowę, premia rośnie z rangą (I/II/III).
/// Values = premia na rangę I, II, III; Badge = odznaka, która ją odblokowuje (-1 = nie); Start = dostępna od początku.
/// </summary>
public sealed record KeepsakeDef(string Id, string Name, string Desc, PerkEffect Effect, int[] Values, int Badge, bool Start);
