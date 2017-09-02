using System;
using LifeLike.Characters;

namespace LifeLike.Utils
{
    public class EnemyGenerator
    {
        public static CharacterStatisticDataModel GenerateEnemy(int lvl)
        {
            var statistic = new CharacterStatisticDataModel
            {
                Agility=2,
                Strength=2,
                Endurance=2,
                Charisma = 2,
                Inteligence = 2,
                Level = lvl,                
                Name = GenerateName(),
                IsEnemy = true
            };
            AssignPoints(statistic,lvl);
            return statistic;
        }
        private static void AssignPoints(CharacterStatisticDataModel statistic, int lvl)
        {
            var random = new Random();

            int points = 8 + lvl;
            int temp;
            if (points > 1)
            {
                statistic.Agility = temp = random.Next(1, points);
                points -= temp;
            }
            if (points > 1)
            {
                statistic.Endurance = temp = random.Next(1, points);
                points -= temp;
            }
            if (points > 1)
            {
                statistic.Strength = temp = random.Next(1, points);
                points -= temp;
            }
             if (points > 1)
            {
                statistic.Inteligence = temp = random.Next(1, points);
                points -= temp;
            }

        }
        public static string[] Names = new string[]{
            "Janusz", "Przemek", "Jan", "Zbyszek",
            "Gienio", "Wiesław", "Wacław","Artur",
            "Szymon", "Bronek", "Andrzej"
        };
        private static string GenerateName()
        {
            var name = new Random().Next(0, Names.Length);
            return Names[name];
        }
    }
}
