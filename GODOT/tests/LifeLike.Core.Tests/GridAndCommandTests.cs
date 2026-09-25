using LifeLike.Core.Actions;
using LifeLike.Core.Data;
using LifeLike.Core.Entities;
using LifeLike.Core.Grid;

namespace LifeLike.Core.Tests;

public class GridAndCommandTests
{
    [Fact]
    public void Move_IntoFreeTile_Succeeds()
    {
        var map = new TileGrid(3, 3);
        var p = new Actor("P", new Stats());
        map.Place(p, new GridPos(1, 1));
        Assert.Equal(CommandResult.Success, new MoveCommand(p, GridPos.Right).Execute(map));
        Assert.Equal(new GridPos(2, 1), p.Position);
        Assert.Same(p, map.ActorAt(new GridPos(2, 1)));
        Assert.Null(map.ActorAt(new GridPos(1, 1)));
    }

    [Theory]
    [InlineData(0, -1)] // poza mapą
    [InlineData(1, 0)]  // ściana
    public void Move_IntoBlockedTile_Fails(int dx, int dy)
    {
        var map = new TileGrid(3, 3);
        map.SetWall(new GridPos(1, 0));
        var p = new Actor("P", new Stats());
        map.Place(p, new GridPos(0, 0));
        Assert.Equal(CommandResult.Failed, new MoveCommand(p, new GridPos(dx, dy)).Execute(map));
        Assert.Equal(new GridPos(0, 0), p.Position);
    }

    [Fact]
    public void MoveOrAttack_BumpingEnemy_CreatesAttack()
    {
        var map = new TileGrid(3, 3);
        var p = new Actor("P", new Stats());
        var e = new Actor("E", new Stats());
        map.Place(p, new GridPos(0, 0));
        map.Place(e, new GridPos(1, 0));
        Assert.IsType<AttackCommand>(CommandFactory.MoveOrAttack(p, GridPos.Right, map, new FixedRandom()));
        Assert.IsType<MoveCommand>(CommandFactory.MoveOrAttack(p, GridPos.Down, map, new FixedRandom()));
    }
}
