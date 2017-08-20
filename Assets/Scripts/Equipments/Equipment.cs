using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LifeLike.Enums;
using LifeLike.Interfaces;
using LifeLike.Enums.Equipment;
using System;

namespace LifeLike.Equipments{

	public abstract class Equipment :  IEquipment {    
        public Equipment(){
           
        }
        public string Name { get; set; }
		public EquipmentType EquipmentType {get; set;}
		public virtual string IconImageName {get; set;}
        public string SpriteImageName { get ; set ; }
        public virtual string Stats { get;  }
		public virtual string FullStats { get;  }
	    public virtual int Bonus1 { get;  set; }
        public virtual StatEnum Bonus1Type { get; set; }
        public virtual int Bonus2 { get;  set; }
        public virtual StatEnum Bonus2Type { get; set; }
    }
}
