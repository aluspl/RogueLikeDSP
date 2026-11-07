namespace LifeLike.Core;

/// <summary>
/// v0.23.0: broń w ręce bohatera na mapie (art/weapons/weapons.json). Kierunek = ostatni ruch / atak (kolejność kolumn
/// arkusza: down, up, left, right). Bohater nie ma klatek kierunkowych – w lewo jest odbijany, a broń rysuje się w jego
/// transformacji, więc „left” bierze kolumnę „right”. Przy „up” broń pod postacią. Wymach: 3 krótkie klatki obrotu.
/// </summary>
public static class WeaponPose
{
    public const int Down = 0, Up = 1, Left = 2, Right = 3;

    /// <summary>Prawa dłoń bohatera w klatce 32x32 (heroHand.godot w weapons.json).</summary>
    public const int HandX = 24, HandY = 22;

    /// <summary>Czas klatki wymachu (s); 3 klatki = 0,15 s – krócej niż ruch na sąsiednie pole.</summary>
    public const float SwingStep = 0.05f;

    private static readonly float[] SwingAngles = [-60f, 30f, 10f];

    /// <summary>Kierunek z przesunięcia (skos – poziomo); bez przesunięcia – poprzedni.</summary>
    public static int Dir(int dx, int dy, int prev)
    {
        if (dx != 0) return dx < 0 ? Left : Right;
        if (dy != 0) return dy < 0 ? Up : Down;
        return prev;
    }

    /// <summary>Odbicie bohatera po zwrocie: w lewo – tak, w prawo – nie, góra/dół – bez zmian.</summary>
    public static bool Flip(int dir, bool flip) => dir == Left || (dir != Right && flip);

    /// <summary>Kierunek w transformacji bohatera (odbitej przy „left”).</summary>
    public static int LocalDir(int dir) => dir == Left ? Right : dir;

    /// <summary>Kolumna arkusza: rzadkość * 4 + kierunek; broń mistrza ma tylko kolumny Mistrzowskiej.</summary>
    public static int Column(int rarity, bool master, int dir) => (master ? 3 : rarity) * 4 + LocalDir(dir);

    /// <summary>Przy „up” broń za postacią (pod nią i pod sprzętem).</summary>
    public static bool Behind(int dir) => dir == Up;

    /// <summary>Lewy górny róg klatki broni względem klatki bohatera: piksel chwytu (gripGodot) na dłoni.</summary>
    public static (int X, int Y) Origin(int gripX, int gripY) => (HandX - gripX, HandY - gripY);

    /// <summary>Obrót broni wokół chwytu t sekund po ataku (stopnie); poza wymachem i przy ograniczonym ruchu 0.</summary>
    public static float SwingAngle(float t, bool reduceMotion)
    {
        if (reduceMotion || t < 0f) return 0f;
        var k = (int)(t / SwingStep);
        return k < SwingAngles.Length ? SwingAngles[k] : 0f;
    }
}
