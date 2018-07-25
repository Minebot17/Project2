using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SimpleObject), typeof(TypedObject))]
public class SerializeTypedObject : SerializeSimplyObject {

	public override List<string> Serialize() {
		List<string> result = base.Serialize();
		result.AddRange(new [] {
			GetComponent<TypedObject>().TypeIndex+""
		});
		return result;
	}

	public override void Deserialize(List<string> data) {
		base.Deserialize(data);
		
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
		
		data.RemoveAt(0);
	}
}
