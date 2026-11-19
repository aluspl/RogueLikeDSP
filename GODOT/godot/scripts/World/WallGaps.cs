using LifeLike.Core;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.World;

/// <summary>
/// v0.23.0: boczne przejście w cienkim pionowym murze (zgłoszenie testera: „przejście w murze nieczytelne”). Pole g
/// przejścia: nie mur, odkryte, mur nad nim (do 2 pól wyżej) i pod nim, po bokach przejścia i muru pod nim brak muru.
/// Bryła muru sięga 1,5 wiersza w górę, więc dwa mury pod przejściem zakrywały je całe – wyglądało jak ciągły mur
/// ze szwem. Te dwa mury są ucinane na dolnej krawędzi przejścia (WallLayer), a mgła (FogLayer) liczy tam światło podłogi.
/// Tylko rysowanie – logika mapy bez zmian.
/// </summary>
public static class WallGaps
{
    public const int None = -1;

    private static bool Solid(CoreGame g, int x, int y) => g.Lv.At(x, y) == Tile.Wall;

    public static bool SideGap(CoreGame g, int x, int y)
    {
        if (!Level.In(x, y) || Solid(g, x, y) || !g.Explored(x, y) || !Solid(g, x, y + 1)) return false;
        if (Solid(g, x - 1, y) || Solid(g, x + 1, y) || Solid(g, x - 1, y + 1) || Solid(g, x + 1, y + 1)) return false;
        return Solid(g, x, y - 1) || Solid(g, x, y - 2);
    }

    /// <summary>Wiersz, od którego górnej krawędzi rysuje się mur (x, y) zasłaniający boczne przejście; inaczej None.</summary>
    public static int ClipRow(CoreGame g, int x, int y)
    {
        if (!Solid(g, x, y) || IsTempWall(g, x, y) || (x == g.SecretX && y == g.SecretY && g.SecretClosed())) return None;
        if (SideGap(g, x, y - 1)) return y;
        if (Solid(g, x, y - 1) && SideGap(g, x, y - 2)) return y - 1;
        return None;
    }

    public static bool IsTempWall(CoreGame g, int x, int y)
    {
        for (var i = 0; i < g.WallsCount; i++)
        {
            if (g.Walls[i].X == x && g.Walls[i].Y == y && g.Walls[i].Turns > 0) return true;
        }
        return false;
    }
}
