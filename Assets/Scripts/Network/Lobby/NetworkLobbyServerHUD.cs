using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class NetworkLobbyServerHUD : NetworkLobbyClientHUD {

	private Dictionary<NetworkConnection, bool> readyMap = new Dictionary<NetworkConnection, bool>();
	private Dictionary<NetworkConnection, List<string>> profilesMap = new Dictionary<NetworkConnection, List<string>>();
	private int lastConnections;
	private string loadedGameName;
	
	public override void Initialize(List<string> arguments) {
		lobbyMode = arguments[0].Equals("new game") ? LobbyMode.NEW_GAME :
			arguments[0].Equals("load game") ? LobbyMode.LOAD_GAME : LobbyMode.ONLY_SERVER;
		ServerEvents.Initialize();
		ServerEvents.singleton.StartAgrs = arguments;
		if (lobbyMode == LobbyMode.LOAD_GAME) {
			allProfiles = new List<List<string>>();
			for (int i = 2; i < arguments.Count; i++) {
				List<string> toAdd = new List<string>();
				toAdd.AddRange(arguments[i].Split(new []{'|'}, StringSplitOptions.RemoveEmptyEntries));
				allProfiles.Add(toAdd);
			}
			loadedGameName = arguments[1];
		}
		else {
			profile = new GameProfile().Serialize();
			ServerEvents.singleton.NewWorldName = "World " + GameManager.rnd.Next();
			ServerEvents.singleton.SeedToGenerate = "5";
			ServerEvents.singleton.SeedToSpawn = "4";
			profileName = "Player " + GameManager.rnd.Next();
		}
	}
	
	protected override void OnGUI() {
		if (ServerEvents.singleton.InProgress)
			return;
		
		if (lobbyMode != LobbyMode.NONE) {
			if (lobbyMode == LobbyMode.ONLY_SERVER) {
				GameProfile gm = new GameProfile();
				gm.ProfileName = "Minebot";
				profile = allProfiles == null ? gm.Serialize() : allProfiles[0];
				NetworkManager.singleton.ServerChangeScene("Start");
				ServerEvents.singleton.ServerOnly = true;
				ServerEvents.singleton.ServerOnlyProfile = profile;
				ServerEvents.singleton.InProgress = true;
				return;
			}

			int readyCount = 0;
			foreach (var conn in readyMap.Keys) {
				if (readyMap[conn])
					readyCount++;
			}

			int connectionsCount = 0;
			foreach (NetworkConnection conn in NetworkServer.connections) {
				if (conn != null)
					connectionsCount++;
			}
			
			GUILayout.Space(20);
			GUILayout.Label("Вы в лобби. Подключено " + connectionsCount + " игроков");
			GUILayout.Label("Готовы " + readyCount + " из " + connectionsCount);

			if (!ready) {
				if (lobbyMode == LobbyMode.NEW_GAME) {
					GUILayout.Label("Название мира:");
					ServerEvents.singleton.NewWorldName = GUILayout.TextField(ServerEvents.singleton.NewWorldName);
					GUILayout.Label("Сид структуры мира (0 - рандом):");
					ServerEvents.singleton.SeedToGenerate = GUILayout.TextField(ServerEvents.singleton.SeedToGenerate);
					GUILayout.Label("Сид спавна комнат (0 - рандом):");
					ServerEvents.singleton.SeedToSpawn = GUILayout.TextField(ServerEvents.singleton.SeedToSpawn);
					GUILayout.Label("Имя персонажа:");
					profileName = GUILayout.TextField(profileName);
				}
				else if (lobbyMode == LobbyMode.LOAD_GAME) {
					foreach (List<string> current in allProfiles) {
						if (GUILayout.Button(current[0] + (profile == current ? " Выбран" : "")))
							profile = current;
					}
				}
			}

			if (lobbyMode == LobbyMode.NEW_GAME || profile != null)
				if (GUILayout.Button(ready ? "Не готов" : "Готов")) {
					ready = !ready;
					SetReady(NetworkManager.singleton.client.connection, ready);
				}

			if (readyCount == connectionsCount && GUILayout.Button("Старт!")) {
				ServerEvents.singleton.InProgress = true;
				MessageManager.RequestProfileClientMessage.SendToAllClients(new EmptyMessage());
				lastConnections = readyCount;
			}
		}
	}

	public void SetReady(NetworkConnection conn, bool ready) {
		readyMap[conn] = ready;
	}

	public void RemoveConnection(NetworkConnection conn) {
		readyMap.Remove(conn);
	}

	public void AddNewProfile(NetworkConnection conn, List<string> data) {
		profilesMap.Add(conn, data);
		lastConnections--;
		if (lastConnections == 0) {
			NetworkManager.singleton.ServerChangeScene("Start");
		}
	}

	public List<string> GetClientProfile(NetworkConnection conn) {
		return profilesMap[conn];
	}

	public override List<string> GetProfile() {
		if (lobbyMode == LobbyMode.NEW_GAME) {
			GameProfile prf = new GameProfile();
			prf.ProfileName = profileName;
			return prf.Serialize();
		}
		else
			return profile;
	}

	public string GetLoadedName() {
		return loadedGameName;
	}
}
