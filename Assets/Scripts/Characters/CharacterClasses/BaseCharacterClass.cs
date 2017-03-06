namespace Characters.CharacterClasses
{
    public abstract class BaseCharacterClass
    {
        public string Name { get; set; }
        public int Strength { get; set; }
        public int Level { get; set; }
        public long CurrentExperience { get; set; }
        public int Inteligence { get; set; }
        public int Charisma { get; set; }
        public int Agility { get; set; }


    }
}