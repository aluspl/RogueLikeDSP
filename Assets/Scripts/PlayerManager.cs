using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LifeLike.Characters;
using LifeLike.Controls;
using LifeLike.Enums.Equipment;
using LifeLike.Equipments;
using LifeLike.Inferfaces;
using LifeLike.Interfaces;
using UnityEngine;
namespace LifeLike{
public class PlayerManager : MonoBehaviour, IPlayerManager {
	
		[InjectAttribute("Player")]
		public static IPlayerManager Instance = null;
		[SerializeField]
		public Character Statistic {get; set;}
		public List<IEquipment> Equipments {get; set;}

        public Player Object {get; set;}

        public void Destroy()
        {
			Destroy(Object);
			Statistic=null;
        }
		public void AddEquipment(IEquipment equipment)
		{
			Equipments.Add(equipment);
		}
		
	
        // Use this for initialization
        void Awake () {
			if (Instance == null) Instance = this;
			Equipments=new List<IEquipment>();	
			Equipments.AddRange(RandomWeapon());
		}

        private ICollection<IEquipment> RandomWeapon()
        {
			List<IEquipment> weapons=new List<IEquipment>();
			weapons.Add(new Weapon{ Name="Test", Attack=10});
			weapons.Add(new Weapon{ Name="Test 1", Attack=10});
			weapons.Add(new Weapon{ Name="Test 2", Attack=10});
			weapons.Add(new Weapon{ Name="Test 3", Attack=10});
			weapons.Add(new Weapon{ Name="Test 4", Attack=10});
			return weapons;
        }
       
      
		public ICollection<IEquipment> GetEquipments(EquipmentType type)
		{
			return Equipments
					.Where(p=>p.EquipmentType==type)
					.ToList();
		}
    }
}

