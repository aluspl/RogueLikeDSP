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
    /// <summary>ActorFlag: dziecko z podziału, już wrócił, czeka na powrót.</summary>
    public byte Flags;
    /// <summary>Stopnie wzrostu (zachowanie „grows”).</summary>
    public sbyte Grow;
    /// <summary>Odnowienie ucieczki / łatania / odepchnięcia; u czekającego na powrót – tury do powrotu.</summary>
    public sbyte Timer;

    public Actor()
    {
        DefId = -1;
    }
}
