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
}
