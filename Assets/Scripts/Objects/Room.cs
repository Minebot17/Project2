using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour {
	[SerializeField] 
	private string roomName;
	[SerializeField]
	private Vector2Int size;

	public bool NeedInitializeObjects;
	public int ObjectToInitialize;
	public bool Initialized;
	
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
}
