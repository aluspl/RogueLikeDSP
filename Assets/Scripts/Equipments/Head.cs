using System.Collections;
using System.Collections.Generic;
using System;
using LifeLike.Enums.Equipment;
using LifeLike.Interfaces;

namespace LifeLike.Equipments
{

    public class Head : Equipment, IHead
    {
        public Head() : base()
        {
            EquipmentType = EquipmentType.Head;
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
        public static string GetTypes()
        {
            var type=new Random().Next(0,4);
            switch(type)
            {
                case 0: return "Hat";
                case 1: return "Baseball Cat";
                case 2: return "Fedora";
                case 3: return "Helmet";
                default: return "Paper Bag";
            }
        }
        public override string IconImageName { get { return "head"; } }

        public int Defense { get; set; }
    }
}
