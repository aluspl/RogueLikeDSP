using Assets.Scripts.Enums;
using Enums;
using UnityEngine;

namespace Interfaces
{
    public interface IMapManager
    {
        MapElement[,] Map { get; set; }
        Transform MapCollection { get; set; }
        void StartLevel(int level);
        GameObject GetMapObject(MapElement pos);
        void InsertMapElements();
        void GenerateMapItem(MapElement[,] map, int x, int y);
        void AddElement(GameObject mapObject, int x, int y);
        void CleanMap();
    }
}