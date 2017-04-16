using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Characters;
using Assets.Scripts.Utils;
using Characters;
using Controls;
using UnityEngine;
using Utils;

public class GameManager : MonoBehaviour
{
    private MapManager _mapManager;
    public static GameManager Instance = null;
    private int _level=0;
    public BaseCharacter Player;
    public KeyCode KeyUp=KeyCode.UpArrow;
    public KeyCode KeyDown=KeyCode.DownArrow;
    public KeyCode KeyLeft=KeyCode.LeftArrow;
    public KeyCode KeyRight=KeyCode.RightArrow;
    public KeyCode FightNormal=KeyCode.Space;
    public KeyCode FightSpecial=KeyCode.E;
    public KeyCode ReloadWeapon=KeyCode.R;
    public KeyCode SelectEnemyKey=KeyCode.Tab;

    public KeyCode LightKey=KeyCode.F;

    public bool IsDay = false;
    public FightSystemUtils FightSystem;
    //Private
    public List<Enemy> Enemies=new List<Enemy>();

    // Use this for initialization
    void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance!=this) Destroy(gameObject);
       DontDestroyOnLoad(gameObject);
        _mapManager = GetComponent<MapManager>();
        FightSystem = GetComponent<FightSystemUtils>();
        InitGame();
    }

    private void InitGame()
    {
        Enemies.Clear();

        _mapManager.StartLevel(_level++);

    }

    public void GameOver()
    {
        enabled = false;
    }


	// Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(LightKey)) IsDay = !IsDay;
        if (Input.GetKeyDown(FightNormal)) FightSystem.AttackEnemy();
        if (Input.GetKeyDown(SelectEnemyKey)) SelectEnemy();

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
        if (Enemies.Count <= 0) return;
        if (EnemyUtils.EnemyIndex == Enemies.Count) EnemyUtils.EnemyIndex = 0;
        foreach (var enemy in Enemies)
        {
            enemy.IsSelected = false;
        }
        Debug.Log("Selected Enemy: "+EnemyUtils.SelectedEnemy.EnemyCharacter.Name + "Current Index: "+ EnemyUtils.EnemyIndex);
        Enemies[EnemyUtils.EnemyIndex].IsSelected = true;
        EnemyUtils.EnemyIndex++;
    }


    public void SelectEnemy(Enemy component)
    {
         //Yeah !
        EnemyUtils.EnemyIndex = 0;
        Debug.Log("Selected Enemy Index: "+EnemyUtils.EnemyIndex);

    }
}

