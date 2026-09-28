namespace LifeLike.Core;

/// <summary>Bity Actor.Flags (zachowania problemów).</summary>
public static class ActorFlag
{
    /// <summary>Dziecko z podziału (samo się już nie dzieli).</summary>
    public const byte Child = 1;
    /// <summary>Już raz wrócił (zachowanie „returns”).</summary>
    public const byte Returned = 2;
    /// <summary>Usunięty, czeka na powrót.</summary>
    public const byte Reviving = 4;
    /// <summary>Boss w drugiej fazie (Decyzja odmowna: Odwołanie).</summary>
    public const byte Phase = 8;
}
