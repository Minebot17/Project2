using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[AddComponentMenu("NetworkCustom/NetworkSyncScale")]
public class NetworkSyncScale : NetworkVectors {
	
	[SerializeField]
	private bool fromLocalPlayer;
	[SyncVar(hook = nameof(OnScaleChange))]
	private Vector3 lastScale;

	private void Start() {
		if (isServer)
			lastScale = transform.localScale;
	}
	
	private void OnScaleChange(Vector3 value) {
		lastScale = value;
		if (fromLocalPlayer) {
			if (isLocalPlayer)
				return;
		}
		else if (isServer)
			return;

		transform.localScale = new Vector3(X ? lastScale.x : transform.localScale.x, Y ? lastScale.y : transform.localScale.y, Z ? lastScale.z : transform.localScale.z);
	}

	private void FixedUpdate() {
		if (fromLocalPlayer) {
			if (!isLocalPlayer)
				return;
		}
		else if (!isServer)
			return;

		if (IsScaleChanged()) {
			if (!isServer)
				CmdSendScale(transform.localScale);
			lastScale = transform.localScale;
		}
	}

	private bool IsScaleChanged() {
		Vector3 scale = new Vector3(X ? transform.localScale.x : lastScale.x, Y ? transform.localScale.y : lastScale.y, Z ? transform.localScale.z : lastScale.z);
		return !scale.Equals(lastScale);
	}

	[Command(channel = Channels.DefaultReliable)]
	private void CmdSendScale(Vector3 scale) {
		lastScale = scale;
	}

	public override int GetNetworkChannel() {
		return Channels.DefaultReliable;
	}

	public override float GetNetworkSendInterval() {
		return 0.05f;
	}
}
