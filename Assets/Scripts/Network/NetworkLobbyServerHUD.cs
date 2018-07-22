using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class NetworkLobbyServerHUD : MonoBehaviour {

	private Dictionary<NetworkConnection, bool> readyMap = new Dictionary<NetworkConnection, bool>();
	private Dictionary<NetworkConnection, string> profilesMap = new Dictionary<NetworkConnection, string>();
	private LobbyMode lobbyMode = LobbyMode.NONE;
	private bool ready;
	private GameProfile profile;
	private GameProfile[] allProfiles;
	private int lastConnections;
	private string loadedGameName;
	public static bool ServerOnly;
	public static string ServerOnlyProfile;
	
	public void Initialize(string arguments) {
		lobbyMode = arguments.Equals("new game") ? LobbyMode.NEW_GAME :
			arguments.Contains("load game") ? LobbyMode.LOAD_GAME : LobbyMode.ONLY_SERVER;
		profile = new GameProfile();
		if (lobbyMode == LobbyMode.LOAD_GAME) {
			string[] gameProfiles = arguments.Split('|')[2].Split('&');
			allProfiles = gameProfiles.Select(x => {
				GameProfile prf = new GameProfile();
				prf.Deserialize(x);
				return prf;
			}).ToArray();
			loadedGameName = arguments.Split('|')[1];
		}
	}
	
	private void OnGUI() {
		if (lobbyMode != LobbyMode.NONE) {
			if (lobbyMode == LobbyMode.ONLY_SERVER) {
				profile = allProfiles.Length == 0 ? new GameProfile() : allProfiles[0];
				NetworkManager.singleton.ServerChangeScene("Start");
				ServerOnly = true;
				ServerOnlyProfile = profile.Serialize();
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

			if (lobbyMode == LobbyMode.NEW_GAME) {
				GUILayout.Label("Имя персонажа:");
				profile.ProfileName = GUILayout.TextField(profile.ProfileName);
			}
			else if (lobbyMode == LobbyMode.LOAD_GAME) {
				foreach (GameProfile current in allProfiles) {
					if (GUILayout.Button(current.ProfileName + (profile == current ? "Выбран" : "")))
						profile = current;
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

	public string GetLoadedName() {
		return loadedGameName;
	}

	private enum LobbyMode {
		ONLY_SERVER, NEW_GAME, LOAD_GAME, NONE
	}
}
