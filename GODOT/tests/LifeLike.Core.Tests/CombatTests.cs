using LifeLike.Core.Actions;
using LifeLike.Core.Data;
using LifeLike.Core.Entities;
using LifeLike.Core.Grid;

namespace LifeLike.Core.Tests;

public class CombatTests
{
    private static readonly WeaponData Sword = new() { Id = "s", Name = "Miecz", MinDamage = 2, MaxDamage = 4, ScalesWith = StatType.Strength };
    private static readonly WeaponData Bow = new() { Id = "b", Name = "Łuk", MinDamage = 1, MaxDamage = 1, Range = 3, ScalesWith = StatType.Agility };

    [Fact]
    public void Damage_IsWeaponRollPlusScalingStatMinusDefense()
    {
        var a = new Actor("A", new Stats { Strength = 3 }, Sword);
        var t = new Actor("T", new Stats { Defense = 2 });
        Assert.Equal(4 + 3 - 2, DamageCalculator.Roll(a, t, new FixedRandom()));
    }

    [Fact]
    public void Damage_IsAtLeastOne()
    {
        var a = new Actor("A", new Stats(), Sword);
        var t = new Actor("T", new Stats { Defense = 100 });
        Assert.Equal(1, DamageCalculator.Roll(a, t, new FixedRandom()));
    }

    [Fact]
    public void Attack_OutOfRange_Fails_InRange_Succeeds()
    {
        var map = new TileGrid(10, 10);
        var archer = new Actor("A", new Stats { Agility = 1 }, Bow);
        var target = new Actor("T", new Stats { MaxHealth = 10 });
        map.Place(archer, new GridPos(0, 0));
        map.Place(target, new GridPos(4, 0));

        Assert.Equal(CommandResult.Failed, new AttackCommand(archer, target, new FixedRandom()).Execute(map));
        map.Remove(target); map.Place(target, new GridPos(3, 0));
        Assert.Equal(CommandResult.Success, new AttackCommand(archer, target, new FixedRandom()).Execute(map));
        Assert.Equal(8, target.Health);
    }

    [Fact]
    public void Kill_RaisesEvents_AndFreesTile()
    {
        var map = new TileGrid(5, 5);
        var a = new Actor("A", new Stats { Strength = 10 }, Sword);
        var t = new Actor("T", new Stats { MaxHealth = 3 });
        map.Place(a, new GridPos(1, 1));
        map.Place(t, new GridPos(2, 1));
        int damaged = 0; bool died = false;
        t.Damaged += (_, d) => damaged = d;
        t.Died += _ => died = true;

        new AttackCommand(a, t, new FixedRandom()).Execute(map);

        Assert.True(t.IsDead);
        Assert.True(died);
        Assert.Equal(14, damaged);
        Assert.Null(map.ActorAt(new GridPos(2, 1)));
    }

    [Fact]
    public void Actor_FromClass_UsesJsonDefinition()
    {
        var db = GameDatabase.LoadFromDirectory(Paths.GameData);
        var hero = Actor.FromClass(db.Classes["warrior"], db);
        Assert.Equal(db.Classes["warrior"].BaseStats.MaxHealth, hero.Health);
        Assert.Equal("rusty_sword", hero.Weapon!.Id);
    }
}
