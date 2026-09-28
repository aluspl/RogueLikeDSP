namespace LifeLike.Core.Data;

/// <summary>
/// Stałe ulepszenie za Respekt (telefon profilu, strona Respekt): Values = wartość na randze 1..Ranks (łącznie),
/// Costs = koszt kolejnych rang w Respekcie.
/// </summary>
public sealed record RespectDef(string Id, string Name, string Desc, RespectEffect Effect, int[] Values, int[] Costs)
{
    public int Ranks => Values.Length;
}
