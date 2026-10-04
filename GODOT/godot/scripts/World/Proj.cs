using Godot;
using LifeLike.Game.Gfx;
using LifeLike.Game.Settings;

namespace LifeLike.Game.World;

/// <summary>
/// v0.21.54: rzutowanie siatka -> piksele świata dla obu widoków mapy (tylko rysowanie, logika siatki bez zmian).
/// Płaski: pole 32x32, mur to bryła 1,5 pola (wyższe ściany) – wierzch 48 px nad podstawą, pod nim wysokie lico. Widok 3/4:
/// wiersz 24 px (podłoga widziana lekko z góry), mur 36 px (1,5 wiersza), postacie o 4 px wyżej i z rzuconym cieniem.
/// Mur w wierszu y zasłania całe pole (x, y-1) i dolną połowę pola (x, y-2) – stąd półprzezroczystość muru (WallLayer).
/// </summary>
public static class Proj
{
    public const int W = Assets.Cell;

    public static bool ThreeQuarter => GameSettings.ThreeQuarter;

    /// <summary>Wysokość wiersza mapy w pikselach świata.</summary>
    public static int RowH => ThreeQuarter ? 24 : W;

    /// <summary>Wysokość bryły muru (o tyle wyżej rysowany jest wierzch, tyle ma lico).</summary>
    public static int WallH => ThreeQuarter ? 36 : 48;

    /// <summary>Przesunięcie rysowania postaci (stopy w dolnej części wiersza 3/4).</summary>
    public static float SpriteLift => ThreeQuarter ? -4f : 0f;

    public static Rect2 CellRect(int x, int y) => new(x * W, y * RowH, W, RowH);

    public static Vector2 Center(int x, int y) => new(x * W + W / 2, y * RowH + RowH / 2);

    /// <summary>Pole podłogi pod punktem świata (bez uwzględniania murów i postaci).</summary>
    public static Vector2I Floor(Vector2 p) => new(Mathf.FloorToInt(p.X / W), Mathf.FloorToInt(p.Y / RowH));
}
