namespace LifeLike.Core.Data;

/// <summary>Broń; Elem (v0.21.50): prąd (Próbnik), iskra (Szlifierka, Pistolet do kotew).</summary>
public sealed record WeaponDef(string Id, string Name, int MinDamage, int MaxDamage, int Range, Stat ScalesWith, Element Elem = Element.None);
