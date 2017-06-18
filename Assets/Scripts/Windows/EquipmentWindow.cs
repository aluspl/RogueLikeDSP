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
	public class EquipmentWindow : Window
	{

	
        public List<IEquipment> _weapons { get; set; }

        // Use this for initialization
      
		 
		public override void SetupView()
		{
			enabled = true;
		
		}

        private void OnWeaponSelect(int value)
        {
			if (_weapons.Count>=value)
			{
				PlayerManager.Instance.Statistic.SelectedWeapon=_weapons[value] as IWeapon;
			}
        }

        public void OnDestroy()
		{
            WindowManager.Instance.Status = WindowState.Close;;
		}


		// Update is called once per frame
		void Update()
        {
            KeyBinding();

        }

        private void KeyBinding()
        {
            if (Input.GetKeyDown(InputManager.Instance.ExitKey)) CloseWindow();
            if (Input.GetKeyDown(InputManager.Instance.OpenDetailKey)) CloseWindow();
        }
    }
}