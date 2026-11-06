namespace LifeLike.Core;

/// <summary>
/// v0.23.0: ustawienia bota z testów balansu. DefaultWeaponOnly – bot gra tylko bronią domyślną zawodu: omija znalezione narzędzia,
/// odrzuca wymianę i nie zmienia broni z pasa (tryb B swojej broni nadal używa). Weapons = false – bot jak w v0.22.1
/// (bez trybu B, wzorów, zamiany broni i oceny znajdziek) – do porównań.
/// </summary>
public sealed class BotOptions
{
    /// <summary>Bot grający jak gracz: tryb B, ustawianie pod wzór, zamiana broni z pasa, wybór znalezionych broni.</summary>
    public static readonly BotOptions Default = new();

    /// <summary>Bot tylko z bronią domyślną (spec. 4.7 pkt 2–3).</summary>
    public static readonly BotOptions DefaultOnly = new() { DefaultWeaponOnly = true };

    public bool DefaultWeaponOnly { get; init; }

    public bool Weapons { get; init; } = true;
}
