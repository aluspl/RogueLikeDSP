using System.Collections;
using System.Collections.Generic;
using LifeLike.Enums;
using System;
using LifeLike.Enums.Equipment;
using LifeLike.Interfaces;

namespace LifeLike.Equipments
{

    public class Stamina : Equipment, IStamina
    {
        public Stamina()  : base()
        {
            EquipmentType=EquipmentType.Stamina;
        }   
        public override string Stats { 
            get
            {
                return string.Format( "Stamina {0},", StaminaRecover);   
            }
        }
        public override string FullStats { 
            get
            {
                return string.Format( "Name {0}, Stamina Recovery {1},",Name, StaminaRecover);   
            }
        }
        public override string IconImageName {get {return "aeropress";} }
        public int StaminaRecover { get; set; }        
    }
}
