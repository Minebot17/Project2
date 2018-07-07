using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour {
	[SerializeField]
	private Vector2Int size;
	
	public Vector2Int Size {
		get { return size; }
	}

	public void Initialize(Vector2Int size) {
		this.size = size;
	}
}
