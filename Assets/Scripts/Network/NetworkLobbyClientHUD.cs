using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
	private GameProfile[] allProfiles;
	private int select = 0;

	public void Initialize(string arguments) {
		lobbyMode = arguments.Equals("new game") ? LobbyMode.NEW_GAME :
			arguments.Contains("load game") ? LobbyMode.LOAD_GAME : LobbyMode.ONLY_SERVER;
		profile = new GameProfile();
		string[] gameProfiles = arguments.Split('|')[2].Split('&');
		if (lobbyMode == LobbyMode.LOAD_GAME) {
			allProfiles = gameProfiles.Select(x => {
				GameProfile prf = new GameProfile();
				prf.Deserialize(x);
				return prf;
			}).ToArray();
		}
	}

	private void OnGUI() {
		if (lobbyMode != LobbyMode.NONE && lobbyMode != LobbyMode.ONLY_SERVER) {
			GUILayout.Label("Вы в лобби у " + NetworkManager.singleton.client.serverIp);
			
			if (lobbyMode == LobbyMode.NEW_GAME) {
				GUILayout.Label("Имя персонажа:");
				profile.ProfileName = GUILayout.TextField(profile.ProfileName);
				// TODO полная кастомизация
			}
			else if (lobbyMode == LobbyMode.LOAD_GAME) {
				foreach (GameProfile current in allProfiles) {
					if (GUILayout.Button(current.ProfileName + (profile == current ? "Выбран" : "")))
						profile = current;
				}
			}

			if (GUILayout.Button(ready ? "Не готов" : "Готов")) {
				ready = !ready;
				MessageManager.SetReadyLobbyServerMessage.SendToServer(new StringMessage(ready+""));
			}
		}
	}

	public string GetProfile() {
		return profile.Serialize();
	}

	private enum LobbyMode {
		ONLY_SERVER, NEW_GAME, LOAD_GAME, NONE
	}
}
