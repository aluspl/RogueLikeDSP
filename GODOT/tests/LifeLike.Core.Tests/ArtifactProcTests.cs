namespace LifeLike.Core.Tests;

/// <summary>
/// v0.23.0: artefakty – „co N-ty cios” (licznik w stanie gry, zeruje się przy zmianie broni): Młot Gromowładny (piorun: prąd +50%,
/// paraliż nie na bossa, przeskok na mokre), Kielnia Pierwszej Cegły (unieruchomienie 2 t.), Szlifierka Iskrownica (każdy cios
/// podpala, ogień skacze na jednego sąsiada), Łom Ostatniej Deski (Patent na ścianie bez odnowienia, +100% na drewno).
/// </summary>
public class ArtifactProcTests
{
    private static GameData D => TestData.D;

    private static int Wi(string id) => Array.FindIndex(D.Weapons, w => w.Id == id);

    private static int Ei(string id) => Array.FindIndex(D.Enemies, e => e.Id == id);

    private static int Ti(string weapon) => Array.FindIndex(D.Tools, t => D.Weapons[t.Weapon].Id == weapon);

    /// <summary>Plac z artefaktem w ręku: broń bez rozrzutu i krytu (obrażenia liczone dokładnie), bez ogłuszenia obuchem.</summary>
    private static Game Arena(string weapon)
    {
        var wi = Wi(weapon);
        var w = D.Weapons[wi] with { MinDamage = 6, MaxDamage = 6, Crit = 0 };
        var g = new Game(D.WithWeapon(wi, w).WithHitTypes(D.HitTypes with { StunChancePct = 0 }));
        g.NewRun(0, 77);
        g.Lv.Fill(Tile.Wall);
        for (var y = 1; y <= 14; ++y)
            for (var x = 1; x <= 14; ++x)
                g.Lv[x, y] = Tile.Floor;
        g.EnemiesCount = 0;
        g.PickupsCount = 0;
        g.StairsX = g.StairsY = -1;
        g.Weather = 0;
        g.Hero.X = 7;
        g.Hero.Y = 7;
        g.WeaponOverride = wi;
        g.WeaponRarity = 3;
        g.CritBuff = sbyte.MinValue;
        g.UpdateFov();
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

    private static int Loss(Game g, int ei, WeaponMode m)
    {
        var hp = g.Enemies[ei].Hp;
        g.AttackWith(m, ei);
        return hp - g.Enemies[ei].Hp;
    }

    [Fact]
    public void ArtifactsAreParsed()
    {
        var grom = D.Weapons[Wi("mlot_grom")];
        Assert.True(grom.Master);
        Assert.Equal("mlot", grom.Artifact);
        Assert.Equal(5, grom.Proc.Every);
        Assert.Equal(ModeEffect.Stun, grom.Proc.Effect);
        Assert.Equal(ModeEffect.Chain, grom.Proc.Effect2);
        Assert.Equal(Element.Power, grom.Proc.Elem);
        Assert.Equal(150, grom.Proc.DmgPct);
        Assert.Equal(ModeEffect.Root, D.Weapons[Wi("kielnia_cegla")].Proc.Effect);
        Assert.Equal(ModeEffect.Burn, D.Weapons[Wi("szlifierka_isk")].Proc.Effect);
        Assert.Equal(ModeEffect.Spread, D.Weapons[Wi("szlifierka_isk")].Proc.Effect2);
        var lom = D.Weapons[Wi("lom_deska")];
        Assert.Null(lom.Proc);
        Assert.True(lom.WallFree);
        Assert.Equal("drewno", lom.BonusMaterial);
        Assert.Equal(100, lom.BonusPct);
        Assert.Null(D.Weapons[Wi("mlot")].Proc);
        Assert.Equal("", D.Weapons[Wi("mlot")].Artifact);
    }

    [Fact]
    public void ProcFiresOnEveryNthHitAndCounts()
    {
        var g = Arena("kielnia_cegla");
        var e = Add(g, 8, 7);
        g.AttackWith(g.Weapon.ModeA, e);
        g.AttackWith(g.Weapon.ModeA, e);
        Assert.Equal(2, g.ProcCount);
        Assert.Equal(0, g.Enemies[e].Root);
        g.AttackWith(g.Weapon.ModeA, e);
        Assert.Equal(0, g.ProcCount);
        Assert.Equal(2, g.Enemies[e].Root);
        Assert.Contains(g.Log.TakeLast(4), m => m.Text.Contains(g.Weapon.Proc.Name));
        g.AttackWith(g.Weapon.ModeB, e); // Patent też liczy się jako cios
        Assert.Equal(1, g.ProcCount);
    }

    [Fact]
    public void MissDoesNotCount()
    {
        var g = Arena("kielnia_cegla");
        var e = Add(g, 8, 7);
        g.Enemies[e].Alive = false;
        g.AttackWith(g.Weapon.ModeA, e);
        Assert.Equal(0, g.ProcCount);
    }

    [Fact]
    public void WeaponChangeResetsCounter()
    {
        var g = Arena("kielnia_cegla");
        var e = Add(g, 8, 7);
        g.AttackWith(g.Weapon.ModeA, e);
        g.AttackWith(g.Weapon.ModeA, e);
        Assert.Equal(2, g.ProcCount);
        g.TakeTool(Ti("lom"), 0, false);
        Assert.Equal(0, g.ProcCount);
        g.Equip(D.BeltSlot, Game.BeltTier, 1);
        g.Weapon2 = Wi("kielnia_cegla");
        g.Weapon2Rarity = 3;
        g.AttackWith(g.Weapon.ModeA, e);
        Assert.Equal(0, g.ProcCount); // łom bez mechaniki – nie liczy
        Assert.True(g.PlayerSwapWeapon());
        g.AttackWith(g.Weapon.ModeA, e);
        Assert.Equal(1, g.ProcCount);
        Assert.True(g.PlayerSwapWeapon());
        Assert.Equal(0, g.ProcCount);
    }

    [Fact]
    public void ThunderFifthHitShocksParalysesAndBoostsDamage()
    {
        var g = Arena("mlot_grom");
        var e = Add(g, 8, 7);
        var plain = Loss(g, e, g.Weapon.ModeB);
        Assert.Equal(0, g.Enemies[e].Stun);
        g.ProcCount = 4;
        var thunder = Loss(g, e, g.Weapon.ModeB);
        Assert.Equal(HitCalc.Apply(plain, 150), thunder);
        Assert.Equal(1, g.Enemies[e].Stun);
        Assert.Equal(0, g.ProcCount);
    }

    [Fact]
    public void ThunderDoesNotParalyseBoss()
    {
        var g = Arena("mlot_grom");
        var b = g.EnemiesCount;
        g.Boss = b;
        Add(g, 8, 7, "termin");
        var plain = Loss(g, b, g.Weapon.ModeB);
        g.ProcCount = 4;
        Assert.Equal(HitCalc.Apply(plain, 150), Loss(g, b, g.Weapon.ModeB));
        Assert.Equal(0, g.Enemies[b].Stun);
    }

    [Fact]
    public void ThunderJumpsToWetNeighbours()
    {
        var g = Arena("mlot_grom");
        var e = Add(g, 8, 7);
        var wet = Add(g, 9, 6);
        g.Enemies[wet].Wet = 3;
        var dry = Add(g, 9, 9);
        g.ProcCount = 4;
        g.AttackWith(g.Weapon.ModeA, e);
        Assert.True(g.Enemies[wet].Hp < 5000);
        Assert.Equal(5000, g.Enemies[dry].Hp);
        // bez pioruna prąd nie skacze
        var h = Arena("mlot_grom");
        var e2 = Add(h, 8, 7);
        var wet2 = Add(h, 9, 6);
        h.Enemies[wet2].Wet = 3;
        h.AttackWith(h.Weapon.ModeA, e2);
        Assert.Equal(5000, h.Enemies[wet2].Hp);
    }

    [Fact]
    public void TrowelRootsForTwoTurnsNotBoss()
    {
        var g = Arena("kielnia_cegla");
        var b = g.EnemiesCount;
        g.Boss = b;
        Add(g, 8, 7, "termin");
        g.ProcCount = 2;
        g.AttackWith(g.Weapon.ModeA, b);
        Assert.Equal(0, g.Enemies[b].Root);
        Assert.Equal(0, g.ProcCount);
    }

    [Fact]
    public void GrinderBurnsEveryHitAndFireSpreadsToOneNeighbour()
    {
        var g = Arena("szlifierka_isk");
        var e = Add(g, 8, 7);
        var n1 = Add(g, 9, 7);
        var n2 = Add(g, 9, 8);
        g.AttackWith(g.Weapon.ModeA, e);
        Assert.True(g.Enemies[e].Dot > 0);
        Assert.Equal(1, g.Enemies[e].DotKind); // podpalenie, nie krwawienie (cięcie)
        var burning = (g.Enemies[n1].Dot > 0 ? 1 : 0) + (g.Enemies[n2].Dot > 0 ? 1 : 0);
        Assert.Equal(1, burning);
        Assert.Equal(0, g.ProcCount); // co 1. cios – licznik zawsze 0
        g.AttackWith(g.Weapon.ModeA, e);
        Assert.True(g.Enemies[e].Dot > 0);
    }

    [Fact]
    public void LastPlankCrowbarOpensWallWithoutCooldown()
    {
        var g = Arena("lom_deska");
        CrackedWall(g, 7, 6);
        Assert.True(g.CanWeaponBOpenWall());
        Assert.True(g.PlayerWeaponBWall());
        Assert.True(g.SecretOpen);
        Assert.Equal(0, g.WeaponBCd);
        // na problem – zwykłe odnowienie
        var e = Add(g, 8, 7);
        Assert.True(g.PlayerWeaponB(e));
        Assert.Equal(5, g.WeaponBCd);
        // zwykły Łom – odnowienie po ścianie
        var h = Arena("lom");
        CrackedWall(h, 7, 6);
        Assert.True(h.PlayerWeaponBWall());
        Assert.True(h.WeaponBCd > 0);
    }

    [Fact]
    public void LastPlankCrowbarDoublesDamageOnWood()
    {
        var wood = D.Enemies[Ei("plesn")];
        var steel = D.Enemies[Ei("dostawa")];
        var art = D.Weapons[Wi("lom_deska")];
        var lom = D.Weapons[Wi("lom")];
        Assert.Equal(2 * HitCalc.Pct(D, lom, wood), HitCalc.Pct(D, art, wood));
        Assert.Equal(HitCalc.Pct(D, lom, steel), HitCalc.Pct(D, art, steel));
    }

    [Fact]
    public void PatentWallsCountForSecret()
    {
        var g = Arena("przecinak");
        CrackedWall(g, 7, 6);
        Assert.True(g.PlayerWeaponBWall());
        Assert.Equal(1, g.PatentWalls);
        var s = Array.FindIndex(D.Secrets, x => x.Kind == SecretKind.PatentWalls);
        Assert.True(s >= 0);
        var p = Meta.NewProfile(D);
        Assert.False(Secrets.Condition(D, p, g, s));
        g.PatentWalls = (byte)D.Secrets[s].Value;
        Assert.True(Secrets.Condition(D, p, g, s));
        Assert.Equal(SecretReward.Tool, D.Secrets[s].Reward);
        Assert.Equal(Ti("lom_deska"), D.Secrets[s].Index);
    }

    [Fact]
    public void ProcStateSurvivesRunSave()
    {
        var g = TestData.Run(1, 4242);
        g.ProcCount = 3;
        g.ArtifactsRun = 5;
        g.PatentWalls = 2;
        var h = RunSave.FromBytes(RunSave.Make(g).ToBytes()).Load(D);
        Assert.NotNull(h);
        Assert.Equal(3, h.ProcCount);
        Assert.Equal(5, h.ArtifactsRun);
        Assert.Equal(2, h.PatentWalls);
        Assert.Equal(StateDigest.Of(g), StateDigest.Of(h));
        h.ProcCount = 1;
        Assert.NotEqual(StateDigest.Of(g), StateDigest.Of(h));
        Assert.Equal("PBRUN19", RunSave.RunMagic);
    }

    [Fact]
    public void BotValuesProcAboveSameWeaponWithout()
    {
        var wi = Wi("mlot_grom");
        var with = TestData.Run(0, 99);
        var without = new Game(D.WithWeapon(wi, D.Weapons[wi] with { Proc = null }));
        without.NewRun(0, 99);
        Assert.True(BotWeapons.Score(with, wi, 3) > BotWeapons.Score(without, wi, 3));
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
}
