namespace LifeLike.Core.Tests;

/// <summary>
/// v0.23.0: easter egg – Kawka (odpowiednik core_tests.cpp, test 57): etap z seeda (~DogChancePct budów), błoto we wnętrzu pokoju,
/// nigdy na starcie ani schodach, RNG gry bez zmian, dośw. raz, potem zamiana pól; profil v20 (budowy z Kawką, odznaka Psi
/// przyjaciel, tytuły za nową odznaką), oceny postępów zależne od stanu, zapis budowy sprzed Kawki.
/// </summary>
public class DogTests
{
    private static GameData D => TestData.D;

    [Fact]
    public void DogSpawnsDeterministicallyOnMudAwayFromStart()
    {
        int with = 0, placed = 0;
        for (uint s = 1; s <= 400; ++s)
        {
            var g = TestData.Run((int)(s % 3), s);
            var ds = g.DogStage();
            if (ds < 0) continue;
            with++;
            Assert.True(g.DogStageOk(ds) && D.Acts[g.SDef(ds).Act].Mechanic == ActMechanic.Mud && g.SDef(ds).Boss < 0);
            if (ds != g.Stage) g.StartStage(ds);
            if (!g.DogHere) continue;
            placed++;
            Assert.True(g.Lv.At(g.DogX, g.DogY) == Tile.Floor && g.Mud(g.DogX, g.DogY) && !(g.DogX == g.StairsX && g.DogY == g.StairsY));
            Assert.True(Game.Cheb(g.DogX, g.DogY, g.Hero.X, g.Hero.Y) > 3 && g.Occupied(g.DogX, g.DogY) && g.EnemyAt(g.DogX, g.DogY) < 0);
            var rs = g.R.S;
            sbyte dx = g.DogX, dy = g.DogY;
            g.DogFlags = (byte)(g.DogFlags & ~(Game.DogOn | Game.DogSpawned));
            g.PlaceDog();
            Assert.True(g.R.S == rs && g.DogX == dx && g.DogY == dy); // osobny generator, ten sam wynik
        }
        Assert.Equal(155, with); // jak core_tests.cpp (ten sam skrót seeda)
        Assert.Equal(with, placed);
        Assert.InRange(with, 400 * D.DogChancePct / 150, 400 * D.DogChancePct * 3 / 200);
    }

    [Fact]
    public void PettingGivesXpOnceThenSwaps()
    {
        var g = TestData.Arena(0);
        g.DogX = 8;
        g.DogY = 7;
        g.DogFlags = Game.DogOn | Game.DogSpawned;
        var xp = g.RunXp;
        Assert.True(g.PlayerMove(1, 0) && g.Hero.X == 7 && g.RunXp == xp + D.DogXp && g.DogPets == 1 && (g.DogFlags & Game.DogPetted) != 0);
        Assert.True(g.PlayerMove(1, 0) && g.Hero.X == 8 && g.DogX == 7 && g.RunXp == xp + D.DogXp && g.DogPets == 2);
        Assert.True(g.PlayerMove(-1, 0) && g.Hero.X == 7 && g.DogX == 8 && g.DogPets == 3);
        Assert.True(g.Occupied(8, 7) && g.EnemyAt(8, 7) < 0);
    }

    [Fact]
    public void BuildsWithDogCountOnceAndGiveBadge()
    {
        var p = Meta.NewProfile(D);
        var g = TestData.Run(0, 5);
        Assert.False(Dog.Bank(p, g)); // bez głaskania – nic
        g.DogFlags = Game.DogPetted;
        Assert.True(Dog.Bank(p, g) && p.DogBuilds == 1 && !Dog.Bank(p, g) && Dog.Met(p));
        p.DogBuilds = (byte)(D.DogBadgeBuilds - 1);
        var h = TestData.Run(0, 6);
        h.DogFlags = Game.DogPetted;
        Assert.NotEqual(0, (Meta.CheckBadges(D, p, h) >> D.BadgePsiPrzyjaciel) & 1);
        Assert.Equal(D.DogBadgeBuilds, p.DogBuilds);
        Assert.Equal(0, (Meta.CheckBadges(D, p, h) >> D.BadgePsiPrzyjaciel) & 1);
        p.DogBuilds = 255;
        var k = TestData.Run(0, 7);
        k.DogFlags = Game.DogPetted;
        Dog.Bank(p, k);
        Assert.Equal(255, p.DogBuilds); // licznik nasycony
        Assert.Equal(D.Badges.Length - 1, D.BadgePsiPrzyjaciel); // nowa odznaka na końcu (bity profilu tylko dopisywane)
    }

    [Fact]
    public void QuipFollowsStateAndRotates()
    {
        var q = Meta.NewProfile(D);
        q.Runs = 3;
        q.Wins = 1;
        var f = TestData.Run(0, 21);
        f.Kills = 1;
        f.DogPets = 1;
        var qi = Dog.Quip(D, q, f);
        Assert.Equal(DogCond.Any, D.DogQuips[qi].When);
        for (var k = 1; k <= 8; ++k)
        {
            f.DogPets = (byte)k;
            Assert.True(Dog.Fits(q, f, D.DogQuips[Dog.Quip(D, q, f)]));
        }
        f.DogPets = 2;
        Assert.NotEqual(qi, Dog.Quip(D, q, f));
        q.Runs = 5;
        f.Kills = 0;
        var first = Array.FindIndex(D.DogQuips, x => x.When == DogCond.First);
        var seenFirst = false;
        for (var k = 1; k <= 4; ++k)
        {
            f.DogPets = (byte)k;
            seenFirst |= Dog.Quip(D, q, f) == first;
            Assert.NotEqual(DogCond.Any, D.DogQuips[Dog.Quip(D, q, f)].When);
        }
        Assert.True(first >= 0 && seenFirst);
        f.Hero.Hp = (short)(f.Hero.MaxHp / 5);
        var low = false;
        for (var k = 1; k <= 6; ++k)
        {
            f.DogPets = (byte)k;
            low |= D.DogQuips[Dog.Quip(D, q, f)].When == DogCond.HpLow;
        }
        Assert.True(low);
        foreach (var x in D.DogQuips) Assert.True(x.Lines.Length == 3 && x.Lines.All(l => l.Length <= 25));
    }

    [Fact]
    public void ProfileV19MigratesToV20()
    {
        var old = Meta.NewProfile(D);
        old.Artifacts = 3;
        old.Title = (byte)(Profile.BadgesV19 + 2);
        old.Best = 1234;
        var raw = old.ToBytes();
        Profile.MagicBytes(Profile.MagicV19).CopyTo(raw, 0);
        raw[Profile.V19Size] = 7; // śmieci w dawnym wyrównaniu
        var p = Profile.FromBytes(raw);
        Assert.Equal(0, p.DogBuilds);
        Assert.True(Meta.ProfileFix(D, p) && p.MagicIs(Profile.MagicCurrent) && Profile.MagicCurrent == "PBRL020");
        Assert.True(p.Artifacts == 3 && p.Best == 1234 && p.DogBuilds == 0);
        Assert.Equal(Profile.BadgesV19 + 2 + (D.Badges.Length - Profile.BadgesV19), p.Title);
        Assert.False(Meta.ProfileFix(D, p));
        var t = Meta.NewProfile(D);
        t.Title = 3;
        var rt = t.ToBytes();
        Profile.MagicBytes(Profile.MagicV19).CopyTo(rt, 0);
        var pt = Profile.FromBytes(rt);
        Assert.True(Meta.ProfileFix(D, pt) && pt.Title == 3); // tytuł z odznaki – bez zmian
        p.DogBuilds = 9;
        Assert.Equal(9, Profile.FromBytes(p.ToBytes()).DogBuilds);
        Assert.Equal(Profile.Size, p.ToBytes().Length);
    }

    [Fact]
    public void RunSaveKeepsDogAndLoadsOlderState()
    {
        var g = TestData.Run(2, 9);
        g.DogX = 5;
        g.DogY = 6;
        g.DogFlags = Game.DogOn | Game.DogPetted;
        g.DogPets = 4;
        var s = RunSave.Make(g);
        Assert.True(s.Valid(D));
        var l = s.Load(D);
        Assert.True(l.DogX == 5 && l.DogY == 6 && l.DogPets == 4 && l.DogHere);
        var data = g.ToBytes()[..^RunSave.DogTail]; // zapis PBRUN19 sprzed Kawki
        var o = new RunSave { Magic = Profile.MagicBytes(RunSave.RunMagic), Size = (uint)data.Length, Data = data, Checksum = RunSave.Fnv1a(data) };
        Assert.True(o.Valid(D));
        var lo = o.Load(D);
        Assert.True(!lo.DogHere && lo.DogX == -1 && lo.DogPets == 0 && lo.Stage == g.Stage);
    }
}
