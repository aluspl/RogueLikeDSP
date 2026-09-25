namespace LifeLike.Core.Data;

/// <summary>Definicja klasy postaci ładowana z data/classes/*.json.</summary>
public sealed record CharacterClassData : IGameData
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string Description { get; init; } = "";
    public Stats BaseStats { get; init; } = new();
    /// <summary>Id broni startowej (musi istnieć w data/weapons).</summary>
    public string? StartingWeapon { get; init; }

    public IEnumerable<string> Validate()
    {
        if (BaseStats.MaxHealth <= 0) yield return "baseStats.maxHealth <= 0";
    }
}
