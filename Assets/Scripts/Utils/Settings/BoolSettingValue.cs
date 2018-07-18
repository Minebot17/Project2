using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoolSettingValue : SettingValue {
	public bool Value;

	public BoolSettingValue(string name, bool defaultValue) : base(name) {
		Value = defaultValue;
	}
	
	public override void Save() {
		PlayerPrefs.SetInt(Name, Value ? 1 : 0);
	}

	public override void Load() {
		Value = PlayerPrefs.GetInt(Name, Value ? 1 : 0) == 1;
	}
}
