using System.Collections.Generic;
using Characters;
using Characters.CharacterClasses;

namespace Assets.Scripts.Characters.CharacterClasses
{
    public static class CharacterFactory
    {
        public static BaseCharacter GetPlayerClass(string classType, CharacterStatisticDataModel statistic )
        {
            switch (classType)
            {
                case ItGuyClass.ClassType:
                    return new ItGuyClass(statistic);

                case SuperHoboClass.ClassType:
                    return new SuperHoboClass(statistic);
                case HoboClass.ClassType:
                    return new HoboClass(statistic);
                case CorpoBoss.ClassType:
                    return new CorpoBoss(statistic);
                case CorpoRat.ClassType:
                    return new CorpoRat(statistic);
                default:
                    return  null;
            }
        }

        public  static Dictionary<string,string> PlayerClassList()
        {
            var list = new Dictionary<string, string>
            {
                {                    ItGuyClass.ClassType, ItGuyClass.ClassName                },
                {                    SuperHoboClass.ClassType, SuperHoboClass.ClassName    }

            };
            return list;
        }
        public static Dictionary<string, string> EnemyClassList()
        {
            var list = new Dictionary<string, string>
            {
                {CorpoBoss.ClassType, CorpoBoss.ClassName},
                {CorpoRat.ClassType, CorpoRat.ClassName},
                {HoboClass.ClassType, SuperHoboClass.ClassName},

            };
            return list;
        }
    }

}