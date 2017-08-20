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

namespace LifeLike
{

	public class LootManager : MonoBehaviour, ILootManager 
	{
		public EquipmentObject Equipment;
	
		[InjectAttribute("Loot")]
		public static ILootManager Instance = null;

		void Awake () {
			if (Instance == null) Instance = this;
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
        }
		public IMapManager MapManager {get {
				return GameLogicManager.Instance.GetMapManager();
		}}
        public IEquipment GenerateLoot(Transform enemyPosition, Character EnemyStatistic)
        {
			try
			{
				if (Equipment==null) return null;
				if (enemyPosition == null) return null;
				if (MapManager==null) return null;
				var position=enemyPosition.transform.position;
				var equipment=GenerateLootItem(EnemyStatistic, PlayerManager.Instance.Statistic);
				var eq=Instantiate(Equipment, position, Quaternion.Euler(0,0,0));
				eq.SetEquipment(equipment);
				return equipment;
			}
			catch(Exception e)
			{
				Debug.Log(e);
				return null;
			}
        }

        private IEquipment GenerateLootItem(Character enemy, Character statistic)
        {
			var chance= UnityEngine.Random.value;
			if (chance>0.5f)   
			return new Health { HealthRecover=10 };
			return  new Stamina { StaminaRecover=10 };
        }

        public void DropLoot()
        {
            throw new NotImplementedException();
        }

      
    }

}
