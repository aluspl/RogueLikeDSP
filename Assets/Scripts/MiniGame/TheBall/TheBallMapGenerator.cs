using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Random = System.Random;

namespace MiniGame.TheBall
{
    public class TheBallMapGenerator : MonoBehaviour {
        public GameObject Wall;
        public GameObject Corner;
        public GameObject FinishPoint;
        public GameObject Enemy;


        public List<TextAsset> Map;
        public float Size = 5;
        private GameObject _player;


        public void GenerateMap(int lvl, GameObject player)
        {
            transform.localScale = new Vector3(0.5f, 0.5f, 0);

            var map = GenerateMap(10+lvl);
            Debug.Log(string.Format("Map Items {0}",transform.childCount));
            if (map==null) return;
            _player = player;
            InsertChilds(map);

            transform.localScale = transform.localScale + new Vector3(0.7f, 0.7f, 0);
        }


        private void InsertChilds(BallMapElement[,] map)
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
            for (var i = 0; i < map.GetLength(0)-10; i++)
            {
                //    GenerateEnemy(map.GetLength(0));
            }
        }

        private void GenerateEnemy(int size)
        {
            var random=new System.Random();
            if (Enemy == null) Enemy = GameObject.FindWithTag("Enemy");
            AddElement(Enemy, random.Next(size)/2,1,random.Next(size)/2,1,0);
        }

        private void GenerateMapItem(BallMapElement[,] map, int x, int y, int mapStartX, int mapStartY)
        {
            var pos = map[x, y];
            if (pos <= 0) return;
            if (pos >0)
                AddElement(GetMapObject(pos), x, mapStartX, y, mapStartY, GetWallAngle(pos));
        }

        private void AddElement(GameObject mapObject, int x, int mapStartX, int y, int mapStartY, int angle)
        {
            Instantiate(mapObject, GetPosition(x, mapStartX, y, mapStartY), Quaternion.Euler(0, 0, angle), transform);
        }

        private static Vector3 GetPosition(int i, int mapStartX, int j, int mapStartY)
        {
            return new Vector3(i/2f+mapStartX, j/2f+mapStartY, 0);
        }

        private static int GetWallAngle(BallMapElement pos)
        {
            switch (pos)
            {
                case BallMapElement.WallVertical:
                    return 90;
                case BallMapElement.WallHorizontal:
                    return 0;
                case BallMapElement.BottomLeftCorner:
                    return 0;
                case BallMapElement.BottomRightCorner:
                    return 90;
                case BallMapElement.TopLeftCorner:
                    return 270;
                case BallMapElement.TopRightCorner:
                    return 180;
                default:
                    return 0;
            }
        }
        private GameObject GetMapObject(BallMapElement pos)
        {
            switch (pos)
            {
                case BallMapElement.WallVertical:
                case BallMapElement.WallHorizontal:
                    return Wall;
                case BallMapElement.BottomLeftCorner:
                case BallMapElement.BottomRightCorner:
                case BallMapElement.TopLeftCorner:
                case BallMapElement.TopRightCorner:
                    return Corner;
                case BallMapElement.FinishPoint:

                    return FinishPoint;
                case BallMapElement.StartPoint:
                    return _player;

                default:
                    return null;
            }

        }


        public void CleanMap()
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }

        /*
            Generate Map
            mapSize: size of generated map
        */
        public static BallMapElement[,] GenerateMap(int mapSize)
        {
            var map = new BallMapElement[mapSize, mapSize];
            var random = new Random(Environment.TickCount);
            SetupBorders(mapSize, map);
            SetupCorners(mapSize, map);
            var mapvalues=new StringBuilder();
            for (var x = 1; x < mapSize - 1; x++)
            {
                mapvalues.AppendLine();
                for (var y = 1; y < mapSize - 1; y++)
                {
                    map[x, y] = (BallMapElement)random.Next(3);
                    mapvalues.AppendFormat("{0} ",map[x, y]);
                }
                mapvalues.AppendLine();
            }
            SetupAssets(mapSize, map,random);
            Debug.Log(string.Format("Map {0}", mapvalues));
            return map;
        }

        private static void SetupAssets(int mapSize, BallMapElement[,] map, Random random)
        {
            map[2, 2] = BallMapElement.StartPoint;
            map[mapSize/2, mapSize/2] = BallMapElement.FinishPoint;
        }

        private static void SetupBorders(int mapSize, BallMapElement[,] map)
        {
            for (var x = 1; x <= mapSize - 2; x++)
            {
                map[x, 0] = BallMapElement.WallHorizontal;
                map[x, mapSize - 1] = BallMapElement.WallHorizontal;;

                for (var y = 1; y <= mapSize - 2; y++)
                {
                    map[mapSize - 1, y] = BallMapElement.WallVertical;
                    map[0, y] = BallMapElement.WallVertical;
                }
            }
        }

        private static void SetupCorners(int mapSize, BallMapElement[,] map)
        {
            map[0, 0] = BallMapElement.BottomLeftCorner;
            map[0, mapSize - 1] = BallMapElement.TopLeftCorner;
           map[mapSize - 1, 0] = BallMapElement.BottomRightCorner;
            map[mapSize - 1, mapSize - 1] = BallMapElement.TopRightCorner;
        }

    }
}


