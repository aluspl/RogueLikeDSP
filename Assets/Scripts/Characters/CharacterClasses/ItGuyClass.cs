﻿using System.Collections.Generic;
using Enums;

namespace Characters.CharacterClasses
{
    public class ItGuyClass : BaseCharacter
    {
        public static string ClassName = "IT Guy";

        public ItGuyClass(CharacterStatisticDataModel statistic) : base(statistic)
        {
        }

        public override string SpecialAction(BaseCharacter enemyCharacter, string actionName)
        {
            switch (actionName)
            {
                case "Tech Talk":
                    return AttackTechTalk(enemyCharacter);
                    break;
                case "Phantom IT Device Attack":
                    return AttackPhantomDevice(enemyCharacter);
                    break;
                default:
                    return Attack(enemyCharacter);
            }
        }

        private string AttackPhantomDevice(BaseCharacter enemyCharacter)
        {
            throw new System.NotImplementedException();
        }

        private string AttackTechTalk(BaseCharacter enemyCharacter)
        {
            if (_random.Next(Charisma) >= _random.Next(Inteligence)) return GameLogSystem.TechTalk(false);
            enemyCharacter.Status = Status.Sleep;
            return GameLogSystem.TechTalk(true);
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
