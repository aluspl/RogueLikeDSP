using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Characters;
using Assets.Scripts.Characters.CharacterClasses;
using Characters;
using Controls;
using UnityEngine;

namespace Utils
{
    public static class EnemyUtils
    {
        private static int EnemyCount=0;
        public static BaseCharacter GenerateEnemy()
        {
            var classes = CharacterFactory.EnemyClassList();
            var statistic = new CharacterStatisticDataModel()
            {
                Agility = 1,
                Charisma = 1,
                CurrentExperience = 0,
                Endurance = 1,
                Inteligence = 1,
                Level = 1,
                Name = "Enemy "+EnemyCount++,
                Strength = 1
            };
            return CharacterFactory.GetPlayerClass(classes.FirstOrDefault().Key, statistic);
        }

        public static Enemy SelectedEnemy
        {
            get
            {
                return GameManager.Instance.Enemies.Count > EnemyIndex
                    ? GameManager.Instance.Enemies[EnemyIndex]
                    : null;
            }
        }

        public static int EnemyIndex { get; set; }

    }
}