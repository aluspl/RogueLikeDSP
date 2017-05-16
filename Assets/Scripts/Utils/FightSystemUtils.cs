using System;
using System.Text;
using LifeLike.Characters;
using LifeLike.Controls;
using UnityEngine;
using UnityEngine.UI;

namespace LifeLike.Utils
{
    public class FightSystemUtils : MonoBehaviour
    {

        public  void AttackPlayer(BaseCharacter enemy)
        {
            GameManager.Instance.UiUtils.AddLog(enemy.Attack(GameManager.Instance.PlayerStatistic));
        }

        public void Awake()
        {

        }

        public UIManager UI
        {
            get { return GameManager.Instance.UiUtils; }
        }

        public BaseCharacter Player
        {
            get { return GameManager.Instance.PlayerStatistic; }
        }
        private bool CheckIsEnemyIsNotNull
        {
            get {
                return EnemyUtils.SelectedEnemy != null && Player != null;
            }
        }

        public void AttackEnemy()
        {
            if (CheckIsEnemyIsNotNull)
            {  
                 GameManager.Instance.EndPlayerTurn();
                var result=Player.Attack(EnemyUtils.SelectedEnemy.EnemyCharacter);
                UI.AddLog(result);
                if (!EnemyUtils.SelectedEnemy.IsDead) return;
                EnemyIsDead(EnemyUtils.SelectedEnemy);
             
            }
            else
            {
                UI.AddLog("Enemy isn't selected");
            }       

        }
        public void SpecialAttackEnemy()
        {
             if (CheckIsEnemyIsNotNull)
             {  
                GameManager.Instance.EndPlayerTurn();
                var result=Player.SpecialAction(EnemyUtils.SelectedEnemy.EnemyCharacter);
                UI.AddLog(result);
                if (!EnemyUtils.SelectedEnemy.IsDead) return;
                 EnemyIsDead(EnemyUtils.SelectedEnemy);
             }
            else
            {
                UI.AddLog("Enemy isn't selected");
            }      
                   
     }

        public  void EnemyIsDead(Enemy selectedEnemy)
        {
            var builder=new StringBuilder();
            builder.AppendFormat("Enemy called {0} is now Dead, SHAME OF YOU! Great Job!! ",
                selectedEnemy.EnemyCharacter.Name);
            var experience=selectedEnemy.EnemyCharacter.Level * 10;
            Player.CurrentExperience += experience;
            builder.AppendFormat("\nYou receive {0} exp from {1} and you have  now {2}",
             experience, selectedEnemy.EnemyCharacter.Name, Player.CurrentExperience);
            GameManager.Instance.KillEnemy(selectedEnemy);
            Player.KilledEnemies++;
            UI.AddLog(builder.ToString());
        }

      
    }

}