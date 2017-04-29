using System.Collections.Generic;
using Assets.Scripts.Characters;
using Assets.Scripts.Enums;
using Interfaces;
using UnityEngine;
using Utils;
using Random = System.Random;

namespace Characters
{
    public abstract class BaseCharacter : ICharacter
    {
        public static string ClassName;
        protected readonly Random _random = new Random();

         /**
        *
        *Gnerate Actions Points based on agility
        */
        protected BaseCharacter(CharacterStatisticDataModel statistic)
        {
            Name = statistic.Name;
            Strength = statistic.Strength;
            Inteligence = statistic.Inteligence;
            Agility = statistic.Agility;
            Charisma = statistic.Charisma;
            Endurance = statistic.Endurance;
            Perception = statistic.Perception;

            HealthPoint = Endurance * 10;
            Level = 1;
            CurrentExperience = 0;
            Debug.Log(Name +":"+ ClassName);
        }
        //Check Greater Random in AgilitySlider
        private bool ChanceToAttack(BaseCharacter enemy)
        {

            var yourChance = _random.Next(Agility);
            var enemyChance = _random.Next(enemy.Agility);
            Debug.Log(string.Format("Chance to attack: {0} vs {1}",yourChance,enemyChance));
            return yourChance > enemyChance;
        }

        private bool CriticalChance()
        {
            return _random.Next(100)*Agility>90;
        }
        public int GetActionsPoints()
        {
            return Agility/2;
        }
        public virtual string Attack(BaseCharacter enemy)
        {
            if (!ChanceToAttack(enemy)) return GameLogSystem.MissedAttack();
            var damage = CriticalChance() ? _random.Next(Strength) : _random.Next(Strength) * 2;
            damage=enemy.Defense(damage);
            return GameLogSystem.Attack(damage,enemy);
        }

        private int Defense(int damage)
        {
            HealthPoint -= damage;
       
            return damage;
        }

        public virtual string SpecialAction(BaseCharacter enemyCharacter, string actionName)
        {
            return string.Empty;
        }

        public virtual List<string> SpecialActionsList()
        {
             return new List<string>();
        }
        public string Name { get; set; }
        public int Strength { get; set; }
        public int Level { get; set; }
        public long CurrentExperience { get; set; }
        public int Inteligence { get; set; }
        public int Perception { get; set; }
        public int Charisma { get; set; }
        public int Agility { get; set; }
        public int Endurance { get; set; }
        public int HealthPoint { get; set; }
        public Status Status { get; set; }
        public string SelectedClass { get { return ClassName; } }
    }
}