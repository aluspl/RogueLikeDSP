namespace LifeLike.Core.Data;

/// <summary>Bazowe statystyki postaci. Niemutowalne — modyfikatory tworzą nową instancję.</summary>
public sealed record Stats
{
    public int MaxHealth { get; init; } = 10;
    public int Strength { get; init; }
    public int Agility { get; init; }
    public int Intelligence { get; init; }
    public int Defense { get; init; }

    public int Get(StatType type) => type switch
    {
        StatType.MaxHealth => MaxHealth,
        StatType.Strength => Strength,
        StatType.Agility => Agility,
        StatType.Intelligence => Intelligence,
        StatType.Defense => Defense,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
}

public enum StatType { MaxHealth, Strength, Agility, Intelligence, Defense }
