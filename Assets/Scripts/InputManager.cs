using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

public class InputManager : MonoBehaviour {
    public static InputManager Instance = null;
 	public KeyCode FightNormalKey {get; set;}
    public KeyCode FightSpecialKey {get; set;}
    public KeyCode ReloadWeaponKey {get; set;}
    public KeyCode SelectEnemyKey {get; set;}
    public KeyCode ExitKey {get; set;}
    public KeyCode OpenDetailKey {get; set;}
    public KeyCode LightKey {get; set;}
    public KeyConfig ControlType { get;  set; }

    // Use this for initialization
    void Awake () {
		  if (Instance == null) Instance = this;
   	  	   else if (Instance!=this) Destroy(gameObject);
          DontDestroyOnLoad(gameObject);
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
				FightNormalKey= KeyCode.Joystick1Button0;
				ReloadWeaponKey= KeyCode.Joystick1Button1;
				SelectEnemyKey=KeyCode.Joystick1Button4;
				ExitKey=KeyCode.Joystick1Button7;
				OpenDetailKey =KeyCode.Joystick1Button6;
				LightKey=KeyCode.Joystick1Button3;		
				break;			
			}
			case KeyConfig.GamePadOSX:
			{
				FightSpecialKey= KeyCode.Joystick1Button14;
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

    // Update is called once per frame
    void Update () {
		
	}
}
