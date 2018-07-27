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
				if (ServerEvents.singleton.StartAgrs.Equals("new game")) {
					string data = GameObject.Find("LobbyManager").GetComponent<NetworkLobbyServerHUD>().GetClientProfile(conn);
					player.GetComponent<GameProfile>().Deserialize(data);
					GenerationManager.TeleportPlayerToStart(player);
				}
				else if (ServerEvents.singleton.StartAgrs.Contains("load game")) {
					List<string> data = SerializationManager.World.Players.Find(x =>
						x[0].Equals(GameObject.Find("LobbyManager").GetComponent<NetworkLobbyServerHUD>().GetClientProfile(conn)));
					if (data == null) {
						conn.Disconnect();
						continue;
					}

					SerializationManager.DeserializePlayer(player, data);
				}
				
				NetworkServer.AddPlayerForConnection(conn, player, GameManager.singleton.indexController++);
				SendPlayerDataClientMessage.SendToClient(conn, new StringListMessage(SerializationManager.SerializePlayer(player)));
			}
			
			GenerationManager.ApplyActiveRooms();
			SetCurrentRoomClientMessage.SendToAllClients(new EmptyMessage());
		}
	});
	
	public static readonly GameMessage RequestLobbyModeServerMessage = new GameMessage(msg => {
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

	public static readonly GameMessage SendPlayerDataClientMessage = new GameMessage(msg => {
		List<string> data = msg.ReadMessage<StringListMessage>().Value;
		SerializationManager.DeserializePlayer(GameManager.singleton.Players.Find(x => x.GetComponent<NetworkIdentity>().isLocalPlayer), data);
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
					parent.GetChild(j).GetComponent<ISerializableObject>().Deserialize(message.Data[i]);
				}
	});
	
	[System.Serializable]
	public class StringListMessage : MessageBase {
		public StringList Value = new StringList();

		public StringListMessage() { }

		public StringListMessage(List<string> value) {
			Value = new StringList();
			Value.AddRange(value);
		}
		
		// De-serialize the contents of the reader into this message
		public override void Deserialize(NetworkReader reader) {
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
				Value.Add(reader.ReadString());
		}

		// Serialize the contents of this message into the writer
		public override void Serialize(NetworkWriter writer) {
			writer.Write(Value.Count);
			foreach (string value in Value) {
				writer.Write(value);
			}
		}
	}

	[System.Serializable]
	public class ActiveRoomMessage : MessageBase {
		public int PositionX;
		public int PositionY;
		public StringList NetworkIDs;
		public MultyStringList Data;

		public ActiveRoomMessage() { }

		public ActiveRoomMessage(Vector2Int position, List<string> networkIDs, List<List<string>> data) {
			PositionX = position.x;
			PositionY = position.y;
			NetworkIDs = new StringList();
			Data = new MultyStringList();
			NetworkIDs.AddRange(networkIDs);
			Data.AddRange(data);
		}
		
		// De-serialize the contents of the reader into this message
		public override void Deserialize(NetworkReader reader) {
			PositionX = reader.ReadInt32();
			PositionY = reader.ReadInt32();
			NetworkIDs = new StringList();
			Data = new MultyStringList();
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
				NetworkIDs.Add(reader.ReadString());

			int count0 = reader.ReadInt32();
			for (int i = 0; i < count0; i++) {
				int count1 = reader.ReadInt32();
				List<string> toAdd = new List<string>();
				for (int j = 0; j < count1; j++) {
					toAdd.Add(reader.ReadString());
				}
				Data.Add(toAdd);
			}
		}

		// Serialize the contents of this message into the writer
		public override void Serialize(NetworkWriter writer) {
			writer.Write(PositionX);
			writer.Write(PositionY);
			writer.Write(NetworkIDs.Count);
			foreach (string value in NetworkIDs) {
				writer.Write(value);
			}
			
			writer.Write(Data.Count);
			foreach (List<string> list in Data) {
				writer.Write(list.Count);
				foreach (string s in list) {
					writer.Write(s);
				}
			}
		}
	}

	[System.Serializable]
	public class StringList : List<string> { }

	[System.Serializable]
	public class MultyStringList : List<List<string>> { }

	#endregion
}
