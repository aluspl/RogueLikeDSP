using System.Collections;
using System.Collections.Generic;
using LifeLike.Enums;
using System;
using LifeLike.Enums.Equipment;
using LifeLike.Interfaces;

namespace LifeLike.Equipments
{

    public class Health : Equipment, IHealth
    {
        public Health()  : base()
        {
            EquipmentType=EquipmentType.Health;
        }   
        public override string Stats { 
            get
            {
                return string.Format( "HP {0},", HealthRecover);   
            }
        }
        public override string FullStats { 
            get
            {
                return string.Format( "Name {0}, HP {1},",Name, HealthRecover);   
            }
        }
        public override string IconImageName {get {return "painkiller";} }

        public int HealthRecover { get;  set; }
    }
}
