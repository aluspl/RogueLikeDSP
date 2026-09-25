namespace LifeLike.Core.Data;

/// <summary>„Problem budowy” (wróg). Slam = boss zapowiada uderzenie w obszar.</summary>
public sealed record EnemyDef(
    string Id, string Name, string Desc, int MaxHealth, int MinDamage, int MaxDamage, int Defense, int Sight, int Score,
    int Frame, bool Slam, StatusEffect OnHit, int StatusChance, int StatusTurns);
