namespace LifeLike.Core.Tests;

// core_tests.cpp: 1 (mapy) i 12 (pole widzenia).
public class MapAndFovTests
{
    [Fact]
    public void MapsAreConnectedDeterministicAndHeroStartsOnFloor()
    {
        var d = TestData.D;
        for (uint seed = 1; seed <= 500; ++seed)
        {
            var g = TestData.Run((int)(seed % (uint)d.Classes.Length), seed);
            Assert.True(g.Lv.RoomsCount >= 2);
            Assert.True(TestData.Connected(g.Lv, g.Hero.X, g.Hero.Y));
            Assert.True(g.Lv.Passable(g.Hero.X, g.Hero.Y));
            var h = TestData.Run((int)(seed % (uint)d.Classes.Length), seed);
            Assert.Equal(g.Lv.T, h.Lv.T);
            for (var i = 0; i < g.EnemiesCount; ++i)
                Assert.False(g.Enemies[i].X == g.Hero.X && g.Enemies[i].Y == g.Hero.Y);
        }
    }

    [Fact]
    public void RngMatchesXorshift32()
    {
        var r = new Rng();
        r.Seed(1);
        Assert.Equal(270369u, r.Next());
        Assert.Equal(67634689u, r.Next());
        r.Seed(0);
        Assert.Equal(Rng.DefaultSeed, r.S);
    }

    [Fact]
    public void StartRoomVisibleFarRoomUnexplored()
    {
        for (uint seed = 1; seed <= 100; ++seed)
        {
            var g = TestData.Run(0, seed);
            var r0 = g.Lv.Rooms[0];
            Assert.True(g.Visible(g.Hero.X, g.Hero.Y));
            for (var y = r0.Y; y < r0.Y + r0.H; ++y)
                for (var x = r0.X; x < r0.X + r0.W; ++x)
                    Assert.True(g.Visible(x, y));
            var last = g.Lv.Rooms[g.Lv.RoomsCount - 1];
            if (Game.Cheb(g.Hero.X, g.Hero.Y, last.Cx, last.Cy) > Game.FovRadius) Assert.False(g.Explored(last.Cx, last.Cy));
        }
    }

    [Fact]
    public void ShadowcastingPillarsRememberedCellsAndHiddenEnemies()
    {
        var g = TestData.Run(0, 3);
        g.Lv.Fill(Tile.Wall);
        for (var y = 1; y <= 12; ++y)
            for (var x = 1; x <= 12; ++x)
                g.Lv[x, y] = Tile.Floor;
        g.Lv[6, 4] = Tile.Wall; // filar nad bohaterem
        g.EnemiesCount = 0;
        g.Hero.X = 6;
        g.Hero.Y = 6;
        g.UpdateFov();
        Assert.True(g.Visible(6, 4));
        Assert.False(g.Visible(6, 3));
        Assert.False(g.Visible(6, 2));
        Assert.True(g.Visible(9, 6) && g.Visible(3, 9));
        Assert.False(g.Visible(6, 6 + Game.FovRadius + 1));
        Assert.False(g.Explored(0, 0));
        Assert.True(g.Visible(1, 6));
        g.Lv[5, 6] = Tile.Wall;
        g.Lv[5, 5] = Tile.Wall;
        g.Lv[5, 7] = Tile.Wall;
        g.UpdateFov();
        Assert.False(g.Visible(1, 6));
        Assert.True(g.Explored(1, 6));
        g.Spawn(0, 2, 6);
        Assert.False(g.Visible(g.Enemies[0].X, g.Enemies[0].Y));
        g.Spawn(0, 9, 7);
        Assert.True(g.Visible(g.Enemies[1].X, g.Enemies[1].Y));
    }
}
