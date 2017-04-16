using System.Text;
using Assets.Scripts.Characters;
using Characters;
using UnityEngine;
using Utils;

namespace Assets.Scripts.Utils
{
    public class FightSystemUtils : MonoBehaviour
    {
        StringBuilder  Log= new StringBuilder();
        public void Awake()
        {

        }
        public  void AttackPlayer(BaseCharacter Enemy)
        {
            AddLog(Enemy.Attack(GameManager.Instance.Player));
        }

        private void AddLog(string log)
        {
            Log.AppendLine(log);
        }

        public void AttackEnemy()
        {
            if (EnemyUtils.SelectedEnemy!= null)
                AddLog(GameManager.Instance.Player.Attack(EnemyUtils.SelectedEnemy.EnemyCharacter));
        }
    }

}