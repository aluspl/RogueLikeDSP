using System.Linq;
using Assets.Scripts.Characters.CharacterClasses;
using Characters;
using Controls;

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
                Agility = 1,
                Charisma = 1,
                CurrentExperience = 0,
                Endurance = 1,
                Inteligence = 1,
                Level = 1,
                Name = "Enemy "+_enemyCount++,
                Strength = 1
            };
            return CharacterFactory.GetPlayerClass(classes.FirstOrDefault().Key, statistic);
        }
        public static void SelectEnemy()
        {
            var nearby = GameManager.Instance.Enemies.Where(p=>p.Distance<=MAXDISTANCE).OrderBy(p => p.Distance).ToList();
            if (nearby.Count <= 0) return;

            EnemyIndex++;
            if (EnemyIndex == nearby.Count) EnemyIndex = 0;
            foreach (var enemy in  GameManager.Instance.Enemies)
            {
                enemy.IsSelected = false;
            }
            nearby[EnemyIndex>=nearby.Count?  0 : EnemyIndex ].IsSelected = true;
        }

        public static int MAXDISTANCE = 1;

        public static Enemy SelectedEnemy
        {
            get
            {
                return EnemyIndex!=-1
                    ? GameManager.Instance.Enemies.FirstOrDefault(p=>p.IsSelected)
                    : null;
            }
        }

        public static int EnemyIndex { get; set; }

    }
}