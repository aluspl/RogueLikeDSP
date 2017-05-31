using System.Collections.Generic;

namespace LifeLike.Characters.CharacterClasses
{
    public class SuperHoboClass : Character
    {
        public new static string ClassName = "Super Hobo";

        public SuperHoboClass(CharacterStatisticDataModel statistic) : base(statistic)
        {            SelectedClass=ClassName;

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
