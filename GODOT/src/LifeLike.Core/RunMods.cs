namespace LifeLike.Core;

/// <summary>Premie z meta-progresji (sklep „Szkolenia”), stałe przez całą budowę.</summary>
public struct RunMods
{
    public int Hp, Def, Dmg, Coffee, Pickups;
    /// <summary>Narzędzia, które mogą wypaść z wrogów (bitmaska).</summary>
    public int Tools;

    public static RunMods Default(Data.GameData d) => new() { Tools = d.StartToolsMask };
}
