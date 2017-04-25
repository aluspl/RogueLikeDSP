using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Characters.CharacterClasses;
using Characters;
using UnityEngine;
using UnityEngine.UI;

public class CharacterDetailView : MonoBehaviour
{

    public static CharacterDetailView Instance = null;

    public Text CharacterName;
    public Text StrengthValaue;
    public Text InteligenceValaue;
    public Text CharismaValaue;
    public Text AgilityValaue;
    public Text EnduranceValaue;
    public Text PerceptionValaue;

	// Use this for initialization
	void Awake ()
	{

	    SetupView();
	}

    private void SetupView()
    {
        enabled = true;

        if (CharacterName != null) CharacterName.text = GameManager.Instance.PlayerStatistic.Name;
        if (StrengthValaue != null) StrengthValaue.text = GameManager.Instance.PlayerStatistic.Strength.ToString();
        if (AgilityValaue != null) AgilityValaue.text = GameManager.Instance.PlayerStatistic.Agility.ToString();
        if (InteligenceValaue != null) InteligenceValaue.text = GameManager.Instance.PlayerStatistic.Inteligence.ToString();
        if (CharismaValaue != null) CharismaValaue.text = GameManager.Instance.PlayerStatistic.Charisma.ToString();
        if (PerceptionValaue != null) PerceptionValaue.text = GameManager.Instance.PlayerStatistic.Perception.ToString();
        if (EnduranceValaue != null) EnduranceValaue.text = GameManager.Instance.PlayerStatistic.Endurance.ToString();
    }


    public void OnDestroy()
    {
        GameManager.Instance.OpenedWindow = false;
    }

    public void CloseWindow()
    {
        Destroy(this.gameObject);
    }


    // Update is called once per frame
	void Update ()
	{
	    if (Input.GetKeyDown(GameManager.Instance.ExitKey)) CloseWindow();
	    if (Input.GetKeyDown(GameManager.Instance.OpenDetailKey)) CloseWindow();

	}


}
