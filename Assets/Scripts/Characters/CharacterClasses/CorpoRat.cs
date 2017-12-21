namespace LifeLike.Characters.CharacterClasses
{
    public class CorpoRat : Character
    {  public new static string ClassName = "Corpo Rat";

        public CorpoRat(CharacterStats statistic) : base(statistic)
        {
            SelectedClass=ClassName;

        }

        public const string ClassType = "CorpoRatClass";
    }
}