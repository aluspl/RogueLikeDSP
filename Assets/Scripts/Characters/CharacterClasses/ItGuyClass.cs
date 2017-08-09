using System.Collections.Generic;
using LifeLike.Enums;
using LifeLike.Utils;
using UnityEngine;

namespace LifeLike.Characters.CharacterClasses
{
    public class ItGuyClass : Character
    {
        public new static readonly string ClassName = "IT Guy";

        public ItGuyClass(CharacterStatisticDataModel statistic) : base(statistic)
        {
            SelectedClass=ClassName;
        }

        public override string SpecialAction(Character enemyCharacter)
        {
            switch (SelectedSpecialAttack)
            {
                case "Tech Talk":
                    return AttackTechTalk(enemyCharacter);
                case "Phantom IT Device Attack":
                    return AttackPhantomDevice(enemyCharacter);
                case "IT Rage":
                    return AttackITRage(enemyCharacter);
                default:
                    return Attack(enemyCharacter);
            }
        }

        private string AttackPhantomDevice(Character enemyCharacter)
        {
            return string.Empty;
        }
        private string AttackITRage(Character enemyCharacter)
        {
            return string.Empty;
        }
        private string AttackTechTalk(Character enemyCharacter)
        {    
            if (!CheckStamina(TechTalkCost)) return GameLogUtils.LowStamina();
           
            int YourPoints = random.Next(Inteligence);
            int EnemyPoints = random.Next(enemyCharacter.Endurance);
                            
            if (YourPoints < EnemyPoints)
                 return GameLogUtils.TechTalk(this, false);

            enemyCharacter.Status = Status.Sleep;
            return GameLogUtils.TechTalk(this, true);
        }

        public const string ClassType = "ITGuyClass";
        private readonly int TechTalkCost=10;

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
