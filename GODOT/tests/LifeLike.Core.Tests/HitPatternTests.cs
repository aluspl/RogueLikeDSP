namespace LifeLike.Core.Tests;

/// <summary>v0.23.0: wzory pól trybu ataku (łuk, wokół, linia, strzał) i atak wieloceowy.</summary>
public class HitPatternTests
{
    private static GameData D => TestData.D;

    private static int Wi(string id) => Array.FindIndex(D.Weapons, w => w.Id == id);

    /// <summary>Arena 14x14, bohater (7,7), broń "mlot" z podanym trybem A (mocni wrogowie, by nikt nie padł od razu).</summary>
    private static Game Arena(PatternKind k, int range, int dmgPct = 100)
    {
        var wi = Wi("mlot");
        var w = D.Weapons[wi] with { Range = range, ModeA = new WeaponMode(k, range, dmgPct, ModeEffect.None, 0, 0, 0) };
        var g = new Game(D.WithWeapon(wi, w));
        g.NewRun(0, 77);
        g.Lv.Fill(Tile.Wall);
        for (var y = 1; y <= 14; ++y)
            for (var x = 1; x <= 14; ++x) g.Lv[x, y] = Tile.Floor;
        g.EnemiesCount = 0;
        g.PickupsCount = 0;
        g.StairsX = g.StairsY = -1;
        g.Weather = 0;
        g.Hero.X = g.Hero.Y = 7;
        g.WeaponOverride = wi;
        return g;
    }

    private static int Add(Game g, int x, int y)
    {
        var i = g.EnemiesCount;
        g.Spawn(Array.FindIndex(D.Enemies, e => e.Id == "plesn"), x, y);
        g.Enemies[i].Hp = g.Enemies[i].MaxHp = 5000;
        g.UpdateFov();
        return i;
    }

    private static bool Hurt(Game g, int i) => g.Enemies[i].Hp < g.Enemies[i].MaxHp;

    [Fact]
    public void ArcHitsThreeInFront()
    {
        var g = Arena(PatternKind.Arc, 1);
        int a = Add(g, 6, 6), b = Add(g, 7, 6), c = Add(g, 8, 6), far = Add(g, 7, 5), side = Add(g, 8, 7);
        g.Hero.Hp = 99;
        Assert.True(g.PlayerAttack(b));
        Assert.True(Hurt(g, a) && Hurt(g, b) && Hurt(g, c));
        Assert.False(Hurt(g, far));
        Assert.False(Hurt(g, side));
    }

    [Fact]
    public void AroundHitsEight()
    {
        var g = Arena(PatternKind.Around, 1);
        var ids = new List<int>();
        for (var dy = -1; dy <= 1; ++dy)
            for (var dx = -1; dx <= 1; ++dx)
                if (dx != 0 || dy != 0) ids.Add(Add(g, 7 + dx, 7 + dy));
        var out2 = Add(g, 7, 5);
        Assert.True(g.PlayerAttack(ids[0]));
        foreach (var i in ids) Assert.True(Hurt(g, i));
        Assert.False(Hurt(g, out2));
    }

    [Fact]
    public void LineStopsAtWall()
    {
        var g = Arena(PatternKind.Line, 3);
        g.Lv[7, 5] = Tile.Wall; // ściana na 2. polu linii
        int first = Add(g, 7, 6), behind = Add(g, 7, 4);
        Assert.True(g.PlayerAttack(first));
        Assert.True(Hurt(g, first));
        Assert.False(Hurt(g, behind));
    }

    [Fact]
    public void LinePierces()
    {
        var g = Arena(PatternKind.Line, 3);
        int a = Add(g, 7, 6), b = Add(g, 7, 5), c = Add(g, 7, 4);
        Assert.True(g.PlayerAttack(a));
        Assert.True(Hurt(g, a) && Hurt(g, b) && Hurt(g, c));
    }

    [Fact]
    public void ShotHitsFirstOnly()
    {
        var g = Arena(PatternKind.Shot, 3);
        int a = Add(g, 7, 6), b = Add(g, 7, 5);
        Assert.True(g.PlayerAttack(b)); // cel dalszy, ale pocisk trafia pierwszego w linii
        Assert.True(Hurt(g, a));
        Assert.False(Hurt(g, b));
    }

    [Fact]
    public void DiagonalArcRotates()
    {
        var g = Arena(PatternKind.Arc, 1);
        int diag = Add(g, 8, 6), up = Add(g, 7, 6), right = Add(g, 8, 7), other = Add(g, 6, 6);
        Assert.True(g.PlayerAttack(diag));
        Assert.True(Hurt(g, diag) && Hurt(g, up) && Hurt(g, right));
        Assert.False(Hurt(g, other));
    }

    [Fact]
    public void PatternMultiplierScalesDamage()
    {
        var full = Arena(PatternKind.Single, 1);
        var half = Arena(PatternKind.Single, 1, 50);
        int a = Add(full, 8, 7), b = Add(half, 8, 7);
        full.Weather = half.Weather = 0;
        full.HeroAttack(a);
        half.AttackWith(half.Weapon.ModeA, b);
        var dFull = full.Enemies[a].MaxHp - full.Enemies[a].Hp;
        var dHalf = half.Enemies[b].MaxHp - half.Enemies[b].Hp;
        Assert.True(dHalf < dFull, $"{dHalf} < {dFull}");
    }

    [Fact]
    public void ThrowHitsAreaAroundTarget()
    {
        var g = Arena(PatternKind.Throw3x3, 3);
        int t = Add(g, 7, 4), n = Add(g, 8, 3), far = Add(g, 7, 6);
        Assert.True(g.PlayerAttack(t));
        Assert.True(Hurt(g, t) && Hurt(g, n));
        Assert.False(Hurt(g, far));
    }

    [Fact]
    public void Throw3x3SkipsCornerBehindWalls()
    {
        // pole skośne obok celu za narożnikiem (oba pola pomiędzy to ściany) nie dostaje odłamków; jedna ściana nie zasłania
        var g = Arena(PatternKind.Throw3x3, 3);
        g.Lv[8, 4] = Tile.Wall;
        g.Lv[7, 3] = Tile.Wall;
        int t = Add(g, 7, 4), corner = Add(g, 8, 3), open = Add(g, 6, 3);
        Assert.True(g.PlayerAttack(t));
        Assert.True(Hurt(g, t) && Hurt(g, open));
        Assert.False(Hurt(g, corner));
        var h = Arena(PatternKind.Throw3x3, 3);
        h.Lv[8, 4] = Tile.Wall; // tylko jedna ściana: narożnik widać z celu
        int t2 = Add(h, 7, 4), c2 = Add(h, 8, 3);
        Assert.True(h.PlayerAttack(t2));
        Assert.True(Hurt(h, t2) && Hurt(h, c2));
    }

    [Fact]
    public void StairsDoNotBlockPattern()
    {
        var g = Arena(PatternKind.Line, 3);
        g.Lv[7, 6] = Tile.Stairs;
        int onStairs = Add(g, 7, 6), behind = Add(g, 7, 5);
        Assert.True(g.PlayerAttack(onStairs));
        Assert.True(Hurt(g, onStairs) && Hurt(g, behind));
        var h = Arena(PatternKind.Line, 3);
        h.Lv[7, 6] = Tile.Stairs; // pusty schód na drodze, wróg za nim
        int far = Add(h, 7, 5);
        Assert.True(h.PlayerAttack(far));
        Assert.True(Hurt(h, far));
    }

    [Fact]
    public void SprayBlockedByWallInFront()
    {
        var g = Arena(PatternKind.Spray, 2);
        int front = Add(g, 7, 6), row = Add(g, 7, 5);
        Assert.True(g.PlayerAttack(front));
        Assert.True(Hurt(g, front) && Hurt(g, row));
        var h = Arena(PatternKind.Spray, 2);
        int r2 = Add(h, 7, 5), r3 = Add(h, 6, 5);
        h.Lv[7, 6] = Tile.Wall;
        h.UpdateFov();
        h.AttackWith(h.Weapon.ModeA, r2);
        Assert.False(Hurt(h, r2) || Hurt(h, r3));
    }

    [Fact]
    public void LegacySingleUnchanged()
    {
        // broń bez trybów w danych: PlayerAttack = HeroAttack(ei) + EndTurn, ten sam rzut przy tym samym seedzie
        var a = TestData.Arena(0);
        var b = TestData.Arena(0);
        a.Spawn(4, 8, 7);
        b.Spawn(4, 8, 7);
        a.Enemies[0].Hp = a.Enemies[0].MaxHp = 500;
        b.Enemies[0].Hp = b.Enemies[0].MaxHp = 500;
        a.HeroAttack(0);
        Assert.True(b.PlayerAttack(0));
        // EndTurn mógł dołożyć obrażenia od problemu, ale nie wrogowi – obrażenia wroga identyczne
        Assert.Equal(a.Enemies[0].Hp, b.Enemies[0].Hp);
    }

    [Fact]
    public void CellsOrderedOutwardAndRotated()
    {
        var xs = new int[16];
        var ys = new int[16];
        var n = HitPattern.Cells(PatternKind.Line, 3, 7, 7, 0, -1, xs, ys);
        Assert.Equal(3, n);
        Assert.Equal((7, 6), (xs[0], ys[0]));
        Assert.Equal((7, 4), (xs[2], ys[2]));
        n = HitPattern.Cells(PatternKind.Arc, 1, 7, 7, 1, -1, xs, ys);
        Assert.Equal(3, n);
        var set = new HashSet<(int, int)>();
        for (var i = 0; i < n; ++i) set.Add((xs[i], ys[i]));
        Assert.Equal(new HashSet<(int, int)> { (7, 6), (8, 6), (8, 7) }, set);
        Assert.Equal(8, HitPattern.Cells(PatternKind.Around, 1, 7, 7, 1, 0, xs, ys));
        Assert.Equal(4, HitPattern.Cells(PatternKind.Cross, 1, 7, 7, 0, -1, xs, ys));
        Assert.Equal(4, HitPattern.Cells(PatternKind.Spray, 1, 7, 7, 0, -1, xs, ys));
        Assert.Equal(6, HitPattern.Cells(PatternKind.LineBoth, 3, 7, 7, 0, -1, xs, ys));
        Assert.Equal(12, HitPattern.Cells(PatternKind.Line4, 3, 7, 7, 0, -1, xs, ys));
    }

    [Theory]
    [InlineData(PatternKind.Single)]
    [InlineData(PatternKind.Arc)]
    [InlineData(PatternKind.Around)]
    [InlineData(PatternKind.Cross)]
    [InlineData(PatternKind.Line)]
    [InlineData(PatternKind.Shot)]
    [InlineData(PatternKind.Throw3x3)]
    [InlineData(PatternKind.Spray)]
    [InlineData(PatternKind.Push)]
    [InlineData(PatternKind.LineBoth)]
    [InlineData(PatternKind.Line4)]
    public void PatternCellsMatchTargets(PatternKind k)
    {
        // v0.23.0: podświetlenie celowania (Game.PatternCells) = pola, na których trafione są problemy (Game.PatternTargets),
        // z murem przed bohaterem i rozstawionymi problemami, dla każdego celu w każdym kierunku
        var g = Arena(k, 3);
        g.Lv[7, 6] = Tile.Wall;
        g.Lv[9, 9] = Tile.Wall;
        Add(g, 8, 7);
        Add(g, 6, 7);
        for (var y = 4; y <= 10; ++y)
        {
            for (var x = 4; x <= 10; ++x)
            {
                if ((x + y) % 2 == 0 && g.Lv[x, y] == Tile.Floor && !(x == 7 && y == 7) && g.EnemiesCount < Game.MaxEnemies) Add(g, x, y);
            }
        }
        var m = g.Weapon.ModeA;
        Span<int> hit = stackalloc int[HitPattern.MaxCells];
        Span<int> xs = stackalloc int[HitPattern.MaxCells];
        Span<int> ys = stackalloc int[HitPattern.MaxCells];
        var checkedTargets = 0;
        for (var ei = 0; ei < g.EnemiesCount; ++ei)
        {
            var n = g.PatternTargets(m, ei, hit);
            if (n == 0) continue;
            ++checkedTargets;
            var cells = g.PatternCells(m, ei, xs, ys);
            var onCells = new HashSet<int>();
            for (var c = 0; c < cells; ++c)
            {
                Assert.True(g.Lv.Passable(xs[c], ys[c]), $"{k}: pole za murem ({xs[c]},{ys[c]})");
                var j = g.EnemyAt(xs[c], ys[c]);
                if (j >= 0) onCells.Add(j);
            }
            Assert.Equal(hit[..n].ToArray().OrderBy(x => x), onCells.OrderBy(x => x));
        }
        Assert.True(checkedTargets > 0, k.ToString());
    }
}
