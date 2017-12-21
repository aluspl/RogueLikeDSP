using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LifeLike.Enums;
using LifeLike.Inferfaces;
using LifeLike.Interfaces;
using LifeLike.Equipments;
using LifeLike.Characters;
using LifeLike.Controls;
using LifeLike.Utils;

namespace LifeLike
{

    public class LootManager : MonoBehaviour, ILootManager
    {
        public EquipmentObject Equipment;

        [InjectAttribute("Loot")]
        public static ILootManager Instance = null;
        private EquipmentGenerator _equipmentGenerator;

        void Awake()
        {
            if (Instance == null) Instance = this;
            _equipmentGenerator = new EquipmentGenerator();
        }

        void LateUpdate()
        {
            // var previousType = ControlType;
            // SetupControlType();
            // if (previousType==ControlType) return;
            // SetupKeys();
        }

        public void Destroy()
        {
            Destroy(this.gameObject);
            LootManager.Instance=null;
        }
        public IMapManager MapManager
        {
            get
            {
                return GameLogicManager.Instance.GetMapManager();
            }
        }
        public IEquipment GenerateLoot(Transform enemyPosition, Character EnemyStatistic)
        {
            try
            {
                if (Equipment == null) return null;
                if (enemyPosition == null) return null;
                if (MapManager == null) return null;
                var position = enemyPosition.transform.position;
                var equipment = GenerateLootItem(EnemyStatistic, PlayerManager.Instance.Statistic);
                if (equipment!=null)
                { 
                    var eq = Instantiate(Equipment, position, Quaternion.Euler(0, 0, 0));
                    eq.SetEquipment(equipment);
                }
               
                return equipment;
            }
            catch (Exception e)
            {
                Debug.Log(e);
                return null;
            }
        }

        private IEquipment GenerateLootItem(Character enemy, Character player)
        {
			var lvl = player.Level-enemy.Level;
			return _equipmentGenerator.GenerateEquipment(lvl);
			

        
        }


        public void DropLoot()
        {
            throw new NotImplementedException();
        }


    }

}
