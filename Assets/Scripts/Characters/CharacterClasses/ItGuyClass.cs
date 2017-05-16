﻿using System.Collections.Generic;
using LifeLike.Enums;
using LifeLike.Utils;
using UnityEngine;

namespace LifeLike.Characters.CharacterClasses
{
    public class ItGuyClass : BaseCharacter
    {
        public new static readonly string ClassName = "IT Guy";

        public ItGuyClass(CharacterStatisticDataModel statistic) : base(statistic)
        {
            SelectedClass=ClassName;
        }

        public override string SpecialAction(BaseCharacter enemyCharacter)
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

        private string AttackPhantomDevice(BaseCharacter enemyCharacter)
        {
            return string.Empty;
        }
        private string AttackITRage(BaseCharacter enemyCharacter)
        {
            return string.Empty;
        }
        private string AttackTechTalk(BaseCharacter enemyCharacter)
        {
            
       
            int YourPoints = random.Next(Inteligence);
            int EnemyPoints = random.Next(enemyCharacter.Endurance);
                 
            Debug.Log(string.Format("Your Generated Points Chance: {0}, Enemy Generated Point: {1}",YourPoints,
            EnemyPoints));
            if (YourPoints < EnemyPoints)
                 return GameLogUtils.TechTalk(this, false);

            enemyCharacter.Status = Status.Sleep;
            return GameLogUtils.TechTalk(this, true);
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
