using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SettingValue {
	public string Name;

	public SettingValue(string name) {
		Name = name;
		GameSettings.values.Add(this);
	}
	
	public abstract void Save();
	public abstract void Load();
}
