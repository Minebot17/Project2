using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatSettingValue : SettingValue {
	public float Value;
	
	public FloatSettingValue(string name, float defaultValue) : base(name) {
		Value = defaultValue;
	}

	public override void Save() {
		PlayerPrefs.SetFloat(Name, Value);
	}

	public override void Load() {
		Value = PlayerPrefs.GetFloat(Name, Value);
	}
}
