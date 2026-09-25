using LifeLike.Core.Actions;
using LifeLike.Core.Entities;
using LifeLike.Core.Grid;
using LifeLike.Core.Turns;

namespace LifeLike.Core.AI;

/// <summary>
/// Przykładowe AI wroga na FSM: Idle -> Chase (gdy gracz w zasięgu wzroku) -> atak przez bump.
/// Kroki bez pathfindingu (zachłanny ruch) — w Godot podmieniasz na AStarGrid2D.
/// </summary>
public sealed class ChaseAI(Actor target, int sightRange, IRandom rng) : IActorController
{
    public sealed class Ctx { public required Actor Self; public required Actor Target; public required int Sight; public IGameCommand? Command; public TileGrid Map = null!; public IRandom Rng = null!; }

    private sealed class Idle : IState<Ctx>
    {
        public IState<Ctx> Update(Ctx c)
        {
            if (c.Self.Position.ChebyshevDistance(c.Target.Position) <= c.Sight) return new Chase().Update(c);
            c.Command = new WaitCommand(c.Self);
            return this;
        }
    }

    private sealed class Chase : IState<Ctx>
    {
        public IState<Ctx> Update(Ctx c)
        {
            if (c.Target.IsDead || c.Self.Position.ChebyshevDistance(c.Target.Position) > c.Sight * 2)
            { c.Command = new WaitCommand(c.Self); return new Idle(); }

            var dx = Math.Sign(c.Target.Position.X - c.Self.Position.X);
            var dy = Math.Sign(c.Target.Position.Y - c.Self.Position.Y);
            var dir = Math.Abs(c.Target.Position.X - c.Self.Position.X) >= Math.Abs(c.Target.Position.Y - c.Self.Position.Y)
                ? new GridPos(dx, 0) : new GridPos(0, dy);
            c.Command = CommandFactory.MoveOrAttack(c.Self, dir, c.Map, c.Rng);
            return this;
        }
    }

    private StateMachine<Ctx>? _fsm;
    private Ctx? _ctx;
    public string StateName => _fsm?.Current.GetType().Name ?? "Idle";

    public IGameCommand? Decide(Actor self, TileGrid map)
    {
        if (_fsm is null)
        {
            _ctx = new Ctx { Self = self, Target = target, Sight = sightRange, Map = map, Rng = rng };
            _fsm = new StateMachine<Ctx>(_ctx, new Idle());
        }
        _ctx!.Map = map;
        _fsm.Update();
        return _ctx.Command;
    }
}
