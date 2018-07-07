using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollower : MonoBehaviour {

	[SerializeField]
	private GameObject room;
	
	[SerializeField]
	private GameObject target;
	private Vector2Int roomSize;

	public float speed;

	public GameObject Room {
		set {
			if (value.GetComponent<Room>() == null)
				throw new Exception("Попытка задать камере объект, который не является комнатой");
			room = value;
			roomSize = new Vector2Int(room.GetComponent<Room>().Size.x * 495, room.GetComponent<Room>().Size.y * 277);
		}
	}

	public GameObject Target {
		set { target = value; }
	}

	private void FixedUpdate() {
		Vector2 min = room.transform.position + new Vector3(247.5f, 138.5f);
		Vector2 max = min + roomSize - new Vector2(247.5f, 138.5f)*2;
		
		if (transform.position != target.transform.position) {
			Vector2 delta = ((target.transform.position - transform.position) * speed) + transform.position;
			
			if (delta.x < min.x)
				delta.x = min.x;
			else if (delta.x > max.x)
				delta.x = max.x;
			
			if (delta.y < min.y)
				delta.y = min.y;
			else if (delta.y > max.y)
				delta.y = max.y;

			if (transform.position != new Vector3(delta.x, delta.y, transform.position.z))
				transform.position = new Vector3(delta.x, delta.y, transform.position.z);
		}
	}
}
