﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

[AddComponentMenu("NetworkCustom/NetworkManagerCustomGUI")]
[RequireComponent(typeof(NetworkManagerCustom))]
public class NetworkManagerCustomGUI : MonoBehaviour {
	public string StartArguments;
	public string IpAddress;
	public string Port;
	public string Seed;
	private bool _started;

	private void Start() {
		_started = false;
		IpAddress = "localhost";
		Port = "7777";
		Seed = "0";
	}

	private void OnGUI() {
		if (!_started) {
			GUILayout.Label("Name:");

			GUILayout.Space(10);
			GUILayout.Label("Ip:");
			IpAddress = GUILayout.TextField(IpAddress, GUILayout.Width(100));
			GUILayout.Label("Port:");
			Port = GUILayout.TextField(Port, 5);
			GUILayout.Label("Map seed (0 - random):");
			Seed = GUILayout.TextField(Seed);

			if (GUILayout.Button("Connect")) {
				_started = true;
				NetworkManager.singleton.networkAddress = IpAddress.Equals("localhost") ? "127.0.0.1" : IpAddress;
				NetworkManager.singleton.networkPort = int.Parse(Port);
				NetworkManager.singleton.StartClient();
			}

			if (GUILayout.Button("New game")) {
				_started = true;
				StartArguments = "new game";
				NetworkManager.singleton.networkPort = int.Parse(Port);
				NetworkManager.singleton.StartHost();
			}
			
			if (GUILayout.Button("Load game")) {
				_started = true;
				StartArguments = "load game";
				NetworkManager.singleton.networkPort = int.Parse(Port);
				NetworkManager.singleton.StartHost();
			}

			if (GUILayout.Button("Start test room")) {
				_started = true;
				StartArguments = "test mode";
				NetworkManager.singleton.networkPort = int.Parse(Port);
				NetworkManager.singleton.StartHost();
			}
			
			if (GUILayout.Button("ROOM EDITOR TEST")) {
				_started = true;
				StartArguments = "room editor";
				NetworkManager.singleton.networkPort = int.Parse(Port);
				NetworkManager.singleton.StartHost();
			}
		}
		else {
			if (GUILayout.Button("Disconnect")) {
				string path = Application.dataPath + "/../" + "Build.exe";
				System.Diagnostics.Process.Start(path);
				_started = false;
				NetworkManager.singleton.StopHost();
				Application.Quit();
			}
		}
	}
}
