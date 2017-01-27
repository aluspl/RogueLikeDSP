using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = System.Random;

public class MapLoader
{


    public static int[,] LoadMap(int map)
    {
        return Maps.Count > map ? Maps[0] : Maps.First().Value;
    }

    private static Dictionary<int, int[,]> Maps
    {
        get { return _maps ?? (_maps = new Dictionary<int, int[,]> {{0, Map1},{1,Map2}}); }
    }

    public static int[,] GenerateMap(int mapSize)
    {
        var map = new int[mapSize, mapSize];
        var random = new Random(Environment.TickCount);
        SetupBorders(mapSize, map);
        SetupCorners(mapSize, map);
        var mapvalues=new StringBuilder();
        for (var x = 1; x < mapSize - 2; x++)
        {
            mapvalues.AppendLine();
            for (var y = 1; y < mapSize - 2; y++)
            {
                map[x, y] = random.Next(3);
                mapvalues.AppendFormat("{0} ",map[x, y]);
            }
            mapvalues.AppendLine();
        }
        SetupAssets(mapSize, map,random);
        Debug.Log(string.Format("Map {0}", mapvalues));
        return map;
    }

    private static void SetupAssets(int mapSize, int[,] map, Random random)
    {
        map[random.Next(mapSize - 3) + 1, random.Next(mapSize - 3) + 1] = StartPosition;
        map[random.Next(mapSize - 3) + 1, random.Next(mapSize - 3) + 1] = EndPosition;

    }

    private static void SetupBorders(int mapSize, int[,] map)
    {
        for (var x = 1; x <= mapSize - 2; x++)
        {
            map[x, 0] = 2;
            map[x, mapSize - 1] = 2;

            for (var y = 1; y <= mapSize - 2; y++)
            {
                map[mapSize - 1, y] = 1;
                map[0, y] = 1;
            }
        }
    }

    private static void SetupCorners(int mapSize, int[,] map)
    {
        map[0, 0] = LeftTopCorner;
        map[0, mapSize - 1] = RightTopCorner;
        map[mapSize - 1, 0] = LeftBottomCorner;
        map[mapSize - 1, mapSize - 1] = RightBottomCorner;
    }

    public static int LeftTopCorner = 6;

    public static int RightTopCorner = 5;
    public static int LeftBottomCorner = 3;

    public static int RightBottomCorner = 4;

    public static int StartPosition = 8;
    public static int EndPosition = 7;

    /*
    0. Empty
    1. Horizontal Wall
    2. Vertical Wall
    3-6 Corner
    7. End Game
    8. Start Game
       */
    private static readonly int[,] Map1 ={
        {6, 1, 1, 1, 1, 1, 1, 1, 5 },
        {2, 8, 2, 2, 0, 2, 0, 0, 2 },
        {2, 1, 3, 1, 1, 4, 2, 2, 2 },
        {2, 0, 0, 1, 1, 0, 2, 0, 2 },
        {2, 1, 1, 1, 1, 2, 2, 2, 2 },
        {2, 2, 1, 1, 1, 2, 2, 0, 2 },
        {2, 1, 3, 1, 1, 4, 2, 2, 2 },
        {2, 1, 2, 0, 2, 0, 0, 7, 2 },
        {3, 1, 1, 1, 1, 1, 1, 1, 4 }
    };
    private static readonly int[,] Map2 ={
        {6, 1, 1, 1, 1, 1, 1, 1, 5 },
        {2, 8, 0, 0, 0, 0, 0, 7, 2 },
        {2, 1, 1, 1, 1, 2, 2, 2, 2 },
        {2, 1, 0, 1, 1, 0, 0, 2, 2 },
        {2, 1, 1, 1, 1, 2, 0, 2, 2 },
        {2, 2, 1, 1, 1, 2, 0, 2, 2 },
        {2, 1, 3, 2, 2, 4, 0, 2, 2 },
        {2, 1, 1, 2, 2, 2, 2, 3, 2 },
        {3, 1, 1, 1, 1, 1, 1, 1, 4 }
    };

    private static Dictionary<int, int[,]> _maps;
}
