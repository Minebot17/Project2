using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(Stamina))]
public class StaminaActions : ActionEditor {
	private int value;
	
	protected override ActionBlock[] GetBlocks() {
		return new [] {
			new ActionBlock("Spend", () => {
				value = EditorGUILayout.IntField("Spend value: ", value);
				Stamina stamina = (Stamina)target;
				if(GUILayout.Button("Spend")) {
					stamina.Spend(value);
				}
			}), 
		};
	}
}
