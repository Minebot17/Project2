using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(LanguageText))]
public class LangTextEditor : Editor {

	public override void OnInspectorGUI() {
		DrawDefaultInspector();
	}
}
