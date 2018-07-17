using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour {
	[SerializeField] 
	private string roomName;
	[SerializeField]
	private Vector2Int size;

	public bool IsObjectsInitialized;
	
	public string RoomName {
		get { return roomName; }
	}
	
	public Vector2Int Size {
		get { return size; }
	}

	public void Initialize(string roomName, Vector2Int size) {
		this.roomName = roomName;
		this.size = size;
	}

	public void InitializeObjects() {
		Transform objs = transform.Find("Objects");
		for (int i = 0; i < objs.childCount; i++) {
			if (objs.GetChild(i).GetComponent<ISerializableObject>() != null)
				objs.GetChild(i).GetComponent<ISerializableObject>().Initialize();
		}

		IsObjectsInitialized = true;
	}
}
