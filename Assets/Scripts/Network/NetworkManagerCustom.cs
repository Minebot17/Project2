using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

[AddComponentMenu("NetworkCustom/NetworkManagerCustom")]
public class NetworkManagerCustom : NetworkManager {
	public static bool IsServer = true;
	public List<GameObject> RegisteredPrefabs = new List<GameObject>();

	public override void OnServerDisconnect(NetworkConnection conn) {
		if (networkSceneName.Equals("Lobby")) {
			GameObject.Find("LobbyManager").GetComponent<NetworkLobbyServerHUD>().RemoveConnection(conn);
		}
	}

	public override void OnServerConnect(NetworkConnection conn) {
		if (ServerEvents.singleton != null && ServerEvents.singleton.InProgress) {
			conn.Disconnect();
		}
	}

	public override void OnStopServer() {
		ServerEvents.singleton = null;
	}
}
