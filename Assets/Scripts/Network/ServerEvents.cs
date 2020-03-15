using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ServerEvents {
	public readonly EventHandler<OnServerPlayerAdd> onServerPlayerAdd = new EventHandler<OnServerPlayerAdd>();
	
	public static ServerEvents singleton;
	public List<string> StartAgrs;
	public string SeedToGenerate;
	public string SeedToSpawn;
	public bool ServerOnly;
	public List<string> ServerOnlyProfile;
	public string NewWorldName;
	public bool InProgress;
	public int GenerationReady;

	public static void Initialize() {
		if (singleton == null)
			singleton = new ServerEvents();
	}
	
	public class OnServerPlayerAdd : EventBase {
		public NetworkConnection PlayersConnection;
		public GameObject Player;
		
		public OnServerPlayerAdd(NetworkConnection playersConnection, GameObject player) : base(null, true) {
			PlayersConnection = playersConnection;
			Player = player;
		}
	}
}
