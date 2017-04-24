using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Characters.CharacterClasses;
using Characters;
using UnityEngine;
using UnityEngine.UI;

public class CreateCharacterEditor : MonoBehaviour
{


    private Canvas _characterUiCanvas;
    public Dropdown ClassListDropdown;
    public InputField CharacterName;
    public BaseCharacter Character;
    public Text CharacterLeftPoint;
    public int CharacterLeftPointValue = 10;
    private Dictionary<string, string> _characterClasses;
    public string CharacterNameText;
    public CharacterStatisticDataModel Statistic;
    public string SelectedClass { get; set; }

    public Button SaveButton;

	// Use this for initialization
	void Start ()
	{
	    SetupView();
	}

    private void SetupView()
    {
        GameManager.Instance.GameUI.enabled = false;
        _characterUiCanvas = GetComponentInChildren<Canvas>();
        if (SaveButton!=null) SaveButton.onClick.AddListener(OnSaveClick);
        if (CharacterLeftPoint != null)
        {
            CharacterLeftPoint.text = CharacterLeftPointValue.ToString();
        }
        if (CharacterName != null)
        {
            CharacterName.onEndEdit.AddListener(EditName);
        }
        if (ClassListDropdown != null)
        {
            _characterClasses = CharacterFactory.PlayerClassList();

            ClassListDropdown.options = _characterClasses.Select(p => new Dropdown.OptionData(name=p.Value)).ToList();;
            ClassListDropdown.onValueChanged.AddListener(OnClassSelect);
            OnClassSelect(ClassListDropdown.value);
        }

    }

    private void EditName(string value)
    {
        CharacterNameText = value;
    }



    private void OnClassSelect(int value)
    {
        SelectedClass = _characterClasses.ToList()[value].Key;
        Debug.Log(SelectedClass);
    }



    public void OnDestroy()
    {
        if (GameManager.Instance.GameUI!=null) GameManager.Instance.GameUI.enabled = true;
        _characterUiCanvas.enabled = false;
    }

    public void OnSaveClick()
    {
        if (!string.IsNullOrEmpty(CharacterNameText) && SelectedClass != null)
        {
            GameManager.Instance.PlayerStatistic = CharacterFactory.GetPlayerClass(SelectedClass, Statistic);
            Debug.Log("Ohh no!");
            Destroy(this);
        }
    }


    // Update is called once per frame
	void Update () {
		
	}


}
