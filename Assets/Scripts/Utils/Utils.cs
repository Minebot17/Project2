using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Utils {

	public static EventHandler<T> FindEventHandler<T>(object[] eventHandlers) where T : EventBase {
		return eventHandlers.Single(x => x.GetType().GetGenericArguments()[0] == typeof(T)) as EventHandler<T>;
	}

	public static UnityEngine.Object FindResource(string folderPath, string fileName) {
		UnityEngine.Object[] objs = Resources.LoadAll(folderPath);
		return objs.First(x => x.name.Equals(fileName));
	}

	public static bool IsTouchRoom(Collider2D collider2D) {
		return Physics2D.IsTouchingLayers(collider2D, InitScane.instance.RoomLayerMask);
	}

	public static bool IsFreeBetweenPlayer(Vector3 point) {
		return Physics2D.Raycast(point, ToVector2(InitScane.instance.Player.transform.position - point),
			Vector2.Distance(point, ToVector2(InitScane.instance.Player.transform.position)), LayerMask.GetMask("Flapper", "Room")).collider == null;
	}

	public static bool IsRotatedToPlayer(Transform transform) {
		return (InitScane.instance.Player.transform.position.x - transform.position.x) < 0 ? transform.localScale.x < 0 : transform.localScale.x > 0;
	}

	public static float GetDistanceBetweenPlayer(Vector3 vector) {
		return Vector3.Distance(InitScane.instance.Player.transform.position, vector);
	}

	public static bool IsOnEqualsYWithPlayer(float y, float error) {
		float yPlayer = InitScane.instance.Player.transform.position.y;
		return yPlayer < y + error && yPlayer > y - error;
	}

	public static Vector2 ToVector2(Vector3 vector) {
		return new Vector2(vector.x, vector.y);
	}

	public static Vector2 RandomPoint(float scale) {
		return new Vector2(((float)InitScane.rnd.NextDouble() - 0.5f) * scale * 2, ((float)InitScane.rnd.NextDouble() - 0.5f) * scale * 2);
	}

	public static GameObject FindNearestGameObject(List<GameObject> list, Vector2 point) {
		GameObject result = list[0];
		float distance = Vector2.Distance(point, result.transform.position);
		foreach (GameObject go in list) {
			float thisDistance = Vector2.Distance(point, go.transform.position);
			if (thisDistance < distance) {
				result = go;
				distance = thisDistance;
			}
		}

		return result;
	}

	public static Vector2 GetPlayerLook() {
		Vector2 mousePos = ToVector2(GameObject.Find("Main Camera").GetComponent<Camera>().ScreenToWorldPoint(Input.mousePosition));
		Vector2 headPos = ToVector2(InitScane.instance.PlayerHead.transform.position);
		return (mousePos - headPos).normalized;
	}

	public static List<T> GetComponentsRecursive<T>(GameObject gameObject) {
		List<T> result = new List<T>();
		result.AddRange(gameObject.GetComponents<T>());
		for (int i = 0; i < gameObject.transform.childCount; i++)
			result.AddRange(GetComponentsRecursive<T>(gameObject.transform.GetChild(i).gameObject));
		return result;
	}

	public static Mesh GetQuadMesh(Vector3 start, Vector3 end, Vector2 startUV, Vector2 endUV) {
		Mesh mesh = new Mesh();
		mesh.vertices = new [] {
			new Vector3(start.x, start.y, 0), 
			new Vector3(start.x, end.y, 0),
			new Vector3(end.x, end.y, 0),
			new Vector3(end.x, start.y, 0),
		};
		mesh.uv = new [] {
			new Vector2(startUV.x, startUV.y), 
			new Vector2(startUV.x, endUV.y), 
			new Vector2(endUV.x, endUV.y), 
			new Vector2(endUV.x, startUV.y), 
		};
		bool xMinus = end.x - start.x < 0;
		bool yMinus = end.y - start.y < 0;
		mesh.triangles = (xMinus || yMinus) && !(xMinus && yMinus) ? new [] { 0, 2, 1, 0, 3, 2 } : new [] { 0, 1, 2, 2, 3, 0 };
		mesh.normals = new [] {
			Vector3.back, 
			Vector3.back, 
			Vector3.back, 
			Vector3.back, 
		};
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
		mesh.RecalculateTangents();
		return mesh;
	}
}
