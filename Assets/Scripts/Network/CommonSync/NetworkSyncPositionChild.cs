using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[AddComponentMenu("NetworkCustom/NetworkSyncPositionChild")]
public class NetworkSyncPositionChild : NetworkBehaviour {
	[SerializeField]
	private float posLerpRate = 15;
	[SerializeField]
	private float posThreshold = 0.1f;
	[SerializeField]
	private bool fromLocalPlayer;
	public List<ChildVector> Childs = new List<ChildVector>();

	private Vector3[] lastPositions;

	private void Awake() {
		lastPositions = new Vector3[Childs.Count];
		for (int i = 0; i < Childs.Count; i++)
			lastPositions[i] = Childs[i].Child.localPosition;
	}

	private void Update() {
		if (fromLocalPlayer) {
			if (isLocalPlayer)
				return;
		}
		else if (isServer)
			return;

		for (int i = 0; i < lastPositions.Length; i++) {
			Vector3 pos = Childs[i].Child.localPosition;
			Vector3 newPos = Vector3.Lerp(Childs[i].Child.localPosition, lastPositions[i], Time.deltaTime * posLerpRate);
			Childs[i].Child.localPosition = new Vector3(Childs[i].X ? newPos.x : pos.x, Childs[i].Y ? newPos.y : pos.y, Childs[i].Z ? newPos.z : pos.z);
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
			if (IsPositionChanged(i)) {
				toUpdate.Add(i);
				lastPositions[i] = Childs[i].Child.localPosition;
			}	
		}

		if (toUpdate.Count != 0) {
			List<Vector3> list = new List<Vector3>();
			toUpdate.ForEach(i => list.Add(Childs[i].Child.localPosition));
			if (!isServer)
				CmdSendToServer(list.ToArray(), toUpdate.ToArray());
		}
	}

	private bool IsPositionChanged(int index) {
		Vector3 position = new Vector3(Childs[index].X ? Childs[index].Child.localPosition.x : lastPositions[index].x, Childs[index].Y ? Childs[index].Child.localPosition.y : lastPositions[index].y, Childs[index].Z ? Childs[index].Child.localPosition.z : lastPositions[index].z);
		return Vector3.Distance(position, lastPositions[index]) > posThreshold;
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
			lastPositions[indexes[i]] = array[i];
		RpcSendToClients(array, indexes);
	}

	[ClientRpc]
	private void RpcSendToClients(Vector3[] array, int[] indexes) {
		if (isLocalPlayer)
			return;
		for (int i = 0; i < array.Length; i++)
			lastPositions[indexes[i]] = array[i];
	}
}
