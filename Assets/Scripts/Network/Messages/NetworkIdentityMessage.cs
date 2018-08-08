using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class NetworkIdentityMessage : MessageBase {
	public NetworkIdentity Value;

	public NetworkIdentityMessage() { }

	public NetworkIdentityMessage(NetworkIdentity identity) {
		Value = identity;
	}
		
	public override void Deserialize(NetworkReader reader) {
		Value = reader.ReadNetworkIdentity();
	}

	public override void Serialize(NetworkWriter writer) {
		writer.Write(Value);
	}
}
