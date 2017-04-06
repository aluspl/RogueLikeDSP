using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using Characters;
using Characters.CharacterClasses;
using Enums;
using Interfaces;
using NUnit.Framework;

namespace MapGeneratorTest
{
    [TestFixture]
    public class CharacterSystem
    {
        [Test]
        public void IsPlayerClassListExist()
        {
            var player = CharacterFactory.PlayerClassList();
            Assert.IsNotNull(player);
            Assert.AreNotEqual(player.Count, 0);
            Assert.AreNotEqual(player.Count, -1);

        }
        [Test]
        public void IsAttackWork()
        {
            var basicstatistic = new CharacterStatisticDataModel
            {
                Name = "Test",
                Agility = 1,
                Charisma = 1,
                Endurance = 1,
                Inteligence = 1,
                Strength = 1
            };

            var player = CharacterFactory.GetPlayerClass("ITGuyClass",basicstatistic);

            var enemy = CharacterFactory.GetPlayerClass("ITGuyClass",basicstatistic);
            Assert.IsFalse(player.ChanceToAttack(enemy));
            Assert.IsFalse(player.CriticalChance());
            
            Assert.IsNotNullOrEmpty(player.Attack(enemy));
        }
        [Test]
        public void CheckStatistic()
        {
            var player = CharacterFactory.GetPlayerClass("ITGuyClass",
                new CharacterStatisticDataModel
            {
                Name = "Test",
                Agility = 1,
                Charisma = 1,
                Endurance = 1,
                Inteligence = 1,
                Strength = 1
            });
            Assert.AreNotEqual(player.Agility,0);
            Assert.AreNotEqual(player.Agility,2);
            Assert.AreEqual(player.Agility,1);
            Assert.AreNotEqual(player.Charisma,0);
            Assert.AreNotEqual(player.Charisma,2);
            Assert.AreEqual(player.Charisma,1);
            Assert.AreNotEqual(player.Endurance,0);
            Assert.AreNotEqual(player.Endurance,2);
            Assert.AreEqual(player.Endurance,1);
            Assert.AreNotEqual(player.Inteligence,0);
            Assert.AreNotEqual(player.Inteligence,2);
            Assert.AreEqual(player.Inteligence,1);
            Assert.AreNotEqual(player.Strength,0);
            Assert.AreNotEqual(player.Strength,2);
            Assert.AreEqual(player.Strength,1);

        }


        [Test]
        public void IsEnemyClassListExist()
        {
            var player = CharacterFactory.EnemyClassList();
            Assert.IsNotNull(player);
        }

        [Test]
        public void IsReturnMap()
        {
            int x = 10, y = 10;
            var map = new MapElement[x, y];
            Assert.IsNotNull(map);
        }

        [Test, TestCaseSource(typeof(CharacterData), "Classes")]
        public string GenerateCharacter(string className)
        {
            try
            {
                var statistic = new CharacterStatisticDataModel()
                {
                    Agility = 1,
                    Charisma = 1,
                    Endurance = 1,
                    Inteligence = 1,
                    Strength = 1,
                    CurrentExperience = 0,
                    Level = 0,
                    Name = className
                };
                return CharacterFactory.GetPlayerClass(className, statistic).Name;
            }
            catch (Exception)
            {
                return null;
            }
    }

}


public class CharacterData
{
    public static IEnumerable Classes
    {
        get
        {
            yield return new TestCaseData("any").Returns(null); //Success
            yield return new TestCaseData("any").Returns("IT Guy"); //Success
            yield return new TestCaseData("any").Returns("Hobo"); //Success

            yield return new TestCaseData("ITGuyClass").Returns(null); //true
            yield return new TestCaseData("any").Returns(null); //Success
            yield return new TestCaseData("ITGuyClass").Returns("IT Guy"); //Success
            yield return new TestCaseData("HoboClass").Returns("Hobo"); //Success
            yield return new TestCaseData("SuperHoboClass").Returns("Super Hobo"); //Success

            yield return new TestCaseData(string.Empty).Returns(null); //success
            yield return new TestCaseData("CorpoRatClass").Returns(null); //success
            yield return new TestCaseData("CorpoBossClass").Returns(null); //success
            yield return new TestCaseData("CorpoRatClass").Returns("Corpo Rat"); //success
            yield return new TestCaseData("CorpoBossClass").Returns("Corpo Boss"); //success

        }
    }
}




}
