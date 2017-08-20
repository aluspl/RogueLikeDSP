using LifeLike.Characters;
using LifeLike.Controls;
using LifeLike.Interfaces;
using UnityEngine;

namespace LifeLike.Inferfaces
{
    public interface ILootManager : IManager
    {
        IEquipment GenerateLoot(Transform enemyPosition, Character EnemyStatistic);
        void DropLoot();
        
    }
}