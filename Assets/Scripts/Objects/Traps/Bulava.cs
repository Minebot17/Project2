using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bulava : MonoBehaviour {
	public GameObject Tile;
	private Vector3 initialPosition;
	private Vector3 turnPosition;
	private int tileCount;
	private int rotateMode;
	private float angleSpeed;
	private int startAngle;
	private int motionMode;
	private float motionSpeed;
	private int motionDistance;
	private int startDistance;
	private bool directionAngleToggle;
	private int directionMotion = 1;
	
	private void Start() {
		string[] data = GetComponent<SpawnedData>().spawnedData;
		tileCount = int.Parse(data[0]);
		rotateMode = int.Parse(data[1]);
		angleSpeed = int.Parse(data[2])/50f;
		startAngle = int.Parse(data[3]);
		motionMode = int.Parse(data[4]);
		motionSpeed = int.Parse(data[5])/50f;
		motionDistance = int.Parse(data[6]);
		startDistance = int.Parse(data[7]);
		
		transform.position += new Vector3(4.5f, 4.5f, 0);
		for (int i = 0; i < tileCount; i++) {
			GameObject tile = Instantiate(Tile, transform);
			tile.transform.localPosition += new Vector3(0, -5 * i, 0);
		}
		transform.GetChild(0).localPosition += new Vector3(0, -5 * tileCount, 0);
		
		initialPosition = transform.position;
		turnPosition = initialPosition + new Vector3(motionDistance, motionDistance, 0);
		transform.position += motionMode == 1 ? new Vector3(startDistance, 0, 0) : motionMode == 2 ? new Vector3(0, startDistance, 0) : Vector3.zero;
		transform.localEulerAngles = new Vector3(0, 0, startAngle);
	}

	private void FixedUpdate() {
		if (rotateMode == 1) {
			if (transform.localEulerAngles.z < 270 && transform.localEulerAngles.z > 90)
				directionAngleToggle = !directionAngleToggle;
			
			transform.localEulerAngles += new Vector3(0, 0, directionAngleToggle ? angleSpeed : -angleSpeed);
		}
		else if (rotateMode == 2)
			transform.Rotate(new Vector3(0, 0, 1), angleSpeed);

		if (motionMode == 1) {
			if (directionMotion == 1 && transform.position.x - turnPosition.x > 0)
				directionMotion = -1;
			else if (directionMotion == -1 && transform.position.x - initialPosition.x < 0)
				directionMotion = 1;

			transform.position += new Vector3(directionMotion * motionSpeed, 0, 0);
		}
		else if (motionMode == 2) {
			if (directionMotion == 1 && transform.position.y - turnPosition.y > 0)
				directionMotion = -1;
			else if (directionMotion == -1 && transform.position.y - initialPosition.y < 0)
				directionMotion = 1;
			
			transform.position += new Vector3(0, directionMotion * motionSpeed, 0);
		}
	}
}
