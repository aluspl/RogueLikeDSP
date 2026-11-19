using LifeLike.Core.Data;

namespace LifeLike.Core;

/// <summary>
/// v0.23.0: pola wzoru ataku (wzory z docs/v0.23, bohater zwrócony w górę, obrót razem z kierunkiem). Kierunek (dx, dy) ∈ {-1,0,1}²,
/// skos obraca wzór o 45°. Kolejność pól: od bohatera na zewnątrz (linie: promień po promieniu), więc "pierwszy cel" to pierwszy trafiony.
/// </summary>
public static class HitPattern
{
    /// <summary>Największa liczba pól wzoru (Line4 o zasięgu 4 = 16, Throw3x3 = 9).</summary>
    public const int MaxCells = 32;

    /// <summary>
    /// Pola wzoru dla bohatera w (hx, hy) zwróconego w (dx, dy); zwraca liczbę pól zapisanych w xs/ys. Throw3x3: środek w odległości
    /// range wzdłuż kierunku (przy ataku Game używa pola celu). Push i Single: jedno pole przed bohaterem. Pola poza planszą pomija.
    /// </summary>
    public static int Cells(PatternKind k, int range, int hx, int hy, int dx, int dy, Span<int> xs, Span<int> ys)
    {
        var n = 0;
        if (dx == 0 && dy == 0) return 0;
        var diag = dx != 0 && dy != 0;
        var r = Math.Max(1, range);
        switch (k)
        {
            case PatternKind.Single:
            case PatternKind.Push:
                Add(xs, ys, ref n, hx + dx, hy + dy);
                break;
            case PatternKind.Shot:
            case PatternKind.Line:
                Ray(xs, ys, ref n, hx, hy, dx, dy, r);
                break;
            case PatternKind.LineBoth:
                Ray(xs, ys, ref n, hx, hy, dx, dy, r);
                Ray(xs, ys, ref n, hx, hy, -dx, -dy, r);
                break;
            case PatternKind.Line4:
                Ray(xs, ys, ref n, hx, hy, 0, -1, r);
                Ray(xs, ys, ref n, hx, hy, 1, 0, r);
                Ray(xs, ys, ref n, hx, hy, 0, 1, r);
                Ray(xs, ys, ref n, hx, hy, -1, 0, r);
                break;
            case PatternKind.Arc:
                Add(xs, ys, ref n, hx + dx, hy + dy);
                if (diag)
                {
                    Add(xs, ys, ref n, hx, hy + dy);
                    Add(xs, ys, ref n, hx + dx, hy);
                }
                else
                {
                    Add(xs, ys, ref n, hx + dx - (dy), hy + dy + (dx));
                    Add(xs, ys, ref n, hx + dx + (dy), hy + dy - (dx));
                }
                break;
            case PatternKind.Around:
                for (var oy = -1; oy <= 1; ++oy)
                {
                    for (var ox = -1; ox <= 1; ++ox)
                    {
                        if (ox != 0 || oy != 0) Add(xs, ys, ref n, hx + ox, hy + oy);
                    }
                }
                break;
            case PatternKind.Cross:
                Add(xs, ys, ref n, hx + dx, hy + dy);
                if (diag)
                {
                    Add(xs, ys, ref n, hx + dx, hy - dy);
                    Add(xs, ys, ref n, hx - dx, hy + dy);
                    Add(xs, ys, ref n, hx - dx, hy - dy);
                }
                else
                {
                    Add(xs, ys, ref n, hx - (dy), hy + (dx));
                    Add(xs, ys, ref n, hx + (dy), hy - (dx));
                    Add(xs, ys, ref n, hx - dx, hy - dy);
                }
                break;
            case PatternKind.Spray: // 1 pole przed bohaterem, dalej rząd 3 pól
                Add(xs, ys, ref n, hx + dx, hy + dy);
                if (diag)
                {
                    Add(xs, ys, ref n, hx + 2 * dx, hy + 2 * dy);
                    Add(xs, ys, ref n, hx + dx, hy + 2 * dy);
                    Add(xs, ys, ref n, hx + 2 * dx, hy + dy);
                }
                else
                {
                    var cx = hx + 2 * dx;
                    var cy = hy + 2 * dy;
                    Add(xs, ys, ref n, cx, cy);
                    Add(xs, ys, ref n, cx - (dy), cy + (dx));
                    Add(xs, ys, ref n, cx + (dy), cy - (dx));
                }
                break;
            case PatternKind.Throw3x3:
                Block(xs, ys, ref n, hx + dx * r, hy + dy * r);
                break;
        }
        return n;
    }

    /// <summary>Pola obszaru 3x3 wokół (cx, cy).</summary>
    public static int Area3x3(int cx, int cy, Span<int> xs, Span<int> ys)
    {
        var n = 0;
        Block(xs, ys, ref n, cx, cy);
        return n;
    }

    /// <summary>Czy wzór składa się z promieni (zatrzymują się na pierwszej ścianie).</summary>
    public static bool IsRay(PatternKind k) => k is PatternKind.Line or PatternKind.Shot or PatternKind.LineBoth or PatternKind.Line4;

    private static void Ray(Span<int> xs, Span<int> ys, ref int n, int hx, int hy, int dx, int dy, int len)
    {
        for (var i = 1; i <= len; ++i) Add(xs, ys, ref n, hx + dx * i, hy + dy * i);
    }

    private static void Block(Span<int> xs, Span<int> ys, ref int n, int cx, int cy)
    {
        for (var oy = -1; oy <= 1; ++oy)
        {
            for (var ox = -1; ox <= 1; ++ox) Add(xs, ys, ref n, cx + ox, cy + oy);
        }
    }

    private static void Add(Span<int> xs, Span<int> ys, ref int n, int x, int y)
    {
        if (x < 0 || y < 0 || x >= Level.W || y >= Level.H || n >= xs.Length || n >= ys.Length) return;
        xs[n] = x;
        ys[n] = y;
        ++n;
    }
}
