using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameMessage {

	private short index;
	private NetworkMessageDelegate method;

	public GameMessage(NetworkMessageDelegate method) {
		index = MessageManager.LastIndex;
		this.method = method;
		MessageManager.LastIndex++;
		MessageManager.ToRegister.Add(this);
	}

	public void Register() {
		NetworkManager.singleton.client.RegisterHandler(index, method);
		NetworkServer.RegisterHandler(index, method);
	}

	public void SendToServer(MessageBase message) {
		NetworkManager.singleton.client.Send(index, message);
	}

	public void SendToClient(NetworkConnection connection, MessageBase message) {
		connection.Send(index, message);
	}

	public void SendToAllClients(MessageBase message) {
		NetworkServer.SendToAll(index, message);
	}

	public short GetIndex() {
		return index;
	}
}
