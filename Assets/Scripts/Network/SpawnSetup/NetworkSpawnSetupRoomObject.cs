using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkSpawnSetupRoomObject : MonoBehaviour, INetworkSpawnSetup {
	
	public virtual List<string> SendData(GameObject gameObject) {
		RoomObject obj = gameObject.GetComponent<SpawnedData>().roomObject;
		List<string> result = new List<string>() {
			obj.prefabName,
			obj.coords.x+"",
			obj.coords.y+"",
			obj.coords.z+"",
			obj.ID+"",
			obj.mirrorX+"",
			obj.mirrorY+"",
			obj.type+""
		};
		if (obj.data != null)
			foreach (string data in obj.data)
				result.Add(data);
		result.Add("endRoomObject");
		return result;
	}

	public virtual void RecieveData(GameObject gameObject, List<string> data) {
		data.RemoveAt(0);
		RoomObject obj = new RoomObject(
			data[0], 
			new Vector3(float.Parse(data[1]), float.Parse(data[2]), float.Parse(data[3])),
			int.Parse(data[4]),
			bool.Parse(data[5]),
			bool.Parse(data[6]),
			int.Parse(data[7]),
			GetSpawnedData(data)
		);
		ObjectsManager.SetupRoomObject(gameObject, obj);
		MonoBehaviour[] mbs = gameObject.GetComponents<MonoBehaviour>();
		foreach (MonoBehaviour mb in mbs) {
			mb.Invoke("Start", 0);
		}
		for (int i = 0; i < 8 + obj.data.Length; i++)
			data.RemoveAt(0);
	}

	private string[] GetSpawnedData(List<string> data) {
		int end = 8;
		while (!data[end].Equals("endRoomObject")) {
			end++;
		}

		return data.GetRange(8, end - 8).ToArray();
	}
}
