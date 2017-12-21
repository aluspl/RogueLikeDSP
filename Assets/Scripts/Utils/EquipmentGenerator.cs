using System;
using System.IO;
using LifeLike.Characters;
using LifeLike.Equipments;
using LifeLike.Inferfaces;
using LifeLike.Interfaces;
using UnityEngine;

namespace LifeLike.Utils
{
    public class EquipmentGenerator
    {
        public EquipmentGenerator()
        {
            _random = new System.Random();
            LoadEquipmentConfig();
        }       
        private static readonly string equipmentFileName="Equipment.json";

        private System.Random _random=new System.Random();
        private EquipmentConfig loadedData;

        private  void LoadEquipmentConfig()
        {
            string filePath = Path.Combine(UnityEngine.Application.streamingAssetsPath, equipmentFileName);
            if (File.Exists(filePath))
            {
                // Read the json from the file into a string
                string dataAsJson = File.ReadAllText(filePath);
                // Pass the json to JsonUtility, and tell it to create a GameData object from it
                loadedData = UnityEngine.JsonUtility.FromJson<EquipmentConfig>(dataAsJson);
            }
            else
            {
                loadedData=EquipmentConfig.SampleData();
                Debug.LogError("Cannot load game data!");
            }
        }

        public int RandomNumber(int max)
        {
            return _random.Next(0, max);
        }

        public  IEquipment GenerateEquipment(int lvl)
        {
            var chance=RandomNumber(20);
            var equipmentlvl = RandomWeaponLvl(lvl);
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
                default: return  null;
            }
        }

        private  int RandomWeaponLvl(int lvl)
        {
            if (lvl <= 0) return 0;
            return RandomNumber(lvl);
        }
        public  string GenerateName(string type, int lvl)
        {
            return string.Format("{0} {1} {2}", GetEquipmentRare(lvl), GetEquipmentMaterial(lvl), type);
        }
        private  string GetEquipmentRare(int lvl)
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
        private  string GetEquipmentMaterial(int lvl)
        {
            var material = RandomNumber(loadedData.Materials.Count-1);
            return loadedData.Materials[material];                     
        }
        private  IEquipment GenerateHead(int equipmentlvl)
        {
            string HeadType = Head.GetTypes();
            return new Head { Name = HeadType, Defense = 1 };
        }

        private  IEquipment GenerateGloves(int equipmentlvl)
        {
            return new Gloves { Name = "Gloves", Defense = 1 };
        }

        private  IShoes GenerateShoes(int equipmentlvl)
        {
            return new Shoes { Name = "Shoes", Defense = 1 };
        }

        private  IPants GeneratePants(int equipmentlvl)
        {
            return new Pants { Name = "Pants", Defense = 1 };
        }

        private  IWeapon GenerateWeapon(int equipmentlvl)
        {
            var weapon = Weapon.Type;
            weapon.Name = GenerateName(weapon.Name, equipmentlvl);
            weapon.Attack = equipmentlvl;

            return weapon;
        }

        private  IArmor GenerateArmor(int equipmentlvl)
        {
            return new Armor { Name = "Armor", Defense = 1 };
        }
    }
}