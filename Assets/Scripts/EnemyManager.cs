﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LifeLike.Controls;
using LifeLike.Utils;
using LifeLike.Inferfaces;

namespace LifeLike
{
	public class EnemyManager : MonoBehaviour, IManager {

		public List<Enemy> List = new List<Enemy>();

		public static EnemyManager Instance = null;
		// Use this for initialization
		void Awake () {
				if (Instance == null) Instance = this;
			//	DontDestroyOnLoad(gameObject);
		}
		


        public void AddEnemy(Enemy enemy)
        {
            List.Add(enemy);
        }

        public void KillEnemy(Enemy enemy)
        {
            try
            {
                List.Remove(enemy);
                Destroy(enemy.gameObject);
                EnemyUtils.UnSelectAllEnemies();
            }
            catch (Exception)
            {
                Debug.Log("Enemy shouldn't be here!");
            }
        }

        public void SelectEnemy(Enemy component)
        {
            //Yeah !
            EnemyUtils.SelectedEnemy = component;
            Debug.Log("Selected Enemy Index: " + EnemyUtils.EnemyIndex);
        }
       public void Destroy()
        {
            List.Clear();
//            Destroy(this.gameObject);

        }
    }
}

