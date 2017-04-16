using System.Collections.Generic;

namespace Characters.CharacterClasses
{
    public class HoboClass : BaseCharacter
    {
        public new static string ClassName = "Hobo";

        public HoboClass(CharacterStatisticDataModel statistic) : base(statistic)
        {
            throw new System.NotImplementedException();
        }


        public const string ClassType = "HoboClass";

        public override List<string> SpecialActionsList()
        {
            return new List<string>()
            {
                "Normal Attack",
                "Tech Talk",

            };
        }
    }
}
