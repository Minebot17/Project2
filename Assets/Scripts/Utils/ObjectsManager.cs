using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class ObjectsManager {
	public static List<GameObject> loadedObjects = new List<GameObject>();

	public static void LoadAllObjectsFromResources() {
		loadedObjects = Resources.LoadAll<GameObject>("Prefabs").ToList<GameObject>();
	}

	public static GameObject SpawnRoomObject(RoomObject obj, Transform parent) {
		GameObject go = GetObjectByName(obj.prefabName + "_" + obj.type);
		if (go == null)
			go = GetObjectByName(obj.prefabName);
		if (go == null) {
			Debug.LogError("Try to spawn non-existent object: " + obj.prefabName);
			return null;
		}

		go = SpawnGameObject(go, new Vector3(), go.transform.localEulerAngles, parent, false);
		go.name = obj.prefabName + "_" + obj.ID;
		go.transform.localPosition = obj.coords;
		if (obj.data != null && obj.data.Length != 0) {
			SpawnedData spawnedData = go.GetComponent<SpawnedData>();
			if (spawnedData == null) {
				Debug.LogError("Spawned prefab not have SpawnedData script: " + go.name);
				return null;
			}

			spawnedData.spawnedData = obj.data;
		}

		if (go.GetComponent<TypedObject>() != null) {
			TypedObject.Type type = go.GetComponent<TypedObject>().Types[obj.type];
			go.transform.localScale = new Vector3(type.Size.x, type.Size.y, go.transform.localScale.z);
			for (int i = 0; i < go.transform.childCount; i++)
				go.transform.GetChild(i).position = go.transform.position + go.transform.GetChild(i).localPosition;
			
			Mesh mesh = MonoBehaviour.Instantiate(InitScane.instance.OnePlane);
			mesh.SetUVs(0, new List<Vector2>() {
				new Vector2(type.MinUV.x, type.MinUV.y), 
				new Vector2(type.MaxUV.x, type.MinUV.y),
				new Vector2(type.MaxUV.x, type.MaxUV.y),
				new Vector2(type.MinUV.x, type.MaxUV.y)
			});
			go.GetComponent<MeshFilter>().mesh = mesh;
		}

		MeshFilter filter = go.GetComponent<MeshFilter>();
		if (filter != null && filter.mesh != null) {
			Mesh mesh = filter.mesh;
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
			mesh.RecalculateTangents();
		}

		if (obj.mirrorX) {
			Vector3[] a = go.GetComponent<MeshFilter>().mesh.vertices;
			a = null;
		}

		go.transform.localScale = new Vector3(go.transform.localScale.x * (obj.mirrorX ? -1 : 1), go.transform.localScale.y * (obj.mirrorY ? -1 : 1), go.transform.localScale.z);
		if (filter != null && filter.mesh != null) {
			try {
				go.transform.position += new Vector3(
					obj.mirrorX
						? (filter.mesh.vertices[1].x -
						   filter.mesh.vertices[0].x) * -go.transform.localScale.x
						: 0,
					obj.mirrorY
						? (filter.mesh.vertices[2].y -
						   filter.mesh.vertices[0].y) * -go.transform.localScale.y
						: 0, 0); // mark
			}
			catch (IndexOutOfRangeException e) {
				Debug.Log("OutOfRange");
			}
		}

		int childCount = go.transform.childCount;
		if (go.GetComponent<SimpleObject>().NotMirrorChildrensOnSpawn)
			for (int i = 0; i < childCount; i++)
				go.transform.GetChild(i).localScale = new Vector3(go.transform.GetChild(i).localScale.x * (obj.mirrorX ? -1 : 1), go.transform.GetChild(i).localScale.y * (obj.mirrorY ? -1 : 1), go.transform.GetChild(i).localScale.z);

		return go;
	}
	
	public static GameObject SpawnGameObject(GameObject obj, Vector2 coords, Vector3 rotate, Transform parent, bool forceInit) {
		GameObject go = MonoBehaviour.Instantiate(obj);
		go.transform.parent = parent;
		go.transform.position = new Vector3(coords.x, coords.y, go.transform.position.z);
		go.transform.localEulerAngles = rotate;
		if (obj.GetComponent<SimpleObject>() == null) {
			obj.AddComponent<SimpleObject>().Initialize();
		}
		else if (forceInit) {
			MonoBehaviour.Destroy(go.GetComponent<SimpleObject>());
			go.AddComponent<SimpleObject>().Initialize();
		}

		return go;
	}

	private static GameObject GetObjectByName(string name) {
		GameObject result = null;
		foreach (GameObject obj in loadedObjects)
			if (obj.name.Equals(name)) {
				result = obj;
				break;
			}
		return result;
	}
}

/// <summary>
/// RoomObject - объект, который есть в файле комнаты, и спавнится вместе с комнатой. При спавне имеет spawnData
/// </summary>
public class RoomObject {
	public string prefabName;
	public Vector3 coords;
	public int ID;
	public bool mirrorX;
	public bool mirrorY;
	public int type;
	public string[] data;

	public RoomObject(string prefabName, Vector2Int coords, int ID, bool mirrorX, bool mirrorY, int type, string[] data) :
		this(prefabName, new Vector3(coords.x, coords.y, 0), ID, mirrorX, mirrorX, type, data) { }

	public RoomObject(string prefabName, Vector3 coords, int ID, bool mirrorX, bool mirrorY, int type, string[] data) {
		this.prefabName = prefabName;
		this.coords = coords;
		this.ID = ID;
		this.mirrorX = mirrorX;
		this.mirrorY = mirrorY;
		this.type = type;
		this.data = data;
	}
}
