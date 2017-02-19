using UnityEngine;

namespace MiniGame.TheBall
{
    public class TheBallMapLogic : MonoBehaviour
    {

        private TheBallGameLogic _theBallGameLogic;

        // Use this for initialization
        void Start ()
        {
//            var gameLogicObject = GameObject.FindWithTag(TheBallGameLogic.GameLogicTag);
//            if (gameLogicObject == null) return;
            _theBallGameLogic = FindObjectOfType<TheBallGameLogic>();
            Debug.Log(string.Format("TheBallGameLogic status {0}",_theBallGameLogic!=null));
        }
	

        void OnTriggerEnter2D(Collider2D other) {

            if (!other.CompareTag(TheBallGameLogic.PlayerTag)) return;
            Debug.Log(string.Format("Finish Point status {0}",other.name));

            if (_theBallGameLogic!=null) _theBallGameLogic.NextMap();
        }


    }
}
