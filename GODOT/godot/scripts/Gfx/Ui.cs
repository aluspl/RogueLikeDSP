using System.Collections.Generic;
using Godot;

namespace LifeLike.Game;

/// <summary>Pomocnicze rysowanie interfejsu w stylu pikselowym: zaokrąglone prostokąty bez wygładzania, gradient, pasy.</summary>
public static class Ui
{
    private static readonly Dictionary<(Color, int, Color), StyleBoxFlat> Boxes = new();

    public static void ClearCache() => Boxes.Clear();

    public static StyleBoxFlat Box(Color fill, int radius) => Box(fill, radius, Colors.Transparent);

    /// <summary>Zaokrąglony prostokąt (StyleBoxFlat bez antyaliasingu = ostre piksele jak kafle rogów na GBA).</summary>
    public static StyleBoxFlat Box(Color fill, int radius, Color border)
    {
        var key = (fill, radius, border);
        if (Boxes.TryGetValue(key, out var sb)) return sb;
        sb = new StyleBoxFlat { BgColor = fill, AntiAliasing = false };
        sb.SetCornerRadiusAll(radius);
        if (border.A > 0)
        {
            sb.BorderColor = border;
            sb.SetBorderWidthAll(1);
        }
        Boxes[key] = sb;
        return sb;
    }

    /// <summary>Pionowy gradient fioletu marki (make_gradient na GBA: HDMA zmienia kolor tła co linię), w pasach 2 px.</summary>
    public static void VioletGradient(CanvasItem ci, Rect2 r)
    {
        var top = new Color(124 / 255f, 98 / 255f, 1f);
        var bottom = new Color(44 / 255f, 30 / 255f, 150 / 255f);
        for (var y = 0; y < r.Size.Y; y += 2)
        {
            var t = y / r.Size.Y;
            ci.DrawRect(new Rect2(r.Position.X, r.Position.Y + y, r.Size.X, 2), top.Lerp(bottom, t));
        }
    }

    /// <summary>Pas ostrzegawczy placu budowy (pomarańczowe skosy na ciemnym tle, jak na ekranie tytułowym GBA); na całą szerokość ekranu.</summary>
    public static void WarningStripe(CanvasItem ci, Rect2 r, float scroll = 0)
    {
        ci.DrawRect(r, Pal.Text);
        var h = r.Size.Y;
        for (var x = -2 * h + (scroll % (2 * h)); x < r.Size.X + h; x += 2 * h)
        {
            var x0 = r.Position.X + x;
            Vector2[] pts = [new(x0, r.End.Y), new(x0 + h, r.End.Y), new(x0 + 2 * h, r.Position.Y), new(x0 + h, r.Position.Y)];
            ci.DrawColoredPolygon(pts, Pal.Accent);
        }
        ci.DrawRect(new Rect2(r.Position.X, r.Position.Y - 2, r.Size.X, 2), Pal.Text);
    }
}
