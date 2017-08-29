using System;
using LifeLike.Characters;
using LifeLike.Equipments;
using LifeLike.Inferfaces;
using LifeLike.Interfaces;

namespace LifeLike.Utils
{
    public class EquipmentGenerator
    {
        public static IEquipment GenerateEquipment(int lvl)
        {
            var chance = (int)(UnityEngine.Random.value * 10);
            var equipmentlvl=RandomWeaponLvl(lvl);
            switch (chance)
            {
                case 0: return new Health { HealthRecover = 10 };
                case 1: return new Stamina { StaminaRecover = 10 };
                case 2: return GenerateArmor(equipmentlvl);
                case 3: return GenerateWeapon(equipmentlvl);
                case 4: return GeneratePants(equipmentlvl);
                case 5: return GenerateShoes(equipmentlvl);
                case 6: return GenerateGloves(equipmentlvl);
                case 7: return GenerateHead(equipmentlvl);
                default: return null;
            }
        }

        private static int RandomWeaponLvl(int lvl)
        {
            if (lvl<=0) return 0;
            return new Random().Next(0,lvl);
        }
        public static string GenerateName(string type, int lvl)
        {
            return string.Format("{0} {1} {2}", GetEquipmentRare(lvl), GetEquipmentMaterial(lvl),type);
        }
        private static string GetEquipmentRare(int lvl)
        {
            switch (lvl)
            {
                case 0: return "Crappy";
                case 1: return "Normal";
                case 2: return "Rare";
                case 3: return "Special";
                case 4: return "Legendary";                
                default: return "Normal";
            }
        }
        private static string GetEquipmentMaterial(int lvl)
        {
            var material=new Random().Next(0,6);
            switch (material)
            {
                case 0: return "Paper";
                case 1: return "Rusty";
                case 2: return "Cotton";
                case 3: return "Poliester";

                case 4: return "Metal";
                case 5: return "Leather";
                case 6: return "Titan";                            
                default: return "Cotton";
            }
        }
        private static IEquipment GenerateHead(int equipmentlvl)
        {
            string HeadType=Head.GetTypes();
            return new Head { Name = HeadType, Defense = 1 };
        }

        private static IEquipment GenerateGloves(int equipmentlvl)
        {
            return new Gloves { Name = "Gloves", Defense = 1 };
        }

        private static IShoes GenerateShoes(int equipmentlvl)
        {
            return new Shoes { Name = "Shoes", Defense = 1 };
        }

        private static IPants GeneratePants(int equipmentlvl)
        {
            return new Pants { Name = "Pants", Defense = 1 };
        }

        private static IWeapon GenerateWeapon(int equipmentlvl)
        {
            var weapon=Weapon.Type;
            weapon.Name=GenerateName(weapon.Name, equipmentlvl);
            weapon.Attack=equipmentlvl;
            
            return weapon;
        }

        private static IArmor GenerateArmor(int equipmentlvl)
        {
            return new Armor { Name = "Armor", Defense = 1 };
        }
    }
}