namespace LifeLike.Core;

/// <summary>
/// v0.21.52 cz. b: mistrzostwo zawodu w budowie (RunMods.Mastery, core::mastery_bit) – ustawia profil (Progress.MasteryBits).
/// </summary>
public static class MasteryBit
{
    /// <summary>Wariant mocy (siła i tury odnowienia z danych).</summary>
    public const int Power = 1;
    /// <summary>Broń mistrza: kryt z bronią zawodu, złoty błysk przy krycie.</summary>
    public const int Weapon = 2;
    /// <summary>Premia mistrzostwa w ofercie po etapie.</summary>
    public const int Boon = 4;
    /// <summary>v0.21.52 cz. c: wartość Siły rozpędu (drzewko) w bitach 8-11 RunMods.Mastery.</summary>
    public const int FirstHitShift = 8;
}
