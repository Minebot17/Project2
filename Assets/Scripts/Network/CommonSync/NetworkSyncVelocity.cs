using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[AddComponentMenu("NetworkCustom/NetworkSyncVelocity")]
public class NetworkSyncVelocity : NetworkVectors {
	[SerializeField]
	private float velocityLerpRate = 15;
	[SerializeField]
	private float velocityThreshold = 0.1f;
	[SerializeField]
	private bool fromLocalPlayer;
	[SyncVar]
	private Vector3 lastVelocity;
	private Rigidbody2D rigidbody2D;

	private void Start() {
		rigidbody2D = GetComponent<Rigidbody2D>();
	}

	private void Update() {
		if (fromLocalPlayer) {
			if (isLocalPlayer)
				return;
		}
		else if (isServer)
			return;

		InterpolateVelocity();
	}

	private void FixedUpdate() {
		if (fromLocalPlayer) {
			if (!isLocalPlayer)
				return;
		}
		else if (!isServer)
			return;

		if (IsVelocityChanged()) {
			if (!isServer)
				CmdSendVelocity(rigidbody2D.velocity);
			lastVelocity = rigidbody2D.velocity;
		}
	}

	private bool IsVelocityChanged() {
		Vector2 velocity = new Vector2(X ? rigidbody2D.velocity.x : lastVelocity.x, Y ? rigidbody2D.velocity.y : lastVelocity.y);
		return Vector2.Distance(velocity, lastVelocity) > velocityThreshold;
	}

	private void InterpolateVelocity() {
		Vector3 velocity = rigidbody2D.velocity;
		Vector3 newVelocity = Vector3.Lerp(rigidbody2D.velocity, lastVelocity, Time.deltaTime * velocityLerpRate);
		rigidbody2D.velocity = new Vector3(X ? newVelocity.x : velocity.x, Y ? newVelocity.y : velocity.y, Z ? newVelocity.z : velocity.z);
	}

	[Command(channel = Channels.DefaultUnreliable)]
	private void CmdSendVelocity(Vector3 velocity) {
		lastVelocity = velocity;
	}

	public override int GetNetworkChannel() {
		return Channels.DefaultUnreliable;
	}

	public override float GetNetworkSendInterval() {
		return 0.02f;
	}
}
