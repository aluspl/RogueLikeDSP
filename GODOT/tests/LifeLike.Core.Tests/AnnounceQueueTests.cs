namespace LifeLike.Core.Tests;

/// <summary>v0.23.0: ogłoszenia „z przytupem” – kolejka (jedno naraz, ~1 s), limit, bez powtórzeń, które zdarzenia
/// zasługują na ogłoszenie (Markowa / Mistrzowska broń).</summary>
public class AnnounceQueueTests
{
    private static Announcement A(string title, AnnounceKind k = AnnounceKind.LevelUp) => new(k, title, "", -1, 0);

    [Fact]
    public void ShowsOneAtATimeInOrder()
    {
        var q = new AnnounceQueue();
        Assert.Null(q.Current);
        q.Push(A("1"));
        q.Push(A("2", AnnounceKind.Boss));
        Assert.Equal("1", q.Current.Title);
        q.Update(AnnounceQueue.Duration * 0.5f);
        Assert.Equal("1", q.Current.Title);
        Assert.Equal(0.5f, q.Progress, 3);
        q.Update(AnnounceQueue.Duration * 0.6f);
        Assert.Equal("2", q.Current.Title);
        Assert.Equal(0f, q.Elapsed);
        q.Update(AnnounceQueue.Duration + 0.01f);
        Assert.Null(q.Current);
        Assert.False(q.Busy);
    }

    [Fact]
    public void HolsterSwapIsNotAFind()
    {
        // zamiana z kabury (ręka <-> kabura) to nie znalezisko: bez ogłoszenia „ARTEFAKT!” / nowej broni
        Assert.True(AnnounceQueue.IsHolsterSwap(3, 9, 9, 3));
        Assert.False(AnnounceQueue.IsHolsterSwap(3, -1, 9, -1)); // pusta kabura: nowa broń w ręku
        Assert.False(AnnounceQueue.IsHolsterSwap(3, 9, 12, 9)); // nowa broń w ręku, kabura bez zmian
        Assert.False(AnnounceQueue.IsHolsterSwap(3, 9, 9, -1)); // broń z kabury do ręki po utracie pasa – nie zamiana
    }

    [Fact]
    public void DurationIsAboutOneSecond() => Assert.InRange(AnnounceQueue.Duration, 0.8f, 1.4f);

    [Fact]
    public void QueueIsCappedKeepingNewest()
    {
        var q = new AnnounceQueue();
        for (var i = 0; i < AnnounceQueue.MaxQueued + 3; i++) q.Push(A(i.ToString(), (AnnounceKind)(i % 4)));
        Assert.Equal(AnnounceQueue.MaxQueued, q.Count);
        Assert.Equal("0", q.Current.Title); // na ekranie zostaje
    }

    [Fact]
    public void SameAnnouncementIsNotQueuedTwice()
    {
        var q = new AnnounceQueue();
        q.Push(A("Awans"));
        q.Push(A("Awans"));
        Assert.Equal(1, q.Count);
    }

    [Fact]
    public void ClearDropsEverything()
    {
        var q = new AnnounceQueue();
        q.Push(A("1"));
        q.Push(A("2", AnnounceKind.Boss));
        q.Clear();
        Assert.Null(q.Current);
        Assert.Equal(0, q.Count);
    }

    [Theory]
    [InlineData(0, false, false)]
    [InlineData(1, false, false)]
    [InlineData(2, false, true)]
    [InlineData(3, false, true)]
    [InlineData(0, true, true)]   // broń mistrza (unikat) – zawsze
    public void WeaponWorthAnnouncing(int rarity, bool master, bool worth) => Assert.Equal(worth, AnnounceQueue.WeaponWorthy(rarity, master));

    [Fact]
    public void WeaponKindIsUniqueForMasterOtherwiseBranded()
    {
        Assert.Equal(AnnounceKind.Unique, AnnounceQueue.WeaponKind(3, true));
        Assert.Equal(AnnounceKind.Unique, AnnounceQueue.WeaponKind(3, false));
        Assert.Equal(AnnounceKind.Branded, AnnounceQueue.WeaponKind(2, false));
    }

    [Fact]
    public void SwingFrameAdvancesEveryStepAndHoldsLast()
    {
        Assert.Equal(0, AnnounceQueue.SwingFrame(0f, 6));
        Assert.Equal(1, AnnounceQueue.SwingFrame(AnnounceQueue.SwingStep * 1.2f, 6));
        Assert.Equal(5, AnnounceQueue.SwingFrame(AnnounceQueue.SwingStep * 5.5f, 6));
        Assert.Equal(5, AnnounceQueue.SwingFrame(10f, 6));
        Assert.InRange(AnnounceQueue.SwingStep, 0.06f, 0.08f);
    }

    [Fact]
    public void SeekFreezesCurrentAtTime()
    {
        var q = new AnnounceQueue();
        q.Push(A("1"));
        q.Seek(0.5f);
        Assert.Equal(0.5f, q.Elapsed);
        q.Seek(AnnounceQueue.Duration + 1f); // zrzut końca: zostaje na ekranie
        Assert.Equal("1", q.Current.Title);
        Assert.True(q.Elapsed < AnnounceQueue.Duration);
    }
}
