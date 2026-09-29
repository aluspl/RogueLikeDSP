namespace LifeLike.Core;

/// <summary>Co się działo na etapie – oś czasu podsumowania (#33, core::recap_flag; bity Game.StageFlags).</summary>
public static class RecapFlag
{
    public const byte Secret = 1;
    public const byte Upgrade = 2;
    public const byte Elite = 4;
    public const byte Boss = 8;
    public const byte Combo = 16;
    public const byte Synergy = 32;
    public const byte EventBoon = 64;
}
