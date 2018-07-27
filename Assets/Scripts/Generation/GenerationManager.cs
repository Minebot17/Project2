using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using Random = System.Random;

public static class GenerationManager {

	public static GenerationInfo currentGeneration;
	public static GameObject currentRoom;
	public static List<GameObject> activeRooms = new List<GameObject>();
	public static GameObject[,] spawnedRooms;
	private static Vector2Int currentRoomCoords = new Vector2Int(-1, -1);

	/// <summary>
	/// Генерирует новый уровень
	/// </summary>
	/// <param name="levelIndex">Индекс уровня (0 - 8 включительно)</param>
	/// <param name="size">Размер в единичных комнатах</param>
	/// <param name="startEndReserver">Алгоритм, который резервирует и возвращает 2е выбранные комнаты, которые являются началом и концом уровня</param>
	/// <param name="remover">Алгоритм удаления комнат в сетке</param>
	/// <param name="removeGatesProcent">Процент удаления проходов (там не прямолинейно сё)</param>
	/// <param name="bigRoomProcent">Общий процент спавна большой комнаты</param>
	/// <param name="bigRoomTypesProcents">Карта, показывающая процент спавна каждой большой комнаты комнаты</param>
	/// <param name="reservers">Алгоритмы, которые будут резервировать обязательные комнаты (ставят true в reserved и задают имя нужной комнаты)</param>
	/// <param name="location">Индекс генерируемой локации</param>
	/// <param name="seed">Сид для рандома</param>
	/// <returns>Сгенерированный уровень</returns>
	public static GenerationInfo Generate(
			int levelIndex,
			Vector2Int size,
			Func<RoomInfo[,], RoomInfo[]> startEndReserver,
			Action<System.Random, RoomInfo[,]> remover,
			float removeGatesProcent,
			float bigRoomProcent,
			Dictionary<Vector2Int, float> bigRoomTypesProcents,
			List<Func<GenerationInfo, List<RoomInfo>>> reservers,
			int location,
			int seed
		) {
		Random random = new Random(seed);
		GenerationInfo generation = new GenerationInfo(levelIndex, size, location, seed); // создаем сетку комнат
		remover.Invoke(random, generation.Rooms); // Удаляем комнаты по алгоритму
		generation.ReserveRooms(reservers); // Резервируем некоторые комнаты по алгоритму
		generation.ReserveStartAndEnd(startEndReserver); // Отмечаем начало и конец уровня по алгоритму
		generation.GenerateBigRooms(random, bigRoomProcent, bigRoomTypesProcents); // Генерируем большие комнаты, не затрагивая зарезервированные
		generation.GenerateAndRemoveGates(random, removeGatesProcent); // генерируем проходы и удаляем рандомные
		generation.CheckAllGates(); // задаем проходам room to, удаляем проходы в никуда
		generation.RemoveSeparatedRooms(); // удаляем комнаты, к которым нельзя дойти от начала или от конца
		generation.ClearReservation(); // убираем резервацию (кроме начала и конца), и перезервируем
		generation.ReserveRooms(reservers);
		generation.CheckReservedRooms(); // проверяем все зарезервированные комнаты, что бы их обязательные проходы не вели в пустоту (создать комнату, если ведут)
		generation.ConstructPathToEndIfNotExists(); // конструируем путь от начала к концу (если он не существует) (обходя зарезервированные комнаты)
		generation.CheckAllGates(); // задаем проходам room to, удаляем проходы в никуда
		generation.WriteStats(); // записать характеристику и статистику о генерации в объект генерации
		
		return generation;
	}

	public static GenerationInfo Generate(GenerationData data) {
		return Generate(data.levelIndex, data.size, data.startEndReserver, data.remover, data.removeGatesProcent,
			data.bigRoomProcent, data.bigRoomTypesProcents, data.reservers, data.location, data.seed);
	}

	public static void SpawnGeneration(List<RoomLoader.Room> roomsToSpawn, GenerationInfo generation, int seed, bool onServer) {
		Random random = new Random(seed);
		currentGeneration = generation;
		spawnedRooms = new GameObject[generation.size.x, generation.size.y];
		GameObject generationObject = new GameObject("generation");
		SpawnRoom(roomsToSpawn.Find(x => x.fileName.Contains("startRoom")), generation.startRoom, generationObject.transform, onServer);
		SpawnRoom(roomsToSpawn.Find(x => x.fileName.Contains("endRoom")), generation.endRoom, generationObject.transform, onServer);
		foreach (RoomInfo reserved in generation.reservedRooms)
			SpawnRoom(roomsToSpawn.Find(x => x.fileName.Equals(reserved.ReservedRoomName)), reserved, generationObject.transform, onServer);
		for(int x = 0; x < generation.size.x; x++)
			for (int y = 0; y < generation.size.y; y++) {
				RoomInfo currentPosition = generation.rooms[x, y];
				if (currentPosition != null && currentPosition.Position == new Vector2Int(x, y) && !currentPosition.IsReserved) {
					List<RoomLoader.Room> validRooms = roomsToSpawn.Where(r =>
						IsRoomValid(r, currentPosition) &&
						!r.fileName.Contains("startRoom") &&
						!r.fileName.Contains("endRoom") &&
						!generation.reservedRooms.Exists(m => m.ReservedRoomName.Equals(r.fileName)) &&
						(r.location == 0 || r.location == generation.location)
					).ToList();
					
					if (validRooms.Count != 0)
						SpawnRoom(validRooms[random.Next(validRooms.Count)], currentPosition, generationObject.transform, onServer);
				}
			}
	}

	private static GameObject SpawnRoom(RoomLoader.Room room, RoomInfo position, Transform parent, bool onServer) {
		GameObject spawnedRoom = RoomLoader.SpawnRoom(room, new Vector3(position.Position.x * 495, position.Position.y * 277, 0), onServer);
		spawnedRoom.GetComponent<Room>().Position = position.Position;
		spawnedRoom.transform.parent = parent;
		List<GameObject> gates = Utils.GetComponentsRecursive<GateObject>(spawnedRoom).ConvertAll(x => x.gameObject);
		gates.ForEach(x =>
			x.transform.Find("trigger").GetComponent<GateTrigger>().GetEventSystem<GateTrigger.EnterGateEvent>()
				.SubcribeEvent(y => OnGateEnter(y.Player, y.Sender)));
				
		foreach (GameObject gateObject in gates) {
			Vector2Int localPosition = GateInfo.RoomObjectToLocalPosition(gateObject.transform.localPosition);
			if (gateObject.GetComponent<GateObject>().gateType == 0)
				gateObject.GetComponent<GateObject>().Enable =
					position.Gates.Exists(x => x.LocalPosition == localPosition);
		}
		
		for(int x = 0; x < position.Size.x; x++)
			for (int y = 0; y < position.Size.y; y++)
				spawnedRooms[x + position.Position.x, y + position.Position.y] = spawnedRoom;
		spawnedRoom.SetActive(false);
		return spawnedRoom;
	}

	private static void OnGateEnter(GameObject player, GameObject gateObject) {
		Vector2Int localPosition = GateInfo.RoomObjectToLocalPosition(gateObject.transform.localPosition);
		Vector2Int nextCoord = GateInfo.LocalPositionToVector(localPosition) + currentRoomCoords;
		if (spawnedRooms != null && nextCoord.x >= 0 && nextCoord.x < spawnedRooms.GetLength(0) && nextCoord.y >= 0 &&
		    nextCoord.y < spawnedRooms.GetLength(1)
		    ) {
			if (player.GetComponent<NetworkIdentity>().isLocalPlayer)
				SetCurrentRoom(nextCoord);
			Vector3 toPlayerCoords =
				localPosition.x == 0 ? new Vector3(0, -55f) :
				localPosition.x == 1 ? new Vector3(0, 55f) :
				localPosition.x == 2 ? new Vector3(-45f, 0) :
				new Vector3(45f, 0);
			player.transform.position += toPlayerCoords;
			if (localPosition.x == 1)
				player.GetComponent<Rigidbody2D>().AddForce(new Vector2(0, 30000f));
		}

		ApplyActiveRooms();
	}

	public static void TeleportPlayerToStart(GameObject player) {
		Transform objectFolder = null;
		try {
			spawnedRooms[currentRoomCoords.x, currentRoomCoords.y].transform.Find("Objects");
		}
		catch (IndexOutOfRangeException e) {
			objectFolder = spawnedRooms[currentGeneration.startRoom.Position.x, currentGeneration.startRoom.Position.y].transform.Find("Objects");
		}

		for (int i = 0; i < objectFolder.childCount; i++)
			if (objectFolder.GetChild(i).name.Contains("playerPosition")) {
				player.transform.position = new Vector3(objectFolder.GetChild(i).position.x + 8, objectFolder.GetChild(i).position.y + 33, player.transform.position.z);
				return;
			}
	}

	public static void SetCurrentRoom() {
		Vector3 position = GameManager.singleton.LocalPlayer.transform.position;
		SetCurrentRoom(new Vector2Int((int)position.x / 495, (int)position.y / 277));
	}

	public static void SetCurrentRoom(Vector2Int coord) {
		currentRoomCoords = coord;
		spawnedRooms[coord.x, coord.y].SetActive(true);
		currentRoom = spawnedRooms[coord.x, coord.y];
		GameObject.Find("Main Camera").GetComponent<CameraFollower>().Room = currentRoom;
	}

	public static void VisualizeGeneration(GenerationInfo generation) {
		GameObject visualizedGeneration = new GameObject("Generation");
		visualizedGeneration.transform.position = new Vector3(0, 0, -201);
		for(int x = 0; x < generation.size.x; x++)
			for (int y = 0; y < generation.size.y; y++) {
				if (generation.rooms[x, y] != null && generation.rooms[x, y].Position == new Vector2Int(x, y)) {
					RoomInfo room = generation.rooms[x, y];
					GameObject roomObject = new GameObject("room");
					roomObject.transform.parent = visualizedGeneration.transform;
					roomObject.AddComponent<MeshFilter>().mesh = GameManager.singleton.OnePlane;
					roomObject.AddComponent<MeshRenderer>().material = new Material(GameManager.singleton.GenerationMaterial);
					roomObject.GetComponent<MeshRenderer>().material.color =
						(room.Equals(generation.startRoom) || room.Equals(generation.endRoom) ? Color.green : Color.red) - new Color(0, 0, 0, 0.8f);
					roomObject.transform.localPosition = new Vector3(x * 495, y * 277);
					roomObject.transform.localScale = new Vector3(room.Size.x * 495, room.Size.y * 277, 1);

					foreach (GateInfo gate in room.Gates) {
						GameObject gateObject = new GameObject("gate");
						gateObject.AddComponent<MeshFilter>().mesh = GameManager.singleton.OnePlaneCenter;
						gateObject.AddComponent<MeshRenderer>().material = new Material(GameManager.singleton.GenerationMaterial);
						gateObject.GetComponent<MeshRenderer>().material.color = Color.blue - new Color(0, 0, 0, 0.8f);
						gateObject.transform.localScale = new Vector3(100, 100, 1);

						Vector3 pos = gate.LocalPosition.x == 0 || gate.LocalPosition.x == 2 ? new Vector3() :
							gate.LocalPosition.x == 1 ? new Vector3(0, 277 * room.Size.y) :
							new Vector3(495 * room.Size.x, 0);
						pos = new Vector3(
							gate.LocalPosition.x == 0 || gate.LocalPosition.x == 1 ? 247.5f + 495f * gate.LocalPosition.y : pos.x, 
							gate.LocalPosition.x == 2 || gate.LocalPosition.x == 3 ? 138.5f + 277f * gate.LocalPosition.y : pos.y
						);
						gateObject.transform.localPosition = roomObject.transform.localPosition + pos + new Vector3(0, 0, -201f);
						gateObject.transform.parent = roomObject.transform;
					}
				}
			}
	}

	public static bool IsRoomValid(RoomLoader.Room room, RoomInfo position) {
		if (room != null && room.size == position.Size) {
			List<RoomObject> gatesObject = room.objects.Where(x => x.prefabName.Contains("Gate")).ToList();
			foreach (RoomObject gateObject in gatesObject) {
				Vector2Int localPosition = GateInfo.RoomObjectToLocalPosition(gateObject.coords);
				int type = int.Parse(gateObject.data[0]);
				if (type == 1 && !position.Gates.Exists(x => x.LocalPosition == localPosition))
					return false;
				if (type == 2 && position.Gates.Exists(x => x.LocalPosition == localPosition))
					return false;
			}

			return true;
		}

		return false;
	}

	public static void ApplyActiveRooms() {
		List<Vector2Int> coords = new List<Vector2Int>();
		List<GameObject> newActiveRooms = new List<GameObject>();
		foreach (GameObject p in GameManager.singleton.Players) {
			Vector2Int coord = new Vector2Int((int) p.transform.position.x / 495, (int) p.transform.position.y / 277);
			if (!coords.Contains(coord))
				coords.Add(coord);
		}

		foreach (Vector2Int coord in coords)
			newActiveRooms.Add(spawnedRooms[coord.x, coord.y]);
		ApplyActiveRooms(newActiveRooms);
	}

	public static void ApplyActiveRooms(List<GameObject> newActiveRooms) {
		foreach (GameObject room in activeRooms)
			if (!newActiveRooms.Contains(room))
				ActivateRoom(room, false);
		foreach (GameObject room in newActiveRooms)
			if (!activeRooms.Contains(room))
				ActivateRoom(room, true);
		activeRooms = newActiveRooms;
	}

	public static void ActivateRoom(GameObject room, bool active) {
		if (!NetworkManagerCustom.IsServer) {
			room.SetActive(active);
			return;
		}

		if (active) {
			room.SetActive(true);
			if (room.GetComponent<Room>().Initialized) {
				Vector2Int pos = room.GetComponent<Room>().Position;

				Transform parent = room.transform.Find("Objects");
				foreach (SerializationManager.LoadedWorld.LoadedObject loadedObject in SerializationManager.World.Objects[pos.x, pos.y]) {
					GameObject gameObject = ObjectsManager.SpawnGameObject(Utils.FindAssetID(loadedObject.AssetID),
						Vector2.zero, Vector3.zero, parent, true);
					NetworkServer.Spawn(gameObject);
					gameObject.GetComponent<ISerializableObject>().Deserialize(loadedObject.Data);
				}
			}
			else {
				InitializeRoom(room);
			}

			SerializationManager.MarkDirtySave = true;
			List<string> networkIds = new List<string>();
			List<List<string>> data = new List<List<string>>();
			Transform parent0 = room.transform.Find("Objects");
			for (int i = 0; i < parent0.childCount; i++) {
				if (parent0.GetChild(i).GetComponent<NetworkIdentity>() != null && parent0.GetChild(i).GetComponent<ISerializableObject>() != null ) {
					networkIds.Add(parent0.transform.GetChild(i).GetComponent<NetworkIdentity>().netId.ToString());
					data.Add(parent0.GetChild(i).GetComponent<ISerializableObject>().Serialize());
				}
			}
			MessageManager.SetActiveRoomClientMessage.SendToAllClients(new MessageManager.ActiveRoomMessage(room.GetComponent<Room>().Position, networkIds, data));
		}
		else {
			SerializationManager.MarkDirtySave = true;
			Transform parent = room.transform.Find("Objects");
			for (int i = 0; i < parent.childCount; i++)
				if (parent.GetChild(i).gameObject.GetComponent<NetworkIdentity>() != null)
					NetworkServer.Destroy(parent.GetChild(i).gameObject);
			room.SetActive(false);
		}
	}

	public static void InitializeRoom(GameObject room) {
		if (!NetworkManagerCustom.IsServer)
			return;
		
		List<GameObject> objects = RoomLoader.SpawnSerializebleObjects(RoomLoader.loadedRooms.Find(x => x.fileName.Equals(room.GetComponent<Room>().RoomName)), room);
		foreach (GameObject gameObject in objects) {
			if (gameObject.GetComponent<ISerializableObject>() != null)
				gameObject.GetComponent<ISerializableObject>().Initialize();
		}

		room.GetComponent<Room>().Initialized = true;
	}

	public class GenerationData {
		public int levelIndex;
		public Vector2Int size;
		public Func<RoomInfo[,], RoomInfo[]> startEndReserver;
		public Action<System.Random, RoomInfo[,]> remover;
		public float removeGatesProcent;
		public float bigRoomProcent;
		public Dictionary<Vector2Int, float> bigRoomTypesProcents;
		public List<Func<GenerationInfo, List<RoomInfo>>> reservers;
		public int location;
		public int seed;
	}
}
