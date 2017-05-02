using System.Collections.Generic;
using Enums;

namespace Characters.CharacterClasses
{
    public class ItGuyClass : BaseCharacter
    {
        public new static readonly string ClassName = "IT Guy";

        public ItGuyClass(CharacterStatisticDataModel statistic) : base(statistic)
        {
            SelectedClass=ClassName;
        }

        public override string SpecialAction(BaseCharacter enemyCharacter, string actionName)
        {
            switch (actionName)
            {
                case "Tech Talk":
                    return AttackTechTalk(enemyCharacter);
                case "Phantom IT Device Attack":
                    return AttackPhantomDevice(enemyCharacter);
                default:
                    return Attack(enemyCharacter);
            }
        }

        private string AttackPhantomDevice(BaseCharacter enemyCharacter)
        {
            return string.Empty;
        }

        private string AttackTechTalk(BaseCharacter enemyCharacter)
        {
            if (_random.Next(Charisma) >= _random.Next(Inteligence)) return GameLogSystem.TechTalk(this, false);
            enemyCharacter.Status = Status.Sleep;
            return GameLogSystem.TechTalk(this, true);
        }

        public const string ClassType = "ITGuyClass";

        public override List<string> SpecialActionsList()
        {
            return new List<string>()
            {
                "Tech Talk",
                "Phantom IT Device Attack"
            };
        }
    }
}
