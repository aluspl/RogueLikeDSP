using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Enums;
using MiniGame.TheBall;
using UnityEngine;
using Random = System.Random;

public class MapManager : MonoBehaviour
{
    /**
        Items based on selected theme :)
    **/
    public List<GameObject> Walls;
    public List<GameObject> Floors;
    public List<GameObject> Doors;
    public GameObject EndPoint;

    public GameObject Player;

    public int MapSizeX = 100;

    public int MapSizeY = 100;

    public int MaxHorizontalLines=5;
    public int MaxVerticalLines=4;

    // Use this for initialization
	void Start () {
	    var map=MapGenerator.GenerateMap(MapSizeX,MapSizeY, MaxHorizontalLines, MaxVerticalLines);
	    InsertMapElements(map);
	}

    private GameObject GetMapObject(MapElement pos)
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
            case MapElement.StartPoint:
                return Player;
            case MapElement.EndPoint:
                return EndPoint;
            default:
                return null;
        }
    }


    private void InsertMapElements(MapElement[,] map)
    {
        for (var x = 0; x < map.GetLength(0); x++)
        {
            for (var y = 0; y < map.GetLength(1); y++)
            {
                GenerateMapItem(map, x, y);
            }
        }
    }


    private void GenerateMapItem(MapElement[,] map, int x, int y)
    {
        var pos = map[x, y];
        if (pos >=0)
            AddElement(GetMapObject(pos), x, y);
    }

    private void AddElement(GameObject mapObject, int x, int y)
    {
        Instantiate(mapObject, GetPosition(x, y), Quaternion.identity , transform);
    }

    private static Vector3 GetPosition(int i, int j)
    {
        return new Vector3(i/2f, j/2f, 0);
    }
    public void CleanMap()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}

