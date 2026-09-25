namespace LifeLike.Core.Tests;

// core_tests.cpp: 3-8 (trudność, wynik, NG+, premie ze Szkoleń, doświadczenie), 14 (poziomy), 20 (fabuła).
public class DifficultyAndProgressionTests
{
    private static GameData D => TestData.D;

    [Fact]
    public void EnemyHpDamageAndScoreScaleWithDifficultyStageAndBoss()
    {
        var n = TestData.Run(0, 42, 1);
        Assert.True(n.Diff == 1 && n.Tier == 0);
        Assert.Equal(D.Stages[0].HpPct, n.EnemyHpPct());
        for (var i = 0; i < n.EnemiesCount; ++i)
            Assert.Equal(D.Enemies[n.Enemies[i].DefId].MaxHealth * D.Stages[0].HpPct / 100, n.Enemies[i].MaxHp);
        var e = TestData.Run(0, 42, 0);
        var h = TestData.Run(0, 42, 2);
        Assert.True(e.EnemyHpPct() < n.EnemyHpPct() && n.EnemyHpPct() < h.EnemyHpPct());
        Assert.True(e.EnemyDmgBonus() < n.EnemyDmgBonus() && n.EnemyDmgBonus() <= h.EnemyDmgBonus());
        Assert.True(e.ScorePct() < n.ScorePct() && n.ScorePct() < h.ScorePct());
        for (var i = 0; i < e.EnemiesCount; ++i) Assert.True(e.Enemies[i].MaxHp >= 1);
        var s = TestData.Run(0, 42, 1);
        int prevHp = s.EnemyHpPct(), prevDmg = s.EnemyDmgBonus();
        for (var k = 1; k < D.Stages.Length; ++k)
        {
            s.NextStage();
            Assert.True(s.EnemyHpPct() >= prevHp);
            Assert.True(s.EnemyDmgBonus() >= prevDmg);
            prevHp = s.EnemyHpPct();
            prevDmg = s.EnemyDmgBonus();
        }
        Assert.True(s.Boss >= 0 && s.Enemies[s.Boss].MaxHp > D.Enemies[D.Stages[s.Stage].Boss].MaxHealth);
    }

    [Fact]
    public void EnemyDamageIncludesDifficultyBonus()
    {
        var lost = new int[2];
        for (var k = 0; k < 2; ++k)
        {
            var g = TestData.Run(3, 7, k); // Łatwy (-1) vs Normalny
            g.EnemiesCount = 0;
            g.Spawn(8, g.Hero.X + 1, g.Hero.Y);
            g.Enemies[0].Awake = true;
            int before = g.Hero.Hp;
            g.PlayerWait();
            lost[k] = before - g.Hero.Hp;
        }
        Assert.True(lost[0] >= 1 && lost[1] == lost[0] + 1);
    }

    [Fact]
    public void KillScoreUsesDifficultyMultiplier()
    {
        var g = TestData.Run(1, 7, 2);
        g.EnemiesCount = 0;
        g.Spawn(0, g.Hero.X + 1, g.Hero.Y);
        g.Enemies[0].Hp = 1;
        g.PlayerMove(1, 0);
        Assert.True(g.Kills == 1 && g.Score == D.Enemies[0].Score * g.ScorePct() / 100);
        Assert.True(g.KillsByType[0] == 1 && g.KillsByType[1] == 0);
    }

    [Fact]
    public void NewGamePlusOnlyAfterWinKeepsBonusesAndRaisesTier()
    {
        var g = TestData.Run(2, 99, 1);
        Assert.False(g.NewGamePlus());
        g.DmgBonus = 2;
        g.DefBonus = 1;
        g.Score = 1234;
        g.St = GameStatus.Won;
        g.Stage = D.Stages.Length - 1;
        var dmg0 = D.Stages[0].DmgBonus;
        Assert.True(g.NewGamePlus());
        Assert.True(g.Tier == 1 && g.Stage == 0 && g.St == GameStatus.Playing && g.Cls == 2 && g.Diff == 1);
        Assert.True(g.DmgBonus == 2 && g.DefBonus == 1 && g.Score == 1234 && g.Hero.Hp == g.Hero.MaxHp);
        Assert.True(g.EnemyHpPct() > D.Stages[0].HpPct && g.EnemyDmgBonus() > dmg0);
    }

    [Fact]
    public void RunModsApplyAtStartAndPersist()
    {
        var a = TestData.Run(1, 5);
        var m = RunMods.Default(D);
        m.Hp = 8;
        m.Def = 1;
        m.Dmg = 2;
        m.Coffee = 4;
        m.Pickups = 2;
        var b = TestData.NewGame();
        b.NewRun(1, 5, D.DefaultDifficulty, m);
        Assert.True(b.Hero.MaxHp == a.Hero.MaxHp + 8 && b.Hero.Hp == b.Hero.MaxHp);
        Assert.True(b.DefBonus == 1 && b.DmgBonus == 2);
        Assert.Equal(a.PickupsCount + 2, b.PickupsCount);
        b.Hero.Hp = 1;
        b.Pickups[0] = new Pickup(b.Hero.X, b.Hero.Y, PickupType.Coffee, true);
        b.Collect();
        Assert.True(b.Hero.Hp == 1 && b.Thermos == 1); // kawa trafia do termosu
        Assert.True(b.PlayerDrink() && b.Hero.Hp == 1 + D.CoffeeHeal + 4 && b.Thermos == 0 && b.Turns == 1);
        Assert.False(b.PlayerDrink()); // pusty termos: bez tury
        b.Thermos = D.ThermosCapacity;
        b.Hero.Hp = 1;
        b.Pickups[0] = new Pickup(b.Hero.X, b.Hero.Y, PickupType.Coffee, true);
        b.Collect();
        Assert.True(b.Hero.Hp == 1 + D.CoffeeHeal + 4 && b.Thermos == D.ThermosCapacity); // pełny: pije od razu
        b.Hero.Hp = b.Hero.MaxHp;
        Assert.True(!b.PlayerDrink() && b.Thermos == D.ThermosCapacity);
        b.NextStage();
        Assert.Equal(a.PickupsCount + 2, b.PickupsCount);
    }

    [Fact]
    public void ExperienceFromKillsStagesAndBossScaledByDifficulty()
    {
        var g = TestData.Run(1, 7);
        g.EnemiesCount = 0;
        g.Spawn(0, g.Hero.X + 1, g.Hero.Y);
        g.Enemies[0].Hp = 1;
        g.PlayerMove(1, 0);
        Assert.Equal(D.XpPerKill, g.Xp);
        g.DebugSkip();
        Assert.True(g.St == GameStatus.StageClear && g.Xp == D.XpPerKill + D.XpPerStage);
        var h = TestData.Run(1, 7, 2);
        h.EnemiesCount = 0;
        h.Spawn(0, h.Hero.X + 1, h.Hero.Y);
        h.Enemies[0].Hp = 1;
        h.PlayerMove(1, 0);
        h.DebugSkip();
        Assert.Equal((D.XpPerKill + D.XpPerStage) * h.ScorePct() / 100, h.Xp);
        var w = TestData.Run(1, 7);
        for (var k = 0; k < D.Stages.Length - 1; ++k)
        {
            w.DebugSkip();
            w.NextStage();
        }
        var before = w.Xp;
        w.DebugSkip();
        Assert.True(w.St == GameStatus.Won && w.Xp == before + D.XpPerKill + D.XpBoss);
    }

    [Fact]
    public void HeroLevelsDuringRun()
    {
        var g = TestData.Run(1, 5, 0); // Łatwy: poziomy liczone z surowego doświadczenia
        int hp0 = g.Hero.MaxHp, dmg0 = g.DmgBonus, def0 = g.DefBonus;
        Assert.Equal(1, g.HeroLevel);
        g.GainXp(D.LevelThresholds[0] - 1);
        Assert.Equal(1, g.HeroLevel);
        g.GainXp(1);
        Assert.True(g.HeroLevel == 2 && g.Hero.MaxHp == hp0 + D.HpPerLevel);
        Assert.True(g.DmgBonus == dmg0 + ((D.DmgLevelsMask >> 2) & 1) && g.DefBonus == def0 + ((D.DefLevelsMask >> 2) & 1));
        g.GainXp(D.LevelThresholds[1] - D.LevelThresholds[0]);
        Assert.True(g.HeroLevel == 3 && g.Hero.MaxHp == hp0 + 2 * D.HpPerLevel);
        g.GainXp(10000);
        Assert.Equal(D.MaxHeroLevel, g.HeroLevel);
        Assert.True(g.XpToNext() < 0);
        var h = TestData.Run(1, 5);
        Assert.Equal(D.LevelThresholds[0], h.XpToNext());
        g.St = GameStatus.Won;
        g.NewGamePlus();
        Assert.Equal(D.MaxHeroLevel, g.HeroLevel);
    }

    [Fact]
    public void StageStoryAndNgPlusStory()
    {
        var g = TestData.Run(1, 3);
        Assert.Same(D.StoryStages[0], g.StageStory);
        g.NextStage();
        Assert.Same(D.StoryStages[1], g.StageStory);
        g.St = GameStatus.Won;
        g.Stage = D.Stages.Length - 1;
        g.NewGamePlus();
        Assert.Same(D.StoryNgPlus, g.StageStory);
        foreach (var s in D.StoryStages) Assert.True(s.From.Length > 0 && s.Lines[0].Length > 0);
    }
}
