using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LifeLike.Characters;
using LifeLike.Characters.CharacterClasses;
using LifeLike.Enums;
using LifeLike.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace LifeLike
{
    public class CreateWindow : Window
    {
        private const int PointsToSpend = 6;
        public Slider StrengthSlider;
        public Slider InteligenceSlider;
        public Slider AgilitySlider;

        public Text StrengthValue;
        public Text InteligenceValue;
        public Text AgilityValue;
        public Text ClassDescription;

        public Dropdown ClassListDropdown;
        public InputField CharacterName;
        public Character Character;
        public Text CharacterLeftPoint;
        public int CharacterLeftPointValue = PointsToSpend;

        private Dictionary<string, string> _characterClasses;
        public string SelectedClass { get; set; }

        public Button SaveButton;

        public CharacterStats Statistic = new CharacterStats()
        {
            Strength = 2,
            Agility = 2,
            Inteligence = 2,
        };


        public override void SetupView()
        {            
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
            if (AgilityValue != null) AgilityValue.text = Statistic.Agility.ToString();
            if (StrengthValue != null) StrengthValue.text = Statistic.Strength.ToString();
            if (InteligenceValue != null) InteligenceValue.text = Statistic.Inteligence.ToString();

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
            return (CharacterLeftPointValue + PointsToSpend) - (Statistic.Strength +
                                                    Statistic.Agility +
                                                    // Statistic.Charisma +
                                                    // Statistic.Endurance +
                                                    Statistic.Inteligence);  //+
                                                    // Statistic.Perception);


        }
     
        public void OnSaveClick()
        {
            if (string.IsNullOrEmpty(Statistic.Name) || SelectedClass == null) return;
            if (CalculatePointLeft() < 0) return;
            if (PlayerManager.Instance==null)
            {
                GameStatic.Statistic = CharacterFactory.GetPlayerClass(SelectedClass, Statistic);
                SceneManager.LoadScene ("LifeLike");
            }
            else
            {
                PlayerManager.Instance.Statistic=CharacterFactory.GetPlayerClass(SelectedClass, Statistic);
                        //  SceneManager.LoadScene ("LifeLike");
                CloseWindow();
            }
        }


        // Update is called once per frame
        void FixedUpdate()
        {
            if (CharacterLeftPoint != null)
            {
                CharacterLeftPoint.text = CalculatePointLeft().ToString();
                CharacterLeftPoint.color = CalculatePointLeft() >= 0 ? Color.white : Color.red;
                SaveButton.enabled = CalculatePointLeft() >= 0;

            }
        }

    }
}
