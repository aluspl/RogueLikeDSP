namespace LifeLike.Core.Data;

/// <summary>Narzędzie do znalezienia (drop); Weapon = indeks broni, Cost 0 = dostępne od początku.</summary>
public sealed record ToolDef(int Weapon, int Cost);
