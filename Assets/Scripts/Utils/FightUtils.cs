using System;
using System.Text;
using LifeLike.Characters;
using LifeLike.Controls;
using LifeLike.Inferfaces;
using UnityEngine;
using UnityEngine.UI;

namespace LifeLike.Utils
{
    public class FightUtils : MonoBehaviour
    {

        public void AttackPlayer(Character enemy)
        {
            UI.AddLog(enemy.Attack(PlayerManager.Instance.Statistic));
        }

        public void Awake()
        {
            if (Instance == null) Instance = this;
        }

        public IUIManager UI
        {
            get { return UIManager.Instance; }
        }

        public Character Player
        {
            get { return PlayerManager.Instance.Statistic; }
        }
        private bool CheckIsEnemyIsNotNull
        {
            get
            {
                return EnemyUtils.SelectedEnemy != null && Player != null;
            }
        }

        public static FightUtils Instance { get; set; }

        public void AttackEnemy()
        {
            if (CheckIsEnemyIsNotNull)
            {
                GameLogicManager.Instance.EndPlayerTurn();
                var result = Player.Attack(EnemyUtils.SelectedEnemy.Statistic);
                UI.AddLog(result);
                if (!EnemyUtils.SelectedEnemy.IsDead) return;
                EnemyIsDead(EnemyUtils.SelectedEnemy);

            }
        }
        public void SpecialAttackEnemy()
        {
            if (CheckIsEnemyIsNotNull)
            {
                GameLogicManager.Instance.EndPlayerTurn();
                var result = Player.SpecialAction(EnemyUtils.SelectedEnemy.Statistic);
                UI.AddLog(result);
                if (!EnemyUtils.SelectedEnemy.IsDead) return;
                EnemyIsDead(EnemyUtils.SelectedEnemy);
            }
        }

        public void EnemyIsDead(Enemy selectedEnemy)
        {
            var builder = new StringBuilder();
            builder.AppendFormat("Enemy called {0} is now Dead, SHAME OF YOU! Great Job!! ",
                selectedEnemy.Statistic.Name);
            var experience = selectedEnemy.Statistic.Level * 10;
            Player.CurrentExperience += experience;
            builder.AppendFormat("\nYou receive {0} exp from {1}",
             experience, selectedEnemy.Statistic.Name);
            builder.AppendLine(Player.CalculateLvl());
            EnemyManager.Instance.KillEnemy(selectedEnemy);
            Player.KilledEnemies++;
            UI.AddLog(builder.ToString());
        }


    }

}