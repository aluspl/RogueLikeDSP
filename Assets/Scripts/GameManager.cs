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
    private MapManager _mapManager;
    public static GameManager Instance = null;
    private int _level=0;
    public BaseCharacter PlayerStatistic;
    public KeyCode KeyUp=KeyCode.UpArrow;
    public KeyCode KeyDown=KeyCode.DownArrow;
    public KeyCode KeyLeft=KeyCode.LeftArrow;
    public KeyCode KeyRight=KeyCode.RightArrow;
    public KeyCode FightNormalKey=KeyCode.Space; 
    public KeyCode FightNormalKeyPad= KeyCode.Joystick1Button16;
    public KeyCode FightSpecial=KeyCode.E | KeyCode.Joystick1Button14;
    public KeyCode FightSpecialPad= KeyCode.Joystick1Button14;
    
    public KeyCode ReloadWeapon=KeyCode.R;
    public KeyCode ReloadWeaponPAD= KeyCode.Joystick1Button17;

    public KeyCode SelectEnemyKey=KeyCode.Tab  | KeyCode.Joystick1Button13;
    public KeyCode SelectEnemyKeyPAD=KeyCode.Joystick1Button13;
    public KeyCode ExitKey=KeyCode.Escape;
    public KeyCode ExitKeyPAD=KeyCode.Joystick1Button9;
    public KeyCode OpenDetailKey = KeyCode.I;
    public KeyCode OpenDetailKeyPAD =KeyCode.Joystick1Button10;
    public KeyCode LightKey=KeyCode.F;
    public KeyCode LightKeyPAD=KeyCode.Joystick1Button18;

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


    // Use this for initialization
    void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance!=this) Destroy(gameObject);
           DontDestroyOnLoad(gameObject);


        _mapManager = GetComponent<MapManager>();
        FightSystem = GetComponent<FightSystemUtils>();
        UiUtils = GetComponent<UIManager>();
        _gameUI = GetComponentInChildren<Canvas>();
        InitGame();
    }

    private void InitGame()
    {
       // IsDay=!IsDay;
        OpenCharacterCreatorWindow();
        _mapManager.CleanMap();
        _mapManager.StartLevel(_level++);
        IsPlayerTurn = true;
        UiUtils.ClearLog();
    }

    public void EndPlayerTurn()
    {
        UiUtils.AddLog("Enemies Turn");
        IsPlayerTurn = false;
        if (PlayerObject!=null && PlayerStatistic!=null)
           EnemyUtils.EnemiesMove(PlayerObject);
        UiUtils.AddLog("Player Turn");
        
        IsPlayerTurn = true;
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
            if (Input.GetKeyDown(FightNormalKey) || Input.GetKeyDown(FightNormalKeyPad)) FightSystem.AttackEnemy();
            if (Input.GetKeyDown(SelectEnemyKey)|| Input.GetKeyDown(SelectEnemyKeyPAD)) EnemyUtils.SelectEnemy();
            if (Input.GetKeyDown(ExitKey)|| Input.GetKeyDown(ExitKeyPAD)) EndGame();
            if (Input.GetKeyDown(OpenDetailKey)|| Input.GetKeyDown(OpenDetailKeyPAD)) OpenDetail();
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