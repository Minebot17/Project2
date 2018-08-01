using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[AddComponentMenu("NetworkCustom/NetworkSyncRotationChild")]
public class NetworkSyncRotationChild : NetworkBehaviour {
	[SerializeField]
	private float posLerpRate = 15;
	[SerializeField]
	private float posThreshold = 0.1f;
	[SerializeField]
	private bool fromLocalPlayer;
	public List<ChildVector> Childs = new List<ChildVector>();

	private Vector3[] lastRorations;

	private void Awake() {
		lastRorations = new Vector3[Childs.Count];
		for (int i = 0; i < Childs.Count; i++)
			lastRorations[i] = Childs[i].Child.localEulerAngles;
	}

	private void Update() {
		if (fromLocalPlayer) {
			if (isLocalPlayer)
				return;
		}
		else if (isServer)
			return;

		for (int i = 0; i < lastRorations.Length; i++) {
			Vector3 pos = Childs[i].Child.localEulerAngles;
			Vector3 newPos = Vector3.Lerp(Childs[i].Child.localEulerAngles, lastRorations[i], Time.deltaTime * posLerpRate);
			Childs[i].Child.localEulerAngles = new Vector3(Childs[i].X ? newPos.x : pos.x, Childs[i].Y ? newPos.y : pos.y, Childs[i].Z ? newPos.z : pos.z);
		}
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
			if (IsRotationChanged(i)) {
				toUpdate.Add(i);
				lastRorations[i] = Childs[i].Child.localEulerAngles;
			}	
		}

		if (toUpdate.Count != 0) {
			List<Vector3> list = new List<Vector3>();
			toUpdate.ForEach(i => list.Add(Childs[i].Child.localEulerAngles));
			if (!isServer)
				CmdSendToServer(list.ToArray(), toUpdate.ToArray());
		}
	}

	private bool IsRotationChanged(int index) {
		Vector3 rotation = new Vector3(Childs[index].X ? Childs[index].Child.localEulerAngles.x : lastRorations[index].x, Childs[index].Y ? Childs[index].Child.localEulerAngles.y : lastRorations[index].y, Childs[index].Z ? Childs[index].Child.localEulerAngles.z : lastRorations[index].z);
		return Vector3.Distance(rotation, lastRorations[index]) > posThreshold;
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
			lastRorations[indexes[i]] = array[i];
		RpcSendToClients(array, indexes);
	}

	[ClientRpc]
	private void RpcSendToClients(Vector3[] array, int[] indexes) {
		if (isLocalPlayer)
			return;
		for (int i = 0; i < array.Length; i++)
			lastRorations[indexes[i]] = array[i];
	}
}
