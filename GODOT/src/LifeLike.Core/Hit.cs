namespace LifeLike.Core;

/// <summary>Rodzaj trafienia (core::hit_kind): zwykłe, krytyczne (x2), unik bohatera.</summary>
public enum HitKind : byte { Normal, Crit, Dodge }

/// <summary>Zdarzenie trafienia (liczby obrażeń nad polem).</summary>
public struct Hit
{
    public sbyte X, Y;
    public short Amount;
    public bool OnHero;
    public HitKind Kind;
}
