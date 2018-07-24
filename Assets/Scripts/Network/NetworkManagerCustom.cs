using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

[AddComponentMenu("NetworkCustom/NetworkManagerCustom")]
public class NetworkManagerCustom : NetworkManager {
	public static bool IsServer = true;
	public static NetworkLobbyClientHUD.LobbyMode Mode;

	public override void OnServerSceneChanged(string sceneName) {
		if (ServerEvents.singleton.ServerOnly) {
			GameObject player = Instantiate(GameManager.singleton.LocalPlayer);
			player.GetComponent<GameProfile>().Deserialize(ServerEvents.singleton.ServerOnlyProfile);
			player.transform.position = GameObject.Find("StartPosition").transform.position;
			NetworkServer.AddPlayerForConnection(NetworkServer.connections[0], player, GameManager.singleton.indexController);
			GameManager.singleton.indexController++;
		}
	}

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
