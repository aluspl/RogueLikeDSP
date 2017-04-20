using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MiniGame.TheBall
{
    public class TheBallGameLogic : MonoBehaviour
    {
        //GAME OBJECTS
        public GameObject Player;
        public GameObject FinishMap;

//UI
        public Text MapCounterText;
        public Text MapTimeText;
        public Text MapTotalText;

//Scripts
        public TheBallMapGenerator Generator;

        private int _map=1;

        public const string FinishPointTag="FinishPoint";
        public const string GameLogicTag="GameLogic";
        public const string PlayerTag="PlayerStatistic";
        public DateTime CurrentTime = DateTime.Now;
        private TimeSpan _totalTime;

        // Use this for initialization
        void Start ()
        {
            if (Generator == null) Generator = FindObjectOfType<TheBallMapGenerator>();
            //_mapLogic=FinishMap.GetComponent<TheBallMapLogic>();
            NewGame();

        }

        public void NewGame()
        {
            Debug.Log(string.Format("Map: {0}, Next Map Event",_map));

            CurrentTime = DateTime.Now;
            _totalTime = CurrentTime - CurrentTime;
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
            _totalTime += DateTime.Now - CurrentTime;
            CurrentTime = DateTime.Now;
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
            var seconds = (DateTime.Now - CurrentTime).Seconds;

            MapTimeText.text = string.Format("Time: {0}s",seconds);

        }

        public void GameOver()
        {
            SceneManager.LoadScene (0);
        }
    }
}
