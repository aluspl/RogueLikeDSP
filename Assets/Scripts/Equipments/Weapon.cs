using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LifeLike.Enums;
using LifeLike.Inferfaces;
using System;
using LifeLike.Enums.Equipment;

namespace LifeLike.Equipments
{

    public class Weapon : Equipment, IWeapon
    {
        public Weapon()  : base()
        {
            EquipmentType=EquipmentType.Weapon;
        }
        public int Attack { get; set; }
        public WeaponType WeaponType { get; set; } 
        public bool IsRange { get; set; }
        public int Range { get ; set ; }
        public override string Stats { 
            get
            {
            return string.Format( "Damage {1}, Type: {0}", WeaponType, Attack);   
            }
        }

    }
}
