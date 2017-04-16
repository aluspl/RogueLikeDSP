using System.Collections.Generic;
using Characters;

namespace Assets.Scripts.Characters.CharacterClasses
{
    public class SuperHoboClass : BaseCharacter
    {
        public new static string ClassName = "Super Hobo";

        public SuperHoboClass(CharacterStatisticDataModel statistic) : base(statistic)
        {
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
