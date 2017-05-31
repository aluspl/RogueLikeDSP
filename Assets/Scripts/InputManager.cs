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
		public KeyCode OpenDetailKey {get; set;}
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
					OpenDetailKey = KeyCode.I;
					LightKey=KeyCode.F;
					break;
				}
				case KeyConfig.GamePad:
				{
					FightSpecialKey= KeyCode.Joystick1Button5;
					SelectSpecialAttackKey = KeyCode.Joystick1Button1;
					FightNormalKey= KeyCode.Joystick1Button0;
					ReloadWeaponKey= KeyCode.Joystick1Button2;
					SelectEnemyKey=KeyCode.Joystick1Button4;
					ExitKey=KeyCode.Joystick1Button7;
					OpenDetailKey =KeyCode.Joystick1Button6;
					LightKey=KeyCode.Joystick1Button2;

					break;
				}
				case KeyConfig.GamePadOSX:
				{
					FightSpecialKey= KeyCode.Joystick1Button14;
					SelectSpecialAttackKey = KeyCode.Joystick1Button17;

					FightNormalKey= KeyCode.Joystick1Button16;
					ReloadWeaponKey= KeyCode.Joystick1Button17;
					SelectEnemyKey=KeyCode.Joystick1Button13;
					ExitKey=KeyCode.Joystick1Button9;
					OpenDetailKey =KeyCode.Joystick1Button10;
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
