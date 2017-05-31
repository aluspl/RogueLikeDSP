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

namespace LifeLike
{
    public class GameManager : MonoBehaviour, IGameManager
    {
        private MapManager MapManager;
        public static IGameManager Instance = null;

        private int _level = 0;

        public bool IsDay {get; set; }
        public CreateCharacterEditor CharacterEditorPrefab;
        public CharacterDetailView PlayerDetailPrefab;

        public bool OpenedWindow { get; set;}
        public bool IsPlayerTurn {get; set;}

        public bool IsEnemyTurn { get; internal set; }
    
        // Use this for initialization
        void Awake()
        {
            if (Instance == null) Instance = this;
         //   DontDestroyOnLoad(gameObject);
           // DI.Inject(this);

            MapManager = GetComponent<MapManager>();
            InitGame();
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
            UIManager.Instance.Destroy();
            PlayerManager.Instance.Destroy();
            EnemyManager.Instance.Destroy();
            
            Destroy();
            SceneManager.LoadScene(0);
        }


        // Update is called once per frame
        void Update()
        {
            if (!OpenedWindow)
            {
                if (Input.GetKeyDown(InputManager.Instance.FightNormalKey)) FightUtils.Instance.AttackEnemy();
                if (Input.GetKeyDown(InputManager.Instance.FightSpecialKey)) FightUtils.Instance.SpecialAttackEnemy();
                if (Input.GetKeyDown(InputManager.Instance.SelectEnemyKey)) EnemyUtils.SelectEnemy();
                if (Input.GetKeyDown(InputManager.Instance.SelectSpecialAttackKey))  PlayerManager.Instance.Statistic.SelectSpecialAttack();
                if (Input.GetKeyDown(InputManager.Instance.ExitKey)) EndGame();
                if (Input.GetKeyDown(InputManager.Instance.OpenDetailKey)) OpenDetail();
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

        private void OpenDetail()
        {
            if (PlayerManager.Instance.Statistic == null || PlayerDetailPrefab == null) return;
            Instantiate(PlayerDetailPrefab);
            OpenedWindow = true;
        }

        private void OpenCharacterCreatorWindow()
        {
            // PlayerStatistic = CharacterFactory.GetPlayerClass("ITGuyClass", new CharacterStatisticDataModel(){
            //     Agility=6,
            //     Strength=6,
            //      Name="Test",
            //      Endurance=1 });

            if (PlayerManager.Instance.Statistic != null || CharacterEditorPrefab == null) return;
            Instantiate(CharacterEditorPrefab);
            OpenedWindow = true;
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