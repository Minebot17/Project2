using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class MessageManager {

	private static GameManager init;
	public static short LastIndex = 99;
	public static readonly List<GameMessage> ToRegister = new List<GameMessage>();

	public static void Initialize() {
		init = GameManager.Instance;
		foreach (GameMessage message in ToRegister)
			message.Register();
	}
	
	
	// ClientMessage - server to server, ServerMessage - client to server
	#region messages
	
	public static readonly GameMessage GetGenServerMessage = new GameMessage(msg => {
		SpawnGenClientMessage.SendToClient(msg.conn, new StringMessage(GenerationManager.currentGeneration.Seed + "," + GameManager.Instance.seedToSpawn));
	});	
	
	public static readonly GameMessage SpawnGenClientMessage = new GameMessage(msg => {
		string[] data = msg.ReadMessage<StringMessage>().value.Split(',');
		GenerationInfo generation = GameManager.Instance.GetGeneration(int.Parse(data[0]));
		GenerationManager.SpawnGeneration(RoomLoader.loadedRooms, generation, int.Parse(data[1]), false);
		GenerationManager.SetCurrentRoom(new Vector2Int(generation.startRoom.Position.x, generation.startRoom.Position.y));
		if (GameSettings.SettingVisualizeTestGeneration.Value)
			GenerationManager.VisualizeGeneration(generation);
		SpawnObjServerMessage.SendToServer(new EmptyMessage());
	});	
	
	public static readonly GameMessage SpawnObjServerMessage = new GameMessage(msg => {
		GameObject player = MonoBehaviour.Instantiate(GameManager.Instance.LocalPlayer);
		string data = GameObject.Find("LobbyManager").GetComponent<NetworkLobbyServerHUD>().GetClientProfile(msg.conn);
		player.GetComponent<GameProfile>().Deserialize(data);
		if (player.transform.position == Vector3.zero)
			GenerationManager.TeleportPlayerToStart(player);
		ServerEvents.OnServerPlayerAdd e = GameManager.ServerEvents.GetEventSystem<ServerEvents.OnServerPlayerAdd>()
			.CallListners(new ServerEvents.OnServerPlayerAdd(msg.conn, player));
		if (e.IsCancel)
			MonoBehaviour.Destroy(player);
		else
			NetworkServer.AddPlayerForConnection(msg.conn, player, init.indexController);
		init.indexController++;

		GenerationManager.SendToClientActiveRooms(msg.conn);
		NetworkSpawnSetupHandler.dirtyConnection = msg.conn;
		NetworkSpawnSetupHandler.markDirty = true;
	});	
	
	public static readonly GameMessage SpawnSetupClientMessage = new GameMessage(msg => {
		string[] data = msg.ReadMessage<StringMessage>().value.Split(';');
		uint id = uint.Parse(data[0]);
		NetworkSpawnSetupHandler[] gos = GameObject.FindObjectsOfType<NetworkSpawnSetupHandler>();
		foreach (NetworkSpawnSetupHandler go in gos) {
			if (go.gameObject.GetComponent<NetworkIdentity>().netId.Value == id) {
				int x = (int)go.transform.position.x / 495;
				int y = (int)go.transform.position.y / 277;
				go.name += id;
				go.transform.parent = GenerationManager.spawnedRooms[x, y].transform.Find("Objects").transform;
				go.gameObject.GetComponent<INetworkSpawnSetup>().RecieveData(go.gameObject, data.ToList());
				break;
			}
		}
	});	
	
	public static readonly GameMessage MarkDirtyActiveRoomsClientMessage = new GameMessage(msg => {
		string[] coords = msg.ReadMessage<StringMessage>().value.Split(new []{';'}, StringSplitOptions.RemoveEmptyEntries);
		List<GameObject> newActiveRooms = new List<GameObject>();
		foreach (string coord in coords) {
			string[] splitted = coord.Split(',');
			newActiveRooms.Add(GenerationManager.spawnedRooms[int.Parse(splitted[0]), int.Parse(splitted[1])]);
		}
		GenerationManager.ApplyActiveRooms(newActiveRooms);
		GenerationManager.SetCurrentRoom();
	});
	
	public static readonly GameMessage RequestLobbyModeServerMessage = new GameMessage(msg => {
		NetworkServer.SpawnObjects();
		ResponseLobbyModeClientMessage.SendToClient(msg.conn, new StringMessage(GameObject.Find("Manager").GetComponent<NetworkManagerCustomGUI>().StartArguments));
	});
	
	public static readonly GameMessage ResponseLobbyModeClientMessage = new GameMessage(msg => {
		MonoBehaviour.Destroy(GameObject.Find("LobbyManager").GetComponent<NetworkLobbyCommon>());
		GameObject.Find("LobbyManager").AddComponent<NetworkLobbyClientHUD>().Initialize(msg.ReadMessage<StringMessage>().value);
	});
	
	public static readonly GameMessage SetReadyLobbyServerMessage = new GameMessage(msg => {
		bool ready = bool.Parse(msg.ReadMessage<StringMessage>().value);
		GameObject.Find("LobbyManager").GetComponent<NetworkLobbyServerHUD>().SetReady(msg.conn, ready);
	});
	
	public static readonly GameMessage RequestProfileClientMessage = new GameMessage(msg => {
		ResponseProfileServerMessage.SendToServer(new StringMessage(GameObject.Find("LobbyManager").GetComponent<NetworkLobbyClientHUD>().GetProfile()));
	});
	
	public static readonly GameMessage ResponseProfileServerMessage = new GameMessage(msg => {
		NetworkLobbyServerHUD hud = GameObject.Find("LobbyManager").GetComponent<NetworkLobbyServerHUD>();
		hud.AddNewProfile(msg.conn, msg.ReadMessage<StringMessage>().value);
	});
	
	#endregion
}
