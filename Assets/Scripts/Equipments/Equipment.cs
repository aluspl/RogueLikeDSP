using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LifeLike.Enums;
using LifeLike.Interfaces;
using LifeLike.Enums.Equipment;
using System;

namespace LifeLike.Equipments{

	public abstract class Equipment : IEquipment {
		public string Name { get; set; }
		public EquipmentType EquipmentType {get; set;}
		public string IconImageName {get; set;}
        public string SpriteImageName { get ; set ; }
        public virtual string Stats { get;  }
    }
}
