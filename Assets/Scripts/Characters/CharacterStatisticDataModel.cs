using Characters.CharacterClasses;

namespace Characters
{
    public class CharacterStatisticDataModel
    {
        public string Name { get; set; }
        public int Strength { get; set; }
        public int Level { get; set; }
        public long CurrentExperience { get; set; }
        public int Inteligence { get; set; }
        public int Charisma { get; set; }
        public int Agility { get; set; }
        public int Endurance { get; set; }
    }
}