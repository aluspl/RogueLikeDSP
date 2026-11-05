namespace LifeLike.Core.Data;

/// <summary>Broń; Elem (v0.21.50): prąd (Próbnik), iskra (Szlifierka, Pistolet do kotew). v0.21.51 cz. 2: Crit = kryt +%
/// (Poziomica mistrza), Knockback = cios wręcz odpycha (Młot Zenka), Reveal = magazyn na podglądzie mapy.
/// v0.23.0: Hit = typy ciosu (1-2), ModeA/ModeB = tryby ataku, Master = Mistrzowska (stała rzadkość).
/// Bez trybów w danych: A = Single (zasięg 1) albo Shot (zasięg broni), B = pusty tryb z odnowieniem 1.</summary>
public sealed record WeaponDef(string Id, string Name, int MinDamage, int MaxDamage, int Range, Stat ScalesWith, Element Elem = Element.None,
    int Crit = 0, bool Knockback = false, bool Reveal = false, HitType[] Hit = null, WeaponMode ModeA = null, WeaponMode ModeB = null,
    bool Master = false)
{
    /// <summary>Typy ciosu; zawsze co najmniej jeden (domyślnie obuch).</summary>
    public HitType[] Hit { get; init; } = Hit is { Length: > 0 } ? Hit : [HitType.Blunt];

    /// <summary>Tryb A; domyślnie wg zasięgu broni (dzisiejsze zachowanie).</summary>
    public WeaponMode ModeA { get; init; } = ModeA ?? new WeaponMode(Range == 1 ? PatternKind.Single : PatternKind.Shot, Range, 100, ModeEffect.None, 0, 0, 0);

    /// <summary>Tryb B; domyślnie jak A z odnowieniem 1.</summary>
    public WeaponMode ModeB { get; init; } = ModeB ?? new WeaponMode(Range == 1 ? PatternKind.Single : PatternKind.Shot, Range, 100, ModeEffect.None, 0, 0, 1);
}
