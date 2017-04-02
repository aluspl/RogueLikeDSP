using System;
using System.Collections.Generic;
using Characters.CharacterClasses;
using Interfaces;

namespace Characters
{
    public abstract class BaseCharacter : ICharacter
    {
        public static string ClassName;

         /**
        *
        *Gnerate Actions Points based on agility
        */
        protected BaseCharacter(CharacterStatisticDataModel statistic)
        {
            if (statistic != null)
            {
                Name = statistic.Name;
                Strength = statistic.Strength;
                Inteligence = statistic.Inteligence;
                Agility = statistic.Agility;
                Charisma = statistic.Charisma;
                Endurance = statistic.Endurance;
            }
            else
            {
                Name = "Default";
                Strength = 1;
                Inteligence = 1;
                Agility = 1;
                Charisma = 1;
                Endurance = 1;
            }
            HealthPoint = Endurance * 10;
            Level = 0;
            CurrentExperience = 0;
        }
        public int ChanceToAttack(int chance)
        {
            throw new System.NotImplementedException();
        }
        public int CriticalChance(int chance)
        {
            throw new System.NotImplementedException();
        }
        public int GetActionsPoints()
        {
            return Agility/2;
        }
        public virtual string Attack(BaseCharacter enemy)
        {
            var damage = Strength;
            enemy.HealthPoint -= damage;
            return GameLogSystem.Attack(damage, enemy);
        }
        public virtual string SpecialAction(BaseCharacter enemyCharacter, string actionName)
        {
            throw new System.NotImplementedException();
        }

        public virtual List<string> SpecialActionsList()
        {
            throw new System.NotImplementedException();

        }
        public string Name { get; set; }
        public int Strength { get; set; }
        public int Level { get; set; }
        public long CurrentExperience { get; set; }
        public int Inteligence { get; set; }
        public int Charisma { get; set; }
         public int Agility { get; set; }
        public int Endurance { get; set; }
        public int HealthPoint { get; set; }

    }
}