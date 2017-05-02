using System;
using System.Text;
using Characters;
using Controls;
using UnityEngine;
using UnityEngine.UI;

namespace Utils
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
                result = EnemyIsDead(EnemyUtils.SelectedEnemy);
                UI.AddLog(result);
            }
            else
            {
                UI.AddLog("Enemy isn't selected");
            }       

        }

        private string EnemyIsDead(Enemy selectedEnemy)
        {
            var builder=new StringBuilder();
            builder.AppendFormat("Enemy called {0} is now Dead, SHAME OF YOU! Great Job!! ",
                selectedEnemy.EnemyCharacter.Name);
            var experience=selectedEnemy.EnemyCharacter.Level * 10;
            Player.CurrentExperience += experience;
            builder.AppendFormat("\nYou receive {0} exp from {1} and you have  now {2}", experience,
                selectedEnemy.EnemyCharacter.Name, Player.CurrentExperience);
            GameManager.Instance.KillEnemy(selectedEnemy);
            return builder.ToString();
        }
    }

}