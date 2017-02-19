using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Enums;
using UnityEngine;
using Random = System.Random;

internal static class MapGenerator
{
/*
          Generate Map
          mapSize: size of generated map
      */
    public static MapElement[,] GenerateMap(int mapSize)
    {
        var map = new MapElement[mapSize, mapSize];
        var random = new Random(Environment.TickCount);
        SetupBorders(mapSize, map);
        for (var x = 1; x < mapSize - 1; x++)
        {
            for (var y = 1; y < mapSize - 1; y++)
            {
                map[x, y] = (MapElement)random.Next(1);
            }
        }
        SetupAssets(mapSize, map);
        return map;
    }

    private static void SetupAssets(int mapSize, MapElement[,] map)
    {
        map[2, 2] = MapElement.StartPoint;
        map[mapSize/2, mapSize/2] = MapElement.EndPoint;
    }

    private static void SetupBorders(int mapSize, MapElement[,] map)
    {
        for (var x = 0; x <= mapSize - 2; x++)
        {
            map[x, 0] = MapElement.Wall;
            map[x, mapSize - 1] = MapElement.Wall;;

            for (var y = 0; y <= mapSize - 2; y++)
            {
                map[mapSize - 1, y] = MapElement.Wall;
                map[0, y] = MapElement.Wall;
            }
        }
    }

}