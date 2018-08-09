using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class GameManager : NetworkBehaviour {
	public static GameManager singleton;
	public static System.Random rnd = new System.Random();
	public GameObject LocalPlayer;
	public List<GameObject> Players;
	public Mesh OnePlane;
	public Mesh OnePlaneCenter;
	public Material DefaultMaterial;
	public Material GenerationMaterial;
	public Material IconMaterial;
	public GameObject PointDebugObject;
	public GameObject LineDebugObject;
	public int RoomLayerMask;
	public int PlayerLayerMask;
	public int TrapLayerMask;
	public GameObject PlayerHead;
	public GameObject HitObjectParticle;
	public GameObject FireObjectParticle;
	public GameObject PlayerInventoryPrefab;
	public int seedToSpawn;
	public int seedToGeneration;
	public bool doStartForce;
	public static Vector2Int DefGenerationSize = new Vector2Int(30, 30);
	
	public short indexController = 0;
	
	public void Awake() {
		singleton = this;
		GameSettings.Load();
		Timer.InitializeCreate();
		LanguageManager.Initialize();
		LanguageManager.SetLanguage(x => x.Code.Equals(GameSettings.SettingLanguageCode.Value));
		ItemManager.Initialize();
		//MessageManager.Initialize();
		
		ObjectsManager.LoadAllObjectsFromResources();
		RoomLoader.LoadAllRoomsFromResources();
		RoomLayerMask = LayerMask.GetMask("Room");
		PlayerLayerMask = LayerMask.GetMask("Player");
		TrapLayerMask = LayerMask.GetMask("Trap");
	}

	public GenerationInfo GetGeneration(int seed) {
		GenerationInfo generation = null;
		int threshold = 50;
		while (threshold > 0) {
			threshold--;
			generation = GenerationManager.Generate(
				0, DefGenerationSize,
				x => new[] {
					x.Cast<RoomInfo>().First(y =>
						y != null && y.Position.y > 28 && y.Position.x > 14 && y.Position.x < 16),
					x.Cast<RoomInfo>().First(y =>
						y != null && y.Position.y < 2 && y.Position.x > 14 && y.Position.x < 16)
				},
				(random, rooms) => {
					for (int x = 0; x < rooms.GetLength(0); x++)
					for (int y = 0; y < rooms.GetLength(1); y++) {
						float procent = (-(1f / 3f) * (float) Math.Pow(x - 15, 2) + 100) / 100f;
						if (random.NextDouble() > procent)
							rooms[x, y] = null;
					}
				},
				0.55f, 0.4f,
				new Dictionary<Vector2Int, float>() {
					{new Vector2Int(1, 2), 0.3f},
					{new Vector2Int(1, 3), 0.3f},
					{new Vector2Int(1, 4), 0.2f},
					{new Vector2Int(2, 1), 0.5f},
					{new Vector2Int(3, 1), 0.3f},
					{new Vector2Int(4, 1), 0.2f},
					{new Vector2Int(2, 2), 0.2f},
					{new Vector2Int(2, 3), 0.1f},
					{new Vector2Int(4, 2), 0.1f},
					{new Vector2Int(3, 2), 0.1f},
					{new Vector2Int(3, 3), 0.1f},
					{new Vector2Int(4, 4), 0.05f},
				},
				new List<Func<GenerationInfo, List<RoomInfo>>>(), 1,
				seed // 1082609359 for test
			);
			if (generation.CellsCount >= GameSettings.SettingMinGenerationCellCount.Value) {
				Debug.Log("ReservedCount: " + generation.ReservedCount);
				Debug.Log("RoomsCount: " + generation.RoomsCount);
				Debug.Log("CellsCount: " + generation.CellsCount);
				Debug.Log("GatesCount: " + generation.GatesCount);
				Debug.Log("Seed: " + generation.Seed);
				break;
			}

			Debug.Log("Regenerate");
			seed = new System.Random(seed).Next();
		}
		if (threshold <= 0)
			throw new Exception("incorrect filters for the current generation");

		return generation;
	}

	private void Start() {
		if (isServer) {
			NetworkManagerCustomGUI gui = GameObject.Find("Manager").GetComponent<NetworkManagerCustomGUI>();
			if (gui.StartArguments[0].Equals("new game")) {
				seedToGeneration = int.Parse(ServerEvents.singleton.SeedToGenerate);
				seedToSpawn = int.Parse(ServerEvents.singleton.SeedToSpawn);
				GenerationInfo generation = GetGeneration(seedToGeneration == 0 ? rnd.Next() : seedToGeneration);
				seedToSpawn = seedToSpawn == 0 ? rnd.Next() : seedToSpawn;
				GenerationManager.SpawnGeneration(RoomLoader.loadedRooms, generation, seedToSpawn, false);
				MessageManager.GetGenServerMessage.SendToServer(new EmptyMessage());
				if (GameSettings.SettingVisualizeTestGeneration.Value)
					GenerationManager.VisualizeGeneration(generation);
			}
			else if (gui.StartArguments[0].Equals("load game")) {
				SerializationManager.LoadWorld(gui.StartArguments[1]);
				GenerationInfo generation = GetGeneration(SerializationManager.World.Info.SeedToGenerate);
				GenerationManager.SpawnGeneration(RoomLoader.loadedRooms, generation, SerializationManager.World.Info.SeedToSpawn, false);
				MessageManager.GetGenServerMessage.SendToServer(new EmptyMessage());
				if (GameSettings.SettingVisualizeTestGeneration.Value)
					GenerationManager.VisualizeGeneration(generation);
			}
			else if (gui.StartArguments[0].Equals("test mode")) {
				GenerationManager.currentRoom = RoomLoader.SpawnRoom(RoomLoader.loadedRooms.Find(x => x.fileName.Equals(GameSettings.SettingTestRoomName.Value)), Vector3.zero, false);
				GenerationManager.InitializeRoom(GenerationManager.currentRoom);
				GameObject player = Instantiate(LocalPlayer);
				Vector3 toPos = Vector3.zero;
				Transform parent = GenerationManager.currentRoom.transform.Find("Objects");
				for (int i = 0; i < parent.childCount; i++)
					if (parent.GetChild(i).name.Contains("playerPosition"))
						toPos = parent.GetChild(i).position;
				player.transform.position = toPos + new Vector3(18, 25);
				if (ServerEvents.singleton.ServerOnlyProfile != null)
					player.GetComponent<GameProfile>().Deserialize(ServerEvents.singleton.ServerOnlyProfile);
				GameObject.Find("Main Camera").GetComponent<CameraFollower>().Room = GenerationManager.currentRoom;
				NetworkServer.AddPlayerForConnection(NetworkServer.connections[0], player, indexController++);
			}
			else if (gui.StartArguments[0].Equals("room editor")) {
				RoomLoader.Room room = RoomLoader.LoadRoom(Application.streamingAssetsPath + "/room.json", Encoding.UTF8);
				GenerationManager.currentRoom = RoomLoader.SpawnRoom(room, Vector3.zero, false);
				RoomLoader.loadedRooms.Add(room);
				GenerationManager.InitializeRoom(GenerationManager.currentRoom);
				string[] gateInfo = File.ReadAllLines(Application.streamingAssetsPath + "/gate.txt", Encoding.UTF8);
				GameObject.Find("startPosition").transform.position = new Vector3(int.Parse(gateInfo[0]), int.Parse(gateInfo[1]), -1f);
				doStartForce = bool.Parse(gateInfo[2]);
				GameObject player = Instantiate(LocalPlayer);
				player.transform.position = GameObject.Find("startPosition").transform.position;
				GameObject.Find("Main Camera").GetComponent<CameraFollower>().Room = GenerationManager.currentRoom;
				NetworkServer.AddPlayerForConnection(NetworkServer.connections[0], player, indexController++);
			}
		}
		else
			MessageManager.GetGenServerMessage.SendToServer(new EmptyMessage());
	}

	public static GameObject SpawnObjectsDefault(Vector3 position, NetworkHash128 assetId) {
		if (position.x == 0 && position.y == 0)
			return null;
			
		int x = (int)position.x / 495;
		int y = (int)position.y / 277;
		GameObject spawned = Instantiate(Utils.FindAssetID(assetId.ToString()));
		Room room = GenerationManager.spawnedRooms[x, y].GetComponent<Room>();
		spawned.transform.parent = room.gameObject.transform.Find("Objects");
		spawned.transform.position = position;

		return spawned;
	}
	
	public static void UnSpawnObjectsDefault(GameObject spawned){
		Destroy(spawned);
	}

	private void FixedUpdate() {
		InputManager.Handle();
		if (NetworkManagerCustom.IsServer && SerializationManager.MarkDirtySave) {
			SerializationManager.SaveWorld();
			SerializationManager.MarkDirtySave = false;
		}
	}
	
	[Serializable]
	public class OnChangeLanguage : UnityEvent { } // TODO
}
