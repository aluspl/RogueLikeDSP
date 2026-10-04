using Godot;
using LifeLike.Core;
using LifeLike.Game.Gfx;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.World;

/// <summary>
/// Podłoga etapu 32x32 w palecie etapu (jak bg_map::build w GBA/src/main.cpp): podłoga w 4 wariantach, podłoga z cieniem
/// muru u góry i schody. v0.21.54: mury rysuje WallLayer jako bryły 1,5 pola posortowane z postaciami (wyższe ściany);
/// w widoku 3/4 kafle z arkusza 32x24 (Proj.RowH) i cień rzucany przez mur na podłogę po prawej (światło z lewej góry).
/// Nieznane pola nie są rysowane (widać tło), a miękkie przejście w ciemność, światło i mgłę dokłada FogLayer.
/// </summary>
public partial class MapLayer : Node2D
{
    private CoreGame _g;

    public void Bind(CoreGame g) => _g = g;

    public static int Hash(int x, int y) => (int)(((uint)(x * 73856093) ^ (uint)(y * 19349663)) >> 3);

    /// <summary>Czy pole jest murem (poza mapą też mur).</summary>
    private bool Solid(int x, int y) => _g.Lv.At(x, y) == Tile.Wall;

    public override void _Draw()
    {
        if (_g is null) return;
        var look = _g.SDef().Look;   // v0.21.52 cz. d: wygląd etapu (kontrakty mapy kariery)
        var q = Proj.ThreeQuarter;
        var tiles = q ? Assets.StageTiles34(look) : Assets.StageTiles(look);
        var rh = Proj.RowH;
        var cast = new Color(Pal.Void, 0.2f);
        for (var y = 0; y < Level.H; y++)
        {
            for (var x = 0; x < Level.W; x++)
            {
                if (!_g.Explored(x, y)) continue;
                var t = _g.Lv[x, y];
                if (t == Tile.Wall) continue;
                if (t == Tile.Stairs)
                {
                    Put(tiles, x, y, Assets.TileStairs, rh);
                    continue;
                }
                var h = Hash(x, y);
                Put(tiles, x, y, Solid(x, y - 1) ? Assets.TileFloorShadow + (h & 1) : Assets.TileFloor + (h & 3), rh);
                if (q && Solid(x - 1, y)) // cień rzucany przez mur z lewej (pikselowe stopnie zamiast gradientu)
                {
                    DrawRect(new Rect2(x * Proj.W, y * rh, 6, rh), cast);
                    DrawRect(new Rect2(x * Proj.W, y * rh, 3, rh), cast);
                }
            }
        }
    }

    private void Put(Texture2D tex, int x, int y, int idx, int rh) =>
        DrawTextureRectRegion(tex, Proj.CellRect(x, y), new Rect2(idx * Proj.W, 0, Proj.W, rh));
}
