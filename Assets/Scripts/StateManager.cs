using System;
using System.ComponentModel;
using LifeLike.Inferfaces;
using UnityEngine;

namespace LifeLike
{
    public class StateManager : MonoBehaviour
    {
        public static StateManager Instance = null;
        public GameObject GameManager;
        public GameObject InputManager;
        public GameObject PlayerManager;
        public GameObject UIManager;
        public GameObject EnemyManager;

        void Awake()
        {
            Instance=this;
            DontDestroyOnLoad(Instance);

            BuildContainer();
         //   Build();
        }

        private void BuildContainer()
        {
            if (LifeLike.InputManager.Instance == null)
                gameObject.AddComponent<InputManager>();
            if (LifeLike.UIManager.Instance == null)
               // GameContainer.AddComponent<UIManager>();
                Instantiate(UIManager, transform);

            if (LifeLike.PlayerManager.Instance == null)
                gameObject.AddComponent<PlayerManager>();
            if (LifeLike.EnemyManager.Instance == null)
                gameObject.AddComponent<EnemyManager>();
            if (LifeLike.GameManager.Instance == null)
//                GameContainer.AddComponent<GameManager>();
                  Instantiate(GameManager,transform);

        }        
        public void EndGame(){

        }

        private void Build()
        {
            if (LifeLike.InputManager.Instance == null)
                Instantiate(InputManager);
            if (LifeLike.UIManager.Instance == null)
                Instantiate(UIManager);
            if (LifeLike.PlayerManager.Instance == null)
                Instantiate(PlayerManager);
            if (LifeLike.EnemyManager.Instance == null)
                Instantiate(EnemyManager);
            if (LifeLike.GameManager.Instance == null)
                Instantiate(GameManager);
        }
    }
}