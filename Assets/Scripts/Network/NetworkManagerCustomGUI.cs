﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

[AddComponentMenu("NetworkCustom/NetworkManagerCustomGUI")]
[RequireComponent(typeof(NetworkManagerCustom))]
public class NetworkManagerCustomGUI : MonoBehaviour {
	public List<string> StartArguments;
	public string IpAddress;
	public string Port;
	private string loadWorldName;
	private bool _started;

	private void Start() {
		_started = false;
		IpAddress = "localhost";
		Port = "7777";
		loadWorldName = "test";
	}

	private void OnGUI() {
		if (!_started) {
			GUILayout.Label("Ip:");
			IpAddress = GUILayout.TextField(IpAddress, GUILayout.Width(100));
			GUILayout.Label("Port:");
			Port = GUILayout.TextField(Port, 5);

			if (GUILayout.Button("Connect")) {
				_started = true;
				NetworkManagerCustom.IsServer = false;
				NetworkManager.singleton.networkAddress = IpAddress.Equals("localhost") ? "127.0.0.1" : IpAddress;
				NetworkManager.singleton.networkPort = int.Parse(Port);
				NetworkManager.singleton.StartClient();
			}

			if (GUILayout.Button("New game")) {
				_started = true;
				StartArguments = new List<string>() {"new game"};
				NetworkManager.singleton.networkPort = int.Parse(Port);
				NetworkManager.singleton.StartHost();
			}

			loadWorldName = GUILayout.TextField(loadWorldName);
			if (GUILayout.Button("Load game")) {
				_started = true;
				SerializationManager.LoadWorld(loadWorldName);
				List<string> result = new List<string>();
				SerializationManager.World.Players.ForEach(x => {
					string toResult = "";
					x.ForEach(y => toResult += y + "|");
					result.Add(toResult);
				});
				StartArguments = new List<string>() {"load game", loadWorldName};
				StartArguments.AddRange(result);
				NetworkManager.singleton.networkPort = int.Parse(Port);
				NetworkManager.singleton.StartHost();
			}

			if (GUILayout.Button("Start test room")) {
				_started = true;
				StartArguments = new List<string>() {"test mode"};
				NetworkManager.singleton.networkPort = int.Parse(Port);
				NetworkManager.singleton.StartHost();
			}
			
			if (GUILayout.Button("ROOM EDITOR TEST")) {
				_started = true;
				StartArguments = new List<string>() {"room editor"};
				NetworkManager.singleton.networkPort = int.Parse(Port);
				NetworkManager.singleton.StartHost();
			}
		}
		else {
			if (GUILayout.Button("Disconnect")) {
				string path = Application.dataPath + "/../" + "RogueLikeTest.exe";
				System.Diagnostics.Process.Start(path);
				_started = false;
				NetworkManager.singleton.StopHost();
				Application.Quit();
			}
		}
	}
}
