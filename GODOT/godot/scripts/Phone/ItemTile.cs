using Godot;
using LifeLike.Core;
using LifeLike.Game.Gfx;

namespace LifeLike.Game.Phone;

/// <summary>
/// v0.23.2 (UI 2): kafel przedmiotu (plecak, slot postaci) – biała karta z ramką w kolorze rzadkości, obrazek na środku,
/// ▲ / ▼ w rogu (lepsze / słabsze od noszonego), obwódka fokusu klawiatury. Pusty kafel – jasne tło bez ramki.
/// </summary>
public static class ItemTile
{
    /// <summary>Kafel z przedmiotem it (Empty = pusty); cmp: 1 lepsze, -1 słabsze, 0 bez strzałki; locked – slot niedostępny.</summary>
    public static void Draw(PhonePainter p, LifeLike.Core.Data.GameData d, Rect2 r, BackpackItem it, int cmp, bool focus, bool locked = false)
    {
        if (focus) p.C.DrawStyleBox(Ui.Box(Colors.Transparent, 9, Pal.Brand), r.Grow(3));
        if (it.Empty)
        {
            p.C.DrawStyleBox(Ui.Box(locked ? Pal.Bg : Pal.Group, 7, Pal.Border), r);
            if (locked) Lock(p, r);
            p.Reach(r.End.Y);
            return;
        }
        var rc = ItemArt.Rarity(d, it);
        p.C.DrawStyleBox(Ui.Box(rc, 7), r);
        p.C.DrawStyleBox(Ui.Box(Pal.Card, 5), r.Grow(-2));
        ItemArt.Draw(p.C, d, it, r.Grow(-4));
        if (cmp != 0) Arrow(p, new Vector2(r.End.X - 9, r.Position.Y + 9), cmp > 0);
        p.Reach(r.End.Y);
    }

    /// <summary>Strzałka porównania w kółku (zielona ▲ / czerwona ▼) o środku c.</summary>
    public static void Arrow(PhonePainter p, Vector2 c, bool up)
    {
        var col = up ? Ink.Done.Fill : Ink.Late.Fill;
        p.C.DrawCircle(c, 6.5f, Pal.Card);
        p.C.DrawCircle(c, 5.5f, col);
        Vector2[] pts = up
            ? [c + new Vector2(-3.5f, 2), c + new Vector2(3.5f, 2), c + new Vector2(0, -3)]
            : [c + new Vector2(-3.5f, -2), c + new Vector2(3.5f, -2), c + new Vector2(0, 3)];
        p.C.DrawColoredPolygon(pts, Colors.White);
    }

    /// <summary>Kłódka: slot jeszcze niedostępny (buty / pas z nagród, kabura bez pasa monterskiego).</summary>
    private static void Lock(PhonePainter p, Rect2 r)
    {
        var c = r.GetCenter().Round();
        p.C.DrawRect(new Rect2(c.X - 5, c.Y - 1, 10, 7), Pal.Todo);
        p.C.DrawArc(c + new Vector2(0, -2), 3.5f, Mathf.Pi, Mathf.Tau, 8, Pal.Todo, 2f);
    }
}
