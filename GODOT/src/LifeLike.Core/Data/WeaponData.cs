namespace LifeLike.Core.Data;

/// <summary>Definicja broni ładowana z data/weapons/*.json.</summary>
public sealed record WeaponData : IGameData
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public int MinDamage { get; init; } = 1;
    public int MaxDamage { get; init; } = 1;
    /// <summary>Zasięg w polach siatki (1 = walka wręcz).</summary>
    public int Range { get; init; } = 1;
    /// <summary>Statystyka dodawana do obrażeń (np. Strength dla miecza, Agility dla łuku).</summary>
    public StatType ScalesWith { get; init; } = StatType.Strength;

    public IEnumerable<string> Validate()
    {
        if (MinDamage < 0) yield return $"minDamage < 0";
        if (MaxDamage < MinDamage) yield return $"maxDamage ({MaxDamage}) < minDamage ({MinDamage})";
        if (Range < 1) yield return "range < 1";
    }
}
