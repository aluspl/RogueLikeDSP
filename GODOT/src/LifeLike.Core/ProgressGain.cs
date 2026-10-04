namespace LifeLike.Core;

/// <summary>Co dała część budowy (core::progress_gain): dośw. inspektora i mistrzostwa, poziomy przed / po, Respekt z poziomów.</summary>
public sealed class ProgressGain
{
    public int Gained;
    public int Cls = -1;
    public int InspBefore, InspAfter;
    public int MasteryBefore, MasteryAfter;
    public int Respect;
}
