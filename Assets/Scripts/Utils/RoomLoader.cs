using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public static class RoomLoader {
	public static List<Room> loadedRooms = new List<Room>();
	private static Material[] backgrounds = new Material[3];
	private static Material[] hiddenBackgrounds = new Material[3];
	private static Material[] walls = new Material[3];
	private static Material[] borders = new Material[3];

	private static Vector2[][] lastColliderPoints;
	private static List<RoomObject> objectsToAddToNextRoom;

	public static void LoadAllRoomsFromResources() {
		string path = "Materials/Rooms/";
		for (int i = 0; i < 3; i++) {
			backgrounds[i] = Resources.Load<Material>(path + "Background" + i);
			walls[i] = Resources.Load<Material>(path + "Wall" + i);
			borders[i] = Resources.Load<Material>(path + "Border" + i);
			hiddenBackgrounds[i] = Resources.Load<Material>(path + "HiddenBackground" + i);
		}

		TextAsset[] rooms = Resources.LoadAll<TextAsset>("Rooms");
		string[] fileNames = Array.ConvertAll(rooms, x => x.name);
		RoomSerializeHelper.SaveFile[] files = Array.ConvertAll<string, RoomSerializeHelper.SaveFile>( // TODO: В бетке запеч в префабы и грузить префабы
			fixJsons(
				Array.ConvertAll<TextAsset, string>(
					rooms,
					x => x.text)
			),
			x => JsonUtility.FromJson<RoomSerializeHelper.SaveFile>(x)
		);

		for (int i = 0; i < files.Length; i++)
			loadedRooms.Add(LoadRoom(fileNames[i], files[i]));
		objectsToAddToNextRoom = null;
		lastColliderPoints = null;
	}

	public static Room LoadRoom(string roomName, RoomSerializeHelper.SaveFile file) {
		objectsToAddToNextRoom = new List<RoomObject>();
		Vector2Int size = new Vector2Int(file.matrix.Length / 495, file.matrix[0].array.Length / 277);
		return new Room(roomName, size, generateMeshes(file.matrix, file.objects.FindAll(x => x.prefabName.Contains("Gate")), size, 0), generateObjects(file.objects), lastColliderPoints, file.location);
	}

	public static Room LoadRoom(string path, Encoding encoding) {
		string[] splitted = path.Split('/');
		string fileName = splitted[splitted.Length - 1];
		return LoadRoom(fileName.Substring(0, fileName.Length - 5), JsonUtility.FromJson<RoomSerializeHelper.SaveFile>(fixJsons(new string[] { File.ReadAllText(path, encoding) })[0]));
	}

	public static GameObject SpawnRoom(Room room, Vector3 position, bool onServer) {
		GameObject roomObject = new GameObject("Room " + room.size.x + "x" + room.size.y) { layer = 8 };
		roomObject.transform.position = position;
		PolygonCollider2D collider = roomObject.AddComponent<PolygonCollider2D>();
		collider.pathCount = room.colliderPoints.Length;
		for (int i = 0; i < room.colliderPoints.Length; i++)
			collider.SetPath(i, room.colliderPoints[i]);

		GameObject meshParent = new GameObject("Meshes");
		meshParent.transform.parent = roomObject.transform;
		meshParent.transform.localPosition = Vector3.zero;
		GameObject objectParent = new GameObject("Objects");
		objectParent.transform.parent = roomObject.transform;
		objectParent.transform.localPosition = Vector3.zero;

		foreach (RoomMesh mesh in room.meshes) {
			GameObject go = new GameObject(mesh.name);
			go.transform.parent = meshParent.transform;
			go.transform.localPosition = new Vector3(0, 0, mesh.layer);
			go.AddComponent<MeshFilter>().mesh = mesh.mesh;
			go.AddComponent<MeshRenderer>().material = mesh.material == null ? GameManager.singleton.DefaultMaterial : mesh.material;
			if (mesh.name.Equals("background"))
				go.layer = 9;
			else if (mesh.name.Equals("shadowMeshX") || mesh.name.Equals("shadowMeshY") || mesh.name.Equals("shadowCornerMeshX") || mesh.name.Equals("shadowCornerMeshY"))
				go.layer = 10;
			else if (mesh.name.Equals("wall") || mesh.name.Equals("hiddenWall"))
				go.GetComponent<MeshRenderer>().receiveShadows = false;
		}

		foreach (RoomObject obj in room.objects)
			ObjectsManager.SpawnRoomObject(obj, objectParent.transform, x => x.GetComponent<NetworkIdentity>() == null || onServer);
		
		roomObject.AddComponent<global::Room>().Initialize(room.fileName, room.size);
		return roomObject;
	}

	public static List<GameObject> SpawnSerializebleObjects(Room room, GameObject roomObject) {
		List<GameObject> result = new List<GameObject>();
		Transform parent = roomObject.transform.Find("Objects");
		foreach (RoomObject obj in room.objects) {
			GameObject spawned = ObjectsManager.SpawnRoomObject(obj, parent,
				x => x.GetComponent<NetworkIdentity>() != null && x.GetComponent<ISerializableObject>() != null);
			if (spawned != null) {
				result.Add(spawned);
			}
		}

		return result;
	}

	#region Generators
	private static List<RoomMesh> generateMeshes(RoomSerializeHelper.IntArray[] matrix, List<RoomSerializeHelper.RoomObject> gates, Vector2Int size, int location) {
		List<Corner> originalCorners = findCorners(matrix);
		List<Corner> cuttedCorners = getCuttedCorners(size, originalCorners);
		List<HiddenCorner> hiddenCorners = findHiddenCorners(matrix);
		List<Corner> hiddenInvertedCuttedCornersBarierr = getBarrierCorners(hiddenCorners);
		List<Corner> hiddenInvertedCorners = new List<Corner>(inverseHiddenCorners(hiddenCorners).Cast<Corner>());
		List<Corner> hiddenInvertedCuttedCorners = getCuttedCorners(size, hiddenInvertedCorners);
		List<Corner> concat = new List<Corner>(originalCorners.Concat(hiddenInvertedCorners));

		generateHiddenEntryBorders(hiddenCorners);
		List<RoomMesh> result = new List<RoomMesh>() {
			generateRoomBackground(size, location, 0f),
			generateWall(new List<Corner>(cuttedCorners.Concat(hiddenInvertedCuttedCorners)), -0.001f, false),
			generateCorners(concat, size, -0.003f),
			generateBordersX(concat, size, -0.003f),
			generateBordersY(concat, size, -0.002f),
			generateHiddenBackground(hiddenCorners, -0.001f),
			generateWall(hiddenInvertedCuttedCornersBarierr, -0.008f, true),
			generateShadowMeshX(concat, size),
			generateShadowMeshY(concat, size),
			generateCornerShadowMeshX(concat, size),
			generateCornerShadowMeshY(concat, size)
		};

		lastColliderPoints = generateCollider(new List<Corner>(getGatesCorners(size, removePreGateCorners(concat), gates).Concat(removePreGateCorners(concat))));
		return result;
	}
	
	private static RoomMesh generateShadowMeshX(List<Corner> cornersFake, Vector2Int size) {
		List<Corner> corners = copyCornerList(cornersFake);
		corners.RemoveAll(x => x.IsExtremeX(size.x) && x.IsExtremeY(size.y));

		List<Vector3> vertices = new List<Vector3>();
		List<Vector2> uv = new List<Vector2>();
		List<int> triangles = new List<int>();
		List<Corner> checkedCorners = new List<Corner>();

		int i = 0;
		foreach (Corner corner in corners) {
			corner.FindConnections(corners);
			if (checkedCorners.Contains(corner) || corner.xConnection == null || (!(corner is HiddenCorner) && (corner.IsExtremeCorner(size.x, size.y) && corner.xConnection.IsExtremeCorner(size.x, size.y))))
				continue;
			bool toDown = FacingUtils.GetFacingsFromCornerFacing(corner.cornerFacing)[1] == Facing.DOWN;
			bool connectionRighter = corner.xConnection.coords.x > corner.coords.x;
			Corner leftCorner = connectionRighter ? corner : corner.xConnection;
			Corner rightCorner = connectionRighter ? corner.xConnection : corner;

			bool fail = true;
			if (leftCorner.coords.y == 23 && rightCorner.coords.y == 23 || leftCorner.coords.y == size.y * 277 - 23 && rightCorner.coords.y == size.y * 277 -23) {
				Vector2 lastLeftCoord = leftCorner.coords;
				
				for (int x = 0; x < size.x; x++) {
					float xCenter = 495f * (x + 0.5f);
					if (xCenter - 27f >= leftCorner.coords.x && xCenter + 27f <= rightCorner.coords.x) {

						vertices.Add(new Vector3(lastLeftCoord.x, lastLeftCoord.y, 100));
						vertices.Add(new Vector3(xCenter - 27f, lastLeftCoord.y, 100));
						vertices.Add(new Vector3(xCenter - 27f, lastLeftCoord.y, -100));
						vertices.Add(new Vector3(lastLeftCoord.x, lastLeftCoord.y, -100));

						lastLeftCoord = new Vector2(xCenter + 27f, lastLeftCoord.y);

						/*vertices.Add(new Vector3(xCenter + 27f, leftCorner.coords.y, 100));
						vertices.Add(new Vector3(rightCorner.coords.x, leftCorner.coords.y, 100));
						vertices.Add(new Vector3(rightCorner.coords.x, leftCorner.coords.y, -100));
						vertices.Add(new Vector3(xCenter + 27f, leftCorner.coords.y, -100));*/

						for (int j = 0; j < 4; j++)
							uv.Add(new Vector2());
						triangles.AddRange(!toDown ? new int[] { i + 1, i, i + 3, i + 3, i + 2, i + 1 } : new int[] { i, i + 1, i + 3, i + 2, i + 3, i + 1 });
						//triangles.AddRange(!toDown ? new int[] { i + 5, i + 4, i + 7, i + 7, i + 6, i + 5 } : new int[] { i + 4, i + 5, i + 7, i + 6, i + 7, i + 5 });

						i += 4;
						fail = false;
						//break;
					}
				}

				if (!fail && (int) lastLeftCoord.x != rightCorner.coords.x) {
					vertices.Add(new Vector3(lastLeftCoord.x, lastLeftCoord.y, 100));
					vertices.Add(new Vector3(rightCorner.coords.x, lastLeftCoord.y, 100));
					vertices.Add(new Vector3(rightCorner.coords.x, lastLeftCoord.y, -100));
					vertices.Add(new Vector3(lastLeftCoord.x, lastLeftCoord.y, -100));
					
					for (int j = 0; j < 4; j++)
						uv.Add(new Vector2());
					triangles.AddRange(!toDown ? new int[] { i + 1, i, i + 3, i + 3, i + 2, i + 1 } : new int[] { i, i + 1, i + 3, i + 2, i + 3, i + 1 });
					i += 4;
				}
			}
			if (fail) {
				vertices.Add(new Vector3(leftCorner.coords.x, leftCorner.coords.y, 100));
				vertices.Add(new Vector3(rightCorner.coords.x, leftCorner.coords.y, 100));
				vertices.Add(new Vector3(rightCorner.coords.x, leftCorner.coords.y, -100));
				vertices.Add(new Vector3(leftCorner.coords.x, leftCorner.coords.y, -100));

				for (int j = 0; j < 4; j++)
					uv.Add(new Vector2());
				triangles.AddRange(!toDown ? new int[] { i + 1, i, i + 3, i + 3, i + 2, i + 1 } : new int[] { i, i + 1, i + 3, i + 2, i + 3, i + 1 });

				i += 4;
			}
			checkedCorners.Add(corner);
			checkedCorners.Add(corner.xConnection);
		}

		return new RoomMesh("shadowMeshX", getMesh(vertices, uv, triangles.ToArray(), getNormals(vertices.Count)), null, 0);
	}

	private static RoomMesh generateShadowMeshY(List<Corner> cornersFake, Vector2Int size) {
		List<Corner> corners = copyCornerList(cornersFake);
		corners.RemoveAll(x => x.IsExtremeX(size.x) && x.IsExtremeY(size.y));

		List<Vector3> vertices = new List<Vector3>();
		List<Vector2> uv = new List<Vector2>();
		List<int> triangles = new List<int>();
		List<Corner> checkedCorners = new List<Corner>();

		int i = 0;
		foreach (Corner corner in corners) {
			corner.FindConnections(corners);
			if (checkedCorners.Contains(corner) || corner.yConnection == null || (!(corner is HiddenCorner) && (corner.IsExtremeCorner(size.x, size.y) && corner.yConnection.IsExtremeCorner(size.x, size.y))))
				continue;
			bool toLeft = FacingUtils.GetFacingsFromCornerFacing(corner.cornerFacing)[0] == Facing.LEFT;
			bool connectionUpper = corner.yConnection.coords.y > corner.coords.y;
			Corner downCorner = connectionUpper ? corner : corner.yConnection;
			Corner upCorner = connectionUpper ? corner.yConnection : corner;

			bool fail = true;
			if (downCorner.coords.x == 45 && upCorner.coords.x == 45 || downCorner.coords.x == size.x * 495 - 45 && upCorner.coords.x == size.x * 495 - 45) {
				Vector2 lastDownCoord = downCorner.coords;
				
				for (int y = 0; y < size.y; y++) {
					float yCenter = 277f * (y + 0.5f);
					if (yCenter - 27f >= downCorner.coords.y && yCenter + 27f <= upCorner.coords.y) {

						vertices.Add(new Vector3(lastDownCoord.x, lastDownCoord.y, 100));
						vertices.Add(new Vector3(lastDownCoord.x, yCenter - 27f, 100));
						vertices.Add(new Vector3(lastDownCoord.x, yCenter - 27f, -100));
						vertices.Add(new Vector3(lastDownCoord.x, lastDownCoord.y, -100));

						lastDownCoord = new Vector2(lastDownCoord.x, yCenter + 27f);

						/*vertices.Add(new Vector3(downCorner.coords.x, yCenter + 27f, 100));
						vertices.Add(new Vector3(downCorner.coords.x, upCorner.coords.y, 100));
						vertices.Add(new Vector3(downCorner.coords.x, upCorner.coords.y, -100));
						vertices.Add(new Vector3(downCorner.coords.x, yCenter + 27f, -100));*/

						for (int j = 0; j < 4; j++)
							uv.Add(new Vector2());
						triangles.AddRange(toLeft ? new int[] { i + 1, i, i + 3, i + 3, i + 2, i + 1 } : new int[] { i, i + 1, i + 3, i + 2, i + 3, i + 1 });
						//triangles.AddRange(toLeft ? new int[] { i + 5, i + 4, i + 7, i + 7, i + 6, i + 5 } : new int[] { i + 4, i + 5, i + 7, i + 6, i + 7, i + 5 });

						i += 4;
						fail = false;
						//break;
					}
				}
				
				if (!fail && (int) lastDownCoord.y != upCorner.coords.y) {
					vertices.Add(new Vector3(lastDownCoord.x, lastDownCoord.y, 100));
					vertices.Add(new Vector3(lastDownCoord.x, upCorner.coords.y, 100));
					vertices.Add(new Vector3(lastDownCoord.x, upCorner.coords.y, -100));
					vertices.Add(new Vector3(lastDownCoord.x, lastDownCoord.y, -100));
					
					for (int j = 0; j < 4; j++)
						uv.Add(new Vector2());
					triangles.AddRange(toLeft ? new int[] { i + 1, i, i + 3, i + 3, i + 2, i + 1 } : new int[] { i, i + 1, i + 3, i + 2, i + 3, i + 1 });
					i += 4;
				}
			}
			if (fail) {
				vertices.Add(new Vector3(downCorner.coords.x, downCorner.coords.y, 100));
				vertices.Add(new Vector3(downCorner.coords.x, upCorner.coords.y, 100));
				vertices.Add(new Vector3(downCorner.coords.x, upCorner.coords.y, -100));
				vertices.Add(new Vector3(downCorner.coords.x, downCorner.coords.y, -100));

				for (int j = 0; j < 4; j++)
					uv.Add(new Vector2());
				triangles.AddRange(toLeft ? new int[] { i + 1, i, i + 3, i + 3, i + 2, i + 1 } : new int[] { i, i + 1, i + 3, i + 2, i + 3, i + 1 });

				i += 4;
			}
			checkedCorners.Add(corner);
			checkedCorners.Add(corner.yConnection);
		}

		return new RoomMesh("shadowMeshY", getMesh(vertices, uv, triangles.ToArray(), getNormals(vertices.Count)), null, 0);
	}

	private static RoomMesh generateCornerShadowMeshX(List<Corner> cornersFake, Vector2Int size) {
		List<Corner> corners = copyCornerList(cornersFake);
		corners.RemoveAll(x => !(x.IsExtremeX(size.x) || x.IsExtremeY(size.y)));

		List<Vector3> vertices = new List<Vector3>();
		List<Vector2> uv = new List<Vector2>();
		List<int> triangles = new List<int>();
		List<Corner> checkedCorners = new List<Corner>();

		int i = 0;
		foreach (Corner corner in corners) {
			corner.FindConnections(corners);
			if (checkedCorners.Contains(corner) || corner.xConnection == null)
				continue;
			bool toDown = FacingUtils.GetFacingsFromCornerFacing(corner.cornerFacing)[1] == Facing.DOWN;
			bool connectionRighter = corner.xConnection.coords.x > corner.coords.x;
			Corner leftCorner = connectionRighter ? corner : corner.xConnection;
			Corner rightCorner = connectionRighter ? corner.xConnection : corner;

			bool fail = true;
			if (leftCorner.coords.y == 0 && rightCorner.coords.y == 0 || leftCorner.coords.y == size.y * 277 && rightCorner.coords.y == size.y * 277) {
				Vector2 lastLeftCoord = leftCorner.coords;
				
				for (int x = 0; x < size.x; x++) {
					float xCenter = 495f * (x + 0.5f);
					if (xCenter - 27f >= leftCorner.coords.x && xCenter + 27f <= rightCorner.coords.x) {

						vertices.Add(new Vector3(lastLeftCoord.x, lastLeftCoord.y, 100));
						vertices.Add(new Vector3(xCenter - 27f, lastLeftCoord.y, 100));
						vertices.Add(new Vector3(xCenter - 27f, lastLeftCoord.y, -100));
						vertices.Add(new Vector3(lastLeftCoord.x, lastLeftCoord.y, -100));

						lastLeftCoord = new Vector2(xCenter + 27f, lastLeftCoord.y);

						/*vertices.Add(new Vector3(xCenter + 27f, leftCorner.coords.y, 100));
						vertices.Add(new Vector3(rightCorner.coords.x, leftCorner.coords.y, 100));
						vertices.Add(new Vector3(rightCorner.coords.x, leftCorner.coords.y, -100));
						vertices.Add(new Vector3(xCenter + 27f, leftCorner.coords.y, -100));*/

						for (int j = 0; j < 4; j++)
							uv.Add(new Vector2());
						triangles.AddRange(!toDown ? new int[] { i + 1, i, i + 3, i + 3, i + 2, i + 1 } : new int[] { i, i + 1, i + 3, i + 2, i + 3, i + 1 });
						//triangles.AddRange(!toDown ? new int[] { i + 5, i + 4, i + 7, i + 7, i + 6, i + 5 } : new int[] { i + 4, i + 5, i + 7, i + 6, i + 7, i + 5 });

						i += 4;
						fail = false;
						//break;
					}
				}

				if (!fail && (int) lastLeftCoord.x != rightCorner.coords.x) {
					vertices.Add(new Vector3(lastLeftCoord.x, lastLeftCoord.y, 100));
					vertices.Add(new Vector3(rightCorner.coords.x, lastLeftCoord.y, 100));
					vertices.Add(new Vector3(rightCorner.coords.x, lastLeftCoord.y, -100));
					vertices.Add(new Vector3(lastLeftCoord.x, lastLeftCoord.y, -100));
					
					for (int j = 0; j < 4; j++)
						uv.Add(new Vector2());
					triangles.AddRange(!toDown ? new int[] { i + 1, i, i + 3, i + 3, i + 2, i + 1 } : new int[] { i, i + 1, i + 3, i + 2, i + 3, i + 1 });
					i += 4;
				}
			}
			if (fail) {
				vertices.Add(new Vector3(leftCorner.coords.x, leftCorner.coords.y, 100));
				vertices.Add(new Vector3(rightCorner.coords.x, leftCorner.coords.y, 100));
				vertices.Add(new Vector3(rightCorner.coords.x, leftCorner.coords.y, -100));
				vertices.Add(new Vector3(leftCorner.coords.x, leftCorner.coords.y, -100));

				for (int j = 0; j < 4; j++)
					uv.Add(new Vector2());
				triangles.AddRange(!toDown ? new int[] { i + 1, i, i + 3, i + 3, i + 2, i + 1 } : new int[] { i, i + 1, i + 3, i + 2, i + 3, i + 1 });

				i += 4;
			}
			checkedCorners.Add(corner);
			checkedCorners.Add(corner.xConnection);
		}

		return new RoomMesh("shadowCornerMeshX", getMesh(vertices, uv, triangles.ToArray(), getNormals(vertices.Count)), null, 0);
	}

	private static RoomMesh generateCornerShadowMeshY(List<Corner> cornersFake, Vector2Int size) {
		List<Corner> corners = copyCornerList(cornersFake);
		corners.RemoveAll(x => !(x.IsExtremeX(size.x) || x.IsExtremeY(size.y)));

		List<Vector3> vertices = new List<Vector3>();
		List<Vector2> uv = new List<Vector2>();
		List<int> triangles = new List<int>();
		List<Corner> checkedCorners = new List<Corner>();

		int i = 0;
		foreach (Corner corner in corners) {
			corner.FindConnections(corners);
			if (checkedCorners.Contains(corner) || corner.yConnection == null)
				continue;
			bool toLeft = FacingUtils.GetFacingsFromCornerFacing(corner.cornerFacing)[0] == Facing.LEFT;
			bool connectionUpper = corner.yConnection.coords.y > corner.coords.y;
			Corner downCorner = connectionUpper ? corner : corner.yConnection;
			Corner upCorner = connectionUpper ? corner.yConnection : corner;

			bool fail = true;
			if (downCorner.coords.x == 0 && upCorner.coords.x == 0 || downCorner.coords.x == size.x * 495 && upCorner.coords.x == size.x * 495) {
				Vector2 lastDownCoord = downCorner.coords;
				
				for (int y = 0; y < size.y; y++) {
					float yCenter = 277f * (y + 0.5f);
					if (yCenter - 27f >= downCorner.coords.y && yCenter + 27f <= upCorner.coords.y) {

						vertices.Add(new Vector3(lastDownCoord.x, lastDownCoord.y, 100));
						vertices.Add(new Vector3(lastDownCoord.x, yCenter - 27f, 100));
						vertices.Add(new Vector3(lastDownCoord.x, yCenter - 27f, -100));
						vertices.Add(new Vector3(lastDownCoord.x, lastDownCoord.y, -100));

						lastDownCoord = new Vector2(lastDownCoord.x, yCenter + 27f);

						/*vertices.Add(new Vector3(downCorner.coords.x, yCenter + 27f, 100));
						vertices.Add(new Vector3(downCorner.coords.x, upCorner.coords.y, 100));
						vertices.Add(new Vector3(downCorner.coords.x, upCorner.coords.y, -100));
						vertices.Add(new Vector3(downCorner.coords.x, yCenter + 27f, -100));*/

						for (int j = 0; j < 4; j++)
							uv.Add(new Vector2());
						triangles.AddRange(toLeft ? new int[] { i + 1, i, i + 3, i + 3, i + 2, i + 1 } : new int[] { i, i + 1, i + 3, i + 2, i + 3, i + 1 });
						//triangles.AddRange(toLeft ? new int[] { i + 5, i + 4, i + 7, i + 7, i + 6, i + 5 } : new int[] { i + 4, i + 5, i + 7, i + 6, i + 7, i + 5 });

						i += 4;
						fail = false;
						//break;
					}
				}
				
				if (!fail && (int) lastDownCoord.y != upCorner.coords.y) {
					vertices.Add(new Vector3(lastDownCoord.x, lastDownCoord.y, 100));
					vertices.Add(new Vector3(lastDownCoord.x, upCorner.coords.y, 100));
					vertices.Add(new Vector3(lastDownCoord.x, upCorner.coords.y, -100));
					vertices.Add(new Vector3(lastDownCoord.x, lastDownCoord.y, -100));
					
					for (int j = 0; j < 4; j++)
						uv.Add(new Vector2());
					triangles.AddRange(toLeft ? new int[] { i + 1, i, i + 3, i + 3, i + 2, i + 1 } : new int[] { i, i + 1, i + 3, i + 2, i + 3, i + 1 });
					i += 4;
				}
			}
			if (fail) {
				vertices.Add(new Vector3(downCorner.coords.x, downCorner.coords.y, 100));
				vertices.Add(new Vector3(downCorner.coords.x, upCorner.coords.y, 100));
				vertices.Add(new Vector3(downCorner.coords.x, upCorner.coords.y, -100));
				vertices.Add(new Vector3(downCorner.coords.x, downCorner.coords.y, -100));

				for (int j = 0; j < 4; j++)
					uv.Add(new Vector2());
				triangles.AddRange(toLeft ? new int[] { i + 1, i, i + 3, i + 3, i + 2, i + 1 } : new int[] { i, i + 1, i + 3, i + 2, i + 3, i + 1 });

				i += 4;
			}
			checkedCorners.Add(corner);
			checkedCorners.Add(corner.yConnection);
		}

		return new RoomMesh("shadowCornerMeshY", getMesh(vertices, uv, triangles.ToArray(), getNormals(vertices.Count)), null, 0);
	}

	private static RoomMesh generateBordersY(List<Corner> cornersFake, Vector2Int size, float layer) {
		List<Corner> corners = copyCornerList(cornersFake);
		corners.RemoveAll(x => x.IsExtremeX(size.x) && x.IsExtremeY(size.y));

		List<Vector3> vertices = new List<Vector3>();
		List<Vector2> uv = new List<Vector2>();
		List<int> triangles = new List<int>();
		List<Corner> checkedCorners = new List<Corner>();

		int i = 0;
		foreach (Corner corner in corners) {
			corner.FindConnections(corners);

			if (checkedCorners.Contains(corner) || corner.yConnection == null || (!(corner is HiddenCorner) && (corner.IsExtremeCorner(size.x, size.y) && corner.yConnection.IsExtremeCorner(size.x, size.y))))
				continue;

			bool toLeft = FacingUtils.GetFacingsFromCornerFacing(corner.cornerFacing)[0] == Facing.LEFT;
			int offset = toLeft ? -9 : 9;
			bool connectionUpper = corner.yConnection.coords.y > corner.coords.y;
			Corner downCorner = connectionUpper ? corner : corner.yConnection;
			Corner upCorner = connectionUpper ? corner.yConnection : corner;

			if (!toLeft) {
				Corner buffer = downCorner;
				downCorner = upCorner;
				upCorner = buffer;
			}

			bool hiddenBorder = upCorner is HiddenCorner && downCorner is HiddenCorner && !(downCorner.IsExtremeX(size.x) && upCorner.IsExtremeX(size.x));
			float upCornerOffset = upCorner.convex && (!upCorner.IsExtremeCorner(size.x, size.y) || (upCorner is HiddenCorner && hiddenBorder)) ? -6 * (toLeft ? 1 : -1) : 0;
			float downCornerOffset = downCorner.convex && (!downCorner.IsExtremeCorner(size.x, size.y) || (downCorner is HiddenCorner && hiddenBorder)) ? 6 * (toLeft ? 1 : -1) : 0;
			vertices.Add(new Vector2(downCorner.coords.x, downCorner.coords.y + downCornerOffset));
			vertices.Add(new Vector2(upCorner.coords.x, upCorner.coords.y + upCornerOffset));
			vertices.Add(new Vector2(upCorner.coords.x + offset, upCorner.coords.y + upCornerOffset));
			vertices.Add(new Vector2(downCorner.coords.x + offset, downCorner.coords.y + downCornerOffset));

			int deltaY = Mathf.Abs(upCorner.coords.y - downCorner.coords.y);
			uv.Add(new Vector2(deltaY / 32f, 0.602f - (hiddenBorder ? 0.266f : 0f)));
			uv.Add(new Vector2(0f, 0.602f - (hiddenBorder ? 0.266f : 0f)));
			uv.Add(new Vector2(0f, 0.531f - (hiddenBorder ? 0.266f : 0f)));
			uv.Add(new Vector2(deltaY / 32f, 0.531f - (hiddenBorder ? 0.266f : 0f)));

			triangles.AddRange(new int[] { i, i + 3, i + 2, i, i + 2, i + 1 });

			i += 4;
			checkedCorners.Add(corner);
			checkedCorners.Add(corner.yConnection);
		}

		return new RoomMesh("bordersY", getMesh(vertices, uv, triangles.ToArray(), getNormals(vertices.Count)), borders[0], layer);
	}

	private static RoomMesh generateBordersX(List<Corner> cornersFake, Vector2Int size, float layer) {
		List<Corner> corners = copyCornerList(cornersFake);
		corners.RemoveAll(x => x.IsExtremeX(size.x) && x.IsExtremeY(size.y));

		List<Vector3> vertices = new List<Vector3>();
		List<Vector2> uv = new List<Vector2>();
		List<int> triangles = new List<int>();
		List<Corner> checkedCorners = new List<Corner>();

		int i = 0;
		foreach (Corner corner in corners) {
			corner.FindConnections(corners);
			if (checkedCorners.Contains(corner) || corner.xConnection == null || (!(corner is HiddenCorner) && (corner.IsExtremeCorner(size.x, size.y) && corner.xConnection.IsExtremeCorner(size.x, size.y))))
				continue;

			bool toDown = FacingUtils.GetFacingsFromCornerFacing(corner.cornerFacing)[1] == Facing.DOWN;
			int offset = toDown ? -9 : 9;
			bool connectionRighter = corner.xConnection.coords.x > corner.coords.x;
			Corner leftCorner = connectionRighter ? corner : corner.xConnection;
			Corner rightCorner = connectionRighter ? corner.xConnection : corner;

			if (!toDown) {
				Corner buffer = leftCorner;
				leftCorner = rightCorner;
				rightCorner = buffer;
			}

			bool hiddenBorder = leftCorner is HiddenCorner && rightCorner is HiddenCorner && !(leftCorner.IsExtremeY(size.y) && rightCorner.IsExtremeY(size.y));
			float leftCornerOffset = leftCorner.convex && (!leftCorner.IsExtremeCorner(size.x, size.y) || (leftCorner is HiddenCorner && hiddenBorder)) ? 6 * (toDown ? 1 : -1) : 0;
			float rightCornerOffset = rightCorner.convex && (!rightCorner.IsExtremeCorner(size.x, size.y) || (rightCorner is HiddenCorner && hiddenBorder)) ? -6 * (toDown ? 1 : -1) : 0;
			vertices.Add(new Vector2(rightCorner.coords.x + rightCornerOffset, rightCorner.coords.y));
			vertices.Add(new Vector2(leftCorner.coords.x + leftCornerOffset, leftCorner.coords.y));
			vertices.Add(new Vector2(leftCorner.coords.x + leftCornerOffset, leftCorner.coords.y + offset));
			vertices.Add(new Vector2(rightCorner.coords.x + rightCornerOffset, rightCorner.coords.y + offset));

			int deltaX = Mathf.Abs(rightCorner.coords.x - leftCorner.coords.x);
			uv.Add(new Vector2(deltaX / 32f, 0.602f - (hiddenBorder ? 0.266f : 0f)));
			uv.Add(new Vector2(0f, 0.602f - (hiddenBorder ? 0.266f : 0f)));
			uv.Add(new Vector2(0f, 0.531f - (hiddenBorder ? 0.266f : 0f)));
			uv.Add(new Vector2(deltaX / 32f, 0.531f - (hiddenBorder ? 0.266f : 0f)));

			triangles.AddRange(new int[] { i, i + 3, i + 2, i, i + 2, i + 1 });

			i += 4;
			checkedCorners.Add(corner);
			checkedCorners.Add(corner.xConnection);
		}

		return new RoomMesh("bordersX", getMesh(vertices, uv, triangles.ToArray(), getNormals(vertices.Count)), borders[0], layer);
	}

	private static RoomMesh generateCorners(List<Corner> cornersFake, Vector2Int size, float layer) {
		List<Corner> corners = copyCornerList(cornersFake);
		corners.RemoveAll(x => x.IsExtremeCorner(size.x, size.y));

		List<Vector3> vertices = new List<Vector3>();
		List<Vector2> uv = new List<Vector2>();
		List<int> triangles = new List<int>();

		int j = 0;
		foreach (Corner corner in corners) {
			float k = corner.convex ? 3f : 4.5f;
			Vector2[] vectors = new Vector2[] {
				new Vector2(k, k),
				new Vector2(-k, k),
				new Vector2(-k, -k),
				new Vector2(k, -k)
			};

			float rotationAngle = corner.cornerFacing == CornerFacing.RIGHT_DOWN ? Mathf.PI / 2f : corner.cornerFacing == CornerFacing.RIGHT_UP ? Mathf.PI : corner.cornerFacing == CornerFacing.LEFT_UP ? 3f * Mathf.PI / 2f : 0;
			for (int i = 0; i < vectors.Length; i++)
				vectors[i] = new Vector2(
					vectors[i].x * Mathf.Cos(rotationAngle) - vectors[i].y * Mathf.Sin(rotationAngle) + corner.coords.x + k * FacingUtils.GetVector(corner.cornerFacing).x,
					vectors[i].x * Mathf.Sin(rotationAngle) + vectors[i].y * Mathf.Cos(rotationAngle) + corner.coords.y + k * FacingUtils.GetVector(corner.cornerFacing).y
					);

			foreach (Vector2 vector in vectors)
				vertices.Add(new Vector2(vector.x, vector.y));

			bool hiddenCorner = corner is HiddenCorner;
			Vector4 uvVec = corner.convex ?
				new Vector4(0.344f, 0.531f, 0.375f + (hiddenCorner ? 0.086f : 0f), 0.422f + (hiddenCorner ? 0.086f : 0f)) :
				new Vector4(0, 0.281f, 0.352f + (hiddenCorner ? 0.086f : 0f), 0.422f + (hiddenCorner ? 0.086f : 0f)); // x0, x1, y0, y1 

			uv.Add(new Vector2(uvVec.y, uvVec.w));
			uv.Add(new Vector2(uvVec.x, uvVec.w));
			uv.Add(new Vector2(uvVec.x, uvVec.z));
			uv.Add(new Vector2(uvVec.y, uvVec.z));

			triangles.AddRange(new int[] { j + 2, j, j + 3, j + 1, j, j + 2 });
			j += 4;
		}

		return new RoomMesh("corners", getMesh(vertices, uv, triangles.ToArray(), getNormals(vertices.Count)), borders[0], layer);
	}

	private static RoomMesh generateWall(List<Corner> corners, float layer, bool hidden) {
		foreach (Corner corner in corners) {
			corner.FindConnections(corners);

			if (GameSettings.SettingVisualizeMeshGeneration.Value)
				MonoBehaviour.Instantiate(GameManager.singleton.PointDebugObject, new Vector3(corner.coords.x, corner.coords.y, -1), new Quaternion());
		}

		List<Vector3> vertices = new List<Vector3>();
		List<Vector2> uv = new List<Vector2>();

		foreach (Corner corner in corners) {
			vertices.Add(new Vector2(corner.coords.x, corner.coords.y));
			uv.Add(new Vector2(corner.coords.x / 64f, corner.coords.y / 64f));
		}

		return new RoomMesh(hidden ? "hiddenWall" : "wall", getMesh(vertices, uv, triangleConnectedCorners(corners), getNormals(vertices.Count)), walls[0], layer);
	}

	private static RoomMesh generateHiddenBackground(List<HiddenCorner> corners, float layer) {
		foreach (Corner corner in corners) {
			corner.FindConnections(new List<Corner>(corners.Cast<Corner>()));

			if (GameSettings.SettingVisualizeMeshGeneration.Value)
				MonoBehaviour.Instantiate(GameManager.singleton.PointDebugObject, new Vector3(corner.coords.x, corner.coords.y, -1), new Quaternion());
		}

		List<Vector3> vertices = new List<Vector3>();
		List<Vector2> uv = new List<Vector2>();

		foreach (Corner corner in corners) {
			if (corner.IsExtremeX(0))
				corner.coords += new Vector2Int(FacingUtils.GetVector(corner.cornerFacing).x * 6, 0);
			else if (corner.IsExtremeY(0))
				corner.coords += new Vector2Int(0, FacingUtils.GetVector(corner.cornerFacing).y * 6);

			vertices.Add(new Vector2(corner.coords.x, corner.coords.y));
			uv.Add(new Vector2(corner.coords.x / 64f, corner.coords.y / 64f));
		}

		return new RoomMesh("hiddenBackground", getMesh(vertices, uv, triangleConnectedCorners(new List<Corner>(corners.Cast<Corner>())), getNormals(vertices.Count)), hiddenBackgrounds[0], layer);
	}

	private static RoomMesh generateRoomBackground(Vector2Int size, int location, float layer) {
		List<Vector3> vertices = new List<Vector3>();
		List<Vector2> uv = new List<Vector2>();
		List<int> triangles = new List<int>();

		for (int x = 0; x < size.x; x++)
			for (int y = 0; y < size.y; y++) {
				int offset = 36 * x + (36 * size.x) * y;

				for (int i = 0; i < 3; i++)
					for (int j = 0; j < 3; j++) {
						vertices.Add(new Vector3(165 * i + x * 495, 277 - j * 125 + y * 277, 0));
						vertices.Add(new Vector3(165 * i + x * 495, j == 2 ? y * 277 : 152 - j * 125 + y * 277, 0));
						vertices.Add(new Vector3(165 * (i + 1) + x * 495, j == 2 ? y * 277 : 152 - j * 125 + y * 277, 0));
						vertices.Add(new Vector3(165 * (i + 1) + x * 495, 277 - j * 125 + y * 277, 0));

						triangles.Add(12 * i + 4 * j + offset);
						triangles.Add(12 * i + 4 * j + offset + 3);
						triangles.Add(12 * i + 4 * j + offset + 1);

						triangles.Add(12 * i + 4 * j + offset + 1);
						triangles.Add(12 * i + 4 * j + offset + 3);
						triangles.Add(12 * i + 4 * j + offset + 2);

						uv.Add(new Vector2(0, 0.244f));
						uv.Add(new Vector2(0, j == 2 ? 0.191f : 0));
						uv.Add(new Vector2(0.161f, j == 2 ? 0.191f : 0));
						uv.Add(new Vector2(0.161f, 0.244f));
					}
			}

		return new RoomMesh("background", getMesh(vertices, uv, triangles.ToArray(), getNormals(vertices.Count)), backgrounds[0], layer);
	}

	private static void generateHiddenEntryBorders(List<HiddenCorner> corners) {
		foreach (Corner corner in corners)
			corner.FindConnections(new List<Corner>(corners.Cast<Corner>()));

		foreach (Corner corner in corners) {
			if (corner.IsExtremeX(0) && corner.yConnection is HiddenCorner && corner.yConnection.IsExtremeX(0) && corner.coords.y > corner.yConnection.coords.y) {
				addHiddenBorder(corner.coords, corner.coords.y - corner.yConnection.coords.y, false, FacingUtils.GetFacingsFromCornerFacing(corner.cornerFacing)[0] == Facing.RIGHT);
			}
			else if (corner.IsExtremeY(0) && corner.xConnection is HiddenCorner && corner.xConnection.IsExtremeY(0) && corner.coords.x < corner.xConnection.coords.x) {
				addHiddenBorder(corner.coords, corner.coords.x - corner.xConnection.coords.x, true, FacingUtils.GetFacingsFromCornerFacing(corner.cornerFacing)[1] == Facing.UP);
			}
		}
	}

	private static Vector2[][] generateCollider(List<Corner> corners) {
		List<List<Corner>> groups = new List<List<Corner>>();
		List<Corner> checkedCorners = new List<Corner>();
		foreach (Corner corner in corners) {
			corner.FindConnections(corners);

			//Debug
			if (GameSettings.SettingVisualizeColliders.Value) {
				MonoBehaviour.Instantiate(GameManager.singleton.PointDebugObject, new Vector3(corner.coords.x, corner.coords.y, -0.1f),
					new Quaternion());
				if (corner.xConnection != null) {
					LineRenderer render = MonoBehaviour.Instantiate(GameManager.singleton.LineDebugObject).GetComponent<LineRenderer>();
					render.SetPositions(new Vector3[] {
						new Vector3(corner.coords.x, corner.coords.y, -0.1f),
						new Vector3(corner.xConnection.coords.x, corner.xConnection.coords.y, -0.1f)
					});
				}

				if (corner.yConnection != null) {
					LineRenderer render = MonoBehaviour.Instantiate(GameManager.singleton.LineDebugObject).GetComponent<LineRenderer>();
					render.SetPositions(new Vector3[] {
						new Vector3(corner.coords.x, corner.coords.y, -0.1f),
						new Vector3(corner.yConnection.coords.x, corner.yConnection.coords.y, -0.1f)
					});
				}
			}
		}

		foreach (Corner corner in corners) {
			if (checkedCorners.Contains(corner))
				continue;

			checkedCorners.Add(corner);
			groups.Add(new List<Corner>());
			groups[groups.Count - 1].Add(corner);
			Corner lastCorner = corner.xConnection;
			bool toggle = false;
			int count = 0;
			while (lastCorner != corner) {
				count++;
				checkedCorners.Add(lastCorner);
				groups[groups.Count - 1].Add(lastCorner);
				lastCorner = toggle ? lastCorner.xConnection : lastCorner.yConnection;
				toggle = !toggle;
				if (count > 999)
					throw new Exception("Бесконечный цикл в generateCollider методе");
			}
		}
		Vector2[][] result = new Vector2[groups.Count][];
		for (int i = 0; i < groups.Count; i++)
			result[i] = groups[i].ConvertAll<Vector2>(x => x.coords).ToArray();

		return result;
	}

	private static List<RoomObject> generateObjects(List<RoomSerializeHelper.RoomObject> objects) {
		List<RoomObject> result = new List<RoomObject>();
		foreach (RoomSerializeHelper.RoomObject obj in objects)
			result.Add(new RoomObject(obj.prefabName, new Vector2(obj.coords.x, obj.coords.y), obj.ID, obj.mirror.x, obj.mirror.y, obj.type, obj.data));
		result.AddRange(objectsToAddToNextRoom);
		return result;
	}
	#endregion

	#region Corners utils

	private static List<Corner> removePreGateCorners(List<Corner> list) {
		float TOLERANCE = 0.01f;
		list.RemoveAll(x => 
			(x.coords.y == 23 && (Math.Abs(((x.coords.x+27)/247f)%1) < TOLERANCE || Math.Abs(((x.coords.x-28)/247f)%1) < TOLERANCE) && FacingUtils.GetVector(x.cornerFacing).y == -1) ||
			(x.coords.y == 254 && (Math.Abs(((x.coords.x+27)/247f)%1) < TOLERANCE || Math.Abs(((x.coords.x-28)/247f)%1) < TOLERANCE) && FacingUtils.GetVector(x.cornerFacing).y == 1) ||
			(x.coords.x == 45 && (Math.Abs(((x.coords.y+27)/138f)%1) < TOLERANCE || Math.Abs(((x.coords.y-28)/138f)%1) < TOLERANCE) && FacingUtils.GetVector(x.cornerFacing).x == -1) ||
			(x.coords.x == 450 && (Math.Abs(((x.coords.y+27)/138f)%1) < TOLERANCE || Math.Abs(((x.coords.y-28)/138f)%1) < TOLERANCE) && FacingUtils.GetVector(x.cornerFacing).x == 1)
		);
		return list;
	}

	private static List<Corner> getGatesCorners(Vector2Int roomSize, List<Corner> roomCorners, List<RoomSerializeHelper.RoomObject> gates) {
		List<Corner> corners = new List<Corner>();
		for (int x = 0; x < roomSize.x; x++) {
			float xCenter = 495f*(x + 0.5f);
		
			// Down gate
			RoomSerializeHelper.RoomObject downGate = gates.Find(gate => Math.Abs(gate.coords.x - (xCenter - 33.5f)) < 0.1f && Math.Abs(gate.coords.y) < 0.1f);
			if (int.Parse(downGate.data[0]) != 2) {
				Corner leftUpCorner = new Corner(new Vector2Int((int)(xCenter - 27f), 23), CornerFacing.LEFT_DOWN, true);
				Corner rightUpCorner = new Corner(new Vector2Int((int)(xCenter + 28f), 23), CornerFacing.RIGHT_DOWN, true);
				leftUpCorner.FindConnections(roomCorners);
				rightUpCorner.FindConnections(roomCorners);
				if (leftUpCorner.xConnection != null)
					corners.Add(leftUpCorner);
				if (rightUpCorner.xConnection != null)
					corners.Add(rightUpCorner);
				corners.Add(new Corner(new Vector2Int((int)(xCenter - 27f), 0), CornerFacing.LEFT_UP, true));
				corners.Add(new Corner(new Vector2Int((int)(xCenter + 28f), 0), CornerFacing.RIGHT_UP, true));
			}

			// Up gate
			RoomSerializeHelper.RoomObject upGate = gates.Find(gate => Math.Abs(gate.coords.x - (xCenter - 33.5f)) < 0.1f && Math.Abs(roomSize.y * 277 - 23 - gate.coords.y) < 0.1f);
			if (int.Parse(upGate.data[0]) != 2){
				Corner leftDownCorner = new Corner(new Vector2Int((int)(xCenter - 27f), roomSize.y * 277 - 23), CornerFacing.LEFT_UP, true);
				Corner rightDownCorner = new Corner(new Vector2Int((int)(xCenter + 28f), roomSize.y * 277 - 23), CornerFacing.RIGHT_UP, true);
				leftDownCorner.FindConnections(roomCorners);
				rightDownCorner.FindConnections(roomCorners);
				if (leftDownCorner.xConnection != null)
					corners.Add(leftDownCorner);
				if (rightDownCorner.xConnection != null)
					corners.Add(rightDownCorner);
				corners.Add(new Corner(new Vector2Int((int)(xCenter - 27f), roomSize.y * 277), CornerFacing.LEFT_DOWN, true));
				corners.Add(new Corner(new Vector2Int((int)(xCenter + 28f), roomSize.y * 277), CornerFacing.RIGHT_DOWN, true));
			}
		}
		for (int y = 0; y < roomSize.y; y++) {
			float yCenter = 277f * (y + 0.5f);
			
			// Left gate
			RoomSerializeHelper.RoomObject leftGate = gates.Find(gate => Math.Abs(gate.coords.x) < 0.1f && Math.Abs(gate.coords.y - (yCenter - 33.5f)) < 0.1f);
			if (int.Parse(leftGate.data[0]) != 2) {
				Corner downRightCorner = new Corner(new Vector2Int(45, (int)(yCenter - 27f)), CornerFacing.LEFT_DOWN, true);
				Corner upRightCorner = new Corner(new Vector2Int(45, (int)(yCenter + 28f)), CornerFacing.LEFT_UP, true);
				downRightCorner.FindConnections(roomCorners);
				upRightCorner.FindConnections(roomCorners);
				if (downRightCorner.yConnection != null)
					corners.Add(downRightCorner);
				if (upRightCorner.yConnection != null)
					corners.Add(upRightCorner);
				corners.Add(new Corner(new Vector2Int(0, (int)(yCenter - 27f)), CornerFacing.RIGHT_DOWN, true));
				corners.Add(new Corner(new Vector2Int(0, (int)(yCenter + 28f)), CornerFacing.RIGHT_UP, true));
			}
			
			// Right gate
			RoomSerializeHelper.RoomObject rightGate = gates.Find(gate => Math.Abs(495 * roomSize.x - 45 - gate.coords.x) < 0.1f && Math.Abs(gate.coords.y - (yCenter - 33.5f)) < 0.1f);
			if (int.Parse(rightGate.data[0]) != 2){
				Corner downLeftCorner = new Corner(new Vector2Int(roomSize.x * 495 - 45, (int)(yCenter - 27f)), CornerFacing.RIGHT_DOWN, true);
				Corner upLeftCorner = new Corner(new Vector2Int(roomSize.x * 495 - 45, (int)(yCenter + 28f)), CornerFacing.RIGHT_UP, true);
				downLeftCorner.FindConnections(roomCorners);
				upLeftCorner.FindConnections(roomCorners);
				if (downLeftCorner.yConnection != null)
					corners.Add(downLeftCorner);
				if (upLeftCorner.yConnection != null)
					corners.Add(upLeftCorner);
				corners.Add(new Corner(new Vector2Int(roomSize.x * 495, (int)(yCenter - 27f)), CornerFacing.LEFT_DOWN, true));
				corners.Add(new Corner(new Vector2Int(roomSize.x * 495, (int)(yCenter + 28f)), CornerFacing.LEFT_UP, true));
			}
		}

		return corners;
	}

	private static List<Corner> getBarrierCorners(List<HiddenCorner> corners) {
		List<Corner> result = new List<Corner>();
		foreach (HiddenCorner corner in corners) {
			HiddenCorner copy = (HiddenCorner)corner.Copy();
			copy.coords += !copy.IsExtremeCorner(0, 0) ? new Vector2Int(-FacingUtils.GetVector(copy.cornerFacing).x * 9, -FacingUtils.GetVector(copy.cornerFacing).y * 9) : copy.IsExtremeX(0) ? new Vector2Int(FacingUtils.GetVector(copy.cornerFacing).x * 6, -FacingUtils.GetVector(copy.cornerFacing).y * 9) : new Vector2Int(-FacingUtils.GetVector(copy.cornerFacing).x * 9, FacingUtils.GetVector(copy.cornerFacing).y * 6);
			result.Add(copy);
		}
		return result;
	}

	private static List<Corner> copyCornerList(List<Corner> corners) {
		List<Corner> result = new List<Corner>();
		foreach (Corner corner in corners)
			result.Add(corner);
		return result;
	}

	private static List<HiddenCorner> findHiddenCorners(RoomSerializeHelper.IntArray[] matrix) {
		List<HiddenCorner> result = new List<HiddenCorner>();
		for (int x = 1; x < matrix.Length - 1; x++)
			for (int y = 1; y < matrix[0].array.Length - 1; y++) {
				if (matrix[x].array[y] == 2)
					continue;

				foreach (CornerFacing cornerFacing in FacingUtils.cornerFacings) {
					Vector2Int vector = FacingUtils.GetVector(cornerFacing);
					if (matrix[x + vector.x].array[y + vector.y] != 2)
						continue;

					int xType = matrix[x + vector.x].array[y];
					int yType = matrix[x].array[y + vector.y];

					bool convex = (xType != 2 && yType != 2);
					bool notConvex = (xType == 2 && yType == 2);
					if (convex || notConvex)
						result.Add(new HiddenCorner(new Vector2Int(x + (vector.x == -1 ? 0 : 1), y + (vector.y == -1 ? 0 : 1)), cornerFacing, convex, yType == 0, xType == 0));
				}
			}
		return result;
	}

	private static List<HiddenCorner> inverseHiddenCorners(List<HiddenCorner> corners) {
		List<HiddenCorner> copy = new List<HiddenCorner>();
		foreach (HiddenCorner corner in corners)
			copy.Add((HiddenCorner)corner.Copy());

		foreach (HiddenCorner corner in copy) {
			corner.cornerFacing = !corner.IsExtremeCorner(0, 0) ? FacingUtils.NegativeFacing(corner.cornerFacing) : corner.IsExtremeX(0) ? corner.cornerFacing = FacingUtils.NegativeFacingY(corner.cornerFacing) : corner.cornerFacing = FacingUtils.NegativeFacingX(corner.cornerFacing);
			corner.convex = !corner.IsExtremeCorner(0, 0) ? !corner.convex : corner.convex;
		}
		return copy;
	}

	private static List<Corner> findCorners(RoomSerializeHelper.IntArray[] matrix) {
		List<Corner> result = new List<Corner>();
		for (int x = -1; x < matrix.Length + 1; x++)
			for (int y = -1; y < matrix[0].array.Length + 1; y++) {
				try {
					if (matrix[x].array[y] != 0)
						continue;
				}
				catch (IndexOutOfRangeException e) { }

				foreach (CornerFacing cornerFacing in FacingUtils.cornerFacings) {
					Vector2Int vector = FacingUtils.GetVector(cornerFacing);
					try {
						if (matrix[x + vector.x].array[y + vector.y] == 0)
							continue;
					}
					catch (IndexOutOfRangeException e) { continue; }

					int xType;
					int yType;
					try {
						xType = matrix[x + vector.x].array[y];
					}
					catch (IndexOutOfRangeException e) { xType = 0; }
					try {
						yType = matrix[x].array[y + vector.y];
					}
					catch (IndexOutOfRangeException e) { yType = 0; }

					bool convex = (xType == 0 && yType == 0);
					bool notConvex = (xType != 0 && yType != 0);
					if (convex || notConvex)
						result.Add(new Corner(new Vector2Int(x + (vector.x == -1 ? 0 : 1), y + (vector.y == -1 ? 0 : 1)), cornerFacing, convex));
				}
			}
		return result;
	}

	private static List<Corner> getCuttedCorners(Vector2Int roomSize, List<Corner> corners) {
		List<Corner> result = new List<Corner>();
		foreach (Corner corner in corners) {
			Corner copy = corner.Copy();
			if (!copy.IsExtremeCorner(roomSize.x, roomSize.y))
				copy.coords += FacingUtils.GetVector(copy.cornerFacing) * 6;
			else if (copy.IsExtremeX(roomSize.x) && !copy.IsExtremeY(roomSize.y))
				copy.coords += FacingUtils.GetVector(FacingUtils.GetFacingsFromCornerFacing(copy.cornerFacing)[1]) * 6;
			else if (!copy.IsExtremeX(roomSize.x) && copy.IsExtremeY(roomSize.y))
				copy.coords += FacingUtils.GetVector(FacingUtils.GetFacingsFromCornerFacing(copy.cornerFacing)[0]) * 6;
			result.Add(copy);
		}

		return result;
	}

	private static List<HiddenCorner> getCuttedCorners(List<HiddenCorner> corners) {
		List<HiddenCorner> result = new List<HiddenCorner>();
		foreach (HiddenCorner corner in corners) {
			HiddenCorner copy = (HiddenCorner)corner.Copy();
			if (!copy.IsExtremeCorner(0, 0))
				copy.coords += FacingUtils.GetVector(copy.cornerFacing) * 6;
			else if (copy.IsExtremeX(0) && !copy.IsExtremeY(0))
				copy.coords += FacingUtils.GetVector(FacingUtils.GetFacingsFromCornerFacing(copy.cornerFacing)[1]) * 6;
			else if (!copy.IsExtremeX(0) && copy.IsExtremeY(0))
				copy.coords += FacingUtils.GetVector(FacingUtils.GetFacingsFromCornerFacing(copy.cornerFacing)[0]) * 6;
			result.Add(copy);
		}

		return result;
	}
	#endregion

	#region Extra methods
	private static string[] fixJsons(string[] jsons) {
		for (int i = 0; i < jsons.Length; i++) {
			jsons[i] = jsons[i].Replace("[[", "[{\"array\":[");
			jsons[i] = jsons[i].Replace("],[", "]},{\"array\":[");
			jsons[i] = jsons[i].Replace("1]]", "1]}]");
		}
		return jsons;
	}

	private static Mesh getMesh(List<Vector3> vertices, List<Vector2> uv, int[] triangles, Vector3[] normals) {
		Mesh result = new Mesh();
		result.SetVertices(vertices);
		result.SetUVs(0, uv);
		result.triangles = triangles;
		result.normals = getNormals(vertices.Count);
		result.RecalculateBounds();
		result.RecalculateNormals();
		result.RecalculateTangents();
		return result;
	}

	private static void addHiddenBorder(Vector2Int coords, int length, bool horizontal, bool mirror) {
		objectsToAddToNextRoom.Add(new RoomObject("hiddenEntryBorder", new Vector3(coords.x, coords.y, -0.009f), GameManager.rnd.Next(), false, mirror, 0, new string[] { Mathf.Abs(length) + "", horizontal + "" }));
	}

	public static int[] triangleConnectedCorners(List<Corner> corners) {
		List<Line> allLines = new List<Line>();

		foreach (Corner corner in corners) {
			Line xLine = new Line(corner, corner.xConnection);
			Line yLine = new Line(corner, corner.yConnection);
			if (!allLines.Contains(xLine))
				allLines.Add(xLine);
			if (!allLines.Contains(yLine))
				allLines.Add(yLine);
		}
		foreach (Corner corner0 in corners)
			foreach (Corner corner1 in corners) {
				Line line = new Line(corner0, corner1);
				if (corner0 != corner1 && !allLines.Contains(line) && isLineInFigure(corner0, corner1.coords) && isNotCrossLines(allLines, corner0, corner1))
					allLines.Add(line);
			}

		if (GameSettings.SettingVisualizeMeshGeneration.Value)
			foreach (Line line in allLines) {
				LineRenderer render = MonoBehaviour.Instantiate(GameManager.singleton.LineDebugObject).GetComponent<LineRenderer>();
				render.SetPositions(new Vector3[] { new Vector3(line.start.coords.x, line.start.coords.y, -1), new Vector3(line.end.coords.x, line.end.coords.y, -1) });
			}

		List<Triangle> trianglesList = new List<Triangle>();
		foreach (Corner corner in corners) {
			List<Line> linesWithCorner = getLinesWithCorner(allLines, corner);
			foreach (Line line0 in linesWithCorner)
				foreach (Line line1 in linesWithCorner) {
					if (line0.Equals(line1) || !allLines.Contains(new Line(line0.end, line1.end)))
						continue;

					bool toggle = corner.coords.x == line0.end.coords.x ? (corner.coords.y > line0.end.coords.y ? line1.end.coords.x > line0.end.coords.x :
						line1.end.coords.x < line0.end.coords.x) : corner.coords.y == line0.end.coords.y ? (corner.coords.x > line0.end.coords.x ? line1.end.coords.y < line0.end.coords.y : line1.end.coords.y > line0.end.coords.y) :
						corner.coords.x < line0.end.coords.x ? line1.end.coords.y - corner.coords.y > (line0.end.coords.y - corner.coords.y) / (float)(line0.end.coords.x - corner.coords.x) * (line1.end.coords.x - corner.coords.x) :
						line1.end.coords.y - corner.coords.y < (line0.end.coords.y - corner.coords.y) / (float)(line0.end.coords.x - corner.coords.x) * (line1.end.coords.x - corner.coords.x);

					Triangle triangle = new Triangle(new int[] { corners.IndexOf(toggle ? line0.end : corner), corners.IndexOf(toggle ? corner : line0.end), corners.IndexOf(line1.end) });
					if (!trianglesList.Contains(triangle))
						trianglesList.Add(triangle);
				}
		}

		List<int> triangles = new List<int>();
		foreach (Triangle triangle in trianglesList) {
			//debugTriangle(corners, triangle);
			triangles.Add(triangle.indexes[0]);
			triangles.Add(triangle.indexes[1]);
			triangles.Add(triangle.indexes[2]);
		}
		return triangles.ToArray();
	}

	private static void debugTriangle(List<Corner> corners, Triangle tringle) {
		GameObject obj = new GameObject("triangle");
		obj.AddComponent<MeshRenderer>().material = GameManager.singleton.DefaultMaterial;
		Mesh mesh = new Mesh();
		Vector3[] vecs = new Vector3[3];
		for (int i = 0; i < 3; i++)
			vecs[i] = new Vector3(corners[tringle.indexes[i]].coords.x, corners[tringle.indexes[i]].coords.y, -0.1f);
		mesh.vertices = vecs;
		mesh.triangles = new int[] { 0, 1, 2 };
		mesh.normals = new Vector3[]{ Vector3.back, Vector3.back, Vector3.back };
		obj.AddComponent<MeshFilter>().mesh = mesh;
	}

	public static Vector3[] getNormals(int count) {
		Vector3[] normals = new Vector3[count];
		for (int i = 0; i < normals.Length; i++)
			normals[i] = Vector3.back;
		return normals;
	}

	private static List<Line> getLinesWithCorner(List<Line> lines, Corner corner) {
		List<Line> result = new List<Line>();
		foreach (Line line in lines)
			if (line.start == corner || line.end == corner) {
				Line toAdd = line;
				if (toAdd.end == corner) {
					Corner buffer = toAdd.end;
					toAdd.end = toAdd.start;
					toAdd.start = buffer;
				}
				result.Add(toAdd);
			}
		return result;
	}

	private static bool isNotCrossLines(List<Line> lines, Corner start, Corner end) {
		foreach (Line line in lines)
			if (isLineCross(line.start.coords, line.end.coords, start.coords, end.coords))
				return false;
		return true;
	}

	private static bool isLineInFigure(Corner corner, Vector2Int lineTo) {
		Vector3 normalized = new Vector3(lineTo.x - corner.coords.x, lineTo.y - corner.coords.y, 0).normalized;
		float rotationAngle = corner.cornerFacing == CornerFacing.RIGHT_DOWN ? 3f * Mathf.PI / 2f : corner.cornerFacing == CornerFacing.RIGHT_UP ? 0 : corner.cornerFacing == CornerFacing.LEFT_UP ? Mathf.PI / 2f : Mathf.PI;
		Vector3 lineOne = new Vector3(Mathf.Cos(rotationAngle), Mathf.Sin(rotationAngle)) * (corner.convex ? 1 : -1);
		Vector3 lineTwo = new Vector3(-Mathf.Sin(rotationAngle), Mathf.Cos(rotationAngle)) * (corner.convex ? 1 : -1);

		float angleBetweenOne = Vector3.Angle(normalized, lineOne);
		float angleBetweenTwo = Vector3.Angle(normalized, lineTwo);

		return corner.convex ? angleBetweenOne < 90 && angleBetweenTwo < 90 : angleBetweenOne > 90 || angleBetweenTwo > 90;
	}

	private static bool isLineCross(Vector2 start1, Vector2 end1, Vector2 start2, Vector2 end2) {
		if (start1 == start2 && end1 == end2 || start1 == end2 && end1 == start2)
			return true;

		Vector2 dir1 = end1 - start1;
		Vector2 dir2 = end2 - start2;

		//считаем уравнения прямых проходящих через отрезки
		float a1 = -dir1.y;
		float b1 = +dir1.x;
		float d1 = -(a1 * start1.x + b1 * start1.y);

		float a2 = -dir2.y;
		float b2 = +dir2.x;
		float d2 = -(a2 * start2.x + b2 * start2.y);

		//подставляем концы отрезков, для выяснения в каких полуплоскотях они
		float seg1_line2_start = a2 * start1.x + b2 * start1.y + d2;
		float seg1_line2_end = a2 * end1.x + b2 * end1.y + d2;

		float seg2_line1_start = a1 * start2.x + b1 * start2.y + d1;
		float seg2_line1_end = a1 * end2.x + b1 * end2.y + d1;

		//если концы одного отрезка имеют один знак, значит он в одной полуплоскости и пересечения нет.
		if (seg1_line2_start * seg1_line2_end > 0 || seg2_line1_start * seg2_line1_end >= 0)
			return false;

		return true;
	}


	#endregion

	#region Extra classes
	public enum CornerFacing {
		LEFT_UP,
		LEFT_DOWN,
		RIGHT_UP,
		RIGHT_DOWN
	}

	public enum Facing {
		LEFT,
		UP,
		RIGHT,
		DOWN
	}

	public static class FacingUtils {
		public static CornerFacing[] cornerFacings = new CornerFacing[] { CornerFacing.LEFT_UP, CornerFacing.LEFT_DOWN, CornerFacing.RIGHT_UP, CornerFacing.RIGHT_DOWN };
		public static Facing[] facings = new Facing[] { Facing.LEFT, Facing.UP, Facing.RIGHT, Facing.DOWN };

		public static Facing NegativeFacing(Facing facing) {
			return facing == Facing.DOWN ? Facing.UP : facing == Facing.UP ? Facing.DOWN : facing == Facing.LEFT ? Facing.RIGHT : Facing.LEFT;
		}

		public static CornerFacing NegativeFacing(CornerFacing facing) {
			return facing == CornerFacing.LEFT_DOWN ? CornerFacing.RIGHT_UP : facing == CornerFacing.RIGHT_UP ? CornerFacing.LEFT_DOWN : facing == CornerFacing.RIGHT_DOWN ? CornerFacing.LEFT_UP : CornerFacing.RIGHT_DOWN;
		}

		public static CornerFacing NegativeFacingX(CornerFacing facing) {
			Facing[] buffer = GetFacingsFromCornerFacing(facing);
			return GetCornerFacingFormFacings(NegativeFacing(buffer[0]), buffer[1]);
		}

		public static CornerFacing NegativeFacingY(CornerFacing facing) {
			Facing[] buffer = GetFacingsFromCornerFacing(facing);
			return GetCornerFacingFormFacings(buffer[0], NegativeFacing(buffer[1]));
		}

		public static Facing[] GetFacingsFromCornerFacing(CornerFacing corner) {
			return new Facing[] {
				corner == CornerFacing.LEFT_DOWN || corner == CornerFacing.LEFT_UP ? Facing.LEFT : Facing.RIGHT,
				corner == CornerFacing.LEFT_UP || corner == CornerFacing.RIGHT_UP ? Facing.UP : Facing.DOWN
			};
		}

		public static CornerFacing GetCornerFacingFormFacings(Facing facing0, Facing facing1) {
			if ((facing0 == Facing.DOWN || facing0 == Facing.UP) && (facing1 == Facing.LEFT || facing1 == Facing.RIGHT)) {
				Facing buffer = facing0;
				facing0 = facing1;
				facing1 = buffer;
			}

			return facing0 == Facing.LEFT ? (facing1 == Facing.DOWN ? CornerFacing.LEFT_DOWN : CornerFacing.LEFT_UP) : (facing1 == Facing.DOWN ? CornerFacing.RIGHT_DOWN : CornerFacing.RIGHT_UP);
		}

		public static Vector2Int GetVector(Facing facing) {
			return new Vector2Int(
				facing == Facing.LEFT ? -1 : facing == Facing.RIGHT ? 1 : 0,
				facing == Facing.DOWN ? -1 : facing == Facing.UP ? 1 : 0
			);
		}

		public static Vector2Int GetVector(CornerFacing facing) {
			return new Vector2Int(
				facing == CornerFacing.LEFT_UP || facing == CornerFacing.LEFT_DOWN ? -1 : 1,
				facing == CornerFacing.LEFT_DOWN || facing == CornerFacing.RIGHT_DOWN ? -1 : 1
			);
		}
	}

	public struct Triangle {
		public int[] indexes;

		public Triangle(int[] indexes) {
			this.indexes = indexes;
		}

		public override bool Equals(object obj) {
			int[] m = ((Triangle)obj).indexes;
			bool yes = true;
			for (int i = 0; i < 3; i++) {
				bool yes0 = false;
				for (int j = 0; j < 3; j++)
					if (indexes[i] == m[j])
						yes0 = true;
				if (!yes0)
					yes = false;
			}
			return yes;
		}

		public override int GetHashCode() {
			return indexes[0] + indexes[1] + indexes[2];
		}
	}

	public struct Line {
		public Corner start;
		public Corner end;

		public Line(Corner start, Corner end) {
			this.start = start;
			this.end = end;
		}

		public override bool Equals(object obj) {
			if (obj is Line) {
				Line line = (Line)obj;
				return (line.start == start && line.end == end) || (line.start == end && line.end == start);
			}
			else
				return false;
		}

		public override int GetHashCode() {
			return start.GetHashCode() + start.GetHashCode();
		}
	}

	public class Room {
		public string fileName;
		public Vector2Int size;
		public List<RoomMesh> meshes;
		public List<RoomObject> objects;
		public Vector2[][] colliderPoints;
		public int location;

		public Room(string fileName, Vector2Int size, List<RoomMesh> meshes, List<RoomObject> objects, Vector2[][] colliderPoints, int location) {
			this.fileName = fileName;
			this.size = size;
			this.meshes = meshes;
			this.objects = objects;
			this.colliderPoints = colliderPoints;
			this.location = location;
		}
	}

	public class RoomMesh {
		public string name;
		public Mesh mesh;
		public Material material;
		public float layer;
		public RoomMesh(string name, Mesh mesh, Material material, float layer) {
			this.name = name;
			this.mesh = mesh;
			this.material = material;
			this.layer = layer;
		}
	}

	public class Corner {
		public Vector2Int coords;
		public CornerFacing cornerFacing;
		public bool convex;
		public Corner xConnection;
		public Corner yConnection;

		public Corner(Vector2Int coords, CornerFacing cornerFacing, bool convex) {
			this.coords = coords;
			this.cornerFacing = cornerFacing;
			this.convex = convex;
		}

		public void FindConnections(List<Corner> corners) {
			Facing[] bufferedFacings = FacingUtils.GetFacingsFromCornerFacing(cornerFacing);
			Facing facingX = convex ? bufferedFacings[0] : FacingUtils.NegativeFacing(bufferedFacings[0]);
			Facing facingY = convex ? bufferedFacings[1] : FacingUtils.NegativeFacing(bufferedFacings[1]);

			Corner xConnection = null;
			Corner yConnection = null;
			foreach (Corner corner in corners) {
				if (corner.coords.Equals(coords))
					continue;

				if (corner.coords.y == coords.y && (facingX == Facing.LEFT ? (corner.coords.x < coords.x) : (corner.coords.x > coords.x)))
					if (xConnection == null || Mathf.Abs(coords.x - corner.coords.x) < Mathf.Abs(coords.x - xConnection.coords.x))
						xConnection = corner;

				if (corner.coords.x == coords.x && (facingY == Facing.UP ? (corner.coords.y > coords.y) : (corner.coords.y < coords.y)))
					if (yConnection == null || Mathf.Abs(coords.y - corner.coords.y) < Mathf.Abs(coords.y - yConnection.coords.y))
						yConnection = corner;
			}

			this.xConnection = xConnection;
			this.yConnection = yConnection;
		}

		public bool IsExtremeCorner(int roomWidth, int roomHeight) {
			return IsExtremeX(roomWidth) || IsExtremeY(roomHeight);
		}

		public virtual bool IsExtremeX(int roomWidth) {
			return coords.x == 0 || coords.x == roomWidth * 495;
		}

		public virtual bool IsExtremeY(int roomHeight) {
			return coords.y == 0 || coords.y == roomHeight * 277;
		}

		public virtual Corner Copy() {
			Corner copy = new Corner(coords, cornerFacing, convex);
			copy.xConnection = xConnection;
			copy.yConnection = yConnection;
			return copy;
		}
	}

	public class HiddenCorner : Corner {
		public bool isExtremeX;
		public bool isExtremeY;

		public HiddenCorner(Vector2Int coords, CornerFacing cornerFacing, bool convex, bool isExtremeX, bool isExtremeY) : base(coords, cornerFacing, convex) {
			this.isExtremeX = isExtremeX;
			this.isExtremeY = isExtremeY;
		}

		public override bool IsExtremeX(int roomWidth) {
			return isExtremeX;
		}

		public override bool IsExtremeY(int roomHeight) {
			return isExtremeY;
		}

		public Corner Copy() {
			HiddenCorner copy = new HiddenCorner(coords, cornerFacing, convex, isExtremeX, isExtremeY);
			copy.xConnection = xConnection;
			copy.yConnection = yConnection;
			return copy;
		}
	}
	#endregion
}
