using System.Linq;
using Characters;
using Characters.CharacterClasses;
using Controls;
using UnityEngine;

namespace Utils
{
    public static class EnemyUtils
    {     
        private static int _enemyCount;
        public static BaseCharacter GenerateEnemy()
        {
            var classes = CharacterFactory.EnemyClassList();
            
            var statistic = new CharacterStatisticDataModel
            {
                Agility = Random.Range(0,5),
                Charisma = 1,
                CurrentExperience = 0,
                Endurance = Random.Range(0,5),
                Inteligence = 1,
                Level = 1,                   
                Name = "Random Enemy "+_enemyCount++,
                Strength = Random.Range(0,5),
                IsEnemy=true
            };
            Debug.Log(string.Format("",statistic.Name,statistic.Agility,statistic.Strength, statistic.Endurance));
            return CharacterFactory.GetPlayerClass(classes.FirstOrDefault().Key, statistic);
        }
        public static void SelectEnemy()
        {
            var nearby = GameManager.Instance.Enemies.Where(p=>p.Distance<=MAXDISTANCE).OrderBy(p => p.Distance).ToList();
            if (nearby.Count <= 0) return;

            EnemyIndex++;

            if (EnemyIndex == nearby.Count) EnemyIndex = 0;
            UnSelectAllEnemies();
            nearby[EnemyIndex>=nearby.Count?  0 : EnemyIndex ].IsSelected = true;
        }

        public static void EnemiesMove(GameObject playerObject)
        {
            foreach (var enemy in  GameManager.Instance.Enemies)
            {
                enemy.IsHisTurn = true;
                enemy.MoveToPlayer(playerObject);
            }
        }

        public static int MAXDISTANCE = 1;

        public static Enemy SelectedEnemy
        {
            get
            {
                // Property action for unselected enemy or empty list ... or whatever :D 
                return (GameManager.Instance!=null && GameManager.Instance.Enemies.Count>0)
                     ? GameManager.Instance.Enemies.FirstOrDefault(p=>p.IsSelected)
                    : null;
            }
            set
            {
                UnSelectAllEnemies();
                value.IsSelected = true;
            }
        }

        public static int EnemyIndex { get; set; }

        public static void UnSelectAllEnemies()
        {
            foreach (var enemy in  GameManager.Instance.Enemies)
            {
                enemy.IsSelected = false;
            }
        }
    }
}