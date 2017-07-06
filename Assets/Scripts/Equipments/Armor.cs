using System.Collections;
using System.Collections.Generic;
using LifeLike.Enums;
using System;
using LifeLike.Enums.Equipment;
using LifeLike.Interfaces;

namespace LifeLike.Equipments
{

    public class Armor : Equipment, IArmor
    {
        public Armor()  : base()
        {
            EquipmentType=EquipmentType.Armor;
        }   
        public override string Stats { 
            get
            {
            return string.Format( "Defense {0},", Defense);   
            }
        }
        public override string FullStats { 
            get
            {
            return string.Format( "Name {0}, Defense {1},",Name, Defense);   
            }
        }
        public int Defense { get; set; }        
    }
}
