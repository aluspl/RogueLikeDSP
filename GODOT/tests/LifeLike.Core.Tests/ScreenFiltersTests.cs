namespace LifeLike.Core.Tests;

/// <summary>
/// core_tests.cpp (v0.21.53, test 54): filtry ekranu – klasyczny i tryby dla daltonistów zawsze, zabawowe z warunków
/// z danych (inspektor, kolekcja, kontrakt, sekret), wybór tylko po odblokowanych, baner raz, profil v17.
/// </summary>
public class ScreenFiltersTests
{
    private static GameData D => TestData.D;

    private static int Id(string name) => Array.FindIndex(D.ScreenFilters, f => f.Name == name);

    private static int Noir => Id("Noir");
    private static int Retro => Id("Retro LCD");
    private static int Neon => Id("Neon nocy");
    private static int Kwas => Id("Kwas");
    private static int Prot => Id("Protanopia");
    private static int Kontr => Id("Wysoki kontrast");

    [Fact]
    public void DataHasClassicFunAndAccessFilters()
    {
        Assert.True(D.ScreenFilters.Length == 9 && D.ScreenFilters[0].Kind == FilterKind.Classic);
        Assert.True(Noir > 0 && Retro > 0 && Neon > 0 && Kwas > 0 && Prot > 0 && Kontr > 0);
        Assert.True(D.ScreenFilters[Kwas].Motion && !D.ScreenFilters[Noir].Motion);
        Assert.All(D.ScreenFilters.Where(f => f.Kind == FilterKind.Access), f => Assert.True(f.Cues && f.Unlock.Length == 0));
        Assert.All(D.ScreenFilters.Where(f => f.Kind == FilterKind.Fun), f => Assert.True(f.Hint.Length > 0 && f.Unlock.Length is 1 or 2));
        Assert.Equal(7, D.FiltersHelpLines.Length);
    }

    [Fact]
    public void NewProfileHasClassicAndAccessibilityOnly()
    {
        var p = Meta.NewProfile(D);
        for (var f = 0; f < D.ScreenFilters.Length; f++)
            Assert.Equal(D.ScreenFilters[f].Kind != FilterKind.Fun, ScreenFilters.Unlocked(D, p, f));
        Assert.True(ScreenFilters.UnlockedCount(D, p) == 5 && !ScreenFilters.Unlocked(D, p, -1) && !ScreenFilters.Unlocked(D, p, D.ScreenFilters.Length));
        Assert.True(ScreenFilters.Announce(D, p) == 0 && ScreenFilters.Selected(D, p) == 0);
        p.Filter = (byte)Noir;
        Assert.Equal(0, ScreenFilters.Selected(D, p));
        p.Filter = 0;
        ScreenFilters.Cycle(D, p, 1);
        Assert.Equal(Prot, p.Filter);
        ScreenFilters.Cycle(D, p, -1);
        Assert.Equal(0, p.Filter);
        ScreenFilters.Cycle(D, p, -1);
        Assert.Equal(Kontr, p.Filter);
    }

    [Fact]
    public void FunFiltersUnlockFromProgress()
    {
        var p = Meta.NewProfile(D);
        p.InspectorXp = (uint)(Progress.Floor(D.InspectorLevels, 5) - 1);
        Assert.False(ScreenFilters.Unlocked(D, p, Noir));
        p.InspectorXp = (uint)Progress.Floor(D.InspectorLevels, 5);
        Assert.True(ScreenFilters.Unlocked(D, p, Noir));
        Assert.True(ScreenFilters.Announce(D, p) == 1 << Noir && ScreenFilters.Announce(D, p) == 0);
        Assert.Equal(Noir, ScreenFilters.Next(D, p, 0, 1));

        var col = D.Collections[D.ScreenFilters[Retro].Unlock[0].Value];
        for (var e = 0; e < D.Enemies.Length; e++)
            if (((col.Enemies >> e) & 1) != 0) p.KillCount[e] = (byte)col.Count;
        Assert.True(ScreenFilters.Unlocked(D, p, Retro) && ScreenFilters.Announce(D, p) == 1 << Retro);

        var a = Meta.NewProfile(D);
        a.CareerDone = (byte)(1 << D.ScreenFilters[Neon].Unlock[0].Value);
        Assert.True(ScreenFilters.Unlocked(D, a, Neon));
        var b = Meta.NewProfile(D);
        b.Secrets = (ushort)(1 << D.ScreenFilters[Neon].Unlock[1].Value);
        Assert.True(ScreenFilters.Unlocked(D, b, Neon) && !ScreenFilters.Unlocked(D, b, Noir));

        var k = Meta.NewProfile(D);
        k.InspectorXp = (uint)Progress.Floor(D.InspectorLevels, 20);
        Assert.True(ScreenFilters.Unlocked(D, k, Kwas) && ScreenFilters.Unlocked(D, k, Noir) && !ScreenFilters.Unlocked(D, k, Neon));
        var w = Meta.NewProfile(D);
        w.Secrets = (ushort)(1 << D.ScreenFilters[Kwas].Unlock[1].Value);
        Assert.True(ScreenFilters.Unlocked(D, w, Kwas));
        Assert.StartsWith("Inspektor 20 albo Sekret: ", ScreenFilters.UnlockLabel(D, Kwas));
        Assert.Equal("Kolekcja: Stan surowy", ScreenFilters.UnlockLabel(D, Retro));
        Assert.StartsWith("Zawsze", ScreenFilters.UnlockLabel(D, Prot));
    }

    [Fact]
    public void ProfileV17MigratesFromV16AndOlder()
    {
        var p = Meta.NewProfile(D);
        p.Wins = 7;
        p.Best = 321;
        p.CareerDone = 1;
        p.InspectorXp = (uint)Progress.Floor(D.InspectorLevels, 6);
        p.Magic = Profile.MagicBytes(Profile.MagicV16);
        var raw = p.ToBytes();
        raw[363] = 0xAB;  // śmieci w dawnym wyrównaniu v16
        raw[376] = 0x34;
        raw[377] = 0x12;
        var v = Profile.FromBytes(raw);
        Assert.True(Meta.ProfileFix(D, v) && v.MagicIs(Profile.MagicCurrent) && v.Wins == 7 && v.Best == 321 && v.CareerDone == 1);
        Assert.True(v.Filter == 0 && v.FiltersSeen == 0 && ScreenFilters.Selected(D, v) == 0);
        Assert.True(ScreenFilters.Announce(D, v) == 1 << Noir && !Meta.ProfileFix(D, v));
        var bytes = v.ToBytes();
        Assert.True(bytes[363] == 0 && bytes[376] == (byte)(1 << Noir) && Profile.FromBytes(bytes).FiltersSeen == 1 << Noir);

        var o = Meta.NewProfile(D);
        o.Wins = 2;
        o.Magic = Profile.MagicBytes(Profile.MagicV15);
        var ro = o.ToBytes();
        for (var i = Profile.V15Size; i < ro.Length; ++i) ro[i] = 0xEE;
        var q = Profile.FromBytes(ro);
        Assert.True(Meta.ProfileFix(D, q) && q.Filter == 0 && q.FiltersSeen == 0 && q.CareerWins[0] == 2 && q.MagicIs(Profile.MagicCurrent));

        var n = Meta.NewProfile(D);
        n.Filter = (byte)Prot;
        Assert.True(!Meta.ProfileFix(D, n) && ScreenFilters.Selected(D, n) == Prot);
    }
}
