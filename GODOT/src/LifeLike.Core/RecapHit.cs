namespace LifeLike.Core;

/// <summary>Cios w bohatera zapamiętany do podsumowania budowy (#33, core::recap_hit).</summary>
public struct RecapHit
{
    /// <summary>Problem (GameData.Enemies), -1 = brak.</summary>
    public sbyte Src;
    /// <summary>Cecha elity (GameData.Elites), -1 = zwykły.</summary>
    public sbyte Elite;
    /// <summary>RecapKind.</summary>
    public byte Kind;
    /// <summary>Etap (GameData.Stages).</summary>
    public sbyte Stage;
    public short Amount;

    public static RecapHit None => new() { Src = -1, Elite = -1, Stage = -1 };
}
