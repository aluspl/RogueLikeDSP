namespace Characters.CharacterClasses
{
    public class CorpoRat : BaseCharacter
    {  public new static string ClassName = "Corpo Rat";

        public CorpoRat(CharacterStatisticDataModel statistic) : base(statistic)
        {
            SelectedClass=ClassName;

        }

        public const string ClassType = "CorpoRatClass";
    }
}