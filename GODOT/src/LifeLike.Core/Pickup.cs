namespace LifeLike.Core;

/// <summary>
/// Znajdźka na mapie. Arg: indeks narzędzia albo slot * 3 + jakość sprzętu; Trait: cecha sprzętu (GameData.GearTraits).
/// </summary>
public struct Pickup
{
    public sbyte X, Y;
    public PickupType Type;
    public bool Active;
    public byte Arg;
    public byte Trait;
    /// <summary>v0.23.0: ulepszenia upuszczonej broni z kabury (Game.PackWeaponExtra): poziom, cecha, ładunki, wybór cechy; 0 = zwykła znajdźka.</summary>
    public ushort Extra;
    /// <summary>v0.23.0: profity broni (maska puli GameData.WeaponPerkPool) wylosowane przy powstaniu znajdźki.</summary>
    public byte Perks;

    public Pickup(int x, int y, PickupType type, bool active, int arg = 0, int trait = 0)
    {
        X = (sbyte)x;
        Y = (sbyte)y;
        Type = type;
        Active = active;
        Arg = (byte)arg;
        Trait = (byte)trait;
    }
}
