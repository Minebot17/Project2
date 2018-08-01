using System;
using UnityEngine;
using UnityEngine.Networking;

[AddComponentMenu("NetworkCustom/NetworkSyncRotation")]
public class NetworkSyncRotation : NetworkVectors{
	[SerializeField]
	private float rotLerpRate = 15;
	[SerializeField]
	private float rotThreshold = 0.1f;
	[SerializeField]
	private bool fromLocalPlayer;
	[SyncVar]
	private Vector3 lastRotation;

	private void Start() {
		if (isServer)
			lastRotation = transform.localEulerAngles;
	}
	
	private void Update() {
		if (fromLocalPlayer) {
			if (isLocalPlayer)
				return;
		}
		else if (isServer)
			return;

		if (isServer)
			lastRotation = transform.localEulerAngles;

		InterpolateRotation();
	}

	private void FixedUpdate() {
		if (fromLocalPlayer) {
			if (!isLocalPlayer)
				return;
		}
		else if (!isServer)
			return;

		if (IsRotationChanged()) {
			if (!isServer)
				CmdSendRotation(transform.localEulerAngles);
			lastRotation = transform.localEulerAngles;
		}
	}

	private bool IsRotationChanged() {
		Vector3 rotation = new Vector3(X ? transform.localEulerAngles.x : lastRotation.x, Y ? transform.localEulerAngles.y : lastRotation.y, Z ? transform.localEulerAngles.z : lastRotation.z);
		return Vector3.Distance(rotation, lastRotation) > rotThreshold;
	}
	
	private void InterpolateRotation() {
		Vector3 rot = transform.localEulerAngles;
		Vector3 newRot = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(lastRotation), Time.deltaTime * rotLerpRate).eulerAngles;
		transform.localEulerAngles = new Vector3(X ? newRot.x : rot.x, Y ? newRot.y : rot.y, Z ? newRot.z : rot.z);
	}

	[Command(channel = Channels.DefaultUnreliable)]
	private void CmdSendRotation(Vector3 rot) {
		lastRotation = rot;
	}

	public override int GetNetworkChannel() {
		return Channels.DefaultUnreliable;
	}

	public override float GetNetworkSendInterval() {
		return 0.05f;
	}
}

