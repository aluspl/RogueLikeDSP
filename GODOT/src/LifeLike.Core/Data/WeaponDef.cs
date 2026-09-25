namespace LifeLike.Core.Data;

public sealed record WeaponDef(string Id, string Name, int MinDamage, int MaxDamage, int Range, Stat ScalesWith);
