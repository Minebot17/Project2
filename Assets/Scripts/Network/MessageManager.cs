using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class MessageManager {

	public static short LastIndex = 99;
	public static readonly List<GameMessage> ToRegister = new List<GameMessage>();

	public static void Initialize() {
		foreach (GameMessage message in ToRegister)
			message.Register();
	}
	
	
	// ClientMessage - server to server, ServerMessage - client to server
	#region messages
	
	public static readonly GameMessage GetGenServerMessage = new GameMessage(msg => {
		SpawnGenClientMessage.SendToClient(msg.conn, new StringMessage(GenerationManager.currentGeneration.Seed + "," + GameManager.singleton.seedToSpawn));
	});	
	
	public static readonly GameMessage SpawnGenClientMessage = new GameMessage(msg => {
		if (!NetworkManagerCustom.IsServer) {
			string[] data = msg.ReadMessage<StringMessage>().value.Split(',');
			GenerationInfo generation = GameManager.singleton.GetGeneration(int.Parse(data[0]));
			GenerationManager.SpawnGeneration(RoomLoader.loadedRooms, generation, int.Parse(data[1]), false);
			if (GameSettings.SettingVisualizeTestGeneration.Value)
				GenerationManager.VisualizeGeneration(generation);
		}

		GenerationReadyServerMessage.SendToServer(new EmptyMessage());
	});
	
	public static readonly GameMessage GenerationReadyServerMessage = new GameMessage(msg => {
		ServerEvents.singleton.GenerationReady++;
		if (ServerEvents.singleton.GenerationReady == NetworkServer.connections.Count(x => x != null)) {
			foreach (NetworkConnection conn in NetworkServer.connections) {
				if (conn == null)
					continue;
				
				GameObject player = MonoBehaviour.Instantiate(GameManager.singleton.LocalPlayer);
				if (ServerEvents.singleton.StartAgrs[0].Equals("new game")) {
					List<string> data = GameObject.Find("LobbyManager").GetComponent<NetworkLobbyServerHUD>().GetClientProfile(conn);
					player.GetComponent<GameProfile>().Deserialize(data);
					GenerationManager.TeleportPlayerToStart(player);
					SerializationManager.InitializeObject(player);
				}
				else if (ServerEvents.singleton.StartAgrs[0].Equals("load game")) {
					List<string> data = SerializationManager.World.Players.Find(x =>
						x.Count != 0 && x.Equals(GameObject.Find("LobbyManager").GetComponent<NetworkLobbyServerHUD>().GetClientProfile(conn)));
					if (data == null) {
						conn.Disconnect();
						continue;
					}

					SerializationManager.DeserializeObject(player, data);
				}
				
				NetworkServer.AddPlayerForConnection(conn, player, GameManager.singleton.indexController++);
				SendPlayerDataClientMessage.SendToClient(conn, new StringListMessage(SerializationManager.SerializeObject(player)));
			}
			
			GenerationManager.ApplyActiveRooms();
			SetCurrentRoomClientMessage.SendToAllClients(new EmptyMessage());
		}
	});
	
	public static readonly GameMessage RequestLobbyModeServerMessage = new GameMessage(msg => {
		ResponseLobbyModeClientMessage.SendToClient(msg.conn, new StringListMessage(GameObject.Find("Manager").GetComponent<NetworkManagerCustomGUI>().StartArguments));
	});
	
	public static readonly GameMessage ResponseLobbyModeClientMessage = new GameMessage(msg => {
		MonoBehaviour.Destroy(GameObject.Find("LobbyManager").GetComponent<NetworkLobbyCommon>());
		GameObject.Find("LobbyManager").AddComponent<NetworkLobbyClientHUD>().Initialize(msg.ReadMessage<StringListMessage>().Value);
	});
	
	public static readonly GameMessage SetReadyLobbyServerMessage = new GameMessage(msg => {
		bool ready = bool.Parse(msg.ReadMessage<StringMessage>().value);
		GameObject.Find("LobbyManager").GetComponent<NetworkLobbyServerHUD>().SetReady(msg.conn, ready);
	});
	
	public static readonly GameMessage RequestProfileClientMessage = new GameMessage(msg => {
		ResponseProfileServerMessage.SendToServer(new StringListMessage(GameObject.Find("LobbyManager").GetComponent<NetworkLobbyClientHUD>().GetProfile()));
	});
	
	public static readonly GameMessage ResponseProfileServerMessage = new GameMessage(msg => {
		NetworkLobbyServerHUD hud = GameObject.Find("LobbyManager").GetComponent<NetworkLobbyServerHUD>();
		hud.AddNewProfile(msg.conn, msg.ReadMessage<StringListMessage>().Value);
	});

	public static readonly GameMessage SendPlayerDataClientMessage = new GameMessage(msg => {
		List<string> data = msg.ReadMessage<StringListMessage>().Value;
		SerializationManager.DeserializeObject(GameManager.singleton.Players.Find(x => x.GetComponent<NetworkIdentity>().isLocalPlayer), data);
		MonoBehaviour.Destroy(GameObject.Find("LobbyManager"));
	});

	public static readonly GameMessage SetCurrentRoomClientMessage = new GameMessage(msg => {
		foreach (GameObject player in GameManager.singleton.Players) {
			int x = (int)player.transform.position.x / 495;
			int y = (int)player.transform.position.y / 277;
			GenerationManager.spawnedRooms[x, y].SetActive(true);
			
			if (player.GetComponent<NetworkIdentity>().isLocalPlayer)
				GenerationManager.SetCurrentRoom(new Vector2Int(x, y));
		}
	});

	public static readonly GameMessage SetActiveRoomClientMessage = new GameMessage(msg => {
		ActiveRoomMessage message = msg.ReadMessage<ActiveRoomMessage>();
		Transform parent = GenerationManager.spawnedRooms[message.PositionX, message.PositionY].transform.Find("Objects");
		for (int i = 0; i < message.NetworkIDs.Count; i++)
			for (int j = 0; j < parent.childCount; j++)
				if (parent.GetChild(j).GetComponent<NetworkIdentity>() != null && parent.GetChild(j)
						.GetComponent<NetworkIdentity>().netId.ToString().Equals(message.NetworkIDs[i])) {
					SerializationManager.DeserializeObject(parent.GetChild(j).gameObject, message.Data[i]);
				}
	});

	public static readonly GameMessage GateEnterClientMessage = new GameMessage(msg => {
		string[] splitted = msg.ReadMessage<StringMessage>().value.Split(';');
		Vector2Int localPosition = new Vector2Int(int.Parse(splitted[0]), int.Parse(splitted[1]));
		Transform parent = GenerationManager.currentRoom.transform.Find("Objects");
		for (int i = 0; i < parent.childCount; i++)
			if (parent.GetChild(i).tag.Equals("Gate") &&
			    GateInfo.RoomObjectToLocalPosition(new Vector3(parent.GetChild(i).position.x % 495, parent.GetChild(i).position.y % 277)) == localPosition) {
				GenerationManager.OnGateEnter(GameManager.singleton.LocalPlayer, parent.GetChild(i).gameObject, true);
				break;
			}
	});

	public static readonly GameMessage DestroyServerMessage = new GameMessage(msg => {
		NetworkServer.Destroy(msg.ReadMessage<NetworkIdentityMessage>().Value.gameObject);
	});

	[System.Serializable]
	public class StringList : List<string> { }

	[System.Serializable]
	public class MultyStringList : List<List<string>> { }
	
	[System.Serializable]
	public class Vector3List : List<Vector3> { }

	#endregion
}
