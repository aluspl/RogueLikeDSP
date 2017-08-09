using System;
using System.ComponentModel;
using LifeLike.Inferfaces;
using UnityEngine;

namespace LifeLike
{
    public class GameManager : MonoBehaviour 
    {
        public static GameManager Instance = null;
        public GameObject GameLogicManager;
        // public GameObject InputManager;
        // public GameObject PlayerManager;
        public GameObject UIManager;
        // public GameObject EnemyManager;
        public GameObject WindowManager;


        void Awake()
        {
            Instance=this;
            DontDestroyOnLoad(Instance);

            BuildContainer();
         //   Build();
        }

        public void BuildContainer()
        {
            
            if (LifeLike.InputManager.Instance == null)
                gameObject.AddComponent<InputManager>();
            if (LifeLike.PlayerManager.Instance == null)
                gameObject.AddComponent<PlayerManager>();
                      
            if (LifeLike.WindowManager.Instance==null)
                Instantiate(WindowManager, transform);
          
            if (LifeLike.EnemyManager.Instance == null)
                gameObject.AddComponent<EnemyManager>();
            
            if (LifeLike.UIManager.Instance == null)
                Instantiate(UIManager, transform);
           
            if (LifeLike.GameLogicManager.Instance == null)
                Instantiate(GameLogicManager,transform);

        }        
        public bool AreAllModulesWork(out string ErrorModule)
        {
            if (LifeLike.InputManager.Instance==null) 
            {
                ErrorModule="InputManager";
                return false;
            }
            if (LifeLike.PlayerManager.Instance==null)             
            {
                ErrorModule="PlayerManager";
                return false;
            }

            if (LifeLike.EnemyManager.Instance==null)             
            {
                ErrorModule="EnemyManager";
                return false;
            }


            if (LifeLike.UIManager.Instance==null)             
            {
                ErrorModule="UIManager";
                return false;
            }

            if (LifeLike.WindowManager.Instance==null)             
            {
                ErrorModule="WindowManager";
                return false;
            }

            if (LifeLike.GameLogicManager.Instance==null)             
            {
                ErrorModule="GameLogicManager";
                return false;
            }

            ErrorModule=string.Empty;
            return true;
        }
        public void EndGame(){
            LifeLike.UIManager.Instance.Destroy();
            LifeLike.PlayerManager.Instance.Destroy();
            LifeLike.EnemyManager.Instance.Destroy();

            LifeLike.WindowManager.Instance.Destroy();
            
            LifeLike.GameLogicManager.Instance.Destroy();

        }

       
    }
}