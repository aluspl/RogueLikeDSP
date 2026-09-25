namespace LifeLike.Core;

/// <summary>Znajdźka na mapie. Arg: indeks narzędzia albo slot * 3 + jakość sprzętu.</summary>
public struct Pickup
{
    public sbyte X, Y;
    public PickupType Type;
    public bool Active;
    public byte Arg;

    public Pickup(int x, int y, PickupType type, bool active, int arg = 0)
    {
        X = (sbyte)x;
        Y = (sbyte)y;
        Type = type;
        Active = active;
        Arg = (byte)arg;
    }
}
