using Godot;

namespace LifeLike.Game.Gfx;

/// <summary>
/// Ikony materiałów (cement, stal, drewno) rysowane prostokątami 12x12 px: worek cementu, pręty zbrojeniowe, deski.
/// HUD, telefon (Sprzęt, Brygada i naprawy, Hurtownia). Indeks jak GameData.Materials.
/// </summary>
public static class MaterialIcon
{
    public const int Size = 12;

    private static readonly Color Outline = new(0.12f, 0.1f, 0.14f);

    public static void Draw(CanvasItem c, int m, Vector2 at)
    {
        var x = Mathf.Round(at.X);
        var y = Mathf.Round(at.Y);
        switch (m)
        {
            case 0: // worek cementu: szary worek z zawiązanym brzegiem
                c.DrawRect(new Rect2(x + 1, y + 2, 10, 10), Outline);
                c.DrawRect(new Rect2(x + 2, y + 3, 8, 8), new Color(0.72f, 0.72f, 0.7f));
                c.DrawRect(new Rect2(x + 2, y + 3, 8, 2), new Color(0.55f, 0.55f, 0.53f));
                c.DrawRect(new Rect2(x + 4, y + 1, 4, 2), Outline);
                c.DrawRect(new Rect2(x + 3, y + 7, 6, 2), new Color(0.92f, 0.78f, 0.25f)); // naklejka
                break;
            case 1: // stal: trzy pręty
                for (var k = 0; k < 3; k++)
                {
                    var r = new Rect2(x, y + 1 + k * 4, 12, 3);
                    c.DrawRect(r, Outline);
                    c.DrawRect(new Rect2(r.Position + new Vector2(1, 1), new Vector2(10, 1)), new Color(0.62f, 0.72f, 0.82f));
                }
                break;
            default: // drewno: dwie deski ze słojami
                for (var k = 0; k < 2; k++)
                {
                    var r = new Rect2(x, y + 1 + k * 6, 12, 5);
                    c.DrawRect(r, Outline);
                    c.DrawRect(new Rect2(r.Position + new Vector2(1, 1), new Vector2(10, 3)), new Color(0.78f, 0.52f, 0.28f));
                    c.DrawRect(new Rect2(r.Position + new Vector2(3 + k * 3, 2), new Vector2(4, 1)), new Color(0.58f, 0.36f, 0.18f));
                }
                break;
        }
    }
}
