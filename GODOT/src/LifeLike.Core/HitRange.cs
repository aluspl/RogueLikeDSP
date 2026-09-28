namespace LifeLike.Core;

/// <summary>Zakres ciosu problemu w bohatera (port core::hit_range).</summary>
public struct HitRange
{
    public int Min, Max;

    public HitRange(int min, int max)
    {
        Min = min;
        Max = max;
    }
}
