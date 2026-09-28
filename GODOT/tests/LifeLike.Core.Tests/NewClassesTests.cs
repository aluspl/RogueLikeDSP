namespace LifeLike.Core.Tests;

// core_tests.cpp: 44 (nowe zawody: Rynna, Narzut, Taran, cechy) i 45 (sprzęt z nagród: buty, pas, Bez poślizgu).
public class NewClassesTests
{
    private static GameData D => TestData.D;

    private static int ClassOf(AbilityEffect e) => Array.FindIndex(D.Classes, c => c.Ability == e);

    private static int Budzet => D.EnemyIndex("budzet");

    [Fact]
    public void RooferLineHitsWholeLineAndIgnoresWind()
    {
        var roofer = ClassOf(AbilityEffect.Line);
        Assert.True(roofer >= 0 && D.Classes[roofer].Passive == ClassPassive.Windproof);
        var g = TestData.Arena(roofer);
        g.Spawn(Budzet, 9, 7);
        g.Spawn(Budzet, 11, 7);
        g.Spawn(Budzet, 7, 10);
        for (var i = 0; i < 3; ++i) g.Enemies[i].Hp = 99;
        g.UpdateFov();
        Assert.True(g.PlayerAbility());
        Assert.True(g.Enemies[0].Hp < 99 && g.Enemies[1].Hp < 99 && g.Enemies[2].Hp == 99 && g.AbilityCd > 0);
        var w = TestData.Arena(roofer);
        w.Lv[10, 7] = Tile.Wall;
        w.Spawn(Budzet, 9, 7);
        w.Spawn(Budzet, 11, 7);
        w.Enemies[0].Hp = w.Enemies[1].Hp = 99;
        w.UpdateFov();
        Assert.True(w.PlayerAbility() && w.Enemies[0].Hp < 99 && w.Enemies[1].Hp == 99);
        var n = TestData.Arena(roofer);
        Assert.True(!n.PlayerAbility() && n.Turns == 0); // bez celu nic
        var wd = TestData.Arena(roofer);
        var rg = wd.WeaponRange();
        wd.Weather = (sbyte)Array.FindIndex(D.Weather, x => x.Effect == WeatherEffect.Wind);
        Assert.True(wd.WeaponRange() == rg && rg > 1);
    }

    [Fact]
    public void PlastererSplashHitsAreaAroundTarget()
    {
        var plaster = ClassOf(AbilityEffect.Splash);
        Assert.True(plaster >= 0);
        var g = TestData.Arena(plaster);
        g.Spawn(Budzet, 9, 7);
        g.Spawn(Budzet, 10, 8);
        g.Spawn(Budzet, 12, 7);
        for (var i = 0; i < 3; ++i) g.Enemies[i].Hp = 99;
        g.UpdateFov();
        Assert.True(g.PlayerAbility() && g.Enemies[0].Hp < 99 && g.Enemies[1].Hp < 99 && g.Enemies[2].Hp == 99);
        var r2 = TestData.Arena(plaster);
        r2.HeroLevel = 3;
        r2.Spawn(Budzet, 8, 7);
        r2.Enemies[0].Hp = 99;
        r2.UpdateFov();
        Assert.True(r2.PlayerAbility() && r2.Enemies[0].Stun >= 0 && r2.Enemies[0].Hp < 99);
    }

    [Fact]
    public void DiggerRamChargesAndPushes()
    {
        var digger = ClassOf(AbilityEffect.Ram);
        Assert.True(digger >= 0 && D.Classes[digger].Passive == ClassPassive.Push);
        var g = TestData.Arena(digger);
        g.Spawn(Budzet, 10, 7);
        g.Enemies[0].Hp = 99;
        g.UpdateFov();
        Assert.True(g.PlayerAbility());
        Assert.True(g.Hero.X == 9 && g.Hero.Y == 7 && g.Enemies[0].Hp < 99 && g.Enemies[0].X == 12);
        var b = TestData.Arena(digger);
        b.Spawn(Budzet, 7, 3);
        b.Enemies[0].Hp = 99;
        b.UpdateFov();
        Assert.True(b.PlayerAbility() && b.Hero.X == 7 && b.Hero.Y == 4 && b.Enemies[0].Hp == 99); // za daleko: tylko szarża
        var c = TestData.Arena(digger);
        c.Spawn(Budzet, 7, 4);
        c.Enemies[0].Hp = 99;
        c.UpdateFov();
        Assert.True(c.PlayerAbility() && c.Hero.X == 7 && c.Hero.Y == 5 && c.Enemies[0].Y == 2 && c.Enemies[0].Hp < 99); // pionowo
        // cios wręcz czasem odpycha (inne zawody nie)
        var pushed = 0;
        for (uint seed = 1; seed <= 200; ++seed)
        {
            var a = TestData.Arena(digger);
            a.R.Seed(seed);
            a.Spawn(Budzet, 8, 7);
            a.Enemies[0].Hp = 99;
            a.HeroAttack(0);
            if (a.Enemies[0].X == 9) pushed++;
            Assert.True(a.Enemies[0].X is 8 or 9);
        }
        Assert.True(pushed > 200 * D.PushChancePct / 200 && pushed < 200 * D.PushChancePct * 2 / 100, $"{pushed}");
        var o = TestData.Arena(1);
        o.Spawn(Budzet, 8, 7);
        o.Enemies[0].Hp = 99;
        for (var k = 0; k < 30; ++k) o.HeroAttack(0);
        Assert.Equal(8, o.Enemies[0].X);
    }

    [Fact]
    public void RewardGearBootsBeltAndSlipResistance()
    {
        int boots = -1, belt = -1;
        for (var i = 0; i < D.GearSlotsCount; ++i)
        {
            if (D.Gear[i * 3].Stat == GearStat.Dodge) boots = i;
            if (D.Gear[i * 3].Stat == GearStat.Thermos) belt = i;
        }
        var slip = Array.FindIndex(D.GearTraits, t => t.Effect == TraitEffect.SlipRes);
        Assert.True(boots >= 0 && belt >= 0 && slip >= 0 && ((D.GearRewardMask >> boots) & 1) != 0 && ((D.GearRewardMask >> belt) & 1) != 0);
        var g = TestData.Arena(1);
        int d0 = g.DodgePct(), cap0 = g.ThermosCap();
        var plain = Array.FindIndex(D.GearTraits, t => t.Effect == TraitEffect.Sight); // cecha bez wpływu na unik
        g.Equip(boots, 2, plain);
        Assert.Equal(Math.Min(D.DodgeMaxPct, d0 + D.Gear[boots * 3 + 2].Value), g.DodgePct());
        g.Equip(belt, 2, plain);
        Assert.Equal(cap0 + D.Gear[belt * 3 + 2].Value, g.ThermosCap());
        g.Thermos = g.ThermosCap();
        g.Equip(belt, 0, plain);
        Assert.Equal(g.ThermosCap(), g.Thermos); // słabszy pas: kawy ponad limit przepadają
        g.Equip(0, 0, slip);
        g.ApplyStatus(StatusEffect.Slip, 3);
        Assert.Equal(0, g.StatusTurns(StatusEffect.Slip));
        var f = TestData.Arena(1);
        for (var i = 0; i < D.GearSlotsCount; ++i)
        {
            if (((D.GearBaseMask >> i) & 1) != 0) f.Equip(i, 0, 0);
        }
        Assert.True(f.FullGear());
        // dropy: bez nagród tylko sloty bazowe, z nagrodami też buty i pas
        int seen0 = 0, seen1 = 0;
        for (uint seed = 1; seed <= 300; ++seed)
        {
            var a = TestData.Arena(1);
            a.R.Seed(seed);
            seen0 |= 1 << a.RandomSlot();
            var b = TestData.Arena(1);
            b.Bonus.GearSlots = (1 << D.GearSlotsCount) - 1;
            b.R.Seed(seed);
            seen1 |= 1 << b.RandomSlot();
            var c = TestData.Arena(1);
            c.R.Seed(seed);
            var r2 = new Rng();
            r2.Seed(seed);
            Assert.Equal(r2.Range(0, 2), c.RandomSlot()); // 3 sloty: to samo losowanie co dawniej
        }
        Assert.True(seen0 == D.GearBaseMask && seen1 == (1 << D.GearSlotsCount) - 1);
    }
}
