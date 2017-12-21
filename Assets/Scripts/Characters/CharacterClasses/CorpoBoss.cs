using System.Collections.Generic;

namespace LifeLike.Characters.CharacterClasses
{
    public class CorpoBoss : Character
    {
        public new static string ClassName = "Corpo Boss";

        public CorpoBoss(CharacterStats statistic) : base(statistic)
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