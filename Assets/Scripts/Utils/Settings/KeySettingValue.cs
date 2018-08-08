using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeySettingValue : SettingValue {
	public KeyCode Value;

	public KeySettingValue(string name, KeyCode defaultValue) : base(name) {
		Value = defaultValue;
	}
	
	public override void Save() {
		PlayerPrefs.SetInt(Name, (int)Value);
	}

	public override void Load() {
		Value = (KeyCode)PlayerPrefs.GetInt(Name, (int)Value);
	}

	public bool IsDown() {
		return Input.GetKeyDown(Value);
	}
	
	public bool IsUp() {
		return Input.GetKeyUp(Value);
	}
	
	public bool IsPressed() {
		return Input.GetKey(Value);
	}
}
