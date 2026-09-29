using Godot;
using LifeLike.Core;
using LifeLike.Game.Gfx;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game.World;

/// <summary>
/// Kafle etapu 32x32 w palecie etapu (jak bg_map::build w GBA/src/main.cpp): podłoga w 4 wariantach,
/// podłoga z cieniem muru u góry i schody. Mur jest autokaflowany (v0.21.51, widok 3/4, światło z lewej góry):
/// pole muru z murem poniżej to ciemny wierzch masy muru (bez pasów), pole z podłogą poniżej to lico z wzorem
/// materiału; od strony podłogi dochodzą nakładki - jasna krawędź wierzchu u góry i z lewej, ciemna z prawej,
/// końce lica i róg wewnętrzny. Nieznane pola nie są rysowane (widać tło), a miękkie przejście w ciemność,
/// światło i mgłę dokłada FogLayer. Ścianka z mocy Murarza ma cegły z etapu „Mury parteru”.
/// </summary>
public partial class MapLayer : Node2D
{
    private CoreGame _g;

    public void Bind(CoreGame g) => _g = g;

    private static int Hash(int x, int y) => (int)(((uint)(x * 73856093) ^ (uint)(y * 19349663)) >> 3);

    /// <summary>Czy pole jest murem dla autokafli (poza mapą też mur; ścianka Murarza nie łączy się z murem).</summary>
    private bool Solid(int x, int y) => _g.Lv.At(x, y) == Tile.Wall;

    public override void _Draw()
    {
        if (_g is null) return;
        var tiles = Assets.StageTiles(_g.Stage);
        var bricks = Assets.StageTiles(1);
        for (var y = 0; y < Level.H; y++)
        {
            for (var x = 0; x < Level.W; x++)
            {
                if (!_g.Explored(x, y)) continue;
                var t = _g.Lv[x, y];
                if (t == Tile.Stairs)
                {
                    Put(tiles, x, y, Assets.TileStairs);
                }
                else if (t == Tile.Wall)
                {
                    if (IsTempWall(x, y)) Put(bricks, x, y, Assets.TileWallFace);
                    else DrawWall(tiles, x, y);
                }
                else
                {
                    var h = Hash(x, y);
                    Put(tiles, x, y, Solid(x, y - 1) ? Assets.TileFloorShadow + (h & 1) : Assets.TileFloor + (h & 3));
                }
            }
        }
    }

    /// <summary>Autokafel muru: wierzch albo lico wg pola poniżej, krawędzie wg sąsiadów z podłogą.</summary>
    private void DrawWall(Texture2D tiles, int x, int y)
    {
        bool n = Solid(x, y - 1), s = Solid(x, y + 1), w = Solid(x - 1, y), e = Solid(x + 1, y);
        var face = !s;
        Put(tiles, x, y, face ? Assets.TileWallFace : Assets.TileWall);
        if (!n) Put(tiles, x, y, Assets.TileEdgeTop);
        if (face)
        {
            if (!w) Put(tiles, x, y, Assets.TileFaceLeft);
            if (!e) Put(tiles, x, y, Assets.TileFaceRight);
            return;
        }
        if (!w) Put(tiles, x, y, Assets.TileEdgeLeft);
        if (!e) Put(tiles, x, y, Assets.TileEdgeRight);
        if (n && w && !Solid(x - 1, y - 1)) Put(tiles, x, y, Assets.TileInnerCorner);
    }

    private void Put(Texture2D tex, int x, int y, int idx)
    {
        const int c = Assets.Cell;
        DrawTextureRectRegion(tex, new Rect2(x * c, y * c, c, c), new Rect2(idx * c, 0, c, c));
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
