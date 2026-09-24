using LifeLike.Core.Generation;
using LifeLike.Core.Grid;

namespace LifeLike.Core.Tests;

public class DungeonGeneratorTests
{
    [Fact]
    public void SameSeed_GivesSameDungeon()
    {
        var a = DungeonGenerator.Generate(60, 40, seed: 2017);
        var b = DungeonGenerator.Generate(60, 40, seed: 2017);
        Assert.Equal(a.Rooms, b.Rooms);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(42)]
    [InlineData(2017)]
    [InlineData(2026)]
    public void AllFloorTiles_AreReachableFromStart(int seed)
    {
        var d = DungeonGenerator.Generate(60, 40, seed);
        var floor = new HashSet<GridPos>();
        for (var x = 0; x < d.Map.Width; x++)
            for (var y = 0; y < d.Map.Height; y++)
                if (!d.Map.IsWall(new GridPos(x, y))) floor.Add(new GridPos(x, y));

        var seen = new HashSet<GridPos> { d.PlayerStart };
        var queue = new Queue<GridPos>(seen);
        while (queue.Count > 0)
        {
            var p = queue.Dequeue();
            foreach (var dir in new[] { GridPos.Up, GridPos.Down, GridPos.Left, GridPos.Right })
                if (floor.Contains(p + dir) && seen.Add(p + dir)) queue.Enqueue(p + dir);
        }

        Assert.Equal(floor.Count, seen.Count);
        Assert.False(d.Map.IsWall(d.PlayerStart));
    }

    [Fact]
    public void MapBorder_IsAlwaysWall()
    {
        var d = DungeonGenerator.Generate(40, 30, seed: 7);
        for (var x = 0; x < 40; x++) { Assert.True(d.Map.IsWall(new(x, 0))); Assert.True(d.Map.IsWall(new(x, 29))); }
        for (var y = 0; y < 30; y++) { Assert.True(d.Map.IsWall(new(0, y))); Assert.True(d.Map.IsWall(new(39, y))); }
    }
}
