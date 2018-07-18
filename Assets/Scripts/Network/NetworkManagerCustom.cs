﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

[AddComponentMenu("NetworkCustom/NetworkManagerCustom")]
public class NetworkManagerCustom : NetworkManager {
	
	public override void OnClientSceneChanged(NetworkConnection conn) {
		if (networkSceneName.Equals("Lobby")) {
			MessageManager.RequestLobbyModeServerMessage.SendToServer(new EmptyMessage());
		}
	}

	public override void OnServerSceneChanged(string sceneName) {
		if (sceneName.Equals("Lobby")) {
			GameObject.Find("LobbyManager").AddComponent<NetworkLobbyServerHUD>().Initialize(GameObject.Find("Manager").GetComponent<NetworkManagerCustomGUI>().StartArguments);
		}
	}

	public override void OnServerDisconnect(NetworkConnection conn) {
		if (networkSceneName.Equals("Lobby")) {
			GameObject.Find("LobbyManager").AddComponent<NetworkLobbyServerHUD>().RemoveConnection(conn);
		}
	}
}
