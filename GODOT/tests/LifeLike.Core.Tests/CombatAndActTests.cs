namespace LifeLike.Core.Tests;

// core_tests.cpp: 2 i 22 (dziennik), 13 (trafienia), 23 (celowanie), 24 (akty), 25 (uderzenie bossa),
// 25b (Inspekcja Pracy), 26 (Hurtownia).
public class CombatAndActTests
{
    private static GameData D => TestData.D;

    [Fact]
    public void MessageFormatsPolishTextAndNumbers()
    {
        var m = new Message().Add("Kawa: +").Add(8).Add(" HP");
        Assert.Equal("Kawa: +8 HP", m.Text);
        Assert.Equal("-12", new Message().Add(-12).Text);
    }

    [Fact]
    public void MessageTruncatesTo47BytesLikeGba()
    {
        var m = new Message().Add("Poziomica laserowa: -12 (Nieprzekraczalny Termin)");
        Assert.Equal(Message.Len - 1, m.N);
        Assert.Equal(0, m.S[m.N]);
    }

    [Fact]
    public void LogRepeatsKindsAndSerial()
    {
        var g = TestData.Arena(1);
        var serial = g.LogSerial;
        g.Push(new Message().Add("Brak celu").As(LogKind.Info));
        g.Push(new Message().Add("Brak celu").As(LogKind.Info));
        Assert.Equal(serial + 2, g.LogSerial);
        Assert.True(g.Log[Game.LogLines - 1].Text == "Brak celu" && g.Log[Game.LogLines - 1].Repeat == 2);
        Assert.NotEqual("Brak celu", g.Log[Game.LogLines - 2].Text);
        g.Push(new Message().Add("Awans").As(LogKind.Good));
        Assert.True(g.Log[Game.LogLines - 1].Kind == LogKind.Good && g.Log[Game.LogLines - 2].Repeat == 2);
        g.Spawn(8, 8, 7);
        g.Enemies[0].Awake = true;
        g.PlayerWait();
        Assert.Equal(LogKind.Bad, g.Log[Game.LogLines - 1].Kind);
    }

    [Fact]
    public void HitEventsForDamageNumbersAndTargetBar()
    {
        var g = TestData.Run(1, 11);
        g.EnemiesCount = 0;
        g.Spawn(8, g.Hero.X + 1, g.Hero.Y);
        g.Enemies[0].Awake = true;
        int ehp = g.Enemies[0].Hp, hhp = g.Hero.Hp;
        g.PlayerMove(1, 0);
        Assert.Equal(2, g.HitsCount);
        Assert.True(!g.Hits[0].OnHero && g.Hits[0].Amount == ehp - g.Enemies[0].Hp && g.Hits[0].X == g.Enemies[0].X);
        Assert.True(g.Hits[1].OnHero && g.Hits[1].Amount == hhp - g.Hero.Hp && g.Hits[1].X == g.Hero.X);
        Assert.Equal(0, g.LastTarget);
        g.HitsCount = 0;
        for (var k = 0; k < 20; ++k) g.PlayerWait();
        Assert.True(g.HitsCount <= Game.MaxHits);
    }

    [Fact]
    public void TargetingNearestFirstAndOnlyInRange()
    {
        var g = TestData.Arena(2);
        g.Spawn(4, 10, 7);
        g.Spawn(4, 8, 8);
        g.Spawn(4, 13, 7);
        Span<sbyte> t = stackalloc sbyte[8];
        var n = g.TargetsInRange(t);
        Assert.True(n == 2 && t[0] == 1 && t[1] == 0);
        Assert.True(!g.PlayerAttack(2) && g.Turns == 0);
        int hp = g.Enemies[0].Hp;
        Assert.True(g.PlayerAttack(0) && g.Enemies[0].Hp < hp && g.Turns == 1);
    }

    [Fact]
    public void KillGivesCash()
    {
        var g = TestData.Run(1, 5);
        g.EnemiesCount = 0;
        g.Spawn(0, g.Hero.X + 1, g.Hero.Y);
        g.Enemies[0].Hp = 1;
        g.PlayerMove(1, 0);
        Assert.Equal(D.Enemies[0].Score / D.CashPerScore, g.Cash);
    }

    [Fact]
    public void ActBossGivesBonusAndHurtowniaLastBossWins()
    {
        var g = TestData.Run(1, 5);
        var lastOfAct0 = 0;
        while (D.Stages[lastOfAct0 + 1].Act == 0) ++lastOfAct0;
        for (var k = 0; k < lastOfAct0; ++k)
        {
            g.DebugSkip();
            Assert.True(g.St == GameStatus.StageClear && !g.ActCleared);
            g.NextStage();
        }
        Assert.True(g.Boss >= 0 && D.Enemies[g.Enemies[g.Boss].DefId].Slam);
        var c0 = g.Cash;
        g.DebugSkip();
        Assert.True(g.St == GameStatus.StageClear && g.ActCleared);
        var stagesInAct = lastOfAct0 + 1;
        Assert.Equal(D.Acts[0].BonusPerStage * stagesInAct + D.Acts[0].BonusPerKill * 1, g.ActBonus);
        Assert.Equal(c0 + g.ActBonus + D.Enemies[g.Enemies[g.Boss].DefId].Score / D.CashPerScore, g.Cash);
        g.NextStage();
        Assert.True(!g.ActCleared && D.Stages[g.Stage].Act == 1);
        while (g.Stage < D.Stages.Length - 1)
        {
            g.DebugSkip();
            g.NextStage();
        }
        g.DebugSkip();
        Assert.Equal(GameStatus.Won, g.St);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BossSlamTelegraphedAndDodgeable(bool dodge)
    {
        var g = TestData.Arena(1);
        g.Spawn(D.EnemyIndex("betoniarka"), 10, 7);
        g.Boss = 0;
        g.Enemies[0].Awake = true;
        for (var k = 0; k < 12 && g.SlamTimer == 0; ++k) g.PlayerWait();
        Assert.True(g.SlamTimer == 2 && g.SlamX == g.Hero.X && g.SlamY == g.Hero.Y);
        Assert.True(g.SlamCell(g.Hero.X, g.Hero.Y) && g.SlamCell(g.Hero.X + 1, g.Hero.Y + 1) && !g.SlamCell(g.Hero.X + 2, g.Hero.Y));
        int bx = g.Enemies[0].X, hp = g.Hero.Hp;
        if (dodge)
        {
            g.PlayerMove(-1, 0);
            g.PlayerMove(-1, 0);
        }
        else
        {
            g.PlayerWait();
            g.PlayerWait();
        }
        Assert.True(g.SlamTimer == 0 && g.Enemies[0].X <= bx);
        var bet = D.Enemies[D.EnemyIndex("betoniarka")];
        if (dodge) Assert.True(g.Hero.Hp >= hp);
        else Assert.True(g.Hero.Hp <= hp - (bet.MinDamage + D.SlamDamageBonus - (g.CDef.Defense + g.DefBonus) / 2));
    }

    [Fact]
    public void InspekcjaCrossSlamAndSummonLimit()
    {
        var ii = D.EnemyIndex("inspekcja");
        var id = D.Enemies[ii];
        Assert.True(id.Shape == SlamShape.Cross && id.Summon == D.EnemyIndex("papierologia") && id.SummonMax > 0 && id.RewardCash > 0);
        var g = TestData.Arena(1);
        g.Spawn(ii, 10, 7);
        g.Boss = 0;
        g.Enemies[0].Awake = true;
        for (var k = 0; k < id.SummonMax; ++k)
        {
            g.Spawn(id.Summon, 10, 7);
            g.Enemies[g.EnemiesCount - 1].Alive = false;
        }
        for (var k = 0; k < 12 && g.SlamTimer == 0; ++k) g.PlayerWait();
        Assert.True(g.SlamTimer == D.SlamCrossDelay && g.SlamX == g.Hero.X && g.SlamY == g.Hero.Y);
        Assert.True(g.SlamCell(g.Hero.X + D.SlamCrossReach, g.Hero.Y) && g.SlamCell(g.Hero.X, g.Hero.Y - D.SlamCrossReach));
        Assert.True(!g.SlamCell(g.Hero.X + 1, g.Hero.Y + 1) && !g.SlamCell(g.Hero.X + D.SlamCrossReach + 1, g.Hero.Y));
        var hp = g.Hero.Hp;
        g.PlayerMove(0, 1); // zejście z krzyża po skosie
        g.PlayerMove(-1, 0);
        g.PlayerWait();
        Assert.True(g.SlamTimer == 0 && g.Hero.Hp >= hp);
        for (var k = 0; k < 60 && g.St == GameStatus.Playing; ++k)
        {
            g.Hero.Hp = g.Hero.MaxHp;
            g.PlayerWait();
        }
        var alive = 0;
        for (var i = 1; i < g.EnemiesCount; ++i)
        {
            alive += g.Enemies[i].Alive ? 1 : 0;
            Assert.Equal(D.EnemyIndex("papierologia"), g.Enemies[i].DefId);
        }
        Assert.True(g.SummonsUsed == id.SummonMax && alive == id.SummonMax);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void InspekcjaStunnedByFullGear(bool gear)
    {
        var ii = D.EnemyIndex("inspekcja");
        var st = Array.FindIndex(D.Stages, s => s.Boss == ii);
        var id = D.Enemies[ii];
        var g = TestData.Run(1, 31);
        g.StartStage(st);
        if (gear)
        {
            for (var i = 0; i < D.GearSlotsCount; ++i) g.Equip(i, 0, 0);
        }
        Assert.Equal(D.Stages[st].EnemyCount + 1 + id.SummonMax, g.EnemiesCount);
        Assert.True(!g.Enemies[g.Boss + 1].Alive && g.StairsX < 0);
        g.Enemies[g.Boss].Awake = true;
        g.Enemies[g.Boss].X = (sbyte)(g.Hero.X + 5);
        g.Enemies[g.Boss].Y = g.Hero.Y;
        g.EnemyAct(g.Boss);
        Assert.True(g.BossWakeDamage >= 0);
        Assert.Equal(gear ? id.GearStun - 1 : 0, g.Enemies[g.Boss].Stun);
    }

    [Fact]
    public void InspekcjaMidActRewardWithoutHurtownia()
    {
        var ii = D.EnemyIndex("inspekcja");
        var st = Array.FindIndex(D.Stages, s => s.Boss == ii);
        var id = D.Enemies[ii];
        Assert.True(st + 1 < D.Stages.Length && D.Stages[st + 1].Act == D.Stages[st].Act);
        var g = TestData.Run(1, 31);
        for (var k = 0; k < st; ++k)
        {
            g.DebugSkip();
            g.ActCleared = false;
            g.NextStage();
        }
        int cash = g.Cash, actKills = g.ActKills;
        g.DebugSkip();
        Assert.True(g.St == GameStatus.StageClear && !g.ActCleared);
        Assert.Equal(cash + id.RewardCash + id.Score / D.CashPerScore, g.Cash);
        Assert.Equal(actKills + 1, g.ActKills);
        g.NextStage();
        Assert.True(g.Stage == st + 1 && g.StairsX >= 0);
    }

    [Fact]
    public void HurtowniaPricesAndEffects()
    {
        var g = TestData.Arena(1);
        Assert.True(!g.HurtowniaBuy(0) && g.Cash == 0);
        g.Cash = 1000;
        for (var i = 0; i < D.Hurtownia.Length; ++i)
        {
            var it = D.Hurtownia[i];
            int cash = g.Cash, maxhp = g.Hero.MaxHp;
            g.Hero.Hp = 3;
            g.AbilityCd = 9;
            Assert.True(g.HurtowniaBuy(i) && g.Cash == cash - it.Price);
            switch (it.Effect)
            {
                case ShopEffect.Heal: Assert.Equal(g.Hero.MaxHp, g.Hero.Hp); break;
                case ShopEffect.MaxHp: Assert.Equal(maxhp + 3, g.Hero.MaxHp); break;
                case ShopEffect.Ability: Assert.Equal(0, g.AbilityCd); break;
                case ShopEffect.Tool: Assert.True(g.WeaponOverride >= 0); break;
                case ShopEffect.Gear: Assert.Contains(g.Equipped.Take(D.GearSlotsCount), e => e >= 1); break;
            }
        }
    }
}
