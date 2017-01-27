using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameLogic : MonoBehaviour
{
    //GAME OBJECTS
    public GameObject Player;
    public GameObject FinishMap;

//UI
    public Text MapCounterText;
    public Text MapTimeText;
    public Text MapTotalText;

//Scripts
    public MapGenerate Generator;

    public static string FinishPointTag="FinishPoint";
    private int _map=0;
    public static string GameLogicTag="GameLogic";
    public static string PlayerTag="Player";
    DateTime _currentTime = DateTime.Now;
    private TimeSpan _totalTime;

    // Use this for initialization
	void Start ()
	{
	    //_mapLogic=FinishMap.GetComponent<MapLogic>();
	    NewGame();

	}

    public void NewGame()
    {
        Debug.Log(string.Format("Map: {0}, Next Map Event",_map));

        _currentTime = DateTime.Now;
        _totalTime = _currentTime - _currentTime;
        if (Generator == null) return;
        Generator.GenerateMap(_map,Player);
        SetMapText();

    }
    public void NextMap()
    {
        Debug.Log(string.Format("Map: {0}, Next Map Event",_map));

        SetupTime();

        if (Generator == null) return;
        Generator.CleanMap();
        Generator.GenerateMap(_map++,Player);
        SetMapText();
    }

    private void SetupTime()
    {
        _totalTime += DateTime.Now - _currentTime;
        _currentTime = DateTime.Now;
    }

    private void SetMapText()
    {
        if (MapCounterText == null) return;

        MapCounterText.text = string.Format("Map: {0}",_map);
        if (MapTotalText==null) return;
        MapTotalText.text = string.Format("Total Time {0}s",_totalTime.Seconds);
    }

    // Update is called once per frame
	void Update ()
	{
	    if (MapTimeText == null) return;
	    var seconds = (DateTime.Now - _currentTime).Seconds;

	    MapTimeText.text = string.Format("Time: {0}s",seconds);

	}

}
