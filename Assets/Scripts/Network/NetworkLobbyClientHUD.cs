using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

[AddComponentMenu("NetworkCustom/NetworkLobbyHUD")]
[RequireComponent(typeof(NetworkManagerCustom))]
public class NetworkLobbyClientHUD : MonoBehaviour {

	private LobbyMode lobbyMode = LobbyMode.NONE;
	private bool ready;
	private GameProfile profile;

	public void Initialize(string arguments) {
		lobbyMode = arguments.Equals("new game") ? LobbyMode.NEW_GAME :
			arguments.Contains("load game") ? LobbyMode.LOAD_GAME : LobbyMode.ONLY_SERVER;
		profile = new GameProfile();
	}

	private void OnGUI() {
		if (lobbyMode != LobbyMode.NONE) {
			GUILayout.Label("Вы в лобби у " + NetworkManager.singleton.client.serverIp);
			
			if (lobbyMode == LobbyMode.NEW_GAME) {
				GUILayout.Label("Имя персонажа:");
				profile.ProfileName = GUILayout.TextField(profile.ProfileName);
			}

			if (GUILayout.Button(ready ? "Не готов" : "Готов")) {
				ready = !ready;
				MessageManager.SetReadyLobbyServerMessage.SendToServer(new StringMessage(ready+""));
			}
		}
	}

	private enum LobbyMode {
		ONLY_SERVER, NEW_GAME, LOAD_GAME, NONE
	}
}
