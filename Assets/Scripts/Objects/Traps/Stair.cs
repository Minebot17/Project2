using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Stair : MonoBehaviour {
	public Material StairMaterial;
	public int type; // 0 - обычная, 1 - исчезающая, 2 - ломающаяся
	public float disableTime;
	public List<GameObject> childs = new List<GameObject>();
	
	private void Start() {
		childs.Add(transform.GetChild(0).gameObject);
		string[] data = GetComponent<SpawnedData>().spawnedData;
		type = int.Parse(data[0]);
		disableTime = float.Parse(data[1]);
		int stairWidth = int.Parse(data[2]);
		bool withEnders = bool.Parse(data[3]);

		BoxCollider2D col = childs[0].GetComponent<BoxCollider2D>();
		col.size = new Vector2(stairWidth, col.size.y);
		col.offset = new Vector2(stairWidth/2f, col.offset.y);

		float xUVOffset = type == 0 ? 0 : type == 1 ? 0.3125f : 0.625f;
		float yUVOffset = type == 0 ? 0.4375f : type == 1 ? 0.125f : 0.75f;
		if (withEnders) {
			GameObject leftCorner = new GameObject("leftCorner");
			leftCorner.transform.parent = transform;
			leftCorner.transform.localPosition = Vector3.zero;
			Mesh leftMesh = Utils.GetQuadMesh(
				new Vector3(0, 0), new Vector3(2, 8),
				new Vector2(0.25f + xUVOffset, 0), new Vector2(xUVOffset, 0.0625f)
			);
			leftMesh.uv = new [] {
				new Vector2(0.25f + xUVOffset, 0),
				new Vector2(xUVOffset, 0),
				new Vector2(xUVOffset, 0.0625f),
				new Vector2(0.25f + xUVOffset, 0.0625f), 
			};
			leftMesh.RecalculateBounds();
			leftMesh.RecalculateNormals();
			leftMesh.RecalculateTangents();
			leftCorner.AddComponent<MeshFilter>().mesh = leftMesh;
			leftCorner.AddComponent<MeshRenderer>().material = StairMaterial;
			childs.Add(leftCorner);
			
			GameObject rightCorner = new GameObject("rightCorner");
			rightCorner.transform.parent = transform;
			rightCorner.transform.localPosition = Vector3.zero;
			Mesh rightMesh = Utils.GetQuadMesh(
				new Vector3(stairWidth - 2, 0), new Vector3(stairWidth, 8),
				new Vector2(0.25f + xUVOffset, 0.0625f), new Vector2(xUVOffset, 0)
			);
			rightMesh.uv = new Vector2[] {
				new Vector2(0.25f + xUVOffset, 0.0625f),
				new Vector2(xUVOffset, 0.0625f),
				new Vector2(xUVOffset, 0),
				new Vector2(0.25f + xUVOffset, 0), 
			};
			rightMesh.RecalculateBounds();
			rightMesh.RecalculateNormals();
			rightMesh.RecalculateTangents();
			rightCorner.AddComponent<MeshFilter>().mesh = rightMesh;
			rightCorner.AddComponent<MeshRenderer>().material = StairMaterial;
			childs.Add(rightCorner);
		}
		
		GameObject center = new GameObject("center");
		center.transform.parent = transform;
		center.transform.localPosition = Vector3.zero;
		center.AddComponent<MeshFilter>().mesh = Utils.GetQuadMesh(
			new Vector3(withEnders ? 2 : 0, 0), new Vector3(withEnders ? stairWidth - 2 : stairWidth, 8),
			new Vector2(0, yUVOffset), new Vector2(stairWidth/32f, yUVOffset + 0.25f)
		);
		center.AddComponent<MeshRenderer>().material = StairMaterial;
		childs.Add(center);

		foreach (GameObject player in GameManager.singleton.Players) {
			player.GetComponent<EntityGroundInfo>().GetEventSystem<EntityGroundInfo.LandingEvent>()
				.SubcribeEvent(
					@event => {
						SetupForPlayer(player);
					}
				);
		}

		if (NetworkManagerCustom.IsServer)
			ServerEvents.singleton.GetEventSystem<ServerEvents.OnServerPlayerAdd>().SubcribeEvent(x => {
				x.Player.GetComponent<EntityGroundInfo>().GetEventSystem<EntityGroundInfo.LandingEvent>()
					.SubcribeEvent(
						@event => {
							SetupForPlayer(x.Player);
						}
					);
			});
	}

	private void SetupForPlayer(GameObject player) {
		Collider2D[] result = new Collider2D[10];
		Physics2D.OverlapCollider(
			player.GetComponent<EntityGroundInfo>().GroundTrigger,
			new ContactFilter2D(), result);
		if (result.ToList().Exists(x => x != null && x.gameObject == childs[0])) {
			if (type == 1) {
				Timer.StartNewTimer("StairDisable", 0.5f, 1, gameObject, x => {
					foreach (GameObject child in childs)
						child.SetActive(false);
				});
				Timer.StartNewTimer("StairEnable", disableTime, 1, gameObject, x => {
					foreach (GameObject child in childs)
						child.SetActive(true);
				});
			}
			else if (type == 2) {
				Timer.StartNewTimer("StairRemove", 0.5f, 1, gameObject, x => {
					foreach (GameObject child in childs)
						child.SetActive(false);
				});
			}
		}
	}
}
