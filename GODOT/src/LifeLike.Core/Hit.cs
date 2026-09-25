namespace LifeLike.Core;

/// <summary>Zdarzenie trafienia (liczby obrażeń nad polem).</summary>
public struct Hit
{
    public sbyte X, Y;
    public short Amount;
    public bool OnHero;
}
