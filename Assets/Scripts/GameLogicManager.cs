﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LifeLike.Characters;
using LifeLike.Characters.CharacterClasses;
using LifeLike.Controls;
using UnityEngine;
using UnityEngine.SceneManagement;
using LifeLike.Utils;
using LifeLike.MapUtils;
using LifeLike.Inferfaces;
using LifeLike.Interfaces;
using LifeLike.Enums;

namespace LifeLike
{
    public class GameLogicManager : MonoBehaviour, IGameLogicManager
    {
        private MapManager MapManager;
        public static IGameLogicManager Instance = null;

        private int _level = 0;

        public bool IsDay {get; set; }

        public bool IsPlayerTurn {get; set;}

        public bool IsEnemyTurn { get; internal set; }
    
        // Use this for initialization
        void Awake()
        {
            if (Instance == null) Instance = this;
            MapManager = GetComponent<MapManager>();
            string ErrorModule=string.Empty;
            if (GameManager.Instance.AreAllModulesWork(out ErrorModule)) InitGame();
            else 
            {
                Debug.LogError("Problem with Loading instance: "+ErrorModule);
                GameManager.Instance.BuildContainer();
                InitGame();
            }
        }

        private void InitGame()
        {
            IsDay = !IsDay;
            OpenCharacterCreatorWindow();
            MapManager.CleanMap();
            MapManager.StartLevel(_level++);
            IsPlayerTurn = true;
            UIManager.Instance.ClearLog();
        }

        public void EndPlayerTurn()
        {
            IsPlayerTurn = false;
            if (PlayerManager.Instance.Object != null && PlayerManager.Instance.Statistic != null)
            {
                var coroutine = EnemyUtils.EnemiesMove();
                 StartCoroutine(coroutine);                
            }
        }


        public void GameOver()
        {
           
            GameManager.Instance.EndGame();
            SceneManager.LoadScene(0);
        }


        // Update is called once per frame
        void Update()
        {
            if (WindowManager.Instance.Status==WindowState.Close)
            {
                if (Input.GetKeyDown(InputManager.Instance.FightNormalKey)) FightUtils.Instance.AttackEnemy();
                if (Input.GetKeyDown(InputManager.Instance.FightSpecialKey)) FightUtils.Instance.SpecialAttackEnemy();
                if (Input.GetKeyDown(InputManager.Instance.SelectEnemyKey)) EnemyUtils.SelectEnemy();
                if (Input.GetKeyDown(InputManager.Instance.SelectSpecialAttackKey))  PlayerManager.Instance.Statistic.SelectSpecialAttack();
                if (Input.GetKeyDown(InputManager.Instance.ExitKey)) EndGame();
                if (Input.GetKeyDown(InputManager.Instance.OpenDetailKey)) WindowManager.Instance.Open(WindowType.Detail);
                //   detectPressedKeyOrButton();
                if (UIManager.Instance!=null)
                UIManager.Instance.Enabled = true;
            }
            else
            {
                if (UIManager.Instance!=null)
                UIManager.Instance.Enabled = false;
            }
            if (EnemyUtils.SelectedEnemy != null && EnemyUtils.SelectedEnemy.Distance > 1)
                EnemyUtils.UnSelectAllEnemies();

        }

       
        private void OpenCharacterCreatorWindow()
        { 
            
            WindowManager.Instance.Open(WindowType.Create);    
        }

        private void EndGame()
        {
            GameOver();
        }

        public void FinishMap()
        {
            InitGame();
        }

        public void Destroy()
        {
            Destroy(this.gameObject);
            Instance=null;
        }
    }
}