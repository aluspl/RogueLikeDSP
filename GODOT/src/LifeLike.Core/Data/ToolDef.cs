namespace LifeLike.Core.Data;

/// <summary>Narzędzie do znalezienia (drop); Weapon = indeks broni, Cost 0 = dostępne od początku; Reward = z nagrody za odbiór;
/// Secret (v0.21.51 cz. 2) = z sekretnego zlecenia.</summary>
public sealed record ToolDef(int Weapon, int Cost, bool Reward = false, bool Secret = false);
