using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(SpawnedData))]
public class HiddenEntryObject : NetworkBehaviour {

	void Start () {
		NetworkServer.Spawn(transform.GetChild(1).gameObject);
		string[] data = GetComponent<SpawnedData>().spawnedData;
		int length = int.Parse(data[0]);
		bool horizontal = bool.Parse(data[1]);

		Mesh meshBorder = new Mesh();
		meshBorder.SetVertices(new List<Vector3>() {
			new Vector3(0, 0, 0), new Vector3(length, 0, 0), new Vector3(length, -9, 0), new Vector3(0, -9, 0),
			new Vector3(0, -6, 0), new Vector3(-9, -6, 0), new Vector3(-9, -9, 0), new Vector3(0, -9, 0),
			new Vector3(length, -6, 0), new Vector3(length + 9, -6, 0), new Vector3(length + 9, -9, 0), new Vector3(length, -9, 0),
		});
		meshBorder.SetUVs(0, new List<Vector2>() {
			new Vector2(0, 0.695f), new Vector2(length/32f, 0.695f), new Vector2(length/32f, 0.625f), new Vector2(0, 0.625f),
			new Vector2(0.594f, 0.508f), new Vector2(0.875f, 0.508f), new Vector2(0.875f, 0.484f), new Vector2(0.594f, 0.484f),
			new Vector2(0.594f, 0.508f), new Vector2(0.875f, 0.508f), new Vector2(0.875f, 0.484f), new Vector2(0.594f, 0.484f),
		});
		meshBorder.triangles = new int[] { 0, 1, 3, 1, 2, 3, 4, 7, 6, 5, 4, 6, 8, 9, 11, 11, 9, 10 };
		meshBorder.normals = RoomLoader.getNormals(12);
		meshBorder.RecalculateBounds();
		meshBorder.RecalculateNormals();
		meshBorder.RecalculateTangents();
		
		Mesh meshShadow = new Mesh();
		meshShadow.SetVertices(new List<Vector3>() {
			new Vector3(0, 0, -100), new Vector3(length, 0, -100), new Vector3(length, 0, 100), new Vector3(0, 0, 100)
		});
		meshShadow.SetUVs(0, new List<Vector2>() {
			new Vector2(0, 0.695f), new Vector2(1, 0.695f), new Vector2(1, 0.65f), new Vector2(0, 0.65f)
		});
		meshShadow.triangles = new int[] { 1, 0, 2, 3, 2, 0 };
		meshShadow.normals = RoomLoader.getNormals(4);
		meshShadow.RecalculateBounds();
		meshShadow.RecalculateNormals();
		meshShadow.RecalculateTangents();
		
		GameObject border = transform.Find("Border").gameObject;
		border.GetComponent<MeshFilter>().mesh= meshBorder;
		border.GetComponent<BoxCollider2D>().offset = new Vector2(length/2f, border.GetComponent<BoxCollider2D>().offset.y);
		border.GetComponent<BoxCollider2D>().size = new Vector2(length, border.GetComponent<BoxCollider2D>().size.y);

		transform.Find("Shadow").gameObject.GetComponent<MeshFilter>().mesh = meshShadow;

		BoxCollider2D flapper = transform.Find("Flapper").gameObject.GetComponent<BoxCollider2D>();
		flapper.offset = new Vector2(length/2f, flapper.offset.y);
		flapper.size = new Vector2(length, flapper.size.y);
		
		BoxCollider2D triggerEnter = transform.Find("TriggerEnter").gameObject.GetComponent<BoxCollider2D>();
		triggerEnter.offset = new Vector2(length/2f, triggerEnter.offset.y);
		triggerEnter.size = new Vector2(length, triggerEnter.size.y);
		
		BoxCollider2D triggerOut = transform.Find("TriggerOut").gameObject.GetComponent<BoxCollider2D>();
		triggerOut.offset = new Vector2(length/2f, triggerOut.offset.y);
		triggerOut.size = new Vector2(length, triggerOut.size.y);

		if (!horizontal)
			transform.localEulerAngles = new Vector3(0, 0, -90);
	}
}
