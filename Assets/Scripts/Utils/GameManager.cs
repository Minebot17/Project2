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
	public static GameManager Instance;
	public static System.Random rnd = new System.Random();
	public static ServerEvents ServerEvents = new ServerEvents();
	public GameSettings Settings;
	public GameObject LocalPlayer;
	public List<GameObject> Players;
	public Mesh OnePlane;
	public Mesh OnePlaneCenter;
	public Material DefaultMaterial;
	public Material GenerationMaterial;
	public GameObject PointDebugObject;
	public GameObject LineDebugObject;
	public int RoomLayerMask;
	public int PlayerLayerMask;
	public int TrapLayerMask;
	public GameObject PlayerHead;
	public GameObject HitObjectParticle;
	public GameObject FireObjectParticle;
	public int seedToSpawn;
	public int seedToGeneration;
	public bool doStartForce;
	
	public short indexController = 0;
	
	public void Awake() {
		Instance = this;
		Settings = new GameSettings();
		Settings.Load();
		Timer.InitializeCreate();
		LanguageManager.Initialize();
		LanguageManager.SetLanguage(x => x.Code.Equals(Settings.SettingLanguageCode.Value));
		MessageManager.Initialize();
		
		ObjectsManager.LoadAllObjectsFromResources();
		RoomLoader.LoadAllRoomsFromResources();
		RoomLayerMask = LayerMask.GetMask("Room");
		PlayerLayerMask = LayerMask.GetMask("Player");
		TrapLayerMask = LayerMask.GetMask("Trap");
		
		if (isClient) {
			foreach (GameObject go in NetworkManager.singleton.spawnPrefabs)
				ClientScene.RegisterSpawnHandler(go.GetComponent<NetworkIdentity>().assetId, SpawnObjectsDefault, UnSpawnObjectsDefault);
		}
	}

	public GenerationInfo GetGeneration(int seed) {
		GenerationInfo generation = null;
		while (true) {
			generation = GenerationManager.Generate(
				0, new Vector2Int(30, 30),
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
				seed
			);
			Debug.Log("ReservedCount: " + generation.ReservedCount);
			Debug.Log("RoomsCount: " + generation.RoomsCount);
			Debug.Log("CellsCount: " + generation.CellsCount);
			Debug.Log("GatesCount: " + generation.GatesCount);
			Debug.Log("Seed: " + generation.Seed);
			if (generation.CellsCount >= Settings.SettingMinGenerationCellCount.Value)
				break;
			Debug.Log("Regenerate");
			seed = new System.Random(seed).Next();
		}

		return generation;
	}

	private void Start() {
		if (isServer) {
			NetworkManagerCustomGUI gui = GameObject.Find("Manager").GetComponent<NetworkManagerCustomGUI>();
			if (gui.RoomMode == RoomSpawnMode.SPAWN_GENERATION) {
				seedToGeneration = int.Parse(gui.Seed);
				GenerationInfo generation = GetGeneration(seedToGeneration == 0 ? rnd.Next() : seedToGeneration);
				seedToSpawn = seedToSpawn == 0 ? rnd.Next() : seedToSpawn;
				GenerationManager.SpawnGeneration(RoomLoader.loadedRooms, generation, seedToSpawn, true);
				GameObject player = Instantiate(LocalPlayer);
				GenerationManager.TeleportPlayerToStart(player);
				NetworkServer.AddPlayerForConnection(NetworkServer.connections[0], player, indexController);
				indexController++;
				if (Settings.SettingVisualizeTestGeneration.Value)
					GenerationManager.VisualizeGeneration(generation);
			}
			else if (gui.RoomMode == RoomSpawnMode.SPAWN_ONE) {
				GenerationManager.currentRoom = RoomLoader.SpawnRoom(RoomLoader.loadedRooms.Find(x => x.fileName.Equals(Settings.SettingTestRoomName.Value)), Vector3.zero, true);
				GameObject player = Instantiate(LocalPlayer);
				player.transform.position = GameObject.Find("startPosition").transform.position;
				GameObject.Find("Main Camera").GetComponent<CameraFollower>().Room = GenerationManager.currentRoom;
				NetworkServer.AddPlayerForConnection(NetworkServer.connections[0], player, indexController);
				ServerEvents.OnServerPlayerAdd e = GameManager.ServerEvents.GetEventSystem<ServerEvents.OnServerPlayerAdd>()
					.CallListners(new ServerEvents.OnServerPlayerAdd(NetworkServer.connections[0], player));
			}
			else if (gui.RoomMode == RoomSpawnMode.SPAWN_ONE_ROOMEDITOR) {
				GenerationManager.currentRoom = RoomLoader.SpawnRoom(RoomLoader.LoadRoom(Application.streamingAssetsPath + "/room.json", Encoding.UTF8), Vector3.zero, true);
				string[] gateInfo = File.ReadAllLines(Application.streamingAssetsPath + "/gate.txt", Encoding.UTF8);
				GameObject.Find("startPosition").transform.position = new Vector3(int.Parse(gateInfo[0]), int.Parse(gateInfo[1]), -1f);
				doStartForce = bool.Parse(gateInfo[2]);
				GameObject player = Instantiate(LocalPlayer);
				player.transform.position = GameObject.Find("startPosition").transform.position;
				GameObject.Find("Main Camera").GetComponent<CameraFollower>().Room = GenerationManager.currentRoom;
				NetworkServer.AddPlayerForConnection(NetworkServer.connections[0], player, indexController);
				ServerEvents.OnServerPlayerAdd e = GameManager.ServerEvents.GetEventSystem<ServerEvents.OnServerPlayerAdd>()
					.CallListners(new ServerEvents.OnServerPlayerAdd(NetworkServer.connections[0], player));
			}
		}
		else
			MessageManager.GetGenServerMessage.SendToServer(new EmptyMessage());
	}

	private GameObject SpawnObjectsDefault(Vector3 position, NetworkHash128 assetId) {
		int x = (int)position.x % 495;
		int y = (int)position.y % 277;
		GameObject spawned = Instantiate(FindAssetID(assetId));
		spawned.transform.parent = GenerationManager.spawnedRooms[x, y].transform.Find("Objects");
		spawned.transform.position = position;
		return spawned;
	}
	
	public void UnSpawnObjectsDefault(GameObject spawned){
		Destroy(spawned);
	}

	private GameObject FindAssetID(NetworkHash128 assetId) {
		foreach (GameObject go in NetworkManager.singleton.spawnPrefabs) {
			if (go.GetComponent<NetworkIdentity>().assetId.Equals(assetId))
				return go;
		}

		return null;
	}

	private void FixedUpdate() {
		InputManager.Handle();
	}

	[Serializable]
	public enum RoomSpawnMode {
		NONE, SPAWN_ONE, SPAWN_GENERATION, SPAWN_ONE_ROOMEDITOR
	}
	
	[Serializable]
	public class OnChangeLanguage : UnityEvent { } // TODO
}
