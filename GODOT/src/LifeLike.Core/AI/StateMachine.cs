namespace LifeLike.Core.AI;

/// <summary>Stan FSM. Zwraca kolejny stan albo siebie.</summary>
public interface IState<TContext>
{
    void Enter(TContext ctx) { }
    IState<TContext> Update(TContext ctx);
    void Exit(TContext ctx) { }
}

/// <summary>Minimalna FSM niezależna od silnika (węzłową wersję robisz w Godot jako adapter).</summary>
public sealed class StateMachine<TContext>
{
    public IState<TContext> Current { get; private set; }
    private readonly TContext _ctx;

    public StateMachine(TContext ctx, IState<TContext> initial)
    {
        _ctx = ctx;
        Current = initial;
        Current.Enter(ctx);
    }

    public void Update()
    {
        var next = Current.Update(_ctx);
        if (ReferenceEquals(next, Current)) return;
        Current.Exit(_ctx);
        Current = next;
        Current.Enter(_ctx);
    }
}
