using System.Collections;
using System.Linq;
using LifeLike.Characters;
using LifeLike.Characters.CharacterClasses;
using LifeLike.Controls;
using UnityEngine;

namespace LifeLike.Utils
{
    public class EnemyUtils
    {     
        private static int _enemyCount;
        public static Character GenerateEnemy()
        {
            var classes = CharacterFactory.EnemyClassList();
            var lvl = PlayerManager.Instance?.Statistic!=null? PlayerManager.Instance.Statistic.Level : 1; 
            var statistic =  EnemyGenerator.GenerateEnemy(lvl);
         
            return CharacterFactory.GetPlayerClass(classes.FirstOrDefault().Key, statistic);
        }
        public static void SelectEnemy()
        {
            var nearby = EnemyManager.Instance.List.Where(p=>p.Distance<=MAXDISTANCE).OrderBy(p => p.Distance).ToList();
            if (nearby.Count <= 0) return;

            EnemyIndex++;

            if (EnemyIndex == nearby.Count) EnemyIndex = 0;
            UnSelectAllEnemies();
            nearby[EnemyIndex>=nearby.Count?  0 : EnemyIndex ].IsSelected = true;
            PlayerManager.Instance?.Object.RotateToEnemy();
        }

        public static IEnumerator EnemiesMove()
        {
            yield return new WaitForSeconds(WaitTime);     

            if (EnemyManager.Instance.List.Count==0)
            {
               yield return new WaitForSeconds(WaitTime);     
            }
            if (EnemyManager.Instance.List!=null)
                for (int i =0; i<EnemyManager.Instance.List.Count; i++)
                {
                    var enemy=EnemyManager.Instance.List[i];
                    enemy.IsHisTurn = true;
                    enemy.MoveToPlayer(PlayerManager.Instance.Object);
                    yield return new WaitForSeconds(enemy.MoveTime);
                }
            if(GameLogicManager.Instance!=null)
            GameLogicManager.Instance.IsPlayerTurn=true;
        }

        public static int MAXDISTANCE = 1;
        private static float WaitTime=0.1f;

        public static Enemy SelectedEnemy
        {
            get
            {
                // Property action for unselected enemy or empty list ... or whatever :D 
                return (EnemyManager.Instance!=null && EnemyManager.Instance.List.Count>0)
                     ? EnemyManager.Instance.List.FirstOrDefault(p=>p.IsSelected)
                    : null;
            }
            set
            {
                UnSelectAllEnemies();
                value.IsSelected = true;
            }
        }

        public static int EnemyIndex { get; set; }

        public static void UnSelectAllEnemies()
        {
            foreach (var enemy in  EnemyManager.Instance.List)
            {
                enemy.IsSelected = false;
            }
        }
    }
}