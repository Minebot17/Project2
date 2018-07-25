using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SimpleObject))]
public class SerializeSimplyObject : MonoBehaviour, ISerializableObject {

	public void Initialize() {
		
	}

	public List<string> Serialize() {
		List<string> result = new List<string>() {
			GetComponent<SimpleObject>().Id+"",
			transform.position.x+"",
			transform.position.y+"",
			transform.position.z+""
		};
		return result;
	}

	public void Deserialize(List<string> data) {
		GetComponent<SimpleObject>().Id = int.Parse(data[0]);
		transform.position = new Vector3(float.Parse(data[1]), float.Parse(data[2]), float.Parse(data[3]));
		for (int i = 0; i < 4; i++)
			data.RemoveAt(0);
	}
}
