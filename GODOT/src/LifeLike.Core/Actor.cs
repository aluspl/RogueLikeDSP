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
    /// <summary>v0.21.50: cecha elity (GameData.Elites), -1 = zwykły problem.</summary>
    public sbyte Elite;
    /// <summary>v0.21.50: tury mokrego (kałuża, Zawór, Wąż ogrodowy); problem wodny jest mokry zawsze.</summary>
    public sbyte Wet;

    public Actor()
    {
        DefId = -1;
        Elite = -1;
    }
}
