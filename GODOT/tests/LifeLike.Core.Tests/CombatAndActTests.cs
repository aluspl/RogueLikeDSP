namespace LifeLike.Core.Tests;

// core_tests.cpp: 2 i 22 (dziennik), 13 (trafienia), 23 (celowanie), 24 (akty), 25 (uderzenie bossa), 26 (Hurtownia).
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
