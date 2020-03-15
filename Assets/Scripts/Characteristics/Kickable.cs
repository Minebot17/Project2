using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Rigidbody2D))]
public class Kickable : NetworkBehaviour {
	
	public readonly EventHandler<KickEvent> kickEvent = new EventHandler<KickEvent>();

	public virtual bool Kick(Vector2 vector) {
		KickEvent result = kickEvent.CallListners(new KickEvent(gameObject, vector));
		if (result.IsCancel || result.Vector.Equals(Vector2.zero))
			return false;

		GetComponent<Rigidbody2D>().AddForce(vector);
		return true;
	}

	public class KickEvent : EventBase {
		public Vector2 Vector;

		public KickEvent(GameObject sender, Vector2 vector) : base(sender, true) {
			Vector = vector;
		}
	}
}
