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
    /// <summary>v0.21.50: zapylony (akt III) – iskra wywoła wybuch pyłu.</summary>
    public const byte Dusty = 16;
    /// <summary>v0.21.50: zmrożony (Mróz, Suchy lód) – cios wręcz go pęka.</summary>
    public const byte Frozen = 32;
    /// <summary>v0.21.50: elita już wezwała pomoc.</summary>
    public const byte Called = 64;
}
