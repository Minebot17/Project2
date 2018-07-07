using System;
using UnityEditor;
using UnityEngine;

public class GenerateMeshBlock : IUtilBlock {

	private String name;
	private int type;
	private bool setTypeToName;
	private Vector2 size;
	
	public void Draw() {
		name = EditorGUILayout.TextField("Path", name);
		type = EditorGUILayout.IntField("Needed type", type);
		setTypeToName = GUILayout.Toggle(setTypeToName, "Are set type to name?");
		size = EditorGUILayout.Vector2IntField("Atlas size", new Vector2Int((int)size.x, (int)size.y));
		if (GUILayout.Button("Start")) {
			TypesSetupBlock.Container container = JsonUtility.FromJson<TypesSetupBlock.Container>(Resources.Load<TextAsset>(name).text);
			TypesSetupBlock.Object type = container.types[this.type];
			Mesh mesh = new Mesh {
				vertices = new[] {
					new Vector3(0, 0),
					new Vector3(type.width, 0),
					new Vector3(type.width, type.height),
					new Vector3(0, type.height)
				},
				normals = new[] {
					Vector3.back,
					Vector3.back,
					Vector3.back,
					Vector3.back
				},
				triangles = new[] {0, 2, 1, 0, 3, 2},
				uv = new[] {
					new Vector2(type.offset.x / size.x, (size.y - type.offset.y - type.height) / size.y),
					new Vector2((type.offset.x + type.width) / size.x, (size.y - type.offset.y - type.height) / size.y),
					new Vector2((type.offset.x + type.width) / size.x, (size.y - type.offset.y) / size.y),
					new Vector2(type.offset.x / size.x, (size.y - type.offset.y) / size.y)
				}
			};
			AssetDatabase.CreateAsset(mesh, "Assets/Meshes/Generated/" + name + (setTypeToName ? "_" + this.type : "") + ".asset");
			AssetDatabase.SaveAssets();
		}
	}

	public string GetName() {
		return "Generate Mesh";
	}
}
