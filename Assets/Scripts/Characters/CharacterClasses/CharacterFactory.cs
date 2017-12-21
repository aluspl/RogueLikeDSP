using System.Collections.Generic;

namespace LifeLike.Characters.CharacterClasses
{
    public static class CharacterFactory
    {
        public static Character GetPlayerClass(string classType, CharacterStats statistic )
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
                case CouchTrainerClass.ClassType:
                    return new CouchTrainerClass(statistic);
                default:
                    return  null;
            }
        }

        public  static Dictionary<string,string> PlayerClassList()
        {
            var list = new Dictionary<string, string>
            {
                {                    ItGuyClass.ClassType, ItGuyClass.ClassName                },
                {                    SuperHoboClass.ClassType, SuperHoboClass.ClassName    },
                {                    CouchTrainerClass.ClassType, CouchTrainerClass.ClassName    }


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
                {CouchTrainerClass.ClassType, CouchTrainerClass.ClassName }


            };
            return list;
        }
    }

}