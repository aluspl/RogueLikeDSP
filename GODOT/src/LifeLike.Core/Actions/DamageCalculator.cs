using LifeLike.Core.Entities;

namespace LifeLike.Core.Actions;

/// <summary>Abstrakcja losowości — w testach deterministyczna, w grze Random/Godot RNG.</summary>
public interface IRandom { int Next(int minInclusive, int maxInclusive); }

public sealed class SystemRandom(int? seed = null) : IRandom
{
    private readonly Random _r = seed is { } s ? new Random(s) : new Random();
    public int Next(int min, int max) => _r.Next(min, max + 1);
}

public static class DamageCalculator
{
    /// <summary>obrażenia = rzut broni + statystyka skalująca − obrona celu, min 1.</summary>
    public static int Roll(Actor attacker, Actor target, IRandom rng)
    {
        var w = attacker.Weapon;
        var baseDmg = w is null ? 1 : rng.Next(w.MinDamage, w.MaxDamage);
        var bonus = w is null ? attacker.Stats.Strength : attacker.Stats.Get(w.ScalesWith);
        return Math.Max(1, baseDmg + bonus - target.Stats.Defense);
    }
}
