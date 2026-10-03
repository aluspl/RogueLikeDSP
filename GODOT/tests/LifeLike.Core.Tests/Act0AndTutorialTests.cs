namespace LifeLike.Core.Tests;

/// <summary>
/// core_tests.cpp 41 (v0.21.49 cz. 3): Akt 0 (Papierologia) – nagroda za odbiór, pieczątki zamykają schody, druga faza
/// bossa (Odwołanie), bot przechodzi Akt 0; samouczek menu (flagi w profilu, dymki odblokowań), profil v10.
/// </summary>
public class Act0AndTutorialTests
{
    private static GameData D => TestData.D;

    private static int F0 => TestData.F0;

    private static int ActReward() => Array.FindIndex(D.Rewards, r => r.Kind == RewardKind.Act);

    private static Profile WithAct0()
    {
        var p = Meta.NewProfile(D);
        for (var k = 0; k <= ActReward(); ++k) Meta.RecordWin(D, p);
        return p;
    }

    [Fact]
    public void Act0RewardStartsRunAtPrelude()
    {
        var p = Meta.NewProfile(D);
        Assert.True(!Meta.Act0Unlocked(D, p) && Meta.Mods(D, p).Act0 == 0);
        var ai = ActReward();
        Assert.True(ai == D.Rewards.Length - 1 && Meta.RewardsAvailable(D) == D.Rewards.Length);
        for (var k = 0; k <= ai; ++k) Assert.Equal(k, Meta.RecordWin(D, p));
        Assert.True(Meta.Act0Unlocked(D, p) && Meta.Mods(D, p).Act0 == 1);
        var n = TestData.Run(1, 11);
        Assert.True(n.FirstStage == F0 && n.Stage == F0 && n.StageNumber() == 1 && n.StagesInRun() == D.StagesCount - F0);
        var dly = new Game(D);
        Daily.Start(dly, 42);
        Assert.Equal(F0, dly.FirstStage); // budowa dnia bez Aktu 0
        var g = new Game(D);
        g.NewRun(1, 11, D.DefaultDifficulty, Meta.Mods(D, p));
        Assert.True(g.FirstStage == 0 && g.Stage == 0 && g.StageNumber() == 1 && g.StagesInRun() == D.StagesCount);
        Assert.True(D.Acts[D.Stages[0].Act].Prelude && g.ActIs(ActMechanic.Stamps) && g.ActNumeral() == "0");
        Assert.Equal("I", D.Acts[D.Stages[F0].Act].Numeral);
    }

    [Fact]
    public void StampsLockStairsUntilAllDocuments()
    {
        var g = new Game(D);
        g.NewRun(1, 11, D.DefaultDifficulty, Meta.Mods(D, WithAct0()));
        var docs = g.Pickups.Take(g.PickupsCount).Count(x => x.Type == PickupType.Document && x.Active);
        Assert.True(g.DocsNeeded() == D.Documents.Length && docs == D.Documents.Length && g.StairsLocked() && g.StairsX >= 0);
        g.EnemiesCount = 0;
        g.Hero.X = (sbyte)g.StairsX;
        g.Hero.Y = (sbyte)g.StairsY;
        g.PlayerWait();
        Assert.True(g.St == GameStatus.Playing && g.StairsLocked());
        for (var i = 0; i < g.PickupsCount; ++i)
        {
            if (g.Pickups[i].Type != PickupType.Document || !g.Pickups[i].Active) continue;
            g.Hero.X = g.Pickups[i].X;
            g.Hero.Y = g.Pickups[i].Y;
            g.Collect();
        }
        Assert.True(!g.StairsLocked() && g.DocsCount() == D.Documents.Length);
        g.Hero.X = (sbyte)g.StairsX;
        g.Hero.Y = (sbyte)g.StairsY;
        g.PlayerWait();
        Assert.Equal(GameStatus.StageClear, g.St);
        g.NextStage();
        Assert.True(g.Stage == 1 && g.DocsNeeded() == 0 && g.StairsX < 0 && g.Boss >= 0);
    }

    [Fact]
    public void BossSecondPhaseOnceAtHalfHp()
    {
        var p = WithAct0();
        var g = new Game(D);
        g.NewRun(1, 11, D.DefaultDifficulty, Meta.Mods(D, p));
        g.StartStage(1);
        var bd = D.Enemies[g.Enemies[g.Boss].DefId];
        Assert.True(bd.PhasePct > 0 && bd.PhaseHeal > 0 && bd.PhaseSummon > 0 && bd.Summon >= 0 && bd.Slam);
        g.Enemies[g.Boss].Awake = true;
        int mx = g.Enemies[g.Boss].MaxHp, at = mx * bd.PhasePct / 100;
        g.Enemies[g.Boss].Hp = (short)(at + 2);
        var alive0 = g.Enemies.Take(g.EnemiesCount).Count(e => e.Alive);
        g.DamageEnemy(g.Boss, 2, false, "test");
        var alive1 = g.Enemies.Take(g.EnemiesCount).Count(e => e.Alive);
        Assert.True((g.Enemies[g.Boss].Flags & ActorFlag.Phase) != 0 && g.Enemies[g.Boss].Hp == Math.Min(mx, at + mx * bd.PhaseHeal / 100));
        Assert.True(g.SummonsUsed == bd.PhaseSummon && alive1 == alive0 + bd.PhaseSummon);
        g.Enemies[g.Boss].Hp = 3;
        g.DamageEnemy(g.Boss, 1, false, "test");
        Assert.True(g.Enemies[g.Boss].Hp == 2 && g.SummonsUsed == bd.PhaseSummon); // tylko raz

        // cios, który by usunął bossa przed drugą fazą: zostaje z 1 HP + leczenie
        var h = new Game(D);
        h.NewRun(1, 12, D.DefaultDifficulty, Meta.Mods(D, p));
        h.StartStage(1);
        h.Enemies[h.Boss].Awake = true;
        h.DamageEnemy(h.Boss, 999, false, "test");
        var b = h.Enemies[h.Boss];
        Assert.True(b.Alive && (b.Flags & ActorFlag.Phase) != 0 && b.Hp == Math.Min(b.MaxHp, 1 + b.MaxHp * bd.PhaseHeal / 100));
        h.DebugSkip(); // skrót pokazowy: boss Aktu 0 = boss aktu (premia, Hurtownia, Respekt za akt)
        Assert.True(h.St == GameStatus.StageClear && h.ActCleared && h.Respect == D.RespectActBoss * h.ScorePct() / 100);
        h.NextStage();
        Assert.True(h.Stage == F0 && h.ActNumeral() == "I");
    }

    [Fact]
    public void BotPassesAct0CollectingDocuments()
    {
        var m = Meta.Mods(D, WithAct0());
        var passed = 0;
        for (var k = 0; k < 20; ++k)
        {
            var b = new Game(D);
            b.NewRun(k % D.Classes.Length, (uint)(300 + k * 13), 0, m);
            for (var step = 0; step < 3000 && b.St == GameStatus.Playing && b.Stage < F0; ++step) Bot.Step(b);
            while (b.St == GameStatus.StageClear && b.Stage < F0)
            {
                Bot.Next(b); // z premią po etapie jak bot_next w core_tests.cpp
                for (var step = 0; step < 3000 && b.St == GameStatus.Playing; ++step) Bot.Step(b);
            }
            if (b.Stage >= F0 || (b.St == GameStatus.StageClear && b.Stage == F0 - 1)) ++passed;
        }
        Assert.True(passed >= 14, $"Akt 0 przeszło {passed}/20");
    }

    [Fact]
    public void TutorialFlagsAndUnlockBubbles()
    {
        var t = Meta.NewProfile(D);
        Assert.True(Meta.TutorialPending(t, 0) && Meta.TutorialPending(t, 1) && Meta.PendingUnlock(D, t, 0, out _) == -1);
        int gba = 0, godot = 0;
        for (var i = 0; i < D.TutorialSteps.Length; ++i)
        {
            if (Meta.TutorialStepShown(D, t, i, false)) ++gba;
            if (Meta.TutorialStepShown(D, t, i, true)) ++godot;
        }
        Assert.True(gba > 5 && godot == gba + 1); // klucz z opcjami tylko w Godocie; tryb inwestora jeszcze ukryty
        Meta.TutorialDone(t, 0);
        Meta.TutorialDone(t, 1);
        Assert.True(!Meta.TutorialPending(t, 0) && !Meta.TutorialPending(t, 1));
        Assert.True(Meta.PendingUnlock(D, t, 0, out _) == -1 && Meta.PendingUnlock(D, t, 1, out _) == -1);
        t.RespectTotal = 2;
        Assert.Equal(TutorialUnlock.Respect, Meta.PendingUnlock(D, t, 0, out _));
        Meta.MarkUnlock(t, TutorialUnlock.Respect, -1);
        t.Runs = 1;
        Assert.Equal(TutorialUnlock.Daily, Meta.PendingUnlock(D, t, 0, out _));
        Meta.MarkUnlock(t, TutorialUnlock.Daily, -1);
        Assert.Equal(-1, Meta.PendingUnlock(D, t, 0, out _));
        Meta.RecordWin(D, t);
        Assert.Equal(TutorialUnlock.Investor, Meta.PendingUnlock(D, t, 1, out _));
        Meta.MarkUnlock(t, TutorialUnlock.Investor, -1);
        var gba2 = Enumerable.Range(0, D.TutorialSteps.Length).Count(i => Meta.TutorialStepShown(D, t, i, false));
        Assert.Equal(gba + 1, gba2); // krok o trybie inwestora po odblokowaniu
        int cls;
        while (Meta.PendingUnlock(D, t, 1, out cls) == -1 && t.Rewards < D.Rewards.Length) Meta.RecordWin(D, t);
        Assert.True(Meta.PendingUnlock(D, t, 1, out cls) == TutorialUnlock.Class && Meta.ClassReward(D, cls) && Meta.ClassUnlocked(D, t, cls));
        Meta.MarkUnlock(t, TutorialUnlock.Class, cls);
        while (t.Rewards < D.Rewards.Length) Meta.RecordWin(D, t);
        for (var k = 0; k < D.Classes.Length; ++k)
        {
            if (Meta.PendingUnlock(D, t, 1, out var c2) == TutorialUnlock.Class) Meta.MarkUnlock(t, TutorialUnlock.Class, c2);
        }
        Assert.True(Meta.PendingUnlock(D, t, 1, out _) == -1 && Meta.PendingUnlock(D, t, 0, out _) == TutorialUnlock.Act0);
        Meta.MarkUnlock(t, TutorialUnlock.Act0, -1);
        Assert.Equal(-1, Meta.PendingUnlock(D, t, 0, out _));
        Meta.TutorialReset(t); // powtórka z Jak grać
        Assert.True(Meta.TutorialPending(t, 0) && Meta.TutorialPending(t, 1) && (t.Tutorial & Tutorial.Act0) != 0);
    }

    [Fact]
    public void ProfileV9MigratesToV10()
    {
        var v = Meta.NewProfile(D);
        v.Magic = Profile.MagicBytes(Profile.MagicV9);
        v.Runs = 12;
        v.Wins = 9;
        v.Rewards = (byte)ActReward();
        v.RespectTotal = 40;
        v.Best = 999;
        v.Tutorial = 0xABCD;
        v.ClassesSeen = 0x1234;
        Assert.True(Meta.ProfileFix(D, v) && v.MagicIs(Profile.MagicCurrent) && v.Best == 999 && v.Rewards == D.Rewards.Length && Meta.Act0Unlocked(D, v));
        Assert.True(!Meta.TutorialPending(v, 0) && !Meta.TutorialPending(v, 1));
        Assert.True(Meta.PendingUnlock(D, v, 0, out _) == TutorialUnlock.Act0 && Meta.PendingUnlock(D, v, 1, out _) == -1);
        var nv = Meta.NewProfile(D);
        nv.Magic = Profile.MagicBytes(Profile.MagicV9);
        Assert.True(Meta.ProfileFix(D, nv) && Meta.TutorialPending(nv, 0) && nv.Rewards == 0 && nv.Tutorial == 0);
        Assert.Equal(Profile.Size, v.ToBytes().Length);
    }

    [Fact]
    public void RunSaveKeepsAct0State()
    {
        var g = new Game(D);
        g.NewRun(1, 11, D.DefaultDifficulty, Meta.Mods(D, WithAct0()));
        g.Docs = 5;
        var c = g.Clone();
        Assert.True(c.FirstStage == 0 && c.Docs == 5 && c.Bonus.Act0 == 1);
        Assert.Equal(StateDigest.Of(g), StateDigest.Of(c));
    }
}
