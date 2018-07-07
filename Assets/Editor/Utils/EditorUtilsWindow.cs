using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EditorUtilsWindow : EditorWindow {
	private static readonly IUtilBlock[] utilBlocks = {
		new TypesSetupBlock(),
		new GenerateMeshBlock(), 
		new PlayerSetupBlock(), 
		new FixAnimationEventsBlock(), 
	};
	private bool[] togglers = new bool[utilBlocks.Length];

	[MenuItem ("Window/EditorUtils")]
	public static void  ShowWindow () {
		GetWindow(typeof(EditorUtilsWindow));
	}
    
	void OnGUI () {
		try {
			for (int i = 0; i < utilBlocks.Length; i++) {
				// ReSharper disable once AssignmentInConditionalExpression
				if (togglers[i] = EditorGUILayout.Foldout(togglers[i], utilBlocks[i].GetName()))
					utilBlocks[i].Draw();
				EditorGUILayout.Space();
			}
		}
		catch (IndexOutOfRangeException e) {
			togglers = new bool[utilBlocks.Length];
		}
	}
}
