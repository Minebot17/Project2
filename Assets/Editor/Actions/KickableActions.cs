using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(Kickable))]
public class KickableEditor : ActionEditor {
	private Vector2 vector;

	protected override ActionBlock[] GetBlocks() {
		return new [] {
			new ActionBlock("Kick", () => {
				vector = EditorGUILayout.Vector2Field("Value", vector);
				Kickable tool = (Kickable) target;
				if (GUILayout.Button("Kick")) {
					tool.Kick(vector);
				}
			}), 
		};
	}
}
