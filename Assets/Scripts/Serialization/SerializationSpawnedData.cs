using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpawnedData))]
public class SerializationSpawnedData : MonoBehaviour, ISerializableObject {

	public void Initialize() {
		
	}

	public List<string> Serialize() {
		List<string> result = new List<string>();
		result.AddRange(GetComponent<SpawnedData>().spawnedData);
		result.Add("endSpawnedData");
		return result;
	}

	public int Deserialize(List<string> data) {
		int count = 0;
		List<string> toData = new List<string>();
		foreach (string str in data) {
			count++;
			if (str.Equals("endSpawnedData"))
				break;
			toData.Add(str);
		}

		GetComponent<SpawnedData>().spawnedData = toData.ToArray();
		return count;
	}
}
