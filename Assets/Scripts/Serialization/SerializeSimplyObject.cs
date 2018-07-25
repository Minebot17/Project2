using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SimpleObject))]
public class SerializeSimplyObject : MonoBehaviour, ISerializableObject {

	public virtual void Initialize() {
		
	}

	public virtual List<string> Serialize() {
		List<string> result = new List<string>() {
			GetComponent<SimpleObject>().Id+"",
			transform.position.x+"",
			transform.position.y+"",
			transform.position.z+"",
			transform.localEulerAngles.x+"",
			transform.localEulerAngles.y+"",
			transform.localEulerAngles.z+"",
			transform.localScale.x+"",
			transform.localScale.y+"",
			transform.localScale.z+""
		};
		return result;
	}

	public virtual void Deserialize(List<string> data) {
		GetComponent<SimpleObject>().Id = int.Parse(data[0]);
		transform.position = new Vector3(float.Parse(data[1]), float.Parse(data[2]), float.Parse(data[3]));
		transform.localEulerAngles = new Vector3(float.Parse(data[4]), float.Parse(data[5]), float.Parse(data[6]));
		transform.localScale = new Vector3(float.Parse(data[7]), float.Parse(data[8]), float.Parse(data[9]));
		for (int i = 0; i < 10; i++)
			data.RemoveAt(0);
	}
}
