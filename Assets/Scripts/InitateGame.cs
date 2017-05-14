using UnityEngine;

public class InitateGame : MonoBehaviour
{

    public GameObject GameManager;
    public GameObject InputManager;

    void Awake ()
    {
        if (global::GameManager.Instance == null)
            Instantiate(GameManager);
         if (global::InputManager.Instance == null)
             Instantiate(InputManager);
    } 
}