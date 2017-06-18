﻿using System;
using System.Collections.Generic;
using LifeLike.Enums;
using LifeLike.Interfaces;
using UnityEngine;
using LifeLike.Utils;
using Random = System.Random;

namespace LifeLike.Characters
{
    public abstract class BaseCharacter : ICharacter
    {
        public static string ClassName;
        protected readonly Random random = new Random();

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
            isEnemy= statistic.IsEnemy;

            HealthPoint = Endurance * 10;
            MaxHealthPoint = HealthPoint;
            Level = 1;
            CurrentExperience = 0;
            Debug.Log(Name +":"+ ClassName);
        }
        //Check Greater Random in AgilitySlider
        private bool ChanceToAttack(BaseCharacter enemy)
        {
           
            var yourChance = random.Next(Agility);
            var enemyChance = random.Next(enemy.Agility);
            return yourChance > enemyChance;
        }

        private bool CriticalChance()
        {
            return random.Next(100)*Agility>90;
        }
        public int GetActionsPoints()
        {
            return Agility/2;
        }
        public virtual string Attack(BaseCharacter enemy)
        {
            if (!ChanceToAttack(enemy)) return GameLogUtils.MissedAttack(this);
            var damage = CriticalChance() ? random.Next(Strength) : random.Next(Strength) * 2;
            damage=enemy.Defense(damage);
            return GameLogUtils.Attack(damage,this, enemy);
        }

        private int Defense(int damage)
        {
            HealthPoint -= damage;
            CheckHealth();
            return damage;
        }

        internal void StatusChange()
        {
        }

        private void CheckHealth()
        {
            if (HealthPoint<=0)
            {
                if (!isEnemy) 
                    GameManager.Instance.GameOver();                   
            }
        }

        public virtual string SpecialAction(BaseCharacter enemyCharacter)
        {
            return string.Empty;
        }

        public virtual List<string> SpecialActionsList()
        {
             return new List<string>();
        }
        public string SelectedSpecialAttack {get; set;}

        public void SelectSpecialAttack()
        {
            if (SpecialActionsList().Count==0) SelectedSpecialAttack=string.Empty;
            else
            {
                if (SpecialActionIndex>SpecialActionsList().Count) SpecialActionIndex=0;
                    SelectedSpecialAttack=SpecialActionsList()[SpecialActionIndex++];
            }
            Debug.Log("Selected Special Attack: "+SelectedSpecialAttack);
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
        public string SelectedClass { get; set; }
        public int MaxHealthPoint { get; set; }
        public bool isEnemy {get; set; }
        public int KilledEnemies { get;  set; }
        private int SpecialActionIndex;
    }
}