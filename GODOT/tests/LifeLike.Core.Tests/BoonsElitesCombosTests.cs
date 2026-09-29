namespace LifeLike.Core.Tests;

/// <summary>
/// core_tests.cpp: 47 (v0.21.50 cz. 2) – premie po etapie, elity, kombinacje stanów, rozpiska z premiami i Tarczą.
/// </summary>
public class BoonsElitesCombosTests
{
    private static GameData D => TestData.D;

    private static int BoonIdx(string n) => Array.FindIndex(D.Boons, b => b.Name == n);

    private static int SynIdx(string n) => Array.FindIndex(D.Synergies, s => s.Name == n);

    private static int Trait(EliteEffect e) => Array.FindIndex(D.Elites, t => t.Effect == e);

    private static void Give(Game g, string n)
    {
        var b = BoonIdx(n);
        Assert.True(b >= 0, n);
        g.BoonOffer[0] = (sbyte)b;
        Assert.True(g.PickBoon(0));
    }

    [Fact]
    public void OfferIsDeterministicAndLeavesGameRngUntouched()
    {
        var differ = 0;
        for (uint seed = 1; seed <= 60; ++seed)
        {
            var cls = (int)(seed % (uint)D.Classes.Length);
            var a = TestData.Run(cls, seed);
            a.DebugSkip();
            var b = TestData.Run(cls, seed);
            b.DebugSkip();
            var c = TestData.Run(cls, seed + 1000);
            c.DebugSkip();
            Assert.Equal(GameStatus.StageClear, a.St);
            Assert.True(a.HasBoonOffer);
            Assert.Equal(a.BoonOffer, b.BoonOffer);
            Assert.Equal(a.R.S, b.R.S);
            if (!a.BoonOffer.SequenceEqual(c.BoonOffer)) differ++;
            for (var k = 0; k < 3; ++k)
            {
                int bo = a.BoonOffer[k];
                Assert.True(bo >= 0 && (D.Boons[bo].Cls < 0 || D.Boons[bo].Cls == a.Cls) && !a.HasBoon(bo));
                for (var j = 0; j < k; ++j) Assert.NotEqual(a.BoonOffer[j], a.BoonOffer[k]);
            }
            var rs = a.R.S;
            var p = a.Clone();
            p.PickBoon(0);
            Assert.True(p.R.S == rs && !p.HasBoonOffer && p.HasBoon(a.BoonOffer[0]) && p.BoonsOwned() == 1);
            var n = a.Clone();
            n.NextStage();
            Assert.True(!n.HasBoonOffer && n.Boons == 0);
        }
        Assert.True(differ >= 50);
    }

    [Fact]
    public void LuckRaisesRarity()
    {
        var cnt = new int[2, 3];
        for (var lk = 0; lk < 2; ++lk)
        {
            for (uint seed = 1; seed <= 400; ++seed)
            {
                var g = TestData.Run(1, seed * 13);
                if (lk == 1) g.Bonus.Luck = 5;
                g.DebugSkip();
                for (var k = 0; k < 3; ++k) cnt[lk, D.Boons[g.BoonOffer[k]].Rarity]++;
            }
        }
        Assert.True(cnt[0, 0] > cnt[0, 1] && cnt[0, 1] > cnt[0, 2] && cnt[0, 2] > 0);
        Assert.True(cnt[1, 1] + cnt[1, 2] > cnt[0, 1] + cnt[0, 2] && cnt[1, 2] > cnt[0, 2]);
    }

    [Fact]
    public void RerollOncePaidAndFreeFromRespect()
    {
        var g = TestData.Run(2, 77);
        g.DebugSkip();
        var before = g.BoonOffer.ToArray();
        g.Cash = D.BoonRerollCost - 1;
        Assert.False(g.CanReroll());
        Assert.False(g.RerollBoons());
        g.Cash = D.BoonRerollCost + 5;
        Assert.Equal(D.BoonRerollCost, g.RerollPrice());
        Assert.True(g.RerollBoons());
        Assert.True(g.Cash == 5 && g.RerollsLeft() == 0 && !g.RerollBoons() && !before.SequenceEqual(g.BoonOffer) && g.HasBoonOffer);

        var m = RunMods.Default(D);
        m.Rerolls = 1;
        var f = new Game(D);
        f.NewRun(2, 77, D.DefaultDifficulty, m);
        f.DebugSkip();
        f.Cash = 0;
        Assert.True(f.RerollPrice() == 0 && f.RerollBoons() && f.Cash == 0 && f.RerollsLeft() == 1);
        f.Cash = 100;
        Assert.True(f.RerollPrice() == D.BoonRerollCost && f.RerollBoons() && f.Cash == 100 - D.BoonRerollCost);

        var p = Meta.NewProfile(D);
        for (var i = 0; i < D.Respect.Length; ++i) Meta.SetRespectRank(p, i, D.Respect[i].Ranks);
        Assert.Equal(1, Meta.Mods(D, p).Rerolls);
    }

    [Fact]
    public void BoonEffectsSynergiesAndBreakdownMatchCombat()
    {
        var g = TestData.Arena(1);
        int hp = g.Hero.MaxHp, cash = g.Cash, def = g.HeroDefense(), cool = g.AbilityCooldown(), cap = g.ThermosCap();
        int Val(string n) => D.Boons[BoonIdx(n)].Value;
        Give(g, "Płyta warstwowa");
        Assert.Equal(hp + Val("Płyta warstwowa"), g.Hero.MaxHp);
        Give(g, "Premia od inwestora");
        Assert.Equal(cash + g.Income(Val("Premia od inwestora")), g.Cash);
        Give(g, "Paleta materiałów");
        for (var i = 0; i < D.Materials.Length; ++i) Assert.Equal(Val("Paleta materiałów"), g.Mats[i]);
        var zb = SynIdx("Zbrojenie");
        Give(g, "Beton B30");
        Assert.True(g.HeroDefense() == def + Val("Beton B30") && !g.SynergyActive(zb));
        Give(g, "Druga zmiana");
        Assert.Equal(Math.Max(3, cool - Val("Druga zmiana")), g.AbilityCooldown());
        Give(g, "Termos z bufetu");
        Assert.True(g.ThermosCap() == cap + 1 && g.Thermos == 1);
        var b0 = g.WeaponBreakdown();
        Give(g, "Hartowana kielnia");
        Give(g, "Zbrojona rękawica");
        Give(g, "Hydrofor");
        Give(g, "Szczęśliwa moneta");
        var b1 = g.WeaponBreakdown();
        int fb = Val("Hartowana kielnia") + Val("Zbrojona rękawica"), pb = Val("Hydrofor"), cb = Val("Szczęśliwa moneta");
        Assert.True(b1.FlatBoon == fb && b1.PctBoon == pb && b1.CritBoon == cb && b1.CritPct == g.CritPct() && b1.Min > b0.Min);
        Assert.Equal($"Premie etapów: +{fb}, +{pb}%, kryt +{cb}%", DamageHelp.Text(D, b1, DmgText.Boon));
        Assert.True(zb >= 0 && g.SynergyActive(zb) && g.TagCount(2) == 2);
        Assert.Equal(def + Val("Beton B30") + 2 * D.Synergies[zb].Value, g.HeroDefense());

        var kornik = D.EnemyIndex("kornik");
        for (var t = 0; t < D.Elites.Length; ++t)
        {
            g.EnemiesCount = 0;
            g.Spawn(kornik, 8, 7);
            g.MakeElite(0, t);
            g.Enemies[0].Hp = g.Enemies[0].MaxHp = 30000;
            var be = g.ActorBreakdown(0);
            Assert.True(be.EnemyElite == g.EnemyEliteDef(0) && be.DefCut == g.EnemyDefense(0) / 2);
            int lo = 999, hi = 0;
            for (var k = 0; k < 1500; ++k)
            {
                g.DmgCarry = k % 100;
                g.HitsCount = 0;
                g.HeroAttack(0);
                if (g.Hits[0].Kind == HitKind.Crit) continue;
                lo = Math.Min(lo, g.Hits[0].Amount);
                hi = Math.Max(hi, g.Hits[0].Amount);
            }
            Assert.True(lo == be.Min && hi == be.Max, $"elita {t}: {lo}-{hi} vs {be.Min}-{be.Max}");
            var h = g.EnemyHit(0);
            var h0 = DamageHelp.EnemyHitRange(1, 3, g.EnemyDmgBonus() + D.EliteDmg, g.HeroDefense(), 0);
            Assert.True(h.Min == h0.Min && h.Max == h0.Max);
            Assert.StartsWith(D.Elites[t].Prefix[0] + " ", g.EnemyName(0));
        }
    }

    [Fact]
    public void SynergiesConductSafetyEspressoAndClassBoons()
    {
        var g = TestData.Arena(1);
        var pr = SynIdx("Przepięcie");
        Give(g, "Wąż ogrodowy");
        Assert.True(!g.SynergyActive(pr) && !g.HitPower());
        Give(g, "Przedłużacz");
        Assert.True(g.SynergyActive(pr) && g.HitPower());
        Assert.Contains("Synergia: Przepięcie", g.Log[Game.LogLines - 1].Text);
        var bhp = SynIdx("Pełne BHP");
        Give(g, "Szelki asekuracyjne");
        Give(g, "Kask z latarką");
        Assert.True(g.SynergyActive(bhp));
        g.ApplyStatus(StatusEffect.Shock, 1);
        g.ApplyStatus(StatusEffect.Poison, 3);
        Assert.True(g.StatusTurns(StatusEffect.Shock) == 0 && g.StatusTurns(StatusEffect.Poison) == 0);

        var e = TestData.Arena(1);
        Give(e, "Podwójne espresso");
        Give(e, "Termos z bufetu");
        e.AbilityCd = 10;
        e.Hero.Hp = 5;
        e.Thermos = 1;
        Assert.True(e.PlayerDrink() && e.AbilityCd <= 10 - 3);

        for (var b = 0; b < D.Boons.Length; ++b)
        {
            if (D.Boons[b].Cls < 0) continue;
            var q = TestData.Run(D.Boons[b].Cls == 0 ? 1 : 0, 3);
            Assert.False(q.BoonAvailable(b));
        }
    }

    [Fact]
    public void ElitesChanceTraitsAndReward()
    {
        var easy = TestData.Run(0, 9, 0);
        var hard = TestData.Run(0, 9, 2);
        Assert.True(easy.EliteChance() < hard.EliteChance());
        var l = TestData.Run(0, 9);
        var c1 = l.EliteChance();
        l.StartStage(D.Stages.Length - 2);
        Assert.True(l.EliteChance() > c1);
        int elites = 0, all = 0;
        for (uint seed = 1; seed <= 300; ++seed)
        {
            var g = TestData.Run(0, seed);
            g.StartStage(TestData.F0 + 8);
            for (var i = 0; i < g.EnemiesCount; ++i)
            {
                if (i == g.Boss || !g.Enemies[i].Alive) continue;
                all++;
                if (g.Enemies[i].Elite >= 0) elites++;
            }
        }
        Assert.True(elites > 0 && Math.Abs(elites * 100 / all - l.EliteChance()) <= 6, $"elity {elites}/{all}");

        var kornik = D.EnemyIndex("kornik");
        var f = TestData.Arena(1);
        f.Spawn(kornik, 10, 7);
        var baseHp = f.Enemies[0].MaxHp;
        f.MakeElite(0, Trait(EliteEffect.Fast));
        Assert.True(f.Enemies[0].MaxHp == baseHp * D.EliteHpPct / 100 && f.IsElite(0));
        f.Enemies[0].Awake = true;
        f.PlayerWait();
        Assert.Equal(8, f.Enemies[0].X);

        var rg = TestData.Arena(1);
        rg.Spawn(D.EnemyIndex("kamien"), 12, 12);
        rg.MakeElite(0, Trait(EliteEffect.Regen));
        rg.Enemies[0].Awake = true;
        rg.Enemies[0].Hp = 5;
        rg.PlayerWait();
        Assert.Equal(5 + D.Elites[Trait(EliteEffect.Regen)].Value, rg.Enemies[0].Hp);

        var ex = TestData.Arena(1);
        ex.Spawn(kornik, 8, 7);
        ex.MakeElite(0, Trait(EliteEffect.Explode));
        ex.Enemies[0].Hp = 1;
        ex.HeroAttack(0);
        Assert.True(ex.BlastTimer > 0 && ex.Respect == D.EliteRespect);
        var box = false;
        for (var i = 0; i < ex.PickupsCount; ++i)
        {
            var p = ex.Pickups[i];
            box |= p.X == 8 && p.Y == 7 && (p.Type != PickupType.GearBox || p.Arg % 3 >= D.EliteGearMin);
        }
        Assert.True(box);

        var sm = TestData.Arena(1);
        sm.Spawn(kornik, 8, 7);
        sm.MakeElite(0, Trait(EliteEffect.Summon));
        sm.Enemies[0].Hp = 3;
        sm.DamageEnemy(0, 1, false, "t");
        Assert.True(sm.EnemiesCount == 2 && sm.Enemies[1].Alive && (sm.Enemies[0].Flags & ActorFlag.Called) != 0);
        sm.DamageEnemy(0, 1, false, "t");
        Assert.Equal(2, sm.EnemiesCount);
    }

    [Fact]
    public void CombosOnEnemiesAndHero()
    {
        var kornik = D.EnemyIndex("kornik");
        var g = TestData.Arena(3); // Elektryk: Próbnik (prąd)
        Assert.True(g.HitPower() && !g.HitSpark());
        g.Spawn(D.EnemyIndex("przeciek"), 8, 7);
        g.Spawn(kornik, 9, 7);
        g.Spawn(kornik, 9, 8);
        for (var i = 0; i < 3; ++i) g.Enemies[i].Hp = g.Enemies[i].MaxHp = 500;
        g.Enemies[1].Wet = 3;
        g.HeroAttack(0);
        Assert.True((g.ComboEvents & 1) != 0);
        Assert.True(g.Enemies[1].Hp == 500 - D.Combos[0].Value && g.Enemies[2].Hp == 500);

        var d = TestData.Arena(5); // Glazurnik: Szlifierka (iskra)
        Assert.True(d.HitSpark());
        d.Spawn(kornik, 8, 7);
        d.Spawn(kornik, 9, 8);
        d.Spawn(kornik, 11, 7);
        for (var i = 0; i < 3; ++i)
        {
            d.Enemies[i].Hp = d.Enemies[i].MaxHp = 500;
            d.Enemies[i].Flags = ActorFlag.Dusty;
        }
        d.HeroAttack(0);
        Assert.True((d.ComboEvents & 2) != 0 && d.Enemies[1].Hp == 500 - D.Combos[1].Value && d.Enemies[2].Hp == 500);
        Assert.True(!d.EnemyDusty(0) && !d.EnemyDusty(1) && d.EnemyDusty(2));

        var f = TestData.Arena(1); // Murarz: wręcz
        f.Spawn(kornik, 8, 7);
        f.Enemies[0].Hp = f.Enemies[0].MaxHp = 500;
        f.Enemies[0].Flags = ActorFlag.Frozen;
        f.HeroAttack(0);
        Assert.True((f.ComboEvents & 4) != 0 && !f.EnemyFrozen(0) && f.HitsCount == 2
                    && f.Hits[1].Amount == Math.Max(1, f.Hits[0].Amount * D.Combos[2].Value / 100));

        var t = TestData.Run(0, 4);
        t.StartStage(TestData.F0 + 8);
        for (var i = 0; i < t.EnemiesCount; ++i)
        {
            if (i != t.Boss && t.Enemies[i].Alive) Assert.True(t.EnemyDusty(i));
        }

        var hh = TestData.Arena(1);
        hh.Spawn(D.EnemyIndex("przeciek"), 8, 7);
        hh.Spawn(D.EnemyIndex("zwarcie"), 6, 7);
        hh.Hero.Hp = hh.Hero.MaxHp = 500;
        for (var k = 0; k < 50 && !hh.HeroWet(); ++k) hh.EnemyStrike(0, false);
        Assert.True(hh.HeroWet());
        var before = hh.Hero.Hp;
        for (var k = 0; k < 50 && hh.Hero.Hp == before; ++k) hh.EnemyStrike(1, false);
        Assert.True((hh.ComboEvents & 8) != 0 && hh.StatusTurns(StatusEffect.Shock) > 0);

        var z = TestData.Arena(4); // Zawór moczy odepchniętych
        z.Spawn(kornik, 8, 7);
        z.Enemies[0].Hp = 99;
        z.Hero.Hp = 1;
        Assert.True(z.PlayerAbility() && z.Enemies[0].Wet > 0);
        var w = TestData.Arena(1);
        Give(w, "Wąż ogrodowy");
        w.Spawn(kornik, 8, 7);
        w.Enemies[0].Hp = 99;
        w.HeroAttack(0);
        Assert.True(w.EnemyWet(0));
    }

    [Fact]
    public void RunSaveKeepsBoonsAndElites()
    {
        var g = TestData.Run(3, 42);
        g.DebugSkip();
        g.PickBoon(1);
        g.Enemies[0].Elite = 2;
        g.Enemies[0].Wet = 3;
        var s = RunSave.Make(g);
        Assert.True(s.Valid(D));
        var l = s.Load(D);
        Assert.True(l.Boons == g.Boons && l.Enemies[0].Elite == 2 && l.Enemies[0].Wet == 3 && StateDigest.Of(l) == StateDigest.Of(g));
    }
}
