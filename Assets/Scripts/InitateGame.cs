using UnityEngine;

public class InitateGame : MonoBehaviour
{

    public GameObject GameManager;
    // Use this for initialization
    void Awake ()
    {
        if (global::GameManager.Instance == null)
            Instantiate(GameManager);
//        if (global::UIManager.Instance == null)
//            Instantiate(UiUtils);
    }

    // Update is called once per frame
    void Update () {
		
    }
}