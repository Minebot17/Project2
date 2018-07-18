using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StringSettingValue : SettingValue {
	public string Value;

	public StringSettingValue(string name, string defaultValue) : base(name) {
		Value = defaultValue;
	}
	
	public override void Save() {
		PlayerPrefs.SetString(Name, Value);
	}

	public override void Load() {
		Value = PlayerPrefs.GetString(Name, Value);
	}
}
