using System.Linq;
using Characters;
using Characters.CharacterClasses;

namespace Utils
{
    public static class EnemyUtils
    {
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
                Name = "Enemy",
                Strength = 1
            };
            return CharacterFactory.GetPlayerClass(classes.FirstOrDefault().Key, statistic);
        }
    }
}