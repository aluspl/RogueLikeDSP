using UnityEngine;

namespace MiniGame.TheBall
{
    public class TheBallEnemyLogic : MonoBehaviour {

        private TheBallGameLogic _theBallGameLogic;

        // Use this for initialization
        void Start () {
            var gameLogicObject = GameObject.FindWithTag(TheBallGameLogic.GameLogicTag);
            if (gameLogicObject == null) return;
            _theBallGameLogic = gameLogicObject.GetComponent<TheBallGameLogic>();
        }

        // Update is called once per frame
        void Update () {

        }
        void OnCollisionEnter(Collision other) {
            if (!other.gameObject.CompareTag(TheBallGameLogic.PlayerTag)) return;
            Debug.Log(string.Format("Szach Mat {0}",other.gameObject.name));

            if (_theBallGameLogic!=null) _theBallGameLogic.GameOver();
        }
        void OnTriggerEnter2D(Collider2D other) {

            if (!other.CompareTag(TheBallGameLogic.PlayerTag)) return;
            Debug.Log(string.Format("Szach Mat {0}",other.name));

            if (_theBallGameLogic!=null) _theBallGameLogic.GameOver();
        }
    }
}
