using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Characters;
using Controls;
using Enums;
using Interfaces;
using MiniGame.TheBall;
using UnityEngine;
using Random = System.Random;

public class MapManager : MonoBehaviour, IMapManager
{
    /**
        Items based on selected theme :)
    **/
    public List<GameObject> Walls;
    public List<GameObject> Floors;
    public List<GameObject> Doors;
    public GameObject EndPoint;

    public Player Player;

    public int MapSizeX = 100;

    public int MapSizeY = 100;

    public int MaxHorizontalLines=5;
    public int MaxVerticalLines=4;
    public int MinimalWallSize = 10;
    public MapElement[,] Map { get; set; }

    // Use this for initialization

    public void StartLevel(int level)
    {
        using (var generator = new MapGenerator())
        {
            generator.MinimumWallSize = MinimalWallSize;
            Map=generator.GenerateMap(MapSizeX,MapSizeY, MaxHorizontalLines, MaxVerticalLines);
        }
        InsertMapElements();

    }

    void Awake()
    {
         MapCollection=new GameObject("Map").transform;
    }

    public Transform MapCollection { get; set; }

    public GameObject GetMapObject(MapElement pos)
    {
        switch (pos)
        {
            case MapElement.Wall:
                return Walls.FirstOrDefault();
            case MapElement.Door:
                return Doors.FirstOrDefault();
            case MapElement.Floor:
                return Floors.FirstOrDefault();
            case MapElement.Floor1:
                return Floors.Count == 1 ? Floors[0] : Floors.LastOrDefault();
            case MapElement.Floor2:
                return Floors.Count == 2 ? Floors[1] : Floors.LastOrDefault();
            case MapElement.Floor3:
                return Floors.Count == 3 ? Floors[2] : Floors.LastOrDefault();
            case MapElement.Floor4:
                return Floors.Count == 4 ? Floors[3] : Floors.LastOrDefault();
            case MapElement.PlayerStart:
                return Player.gameObject;
            case MapElement.EndPoint:
                return EndPoint;
            default:
                return null;
        }
    }


    public void InsertMapElements()
    {
        for (var x = 0; x < Map.GetLength(0); x++)
        {
            for (var y = 0; y < Map.GetLength(1); y++)
            {
                GenerateMapItem(Map, x, y);
            }
        }
    }


    public void GenerateMapItem(MapElement[,] map, int x, int y)
    {
        var pos = map[x, y];
        if (pos >=0)
            AddElement(GetMapObject(pos), x, y);
    }

    public void AddElement(GameObject mapObject, int x, int y)
    {
        Instantiate(mapObject, GetPosition(x, y), Quaternion.identity , MapCollection);
    }

    private static Vector3 GetPosition(int i, int j)
    {
        return new Vector3(i, j, 0);
    }
    public void CleanMap()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}

