﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class NetworkLobbyClientHUD : MonoBehaviour {

	protected LobbyMode lobbyMode = LobbyMode.NONE;
	protected bool ready;
	protected string profile;
	protected string[] allProfiles;

	protected string profileName;

	public virtual void Initialize(string arguments) {
		lobbyMode = arguments.Equals("new game") ? LobbyMode.NEW_GAME :
			arguments.Contains("load game") ? LobbyMode.LOAD_GAME : LobbyMode.ONLY_SERVER;
		if (lobbyMode == LobbyMode.LOAD_GAME)
			allProfiles = arguments.Split('|')[2].Split(new []{'&'}, StringSplitOptions.RemoveEmptyEntries);
		else {
			profile = new GameProfile().Serialize();
			profileName = "Player " + GameManager.rnd.Next();
		}
		
		if (!NetworkManagerCustom.IsServer) {
			foreach (GameObject go in ((NetworkManagerCustom)NetworkManager.singleton).RegisteredPrefabs)
				ClientScene.RegisterSpawnHandler(go.GetComponent<NetworkIdentity>().assetId, GameManager.SpawnObjectsDefault, GameManager.UnSpawnObjectsDefault);
		}
	}

	protected virtual void OnGUI() {
		if (ServerEvents.singleton != null && ServerEvents.singleton.InProgress)
			return;
		
		if (lobbyMode != LobbyMode.NONE && lobbyMode != LobbyMode.ONLY_SERVER) {
			GUILayout.Space(20);
			GUILayout.Label("Вы в лобби у " + NetworkManager.singleton.client.serverIp);

			if (!ready) {
				if (lobbyMode == LobbyMode.NEW_GAME) {
					GUILayout.Label("Имя персонажа:");
					profileName = GUILayout.TextField(profileName);
					// TODO полная кастомизация
				}
				else if (lobbyMode == LobbyMode.LOAD_GAME) {
					foreach (string current in allProfiles) {
						if (GUILayout.Button(current.Split(';')[0] + (profile == current ? "Выбран" : "")))
							profile = current;
					}
				}
			}

			if (lobbyMode == LobbyMode.NEW_GAME || profile != null)
				if (GUILayout.Button(ready ? "Не готов" : "Готов")) {
					ready = !ready;
					MessageManager.SetReadyLobbyServerMessage.SendToServer(new StringMessage(ready+""));
				}
		}
	}

	public virtual string GetProfile() {
		if (lobbyMode == LobbyMode.NEW_GAME) {
			GameProfile prf = new GameProfile();
			prf.ProfileName = profileName;
			return prf.Serialize();
		}
		else
			return profile;
	}

	public enum LobbyMode {
		ONLY_SERVER, NEW_GAME, LOAD_GAME, NONE
	}
}
