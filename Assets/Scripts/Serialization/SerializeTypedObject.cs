using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SimpleObject), typeof(TypedObject))]
public class SerializeTypedObject : MonoBehaviour, ISerializableObject {
	
	public void Initialize() {
		
	}

	public List<string> Serialize() {
		return new List<string>(){GetComponent<TypedObject>().TypeIndex+""};
	}

	public int Deserialize(List<string> data) {
		GetComponent<TypedObject>().TypeIndex = int.Parse(data[0]);
		TypedObject.Type type = GetComponent<TypedObject>().Types[GetComponent<TypedObject>().TypeIndex];
		transform.localScale = new Vector3(type.Size.x, type.Size.y, transform.localScale.z);
		for (int i = 0; i < transform.childCount; i++)
			transform.GetChild(i).position = transform.position + transform.GetChild(i).localPosition;
			
		Mesh mesh = Instantiate(GameManager.singleton.OnePlane);
		mesh.SetUVs(0, new List<Vector2>() {
			new Vector2(type.MinUV.x, type.MinUV.y), 
			new Vector2(type.MaxUV.x, type.MinUV.y),
			new Vector2(type.MaxUV.x, type.MaxUV.y),
			new Vector2(type.MinUV.x, type.MaxUV.y)
		});
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
		mesh.RecalculateTangents();
		GetComponent<MeshFilter>().mesh = mesh;

		return 1;
	}
}
