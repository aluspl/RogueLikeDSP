using System.Collections.Generic;

namespace Characters.CharacterClasses
{
    public class SuperHoboClass : BaseCharacter
    {
        public static string ClassName = "Super Hobo";

        public SuperHoboClass(CharacterStatisticDataModel statistic) : base(statistic)
        {
            throw new System.NotImplementedException();
        }


        public const string ClassType = "SuperHoboClass";

        public override List<string> SpecialActionsList()
        {
            return new List<string>()
            {
                "Normal Attack",
                "Tech Talk",
                "Drunk Monkey Attack"
            };
        }
    }
}
