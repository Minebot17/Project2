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

public class InitScane : NetworkBehaviour {
	public static InitScane instance;
	public static System.Random rnd = new System.Random();
	public string LanguageCode;
	public RoomSpawnMode RoomMode;
	public string TestRoomName;
	public GameObject LocalPlayer;
	public List<GameObject> Players;
	public Mesh OnePlane;
	public Mesh OnePlaneCenter;
	public Material DefaultMaterial;
	public Material GenerationMaterial;
	public bool VisualizeMeshGeneration;
	public bool VisualizeColliders;
	public bool VisualizeTraectorySimple;
	public bool VisualizeTraectoryAdvanced;
	public bool VisualizeTestGeneration;
	public bool LogEvents;
	public int MinGenerationCellCount;
	public float TraectoryTracingFrequency;
	public GameObject PointDebugObject;
	public GameObject LineDebugObject;
	public int RoomLayerMask;
	public int PlayerLayerMask;
	public int TrapLayerMask;
	public GameObject PlayerHead;
	public GameObject HitObjectParticle;
	public GameObject FireObjectParticle;

	private bool doStartForce;
	public void Awake() {
		instance = this;
		Timer.InitializeCreate();
		LanguageManager.Initialize();
		LanguageManager.SetLanguage(x => x.Code.Equals(LanguageCode));
		
		ObjectsManager.LoadAllObjectsFromResources();
		RoomLoader.LoadAllRoomsFromResources();
		RoomLayerMask = LayerMask.GetMask("Room");
		PlayerLayerMask = LayerMask.GetMask("Player");
		TrapLayerMask = LayerMask.GetMask("Trap");
		GenerationInfo generation = GetGeneration();
		
		switch (RoomMode) {
			case RoomSpawnMode.SPAWN_ONE:
				GenerationManager.currentRoom = RoomLoader.SpawnRoom(RoomLoader.loadedRooms.Find(x => x.fileName.Equals(TestRoomName)), Vector3.zero);
				GameObject.Find("Main Camera").GetComponent<CameraFollower>().Room = GenerationManager.currentRoom;
				break;
			case RoomSpawnMode.SPAWN_ONE_ROOMEDITOR:
				GenerationManager.currentRoom = RoomLoader.SpawnRoom(RoomLoader.LoadRoom(Application.streamingAssetsPath + "/room.json", Encoding.UTF8), Vector3.zero);
				string[] gateInfo = File.ReadAllLines(Application.streamingAssetsPath + "/gate.txt", Encoding.UTF8);
				GameObject startPos = new GameObject("startPosition");
				startPos.transform.position = new Vector3(int.Parse(gateInfo[0]), int.Parse(gateInfo[1]), -1f);
				startPos.AddComponent<NetworkStartPosition>();
				doStartForce = bool.Parse(gateInfo[2]);
				GameObject.Find("Main Camera").GetComponent<CameraFollower>().Room = GenerationManager.currentRoom;
				break;
			case RoomSpawnMode.SPAWN_GENERATION:
				GenerationManager.SpawnGeneration(RoomLoader.loadedRooms, generation);
				break;
		}
		
		if (VisualizeTestGeneration)
			GenerationManager.VisualizeGeneration(generation);
	}

	private GenerationInfo GetGeneration() {
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
				rooms => {
					for (int x = 0; x < rooms.GetLength(0); x++)
					for (int y = 0; y < rooms.GetLength(1); y++) {
						float procent = (-(1f / 3f) * (float) Math.Pow(x - 15, 2) + 100) / 100f;
						if (rnd.NextDouble() > procent)
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
				new List<Func<GenerationInfo, List<RoomInfo>>>(), 1
			);
			if (generation.CellsCount >= MinGenerationCellCount)
				break;
			Debug.Log("Regenerate");
		}

		return generation;
	}

	private void Start() {
		if (doStartForce)
			LocalPlayer.GetComponent<Rigidbody2D>().AddForce(new Vector2(0, 30000)); // tODO;
	}

	private void FixedUpdate() {
		InputManager.Handle();
	}

	[Serializable]
	public enum RoomSpawnMode {
		NONE, SPAWN_ONE, SPAWN_GENERATION, SPAWN_ONE_ROOMEDITOR
	}
	
	[Serializable]
	public class OnChangeLanguage : UnityEvent { }
}
