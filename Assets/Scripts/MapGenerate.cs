using System.Collections.Generic;
using UnityEngine;

public class MapGenerate : MonoBehaviour {
    public GameObject Wall;
    public GameObject Corner;
    public GameObject FinishPoint;


    public List<TextAsset> Map;
    public float Size = 5;
    private GameObject _player;

    void Start ()
	{

	}

    public void GenerateMap(int lvl, GameObject player)
    {
        transform.localScale = new Vector3(0.5f, 0.5f, 0);

        //   var map = MapLoader.LoadMap(0);
        var map = MapLoader.GenerateMap(10);
        Debug.Log(string.Format("Map Items {0}",transform.childCount));
        if (map==null) return;
        _player = player;
        InsertChilds(map);

      //  CenterChilds();
        transform.localScale = transform.localScale + new Vector3(0.7f, 0.7f, 0);
    }


    private void InsertChilds(int[,] map)
    {
        var mapStartX = (map.GetLength(0) * -1) / 4;
        var mapStartY = (map.GetLength(0) * -1) / 4;

        for (var x = 0; x < map.GetLength(0); x++)
        {
            for (var y = 0; y < map.GetLength(1); y++)
            {
                GenerateMapItem(map, x, y, mapStartX, mapStartY);
            }
        }
    }

    private void GenerateMapItem(int[,] map, int x, int y, int mapStartX, int mapStartY)
    {
        var pos = map[x, y];
        if (pos <= 0) return;
        if (pos >0)
            AddWall(GetMapObject(pos), x, mapStartX, y, mapStartY, GetWallAngle(pos));
     }

    private void AddWall(GameObject mapObject, int x, int mapStartX, int y, int mapStartY, int angle)
    {
        Instantiate(mapObject, GetPosition(x, mapStartX, y, mapStartY), Quaternion.Euler(0, 0, angle), transform);
    }

    private static Vector3 GetPosition(int i, int mapStartX, int j, int mapStartY)
    {
        return new Vector3(i/2f+mapStartX, j/2f+mapStartY, 0);
    }

    private static int GetWallAngle(int pos)
    {
        switch (pos)
        {
            case 1:
                return 90;
            case 2:
                return 0;
            case 3:
                return 90;
            case 4:
                return 180;
            case 5:
                return 270;
            default:
                return 0;
        }
    }
    private GameObject GetMapObject(int pos)
    {
        switch (pos)
        {
            case 1:
            case 2:
                return Wall;
            case 3:
            case 4:
            case 5:
            case 6:
                return Corner;
            case 7:
                return FinishPoint;
            case 8:
                return _player;
            default:
                return null;

        }
    }


    // Update is called once per frame
	void Update () {
		
	}

    public void CleanMap()
    {
//        Destroy(PlayerObject);
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}
