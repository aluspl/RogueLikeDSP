using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private MapManager _mapManager;
    public static GameManager Instance = null;
    private int _level=0;
    public KeyCode KeyUp=KeyCode.UpArrow;
    public KeyCode KeyDown=KeyCode.DownArrow;
    public KeyCode KeyLeft=KeyCode.LeftArrow;
    public KeyCode KeyRight=KeyCode.RightArrow;

    // Use this for initialization
    void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance!=this) Destroy(gameObject);
       DontDestroyOnLoad(gameObject);
        _mapManager = GetComponent<MapManager>();
        InitGame();
    }

    private void InitGame()
    {
        _mapManager.StartLevel(_level++);
    }

    public void GameOver()
    {
        enabled = false;
    }


	// Update is called once per frame
	void Update () {
		
	}

    public void FinishMap()
    {
        _mapManager.StartLevel(_level++);
    }
}
