using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LifeLike.Enums;
using LifeLike.Inferfaces;

namespace LifeLike
{

	public class InputManager : MonoBehaviour, IManager 
	{
		[InjectAttribute("Input")]
		public static InputManager Instance = null;
		public KeyCode FightNormalKey {get; set;}
		public KeyCode FightSpecialKey {get; set;}
		public KeyCode ReloadWeaponKey {get; set;}
		public KeyCode SelectEnemyKey {get; set;}
		public KeyCode ExitKey {get; set;}
		public KeyCode OpenDetailWindowKey {get; set;}
		public KeyCode OpenEquipmentWindowKey {get; set;}

		public KeyCode LightKey {get; set;}
		public KeyConfig ControlType { get;  set; }
		public KeyCode SelectSpecialAttackKey { get;  set; }

		// Use this for initialization
		void Awake () {
			if (Instance == null) Instance = this;
			//DontDestroyOnLoad(gameObject);
		//	DI.Inject(this);
			SetupControlType();
			SetupKeys();
		}

		private void SetupControlType()
		{
			var PadName=Input.GetJoystickNames();
			if (PadName.Length==0)
				ControlType=KeyConfig.Keyboard;
			else {
				Debug.Log(string.Format("Platform : {0} Pad Name : {1}",Application.platform, PadName[0]));
				ControlType=KeyConfig.GamePadOSX;

				if ( Application.platform==RuntimePlatform.OSXPlayer)
					ControlType=KeyConfig.GamePadOSX;
				if ( Application.platform==RuntimePlatform.WindowsPlayer || Application.platform==RuntimePlatform.LinuxPlayer)
					ControlType=KeyConfig.GamePad;
			}
			Debug.Log("Selected Input: "+ ControlType);

		}
		public void SetupControlType(KeyConfig config)
		{
			ControlType=config;
			SetupKeys();

		}
		private void SetupKeys()
		{
			switch (ControlType)
			{
				case KeyConfig.Keyboard:
				{
					FightNormalKey=KeyCode.Space;
					FightSpecialKey=KeyCode.E;
					SelectSpecialAttackKey = KeyCode.Q;
					ReloadWeaponKey=KeyCode.R;
					SelectEnemyKey=KeyCode.Tab;
					ExitKey=KeyCode.Escape;
					OpenDetailWindowKey = KeyCode.I;
					OpenEquipmentWindowKey=KeyCode.O;
					LightKey=KeyCode.F;
					break;
				}
				case KeyConfig.GamePad:
				{
					FightSpecialKey= KeyCode.Joystick1Button5;
					SelectSpecialAttackKey = KeyCode.Joystick1Button1;
					FightNormalKey= KeyCode.Joystick1Button0;
					ReloadWeaponKey= KeyCode.Joystick1Button3;
					SelectEnemyKey=KeyCode.Joystick1Button4;
					ExitKey=KeyCode.Joystick1Button7;
					OpenDetailWindowKey =KeyCode.Joystick1Button6;
					OpenEquipmentWindowKey =KeyCode.Joystick1Button8;

					LightKey=KeyCode.Joystick1Button2;

					break;
				}
				case KeyConfig.GamePadOSX:
				{
					FightSpecialKey= KeyCode.Joystick1Button14;
					SelectSpecialAttackKey = KeyCode.Joystick1Button17;

					FightNormalKey= KeyCode.Joystick1Button16;
					ReloadWeaponKey= KeyCode.Joystick1Button19;
					SelectEnemyKey=KeyCode.Joystick1Button13;
					ExitKey=KeyCode.Joystick1Button9;
					OpenDetailWindowKey =KeyCode.Joystick1Button10;
										OpenEquipmentWindowKey =KeyCode.Joystick1Button9;

					LightKey=KeyCode.Joystick1Button18;
					break;
				}
			}
		}

		void LateUpdate()
		{
			// var previousType = ControlType;
			// SetupControlType();
			// if (previousType==ControlType) return;
			// SetupKeys();
		}

        public void Destroy()
        {
			Destroy(this.gameObject);
			
        }
    }

}
