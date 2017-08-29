using System.Collections;
using System.Collections.Generic;
using System;
using LifeLike.Enums.Equipment;
using LifeLike.Interfaces;

namespace LifeLike.Equipments
{

    public class Shoes : Equipment, IShoes
    {
        public Shoes()  : base()
        {
            EquipmentType=EquipmentType.Shoes;
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
                public override string IconImageName {get {return "shoes";} }

        public int Defense { get; set; }
    }
}
