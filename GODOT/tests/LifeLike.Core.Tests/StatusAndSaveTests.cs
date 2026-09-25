namespace LifeLike.Core.Tests;

// core_tests.cpp: 18 (zapis budowy w trakcie), 27 (stany od problemów budowy).
public class StatusAndSaveTests
{
    private static GameData D => TestData.D;

    private static void Play(Game x)
    {
        if (x.St == GameStatus.StageClear) x.NextStage();
        else if (x.St == GameStatus.Playing) Bot.Step(x);
    }

    [Fact]
    public void RunSaveResumesIdenticallyAndRejectsCorruption()
    {
        var g = TestData.Run(2, 555, 2);
        for (var k = 0; k < 30; ++k) Play(g);
        var rs = RunSave.Make(g);
        Assert.True(rs.Valid(D));
        var h = RunSave.FromBytes(rs.ToBytes()).Load(D);
        for (var k = 0; k < 50; ++k)
        {
            Play(g);
            Play(h);
        }
        Assert.True(g.R.S == h.R.S && g.Turns == h.Turns && g.Score == h.Score && g.Stage == h.Stage && g.St == h.St);
        Assert.True(g.Hero.X == h.Hero.X && g.Hero.Y == h.Hero.Y && g.Hero.Hp == h.Hero.Hp && g.Xp == h.Xp);
        Assert.Equal(g.Lv.T, h.Lv.T);
        Assert.Equal(g.Fov, h.Fov);
        for (var i = 0; i < g.EnemiesCount; ++i)
            Assert.True(g.Enemies[i].X == h.Enemies[i].X && g.Enemies[i].Y == h.Enemies[i].Y && g.Enemies[i].Hp == h.Enemies[i].Hp);
        Assert.Equal(StateDigest.Of(g), StateDigest.Of(h));
        rs.Data[100] ^= 0x5A;
        Assert.False(rs.Valid(D));
        rs = RunSave.Make(g);
        rs.Size -= 4;
        Assert.False(rs.Valid(D));
        rs = RunSave.Make(g);
        rs.Clear();
        Assert.False(rs.Valid(D));
    }

    [Fact]
    public void PoisonHurtsButNeverKills()
    {
        var g = TestData.Arena(1);
        g.ApplyStatus(StatusEffect.Poison, 3);
        int hp = g.Hero.Hp;
        g.PlayerWait();
        g.PlayerWait();
        Assert.True(g.Hero.Hp <= hp - 2 + 1 && g.StatusTurns(StatusEffect.Poison) == 1);
        g.Hero.Hp = 1;
        g.PlayerWait();
        Assert.True(g.Hero.Hp == 1 && g.Hero.Alive);
    }

    [Fact]
    public void ShockLosesTurn()
    {
        var g = TestData.Arena(1);
        g.ApplyStatus(StatusEffect.Shock, 1);
        int x = g.Hero.X;
        Assert.True(g.PlayerMove(-1, 0) && g.Hero.X == x && g.Turns == 1);
        Assert.True(g.PlayerMove(-1, 0) && g.Hero.X == x - 1);
    }

    [Fact]
    public void SlipMovesTwoCells()
    {
        var g = TestData.Arena(1);
        g.ApplyStatus(StatusEffect.Slip, 2);
        int x = g.Hero.X;
        Assert.True(g.PlayerMove(-1, 0) && g.Hero.X == x - 2);
        g.Hero.X = 2;
        Assert.True(g.PlayerMove(-1, 0) && g.Hero.X == 1);
    }

    [Fact]
    public void PaperworkDelaysAbility()
    {
        var g = TestData.Arena(1);
        g.AbilityCd = 1;
        g.ApplyStatus(StatusEffect.Paper, 0);
        Assert.Equal(4, g.AbilityCd);
    }

    [Fact]
    public void MoldPoisonsOnHitSometimes()
    {
        var applied = 0;
        for (uint seed = 1; seed <= 200; ++seed)
        {
            var g = TestData.Run(1, seed);
            g.EnemiesCount = 0;
            g.Spawn(D.EnemyIndex("plesn"), g.Hero.X + 1, g.Hero.Y);
            g.Enemies[0].Awake = true;
            g.PlayerWait();
            if (g.StatusTurns(StatusEffect.Poison) > 0) applied++;
        }
        Assert.True(applied > 30 && applied < 120);
    }
}
