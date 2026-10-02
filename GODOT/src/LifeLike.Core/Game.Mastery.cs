namespace LifeLike.Core;

/// <summary>v0.21.52 cz. b (#45): mistrzostwo zawodu w budowie – wariant mocy i broń mistrza (port z core.h).</summary>
public sealed partial class Game
{
    /// <summary>Wariant mocy: + do siły mocy (jak premie zawodu z efektem power; Majster – moc pożyczona).</summary>
    public int MasteryPower() => (Bonus.Mastery & MasteryBit.Power) != 0 ? D.MasteryClasses[Cls].Power : 0;

    /// <summary>Wariant mocy: + tury odnowienia (ujemne = szybciej).</summary>
    public int MasteryCooldown() => (Bonus.Mastery & MasteryBit.Power) != 0 ? D.MasteryClasses[Cls].Cooldown : 0;

    /// <summary>Broń mistrza: złoty błysk przy krycie (warstwa Godota).</summary>
    public bool MasterWeapon() => (Bonus.Mastery & MasteryBit.Weapon) != 0;

    /// <summary>Broń mistrza: kryt +% tylko z bronią zawodu (podniesione narzędzie jej nie ma).</summary>
    public int MasterCrit() => MasterWeapon() && WeaponOverride < 0 ? D.MasteryClasses[Cls].WeaponPerk.Value : 0;
}
