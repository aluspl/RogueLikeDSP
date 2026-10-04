namespace LifeLike.Core.Data;

/// <summary>Zachowania problemów budowy (bity EnemyDef.Tags, pole "behaviors"; kolejność jak w gen_data.py).</summary>
public static class Behavior
{
    /// <summary>Strzela z odległości 2-3 w linii (prosto albo po skosie).</summary>
    public const int Ranged = 1;
    /// <summary>Po usunięciu dzieli się na dwa słabsze.</summary>
    public const int Splits = 2;
    /// <summary>Łata rannych sąsiadów (co kilka tur).</summary>
    public const int Heals = 4;
    /// <summary>Po usunięciu wybucha: czerwone pola wokół, tura na zejście.</summary>
    public const int Explodes = 8;
    /// <summary>Rośnie z czasem: +HP, co 2 stopnie +1 obrażeń.</summary>
    public const int Grows = 16;
    /// <summary>Trzyma dystans: obok bohatera odskakuje.</summary>
    public const int Flees = 32;
    /// <summary>Nie rusza się (za to twardy).</summary>
    public const int Stationary = 64;
    /// <summary>Cios odpycha bohatera o pole.</summary>
    public const int Pushes = 128;
    /// <summary>Raz wraca po usunięciu.</summary>
    public const int Returns = 256;
    public const int Count = 9;

    /// <summary>Identyfikatory w game.json, indeks = numer bitu.</summary>
    public static readonly string[] Ids = ["ranged", "splits", "heals", "explodes", "grows", "flees", "stationary", "pushes", "returns"];
}
