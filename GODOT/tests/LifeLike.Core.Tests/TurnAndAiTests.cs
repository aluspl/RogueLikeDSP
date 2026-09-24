using LifeLike.Core.Actions;
using LifeLike.Core.AI;
using LifeLike.Core.Data;
using LifeLike.Core.Entities;
using LifeLike.Core.Grid;
using LifeLike.Core.Turns;

namespace LifeLike.Core.Tests;

public class TurnAndAiTests
{
    [Fact]
    public void TurnManager_WaitsForPlayerInput()
    {
        var map = new TileGrid(10, 10);
        var player = new Actor("P", new Stats());
        map.Place(player, new GridPos(0, 0));
        var pc = new PlayerController();
        var tm = new TurnManager(map);
        tm.Add(player, pc);

        tm.Advance();
        Assert.Equal(new GridPos(0, 0), player.Position);

        pc.Submit(new MoveCommand(player, GridPos.Right));
        tm.Advance();
        Assert.Equal(new GridPos(1, 0), player.Position);
    }

    [Fact]
    public void Enemy_ActsAfterPlayer_EachRound()
    {
        var map = new TileGrid(10, 10);
        var player = new Actor("P", new Stats { MaxHealth = 50 });
        var rat = new Actor("R", new Stats());
        map.Place(player, new GridPos(0, 0));
        map.Place(rat, new GridPos(5, 0));
        var pc = new PlayerController();
        var tm = new TurnManager(map);
        tm.Add(player, pc);
        tm.Add(rat, new ChaseAI(player, sightRange: 8, new FixedRandom()));

        pc.Submit(new WaitCommand(player));
        tm.Advance();

        Assert.Equal(new GridPos(4, 0), rat.Position);
        Assert.Equal(2, tm.Round);
    }

    [Fact]
    public void ChaseAI_StaysIdle_WhenPlayerOutOfSight()
    {
        var map = new TileGrid(30, 30);
        var player = new Actor("P", new Stats());
        var rat = new Actor("R", new Stats());
        map.Place(player, new GridPos(0, 0));
        map.Place(rat, new GridPos(20, 20));
        var ai = new ChaseAI(player, sightRange: 5, new FixedRandom());

        Assert.IsType<WaitCommand>(ai.Decide(rat, map));
        Assert.Equal("Idle", ai.StateName);
    }

    [Fact]
    public void ChaseAI_AttacksWhenAdjacent()
    {
        var map = new TileGrid(5, 5);
        var player = new Actor("P", new Stats { MaxHealth = 10 });
        var rat = new Actor("R", new Stats { Strength = 1 });
        map.Place(player, new GridPos(1, 1));
        map.Place(rat, new GridPos(2, 1));
        var ai = new ChaseAI(player, sightRange: 5, new FixedRandom());

        var cmd = ai.Decide(rat, map);
        Assert.IsType<AttackCommand>(cmd);
        cmd!.Execute(map);
        Assert.True(player.Health < 10);
        Assert.Equal("Chase", ai.StateName);
    }

    [Fact]
    public void StateMachine_CallsEnterAndExit()
    {
        var log = new List<string>();
        var b = new LoggingState("B", log, null);
        var a = new LoggingState("A", log, b);
        var fsm = new StateMachine<object>(new object(), a);
        fsm.Update();
        Assert.Equal(["enter A", "exit A", "enter B"], log);
        Assert.Same(b, fsm.Current);
    }

    private sealed class LoggingState(string name, List<string> log, IState<object>? next) : IState<object>
    {
        public void Enter(object c) => log.Add($"enter {name}");
        public void Exit(object c) => log.Add($"exit {name}");
        public IState<object> Update(object c) => next ?? this;
    }
}
