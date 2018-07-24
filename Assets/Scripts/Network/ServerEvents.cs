using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ServerEvents : IEventProvider {
	public static ServerEvents singleton;
	public string StartAgrs;
	public string SeedToGenerate;
	public string SeedToSpawn;
	public SerializationManager.LoadedWorld LastLoadedWorld;
	public bool ServerOnly;
	public string ServerOnlyProfile;
	public string NewWorldName;
	public bool InProgress;
	public bool MarkDirtyForSave;
	public int GenerationReady;

	public static void Initialize() {
		if (singleton == null)
			singleton = new ServerEvents();
	}

	private readonly object[] eventHandlers = {
		new EventHandler<OnServerPlayerAdd>(), 
	};

	public EventHandler<T> GetEventSystem<T>() where T : EventBase {
		return Utils.FindEventHandler<T>(eventHandlers);
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
