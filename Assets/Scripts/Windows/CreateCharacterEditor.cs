using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LifeLike.Characters;
using LifeLike.Characters.CharacterClasses;
using UnityEngine;
using UnityEngine.UI;
namespace LifeLike
{
    public class CreateCharacterEditor : MonoBehaviour
    {

        public Slider StrengthSlider;
        public Slider InteligenceSlider;
        public Slider CharismaSlider;
        public Slider AgilitySlider;
        public Slider EnduranceSlider;
        public Slider PerceptionSlider;

        public Text StrengthValue;
        public Text InteligenceValue;
        public Text CharismaValue;
        public Text AgilityValue;
        public Text EnduranceValue;
        public Text PerceptionValue;

        private Canvas _characterUiCanvas;
        public Dropdown ClassListDropdown;
        public InputField CharacterName;
        public Character Character;
        public Text CharacterLeftPoint;
        public int CharacterLeftPointValue = 10;
        private Dictionary<string, string> _characterClasses;
        public string SelectedClass { get; set; }

        public Button SaveButton;

        public CharacterStatisticDataModel Statistic = new CharacterStatisticDataModel()
        {
            Strength = 1,
            Agility = 1,
            Charisma = 1,
            Inteligence = 1,
            Endurance = 1,
            Perception = 1
        };


        // Use this for initialization
        void Start()
        {
            SetupView();
        }

        private void SetupView()
        {
            _characterUiCanvas = GetComponentInChildren<Canvas>();
            if (SaveButton != null) SaveButton.onClick.AddListener(OnSaveClick);
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

                ClassListDropdown.options = _characterClasses.Select(p => new Dropdown.OptionData(name = p.Value))
                    .ToList();
                ;
                ClassListDropdown.onValueChanged.AddListener(OnClassSelect);
                OnClassSelect(ClassListDropdown.value);
            }
            if (StrengthSlider != null) StrengthSlider.onValueChanged.AddListener(StrengthChanged);
            if (AgilitySlider != null) AgilitySlider.onValueChanged.AddListener(AgilityChanged);
            if (InteligenceSlider != null) InteligenceSlider.onValueChanged.AddListener(InteligenceChanged);
            if (EnduranceSlider != null) EnduranceSlider.onValueChanged.AddListener(EnduranceChanged);
            if (PerceptionSlider != null) PerceptionSlider.onValueChanged.AddListener(PerceptionChanged);
            if (CharismaSlider != null) CharismaSlider.onValueChanged.AddListener(CharismaChanged);
            if (CharismaValue != null) CharismaValue.text = Statistic.Charisma.ToString();
            if (PerceptionValue != null) PerceptionValue.text = Statistic.Perception.ToString();
            if (EnduranceValue != null) EnduranceValue.text = Statistic.Endurance.ToString();
            if (AgilityValue != null) AgilityValue.text = Statistic.Agility.ToString();
            if (StrengthValue != null) StrengthValue.text = Statistic.Strength.ToString();
            if (InteligenceValue != null) InteligenceValue.text = Statistic.Inteligence.ToString();

        }

        private void CharismaChanged(float value)
        {
            Statistic.Charisma = (int) value;
            CharismaValue.text = Statistic.Charisma.ToString();
          //  Debug.Log("Charisma: " + value);
        }

        private void PerceptionChanged(float value)
        {
            Statistic.Perception = (int) value;
            PerceptionValue.text = Statistic.Perception.ToString();
            //Debug.Log("Perception: " + value);
        }

        private void EnduranceChanged(float value)
        {
            Statistic.Endurance = (int) value;
            EnduranceValue.text = Statistic.Endurance.ToString();

          //  Debug.Log("Endurance: " + value);
        }

        private void InteligenceChanged(float value)
        {
            Statistic.Inteligence = (int) value;
            InteligenceValue.text = Statistic.Inteligence.ToString();
    //        Debug.Log("Inteligence: " + value);
        }

        private void AgilityChanged(float value)
        {
            Statistic.Agility = (int) value;
            AgilityValue.text = Statistic.Agility.ToString();
      //      Debug.Log("Agility: " + value);
        }

        private void StrengthChanged(float value)
        {
            Statistic.Strength = (int) value;
            StrengthValue.text = Statistic.Strength.ToString();
        //    Debug.Log("Strength: " + value);
        }



        private void EditName(string value)
        {
            Statistic.Name = value;
            Debug.Log("Name: " + Statistic.Name);
        }



        private void OnClassSelect(int value)
        {
            SelectedClass = _characterClasses.ToList()[value].Key;
            Debug.Log(SelectedClass);
        }

        int CalculatePointLeft()
        {
            return (CharacterLeftPointValue + 6) - (Statistic.Strength +
                                                    Statistic.Agility +
                                                    Statistic.Charisma +
                                                    Statistic.Endurance +
                                                    Statistic.Inteligence +
                                                    Statistic.Perception);


        }


        public void OnDestroy()
        {
            GameManager.Instance.OpenedWindow = false;
            _characterUiCanvas.enabled = false;
        }

        public void OnSaveClick()
        {
            if (string.IsNullOrEmpty(Statistic.Name) || SelectedClass == null) return;
            if (CalculatePointLeft() < 0) return;

            PlayerManager.Instance.Statistic = CharacterFactory.GetPlayerClass(SelectedClass, Statistic);
            Destroy(this.gameObject);
        }


        // Update is called once per frame
        void Update()
        {
            if (CharacterLeftPoint != null)
            {
                CharacterLeftPoint.text = CalculatePointLeft().ToString();
                CharacterLeftPoint.color = CalculatePointLeft() >= 0 ? Color.black : Color.red;
                SaveButton.enabled = CalculatePointLeft() >= 0;

            }
        }

    }
}
