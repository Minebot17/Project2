using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class NetworkLobbyServerHUD : NetworkLobbyClientHUD {

	private Dictionary<NetworkConnection, bool> readyMap = new Dictionary<NetworkConnection, bool>();
	private Dictionary<NetworkConnection, string> profilesMap = new Dictionary<NetworkConnection, string>();
	private int lastConnections;
	private string loadedGameName;
	public static bool ServerOnly;
	public static string ServerOnlyProfile;
	
	public override void Initialize(string arguments) {
		lobbyMode = arguments.Equals("new game") ? LobbyMode.NEW_GAME :
			arguments.Contains("load game") ? LobbyMode.LOAD_GAME : LobbyMode.ONLY_SERVER;
		profile = new GameProfile().Serialize();
		if (lobbyMode == LobbyMode.LOAD_GAME) {
			allProfiles = arguments.Split('|')[2].Split('&');
			loadedGameName = arguments.Split('|')[1];
		}
	}
	
	protected override void OnGUI() {
		if (lobbyMode != LobbyMode.NONE) {
			if (lobbyMode == LobbyMode.ONLY_SERVER) {
				profile = allProfiles.Length == 0 ? new GameProfile().Serialize() : allProfiles[0];
				NetworkManager.singleton.ServerChangeScene("Start");
				ServerOnly = true;
				ServerOnlyProfile = profile;
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
					GUILayout.Label("Имя персонажа:");
					profileName = GUILayout.TextField(profileName);
				}
				else if (lobbyMode == LobbyMode.LOAD_GAME) {
					foreach (string current in allProfiles) {
						if (GUILayout.Button(current.Split(';')[0] + (profile == current ? "Выбран" : "")))
							profile = current;
					}
				}
			}

			if (GUILayout.Button(ready ? "Не готов" : "Готов")) {
				ready = !ready;
				SetReady(NetworkManager.singleton.client.connection, ready);
			}

			if (readyCount == connectionsCount && GUILayout.Button("Поiхали!")) {
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

	public void AddNewProfile(NetworkConnection conn, string data) {
		profilesMap.Add(conn, data);
		lastConnections--;
		if (lastConnections == 0) {
			NetworkManager.singleton.ServerChangeScene("Start");
		}
	}

	public string GetClientProfile(NetworkConnection conn) {
		return profilesMap[conn];
	}

	public override string GetProfile() {
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
