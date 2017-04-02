using System.Collections.Generic;

namespace Characters.CharacterClasses
{
    public class CorpoBoss : BaseCharacter
    {
        public static string ClassName = "Corpo Boss";

        public CorpoBoss(CharacterStatisticDataModel statistic) : base(statistic)
        {
        }

        public const string ClassType = "CorpoBossClass";

        public override List<string> SpecialActionsList()
        {
            return new List<string>()
            {
                "Normal Attack",
                "Fire Attack",
            };
        }
    }
}