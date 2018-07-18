using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class NetworkLobbyServerHUD : MonoBehaviour {

	private Dictionary<NetworkConnection, bool> readyMap = new Dictionary<NetworkConnection, bool>();
	private Dictionary<NetworkConnection, int> ownProfileIndexMap = new Dictionary<NetworkConnection, int>();
	private Dictionary<NetworkConnection, string> newProfilesMap = new Dictionary<NetworkConnection, string>();
	private LobbyMode lobbyMode = LobbyMode.NONE;
	private bool ready;
	private GameProfile profile;
	private int lastConnections;
	
	public void Initialize(string arguments) {
		lobbyMode = arguments.Equals("new game") ? LobbyMode.NEW_GAME :
			arguments.Contains("load game") ? LobbyMode.LOAD_GAME : LobbyMode.ONLY_SERVER;
	}
	
	private void OnGUI() {
		if (lobbyMode != LobbyMode.NONE) {
			int readyCount = 0;
			foreach (var conn in readyMap.Keys) {
				if (readyMap[conn])
					readyCount++;
			}
			
			GUILayout.Label("Вы в лобби. Подключено " + NetworkServer.connections.Count + " игроков");
			GUILayout.Label("Готовы " + readyCount + " из " + readyMap.Count);

			if (lobbyMode == LobbyMode.NEW_GAME) {
				GUILayout.Label("Имя персонажа:");
				profile.ProfileName = GUILayout.TextField(profile.ProfileName);
			}

			if (GUILayout.Button(ready ? "Не готов" : "Готов")) {
				ready = !ready;
				SetReady(NetworkManager.singleton.client.connection, ready);
			}

			if (readyCount == readyMap.Count && GUILayout.Button("Поiхали!")) {
				MessageManager.RequestProfileClientMessage.SendToAllClients(new StringMessage(lobbyMode == LobbyMode.NEW_GAME ? "new" : "load"));
				lastConnections = readyCount;
			}
		}
	}

	public void SetReady(NetworkConnection conn, bool ready) {
		readyMap.Add(conn, ready);
	}

	public void RemoveConnection(NetworkConnection conn) {
		readyMap.Remove(conn);
	}

	public void SetOwnProfile(NetworkConnection conn, int index) {
		ownProfileIndexMap.Add(conn, index);
		lastConnections--;
		if (lastConnections == 0) {
			LoadGame();
		}
	}

	public void AddNewProfile(NetworkConnection conn, string data) {
		newProfilesMap.Add(conn, data);
		lastConnections--;
		if (lastConnections == 0) {
			StartNewGame();
		}
	}

	public void StartNewGame() {
		// TODO
	}

	public void LoadGame() {
		// TODO
	}

	private enum LobbyMode {
		ONLY_SERVER, NEW_GAME, LOAD_GAME, NONE
	}
}
