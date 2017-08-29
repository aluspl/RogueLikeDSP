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
        public Weapon() : base()
        {
            EquipmentType = EquipmentType.Weapon;
        }
        public int Attack { get; set; }
        public WeaponType WeaponType { get; set; }
        public bool IsRange { get; set; }
        public int Range { get; set; }
        public override string Stats
        {
            get
            {
                return string.Format("Damage {1}, Type: {0}", WeaponType, Attack);
            }
        }
        public override string FullStats
        {
            get
            {
                return string.Format("Name {0}, Damage {1}, Type: {2}", Name, Attack, WeaponType);
            }
        }
        public static IWeapon Type
        {
            get
            {
                var type = new System.Random().Next(0, 7);
                switch (type)
                {
                    case 0: return new Weapon { WeaponType = WeaponType.Blade, Name = "Katana" };
                    case 1: return new Weapon { WeaponType = WeaponType.Blade, Name = "One Hand Sword" };
                    case 2: return new Weapon { WeaponType = WeaponType.Blade, Name = "One Hand Axe" };

                    case 3: return new Weapon { WeaponType = WeaponType.ItItem, Name = "Keyboard" };
                    case 4: return new Weapon { WeaponType = WeaponType.ItItem, Name = "Mouse" };
                    case 5: return new Weapon { WeaponType = WeaponType.Bottle, Name = "Bottle Flower" };
                    case 6: return new Weapon { WeaponType = WeaponType.OfficeItem, Name = "Pen" };
                    case 7: return new Weapon { WeaponType = WeaponType.OtherItem, Name = "Umbrella" };
                    case 8: return new Weapon { WeaponType = WeaponType.Blunt, Name = "Baseball Bat" };


                    default: return new Weapon { WeaponType = WeaponType.OtherItem, Name = "Umbrella" };
                }
            }
        }

        public override string IconImageName { get { return "weapon"; } }

    }
}
