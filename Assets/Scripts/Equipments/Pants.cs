using System.Collections;
using System.Collections.Generic;
using System;
using LifeLike.Enums.Equipment;
using LifeLike.Interfaces;

namespace LifeLike.Equipments
{
    public class Pants : Equipment, IPants
    {
        public Pants()  : base()
        {
            EquipmentType=EquipmentType.Pants;
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
