using System;
using LifeLike.Characters;
using LifeLike.Characters.CharacterClasses;

namespace LifeLike.Utils
{
    public static class GameLogUtils
    {
        internal static string NoSelectedAttack="You should select special attack";

        public static string Attack(int damage, Character character, Character enemy)
        {
            return string.Format("<b>{0}</b> Attack <b>{1}</b> for <b>{2}</b> damage. Enemy has now <b>{3}</b> HP",character.Name,  enemy.Name, damage, enemy.HealthPoint);
        }

        public static string MissedAttack(Character character)
        {
            return string.Format("<b>{0}</b>: Attack Missed",character.Name);
        }

        public static string TechTalk(Character character, bool success)
        {
            return success ? 
                string.Format("<b>{0}</b> has successfully bored <b>The Enemy</b>! He is sleeping now.  Good Job!",character.Name) 
                : string.Format("<b>{0}</b>'s Talk is not so boring to <b>Enemy</b>", character.Name);
        }

        internal static string CouchTraining(CouchTrainerClass character, bool success)
        {
            return success ? 
                string.Format("<b>{0}</b> has successfully changed <b>Enemy</b>! Who stole your brain?!",character.Name) 
                : string.Format("<b>{0}</b>'s You can't change <b>Enemy</b> to Zombie! He is too inteligent for you! ", character.Name);
                }

        internal static string LowStamina()
        {
            return "You need more stamina points! Drink a Coffee or something!";
        }
    }
}

