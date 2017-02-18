using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLogic : MonoBehaviour {

    private GameLogic _gameLogic;

    // Use this for initialization
	void Start () {
	    var gameLogicObject = GameObject.FindWithTag(GameLogic.GameLogicTag);
	    if (gameLogicObject == null) return;
	    _gameLogic = gameLogicObject.GetComponent<GameLogic>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    void OnCollisionEnter(Collision other) {
        if (!other.gameObject.CompareTag(GameLogic.PlayerTag)) return;
        Debug.Log(string.Format("Szach Mat {0}",other.gameObject.name));

        if (_gameLogic!=null) _gameLogic.GameOver();
    }
    void OnTriggerEnter2D(Collider2D other) {

        if (!other.CompareTag(GameLogic.PlayerTag)) return;
        Debug.Log(string.Format("Szach Mat {0}",other.name));

        if (_gameLogic!=null) _gameLogic.GameOver();
    }
}
