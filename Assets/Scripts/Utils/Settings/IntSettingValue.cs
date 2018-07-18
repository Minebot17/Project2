using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntSettingValue : SettingValue {
	public int Value;

	public IntSettingValue(string name, int defaultValue) : base(name) {
		Value = defaultValue;
	}
	
	public override void Save() {
		PlayerPrefs.SetInt(Name, Value);
	}

	public override void Load() {
		Value = PlayerPrefs.GetInt(Name, Value);
	}
}
