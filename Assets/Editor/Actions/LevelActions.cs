using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(Level))]
public class LevelActions : ActionEditor {

	protected override ActionBlock[] GetBlocks() {
		return new [] {
			new ActionBlock("Level up", () => {
				Level level = (Level)target;
				if(GUILayout.Button("Level up!")) {
					level.LevelUp();
				}
			}), 
		};
	}
}
