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
            GameManager.Instance.UIUtils.AddLog(enemy.Attack(GameManager.Instance.PlayerStatistic));
        }

        public void Awake()
        {

        }



        private bool CheckIsEnemyIsNotNull
        {
            get {
                return EnemyUtils.SelectedEnemy != null && GameManager.Instance.PlayerStatistic != null;
            }
        }

        public void AttackEnemy()
        {
            if (CheckIsEnemyIsNotNull)
                GameManager.Instance.UIUtils.AddLog(GameManager.Instance.PlayerStatistic.Attack(EnemyUtils.SelectedEnemy.EnemyCharacter));
        }
    }

}