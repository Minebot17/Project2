using System;
using UnityEditor;
using UnityEngine;

public class TypesSetupBlock : IUtilBlock {
	private GameObject target;
	private Vector2 size;
	
	public void Draw() {
		target = (GameObject) EditorGUILayout.ObjectField("Target", target, typeof(GameObject), true);
		size = EditorGUILayout.Vector2IntField("Texture size", new Vector2Int((int)size.x, (int)size.y));
		if(GUILayout.Button("Start")) {
			Container container = JsonUtility.FromJson<Container>(Resources.Load<TextAsset>(target.name).text);
			TypedObject component = target.GetComponent<TypedObject>();
			TypedObject.Type[] types = new TypedObject.Type[container.types.Length];
			for (int i = 0; i < types.Length; i++) {
				Object type = container.types[i];
				types[i] = new TypedObject.Type(
					new Vector2(type.width, type.height),
					new Vector2(type.offset.x/size.x, (size.y - type.offset.y - type.height)/size.y), 
					new Vector2((type.offset.x + type.width)/size.x, (size.y - type.offset.y)/size.y)
				);
			}

			component.Types = types;
		}
	}

	public string GetName() {
		return "Types Setup Tool";
	}
	
	[Serializable]
	public class Container {
		public string texture;
		public Object[] types;
	}

	[Serializable]
	public class Object {
		public int width;
		public int height;
		public RoomSerializeHelper.Veci offset;
	}
}
