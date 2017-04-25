﻿﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Characters;
using Characters;
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
    public KeyCode FightSpecial=KeyCode.E;
    public KeyCode ReloadWeapon=KeyCode.R;
    public KeyCode SelectEnemyKey=KeyCode.Tab;
    public KeyCode ExitKey=KeyCode.Escape;
    public KeyCode OpenDetailKey = KeyCode.I;
    public KeyCode LightKey=KeyCode.F;

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
        OpenCharacterCreatorWindow();
        _mapManager.CleanMap();
        _mapManager.StartLevel(_level++);

        UiUtils.ClearLog();
    }



    public void GameOver()
    {
        enabled = false;
    }


	// Update is called once per frame
    void Update()
    {
        if (!OpenedWindow)
        {
            if (Input.GetKeyDown(LightKey)) IsDay = !IsDay;
            if (Input.GetKeyDown(FightNormalKey)) FightSystem.AttackEnemy();
            if (Input.GetKeyDown(SelectEnemyKey)) SelectEnemy();
            if (Input.GetKeyDown(ExitKey)) EndGame();
            if (Input.GetKeyDown(OpenDetailKey)) OpenDetail();
            _gameUI.enabled = true;
        }
        else
        {
            _gameUI.enabled = false;
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
        if (PlayerStatistic != null || CharacterEditorPrefab == null) return;
        Instantiate(CharacterEditorPrefab);
        OpenedWindow = true;
    }

    private void EndGame()
    {
        InitGame();
    }

    public void FinishMap()
    {
        _mapManager.StartLevel(_level++);
    }

    public void AddEnemy(Enemy enemy)
    {
        Enemies.Add(enemy);
    }

    public void KillEnemy(Enemy enemy)
    {
        Enemies.Remove(enemy);
    }

    private void SelectEnemy()
    {
        Enemies = Enemies.OrderBy(p => p.Distance).ToList();
        if (Enemies.Count <= 0) return;
        EnemyUtils.EnemyIndex++;
        if (EnemyUtils.EnemyIndex == Enemies.Count) EnemyUtils.EnemyIndex = 0;
        foreach (var enemy in Enemies)
        {
            enemy.IsSelected = false;
        }
        Enemies[EnemyUtils.EnemyIndex].IsSelected = true;
        UiUtils.AddLog("<b>Selected Enemy</b>: "+ EnemyUtils.SelectedEnemy.EnemyCharacter.Name);

    }


    public void SelectEnemy(Enemy component)
    {
         //Yeah !
        EnemyUtils.EnemyIndex = 0;
        Debug.Log("Selected Enemy Index: "+EnemyUtils.EnemyIndex);

    }
}