using LifeLike.Core.Data;
using LifeLike.Core.Grid;

namespace LifeLike.Core.Entities;

/// <summary>
/// Postać w logice gry (gracz lub wróg). Zdarzenia C# mapujesz w Godot na sygnały
/// w cienkim node-adapterze — logika nie wie nic o silniku.
/// </summary>
public sealed class Actor
{
    public string Name { get; }
    public Stats Stats { get; }
    public WeaponData? Weapon { get; set; }
    public int Health { get; private set; }
    public GridPos Position { get; internal set; }
    public bool IsDead => Health <= 0;

    public event Action<Actor, int>? Damaged;
    public event Action<Actor>? Died;

    public Actor(string name, Stats stats, WeaponData? weapon = null)
    {
        Name = name;
        Stats = stats;
        Weapon = weapon;
        Health = stats.MaxHealth;
    }

    /// <summary>Fabryka: tworzy aktora z definicji klasy (Factory + data-driven).</summary>
    public static Actor FromClass(CharacterClassData cls, GameDatabase db) =>
        new(cls.Name, cls.BaseStats,
            cls.StartingWeapon is { } w ? db.Weapons[w] : null);

    public void TakeDamage(int amount)
    {
        if (IsDead || amount <= 0) return;
        Health = Math.Max(0, Health - amount);
        Damaged?.Invoke(this, amount);
        if (IsDead) Died?.Invoke(this);
    }
}
