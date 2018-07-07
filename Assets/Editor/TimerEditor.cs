using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(Timer.TimerSkin))]
public class TimerEditor : Editor {
	
	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		Timer.TimerSkin skin = (Timer.TimerSkin)target;
		if (skin.GetTimer() is Timer.StandartTimer) {
			Timer.StandartTimer timer = (Timer.StandartTimer) skin.GetTimer();
			EditorGUILayout.LabelField("Name: " + timer.timerName);
			EditorGUILayout.LabelField("Lasted repeats: " + timer.countOfRepeat);
			EditorGUILayout.LabelField("Current time: " + timer.n);
			EditorGUILayout.LabelField("Total time: " + timer.duration);
			Repaint();
		}
	}
}
