using UnityEngine;

namespace LifeLike
{
    public class InitateGame : MonoBehaviour
    {

        public GameObject GameManager;
        public GameObject InputManager;

        void Awake()
        {
            if (LifeLike.GameManager.Instance == null)
                Instantiate(GameManager);
            if (LifeLike.InputManager.Instance == null)
                Instantiate(InputManager);
        }
    }
}