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
			List<IEquipment> equipments=new List<IEquipment>();
			equipments.Add(new Weapon{ Name="Sword", Attack=1, WeaponType= WeaponType.Blade});
			equipments.Add(new Weapon{ Name="Keyboard", Attack=2, WeaponType=WeaponType.ItStuff});
			equipments.Add(new Weapon{ Name="Broken Glass", Attack=3, WeaponType=WeaponType.Bottle});
		
			equipments.Add(new Armor{ Name="Metal Armor", Defense=1, Bonus1=1, Bonus1Type=StatEnum.Strength });
			equipments.Add(new Armor{ Name="Shitty Shirt", Defense=2 });
			equipments.Add(new Armor{ Name="Geek Tshirt", Defense=1, Bonus1=1, Bonus1Type=StatEnum.Inteligence });
			equipments.Add(new Armor{ Name="Suit", Defense=1, Bonus1=1, Bonus1Type=StatEnum.Charisma });
			
			equipments.Add(new Head{ Name="Metal Helmet", Defense=1, Bonus1=1, Bonus1Type=StatEnum.Strength });
			equipments.Add(new Head{ Name="Plastic Bag", Defense=2 });
			equipments.Add(new Head{ Name="Baseball Cap", Defense=1, Bonus1=1, Bonus1Type=StatEnum.Inteligence });
			equipments.Add(new Head{ Name="Hat", Defense=1, Bonus1=1, Bonus1Type=StatEnum.Charisma });
		
			equipments.Add(new Gloves{ Name="Metal Gloves", Defense=1, Bonus1=1, Bonus1Type=StatEnum.Strength });
			equipments.Add(new Gloves{ Name="Plastic Gloves", Defense=2 });
			equipments.Add(new Gloves{ Name="Winteg Gloves", Defense=1, Bonus1=1, Bonus1Type=StatEnum.Inteligence });
			equipments.Add(new Gloves{ Name="Leather Gloves", Defense=1, Bonus1=1, Bonus1Type=StatEnum.Charisma });

			equipments.Add(new Pants{ Name="Metal Pants of Virginity", Defense=1, Bonus1=1, Bonus1Type=StatEnum.Strength });
			equipments.Add(new Pants{ Name="Dirty Pants", Defense=2 });
			equipments.Add(new Pants{ Name="Short Pants", Defense=1, Bonus1=1, Bonus1Type=StatEnum.Inteligence });
			equipments.Add(new Pants{ Name="Suits Pants", Defense=1, Bonus1=1, Bonus1Type=StatEnum.Charisma });

			equipments.Add(new Shoes{ Name="Metal Shoes", Defense=1, Bonus1=1, Bonus1Type=StatEnum.Strength });
			equipments.Add(new Shoes{ Name="Dirty Socks", Defense=2 });
			equipments.Add(new Shoes{ Name="Sport Shoes", Defense=1, Bonus1=1, Bonus1Type=StatEnum.Inteligence });
			equipments.Add(new Shoes{ Name="Elegant Shoes", Defense=1, Bonus1=1, Bonus1Type=StatEnum.Charisma });

			return equipments;
        }
       
      
		public ICollection<IEquipment> GetEquipments(EquipmentType type)
		{
			return Equipments
					.Where(p=>p.EquipmentType==type)
					.ToList();
		}
    }
}

