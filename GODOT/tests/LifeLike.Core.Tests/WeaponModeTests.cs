namespace LifeLike.Core.Tests;

/// <summary>v0.23.0: tryb B broni z odnowieniem i nowe stany problemów (unieruchomienie, krwawienie/podpalenie, osłabienie, znak).</summary>
public class WeaponModeTests
{
    private static GameData D => TestData.D;

    private static int Wi(string id) => Array.FindIndex(D.Weapons, w => w.Id == id);

    private static int Ei(string id) => Array.FindIndex(D.Enemies, e => e.Id == id);

    private static Game Arena(string weapon)
    {
        var g = TestData.Arena(0);
        g.WeaponOverride = Wi(weapon);
        return g;
    }

    private static int Add(Game g, int x, int y, string id = "plesn")
    {
        var i = g.EnemiesCount;
        g.Spawn(Ei(id), x, y);
        g.Enemies[i].Hp = g.Enemies[i].MaxHp = 5000;
        g.UpdateFov();
        return i;
    }

    /// <summary>Sam efekt trybu B bez tury i odnowienia (bez fazy problemów).</summary>
    private static bool B(Game g, int ei) => g.AttackWith(g.Weapon.ModeB, ei) > 0;

    [Fact]
    public void ModeBWithoutTargetCostsNothing()
    {
        var g = Arena("kielnia");
        var far = Add(g, 12, 12);
        var turns = g.Turns;
        Assert.False(g.PlayerWeaponB(far));
        Assert.False(g.PlayerWeaponB(-1));
        Assert.Equal(0, g.WeaponBCd);
        Assert.Equal(turns, g.Turns);
    }

    [Fact]
    public void ModeBStartsCooldown()
    {
        var g = Arena("kielnia");
        var e = Add(g, 9, 7); // zasięg 2
        Assert.True(g.PlayerWeaponB(e));
        Assert.Equal(3, g.WeaponBCd);
        Assert.False(g.PlayerWeaponB(e));
        Assert.Equal(3, g.WeaponBCd);
        for (var k = 0; k < 3; ++k) g.EndTurn();
        Assert.Equal(0, g.WeaponBCd);
    }

    [Fact]
    public void CooldownSurvivesWeaponChange()
    {
        var g = Arena("kielnia");
        var e = Add(g, 9, 7);
        Assert.True(g.PlayerWeaponB(e));
        g.WeaponOverride = Wi("mlot");
        Assert.Equal(3, g.WeaponBCd);
    }

    [Fact]
    public void StunEffectStunsTarget()
    {
        var g = Arena("kielnia");
        var e = Add(g, 9, 7);
        Assert.True(B(g, e));
        Assert.True(g.Enemies[e].Stun >= 1);
    }

    [Fact]
    public void RootStopsMovement()
    {
        var g = Arena("kotwy");
        var e = Add(g, 11, 7);
        g.Enemies[e].Awake = true;
        g.Enemies[e].Root = 2;
        g.EnemyAct(e);
        g.EnemyAct(e);
        Assert.Equal(11, g.Enemies[e].X);
        Assert.Equal(0, g.Enemies[e].Root);
        g.EnemyAct(e);
        Assert.NotEqual(11, g.Enemies[e].X);
    }

    [Fact]
    public void RootEffectFromModeB()
    {
        var g = Arena("kotwy");
        var e = Add(g, 8, 7);
        Assert.True(B(g, e));
        Assert.True(g.Enemies[e].Root > 0);
    }

    [Fact]
    public void RootedEnemyStillStrikesAdjacent()
    {
        var g = Arena("kotwy");
        var e = Add(g, 8, 7);
        g.Enemies[e].Awake = true;
        g.Enemies[e].Root = 3;
        g.Hero.Hp = g.Hero.MaxHp = 500;
        g.EnemyAct(e);
        Assert.True(g.Hero.Hp < 500);
    }

    [Fact]
    public void DotTicks()
    {
        var g = Arena("spawarka");
        var e = Add(g, 8, 7);
        Assert.True(B(g, e));
        Assert.Equal(3, g.Enemies[e].Dot);
        g.Enemies[e].Stun = 99; // nie atakuje – liczy się tylko stan
        var hp = (int)g.Enemies[e].Hp;
        for (var k = 1; k <= 3; ++k)
        {
            g.EnemyAct(e);
            Assert.Equal(hp - 2 * k, g.Enemies[e].Hp);
        }
        g.EnemyAct(e);
        Assert.Equal(hp - 6, g.Enemies[e].Hp);
        Assert.Equal(0, g.Enemies[e].Dot);
    }

    [Fact]
    public void DotKillsEnemy()
    {
        var g = Arena("spawarka");
        var e = Add(g, 8, 7);
        g.Enemies[e].Dot = 3;
        g.Enemies[e].DotDmg = 2;
        g.Enemies[e].Hp = 1;
        g.Enemies[e].Awake = true;
        g.EnemyAct(e);
        Assert.False(g.Enemies[e].Alive);
    }

    [Fact]
    public void DefDownLowersDefense()
    {
        var g = Arena("wkretarka");
        var e = Add(g, 8, 7, "kamien");
        var def = g.EnemyDefense(e);
        Assert.True(B(g, e));
        Assert.Equal(3, g.Enemies[e].DefDown);
        Assert.Equal(Math.Max(0, def - 3), g.EnemyDefense(e));
        g.Enemies[e].Stun = 99;
        for (var k = 0; k < 3; ++k) g.EnemyAct(e);
        Assert.Equal(def, g.EnemyDefense(e));
    }

    [Fact]
    public void SwapSwapsPlaces()
    {
        var g = Arena("tablet");
        var e = Add(g, 9, 7);
        Assert.True(B(g, e));
        Assert.Equal((9, 7), ((int)g.Hero.X, (int)g.Hero.Y));
        Assert.Equal((7, 7), ((int)g.Enemies[e].X, (int)g.Enemies[e].Y));
    }

    [Fact]
    public void SwapNotOnBoss()
    {
        var g = Arena("tablet");
        var b = g.EnemiesCount;
        g.Boss = b;
        g.Spawn(Ei("termin"), 9, 7);
        g.Enemies[b].Hp = g.Enemies[b].MaxHp = 5000;
        g.UpdateFov();
        Assert.True(B(g, b));
        Assert.Equal((7, 7), ((int)g.Hero.X, (int)g.Hero.Y));
        Assert.Equal(9, g.Enemies[b].X);
    }

    [Fact]
    public void PushAndPull()
    {
        var g = Arena("lom");
        var e = Add(g, 8, 7);
        Assert.True(B(g, e));
        Assert.Equal(10, g.Enemies[e].X);
        var h = Arena("lyzka");
        var f = Add(h, 10, 7);
        Assert.True(B(h, f));
        Assert.Equal(8, h.Enemies[f].X);
    }

    [Fact]
    public void MarkAddsPercentDamage()
    {
        var g = Arena("dziennik");
        var e = Add(g, 8, 7);
        Assert.True(B(g, e));
        Assert.Equal(30, g.Enemies[e].MarkPct);
        Assert.True(g.Enemies[e].MarkTurns > 0);
        for (var k = 0; k < 4; ++k) g.EndTurn();
        Assert.Equal(0, g.Enemies[e].MarkTurns);
    }

    [Fact]
    public void ChainJumpsToWetNeighbours()
    {
        var g = Arena("probnik");
        var e = Add(g, 8, 7);
        var wet = Add(g, 8, 8);
        g.Enemies[wet].Wet = 3;
        var hp = (int)g.Enemies[wet].Hp;
        Assert.True(B(g, e));
        Assert.True(g.Enemies[wet].Hp < hp);
    }

    [Fact]
    public void DryRemovesWet()
    {
        var g = Arena("klucz");
        var e = Add(g, 8, 7);
        g.Enemies[e].Wet = 5;
        Assert.True(B(g, e));
        Assert.Equal(0, g.Enemies[e].Wet);
    }

    [Fact]
    public void RevealUncoversRoomAndBuffsCrit()
    {
        var g = Arena("miernik");
        var e = Add(g, 8, 7);
        var crit = g.CritPct();
        Assert.True(B(g, e));
        Assert.Equal(crit + 10, g.CritPct());
        for (var k = 0; k < 4; ++k) g.EndTurn();
        Assert.Equal(crit, g.CritPct());
    }

    private static void CrackedWall(Game g, int x, int y)
    {
        var kind = Array.FindIndex(D.SecretKinds, s => s.Breakable);
        g.Lv[x, y] = Tile.Wall;
        g.SecretX = (sbyte)x;
        g.SecretY = (sbyte)y;
        g.SecretKind = (sbyte)kind;
        g.SecretOpen = false;
        g.SecretDir = 0;
    }

    [Fact]
    public void PrzecinakOpensCrackedWall()
    {
        var g = Arena("przecinak");
        CrackedWall(g, 7, 6);
        var e = Add(g, 8, 7);
        Assert.True(g.PlayerWeaponB(e));
        Assert.True(g.SecretOpen);
        Assert.Equal(Tile.Floor, g.Lv.At(7, 6));
        Assert.Equal(2, g.WeaponCharges);
        Assert.Equal(Wi("przecinak"), g.WeaponOverride);
        for (var k = 0; k < 2; ++k)
        {
            g.WeaponBCd = 0;
            CrackedWall(g, 7, 6); // kolejna ściana – ładunek schodzi tylko za otwartą
            Assert.True(g.PlayerWeaponB(e));
        }
        Assert.Equal(-1, g.WeaponOverride); // ładunki zużyte: wraca broń zawodu
        Assert.Equal(0, g.WeaponCharges);
    }

    [Fact]
    public void LomOpensCrackedWallAndPushes()
    {
        var g = Arena("lom");
        CrackedWall(g, 7, 6);
        var e = Add(g, 8, 7);
        Assert.True(B(g, e));
        Assert.True(g.SecretOpen);
        Assert.Equal(10, g.Enemies[e].X);
    }

    [Fact]
    public void LomLeavesDistantWallClosed()
    {
        var g = Arena("lom");
        CrackedWall(g, 2, 2);
        var e = Add(g, 8, 7);
        Assert.True(B(g, e));
        Assert.False(g.SecretOpen);
    }

    [Fact]
    public void LutownicaDriesAndBurns()
    {
        var g = Arena("lutownica");
        var e = Add(g, 8, 7);
        g.Enemies[e].Wet = 4;
        Assert.True(B(g, e));
        Assert.Equal(0, g.Enemies[e].Wet);
        Assert.True(g.Enemies[e].Dot > 0);
    }

    [Fact]
    public void SzlifierkaBlastsDust()
    {
        var g = Arena("szlifierka");
        var e = Add(g, 8, 7);
        var n = Add(g, 8, 8);
        g.Enemies[e].Flags |= ActorFlag.Dusty;
        g.Enemies[n].Flags |= ActorFlag.Dusty;
        g.ComboEvents = 0;
        Assert.True(B(g, e));
        Assert.NotEqual(0, g.ComboEvents & (1 << (int)ComboEffect.DustBlast));
        Assert.Equal(0, g.Enemies[n].Flags & ActorFlag.Dusty);
    }

    [Fact]
    public void MyjkaModeAMoistensTarget()
    {
        var g = Arena("myjka");
        var e = Add(g, 8, 7);
        g.Enemies[e].Wet = 0;
        g.AttackWith(g.Weapon.ModeA, e);
        Assert.True(g.Enemies[e].Wet > 0);
    }

    [Fact]
    public void Effect2IsParsedFromData()
    {
        Assert.Equal(ModeEffect.OpenWall, D.Weapons[Wi("lom")].ModeB.Effect2);
        Assert.Equal(ModeEffect.Dry, D.Weapons[Wi("lutownica")].ModeB.Effect2);
        Assert.Equal(ModeEffect.Ignite, D.Weapons[Wi("szlifierka")].ModeB.Effect2);
        Assert.Equal(ModeEffect.Wet, D.Weapons[Wi("myjka")].ModeA.Effect);
    }

    [Fact]
    public void PrzecinakOpensWallWithoutEnemies()
    {
        var g = Arena("przecinak");
        CrackedWall(g, 7, 6);
        Assert.True(g.CanWeaponBOpenWall());
        Assert.True(g.PlayerWeaponBWall());
        Assert.True(g.SecretOpen);
        Assert.Equal(2, g.WeaponCharges);
        Assert.Equal(1, g.WeaponBCd);
    }

    [Fact]
    public void NoWallNoTargetCostsNothing()
    {
        var g = Arena("przecinak");
        var turns = g.Turns;
        Assert.False(g.CanWeaponBOpenWall());
        Assert.False(g.PlayerWeaponBWall());
        Assert.Equal(turns, g.Turns);
        Assert.Equal(0, g.WeaponCharges);
        Assert.Equal(0, g.WeaponBCd);
    }

    [Fact]
    public void PrzecinakWithEnemyButNoWallKeepsCharge()
    {
        var g = Arena("przecinak");
        var e = Add(g, 8, 7);
        Assert.True(g.PlayerWeaponB(e));
        Assert.Equal(0, g.WeaponCharges);
        Assert.Equal(Wi("przecinak"), g.WeaponOverride);
    }

    [Fact]
    public void LomOpensWallWithoutEnemies()
    {
        var g = Arena("lom");
        CrackedWall(g, 7, 6);
        Assert.True(g.PlayerWeaponBWall());
        Assert.True(g.SecretOpen);
        Assert.Equal(Wi("lom"), g.WeaponOverride);
    }

    [Fact]
    public void KluczWithoutWallOrTargetIsNotAWallAction()
    {
        var g = Arena("klucz");
        Assert.False(g.PlayerWeaponBWall());
    }

    [Fact]
    public void SpawarkaBurnsSteelHarder()
    {
        var g = Arena("spawarka");
        var steel = Add(g, 8, 7, "dostawa");
        var cement = Add(g, 7, 8, "kamien");
        Assert.True(B(g, steel));
        g.WeaponOverride = Wi("spawarka");
        Assert.True(B(g, cement));
        Assert.Equal(3, g.Enemies[steel].DotDmg); // 2 x 150%
        Assert.Equal(2, g.Enemies[cement].DotDmg);
    }

    [Fact]
    public void KluczStopsWaterHealing()
    {
        var g = Arena("klucz");
        var w = Add(g, 8, 7, "wilgoc");
        var other = Add(g, 8, 9, "wilgoc");
        g.Enemies[other].Hp = 100;
        g.Enemies[w].Hp = 100; // ranny: Wilgoć leczy sąsiadów
        g.Enemies[w].Awake = g.Enemies[other].Awake = true;
        Assert.True(B(g, w));
        Assert.True(g.Enemies[w].NoHeal >= 2);
        var hp = (int)g.Enemies[other].Hp;
        g.Enemies[w].Stun = 0;
        g.Enemies[w].X = 8; g.Enemies[w].Y = 11; // z dala od bohatera, by chciał leczyć
        g.EnemyAct(w);
        g.EnemyAct(w);
        Assert.Equal(hp, g.Enemies[other].Hp);
        Assert.Equal(0, g.Enemies[w].NoHeal);
    }
}
