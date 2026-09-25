namespace LifeLike.Core;

/// <summary>xorshift32 – ten sam algorytm co w GBA (core::rng), więc seed daje identyczną grę.</summary>
public struct Rng
{
    public const uint DefaultSeed = 2463534242u;

    public uint S;

    public Rng()
    {
        S = DefaultSeed;
    }

    public void Seed(uint v) => S = v != 0 ? v : DefaultSeed;

    public uint Next()
    {
        S ^= S << 13;
        S ^= S >> 17;
        S ^= S << 5;
        return S;
    }

    /// <summary>Liczba z przedziału [lo, hi] (modulo na uint32 jak w C++).</summary>
    public int Range(int lo, int hi) => lo + (int)(Next() % (uint)(hi - lo + 1));
}
