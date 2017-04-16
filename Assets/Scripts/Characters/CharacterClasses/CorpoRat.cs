using Characters;

namespace Assets.Scripts.Characters.CharacterClasses
{
    public class CorpoRat : BaseCharacter
    {  public new static string ClassName = "Corpo Rat";

        public CorpoRat(CharacterStatisticDataModel statistic) : base(statistic)
        {

        }

        public const string ClassType = "CorpoRatClass";
    }
}