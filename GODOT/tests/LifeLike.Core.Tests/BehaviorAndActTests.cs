namespace LifeLike.Core.Tests;

/// <summary>
/// core_tests.cpp: 40 (v0.21.49 cz. 2) – zachowania problemów (każdy znacznik), mechaniki aktów (błoto, porywy, pył),
/// 10 etapów (+2 Aktu 0), profil z katalogiem 16-47 (v9), zapis budowy.
/// </summary>
public class BehaviorAndActTests
{
    private static GameData D => TestData.D;

    private static int DefWith(int tag)
    {
        for (var d = 0; d < D.Enemies.Length; ++d)
        {
            if ((D.Enemies[d].Tags & tag) != 0) return d;
        }
        return -1;
    }

    private static int DefExactly(int tags)
    {
        for (var d = 0; d < D.Enemies.Length; ++d)
        {
            if (D.Enemies[d].Tags == tags) return d;
        }
        return -1;
    }

    private static int Put(Game g, int def, int x, int y)
    {
        g.Spawn(def, x, y);
        g.Enemies[g.EnemiesCount - 1].Awake = true;
        return g.EnemiesCount - 1;
    }

    [Fact]
    public void StagesPoolsAndActsHaveNewContent()
    {
        Assert.True(D.Stages.Length == 12 && D.Enemies.Length <= Game.MaxEnemyTypes);
        foreach (var st in D.Stages)
        {
            var tagged = st.Pool.Count(p => D.Enemies[p].Tags != 0);
            Assert.True(tagged >= 2, st.Name);
        }
        foreach (var a in D.Acts) Assert.NotEqual(ActMechanic.None, a.Mechanic);
        for (var t = 1; t <= Behavior.Returns; t <<= 1) Assert.True(DefWith(t) >= 0, $"znacznik {t}");
    }

    [Fact]
    public void RangedShootsInLineAndWallBlocks()
    {
        var g = TestData.Arena(1);
        g.Stage = TestData.F0 + 4; // akt II (bez błota), porywy nie w tej turze
        g.StageStartTurn = -100;
        var d = DefWith(Behavior.Ranged);
        var i = Put(g, d, 10, 7);
        int hp = g.Hero.Hp;
        g.PlayerWait();
        Assert.True(g.Hero.Hp < hp && g.Enemies[i].X == 10 && (g.ShotEvents & (1u << i)) != 0);
        var h = TestData.Arena(1);
        h.Stage = TestData.F0 + 4;
        var j = Put(h, d, 10, 7);
        h.Lv[9, 7] = Tile.Wall;
        h.Lv[9, 6] = Tile.Wall;
        h.Lv[9, 8] = Tile.Wall;
        hp = h.Hero.Hp;
        h.PlayerWait();
        Assert.True(h.Hero.Hp == hp && (h.ShotEvents & (1u << j)) == 0);
    }

    [Fact]
    public void SplitsIntoTwoWeakerChildren()
    {
        var g = TestData.Arena(1);
        var i = Put(g, DefWith(Behavior.Splits), 8, 7);
        int mx = g.Enemies[i].MaxHp;
        g.Enemies[i].Hp = 1;
        g.HeroAttack(i);
        int kids = 0, kid = -1;
        for (var k = 0; k < g.EnemiesCount; ++k)
        {
            if (!g.Enemies[k].Alive || (g.Enemies[k].Flags & ActorFlag.Child) == 0) continue;
            kids++;
            kid = k;
            Assert.Equal(Math.Max(1, mx * D.BehaviorSplitHpPct / 100), (int)g.Enemies[k].MaxHp);
        }
        Assert.True(kids == 2 && g.Kills == 1);
        var n = g.EnemiesCount;
        g.Enemies[kid].Hp = 1;
        g.HeroAttack(kid);
        Assert.True(g.EnemiesCount == n && g.Kills == 2);
    }

    [Fact]
    public void HealsWoundedNeighbour()
    {
        var g = TestData.Arena(1);
        Put(g, DefWith(Behavior.Heals), 11, 7);
        var j = Put(g, D.EnemyIndex("plesn"), 11, 9);
        g.Enemies[j].Stun = 5;
        g.Enemies[j].Hp = (short)(g.Enemies[j].MaxHp - 5);
        g.PlayerWait();
        Assert.Equal(g.Enemies[j].MaxHp - 5 + D.BehaviorHealValue, (int)g.Enemies[j].Hp);
    }

    [Fact]
    public void ExplodesAfterOneTurnOnDangerCells()
    {
        var d = DefWith(Behavior.Explodes);
        var g = TestData.Arena(1);
        var i = Put(g, d, 8, 7);
        g.Enemies[i].Hp = 1;
        g.HeroAttack(i);
        g.EndTurn();
        Assert.True(g.BlastTimer == 1 && g.DangerCell(7, 7) && g.DangerCell(9, 8) && !g.DangerCell(10, 7));
        int hp = g.Hero.Hp;
        g.PlayerWait();
        Assert.True(g.Hero.Hp < hp && g.BlastTimer == 0);
        var h = TestData.Arena(1);
        var k = Put(h, d, 8, 7);
        h.Enemies[k].Hp = 1;
        h.HeroAttack(k);
        h.EndTurn();
        h.Hero.X = 5;
        hp = h.Hero.Hp;
        h.PlayerWait();
        Assert.Equal(hp, (int)h.Hero.Hp);
    }

    [Fact]
    public void GrowsAndStaysInPlace()
    {
        var g = TestData.Arena(1);
        var d = -1;
        for (var k = 0; k < D.Enemies.Length; ++k)
        {
            if ((D.Enemies[k].Tags & Behavior.Grows) != 0 && (D.Enemies[k].Tags & Behavior.Stationary) != 0) d = k;
        }
        Assert.True(d >= 0);
        var i = Put(g, d, 11, 11);
        int mx = g.Enemies[i].MaxHp;
        for (var t = 0; t < D.BehaviorGrowEvery * 2; ++t) g.PlayerWait();
        Assert.True(g.Enemies[i].Grow == 2 && g.Enemies[i].MaxHp == mx + 2 * D.BehaviorGrowHp && g.Enemies[i].X == 11 && g.Enemies[i].Y == 11);
        for (var t = 0; t < D.BehaviorGrowEvery * 10; ++t) g.PlayerWait();
        Assert.Equal(D.BehaviorGrowMax, (int)g.Enemies[i].Grow);
    }

    [Fact]
    public void FleesFromHeroWithCooldown()
    {
        var g = TestData.Arena(1);
        var i = Put(g, DefWith(Behavior.Flees), 8, 7);
        g.PlayerWait();
        Assert.True(Game.Cheb(g.Enemies[i].X, g.Enemies[i].Y, 7, 7) == 2 && g.Enemies[i].Timer == D.BehaviorFleeCooldown);
    }

    [Fact]
    public void PushesHeroWithCooldown()
    {
        var g = TestData.Arena(1);
        var i = Put(g, DefExactly(Behavior.Pushes), 8, 7);
        g.PlayerWait();
        Assert.True(g.Hero.X == 6 && g.Hero.Y == 7 && g.Enemies[i].Timer == D.BehaviorPushCooldown);
    }

    [Fact]
    public void ReturnsOnceWithHalfHp()
    {
        var g = TestData.Arena(1);
        var i = Put(g, DefExactly(Behavior.Returns), 11, 11);
        g.Enemies[i].Stun = 99;
        g.Enemies[i].Hp = 1;
        g.HeroAttack(i);
        Assert.True(!g.Enemies[i].Alive && (g.Enemies[i].Flags & ActorFlag.Reviving) != 0 && g.Kills == 0);
        for (var t = 0; t < D.BehaviorReturnTurns; ++t) g.PlayerWait();
        Assert.True(g.Enemies[i].Alive && g.Enemies[i].Hp == Math.Max(1, g.Enemies[i].MaxHp * D.BehaviorReturnHpPct / 100));
        g.Enemies[i].Hp = 1;
        g.HeroAttack(i);
        for (var t = 0; t < D.BehaviorReturnTurns * 2; ++t) g.PlayerWait();
        Assert.True(!g.Enemies[i].Alive && g.Kills == 1);
    }

    [Fact]
    public void MudCostsExtraTurnAndBridgeCoversIt()
    {
        var g = TestData.Arena(1);
        int mx = -1, my = -1;
        for (var y = 2; y <= 13 && mx < 0; ++y)
        {
            for (var x = 2; x <= 13; ++x)
            {
                if (!g.Mud(x, y) || g.Mud(x - 1, y)) continue;
                mx = x;
                my = y;
                break;
            }
        }
        Assert.True(mx >= 0);
        g.Hero.X = (sbyte)(mx - 1);
        g.Hero.Y = (sbyte)my;
        var t0 = g.Turns;
        Assert.True(g.PlayerMove(1, 0) && g.Turns == t0 + 2);
        g.Bridges = 1;
        g.BridgeX[0] = (sbyte)mx;
        g.BridgeY[0] = (sbyte)my;
        Assert.False(g.Mud(mx, my));
    }

    [Fact]
    public void GustPushesHeroAfterWarning()
    {
        var g = TestData.Arena(1);
        g.Stage = TestData.F0 + 4;
        g.StageStartTurn = g.Turns;
        Assert.True(g.ActIs(ActMechanic.Gust));
        var v = D.Acts[1].MechValue;
        for (var t = 0; t < v - 1; ++t) g.PlayerWait();
        Assert.Equal(1, g.GustIn());
        int dir = g.GustDir(), hx = g.Hero.X, hy = g.Hero.Y;
        g.PlayerWait();
        Assert.True(g.Hero.X == hx + Game.GustVec[dir, 0] && g.Hero.Y == hy + Game.GustVec[dir, 1]);
    }

    [Fact]
    public void DustReducesSight()
    {
        var g = TestData.Arena(1);
        var r0 = g.SightRadius();
        g.Stage = TestData.F0 + 8;
        Assert.True(g.ActIs(ActMechanic.Dust) && g.SightRadius() == r0 - D.Acts[2].MechValue);
    }

    [Fact]
    public void ProfileV8MigratesToV9Catalog()
    {
        var p = Meta.NewProfile(D);
        p.Best = 4321;
        p.Respect = 77;
        p.Catalog = 0x0F0F;
        p.CatalogHi = 0xDEADBEEF;
        p.Magic = Profile.MagicBytes(Profile.MagicV8);
        Assert.True(Meta.ProfileFix(D, p) && p.MagicIs(Profile.MagicV11) && p.CatalogHi == 0 && p.Best == 4321 && p.Respect == 77 && p.Catalog == 0x0F0F);
        Meta.CatalogAdd(p, 20);
        Meta.CatalogAdd(p, 31);
        Assert.True(Meta.CatalogHas(p, 20) && Meta.CatalogHas(p, 31) && !Meta.CatalogHas(p, 21) && Meta.CatalogCount(D, p) == 8 + 2);
        Assert.Equal("PBRUN13", RunSave.RunMagic);
    }

    [Fact]
    public void StatHelpUsesRealFormulas()
    {
        Assert.Equal("+2 obrażeń broni", StatHelp.Effect(D, new Message(), StatKind.Str, 5, true).Text);
        Assert.Equal("nie dla tej broni", StatHelp.Effect(D, new Message(), StatKind.Agi, 5, false).Text);
        Assert.Equal("-2 obrażeń od problemów", StatHelp.Effect(D, new Message(), StatKind.Def, 5, false).Text);
        Assert.Equal("kryt 20%, unik 10%", StatHelp.Effect(D, new Message(), StatKind.Luck, 5, false).Text);
        Assert.Equal("SZCZ: kryt 5% +3%/pkt", StatHelp.Rule(D, new Message(), StatKind.Luck).Text);
        Assert.Equal("unik +2%/pkt (maks. 20%)", StatHelp.Rule(D, new Message(), StatKind.Luck, 1).Text);
        Assert.Equal("łupy: +2% szansy/pkt", StatHelp.Rule(D, new Message(), StatKind.Luck, 2).Text);
    }
}
