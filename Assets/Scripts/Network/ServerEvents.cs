using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ServerEvents : IEventProvider {
	
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
