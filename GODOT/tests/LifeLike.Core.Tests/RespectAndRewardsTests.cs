namespace LifeLike.Core.Tests;

// core_tests.cpp: 42 (Respekt) i 43 (nagrody za odbiór, profil v7 -> v8).
public class RespectAndRewardsTests
{
    private static GameData D => TestData.D;

    private static int RespectOf(RespectEffect e) => Array.FindIndex(D.Respect, x => x.Effect == e);

    [Fact]
    public void RespectEarnedPerStageBankedAndKept()
    {
        var g = TestData.Run(1, 4242);
        var p = Meta.NewProfile(D);
        Meta.StartRun(D, p);
        Assert.True(g.Respect == 0 && g.StageRespect() == D.RespectStage);
        g.DebugSkip();
        Assert.True(g.St == GameStatus.StageClear && g.Respect == D.RespectStage);
        Meta.CheckBadges(D, p, g);
        Assert.True(p.Respect == D.RespectStage && p.RespectTotal == D.RespectStage && p.RunRespect == D.RespectStage);
        Meta.CheckBadges(D, p, g);
        Assert.Equal(D.RespectStage, p.Respect); // drugi raz nie dolicza
        var expect = D.RespectStage;
        while (g.St == GameStatus.StageClear)
        {
            g.NextStage();
            var want = g.StageRespect();
            var sd = D.Stages[g.Stage];
            if (g.Stage == D.Stages.Length - 1) Assert.Equal(D.RespectFinal, want);
            else if (sd.Boss >= 0) Assert.Equal(D.Stages[g.Stage + 1].Act == sd.Act ? D.RespectBoss : D.RespectActBoss, want);
            else Assert.Equal(D.RespectStage, want);
            g.Hero.Hp = g.Hero.MaxHp = 999;
            g.DebugSkip();
            expect += want;
            Meta.CheckBadges(D, p, g);
            Assert.True(g.Respect == expect && p.Respect == expect);
        }
        Assert.True(g.St == GameStatus.Won && expect > 10 * D.RespectStage);
        var e = TestData.Run(1, 1, 0);
        Assert.Equal(Math.Max(1, D.RespectStage * D.Difficulties[0].ScorePct / 100), e.StageRespect()); // Łatwy mniej
        // śmierć: Respekt z ukończonych etapów zostaje w profilu, nowa budowa liczy od zera
        var q = Meta.NewProfile(D);
        Meta.StartRun(D, q);
        var d = TestData.Run(0, 77);
        d.DebugSkip();
        Meta.CheckBadges(D, q, d);
        d.NextStage();
        d.Hero.Hp = 1;
        d.HeroDown();
        Assert.Equal(GameStatus.Dead, d.St);
        Meta.CheckBadges(D, q, d);
        Assert.Equal(D.RespectStage, q.Respect);
        Meta.StartRun(D, q);
        Assert.Equal(0, q.RunRespect);
        var d2 = TestData.Run(0, 78);
        d2.DebugSkip();
        Meta.CheckBadges(D, q, d2);
        Assert.Equal(2 * D.RespectStage, q.Respect);
    }

    [Fact]
    public void RespectShopRanksAndMods()
    {
        var p = Meta.NewProfile(D);
        var s = Meta.NewProfile(D);
        Assert.True(Meta.RespectCost(D, s, 0) == D.Respect[0].Costs[0] && !Meta.BuyRespect(D, s, 0));
        s.Respect = 60000;
        s.Secrets = (ushort)((1 << D.Secrets.Length) - 1); // Respekt z sekretnego zlecenia (Zaprawiony w boju) odblokowany
        var spent = 0;
        for (var i = 0; i < D.Respect.Length; ++i)
        {
            for (var r = 0; r < D.Respect[i].Ranks; ++r)
            {
                if (r > 0) Assert.True(D.Respect[i].Costs[r] > D.Respect[i].Costs[r - 1]);
                Assert.True(Meta.RespectCost(D, s, i) == D.Respect[i].Costs[r] && Meta.BuyRespect(D, s, i));
                spent += D.Respect[i].Costs[r];
                Assert.Equal(D.Respect[i].Values[r], Meta.RespectValue(D, s, i));
            }
            Assert.True(Meta.RespectCost(D, s, i) == -1 && !Meta.BuyRespect(D, s, i));
        }
        Assert.True(s.Respect == 60000 - spent && Meta.RespectSpent(D, s) == Meta.RespectTotalCost(D) && spent == Meta.RespectTotalCost(D));
        Assert.True(Meta.RespectTotalCost(D) > 30 * (2 * D.RespectStage + D.RespectFinal)); // pełny Respekt = wiele budów
        RunMods m = Meta.Mods(D, s), m0 = Meta.Mods(D, p);
        int dmg = RespectOf(RespectEffect.DmgPct), tak = RespectOf(RespectEffect.TakenPct), gpc = RespectOf(RespectEffect.GearPct);
        int mat = RespectOf(RespectEffect.MatsPct), sec = RespectOf(RespectEffect.SecondChance);
        Assert.True(dmg >= 0 && tak >= 0 && gpc >= 0 && mat >= 0 && sec >= 0);
        Assert.True(m.DmgPct == m0.DmgPct + Meta.RespectValue(D, s, dmg) && m.TakenPct == m0.TakenPct + Meta.RespectValue(D, s, tak));
        Assert.True(m.SecondChance > 0 && m.GearPct == Meta.RespectValue(D, s, gpc) && m.MatsPct == Meta.RespectValue(D, s, mat));
        Assert.True(Daily.Mods(D, 1).DmgPct == 0 && Daily.Mods(D, 1).SecondChance == 0); // budowa dnia bez Respektu
    }

    [Fact]
    public void PercentBonusesAndSecondChance()
    {
        // obrażenia +%: średnio dokładnie (reszta przenoszona), bez losowania
        var a = TestData.Arena(1);
        var b = TestData.Arena(1);
        b.Bonus.DmgPct = 25;
        long sa = 0, sb = 0;
        var budzet = D.EnemyIndex("budzet");
        for (var k = 0; k < 400; ++k)
        {
            a.R.Seed((uint)(900 + k));
            b.R.Seed((uint)(900 + k));
            a.Spawn(budzet, 8, 7);
            b.Spawn(budzet, 8, 7);
            a.Enemies[a.EnemiesCount - 1].Hp = b.Enemies[b.EnemiesCount - 1].Hp = 999;
            a.HeroAttack(a.EnemiesCount - 1);
            b.HeroAttack(b.EnemiesCount - 1);
            sa += 999 - a.Enemies[a.EnemiesCount - 1].Hp;
            sb += 999 - b.Enemies[b.EnemiesCount - 1].Hp;
            a.EnemiesCount = b.EnemiesCount = 0;
        }
        Assert.True(sb * 100 >= sa * 122 && sb * 100 <= sa * 128, $"{sa} {sb}");
        // otrzymane obrażenia -%: mniej, ale zawsze co najmniej 1
        var ta0 = TestData.Run(1, 5);
        var tb0 = TestData.Run(1, 5);
        tb0.Bonus.TakenPct = 30;
        long ta = 0, tb = 0;
        for (var k = 0; k < 300; ++k)
        {
            var d0 = 1 + k % 7;
            ta += ta0.TakenDamage(d0);
            tb += tb0.TakenDamage(d0);
            Assert.True(tb0.TakenDamage(1) >= 1);
        }
        Assert.True(tb * 100 >= ta * 66 && tb * 100 <= ta * 80, $"{ta} {tb}");
        // kawa +%, unik z limitem, Druga szansa raz na budowę
        var c = TestData.Run(1, 5);
        c.Bonus.CoffeePct = 50;
        Assert.Equal(Pct.DivRound(D.CoffeeHeal * 150, 100), c.CoffeeHeal());
        var u = TestData.Run(5, 5);
        u.Bonus.Dodge = 90;
        Assert.Equal(D.DodgeMaxPct, u.DodgePct());
        var z = TestData.Run(1, 5);
        z.Bonus.SecondChance = 1;
        z.Hero.Hp = 0;
        z.HeroDown();
        Assert.True(z.St == GameStatus.Playing && z.Hero.Hp == 1 && z.Hero.Alive && z.SecondUsed);
        z.Hero.Hp = 0;
        z.HeroDown();
        Assert.True(z.St == GameStatus.Dead && !z.Hero.Alive);
        var n = TestData.Run(1, 5);
        n.Hero.Hp = 0;
        n.HeroDown();
        Assert.Equal(GameStatus.Dead, n.St);
        // brygada i Hurtownia taniej
        var h = TestData.Run(1, 5);
        h.Bonus.BrigadePct = 30;
        h.Bonus.ShopPct = 25;
        Assert.Equal(D.Brigade[0].Price * 70 / 100, h.HelperPrice(0));
        var first = -1;
        for (var i = 0; i < D.Hurtownia.Length; ++i)
        {
            if (D.Hurtownia[i].Effect == ShopEffect.Upgrade)
            {
                Assert.Equal(D.ToolLevels[0].Cash * 75 / 100, h.HurtowniaPrice(i));
                continue;
            }
            Assert.Equal(D.Hurtownia[i].Price * 75 / 100, h.HurtowniaPrice(i));
            if (first < 0 && D.Hurtownia[i].Material < 0) first = i;
        }
        h.Cash = h.HurtowniaPrice(first);
        Assert.True(h.HurtowniaCan(first) && h.HurtowniaBuy(first) && h.Cash == 0);
    }

    [Fact]
    public void EachWinUnlocksNextReward()
    {
        var p = Meta.NewProfile(D);
        var avail = Meta.RewardsAvailable(D);
        Assert.True(avail > 0 && avail <= D.Rewards.Length && p.Rewards == 0);
        for (var i = 0; i < D.Classes.Length; ++i)
        {
            if (Meta.ClassReward(D, i)) Assert.True(!Meta.ClassUnlocked(D, p, i) && !Meta.BuyClass(D, p, i));
        }
        for (var i = 0; i < D.Tools.Length; ++i)
        {
            if (D.Tools[i].Reward) Assert.False(Meta.ToolUnlocked(D, p, i));
        }
        Assert.True(Meta.GearSlotsMask(D, p) == D.GearBaseMask && Meta.Mods(D, p).GearSlots == D.GearBaseMask);
        Assert.True(Meta.RewardWin(D, p, 0) == 1 && Meta.RewardWin(D, p, 2) == 3);
        p.Xp = 1 << 20;
        for (var i = 0; i < D.Tools.Length; ++i)
        {
            if (D.Tools[i].Reward) Assert.False(Meta.BuyTool(D, p, i));
        }
        for (var k = 0; k < avail; ++k)
        {
            Assert.True(Meta.RecordWin(D, p) == k && p.Wins == k + 1 && Meta.RewardOwned(p, k) && Meta.RewardWin(D, p, k) == -1);
            var rd = D.Rewards[k];
            if (rd.Kind == RewardKind.Cls) Assert.True(Meta.ClassUnlocked(D, p, rd.Index));
            if (rd.Kind == RewardKind.Tool) Assert.True(Meta.ToolUnlocked(D, p, rd.Index) && ((Meta.Mods(D, p).Tools >> rd.Index) & 1) != 0);
            if (rd.Kind == RewardKind.Gear) Assert.True(((Meta.GearSlotsMask(D, p) >> rd.Index) & 1) != 0);
        }
        Assert.True(Meta.RecordWin(D, p) == -1 && p.Rewards == avail); // „wkrótce” się nie odblokowuje
        for (var i = 0; i < D.Classes.Length; ++i)
        {
            Assert.Equal((Meta.ClassReward(D, i) && !Meta.ClassSecret(D, i)) || ((p.Classes >> i) & 1) != 0, Meta.ClassUnlocked(D, p, i));
        }
        // wygrane i stawki zawodów 8+
        var w = Meta.NewProfile(D);
        for (var c = 0; c < D.Classes.Length; ++c)
        {
            Assert.False(Meta.ClassWon(w, c));
            Meta.SetClassWon(w, c);
            Assert.True(Meta.ClassWon(w, c));
            Meta.SetBestStake(w, c, c + 1);
        }
        Assert.Equal(D.Classes.Length, Meta.ClassesWon(D, w));
        for (var c = 0; c < D.Classes.Length; ++c) Assert.Equal(c + 1, Meta.BestStake(w, c));
        var wg = TestData.Run(D.Classes.Length - 1, 3);
        wg.St = GameStatus.Won;
        var w2 = Meta.NewProfile(D);
        Meta.RecordRun(D, w2, wg);
        Assert.True(Meta.ClassWon(w2, D.Classes.Length - 1) && w2.ClassWins == 0);
    }

    [Fact]
    public void ProfileV7MigratesToV8()
    {
        var avail = Meta.RewardsAvailable(D);
        var v7 = Meta.NewProfile(D);
        v7.Magic = Profile.MagicBytes(Profile.MagicV7);
        v7.Wins = 3;
        v7.Xp = 11;
        v7.Best = 777;
        v7.DailyScore[4] = 55;
        var refund = 0;
        for (var i = 0; i < D.Upgrades.Length; ++i) // v13: Szkolenia wracają jako doświadczenie po starej cenie
        {
            v7.Levels[i] = 1;
            refund += D.Upgrades[i].LegacyCosts[0];
        }
        Assert.True(refund > 0);
        var b = v7.ToBytes();
        for (var i = Profile.V7Size; i < b.Length; ++i) b[i] = 0xEE;
        var p = Profile.FromBytes(b);
        Assert.True(Meta.ProfileFix(D, p) && p.MagicIs(Profile.MagicCurrent));
        Assert.True(p.Best == 777 && p.Wins == 3 && p.DailyScore[4] == 55 && p.Rewards == Math.Min(3, avail) && p.Xp == 11 + refund);
        var r7 = TestData.InspRespect(TestData.InspMigrated(0, 3, 0)); // v0.21.52 cz. b: Respekt z poziomów inspektora (migracja v14)
        Assert.True(p.Respect == r7 && p.RespectTotal == r7 && p.ClassWinsHi == 0 && p.RespectRanks[0] == 0 && p.BestStakeHi[0] == 0);
        for (var i = 0; i < D.Upgrades.Length; ++i) Assert.Equal(0, p.Levels[i]);
        Assert.False(Meta.ProfileFix(D, p));
        var v1 = Profile.FromBytes(new byte[Profile.Size]);
        v1.Magic = Profile.MagicBytes(Profile.MagicV1);
        v1.Wins = 99;
        Assert.True(Meta.ProfileFix(D, v1) && v1.Wins == 99 && v1.Rewards == avail);
    }
}
