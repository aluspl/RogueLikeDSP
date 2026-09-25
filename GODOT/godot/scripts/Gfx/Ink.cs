using Godot;

namespace LifeLike.Game;

/// <summary>
/// Kolor tekstu pikselowego fontu: wypełnienie + obrys (cień prawy-dolny z fontu GBA).
/// Odpowiednik palet font_*.bmp z GBA. Obrys przezroczysty = bez cienia (tekst na jasnych kartach telefonu).
/// </summary>
public readonly struct Ink
{
    public readonly Color Fill;
    public readonly Color Edge;

    public Ink(Color fill, Color edge)
    {
        Fill = fill;
        Edge = edge;
    }

    public Ink WithAlpha(float a) => new(new Color(Fill, Fill.A * a), new Color(Edge, Edge.A * a));

    private static readonly Color MapEdge = new("14141e");

    // telefon (jasne tło, bez cienia)
    public static readonly Ink Dark = new(Pal.Text, Colors.Transparent);
    public static readonly Ink Dim = new(Pal.Dim, Colors.Transparent);
    public static readonly Ink Brand = new(Pal.Brand, Colors.Transparent);
    public static readonly Ink Prog = new(new Color("b46e05"), Colors.Transparent);
    public static readonly Ink Done = new(new Color("047857"), Colors.Transparent);
    public static readonly Ink Late = new(new Color("b91c1c"), Colors.Transparent);
    public static readonly Ink White = new(Colors.White, Colors.Transparent);

    // mapa i ekrany na ciemnym tle (z cieniem)
    public static readonly Ink Map = new(new Color("fafafa"), MapEdge);
    public static readonly Ink MapBad = new(new Color("ff7878"), MapEdge);
    public static readonly Ink MapGood = new(new Color("82eb96"), MapEdge);
    public static readonly Ink MapLoot = new(new Color("ffd75a"), MapEdge);
    public static readonly Ink MapDim = new(new Color("b8b4d8"), MapEdge);
    public static readonly Ink MapBrand = new(new Color("ff9a66"), MapEdge);
    public static readonly Ink OnBrand = new(Colors.White, new Color("3a2a99"));
}
