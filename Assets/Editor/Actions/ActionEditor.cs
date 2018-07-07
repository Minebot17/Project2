using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Наследуя от этого метода, можно создавать блоки действий под любыми скриптами
/// </summary>
public abstract class ActionEditor : Editor {
	private bool toggle;
	private bool[] toggles;

	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		try {
			toggle = EditorGUILayout.Foldout(toggle, "Actions");
			if (toggle) {
				ActionBlock[] blocks = GetBlocks();
				if (toggles == null)
					toggles = new bool[blocks.Length];
				for (int i = 0; i < blocks.Length; i++) {
					toggles[i] = EditorGUILayout.Foldout(toggles[i], blocks[i].name, true);
					if (toggles[i])
						blocks[i].draw.Invoke();
				}
			}
		}
		catch (IndexOutOfRangeException e) {
			Debug.Log("Update actions");
			ActionBlock[] blocks = GetBlocks();
			toggles = new bool[blocks.Length];
		}
	}

	protected abstract ActionBlock[] GetBlocks();
}
