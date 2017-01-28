using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapLogic : MonoBehaviour
{

    public GameLogic gameLogic;

    // Use this for initialization
	void Start ()
	{
	    var gameLogicObject = GameObject.FindWithTag(GameLogic.GameLogicTag);
	    if (gameLogicObject == null) return;
	    gameLogic = gameLogicObject.GetComponent<GameLogic>();
	    Debug.Log(string.Format("GameLogic status {0}",gameLogic!=null));
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    void OnTriggerEnter2D(Collider2D other) {

        if (!other.CompareTag(GameLogic.PlayerTag)) return;
        Debug.Log(string.Format("Finish Point status {0}",other.name));

        if (gameLogic!=null) gameLogic.NextMap();
    }


}
