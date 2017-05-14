﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Characters;
using Characters.CharacterClasses;
using Controls;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

public class GameManager : MonoBehaviour
{
    private MapManager MapManager;
    public static GameManager Instance = null;

    private int _level=0;
    public BaseCharacter PlayerStatistic;
  


    public GameObject PlayerObject;
    public bool IsDay = false;
    public FightSystemUtils FightSystem;
    public CreateCharacterEditor CharacterEditorPrefab;
    public CharacterDetailView PlayerDetailPrefab;

    public UIManager UiUtils;

    private Canvas _gameUI;
    //Private
    public List<Enemy> Enemies=new List<Enemy>();

    public bool OpenedWindow;
    public bool IsPlayerTurn;

    public bool IsEnemyTurn { get; internal set; }


    // Use this for initialization
    void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance!=this) Destroy(gameObject);
           DontDestroyOnLoad(gameObject);


        MapManager = GetComponent<MapManager>();
        FightSystem = GetComponent<FightSystemUtils>();
        UiUtils = GetComponent<UIManager>();
        _gameUI = GetComponentInChildren<Canvas>();
        InitGame();
    }

    private void InitGame()
    {
        IsDay=!IsDay;
        OpenCharacterCreatorWindow();
        MapManager.CleanMap();
        MapManager.StartLevel(_level++);
        IsPlayerTurn = true;
        UiUtils.ClearLog();
    }

    public void EndPlayerTurn()
    {
    //    UiUtils.AddLog("Enemies Turn");
        IsPlayerTurn = false;
        if (PlayerObject!=null && PlayerStatistic!=null)
        {
            try
            {
                var coroutine = EnemyUtils.EnemiesMove(PlayerObject);
                StartCoroutine(coroutine);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
        //UiUtils.AddLog("Player Turn");
        
    }


    public void GameOver()
    {
        Destroy(_gameUI.gameObject);
        Destroy(Instance);

        SceneManager.LoadScene(0);
    }


	// Update is called once per frame
    void Update()
    {
        if (!OpenedWindow)
        {
            if (Input.GetKeyDown(InputManager.Instance.FightNormalKey)) FightSystem.AttackEnemy();
            if (Input.GetKeyDown(InputManager.Instance.SelectEnemyKey)) EnemyUtils.SelectEnemy();
            if (Input.GetKeyDown(InputManager.Instance.ExitKey)) EndGame();
            if (Input.GetKeyDown(InputManager.Instance.OpenDetailKey)) OpenDetail();
         //   detectPressedKeyOrButton();
            _gameUI.enabled = true;
        }
        else
        {
            _gameUI.enabled = false;
        }
        if (EnemyUtils.SelectedEnemy!=null && EnemyUtils.SelectedEnemy.Distance>1) EnemyUtils.UnSelectAllEnemies();
        
    }
 public void detectPressedKeyOrButton()
 {
     foreach(KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
     {
         if (Input.GetKeyDown(kcode))
             Debug.Log("KeyCode down: " + kcode);
     }
 }
    private void OpenDetail()
    {
        if (PlayerStatistic == null || PlayerDetailPrefab == null) return;
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

        if (PlayerStatistic != null || CharacterEditorPrefab == null) return;
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

    public void AddEnemy(Enemy enemy)
    {
        Enemies.Add(enemy);
    }

    public void KillEnemy(Enemy enemy)
    {
        try
        {
            Enemies.Remove(enemy);
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
        Debug.Log("Selected Enemy Index: "+EnemyUtils.EnemyIndex);
    }
}