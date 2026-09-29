namespace LifeLike.Core.Data;

/// <summary>
/// Stałe ulepszenie za Respekt (telefon profilu, strona Respekt): Values = wartość na randze 1..Ranks (łącznie),
/// Costs = koszt kolejnych rang w Respekcie; Secret (v0.21.51 cz. 2) = odblokowuje sekretne zlecenie (-1 = od początku).
/// </summary>
public sealed record RespectDef(string Id, string Name, string Desc, RespectEffect Effect, int[] Values, int[] Costs, int Secret = -1)
{
    public int Ranks => Values.Length;
}
