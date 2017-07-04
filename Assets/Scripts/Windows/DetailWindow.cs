using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LifeLike.Characters;
using LifeLike.Enums;
using LifeLike.Enums.Equipment;
using LifeLike.Inferfaces;
using LifeLike.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace LifeLike
{
	public class DetailWindow : Window
	{
		public Text CharacterName;
		public Text StrengthValaue;
		public Text InteligenceValaue;
		public Text CharismaValaue;
		public Text AgilityValaue;
		public Text EnduranceValaue;
		public Text PerceptionValaue;


       
		public override void SetupView()
		{
			enabled = true;

			if (CharacterName != null) CharacterName.text = PlayerManager.Instance.Statistic.Name;
			if (StrengthValaue != null) StrengthValaue.text = PlayerManager.Instance.Statistic.Strength.ToString();
			if (AgilityValaue != null) AgilityValaue.text = PlayerManager.Instance.Statistic.Agility.ToString();
			if (InteligenceValaue != null) InteligenceValaue.text = PlayerManager.Instance.Statistic.Inteligence.ToString();
			if (CharismaValaue != null) CharismaValaue.text = PlayerManager.Instance.Statistic.Charisma.ToString();
			if (PerceptionValaue != null) PerceptionValaue.text = PlayerManager.Instance.Statistic.Perception.ToString();
			if (EnduranceValaue != null) EnduranceValaue.text = PlayerManager.Instance.Statistic.Endurance.ToString();
		
		}

     

        public void OnDestroy()
		{
            WindowManager.Instance.Status = WindowState.Close;;
		}

	


		// Update is called once per frame
		void Update()
		{
			if (Input.GetKeyDown(InputManager.Instance.ExitKey)) CloseWindow();
			if (Input.GetKeyDown(InputManager.Instance.OpenDetailWindowKey)) CloseWindow();

		}

	}
}