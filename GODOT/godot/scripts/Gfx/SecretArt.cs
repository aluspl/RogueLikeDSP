using Godot;

namespace LifeLike.Game.Gfx;

/// <summary>
/// v0.23.1: wejście do magazynu rysowane pikselami na licu muru (zamiast klatek 125/126 z actors.png).
/// Pęknięta ściana: poszarpana schodkowa szczelina z jasną krawędzią i wyłamanym kawałkiem przy podstawie – półprzezroczysta
/// ciemność i światło na materiale muru, więc pasuje do cegły, betonu i drewna każdego aktu; okruchy leżą na podłodze
/// pod murem. Drzwi magazynu (tylko klucz): drewniane drzwi w ościeżnicy z okuciem i kłódką z dziurką od klucza.
/// Gdy da się otworzyć – zamiast ramki delikatnie pulsuje krawędź pęknięcia albo kłódka i obrys drzwi.
/// Piksel wzoru = 2 piksele świata (jak klatki Scale2x z 16x16 GBA).
/// </summary>
public static class SecretArt
{
    /// <summary>Wzór pęknięcia 16x16 u dołu lica: K szczelina, k cień, l jasna krawędź, h wyłamana dziura, r/R okruchy.</summary>
    private static readonly string[] Crack =
    [
        "................",
        "..........K.....",
        "..........lK....",
        "...........K....",
        "........KKKKk...",
        ".......lKk......",
        ".......lK.......",
        ".......lKk......",
        "....KKKKKk......",
        "...lKk..lKk.....",
        "...lK.....Kk....",
        "..lKk......K....",
        "..lK............",
        ".lKhk...........",
        ".Khhhk..........",
        "lKhhhhk.......r.",
    ];

    /// <summary>Okruchy na podłodze tuż pod murem (2 wiersze wzoru pod licem).</summary>
    private static readonly string[] Rubble =
    [
        ".rR.r.Rr..r.....",
        "..r.....R...r...",
    ];

    /// <summary>Drzwi magazynu 16x16: F ościeżnica, K kontur, P deski, p szczelina, q jasna deska, I okucie, i nit,
    /// g kabłąk kłódki, Y mosiądz, y cień mosiądzu, o dziurka od klucza.</summary>
    private static readonly string[] Door =
    [
        "..FFFFFFFFFFFF..",
        ".FFKKKKKKKKKKFF.",
        ".FKqPpqPpqPPpKF.",
        ".FKqPpqPpqPPpKF.",
        ".FKIiIIIIIIiIKF.",
        ".FKqPpqPpqPPpKF.",
        ".FKqPpqPpgggpKF.",
        ".FKqPpqPpgPgpKF.",
        ".FKqPpqPYYYYyKF.",
        ".FKqPpqPYooYyKF.",
        ".FKqPpqPYYoYyKF.",
        ".FKqPpqPyyyyyKF.",
        ".FKIiIIIIIIiIKF.",
        ".FKqPpqPpqPPpKF.",
        ".FKqPpqPpqPPpKF.",
        "FFFFFFFFFFFFFFFF",
    ];

    private static readonly Color Gap = new(0.03f, 0.02f, 0.03f, 0.85f);
    private static readonly Color Shade = new(0f, 0f, 0f, 0.38f);
    private static readonly Color Edge = new(1f, 0.96f, 0.88f, 0.42f);
    private static readonly Color Hole = new(0.05f, 0.04f, 0.05f, 0.92f);
    private static readonly Color Crumb = new("9c8a78");
    private static readonly Color CrumbDark = new("5a4c40");
    private static readonly Color Glow = new(1f, 0.82f, 0.32f);

    private static readonly Color Frame = new("3a302a");
    private static readonly Color Outline = new("140e0c");
    private static readonly Color Plank = new("8a5a32");
    private static readonly Color Seam = new("5e3b1f");
    private static readonly Color PlankLight = new("a8743f");
    private static readonly Color Iron = new("4a4c55");
    private static readonly Color Rivet = new("9ea2ad");
    private static readonly Color Shackle = new("b4b8c2");
    private static readonly Color Brass = new("eebc3c");
    private static readonly Color BrassDark = new("a87a22");

    /// <summary>Pęknięcie w polu r (dolne 16x16 lica, piksel = r.Size.X / 16); glow 0–1 – da się otworzyć (puls krawędzi);
    /// rubble – okruchy na podłodze pod licem.</summary>
    public static void DrawCrack(CanvasItem ci, Rect2 r, float alpha, float glow, bool rubble)
    {
        var px = r.Size.X / 16f;
        Pattern(ci, Crack, r.Position, px, alpha, glow, false);
        if (rubble) Pattern(ci, Rubble, r.Position + new Vector2(0, r.Size.Y), px, alpha, glow, false);
    }

    /// <summary>Zamknięte drzwi magazynu z kłódką w polu r; glow 0–1 – gracz ma klucz (kłódka lśni, obrys drzwi).</summary>
    public static void DrawDoor(CanvasItem ci, Rect2 r, float alpha, float glow)
    {
        var px = r.Size.X / 16f;
        Pattern(ci, Door, r.Position, px, alpha, glow, true);
        if (glow <= 0f) return;
        var outline = new Rect2(r.Position + new Vector2(px, 0), new Vector2(14 * px, 15 * px));
        ci.DrawRect(outline, new Color(Glow, (0.15f + 0.35f * glow) * alpha), false, Mathf.Max(1f, px * 0.75f));
    }

    private static void Pattern(CanvasItem ci, string[] rows, Vector2 at, float px, float alpha, float glow, bool door)
    {
        for (var y = 0; y < rows.Length; y++)
        {
            var row = rows[y];
            for (var x = 0; x < row.Length; x++)
            {
                var ch = row[x];
                if (ch == '.') continue;
                var c = door ? DoorColor(ch, glow) : CrackColor(ch, glow);
                c.A *= alpha;
                ci.DrawRect(new Rect2(at.X + x * px, at.Y + y * px, px, px), c);
            }
        }
    }

    private static Color CrackColor(char ch, float glow) => ch switch
    {
        'K' => Gap,
        'k' => Shade,
        'l' => glow > 0f ? Edge.Lerp(new Color(Glow, 0.7f), 0.6f * glow) : Edge,
        'h' => Hole,
        'r' => Crumb,
        'R' => CrumbDark,
        _ => Colors.Transparent,
    };

    private static Color DoorColor(char ch, float glow) => ch switch
    {
        'F' => Frame,
        'K' => Outline,
        'P' => Plank,
        'p' => Seam,
        'q' => PlankLight,
        'I' => Iron,
        'i' => Rivet,
        'g' => Shackle,
        'Y' => glow > 0f ? Brass.Lerp(Colors.White, 0.45f * glow) : Brass,
        'y' => BrassDark,
        'o' => Outline,
        _ => Colors.Transparent,
    };
}
