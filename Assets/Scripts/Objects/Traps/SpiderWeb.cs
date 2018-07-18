using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpiderWeb : MonoBehaviour {
	public GameObject Tile;
	public GameObject EndWeb;
	public Material[] Materials;
	
	private void Start() {
		float rayDistance = transform.parent.parent.gameObject.GetComponent<Room>().Size.y * 277 - (transform.position - GenerationManager.currentRoom.transform.position).y;
		RaycastHit2D ray = Physics2D.Raycast(Utils.ToVector2(transform.position) + new Vector2(10, 22), Vector2.up,
			rayDistance, GameManager.Instance.RoomLayerMask);
		if (ray.collider == null) {
			Destroy(gameObject);
			return;
		}
		
		int count = (int) Mathf.Round((ray.distance - 8) / 16f);
		Rigidbody2D lastRigidbody = null;

		for (int i = count - 1; i >= 0; i--) {
			GameObject tile = Instantiate(Tile, transform);
			tile.GetComponent<MeshRenderer>().material = Materials[GameManager.rnd.Next(Materials.Length)];
			tile.transform.localPosition = new Vector3(6, 22 + i * 16);
			tile.GetComponent<HingeJoint2D>().connectedBody = lastRigidbody;
			lastRigidbody = tile.GetComponent<Rigidbody2D>();

			if (i == count - 1)
				tile.transform.localScale = new Vector3(tile.transform.localScale.x, (ray.distance - 8) - i * 16, 1);
		}

		HingeJoint2D lastJoint = transform.GetChild(1).gameObject.GetComponents<HingeJoint2D>()
			.First(x => x.anchor.Equals(new Vector2(0.5f, 1)));
		lastJoint.connectedBody = lastRigidbody;

		GameObject endWeb = Instantiate(EndWeb, transform);
		endWeb.transform.localPosition = new Vector3(0, 22 + ray.distance, 0);
		endWeb.transform.localScale = new Vector3(24, -8, 1);
	}
}
