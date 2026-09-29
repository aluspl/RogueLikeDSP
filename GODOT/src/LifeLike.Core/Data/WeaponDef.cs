namespace LifeLike.Core.Data;

/// <summary>Broń; Elem (v0.21.50): prąd (Próbnik), iskra (Szlifierka, Pistolet do kotew). v0.21.51 cz. 2: Crit = kryt +%
/// (Poziomica mistrza), Knockback = cios wręcz odpycha (Młot Zenka), Reveal = magazyn na podglądzie mapy.</summary>
public sealed record WeaponDef(string Id, string Name, int MinDamage, int MaxDamage, int Range, Stat ScalesWith, Element Elem = Element.None,
    int Crit = 0, bool Knockback = false, bool Reveal = false);
