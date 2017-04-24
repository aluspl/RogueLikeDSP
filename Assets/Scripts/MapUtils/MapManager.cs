using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Enums;
using Controls;
using Interfaces;
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
    public List<Enemy> Enemies;

    public GameObject EndPoint;

    public Player Player;

    public int MapSizeX = 100;
    public int MapSizeY = 100;
    public int MaxEnemies = 2;
    public int MaxHorizontalLines=5;
    public int MaxVerticalLines=4;
    public int MinimalWallSize = 10;
    private readonly Random _random=new Random();

    public MapElement[,] Map { get; set; }

    // Use this for initialization

    public void StartLevel(int level)
    {
        using (var generator = new MapGenerator())
        {
            generator.MinimumWallSize = MinimalWallSize;
            Map=generator.GenerateMap(MapSizeX,MapSizeY, MaxHorizontalLines, MaxVerticalLines);
        }
        MaxEnemies = level + 10;
        InsertMapElements();
    }

    void Awake()
    {
         MapCollection=new GameObject("Map").transform;
        EnemiesCollection=new GameObject("Enemies").transform;
    }

    public Transform EnemiesCollection { get; set; }

    public Transform MapCollection { get; set; }

    public GameObject GetMapObject(MapElement pos)
    {
        switch (pos)
        {
            case MapElement.Wall:
                return Walls.FirstOrDefault();
            case MapElement.InsideWall:
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
            case MapElement.Player:

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
        if (pos < 0) return;
        if (pos == MapElement.Player)
            AddPlayer(GetMapObject(pos), x, y);
        else
            AddElement(GetMapObject(pos), x, y);
        AddEnemy(map, x, y);
    }

    private void AddPlayer(GameObject mapObject, int x, int y)
    {
        GameManager.Instance.PlayerObject=Instantiate(mapObject, GetPosition(x, y), Quaternion.identity );
    }

    private void AddEnemy(MapElement[,] map, int x, int y)
    {
        if (MaxEnemies <= 0 || Enemies.Count==0 || map[x,y]!=MapElement.Floor) return;
        var chance = _random.Next(20)==0;
        if (!chance) return;

        var enemy=Instantiate(Enemies.FirstOrDefault(), GetPosition(x, y), Quaternion.identity , EnemiesCollection);

        if (enemy == null) return;
        GameManager.Instance.AddEnemy(enemy);
        MaxEnemies--;
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
        try
        {
            Destroy(GameManager.Instance.PlayerObject.gameObject);

            if (EnemiesCollection != null)
            {
                GameManager.Instance.Enemies.Clear();
                foreach (Transform child in EnemiesCollection.transform)
                {
                    Destroy(child.gameObject);
                }
            }
            if (MapCollection == null) return;
            {
                foreach (Transform child in MapCollection.transform)
                {
                    Destroy(child.gameObject);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }
}

