using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[AddComponentMenu("NetworkCustom/NetworkSyncPosition")]
public class NetworkSyncPosition : NetworkVectors {
	[SerializeField]
	private float posLerpRate = 15;
	[SerializeField]
	private float posThreshold = 0.1f;
	[SerializeField]
	private bool fromLocalPlayer;
	[SyncVar]
	private Vector3 lastPosition;

	private void Start() {
		if (isServer)
			lastPosition = transform.position;
	}

	private void Update() {
		if (fromLocalPlayer) {
			if (isLocalPlayer)
				return;
		}
		else if (isServer)
			return;

		InterpolatePosition();
	}

	private void FixedUpdate() {
		if (fromLocalPlayer) {
			if (!isLocalPlayer)
				return;
		}
		else if (!isServer)
			return;

		if (IsPositionChanged()) {
			if (!isServer)
				CmdSendPosition(transform.position);
			lastPosition = transform.position;
		}
	}

	private bool IsPositionChanged() {
		Vector3 position = new Vector3(X ? transform.position.x : lastPosition.x, Y ? transform.position.y : lastPosition.y, Z ? transform.position.z : lastPosition.z);
		return Vector3.Distance(position, lastPosition) > posThreshold;
	}

	private void InterpolatePosition() {
		Vector3 pos = transform.position;
		Vector3 newPos = Vector3.Lerp(transform.position, lastPosition, Time.deltaTime * posLerpRate);
		transform.position = new Vector3(X ? newPos.x : pos.x, Y ? newPos.y : pos.y, Z ? newPos.z : pos.z);
	}

	[Command(channel = Channels.DefaultUnreliable)]
	private void CmdSendPosition(Vector3 pos) {
		lastPosition = pos;
	}

	public override int GetNetworkChannel() {
		return Channels.DefaultUnreliable;
	}

	public override float GetNetworkSendInterval() {
		return 0.02f;
	}
}
