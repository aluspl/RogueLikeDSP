using Godot;
using LifeLike.Core;
using LifeLike.Game.Gfx;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.World;

/// <summary>
/// Kafle etapu 32x32 w palecie etapu (jak bg_map::build w GBA/src/main.cpp): podłoga w 4 wariantach,
/// podłoga z cieniem muru u góry, mur i lico muru nad podłogą (korona), schody. Nieznane pola nie są rysowane
/// (widać tło), światło i mgłę dokłada FogLayer. Ścianka z mocy Murarza ma cegły z etapu „Mury parteru”.
/// </summary>
public partial class MapLayer : Node2D
{
    private CoreGame _g;

    public void Bind(CoreGame g) => _g = g;

    private static int Hash(int x, int y) => (int)(((uint)(x * 73856093) ^ (uint)(y * 19349663)) >> 3);

    public override void _Draw()
    {
        if (_g is null) return;
        const int c = Assets.Cell;
        var tiles = Assets.StageTiles(_g.Stage);
        var bricks = Assets.StageTiles(1);
        for (var y = 0; y < Level.H; y++)
        {
            for (var x = 0; x < Level.W; x++)
            {
                if (!_g.Explored(x, y)) continue;
                var t = _g.Lv[x, y];
                int idx;
                var tex = tiles;
                if (t == Tile.Stairs)
                {
                    idx = Assets.TileStairs;
                }
                else if (t == Tile.Wall)
                {
                    idx = _g.Lv.At(x, y + 1) != Tile.Wall ? Assets.TileWallFace : Assets.TileWall;
                    if (IsTempWall(x, y))
                    {
                        tex = bricks;
                        idx = Assets.TileWallFace;
                    }
                }
                else
                {
                    var h = Hash(x, y);
                    idx = _g.Lv.At(x, y - 1) == Tile.Wall ? Assets.TileFloorShadow + (h & 1) : Assets.TileFloor + (h & 3);
                }
                DrawTextureRectRegion(tex, new Rect2(x * c, y * c, c, c), new Rect2(idx * c, 0, c, c));
            }
        }
    }

    private bool IsTempWall(int x, int y)
    {
        for (var i = 0; i < _g.WallsCount; i++)
        {
            if (_g.Walls[i].X == x && _g.Walls[i].Y == y && _g.Walls[i].Turns > 0) return true;
        }
        return false;
    }
}
