using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class PlayerSetupBlock : IUtilBlock {
	private GameObject oldPlayer;
	private GameObject newPlayer;
	private bool setupComponents;
	private bool setupObjects;

	private Behaviour[] oldComponets;
	private bool[] oldComponetsToggles;
	private bool foldComponents;
	
	private GameObject[] oldObjects;
	private bool[] oldObjectsToggles;
	private bool foldObjects;
	
	public void Draw() {
		oldPlayer = (GameObject) EditorGUILayout.ObjectField("Old Player", oldPlayer, typeof(GameObject), true);
		newPlayer = (GameObject) EditorGUILayout.ObjectField("New Player", newPlayer, typeof(GameObject), true);
		if (GUILayout.Button("Set players")) {
			oldComponets = oldPlayer.GetComponents<Behaviour>();
			oldComponetsToggles = new bool[oldComponets.Length];
			for (int i = 0; i < oldComponets.Length; i++)
				oldComponetsToggles[i] = true;
			
			oldObjects = new GameObject[oldPlayer.transform.childCount];
			for (int i = 0; i < oldObjects.Length; i++)
				oldObjects[i] = oldPlayer.transform.GetChild(i).gameObject;
			oldObjectsToggles = new bool[oldObjects.Length];
			for (int i = 0; i < oldObjects.Length; i++)
				oldObjectsToggles[i] = true;
		}

		setupComponents = GUILayout.Toggle(setupComponents, "Is setup components");
		if (oldComponets != null && (foldComponents = EditorGUILayout.Foldout(foldComponents, "Componets to setup"))) {
			for (int i = 0; i < oldComponets.Length; i++) {
				oldComponetsToggles[i] = GUILayout.Toggle(oldComponetsToggles[i], oldComponets[i].GetType().ToString());
			}
		}

		setupObjects = GUILayout.Toggle(setupObjects, "Is setup objests");
		if (oldObjects != null && (foldObjects = EditorGUILayout.Foldout(foldObjects, "Objects to setup"))) {
			for (int i = 0; i < oldObjects.Length; i++) {
				oldObjectsToggles[i] = GUILayout.Toggle(oldObjectsToggles[i], oldObjects[i].name);
			}
		}

		if (GUILayout.Button("Setup")) {
			if (setupComponents) {
				for (int i = 0; i < oldComponets.Length; i++)
					if (oldComponetsToggles[i])
						CopyComponent(oldComponets[i], newPlayer);
			}

			if (setupObjects) {
				for (int i = 0; i < oldObjects.Length; i++)
					if (oldObjectsToggles[i]) {
						Vector3 localPos = oldObjects[i].transform.localPosition;
						oldObjects[i].transform.parent = newPlayer.transform;
						oldObjects[i].transform.localPosition = localPos;
					}
			}
		}
	}
	
	private T CopyComponent<T>(T original, GameObject destination) where T : Component {
		System.Type type = original.GetType();
		if (destination.GetComponent(type) != null)
			MonoBehaviour.DestroyImmediate(destination.GetComponent(type));
		Component copy = destination.AddComponent(type);
		FieldInfo[] fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
		foreach (System.Reflection.FieldInfo field in fields) {
			if (!field.IsLiteral) {
				field.SetValue(copy, field.GetValue(original));
			}
		}
		return copy as T;
	}

	public string GetName() {
		return "Setup Player";
	}
}
