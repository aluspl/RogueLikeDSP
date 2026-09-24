using LifeLike.Core.Actions;
using LifeLike.Core.Entities;
using LifeLike.Core.Grid;

namespace LifeLike.Core.Turns;

/// <summary>Źródło decyzji aktora: gracz (input) albo AI (strategia/FSM).</summary>
public interface IActorController
{
    /// <summary>null = czekamy na input gracza (tura wstrzymana).</summary>
    IGameCommand? Decide(Actor self, TileGrid map);
}

/// <summary>Prosta kolejka tur: każdy żywy aktor wykonuje jedną komendę na rundę.</summary>
public sealed class TurnManager(TileGrid map)
{
    private readonly List<(Actor Actor, IActorController Controller)> _order = new();
    private int _index;

    public event Action<Actor, IGameCommand, CommandResult>? CommandExecuted;
    public int Round { get; private set; } = 1;

    public void Add(Actor actor, IActorController controller) => _order.Add((actor, controller));

    /// <summary>Przetwarza tury aż do momentu, gdy któryś kontroler (gracz) potrzebuje inputu.</summary>
    public void Advance()
    {
        _order.RemoveAll(e => e.Actor.IsDead);
        var guard = _order.Count * 2 + 1;
        while (_order.Count > 0 && guard-- > 0)
        {
            if (_index >= _order.Count) { _index = 0; Round++; }
            var (actor, ctrl) = _order[_index];
            var cmd = ctrl.Decide(actor, map);
            if (cmd is null) return;

            var result = cmd.Execute(map);
            CommandExecuted?.Invoke(actor, cmd, result);
            _order.RemoveAll(e => e.Actor.IsDead);
            _index = Math.Min(_index + 1, _order.Count);
        }
    }
}

/// <summary>Kontroler gracza: warstwa inputu wkłada komendę, TurnManager ją zabiera.</summary>
public sealed class PlayerController : IActorController
{
    private IGameCommand? _pending;
    public void Submit(IGameCommand command) => _pending = command;
    public IGameCommand? Decide(Actor self, TileGrid map)
    {
        var c = _pending; _pending = null; return c;
    }
}
