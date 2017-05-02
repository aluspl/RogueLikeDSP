using System.Collections.Generic;

namespace Characters.CharacterClasses
{
    public class CorpoBoss : BaseCharacter
    {
        public new static string ClassName = "Corpo Boss";

        public CorpoBoss(CharacterStatisticDataModel statistic) : base(statistic)
        {           
             SelectedClass=ClassName;
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