namespace LifeLike.Core;

/// <summary>Bohater albo problem budowy. DefId = indeks w GameData.Enemies, -1 = gracz.</summary>
public struct Actor
{
    public sbyte X, Y;
    public short Hp, MaxHp;
    public sbyte DefId;
    public bool Alive, Awake;
    /// <summary>Tury ogłuszenia (Odprawa).</summary>
    public sbyte Stun;

    public Actor()
    {
        DefId = -1;
    }
}
