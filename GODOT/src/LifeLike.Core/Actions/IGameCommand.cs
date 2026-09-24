using LifeLike.Core.Entities;
using LifeLike.Core.Grid;

namespace LifeLike.Core.Actions;

public enum CommandResult { Success, Failed }

/// <summary>Command pattern: każda akcja w turze to obiekt. Input (klawiatura/mysz/pad) tylko je tworzy.</summary>
public interface IGameCommand
{
    CommandResult Execute(TileGrid map);
}

public sealed record WaitCommand(Actor Actor) : IGameCommand
{
    public CommandResult Execute(TileGrid map) => CommandResult.Success;
}

public sealed record MoveCommand(Actor Actor, GridPos Direction) : IGameCommand
{
    public CommandResult Execute(TileGrid map)
    {
        var target = Actor.Position + Direction;
        if (!map.IsWalkable(target)) return CommandResult.Failed;
        map.Move(Actor, target);
        return CommandResult.Success;
    }
}

public sealed record AttackCommand(Actor Attacker, Actor Target, IRandom Rng) : IGameCommand
{
    public CommandResult Execute(TileGrid map)
    {
        var range = Attacker.Weapon?.Range ?? 1;
        if (Target.IsDead || Attacker.Position.ChebyshevDistance(Target.Position) > range)
            return CommandResult.Failed;

        Target.TakeDamage(DamageCalculator.Roll(Attacker, Target, Rng));
        if (Target.IsDead) map.Remove(Target);
        return CommandResult.Success;
    }
}

/// <summary>Ruch w kierunku: jeśli na polu stoi wróg — atak (klasyczny "bump to attack").</summary>
public static class CommandFactory
{
    public static IGameCommand MoveOrAttack(Actor actor, GridPos dir, TileGrid map, IRandom rng) =>
        map.ActorAt(actor.Position + dir) is { IsDead: false } other
            ? new AttackCommand(actor, other, rng)
            : new MoveCommand(actor, dir);
}
