using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings {

	public static readonly List<SettingValue> values = new List<SettingValue>();

	public static readonly BoolSettingValue SettingVisualizeMeshGeneration = new BoolSettingValue("SettingVisualizeMeshGeneration", false);
	public static readonly BoolSettingValue SettingVisualizeColliders = new BoolSettingValue("SettingVisualizeColliders", false);
	public static readonly BoolSettingValue SettingVisualizeTraectorySimple = new BoolSettingValue("SettingVisualizeTraectorySimple", false);
	public static readonly BoolSettingValue SettingVisualizeTraectoryAdvanced = new BoolSettingValue("SettingVisualizeTraectoryAdvanced", false);
	public static readonly BoolSettingValue SettingVisualizeTestGeneration = new BoolSettingValue("SettingVisualizeTestGeneration", true);
	public static readonly BoolSettingValue SettingLogEvents = new BoolSettingValue("SettingLogEvents", false);
	public static readonly IntSettingValue SettingMinGenerationCellCount = new IntSettingValue("MinGenerationCellCount", 100);
	public static readonly FloatSettingValue SettingTraectoryTracingFrequency = new FloatSettingValue("TraectoryTracingFrequency", 10);
	public static readonly StringSettingValue SettingLanguageCode = new StringSettingValue("LanguageCode", "en");
	public static readonly StringSettingValue SettingTestRoomName = new StringSettingValue("TestRoomName", "test");
	public static readonly KeySettingValue SettingOpenInventoryKey = new KeySettingValue("SettingOpenInventoryKey", KeyCode.I);
	public static readonly KeySettingValue SettingUseItemKey = new KeySettingValue("SettingUseItemKey", KeyCode.E);

	public static void Save() {
		values.ForEach(val => val.Save());
	}

	public static void Load() {
		values.ForEach(val => val.Load());
	}
}
