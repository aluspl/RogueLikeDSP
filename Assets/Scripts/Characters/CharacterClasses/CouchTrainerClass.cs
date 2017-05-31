using System.Collections.Generic;
using LifeLike.Enums;
using LifeLike.Utils;
using UnityEngine;

namespace LifeLike.Characters.CharacterClasses
{
    public class CouchTrainerClass : Character
    {
        public new static readonly string ClassName = "Couch Trainer";

        public CouchTrainerClass(CharacterStatisticDataModel statistic) : base(statistic)
        {
            SelectedClass=ClassName;
        }

        public override string SpecialAction(Character enemyCharacter)
        {
            switch (SelectedSpecialAttack)
            {
                case "Invite to Training":
                    return InviteToTraining(enemyCharacter);              
            }
            return GameLogUtils.NoSelectedAttack;
        }

        private string AttackPhantomDevice(Character enemyCharacter)
        {
            return string.Empty;
        }

        private string InviteToTraining(Character enemyCharacter)
        {
            int YourPoints = random.Next(Charisma);
            int EnemyPoints = random.Next(enemyCharacter.Inteligence);
            Debug.Log(string.Format("Your Generated Charisma Chance: {0}, Enemy Generated Inteligence Point: {1}",YourPoints,
            EnemyPoints));
            if (YourPoints <EnemyPoints) 
              return GameLogUtils.CouchTraining(this, false);
            enemyCharacter.isEnemy =false;
            return GameLogUtils.CouchTraining(this, true);
        }

        public const string ClassType = "CouchTrainerClass";

        public override List<string> SpecialActionsList()
        {
            return new List<string>()
            {
                "Invite to Training",
            };
        }
    }
}
