using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

/// <summary>
/// Компонент, который сихнронизирует объекты при их спавне
/// </summary>
[RequireComponent(typeof(SimpleObject), typeof(INetworkSpawnSetup))]
public class NetworkSpawnSetupHandler : NetworkBehaviour {
	public static bool markDirty = false;
	public static NetworkConnection dirtyConnection = null;
	private bool localMarkDirty = false;

	private void FixedUpdate() {
		if (!isServer)
			return;
		
		if (localMarkDirty) {
			markDirty = false;
			localMarkDirty = false;
			dirtyConnection = null;
		}

		if (markDirty && !localMarkDirty) {
			string[] array = GetComponent<INetworkSpawnSetup>().SendData(gameObject).ToArray();
			string result = GetComponent<NetworkIdentity>().netId.Value + ";";
			foreach (string element in array)
				result += element + ";";

			MessageManager.SpawnSetupClientMessage.SendToClient(dirtyConnection, new StringMessage(result));
			localMarkDirty = true;
		}
	}
}
