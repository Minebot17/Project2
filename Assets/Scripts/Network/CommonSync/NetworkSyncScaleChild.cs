using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[AddComponentMenu("NetworkCustom/NetworkSyncScaleChild")]
public class NetworkSyncScaleChild : NetworkBehaviour {

	[SerializeField]
	private bool fromLocalPlayer;
	public List<ChildVector> Childs = new List<ChildVector>();

	private Vector3[] lastScales;

	private void Awake() {
		lastScales = new Vector3[Childs.Count];
		for (int i = 0; i < Childs.Count; i++)
			lastScales[i] = Childs[i].Child.localScale;
	}
	
	private void FixedUpdate() {
		if (fromLocalPlayer) {
			if (!isLocalPlayer)
				return;
		}
		else if (!isServer)
			return;
		List<int> toUpdate = new List<int>();
		for (int i = 0; i < Childs.Count; i++) {
			if (IsScaleChanged(i)) {
				toUpdate.Add(i);
				lastScales[i] = Childs[i].Child.localScale;
			}	
		}

		if (toUpdate.Count != 0) {
			List<Vector3> list = new List<Vector3>();
			toUpdate.ForEach(i => list.Add(Childs[i].Child.localScale));
			if (!isServer)
				CmdSendToServer(list.ToArray(), toUpdate.ToArray());
		}
	}

	private bool IsScaleChanged(int index) {
		Vector3 scale = new Vector3(Childs[index].X ? Childs[index].Child.localScale.x : lastScales[index].x, Childs[index].Y ? Childs[index].Child.localScale.y : lastScales[index].y, Childs[index].Z ? Childs[index].Child.localScale.z : lastScales[index].z);
		return !scale.Equals(lastScales[index]);
	}

	public override int GetNetworkChannel() {
		return Channels.DefaultReliable;
	}

	public override float GetNetworkSendInterval() {
		return 0.05f;
	}

	[Command]
	private void CmdSendToServer(Vector3[] array, int[] indexes) {
		for (int i = 0; i < array.Length; i++)
			lastScales[indexes[i]] = array[i];
		RpcSendToClients(array, indexes);
	}

	[ClientRpc]
	private void RpcSendToClients(Vector3[] array, int[] indexes) {
		for (int i = 0; i < array.Length; i++)
			lastScales[indexes[i]] = array[i];
		if (fromLocalPlayer) {
			if (isLocalPlayer)
				return;
		}
		else if (isServer)
			return;

		for (int i = 0; i < lastScales.Length; i++)
			Childs[i].Child.localScale = new Vector3(
				Childs[i].X ? lastScales[i].x : Childs[i].Child.localScale.x,
				Childs[i].Y ? lastScales[i].y : Childs[i].Child.localScale.y,
				Childs[i].Z ? lastScales[i].z : Childs[i].Child.localScale.z
			);
	}
}
