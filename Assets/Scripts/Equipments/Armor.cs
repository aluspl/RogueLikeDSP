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
        public Armor() : base()
        {
            EquipmentType = EquipmentType.Armor;
        }
        public override string Stats
        {
            get
            {
                return string.Format("Defense {0},", Defense);
            }
        }
        public override string FullStats
        {
            get
            {
                return string.Format("Name {0}, Defense {1},", Name, Defense);
            }
        }
        public override string IconImageName { get { return "armor"; } }
        public static string GetTypes()
        {
            var type = new Random().Next(0, 4);
            switch (type)
            {
                case 0: return "T-Shirt";
                case 1: return "Jacket";
                case 2: return "Armor";
                case 3: return "Shirt";

                default: return "Paper Bag";
            }
        }
        public int Defense { get; set; }
    }
}
