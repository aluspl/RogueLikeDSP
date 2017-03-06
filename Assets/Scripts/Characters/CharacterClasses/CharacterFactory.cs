namespace Characters.CharacterClasses
{
    public class CharacterFactory
    {
        public static BaseCharacterClass GetClass(string classType)
        {
            switch (classType)
            {
                case ItGuyClass.ClassType:
                    return new ItGuyClass();

            }
            return new ItGuyClass();
        }
    }
}