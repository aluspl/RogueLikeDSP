using System;
using Assets.Scripts.Enums;
using Enums;
using Interfaces;
using UnityEngine;
using Random = UnityEngine.Random;

public  class MapGenerator : IDisposable, IMapGenerator
{
    public  int MinimumWallSize = 20;

    public int MapSizeY { get; set; }

    public int MapSizeX { get; set; }
    /*
          Generate Map
          mapSize: size of generated map
      */
    public  MapElement[,] GenerateMap(int mapSizeX, int mapSizeY, int maxHorizontalLines, int maxVerticalLines)
    {
        MapSizeX = mapSizeX;
        MapSizeY = mapSizeY;

        var map = new MapElement[mapSizeX, mapSizeY];
        SetupAddons(map);
        SetupAssets(map);
        GenerateRooms(map, totalHorizontalLines: maxHorizontalLines, totalVerticalLines: maxVerticalLines);
        SetupBorders(map);
        return map;
    }

    public void SetupAddons(MapElement[,] map)
    {
        for (var x = 1; x < MapSizeX - 1; x++)
        {
            for (var y = 1; y < MapSizeY - 1; y++)
            {
                map[x, y] = (MapElement) Random.Range(0, 1);
            }
        }
    }

    public void GenerateRooms(MapElement[,] map, int  totalHorizontalLines, int totalVerticalLines)
    {
        var leftValueY = MapSizeY-1;
        var posY=0;
        var horizontalLines = Random.Range(1, totalVerticalLines);

        for (var i = 0; i < horizontalLines; i++)
        {
            var y = GetWallSize(ref leftValueY);
            GetVerticalWalls(map, totalVerticalLines, 0, posY, y);
            Debug.Log(string.Format("Line Horizontal {0} of {1},  PosY: {2}", i+1,totalVerticalLines, posY));
            GetHorizontalWall(map, ref posY, y);

        }

        GetVerticalWalls(map, totalVerticalLines, 0, posY, leftValueY);

    }

    public void GetVerticalWalls(MapElement[,] map, int totalVerticalLines,  int posX, int posY,
        int y)
    {
        var verticalLines = Random.Range(0, totalVerticalLines);
        var leftValueX = MapSizeX-1;

        for (var j = 0; j < verticalLines; j++)
        {
            Debug.Log(string.Format("Line Vertical {0} of {1}", j + 1, verticalLines));

            var x = GetWallSize(ref leftValueX);

            GetVerticalWall(map, ref posX, posY, x, y);

        }
    }
/*
In future : generate maps different floor enums :)
*/
    public void GetFloors(MapElement[,] map, int posX, int posY, int xSize, int ySize)
    {
        var floorElement = (MapElement)Random.Range(1, 4);
        for (var x=posX;x<posX+xSize;x++)
        for (var y = posY; y < posX + ySize; y++)
        {
            map[x, y] = floorElement;
        }
    }

    public void GetVerticalWall(MapElement[,] map, ref int posX,  int posY, int xSize, int ySize)
    {
        Debug.Log(string.Format("X: {0} Y: {1} PosX: {2} PosY: {3} PosY+Y={4}", xSize,ySize,posX,posY,posY+ySize));
        posX += xSize;

        var door = Random.Range(posY+1, (posY + xSize)-1);
        for (var y = posY; y < posY + ySize; y++)
        {
            if (y >= MapSizeY - 1 || posX >= MapSizeX - 1) continue;
            if (y==door)
                map[posX,y ]= MapElement.Door;
            else
                map[posX, y] = MapElement.Wall;
        }

    }

    public void GetHorizontalWall(MapElement[,] map, ref int posY, int ySize)
    {
        posY += ySize;
        //From 1. border to 2.border
        var door = Random.Range(1, MapSizeX-2);

        for (var x = 0; x < MapSizeX - 1; x++)
        {
            if (x >= MapSizeX - 1 || posY >= MapSizeY - 1) continue;
            if (x==door)
                map[x,posY] = MapElement.Door;
            else
                map[x, posY] = MapElement.Wall;
        }
    }

    public int GetWallSize(ref int left)
    {

        if (left <= MinimumWallSize)
        {
            return left;
        }
        var randomX = Random.Range(MinimumWallSize, left/2);

        left -= randomX;

        return randomX;
    }


    public void SetupAssets(MapElement[,] map)
    {
        map[2, 2] = MapElement.PlayerStart;
        map[MapSizeX/2, MapSizeY/2] = MapElement.EndPoint;
    }

    public void SetupBorders(MapElement[,] map)
    {
        for (var x = 0; x <= MapSizeX - 2; x++)
        {
            map[x, 0] = MapElement.Wall;
            map[x, MapSizeY - 1] = MapElement.Wall;;

            for (var y = 0; y <= MapSizeY - 2; y++)
            {
                map[MapSizeX - 1, y] = MapElement.Wall;
                map[0, y] = MapElement.Wall;
            }
        }
    }

    public void Dispose()
    {
    }
}