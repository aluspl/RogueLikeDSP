using Assets.Scripts.Enums;
using Enums;

namespace Interfaces
{
    public interface IMapGenerator
    {
        int MapSizeY { get; set; }
        int MapSizeX { get; set; }
        MapElement[,] GenerateMap(int mapSizeX, int mapSizeY, int maxHorizontalLines, int maxVerticalLines);
        void SetupAddons(MapElement[,] map);
        void GenerateRooms(MapElement[,] map, int  totalHorizontalLines, int totalVerticalLines);

        void GetVerticalWalls(MapElement[,] map, int totalVerticalLines,  int posX, int posY,
            int y);

        void GetFloors(MapElement[,] map, int posX, int posY, int xSize, int ySize);
        void GetVerticalWall(MapElement[,] map, ref int posX,  int posY, int xSize, int ySize);
        void GetHorizontalWall(MapElement[,] map, ref int posY, int ySize);
        int GetWallSize(ref int left);
        void SetupAssets(MapElement[,] map);
        void SetupBorders(MapElement[,] map);
    }
}

